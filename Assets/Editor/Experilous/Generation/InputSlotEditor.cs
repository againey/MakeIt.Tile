using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace Experilous.Generation
{
	public static class InputSlotEditor
	{
		public static Predicate<OutputSlot> GetDefaultPredicate(InputSlot input)
		{
			return (OutputSlot output) =>
			{
				return input.assetType.IsAssignableFrom(output.assetType);
			};
		}

		public static bool OnInspectorGUI(SerializedProperty property)
		{
			var obj = property.serializedObject.targetObject;
			var field = obj.GetType().GetField(property.name);
			var input = (InputSlot)field.GetValue(obj);
			if (input.isActive)
			{
				return OnInspectorGUI(new GUIContent(property.displayName), input, InputSlot.ShouldAutoSelect(field));
			}
			else
			{
				return false;
			}
		}

		public static bool OnInspectorGUI(GUIContent label, InputSlot input, bool autoSelect = false)
		{
			return OnInspectorGUI(label, input, autoSelect, GetDefaultPredicate(input));
		}

		public static bool OnInspectorGUI(GUIContent label, InputSlot input, bool autoSelect, Predicate<OutputSlot> predicate)
		{
			var potentialSources = input.generator.executive.GetMatchingGeneratedAssets(input.generator, predicate);
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

				sourceIndex = EditorGUILayout.Popup(label, sourceIndex, potentialSourcesContent);

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
