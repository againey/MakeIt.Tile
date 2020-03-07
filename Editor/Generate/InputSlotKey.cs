/******************************************************************************\
* Copyright Andy Gainey                                                        *
*                                                                              *
* Licensed under the Apache License, Version 2.0 (the "License");              *
* you may not use this file except in compliance with the License.             *
* You may obtain a copy of the License at                                      *
*                                                                              *
*     http://www.apache.org/licenses/LICENSE-2.0                               *
*                                                                              *
* Unless required by applicable law or agreed to in writing, software          *
* distributed under the License is distributed on an "AS IS" BASIS,            *
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.     *
* See the License for the specific language governing permissions and          *
* limitations under the License.                                               *
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
