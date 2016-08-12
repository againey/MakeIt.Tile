/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System.Collections;
using System.Collections.Generic;
using Experilous.MakeIt.Random;
using Experilous.Core;

namespace Experilous.MakeIt.Generate
{
	[Generator(typeof(GeneratorExecutive), "Utility/Random Engine")]
	public class RandomEngineGenerator : Generator
	{
		[Label(null)] public RandomnessDescriptor randomness;

		public OutputSlot randomEngineOutputSlot;

		protected override void Initialize()
		{
			// Fields
			randomness.Initialize(this);

			// Outputs
			OutputSlot.CreateOrResetUnpersisted<IRandomEngine>(ref randomEngineOutputSlot, this, "Random Engine", true);
		}

		protected override void OnUpdate()
		{
			randomness.Update();
		}

		public override IEnumerable<InputSlot> inputs
		{
			get
			{
				yield return randomness.randomEngineSeedInputSlot;
			}
		}

		public override IEnumerable<OutputSlot> outputs
		{
			get
			{
				yield return randomEngineOutputSlot;
			}
		}

		public override IEnumerator BeginGeneration()
		{
			randomEngineOutputSlot.SetAsset(randomness.GetRandomEngine());
			yield break;
		}
	}
}
