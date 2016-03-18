/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;

namespace Experilous.Topological
{
	public class Vector2EdgeAttribute : EdgeArrayAttribute<Vector2>
	{
		public static Vector2EdgeAttribute Create(Vector2[] array) { return CreateDerived<Vector2EdgeAttribute>(array); }
		public static Vector2EdgeAttribute Create(int edgeCount) { return CreateDerived<Vector2EdgeAttribute>(edgeCount); }
	}
}
