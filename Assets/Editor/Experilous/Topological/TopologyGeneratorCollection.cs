using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;

namespace Experilous.Topological
{
	[AssetGeneratorCategory(typeof(TopologyGeneratorCollection), "Topology")] public struct TopologyCategory { }
	[AssetGeneratorCategory(typeof(TopologyGeneratorCollection), "Vertex Attributes", after = typeof(TopologyCategory))] public struct VertexAttributesCategory { }
	[AssetGeneratorCategory(typeof(TopologyGeneratorCollection), "Edge Attributes", after = typeof(VertexAttributesCategory))] public struct EdgeAttributesCategory { }
	[AssetGeneratorCategory(typeof(TopologyGeneratorCollection), "Face Attributes", after = typeof(EdgeAttributesCategory))] public struct FaceAttributesCategory { }
	[AssetGeneratorCategory(typeof(TopologyGeneratorCollection), "Utilities", after = typeof(FaceAttributesCategory))] public struct UtilitiesCategory { }
	[AssetGeneratorCategory(typeof(TopologyGeneratorCollection), "Mesh", after = typeof(UtilitiesCategory))] public struct MeshCategory { }

	public class TopologyGeneratorCollection : AssetGeneratorCollection
	{
		public static TopologyGeneratorCollection Create(string name)
		{
			var collection = CreateInstance<TopologyGeneratorCollection>();
			collection.name = name;
			return collection;
		}
	}
}
