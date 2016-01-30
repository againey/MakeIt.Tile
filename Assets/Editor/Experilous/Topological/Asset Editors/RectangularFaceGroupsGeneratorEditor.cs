using UnityEngine;
using UnityEditor;

namespace Experilous.Topological
{
	[CustomEditor(typeof(RectangularFaceGroupsGenerator))]
	public class RectangularFaceGroupsGeneratorEditor : AssetGeneratorEditor
	{
		private SerializedObject _serializedGenerator;

		protected new void OnEnable()
		{
			base.OnEnable();

			_serializedGenerator = (target != null) ? new SerializedObject(target) : null;
		}

		protected override void OnPropertiesGUI()
		{
			var generator = (RectangularFaceGroupsGenerator)target;

			EditorGUILayout.PropertyField(_serializedGenerator.FindProperty("axisDivisions"), new GUIContent("Divisions"), false);

			OnDependencyGUI("Topology", generator.topologyInputSlot, true);
			OnDependencyGUI("Face Positions", generator.facePositionsInputSlot);
			OnDependencyGUI("Surface Descriptor", generator.surfaceDescriptorInputSlot);

			_serializedGenerator.ApplyModifiedProperties();
		}
	}
}
