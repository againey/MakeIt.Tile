/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;

namespace Experilous.MakeIt.Tile
{
	public class ConstantColorVertexAttribute : VertexConstantAttribute<Color>
	{
		public static ConstantColorVertexAttribute Create(Color constant) { return CreateDerived<ConstantColorVertexAttribute>(constant); }
	}
}
