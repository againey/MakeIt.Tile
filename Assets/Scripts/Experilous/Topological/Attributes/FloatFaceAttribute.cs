namespace Experilous.Topological
{
	public class FloatFaceAttribute : FaceAttribute<float>
	{
		public static FloatFaceAttribute CreateInstance() { return CreateDerivedInstance<FloatFaceAttribute>(); }
		public static FloatFaceAttribute CreateInstance(float[] array) { return CreateDerivedInstance<FloatFaceAttribute>(array); }
		public static FloatFaceAttribute CreateInstance(float[] array, string name) { return CreateDerivedInstance<FloatFaceAttribute>(array, name); }
		public static new FloatFaceAttribute CreateInstance(string name) { return CreateDerivedInstance<FloatFaceAttribute>(name); }
	}
}
