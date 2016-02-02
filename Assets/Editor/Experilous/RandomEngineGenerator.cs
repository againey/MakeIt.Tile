using System.Collections;
using System.Collections.Generic;
using Experilous.Randomization;

namespace Experilous
{
	[AssetGenerator(typeof(AssetGeneratorCollection), typeof(UtilitiesCategory), "Random Engine")]
	public class RandomEngineGenerator : AssetGenerator
	{
		[Label(null)] public AssetGeneratorRandomization randomization;

		public AssetDescriptor randomEngineDescriptor;

		protected override void Initialize(bool reset = true)
		{
			// Fields
			randomization.Initialize(this, reset);

			// Outputs
			if (reset || randomEngineDescriptor == null) randomEngineDescriptor = AssetDescriptor.CreateUnpersisted<IRandomEngine>(this, "Random Engine", true);
		}

		public override IEnumerable<AssetInputSlot> inputs
		{
			get
			{
				foreach (var input in randomization.inputs) yield return input;
			}
		}

		public override IEnumerable<AssetDescriptor> outputs
		{
			get
			{
				yield return randomEngineDescriptor;
			}
		}

		public override IEnumerator BeginGeneration()
		{
			randomEngineDescriptor.SetAsset(randomization.GetRandomEngine());
			yield break;
		}
	}
}
