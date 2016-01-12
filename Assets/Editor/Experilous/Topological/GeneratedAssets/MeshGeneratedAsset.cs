using UnityEngine;

namespace Experilous.Topological
{
	public class MeshGeneratedAsset : GenericGeneratedAsset<Mesh>
	{
		public static MeshGeneratedAsset CreateDefaultInstance(AssetGenerator assetGenerator, string name)
		{
			return CreateDefaultInstance<MeshGeneratedAsset>(assetGenerator, name);
		}

		public static MeshGeneratedAsset CreateOptionalInstance(AssetGenerator assetGenerator, string name, bool enabled = true)
		{
			return CreateOptionalInstance<MeshGeneratedAsset>(assetGenerator, name, enabled);
		}
	}
}
