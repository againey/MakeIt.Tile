/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System;

namespace MakeIt.Generate
{
	public class InputSlotKey : ScriptableObject
	{
		[NonSerialized] private InputSlot _slot;
		[SerializeField] private Generator _generator;

		public static InputSlotKey Create(InputSlot slot, Generator generator)
		{
			var instance = CreateInstance<InputSlotKey>();
			instance._slot = slot;
			instance._generator = generator;
			instance.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;
			return instance;
		}

		public InputSlot slot
		{
			get
			{
				if (_slot == null || !ReferenceEquals(_slot.key, this))
				{
					if (_generator != null)
					{
						foreach (var input in _generator.activeInputs)
						{
							if (ReferenceEquals(input.key, this))
							{
								_slot = input;
								return _slot;
							}
						}
					}

					_slot = null;
					return null;
				}
				else
				{
					return _slot;
				}
			}
		}
	}
}
