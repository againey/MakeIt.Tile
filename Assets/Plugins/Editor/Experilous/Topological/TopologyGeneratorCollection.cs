/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using Experilous.Generation;

namespace Experilous.Topological
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
