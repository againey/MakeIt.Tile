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
	[Generator(typeof(TopologyGeneratorCollection), "Face/Normals")]
	public class FaceNormalsGenerator : Generator, ISerializationCallbackReceiver
	{
		public enum CalculationMethod
		{
			FromSurfaceNormal,
			FromVertexPositions,
			FromVertexNormals,
			FromFacePositions,
		}

		public CalculationMethod calculationMethod;

		[AutoSelect] public InputSlot surfaceInputSlot;
		[AutoSelect] public InputSlot topologyInputSlot;
		public InputSlot facePositionsInputSlot;
		public InputSlot vertexPositionsInputSlot;
		public InputSlot vertexNormalsInputSlot;

		public OutputSlot faceNormalsOutputSlot;

		protected override void Initialize()
		{
			// Inputs
			InputSlot.CreateOrResetRequired<Surface>(ref surfaceInputSlot, this);
			InputSlot.CreateOrResetRequired<Topology>(ref topologyInputSlot, this);
			InputSlot.CreateOrResetRequired<IVertexAttribute<Vector3>>(ref vertexPositionsInputSlot, this);
			InputSlot.CreateOrResetRequired<IVertexAttribute<Vector3>>(ref vertexNormalsInputSlot, this);
			InputSlot.CreateOrResetRequired<IFaceAttribute<Vector3>>(ref facePositionsInputSlot, this);

			// Fields
			calculationMethod = CalculationMethod.FromSurfaceNormal;

			// Outputs
			OutputSlot.CreateOrResetGrouped<IFaceAttribute<Vector3>>(ref faceNormalsOutputSlot, this, "Face Normals", "Attributes");
		}

		public void OnAfterDeserialize()
		{
			InputSlot.ResetAssetTypeIfNull<Surface>(surfaceInputSlot);
			InputSlot.ResetAssetTypeIfNull<Topology>(topologyInputSlot);
			InputSlot.ResetAssetTypeIfNull<IVertexAttribute<Vector3>>(vertexPositionsInputSlot);
			InputSlot.ResetAssetTypeIfNull<IVertexAttribute<Vector3>>(vertexNormalsInputSlot);
			InputSlot.ResetAssetTypeIfNull<IFaceAttribute<Vector3>>(facePositionsInputSlot);

			OutputSlot.ResetAssetTypeIfNull<IFaceAttribute<Vector3>>(faceNormalsOutputSlot);
		}

		public void OnBeforeSerialize()
		{
		}

		protected override void OnUpdate()
		{
			surfaceInputSlot.isActive = (calculationMethod == CalculationMethod.FromSurfaceNormal);
			vertexPositionsInputSlot.isActive = (calculationMethod == CalculationMethod.FromVertexPositions);
			vertexNormalsInputSlot.isActive = (calculationMethod == CalculationMethod.FromVertexNormals);
			facePositionsInputSlot.isActive = (calculationMethod == CalculationMethod.FromSurfaceNormal || calculationMethod == CalculationMethod.FromFacePositions);
		}

		public override IEnumerable<InputSlot> inputs
		{
			get
			{
				yield return topologyInputSlot;
				yield return surfaceInputSlot;
				yield return vertexPositionsInputSlot;
				yield return vertexNormalsInputSlot;
				yield return facePositionsInputSlot;
			}
		}

		public override IEnumerable<OutputSlot> outputs
		{
			get
			{
				yield return faceNormalsOutputSlot;
			}
		}

		public override IEnumerator BeginGeneration()
		{
			var topology = topologyInputSlot.GetAsset<Topology>();
			var faceNormals = Vector3FaceAttribute.Create(new Vector3[topology.internalFaces.Count]).SetName("Face Normals");

			yield return executive.GenerateConcurrently(() =>
			{
				switch (calculationMethod)
				{
					case CalculationMethod.FromSurfaceNormal:
						FaceAttributeUtility.CalculateFaceNormalsFromSurface(topology.internalFaces, surfaceInputSlot.GetAsset<Surface>(), facePositionsInputSlot.GetAsset<IFaceAttribute<Vector3>>(), faceNormals);
						break;
					case CalculationMethod.FromVertexPositions:
						FaceAttributeUtility.CalculateFaceNormalsFromVertexPositions(topology.internalFaces, vertexPositionsInputSlot.GetAsset<IVertexAttribute<Vector3>>(), faceNormals);
						break;
					case CalculationMethod.FromVertexNormals:
						FaceAttributeUtility.CalculateFaceNormalsFromVertexNormals(topology.internalFaces, vertexNormalsInputSlot.GetAsset<IVertexAttribute<Vector3>>(), faceNormals);
						break;
					case CalculationMethod.FromFacePositions:
						FaceAttributeUtility.CalculateFaceNormalsFromFacePositions(topology.internalFaces, facePositionsInputSlot.GetAsset<IFaceAttribute<Vector3>>(), faceNormals);
						break;
					default:
						throw new System.NotImplementedException();
				}
			});

			faceNormalsOutputSlot.SetAsset(faceNormals);

			yield break;
		}

		public override float estimatedGenerationTime
		{
			get
			{
				return 0.05f;
			}
		}
	}
}
