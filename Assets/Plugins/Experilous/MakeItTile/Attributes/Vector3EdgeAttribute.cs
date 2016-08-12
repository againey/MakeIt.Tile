/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;

namespace Experilous.Topological
{
	public class Vector3EdgeAttribute : EdgeArrayAttribute<Vector3>
	{
		public static Vector3EdgeAttribute Create(Vector3[] array) { return CreateDerived<Vector3EdgeAttribute>(array); }
		public static Vector3EdgeAttribute Create(int edgeCount) { return CreateDerived<Vector3EdgeAttribute>(edgeCount); }
	}
}
