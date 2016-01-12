using UnityEngine;

namespace Experilous.Topological
{
	public class Vector2EdgeAttribute : EdgeAttribute<Vector2>
	{
		public static Vector2EdgeAttribute CreateInstance() { return CreateDerivedInstance<Vector2EdgeAttribute>(); }
		public static Vector2EdgeAttribute CreateInstance(Vector2[] array) { return CreateDerivedInstance<Vector2EdgeAttribute>(array); }
		public static Vector2EdgeAttribute CreateInstance(Vector2[] array, string name) { return CreateDerivedInstance<Vector2EdgeAttribute>(array, name); }
		public static new Vector2EdgeAttribute CreateInstance(string name) { return CreateDerivedInstance<Vector2EdgeAttribute>(name); }
	}
}
