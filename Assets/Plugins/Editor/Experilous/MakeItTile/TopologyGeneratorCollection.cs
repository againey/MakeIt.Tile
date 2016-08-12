/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using Experilous.MakeIt.Generate;

namespace Experilous.MakeIt.Tile
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
