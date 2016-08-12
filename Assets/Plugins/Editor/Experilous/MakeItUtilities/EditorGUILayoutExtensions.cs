/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using UnityEditor;

namespace Experilous
{
	[InitializeOnLoad]
	public static class EditorGUILayoutExtensions
	{
		public static TObject ObjectField<TObject>(TObject obj, bool allowSceneObjects, params GUILayoutOption[] options) where TObject : Object
		{
			return (TObject)ObjectField(obj, typeof(TObject), allowSceneObjects, options);
		}

		public static TObject ObjectField<TObject>(string label, TObject obj, bool allowSceneObjects, params GUILayoutOption[] options) where TObject : Object
		{
			return (TObject)ObjectField(label, obj, typeof(TObject), allowSceneObjects, options);
		}

		public static TObject ObjectField<TObject>(GUIContent label, TObject obj, bool allowSceneObjects, params GUILayoutOption[] options) where TObject : Object
		{
			return (TObject)ObjectField(label, obj, typeof(TObject), allowSceneObjects, options);
		}

		public static Object ObjectField(Object obj, System.Type objType, bool allowSceneObjects, params GUILayoutOption[] options)
		{
			return ObjectField((GUIContent)null, obj, objType, allowSceneObjects, options);
		}

		public static Object ObjectField(string label, Object obj, System.Type objType, bool allowSceneObjects, params GUILayoutOption[] options)
		{
			return ObjectField(string.IsNullOrEmpty(label) ? (GUIContent)null : new GUIContent(label), obj, objType, allowSceneObjects, options);
		}

		public static Object ObjectField(GUIContent label, Object obj, System.Type objType, bool allowSceneObjects, params GUILayoutOption[] options)
		{
			bool hasLabel = label != null && !string.IsNullOrEmpty(label.text);

			var controlRect = EditorGUILayout.GetControlRect(hasLabel, EditorGUIUtility.singleLineHeight, EditorStyles.objectField);
			var labelRect = new Rect(controlRect.xMin, controlRect.yMin, hasLabel ? EditorGUIUtility.labelWidth : 0f, controlRect.height);
			var fieldRect = new Rect(controlRect.xMin + labelRect.width, controlRect.yMin, controlRect.width - labelRect.width, controlRect.height);
			var boxRect = new Rect(fieldRect.xMin, fieldRect.yMin, fieldRect.width - EditorStyles.objectField.padding.right, fieldRect.height);
			var pickerRect = new Rect(fieldRect.xMax - EditorStyles.objectField.padding.right, fieldRect.yMin, EditorStyles.objectField.padding.right, fieldRect.height);

			GUIContent content = EditorGUIUtility.ObjectContent(obj, objType);
			if (obj == null)
			{
				content.text = string.Format("None ({0})", objType.GetPrettyName());
			}
			else
			{
				content.text = string.Format("{0} ({1})", obj.name, obj.GetType().GetPrettyName());
			}

			var controlID = GUIUtility.GetControlID(FocusType.Keyboard, fieldRect);

			if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
			{
				if (fieldRect.Contains(Event.current.mousePosition))
				{
					if (DragAndDrop.objectReferences.Length == 1)
					{
						var dropObject = GetMatchingObject(DragAndDrop.objectReferences[0], objType);
						if (dropObject != null)
						{
							DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

							if (Event.current.type == EventType.DragPerform)
							{
								DragAndDrop.AcceptDrag();
								obj = dropObject;
								GUIUtility.keyboardControl = controlID;
							}
						}
						else
						{
							DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
						}
					}
					else
					{
						DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
					}

					Event.current.Use();
				}
			}

			if (Event.current.type == EventType.Repaint)
			{
				if (hasLabel)
				{
					EditorStyles.label.Draw(labelRect, label, false, false, false, GUIUtility.keyboardControl == controlID);
				}

				if (DragAndDrop.visualMode == DragAndDropVisualMode.Copy && fieldRect.Contains(Event.current.mousePosition))
				{
					EditorStyles.objectField.Draw(fieldRect, content, controlID, true);
				}
				else
				{
					EditorStyles.objectField.Draw(fieldRect, content, controlID, false);
				}
			}

			if (Event.current.type == EventType.MouseDown)
			{
				if (Event.current.clickCount == 1)
				{
					if (pickerRect.Contains(Event.current.mousePosition))
					{
						ShowObjectPicker(obj, objType, allowSceneObjects, "", controlID);
						Event.current.Use();
					}
					else if (boxRect.Contains(Event.current.mousePosition))
					{
						if (obj != null)
						{
							PingObject(obj);
						}
						FocusControl(controlID);
						Event.current.Use();
					}
					else if (controlRect.Contains(Event.current.mousePosition))
					{
						FocusControl(controlID);
						Event.current.Use();
					}
				}
				else if (Event.current.clickCount == 2)
				{
					if (boxRect.Contains(Event.current.mousePosition))
					{
						if (obj != null)
						{
							Selection.activeObject = obj;
							PingObject(obj);
						}
						Event.current.Use();
					}
				}
			}

			if (Event.current.type == EventType.ExecuteCommand && EditorGUIUtility.GetObjectPickerControlID() == controlID)
			{
				if (Event.current.commandName == "ObjectSelectorUpdated")
				{
					var selectedObject = EditorGUIUtility.GetObjectPickerObject();
					if (selectedObject == null)
					{
						obj = null;
					}
					else
					{
						var matchingObject = GetMatchingObject(selectedObject, objType);
						if (matchingObject != null)
						{
							obj = matchingObject;
						}
					}
					Event.current.Use();
				}
				else if (Event.current.commandName == "ObjectSelectorClosed")
				{
					var selectedObject = EditorGUIUtility.GetObjectPickerObject();
					if (selectedObject == null)
					{
						obj = null;
					}
					else
					{
						var matchingObject = GetMatchingObject(selectedObject, objType);
						if (matchingObject != null)
						{
							obj = matchingObject;
						}
					}
					Event.current.Use();
				}
			}

			return obj;
		}

