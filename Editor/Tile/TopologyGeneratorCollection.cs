/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using MakeIt.Generate;

namespace MakeIt.Tile
{
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
