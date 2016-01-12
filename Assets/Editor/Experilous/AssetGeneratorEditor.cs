using UnityEngine;
using UnityEditor.AnimatedValues;
using UnityEditor;
using System.Collections.Generic;

namespace Experilous
{
	public abstract class AssetGeneratorEditor : Editor
	{
		[SerializeField] private Dictionary<GeneratedAsset, Editor> _editorStates = new Dictionary<GeneratedAsset, Editor>();

		protected AssetGenerator _generator;

		[SerializeField] protected bool _outputFoldout = false;
		protected AnimBool _outputFoldoutAnimation;

		protected static GUIStyle _outputGUIStyle = null;

		protected void OnEnable()
		{
			_generator = (AssetGenerator)target;

			_outputFoldoutAnimation = new AnimBool(_outputFoldout);
			_outputFoldoutAnimation.valueChanged.AddListener(Repaint);
		}

		protected virtual void InitializeStyles()
		{
			if (_outputGUIStyle == null)
			{
				_outputGUIStyle = new GUIStyle(EditorStyles.helpBox);
			}
		}

		protected abstract void OnPropertiesGUI();

		public override void OnInspectorGUI()
		{
			InitializeStyles();

			OnPropertiesGUI();

			OnOutputsGUI();
		}

		protected TAsset OnDependencyGUI<TAsset>(string label, TAsset currentSelection) where TAsset : GeneratedAsset
		{
			return OnDependencyGUI(label, currentSelection, false, true);
		}

		protected TAsset OnDependencyGUI<TAsset>(string label, TAsset currentSelection, bool hideForOneOption) where TAsset : GeneratedAsset
		{
			return OnDependencyGUI(label, currentSelection, hideForOneOption, true);
		}

		protected TAsset OnDependencyGUI<TAsset>(string label, TAsset currentSelection, bool hideForOneOption, bool includeNone) where TAsset : GeneratedAsset
		{
			var potentialSources = _generator.bundle.GetMatchingGeneratedAssets<TAsset>(_generator);
			if (potentialSources.Count == 1 && hideForOneOption)
			{
				return potentialSources[0];
			}
			else if (includeNone)
			{
				var sourceIndex = potentialSources.IndexOf(currentSelection) + 1;

				var potentialSourcesContent = new GUIContent[potentialSources.Count + 1];
				potentialSourcesContent[0] = new GUIContent("None");
				for (int i = 0; i < potentialSources.Count; ++i)
				{
					potentialSourcesContent[i + 1] = new GUIContent(potentialSources[i].name);
				}

				sourceIndex = EditorGUILayout.Popup(new GUIContent(label), sourceIndex, potentialSourcesContent);
				if (sourceIndex > 0)
				{
					return potentialSources[sourceIndex - 1];
				}
				else
				{
					return null;
				}
			}
			else if (potentialSources.Count > 0)
			{
				var sourceIndex = potentialSources.IndexOf(currentSelection);
				if (sourceIndex == -1) sourceIndex = 0;

				var potentialSourcesContent = new GUIContent[potentialSources.Count];
				for (int i = 0; i < potentialSources.Count; ++i)
				{
					potentialSourcesContent[i] = new GUIContent(potentialSources[i].name);
				}

				sourceIndex = EditorGUILayout.Popup(new GUIContent(label), sourceIndex, potentialSourcesContent);
				return potentialSources[sourceIndex];
			}
			else
			{
				var potentialSourcesContent = new GUIContent[1];
				potentialSourcesContent[0] = new GUIContent("(none available)");
				EditorGUILayout.Popup(new GUIContent(label), 0, potentialSourcesContent);
				return null;
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
					editor = CreateEditor(output, typeof(GeneratedAssetEditor));
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

				EditorGUILayout.BeginVertical(_outputGUIStyle);
				editor.OnInspectorGUI();
				EditorGUILayout.EndVertical();
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
