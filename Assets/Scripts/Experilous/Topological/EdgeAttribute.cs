using UnityEngine;
using System;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public interface IEdgeAttribute<T> where T : new()
	{
		T this[int edgeIndex] { get; set; }
		T this[Topology.VertexEdge edge] { get; set; }
		T this[Topology.FaceEdge edge] { get; set; }

		int Count { get; }

		void Clear();
	}

	[Serializable]
	public struct EdgeAttribute<T> : IEdgeAttribute<T> where T : new()
	{
		[SerializeField]
		private T[] _values;

		public EdgeAttribute(int edgeCount)
		{
			_values = new T[edgeCount];
		}

		public EdgeAttribute(T[] values)
		{
			_values = values.Clone() as T[];
		}

		public EdgeAttribute(ICollection<T> collection)
		{
			_values = new T[collection.Count];
			collection.CopyTo(_values, 0);
		}

		public EdgeAttribute<T> Clone()
		{
			return new EdgeAttribute<T>(_values);
		}

		public T this[int edgeIndex]
		{
			get {  return _values[edgeIndex]; }
			set {  _values[edgeIndex] = value; }
		}

		public T this[Topology.VertexEdge edge]
		{
			get {  return _values[edge.index]; }
			set { _values[edge.index] = value; }
		}

		public T this[Topology.FaceEdge edge]
		{
			get {  return _values[edge.index]; }
			set { _values[edge.index] = value; }
		}

		public int Count
		{
			get { return (_values != null) ? _values.Length : 0; }
		}

		public void Clear()
		{
			System.Array.Clear(_values, 0, _values.Length);
		}

		public void Reset()
		{
			_values = null;
		}

		public bool isEmpty
		{
			get { return _values == null || _values.Length == 0; }
		}

		public T[] underlyingArray
		{
			get { return _values; }
		}
	}
}
