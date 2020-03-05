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

using UnityEditor;
using MakeIt.Generate;

namespace MakeIt.Tile
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
