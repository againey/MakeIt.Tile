/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using UnityEditor;

namespace Experilous.MakeItGenerate
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

			randomness.randomType = (RandomnessDescriptor.RandomType)EditorGUILayout.EnumPopup("RNG Type", randomness.randomType);

			var priorSeedSource = randomness.seedSource;
			randomness.seedSource = (RandomnessDescriptor.SeedSource)EditorGUILayout.EnumPopup("Seed Source", randomness.seedSource);

			switch (priorSeedSource)
			{
				case RandomnessDescriptor.SeedSource.Random:
				case RandomnessDescriptor.SeedSource.RandomAndSystemTime:
				case RandomnessDescriptor.SeedSource.RandomAndNumerical:
				case RandomnessDescriptor.SeedSource.RandomAndTextual:
					InputSlotEditor.OnInspectorGUI(new GUIContent("Seeder"), randomness.randomSeedInputSlot, InputSlot.ShouldAutoSelect(randomness, "randomSeedInputSlot"));
					break;
			}

			switch (priorSeedSource)
			{
				case RandomnessDescriptor.SeedSource.Numerical:
				case RandomnessDescriptor.SeedSource.RandomAndNumerical:
					randomness.seedNumber = EditorGUILayout.IntField("Seed Number", randomness.seedNumber);
					break;
				case RandomnessDescriptor.SeedSource.Textual:
				case RandomnessDescriptor.SeedSource.RandomAndTextual:
					randomness.seedText = EditorGUILayout.TextField("Seed Text", randomness.seedText);
					break;
			}

			EditorGUI.indentLevel = priorIndentLevel;

			return randomness;
		}
	}
}
