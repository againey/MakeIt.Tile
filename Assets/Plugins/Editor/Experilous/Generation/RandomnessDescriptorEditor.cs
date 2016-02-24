/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using UnityEngine;
using UnityEditor;

namespace Experilous.Generation
{
	public static class RandomnessDescriptorEditor
	{
		public static RandomnessDescriptor OnInspectorGUI(GUIContent label, RandomnessDescriptor randomness)
		{
			var priorIndentLevel = EditorGUI.indentLevel;

			if (!string.IsNullOrEmpty(label.text))
			{
				EditorGUILayout.LabelField(label);
				EditorGUI.indentLevel += 1;
			}

			randomness.randomEngineType = (RandomnessDescriptor.RandomEngineType)EditorGUILayout.EnumPopup("RNG Type", randomness.randomEngineType);

			var priorSeedSource = randomness.seedSource;
			randomness.seedSource = (RandomnessDescriptor.SeedSource)EditorGUILayout.EnumPopup("Seed Source", randomness.seedSource);

			switch (priorSeedSource)
			{
				case RandomnessDescriptor.SeedSource.RandomEngine:
				case RandomnessDescriptor.SeedSource.RandomEngineAndSystemTime:
				case RandomnessDescriptor.SeedSource.RandomEngineAndNumerical:
				case RandomnessDescriptor.SeedSource.RandomEngineAndTextual:
					InputSlotEditor.OnInspectorGUI(new GUIContent("Seeder"), randomness.randomEngineSeedInputSlot, InputSlot.ShouldAutoSelect(randomness, "randomEngineSeedInputSlot"));
					break;
			}

			switch (priorSeedSource)
			{
				case RandomnessDescriptor.SeedSource.Numerical:
				case RandomnessDescriptor.SeedSource.RandomEngineAndNumerical:
					randomness.seedNumber = EditorGUILayout.IntField("Seed Number", randomness.seedNumber);
					break;
				case RandomnessDescriptor.SeedSource.Textual:
				case RandomnessDescriptor.SeedSource.RandomEngineAndTextual:
					randomness.seedText = EditorGUILayout.TextField("Seed Text", randomness.seedText);
					break;
			}

			EditorGUI.indentLevel = priorIndentLevel;

			return randomness;
		}
	}
}
