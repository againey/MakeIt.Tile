/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using UnityEditor;
using Experilous.Generation;

namespace Experilous.Topological
{
	[CustomEditor(typeof(FaceEdgeColorsGenerator))]
	public class FaceEdgeColorsGeneratorEditor : GeneratorEditor
	{
		protected override void OnPropertiesGUI()
		{
			var generator = (FaceEdgeColorsGenerator)target;

			var colorSource = generator.colorSource;

			generator.colorSource = (FaceEdgeColorsGenerator.ColorSource)EditorGUILayout.EnumPopup("Color Source", generator.colorSource);

			switch (colorSource)
			{
				case FaceEdgeColorsGenerator.ColorSource.Constant:
					generator.constantColor = EditorGUILayout.ColorField("Constant Color", generator.constantColor);
					break;
				case FaceEdgeColorsGenerator.ColorSource.FaceColor:
					InputSlotEditor.OnInspectorGUI(_serializedGenerator.FindProperty("topologyInputSlot"));
					InputSlotEditor.OnInspectorGUI(_serializedGenerator.FindProperty("faceColorsInputSlot"));
					generator.constantColor = EditorGUILayout.ColorField("Constant Color", generator.constantColor);
					generator.blendWeight = EditorGUILayout.Slider("Blend Weight", generator.blendWeight, 0f, 1f);
					break;
				default:
					throw new System.NotImplementedException();
			}
		}
	}
}
