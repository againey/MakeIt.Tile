using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Experilous.Topological
{
	[System.Serializable]
	[CustomEditor(typeof(AssetGeneratorCollection), true)]
	public class AssetGeneratorCollectionEditor : Editor
	{
		[SerializeField] protected AssetGeneratorCollection _generatorCollection;
		[SerializeField] protected string _generationName;
		[SerializeField] protected string _generationPath;

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

		private Rect _addGeneratorButtonRect;

		private static Dictionary<System.Type, List<AddGeneratorCategoryGUI>> _addGeneratorCategoryGUIs = new Dictionary<System.Type, List<AddGeneratorCategoryGUI>>();

		protected void OnEnable()
		{
			if (_generatorCollection == null)
			{
				if (!(target is AssetGeneratorCollection)) return;
				_generatorCollection = (AssetGeneratorCollection)target;
			}

			foreach (var editorState in _editorStates)
			{
				editorState.Value.foldoutAnimation = new AnimBool(editorState.Value.foldout);
				editorState.Value.foldoutAnimation.valueChanged.AddListener(Repaint);
			}

			_boldFoldoutStyle = null;
			_popupSelectionLabelStyle = null;

			_sectionEndSpaceHeight = 0f;

			if (string.IsNullOrEmpty(_generationName)) _generationName = _generatorCollection.generationName;
			if (string.IsNullOrEmpty(_generationPath)) _generationPath = _generatorCollection.generationPath;
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

			_generationName = EditorGUILayout.TextField("Generation Name", _generationName);
			EditorGUILayout.BeginHorizontal();
			_generationPath = EditorGUILayout.TextField("Generation Path", _generationPath, GUILayout.ExpandWidth(true));
			if (GUILayout.Button("...", GUILayout.ExpandWidth(false)))
			{
				var path = EditorUtility.SaveFolderPanel("Select Generation Folder", AssetUtility.GetCanonicalPath(Path.Combine(AssetUtility.canonicalProjectPath, _generationPath)), "");
				if (!string.IsNullOrEmpty(path))
				{
					try
					{
						_generationPath = AssetUtility.GetCanonicalPath(Path.Combine(AssetUtility.projectRelativeDataPath, AssetUtility.TrimDataPath(path)));
					}
					catch (System.InvalidOperationException)
					{
						EditorUtility.DisplayDialog("Select Generation Folder", string.Format("The selected folder must be within the project's data folder ({0}).", AssetUtility.projectRelativeDataPath), "OK");
					}
				}
			}
			EditorGUILayout.EndHorizontal();

			GUI.enabled = _generatorCollection.canGenerate;

			if (CenteredButton("Generate", 0.6f))
			{
				_generatorCollection.Generate(_generationName, _generationPath);
			}
			GUI.enabled = true;
			EditorGUILayout.EndVertical();

			GUILayout.Space(_sectionEndSpaceHeight);

			foreach (var generator in _generatorCollection.generators)
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

		private static AddGeneratorCategoryGUI AddCategory(List<AddGeneratorCategoryGUI> categoryGUIs, System.Type categoryType)
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

		private static bool HasCreatorMethodSignature(MethodInfo methodInfo)
		{
			if (!methodInfo.ReturnType.IsSubclassOf(typeof(AssetGenerator))) return false;
			var parameters = methodInfo.GetParameters();
			if (parameters.Length != 2) return false;
			if (parameters[0].IsOut || parameters[0].ParameterType.IsByRef) return false;
			if (parameters[1].IsOut || parameters[1].ParameterType.IsByRef) return false;
			if (parameters[0].ParameterType != typeof(AssetGeneratorCollection) || parameters[1].ParameterType != typeof(string)) return false;

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
				if (generatorType.IsSubclassOf(typeof(AssetGenerator)))
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
								categoryGUI.items.Add(new AddGeneratorItemGUI(attribute.name, (AssetGeneratorCollection collection) =>
								{
									var generator = (AssetGenerator)creatorMethod.Invoke(null, new object[2] { collection, attribute.name });
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
			if (CenteredButton("Add Generator", 0.6f, ref _addGeneratorButtonRect))
			{
				List<AddGeneratorCategoryGUI> categoryGUIs;
				var collectionType = _generatorCollection.GetType();
				if (!_addGeneratorCategoryGUIs.TryGetValue(collectionType, out categoryGUIs))
				{
					categoryGUIs = BuildAddGeneratorCategoryGUIs(collectionType);
					_addGeneratorCategoryGUIs.Add(collectionType, categoryGUIs);
				}

				PopupWindow.Show(_addGeneratorButtonRect, new AddGeneratorPopupContent(_generatorCollection, categoryGUIs, _addGeneratorButtonRect.width));
			}
		}

		private class AddGeneratorItemGUI
		{
			public readonly GUIContent content;
			public System.Action<AssetGeneratorCollection> onSelect;

			public AddGeneratorItemGUI(string content, System.Action<AssetGeneratorCollection> onSelect)
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
			private readonly AssetGeneratorCollection _collection;
			private readonly List<AddGeneratorCategoryGUI> _categories;

			private float _width;

			private GUIStyle _categoryStyle;
			private GUIStyle _itemStyle;

			private AddGeneratorCategoryGUI _openCategory;

			public AddGeneratorPopupContent(AssetGeneratorCollection collection, List<AddGeneratorCategoryGUI> categories, float width)
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
							item.onSelect(_collection);
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
