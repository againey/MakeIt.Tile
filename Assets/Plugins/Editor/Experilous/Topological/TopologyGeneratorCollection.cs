/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
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
