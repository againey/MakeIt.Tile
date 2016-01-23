using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorBundle), typeof(FaceAttributesCategory), "Face Normals", after = typeof(FaceCentroidsGenerator))]
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

		public AssetDescriptor topology;
		public AssetDescriptor facePositions;
		public AssetDescriptor vertexPositions;
		public AssetDescriptor vertexNormals;

		public AssetDescriptor faceNormals;

		public static FaceNormalsGenerator CreateDefaultInstance(AssetGeneratorBundle bundle, string name)
		{
			var generator = CreateInstance<FaceNormalsGenerator>();
			generator.bundle = bundle;
			generator.name = name;
			generator.hideFlags = HideFlags.HideInHierarchy;
			return generator;
		}

		public override IEnumerable<AssetDescriptor> dependencies
		{
			get
			{
				switch (calculationMethod)
				{
					case CalculationMethod.FromFacePositions:
						if (topology != null) yield return topology;
						if (facePositions != null) yield return facePositions;
						break;
					case CalculationMethod.FromVertexPositions:
						if (topology != null) yield return topology;
						if (vertexPositions != null) yield return vertexPositions;
						break;
					case CalculationMethod.FromVertexNormals:
						if (topology != null) yield return topology;
						if (vertexNormals != null) yield return vertexNormals;
						break;
					case CalculationMethod.FromSphericalFacePositions:
						if (facePositions != null) yield return facePositions;
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
				if (faceNormals == null) faceNormals = AssetDescriptor.Create(this, typeof(IFaceAttribute<Vector3>), "Face Normals", "Attributes");
				yield return faceNormals;
			}
		}

		public override void ResetDependency(AssetDescriptor dependency)
		{
			if (dependency == null) throw new System.ArgumentNullException("dependency");
			if (!ResetMemberDependency(dependency, ref topology, ref facePositions, ref vertexPositions, ref vertexNormals))
			{
				throw new System.ArgumentException(string.Format("Generated asset \"{0}\" of type {1} is not a dependency of this face normals generator.", dependency.name, dependency.GetType().Name), "dependency");
			}
		}

		public override void Generate()
		{
			var topologyAsset = topology.GetAsset<Topology>();
			var faceNormalsAsset = Vector3FaceAttribute.CreateInstance(new Vector3[topologyAsset.internalFaces.Count], "Face Normals");
			switch (calculationMethod)
			{
				case CalculationMethod.FromFacePositions:
					FaceAttributeUtility.CalculateFaceNormalsFromFacePositions(topologyAsset.internalFaces, facePositions.GetAsset<IFaceAttribute<Vector3>>(), faceNormalsAsset);
					break;
				case CalculationMethod.FromVertexPositions:
					if (typeof(IVertexAttribute<Vector3>).IsAssignableFrom(vertexPositions.assetType))
					{
						FaceAttributeUtility.CalculateFaceNormalsFromVertexPositions(topologyAsset.internalFaces, vertexPositions.GetAsset<IVertexAttribute<Vector3>>(), faceNormalsAsset);
					}
					else if (typeof(IEdgeAttribute<Vector3>).IsAssignableFrom(vertexPositions.assetType))
					{
						FaceAttributeUtility.CalculateFaceNormalsFromVertexPositions(topologyAsset.internalFaces, vertexPositions.GetAsset<IEdgeAttribute<Vector3>>(), faceNormalsAsset);
					}
					break;
				case CalculationMethod.FromVertexNormals:
					FaceAttributeUtility.CalculateFaceNormalsFromVertexNormals(topologyAsset.internalFaces, vertexNormals.GetAsset<IVertexAttribute<Vector3>>(), faceNormalsAsset);
					break;
				case CalculationMethod.FromSphericalFacePositions:
					FaceAttributeUtility.CalculateSphericalFaceNormalsFromFacePositions(topologyAsset.internalFaces, facePositions.GetAsset<IFaceAttribute<Vector3>>(), faceNormalsAsset);
					break;
				default:
					throw new System.NotImplementedException();
			}

			faceNormals.SetAsset(faceNormalsAsset);
		}

		public override bool CanGenerate()
		{
			switch (calculationMethod)
			{
				case CalculationMethod.FromFacePositions: return (topology != null && facePositions != null);
				case CalculationMethod.FromVertexPositions: return (topology != null && vertexPositions != null);
				case CalculationMethod.FromVertexNormals: return (topology != null && vertexNormals != null);
				case CalculationMethod.FromSphericalFacePositions: return (facePositions != null);
				default: throw new System.NotImplementedException();
			}
		}
	}
}
