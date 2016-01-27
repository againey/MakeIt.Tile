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

		public AssetDescriptor topology;
		public AssetDescriptor vertexPositions;
		public AssetDescriptor surfaceDescriptor;
		public AssetDescriptor edgeWrapData;

		public AssetDescriptor faceCentroids;
		public AssetDescriptor wrappedFaceCentroids;

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
				if (topology != null) yield return topology;
				if (vertexPositions != null) yield return vertexPositions;
				if (surfaceDescriptor != null) yield return surfaceDescriptor;
				if (edgeWrapData != null) yield return edgeWrapData;
			}
		}

		public override IEnumerable<AssetDescriptor> outputs
		{
			get
			{
				if (faceCentroids == null) faceCentroids = AssetDescriptor.Create(this, typeof(IFaceAttribute<Vector3>), "Face Centroids", "Attributes");
				if (wrappedFaceCentroids == null) wrappedFaceCentroids = AssetDescriptor.CreateOptional(this, typeof(IFaceAttribute<Vector3>), "Wrapped Face Centroids", false, "Attributes");
				yield return faceCentroids;
				yield return wrappedFaceCentroids;
			}
		}

		public override void ResetDependency(AssetDescriptor dependency)
		{
			if (dependency == null) throw new System.ArgumentNullException("dependency");
			if (!ResetMemberDependency(dependency, ref topology, ref vertexPositions, ref surfaceDescriptor, ref edgeWrapData))
			{
				throw new System.ArgumentException(string.Format("Generated asset \"{0}\" of type {1} is not a dependency of this face centroids generator.", dependency.name, dependency.GetType().Name), "dependency");
			}
		}

		public override void Generate()
		{
			var topologyAsset = topology.GetAsset<Topology>();
			var vertexPositionsAsset = vertexPositions.GetAsset<IVertexAttribute<Vector3>>();
			var faceCentroidsAsset = Vector3FaceAttribute.CreateInstance(new Vector3[topologyAsset.internalFaces.Count], "Face Centroids");
			switch (surfaceType)
			{
				case SurfaceType.Flat:
					FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topologyAsset.internalFaces, vertexPositionsAsset, faceCentroidsAsset);
					break;
				case SurfaceType.Spherical:
					FaceAttributeUtility.CalculateSphericalFaceCentroidsFromVertexPositions(topologyAsset.internalFaces, vertexPositionsAsset, 1f, faceCentroidsAsset);
					break;
				default:
					throw new System.NotImplementedException();
			}

			if (wrappedFaceCentroids.isUsed)
			{
				var surfaceDescriptorAsset = surfaceDescriptor.GetAsset<PlanarSurfaceDescriptor>();
				var edgeWrapDataAsset = edgeWrapData.GetAsset<EdgeWrapDataEdgeAttribute>();
				wrappedFaceCentroids.SetAsset(Vector3OffsetWrappedFaceAttribute.Create(edgeWrapDataAsset, faceCentroids.GetAsset<IFaceAttribute<Vector3>>(), surfaceDescriptorAsset));
			}

			faceCentroids.SetAsset(faceCentroidsAsset);
		}

		public override bool CanGenerate()
		{
			return (
				vertexPositions != null &&
				wrappedFaceCentroids != null && (!wrappedFaceCentroids.isUsed || surfaceDescriptor != null && edgeWrapData != null));
		}
	}
}
