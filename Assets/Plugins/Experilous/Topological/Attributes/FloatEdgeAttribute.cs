﻿/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

namespace Experilous.Topological
{
	public class FloatEdgeAttribute : EdgeArrayAttribute<float>
	{
		public static FloatEdgeAttribute Create(float[] array) { return CreateDerived<FloatEdgeAttribute>(array); }
		public static FloatEdgeAttribute Create(int edgeCount) { return CreateDerived<FloatEdgeAttribute>(edgeCount); }
	}
}
