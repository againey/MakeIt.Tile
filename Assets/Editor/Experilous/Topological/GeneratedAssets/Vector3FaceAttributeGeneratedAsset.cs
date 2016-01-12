using UnityEngine;

namespace Experilous.Topological
{
	public class Vector3FaceAttributeGeneratedAsset : GenericGeneratedAsset<Vector3FaceAttribute>
	{
		public static Vector3FaceAttributeGeneratedAsset CreateDefaultInstance(AssetGenerator assetGenerator, string name)
		{
			return CreateDefaultInstance<Vector3FaceAttributeGeneratedAsset>(assetGenerator, name);
		}

		public static Vector3FaceAttributeGeneratedAsset CreateOptionalInstance(AssetGenerator assetGenerator, string name, bool enabled = true)
		{
			return CreateOptionalInstance<Vector3FaceAttributeGeneratedAsset>(assetGenerator, name, enabled);
		}
	}
}
