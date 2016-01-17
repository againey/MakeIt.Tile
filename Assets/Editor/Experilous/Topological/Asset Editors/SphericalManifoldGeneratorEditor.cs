using UnityEngine;
using UnityEditor;

namespace Experilous.Topological
{
	[CustomEditor(typeof(SphericalManifoldGenerator))]
	public class SphericalManifoldGeneratorEditor : AssetGeneratorEditor
	{
		protected void ShrinkWrappedLabel(string label)
		{
			var content = new GUIContent(label);
			var size = GUI.skin.label.CalcSize(content);
			EditorGUILayout.LabelField(content, GUILayout.Width(size.x), GUILayout.Height(size.y));
		}

		protected override void OnPropertiesGUI()
		{
			var generator = (SphericalManifoldGenerator)target;

			generator.sphericalPolyhedron = (SphericalManifoldGenerator.SphericalPolyhedrons)EditorGUILayout.EnumPopup("Base Polyhedron", generator.sphericalPolyhedron);
			generator.subdivisionDegree = EditorGUILayout.IntField("Subdivision Degree", generator.subdivisionDegree);
			generator.useDualPolyhedron = EditorGUILayout.Toggle("Use Dual Polyhedron", generator.useDualPolyhedron);
		}
	}
}
