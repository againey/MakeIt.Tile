/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using Experilous.Core;

namespace Experilous.MakeItGenerate
{
	[System.Serializable] public sealed class OutputSlot
	{
		[System.Flags]
		public enum Availability
		{
			Always = 3,
			DuringGeneration = 1,
			AfterGeneration = 2,
		}

		[SerializeField] private Generator _generator;
		[SerializeField] private SerializableType _assetType;

		private object _asset;

		[SerializeField] private Object _persistedAsset;

		[SerializeField] private Availability _availability;
		[SerializeField] private bool _persistable;

		[SerializeField] private List<InputSlotKey> _connectionTargets = new List<InputSlotKey>();

		[SerializeField] private OutputSlotKey _key;

		[SerializeField] public string name;
		[SerializeField] public string path;
		[SerializeField] public bool isActive;
		[SerializeField] public bool isEnabled;

		public static OutputSlot Create<TAsset>(Generator generator, string name, bool isEnabled = true, Availability availability = Availability.Always)
		{
			return new OutputSlot(generator, typeof(TAsset), name, null, isEnabled, availability, true);
		}

		public static OutputSlot CreateOrReset<TAsset>(ref OutputSlot output, Generator generator, string name, bool isEnabled = true, Availability availability = Availability.Always)
		{
			if (output == null) output = new OutputSlot();
			output.Initialize(generator, typeof(TAsset), name, null, isEnabled, availability, true);
			return output;
		}

		public static OutputSlot CreateGrouped<TAsset>(Generator generator, string name, string path, bool isEnabled = true, Availability availability = Availability.Always)
		{
			return new OutputSlot(generator, typeof(TAsset), name, path, isEnabled, availability, true);
		}

		public static OutputSlot CreateOrResetGrouped<TAsset>(ref OutputSlot output, Generator generator, string name, string path, bool isEnabled = true, Availability availability = Availability.Always)
		{
			if (output == null) output = new OutputSlot();
			output.Initialize(generator, typeof(TAsset), name, path, isEnabled, availability, true);
			return output;
		}

		public static OutputSlot CreateUnpersisted<TAsset>(Generator generator, string name, bool isEnabled = true)
		{
			return new OutputSlot(generator, typeof(TAsset), name, null, isEnabled, Availability.DuringGeneration, false);
		}

		public static OutputSlot CreateOrResetUnpersisted<TAsset>(ref OutputSlot output, Generator generator, string name, bool isEnabled = true)
		{
			if (output == null) output = new OutputSlot();
			output.Initialize(generator, typeof(TAsset), name, null, isEnabled, Availability.DuringGeneration, false);
			return output;
		}

		private OutputSlot()
		{
		}

		private OutputSlot(Generator generator, System.Type assetType, string name, string path, bool isEnabled, Availability availability, bool persistable)
		{
			Initialize(generator, assetType, name, path, isEnabled, availability, persistable);
		}

		private void Initialize(Generator generator, System.Type assetType, string name, string path, bool isEnabled, Availability availability, bool persistable)
		{
			if (generator == null)
				throw new System.ArgumentNullException("generator");
			if (assetType == null)
				throw new System.ArgumentNullException("assetType");
			if (string.IsNullOrEmpty(name))
				throw new System.ArgumentException("Output slot must be given a non-empty name.", "name");
			if (assetType == typeof(GameObject) && availability == Availability.DuringGeneration)
				throw new System.ArgumentException("Assets of type GameObject must always be available after generation.", "availability");
			if (assetType == typeof(GameObject) && persistable == false)
				throw new System.ArgumentException("Assets of type GameObject must always be available after generation.", "persistable");
			if ((availability & Availability.AfterGeneration) != 0 && persistable == false)
				throw new System.ArgumentException("Inconsistent argument values.", "persistable");

			_generator = generator;
			_assetType = assetType;
			_persistable = persistable;
			_availability = availability;

			this.path = string.IsNullOrEmpty(path) ? null : path;
			this.name = name;
			this.isEnabled = isEnabled;

			isActive = true;

			DisconnectAll();
			ForgetAsset();
		}

		public Generator generator { get { return _generator; } }
		public System.Type assetType { get { return _assetType; } }
		public object asset { get { return _asset; } }
		public Object persistedAsset { get { return _persistedAsset; } }

		public bool canBeGrouped { get { return isAvailableAfterGeneration; } }

		public Availability availability
		{
			get
			{
				return _availability;
			}
			set
			{
				if (mustBeAvailableAfterGeneration && (value & Availability.AfterGeneration) == 0)
					throw new System.ArgumentException("This asset must always be available after generation.", "value");
				if (!canBeAvailableAfterGeneration && (value & Availability.AfterGeneration) != 0)
					throw new System.ArgumentException("This asset cannot be available after generation.", "value");

				_availability = value;
			}
		}

		public bool canBeAvailableAfterGeneration { get { return _persistable; } }
		public bool mustBeAvailableAfterGeneration { get { return _assetType == typeof(GameObject); } }
		public bool isAvailableDuringGeneration { get { return isEnabled && (availability & Availability.DuringGeneration) != 0; } }
		public bool isAvailableAfterGeneration { get { return isEnabled && (availability & Availability.AfterGeneration) != 0; } }

		public IEnumerable<InputSlot> connections { get { foreach (var target in _connectionTargets) yield return target.slot; } }

		public OutputSlotKey key
		{
			get
			{
				if (_key == null) _key = OutputSlotKey.Create(this, _generator);
				return _key;
			}
		}

		public void Connect(InputSlot target)
		{
			if (!_connectionTargets.Contains(target.key))
			{
				_connectionTargets.Add(target.key);
				target.source = this;
			}
		}

		public void Disconnect(InputSlot target)
		{
			_connectionTargets.Remove(target.key);
			target.source = null;
		}

		public void DisconnectAll()
		{
			var priorConnectionSources = _connectionTargets;
			_connectionTargets = new List<InputSlotKey>();
			foreach (var connection in priorConnectionSources)
			{
				connection.slot.source = null;
			}
		}

		public InternalSlotConnection UsedBy(OutputSlot target)
		{
			return new InternalSlotConnection(this, target);
		}

		public InternalSlotConnection Uses(OutputSlot source)
		{
			return new InternalSlotConnection(source, this);
		}

		public TAsset GetAsset<TAsset>() where TAsset : class
		{
			return _asset as TAsset;
		}

		public TAsset GetPersistedAsset<TAsset>() where TAsset : class
		{
			return _persistedAsset as TAsset;
		}

		public TAsset SetAsset<TAsset>(TAsset newAsset) where TAsset : class
		{
			return SetAsset(newAsset, _persistable);
		}

		public TAsset SetAsset<TAsset>(TAsset newAsset, bool persist) where TAsset : class
		{
			if (!_persistable && persist)
			{
				throw new System.ArgumentException("The output slot is not persistable, but the new asset was requested to be persisted.", "persist");
			}

			var assetObject = (object)newAsset;

			// Are we setting or clearing the generated asset?
			if (assetObject != null)
			{
				if (!assetType.IsInstanceOfType(assetObject))
				{
					throw new System.ArgumentException("The new asset instance was not of the type specified by this output slot's asset type.", "newAsset");
				}

				if (persist)
				{
					if (assetObject is Object)
					{
						_asset = _persistedAsset = _generator.executive.SetAsset(this, assetObject as Object);
					}
					else
					{
						throw new System.ArgumentException("The new asset instance cannot be persisted because it is not derived from UnityEngine.Object.", "newAsset");
					}
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

			return (TAsset)_asset;
		}

		public TAsset SetAsset<TAsset>(ref TAsset newAsset) where TAsset : class
		{
			return SetAsset(ref newAsset, _persistable);
		}

		public TAsset SetAsset<TAsset>(ref TAsset newAsset, bool persist) where TAsset : class
		{
			newAsset = SetAsset(newAsset, persist);
			return newAsset;
		}

		public void ClearAsset(bool persist = true)
		{
			if (!_persistable && persist)
			{
				throw new System.ArgumentException("The output slot is not persistable, but the cleared asset was requested to be persisted.", "persist");
			}

			if (_asset != null)
			{
				if (persist)
				{
					_generator.executive.ClearAsset(this);
					_persistedAsset = null;
				}
				_asset = null;
			}
		}

		public void ForgetAsset()
		{
			_asset = _persistedAsset = null;
		}

		public void Persist()
		{
			if (!_persistable)
			{
				throw new System.ArgumentException("The output slot is not persistable, but the asset was requested to be persisted.", "persist");
			}

			if (_asset is Object)
			{
				_asset = _persistedAsset = _generator.executive.SetAsset(this, _asset as Object);
			}
			else
			{
				throw new System.InvalidOperationException("The asset instance cannot be persisted because it is not derived from UnityEngine.Object.");
			}
		}

		public static void ResetAssetTypeIfNull<TAsset>(OutputSlot outputSlot)
		{
			if (outputSlot != null && outputSlot._assetType.type == null)
			{
				outputSlot._assetType = typeof(TAsset);
			}
		}
	}
}
