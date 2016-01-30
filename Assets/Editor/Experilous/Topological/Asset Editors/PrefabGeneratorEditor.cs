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

			OnDependencyGUI("Mesh Collection", generator.meshCollectionInputSlot);
			generator.meshPrefab = EditorGUILayoutExtensions.ObjectField("Mesh Prefab", generator.meshPrefab, false);
		}
	}
}
