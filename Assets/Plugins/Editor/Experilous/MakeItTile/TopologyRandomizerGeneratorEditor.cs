/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using UnityEditor;
using Experilous.MakeIt.Generate;

namespace Experilous.MakeIt.Tile
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
