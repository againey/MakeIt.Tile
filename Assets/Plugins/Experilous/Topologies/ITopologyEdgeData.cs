/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System;
using System.Collections.Generic;

namespace Experilous.Topologies
{
	public interface ITopologyEdgeData<T> : IList<T>
	{
		new T this[int i] { get; set; }
		T this[TopologyEdge e] { get; set; }
		T this[TopologyNodeEdge e] { get; set; }
		T this[TopologyFaceEdge e] { get; set; }
	}

	public struct TopologyEdgeDataArray<T> : ITopologyEdgeData<T>
	{
		public readonly T[] array;

		public TopologyEdgeDataArray(T[] array)
		{
			this.array = array;
		}

		public TopologyEdgeDataArray(int elementCount)
		{
			array = new T[elementCount];
		}

		public static implicit operator T[](TopologyEdgeDataArray<T> data)
		{
			return data.array;
		}

		public static implicit operator TopologyEdgeDataArray<T>(T[] array)
		{
			return new TopologyEdgeDataArray<T>(array);
		}

		public T this[int i]
		{
			get { return array[i]; }
			set { array[i] = value; }
		}

		public T this[TopologyEdge e]
		{
			get { return array[e.index]; }
			set { array[e.index] = value; }
		}

		public T this[TopologyNodeEdge e]
		{
			get { return array[e.index]; }
			set { array[e.index] = value; }
		}

		public T this[TopologyFaceEdge e]
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

	public class TopologyEdgeDataList<T> : List<T>, ITopologyEdgeData<T>
	{
		public TopologyEdgeDataList()
		{
		}

		public TopologyEdgeDataList(T[] array)
			: base(array)
		{
		}

		public TopologyEdgeDataList(IEnumerable<T> data)
			: base(data)
		{
		}

		public TopologyEdgeDataList(int capacity)
			: base(capacity)
		{
		}

		public T this[TopologyEdge e]
		{
			get { return this[e.index]; }
			set { this[e.index] = value; }
		}

		public T this[TopologyNodeEdge e]
		{
			get { return this[e.index]; }
			set { this[e.index] = value; }
		}

		public T this[TopologyFaceEdge e]
		{
			get { return this[e.index]; }
			set { this[e.index] = value; }
		}
	}
}
