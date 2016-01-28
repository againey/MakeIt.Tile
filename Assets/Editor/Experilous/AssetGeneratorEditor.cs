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

		protected AssetDescriptor OnDependencyGUI(string label, AssetDescriptor currentSelection, System.Type assetType)
		{
			return OnDependencyGUI(label, currentSelection, assetType, false);
		}

		protected AssetDescriptor OnDependencyGUI(string label, AssetDescriptor currentSelection, System.Type assetType, bool hideForOneOption)
		{
			return OnDependencyGUI(label, currentSelection, hideForOneOption, (AssetDescriptor asset) => { return assetType.IsAssignableFrom(asset.assetType); });
		}

		protected AssetDescriptor OnDependencyGUI(string label, AssetDescriptor currentSelection, System.Predicate<AssetDescriptor> predicate)
		{
			return OnDependencyGUI(label, currentSelection, false, false, false, predicate);
		}

		protected AssetDescriptor OnDependencyGUI(string label, AssetDescriptor currentSelection, bool hideForOneOption, System.Predicate<AssetDescriptor> predicate)
		{
			return OnDependencyGUI(label, currentSelection, false, hideForOneOption, false, predicate);
		}

		protected AssetDescriptor OnOptionalDependencyGUI(string label, AssetDescriptor currentSelection, System.Type assetType)
		{
			return OnOptionalDependencyGUI(label, currentSelection, assetType, false, false);
		}

		protected AssetDescriptor OnOptionalDependencyGUI(string label, AssetDescriptor currentSelection, System.Type assetType, bool hideForNoOption, bool hideForOneOption)
		{
			return OnDependencyGUI(label, currentSelection, hideForNoOption, hideForOneOption, true, (AssetDescriptor asset) => { return assetType.IsAssignableFrom(asset.assetType); });
		}

		protected AssetDescriptor OnOptionalDependencyGUI(string label, AssetDescriptor currentSelection, System.Predicate<AssetDescriptor> predicate)
		{
			return OnOptionalDependencyGUI(label, currentSelection, false, false, predicate);
		}

		protected AssetDescriptor OnOptionalDependencyGUI(string label, AssetDescriptor currentSelection, bool hideForNoOption, bool hideForOneOption, System.Predicate<AssetDescriptor> predicate)
		{
			return OnDependencyGUI(label, currentSelection, hideForNoOption, hideForOneOption, true, predicate);
		}

		protected AssetDescriptor OnDependencyGUI(string label, AssetDescriptor currentSelection, bool hideForNoOption, bool hideForOneOption, bool includeNone, System.Predicate<AssetDescriptor> predicate)
		{
			var potentialSources = _generator.bundle.GetMatchingGeneratedAssets(_generator, predicate);
			if (potentialSources.Count == 0 && hideForNoOption)
			{
				return null;
			}
			else if (potentialSources.Count == 1 && hideForOneOption)
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
