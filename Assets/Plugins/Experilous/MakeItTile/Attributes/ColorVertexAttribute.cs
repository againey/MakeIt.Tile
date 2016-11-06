/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;

namespace Experilous.MakeItTile
{
	public class ColorVertexAttribute : VertexArrayAttribute<Color>
	{
		public static ColorVertexAttribute Create(Color[] array) { return CreateDerived<ColorVertexAttribute>(array); }
		public static ColorVertexAttribute Create(int vertexCount) { return CreateDerived<ColorVertexAttribute>(vertexCount); }
	}
}
