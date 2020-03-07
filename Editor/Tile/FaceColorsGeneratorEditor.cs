/******************************************************************************\
* Copyright Andy Gainey                                                        *
*                                                                              *
* Licensed under the Apache License, Version 2.0 (the "License");              *
* you may not use this file except in compliance with the License.             *
* You may obtain a copy of the License at                                      *
*                                                                              *
*     http://www.apache.org/licenses/LICENSE-2.0                               *
*                                                                              *
* Unless required by applicable law or agreed to in writing, software          *
* distributed under the License is distributed on an "AS IS" BASIS,            *
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.     *
* See the License for the specific language governing permissions and          *
* limitations under the License.                                               *
\******************************************************************************/

using UnityEngine;
using UnityEditor;
using MakeIt.Generate;

namespace MakeIt.Tile
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
