/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEditor;
using Experilous.MakeIt.Generate;

namespace Experilous.MakeIt.Tile
{
	[CustomEditor(typeof(FaceCentroidsGenerator))]
	public class FaceCentroidsGeneratorEditor : GeneratorEditor
	{
		protected override void OnPropertiesGUI()
		{
			var generator = (FaceCentroidsGenerator)target;

			InputSlotEditor.OnInspectorGUI(_serializedGenerator.FindProperty("surfaceInputSlot"));
			InputSlotEditor.OnInspectorGUI(_serializedGenerator.FindProperty("topologyInputSlot"));
			InputSlotEditor.OnInspectorGUI(_serializedGenerator.FindProperty("vertexPositionsInputSlot"));

			var surface = generator.surfaceInputSlot.GetAsset<Surface>();
			if (surface is SphericalSurface)
			{
				generator.flatten = EditorGUILayout.Toggle("Flatten", generator.flatten);
			}
		}
	}
}
