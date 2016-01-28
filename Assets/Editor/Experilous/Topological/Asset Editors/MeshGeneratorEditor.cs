using UnityEngine;
using UnityEditor;

namespace Experilous.Topological
{
	[CustomEditor(typeof(MeshGenerator))]
	public class MeshGeneratorEditor : AssetGeneratorEditor
	{
		protected override void OnPropertiesGUI()
		{
			var generator = (MeshGenerator)target;

			generator.sourceType = (MeshGenerator.SourceType)EditorGUILayout.EnumPopup("Source Type", generator.sourceType);

			switch (generator.sourceType)
			{
				case MeshGenerator.SourceType.InternalFaces:
					generator.topology = OnDependencyGUI("Topology", generator.topology, typeof(Topology), true);
					break;
				case MeshGenerator.SourceType.FaceGroupCollection:
					generator.faceGroupCollection = OnDependencyGUI("Face Groups", generator.faceGroupCollection, typeof(FaceGroupCollection), false);
					break;
				case MeshGenerator.SourceType.FaceGroup:
					generator.faceGroup = OnDependencyGUI("Face Group", generator.faceGroup, typeof(FaceGroup), false);
					break;
			}

			generator.vertexPositions = OnDependencyGUI("Vertex Positions", generator.vertexPositions, typeof(IVertexAttribute<Vector3>), false);
			generator.faceCentroids = OnDependencyGUI("Face Centroids", generator.faceCentroids, typeof(IFaceAttribute<Vector3>), false);
			generator.faceNormals = OnDependencyGUI("Face Normals", generator.faceNormals, typeof(IFaceAttribute<Vector3>), false);
			generator.faceColors = OnDependencyGUI("Face Colors", generator.faceColors, typeof(IFaceAttribute<Color>), false);

			generator.centerOnGroupAverage = EditorGUILayout.Toggle("Center On Group", generator.centerOnGroupAverage);
		}
	}
}
