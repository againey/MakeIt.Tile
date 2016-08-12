/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;

namespace Experilous.MakeIt.Tile
{
	public class Vector2VertexAttribute : VertexArrayAttribute<Vector2>
	{
		public static Vector2VertexAttribute Create(Vector2[] array) { return CreateDerived<Vector2VertexAttribute>(array); }
		public static Vector2VertexAttribute Create(int vertexCount) { return CreateDerived<Vector2VertexAttribute>(vertexCount); }
	}
}
