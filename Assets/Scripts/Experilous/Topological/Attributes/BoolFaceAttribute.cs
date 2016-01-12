namespace Experilous.Topological
{
	public class BoolFaceAttribute : FaceAttribute<bool>
	{
		public static BoolFaceAttribute CreateInstance() { return CreateDerivedInstance<BoolFaceAttribute>(); }
		public static BoolFaceAttribute CreateInstance(bool[] array) { return CreateDerivedInstance<BoolFaceAttribute>(array); }
		public static BoolFaceAttribute CreateInstance(bool[] array, string name) { return CreateDerivedInstance<BoolFaceAttribute>(array, name); }
		public static new BoolFaceAttribute CreateInstance(string name) { return CreateDerivedInstance<BoolFaceAttribute>(name); }
	}
}
