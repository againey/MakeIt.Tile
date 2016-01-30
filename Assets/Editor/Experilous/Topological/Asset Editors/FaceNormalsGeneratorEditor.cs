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
					OnDependencyGUI("Topology", generator.topologyInputSlot, true);
					OnDependencyGUI("Face Positions", generator.facePositionsInputSlot);
					break;
				case FaceNormalsGenerator.CalculationMethod.FromVertexPositions:
					OnDependencyGUI("Topology", generator.topologyInputSlot, true);
					OnDependencyGUI("Vertex Positions", generator.vertexPositionsInputSlot);
					break;
				case FaceNormalsGenerator.CalculationMethod.FromVertexNormals:
					OnDependencyGUI("Topology", generator.topologyInputSlot, true);
					OnDependencyGUI("Vertex Normals", generator.vertexNormalsInputSlot);
					break;
				case FaceNormalsGenerator.CalculationMethod.FromSphericalFacePositions:
					OnDependencyGUI("Face Positions", generator.facePositionsInputSlot);
					break;
			}
		}
	}
}
