using System.Collections;
using System.Collections.Generic;
using Experilous.Randomization;

namespace Experilous.Generation
{
	[AssetGenerator(typeof(GeneratorExecutive), typeof(UtilitiesCategory), "Random Engine")]
	public class RandomEngineGenerator : Generator
	{
		[Label(null)] public RandomnessDescriptor randomness;

		public OutputSlot randomEngineDescriptor;

		protected override void Initialize()
		{
			// Fields
			randomness.Initialize(this);

			// Outputs
			OutputSlot.CreateOrResetUnpersisted<IRandomEngine>(ref randomEngineDescriptor, this, "Random Engine", true);
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
				yield return randomEngineDescriptor;
			}
		}

		public override IEnumerator BeginGeneration()
		{
			randomEngineDescriptor.SetAsset(randomness.GetRandomEngine());
			yield break;
		}
	}
}
