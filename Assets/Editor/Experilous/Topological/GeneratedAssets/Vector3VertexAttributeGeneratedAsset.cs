using UnityEngine;

namespace Experilous.Topological
{
	public class Vector3VertexAttributeGeneratedAsset : GenericGeneratedAsset<Vector3VertexAttribute>
	{
		public static Vector3VertexAttributeGeneratedAsset CreateDefaultInstance(AssetGenerator assetGenerator, string name)
		{
			return CreateDefaultInstance<Vector3VertexAttributeGeneratedAsset>(assetGenerator, name);
		}

		public static Vector3VertexAttributeGeneratedAsset CreateOptionalInstance(AssetGenerator assetGenerator, string name, bool enabled = true)
		{
			return CreateOptionalInstance<Vector3VertexAttributeGeneratedAsset>(assetGenerator, name, enabled);
		}
	}
}
