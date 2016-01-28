using UnityEngine;
using System;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public abstract class WrappedVertexAttribute<T> : VertexAttribute<T>, ISerializationCallbackReceiver
	{
		[NonSerialized] public IEdgeAttribute<EdgeWrap> edgeWrapAttribute;
		[NonSerialized] public IVertexAttribute<T> vertexAttribute;

		[SerializeField] private ScriptableObject _edgeWrapAttributeReference;
		[SerializeField] private ScriptableObject _vertexAttributeReference;

		protected abstract T GetVertexRelativeAttribute(T vertexValue, EdgeWrap edgeWrap);
		protected abstract T GetEdgeRelativeAttribute(T vertexValue, EdgeWrap edgeWrap);
		protected abstract T GetFaceRelativeAttribute(T vertexValue, EdgeWrap edgeWrap);

		public override T this[int i]
		{
			get { return vertexAttribute[i]; }
			set { vertexAttribute[i] = value; }
		}

		public override T this[Topology.Vertex v]
		{
			get { return vertexAttribute[v]; }
			set { vertexAttribute[v] = value; }
		}

		public override T this[Topology.HalfEdge e]
		{
			get { return GetEdgeRelativeAttribute(vertexAttribute[e], edgeWrapAttribute[e]); }
			set { throw new NotSupportedException("An edge-relative wrapped vertex attribute is read only and cannot be modified."); }
		}

		public override T this[Topology.VertexEdge e]
		{
			get { return GetVertexRelativeAttribute(vertexAttribute[e], edgeWrapAttribute[e]); }
			set { throw new NotSupportedException("A vertex-relative wrapped vertex attribute is read only and cannot be modified."); }
		}

		public override T this[Topology.FaceEdge e]
		{
			get { return GetFaceRelativeAttribute(vertexAttribute[e], edgeWrapAttribute[e]); }
			set { throw new NotSupportedException("A face-relative wrapped vertex attribute is read only and cannot be modified."); }
		}

		public override int Count { get { return vertexAttribute.Count; } }
		public override bool IsReadOnly { get { return vertexAttribute.IsReadOnly; } }
		public override void Add(T item) { vertexAttribute.Add(item); }
		public override void Clear() { vertexAttribute.Clear(); }
		public override bool Contains(T item) { return vertexAttribute.Contains(item); }
		public override void CopyTo(T[] array, int arrayIndex) { vertexAttribute.CopyTo(array, arrayIndex); }
		public override IEnumerator<T> GetEnumerator() { return vertexAttribute.GetEnumerator(); }
		public override int IndexOf(T item) { return vertexAttribute.IndexOf(item); }
		public override void Insert(int index, T item) { vertexAttribute.Insert(index, item); }
		public override bool Remove(T item) { return vertexAttribute.Remove(item); }
		public override void RemoveAt(int index) { vertexAttribute.RemoveAt(index); }

		public void OnBeforeSerialize()
		{
			if (edgeWrapAttribute != null && !(edgeWrapAttribute is ScriptableObject)) throw new InvalidOperationException(string.Format("The stored edge wrap attribute of type {0} does not derive from ScriptableObject.", edgeWrapAttribute.GetType().GetPrettyName(true)));
			if (vertexAttribute != null && !(vertexAttribute is ScriptableObject)) throw new InvalidOperationException(string.Format("The stored vertex attribute of type {0} does not derive from ScriptableObject.", vertexAttribute.GetType().GetPrettyName(true)));
			_edgeWrapAttributeReference = (ScriptableObject)edgeWrapAttribute;
			_vertexAttributeReference = (ScriptableObject)vertexAttribute;
		}

		public void OnAfterDeserialize()
		{
			edgeWrapAttribute = (IEdgeAttribute<EdgeWrap>)_edgeWrapAttributeReference;
			vertexAttribute = (IVertexAttribute<T>)_vertexAttributeReference;
		}
	}
}
