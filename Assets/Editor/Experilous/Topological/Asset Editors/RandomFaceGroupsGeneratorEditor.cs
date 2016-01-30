using UnityEngine;
using UnityEditor;

namespace Experilous.Topological
{
	[CustomEditor(typeof(RandomFaceGroupsGenerator))]
	public class RandomFaceGroupsGeneratorEditor : AssetGeneratorEditor
	{
		protected override void OnPropertiesGUI()
		{
			var generator = (RandomFaceGroupsGenerator)target;

			OnDependencyGUI("Topology", generator.topologyInputSlot, true);

			generator.clustered = EditorGUILayout.Toggle("Clustered", generator.clustered);
			generator.groupCount = EditorGUILayout.IntField("Group Count", generator.groupCount);
		}
	}
}
