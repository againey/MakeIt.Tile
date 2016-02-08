using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorCollection), typeof(FaceAttributesCategory), "Face Centroids")]
	public class FaceCentroidsGenerator : AssetGenerator
	{
		public bool flatten;

		[AutoSelect] public AssetInputSlot surfaceInputSlot;
		[AutoSelect] public AssetInputSlot topologyInputSlot;
		public AssetInputSlot vertexPositionsInputSlot;

		public AssetDescriptor faceCentroidsDescriptor;

		protected override void Initialize(bool reset = true)
		{
			// Inputs
			if (reset || surfaceInputSlot == null) surfaceInputSlot = AssetInputSlot.CreateRequired(this, typeof(Surface));
			if (reset || topologyInputSlot == null) topologyInputSlot = AssetInputSlot.CreateRequired(this, typeof(Topology));
			if (reset || vertexPositionsInputSlot == null) vertexPositionsInputSlot = AssetInputSlot.CreateRequired(this, typeof(IVertexAttribute<Vector3>));

			// Outputs
			if (reset || faceCentroidsDescriptor == null) faceCentroidsDescriptor = AssetDescriptor.CreateGrouped<IFaceAttribute<Vector3>>(this, "Face Centroids", "Attributes");
		}

		public override IEnumerable<AssetInputSlot> inputs
		{
			get
			{
				yield return surfaceInputSlot;
				yield return topologyInputSlot;
				yield return vertexPositionsInputSlot;
			}
		}

		public override IEnumerable<AssetDescriptor> outputs
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

			var waitHandle = collection.GenerateConcurrently(() =>
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
			while (waitHandle.WaitOne(10) == false)
			{
				yield return null;
			}

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
