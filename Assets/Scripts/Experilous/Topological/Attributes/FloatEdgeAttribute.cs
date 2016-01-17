namespace Experilous.Topological
{
	public class FloatEdgeAttribute : EdgeArrayAttribute<float>, System.ICloneable
	{
		public static FloatEdgeAttribute CreateInstance() { return CreateDerivedInstance<FloatEdgeAttribute>(); }
		public static FloatEdgeAttribute CreateInstance(float[] array) { return CreateDerivedInstance<FloatEdgeAttribute>(array); }
		public static FloatEdgeAttribute CreateInstance(float[] array, string name) { return CreateDerivedInstance<FloatEdgeAttribute>(array, name); }
		public static new FloatEdgeAttribute CreateInstance(string name) { return CreateDerivedInstance<FloatEdgeAttribute>(name); }
		object System.ICloneable.Clone() { return Clone(); }
		public FloatEdgeAttribute Clone() { return CloneDerived<FloatEdgeAttribute>(); }
	}
}