		private static bool IsValidDropObject(Object obj, System.Type objType)
		{
			return GetMatchingObject(obj, objType) != null;
		}

		private static TObject GetMatchingObject<TObject>(TObject obj) where TObject : Object
		{
			return (TObject)GetMatchingObject(obj, typeof(TObject));
		}

		private static Object GetMatchingObject(Object obj, System.Type objType)
		{
			// We are filtering by game objects.
			if (objType == typeof(GameObject))
			{
				// If we've been given a game object, return the game object itself.
				if (obj is GameObject)
				{
					return obj;
				}
				// If we've been given a component that is attached to a game object, return the game object.
				else if (obj is Component)
				{
					return ((Component)obj).gameObject;
				}
			}
			// We are filtering by some type of Component.
			else if (typeof(Component).IsAssignableFrom(objType))
			{
				// We've been given a game object.
				if (obj is GameObject)
				{
					// If the game object has a component of the proper type, return the component.
					var component = ((GameObject)obj).GetComponent(objType);
					if (component != null)
					{
						return component;
					}
				}
				// We've been given a component attached to a game object.
				else if (obj is Component)
				{
					// If the component is of the proper type, return the component itself.
					if (objType.IsInstanceOfType(obj))
					{
						return obj;
					}
					// If the component's game object has another component of the proper type, then return the sibling component.
					var component = ((Component)obj).GetComponent(objType);
					if (component != null)
					{
						return component;
					}
				}
			}

			// Nothing matched, return null.
			return null;
		}

		private static void ShowObjectPicker(Object obj, System.Type objType, bool allowSceneObjects, string searchFilter, int controlID)
		{
			System.Type pickerType;

			if (objType == typeof(GameObject))
			{
				pickerType = objType;
			}
			else if (typeof(Component).IsAssignableFrom(objType))
			{
				pickerType = typeof(GameObject);
			}
			else if (typeof(ScriptableObject).IsAssignableFrom(objType))
			{
				pickerType = objType;
			}
			else
			{
				throw new System.ArgumentException(string.Format("The type {0} is not valid for use by the object picker.", objType.GetPrettyName(true)), "objType");
			}

			var method = typeof(EditorGUIUtility).GetMethod("ShowObjectPicker", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).MakeGenericMethod(pickerType);
			method.Invoke(null, new object[] { obj, allowSceneObjects, searchFilter, controlID });
		}

		private static void PingObject(Object obj)
		{
			if (obj is GameObject)
			{
				EditorGUIUtility.PingObject((GameObject)obj);
			}
			else if (obj is Component)
			{
				EditorGUIUtility.PingObject(((Component)obj).gameObject);
			}
			else if (obj is ScriptableObject)
			{
				EditorGUIUtility.PingObject((ScriptableObject)obj);
			}
			else
			{
				EditorGUIUtility.PingObject(obj);
			}
		}

		private static void FocusControl(int controlID)
		{
			GUIUtility.keyboardControl = controlID;
			EditorWindow.focusedWindow.Repaint();
		}
	}
}
