﻿/******************************************************************************\
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
using MakeIt.Core;

namespace MakeIt.Generate
{
	[Serializable] public sealed class InputSlot
	{
		[SerializeField] private Generator _generator;
		[SerializeField] private SerializableType _assetType;
		[SerializeField] private OutputSlotKey _sourceKey;

		[SerializeField] private bool _isOptional = false;
		[SerializeField] private bool _isMutating = false;

		[SerializeField] private InputSlotKey _key;

		[SerializeField] public bool isActive;

		public static InputSlot CreateRequired<TAsset>(Generator generator)
		{
			return new InputSlot(generator, typeof(TAsset), false, false);
		}

		public static InputSlot CreateOrResetRequired<TAsset>(ref InputSlot input, Generator generator)
		{
			if (input == null) input = new InputSlot();
			input.Initialize(generator, typeof(TAsset), false, false);
			return input;
		}

		public static InputSlot CreateOptional<TAsset>(Generator generator)
		{
			return new InputSlot(generator, typeof(TAsset), true, false);
		}

		public static InputSlot CreateOrResetOptional<TAsset>(ref InputSlot input, Generator generator)
		{
			if (input == null) input = new InputSlot();
			input.Initialize(generator, typeof(TAsset), true, false);
			return input;
		}

		public static InputSlot CreateRequiredMutating<TAsset>(Generator generator)
		{
			return new InputSlot(generator, typeof(TAsset), false, true);
		}

		public static InputSlot CreateOrResetRequiredMutating<TAsset>(ref InputSlot input, Generator generator)
		{
			if (input == null) input = new InputSlot();
			input.Initialize(generator, typeof(TAsset), false, true);
			return input;
		}

		public static InputSlot CreateOptionalMutating<TAsset>(Generator generator)
		{
			return new InputSlot(generator, typeof(TAsset), true, true);
		}

		public static InputSlot CreateOrResetOptionalMutating<TAsset>(ref InputSlot input, Generator generator)
		{
			if (input == null) input = new InputSlot();
			input.Initialize(generator, typeof(TAsset), true, true);
			return input;
		}

		private InputSlot()
		{
		}

		private InputSlot(Generator generator, Type assetType, bool isOptional, bool isMutating)
		{
			Initialize(generator, assetType, isOptional, isMutating);
		}

		private void Initialize(Generator generator, Type assetType, bool isOptional, bool isMutating)
		{
			if (generator == null)
				throw new ArgumentNullException("generator");
			if (assetType == null)
				throw new ArgumentNullException("assetType");

			_generator = generator;
			_assetType = assetType;
			_isOptional = isOptional;
			_isMutating = isMutating;

			isActive = true;

			Disconnect();
		}

		public Generator generator { get { return _generator; } }
		public Type assetType { get { return _assetType; } }

		public OutputSlot source
		{
			get
			{
				return _sourceKey != null ? _sourceKey.slot : null;
			}
			set
			{
				if (_sourceKey == null && value == null) return;
				if (_sourceKey != null && ReferenceEquals(_sourceKey.slot, value)) return;

				var priorSource = _sourceKey;
				_sourceKey = null;
				if (priorSource != null)
				{
					var slot = priorSource.slot;
					if (slot != null) slot.Disconnect(this);
				}
				_sourceKey = (value != null ? value.key : null);
				if (_sourceKey != null)
				{
					var slot = _sourceKey.slot;
					if (slot != null) slot.Connect(this);
				}
			}
		}

		public bool isOptional { get { return _isOptional; } }
		public bool isRequired { get { return !_isOptional; } }
		public bool isMutating { get { return _isMutating; } }
		public bool isReadOnly { get { return !_isMutating; } }

		public InputSlotKey key
		{
			get
			{
				if (_key == null) _key = InputSlotKey.Create(this, _generator);
				return _key;
			}
		}

		public TAsset GetAsset<TAsset>() where TAsset : class
		{
			return _sourceKey != null ? _sourceKey.slot.GetAsset<TAsset>() : null;
		}

		public void Disconnect()
		{
			if (_sourceKey != null)
			{
				var slot = _sourceKey.slot;
				if (slot != null) slot.Disconnect(this);
				_sourceKey = null;
			}
		}

		public static void ResetAssetTypeIfNull<TAsset>(InputSlot inputSlot)
		{
			if (inputSlot != null && inputSlot._assetType.type == null)
			{
				inputSlot._assetType = typeof(TAsset);
			}
		}

		public static bool ShouldAutoSelect<TObject>(string fieldName)
		{
			return ShouldAutoSelect(typeof(TObject).GetField(fieldName));
		}

		public static bool ShouldAutoSelect(object obj, string fieldName)
		{
			return ShouldAutoSelect(obj.GetType().GetField(fieldName));
		}

		public static bool ShouldAutoSelect(System.Reflection.FieldInfo field)
		{
			return GeneralUtility.GetAttribute<AutoSelectAttribute>(field) != null;
		}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class AutoSelectAttribute : Attribute { }
}
