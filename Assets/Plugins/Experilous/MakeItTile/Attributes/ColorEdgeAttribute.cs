/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;

namespace Experilous.MakeItTile
{
	public class ColorEdgeAttribute : EdgeArrayAttribute<Color>
	{
		public static ColorEdgeAttribute Create(Color[] array) { return CreateDerived<ColorEdgeAttribute>(array); }
		public static ColorEdgeAttribute Create(int edgeCount) { return CreateDerived<ColorEdgeAttribute>(edgeCount); }
	}
}
