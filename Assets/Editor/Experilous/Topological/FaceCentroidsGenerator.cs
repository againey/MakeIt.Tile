using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorCollection), typeof(FaceAttributesCategory), "Face Centroids")]
	public class FaceCentroidsGenerator : AssetGenerator
	{
		public enum SurfaceType
		{
			Flat,
			Spherical,
		}

		public SurfaceType surfaceType;

		[AutoSelect] public AssetInputSlot topologyInputSlot;
		public AssetInputSlot vertexPositionsInputSlot;
		[AutoSelect] [Label("Attribute Adapter")] public AssetInputSlot positionalAttributeAdapterInputSlot;

		public AssetDescriptor faceCentroidsDescriptor;
		public AssetDescriptor wrappedFaceCentroidsDescriptor;

		protected override void Initialize(bool reset = true)
		{
			// Inputs
			if (reset || topologyInputSlot == null) topologyInputSlot = AssetInputSlot.CreateRequired(this, typeof(Topology));
			if (reset || vertexPositionsInputSlot == null) vertexPositionsInputSlot = AssetInputSlot.CreateRequired(this, typeof(IVertexAttribute<Vector3>));
			if (reset || positionalAttributeAdapterInputSlot == null) positionalAttributeAdapterInputSlot = AssetInputSlot.CreateOptional(this, typeof(PositionalAttributeAdapter));

			// Outputs
			if (reset || faceCentroidsDescriptor == null) faceCentroidsDescriptor = AssetDescriptor.CreateGrouped<IFaceAttribute<Vector3>>(this, "Face Centroids", "Attributes");
			if (reset || wrappedFaceCentroidsDescriptor == null) wrappedFaceCentroidsDescriptor = AssetDescriptor.CreateGrouped<IFaceAttribute<Vector3>>(this, "Wrapped Face Centroids", "Attributes");
		}

		public override IEnumerable<AssetInputSlot> inputs
		{
			get
			{
				yield return topologyInputSlot;
				yield return vertexPositionsInputSlot;
				yield return positionalAttributeAdapterInputSlot;
			}
		}

		public override IEnumerable<AssetDescriptor> outputs
		{
			get
			{
				yield return faceCentroidsDescriptor;
				if (positionalAttributeAdapterInputSlot.source != null) yield return wrappedFaceCentroidsDescriptor;
			}
		}

		public override IEnumerable<AssetReferenceDescriptor> references
		{
			get
			{
				if (positionalAttributeAdapterInputSlot.source != null) yield return faceCentroidsDescriptor.ReferencedBy(wrappedFaceCentroidsDescriptor);
			}
		}

		public override IEnumerator BeginGeneration()
		{
			var topology = topologyInputSlot.GetAsset<Topology>();
			var vertexPositions = vertexPositionsInputSlot.GetAsset<IVertexAttribute<Vector3>>();
			var faceCentroids = Vector3FaceAttribute.CreateInstance(new Vector3[topology.internalFaces.Count], "Face Centroids");

			var waitHandle = collection.GenerateConcurrently(() =>
			{
				switch (surfaceType)
				{
					case SurfaceType.Flat:
						FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions, faceCentroids);
						break;
					case SurfaceType.Spherical:
						FaceAttributeUtility.CalculateSphericalFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions, 1f, faceCentroids);
						break;
					default:
						throw new System.NotImplementedException();
				}
			});
			while (waitHandle.WaitOne(10) == false)
			{
				yield return null;
			}

			faceCentroidsDescriptor.SetAsset(faceCentroids);

			if (positionalAttributeAdapterInputSlot.source != null)
			{
				var positionalAttributeAdapter = positionalAttributeAdapterInputSlot.GetAsset<PositionalAttributeAdapter>();
				var wrappedFaceCentroids = positionalAttributeAdapter.Adapt(faceCentroidsDescriptor.GetAsset<IFaceAttribute<Vector3>>());
				wrappedFaceCentroidsDescriptor.SetAsset(wrappedFaceCentroids);
			}

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
