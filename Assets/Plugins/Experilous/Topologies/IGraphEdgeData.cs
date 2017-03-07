/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System;
using System.Collections.Generic;

namespace Experilous.Topologies
{
	public interface IGraphEdgeData<T> : IList<T>
	{
		new T this[int i] { get; set; }
		T this[GraphEdge e] { get; set; }
	}

	public struct GraphEdgeDataArray<T> : IGraphEdgeData<T>
	{
		public readonly T[] array;

		public GraphEdgeDataArray(T[] array)
		{
			this.array = array;
		}

		public GraphEdgeDataArray(int elementCount)
		{
			array = new T[elementCount];
		}

		public static implicit operator T[](GraphEdgeDataArray<T> data)
		{
			return data.array;
		}

		public static implicit operator GraphEdgeDataArray<T>(T[] array)
		{
			return new GraphEdgeDataArray<T>(array);
		}

		public T this[int i]
		{
			get { return array[i]; }
			set { array[i] = value; }
		}

		public T this[GraphEdge e]
		{
			get { return array[e.index]; }
			set { array[e.index] = value; }
		}

		public int Count { get { return array.Length; } }

		public bool IsReadOnly { get { return true; } }

		public void Add(T item) { throw new NotSupportedException(); }

		public void Clear() { throw new NotSupportedException(); }

		public bool Contains(T item) { return ((IList<T>)array).Contains(item); }

		public void CopyTo(T[] array, int arrayIndex) { this.array.CopyTo(array, arrayIndex); }

		public IEnumerator<T> GetEnumerator() { return ((IList<T>)array).GetEnumerator(); }

		public int IndexOf(T item) { return ((IList<T>)array).IndexOf(item); }

		public void Insert(int index, T item) { throw new NotSupportedException(); }

		public bool Remove(T item) { throw new NotSupportedException(); }

		public void RemoveAt(int index) { throw new NotSupportedException(); }

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	public class GraphEdgeDataList<T> : List<T>, IGraphEdgeData<T>
	{
		public GraphEdgeDataList()
		{
		}

		public GraphEdgeDataList(T[] array)
			: base(array)
		{
		}

		public GraphEdgeDataList(IEnumerable<T> data)
			: base(data)
		{
		}

		public GraphEdgeDataList(int capacity)
			: base(capacity)
		{
		}

		public T this[GraphEdge e]
		{
			get { return this[e.index]; }
			set { this[e.index] = value; }
		}
	}
}
