using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace Experilous
{
	[CustomPropertyDrawer(typeof(AssetInputSlot))]
	public class AssetInputSlotPropertyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (ShouldShowInputGUI(property, fieldInfo))
			{
				return EditorGUIUtility.singleLineHeight;
			}
			else
			{
				return -Mathf.Max(EditorStyles.popup.margin.top, EditorStyles.popup.margin.bottom);
			}
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label = EditorGUI.BeginProperty(position, label, property);

			ShowInputGUI(position, label, property, fieldInfo);

			EditorGUI.EndProperty();
		}

		public static Predicate<AssetDescriptor> GetDefaultPredicate(AssetInputSlot input)
		{
			return (AssetDescriptor descriptor) =>
			{
				return input.assetType.IsAssignableFrom(descriptor.assetType);
			};
		}

		public static bool ShouldShowInputGUI(SerializedProperty property, FieldInfo field)
		{
			if (property.objectReferenceValue != null && property.objectReferenceValue is AssetInputSlot)
			{
				var input = (AssetInputSlot)property.objectReferenceValue;
				if (input.isActive)
				{
					var autoSelectAttribute = Utility.GetAttribute<AutoSelectAttribute>(field);

					return ShouldShowInputGUI(input, autoSelectAttribute != null);
				}
			}

			return false;
		}

		public static bool ShouldShowInputGUI(AssetInputSlot input, bool autoSelect = false)
		{
			return ShouldShowInputGUI(input, autoSelect, GetDefaultPredicate(input));
		}

		public static bool ShouldShowInputGUI(AssetInputSlot input, bool autoSelect, Predicate<AssetDescriptor> predicate)
		{
			var count = input.generator.collection.GetMatchingGeneratedAssetsCount(input.generator, predicate);
			// If not auto-selecting, then always show; otherwise, show as long as an
			// optional input has no matches or a required input has exactly one match.
			return !autoSelect || !(count == 0 && input.isOptional || count == 1 && input.isRequired);
		}

		public static bool ShowInputGUI(Rect position, GUIContent label, SerializedProperty property, FieldInfo field)
		{
			if (property.objectReferenceValue != null && property.objectReferenceValue is AssetInputSlot)
			{
				var input = (AssetInputSlot)property.objectReferenceValue;
				if (input.isActive)
				{
					var autoSelectAttribute = Utility.GetAttribute<AutoSelectAttribute>(field);

					return ShowInputGUI(position, label, input, autoSelectAttribute != null);
				}
			}

			return false;
		}

		public static bool ShowInputGUI(Rect position, GUIContent label, AssetInputSlot input, bool autoSelect = false)
		{
			return ShowInputGUI(position, label, input, autoSelect, GetDefaultPredicate(input));
		}

		public static bool ShowInputGUI(Rect position, GUIContent label, AssetInputSlot input, bool autoSelect, Predicate<AssetDescriptor> predicate)
		{
			var potentialSources = input.generator.collection.GetMatchingGeneratedAssets(input.generator, predicate);
			if (autoSelect && potentialSources.Count == 0 && input.isOptional)
			{
				input.source = null;
				return false;
			}
			else if (autoSelect && potentialSources.Count == 1 && input.isRequired)
			{
				input.source = potentialSources[0];
				return false;
			}
			else
			{
				var sourceIndex = potentialSources.IndexOf(input.source) + 1;

				var potentialSourcesContent = new GUIContent[potentialSources.Count + 1];
				potentialSourcesContent[0] = new GUIContent(potentialSources.Count > 0 ? "(no source selected)" : "(no source available)");
				for (int i = 0; i < potentialSources.Count; ++i)
				{
					potentialSourcesContent[i + 1] = new GUIContent(potentialSources[i].name);
				}

				var priorBackgroundColor = GUI.backgroundColor;
				if (input.isRequired && sourceIndex == 0)
				{
					GUI.backgroundColor = Color.Lerp(GUI.color, Color.red, 0.5f);
				}

				if (label.text.EndsWith(" Input"))
				{
					label.text = label.text.Substring(0, label.text.Length - " Input".Length);
				}
				else if (label.text.EndsWith(" Input Slot"))
				{
					label.text = label.text.Substring(0, label.text.Length - " Input Slot".Length);
				}

				sourceIndex = EditorGUI.Popup(position, label, sourceIndex, potentialSourcesContent);

				GUI.backgroundColor = priorBackgroundColor;

				if (sourceIndex > 0)
				{
					input.source = potentialSources[sourceIndex - 1];
				}
				else
				{
					input.source = null;
				}

				return true;
			}
		}
	}
}
