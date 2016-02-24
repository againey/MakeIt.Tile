/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using UnityEngine;
using System;

namespace Experilous.Generation
{
	public class OutputSlotKey : ScriptableObject
	{
		[NonSerialized] private OutputSlot _slot;
		[SerializeField] private Generator _generator;

		public static OutputSlotKey Create(OutputSlot slot, Generator generator)
		{
			var instance = CreateInstance<OutputSlotKey>();
			instance._slot = slot;
			instance._generator = generator;
			instance.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;
			return instance;
		}

		public OutputSlot slot
		{
			get
			{
				if (_slot == null || !ReferenceEquals(_slot.key, this))
				{
					if (_generator != null)
					{
						foreach (var output in _generator.activeOutputs)
						{
							if (ReferenceEquals(output.key, this))
							{
								_slot = output;
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
