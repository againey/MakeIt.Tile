using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace Experilous.Generation
{
	public static class OutputSlotEditor
	{
		private static GUIStyle _outlineStyle = null;

		private static GUIContent[] _fullAvailabilityContent = new GUIContent[]
		{
			new GUIContent("Available Always"),
			new GUIContent("Available During Generation"),
			new GUIContent("Available After Generation"),
		};

		private static int[] _fullAvailabilityValues = new int[]
		{
			(int)OutputSlot.Availability.Always,
			(int)OutputSlot.Availability.DuringGeneration,
			(int)OutputSlot.Availability.AfterGeneration,
		};

		private static GUIContent[] _alwaysAfterAvailabilityContent = new GUIContent[]
		{
			new GUIContent("Available Always"),
			new GUIContent("Available After Generation"),
		};

		private static int[] _alwaysAfterAvailabilityValues = new int[]
		{
			(int)OutputSlot.Availability.Always,
			(int)OutputSlot.Availability.AfterGeneration,
		};

		private static GUIContent[] _neverAfterAvailabilityContent = new GUIContent[]
		{
			new GUIContent("Available During Generation"),
		};

		private static int[] _neverAfterAvailabilityValues = new int[]
		{
			(int)OutputSlot.Availability.DuringGeneration,
		};

		private static void InitializeStyles()
		{
			if (_outlineStyle == null)
			{
				_outlineStyle = new GUIStyle(EditorStyles.helpBox);
				_outlineStyle.padding.left += EditorStyles.foldout.padding.left;
			}
		}

		public static void OnInspectorGUI(OutputSlot output)
		{
			InitializeStyles();

			var descriptor = (OutputSlot)output;

			float foldoutMinWidth, foldoutMaxWidth;
			EditorStyles.foldout.CalcMinMaxWidth(GUIContent.none, out foldoutMinWidth, out foldoutMaxWidth);

			EditorGUILayout.BeginVertical(_outlineStyle);

			EditorGUILayout.BeginHorizontal();
			var foldoutRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, GUILayout.Width(0));
			output.uiFoldout = EditorGUI.Foldout(foldoutRect, output.uiFoldout, GUIContent.none, false);
			output.uiFoldoutAnimation.target = output.uiFoldout;
			EditorGUILayout.BeginVertical();

			if (output.uiFoldout)
			{
				EditorGUILayout.BeginHorizontal();
				descriptor.isEnabled = EditorGUILayout.ToggleLeft("Enabled", descriptor.isEnabled, GUILayout.Width(EditorStyles.toggle.CalcSize(new GUIContent("Enabled")).x));
				GUIExtensions.PushEnable(descriptor.isEnabled);
				if (descriptor.mustBeAvailableAfterGeneration)
				{
					descriptor.availability = (OutputSlot.Availability)EditorGUILayout.IntPopup((int)descriptor.availability, _alwaysAfterAvailabilityContent, _alwaysAfterAvailabilityValues);
				}
				else if (descriptor.canBeAvailableAfterGeneration)
				{
					descriptor.availability = (OutputSlot.Availability)EditorGUILayout.IntPopup((int)descriptor.availability, _fullAvailabilityContent, _fullAvailabilityValues);
				}
				else
				{
					descriptor.availability = (OutputSlot.Availability)EditorGUILayout.IntPopup((int)descriptor.availability, _neverAfterAvailabilityContent, _neverAfterAvailabilityValues);
				}
				EditorGUILayout.EndHorizontal();
			}
			else
			{
				EditorGUILayout.BeginHorizontal();
				descriptor.isEnabled = EditorGUILayout.Toggle(descriptor.isEnabled, GUILayout.Width(EditorStyles.toggle.CalcSize(GUIContent.none).x));
				GUIExtensions.PushEnable(descriptor.isEnabled);
				EditorGUILayout.LabelField(new GUIContent(string.Format("{0} ({1})", descriptor.name, descriptor.assetType.GetPrettyName()), descriptor.assetType.GetPrettyName(true)));
				EditorGUILayout.EndHorizontal();
			}

			if (EditorGUILayout.BeginFadeGroup(output.uiFoldoutAnimation.faded))
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
					var assetPaths = descriptor.generator.executive.GetAllAssetPaths("<none>");
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

			GUIExtensions.PopEnable();
		}
	}
}
