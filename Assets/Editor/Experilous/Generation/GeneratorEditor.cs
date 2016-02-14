using UnityEngine;
using UnityEditor.AnimatedValues;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Experilous.Generation
{
	[CustomEditor(typeof(Generator), true)]
	public class GeneratorEditor : Editor
	{
		protected Generator _generator;
		protected SerializedObject _serializedGenerator;

		[SerializeField] protected bool _outputFoldout = false;
		protected AnimBool _outputFoldoutAnimation;

		protected void OnEnable()
		{
			_generator = (Generator)target;
			_serializedGenerator = (target != null) ? new SerializedObject(target) : null;

			_outputFoldoutAnimation = new AnimBool(_outputFoldout);
			_outputFoldoutAnimation.valueChanged.AddListener(Repaint);
		}

		private static bool IsInAssetGeneratorSubclass(FieldInfo field)
		{
			var baseType = typeof(Generator);
			while (baseType != null)
			{
				if (field.DeclaringType == baseType) return false;
				baseType = baseType.BaseType;
			}
			return true;
		}

		protected virtual void OnPropertiesGUI()
		{
			var generatorType = _generator.GetType();
			var property = _serializedGenerator.GetIterator();
			if (property.NextVisible(true))
			{
				do
				{
					var field = generatorType.GetField(property.name);
					if (field == null) continue;
					if (field.FieldType == typeof(OutputSlot)) continue;
					if (field.FieldType == typeof(OutputSlot[])) continue;
					if (field.FieldType == typeof(List<OutputSlot>)) continue;
					if (!IsInAssetGeneratorSubclass(field)) continue;

					var labelAttribute = Utility.GetAttribute<LabelAttribute>(field);
					var labelContent = new GUIContent(
						labelAttribute != null ? labelAttribute.text : property.displayName,
						property.tooltip);

					if (field.FieldType == typeof(InputSlot))
					{
						var input = (InputSlot)field.GetValue(property.serializedObject.targetObject);
						if (input.isActive)
						{
							InputSlotEditor.OnInspectorGUI(labelContent, input, InputSlot.ShouldAutoSelect(field));
						}
					}
					else if (field.FieldType == typeof(RandomnessDescriptor))
					{
						var randomness = (RandomnessDescriptor)field.GetValue(property.serializedObject.targetObject);
						randomness = RandomnessDescriptorEditor.OnInspectorGUI(labelContent, randomness);
						field.SetValue(property.serializedObject.targetObject, randomness);
					}
					else if (typeof(Component).IsAssignableFrom(field.FieldType))
					{
						field.SetValue(property.serializedObject.targetObject, EditorGUILayoutExtensions.ObjectField(labelContent, property.objectReferenceValue, field.FieldType, false));
					}
					else
					{
						EditorGUILayout.PropertyField(property, labelContent, true);
					}
				} while (property.NextVisible(false));
			}
		}

		public override void OnInspectorGUI()
		{
			_serializedGenerator.Update();

			EditorGUI.BeginChangeCheck();

			OnPropertiesGUI();

			OnOutputsGUI();

			if (EditorGUI.EndChangeCheck())
			{
				_serializedGenerator.ApplyModifiedProperties();
				_generator.Update();
			}
		}

		protected virtual void OnOutputsGUI()
		{
			bool hasOutputs = false;
			foreach (var output in _generator.activeOutputs)
			{
				if (!hasOutputs)
				{
					hasOutputs = true;

					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(10f);
					EditorGUILayout.BeginVertical();

					_outputFoldout = EditorGUILayout.Foldout(_outputFoldout, "Generated Outputs");
					_outputFoldoutAnimation.target = _outputFoldout;
					if (!EditorGUILayout.BeginFadeGroup(_outputFoldoutAnimation.faded))
					{
						break;
					}
				}

				OutputSlotEditor.OnInspectorGUI(output);
			}

			if (hasOutputs)
			{
				EditorGUILayout.EndFadeGroup();
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();
			}
		}
	}
}
