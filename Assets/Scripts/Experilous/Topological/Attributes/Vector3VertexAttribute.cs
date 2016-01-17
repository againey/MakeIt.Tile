using UnityEngine;

namespace Experilous.Topological
{
	public class Vector3VertexAttribute : VertexArrayAttribute<Vector3>, System.ICloneable
	{
		public static Vector3VertexAttribute CreateInstance() { return CreateDerivedInstance<Vector3VertexAttribute>(); }
		public static Vector3VertexAttribute CreateInstance(Vector3[] array) { return CreateDerivedInstance<Vector3VertexAttribute>(array); }
		public static Vector3VertexAttribute CreateInstance(Vector3[] array, string name) { return CreateDerivedInstance<Vector3VertexAttribute>(array, name); }
		public static new Vector3VertexAttribute CreateInstance(string name) { return CreateDerivedInstance<Vector3VertexAttribute>(name); }
		object System.ICloneable.Clone() { return Clone(); }
		public Vector3VertexAttribute Clone() { return CloneDerived<Vector3VertexAttribute>(); }
	}
}
