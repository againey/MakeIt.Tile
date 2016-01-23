using UnityEngine;
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

		[SerializeField] private string _bundleLocation = "";
		[SerializeField] private string _bundleName = "";
		[SerializeField] private string _latestAssetPath = "";

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

		protected void OnEnable()
		{
			if (_generatorBundle == null)
			{
				if (!(target is AssetGeneratorBundle)) return;
				_generatorBundle = (AssetGeneratorBundle)target;
			}

			if (string.IsNullOrEmpty(_bundleLocation) || string.IsNullOrEmpty(_bundleName))
			{
				_latestAssetPath = _generatorBundle.GetPersistedLocationAndName(out _bundleLocation, out _bundleName);
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

		protected void Update()
		{
			// Last time we checked, did we have a known path at which this bundle was persisted,
			// and which hasn't been nullified by any edits in the inspector?
			if (!string.IsNullOrEmpty(_latestAssetPath))
			{
				// Is this bundle still persisted?
				if (AssetDatabase.Contains(_generatorBundle))
				{
					string location;
					string name;
					var assetPath = _generatorBundle.GetPersistedLocationAndName(out location, out name);

					// Is it no longer persisted at the same location as the last known location?
					if (assetPath != _latestAssetPath)
					{
						// The location must have changed for reasons other than through edits in the inspector.
						// We'll automatically replace our inspector values with the newly inferred values.
						_bundleLocation = location;
						_bundleName = name;
						_latestAssetPath = assetPath;
					}
					// It is still persisted at the same location.
					// Has the location or name been changed from within the inspector?
					else if (_bundleLocation != location || _bundleName != name)
					{
						// Let's forget we ever knew what the persisted location ever was.
						// The values in the inspector take precedence because the user is editing them.
						_latestAssetPath = "";
					}
				}
				// Nope, the bundle is no longer persisted.
				else
				{
					// The last known path is irrelevant.  Might as well treat whatever is in
					// the inspector as having precedence.
					_latestAssetPath = "";
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
			Update();

			InitializeStyles();

			EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);

			EditorGUILayout.BeginHorizontal();
			_bundleLocation = EditorGUILayout.TextField("Generation Path", _bundleLocation, GUILayout.ExpandWidth(true));
			if (GUILayout.Button("...", GUILayout.ExpandWidth(false)))
			{
				var path = EditorUtility.SaveFolderPanel("Select Generation Folder", AssetUtility.GetCanonicalPath(Path.Combine(AssetUtility.canonicalProjectPath, _bundleLocation)), "");
				if (!string.IsNullOrEmpty(path))
				{
					try
					{
						_bundleLocation = AssetUtility.GetCanonicalPath(Path.Combine(AssetUtility.projectRelativeDataPath, AssetUtility.TrimDataPath(path)));
					}
					catch (System.InvalidOperationException)
					{
						EditorUtility.DisplayDialog("Select Generation Folder", string.Format("The selected folder must be within the project's data folder ({0}).", AssetUtility.projectRelativeDataPath), "OK");
					}
				}
			}
			EditorGUILayout.EndHorizontal();
			_bundleName = EditorGUILayout.TextField("Generation Name", _bundleName);

			GUI.enabled = _bundleLocation != null && !string.IsNullOrEmpty(_bundleName) && _generatorBundle.CanGenerate();
			if (CenteredButton("Generate", 0.6f))
			{
				_generatorBundle.Generate(_bundleLocation, _bundleName);
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
