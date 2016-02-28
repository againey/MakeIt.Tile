/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.Collections.Generic;
using System.Reflection;

namespace Experilous.Generation
{
	[System.Serializable]
	[CustomEditor(typeof(GeneratorExecutive), true)]
	public class GeneratorExecutiveEditor : Editor
	{
		[SerializeField] protected GeneratorExecutive _executive;

		[System.Serializable]
		public class GeneratorEditorState
		{
			public Editor editor;
			public bool foldout;
			public Rect _menuButtonRect;
			[System.NonSerialized] public AnimBool foldoutAnimation;
		}

		[System.Serializable]
		public class GeneratorEditorStateDictionary : SerializableDictionary<Generator, GeneratorEditorState>
		{
			public GeneratorEditorStateDictionary() { }
		}

		[SerializeField] private GeneratorEditorStateDictionary _editorStates = new GeneratorEditorStateDictionary();

		[System.NonSerialized] private static GUIStyle _generateButtonStyle;
		[System.NonSerialized] private static GUIStyle _generatorHeaderStyle;
		[System.NonSerialized] private static GUIStyle _generatorHeaderLabelStyle;
		[System.NonSerialized] private static GUIStyle _generatorHeaderMenuButtonStyle;
		[System.NonSerialized] private static GUIStyle _popupSelectionLabelStyle;
		[System.NonSerialized] private static float _sectionEndSpaceHeight;

		[System.NonSerialized] private Rect _addGeneratorButtonRect;
		[System.NonSerialized] private Rect _generatorMenuButtonRect;

		[System.NonSerialized] private static Dictionary<System.Type, List<GeneratorTreeItem>> _generatoryTreeDictionary = new Dictionary<System.Type, List<GeneratorTreeItem>>();

		protected void OnEnable()
		{
			if (_executive == null)
			{
				if (!(target is GeneratorExecutive)) return;
				_executive = (GeneratorExecutive)target;
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

			if (_generatorHeaderStyle == null)
			{
				_generatorHeaderStyle = new GUIStyle(EditorStyles.boldLabel);

				var backgroundTexture = new Texture2D(1, 2, TextureFormat.ARGB32, false);
				backgroundTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0f));
				backgroundTexture.SetPixel(0, 1, Color.gray);
				backgroundTexture.filterMode = FilterMode.Point;
				backgroundTexture.wrapMode = TextureWrapMode.Repeat;
				backgroundTexture.Apply(false, true);
				_generatorHeaderStyle.normal.background = backgroundTexture;
				_generatorHeaderStyle.active.background = backgroundTexture;
				_generatorHeaderStyle.border = new RectOffset(0, 0, 1, 0);
				_generatorHeaderStyle.padding.left += EditorStyles.foldout.padding.left;
				_generatorHeaderStyle.padding.top += 2;
				_generatorHeaderStyle.padding.bottom = 0;
				_generatorHeaderStyle.margin = new RectOffset(0, 0, 2, 0);
			}

			if (_generatorHeaderLabelStyle == null)
			{
				_generatorHeaderLabelStyle = new GUIStyle(EditorStyles.boldLabel);
				_generatorHeaderLabelStyle.margin.left = 0;
				_generatorHeaderLabelStyle.margin.top = 24;
				_generatorHeaderLabelStyle.padding.top = 24;
				_generatorHeaderLabelStyle.padding.bottom = 0;
			}

			if (_generatorHeaderMenuButtonStyle == null)
			{
				_generatorHeaderMenuButtonStyle = new GUIStyle(EditorStyles.label);
				var popupTexture = EditorGUIUtility.FindTexture("_Popup");
				_generatorHeaderMenuButtonStyle.normal.background = popupTexture;
				_generatorHeaderMenuButtonStyle.active.background = popupTexture;
				_generatorHeaderMenuButtonStyle.fixedWidth = popupTexture.width;
				_generatorHeaderMenuButtonStyle.fixedHeight = popupTexture.height;
				_generatorHeaderLabelStyle.margin = new RectOffset(0, 0, 0, 0);
				_generatorHeaderLabelStyle.border = new RectOffset(0, 0, 0, 0);
				_generatorHeaderLabelStyle.padding = new RectOffset(0, 0, 0, 0);
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
				_sectionEndSpaceHeight = EditorStyles.label.CalcHeight(new GUIContent("M"), 1920f);
			}
		}

