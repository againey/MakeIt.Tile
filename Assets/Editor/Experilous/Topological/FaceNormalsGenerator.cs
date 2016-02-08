﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorCollection), typeof(FaceAttributesCategory), "Face Normals", after = typeof(FaceCentroidsGenerator))]
	public class FaceNormalsGenerator : AssetGenerator
	{
		public enum CalculationMethod
		{
			FromFacePositions,
			FromVertexPositions,
			FromVertexNormals,
			FromSphericalFacePositions,
		}

		public CalculationMethod calculationMethod;

		[AutoSelect] public AssetInputSlot topologyInputSlot;
		public AssetInputSlot facePositionsInputSlot;
		public AssetInputSlot vertexPositionsInputSlot;
		public AssetInputSlot vertexNormalsInputSlot;

		public AssetDescriptor faceNormalsDescriptor;

		protected override void Initialize(bool reset = true)
		{
			// Inputs
			if (reset || topologyInputSlot == null) topologyInputSlot = AssetInputSlot.CreateRequired(this, typeof(Topology));
			if (reset || facePositionsInputSlot == null) facePositionsInputSlot = AssetInputSlot.CreateRequired(this, typeof(IFaceAttribute<Vector3>));
			if (reset || vertexPositionsInputSlot == null) vertexPositionsInputSlot = AssetInputSlot.CreateRequired(this, typeof(IVertexAttribute<Vector3>));
			if (reset || vertexNormalsInputSlot == null) vertexNormalsInputSlot = AssetInputSlot.CreateRequired(this, typeof(IVertexAttribute<Vector3>));

			// Outputs
			if (reset || faceNormalsDescriptor == null) faceNormalsDescriptor = AssetDescriptor.CreateGrouped<IFaceAttribute<Vector3>>(this, "Face Normals", "Attributes");
		}

		public override IEnumerable<AssetInputSlot> inputs
		{
			get
			{
				switch (calculationMethod)
				{
					case CalculationMethod.FromFacePositions:
						yield return topologyInputSlot;
						yield return facePositionsInputSlot;
						break;
					case CalculationMethod.FromVertexPositions:
						yield return topologyInputSlot;
						yield return vertexPositionsInputSlot;
						break;
					case CalculationMethod.FromVertexNormals:
						yield return topologyInputSlot;
						yield return vertexNormalsInputSlot;
						break;
					case CalculationMethod.FromSphericalFacePositions:
						yield return facePositionsInputSlot;
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
				yield return faceNormalsDescriptor;
			}
		}

		public override IEnumerator BeginGeneration()
		{
			var topology = topologyInputSlot.GetAsset<Topology>();
			var faceNormals = Vector3FaceAttribute.Create(new Vector3[topology.internalFaces.Count]).SetName("Face Normals");

			var waitHandle = collection.GenerateConcurrently(() =>
			{
				switch (calculationMethod)
				{
					case CalculationMethod.FromFacePositions:
						FaceAttributeUtility.CalculateFaceNormalsFromFacePositions(topology.internalFaces, facePositionsInputSlot.GetAsset<IFaceAttribute<Vector3>>(), faceNormals);
						break;
					case CalculationMethod.FromVertexPositions:
						FaceAttributeUtility.CalculateFaceNormalsFromVertexPositions(topology.internalFaces, vertexPositionsInputSlot.GetAsset<IVertexAttribute<Vector3>>(), faceNormals);
						break;
					case CalculationMethod.FromVertexNormals:
						FaceAttributeUtility.CalculateFaceNormalsFromVertexNormals(topology.internalFaces, vertexNormalsInputSlot.GetAsset<IVertexAttribute<Vector3>>(), faceNormals);
						break;
					case CalculationMethod.FromSphericalFacePositions:
						FaceAttributeUtility.CalculateSphericalFaceNormalsFromFacePositions(topology.internalFaces, facePositionsInputSlot.GetAsset<IFaceAttribute<Vector3>>(), faceNormals);
						break;
					default:
						throw new System.NotImplementedException();
				}
			});
			while (waitHandle.WaitOne(10) == false)
			{
				yield return null;
			}

			faceNormalsDescriptor.SetAsset(faceNormals);

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
