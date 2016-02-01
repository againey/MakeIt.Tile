using UnityEngine;
using UnityEditor;
using System;

namespace Experilous
{
	[CustomPropertyDrawer(typeof(AssetGeneratorRandomization))]
	public class AssetGeneratorRandomizationPropertyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var randomization = (AssetGeneratorRandomization)fieldInfo.GetValue(property.serializedObject.targetObject);

			var margins = 0f;
			var lines = 2;

			if (!string.IsNullOrEmpty(label.text))
			{
				lines += 1;
				margins += CombinedVerticalFieldMargins(EditorStyles.label, EditorStyles.popup);
			}

			margins += CombinedVerticalFieldMargins(EditorStyles.popup, EditorStyles.popup);

			switch (randomization.seedSource)
			{
				case AssetGeneratorRandomization.SeedSource.Numerical:
					lines += 1;
					margins += CombinedVerticalFieldMargins(EditorStyles.popup, EditorStyles.numberField);
					break;
				case AssetGeneratorRandomization.SeedSource.Textual:
					lines += 1;
					margins += CombinedVerticalFieldMargins(EditorStyles.popup, EditorStyles.textField);
					break;
				case AssetGeneratorRandomization.SeedSource.RandomEngine:
				case AssetGeneratorRandomization.SeedSource.RandomEngineAndSystemTime:
					if (AssetInputSlotPropertyDrawer.ShouldShowInputGUI(property.FindPropertyRelative("randomEngineSeedInputSlot"), fieldInfo.FieldType.GetField("randomEngineSeedInputSlot")))
					{
						lines += 1;
						margins += CombinedVerticalFieldMargins(EditorStyles.popup, EditorStyles.popup);
					}
					break;
				case AssetGeneratorRandomization.SeedSource.RandomEngineAndNumerical:
					if (AssetInputSlotPropertyDrawer.ShouldShowInputGUI(property.FindPropertyRelative("randomEngineSeedInputSlot"), fieldInfo.FieldType.GetField("randomEngineSeedInputSlot")))
					{
						lines += 1;
						margins += CombinedVerticalFieldMargins(EditorStyles.popup, EditorStyles.popup);
					}
					lines += 1;
					margins += CombinedVerticalFieldMargins(EditorStyles.popup, EditorStyles.numberField);
					break;
				case AssetGeneratorRandomization.SeedSource.RandomEngineAndTextual:
					if (AssetInputSlotPropertyDrawer.ShouldShowInputGUI(property.FindPropertyRelative("randomEngineSeedInputSlot"), fieldInfo.FieldType.GetField("randomEngineSeedInputSlot")))
					{
						lines += 1;
						margins += CombinedVerticalFieldMargins(EditorStyles.popup, EditorStyles.popup);
					}
					lines += 1;
					margins += CombinedVerticalFieldMargins(EditorStyles.popup, EditorStyles.textField);
					break;
			}

			return lines * EditorGUIUtility.singleLineHeight + margins;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			var priorIndentLevel = EditorGUI.indentLevel;

			var randomization = (AssetGeneratorRandomization)fieldInfo.GetValue(property.serializedObject.targetObject);

			var yCurrent = position.yMin;

			if (!string.IsNullOrEmpty(label.text))
			{
				var propertyLabelRect = new Rect(position.xMin, yCurrent, position.width, EditorGUIUtility.singleLineHeight);
				EditorGUI.LabelField(propertyLabelRect, label);

				yCurrent += EditorGUIUtility.singleLineHeight + CombinedVerticalFieldMargins(EditorStyles.label, EditorStyles.popup);

				EditorGUI.indentLevel += 1;
			}

			var randomEngineTypeRect = new Rect(position.xMin, yCurrent, position.width, EditorGUIUtility.singleLineHeight);
			EditorGUI.PropertyField(randomEngineTypeRect, property.FindPropertyRelative("randomEngineType"), new GUIContent("RNG Type"));

			var priorSeedSource = randomization.seedSource;

			yCurrent += EditorGUIUtility.singleLineHeight + CombinedVerticalFieldMargins(EditorStyles.popup, EditorStyles.popup);

			var seedSourceRect = new Rect(position.xMin, yCurrent, position.width, EditorGUIUtility.singleLineHeight);
			EditorGUI.PropertyField(seedSourceRect, property.FindPropertyRelative("seedSource"), new GUIContent("Seed Source"));

