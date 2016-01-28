using UnityEngine;
using System;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public abstract class WrappedFaceAttribute<T> : FaceAttribute<T>, ISerializationCallbackReceiver
	{
		[NonSerialized] public IEdgeAttribute<EdgeWrap> edgeWrapAttribute;
		[NonSerialized] public IFaceAttribute<T> faceAttribute;

		[SerializeField] private ScriptableObject _edgeWrapAttributeReference;
		[SerializeField] private ScriptableObject _faceAttributeReference;

		protected abstract T GetVertexRelativeAttribute(T faceValue, EdgeWrap edgeWrap);
		protected abstract T GetEdgeRelativeAttribute(T faceValue, EdgeWrap edgeWrap);
		protected abstract T GetFaceRelativeAttribute(T faceValue, EdgeWrap edgeWrap);

		public override T this[int i]
		{
			get { return faceAttribute[i]; }
			set { faceAttribute[i] = value; }
		}

		public override T this[Topology.Face f]
		{
			get { return faceAttribute[f]; }
			set { faceAttribute[f] = value; }
		}

		public override T this[Topology.HalfEdge e]
		{
			get { return GetEdgeRelativeAttribute(faceAttribute[e], edgeWrapAttribute[e]); }
			set { throw new NotSupportedException("An edge-relative wrapped face attribute is read only and cannot be modified."); }
		}

		public override T this[Topology.VertexEdge e]
		{
			get { return GetVertexRelativeAttribute(faceAttribute[e], edgeWrapAttribute[e]); }
			set { throw new NotSupportedException("A vertex-relative wrapped face attribute is read only and cannot be modified."); }
		}

		public override T this[Topology.FaceEdge e]
		{
			get { return GetFaceRelativeAttribute(faceAttribute[e], edgeWrapAttribute[e]); }
			set { throw new NotSupportedException("A face-relative wrapped face attribute is read only and cannot be modified."); }
		}

		public override int Count { get { return faceAttribute.Count; } }
		public override bool IsReadOnly { get { return faceAttribute.IsReadOnly; } }
		public override void Add(T item) { faceAttribute.Add(item); }
		public override void Clear() { faceAttribute.Clear(); }
		public override bool Contains(T item) { return faceAttribute.Contains(item); }
		public override void CopyTo(T[] array, int arrayIndex) { faceAttribute.CopyTo(array, arrayIndex); }
		public override IEnumerator<T> GetEnumerator() { return faceAttribute.GetEnumerator(); }
		public override int IndexOf(T item) { return faceAttribute.IndexOf(item); }
		public override void Insert(int index, T item) { faceAttribute.Insert(index, item); }
		public override bool Remove(T item) { return faceAttribute.Remove(item); }
		public override void RemoveAt(int index) { faceAttribute.RemoveAt(index); }

		public void OnBeforeSerialize()
		{
			_edgeWrapAttributeReference = (ScriptableObject)edgeWrapAttribute;
			_faceAttributeReference = (ScriptableObject)faceAttribute;
		}

		public void OnAfterDeserialize()
		{
			edgeWrapAttribute = (IEdgeAttribute<EdgeWrap>)_edgeWrapAttributeReference;
			faceAttribute = (IFaceAttribute<T>)_faceAttributeReference;
		}
	}
}
