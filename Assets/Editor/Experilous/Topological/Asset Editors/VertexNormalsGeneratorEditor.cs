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
					OnDependencyGUI("Topology", generator.topologyInputSlot, true);
					OnDependencyGUI("Vertex Positions", generator.vertexPositionsInputSlot);
					break;
				case VertexNormalsGenerator.CalculationMethod.FromFacePositions:
					OnDependencyGUI("Topology", generator.topologyInputSlot, true);
					OnDependencyGUI("Face Positions", generator.facePositionsInputSlot);
					break;
				case VertexNormalsGenerator.CalculationMethod.FromFaceNormals:
					OnDependencyGUI("Topology", generator.topologyInputSlot, true);
					OnDependencyGUI("Face Normals", generator.faceNormalsInputSlot);
					break;
				case VertexNormalsGenerator.CalculationMethod.FromSphericalVertexPositions:
					OnDependencyGUI("Vertex Positions", generator.vertexPositionsInputSlot);
					break;
			}
		}
	}
}
