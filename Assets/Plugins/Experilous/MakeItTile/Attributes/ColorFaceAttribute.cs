/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;

namespace Experilous.MakeItTile
{
	public class ColorFaceAttribute : FaceArrayAttribute<Color>
	{
		public static ColorFaceAttribute Create(Color[] array) { return CreateDerived<ColorFaceAttribute>(array); }
		public static ColorFaceAttribute Create(int faceCount) { return CreateDerived<ColorFaceAttribute>(faceCount); }
	}
}
