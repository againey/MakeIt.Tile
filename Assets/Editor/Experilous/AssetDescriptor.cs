using UnityEngine;
using System.Collections.Generic;

namespace Experilous
{
	public sealed class AssetDescriptor : ScriptableObject
	{
		[System.Flags]
		public enum Availability
		{
			Always = 3,
			DuringGeneration = 1,
			AfterGeneration = 2,
		}

		[SerializeField] private AssetGenerator _generator;
		[SerializeField] private SerializableType _assetType;
		[SerializeField] private Object _asset;

		[SerializeField] private string _path;

		[SerializeField] private bool _isEnabled = true;
		[SerializeField] private Availability _availability = Availability.Always;
		[SerializeField] private bool _canBeAvailableAfterGeneration = true;

		[SerializeField] private List<AssetInputSlot> _consumers;

		public static AssetDescriptor Create<TAsset>(AssetGenerator generator, string name, bool isEnabled = true, Availability availability = Availability.Always)
		{
			return Create(generator, typeof(TAsset), name, null, isEnabled, availability, true);
		}

		public static AssetDescriptor CreateGrouped<TAsset>(AssetGenerator generator, string name, string path, bool isEnabled = true, Availability availability = Availability.Always)
		{
			return Create(generator, typeof(TAsset), name, path, isEnabled, availability, true);
		}

		public static AssetDescriptor CreateUnpersisted<TAsset>(AssetGenerator generator, string name, bool isEnabled = true)
		{
			return Create(generator, typeof(TAsset), name, null, isEnabled, Availability.DuringGeneration, false);
		}

		private static AssetDescriptor Create(AssetGenerator generator, System.Type assetType, string name, string path, bool isEnabled, Availability availability, bool canBeAvailableAfterGeneration)
		{
			if (generator == null)
				throw new System.ArgumentNullException("generator");
			if (assetType == null)
				throw new System.ArgumentNullException("assetType");
			if (string.IsNullOrEmpty(name))
				throw new System.ArgumentException("Asset descriptor must be given a non-empty name.", "name");
			if (assetType == typeof(GameObject) && availability == Availability.DuringGeneration)
				throw new System.ArgumentException("Assets of type GameObject must always be available after generation.", "availability");
			if (assetType == typeof(GameObject) && canBeAvailableAfterGeneration == false)
				throw new System.ArgumentException("Assets of type GameObject must always be available after generation.", "canBeAvailableAfterGeneration");
			if ((availability & Availability.AfterGeneration) != 0 && canBeAvailableAfterGeneration == false)
				throw new System.ArgumentException("Inconsistent argument values.", "canBeAvailableAfterGeneration");

			var descriptor = CreateInstance<AssetDescriptor>();
			descriptor._generator = generator;
			descriptor._assetType = assetType;
			descriptor._path = string.IsNullOrEmpty(path) ? null : path;
			descriptor._isEnabled = isEnabled;
			descriptor._availability = availability;
			descriptor._canBeAvailableAfterGeneration = canBeAvailableAfterGeneration;
			descriptor._consumers = new List<AssetInputSlot>();
			descriptor.name = name;
			descriptor.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;
			return descriptor;
		}

		public AssetGenerator generator { get { return _generator; } }
		public System.Type assetType { get { return _assetType; } }
		public Object asset { get { return _asset; } }

		public string path { get { return _path; } set { _path = value; } }
		public bool canBeGrouped { get { return isAvailableAfterGeneration; } }

		public bool isEnabled { get { return _isEnabled; } set { _isEnabled = value; } }
		public Availability availability { get { return _availability; } set { _availability = value; } }
		public bool canBeAvailableAfterGeneration { get { return _canBeAvailableAfterGeneration; } }
		public bool mustBeAvailableAfterGeneration { get { return _assetType == typeof(GameObject); } }
		public bool isAvailableDuringGeneration { get { return _isEnabled && (_availability & Availability.DuringGeneration) != 0; } }
		public bool isAvailableAfterGeneration { get { return _isEnabled && (_availability & Availability.AfterGeneration) != 0; } }

		public IEnumerable<AssetInputSlot> consumers { get { foreach (var consumer in _consumers) yield return consumer; } }

		public void AddConsumer(AssetInputSlot consumer)
		{
			if (!_consumers.Contains(consumer))
			{
				_consumers.Add(consumer);
			}
		}

		public void RemoveConsumer(AssetInputSlot consumer)
		{
			_consumers.Remove(consumer);
		}

		public void CleanConsumers()
		{
			_consumers.RemoveAll((AssetInputSlot input) => { return input == null; });
		}

		public AssetReferenceDescriptor ReferencedBy(AssetDescriptor referencer)
		{
			return new AssetReferenceDescriptor(this, referencer);
		}

		public AssetReferenceDescriptor References(AssetDescriptor referencer)
		{
			return new AssetReferenceDescriptor(referencer, this);
		}

		public TAsset GetAsset<TAsset>() where TAsset : class
		{
			return _asset as TAsset;
		}

		public TAsset SetAsset<TAsset>(TAsset newAsset, bool persist = true) where TAsset : class
		{
			var assetObject = (Object)(object)newAsset;

			// Are we setting or clearing the generated asset?
			if (assetObject != null)
			{
				if (!assetType.IsInstanceOfType(assetObject))
				{
					throw new System.ArgumentException("The new asset instance was not of the type specified by this asset descriptor's asset type.", "newAsset");
				}

				if (persist)
				{
					_asset = _generator.collection.SetAsset(this, assetObject as Object);
				}
				else
				{
					_asset = assetObject;
				}
			}
			// New instance is null; clear any stored asset.
			else
			{
				ClearAsset();
			}

			return (TAsset)(object)_asset;
		}

		public TAsset SetAsset<TAsset>(ref TAsset newAsset, bool persist = true) where TAsset : class
		{
			newAsset = SetAsset(newAsset, persist);
			return newAsset;
		}

		public void ClearAsset(bool persist = true)
		{
			if (_asset != null)
			{
				if (persist)
				{
					_generator.collection.ClearAsset(this);
				}
				_asset = null;
			}
		}

		public void Persist()
		{
			_asset = _generator.collection.SetAsset(this, _asset);
		}
	}
}
