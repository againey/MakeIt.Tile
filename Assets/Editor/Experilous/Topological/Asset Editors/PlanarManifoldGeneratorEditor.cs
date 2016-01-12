using UnityEngine;
using UnityEditor;

namespace Experilous.Topological
{
	[CustomEditor(typeof(PlanarManifoldGenerator))]
	public class PlanarManifoldGeneratorEditor : AssetGeneratorEditor
	{
		protected void ShrinkWrappedLabel(string label)
		{
			var content = new GUIContent(label);
			var size = GUI.skin.label.CalcSize(content);
			EditorGUILayout.LabelField(content, GUILayout.Width(size.x), GUILayout.Height(size.y));
		}

		protected override void OnPropertiesGUI()
		{
			var generator = (PlanarManifoldGenerator)target;

			generator.planarTileShape = (PlanarManifoldGenerator.PlanarTileShapes)EditorGUILayout.EnumPopup("Faces", generator.planarTileShape);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Size");
			generator.columns = EditorGUILayout.IntField(generator.columns, GUILayout.ExpandWidth(true));
			ShrinkWrappedLabel("columns by");
			generator.rows = EditorGUILayout.IntField(generator.rows, GUILayout.ExpandWidth(true));
			ShrinkWrappedLabel("rows");
			EditorGUILayout.EndHorizontal();
		}
	}
}
