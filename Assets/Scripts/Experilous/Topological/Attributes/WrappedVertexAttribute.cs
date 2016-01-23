using UnityEngine;

namespace Experilous.Topological
{
	public abstract class WrappedVertexAttribute<T, TVertexAttribute> : EdgeAttribute<T> where TVertexAttribute : VertexAttribute<T>
	{
		public Topology topology;
		public EdgeWrapDataEdgeAttribute edgeWrapData;
		public TVertexAttribute vertexData;

		protected abstract T GetAttribute(T vertexValue, EdgeWrap edgeWrap);

		public T GetAttribute(Topology.FaceEdge prevEdge)
		{
			return GetAttribute(vertexData[prevEdge.nextVertex], edgeWrapData[prevEdge] | edgeWrapData[prevEdge.next]);
		}

		public override T this[int i]
		{
			get { return GetAttribute(topology.faceEdges[i]); }
			set { throw new System.NotSupportedException("A wrapped vertex attribute is read only and cannot be modified."); }
		}

		public override T this[Topology.VertexEdge e]
		{
			get { return GetAttribute(e.faceEdge); }
			set { throw new System.NotSupportedException("A wrapped vertex attribute is read only and cannot be modified."); }
		}

		public override T this[Topology.FaceEdge e]
		{
			get { return GetAttribute(e); }
			set { throw new System.NotSupportedException("A wrapped vertex attribute is read only and cannot be modified."); }
		}
	}
}
