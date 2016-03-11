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
using Experilous.Generation;

namespace Experilous.Topological
{
	[Generator(typeof(TopologyGeneratorCollection), "Face/Normals")]
	public class FaceNormalsGenerator : Generator
	{
		public enum CalculationMethod
		{
			FromSurfaceNormal,
			FromVertexPositions,
			FromVertexNormals,
			FromFacePositions,
		}

		public CalculationMethod calculationMethod;

		[AutoSelect] public InputSlot topologyInputSlot;
		[AutoSelect] public InputSlot surfaceInputSlot;
		public InputSlot facePositionsInputSlot;
		public InputSlot vertexPositionsInputSlot;
		public InputSlot vertexNormalsInputSlot;

		public OutputSlot faceNormalsOutputSlot;

		protected override void Initialize()
		{
			// Inputs
			InputSlot.CreateOrResetRequired<Topology>(ref topologyInputSlot, this);
			InputSlot.CreateOrResetRequired<Surface>(ref surfaceInputSlot, this);
			InputSlot.CreateOrResetRequired<IVertexAttribute<Vector3>>(ref vertexPositionsInputSlot, this);
			InputSlot.CreateOrResetRequired<IVertexAttribute<Vector3>>(ref vertexNormalsInputSlot, this);
			InputSlot.CreateOrResetRequired<IFaceAttribute<Vector3>>(ref facePositionsInputSlot, this);

			// Fields
			calculationMethod = CalculationMethod.FromSurfaceNormal;

			// Outputs
			OutputSlot.CreateOrResetGrouped<IFaceAttribute<Vector3>>(ref faceNormalsOutputSlot, this, "Face Normals", "Attributes");
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
