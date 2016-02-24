/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
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