		public override bool UseDefaultMargins()
		{
			return false;
		}

		public override void OnInspectorGUI()
		{
			InitializeStyles();

			if (AssetDatabase.Contains(_executive))
			{
				var path = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(_executive)));
				GUI.Label(new Rect(44f, 24f, Mathf.Max(0f, Screen.width - 64f), EditorStyles.label.CalcHeight(new GUIContent("M"), 1920f)), path, EditorStyles.boldLabel);
			}

			GUIExtensions.ResetEnable();
			GUIExtensions.PushEnable(!_executive.isGenerating);

			EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);

			GUIExtensions.PushEnable(_executive.canGenerate);
			if (GUILayout.Button("Generate", _generateButtonStyle))
			{
				_executive.Generate();
			}
			GUIExtensions.PopEnable();

			GUILayout.Space(_sectionEndSpaceHeight / 2);

			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Button(new GUIContent("Detach Assets", "Disassociate all assets from this generator.")))
				{
					if (EditorUtility.DisplayDialog("Detach Assets", "This will disassociate all assets from this generator, which means that these assets will no longer be automatically destroyed or replaced by future executions of the generator.\n\nIt is recommended that you move these assets to another location before you execute the generator again, so that they do not conflict with newly generated assets.", "OK", "Cancel"))
					{
						_executive.DetachAssets();
					}
				}

				if (GUILayout.Button(new GUIContent("Delete Assets", "Delete all assets generated by this generator.")))
				{
					if (EditorUtility.DisplayDialog("Delete Assets", "This will delete all assets generated by and still associated with this generator.\n\nAre you sure you want to proceed?", "OK", "Cancel"))
					{
						_executive.DeleteAssets();
					}
				}
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();

			GUILayout.Space(_sectionEndSpaceHeight);

			foreach (var generator in _executive.generators)
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

				GUILayout.BeginHorizontal(_generatorHeaderStyle);
				GUILayout.BeginVertical();
				GUILayout.Space(1f);
				GUILayout.Label(generator.name, _generatorHeaderLabelStyle, GUILayout.ExpandWidth(true));
				GUILayout.EndVertical();
				if (GUILayout.Button(GUIContent.none, _generatorHeaderMenuButtonStyle))
				{
					ShowGeneratorMenu(generator, _editorStates[generator]._menuButtonRect);
				}
				if (Event.current.type == EventType.Repaint)
				{
					_editorStates[generator]._menuButtonRect = GUILayoutUtility.GetLastRect();
				}

				GUILayout.EndHorizontal();
				var rect = GUILayoutUtility.GetLastRect();
				rect.xMin += EditorStyles.foldout.padding.left;
				rect.yMin += 3;
				rect.yMax = rect.yMin + EditorStyles.foldout.CalcHeight(new GUIContent("M"), 1920f);
				editorState.foldout = EditorGUI.Foldout(rect, editorState.foldout, GUIContent.none);

				editorState.foldoutAnimation.target = editorState.foldout;
				if (EditorGUILayout.BeginFadeGroup(editorState.foldoutAnimation.faded))
				{
					EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
					editorState.editor.OnInspectorGUI();
					EditorGUILayout.Space();
					EditorGUILayout.EndVertical();
				}
				EditorGUILayout.EndFadeGroup();
			}

			Separator();
			GUILayout.Space(_sectionEndSpaceHeight);

			OnAddGeneratorButtonGUI();

			GUIExtensions.PopEnable();
		}

		private void BuildInsertNewGeneratorMenu(GenericMenu menu, string path, List<GeneratorTreeItem> items, int insertionIndex)
		{
			foreach (var item in items)
			{
				if (item is GeneratorTreeCategory)
				{
					var category = (GeneratorTreeCategory)item;
					BuildInsertNewGeneratorMenu(menu, path + category.content.text + '/', category.items, insertionIndex);
				}
				else if (item is GeneratorTreeGenerator)
				{
					var generator = (GeneratorTreeGenerator)item;
					GUIExtensions.PushEnable(generator.onSelect != null);

					GenericMenu.MenuFunction callback = null;
					if (generator.onSelect != null)
					{
						callback = () =>
						{
							generator.onSelect(_executive, insertionIndex);
						};
					}
					menu.AddItem(new GUIContent(path + generator.content.text, generator.content.image, generator.content.tooltip), false, callback);
				}
			}
		}

		private void ShowGeneratorMenu(Generator generator, Rect menuButtonRect)
		{
			var menu = new GenericMenu();

			menu.AddItem(new GUIContent("Reset"), false, () => { generator.Reset(); });

			menu.AddSeparator("");

			if (_executive.CanRemove(generator))
			{
				menu.AddItem(new GUIContent("Remove Generator"), false, () => { _executive.Remove(generator); });
			}
			else
			{
				menu.AddDisabledItem(new GUIContent("Remove Generator"));
			}

			if (generatorTree.Count > 0)
			{
				BuildInsertNewGeneratorMenu(menu, "Insert New Generator/", generatorTree, _executive.generators.IndexOf(generator));
			}
			else
			{
				menu.AddDisabledItem(new GUIContent("Insert New Generator"));
			}

			menu.AddSeparator("");

			if (_executive.CanMoveUp(generator))
			{
				menu.AddItem(new GUIContent("Move to Top"), false, () => { _executive.MoveUp(generator, true); });
				menu.AddItem(new GUIContent("Move Up"), false, () => { _executive.MoveUp(generator); });
			}
			else
			{
				menu.AddDisabledItem(new GUIContent("Move to Top"));
				menu.AddDisabledItem(new GUIContent("Move Up"));
			}

			if (_executive.CanMoveDown(generator))
			{
				menu.AddItem(new GUIContent("Move Down"), false, () => { _executive.MoveDown(generator); });
				menu.AddItem(new GUIContent("Move to Bottom"), false, () => { _executive.MoveDown(generator, true); });
			}
			else
			{
				menu.AddDisabledItem(new GUIContent("Move Down"));
				menu.AddDisabledItem(new GUIContent("Move to Bottom"));
			}

			menu.AddSeparator("");

			menu.AddItem(new GUIContent("Edit Script"), false, () => { generator.EditScript(); });

			menu.DropDown(menuButtonRect);
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

		private static GeneratorTreeGenerator AddGeneratorToTree(List<GeneratorTreeItem> generatorTree, System.Type generatorType, GeneratorAttribute attribute)
		{
			var path = attribute.path;

			int depth = 0;
			int categoryCount = path.Length - 1;
			GeneratorTreeCategory parent = null;
			var items = generatorTree;

			while (depth < categoryCount)
			{
				var category = items.Find((GeneratorTreeItem item) =>
				{
					return item.content.text == path[depth] && item is GeneratorTreeCategory;
				});

				if (category == null) break;

				parent = (GeneratorTreeCategory)category;
				items = parent.items;
				++depth;
			}

			while (depth < categoryCount)
			{
				var category = new GeneratorTreeCategory(path[depth], parent);
				items.Add(category);
				parent = category;
				items = parent.items;
				++depth;
			}

			var creatorMethod = FindCreatorMethod(generatorType);
			if (creatorMethod != null)
			{
				var item = new GeneratorTreeGenerator(path[depth], parent, (GeneratorExecutive collection, int insertionIndex) =>
				{
					var generator = (Generator)creatorMethod.Invoke(null, new object[2] { collection, attribute.name });
					collection.Insert(insertionIndex, generator);
				});

				items.Add(item);
				return item;
			}
			else
			{
				var item = new GeneratorTreeGenerator(new GUIContent(path[depth], "A generator of this type cannot be created because no valid creation function was found."), parent, null);
				items.Add(item);
				return item;
			}
		}

		private static void SortGeneratorTree(List<GeneratorTreeItem> items, System.Comparison<GeneratorTreeItem> comparison)
		{
			items.Sort(comparison);
			foreach (var item in items)
			{
				if (item is GeneratorTreeCategory)
				{
					var category = (GeneratorTreeCategory)item;
					SortGeneratorTree(category.items, comparison);
				}
			}
		}

		private static List<GeneratorTreeItem> BuildGeneratorTree(System.Type generatorExecutiveType)
		{
			var generatorTree = new List<GeneratorTreeItem>();

			var assembly = Assembly.GetExecutingAssembly();
			foreach (var generatorType in assembly.GetTypes())
			{
				if (generatorType.IsSubclassOf(typeof(Generator)))
				{
					var attributes = generatorType.GetCustomAttributes(typeof(GeneratorAttribute), true);
					foreach (GeneratorAttribute attribute in attributes)
					{
						if (attribute.generatorExecutiveType.IsAssignableFrom(generatorExecutiveType))
						{
							AddGeneratorToTree(generatorTree, generatorType, attribute);
						}
					}
				}
			}

			SortGeneratorTree(generatorTree, (GeneratorTreeItem lhs, GeneratorTreeItem rhs) =>
			{
				if (lhs.GetType() != rhs.GetType())
				{
					return (lhs is GeneratorTreeGenerator) ? +1 : -1;
				}
				else
				{
					return System.StringComparer.CurrentCultureIgnoreCase.Compare(lhs.content.text, rhs.content.text);
				}
			});

			return generatorTree;
		}

		private void OnAddGeneratorButtonGUI()
		{
			if (EditorGUILayoutExtensions.CenteredButton("Add Generator", 0.6f, out _addGeneratorButtonRect))
			{
				PopupWindow.Show(_addGeneratorButtonRect, new AddGeneratorPopupContent(_executive, generatorTree, _addGeneratorButtonRect.width));
			}
		}

		private List<GeneratorTreeItem> generatorTree
		{
			get
			{
				List<GeneratorTreeItem> categories;
				var generatorExecutiveType = _executive.GetType();
				if (!_generatoryTreeDictionary.TryGetValue(generatorExecutiveType, out categories))
				{
					categories = BuildGeneratorTree(generatorExecutiveType);
					_generatoryTreeDictionary.Add(generatorExecutiveType, categories);
				}
				return categories;
			}
		}

		private class GeneratorTreeItem
		{
			public readonly GUIContent content;
			public GeneratorTreeCategory parent;

			public GeneratorTreeItem(string name)
			{
				content = new GUIContent(name);
			}

			public GeneratorTreeItem(GUIContent content)
			{
				this.content = content;
			}

			public GeneratorTreeItem(string name, GeneratorTreeCategory parent)
			{
				content = new GUIContent(name);
				this.parent = parent;
			}

			public GeneratorTreeItem(GUIContent content, GeneratorTreeCategory parent)
			{
				this.content = content;
				this.parent = parent;
			}
		}

		private class GeneratorTreeGenerator : GeneratorTreeItem
		{
			public System.Action<GeneratorExecutive, int> onSelect;

			public GeneratorTreeGenerator(string name, System.Action<GeneratorExecutive, int> onSelect)
				: base(name)
			{
				this.onSelect = onSelect;
			}

			public GeneratorTreeGenerator(GUIContent content, System.Action<GeneratorExecutive, int> onSelect)
				: base(content)
			{
				this.onSelect = onSelect;
			}

			public GeneratorTreeGenerator(string name, GeneratorTreeCategory parent, System.Action<GeneratorExecutive, int> onSelect)
				: base(name, parent)
			{
				this.onSelect = onSelect;
			}

			public GeneratorTreeGenerator(GUIContent content, GeneratorTreeCategory parent, System.Action<GeneratorExecutive, int> onSelect)
				: base(content, parent)
			{
				this.onSelect = onSelect;
			}
		}

		private class GeneratorTreeCategory : GeneratorTreeItem
		{
			public readonly List<GeneratorTreeItem> items;

			public GeneratorTreeCategory(string name)
				: base(name)
			{
				items = new List<GeneratorTreeItem>();
			}

			public GeneratorTreeCategory(GUIContent content)
				: base(content)
			{
				items = new List<GeneratorTreeItem>();
			}

			public GeneratorTreeCategory(string name, GeneratorTreeCategory parent)
				: base(name, parent)
			{
				items = new List<GeneratorTreeItem>();
			}

			public GeneratorTreeCategory(GUIContent content, GeneratorTreeCategory parent)
				: base(content, parent)
			{
				items = new List<GeneratorTreeItem>();
			}
		}

		private class AddGeneratorPopupContent : PopupWindowContent
		{
			private readonly GeneratorExecutive _executive;
			private readonly List<GeneratorTreeItem> _generatorTree;

			private float _width;

			private GUIStyle _categoryStyle;
			private GUIStyle _generatorStyle;

			private GeneratorTreeCategory _openCategory;

			public AddGeneratorPopupContent(GeneratorExecutive generatorExecutive, List<GeneratorTreeItem> generatorTree, float width)
			{
				_executive = generatorExecutive;
				_generatorTree = generatorTree;
				_width = width;

				var hoverBackgroundTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
				hoverBackgroundTexture.SetPixel(0, 0, GUI.skin.settings.selectionColor);
				hoverBackgroundTexture.Apply(false);

				_categoryStyle = new GUIStyle(EditorStyles.label);
				_categoryStyle.hover.background = hoverBackgroundTexture;
				_categoryStyle.padding = new RectOffset(3, 3, 3, 3);
				_categoryStyle.margin = new RectOffset(0, 0, 0, 0);

				_generatorStyle = new GUIStyle(EditorStyles.label);
				_generatorStyle.hover.background = hoverBackgroundTexture;
				_generatorStyle.padding = new RectOffset(3, 3, 3, 3);
				_generatorStyle.margin = new RectOffset(0, 0, 0, 0);
			}

			public override Vector2 GetWindowSize()
			{
				float height = 0f;
				if (_openCategory == null)
				{
					foreach (var category in _generatorTree)
					{
						height += _categoryStyle.CalcHeight(category.content, _width);
					}
				}
				else
				{
					foreach (var item in _openCategory.items)
					{
						height += _generatorStyle.CalcHeight(item.content, _width);
					}
				}

				return new Vector2(_width, height);
			}

			public override void OnGUI(Rect rect)
			{
				var items = _openCategory == null ? _generatorTree : _openCategory.items;

				foreach (var item in items)
				{
					if (item is GeneratorTreeCategory)
					{
						var category = (GeneratorTreeCategory)item;
						GUIExtensions.PushEnable(category.items.Count > 0);
						if (GUILayout.Button(category.content, _categoryStyle))
						{
							_openCategory = category;
						}
						GUIExtensions.PopEnable();
					}
					else if (item is GeneratorTreeGenerator)
					{
						var generator = (GeneratorTreeGenerator)item;
						GUIExtensions.PushEnable(generator.onSelect != null);
						if (GUILayout.Button(generator.content, _generatorStyle))
						{
							generator.onSelect(_executive, _executive.generators.Count);
							editorWindow.Close();
						}
						GUIExtensions.PopEnable();
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
