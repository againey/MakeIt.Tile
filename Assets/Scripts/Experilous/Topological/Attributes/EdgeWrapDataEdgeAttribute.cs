namespace Experilous.Topological
{
	public class EdgeWrapDataEdgeAttribute : EdgeArrayAttribute<EdgeWrapData>, System.ICloneable
	{
		public static EdgeWrapDataEdgeAttribute CreateInstance() { return CreateDerivedInstance<EdgeWrapDataEdgeAttribute>(); }
		public static EdgeWrapDataEdgeAttribute CreateInstance(EdgeWrapData[] array) { return CreateDerivedInstance<EdgeWrapDataEdgeAttribute>(array); }
		public static EdgeWrapDataEdgeAttribute CreateInstance(EdgeWrapData[] array, string name) { return CreateDerivedInstance<EdgeWrapDataEdgeAttribute>(array, name); }
		public static new EdgeWrapDataEdgeAttribute CreateInstance(string name) { return CreateDerivedInstance<EdgeWrapDataEdgeAttribute>(name); }
		object System.ICloneable.Clone() { return Clone(); }
		public EdgeWrapDataEdgeAttribute Clone() { return CloneDerived<EdgeWrapDataEdgeAttribute>(); }
	}
}
