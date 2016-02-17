using Experilous.Generation;

namespace Experilous.Topological
{
	[GeneratorCategory(typeof(TopologyGeneratorCollection), "Topology")] public struct TopologyCategory { }
	[GeneratorCategory(typeof(TopologyGeneratorCollection), "Vertex Attributes", after = typeof(TopologyCategory))] public struct VertexAttributesCategory { }
	[GeneratorCategory(typeof(TopologyGeneratorCollection), "Edge Attributes", after = typeof(VertexAttributesCategory))] public struct EdgeAttributesCategory { }
	[GeneratorCategory(typeof(TopologyGeneratorCollection), "Face Attributes", after = typeof(EdgeAttributesCategory))] public struct FaceAttributesCategory { }
	[GeneratorCategory(typeof(TopologyGeneratorCollection), "Utilities", after = typeof(FaceAttributesCategory))] public struct UtilitiesCategory { }
	[GeneratorCategory(typeof(TopologyGeneratorCollection), "Mesh", after = typeof(UtilitiesCategory))] public struct MeshCategory { }

	public class TopologyGeneratorCollection : GeneratorExecutive
	{
		public static TopologyGeneratorCollection Create(string name)
		{
			var collection = CreateInstance<TopologyGeneratorCollection>();
			collection.name = name;
			return collection;
		}
	}
}
