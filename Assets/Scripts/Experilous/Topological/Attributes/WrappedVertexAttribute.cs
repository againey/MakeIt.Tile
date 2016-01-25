using UnityEngine;

namespace Experilous.Topological
{
	public abstract class WrappedVertexAttribute<T, TVertexAttribute> : EdgeAttribute<T> where TVertexAttribute : VertexAttribute<T>
	{
		public Topology topology;
		public EdgeWrapDataEdgeAttribute edgeWrapData;
		public TVertexAttribute vertexData;

		protected abstract T GetVertexRelativeAttribute(T vertexValue, EdgeWrap edgeWrap);
		protected abstract T GetFaceRelativeAttribute(T vertexValue, EdgeWrap edgeWrap);

		public override T this[int i]
		{
			get { return GetFaceRelativeAttribute(vertexData[topology.faceEdges[i].nextVertex], edgeWrapData[topology.faceEdges[i]]); }
			set { throw new System.NotSupportedException("A wrapped vertex attribute is read only and cannot be modified."); }
		}

		public override T this[Topology.VertexEdge e]
		{
			get { return GetVertexRelativeAttribute(vertexData[e.farVertex], edgeWrapData[e]); }
			set { throw new System.NotSupportedException("A wrapped vertex attribute is read only and cannot be modified."); }
		}

		public override T this[Topology.FaceEdge e]
		{
			get { return GetFaceRelativeAttribute(vertexData[e.nextVertex], edgeWrapData[e]); }
			set { throw new System.NotSupportedException("A wrapped vertex attribute is read only and cannot be modified."); }
		}
	}
}
