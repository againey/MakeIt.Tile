﻿/******************************************************************************\
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
using UnityEditor.AnimatedValues;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;
using MakeIt.Containers;
using MakeIt.Core;

namespace MakeIt.Generate
{
	[CustomEditor(typeof(Generator), true)]
	public class GeneratorEditor : Editor
	{
		protected Generator _generator;
		protected SerializedObject _serializedGenerator;

		[SerializeField] protected bool _outputFoldout = false;
		protected AnimBool _outputFoldoutAnimation;

		[Serializable] protected class OutputEditorState
		{
			public bool foldout;
			public AnimBool foldoutAnimation;

			public OutputEditorState(GeneratorEditor editor, bool foldout = false)
			{
				this.foldout = foldout;
				foldoutAnimation = new AnimBool(foldout);
				foldoutAnimation.valueChanged.AddListener(editor.Repaint);
			}
		}

		[Serializable] protected class OutputEditorStateDictionary : SerializableDictionary<OutputSlot, OutputEditorState> { }

		[SerializeField] protected OutputEditorStateDictionary _outputEditorStates = new OutputEditorStateDictionary();

		protected virtual void OnEnable()
		{
			_generator = (Generator)target;
			_serializedGenerator = (target != null) ? new SerializedObject(target) : null;

			_outputFoldoutAnimation = new AnimBool(_outputFoldout);
			_outputFoldoutAnimation.valueChanged.AddListener(Repaint);

			foreach (var outputEditorState in _outputEditorStates.Values)
			{
				outputEditorState.foldoutAnimation = new AnimBool(outputEditorState.foldout);
				outputEditorState.foldoutAnimation.valueChanged.AddListener(Repaint);
			}

			if (_generator != null) _generator.Update();
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

					var labelAttribute = GeneralUtility.GetAttribute<LabelAttribute>(field);
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
				_generator.executive.UpdateGenerators();
				_generator.executive.Save();
			}
		}

		protected virtual void OnOutputsGUI()
		{
			bool hasOutputs = false;
			foreach (var output in _generator.visibleOutputs)
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

				OutputEditorState outputEditorState;
				if (!_outputEditorStates.TryGetValue(output, out outputEditorState))
				{
					outputEditorState = new OutputEditorState(this);
					_outputEditorStates[output] = outputEditorState;
				}

				outputEditorState.foldout = OutputSlotEditor.OnInspectorGUI(output, outputEditorState.foldout, outputEditorState.foldoutAnimation);
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
