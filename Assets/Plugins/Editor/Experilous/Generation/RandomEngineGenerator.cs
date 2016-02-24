/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using System.Collections;
using System.Collections.Generic;
using Experilous.Randomization;

namespace Experilous.Generation
{
	[AssetGenerator(typeof(GeneratorExecutive), typeof(UtilitiesCategory), "Random Engine")]
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