			switch (priorSeedSource)
			{
				case AssetGeneratorRandomization.SeedSource.Numerical:
					yCurrent += EditorGUIUtility.singleLineHeight + CombinedVerticalFieldMargins(EditorStyles.popup, EditorStyles.numberField);
					EditorGUI.PropertyField(
						new Rect(position.xMin,yCurrent, position.width, EditorGUIUtility.singleLineHeight),
						property.FindPropertyRelative("seedNumber"), new GUIContent("Seed Number"));
					break;
				case AssetGeneratorRandomization.SeedSource.Textual:
					yCurrent += EditorGUIUtility.singleLineHeight + CombinedVerticalFieldMargins(EditorStyles.popup, EditorStyles.textField);
					EditorGUI.PropertyField(
						new Rect(position.xMin, yCurrent, position.width, EditorGUIUtility.singleLineHeight),
						property.FindPropertyRelative("seedText"), new GUIContent("Seed Text"));
					break;
				case AssetGeneratorRandomization.SeedSource.RandomEngine:
				case AssetGeneratorRandomization.SeedSource.RandomEngineAndSystemTime:
					yCurrent += EditorGUIUtility.singleLineHeight + CombinedVerticalFieldMargins(EditorStyles.popup, EditorStyles.popup);
					AssetInputSlotPropertyDrawer.ShowInputGUI(
						new Rect(position.xMin, yCurrent, position.width, EditorGUIUtility.singleLineHeight), new GUIContent("Seeder"),
						property.FindPropertyRelative("randomEngineSeedInputSlot"), fieldInfo.FieldType.GetField("randomEngineSeedInputSlot"));
					break;
				case AssetGeneratorRandomization.SeedSource.RandomEngineAndNumerical:
					if (AssetInputSlotPropertyDrawer.ShouldShowInputGUI(property.FindPropertyRelative("randomEngineSeedInputSlot"), fieldInfo.FieldType.GetField("randomEngineSeedInputSlot")))
					{
						yCurrent += EditorGUIUtility.singleLineHeight + CombinedVerticalFieldMargins(EditorStyles.popup, EditorStyles.popup);
						AssetInputSlotPropertyDrawer.ShowInputGUI(
							new Rect(position.xMin, yCurrent, position.width, EditorGUIUtility.singleLineHeight), new GUIContent("Seeder"),
							property.FindPropertyRelative("randomEngineSeedInputSlot"), fieldInfo.FieldType.GetField("randomEngineSeedInputSlot"));
					}
					yCurrent += EditorGUIUtility.singleLineHeight + CombinedVerticalFieldMargins(EditorStyles.popup, EditorStyles.numberField);
					EditorGUI.PropertyField(
						new Rect(position.xMin,yCurrent, position.width, EditorGUIUtility.singleLineHeight),
						property.FindPropertyRelative("seedNumber"), new GUIContent("Seed Number"));
					break;
				case AssetGeneratorRandomization.SeedSource.RandomEngineAndTextual:
					if (AssetInputSlotPropertyDrawer.ShouldShowInputGUI(property.FindPropertyRelative("randomEngineSeedInputSlot"), fieldInfo.FieldType.GetField("randomEngineSeedInputSlot")))
					{
						yCurrent += EditorGUIUtility.singleLineHeight + CombinedVerticalFieldMargins(EditorStyles.popup, EditorStyles.popup);
						AssetInputSlotPropertyDrawer.ShowInputGUI(
							new Rect(position.xMin, yCurrent, position.width, EditorGUIUtility.singleLineHeight), new GUIContent("Seeder"),
							property.FindPropertyRelative("randomEngineSeedInputSlot"), fieldInfo.FieldType.GetField("randomEngineSeedInputSlot"));
					}
					yCurrent += EditorGUIUtility.singleLineHeight + CombinedVerticalFieldMargins(EditorStyles.popup, EditorStyles.textField);
					EditorGUI.PropertyField(
						new Rect(position.xMin, yCurrent, position.width, EditorGUIUtility.singleLineHeight),
						property.FindPropertyRelative("seedText"), new GUIContent("Seed Text"));
					break;
			}

			EditorGUI.indentLevel = priorIndentLevel;

			EditorGUI.EndProperty();
		}

		private static float CombinedVerticalFieldMargins(GUIStyle upperStyle, GUIStyle lowerStyle)
		{
			return Mathf.Max(
				Mathf.Max(EditorStyles.label.margin.bottom, upperStyle.margin.bottom),
				Mathf.Max(EditorStyles.label.margin.top, lowerStyle.margin.top));
		}
	}
}
