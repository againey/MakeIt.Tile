using UnityEngine;
using UnityEditor.AnimatedValues;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Experilous
{
	[CustomEditor(typeof(AssetGenerator), true)]
	public class AssetGeneratorEditor : Editor
	{
		[SerializeField] private Dictionary<AssetDescriptor, Editor> _editorStates = new Dictionary<AssetDescriptor, Editor>();

		protected AssetGenerator _generator;
		protected SerializedObject _serializedGenerator;

		[SerializeField] protected bool _outputFoldout = false;
		protected AnimBool _outputFoldoutAnimation;

		protected void OnEnable()
		{
			_generator = (AssetGenerator)target;
			_serializedGenerator = (target != null) ? new SerializedObject(target) : null;

			_outputFoldoutAnimation = new AnimBool(_outputFoldout);
			_outputFoldoutAnimation.valueChanged.AddListener(Repaint);
		}

		private static bool IsInAssetGeneratorSubclass(FieldInfo field)
		{
			var baseType = typeof(AssetGenerator);
			while (baseType != null)
			{
				if (field.DeclaringType == baseType) return false;
				baseType = baseType.BaseType;
			}
			return true;
		}

		private static TAttribute GetAttribute<TAttribute>(FieldInfo field) where TAttribute : Attribute
		{
			foreach (var attribute in field.GetCustomAttributes(true))
			{
				if (attribute is TAttribute)
				{
					return (TAttribute)attribute;
				}
			}
			return null;
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
					if (field.FieldType == typeof(AssetDescriptor)) continue;
					if (field.FieldType == typeof(AssetDescriptor[])) continue;
					if (field.FieldType == typeof(List<AssetDescriptor>)) continue;
					if (!IsInAssetGeneratorSubclass(field)) continue;

					var labelAttribute = GetAttribute<LabelAttribute>(field);
					var labelContent = new GUIContent(
						labelAttribute != null ? labelAttribute.text : property.displayName,
						property.tooltip);

					if (typeof(Component).IsAssignableFrom(field.FieldType))
					{
						property.objectReferenceValue = EditorGUILayoutExtensions.ObjectField(labelContent, property.objectReferenceValue, field.FieldType, false);
					}
					else if (field.FieldType == typeof(AssetInputSlot))
					{
						if (AssetInputSlotPropertyDrawer.ShouldShowInputGUI(property, field))
						{
							EditorGUILayout.PropertyField(property, labelContent, true);
						}
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
			foreach (var output in _generator.outputs)
			{
				Editor editor;
				if (!_editorStates.TryGetValue(output, out editor))
				{
					editor = CreateEditor(output, typeof(AssetDescriptorEditor));
					_editorStates[output] = editor;
				}

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

				editor.OnInspectorGUI();
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
