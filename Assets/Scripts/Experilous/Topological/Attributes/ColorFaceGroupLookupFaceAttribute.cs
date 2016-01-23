using UnityEngine;

namespace Experilous.Topological
{
	public class ColorFaceGroupLookupFaceAttribute : FaceGroupLookupFaceAttribute<Color, ColorFaceGroupAttribute>
	{
		public static ColorFaceGroupLookupFaceAttribute CreateInstance(IntFaceAttribute faceGroupIndices, ColorFaceGroupAttribute faceGroupColors)
		{
			var instance = CreateInstance<ColorFaceGroupLookupFaceAttribute>();
			instance.faceGroupData = faceGroupColors;
			instance.faceGroupIndices = faceGroupIndices;
			return instance;
		}

		public static ColorFaceGroupLookupFaceAttribute CreateInstance(IntFaceAttribute faceGroupIndices, ColorFaceGroupAttribute faceGroupColors, string name)
		{
			var instance = CreateInstance(faceGroupIndices, faceGroupColors);
			instance.name = name;
			return instance;
		}
	}
}
