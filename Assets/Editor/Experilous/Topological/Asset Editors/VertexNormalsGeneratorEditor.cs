using UnityEngine;
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
					generator.topology = OnDependencyGUI("Topology", generator.topology, typeof(Topology), true);
					generator.vertexPositions = OnDependencyGUI("Vertex Positions", generator.vertexPositions, typeof(IVertexAttribute<Vector3>), false);
					break;
				case VertexNormalsGenerator.CalculationMethod.FromFacePositions:
					generator.topology = OnDependencyGUI("Topology", generator.topology, typeof(Topology), true);
					generator.facePositions = OnDependencyGUI("Face Positions", generator.facePositions, typeof(IFaceAttribute<Vector3>), false);
					break;
				case VertexNormalsGenerator.CalculationMethod.FromFaceNormals:
					generator.topology = OnDependencyGUI("Topology", generator.topology, typeof(Topology), true);
					generator.faceNormals = OnDependencyGUI("Face Normals", generator.faceNormals, typeof(IFaceAttribute<Vector3>), false);
					break;
				case VertexNormalsGenerator.CalculationMethod.FromSphericalVertexPositions:
					generator.vertexPositions = OnDependencyGUI("Vertex Positions", generator.vertexPositions, typeof(IVertexAttribute<Vector3>), false);
					break;
			}
		}
	}
}
