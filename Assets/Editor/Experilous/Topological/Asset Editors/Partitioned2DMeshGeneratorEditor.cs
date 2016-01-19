using UnityEngine;
using UnityEditor;

namespace Experilous.Topological
{
	[CustomEditor(typeof(Partitioned2DMeshGenerator))]
	public class Partitioned2DMeshGeneratorEditor : AssetGeneratorEditor
	{
		protected override void OnPropertiesGUI()
		{
			var generator = (Partitioned2DMeshGenerator)target;

			generator.horizontalPartitionCount = EditorGUILayout.IntField("Horizontal Partitions", generator.horizontalPartitionCount);
			generator.verticalPartitionCount = EditorGUILayout.IntField("Vertical Partitions", generator.verticalPartitionCount);

			generator.topology = OnDependencyGUI("Topology", generator.topology, true);
			generator.faceIndexer2D = OnDependencyGUI("Face Indexer", generator.faceIndexer2D, true);
			generator.vertexPositions = OnDependencyGUI("Vertex Positions", generator.vertexPositions, false);
			generator.faceCentroids = OnDependencyGUI("Face Centroids", generator.faceCentroids, false);
			generator.faceNormals = OnDependencyGUI("Face Normals", generator.faceNormals, false);

			generator.horizontalPartitionCount = EditorGUILayout.IntField("Horizontal Partitions", generator.horizontalPartitionCount);
			generator.verticalPartitionCount = EditorGUILayout.IntField("Vertical Partitions", generator.verticalPartitionCount);

			generator.generatePrefab = EditorGUILayout.Toggle("Generate Prefab", generator.generatePrefab);
			if (generator.generatePrefab)
			{
				generator.meshGameObjectPrefab = (MeshFilter)EditorGUILayout.ObjectField("Mesh Renderer Prefab", generator.meshGameObjectPrefab, typeof(MeshFilter), false);
			}
		}
	}
}
