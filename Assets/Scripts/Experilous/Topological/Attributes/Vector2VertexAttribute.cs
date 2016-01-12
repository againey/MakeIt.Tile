using UnityEngine;

namespace Experilous.Topological
{
	public class Vector2VertexAttribute : VertexAttribute<Vector2>
	{
		public static Vector2VertexAttribute CreateInstance() { return CreateDerivedInstance<Vector2VertexAttribute>(); }
		public static Vector2VertexAttribute CreateInstance(Vector2[] array) { return CreateDerivedInstance<Vector2VertexAttribute>(array); }
		public static Vector2VertexAttribute CreateInstance(Vector2[] array, string name) { return CreateDerivedInstance<Vector2VertexAttribute>(array, name); }
		public static new Vector2VertexAttribute CreateInstance(string name) { return CreateDerivedInstance<Vector2VertexAttribute>(name); }
	}
}
