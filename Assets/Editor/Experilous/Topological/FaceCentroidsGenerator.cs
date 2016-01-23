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

		public AssetDescriptor faceCentroids;

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
			}
		}

		public override IEnumerable<AssetDescriptor> outputs
		{
			get
			{
				if (faceCentroids == null) faceCentroids = AssetDescriptor.Create(this, typeof(IFaceAttribute<Vector3>), "Face Centroids", "Attributes");
				yield return faceCentroids;
			}
		}

		public override void ResetDependency(AssetDescriptor dependency)
		{
			if (dependency == null) throw new System.ArgumentNullException("dependency");
			if (!ResetMemberDependency(dependency, ref topology, ref vertexPositions))
			{
				throw new System.ArgumentException(string.Format("Generated asset \"{0}\" of type {1} is not a dependency of this face centroids generator.", dependency.name, dependency.GetType().Name), "dependency");
			}
		}

		public override void Generate()
		{
			var topologyAsset = topology.GetAsset<Topology>();
			var faceCentroidsAsset = Vector3FaceAttribute.CreateInstance(new Vector3[topologyAsset.internalFaces.Count], "Face Centroids");
			switch (surfaceType)
			{
				case SurfaceType.Flat:
					if (typeof(IVertexAttribute<Vector3>).IsAssignableFrom(vertexPositions.assetType))
					{
						FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topologyAsset.internalFaces, (IVertexAttribute<Vector3>)vertexPositions.asset, faceCentroidsAsset);
					}
					else if (typeof(IEdgeAttribute<Vector3>).IsAssignableFrom(vertexPositions.assetType))
					{
						FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topologyAsset.internalFaces, (IEdgeAttribute<Vector3>)vertexPositions.asset, faceCentroidsAsset);
					}
					break;
				case SurfaceType.Spherical:
					if (typeof(IVertexAttribute<Vector3>).IsAssignableFrom(vertexPositions.assetType))
					{
						FaceAttributeUtility.CalculateSphericalFaceCentroidsFromVertexPositions(topologyAsset.internalFaces, (IVertexAttribute<Vector3>)vertexPositions.asset, 1f, faceCentroidsAsset);
					}
					else if (typeof(IEdgeAttribute<Vector3>).IsAssignableFrom(vertexPositions.assetType))
					{
						FaceAttributeUtility.CalculateSphericalFaceCentroidsFromVertexPositions(topologyAsset.internalFaces, (IEdgeAttribute<Vector3>)vertexPositions.asset, 1f, faceCentroidsAsset);
					}
					break;
				default:
					throw new System.NotImplementedException();
			}

			faceCentroids.SetAsset(faceCentroidsAsset);
		}

		public override bool CanGenerate()
		{
			return vertexPositions != null;
		}
	}
}
