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
					generator.topology = OnDependencyGUI("Topology", generator.topology, typeof(Topology), true);
					generator.facePositions = OnDependencyGUI("Face Positions", generator.facePositions, typeof(IFaceAttribute<Vector3>), false);
					break;
				case FaceNormalsGenerator.CalculationMethod.FromVertexPositions:
					generator.topology = OnDependencyGUI("Topology", generator.topology, typeof(Topology), true);
					generator.vertexPositions = OnDependencyGUI("Vertex Positions", generator.vertexPositions, false,
						(AssetDescriptor asset) =>
						{
							return
								typeof(IVertexAttribute<Vector3>).IsAssignableFrom(asset.assetType) ||
								typeof(IEdgeAttribute<Vector3>).IsAssignableFrom(asset.assetType);
						});
					break;
				case FaceNormalsGenerator.CalculationMethod.FromVertexNormals:
					generator.topology = OnDependencyGUI("Topology", generator.topology, typeof(Topology), true);
					generator.vertexNormals = OnDependencyGUI("Vertex Normals", generator.vertexNormals, typeof(IVertexAttribute<Vector3>), false);
					break;
				case FaceNormalsGenerator.CalculationMethod.FromSphericalFacePositions:
					generator.facePositions = OnDependencyGUI("Face Positions", generator.facePositions, typeof(IFaceAttribute<Vector3>), false);
					break;
			}
		}
	}
}
