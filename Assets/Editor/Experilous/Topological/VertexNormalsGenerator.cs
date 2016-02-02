using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorCollection), typeof(VertexAttributesCategory), "Vertex Normals")]
	public class VertexNormalsGenerator : AssetGenerator
	{
		public enum CalculationMethod
		{
			FromVertexPositions,
			FromFacePositions,
			FromFaceNormals,
			FromSphericalVertexPositions,
		}

		public CalculationMethod calculationMethod;

		[AutoSelect] public AssetInputSlot topologyInputSlot;
		public AssetInputSlot vertexPositionsInputSlot;
		public AssetInputSlot facePositionsInputSlot;
		public AssetInputSlot faceNormalsInputSlot;

		public AssetDescriptor vertexNormalsDescriptor;

		protected override void Initialize(bool reset = true)
		{
			// Inputs
			if (reset || topologyInputSlot == null) topologyInputSlot = AssetInputSlot.CreateRequired(this, typeof(Topology));
			if (reset || vertexPositionsInputSlot == null) vertexPositionsInputSlot = AssetInputSlot.CreateRequired(this, typeof(IVertexAttribute<Vector3>));
			if (reset || facePositionsInputSlot == null) facePositionsInputSlot = AssetInputSlot.CreateRequired(this, typeof(IFaceAttribute<Vector3>));
			if (reset || faceNormalsInputSlot == null) faceNormalsInputSlot = AssetInputSlot.CreateRequired(this, typeof(IFaceAttribute<Vector3>));

			// Outputs
			if (reset || vertexNormalsDescriptor == null) vertexNormalsDescriptor = AssetDescriptor.CreateGrouped<IVertexAttribute<Vector3>>(this, "Vertex Normals", "Attributes");
		}

		public override IEnumerable<AssetInputSlot> inputs
		{
			get
			{
				switch (calculationMethod)
				{
					case CalculationMethod.FromVertexPositions:
						yield return topologyInputSlot;
						yield return vertexPositionsInputSlot;
						break;
					case CalculationMethod.FromFacePositions:
						yield return topologyInputSlot;
						yield return facePositionsInputSlot;
						break;
					case CalculationMethod.FromFaceNormals:
						yield return topologyInputSlot;
						yield return faceNormalsInputSlot;
						break;
					case CalculationMethod.FromSphericalVertexPositions:
						yield return vertexPositionsInputSlot;
						break;
					default:
						throw new System.NotImplementedException();
				}
			}
		}

		public override IEnumerable<AssetDescriptor> outputs
		{
			get
			{
				yield return vertexNormalsDescriptor;
			}
		}

		public override IEnumerator BeginGeneration()
		{
			var topology = topologyInputSlot.GetAsset<Topology>();
			var vertexNormals = Vector3VertexAttribute.CreateInstance(new Vector3[topology.vertices.Count], "Vertex Normals");

			var waitHandle = collection.GenerateConcurrently(() =>
			{
				switch (calculationMethod)
				{
					case CalculationMethod.FromVertexPositions:
						VertexAttributeUtility.CalculateVertexNormalsFromVertexPositions(topology.vertices, vertexPositionsInputSlot.GetAsset<IVertexAttribute<Vector3>>(), vertexNormals);
						break;
					case CalculationMethod.FromFacePositions:
						VertexAttributeUtility.CalculateVertexNormalsFromFacePositions(topology.vertices, facePositionsInputSlot.GetAsset<IFaceAttribute<Vector3>>(), vertexNormals);
						break;
					case CalculationMethod.FromFaceNormals:
						VertexAttributeUtility.CalculateVertexNormalsFromFaceNormals(topology.vertices, faceNormalsInputSlot.GetAsset<IFaceAttribute<Vector3>>(), vertexNormals);
						break;
					case CalculationMethod.FromSphericalVertexPositions:
						VertexAttributeUtility.CalculateSphericalVertexNormalsFromVertexPositions(topology.vertices, vertexPositionsInputSlot.GetAsset<IVertexAttribute<Vector3>>(), vertexNormals);
						break;
					default:
						throw new System.NotImplementedException();
				}
			});
			while (waitHandle.WaitOne(10) == false)
			{
				yield return null;
			}

			vertexNormalsDescriptor.SetAsset(vertexNormals);

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
