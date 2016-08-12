/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

namespace Experilous.MakeIt.Tile
{
	public class IntFaceAttribute : FaceArrayAttribute<int>
	{
		public static IntFaceAttribute Create(int[] array) { return CreateDerived<IntFaceAttribute>(array); }
		public static IntFaceAttribute Create(int faceCount) { return CreateDerived<IntFaceAttribute>(faceCount); }
	}
}
