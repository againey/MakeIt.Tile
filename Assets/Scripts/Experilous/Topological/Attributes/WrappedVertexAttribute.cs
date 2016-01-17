using UnityEngine;

namespace Experilous.Topological
{
	public abstract class WrappedVertexAttribute<T, TVertexAttribute> : EdgeAttribute<T> where TVertexAttribute : VertexAttribute<T>
	{
		public Topology topology;
		public EdgeWrapDataEdgeAttribute edgeWrapData;
		public TVertexAttribute vertexData;

		protected abstract T GetAttribute(T vertexValue);
		protected abstract T GetAttribute(T vertexValue, int repetitionAxisIndex);
		protected abstract T GetAttribute(T vertexValue, int repetitionAxisIndex0, int repetitionAxisIndex1);

		public T GetAttribute(Topology.FaceEdge prevEdge)
		{
			var prevEdgeWrapData = edgeWrapData[prevEdge];
			var nextEdgeWrapData = edgeWrapData[prevEdge.next];

			if (!prevEdgeWrapData.isWrapped || prevEdgeWrapData.isNegatedAxis)
			{
				if (!nextEdgeWrapData.isWrapped || nextEdgeWrapData.isNegatedAxis)
				{
					return GetAttribute(vertexData[prevEdge.nextVertex]);
				}
				else
				{
					return GetAttribute(vertexData[prevEdge.nextVertex], nextEdgeWrapData.repetitionAxis);
				}
			}
			else
			{
				if (!nextEdgeWrapData.isWrapped || nextEdgeWrapData.isNegatedAxis)
				{
					return GetAttribute(vertexData[prevEdge.nextVertex], prevEdgeWrapData.repetitionAxis);
				}
				else
				{
					return GetAttribute(vertexData[prevEdge.nextVertex], prevEdgeWrapData.repetitionAxis, nextEdgeWrapData.repetitionAxis);
				}
			}
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
