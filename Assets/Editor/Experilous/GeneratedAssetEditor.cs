using UnityEngine;
using UnityEditor;

namespace Experilous
{
	[CustomEditor(typeof(GeneratedAsset))]
	public class GeneratedAssetEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			var generatedAsset = (GeneratedAsset)target;

			var toggleWidth = EditorStyles.toggle.CalcHeight(new GUIContent(""), 1024f);
			var nameWidth = Screen.width * 0.4f - toggleWidth;

			EditorGUILayout.BeginHorizontal();
			GUI.enabled = generatedAsset.isOptional;
			generatedAsset.isEnabled = EditorGUILayout.Toggle(generatedAsset.isEnabled, GUILayout.Width(toggleWidth));
			var toggleRect = GUILayoutUtility.GetLastRect();
			GUI.enabled = true;
			GUI.Label(toggleRect, new GUIContent("",
				generatedAsset.isOptional
					? "This output is optional, allowing you to choose if it will be generated."
					: "This output is not optional, and will always be generated."));
			generatedAsset.name = EditorGUILayout.TextField(generatedAsset.name, GUILayout.Width(nameWidth));
			var textRect = GUILayoutUtility.GetLastRect();
			GUI.Label(textRect, new GUIContent("", "The name of the generated output, which will be included within parentheses when naming the generated asset file."));
			EditorGUILayout.LabelField(new GUIContent(string.Format("({0})", generatedAsset.generatedType.Name), generatedAsset.generatedType.FullName), GUILayout.ExpandWidth(true), GUILayout.MinWidth(0f));
			EditorGUILayout.EndHorizontal();
		}
	}
}
