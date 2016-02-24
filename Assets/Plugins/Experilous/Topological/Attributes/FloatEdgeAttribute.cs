/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

namespace Experilous.Topological
{
	public class FloatEdgeAttribute : EdgeArrayAttribute<float>
	{
		public static FloatEdgeAttribute Create(float[] array) { return CreateDerived<FloatEdgeAttribute>(array); }
		public static FloatEdgeAttribute Create(int edgeCount) { return CreateDerived<FloatEdgeAttribute>(edgeCount); }
	}
}
