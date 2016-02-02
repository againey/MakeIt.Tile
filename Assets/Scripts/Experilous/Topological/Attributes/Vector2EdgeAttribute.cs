using UnityEngine;

namespace Experilous.Topological
{
	public class Vector2EdgeAttribute : EdgeArrayAttribute<Vector2>, System.ICloneable
	{
		public static Vector2EdgeAttribute CreateInstance() { return CreateDerivedInstance<Vector2EdgeAttribute>(); }
		public static Vector2EdgeAttribute CreateInstance(Vector2[] array) { return CreateDerivedInstance<Vector2EdgeAttribute>(array); }
		public static Vector2EdgeAttribute CreateInstance(Vector2[] array, string name) { return CreateDerivedInstance<Vector2EdgeAttribute>(array, name); }
		public static new Vector2EdgeAttribute CreateInstance(string name) { return CreateDerivedInstance<Vector2EdgeAttribute>(name); }
		object System.ICloneable.Clone() { return Clone(); }
		public Vector2EdgeAttribute Clone() { return CloneDerived<Vector2EdgeAttribute>(); }
	}
}
