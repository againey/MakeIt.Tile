/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System;
using System.Collections.Generic;

namespace Experilous.Topologies
{
	public interface IGraphNodeData<T> : IGraphEdgeData<T>
	{
		new T this[int i] { get; set; }
		T this[GraphNode n] { get; set; }
	}

	public struct GraphNodeDataArray<T> : IGraphNodeData<T>
	{
		public readonly T[] array;

		public GraphNodeDataArray(T[] array)
		{
			this.array = array;
		}

		public GraphNodeDataArray(int elementCount)
		{
			array = new T[elementCount];
		}

		public static implicit operator T[](GraphNodeDataArray<T> data)
		{
			return data.array;
		}

		public static implicit operator GraphNodeDataArray<T>(T[] array)
		{
			return new GraphNodeDataArray<T>(array);
		}

		public T this[int i]
		{
			get { return array[i]; }
			set { array[i] = value; }
		}

		public T this[GraphNode n]
		{
			get { return array[n.index]; }
			set { array[n.index] = value; }
		}

		public T this[GraphEdge e]
		{
			get { return array[e.node.index]; }
			set { array[e.node.index] = value; }
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

		T IGraphNodeData<T>.this[int i]
		{
			get { return this[i]; }
			set { this[i] = value; }
		}

		T IGraphEdgeData<T>.this[int i]
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
	}

	public class GraphNodeDataList<T> : List<T>, IGraphNodeData<T>
	{
		public GraphNodeDataList()
		{
		}

		public GraphNodeDataList(T[] array)
			: base(array)
		{
		}

		public GraphNodeDataList(IEnumerable<T> data)
			: base(data)
		{
		}

		public GraphNodeDataList(int capacity)
			: base(capacity)
		{
		}

		public T this[GraphNode n]
		{
			get { return this[n.index]; }
			set { this[n.index] = value; }
		}

		public T this[GraphEdge e]
		{
			get { return this[e.node.index]; }
			set { this[e.node.index] = value; }
		}

		T IGraphNodeData<T>.this[int i]
		{
			get { return this[i]; }
			set { this[i] = value; }
		}

		T IGraphEdgeData<T>.this[int i]
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
	}
}
