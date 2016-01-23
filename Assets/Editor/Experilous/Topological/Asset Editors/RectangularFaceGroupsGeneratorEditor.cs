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

			generator.topology = OnDependencyGUI("Topology", generator.topology, typeof(Topology), true);
			generator.facePositions = OnDependencyGUI("Face Positions", generator.facePositions, typeof(IFaceAttribute<Vector3>), false);
			generator.surfaceDescriptor = OnDependencyGUI("Surface Descriptor", generator.surfaceDescriptor, typeof(PlanarSurfaceDescriptor), false);

			_serializedGenerator.ApplyModifiedProperties();
		}
	}
}
