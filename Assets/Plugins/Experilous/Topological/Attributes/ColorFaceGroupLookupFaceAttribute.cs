using UnityEngine;

namespace Experilous.Topological
{
	public class ColorFaceGroupLookupFaceAttribute : FaceGroupLookupFaceAttribute<Color, ColorFaceGroupAttribute>
	{
		public static ColorFaceGroupLookupFaceAttribute Create(IntFaceAttribute faceGroupIndices, ColorFaceGroupAttribute faceGroupColors)
		{
			var instance = CreateInstance<ColorFaceGroupLookupFaceAttribute>();
			instance.faceGroupData = faceGroupColors;
			instance.faceGroupIndices = faceGroupIndices;
			return instance;
		}
	}
}
