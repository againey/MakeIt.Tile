using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;

namespace Experilous.Topological
{
	[AssetGeneratorCategory(typeof(TopologyGeneratorBundle), "Topology")] public struct TopologyCategory { }
	[AssetGeneratorCategory(typeof(TopologyGeneratorBundle), "Vertex Attributes", after = typeof(TopologyCategory))] public struct VertexAttributesCategory { }
	[AssetGeneratorCategory(typeof(TopologyGeneratorBundle), "Edge Attributes", after = typeof(VertexAttributesCategory))] public struct EdgeAttributesCategory { }
	[AssetGeneratorCategory(typeof(TopologyGeneratorBundle), "Face Attributes", after = typeof(EdgeAttributesCategory))] public struct FaceAttributesCategory { }
	[AssetGeneratorCategory(typeof(TopologyGeneratorBundle), "Utilities", after = typeof(FaceAttributesCategory))] public struct UtilitiesCategory { }
	[AssetGeneratorCategory(typeof(TopologyGeneratorBundle), "Mesh", after = typeof(UtilitiesCategory))] public struct MeshCategory { }

	public class TopologyGeneratorBundle : AssetGeneratorBundle
	{
		public override string defaultName
		{
			get
			{
				return "New Topology";
			}
		}

		public static TopologyGeneratorBundle CreateDefaultInstance(string name)
		{
			var bundle = CreateInstance<TopologyGeneratorBundle>();
			bundle.name = name;
			return bundle;
		}
	}
}
