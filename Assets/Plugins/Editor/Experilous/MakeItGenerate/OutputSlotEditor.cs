/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using Experilous.Core;

namespace Experilous.MakeIt.Generate
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

		public static bool OnInspectorGUI(OutputSlot output, bool foldout, AnimBool foldoutAnimation)
		{
			InitializeStyles();

			float foldoutMinWidth, foldoutMaxWidth;
			EditorStyles.foldout.CalcMinMaxWidth(GUIContent.none, out foldoutMinWidth, out foldoutMaxWidth);

			EditorGUILayout.BeginVertical(_outlineStyle);

			EditorGUILayout.BeginHorizontal();
			var foldoutRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, GUILayout.Width(0));
			foldout = EditorGUI.Foldout(foldoutRect, foldout, GUIContent.none, false);
			foldoutAnimation.target = foldout;
			EditorGUILayout.BeginVertical();

			if (foldout)
			{
				EditorGUILayout.BeginHorizontal();
				output.isEnabled = EditorGUILayout.ToggleLeft("Enabled", output.isEnabled, GUILayout.Width(EditorStyles.toggle.CalcSize(new GUIContent("Enabled")).x));
				GUIExtensions.PushEnable(output.isEnabled);
				if (output.mustBeAvailableAfterGeneration)
				{
					output.availability = (OutputSlot.Availability)EditorGUILayout.IntPopup((int)output.availability, _alwaysAfterAvailabilityContent, _alwaysAfterAvailabilityValues);
				}
				else if (output.canBeAvailableAfterGeneration)
				{
					output.availability = (OutputSlot.Availability)EditorGUILayout.IntPopup((int)output.availability, _fullAvailabilityContent, _fullAvailabilityValues);
				}
				else
				{
					output.availability = (OutputSlot.Availability)EditorGUILayout.IntPopup((int)output.availability, _neverAfterAvailabilityContent, _neverAfterAvailabilityValues);
				}
				EditorGUILayout.EndHorizontal();
			}
			else
			{
				EditorGUILayout.BeginHorizontal();
				output.isEnabled = EditorGUILayout.Toggle(output.isEnabled, GUILayout.Width(EditorStyles.toggle.CalcSize(GUIContent.none).x));
				GUIExtensions.PushEnable(output.isEnabled);
				EditorGUILayout.LabelField(new GUIContent(string.Format("{0} ({1})", output.name, output.assetType.GetPrettyName()), output.assetType.GetPrettyName(true)));
				EditorGUILayout.EndHorizontal();
			}

			if (EditorGUILayout.BeginFadeGroup(foldoutAnimation.faded))
			{
				var priorLabelWidth = EditorGUIUtility.labelWidth;

				var labelWidth = 0f;
				labelWidth = Mathf.Max(labelWidth, EditorStyles.label.CalcSize(new GUIContent("Name")).x);
				labelWidth = Mathf.Max(labelWidth, EditorStyles.label.CalcSize(new GUIContent("Type")).x);
				if (output.canBeGrouped) labelWidth = Mathf.Max(labelWidth, EditorStyles.label.CalcSize(new GUIContent("Group")).x);

				EditorGUIUtility.labelWidth = labelWidth;
				output.name = EditorGUILayout.TextField("Name", output.name);

				if (output.canBeGrouped)
				{
					EditorGUILayout.BeginHorizontal();
					output.path = EditorGUILayout.TextField("Path", output.path);
					var assetPaths = output.generator.executive.GetAllAssetPaths("<none>");
					var currentPathIndex = System.Array.IndexOf(assetPaths, output.path);
					if (currentPathIndex == -1) currentPathIndex = 0;
					currentPathIndex = EditorGUILayout.Popup(currentPathIndex, assetPaths, GUILayout.Width(EditorGUIUtility.singleLineHeight));
					output.path = currentPathIndex == 0 ? "" : assetPaths[currentPathIndex];
					EditorGUILayout.EndHorizontal();
				}

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Type");
				EditorGUILayout.SelectableLabel(output.assetType.GetPrettyName(), EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
				EditorGUILayout.EndHorizontal();
				var typeRect = GUILayoutUtility.GetLastRect();
				GUI.Label(typeRect, new GUIContent("", output.assetType.GetPrettyName(true)));

				EditorGUIUtility.labelWidth = priorLabelWidth;
			}
			EditorGUILayout.EndFadeGroup();

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			GUIExtensions.PopEnable();

			return foldout;
		}
	}
}
