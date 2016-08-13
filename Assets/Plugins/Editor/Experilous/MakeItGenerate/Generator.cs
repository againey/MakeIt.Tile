/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Experilous.Core;

namespace Experilous.MakeItGenerate
{
	public abstract class Generator : ScriptableObject
	{
		public GeneratorExecutive executive;

		public abstract IEnumerator BeginGeneration();

		public static TGenerator CreateInstance<TGenerator>(GeneratorExecutive executive) where TGenerator : Generator
		{
			var assetGeneratorAttribute = Utilities.GetAttribute<GeneratorAttribute>(typeof(TGenerator));
			if (assetGeneratorAttribute != null)
			{
				return CreateInstance<TGenerator>(executive, assetGeneratorAttribute.name);
			}
			else
			{
				return CreateInstance<TGenerator>(executive, typeof(TGenerator).GetPrettyName());
			}
		}

		public static TGenerator CreateInstance<TGenerator>(GeneratorExecutive executive, string name) where TGenerator : Generator
		{
			var generator = CreateInstance<TGenerator>();
			generator.executive = executive;
			generator.name = name;
			generator.hideFlags = HideFlags.HideInHierarchy;
			return generator;
		}

		public void Reset()
		{
			Initialize();
			Update();
		}

		protected abstract void Initialize();

		protected virtual void OnUpdate()
		{
		}

		protected virtual void OnEnable()
		{
		}

		public virtual IEnumerable<InputSlot> inputs
		{
			get
			{
				yield break; // A generator that creates everything from scratch.
			}
		}

		public virtual IEnumerable<InputSlot> activeInputs
		{
			get
			{
				foreach (var input in inputs)
				{
					if (input.isActive) yield return input;
				}
			}
		}

		public virtual IEnumerable<InputSlot> inactiveInputs
		{
			get
			{
				foreach (var input in inputs)
				{
					if (!input.isActive) yield return input;
				}
			}
		}

		public virtual IEnumerable<OutputSlot> dependencies
		{
			get
			{
				foreach (var input in activeInputs)
				{
					if (input.source != null)
					{
						yield return input.source;
					}
				}
			}
		}

		public virtual IEnumerable<OutputSlot> outputs
		{
			get
			{
				yield break; // A generator that only modifies existing data, does not create anything new.
			}
		}

		public virtual IEnumerable<OutputSlot> activeOutputs
		{
			get
			{
				foreach (var output in outputs)
				{
					if (output.isActive) yield return output;
				}
			}
		}

		public virtual IEnumerable<OutputSlot> inactiveOutputs
		{
			get
			{
				foreach (var output in outputs)
				{
					if (!output.isActive) yield return output;
				}
			}
		}

		public virtual IEnumerable<OutputSlot> visibleOutputs
		{
			get
			{
				return activeOutputs;
			}
		}

		public virtual IEnumerable<InternalSlotConnection> internalConnections
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
				foreach (var input in activeInputs)
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

		public virtual void ResetDependency(OutputSlot dependency)
		{
			foreach (var input in activeInputs)
			{
				if (ReferenceEquals(input.source, dependency))
				{
					input.Disconnect();
				}
			}
		}

		public void Update()
		{
			OnUpdate();

			foreach (var input in inactiveInputs)
			{
				input.Disconnect();
			}

			foreach (var output in inactiveOutputs)
			{
				output.DisconnectAll();
			}
		}
		
		public void EditScript()
		{
			AssetDatabase.OpenAsset(MonoScript.FromScriptableObject(this));
		}
	}
}
