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
using MakeIt.Core;

namespace MakeIt.Tile
{
	[Generator(typeof(TopologyGeneratorCollection), "Face/Centroids")]
	public class FaceCentroidsGenerator : Generator, ISerializationCallbackReceiver
	{
		public bool flatten;

		[AutoSelect] public InputSlot surfaceInputSlot;
		[AutoSelect] public InputSlot topologyInputSlot;
		public InputSlot vertexPositionsInputSlot;

		public OutputSlot faceCentroidsOutputSlot;

		protected override void Initialize()
		{
			// Inputs
			InputSlot.CreateOrResetRequired<Surface>(ref surfaceInputSlot, this);
			InputSlot.CreateOrResetRequired<Topology>(ref topologyInputSlot, this);
			InputSlot.CreateOrResetRequired<IVertexAttribute<Vector3>>(ref vertexPositionsInputSlot, this);

			// Fields
			flatten = false;

			// Outputs
			OutputSlot.CreateOrResetGrouped<IFaceAttribute<Vector3>>(ref faceCentroidsOutputSlot, this, "Face Centroids", "Attributes");
		}

		public void OnAfterDeserialize()
		{
			InputSlot.ResetAssetTypeIfNull<Surface>(surfaceInputSlot);
			InputSlot.ResetAssetTypeIfNull<Topology>(topologyInputSlot);
			InputSlot.ResetAssetTypeIfNull<IVertexAttribute<Vector3>>(vertexPositionsInputSlot);

			OutputSlot.ResetAssetTypeIfNull<IFaceAttribute<Vector3>>(faceCentroidsOutputSlot);
		}

		public void OnBeforeSerialize()
		{
		}

		public override IEnumerable<InputSlot> inputs
		{
			get
			{
				yield return surfaceInputSlot;
				yield return topologyInputSlot;
				yield return vertexPositionsInputSlot;
			}
		}

		public override IEnumerable<OutputSlot> outputs
		{
			get
			{
				yield return faceCentroidsOutputSlot;
			}
		}

		public override IEnumerator BeginGeneration()
		{
			var surface = surfaceInputSlot.GetAsset<Surface>();
			var topology = topologyInputSlot.GetAsset<Topology>();
			var vertexPositions = vertexPositionsInputSlot.GetAsset<IVertexAttribute<Vector3>>();
			var faceCentroids = PositionalFaceAttribute.Create(surface, new Vector3[topology.internalFaces.Count]).SetName("Face Centroids");

			yield return executive.GenerateConcurrently(() =>
			{
				if (flatten || surface is QuadrilateralSurface)
				{
					FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions, faceCentroids);
				}
				else if (surface is SphericalSurface)
				{
					var sphericalSurface = (SphericalSurface)surface;
					FaceAttributeUtility.CalculateSphericalFaceCentroidsFromVertexPositions(topology.internalFaces, sphericalSurface, vertexPositions, faceCentroids);
				}
				else
				{
					throw new System.NotImplementedException();
				}
			});

			faceCentroidsOutputSlot.SetAsset(faceCentroids);

			yield break;
		}

		public override float estimatedGenerationTime
		{
			get
			{
				return 0.025f;
			}
		}
	}
}
