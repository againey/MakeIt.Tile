using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Experilous
{
	public abstract class AssetGenerator : ScriptableObject
	{
		public AssetGeneratorCollection collection;

		public abstract IEnumerator BeginGeneration();

		public static TGenerator CreateInstance<TGenerator>(AssetGeneratorCollection collection) where TGenerator : AssetGenerator
		{
			var assetGeneratorAttribute = Utility.GetAttribute<AssetGeneratorAttribute>(typeof(TGenerator));
			if (assetGeneratorAttribute != null)
			{
				return CreateInstance<TGenerator>(collection, assetGeneratorAttribute.name);
			}
			else
			{
				return CreateInstance<TGenerator>(collection, typeof(TGenerator).GetPrettyName());
			}
		}

		public static TGenerator CreateInstance<TGenerator>(AssetGeneratorCollection collection, string name) where TGenerator : AssetGenerator
		{
			var generator = CreateInstance<TGenerator>();
			generator.collection = collection;
			generator.name = name;
			generator.hideFlags = HideFlags.HideInHierarchy;
			generator.Initialize();
			return generator;
		}

		protected abstract void Initialize(bool reset = true);

		protected virtual void OnEnable()
		{
			Initialize(false);
		}

		public virtual IEnumerable<AssetInputSlot> inputs
		{
			get
			{
				yield break; // A generator that creates everything from scratch.
			}
		}

		public virtual IEnumerable<AssetDescriptor> dependencies
		{
			get
			{
				foreach (var input in inputs)
				{
					if (input.source != null)
					{
						yield return input.source;
					}
				}
			}
		}

		public virtual IEnumerable<AssetDescriptor> outputs
		{
			get
			{
				yield break; // A generator that only modifies existing data, does not create anything new.
			}
		}

		public virtual IEnumerable<AssetReferenceDescriptor> references
		{
			get
			{
				yield break; // A generator whose outputs store references neither to the generator's inputs nor to the generator's other outputs has no references to report.
			}
		}

		public virtual bool canGenerate
		{
			get
			{
				foreach (var input in inputs)
				{
					if (!input.isOptional && input.source == null)
					{
						return false;
					}
				}

				return true;
			}
		}

		public virtual float estimatedGenerationTime
		{
			get
			{
				return 0.01f;
			}
		}

		public virtual void ResetDependency(AssetDescriptor dependency)
		{
			foreach (var input in inputs)
			{
				if (ReferenceEquals(input.source, dependency))
				{
					input.source = null;
				}
			}
		}

		public virtual void ResetAllDependencies()
		{
			foreach (var input in inputs)
			{
				input.source = null;
			}
		}

		public virtual void ResetAssets()
		{
			Initialize(false);
			foreach (var output in outputs)
			{
				output.ClearAsset(false);
			}
		}

		public virtual void Reset()
		{
			Initialize(true);
		}
		
		public virtual void Update()
		{
			Initialize(false);
		}

		public virtual void Pregenerate()
		{
		}

		public void EditScript()
		{
			AssetDatabase.OpenAsset(MonoScript.FromScriptableObject(this));
		}

		[MenuItem("CONTEXT/AssetGenerator/Reset", priority = 0)]
		public static void ResetFromMenu(MenuCommand menuCommand)
		{
			var generator = (AssetGenerator)menuCommand.context;
			generator.Reset();
		}

		[MenuItem("CONTEXT/AssetGenerator/Remove Generator", priority = 20)]
		public static void RemoveGenerator(MenuCommand menuCommand)
		{
			var generator = (AssetGenerator)menuCommand.context;
			generator.collection.Remove(generator);
		}

		[MenuItem("CONTEXT/AssetGenerator/Remove Generator", validate = true)]
		public static bool CanRemoveGenerator(MenuCommand menuCommand)
		{
			var generator = (AssetGenerator)menuCommand.context;
			return generator.collection.CanRemove(generator);
		}

		[MenuItem("CONTEXT/AssetGenerator/Move Up", priority = 21)]
		public static void MoveUp(MenuCommand menuCommand)
		{
			var generator = (AssetGenerator)menuCommand.context;
			generator.collection.MoveUp(generator);
		}

		[MenuItem("CONTEXT/AssetGenerator/Move Up", validate = true)]
		public static bool CanMoveUp(MenuCommand menuCommand)
		{
			var generator = (AssetGenerator)menuCommand.context;
			return generator.collection.CanMoveUp(generator);
		}

		[MenuItem("CONTEXT/AssetGenerator/Move Down", priority = 22)]
		public static void MoveDown(MenuCommand menuCommand)
		{
			var generator = (AssetGenerator)menuCommand.context;
			generator.collection.MoveDown(generator);
		}

		[MenuItem("CONTEXT/AssetGenerator/Move Down", validate = true)]
		public static bool CanMoveDown(MenuCommand menuCommand)
		{
			var generator = (AssetGenerator)menuCommand.context;
			return generator.collection.CanMoveDown(generator);
		}

		[MenuItem("CONTEXT/AssetGenerator/Edit Script", priority = 40)]
		public static void EditScript(MenuCommand menuCommand)
		{
			var generator = (AssetGenerator)menuCommand.context;
			generator.EditScript();
		}
	}
}
