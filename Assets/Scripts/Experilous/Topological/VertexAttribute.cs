using System.Collections.Generic;

namespace Experilous.Topological
{
	public interface IVertexAttribute<T> where T : new()
	{
		T this[int vertexIndex] { get; set; }
		T this[Topology.Vertex vertex] { get; set; }

		int Count { get; }

		void Clear();
	}

	public struct VertexAttribute<T> : IVertexAttribute<T> where T : new()
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

		public T this[int vertexIndex]
		{
			get {  return _values[vertexIndex]; }
			set {  _values[vertexIndex] = value; }
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
