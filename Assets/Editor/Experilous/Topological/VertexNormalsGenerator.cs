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

		public TopologyGeneratedAsset topology;
		public Vector3VertexAttributeGeneratedAsset vertexPositions;
		public Vector3VertexAttributeGeneratedAsset facePositions;
		public Vector3FaceAttributeGeneratedAsset faceNormals;

		public Vector3VertexAttributeGeneratedAsset vertexNormals;

		public static VertexNormalsGenerator CreateDefaultInstance(AssetGeneratorBundle bundle, string name)
		{
			var generator = CreateInstance<VertexNormalsGenerator>();
			generator.bundle = bundle;
			generator.name = name;
			generator.hideFlags = HideFlags.HideInHierarchy;
			return generator;
		}

		public override IEnumerable<GeneratedAsset> dependencies
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

		public override IEnumerable<GeneratedAsset> outputs
		{
			get
			{
				if (vertexNormals == null) vertexNormals = Vector3VertexAttributeGeneratedAsset.CreateDefaultInstance(this, "Vertex Normals");
				yield return vertexNormals;
			}
		}

		public override void ResetDependency(GeneratedAsset dependency)
		{
			if (dependency == null) throw new System.ArgumentNullException("dependency");
			if (!ResetMemberDependency(dependency, ref topology, ref vertexPositions, ref facePositions, ref faceNormals))
			{
				throw new System.ArgumentException(string.Format("Generated asset \"{0}\" of type {1} is not a dependency of this vertex normals generator.", dependency.name, dependency.GetType().Name), "dependency");
			}
		}

		public override void Generate(string location, string name)
		{
			Vector3[] faceNormalsArray;
			switch (calculationMethod)
			{
				case CalculationMethod.FromVertexPositions:
					faceNormalsArray = Manifold.CalculateVertexNormalsFromVertexPositions(topology.generatedInstance.vertices, vertexPositions.generatedInstance.array);
					break;
				case CalculationMethod.FromFacePositions:
					faceNormalsArray = Manifold.CalculateVertexNormalsFromFacePositions(topology.generatedInstance.vertices, facePositions.generatedInstance.array);
					break;
				case CalculationMethod.FromFaceNormals:
					faceNormalsArray = Manifold.CalculateVertexNormalsFromFaceNormals(topology.generatedInstance.vertices, faceNormals.generatedInstance.array);
					break;
				case CalculationMethod.FromSphericalVertexPositions:
					faceNormalsArray = SphericalManifoldUtility.CalculateVertexNormalsFromVertexPositions(vertexPositions.generatedInstance.array);
					break;
				default:
					throw new System.NotImplementedException();
			}

			var instance = Vector3FaceAttribute.CreateInstance(faceNormalsArray, "Face Normals");
			faceNormals.SetGeneratedInstance(location, name, instance);
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
