using UnityEngine;
using UnityEditor;

namespace Experilous.Topological
{
	[CustomEditor(typeof(PlanarManifoldGenerator))]
	public class PlanarManifoldGeneratorEditor : AssetGeneratorEditor
	{
		private SerializedObject _serializedGenerator;

		protected new void OnEnable()
		{
			base.OnEnable();

			_serializedGenerator = (target != null) ? new SerializedObject(target) : null;
		}

		protected override void OnPropertiesGUI()
		{
			var generator = (PlanarManifoldGenerator)target;

			generator.planarTileShape = (PlanarManifoldGenerator.PlanarTileShapes)EditorGUILayout.EnumPopup("Faces", generator.planarTileShape);

			EditorGUILayout.PropertyField(_serializedGenerator.FindProperty("size"));
			EditorGUILayout.PropertyField(_serializedGenerator.FindProperty("horizontalAxis"));
			EditorGUILayout.PropertyField(_serializedGenerator.FindProperty("verticalAxis"));

			_serializedGenerator.ApplyModifiedProperties();
		}
	}
}
