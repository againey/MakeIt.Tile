using UnityEngine;
using UnityEditor;
using System.IO;

namespace Experilous
{
	public abstract class GeneratedAsset : ScriptableObject
	{
		[SerializeField] protected bool _isOptional = false;
		public bool isOptional { get { return _isOptional; } }
		public bool isEnabled = true;

		public abstract AssetGenerator assetGenerator { get; }
		public abstract System.Type generatedType { get; }
		public abstract Object generatedInstance { get; }
		public abstract void SetGeneratedInstance(string location, string name, Object instance);
		public abstract void ClearGeneratedInstance();
	}

	public abstract class GenericGeneratedAssetBase<TAsset> : GeneratedAsset where TAsset : Object
	{
		[SerializeField] protected AssetGenerator _assetGenerator;
		[SerializeField] protected TAsset _generatedInstance;

		protected static TDerived CreateDefaultInstance<TDerived>(AssetGenerator assetGenerator, string name) where TDerived : GenericGeneratedAsset<TAsset>
		{
			var asset = CreateInstance<TDerived>();
			asset._assetGenerator = assetGenerator;
			asset._isOptional = false;
			asset.name = name;
			asset.hideFlags = HideFlags.HideInHierarchy;
			return asset;
		}

		protected static TDerived CreateOptionalInstance<TDerived>(AssetGenerator assetGenerator, string name, bool enabled = true) where TDerived : GenericGeneratedAsset<TAsset>
		{
			var asset = CreateInstance<TDerived>();
			asset._assetGenerator = assetGenerator;
			asset._isOptional = true;
			asset.isEnabled = enabled;
			asset.name = name;
			asset.hideFlags = HideFlags.HideInHierarchy;
			return asset;
		}

		public override AssetGenerator assetGenerator
		{
			get
			{
				return _assetGenerator;
			}
		}

		public override System.Type generatedType
		{
			get
			{
				return typeof(TAsset);
			}
		}

		public override Object generatedInstance
		{
			get
			{
				return _generatedInstance;
			}
		}
	}

	public abstract class GenericGeneratedAsset<TAsset> : GenericGeneratedAssetBase<TAsset> where TAsset : Object
	{
		public new TAsset generatedInstance
		{
			get
			{
				return _generatedInstance;
			}
		}

		public override void SetGeneratedInstance(string location, string name, Object instance)
		{
			if (instance != null)
			{
				var assetPath = AssetUtility.GetCanonicalPath(Path.Combine(location, string.Format("{0} ({1}).asset", name, this.name)));
				if (_generatedInstance != null && AssetDatabase.Contains(_generatedInstance))
				{
					if (_generatedInstance.GetType() == instance.GetType())
					{
						if (AssetUtility.GetProjectRelativeAssetPath(_generatedInstance) != assetPath)
						{
							AssetUtility.MoveOrRenameAsset(_generatedInstance, assetPath, false);
						}

						AssetUtility.UpdateAssetInPlace(instance, _generatedInstance);
						EditorUtility.SetDirty(_generatedInstance);
					}
					else
					{
						AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(_generatedInstance));
						AssetDatabase.CreateAsset(instance, assetPath);
						_generatedInstance = (TAsset)instance;
					}
				}
				else
				{
					AssetDatabase.CreateAsset(instance, assetPath);
					_generatedInstance = (TAsset)instance;
				}
			}
			else if (_generatedInstance != null && AssetDatabase.Contains(_generatedInstance))
			{
				AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(_generatedInstance));
				_generatedInstance = null;
			}
		}

		public override void ClearGeneratedInstance()
		{
			if (_generatedInstance != null && AssetDatabase.Contains(_generatedInstance))
			{
				AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(_generatedInstance));
				_generatedInstance = null;
			}
		}
	}
}
