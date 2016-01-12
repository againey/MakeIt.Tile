namespace Experilous.Topological
{
	public class FloatVertexAttribute : VertexAttribute<float>
	{
		public static FloatVertexAttribute CreateInstance() { return CreateDerivedInstance<FloatVertexAttribute>(); }
		public static FloatVertexAttribute CreateInstance(float[] array) { return CreateDerivedInstance<FloatVertexAttribute>(array); }
		public static FloatVertexAttribute CreateInstance(float[] array, string name) { return CreateDerivedInstance<FloatVertexAttribute>(array, name); }
		public static new FloatVertexAttribute CreateInstance(string name) { return CreateDerivedInstance<FloatVertexAttribute>(name); }
	}
}
