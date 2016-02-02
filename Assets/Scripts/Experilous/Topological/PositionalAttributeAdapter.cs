using UnityEngine;

namespace Experilous.Topological
{
	public class PositionalAttributeAdapter : ScriptableObject
	{
		public static PositionalAttributeAdapter Create()
		{
			var adapter = CreateInstance<PositionalAttributeAdapter>();
			return adapter;
		}

		public static PositionalAttributeAdapter Create(string name)
		{
			var adapter = Create();
			adapter.name = name;
			return adapter;
		}

		public virtual IVertexAttribute<Vector3> Adapt(IVertexAttribute<Vector3> attribute) { return attribute; }
		public virtual IEdgeAttribute<Vector3> Adapt(IEdgeAttribute<Vector3> attribute) { return attribute; }
		public virtual IFaceAttribute<Vector3> Adapt(IFaceAttribute<Vector3> attribute) { return attribute; }
	}
}
