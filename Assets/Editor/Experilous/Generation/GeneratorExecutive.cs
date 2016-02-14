﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Experilous.Generation
{
	[GeneratorCategory(typeof(GeneratorExecutive), "Utilities")] public struct UtilitiesCategory { }

	public abstract class GeneratorExecutive : ScriptableObject
	{
		[SerializeField] private bool _initialized = false;
		[SerializeField] private int _instanceID;

		[SerializeField] protected List<Generator> _generators = new List<Generator>();

		[SerializeField] protected AssetCollection _assetCollection;

		public string generationName;
		public string generationPath;

		[System.NonSerialized] private IEnumerator _generationIterator;
		[System.NonSerialized] private float _generationProgress;
		[System.NonSerialized] private string _generationMessage;
		[System.NonSerialized] private EventWaitHandle _concurrentWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
		[System.NonSerialized] private System.Exception _generationException;

		public bool isGenerating { get { return _generationIterator != null; } }
		public float generationProgress { get { return _generationProgress; } }
		public string generationMessage { get { return _generationMessage; } }

		public Object SetAsset(OutputSlot descriptor, Object asset)
		{
			if (asset != null)
			{
				if (ShouldPersist(descriptor))
				{
					Object persistedAsset = descriptor.persistedAsset;

					if ((descriptor.availability & OutputSlot.Availability.AfterGeneration) != 0)
					{
						persistedAsset = PersistAsset(persistedAsset, asset, descriptor.name, descriptor.path);
					}
					else
					{
						persistedAsset = PersistHiddenAsset(persistedAsset, asset, descriptor.name);
					}

					return persistedAsset;
				}
				else
				{
					return asset;
				}
			}
			else
			{
				ClearAsset(descriptor);
				return null;
			}
		}

		public void ClearAsset(OutputSlot descriptor)
		{
			if (descriptor.persistedAsset != null)
			{
				AssetUtility.DeleteAsset(descriptor.persistedAsset);
			}
		}

		private bool ShouldPersist(OutputSlot descriptor)
		{
			if (descriptor.isEnabled == false) return false;
			if ((descriptor.availability & OutputSlot.Availability.AfterGeneration) != 0) return true;

			foreach (var reference in descriptor.generator.internalConnections)
			{
				if (ReferenceEquals(reference.source, descriptor))
				{
					if (ShouldPersist(reference.target)) return true;
				}
			}

			foreach (var consumer in descriptor.connections)
			{
				foreach (var reference in consumer.generator.internalConnections)
				{
					if (ReferenceEquals(reference.source, descriptor))
					{
						if (ShouldPersist(reference.target)) return true;
					}
				}
			}

			return false;
		}

		public string GetAssetPath(string assetName, string assetPath, string extension)
		{
			var fullPath = Path.Combine(generationPath, generationName);
			if (!string.IsNullOrEmpty(assetPath)) fullPath = Path.Combine(fullPath, assetPath);
			fullPath = Path.Combine(fullPath, string.Format("{0}.{1}", assetName, extension));
			return AssetUtility.GetCanonicalPath(fullPath);
		}

		private void FindAndMoveAnyConflictingObject(string fullAssetPath, Object asset = null)
		{
			var conflictingAsset = AssetDatabase.LoadAssetAtPath<Object>(fullAssetPath);
			if (conflictingAsset != null && !ReferenceEquals(asset, conflictingAsset))
			{
				if (_assetCollection.discreteAssets.Contains(conflictingAsset))
				{
					var availableAssetPath = AssetUtility.GenerateAvailableAssetPath(fullAssetPath);
					var errorMessage = AssetDatabase.MoveAsset(fullAssetPath, availableAssetPath);
					if (!string.IsNullOrEmpty(errorMessage)) throw new System.InvalidOperationException(errorMessage);
				}
				else
				{
					throw new System.InvalidOperationException(string.Format("Cannot save asset at the path specified because there is another asset saved at the same path that is not part of this collection.  Path:  \"{0}\"", fullAssetPath));
				}
			}
		}

		private Object PersistAsset(Object oldAsset, Object newAsset, string assetName, string assetPath)
		{
			if (typeof(GameObject).IsInstanceOfType(newAsset))
			{
				if (oldAsset != null)
				{
					return PersistAsset((GameObject)oldAsset, (GameObject)newAsset, assetName, assetPath);
				}
				else
				{
					return PersistAsset(null, (GameObject)newAsset, assetName, assetPath);
				}
			}

			var fullAssetPath = GetAssetPath(assetName, assetPath, "asset");
			AssetUtility.CreatePathFolders(fullAssetPath);

			// Is there an earlier instance of the asset already generated and persisted?
			if (oldAsset != null && AssetDatabase.Contains(oldAsset))
			{
				// Is the earlier asset and this asset exactly the same type, and is the old asset currently
				// persisted as a discrete asset, not as a hidden embedded asset in the asset collection?
				if (oldAsset.GetType() == newAsset.GetType() && _assetCollection.discreteAssets.Contains(oldAsset))
				{
					// Check the earlier asset's path, to see if it's already in the right place.
					var currentPath = AssetUtility.GetProjectRelativeAssetPath(oldAsset);
					if (currentPath != fullAssetPath)
					{
						// If there's any asset that is a part of this collection, then rename it so that the old asset can be moved.
						FindAndMoveAnyConflictingObject(fullAssetPath, oldAsset);

						// Move the old asset to the new location.
						var errorMessage = AssetDatabase.MoveAsset(currentPath, fullAssetPath);
						if (!string.IsNullOrEmpty(errorMessage)) throw new System.InvalidOperationException(errorMessage);
					}

					// Update the earlier asset to match the new instance.
					AssetUtility.UpdateAssetInPlace(newAsset, oldAsset);
					oldAsset.name = assetName;
					EditorUtility.SetDirty(oldAsset);

					// Re-add the old asset to the asset collection just in case it wasn't already there or in the right list for some reason.
					_assetCollection.AddDiscrete(oldAsset);
					EditorUtility.SetDirty(_assetCollection);

					return oldAsset;
				}
				// The earlier asset and the new instance have different types, or the old asset is embedded in the asset collection.
				else
				{
					// There's no way to maintain references, so just delete the old instance.
					_assetCollection.Remove(oldAsset);
					DestroyImmediate(oldAsset, true);

					// If there's any asset that is a part of this collection, then rename it so that the new asset can be persisted.
					FindAndMoveAnyConflictingObject(fullAssetPath);

					// Persist the new asset.
					newAsset.name = assetName;
					AssetDatabase.CreateAsset(newAsset, fullAssetPath);
					EditorUtility.SetDirty(newAsset);

					// Add the new asset to the asset collection.
					_assetCollection.AddDiscrete(newAsset);
					EditorUtility.SetDirty(_assetCollection);

					return newAsset;
				}
			}
			// There was no earlier persistent instance of the asset, so there is nothing to replace.
			else
			{
				// Persist the new asset.
				newAsset.name = assetName;
				AssetDatabase.CreateAsset(newAsset, fullAssetPath);
				EditorUtility.SetDirty(newAsset);

				// Add the new asset to the asset collection.
				_assetCollection.AddDiscrete(newAsset);
				EditorUtility.SetDirty(_assetCollection);

				return newAsset;
			}
		}

		private GameObject PersistAsset(GameObject oldAsset, GameObject newAsset, string assetName, string assetPath)
		{
			var fullAssetPath = GetAssetPath(assetName, assetPath, "prefab");
			AssetUtility.CreatePathFolders(fullAssetPath);

			// Is there an earlier instance of the asset already generated and persisted?
			if (oldAsset != null && AssetDatabase.Contains(oldAsset))
			{
				// Is the earlier asset and this asset exactly the same type?
				if (oldAsset.GetType() == newAsset.GetType())
				{
					// Check the earlier asset's path, to see if it's already in the right place.
					var currentPath = AssetUtility.GetProjectRelativeAssetPath(oldAsset);
					if (currentPath != fullAssetPath)
					{
						// If there's any asset that is a part of this collection, then rename it so that the old asset can be moved.
						FindAndMoveAnyConflictingObject(fullAssetPath, oldAsset);

						// Move the old asset to the new location.
						var errorMessage = AssetDatabase.MoveAsset(currentPath, fullAssetPath);
						if (!string.IsNullOrEmpty(errorMessage)) throw new System.InvalidOperationException(errorMessage);
					}

					// Replace the earlier prefab with the new instance.
					var prefab = PrefabUtility.ReplacePrefab(newAsset as GameObject, oldAsset, ReplacePrefabOptions.ConnectToPrefab);
					// Destroy the template upon which the newly created prefab was based, since it will have been created within the scene.
					DestroyImmediate(newAsset);
					return prefab;
				}
				// The earlier asset and the new instance have different types.
				else
				{
					// If there's any asset that is a part of this collection, then rename it so that the new asset can be persisted.
					FindAndMoveAnyConflictingObject(fullAssetPath);

					// Create a new prefab based on the supplied template.
					var prefab = PrefabUtility.CreatePrefab(fullAssetPath, newAsset as GameObject);
					// Destroy the template upon which the newly created prefab was based, since it will have been created within the scene.
					DestroyImmediate(newAsset);
					return prefab;
				}
			}
			// There was no earlier persistent instance of the asset, so there is nothing to replace.
			else
			{
				// Create a new prefab based on the supplied template.
				var prefab = PrefabUtility.CreatePrefab(fullAssetPath, newAsset as GameObject);
				// Destroy the template upon which the newly created prefab was based, since it will have been created within the scene.
				DestroyImmediate(newAsset);
				return prefab;
			}
		}

		private Object PersistHiddenAsset(Object oldAsset, Object newAsset, string assetName)
		{
			if (typeof(GameObject).IsInstanceOfType(newAsset))
			{
				throw new System.InvalidOperationException("Assets of type GameObject must always be available after generation.");
			}

			// Is there an earlier instance of the asset already generated and persisted?
			if (oldAsset != null && AssetDatabase.Contains(oldAsset))
			{
				// Is the earlier asset and this asset exactly the same type, and is the old asset currently
				// persisted as a hidden embedded asset in the asset collection, not as a discrete asset?
				if (oldAsset.GetType() == newAsset.GetType() && _assetCollection.embeddedAssets.Contains(oldAsset))
				{
					// Update the earlier asset to match the new instance.
					AssetUtility.UpdateAssetInPlace(newAsset, oldAsset);
					oldAsset.name = assetName;
					oldAsset.hideFlags |= HideFlags.HideInHierarchy;
					EditorUtility.SetDirty(oldAsset);

					// Re-add the old asset to the asset collection just in case it wasn't already there or in the right list for some reason.
					_assetCollection.AddEmbedded(oldAsset);
					EditorUtility.SetDirty(_assetCollection);

					return oldAsset;
				}
				// The earlier asset and the new instance have different types, or the old asset is not embedded in the asset collection.
				else
				{
					// There's no way to maintain references, so just delete the old instance.
					_assetCollection.Remove(oldAsset);
					DestroyImmediate(oldAsset, true);

					// Persist the new asset.
					newAsset.name = assetName;
					newAsset.hideFlags |= HideFlags.HideInHierarchy;
					AssetDatabase.AddObjectToAsset(newAsset, _assetCollection);
					EditorUtility.SetDirty(newAsset);

					// Add the new asset to the asset collection.
					_assetCollection.AddEmbedded(newAsset);
					EditorUtility.SetDirty(_assetCollection);

					return newAsset;
				}
			}
			// There was no earlier persistent instance of the asset, so there is nothing to replace.
			else
			{
				// Persist the new asset.
				newAsset.name = assetName;
				newAsset.hideFlags |= HideFlags.HideInHierarchy;
				AssetDatabase.AddObjectToAsset(newAsset, _assetCollection);
				EditorUtility.SetDirty(newAsset);

				// Add the new asset to the asset collection.
				_assetCollection.AddEmbedded(newAsset);
				EditorUtility.SetDirty(_assetCollection);

				return newAsset;
			}
		}

		public IList<Generator> generators { get {  return _generators.AsReadOnly(); } }

		protected void OnEnable()
		{
			if (!_initialized)
			{
				_initialized = true;
				_instanceID = GetInstanceID();
			}
			else if (_instanceID != GetInstanceID())
			{
				//Asset was copied outside of Unity; all generated assets should be reset.
				foreach (var generator in _generators)
				{
					foreach (var output in generator.activeOutputs)
					{
						output.ForgetAsset();
					}
				}

				_assetCollection = null;

				var original = EditorUtility.InstanceIDToObject(_instanceID) as GeneratorExecutive;
				if (original != null && original.generationName == generationName && original.generationPath == generationPath && AssetDatabase.Contains(this))
				{
					var copyPath = AssetDatabase.GetAssetPath(this);
					generationName = Path.GetFileNameWithoutExtension(copyPath);
				}

				_instanceID = GetInstanceID();
			}
		}

		public List<OutputSlot> GetMatchingGeneratedAssets(Generator excludedGenerator, System.Predicate<OutputSlot> predicate)
		{
			var matchingGeneratedAssets = new List<OutputSlot>();

			foreach (var generator in _generators)
			{
				if (!ReferenceEquals(generator, excludedGenerator))
				{
					foreach (var output in generator.activeOutputs)
					{
						if (output != null && output.isAvailableDuringGeneration && predicate(output))
						{
							matchingGeneratedAssets.Add(output);
						}
					}
				}
			}

			return matchingGeneratedAssets;
		}

		public int GetMatchingGeneratedAssetsCount(Generator excludedGenerator, System.Predicate<OutputSlot> predicate)
		{
			var count = 0;

			foreach (var generator in _generators)
			{
				if (!ReferenceEquals(generator, excludedGenerator))
				{
					foreach (var output in generator.activeOutputs)
					{
						if (output != null && output.isAvailableDuringGeneration && predicate(output))
						{
							++count;
						}
					}
				}
			}

			return count;
		}

		public string[] GetAllAssetPaths(string emptyPath = null)
		{
			var assetPathsSet = new HashSet<string>();
			foreach (var generator in _generators)
			{
				foreach (var output in generator.activeOutputs)
				{
					if (output.isAvailableAfterGeneration && !string.IsNullOrEmpty(output.path))
					{
						assetPathsSet.Add(output.path);
					}
				}
			}

			if (string.IsNullOrEmpty(emptyPath))
			{
				var assetPaths = new string[assetPathsSet.Count];
				assetPathsSet.CopyTo(assetPaths);
				System.Array.Sort(assetPaths);
				return assetPaths;
			}
			else
			{
				var assetPaths = new string[assetPathsSet.Count + 1];
				assetPaths[0] = emptyPath;
				assetPathsSet.CopyTo(assetPaths, 1);
				System.Array.Sort(assetPaths, 1, assetPaths.Length - 1);
				return assetPaths;
			}
		}

		public virtual void Add(Generator generator)
		{
			if (!CanAdd(generator)) throw new System.InvalidOperationException(string.Format("The asset generator \"{0}\" of type {1} cannot be added to this collection.", generator.name, generator.GetType().Name));

			_generators.Add(generator);
		}

		public virtual bool CanAdd(Generator generator)
		{
			if (generator == null) throw new System.ArgumentNullException("generator");
			return !isGenerating && !_generators.Contains(generator);
		}

		public virtual void Insert(int index, Generator generator)
		{
			if (!CanInsert(index, generator)) throw new System.InvalidOperationException(string.Format("The asset generator \"{0}\" of type {1} cannot be insert into this collection.", generator.name, generator.GetType().Name));

			_generators.Insert(index, generator);
		}

		public virtual bool CanInsert(int index, Generator generator)
		{
			if (generator == null) throw new System.ArgumentNullException("generator");
			if (index < 0 || index > _generators.Count) throw new System.ArgumentOutOfRangeException("generator");
			return !isGenerating && !_generators.Contains(generator);
		}

		public virtual void Remove(Generator generator)
		{
			if (!CanRemove(generator)) throw new System.InvalidOperationException(string.Format("The asset generator \"{0}\" of type {1} cannot be removed from this collection.", generator.name, generator.GetType().Name));

			_generators.Remove(generator);
			foreach (var otherGenerator in _generators)
			{
				foreach (var dependency in otherGenerator.dependencies)
				{
					foreach (var output in generator.activeOutputs)
					{
						if (ReferenceEquals(dependency, output))
						{
							otherGenerator.ResetDependency(dependency);
						}
					}
				}
			}
		}

		public virtual bool CanRemove(Generator generator)
		{
			if (generator == null) throw new System.ArgumentNullException("generator");
			return !isGenerating && _generators.Contains(generator);
		}

		public virtual void MoveUp(Generator generator)
		{
			if (!CanMoveUp(generator)) throw new System.InvalidOperationException(string.Format("The asset generator \"{0}\" of type {1} cannot be moved up within this collection.", generator.name, generator.GetType().Name));

			var index = _generators.IndexOf(generator);
			_generators.RemoveAt(index);
			_generators.Insert(index - 1, generator);
		}

		public virtual bool CanMoveUp(Generator generator)
		{
			if (generator == null) throw new System.ArgumentNullException("generator");
			var index = _generators.IndexOf(generator);
			return (!isGenerating && index != -1 && index > 0);
		}

		public virtual void MoveDown(Generator generator)
		{
			if (!CanMoveDown(generator)) throw new System.InvalidOperationException(string.Format("The asset generator \"{0}\" of type {1} cannot be moved down within this collection.", generator.name, generator.GetType().Name));

			var index = _generators.IndexOf(generator);
			_generators.RemoveAt(index);
			_generators.Insert(index + 1, generator);
		}

		public virtual bool CanMoveDown(Generator generator)
		{
			if (generator == null) throw new System.ArgumentNullException("generator");
			var index = _generators.IndexOf(generator);
			return (!isGenerating && index != -1 && index < _generators.Count - 1);
		}

		public void CreateAsset()
		{
			var collectionPath = AssetUtility.selectedFolderOrDefault;
			if (string.IsNullOrEmpty(generationPath)) generationPath = collectionPath;
			if (string.IsNullOrEmpty(generationName)) generationName = name;
			AssetDatabase.CreateAsset(this, AssetUtility.GetCanonicalPath(Path.Combine(collectionPath, name + ".asset")));
			foreach (var generator in _generators)
			{
				AssetDatabase.AddObjectToAsset(generator, this);
				foreach (var output in generator.activeOutputs)
				{
					AssetDatabase.AddObjectToAsset(output.key, this);
				}
			}
			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();

			AssetUtility.SelectAsset(this);
		}

		public WaitHandle GenerateConcurrently(System.Action concurrentAction)
		{
			_concurrentWaitHandle.Reset();
			_generationException = null;
			ThreadPool.QueueUserWorkItem(ExecuteWaitableAction, concurrentAction);
			return _concurrentWaitHandle;
		}

		private void ExecuteWaitableAction(object action)
		{
			try
			{
				((System.Action)action)();
			}
			catch (System.Exception e)
			{
				_generationException = e;
			}
			finally
			{
				_concurrentWaitHandle.Set();
			}
		}

		private object UpdateGenerationProgress(int currentStep, string newMessage = null, List<Generator> generators = null)
		{
			if (currentStep == 0) _generationProgress = 0f;

			float estimatedConsumedTime, estimatedRemainingTime;
			var estimatedTotalTime = GetEstimatedTimes(currentStep, generators != null ? generators : _generators, out estimatedConsumedTime, out estimatedRemainingTime);

			_generationProgress = Mathf.Max(_generationProgress, estimatedConsumedTime / estimatedTotalTime);
			if (newMessage != null) _generationMessage = newMessage;
			return null;
		}

		public void Generate(string generationName, string generationPath)
		{
			_generationIterator = BeginGeneration(generationName, generationPath);

			GenerationUpdate();
		}

		public void GenerationUpdate()
		{
			if (_generationIterator != null)
			{
				var currentTime = Time.realtimeSinceStartup;
				var startTime = currentTime;
				do
				{
					try
					{
						if (!_generationIterator.MoveNext())
						{
							_generationIterator = null;
						}

						if (_generationException != null)
						{
							var exception = _generationException;
							_generationException = null;
							throw exception;
						}
					}
					catch (System.Exception)
					{
						_generationIterator = null;
						EditorUtility.ClearProgressBar();
						throw;
					}

					currentTime = Time.realtimeSinceStartup;
				} while (_generationIterator != null && currentTime >= startTime && currentTime - startTime < 0.05f);

				Editor editor = null;
				Editor.CreateCachedEditor(this, null, ref editor);
				if (editor != null) editor.Repaint();

				if (_generationIterator != null)
				{
					EditorUtility.DisplayProgressBar("Generating", generationMessage, generationProgress);
					EditorApplication.delayCall += GenerationUpdate;
				}
				else
				{
					EditorUtility.ClearProgressBar();
				}
			}
		}

		// Pre-Generation Steps
		private float estimatedStartupTime { get { return 0.01f; } }
		private float estimatedRefreshDatabase0Time { get { return 0.02f; } }
		private float estimatedUpdateAssetCollectionTime { get { return 0.01f; } }
		private float estimatedGetDependencyOrderedGeneratorsTime { get { return 0.01f; } }

		// Post-Generation Steps
		private float estimatedEmbedAssetsTime { get { return 0.01f; } }
		private float estimatedDestroyLeftoversTime { get { return 0.01f; } }
		private float estimatedSaveDatabaseTime { get { return 0.02f; } }
		private float estimatedRefreshDatabase1Time { get { return 0.02f; } }
		private float estimatedDeleteEmptyFoldersTime { get { return 0.01f; } }
		private float estimatedRefreshDatabase2Time { get { return 0.02f; } }

		private static void AddTime(bool consumed, float time, ref float consumedTime, ref float remainingTime)
		{
			if (consumed)
			{
				consumedTime += time;
			}
			else
			{
				remainingTime += time;
			}
		}

		private float GetEstimatedTimes(int currentStep, List<Generator> generators, out float consumedTime, out float remainingTime)
		{
			consumedTime = remainingTime = 0f;

			AddTime(currentStep >= 1, estimatedStartupTime, ref consumedTime, ref remainingTime);
			AddTime(currentStep >= 2, estimatedRefreshDatabase0Time, ref consumedTime, ref remainingTime);
			AddTime(currentStep >= 3, estimatedUpdateAssetCollectionTime, ref consumedTime, ref remainingTime);
			AddTime(currentStep >= 4, estimatedGetDependencyOrderedGeneratorsTime, ref consumedTime, ref remainingTime);

			for (int i = 0; i < generators.Count; ++i)
			{
				AddTime(currentStep >= 5 + i, generators[i].estimatedGenerationTime, ref consumedTime, ref remainingTime);
			}

			AddTime(currentStep >= 4 + generators.Count + 1, estimatedEmbedAssetsTime, ref consumedTime, ref remainingTime);
			AddTime(currentStep >= 4 + generators.Count + 2, estimatedDestroyLeftoversTime, ref consumedTime, ref remainingTime);
			AddTime(currentStep >= 4 + generators.Count + 3, estimatedSaveDatabaseTime, ref consumedTime, ref remainingTime);
			AddTime(currentStep >= 4 + generators.Count + 4, estimatedRefreshDatabase1Time, ref consumedTime, ref remainingTime);
			AddTime(currentStep >= 4 + generators.Count + 5, estimatedDeleteEmptyFoldersTime, ref consumedTime, ref remainingTime);
			AddTime(currentStep >= 4 + generators.Count + 6, estimatedRefreshDatabase2Time, ref consumedTime, ref remainingTime);

			return consumedTime + remainingTime;
		}

		private IEnumerator BeginGeneration(string generationName, string generationPath)
		{
			yield return UpdateGenerationProgress(0, "Preparing...");

			var currentStep = 0;

			var oldGenerationName = this.generationName;
			var oldGenerationPath = this.generationPath;
			this.generationName = generationName;
			this.generationPath = generationPath;

			yield return UpdateGenerationProgress(++currentStep, "Preparing...");

			AssetDatabase.Refresh();

			yield return UpdateGenerationProgress(++currentStep, "Preparing...");

			UpdateAssetCollection();

			yield return UpdateGenerationProgress(++currentStep, "Preparing...");

			var generators = GetDependencyOrderedGenerators();

			// Generate all assets, moving or renaming the outputs if necessary.
			var discreteAssets = new List<Object>();
			foreach (var generator in generators)
			{
				yield return UpdateGenerationProgress(++currentStep, string.Format("Generating ({0}/{1})...", currentStep - 4, generators.Count));

				var generation = generator.BeginGeneration();
				while (generation.MoveNext())
				{
					if (generation.Current is WaitHandle)
					{
						var waitHandle = (WaitHandle)generation.Current;
						while (waitHandle.WaitOne(10) == false)
						{
							yield return UpdateGenerationProgress(currentStep);
						}

						if (_generationException != null)
						{
							var exception = _generationException;
							_generationException = null;
							throw exception;
						}
					}
					else
					{
						yield return UpdateGenerationProgress(currentStep);
					}
				}

				foreach (var output in generator.activeOutputs)
				{
					discreteAssets.Add(output.asset);
				}
			}

			yield return UpdateGenerationProgress(++currentStep, "Finalizing...", _generators);

			// Embed objects within generator collection.
			var embeddedAssets = EmbedAssets();

			yield return UpdateGenerationProgress(++currentStep, "Finalizing...");

			DestroyLeftovers(embeddedAssets, discreteAssets);

			yield return UpdateGenerationProgress(++currentStep, "Finalizing...");

			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();

			yield return UpdateGenerationProgress(++currentStep, "Finalizing...");

			AssetDatabase.Refresh();

			yield return UpdateGenerationProgress(++currentStep, "Finalizing...");

			if (!string.IsNullOrEmpty(oldGenerationName) && !string.IsNullOrEmpty(oldGenerationPath))
			{
				AssetUtility.RecursivelyDeleteEmptyFolders(AssetUtility.GetCanonicalPath(Path.Combine(oldGenerationPath, oldGenerationName)));
			}

			yield return UpdateGenerationProgress(++currentStep, "Finalizing...");

			AssetDatabase.Refresh();

			yield return UpdateGenerationProgress(++currentStep, "Generation Complete");
		}

		public virtual bool canGenerate
		{
			get
			{
				if (string.IsNullOrEmpty(generationName)) return false;
				if (string.IsNullOrEmpty(generationPath)) return false;

				foreach (var generator in _generators)
				{
					if (!generator.canGenerate) return false;
				}

				return true;
			}
		}

		private void UpdateAssetCollection()
		{
			if (_assetCollection == null || !AssetDatabase.Contains(_assetCollection))
			{
				_assetCollection = AssetCollection.Create("Asset Collection");
				var assetCollectionPath = GetAssetPath(_assetCollection.name, null, "asset");
				AssetUtility.CreatePathFolders(assetCollectionPath);
				AssetDatabase.CreateAsset(_assetCollection, assetCollectionPath);
			}
			else
			{
				// Check the asset collection's current path, to see if it's already in the right place.
				var currentPath = AssetUtility.GetProjectRelativeAssetPath(_assetCollection);
				var assetCollectionPath = GetAssetPath(_assetCollection.name, null, "asset");
				if (currentPath != assetCollectionPath)
				{
					// Move the old asset to the new location.
					AssetUtility.CreatePathFolders(assetCollectionPath);
					var errorMessage = AssetDatabase.MoveAsset(currentPath, assetCollectionPath);
					if (!string.IsNullOrEmpty(errorMessage)) throw new System.InvalidOperationException(errorMessage);
				}
			}
		}

		private List<Generator> GetDependencyOrderedGenerators()
		{
			var orderedGenerators = new List<Generator>();
			var unorderedGenerators = new List<Generator>();

			// Build an ordered list of all generators according to dependencies.
			unorderedGenerators.AddRange(_generators);
			while (unorderedGenerators.Count > 0)
			{
				var insertionIndex = 0;
				for (int i = 0; i < unorderedGenerators.Count; ++i)
				{
					var generator = unorderedGenerators[i];
					var dependenciesComplete = true;
					foreach (var dependency in generator.dependencies)
					{
						if (!orderedGenerators.Contains(dependency.generator))
						{
							dependenciesComplete = false;
							break;
						}
					}

					if (dependenciesComplete)
					{
						orderedGenerators.Add(generator);
					}
					else
					{
						unorderedGenerators[insertionIndex++] = generator;
					}
				}

				var removalCount = unorderedGenerators.Count - insertionIndex;
				if (removalCount == 0)
				{
					throw new System.InvalidOperationException("A dependency cycle was found within the asset generators in this collection, preventing the collection from being generated.");
				}
				unorderedGenerators.RemoveRange(insertionIndex, removalCount);
			}

			return orderedGenerators;
		}

		private List<Object> EmbedAssets()
		{
			var embeddedAssets = new List<Object>();
			embeddedAssets.Add(this);

			foreach (var generator in _generators)
			{
				embeddedAssets.Add(generator);
				if (!AssetDatabase.Contains(generator))
				{
					AssetDatabase.AddObjectToAsset(generator, this);
				}

				foreach (var input in generator.activeInputs)
				{
					embeddedAssets.Add(input.key);
					if (!AssetDatabase.Contains(input.key))
					{
						AssetDatabase.AddObjectToAsset(input.key, this);
					}
				}

				foreach (var output in generator.activeOutputs)
				{
					embeddedAssets.Add(output.key);
					if (!AssetDatabase.Contains(output.key))
					{
						AssetDatabase.AddObjectToAsset(output.key, this);
					}
				}
			}

			return embeddedAssets;
		}

		private void DestroyLeftovers(List<Object> embeddedAssets, List<Object> discreteAssets)
		{
			// Destroy any prior embedded objects that are no longer part of the collection.
			foreach (var priorEmbeddedAsset in _assetCollection.embeddedAssets)
			{
				if (priorEmbeddedAsset != null && AssetDatabase.Contains(priorEmbeddedAsset) && !embeddedAssets.Contains(priorEmbeddedAsset))
				{
					DestroyImmediate(priorEmbeddedAsset, true);
				}
			}

			// Destroy any prior embedded objects that are no longer part of the collection.
			foreach (var priorDiscreteAsset in _assetCollection.discreteAssets)
			{
				if (priorDiscreteAsset != null && AssetDatabase.Contains(priorDiscreteAsset) && !discreteAssets.Contains(priorDiscreteAsset))
				{
					DestroyImmediate(priorDiscreteAsset, true);
				}
			}
		}

		public void Save()
		{
		}

		public void Copy()
		{
			if (!canCopy) return;

			var path = AssetUtility.GetProjectRelativeAssetPath(this);
			var newPath = AssetUtility.GenerateAvailableAssetPath(path, "{0}/{1} (Copy {2}){3}");
			AssetDatabase.CopyAsset(path, newPath);
			AssetDatabase.Refresh();
		}

		public bool canCopy
		{
			get
			{
				return AssetDatabase.Contains(this);
			}
		}

		public void Reload()
		{
		}

		public void Delete()
		{
		}

		public void Move()
		{
		}

		public void Clean()
		{
		}
	}
}
