/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System;
using System.Collections.Generic;

namespace Experilous.Topologies
{
	public interface ITopologyFaceData<T> : ITopologyEdgeData<T>
	{
		new T this[int i] { get; set; }
		T this[TopologyFace n] { get; set; }
	}

	public struct TopologyFaceDataArray<T> : ITopologyFaceData<T>
	{
		public readonly T[] array;

		public TopologyFaceDataArray(T[] array)
		{
			this.array = array;
		}

		public TopologyFaceDataArray(int elementCount)
		{
			array = new T[elementCount];
		}

		public static implicit operator T[](TopologyFaceDataArray<T> data)
		{
			return data.array;
		}

		public static implicit operator TopologyFaceDataArray<T>(T[] array)
		{
			return new TopologyFaceDataArray<T>(array);
		}

		public T this[int i]
		{
			get { return array[i]; }
			set { array[i] = value; }
		}

		public T this[TopologyFace f]
		{
			get { return array[f.index]; }
			set { array[f.index] = value; }
		}

		public T this[TopologyEdge e]
		{
			get { return array[e.face.index]; }
			set { array[e.face.index] = value; }
		}

		public T this[TopologyNodeEdge e]
		{
			get { return array[e.prevFace.index]; }
			set { array[e.prevFace.index] = value; }
		}

		public T this[TopologyFaceEdge e]
		{
			get { return array[e.face.index]; }
			set { array[e.face.index] = value; }
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

		T ITopologyFaceData<T>.this[int i]
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

	public class TopologyFaceDataList<T> : List<T>, ITopologyFaceData<T>
	{
		public TopologyFaceDataList()
		{
		}

		public TopologyFaceDataList(T[] array)
			: base(array)
		{
		}

		public TopologyFaceDataList(IEnumerable<T> data)
			: base(data)
		{
		}

		public TopologyFaceDataList(int capacity)
			: base(capacity)
		{
		}

		public T this[TopologyFace f]
		{
			get { return this[f.index]; }
			set { this[f.index] = value; }
		}

		public T this[TopologyEdge e]
		{
			get { return this[e.face.index]; }
			set { this[e.face.index] = value; }
		}

		public T this[TopologyNodeEdge e]
		{
			get { return this[e.prevFace.index]; }
			set { this[e.prevFace.index] = value; }
		}

		public T this[TopologyFaceEdge e]
		{
			get { return this[e.face.index]; }
			set { this[e.face.index] = value; }
		}

		T ITopologyFaceData<T>.this[int i]
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
