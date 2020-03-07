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
	[CustomEditor(typeof(TopologyRandomizerGenerator))]
	public class TopologyRandomizerGeneratorEditor : GeneratorEditor
	{
		protected override void OnPropertiesGUI()
		{
			var generator = (TopologyRandomizerGenerator)target;

			InputSlotEditor.OnInspectorGUI(_serializedGenerator.FindProperty("surfaceInputSlot"));
			InputSlotEditor.OnInspectorGUI(_serializedGenerator.FindProperty("topologyInputSlot"));
			InputSlotEditor.OnInspectorGUI(_serializedGenerator.FindProperty("vertexPositionsInputSlot"));

			generator.passCount = EditorGUILayout.IntField("Passes", generator.passCount);
			generator.frequency = EditorGUILayout.Slider("Frequency", generator.frequency, 0f, 1f);

			generator.randomness = RandomnessDescriptorEditor.OnInspectorGUI(GUIContent.none, generator.randomness);

			generator.minVertexNeighbors = EditorGUILayout.IntSlider("Min Vertex Neighbors", generator.minVertexNeighbors, 2, 20);
			generator.maxVertexNeighbors = EditorGUILayout.IntSlider("Max Vertex Neighbors", generator.maxVertexNeighbors, 2, 20);
			generator.minFaceNeighbors = EditorGUILayout.IntSlider("Min Face Neighbors", generator.minFaceNeighbors, 2, 20);
			generator.maxFaceNeighbors = EditorGUILayout.IntSlider("Max Face Neighbors", generator.maxFaceNeighbors, 2, 20);

			if (generator.vertexPositionsInputSlot.source != null)
			{
				generator.lockBoundaryPositions = EditorGUILayout.Toggle("Lock Boundaries", generator.lockBoundaryPositions);

				generator.relaxForRegularityWeight = EditorGUILayout.Slider("Relax Regularity", generator.relaxForRegularityWeight, 0f, 1f);
				generator.relaxRelativePrecision = EditorGUILayout.Slider("Relax Precision", generator.relaxRelativePrecision, 0f, 1f);
				generator.repairRate = EditorGUILayout.Slider("Repair Rate", generator.repairRate, 0.1f, 1f);

				generator.maxRelaxIterations = EditorGUILayout.IntField("Relax Iterations", generator.maxRelaxIterations);
				generator.maxRepairIterations = EditorGUILayout.IntField("Repair Iterations", generator.maxRepairIterations);
			}
		}
	}
}
