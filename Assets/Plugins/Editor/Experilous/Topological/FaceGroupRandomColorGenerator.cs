/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Experilous.Randomization;
using Experilous.Generation;

namespace Experilous.Topological
{
	[Generator(typeof(TopologyGeneratorCollection), "Face Groups/Random Colors")]
	public class FaceGroupRandomColorGenerator : Generator
	{
		public InputSlot faceGroupCollectionInputSlot;
		public InputSlot faceGroupIndicesInputSlot;

		public RandomnessDescriptor randomness;

		public OutputSlot faceGroupColorsOutputSlot;
		public OutputSlot faceColorsOutputSlot;

		protected override void Initialize()
		{
			// Inputs
			InputSlot.CreateOrResetRequired<FaceGroupCollection>(ref faceGroupCollectionInputSlot, this);
			InputSlot.CreateOrResetRequired<IFaceAttribute<int>>(ref faceGroupIndicesInputSlot, this);

			// Fields
			randomness.Initialize(this);

			// Outputs
			OutputSlot.CreateOrResetGrouped<IFaceGroupAttribute<Color>>(ref faceGroupColorsOutputSlot, this, "Random Face Group Colors", "Attributes");
			OutputSlot.CreateOrResetGrouped<IFaceAttribute<Color>>(ref faceColorsOutputSlot, this, "Random Face Colors", "Attributes");
		}

		public override IEnumerable<InputSlot> inputs
		{
			get
			{
				yield return faceGroupCollectionInputSlot;
				yield return faceGroupIndicesInputSlot;
				yield return randomness.randomEngineSeedInputSlot;
			}
		}

		public override IEnumerable<OutputSlot> outputs
		{
			get
			{
				yield return faceGroupColorsOutputSlot;
				yield return faceColorsOutputSlot;
			}
		}

		public override IEnumerator BeginGeneration()
		{
			var faceGroupCollection = faceGroupCollectionInputSlot.GetAsset<FaceGroupCollection>();
			var faceGroups = faceGroupCollection.faceGroups;
			var faceGroupColorsArray = new Color[faceGroups.Length];

			var random = new RandomUtility(randomness.GetRandomEngine());

			for (int i = 0; i < faceGroups.Length; ++i)
			{
				faceGroupColorsArray[i] = new Color(random.ClosedFloatUnit(), random.ClosedFloatUnit(), random.ClosedFloatUnit());
			}

			faceGroupColorsOutputSlot.SetAsset(ColorFaceGroupAttribute.Create(faceGroupColorsArray).SetName((faceGroupCollection.name + " Colors").TrimStart()));
			faceColorsOutputSlot.SetAsset(ColorFaceGroupLookupFaceAttribute.Create(
				faceGroupIndicesInputSlot.GetAsset<IntFaceAttribute>(),
				faceGroupColorsOutputSlot.GetAsset<ColorFaceGroupAttribute>()).SetName("Face Colors"));

			yield break;
		}

		public override float estimatedGenerationTime
		{
			get
			{
				return 0.01f;
			}
		}
	}
}
