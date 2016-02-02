﻿using UnityEngine;
using System.Collections.Generic;
using System;

namespace Experilous
{
	public sealed class AssetInputSlot : ScriptableObject
	{
		[SerializeField] private AssetGenerator _generator;
		[SerializeField] private SerializableType _assetType;
		[SerializeField] private AssetDescriptor _source;

		[SerializeField] private bool _isOptional = false;
		[SerializeField] private bool _isMutating = false;

		[SerializeField] private List<AssetDescriptor> _referencers;

		private static AssetInputSlot Create(AssetGenerator generator, Type assetType, bool isOptional, bool isMutating)
		{
			if (generator == null)
				throw new ArgumentNullException("generator");
			if (assetType == null)
				throw new ArgumentNullException("assetType");

			var inputSlot = CreateInstance<AssetInputSlot>();
			inputSlot._generator = generator;
			inputSlot._assetType = assetType;
			inputSlot._isOptional = isOptional;
			inputSlot._isMutating = isMutating;
			inputSlot._referencers = new List<AssetDescriptor>();
			inputSlot.hideFlags = HideFlags.HideInHierarchy;
			return inputSlot;
		}

		private static AssetInputSlot Create(AssetGenerator generator, Type assetType, bool isOptional, bool isMutating, string name)
		{
			var inputSlot = Create(generator, assetType, isOptional, isMutating);
			inputSlot.name = name;
			return inputSlot;
		}

		public static AssetInputSlot CreateRequired(AssetGenerator generator, Type assetType)
		{
			return Create(generator, assetType, false, false);
		}

		public static AssetInputSlot CreateRequired(AssetGenerator generator, Type assetType, string name)
		{
			return Create(generator, assetType, false, false, name);
		}

		public static AssetInputSlot CreateOptional(AssetGenerator generator, Type assetType)
		{
			return Create(generator, assetType, true, false);
		}

		public static AssetInputSlot CreateOptional(AssetGenerator generator, Type assetType, string name)
		{
			return Create(generator, assetType, true, false, name);
		}

		public static AssetInputSlot CreateRequiredMutating(AssetGenerator generator, Type assetType)
		{
			return Create(generator, assetType, false, true);
		}

		public static AssetInputSlot CreateRequiredMutating(AssetGenerator generator, Type assetType, string name)
		{
			return Create(generator, assetType, false, true, name);
		}

		public static AssetInputSlot CreateOptionalMutating(AssetGenerator generator, Type assetType)
		{
			return Create(generator, assetType, true, true);
		}

		public static AssetInputSlot CreateOptionalMutating(AssetGenerator generator, Type assetType, string name)
		{
			return Create(generator, assetType, true, true, name);
		}

		public AssetGenerator generator { get { return _generator; } }
		public System.Type assetType { get { return _assetType; } }

		public AssetDescriptor source
		{
			get
			{
				return _source;
			}
			set
			{
				if (ReferenceEquals(_source, value)) return;

				if (_source != null) _source.RemoveConsumer(this);
				_source = value;
				if (_source != null) _source.AddConsumer(this);
			}
		}

		public bool isOptional { get { return _isOptional; } }
		public bool isRequired { get { return !_isOptional; } }
		public bool isMutating { get { return _isMutating; } }
		public bool isReadOnly { get { return !_isMutating; } }

		public IEnumerable<AssetDescriptor> referencers { get { foreach (var referencer in _referencers) yield return referencer; } }

		public bool isActive
		{
			get
			{
				foreach (var input in _generator.inputs)
				{
					if (ReferenceEquals(input, this))
					{
						return true;
					}
				}
				return false;
			}
		}

		public TAsset GetAsset<TAsset>() where TAsset : class
		{
			return _source != null ? _source.GetAsset<TAsset>() : null;
		}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class AutoSelectAttribute : Attribute { }
}
