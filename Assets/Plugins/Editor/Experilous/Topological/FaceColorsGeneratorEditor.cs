/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using UnityEngine;
using UnityEditor;
using Experilous.Generation;

namespace Experilous.Topological
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
