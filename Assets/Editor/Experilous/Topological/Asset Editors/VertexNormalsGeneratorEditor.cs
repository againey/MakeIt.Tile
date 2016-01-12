using UnityEditor;

namespace Experilous.Topological
{
	[CustomEditor(typeof(VertexNormalsGenerator))]
	public class VertexNormalsGeneratorEditor : AssetGeneratorEditor
	{
		protected override void OnPropertiesGUI()
		{
			var generator = (VertexNormalsGenerator)target;

			generator.calculationMethod = (VertexNormalsGenerator.CalculationMethod)EditorGUILayout.EnumPopup("Calculation Method", generator.calculationMethod);

			switch (generator.calculationMethod)
			{
				case VertexNormalsGenerator.CalculationMethod.FromVertexPositions:
					generator.topology = OnDependencyGUI("Topology", generator.topology, true);
					generator.vertexPositions = OnDependencyGUI("Vertex Positions", generator.vertexPositions, false);
					break;
				case VertexNormalsGenerator.CalculationMethod.FromFacePositions:
					generator.topology = OnDependencyGUI("Topology", generator.topology, true);
					generator.facePositions = OnDependencyGUI("Face Positions", generator.facePositions, false);
					break;
				case VertexNormalsGenerator.CalculationMethod.FromFaceNormals:
					generator.topology = OnDependencyGUI("Topology", generator.topology, true);
					generator.faceNormals = OnDependencyGUI("Face Normals", generator.faceNormals, false);
					break;
				case VertexNormalsGenerator.CalculationMethod.FromSphericalVertexPositions:
					generator.vertexPositions = OnDependencyGUI("Vertex Positions", generator.vertexPositions, false);
					break;
			}
		}
	}
}
