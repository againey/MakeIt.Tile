namespace Experilous.Topological
{
	public class FloatEdgeAttribute : EdgeAttribute<float>
	{
		public static FloatEdgeAttribute CreateInstance() { return CreateDerivedInstance<FloatEdgeAttribute>(); }
		public static FloatEdgeAttribute CreateInstance(float[] array) { return CreateDerivedInstance<FloatEdgeAttribute>(array); }
		public static FloatEdgeAttribute CreateInstance(float[] array, string name) { return CreateDerivedInstance<FloatEdgeAttribute>(array, name); }
		public static new FloatEdgeAttribute CreateInstance(string name) { return CreateDerivedInstance<FloatEdgeAttribute>(name); }
	}
}
