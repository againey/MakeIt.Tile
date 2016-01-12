using UnityEngine;

namespace Experilous.Topological
{
	public class Vector3EdgeAttribute : EdgeAttribute<Vector3>
	{
		public static Vector3EdgeAttribute CreateInstance() { return CreateDerivedInstance<Vector3EdgeAttribute>(); }
		public static Vector3EdgeAttribute CreateInstance(Vector3[] array) { return CreateDerivedInstance<Vector3EdgeAttribute>(array); }
		public static Vector3EdgeAttribute CreateInstance(Vector3[] array, string name) { return CreateDerivedInstance<Vector3EdgeAttribute>(array, name); }
		public static new Vector3EdgeAttribute CreateInstance(string name) { return CreateDerivedInstance<Vector3EdgeAttribute>(name); }
	}
}
