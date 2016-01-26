using System;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public abstract class WrappedVertexAttribute<T, TVertexAttribute> : VertexAttribute<T> where TVertexAttribute : VertexAttribute<T>
	{
		public Topology topology;
		public EdgeWrapDataEdgeAttribute edgeWrapData;
		public TVertexAttribute vertexData;

		protected abstract T GetVertexRelativeAttribute(T vertexValue, EdgeWrap edgeWrap);
		protected abstract T GetFaceRelativeAttribute(T vertexValue, EdgeWrap edgeWrap);

		public override T this[int i]
		{
			get { return vertexData[i]; }
			set { vertexData[i] = value; }
		}

		public override T this[Topology.Vertex v]
		{
			get { return vertexData[v]; }
			set { vertexData[v] = value; }
		}

		public override T this[Topology.VertexEdge e]
		{
			get { return GetVertexRelativeAttribute(vertexData[e], edgeWrapData[e]); }
			set { throw new NotSupportedException("A vertex-relative wrapped vertex attribute is read only and cannot be modified."); }
		}

		public override T this[Topology.FaceEdge e]
		{
			get { return GetFaceRelativeAttribute(vertexData[e], edgeWrapData[e]); }
			set { throw new NotSupportedException("A face-relative wrapped vertex attribute is read only and cannot be modified."); }
		}

		public override int Count { get { return vertexData.Count; } }
		public override bool IsReadOnly { get { return vertexData.IsReadOnly; } }
		public override void Add(T item) { vertexData.Add(item); }
		public override void Clear() { vertexData.Clear(); }
		public override bool Contains(T item) { return vertexData.Contains(item); }
		public override void CopyTo(T[] array, int arrayIndex) { vertexData.CopyTo(array, arrayIndex); }
		public override IEnumerator<T> GetEnumerator() { return vertexData.GetEnumerator(); }
		public override int IndexOf(T item) { return vertexData.IndexOf(item); }
		public override void Insert(int index, T item) { vertexData.Insert(index, item); }
		public override bool Remove(T item) { return vertexData.Remove(item); }
		public override void RemoveAt(int index) { vertexData.RemoveAt(index); }
	}
}
