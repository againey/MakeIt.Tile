using UnityEngine;
using UnityEditor;
using System.IO;

namespace Experilous.Topological
{
	public class GameObjectGeneratedAsset : GenericGeneratedAsset<GameObject>
	{
		public static GameObjectGeneratedAsset CreateDefaultInstance(AssetGenerator assetGenerator, string name)
		{
			return CreateDefaultInstance<GameObjectGeneratedAsset>(assetGenerator, name);
		}

		public override void SetGeneratedInstance(string location, string name, Object instance)
		{
			if (instance != null)
			{
				var assetPath = AssetUtility.GetCanonicalPath(Path.Combine(location, string.Format("{0} ({1}).prefab", name, this.name)));
				if (_generatedInstance != null && AssetDatabase.Contains(_generatedInstance))
				{
					if (_generatedInstance.GetType() == instance.GetType())
					{
						if (AssetUtility.GetProjectRelativeAssetPath(_generatedInstance) != assetPath)
						{
							AssetUtility.MoveOrRenameAsset(_generatedInstance, assetPath, false);
						}

						var prefab = PrefabUtility.ReplacePrefab(instance as GameObject, _generatedInstance, ReplacePrefabOptions.ReplaceNameBased);
						DestroyImmediate(instance);
						_generatedInstance = prefab;
					}
					else
					{
						AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(_generatedInstance));

						var prefab = PrefabUtility.CreatePrefab(assetPath, instance as GameObject);
						DestroyImmediate(instance);
						_generatedInstance = prefab;
					}
				}
				else
				{
					var prefab = PrefabUtility.CreatePrefab(assetPath, instance as GameObject);
					DestroyImmediate(instance);
					_generatedInstance = prefab;
				}
			}
			else if (_generatedInstance != null && AssetDatabase.Contains(_generatedInstance))
			{
				AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(_generatedInstance));
				_generatedInstance = null;
			}
		}
	}
}
