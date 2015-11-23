using UnityEngine;
using UnityEditor;

namespace Experilous.Topological
{
	[CustomEditor(typeof(ManifoldMeshGenerator))]
	public class ManifoldMeshGeneratorEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			var generator = (ManifoldMeshGenerator)target;

			generator.ManifoldGenerator = EditorGUILayout.ObjectField("Manifold Generator", generator.ManifoldGenerator as Object, typeof(IManifoldProvider), true) as IManifoldProvider;
			generator.CentroidGenerator = EditorGUILayout.ObjectField("Centroid Generator", generator.CentroidGenerator as Object, typeof(IFaceAttributeProvider<Vector3>), true) as IFaceAttributeProvider<Vector3>;
			generator.FaceColors = EditorGUILayout.ObjectField("Face Colors", generator.FaceColors as Object, typeof(IFaceAttribute<Color>), true) as IFaceAttribute<Color>;
			generator.SubmeshPrefab = EditorGUILayout.ObjectField("Submesh Prefab", generator.SubmeshPrefab as Object, typeof(UniqueMesh), true) as UniqueMesh;
		}
	}
}
