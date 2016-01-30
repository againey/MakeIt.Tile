﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace Experilous
{
	public abstract class AssetGeneratorCollection : ScriptableObject
	{
		[SerializeField] private bool _initialized = false;
		[SerializeField] private int _instanceID;

		[SerializeField] protected List<AssetGenerator> _generators = new List<AssetGenerator>();
		[SerializeField] protected List<Object> _embeddedAssets = new List<Object>();
		[SerializeField] protected List<Object> _persistedAssets = new List<Object>();
		[SerializeField] protected AssetCollection _assetCollection;

		public string generationName;
		public string generationPath;

		public Object SetAsset(AssetDescriptor descriptor, Object asset)
		{
			if (asset != null)
			{
				if (ShouldPersist(descriptor))
				{
					if ((descriptor.availability & AssetDescriptor.Availability.AfterGeneration) != 0)
					{
						return PersistAsset(descriptor.asset, asset, descriptor.name, descriptor.path);
					}
					else
					{
						return PersistHiddenAsset(descriptor.asset, asset, descriptor.name);
					}
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

		public void ClearAsset(AssetDescriptor descriptor)
		{
			if (descriptor.asset != null)
			{
				AssetUtility.DeleteAsset(descriptor.asset);
			}
		}

		private bool ShouldPersist(AssetDescriptor descriptor)
		{
			if (descriptor.isEnabled == false) return false;
			if ((descriptor.availability & AssetDescriptor.Availability.AfterGeneration) != 0) return true;

			foreach (var reference in descriptor.generator.references)
			{
				if (ReferenceEquals(reference.referencee, descriptor))
				{
					if (ShouldPersist(reference.referencer)) return true;
				}
			}

			foreach (var consumer in descriptor.consumers)
			{
				foreach (var reference in consumer.generator.references)
				{
					if (ReferenceEquals(reference.referencee, descriptor))
					{
						if (ShouldPersist(reference.referencer)) return true;
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
						// Move the old asset to the new location.
						var errorMessage = AssetDatabase.MoveAsset(currentPath, fullAssetPath);
						if (!string.IsNullOrEmpty(errorMessage)) throw new System.InvalidOperationException(errorMessage);
					}

					// Replace the earlier prefab with the new instance.
					var prefab = PrefabUtility.ReplacePrefab(newAsset as GameObject, oldAsset, ReplacePrefabOptions.ReplaceNameBased);
					// Destroy the template upon which the newly created prefab was based, since it will have been created within the scene.
					DestroyImmediate(newAsset);
					return prefab;
				}
				// The earlier asset and the new instance have different types.
				else
				{
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

		public IList<AssetGenerator> generators { get {  return _generators.AsReadOnly(); } }

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
					generator.ResetAssets();
				}

				_persistedAssets.Clear();
				_assetCollection = null;

				_instanceID = GetInstanceID();
			}
		}

		public List<AssetDescriptor> GetMatchingGeneratedAssets(AssetGenerator excludedGenerator, System.Predicate<AssetDescriptor> predicate)
		{
			var matchingGeneratedAssets = new List<AssetDescriptor>();

			foreach (var generator in _generators)
			{
				if (!ReferenceEquals(generator, excludedGenerator))
				{
					foreach (var output in generator.outputs)
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

		public string[] GetAllAssetPaths(string emptyPath = null)
		{
			var assetPathsSet = new HashSet<string>();
			foreach (var generator in _generators)
			{
				foreach (var output in generator.outputs)
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

		public virtual void Add(AssetGenerator generator)
		{
			if (!CanAdd(generator)) throw new System.InvalidOperationException(string.Format("The asset generator \"{0}\" of type {1} cannot be added to this collection.", generator.name, generator.GetType().Name));

			_generators.Add(generator);
		}

		public virtual bool CanAdd(AssetGenerator generator)
		{
			if (generator == null) throw new System.ArgumentNullException("generator");
			return !_generators.Contains(generator);
		}

		public virtual void Insert(int index, AssetGenerator generator)
		{
			if (!CanInsert(index, generator)) throw new System.InvalidOperationException(string.Format("The asset generator \"{0}\" of type {1} cannot be insert into this collection.", generator.name, generator.GetType().Name));

			_generators.Insert(index, generator);
		}

		public virtual bool CanInsert(int index, AssetGenerator generator)
		{
			if (generator == null) throw new System.ArgumentNullException("generator");
			if (index < 0 || index > _generators.Count) throw new System.ArgumentOutOfRangeException("generator");
			return !_generators.Contains(generator);
		}

		public virtual void Remove(AssetGenerator generator)
		{
			if (!CanRemove(generator)) throw new System.InvalidOperationException(string.Format("The asset generator \"{0}\" of type {1} cannot be removed from this collection.", generator.name, generator.GetType().Name));

			_generators.Remove(generator);
			foreach (var otherGenerator in _generators)
			{
				foreach (var dependency in otherGenerator.dependencies)
				{
					foreach (var output in generator.outputs)
					{
						if (ReferenceEquals(dependency, output))
						{
							otherGenerator.ResetDependency(dependency);
						}
					}
				}
			}
		}

		public virtual bool CanRemove(AssetGenerator generator)
		{
			if (generator == null) throw new System.ArgumentNullException("generator");
			return _generators.Contains(generator);
		}

		public virtual void MoveUp(AssetGenerator generator)
		{
			if (!CanMoveUp(generator)) throw new System.InvalidOperationException(string.Format("The asset generator \"{0}\" of type {1} cannot be moved up within this collection.", generator.name, generator.GetType().Name));

			var index = _generators.IndexOf(generator);
			_generators.RemoveAt(index);
			_generators.Insert(index - 1, generator);
		}

		public virtual bool CanMoveUp(AssetGenerator generator)
		{
			if (generator == null) throw new System.ArgumentNullException("generator");
			var index = _generators.IndexOf(generator);
			return (index != -1 && index > 0);
		}

		public virtual void MoveDown(AssetGenerator generator)
		{
			if (!CanMoveDown(generator)) throw new System.InvalidOperationException(string.Format("The asset generator \"{0}\" of type {1} cannot be moved down within this collection.", generator.name, generator.GetType().Name));

			var index = _generators.IndexOf(generator);
			_generators.RemoveAt(index);
			_generators.Insert(index + 1, generator);
		}

		public virtual bool CanMoveDown(AssetGenerator generator)
		{
			if (generator == null) throw new System.ArgumentNullException("generator");
			var index = _generators.IndexOf(generator);
			return (index != -1 && index < _generators.Count - 1);
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
				foreach (var output in generator.outputs)
				{
					AssetDatabase.AddObjectToAsset(output, this);
				}
			}
			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();

			AssetUtility.SelectAsset(this);
		}

		public void Generate(string generationName, string generationPath)
		{
			var oldGenerationName = this.generationName;
			var oldGenerationPath = this.generationPath;
			this.generationName = generationName;
			this.generationPath = generationPath;

			_persistedAssets.RemoveAll((Object obj) => { return obj == null; });

			AssetDatabase.Refresh();

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

			var orderedGenerators = new List<AssetGenerator>();
			var unorderedGenerators = new List<AssetGenerator>();

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

			// Generate all assets, moving or renaming the outputs if necessary.
			var persistedAssets = new List<Object>();
			foreach (var generator in orderedGenerators)
			{
				generator.Generate();
				foreach (var output in generator.outputs)
				{
					if (AssetDatabase.Contains(output.asset))
					{
						persistedAssets.Add(output.asset);
					}
				}
			}

			// Embed objects within generator collection.
			var embeddedAssets = new List<Object>();
			embeddedAssets.Add(this);

			foreach (var generator in _generators)
			{
				embeddedAssets.Add(generator);
				if (!AssetDatabase.Contains(generator))
				{
					AssetDatabase.AddObjectToAsset(generator, this);
				}

				foreach (var input in generator.inputs)
				{
					embeddedAssets.Add(input);
					if (!AssetDatabase.Contains(input))
					{
						AssetDatabase.AddObjectToAsset(input, this);
					}
				}

				foreach (var output in generator.outputs)
				{
					embeddedAssets.Add(output);
					if (!AssetDatabase.Contains(output))
					{
						AssetDatabase.AddObjectToAsset(output, this);
					}
				}
			}

			// Destroy any prior embedded objects that are no longer part of the collection.
			foreach (var priorEmbeddedAsset in _embeddedAssets)
			{
				if (priorEmbeddedAsset != null && AssetDatabase.Contains(priorEmbeddedAsset) && !embeddedAssets.Contains(priorEmbeddedAsset))
				{
					DestroyImmediate(priorEmbeddedAsset, true);
				}
			}
			_embeddedAssets = embeddedAssets;

			// Delete any external assets that are no longer part of the collection.
			foreach (var priorPersistedAsset in _persistedAssets)
			{
				if (priorPersistedAsset != null && AssetDatabase.Contains(priorPersistedAsset) && !persistedAssets.Contains(priorPersistedAsset))
				{
					_assetCollection.Remove(priorPersistedAsset);
					EditorUtility.SetDirty(_assetCollection);
					AssetUtility.DeleteAsset(priorPersistedAsset);
				}
			}
			_persistedAssets = persistedAssets;

			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();

			AssetDatabase.Refresh();

			if (!string.IsNullOrEmpty(oldGenerationName) && !string.IsNullOrEmpty(oldGenerationPath))
			{
				AssetUtility.RecursivelyDeleteEmptyFolders(AssetUtility.GetCanonicalPath(Path.Combine(oldGenerationPath, oldGenerationName)));
			}

			AssetDatabase.Refresh();
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
	}
}
