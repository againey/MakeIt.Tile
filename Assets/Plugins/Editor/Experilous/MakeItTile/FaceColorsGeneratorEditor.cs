/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/
#if false
using UnityEngine;
using UnityEditor;
using Experilous.MakeItGenerate;

namespace Experilous.MakeItTile
{
	[CustomEditor(typeof(FaceColorsGenerator))]
	public class FaceColorsGeneratorEditor : GeneratorEditor
	{
		protected override void OnPropertiesGUI()
		{
			var generator = (FaceColorsGenerator)target;

			var colorSource = generator.colorSource;

			generator.colorSource = (FaceColorsGenerator.ColorSource)EditorGUILayout.EnumPopup("Color Source", generator.colorSource);

			switch (colorSource)
			{
				case FaceColorsGenerator.ColorSource.Constant:
					generator.constantColor = EditorGUILayout.ColorField("Constant Color", generator.constantColor);
					break;
				case FaceColorsGenerator.ColorSource.RandomPerFace:
					InputSlotEditor.OnInspectorGUI(_serializedGenerator.FindProperty("topologyInputSlot"));
					generator.randomness = RandomnessDescriptorEditor.OnInspectorGUI(new GUIContent("Randomness"), generator.randomness);
					break;
				case FaceColorsGenerator.ColorSource.RandomPerGroup:
					InputSlotEditor.OnInspectorGUI(_serializedGenerator.FindProperty("faceGroupCollectionInputSlot"));
					InputSlotEditor.OnInspectorGUI(_serializedGenerator.FindProperty("faceGroupIndicesInputSlot"));
					generator.randomness = RandomnessDescriptorEditor.OnInspectorGUI(new GUIContent("Randomness"), generator.randomness);
					break;
				default:
					throw new System.NotImplementedException();
			}
		}
	}
}
#endif