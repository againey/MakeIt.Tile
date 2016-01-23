using UnityEngine;
using UnityEditor;

namespace Experilous
{
	[CustomEditor(typeof(AssetDescriptor))]
	public class AssetDescriptorEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			var descriptor = (AssetDescriptor)target;

			var toggleWidth = EditorStyles.toggle.CalcHeight(new GUIContent(""), 1024f);
			var nameWidth = Screen.width * 0.4f - toggleWidth;

			EditorGUILayout.BeginHorizontal();
			if (descriptor.isOptional)
			{
				descriptor.isPersisted = EditorGUILayout.Toggle(descriptor.isPersisted, GUILayout.Width(toggleWidth));
				var toggleRect = GUILayoutUtility.GetLastRect();
				GUI.Label(toggleRect, new GUIContent("", "This output is optional, allowing you to choose if it will be persisted."));
			}
			else
			{
				GUI.enabled = false;
				EditorGUILayout.Toggle(descriptor.isPersisted, GUILayout.Width(toggleWidth));
				var toggleRect = GUILayoutUtility.GetLastRect();
				GUI.enabled = true;
				GUI.Label(toggleRect, new GUIContent("",
					descriptor.isPersisted
						? "This output is not optional, and will always be persisted."
						: "This output is not optional, and will never be persisted."));
			}
			descriptor.name = EditorGUILayout.TextField(descriptor.name, GUILayout.Width(nameWidth));
			var textRect = GUILayoutUtility.GetLastRect();
			GUI.Label(textRect, new GUIContent("", "The name of the generated output, which will be included within parentheses when naming the generated asset file."));
			EditorGUILayout.LabelField(new GUIContent(string.Format("({0})", descriptor.assetType.GetPrettyName()), descriptor.assetType.GetPrettyName(true)), GUILayout.ExpandWidth(true), GUILayout.MinWidth(0f));
			EditorGUILayout.EndHorizontal();
		}
	}
}
