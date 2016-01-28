using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorBundle), typeof(FaceAttributesCategory), "Face Centroids")]
	public class FaceCentroidsGenerator : AssetGenerator
	{
		public enum SurfaceType
		{
			Flat,
			Spherical,
		}

		public SurfaceType surfaceType;

		public AssetDescriptor topologyDescriptor;
		public AssetDescriptor vertexPositionsDescriptor;
		public AssetDescriptor positionalAttributeAdapterDescriptor;

		public AssetDescriptor faceCentroidsDescriptor;
		public AssetDescriptor wrappedFaceCentroidsDescriptor;

		public static FaceCentroidsGenerator CreateDefaultInstance(AssetGeneratorBundle bundle, string name)
		{
			var generator = CreateInstance<FaceCentroidsGenerator>();
			generator.bundle = bundle;
			generator.name = name;
			generator.hideFlags = HideFlags.HideInHierarchy;
			return generator;
		}

		public override IEnumerable<AssetDescriptor> dependencies
		{
			get
			{
				if (topologyDescriptor != null) yield return topologyDescriptor;
				if (vertexPositionsDescriptor != null) yield return vertexPositionsDescriptor;
				if (positionalAttributeAdapterDescriptor != null) yield return positionalAttributeAdapterDescriptor;
			}
		}

		public override IEnumerable<AssetDescriptor> outputs
		{
			get
			{
				if (faceCentroidsDescriptor == null) faceCentroidsDescriptor = AssetDescriptor.Create(this, typeof(IFaceAttribute<Vector3>), "Face Centroids", "Attributes");
				if (wrappedFaceCentroidsDescriptor == null) wrappedFaceCentroidsDescriptor = AssetDescriptor.CreateOptional(this, typeof(IFaceAttribute<Vector3>), "Wrapped Face Centroids", false, "Attributes");
				yield return faceCentroidsDescriptor;

				if (positionalAttributeAdapterDescriptor != null && positionalAttributeAdapterDescriptor.asset != null)
				{
					yield return wrappedFaceCentroidsDescriptor;
				}
			}
		}

		public override void ResetDependency(AssetDescriptor dependency)
		{
			if (dependency == null) throw new System.ArgumentNullException("dependency");
			if (!ResetMemberDependency(dependency, ref topologyDescriptor, ref vertexPositionsDescriptor, ref positionalAttributeAdapterDescriptor))
			{
				throw new System.ArgumentException(string.Format("Generated asset \"{0}\" of type {1} is not a dependency of this face centroids generator.", dependency.name, dependency.GetType().Name), "dependency");
			}
		}

		public override void Generate()
		{
			var topology = topologyDescriptor.GetAsset<Topology>();
			var vertexPositions = vertexPositionsDescriptor.GetAsset<IVertexAttribute<Vector3>>();
			var faceCentroids = Vector3FaceAttribute.CreateInstance(new Vector3[topology.internalFaces.Count], "Face Centroids");
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

			faceCentroidsDescriptor.SetAsset(faceCentroids);

			if (positionalAttributeAdapterDescriptor != null && positionalAttributeAdapterDescriptor.asset != null)
			{
				var positionalAttributeAdapter = positionalAttributeAdapterDescriptor.GetAsset<PositionalAttributeAdapter>();
				var wrappedFaceCentroids = positionalAttributeAdapter.Adapt(faceCentroidsDescriptor.GetAsset<IFaceAttribute<Vector3>>());
				wrappedFaceCentroidsDescriptor.SetAsset(wrappedFaceCentroids);
			}
		}

		public override bool CanGenerate()
		{
			return (
				vertexPositionsDescriptor != null);
		}
	}
}
