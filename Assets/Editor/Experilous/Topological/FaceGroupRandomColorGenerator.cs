using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Experilous.Randomization;
using Experilous.Generation;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorCollection), typeof(FaceAttributesCategory), "Group Random Colors")]
	public class FaceGroupRandomColorGenerator : Generator
	{
		public InputSlot faceGroupCollectionInputSlot;
		public InputSlot faceGroupIndicesInputSlot;

		public RandomnessDescriptor randomness;

		public OutputSlot faceGroupColorsDescriptor;
		public OutputSlot faceColorsDescriptor;

		protected override void Initialize()
		{
			// Inputs
			InputSlot.CreateOrResetRequired<FaceGroupCollection>(ref faceGroupCollectionInputSlot, this);
			InputSlot.CreateOrResetRequired<IFaceAttribute<int>>(ref faceGroupIndicesInputSlot, this);

			// Fields
			randomness.Initialize(this);

			// Outputs
			OutputSlot.CreateOrResetGrouped<IFaceGroupAttribute<Color>>(ref faceGroupColorsDescriptor, this, "Random Face Group Colors", "Attributes");
			OutputSlot.CreateOrResetGrouped<IFaceAttribute<Color>>(ref faceColorsDescriptor, this, "Random Face Colors", "Attributes");
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
				yield return faceGroupColorsDescriptor;
				yield return faceColorsDescriptor;
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

			faceGroupColorsDescriptor.SetAsset(ColorFaceGroupAttribute.Create(faceGroupColorsArray).SetName((faceGroupCollection.name + " Colors").TrimStart()));
			faceColorsDescriptor.SetAsset(ColorFaceGroupLookupFaceAttribute.Create(
				faceGroupIndicesInputSlot.GetAsset<IntFaceAttribute>(),
				faceGroupColorsDescriptor.GetAsset<ColorFaceGroupAttribute>()).SetName("Face Colors"));

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
