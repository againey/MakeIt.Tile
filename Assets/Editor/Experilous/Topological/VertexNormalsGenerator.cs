using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorBundle), typeof(VertexAttributesCategory), "Vertex Normals")]
	public class VertexNormalsGenerator : AssetGenerator
	{
		public enum CalculationMethod
		{
			FromVertexPositions,
			FromFacePositions,
			FromFaceNormals,
			FromSphericalVertexPositions,
		}

		public CalculationMethod calculationMethod;

		public AssetDescriptor topology;
		public AssetDescriptor vertexPositions;
		public AssetDescriptor facePositions;
		public AssetDescriptor faceNormals;

		public AssetDescriptor vertexNormals;

		public static VertexNormalsGenerator CreateDefaultInstance(AssetGeneratorBundle bundle, string name)
		{
			var generator = CreateInstance<VertexNormalsGenerator>();
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
					case CalculationMethod.FromVertexPositions:
						if (topology != null) yield return topology;
						if (vertexPositions != null) yield return vertexPositions;
						break;
					case CalculationMethod.FromFacePositions:
						if (topology != null) yield return topology;
						if (facePositions != null) yield return facePositions;
						break;
					case CalculationMethod.FromFaceNormals:
						if (topology != null) yield return topology;
						if (faceNormals != null) yield return faceNormals;
						break;
					case CalculationMethod.FromSphericalVertexPositions:
						if (vertexPositions != null) yield return vertexPositions;
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
				if (vertexNormals == null) vertexNormals = AssetDescriptor.Create(this, typeof(IVertexAttribute<Vector3>), "Vertex Normals");
				yield return vertexNormals;
			}
		}

		public override void ResetDependency(AssetDescriptor dependency)
		{
			if (dependency == null) throw new System.ArgumentNullException("dependency");
			if (!ResetMemberDependency(dependency, ref topology, ref vertexPositions, ref facePositions, ref faceNormals))
			{
				throw new System.ArgumentException(string.Format("Generated asset \"{0}\" of type {1} is not a dependency of this vertex normals generator.", dependency.name, dependency.GetType().Name), "dependency");
			}
		}

		public override void Generate()
		{
			var topologyAsset = topology.GetAsset<Topology>();
			var vertexNormalsAsset = Vector3FaceAttribute.CreateInstance(new Vector3[topologyAsset.vertices.Count], "Vertex Normals");
			switch (calculationMethod)
			{
				case CalculationMethod.FromVertexPositions:
					VertexAttributeUtility.CalculateVertexNormalsFromVertexPositions(topologyAsset.vertices, vertexPositions.GetAsset<IVertexAttribute<Vector3>>(), vertexNormalsAsset);
					break;
				case CalculationMethod.FromFacePositions:
					VertexAttributeUtility.CalculateVertexNormalsFromFacePositions(topologyAsset.vertices, facePositions.GetAsset<IFaceAttribute<Vector3>>(), vertexNormalsAsset);
					break;
				case CalculationMethod.FromFaceNormals:
					VertexAttributeUtility.CalculateVertexNormalsFromFaceNormals(topologyAsset.vertices, faceNormals.GetAsset<IFaceAttribute<Vector3>>(), vertexNormalsAsset);
					break;
				case CalculationMethod.FromSphericalVertexPositions:
					VertexAttributeUtility.CalculateSphericalVertexNormalsFromVertexPositions(topologyAsset.vertices, vertexPositions.GetAsset<IVertexAttribute<Vector3>>(), vertexNormalsAsset);
					break;
				default:
					throw new System.NotImplementedException();
			}

			vertexNormals.SetAsset(vertexNormalsAsset);
		}

		public override bool CanGenerate()
		{
			switch (calculationMethod)
			{
				case CalculationMethod.FromVertexPositions: return (topology != null && vertexPositions != null);
				case CalculationMethod.FromFacePositions: return (topology != null && facePositions != null);
				case CalculationMethod.FromFaceNormals: return (topology != null && faceNormals != null);
				case CalculationMethod.FromSphericalVertexPositions: return (vertexPositions != null);
				default: throw new System.NotImplementedException();
			}
		}
	}
}
