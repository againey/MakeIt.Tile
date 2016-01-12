using UnityEngine;

namespace Experilous.Topological
{
	public class Vector2FaceAttribute : FaceAttribute<Vector2>
	{
		public static Vector2FaceAttribute CreateInstance() { return CreateDerivedInstance<Vector2FaceAttribute>(); }
		public static Vector2FaceAttribute CreateInstance(Vector2[] array) { return CreateDerivedInstance<Vector2FaceAttribute>(array); }
		public static Vector2FaceAttribute CreateInstance(Vector2[] array, string name) { return CreateDerivedInstance<Vector2FaceAttribute>(array, name); }
		public static new Vector2FaceAttribute CreateInstance(string name) { return CreateDerivedInstance<Vector2FaceAttribute>(name); }
	}
}
