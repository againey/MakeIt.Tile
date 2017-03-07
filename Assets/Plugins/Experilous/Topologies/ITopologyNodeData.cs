/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System;
using System.Collections.Generic;

namespace Experilous.Topologies
{
	public interface ITopologyNodeData<T> : ITopologyEdgeData<T>
	{
		new T this[int i] { get; set; }
		T this[TopologyNode n] { get; set; }
	}

	public struct TopologyNodeDataArray<T> : ITopologyNodeData<T>
	{
		public readonly T[] array;

		public TopologyNodeDataArray(T[] array)
		{
			this.array = array;
		}

		public TopologyNodeDataArray(int elementCount)
		{
			array = new T[elementCount];
		}

		public static implicit operator T[](TopologyNodeDataArray<T> data)
		{
			return data.array;
		}

		public static implicit operator TopologyNodeDataArray<T>(T[] array)
		{
			return new TopologyNodeDataArray<T>(array);
		}

		public T this[int i]
		{
			get { return array[i]; }
			set { array[i] = value; }
		}

		public T this[TopologyNode n]
		{
			get { return array[n.index]; }
			set { array[n.index] = value; }
		}

		public T this[TopologyEdge e]
		{
			get { return array[e.node.index]; }
			set { array[e.node.index] = value; }
		}

		public T this[TopologyNodeEdge e]
		{
			get { return array[e.node.index]; }
			set { array[e.node.index] = value; }
		}

		public T this[TopologyFaceEdge e]
		{
			get { return array[e.nextNode.index]; }
			set { array[e.nextNode.index] = value; }
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

		T ITopologyNodeData<T>.this[int i]
		{
			get { return this[i]; }
			set { this[i] = value; }
		}

		T ITopologyEdgeData<T>.this[int i]
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
	}

	public class TopologyNodeDataList<T> : List<T>, ITopologyNodeData<T>
	{
		public TopologyNodeDataList()
		{
		}

		public TopologyNodeDataList(T[] array)
			: base(array)
		{
		}

		public TopologyNodeDataList(IEnumerable<T> data)
			: base(data)
		{
		}

		public TopologyNodeDataList(int capacity)
			: base(capacity)
		{
		}

		public T this[TopologyNode n]
		{
			get { return this[n.index]; }
			set { this[n.index] = value; }
		}

		public T this[TopologyEdge e]
		{
			get { return this[e.node.index]; }
			set { this[e.node.index] = value; }
		}

		public T this[TopologyNodeEdge e]
		{
			get { return this[e.node.index]; }
			set { this[e.node.index] = value; }
		}

		public T this[TopologyFaceEdge e]
		{
			get { return this[e.nextNode.index]; }
			set { this[e.nextNode.index] = value; }
		}

		T ITopologyNodeData<T>.this[int i]
		{
			get { return this[i]; }
			set { this[i] = value; }
		}

		T ITopologyEdgeData<T>.this[int i]
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
	}
}
