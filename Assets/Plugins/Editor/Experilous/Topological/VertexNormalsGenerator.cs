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
	[Generator(typeof(TopologyGeneratorCollection), "Vertices/Normals")]
	public class VertexNormalsGenerator : Generator
	{
		public enum CalculationMethod
		{
			FromSurfaceNormal,
			FromVertexPositions,
			FromFacePositions,
			FromFaceNormals,
		}

		public CalculationMethod calculationMethod;

		[AutoSelect] public InputSlot topologyInputSlot;
		[AutoSelect] public InputSlot surfaceInputSlot;
		public InputSlot vertexPositionsInputSlot;
		public InputSlot facePositionsInputSlot;
		public InputSlot faceNormalsInputSlot;

		public OutputSlot vertexNormalsOutputSlot;

		protected override void Initialize()
		{
			// Inputs
			InputSlot.CreateOrResetRequired<Topology>(ref topologyInputSlot, this);
			InputSlot.CreateOrResetRequired<Surface>(ref surfaceInputSlot, this);
			InputSlot.CreateOrResetRequired<IVertexAttribute<Vector3>>(ref vertexPositionsInputSlot, this);
			InputSlot.CreateOrResetRequired<IFaceAttribute<Vector3>>(ref facePositionsInputSlot, this);
			InputSlot.CreateOrResetRequired<IFaceAttribute<Vector3>>(ref faceNormalsInputSlot, this);

			// Fields
			calculationMethod = CalculationMethod.FromSurfaceNormal;

			// Outputs
			OutputSlot.CreateOrResetGrouped<IVertexAttribute<Vector3>>(ref vertexNormalsOutputSlot, this, "Vertex Normals", "Attributes");
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
