using UnityEngine;
using UnityEditor.AnimatedValues;
using UnityEditor;
using System.Collections.Generic;

namespace Experilous
{
	public abstract class AssetGeneratorEditor : Editor
	{
		[SerializeField] private Dictionary<AssetDescriptor, Editor> _editorStates = new Dictionary<AssetDescriptor, Editor>();

		protected AssetGenerator _generator;

		[SerializeField] protected bool _outputFoldout = false;
		protected AnimBool _outputFoldoutAnimation;

		protected void OnEnable()
		{
			_generator = (AssetGenerator)target;

			_outputFoldoutAnimation = new AnimBool(_outputFoldout);
			_outputFoldoutAnimation.valueChanged.AddListener(Repaint);
		}

		protected abstract void OnPropertiesGUI();

		public override void OnInspectorGUI()
		{
			OnPropertiesGUI();

			OnOutputsGUI();
		}

		protected void OnDependencyGUI(string label, AssetInputSlot inputSlot)
		{
			OnDependencyGUI(label, inputSlot, false, inputSlot.isOptional);
		}

		protected void OnDependencyGUI(string label, AssetInputSlot inputSlot, bool hideForOneOption)
		{
			OnDependencyGUI(label, inputSlot, hideForOneOption, inputSlot.isOptional);
		}

		protected void OnDependencyGUI(string label, AssetInputSlot inputSlot, bool hideForOneOption, bool hideForNoOption)
		{
			OnDependencyGUI(label, inputSlot, hideForOneOption, hideForNoOption, (AssetDescriptor asset) => { return inputSlot.assetType.IsAssignableFrom(asset.assetType); });
		}

		protected void OnDependencyGUI(string label, AssetInputSlot inputSlot, System.Predicate<AssetDescriptor> predicate)
		{
			OnDependencyGUI(label, inputSlot, false, inputSlot.isOptional, predicate);
		}

		protected void OnDependencyGUI(string label, AssetInputSlot inputSlot, bool hideForOneOption, System.Predicate<AssetDescriptor> predicate)
		{
			OnDependencyGUI(label, inputSlot, hideForOneOption, inputSlot.isOptional, predicate);
		}

		protected void OnDependencyGUI(string label, AssetInputSlot inputSlot, bool hideForOneOption, bool hideForNoOption, System.Predicate<AssetDescriptor> predicate)
		{
			var potentialSources = _generator.collection.GetMatchingGeneratedAssets(_generator, predicate);
			if (potentialSources.Count == 0 && hideForNoOption)
			{
				inputSlot.source = null;
			}
			else if (potentialSources.Count == 1 && hideForOneOption)
			{
				inputSlot.source = potentialSources[0];
			}
			else if (inputSlot.isOptional)
			{
				var sourceIndex = potentialSources.IndexOf(inputSlot.source) + 1;

				var potentialSourcesContent = new GUIContent[potentialSources.Count + 1];
				potentialSourcesContent[0] = new GUIContent("None");
				for (int i = 0; i < potentialSources.Count; ++i)
				{
					potentialSourcesContent[i + 1] = new GUIContent(potentialSources[i].name);
				}

				sourceIndex = EditorGUILayout.Popup(new GUIContent(label), sourceIndex, potentialSourcesContent);
				if (sourceIndex > 0)
				{
					inputSlot.source = potentialSources[sourceIndex - 1];
				}
				else
				{
					inputSlot.source = null;
				}
			}
			else if (potentialSources.Count > 0)
			{
				var sourceIndex = potentialSources.IndexOf(inputSlot.source);
				if (sourceIndex == -1) sourceIndex = 0;

				var potentialSourcesContent = new GUIContent[potentialSources.Count];
				for (int i = 0; i < potentialSources.Count; ++i)
				{
					potentialSourcesContent[i] = new GUIContent(potentialSources[i].name);
				}

				sourceIndex = EditorGUILayout.Popup(new GUIContent(label), sourceIndex, potentialSourcesContent);
				inputSlot.source = potentialSources[sourceIndex];
			}
			else
			{
				var potentialSourcesContent = new GUIContent[1];
				potentialSourcesContent[0] = new GUIContent("(none available)");
				EditorGUILayout.Popup(new GUIContent(label), 0, potentialSourcesContent);
				inputSlot.source = null;
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
