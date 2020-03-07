/******************************************************************************\
* Copyright Andy Gainey                                                        *
*                                                                              *
* Licensed under the Apache License, Version 2.0 (the "License");              *
* you may not use this file except in compliance with the License.             *
* You may obtain a copy of the License at                                      *
*                                                                              *
*     http://www.apache.org/licenses/LICENSE-2.0                               *
*                                                                              *
* Unless required by applicable law or agreed to in writing, software          *
* distributed under the License is distributed on an "AS IS" BASIS,            *
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.     *
* See the License for the specific language governing permissions and          *
* limitations under the License.                                               *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MakeIt.Random;
using MakeIt.Generate;
using MakeIt.Core;

namespace MakeIt.Tile
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
#if MAKEIT_COLORFUL_2_0_OR_NEWER
				faceColors[face] = random.ColorRGB();
#else
				faceColors[face] = new Color(random.FloatCC(), random.FloatCC(), random.FloatCC());
#endif
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
#if MAKEIT_COLORFUL_2_0_OR_NEWER
				faceGroupColorsArray[i] = random.ColorRGB();
#else
				faceGroupColorsArray[i] = new Color(random.FloatCC(), random.FloatCC(), random.FloatCC());
#endif
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
