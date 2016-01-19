using UnityEngine;
using UnityEditor;

namespace Experilous.Topological
{
	[CustomEditor(typeof(WrappedPlanarManifoldGenerator))]
	public class WrappedPlanarManifoldGeneratorEditor : AssetGeneratorEditor
	{
		private SerializedObject _serializedGenerator;

		protected new void OnEnable()
		{
			base.OnEnable();

			_serializedGenerator = (target != null) ? new SerializedObject(target) : null;
		}

		protected override void OnPropertiesGUI()
		{
			var generator = (WrappedPlanarManifoldGenerator)target;

			generator.planarTileShape = (WrappedPlanarManifoldGenerator.PlanarTileShapes)EditorGUILayout.EnumPopup("Faces", generator.planarTileShape);
			generator.wrapOption = (WrappedPlanarManifoldGenerator.PlanarTwoAxisWrapOptions)EditorGUILayout.EnumPopup("Wrap", generator.wrapOption);

			EditorGUILayout.PropertyField(_serializedGenerator.FindProperty("size"));
			EditorGUILayout.PropertyField(_serializedGenerator.FindProperty("horizontalAxis"));
			EditorGUILayout.PropertyField(_serializedGenerator.FindProperty("verticalAxis"));

			_serializedGenerator.ApplyModifiedProperties();
		}
	}
}
