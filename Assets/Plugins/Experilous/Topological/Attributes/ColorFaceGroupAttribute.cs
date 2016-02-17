using UnityEngine;

namespace Experilous.Topological
{
	public class ColorFaceGroupAttribute : FaceGroupArrayAttribute<Color>
	{
		public static ColorFaceGroupAttribute Create(Color[] array) { return CreateDerived<ColorFaceGroupAttribute>(array); }
		public static ColorFaceGroupAttribute Create(int groupCount) { return CreateDerived<ColorFaceGroupAttribute>(groupCount); }
	}
}
