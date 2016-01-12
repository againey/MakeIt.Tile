using UnityEngine;

namespace Experilous.Topological
{
	public class PlanarWrapAroundPartitioningGeneratedAsset : GenericGeneratedAsset<PlanarWrapAroundPartitioning>
	{
		public static PlanarWrapAroundPartitioningGeneratedAsset CreateDefaultInstance(AssetGenerator assetGenerator, string name)
		{
			return CreateDefaultInstance<PlanarWrapAroundPartitioningGeneratedAsset>(assetGenerator, name);
		}

		public static PlanarWrapAroundPartitioningGeneratedAsset CreateOptionalInstance(AssetGenerator assetGenerator, string name, bool enabled = true)
		{
			return CreateOptionalInstance<PlanarWrapAroundPartitioningGeneratedAsset>(assetGenerator, name, enabled);
		}
	}
}
