using UnityEngine;
using UnityEditor;
using System.IO;

namespace Experilous
{
	public class AssetDescriptor : ScriptableObject
	{
		[SerializeField] protected AssetGenerator _generator;
		[SerializeField] protected SerializableType _assetType;
		[SerializeField] protected Object _asset;

		[SerializeField] protected string _groupName;

		[SerializeField] protected bool _isOptional = false;
		[SerializeField] protected bool _isPersisted = true;

		public static AssetDescriptor Create<TAsset>(AssetGenerator generator, string name, string groupName = null) where TAsset : Object
		{
			return Create(generator, typeof(TAsset), name, groupName, false, true);
		}

		public static AssetDescriptor Create(AssetGenerator generator, System.Type assetType, string name, string groupName = null)
		{
			return Create(generator, assetType, name, groupName, false, true);
		}

		public static AssetDescriptor CreateUnpersisted<TAsset>(AssetGenerator generator, string name, string groupName = null) where TAsset : Object
		{
			return Create(generator, typeof(TAsset), name, groupName, false, false);
		}

		public static AssetDescriptor CreateUnpersisted(AssetGenerator generator, System.Type assetType, string name, string groupName = null)
		{
			return Create(generator, assetType, name, groupName, false, false);
		}

		public static AssetDescriptor CreateOptional<TAsset>(AssetGenerator generator, string name, bool isPersisted = true, string groupName = null) where TAsset : Object
		{
			return Create(generator, typeof(TAsset), name, groupName, true, isPersisted);
		}

		public static AssetDescriptor CreateOptional(AssetGenerator generator, System.Type assetType, string name, bool isPersisted = true, string groupName = null)
		{
			return Create(generator, assetType, name, groupName, true, isPersisted);
		}

		protected static AssetDescriptor Create(AssetGenerator generator, System.Type assetType, string name, string groupName, bool isOptional, bool isPersisted)
		{
			if (generator == null)
				throw new System.ArgumentNullException("generator");
			if (assetType == null)
				throw new System.ArgumentNullException("assetType");
			//if (!assetType.IsSubclassOf(typeof(Object)))
			//	throw new System.ArgumentException("Asset descriptor must be for an asset derived from type UnityEngine.Object.", "assetType");
			if (assetType.IsSubclassOf(typeof(GameObject)) && !string.IsNullOrEmpty(groupName))
				throw new System.ArgumentException("Cannot store a GameObject asset within a group.", "groupName");
			if (assetType.IsSubclassOf(typeof(GameObject)) && isPersisted == false)
				throw new System.ArgumentException("Cannot generate an unpersisted GameObject asset.", "isPersisted");
			if (string.IsNullOrEmpty(name))
				throw new System.ArgumentException("Asset descriptor must be given a non-empty name.");

			var descriptor = CreateInstance<AssetDescriptor>();
			descriptor._generator = generator;
			descriptor._assetType = assetType;
			descriptor._groupName = string.IsNullOrEmpty(groupName) ? null : groupName;
			descriptor._isOptional = isOptional;
			descriptor._isPersisted = isPersisted;
			descriptor.name = name;
			descriptor.hideFlags = HideFlags.HideInHierarchy;
			return descriptor;
		}

		public AssetGenerator generator { get { return _generator; } }
		public System.Type assetType { get { return _assetType; } }
		public Object asset { get { return _asset; } }

		public string groupName { get { return _groupName; } set { _groupName = value; } }

		public bool isOptional { get { return _isOptional; } set { _isOptional = value; } }

		public bool isPersisted
		{
			get
			{
				return _isPersisted;
			}
			set
			{
				if (!_isOptional)
					throw new System.NotSupportedException("Cannot change the persistence of a non-optional asset.");

				_isPersisted = value;
			}
		}

		public bool isUnused { get { return _generator.bundle.IsUnused(this); } }

		public TAsset GetAsset<TAsset>() where TAsset : class
		{
			return (TAsset)(object)_asset;
		}

		public virtual Object SetAsset(Object newAsset)
		{
			// Are we setting or clearing the generated asset?
			if (newAsset != null)
			{
				if (!assetType.IsInstanceOfType(newAsset))
				{
					throw new System.ArgumentException("The new asset instance was not of the type specified by this asset descriptor's asset type.", "newAsset");
				}

				// Should the new asset instance be persisted?
				if (_isPersisted)
				{
					_asset = _generator.bundle.PersistAsset(_asset, newAsset, name, _groupName);
				}
				// We're not bothering to persist the asset.
				else
				{
					if (_asset != null)
					{
						_generator.bundle.DepersistAsset(_asset);
					}

					_asset = newAsset;
				}
			}
			// New instance is null; clear any stored asset.
			else
			{
				ClearAsset();
			}

			return _asset;
		}

		public virtual TAsset SetAsset<TAsset>(TAsset newAsset) where TAsset : class
		{
			return (TAsset)(object)SetAsset((Object)(object)newAsset);
		}

		public virtual void ClearAsset()
		{
			if (_asset != null)
			{
				_generator.bundle.DepersistAsset(_asset);
				_asset = null;
			}
		}
	}
}
