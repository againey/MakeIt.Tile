﻿using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Experilous.Generation
{
	[System.Serializable]
	[CustomEditor(typeof(GeneratorExecutive), true)]
	public class GeneratorExecutiveEditor : Editor
	{
		[SerializeField] protected GeneratorExecutive _executor;

		[System.Serializable]
		public class GeneratorEditorState
		{
			public Editor editor;
			public bool foldout;
			[System.NonSerialized] public AnimBool foldoutAnimation;
		}

		[System.Serializable]
		public class GeneratorEditorStateDictionary : SerializableDictionary<Generator, GeneratorEditorState>
		{
			public GeneratorEditorStateDictionary() { }
		}

		[SerializeField] private GeneratorEditorStateDictionary _editorStates = new GeneratorEditorStateDictionary();

		[System.NonSerialized] private GUIStyle _generateButtonStyle;
		[System.NonSerialized] private GUIStyle _boldFoldoutStyle;
		[System.NonSerialized] private GUIStyle _popupSelectionLabelStyle;
		private float _sectionEndSpaceHeight;

		private Rect _addGeneratorButtonRect;

		private static Dictionary<System.Type, List<AddGeneratorCategoryGUI>> _addGeneratorCategoryGUIs = new Dictionary<System.Type, List<AddGeneratorCategoryGUI>>();

		protected void OnEnable()
		{
			if (_executor == null)
			{
				if (!(target is GeneratorExecutive)) return;
				_executor = (GeneratorExecutive)target;
			}

			foreach (var editorState in _editorStates)
			{
				editorState.Value.foldoutAnimation = new AnimBool(editorState.Value.foldout);
				editorState.Value.foldoutAnimation.valueChanged.AddListener(Repaint);
			}

			_sectionEndSpaceHeight = 0f;
		}

		private void InitializeStyles()
		{
			if (_generateButtonStyle == null)
			{
				_generateButtonStyle = new GUIStyle(GUI.skin.button);
				_generateButtonStyle.padding.top += 4;
				_generateButtonStyle.padding.bottom += 4;
			}

			if (_boldFoldoutStyle == null)
			{
				_boldFoldoutStyle = new GUIStyle(EditorStyles.foldout);
				_boldFoldoutStyle.fontStyle = FontStyle.Bold;
			}

			if (_popupSelectionLabelStyle == null)
			{
				_popupSelectionLabelStyle = new GUIStyle(EditorStyles.label);
				var hoverBackgroundTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
				hoverBackgroundTexture.SetPixel(0, 0, GUI.skin.settings.selectionColor);
				hoverBackgroundTexture.Apply(false);
				_popupSelectionLabelStyle.hover.background = hoverBackgroundTexture;
				_popupSelectionLabelStyle.padding = new RectOffset(3, 3, 3, 3);
				_popupSelectionLabelStyle.margin = new RectOffset(0, 0, 0, 0);
			}

			if (_sectionEndSpaceHeight == 0f)
			{
				_sectionEndSpaceHeight = _boldFoldoutStyle.CalcHeight(new GUIContent("M"), 100f);
			}
		}

		public override bool UseDefaultMargins()
		{
			return false;
		}

		public override void OnInspectorGUI()
		{
			InitializeStyles();

			GUIExtensions.ResetEnable();
			GUIExtensions.PushEnable(!_executor.isGenerating);

			EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);

			GUIExtensions.PushEnable(_executor.canGenerate);
			if (GUILayout.Button("Generate", _generateButtonStyle))
			{
				_executor.Generate();
			}
			GUIExtensions.PopEnable();

			GUILayout.Space(_sectionEndSpaceHeight / 2);

			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Button(new GUIContent("Detach Assets", "Disassociate all assets from this generator.")))
				{
					if (EditorUtility.DisplayDialog("Detach Assets", "This will disassociate all assets from this generator, which means that these assets will no longer be automatically destroyed or replaced by future executions of the generator.\n\nIt is recommended that you move these assets to another location before you execute the generator again, so that they do not conflict with newly generated assets.", "OK", "Cancel"))
					{
						_executor.DetachAssets();
					}
				}

				if (GUILayout.Button(new GUIContent("Delete Assets", "Delete all assets generated by this generator.")))
				{
					if (EditorUtility.DisplayDialog("Delete Assets", "This will delete all assets generated by and still associated with this generator.\n\nAre you sure you want to proceed?", "OK", "Cancel"))
					{
						_executor.DeleteAssets();
					}
				}
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();

			GUILayout.Space(_sectionEndSpaceHeight);

			foreach (var generator in _executor.generators)
			{
				GeneratorEditorState editorState;
				if (!_editorStates.TryGetValue(generator, out editorState))
				{
					editorState = new GeneratorEditorState();
					editorState.editor = CreateEditor(generator);
					editorState.foldout = true;
					editorState.foldoutAnimation = new AnimBool(editorState.foldout);
					editorState.foldoutAnimation.valueChanged.AddListener(Repaint);
					_editorStates[generator] = editorState;
				}
				editorState.foldout = EditorGUILayout.InspectorTitlebar(editorState.foldout, generator);

				editorState.foldoutAnimation.target = editorState.foldout;
				if (EditorGUILayout.BeginFadeGroup(editorState.foldoutAnimation.faded))
				{
					EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
					editorState.editor.OnInspectorGUI();
					EditorGUILayout.EndVertical();
				}
				EditorGUILayout.EndFadeGroup();
				EditorGUILayout.Space();
			}

			Separator();
			GUILayout.Space(_sectionEndSpaceHeight);

			OnAddGeneratorButtonGUI();

			GUIExtensions.PopEnable();
		}

		private static AddGeneratorCategoryGUI AddCategory(List<AddGeneratorCategoryGUI> categoryGUIs, System.Type categoryType)
		{
			var category = (GeneratorCategoryAttribute)System.Attribute.GetCustomAttribute(categoryType, typeof(GeneratorCategoryAttribute));
			var categoryGUI = new AddGeneratorCategoryGUI(categoryType, category.name);

			if (category.after != null)
			{
				var preceedingCategoryGUI = categoryGUIs.Find((AddGeneratorCategoryGUI existingCategoryGUI) => { return existingCategoryGUI.categoryType == category.after; });
				if (preceedingCategoryGUI == null)
				{
					preceedingCategoryGUI = AddCategory(categoryGUIs, category.after);
				}
				var index = categoryGUIs.IndexOf(preceedingCategoryGUI);
				categoryGUIs.Insert(index + 1, categoryGUI);
			}
			else
			{
				categoryGUIs.Add(categoryGUI);
			}

			return categoryGUI;
		}

		private static bool HasCreatorMethodSignature(MethodInfo methodInfo)
		{
			if (!methodInfo.ReturnType.IsSubclassOf(typeof(Generator))) return false;
			var parameters = methodInfo.GetParameters();
			if (parameters.Length != 2) return false;
			if (parameters[0].IsOut || parameters[0].ParameterType.IsByRef) return false;
			if (parameters[1].IsOut || parameters[1].ParameterType.IsByRef) return false;
			if (parameters[0].ParameterType != typeof(GeneratorExecutive) || parameters[1].ParameterType != typeof(string)) return false;

			return true;
		}

		private static MethodInfo FindCreatorMethod(System.Type generatorType)
		{
			var methods = generatorType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			foreach (var methodInfo in methods )
			{
				if (methodInfo.Name == "CreateInstance")
				{
					if (methodInfo.IsGenericMethod)
					{
						if (methodInfo.GetGenericArguments().Length == 1)
						{
							var concreteMethodInfo = methodInfo.MakeGenericMethod(generatorType);
							if (HasCreatorMethodSignature(concreteMethodInfo))
							{
								return concreteMethodInfo;
							}
						}
					}
					else if (HasCreatorMethodSignature(methodInfo))
					{
						return methodInfo;
					}
				}
			}

			return null;
		}

		private static List<AddGeneratorCategoryGUI> BuildAddGeneratorCategoryGUIs(System.Type generatorCollectionType)
		{
			var categoryGUIs = new List<AddGeneratorCategoryGUI>();

			var assembly = Assembly.GetExecutingAssembly();
			foreach (var generatorType in assembly.GetTypes())
			{
				if (generatorType.IsSubclassOf(typeof(Generator)))
				{
					var attributes = generatorType.GetCustomAttributes(typeof(AssetGeneratorAttribute), true);
					foreach (AssetGeneratorAttribute attribute in attributes)
					{
						if (attribute.assetGeneratorCollectionType.IsAssignableFrom(generatorCollectionType))
						{
							var categoryGUI = categoryGUIs.Find((AddGeneratorCategoryGUI existingCategoryGUI) =>
							{
								return existingCategoryGUI.categoryType == attribute.categoryType;
							});

							if (categoryGUI == null)
							{
								categoryGUI = AddCategory(categoryGUIs, attribute.categoryType);
							}

							var creatorMethod = FindCreatorMethod(generatorType);
							if (creatorMethod != null)
							{
								categoryGUI.items.Add(new AddGeneratorItemGUI(attribute.name, (GeneratorExecutive collection) =>
								{
									var generator = (Generator)creatorMethod.Invoke(null, new object[2] { collection, attribute.name });
									collection.Add(generator);
								}));
								break;
							}
						}
					}
				}
			}

			return categoryGUIs;
		}

		private void OnAddGeneratorButtonGUI()
		{
			if (EditorGUILayoutExtensions.CenteredButton("Add Generator", 0.6f, out _addGeneratorButtonRect))
			{
				List<AddGeneratorCategoryGUI> categoryGUIs;
				var collectionType = _executor.GetType();
				if (!_addGeneratorCategoryGUIs.TryGetValue(collectionType, out categoryGUIs))
				{
					categoryGUIs = BuildAddGeneratorCategoryGUIs(collectionType);
					_addGeneratorCategoryGUIs.Add(collectionType, categoryGUIs);
				}

				PopupWindow.Show(_addGeneratorButtonRect, new AddGeneratorPopupContent(_executor, categoryGUIs, _addGeneratorButtonRect.width));
			}
		}

		private class AddGeneratorItemGUI
		{
			public readonly GUIContent content;
			public System.Action<GeneratorExecutive> onSelect;

			public AddGeneratorItemGUI(string content, System.Action<GeneratorExecutive> onSelect)
			{
				this.content = new GUIContent(content);
				this.onSelect = onSelect;
			}
		}

		private class AddGeneratorCategoryGUI
		{
			public readonly System.Type categoryType;
			public readonly GUIContent content;
			public readonly List<AddGeneratorItemGUI> items;

			public AddGeneratorCategoryGUI(System.Type categoryType, string content)
			{
				this.categoryType = categoryType;
				this.content = new GUIContent(content);
				items = new List<AddGeneratorItemGUI>();
			}

			public AddGeneratorCategoryGUI(System.Type categoryType, string content, List<AddGeneratorItemGUI> items)
			{
				this.categoryType = categoryType;
				this.content = new GUIContent(content);
				this.items = items;
			}
		}

		private class AddGeneratorPopupContent : PopupWindowContent
		{
			private readonly GeneratorExecutive _collection;
			private readonly List<AddGeneratorCategoryGUI> _categories;

			private float _width;

			private GUIStyle _categoryStyle;
			private GUIStyle _itemStyle;

			private AddGeneratorCategoryGUI _openCategory;

			public AddGeneratorPopupContent(GeneratorExecutive collection, List<AddGeneratorCategoryGUI> categories, float width)
			{
				_collection = collection;
				_categories = categories;
				_width = width;

				var hoverBackgroundTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
				hoverBackgroundTexture.SetPixel(0, 0, GUI.skin.settings.selectionColor);
				hoverBackgroundTexture.Apply(false);

				_categoryStyle = new GUIStyle(EditorStyles.label);
				_categoryStyle.hover.background = hoverBackgroundTexture;
				_categoryStyle.padding = new RectOffset(3, 3, 3, 3);
				_categoryStyle.margin = new RectOffset(0, 0, 0, 0);

				_itemStyle = new GUIStyle(EditorStyles.label);
				_itemStyle.hover.background = hoverBackgroundTexture;
				_itemStyle.padding = new RectOffset(3, 3, 3, 3);
				_itemStyle.margin = new RectOffset(0, 0, 0, 0);
			}

			public override Vector2 GetWindowSize()
			{
				float height = 0f;
				if (_openCategory == null)
				{
					foreach (var category in _categories)
					{
						height += _categoryStyle.CalcHeight(category.content, _width);
					}
				}
				else
				{
					foreach (var item in _openCategory.items)
					{
						height += _itemStyle.CalcHeight(item.content, _width);
					}
				}

				return new Vector2(_width, height);
			}

			public override void OnGUI(Rect rect)
			{
				if (_openCategory == null)
				{
					foreach (var category in _categories)
					{
						GUIExtensions.PushEnable(category.items.Count > 0);
						if (GUILayout.Button(category.content, _categoryStyle))
						{
							_openCategory = category;
						}
						GUIExtensions.PopEnable();
					}
				}
				else
				{
					foreach (var item in _openCategory.items)
					{
						if (GUILayout.Button(item.content, _itemStyle))
						{
							item.onSelect(_collection);
							editorWindow.Close();
						}
					}
				}

				editorWindow.Repaint();
			}
		}

		protected void Separator()
		{
			var originalColor = GUI.color;
			GUI.color = Color.grey;
			GUI.DrawTexture(new Rect(0, GUILayoutUtility.GetLastRect().yMax, Screen.width, 1), Texture2D.whiteTexture, ScaleMode.StretchToFill);
			GUI.color = originalColor;
		}
	}
}
