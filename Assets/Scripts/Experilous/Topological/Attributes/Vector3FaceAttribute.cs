using UnityEngine;

namespace Experilous.Topological
{
	public class Vector3FaceAttribute : FaceArrayAttribute<Vector3>, System.ICloneable
	{
		public static Vector3FaceAttribute CreateInstance() { return CreateDerivedInstance<Vector3FaceAttribute>(); }
		public static Vector3FaceAttribute CreateInstance(Vector3[] array) { return CreateDerivedInstance<Vector3FaceAttribute>(array); }
		public static Vector3FaceAttribute CreateInstance(Vector3[] array, string name) { return CreateDerivedInstance<Vector3FaceAttribute>(array, name); }
		public static new Vector3FaceAttribute CreateInstance(string name) { return CreateDerivedInstance<Vector3FaceAttribute>(name); }
		object System.ICloneable.Clone() { return Clone(); }
		public Vector3FaceAttribute Clone() { return CloneDerived<Vector3FaceAttribute>(); }
	}
}
