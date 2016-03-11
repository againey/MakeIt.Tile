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
	public class ColorEdgeAttribute : EdgeArrayAttribute<Color>
	{
		public static ColorEdgeAttribute Create(Color[] array) { return CreateDerived<ColorEdgeAttribute>(array); }
		public static ColorEdgeAttribute Create(int edgeCount) { return CreateDerived<ColorEdgeAttribute>(edgeCount); }
	}
}
