namespace Experilous.Topological
{
	public class BoolFaceAttribute : FaceArrayAttribute<bool>, System.ICloneable
	{
		public static BoolFaceAttribute CreateInstance() { return CreateDerivedInstance<BoolFaceAttribute>(); }
		public static BoolFaceAttribute CreateInstance(bool[] array) { return CreateDerivedInstance<BoolFaceAttribute>(array); }
		public static BoolFaceAttribute CreateInstance(bool[] array, string name) { return CreateDerivedInstance<BoolFaceAttribute>(array, name); }
		public static new BoolFaceAttribute CreateInstance(string name) { return CreateDerivedInstance<BoolFaceAttribute>(name); }
		object System.ICloneable.Clone() { return Clone(); }
		public BoolFaceAttribute Clone() { return CloneDerived<BoolFaceAttribute>(); }
	}
}
