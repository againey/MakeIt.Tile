using UnityEngine;

namespace Experilous.Topological
{
	public class TopologyGeneratedAsset : GenericGeneratedAsset<Topology>
	{
		public static TopologyGeneratedAsset CreateDefaultInstance(AssetGenerator assetGenerator, string name)
		{
			return CreateDefaultInstance<TopologyGeneratedAsset>(assetGenerator, name);
		}

		public static TopologyGeneratedAsset CreateOptionalInstance(AssetGenerator assetGenerator, string name, bool enabled = true)
		{
			return CreateOptionalInstance<TopologyGeneratedAsset>(assetGenerator, name, enabled);
		}
	}
}
