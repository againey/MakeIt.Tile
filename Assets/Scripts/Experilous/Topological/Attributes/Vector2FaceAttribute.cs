using UnityEngine;

namespace Experilous.Topological
{
	public class Vector2FaceAttribute : FaceArrayAttribute<Vector2>, System.ICloneable
	{
		public static Vector2FaceAttribute CreateInstance() { return CreateDerivedInstance<Vector2FaceAttribute>(); }
		public static Vector2FaceAttribute CreateInstance(Vector2[] array) { return CreateDerivedInstance<Vector2FaceAttribute>(array); }
		public static Vector2FaceAttribute CreateInstance(Vector2[] array, string name) { return CreateDerivedInstance<Vector2FaceAttribute>(array, name); }
		public static new Vector2FaceAttribute CreateInstance(string name) { return CreateDerivedInstance<Vector2FaceAttribute>(name); }
		object System.ICloneable.Clone() { return Clone(); }
		public Vector2FaceAttribute Clone() { return CloneDerived<Vector2FaceAttribute>(); }
	}
}
