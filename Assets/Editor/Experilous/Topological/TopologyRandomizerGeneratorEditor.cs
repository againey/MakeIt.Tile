using UnityEngine;
using UnityEditor;

namespace Experilous.Topological
{
	[CustomEditor(typeof(TopologyRandomizerGenerator))]
	public class TopologyRandomizerGeneratorEditor : AssetGeneratorEditor
	{
		protected override void OnPropertiesGUI()
		{
			var generator = (TopologyRandomizerGenerator)target;

			EditorGUILayout.PropertyField(_serializedGenerator.FindProperty("topologyInputSlot"));
			EditorGUILayout.PropertyField(_serializedGenerator.FindProperty("vertexPositionsInputSlot"));
			EditorGUILayout.PropertyField(_serializedGenerator.FindProperty("edgeWrapInputSlot"));
			EditorGUILayout.PropertyField(_serializedGenerator.FindProperty("positionalAttributeAdapterInputSlot"));

			generator.surfaceType = (TopologyRandomizerGenerator.SurfaceType)EditorGUILayout.EnumPopup("Surface Type", generator.surfaceType);

			generator.passCount = EditorGUILayout.IntField("Passes", generator.passCount);
			generator.frequency = EditorGUILayout.Slider("Frequency", generator.frequency, 0f, 1f);

			EditorGUILayout.PropertyField(_serializedGenerator.FindProperty("randomization"));

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
