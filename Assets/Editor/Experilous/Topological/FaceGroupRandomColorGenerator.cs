using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Experilous.Randomization;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorCollection), typeof(FaceAttributesCategory), "Group Random Colors")]
	public class FaceGroupRandomColorGenerator : AssetGenerator
	{
		public AssetInputSlot faceGroupCollectionInputSlot;
		public AssetInputSlot faceGroupIndicesInputSlot;

		public AssetGeneratorRandomization randomization;

		public AssetDescriptor faceGroupColorsDescriptor;
		public AssetDescriptor faceColorsDescriptor;

		protected override void Initialize(bool reset = true)
		{
			// Inputs
			if (reset || faceGroupCollectionInputSlot == null) faceGroupCollectionInputSlot = AssetInputSlot.CreateRequired(this, typeof(FaceGroupCollection));
			if (reset || faceGroupIndicesInputSlot == null) faceGroupIndicesInputSlot = AssetInputSlot.CreateRequired(this, typeof(IFaceAttribute<int>));

			// Fields
			randomization.Initialize(this, reset);

			// Outputs
			if (reset || faceGroupColorsDescriptor == null) faceGroupColorsDescriptor = AssetDescriptor.CreateGrouped<IFaceGroupAttribute<Color>>(this, "Random Face Group Colors", "Attributes");
			if (reset || faceColorsDescriptor == null) faceColorsDescriptor = AssetDescriptor.CreateGrouped<IFaceAttribute<Color>>(this, "Random Face Colors", "Attributes");
		}

		public override IEnumerable<AssetInputSlot> inputs
		{
			get
			{
				yield return faceGroupCollectionInputSlot;
				yield return faceGroupIndicesInputSlot;
				foreach (var input in randomization.inputs) yield return input;
			}
		}

		public override IEnumerable<AssetDescriptor> outputs
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

			var random = new RandomUtility(randomization.GetRandomEngine());

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
