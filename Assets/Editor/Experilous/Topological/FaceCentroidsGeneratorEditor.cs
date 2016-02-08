using UnityEngine;
using UnityEditor;

namespace Experilous.Topological
{
	[CustomEditor(typeof(FaceCentroidsGenerator))]
	public class FaceCentroidsGeneratorEditor : AssetGeneratorEditor
	{
		protected override void OnPropertiesGUI()
		{
			var generator = (FaceCentroidsGenerator)target;

			EditorGUILayout.PropertyField(_serializedGenerator.FindProperty("surfaceInputSlot"));
			EditorGUILayout.PropertyField(_serializedGenerator.FindProperty("topologyInputSlot"));
			EditorGUILayout.PropertyField(_serializedGenerator.FindProperty("vertexPositionsInputSlot"));

			var surface = generator.surfaceInputSlot.GetAsset<Surface>();
			if (surface is SphericalSurface)
			{
				generator.flatten = EditorGUILayout.Toggle("Flatten", generator.flatten);
			}
		}
	}
}
