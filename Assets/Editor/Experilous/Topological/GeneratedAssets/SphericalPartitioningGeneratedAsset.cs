using UnityEngine;

namespace Experilous.Topological
{
	public class SphericalPartitioningGeneratedAsset : GenericGeneratedAsset<SphericalPartitioning>
	{
		public static SphericalPartitioningGeneratedAsset CreateDefaultInstance(AssetGenerator assetGenerator, string name)
		{
			return CreateDefaultInstance<SphericalPartitioningGeneratedAsset>(assetGenerator, name);
		}

		public static SphericalPartitioningGeneratedAsset CreateOptionalInstance(AssetGenerator assetGenerator, string name, bool enabled = true)
		{
			return CreateOptionalInstance<SphericalPartitioningGeneratedAsset>(assetGenerator, name, enabled);
		}
	}
}
