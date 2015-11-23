using UnityEngine;
using UnityEditor;

namespace Experilous.Topological
{
	[CustomEditor(typeof(ManifoldCentroidGenerator))]
	public class ManifoldCentroidGeneratorEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			var generator = (ManifoldCentroidGenerator)target;

			generator.ManifoldGenerator = EditorGUILayout.ObjectField("Manifold Generator", generator.ManifoldGenerator as Object, typeof(IManifoldProvider), true) as IManifoldProvider;
		}
	}
}
