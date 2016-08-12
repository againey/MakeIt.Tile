/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;

namespace Experilous.MakeIt.Tile
{
	public class ConstantColorEdgeAttribute : EdgeConstantAttribute<Color>
	{
		public static ConstantColorEdgeAttribute Create(Color constant) { return CreateDerived<ConstantColorEdgeAttribute>(constant); }
	}
}
