using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace Experilous
{
	[CustomEditor(typeof(AssetDescriptor))]
	public class AssetDescriptorEditor : Editor
	{
		[SerializeField] protected bool _editModeFoldout = false;
		protected AnimBool _editModeFoldoutAnimation;

		protected static GUIStyle _outlineStyle = null;

		protected static GUIContent[] _fullAvailabilityContent = new GUIContent[]
		{
			new GUIContent("Available Always"),
			new GUIContent("Available During Generation"),
			new GUIContent("Available After Generation"),
		};

		protected static int[] _fullAvailabilityValues = new int[]
		{
			(int)AssetDescriptor.Availability.Always,
			(int)AssetDescriptor.Availability.DuringGeneration,
			(int)AssetDescriptor.Availability.AfterGeneration,
		};

		protected static GUIContent[] _alwaysAfterAvailabilityContent = new GUIContent[]
		{
			new GUIContent("Available Always"),
			new GUIContent("Available After Generation"),
		};

		protected static int[] _alwaysAfterAvailabilityValues = new int[]
		{
			(int)AssetDescriptor.Availability.Always,
			(int)AssetDescriptor.Availability.AfterGeneration,
		};

		protected static GUIContent[] _neverAfterAvailabilityContent = new GUIContent[]
		{
			new GUIContent("Available During Generation"),
		};

		protected static int[] _neverAfterAvailabilityValues = new int[]
		{
			(int)AssetDescriptor.Availability.DuringGeneration,
		};

		protected void OnEnable()
		{
			_editModeFoldoutAnimation = new AnimBool(_editModeFoldout);
			_editModeFoldoutAnimation.valueChanged.AddListener(Repaint);
		}

		protected virtual void InitializeStyles()
		{
			if (_outlineStyle == null)
			{
				_outlineStyle = new GUIStyle(EditorStyles.helpBox);
				_outlineStyle.padding.left += EditorStyles.foldout.padding.left;
			}
		}

		public override void OnInspectorGUI( )
		{
			InitializeStyles();

			var descriptor = (AssetDescriptor)target;

			float foldoutMinWidth, foldoutMaxWidth;
			EditorStyles.foldout.CalcMinMaxWidth(GUIContent.none, out foldoutMinWidth, out foldoutMaxWidth);

			EditorGUILayout.BeginVertical(_outlineStyle);

			EditorGUILayout.BeginHorizontal();
			var foldoutRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, GUILayout.Width(0));
			_editModeFoldout = EditorGUI.Foldout(foldoutRect, _editModeFoldout, GUIContent.none, false);
			_editModeFoldoutAnimation.target = _editModeFoldout;
			EditorGUILayout.BeginVertical();

			if (_editModeFoldout)
			{
				EditorGUILayout.BeginHorizontal();
				descriptor.isEnabled = EditorGUILayout.ToggleLeft("Enabled", descriptor.isEnabled, GUILayout.Width(EditorStyles.toggle.CalcSize(new GUIContent("Enabled")).x));
				GUI.enabled = descriptor.isEnabled;
				if (descriptor.mustBeAvailableAfterGeneration)
				{
					descriptor.availability = (AssetDescriptor.Availability)EditorGUILayout.IntPopup((int)descriptor.availability, _alwaysAfterAvailabilityContent, _alwaysAfterAvailabilityValues);
				}
				else if (descriptor.canBeAvailableAfterGeneration)
				{
					descriptor.availability = (AssetDescriptor.Availability)EditorGUILayout.IntPopup((int)descriptor.availability, _fullAvailabilityContent, _fullAvailabilityValues);
				}
				else
				{
					descriptor.availability = (AssetDescriptor.Availability)EditorGUILayout.IntPopup((int)descriptor.availability, _neverAfterAvailabilityContent, _neverAfterAvailabilityValues);
				}
				EditorGUILayout.EndHorizontal();
			}
			else
			{
				EditorGUILayout.BeginHorizontal();
				descriptor.isEnabled = EditorGUILayout.Toggle(descriptor.isEnabled, GUILayout.Width(EditorStyles.toggle.CalcSize(GUIContent.none).x));
				GUI.enabled = descriptor.isEnabled;
				EditorGUILayout.LabelField(new GUIContent(string.Format("{0} ({1})", descriptor.name, descriptor.assetType.GetPrettyName()), descriptor.assetType.GetPrettyName(true)));
				EditorGUILayout.EndHorizontal();
			}

			if (EditorGUILayout.BeginFadeGroup(_editModeFoldoutAnimation.faded))
			{
				var priorLabelWidth = EditorGUIUtility.labelWidth;

				var labelWidth = 0f;
				labelWidth = Mathf.Max(labelWidth, EditorStyles.label.CalcSize(new GUIContent("Name")).x);
				labelWidth = Mathf.Max(labelWidth, EditorStyles.label.CalcSize(new GUIContent("Type")).x);
				if (descriptor.canBeGrouped) labelWidth = Mathf.Max(labelWidth, EditorStyles.label.CalcSize(new GUIContent("Group")).x);

				EditorGUIUtility.labelWidth = labelWidth;
				descriptor.name = EditorGUILayout.TextField("Name", descriptor.name);

				if (descriptor.canBeGrouped)
				{
					EditorGUILayout.BeginHorizontal();
					descriptor.path = EditorGUILayout.TextField("Path", descriptor.path);
					var assetPaths = descriptor.generator.collection.GetAllAssetPaths("<none>");
					var currentPathIndex = System.Array.IndexOf(assetPaths, descriptor.path);
					if (currentPathIndex == -1) currentPathIndex = 0;
					currentPathIndex = EditorGUILayout.Popup(currentPathIndex, assetPaths, GUILayout.Width(EditorGUIUtility.singleLineHeight));
					descriptor.path = currentPathIndex == 0 ? "" : assetPaths[currentPathIndex];
					EditorGUILayout.EndHorizontal();
				}

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Type");
				EditorGUILayout.SelectableLabel(descriptor.assetType.GetPrettyName(), EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
				EditorGUILayout.EndHorizontal();
				var typeRect = GUILayoutUtility.GetLastRect();
				GUI.Label(typeRect, new GUIContent("", descriptor.assetType.GetPrettyName(true)));

				EditorGUIUtility.labelWidth = priorLabelWidth;
			}
			EditorGUILayout.EndFadeGroup();

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			GUI.enabled = true;
		}
	}
}
