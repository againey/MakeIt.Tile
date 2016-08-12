/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;

namespace Experilous.MakeIt.Tile
{
	public class ColorFaceGroupAttribute : FaceGroupArrayAttribute<Color>
	{
		public static ColorFaceGroupAttribute Create(Color[] array) { return CreateDerived<ColorFaceGroupAttribute>(array); }
		public static ColorFaceGroupAttribute Create(int groupCount) { return CreateDerived<ColorFaceGroupAttribute>(groupCount); }
	}
}
