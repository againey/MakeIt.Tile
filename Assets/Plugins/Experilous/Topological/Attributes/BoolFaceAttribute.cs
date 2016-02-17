namespace Experilous.Topological
{
	public class BoolFaceAttribute : FaceArrayAttribute<bool>
	{
		public static BoolFaceAttribute Create(bool[] array) { return CreateDerived<BoolFaceAttribute>(array); }
		public static BoolFaceAttribute Create(int faceCount) { return CreateDerived<BoolFaceAttribute>(faceCount); }
	}
}
