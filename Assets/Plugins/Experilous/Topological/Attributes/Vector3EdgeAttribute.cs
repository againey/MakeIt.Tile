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
	public class Vector3EdgeAttribute : EdgeArrayAttribute<Vector3>
	{
		public static Vector3EdgeAttribute Create(Vector3[] array) { return CreateDerived<Vector3EdgeAttribute>(array); }
		public static Vector3EdgeAttribute Create(int edgeCount) { return CreateDerived<Vector3EdgeAttribute>(edgeCount); }
	}
}
