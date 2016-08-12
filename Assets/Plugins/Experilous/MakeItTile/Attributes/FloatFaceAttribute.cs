/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

namespace Experilous.Topological
{
	public class FloatFaceAttribute : FaceArrayAttribute<float>
	{
		public static FloatFaceAttribute Create(float[] array) { return CreateDerived<FloatFaceAttribute>(array); }
		public static FloatFaceAttribute Create(int faceCount) { return CreateDerived<FloatFaceAttribute>(faceCount); }
	}
}
