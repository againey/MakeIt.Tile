using System.Collections.Generic;

namespace Experilous.Topological
{
	public struct EdgeAttribute<T> where T : new()
	{
		public T[] _values;

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

		public T this[int i]
		{
			get {  return _values[i]; }
			set {  _values[i] = value; }
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
			get { return _values.Length; }
		}
	}
}
