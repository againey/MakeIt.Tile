/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/
#if false
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Experilous.MakeItRandom;
using Experilous.MakeItGenerate;
using Experilous.Core;
using Experilous.Topologies;

namespace Experilous.MakeItTile
{
	[Generator(typeof(TopologyGeneratorCollection), "Face/Colors")]
	public class FaceColorsGenerator : Generator, ISerializationCallbackReceiver
	{
		public enum ColorSource
		{
			Constant,
			RandomPerFace,
			RandomPerGroup,
		}

		public ColorSource colorSource;

		[AutoSelect] public InputSlot topologyInputSlot;

		public Color constantColor;

		public InputSlot faceGroupCollectionInputSlot;
		public InputSlot faceGroupIndicesInputSlot;

		public RandomnessDescriptor randomness;

		public OutputSlot faceGroupColorsOutputSlot;
		public OutputSlot faceColorsOutputSlot;

		protected override void Initialize()
		{
			// Inputs
			InputSlot.CreateOrResetRequired<Topology>(ref topologyInputSlot, this);
			InputSlot.CreateOrResetRequired<FaceGroupCollection>(ref faceGroupCollectionInputSlot, this);
			InputSlot.CreateOrResetRequired<IFaceAttribute<int>>(ref faceGroupIndicesInputSlot, this);

			// Fields
			colorSource = ColorSource.Constant;
			constantColor = Color.white;
			randomness.Initialize(this);

			// Outputs
			OutputSlot.CreateOrResetGrouped<IFaceGroupAttribute<Color>>(ref faceGroupColorsOutputSlot, this, "Face Group Colors", "Attributes");
			OutputSlot.CreateOrResetGrouped<IFaceAttribute<Color>>(ref faceColorsOutputSlot, this, "Face Colors", "Attributes");
		}

		public void OnAfterDeserialize()
		{
			InputSlot.ResetAssetTypeIfNull<Topology>(topologyInputSlot);
			InputSlot.ResetAssetTypeIfNull<FaceGroupCollection>(faceGroupCollectionInputSlot);
			InputSlot.ResetAssetTypeIfNull<IFaceAttribute<int>>(faceGroupIndicesInputSlot);
			randomness.ResetIfBroken(this);

			OutputSlot.ResetAssetTypeIfNull<IFaceGroupAttribute<Color>>(faceGroupColorsOutputSlot);
			OutputSlot.ResetAssetTypeIfNull<IFaceAttribute<Color>>(faceColorsOutputSlot);
		}

		public void OnBeforeSerialize()
		{
		}

		protected override void OnUpdate()
		{
			topologyInputSlot.isActive = (colorSource == ColorSource.RandomPerFace);
			faceGroupCollectionInputSlot.isActive = (colorSource == ColorSource.RandomPerGroup);
			faceGroupIndicesInputSlot.isActive = (colorSource == ColorSource.RandomPerGroup);

			if (colorSource == ColorSource.RandomPerFace || colorSource == ColorSource.RandomPerGroup)
			{
				randomness.Update();
			}
			else
			{
				randomness.randomSeedInputSlot.isActive = false;
			}
		}

		public override IEnumerable<InputSlot> inputs
		{
			get
			{
				yield return topologyInputSlot;
				yield return faceGroupCollectionInputSlot;
				yield return faceGroupIndicesInputSlot;
				yield return randomness.randomSeedInputSlot;
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

		public override IEnumerable<InternalSlotConnection> internalConnections
		{
			get
			{
				if (colorSource == ColorSource.RandomPerGroup)
				{
					if (faceGroupIndicesInputSlot.source != null) yield return faceColorsOutputSlot.Uses(faceGroupIndicesInputSlot.source);
					yield return faceColorsOutputSlot.Uses(faceGroupColorsOutputSlot);
				}
			}
		}

		public override IEnumerator BeginGeneration()
		{
			switch (colorSource)
			{
				case ColorSource.Constant: GenerateConstant(); break;
				case ColorSource.RandomPerFace: GenerateRandomPerFace(); break;
				case ColorSource.RandomPerGroup: GenerateRandomPerGroup(); break;
				default: throw new System.NotImplementedException();
			}

			yield break;
		}

		private void GenerateConstant()
		{
			faceColorsOutputSlot.SetAsset(ConstantColorFaceAttribute.Create(constantColor));
		}

		private void GenerateRandomPerFace()
		{
			var topology = topologyInputSlot.GetAsset<Topology>();
			var faceColors = ColorFaceAttribute.Create(topology.internalFaces.Count);

			var random = randomness.GetRandom();

			foreach (var face in topology.internalFaces)
			{
				faceColors[face] = random.ColorRGB();
			}

			faceColorsOutputSlot.SetAsset(faceColors);
		}

		private void GenerateRandomPerGroup()
		{
			var faceGroupCollection = faceGroupCollectionInputSlot.GetAsset<FaceGroupCollection>();
			var faceGroups = faceGroupCollection.faceGroups;
			var faceGroupColorsArray = new Color[faceGroups.Length];

			var random = randomness.GetRandom();

			for (int i = 0; i < faceGroups.Length; ++i)
			{
				faceGroupColorsArray[i] = random.ColorRGB();
			}

			faceGroupColorsOutputSlot.SetAsset(ColorFaceGroupAttribute.Create(faceGroupColorsArray).SetName((faceGroupCollection.name + " Colors").TrimStart()));
			faceColorsOutputSlot.SetAsset(ColorFaceGroupLookupFaceAttribute.Create(
				faceGroupIndicesInputSlot.GetAsset<IntFaceAttribute>(),
				faceGroupColorsOutputSlot.GetAsset<ColorFaceGroupAttribute>()).SetName("Face Colors"));
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
#endif