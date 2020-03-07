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
using MakeIt.Generate;

namespace MakeIt.Tile
{
	[Generator(typeof(TopologyGeneratorCollection), "Face Edge/Colors")]
	public class FaceEdgeColorsGenerator : Generator, ISerializationCallbackReceiver
	{
		public enum ColorSource
		{
			Constant,
			FaceColor,
		}

		public ColorSource colorSource;

		[AutoSelect] public InputSlot topologyInputSlot;
		public InputSlot faceColorsInputSlot;

		public Color constantColor;
		public float blendWeight;

		public OutputSlot faceEdgeColorsOutputSlot;

		protected override void Initialize()
		{
			// Inputs
			InputSlot.CreateOrResetRequired<Topology>(ref topologyInputSlot, this);
			InputSlot.CreateOrResetRequired<IFaceAttribute<Color>>(ref faceColorsInputSlot, this);

			// Fields
			colorSource = ColorSource.Constant;
			constantColor = Color.black;
			blendWeight = 0.5f;

			// Outputs
			OutputSlot.CreateOrResetGrouped<IEdgeAttribute<Color>>(ref faceEdgeColorsOutputSlot, this, "Face Edge Colors", "Attributes");
		}

		public void OnAfterDeserialize()
		{
			InputSlot.ResetAssetTypeIfNull<Topology>(topologyInputSlot);
			InputSlot.ResetAssetTypeIfNull<IFaceAttribute<Color>>(faceColorsInputSlot);

			OutputSlot.ResetAssetTypeIfNull<IEdgeAttribute<Color>>(faceEdgeColorsOutputSlot);
		}

		public void OnBeforeSerialize()
		{
		}

		protected override void OnUpdate()
		{
			topologyInputSlot.isActive = (colorSource == ColorSource.FaceColor);
			faceColorsInputSlot.isActive = (colorSource == ColorSource.FaceColor);
		}

		public override IEnumerable<InputSlot> inputs
		{
			get
			{
				yield return topologyInputSlot;
				yield return faceColorsInputSlot;
			}
		}

		public override IEnumerable<OutputSlot> outputs
		{
			get
			{
				yield return faceEdgeColorsOutputSlot;
			}
		}

		public override IEnumerator BeginGeneration()
		{
			switch (colorSource)
			{
				case ColorSource.Constant: GenerateConstant(); break;
				case ColorSource.FaceColor: GenerateFromFaceColors(); break;
				default: throw new System.NotImplementedException();
			}

			yield break;
		}

		private void GenerateConstant()
		{
			faceEdgeColorsOutputSlot.SetAsset(ConstantColorEdgeAttribute.Create(constantColor));
		}

		private void GenerateFromFaceColors()
		{
			var topology = topologyInputSlot.GetAsset<Topology>();
			var faceColors = faceColorsInputSlot.GetAsset<IFaceAttribute<Color>>();
			var faceEdgeColors = ColorEdgeAttribute.Create(topology.faceEdges.Count);

			foreach (var face in topology.internalFaces)
			{
				var faceColor = faceColors[face];
				var edgeColor = Color.Lerp(constantColor, faceColor, blendWeight);
				foreach (var edge in face.edges)
				{
					faceEdgeColors[edge] = edgeColor;
				}
			}

			faceEdgeColorsOutputSlot.SetAsset(faceEdgeColors);
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
