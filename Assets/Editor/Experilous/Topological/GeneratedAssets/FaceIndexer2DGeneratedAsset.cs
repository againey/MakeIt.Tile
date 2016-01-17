using UnityEngine;

namespace Experilous.Topological
{
	public class FaceIndexer2DGeneratedAsset : GenericGeneratedAsset<FaceIndexer2D>
	{
		public static FaceIndexer2DGeneratedAsset CreateDefaultInstance(AssetGenerator assetGenerator, string name)
		{
			return CreateDefaultInstance<FaceIndexer2DGeneratedAsset>(assetGenerator, name);
		}

		public static FaceIndexer2DGeneratedAsset CreateOptionalInstance(AssetGenerator assetGenerator, string name, bool enabled = true)
		{
			return CreateOptionalInstance<FaceIndexer2DGeneratedAsset>(assetGenerator, name, enabled);
		}
	}
}
