using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Experilous.Generation;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorCollection), typeof(FaceAttributesCategory), "Face Centroids")]
	public class FaceCentroidsGenerator : Generator
	{
		public bool flatten;

		[AutoSelect] public InputSlot surfaceInputSlot;
		[AutoSelect] public InputSlot topologyInputSlot;
		public InputSlot vertexPositionsInputSlot;

		public OutputSlot faceCentroidsDescriptor;

		protected override void Initialize()
		{
			// Inputs
			InputSlot.CreateOrResetRequired<Surface>(ref surfaceInputSlot, this);
			InputSlot.CreateOrResetRequired<Topology>(ref topologyInputSlot, this);
			InputSlot.CreateOrResetRequired<IVertexAttribute<Vector3>>(ref vertexPositionsInputSlot, this);

			// Fields
			flatten = false;

			// Outputs
			OutputSlot.CreateOrResetGrouped<IFaceAttribute<Vector3>>(ref faceCentroidsDescriptor, this, "Face Centroids", "Attributes");
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
				yield return faceCentroidsDescriptor;
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
				if (flatten || surface is PlanarSurface)
				{
					FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions, faceCentroids);
				}
				else if (surface is SphericalSurface)
				{
					var sphericalSurface = (SphericalSurface)surface;
					FaceAttributeUtility.CalculateSphericalFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions, sphericalSurface.radius, faceCentroids);
				}
				else
				{
					throw new System.NotImplementedException();
				}
			});

			faceCentroidsDescriptor.SetAsset(faceCentroids);

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
