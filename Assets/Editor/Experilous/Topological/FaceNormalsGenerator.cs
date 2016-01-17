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

		public TopologyGeneratedAsset topology;
		public Vector3FaceAttributeGeneratedAsset facePositions;
		public Vector3VertexAttributeGeneratedAsset vertexPositions;
		public Vector3VertexAttributeGeneratedAsset vertexNormals;

		public Vector3FaceAttributeGeneratedAsset faceNormals;

		public static FaceNormalsGenerator CreateDefaultInstance(AssetGeneratorBundle bundle, string name)
		{
			var generator = CreateInstance<FaceNormalsGenerator>();
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

		public override IEnumerable<GeneratedAsset> outputs
		{
			get
			{
				if (faceNormals == null) faceNormals = Vector3FaceAttributeGeneratedAsset.CreateDefaultInstance(this, "Face Normals");
				yield return faceNormals;
			}
		}

		public override void ResetDependency(GeneratedAsset dependency)
		{
			if (dependency == null) throw new System.ArgumentNullException("dependency");
			if (!ResetMemberDependency(dependency, ref topology, ref facePositions, ref vertexPositions, ref vertexNormals))
			{
				throw new System.ArgumentException(string.Format("Generated asset \"{0}\" of type {1} is not a dependency of this face normals generator.", dependency.name, dependency.GetType().Name), "dependency");
			}
		}

		public override void Generate(string location, string name)
		{
			var faceNormals = Vector3FaceAttribute.CreateInstance(new Vector3[topology.generatedInstance.internalFaces.Count], "Face Normals");
			switch (calculationMethod)
			{
				case CalculationMethod.FromFacePositions:
					FaceAttributeUtility.CalculateFaceNormalsFromFacePositions(topology.generatedInstance.internalFaces, facePositions.generatedInstance, faceNormals);
					break;
				case CalculationMethod.FromVertexPositions:
					FaceAttributeUtility.CalculateFaceNormalsFromVertexPositions(topology.generatedInstance.internalFaces, vertexPositions.generatedInstance, faceNormals);
					break;
				case CalculationMethod.FromVertexNormals:
					FaceAttributeUtility.CalculateFaceNormalsFromVertexNormals(topology.generatedInstance.internalFaces, this.vertexNormals.generatedInstance, faceNormals);
					break;
				case CalculationMethod.FromSphericalFacePositions:
					FaceAttributeUtility.CalculateSphericalFaceNormalsFromFacePositions(topology.generatedInstance.internalFaces, facePositions.generatedInstance, faceNormals);
					break;
				default:
					throw new System.NotImplementedException();
			}

			this.faceNormals.SetGeneratedInstance(location, name, faceNormals);
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
