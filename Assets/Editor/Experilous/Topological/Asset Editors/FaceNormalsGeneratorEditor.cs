using UnityEngine;
using UnityEditor;

namespace Experilous.Topological
{
	[CustomEditor(typeof(FaceNormalsGenerator))]
	public class FaceNormalsGeneratorEditor : AssetGeneratorEditor
	{
		protected override void OnPropertiesGUI()
		{
			var generator = (FaceNormalsGenerator)target;

			generator.calculationMethod = (FaceNormalsGenerator.CalculationMethod)EditorGUILayout.EnumPopup("Calculation Method", generator.calculationMethod);

			switch (generator.calculationMethod)
			{
				case FaceNormalsGenerator.CalculationMethod.FromFacePositions:
					generator.topology = OnDependencyGUI("Topology", generator.topology, true);
					generator.facePositions = OnDependencyGUI("Face Positions", generator.facePositions, false);
					break;
				case FaceNormalsGenerator.CalculationMethod.FromVertexPositions:
					generator.topology = OnDependencyGUI("Topology", generator.topology, true);
					generator.vertexPositions = OnDependencyGUI("Vertex Positions", generator.vertexPositions, false,
						(GeneratedAsset asset) =>
						{
							return
								typeof(IVertexAttribute<Vector3>).IsAssignableFrom(asset.generatedType) ||
								typeof(IEdgeAttribute<Vector3>).IsAssignableFrom(asset.generatedType);
						});
					break;
				case FaceNormalsGenerator.CalculationMethod.FromVertexNormals:
					generator.topology = OnDependencyGUI("Topology", generator.topology, true);
					generator.vertexNormals = OnDependencyGUI("Vertex Normals", generator.vertexNormals, false);
					break;
				case FaceNormalsGenerator.CalculationMethod.FromSphericalFacePositions:
					generator.facePositions = OnDependencyGUI("Face Positions", generator.facePositions, false);
					break;
			}
		}
	}
}
