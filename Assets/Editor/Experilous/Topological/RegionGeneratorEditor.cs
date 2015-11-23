using UnityEngine;
using UnityEditor;

namespace Experilous.Topological
{
	[CustomEditor(typeof(RegionGenerator))]
	public class RegionGeneratorEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			var generator = (RegionGenerator)target;

			generator.ManifoldGenerator = EditorGUILayout.ObjectField("Manifold Generator", generator.ManifoldGenerator as Object, typeof(IManifoldProvider), true) as IManifoldProvider;

			if (generator.ManifoldGenerator != null && generator.ManifoldGenerator.manifold != null)
			{
				generator.DesiredRegionCount = EditorGUILayout.IntSlider(generator.DesiredRegionCount, 1, generator.ManifoldGenerator.manifold.topology.faces.Count);
			}
			else
			{
				generator.DesiredRegionCount = 0;
			}
		}
	}
}
