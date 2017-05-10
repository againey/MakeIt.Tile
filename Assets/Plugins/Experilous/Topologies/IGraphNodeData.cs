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

	[Serializable]
	public struct GraphNodeDataArray<T> : IGraphNodeData<T>
	{
		[UnityEngine.SerializeField] private T[] _array;

		public T[] array { get { return _array; } }

		public GraphNodeDataArray(T[] array)
		{
			_array = array;
		}

		public GraphNodeDataArray(int elementCount)
		{
			_array = new T[elementCount];
		}

		public static implicit operator T[](GraphNodeDataArray<T> data)
		{
			return data._array;
		}

		public static implicit operator GraphNodeDataArray<T>(T[] array)
		{
			return new GraphNodeDataArray<T>(array);
		}

		public T this[int i]
		{
			get { return _array[i]; }
			set { _array[i] = value; }
		}

		public T this[GraphNode n]
		{
			get { return _array[n.index]; }
			set { _array[n.index] = value; }
		}

		public T this[GraphEdge e]
		{
			get { return _array[e.node.index]; }
			set { _array[e.node.index] = value; }
		}

		public int Count { get { return _array.Length; } }

		public bool IsReadOnly { get { return true; } }

		public void Add(T item) { throw new NotSupportedException(); }

		public void Clear() { throw new NotSupportedException(); }

		public bool Contains(T item) { return ((IList<T>)_array).Contains(item); }

		public void CopyTo(T[] array, int arrayIndex) { this._array.CopyTo(array, arrayIndex); }

		public IEnumerator<T> GetEnumerator() { return ((IList<T>)_array).GetEnumerator(); }

		public int IndexOf(T item) { return ((IList<T>)_array).IndexOf(item); }

		public void Insert(int index, T item) { throw new NotSupportedException(); }

		public bool Remove(T item) { throw new NotSupportedException(); }

		public void RemoveAt(int index) { throw new NotSupportedException(); }

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }

		T IGraphNodeData<T>.this[int i]
		{
			get { return _array[i]; }
			set { _array[i] = value; }
		}

		T IGraphEdgeData<T>.this[int i]
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
	}

	[Serializable]
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

	[Serializable]
	public class GraphNodeDataListWrapper<T> : IGraphNodeData<T>
	{
		[UnityEngine.SerializeField] private List<T> _list;

		public List<T> list { get { return _list; } }

		public GraphNodeDataListWrapper()
		{
			_list = new List<T>();
		}

		public GraphNodeDataListWrapper(List<T> list)
		{
			_list = list;
		}

		public GraphNodeDataListWrapper(IEnumerable<T> data)
		{
			_list = new List<T>(data);
		}

		public GraphNodeDataListWrapper(int capacity)
		{
			_list = new List<T>(capacity);
		}

		public T this[int i]
		{
			get { return _list[i]; }
			set { _list[i] = value; }
		}

		public T this[GraphNode n]
		{
			get { return _list[n.index]; }
			set { _list[n.index] = value; }
		}

		public T this[GraphEdge e]
		{
			get { return _list[e.node.index]; }
			set { _list[e.node.index] = value; }
		}

		public int Count { get { return _list.Count; } }

		public bool IsReadOnly { get { return ((IList<T>)_list).IsReadOnly; } }

		public void Add(T item) { _list.Add(item); }

		public void Clear() { _list.Clear(); }

		public bool Contains(T item) { return _list.Contains(item); }

		public void CopyTo(T[] array, int arrayIndex) { _list.CopyTo(array, arrayIndex); }

		public IEnumerator<T> GetEnumerator() { return _list.GetEnumerator(); }

		public int IndexOf(T item) { return _list.IndexOf(item); }

		public void Insert(int index, T item) { _list.Insert(index, item); }

		public bool Remove(T item) { return _list.Remove(item); }

		public void RemoveAt(int index) { _list.RemoveAt(index); }

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }

		T IGraphNodeData<T>.this[int i]
		{
			get { return _list[i]; }
			set { _list[i] = value; }
		}

		T IGraphEdgeData<T>.this[int i]
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
	}
}
