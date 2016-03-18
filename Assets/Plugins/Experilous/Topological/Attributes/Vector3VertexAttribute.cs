/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;

namespace Experilous.Topological
{
	public class Vector3VertexAttribute : VertexArrayAttribute<Vector3>
	{
		public static Vector3VertexAttribute Create(Vector3[] array) { return CreateDerived<Vector3VertexAttribute>(array); }
		public static Vector3VertexAttribute Create(int vertexCount) { return CreateDerived<Vector3VertexAttribute>(vertexCount); }
	}
}
