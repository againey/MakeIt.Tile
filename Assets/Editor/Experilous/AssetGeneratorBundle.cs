using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace Experilous
{
	public abstract class AssetGeneratorBundle : ScriptableObject
	{
		[SerializeField] private AssetGeneratorBundle _self;

		[SerializeField] protected List<AssetGenerator> _generators = new List<AssetGenerator>();
		[SerializeField] protected List<Object> _embeddedAssets = new List<Object>();
		[SerializeField] protected List<Object> _persistedAssets = new List<Object>();

		[System.Serializable] protected struct AssetGroup { public List<Object> assets; public static AssetGroup Create() { AssetGroup group; group.assets = new List<Object>(); return group; } }
		[System.Serializable] protected class PersistedAssetGroupDictionary : SerializableDictionary<string, AssetGroup> { }
		[SerializeField] protected PersistedAssetGroupDictionary _persistedGroups = new PersistedAssetGroupDictionary();

		protected string _location;

		public abstract string defaultName { get; }
		public string location { get { return _location; } }

		public string GetAssetPath(string assetName, string extension)
		{
			return AssetUtility.GetCanonicalPath(Path.Combine(_location, string.Format("{0} ({1}).{2}", name, assetName, extension)));
		}

		public string GetPersistedLocationAndName(out string location, out string name)
		{
			if (AssetDatabase.Contains(this))
			{
				var assetPath = AssetUtility.GetFullCanonicalAssetPath(this);

				location = AssetUtility.TrimProjectPath(Path.GetDirectoryName(assetPath));
				name = Path.GetFileNameWithoutExtension(assetPath);
				
				return assetPath;
			}
			else
			{
				location = AssetUtility.projectRelativeDataPath;
				name = defaultName;

				return "";
			}
		}

		public bool IsUnused(AssetDescriptor assetDescriptor)
		{
			if (assetDescriptor.isPersisted) return true;

			return false; //TODO graph analysis, looking for at least one downstream persisted asset.
		}

		public Object PersistAsset(Object oldAssetInstance, Object newAssetInstance, string name)
		{
			if (typeof(GameObject).IsInstanceOfType(newAssetInstance))
			{
				if (oldAssetInstance != null)
				{
					return PersistAsset((GameObject)oldAssetInstance, (GameObject)newAssetInstance, name);
				}
				else
				{
					return PersistAsset(null, (GameObject)newAssetInstance, name);
				}
			}

			var assetPath = GetAssetPath(name, "asset");

			// Is their an earlier instance of the asset already generated and persisted?
			if (oldAssetInstance != null && AssetDatabase.Contains(oldAssetInstance))
			{
				// Is the earlier asset and this asset exactly the same type?
				if (oldAssetInstance.GetType() == newAssetInstance.GetType())
				{
					// Check the earlier asset's path, and move or rename it to the new path if necessary.
					if (AssetUtility.GetProjectRelativeAssetPath(oldAssetInstance) != assetPath)
					{
						AssetUtility.MoveOrRenameAsset(oldAssetInstance, assetPath, false);
					}

					// Update the earlier asset to match the new instance.
					AssetUtility.UpdateAssetInPlace(newAssetInstance, oldAssetInstance);
					EditorUtility.SetDirty(oldAssetInstance);
					return oldAssetInstance;
				}
				// The earlier asset and the new instance have different types.
				else
				{
					// Delete the earlier asset outright; don't bother to update it in place.
					AssetUtility.DeleteAsset(oldAssetInstance);

					// Persist the new asset.
					AssetDatabase.CreateAsset(newAssetInstance, assetPath);
					return newAssetInstance;
				}
			}
			// There was no earlier persistent instance of the asset, so there is nothing to replace.
			else
			{
				// Persist the new asset.
				AssetDatabase.CreateAsset(newAssetInstance, assetPath);
				return newAssetInstance;
			}
		}

		public GameObject PersistAsset(GameObject oldAssetInstance, GameObject newAssetInstance, string name)
		{
			var assetPath = GetAssetPath(name, "prefab");

			// Is their an earlier instance of the asset already generated and persisted?
			if (oldAssetInstance != null && AssetDatabase.Contains(oldAssetInstance))
			{
				// Is the earlier asset and this asset exactly the same type?
				if (oldAssetInstance.GetType() == newAssetInstance.GetType())
				{
					// Check the earlier asset's path, and move or rename it to the new path if necessary.
					if (AssetUtility.GetProjectRelativeAssetPath(oldAssetInstance) != assetPath)
					{
						AssetUtility.MoveOrRenameAsset(oldAssetInstance, assetPath, false);
					}

					// Replace the earlier prefab with the new instance.
					var prefab = PrefabUtility.ReplacePrefab(newAssetInstance as GameObject, oldAssetInstance, ReplacePrefabOptions.ReplaceNameBased);
					// Destroy the template upon which the newly created prefab was based, since it will have been created within the scene.
					DestroyImmediate(newAssetInstance);
					return prefab;
				}
				// The earlier asset and the new instance have different types.
				else
				{
					// Delete the earlier asset outright; don't bother to update it in place.
					AssetUtility.DeleteAsset(oldAssetInstance);

					// Create a new prefab based on the supplied template.
					var prefab = PrefabUtility.CreatePrefab(assetPath, newAssetInstance as GameObject);
					// Destroy the template upon which the newly created prefab was based, since it will have been created within the scene.
					DestroyImmediate(newAssetInstance);
					return prefab;
				}
			}
			// There was no earlier persistent instance of the asset, so there is nothing to replace.
			else
			{
				// Create a new prefab based on the supplied template.
				var prefab = PrefabUtility.CreatePrefab(assetPath, newAssetInstance as GameObject);
				// Destroy the template upon which the newly created prefab was based, since it will have been created within the scene.
				DestroyImmediate(newAssetInstance);
				return prefab;
			}
		}

		public Object PersistAsset(Object oldAssetInstance, Object newAssetInstance, string name, string groupName)
		{
			if (string.IsNullOrEmpty(groupName))
			{
				return PersistAsset(oldAssetInstance, newAssetInstance, name);
			}

			if (typeof(GameObject).IsInstanceOfType(newAssetInstance))
			{
				throw new System.ArgumentException("Cannot store a GameObject asset within a group.", "groupName");
			}

			var assetPath = GetAssetPath(groupName, "asset");

			// Is their an earlier instance of the asset already generated and persisted?
			if (oldAssetInstance != null && AssetDatabase.Contains(oldAssetInstance))
			{
				// Is the earlier asset and this asset exactly the same type?
				if (oldAssetInstance.GetType() == newAssetInstance.GetType())
				{
					// Check the earlier asset's path, and move or rename it to the new path if necessary.
					if (AssetUtility.GetProjectRelativeAssetPath(oldAssetInstance) != assetPath)
					{
						AssetUtility.MoveOrRenameAsset(oldAssetInstance, assetPath, false);
					}

					// Update the earlier asset to match the new instance.
					AssetUtility.UpdateAssetInPlace(newAssetInstance, oldAssetInstance);
					oldAssetInstance.name = name;
					EditorUtility.SetDirty(oldAssetInstance);
					return oldAssetInstance;
				}
				// The earlier asset and the new instance have different types.
				else
				{
					// Check the earlier asset's path, and move or rename it to the new path if necessary.
					if (AssetUtility.GetProjectRelativeAssetPath(oldAssetInstance) != assetPath)
					{
						AssetUtility.MoveOrRenameAsset(oldAssetInstance, assetPath, false);
					}

					// Add the new asset instance to the same group that the old asset instance is in.
					AssetDatabase.AddObjectToAsset(newAssetInstance, oldAssetInstance);

					// Look up the list of assets already in the group, creating the list if necessary.
					AssetGroup persistedGroup;
					if (!_persistedGroups.TryGetValue(groupName, out persistedGroup))
					{
						persistedGroup = AssetGroup.Create();
						_persistedGroups.Add(groupName, persistedGroup);
					}

					// Remove the old instance from the group, and add the new instance.
					persistedGroup.assets.Remove(oldAssetInstance);
					persistedGroup.assets.Add(newAssetInstance);
					EditorUtility.SetDirty(this);

					// Delete the earlier asset outright; don't bother to update it in place.
					DestroyImmediate(oldAssetInstance);

					newAssetInstance.name = name;
					EditorUtility.SetDirty(newAssetInstance);

					return newAssetInstance;
				}
			}
			// There was no earlier persistent instance of the asset, so there is nothing to replace.
			else
			{
				// Look up the list of assets already in the group, creating the list if necessary.
				AssetGroup persistedGroup;
				if (!_persistedGroups.TryGetValue(groupName, out persistedGroup))
				{
					persistedGroup = AssetGroup.Create();
					_persistedGroups.Add(groupName, persistedGroup);
				}

				EditorUtility.SetDirty(this);

				// How we add the asset to the group depends on how many assets are already in the group.
				if (persistedGroup.assets.Count == 0)
				{
					// Persist the new asset as a singular asset rather than a group,
					// using the asset name rather than the group name.
					AssetDatabase.CreateAsset(newAssetInstance, GetAssetPath(name, "asset"));
					persistedGroup.assets.Add(newAssetInstance);
					newAssetInstance.name = name;
					EditorUtility.SetDirty(newAssetInstance);
					return newAssetInstance;
				}
				// The group already exists and has at least one asset in it.
				else
				{
					// Persist the new asset, adding it to the existing group.
					AssetDatabase.AddObjectToAsset(newAssetInstance, persistedGroup.assets[0]);
					persistedGroup.assets.Add(newAssetInstance);
					newAssetInstance.name = name;
					EditorUtility.SetDirty(newAssetInstance);

					// If there are now exactly two items in the group, then the group asset is probably still
					// named according to the first item, and needs to be renamed with the proper group name.
					if (persistedGroup.assets.Count == 2)
					{
						var existingName = persistedGroup.assets[0].name;
						AssetUtility.MoveOrRenameAsset(persistedGroup.assets[0], assetPath, false);
						persistedGroup.assets[0].name = existingName;
						EditorUtility.SetDirty(persistedGroup.assets[0]);
					}

					return newAssetInstance;
				}
			}
		}

		public void DepersistAsset(Object oldAssetInstance)
		{
			AssetUtility.DeleteAsset(oldAssetInstance);
		}

		public IList<AssetGenerator> generators { get {  return _generators.AsReadOnly(); } }

		protected void OnEnable()
		{
			if (_self != this)
			{
				if (_self != null)
				{
					//Asset was copied; all external references should be reset.
					foreach (var generator in _generators)
					{
						foreach (var output in generator.outputs)
						{
							if (output.asset != null)
							{
								if (AssetDatabase.Contains(output.asset))
								{
									DestroyImmediate(output.asset, true);
								}
								output.ClearAsset();
							}
						}
					}
				}

				_self = this;
			}

			string name;
			GetPersistedLocationAndName(out _location, out name);
			this.name = name;
		}

		public List<TAsset> GetMatchingGeneratedAssets<TAsset>(AssetGenerator excludedGenerator, bool excludeUnpersistedOutputs = false) where TAsset : AssetDescriptor
		{
			var matchingGeneratedAssets = new List<TAsset>();

			foreach (var generator in _generators)
			{
				if (!ReferenceEquals(generator, excludedGenerator))
				{
					foreach (var output in generator.outputs)
					{
						if (output is TAsset && output.isPersisted || !excludeUnpersistedOutputs)
						{
							matchingGeneratedAssets.Add((TAsset)output);
						}
					}
				}
			}

			return matchingGeneratedAssets;
		}

		public List<AssetDescriptor> GetMatchingGeneratedAssets(AssetGenerator excludedGenerator, System.Predicate<AssetDescriptor> predicate, bool excludeUnpersistedOutputs = false)
		{
			var matchingGeneratedAssets = new List<AssetDescriptor>();

			foreach (var generator in _generators)
			{
				if (!ReferenceEquals(generator, excludedGenerator))
				{
					foreach (var output in generator.outputs)
					{
						if (output != null && predicate(output) && (output.isPersisted || !excludeUnpersistedOutputs))
						{
							matchingGeneratedAssets.Add(output);
						}
					}
				}
			}

			return matchingGeneratedAssets;
		}

		public virtual void Add(AssetGenerator generator)
		{
			if (!CanAdd(generator)) throw new System.InvalidOperationException(string.Format("The asset generator \"{0}\" of type {1} cannot be added to this bundle.", generator.name, generator.GetType().Name));

			_generators.Add(generator);
		}

		public virtual bool CanAdd(AssetGenerator generator)
		{
			if (generator == null) throw new System.ArgumentNullException("generator");
			return !_generators.Contains(generator);
		}

		public virtual void Insert(int index, AssetGenerator generator)
		{
			if (!CanInsert(index, generator)) throw new System.InvalidOperationException(string.Format("The asset generator \"{0}\" of type {1} cannot be insert into this bundle.", generator.name, generator.GetType().Name));

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
			if (!CanRemove(generator)) throw new System.InvalidOperationException(string.Format("The asset generator \"{0}\" of type {1} cannot be removed from this bundle.", generator.name, generator.GetType().Name));

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
			if (!CanMoveUp(generator)) throw new System.InvalidOperationException(string.Format("The asset generator \"{0}\" of type {1} cannot be moved up within this bundle.", generator.name, generator.GetType().Name));

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
			if (!CanMoveDown(generator)) throw new System.InvalidOperationException(string.Format("The asset generator \"{0}\" of type {1} cannot be moved down within this bundle.", generator.name, generator.GetType().Name));

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
			AssetDatabase.CreateAsset(this, AssetUtility.GetCanonicalPath(Path.Combine(AssetUtility.selectedFolderOrDefault, base.name + ".asset")));
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

		public void Generate(string location, string name)
		{
			_location = location;
			this.name = name;

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
					throw new System.InvalidOperationException("A dependency cycle was found within the asset generators in this bundle, preventing the bundle from being generated.");
				}
				unorderedGenerators.RemoveRange(insertionIndex, removalCount);
			}

			// Move or rename the bundle asset if necessary.
			var bundlePath = AssetUtility.GetCanonicalPath(Path.Combine(location, name + ".asset"));
			if (AssetUtility.GetProjectRelativeAssetPath(this) != bundlePath)
			{
				AssetUtility.MoveOrRenameAsset(this, bundlePath, true);
			}

			// Generate all assets, moving or renaming the outputs if necessary.
			var persistedAssets = new List<Object>();
			foreach (var generator in orderedGenerators)
			{
				generator.Generate();
				foreach (var output in generator.outputs)
				{
					if (output.isPersisted)
					{
						persistedAssets.Add(output.asset);
					}
				}
			}

			// Embed objects within generator bundle.
			var embeddedAssets = new List<Object>();
			embeddedAssets.Add(this);

			foreach (var generator in _generators)
			{
				embeddedAssets.Add(generator);
				if (!AssetDatabase.Contains(generator))
				{
					AssetDatabase.AddObjectToAsset(generator, this);
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

			// Destroy any prior embedded objects that are no longer part of the bundle.
			foreach (var priorEmbeddedAsset in _embeddedAssets)
			{
				if (priorEmbeddedAsset != null && AssetDatabase.Contains(priorEmbeddedAsset) && !embeddedAssets.Contains(priorEmbeddedAsset))
				{
					DestroyImmediate(priorEmbeddedAsset, true);
				}
			}
			_embeddedAssets = embeddedAssets;

			// Delete any external assets that are no longer part of the bundle.
			foreach (var priorPersistedAsset in _persistedAssets)
			{
				if (priorPersistedAsset != null && AssetDatabase.Contains(priorPersistedAsset) && !persistedAssets.Contains(priorPersistedAsset))
				{
					AssetUtility.DeleteAsset(priorPersistedAsset);
				}
			}
			_persistedAssets = persistedAssets;

			// Remove any assets from asset groups that no longer exist or are no longer part of the bundle.
			foreach (var persistedGroup in _persistedGroups)
			{
				var groupAssets = persistedGroup.Value.assets;
				int index = 0;
				int endIndex = groupAssets.Count;
				while (index < endIndex)
				{
					// Check if the asset is no longer valid, if it either got destroyed (evaluates to false),
					// or if it simply is no longer included in the list of persisted assets.
					if (groupAssets[index] == null || !_persistedAssets.Contains(groupAssets[index]))
					{
						// If so, overwrite it with the last item in the list, and decrement the end index
						// so that we no to remove all items from that point onward when we're done.
						groupAssets[index] = groupAssets[--endIndex];
					}
					else
					{
						++index;
					}
				}

				// Remove all redundant items at the end due to removals.
				if (endIndex < groupAssets.Count)
				{
					groupAssets.RemoveRange(endIndex, groupAssets.Count - endIndex);

					// If there's only one item left, but there used to be more, then we need to rename
					// the asset to reflect its actual name, not the name of the group.
					if (groupAssets.Count == 1)
					{
						AssetUtility.MoveOrRenameAsset(groupAssets[0], GetAssetPath(groupAssets[0].name, "asset"), false);
					}
				}
			}

			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();
		}

		public virtual bool CanGenerate()
		{
			foreach (var generator in _generators)
			{
				if (!generator.CanGenerate()) return false;
			}

			return true;
		}
	}
}
