namespace Experilous.Topological
{
	public class EdgeWrapDataEdgeAttribute : EdgeArrayAttribute<EdgeWrap>, System.ICloneable
	{
		public static EdgeWrapDataEdgeAttribute CreateInstance() { return CreateDerivedInstance<EdgeWrapDataEdgeAttribute>(); }
		public static EdgeWrapDataEdgeAttribute CreateInstance(EdgeWrap[] array) { return CreateDerivedInstance<EdgeWrapDataEdgeAttribute>(array); }
		public static EdgeWrapDataEdgeAttribute CreateInstance(EdgeWrap[] array, string name) { return CreateDerivedInstance<EdgeWrapDataEdgeAttribute>(array, name); }
		public static new EdgeWrapDataEdgeAttribute CreateInstance(string name) { return CreateDerivedInstance<EdgeWrapDataEdgeAttribute>(name); }
		object System.ICloneable.Clone() { return Clone(); }
		public EdgeWrapDataEdgeAttribute Clone() { return CloneDerived<EdgeWrapDataEdgeAttribute>(); }
	}
}
