using UnityEngine;
using UnityEditor;

namespace Experilous.Topological
{
	[CustomEditor(typeof(EditorMeshGenerator))]
	public class EditorMeshGeneratorEditor : AssetGeneratorEditor
	{
		protected override void OnPropertiesGUI()
		{
			var generator = (EditorMeshGenerator)target;

			generator.topology = OnDependencyGUI("Topology", generator.topology, true);
			generator.vertexPositions = OnDependencyGUI("Vertex Positions", generator.vertexPositions, false);
			generator.faceCentroids = OnDependencyGUI("Face Centroids", generator.faceCentroids, false);
			generator.faceNormals = OnDependencyGUI("Face Normals", generator.faceNormals, false);

			generator.generatePrefab = EditorGUILayout.Toggle("Generate Prefab", generator.generatePrefab);
			if (generator.generatePrefab)
			{
				generator.meshGameObjectPrefab = (MeshFilter)EditorGUILayout.ObjectField("Mesh Renderer Prefab", generator.meshGameObjectPrefab, typeof(MeshFilter), false);
			}
		}
	}
}
