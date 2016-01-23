using UnityEngine;
using UnityEditor;

namespace Experilous.Topological
{
	[CustomEditor(typeof(PrefabGenerator))]
	public class PrefabGeneratorEditor : AssetGeneratorEditor
	{
		protected override void OnPropertiesGUI()
		{
			var generator = (PrefabGenerator)target;

			generator.meshCollection = OnDependencyGUI("Mesh Collection", generator.meshCollection, typeof(MeshCollection), false);
			generator.meshPrefab = EditorGUILayoutExtensions.ObjectField("Mesh Prefab", generator.meshPrefab, false);
		}
	}
}
