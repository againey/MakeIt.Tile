﻿using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Experilous.Topological
{
	[System.Serializable ]
	public abstract class AssetGeneratorBundleEditor : Editor
	{
		[SerializeField] protected AssetGeneratorBundle _generatorBundle;

		[SerializeField] private string _generatePath = null;
		[SerializeField] private string _generateName = null;
		private string _latestAssetPath = null;

		[System.Serializable]
		public class AssetGeneratorEditorState
		{
			public Editor editor;
			public bool foldout;
			[System.NonSerialized] public AnimBool foldoutAnimation;
		}

		[System.Serializable]
		public class AssetGeneratorEditorStateDictionary : SerializableDictionary<AssetGenerator, AssetGeneratorEditorState>
		{
			public AssetGeneratorEditorStateDictionary() { }
		}

		[SerializeField] private AssetGeneratorEditorStateDictionary _editorStates = new AssetGeneratorEditorStateDictionary();

		private GUIStyle _boldFoldoutStyle;
		private GUIStyle _popupSelectionLabelStyle;
		private float _sectionEndSpaceHeight;

		private Rect _addVertexAttributesButtonRect;
		private Rect _addGeneratorButtonRect;

		private static Dictionary<System.Type, List<AddGeneratorCategoryGUI>> _addGeneratorCategoryGUIs = new Dictionary<System.Type, List<AddGeneratorCategoryGUI>>();

		protected string GenerateNameAndPathFromAssetPath(string assetPath, out string name, out string path)
		{
			name = Path.GetFileNameWithoutExtension(assetPath);

			if (name.EndsWith(_generatorBundle.nameSuffix))
			{
				name = name.Substring(0, name.Length - _generatorBundle.nameSuffix.Length);
			}

			path = AssetUtility.TrimProjectPath(Path.GetDirectoryName(assetPath));

			return assetPath;
		}

		protected void OnEnable()
		{
			if (_generatorBundle == null)
			{
				if (!(target is AssetGeneratorBundle)) return;
				_generatorBundle = (AssetGeneratorBundle)target;
			}

			if (_generateName == null)
			{
				if (AssetDatabase.Contains(_generatorBundle))
				{
					_latestAssetPath = GenerateNameAndPathFromAssetPath(AssetUtility.GetFullCanonicalAssetPath(_generatorBundle), out _generateName, out _generatePath);
				}
				else
				{
					_generateName = _generatorBundle.defaultName;

					if (_generatePath == null)
					{
						_generatePath = AssetUtility.projectRelativeDataPath;
					}
				}
			}

			if (_generatePath == null)
			{
				_generatePath = AssetUtility.projectRelativeDataPath;
			}

			foreach (var editorState in _editorStates)
			{
				editorState.Value.foldoutAnimation = new AnimBool(editorState.Value.foldout);
				editorState.Value.foldoutAnimation.valueChanged.AddListener(Repaint);
			}

			_boldFoldoutStyle = null;
			_popupSelectionLabelStyle = null;

			_sectionEndSpaceHeight = 0f;
		}

		protected void OnDisable()
		{
		}

		protected void OnUpdate()
		{
			if (_latestAssetPath != null)
			{
				if (AssetDatabase.Contains(_generatorBundle))
				{
					string name;
					string path;
					var assetPath = AssetUtility.GetFullCanonicalAssetPath(_generatorBundle);
					GenerateNameAndPathFromAssetPath(AssetUtility.GetFullCanonicalAssetPath(_generatorBundle), out name, out path);

					if (assetPath != _latestAssetPath)
					{
						_generateName = name;
						_generatePath = path;
						_latestAssetPath = assetPath;
					}
					else if (_generateName != name || _generatePath != path)
					{
						_latestAssetPath = null;
					}
				}
				else
				{
					_latestAssetPath = null;
				}
			}
		}

		private void InitializeStyles()
		{
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

			EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);

			EditorGUILayout.BeginHorizontal();
			_generatePath = EditorGUILayout.TextField("Generation Path", _generatePath, GUILayout.ExpandWidth(true));
			if (GUILayout.Button("...", GUILayout.ExpandWidth(false)))
			{
				var path = EditorUtility.SaveFolderPanel("Select Generation Folder", AssetUtility.GetCanonicalPath(Path.Combine(AssetUtility.canonicalProjectPath, _generatePath)), "");
				if (!string.IsNullOrEmpty(path))
				{
					try
					{
						_generatePath = AssetUtility.GetCanonicalPath(Path.Combine(AssetUtility.projectRelativeDataPath, AssetUtility.TrimDataPath(path)));
					}
					catch (System.InvalidOperationException)
					{
						EditorUtility.DisplayDialog("Select Generation Folder", string.Format("The selected folder must be within the project's data folder ({0}).", AssetUtility.projectRelativeDataPath), "OK");
					}
				}
			}
			EditorGUILayout.EndHorizontal();
			_generateName = EditorGUILayout.TextField("Generation Name", _generateName);

			GUI.enabled = _generatePath != null && !string.IsNullOrEmpty(_generateName) && _generatorBundle.CanGenerate();
			if (CenteredButton("Generate", 0.6f))
			{
				_generatorBundle.Generate(_generatePath, _generateName);
			}
			GUI.enabled = true;
			EditorGUILayout.EndVertical();

			GUILayout.Space(_sectionEndSpaceHeight);

			foreach (var generator in _generatorBundle.generators)
			{
				AssetGeneratorEditorState editorState;
				if (!_editorStates.TryGetValue(generator, out editorState))
				{
					editorState = new AssetGeneratorEditorState();
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
		}

		private AddGeneratorCategoryGUI AddCategory(List<AddGeneratorCategoryGUI> categoryGUIs, System.Type categoryType)
		{
			var category = (AssetGeneratorCategoryAttribute)System.Attribute.GetCustomAttribute(categoryType, typeof(AssetGeneratorCategoryAttribute));
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

		private bool HasCreatorMethodSignature(MethodInfo methodInfo)
		{
			if (!methodInfo.ReturnType.IsSubclassOf(typeof(AssetGenerator))) return false;
			if (methodInfo.IsGenericMethod) return false;
			var parameters = methodInfo.GetParameters();
			if (parameters.Length != 2) return false;
			if (parameters[0].IsOut || parameters[0].ParameterType.IsByRef) return false;
			if (parameters[1].IsOut || parameters[1].ParameterType.IsByRef) return false;
			if (parameters[0].ParameterType != typeof(AssetGeneratorBundle) || parameters[1].ParameterType != typeof(string)) return false;

			return true;
		}

		private MethodInfo FindCreatorMethod(System.Type generatorType)
		{
			var methods = generatorType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (var methodInfo in methods)
			{
				var creatorAttribute = System.Attribute.GetCustomAttribute(methodInfo, typeof(AssetGeneratorCreatorAttribute));
				if (creatorAttribute != null && HasCreatorMethodSignature(methodInfo))
				{
					return methodInfo;
				}
			}

			foreach (var methodInfo in methods)
			{
				if (methodInfo.Name == "CreateDefaultInstance" && HasCreatorMethodSignature(methodInfo))
				{
					return methodInfo;
				}
			}

			return null;
		}

		private List<AddGeneratorCategoryGUI> BuildAddGeneratorCategoryGUIs()
		{
			var categoryGUIs = new List<AddGeneratorCategoryGUI>();

			var assembly = Assembly.GetExecutingAssembly();
			foreach (var generatorType in assembly.GetTypes())
			{
				if (generatorType.IsSubclassOf(typeof(AssetGenerator)))
				{
					var attributes = generatorType.GetCustomAttributes(typeof(AssetGeneratorAttribute), true);
					foreach (AssetGeneratorAttribute attribute in attributes)
					{
						if (attribute.assetGeneratorBundleType.IsInstanceOfType(_generatorBundle))
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
								categoryGUI.items.Add(new AddGeneratorItemGUI(attribute.name, (AssetGeneratorBundle bundle) =>
								{
									var generator = (AssetGenerator)creatorMethod.Invoke(null, new object[2] { bundle, name });
									bundle.Add(generator);
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
			if (CenteredButton("Add Generator", 0.6f, ref _addGeneratorButtonRect))
			{
				List<AddGeneratorCategoryGUI> categoryGUIs;
				var bundleType = _generatorBundle.GetType();
				if (!_addGeneratorCategoryGUIs.TryGetValue(bundleType, out categoryGUIs))
				{
					categoryGUIs = BuildAddGeneratorCategoryGUIs();
					_addGeneratorCategoryGUIs.Add(bundleType, categoryGUIs);
				}

				PopupWindow.Show(_addGeneratorButtonRect, new AddGeneratorPopupContent(_generatorBundle, categoryGUIs, _addGeneratorButtonRect.width));
			}
		}

		private class AddGeneratorItemGUI
		{
			public readonly GUIContent content;
			public System.Action<AssetGeneratorBundle> onSelect;

			public AddGeneratorItemGUI(string content, System.Action<AssetGeneratorBundle> onSelect)
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
			private readonly AssetGeneratorBundle _bundle;
			private readonly List<AddGeneratorCategoryGUI> _categories;

			private float _width;

			private GUIStyle _categoryStyle;
			private GUIStyle _itemStyle;

			private AddGeneratorCategoryGUI _openCategory;

			public AddGeneratorPopupContent(AssetGeneratorBundle bundle, List<AddGeneratorCategoryGUI> categories, float width)
			{
				_bundle = bundle;
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
						GUI.enabled = category.items.Count > 0;
						if (GUILayout.Button(category.content, _categoryStyle))
						{
							_openCategory = category;
						}
					}
					GUI.enabled = true;
				}
				else
				{
					foreach (var item in _openCategory.items)
					{
						if (GUILayout.Button(item.content, _itemStyle))
						{
							item.onSelect(_bundle);
							editorWindow.Close();
						}
					}
				}

				editorWindow.Repaint();
			}
		}

		protected bool CenteredButton(string content, float widthProportion)
		{
			if (widthProportion >= 1f || widthProportion <= 0f)
			{
				return GUILayout.Button(content);
			}
			else
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				bool clicked = GUILayout.Button(content, GUILayout.Width(Screen.width * widthProportion));
				EditorGUILayout.Space();
				EditorGUILayout.EndHorizontal();
				return clicked;
			}
		}

		protected bool CenteredButton(string content, float widthProportion, ref Rect buttonRect)
		{
			if (widthProportion >= 1f || widthProportion <= 0f)
			{
				var clicked = GUILayout.Button(content);
				buttonRect = (Event.current.type == EventType.Repaint) ? GUILayoutUtility.GetLastRect() : buttonRect;
				return clicked;
			}
			else
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				bool clicked = GUILayout.Button(content, GUILayout.Width(Screen.width * widthProportion));
				buttonRect = (Event.current.type == EventType.Repaint) ? GUILayoutUtility.GetLastRect() : buttonRect;
				EditorGUILayout.Space();
				EditorGUILayout.EndHorizontal();
				return clicked;
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
