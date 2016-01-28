using UnityEngine;
using System;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public abstract class WrappedEdgeAttribute<T> : EdgeAttribute<T>, ISerializationCallbackReceiver
	{
		[NonSerialized] public IEdgeAttribute<EdgeWrap> edgeWrapAttribute;
		[NonSerialized] public IEdgeAttribute<T> edgeAttribute;

		[SerializeField] private ScriptableObject _edgeWrapAttributeReference;
		[SerializeField] private ScriptableObject _edgeAttributeReference;

		protected abstract T GetVertexRelativeAttribute(T edgeValue, EdgeWrap edgeWrap);
		protected abstract T GetFaceRelativeAttribute(T edgeValue, EdgeWrap edgeWrap);

		public override T this[int i]
		{
			get { return edgeAttribute[i]; }
			set { edgeAttribute[i] = value; }
		}

		public override T this[Topology.HalfEdge e]
		{
			get { return edgeAttribute[e]; }
			set { edgeAttribute[e] = value; }
		}

		public override T this[Topology.VertexEdge e]
		{
			get { return GetVertexRelativeAttribute(edgeAttribute[e], edgeWrapAttribute[e]); }
			set { throw new NotSupportedException("A vertex-relative wrapped edge attribute is read only and cannot be modified."); }
		}

		public override T this[Topology.FaceEdge e]
		{
			get { return GetFaceRelativeAttribute(edgeAttribute[e], edgeWrapAttribute[e]); }
			set { throw new NotSupportedException("A face-relative wrapped edge attribute is read only and cannot be modified."); }
		}

		public override int Count { get { return edgeAttribute.Count; } }
		public override bool IsReadOnly { get { return edgeAttribute.IsReadOnly; } }
		public override void Add(T item) { edgeAttribute.Add(item); }
		public override void Clear() { edgeAttribute.Clear(); }
		public override bool Contains(T item) { return edgeAttribute.Contains(item); }
		public override void CopyTo(T[] array, int arrayIndex) { edgeAttribute.CopyTo(array, arrayIndex); }
		public override IEnumerator<T> GetEnumerator() { return edgeAttribute.GetEnumerator(); }
		public override int IndexOf(T item) { return edgeAttribute.IndexOf(item); }
		public override void Insert(int index, T item) { edgeAttribute.Insert(index, item); }
		public override bool Remove(T item) { return edgeAttribute.Remove(item); }
		public override void RemoveAt(int index) { edgeAttribute.RemoveAt(index); }

		public void OnBeforeSerialize()
		{
			_edgeWrapAttributeReference = (ScriptableObject)edgeWrapAttribute;
			_edgeAttributeReference = (ScriptableObject)edgeAttribute;
		}

		public void OnAfterDeserialize()
		{
			edgeWrapAttribute = (IEdgeAttribute<EdgeWrap>)_edgeWrapAttributeReference;
			edgeAttribute = (IEdgeAttribute<T>)_edgeAttributeReference;
		}
	}
}
