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
					OnDependencyGUI("Topology", generator.topologyInputSlot, true);
					break;
				case MeshGenerator.SourceType.FaceGroupCollection:
					OnDependencyGUI("Face Groups", generator.faceGroupCollectionInputSlot);
					break;
				case MeshGenerator.SourceType.FaceGroup:
					OnDependencyGUI("Face Group", generator.faceGroupInputSlot);
					break;
			}

			OnDependencyGUI("Vertex Positions", generator.vertexPositionsInputSlot);
			OnDependencyGUI("Face Centroids", generator.faceCentroidsInputSlot);
			OnDependencyGUI("Face Normals", generator.faceNormalsInputSlot);
			OnDependencyGUI("Face Colors", generator.faceColorsInputSlot);

			generator.centerOnGroupAverage = EditorGUILayout.Toggle("Center On Group", generator.centerOnGroupAverage);
		}
	}
}
