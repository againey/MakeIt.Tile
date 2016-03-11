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
	[Generator(typeof(TopologyGeneratorCollection), "Face/Colors")]
	public class FaceColorsGenerator : Generator
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
				randomness.randomEngineSeedInputSlot.isActive = false;
			}
		}

		public override IEnumerable<InputSlot> inputs
		{
			get
			{
				yield return topologyInputSlot;
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

			var random = new RandomUtility(randomness.GetRandomEngine());

			foreach (var face in topology.internalFaces)
			{
				faceColors[face] = new Color(random.ClosedFloatUnit(), random.ClosedFloatUnit(), random.ClosedFloatUnit());
			}

			faceColorsOutputSlot.SetAsset(faceColors);
		}

		private void GenerateRandomPerGroup()
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
