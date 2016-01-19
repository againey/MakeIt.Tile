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
		[SerializeField] protected List<Object> _generatedAssets = new List<Object>();

		public virtual string nameSuffix { get { return " (Generator)"; } }
		public abstract string defaultName { get; }
		public string GetDefaultFullName() { return defaultName + nameSuffix; }
		public string GetFullName(string name) { return name + nameSuffix; }

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
							if (output.generatedInstance != null)
							{
								if (AssetDatabase.Contains(output.generatedInstance))
								{
									DestroyImmediate(output.generatedInstance, true);
								}
								output.ClearGeneratedInstance();
							}
						}
					}
				}

				_self = this;
			}
		}

		public List<TAsset> GetMatchingGeneratedAssets<TAsset>(AssetGenerator excludedGenerator, bool excludeDisabledOutputs = true) where TAsset : GeneratedAsset
		{
			var matchingGeneratedAssets = new List<TAsset>();

			foreach (var generator in _generators)
			{
				if (!ReferenceEquals(generator, excludedGenerator))
				{
					foreach (var output in generator.outputs)
					{
						if (output is TAsset && output.isEnabled || !excludeDisabledOutputs)
						{
							matchingGeneratedAssets.Add((TAsset)output);
						}
					}
				}
			}

			return matchingGeneratedAssets;
		}

		public List<GeneratedAsset> GetMatchingGeneratedAssets(AssetGenerator excludedGenerator, System.Predicate<GeneratedAsset> predicate, bool excludeDisabledOutputs = true)
		{
			var matchingGeneratedAssets = new List<GeneratedAsset>();

			foreach (var generator in _generators)
			{
				if (!ReferenceEquals(generator, excludedGenerator))
				{
					foreach (var output in generator.outputs)
					{
						if (output != null && predicate(output) && output.isEnabled || !excludeDisabledOutputs)
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
			AssetDatabase.CreateAsset(this, AssetUtility.GetCanonicalPath(Path.Combine(AssetUtility.selectedFolderOrDefault, name + ".asset")));
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
						if (!orderedGenerators.Contains(dependency.assetGenerator))
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
			var bundlePath = AssetUtility.GetCanonicalPath(Path.Combine(location, GetFullName(name) + ".asset"));
			if (AssetUtility.GetProjectRelativeAssetPath(this) != bundlePath)
			{
				AssetUtility.MoveOrRenameAsset(this, bundlePath, true);
			}

			// Generate all assets, moving or renaming the outputs if necessary.
			var generatedAssets = new List<Object>();
			foreach (var generator in orderedGenerators)
			{
				generator.Generate(location, name);
				foreach (var output in generator.outputs)
				{
					if (output.isEnabled)
					{
						generatedAssets.Add(output.generatedInstance);
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
			foreach (var priorGeneratedAsset in _generatedAssets)
			{
				if (priorGeneratedAsset != null && AssetDatabase.Contains(priorGeneratedAsset) && !generatedAssets.Contains(priorGeneratedAsset))
				{
					AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(priorGeneratedAsset));
				}
			}
			_generatedAssets = generatedAssets;

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
