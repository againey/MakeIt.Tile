using System.Collections.Generic;

namespace Experilous.Topological
{
	public struct VertexAttribute<T> where T : new()
	{
		public T[] _values;

		public VertexAttribute(int vertexCount)
		{
			_values = new T[vertexCount];
		}

		public VertexAttribute(T[] values)
		{
			_values = values.Clone() as T[];
		}

		public VertexAttribute(ICollection<T> collection)
		{
			_values = new T[collection.Count];
			collection.CopyTo(_values, 0);
		}

		public VertexAttribute<T> Clone()
		{
			return new VertexAttribute<T>(_values);
		}

		public T this[int i]
		{
			get {  return _values[i]; }
			set {  _values[i] = value; }
		}

		public T this[Topology.Vertex vertex]
		{
			get {  return _values[vertex.index]; }
			set { _values[vertex.index] = value; }
		}

		public int Count
		{
			get { return _values.Length; }
		}

		public void Clear()
		{
			System.Array.Clear(_values, 0, _values.Length);
		}
	}
}
