/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System.Collections.Generic;

namespace Experilous
{
	public static class GUIExtensions
	{
		private enum EnableDirectives
		{
			Disable,
			Enable,
			ForceEnable,
		}

		private static List<EnableDirectives> _enableStack = new List<EnableDirectives>();
		private static bool _enableState = true;

		public static bool PushEnable(bool enable, bool force = false)
		{
			if (enable == false)
			{
				_enableStack.Add(EnableDirectives.Disable);
				_enableState = false;
			}
			else if (force == false)
			{
				_enableStack.Add(EnableDirectives.Enable);
			}
			else
			{
				_enableStack.Add(EnableDirectives.ForceEnable);
				_enableState = true;
			}

			return GUI.enabled = _enableState;
		}

		public static bool PopEnable()
		{
			var index = _enableStack.Count - 1;
			if (index >= 0)
			{
				var top = _enableStack[index];
				_enableStack.RemoveAt(index);
				if (top != EnableDirectives.Enable)
				{
					_enableState = true;
					while (--index >= 0)
					{
						if (_enableStack[index] == EnableDirectives.Disable)
						{
							_enableState = false;
							break;
						}
						else if (_enableStack[index] == EnableDirectives.ForceEnable)
						{
							break;
						}
					}
				}
			}

			return GUI.enabled = _enableState;
		}

		public static void ResetEnable()
		{
			_enableStack.Clear();
			GUI.enabled = _enableState = true;
		}
	}
}
