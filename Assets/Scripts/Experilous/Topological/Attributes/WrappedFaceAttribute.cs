using System;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public abstract class WrappedFaceAttribute<T, TFaceAttribute> : FaceAttribute<T> where TFaceAttribute : FaceAttribute<T>
	{
		public Topology topology;
		public EdgeWrapDataEdgeAttribute edgeWrapData;
		public TFaceAttribute faceData;

		protected abstract T GetVertexRelativeAttribute(T faceValue, EdgeWrap edgeWrap);
		protected abstract T GetFaceRelativeAttribute(T faceValue, EdgeWrap edgeWrap);

		public override T this[int i]
		{
			get { return faceData[i]; }
			set { faceData[i] = value; }
		}

		public override T this[Topology.Face f]
		{
			get { return faceData[f]; }
			set { faceData[f] = value; }
		}

		public override T this[Topology.VertexEdge e]
		{
			get { return GetVertexRelativeAttribute(faceData[e], edgeWrapData[e]); }
			set { throw new NotSupportedException("A vertex-relative wrapped vertex attribute is read only and cannot be modified."); }
		}

		public override T this[Topology.FaceEdge e]
		{
			get { return GetFaceRelativeAttribute(faceData[e], edgeWrapData[e]); }
			set { throw new NotSupportedException("A face-relative wrapped vertex attribute is read only and cannot be modified."); }
		}

		public override int Count { get { return faceData.Count; } }
		public override bool IsReadOnly { get { return faceData.IsReadOnly; } }
		public override void Add(T item) { faceData.Add(item); }
		public override void Clear() { faceData.Clear(); }
		public override bool Contains(T item) { return faceData.Contains(item); }
		public override void CopyTo(T[] array, int arrayIndex) { faceData.CopyTo(array, arrayIndex); }
		public override IEnumerator<T> GetEnumerator() { return faceData.GetEnumerator(); }
		public override int IndexOf(T item) { return faceData.IndexOf(item); }
		public override void Insert(int index, T item) { faceData.Insert(index, item); }
		public override bool Remove(T item) { return faceData.Remove(item); }
		public override void RemoveAt(int index) { faceData.RemoveAt(index); }
	}
}
