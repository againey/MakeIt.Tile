/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MakeIt.Generate;
using MakeIt.Core;

namespace MakeIt.Tile
{
	[Generator(typeof(TopologyGeneratorCollection), "Vertex/Normals")]
	public class VertexNormalsGenerator : Generator, ISerializationCallbackReceiver
	{
		public enum CalculationMethod
		{
			FromSurfaceNormal,
			FromVertexPositions,
			FromFacePositions,
			FromFaceNormals,
		}

		public CalculationMethod calculationMethod;

		[AutoSelect] public InputSlot surfaceInputSlot;
		[AutoSelect] public InputSlot topologyInputSlot;
		public InputSlot vertexPositionsInputSlot;
		public InputSlot facePositionsInputSlot;
		public InputSlot faceNormalsInputSlot;

		public OutputSlot vertexNormalsOutputSlot;

		protected override void Initialize()
		{
			// Inputs
			InputSlot.CreateOrResetRequired<Surface>(ref surfaceInputSlot, this);
			InputSlot.CreateOrResetRequired<Topology>(ref topologyInputSlot, this);
			InputSlot.CreateOrResetRequired<IVertexAttribute<Vector3>>(ref vertexPositionsInputSlot, this);
			InputSlot.CreateOrResetRequired<IFaceAttribute<Vector3>>(ref facePositionsInputSlot, this);
			InputSlot.CreateOrResetRequired<IFaceAttribute<Vector3>>(ref faceNormalsInputSlot, this);

			// Fields
			calculationMethod = CalculationMethod.FromSurfaceNormal;

			// Outputs
			OutputSlot.CreateOrResetGrouped<IVertexAttribute<Vector3>>(ref vertexNormalsOutputSlot, this, "Vertex Normals", "Attributes");
		}

		public void OnAfterDeserialize()
		{
			InputSlot.ResetAssetTypeIfNull<Surface>(surfaceInputSlot);
			InputSlot.ResetAssetTypeIfNull<Topology>(topologyInputSlot);
			InputSlot.ResetAssetTypeIfNull<IVertexAttribute<Vector3>>(vertexPositionsInputSlot);
			InputSlot.ResetAssetTypeIfNull<IFaceAttribute<Vector3>>(facePositionsInputSlot);
			InputSlot.ResetAssetTypeIfNull<IFaceAttribute<Vector3>>(faceNormalsInputSlot);

			OutputSlot.ResetAssetTypeIfNull<IVertexAttribute<Vector3>>(vertexNormalsOutputSlot);
		}

		public void OnBeforeSerialize()
		{
		}

		protected override void OnUpdate()
		{
			surfaceInputSlot.isActive = (calculationMethod == CalculationMethod.FromSurfaceNormal);
			vertexPositionsInputSlot.isActive = (calculationMethod == CalculationMethod.FromSurfaceNormal || calculationMethod == CalculationMethod.FromVertexPositions);
			facePositionsInputSlot.isActive = (calculationMethod == CalculationMethod.FromFacePositions);
			faceNormalsInputSlot.isActive = (calculationMethod == CalculationMethod.FromFaceNormals);
		}

		public override IEnumerable<InputSlot> inputs
		{
			get
			{
				yield return topologyInputSlot;
				yield return surfaceInputSlot;
				yield return vertexPositionsInputSlot;
				yield return vertexPositionsInputSlot;
				yield return facePositionsInputSlot;
				yield return faceNormalsInputSlot;
			}
		}

		public override IEnumerable<OutputSlot> outputs
		{
			get
			{
				yield return vertexNormalsOutputSlot;
			}
		}

		public override IEnumerator BeginGeneration()
		{
			var topology = topologyInputSlot.GetAsset<Topology>();
			var vertexNormals = Vector3VertexAttribute.Create(new Vector3[topology.vertices.Count]).SetName("Vertex Normals");

			yield return executive.GenerateConcurrently(() =>
			{
				switch (calculationMethod)
				{
					case CalculationMethod.FromSurfaceNormal:
						VertexAttributeUtility.CalculateVertexNormalsFromSurface(topology.vertices, surfaceInputSlot.GetAsset<Surface>(), vertexPositionsInputSlot.GetAsset<IVertexAttribute<Vector3>>(), vertexNormals);
						break;
					case CalculationMethod.FromVertexPositions:
						VertexAttributeUtility.CalculateVertexNormalsFromVertexPositions(topology.vertices, vertexPositionsInputSlot.GetAsset<IVertexAttribute<Vector3>>(), vertexNormals);
						break;
					case CalculationMethod.FromFacePositions:
						VertexAttributeUtility.CalculateVertexNormalsFromFacePositions(topology.vertices, facePositionsInputSlot.GetAsset<IFaceAttribute<Vector3>>(), vertexNormals);
						break;
					case CalculationMethod.FromFaceNormals:
						VertexAttributeUtility.CalculateVertexNormalsFromFaceNormals(topology.vertices, faceNormalsInputSlot.GetAsset<IFaceAttribute<Vector3>>(), vertexNormals);
						break;
					default:
						throw new System.NotImplementedException();
				}
			});

			vertexNormalsOutputSlot.SetAsset(vertexNormals);

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
