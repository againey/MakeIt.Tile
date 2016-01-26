using UnityEngine;
using System;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public partial class Topology
	{
		public struct Face : IEquatable<Face>, IComparable<Face>
		{
			private Topology _topology;
			private int _index;

			public Face(Topology topology, int index)
			{
				_topology = topology;
				_index = index;
			}

			public Topology topology { get { return _topology; } }

			public int index { get { return _index; } }
			public bool isInternal { get { return _index < _topology.firstExternalFaceIndex; } }
			public bool isExternal { get { return _index >= _topology.firstExternalFaceIndex; } }
			public int neighborCount { get { return (int)_topology.faceNeighborCounts[_index]; } }
			public FaceEdge firstEdge { get { return new FaceEdge(_topology, _topology.faceFirstEdgeIndices[_index]); } }

			public bool isInitialized { get { return _topology != null; } }

			public bool hasExternalFaceNeighbor
			{
				get
				{
					foreach (var edge in edges)
						if (edge.farFace.isExternal)
							return true;
					return false;
				}
			}

			public T Attribute<T>(T[] attributeArray)
			{
				return attributeArray[_index];
			}

			public struct FaceEdgesIndexer
			{
				private Topology _topology;
				private int _index;

				public FaceEdgesIndexer(Topology topology, int index)
				{
					_topology = topology;
					_index = index;
				}

				public int Count { get { return (int)(_topology.faceNeighborCounts[_index] & 0x7FFFu); } }
				
				public struct FaceEdgeEnumerator
				{
					private Topology _topology;
					private int _firstEdgeIndex;
					private int _currentEdgeIndex;
					private int _nextEdgeIndex;

					public FaceEdgeEnumerator(Topology topology, int firstEdgeIndex)
					{
						_topology = topology;
						_firstEdgeIndex = firstEdgeIndex;
						_currentEdgeIndex = -1;
						_nextEdgeIndex = firstEdgeIndex;
					}

					public FaceEdge Current { get { return new FaceEdge(_topology, _currentEdgeIndex); } }

					public bool MoveNext()
					{
						if (_currentEdgeIndex == -1 || _nextEdgeIndex != _firstEdgeIndex)
						{
							_currentEdgeIndex = _nextEdgeIndex;
							_nextEdgeIndex = _topology.edgeData[_currentEdgeIndex]._fNext;
							return true;
						}
						else
						{
							return false;
						}
					}

					public void Reset()
					{
						_currentEdgeIndex = -1;
						_nextEdgeIndex = _firstEdgeIndex;
					}
				}

				public FaceEdgeEnumerator GetEnumerator()
				{
					return new FaceEdgeEnumerator(_topology, _topology.faceFirstEdgeIndices[_index]);
				}
			}

			public FaceEdgesIndexer edges { get { return new FaceEdgesIndexer(_topology, _index); } }

			public FaceEdge FindEdge(Vertex vertex)
			{
				FaceEdge neighbor;
				if (!TryFindEdge(vertex, out neighbor)) throw new InvalidOperationException("The specified vertex is not a neighbor of this face.");
				return neighbor;
			}

			public FaceEdge FindEdge(Face face)
			{
				FaceEdge edge;
				if (!TryFindEdge(face, out edge)) throw new InvalidOperationException("The specified face is not a neighbor of this face.");
				return edge;
			}

			public bool TryFindEdge(Vertex vertex, out FaceEdge edge)
			{
				foreach (var faceEdge in edges)
				{
					if (faceEdge.nextVertex == vertex)
					{
						edge = faceEdge;
						return true;
					}
				}
				edge = new FaceEdge();
				return false;
			}

			public bool TryFindEdge(Face face, out FaceEdge edge)
			{
				foreach (var faceEdge in edges)
				{
					if (faceEdge.farFace == face)
					{
						edge = faceEdge;
						return true;
					}
				}
				edge = new FaceEdge();
				return false;
			}

			public override bool Equals(object other) { return other is Face && _index == ((Face)other)._index; }
			public bool Equals(Face other) { return _index == other._index; }
			public int CompareTo(Face other) { return _index - other._index; }
			public static bool operator ==(Face lhs, Face rhs) { return lhs._index == rhs._index; }
			public static bool operator !=(Face lhs, Face rhs) { return lhs._index != rhs._index; }
			public static bool operator < (Face lhs, Face rhs) { return lhs._index <  rhs._index; }
			public static bool operator > (Face lhs, Face rhs) { return lhs._index >  rhs._index; }
			public static bool operator <=(Face lhs, Face rhs) { return lhs._index <= rhs._index; }
			public static bool operator >=(Face lhs, Face rhs) { return lhs._index >= rhs._index; }
			public override int GetHashCode() { return _index.GetHashCode(); }

			public override string ToString()
			{
				var sb = new System.Text.StringBuilder();
				sb.AppendFormat("Face {0} (", _index);
				foreach (var edge in edges)
					sb.AppendFormat(edge.next != firstEdge ? "{0}, " : "{0}), (", edge.farFace.index);
				foreach (var edge in edges)
					sb.AppendFormat(edge.next != firstEdge ? "{0}, " : "{0})", edge.nextVertex.index);
				return sb.ToString();
			}
		}

		public struct FacesIndexer : IList<Face>
		{
			private Topology _topology;
			private int _first;
			private int _last;

			public FacesIndexer(Topology topology) { _topology = topology; _first = 0; _last = _topology.faceFirstEdgeIndices.Length; }
			public FacesIndexer(Topology topology, int first, int last) { _topology = topology; _first = first; _last = last; }
			public Face this[int i] { get { return new Face(_topology, _first + i); } set { throw new NotSupportedException(); } }
			public int Count { get { return _last - _first; } }
			public int IndexOf(Face item) { return Contains(item) ? item.index - _first : -1; }
			public bool Contains(Face item) { return item.topology == _topology && item.index >= _first && item.index < _last; }
			public bool IsReadOnly { get { return true; } }

			public IEnumerator<Face> GetEnumerator()
			{
				for (int faceIndex = _first; faceIndex < _last; ++faceIndex)
				{
					yield return new Face(_topology, faceIndex);
				}
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public void CopyTo(Face[] array, int arrayIndex)
			{
				int offset = arrayIndex - _first;
				for (int i = _first; i < _last; ++i)
				{
					array[i + offset] = new Face(_topology, i);
				}
			}

			public void Add(Face item) { throw new NotSupportedException(); }
			public void Insert(int index, Face item) { throw new NotSupportedException(); }
			public bool Remove(Face item) { throw new NotSupportedException(); }
			public void RemoveAt(int index) { throw new NotSupportedException(); }
			public void Clear() { throw new NotSupportedException(); }
		}

		public FacesIndexer faces { get { return new FacesIndexer(this); } }
		public FacesIndexer internalFaces { get { return new FacesIndexer(this, 0, firstExternalFaceIndex); } }
		public FacesIndexer externalFaces { get { return new FacesIndexer(this, firstExternalFaceIndex, faceFirstEdgeIndices.Length); } }
	}

	public interface IFaceAttribute<T> : IList<T>
	{
		T this[Topology.Face f] { get; set; }
		T this[Topology.VertexEdge e] { get; set; }
		T this[Topology.FaceEdge e] { get; set; }
	}

	public struct FaceAttributeArrayWrapper<T> : IFaceAttribute<T>
	{
		public T[] array;

		public FaceAttributeArrayWrapper(T[] array)
		{
			this.array = array;
		}

		public FaceAttributeArrayWrapper(int elementCount)
		{
			array = new T[elementCount];
		}

		public T this[int i]
		{
			get { return array[i]; }
			set { array[i] = value; }
		}

		public T this[Topology.Face f]
		{
			get { return array[f.index]; }
			set { array[f.index] = value; }
		}

		public T this[Topology.VertexEdge e]
		{
			get { return array[e.prevFace.index]; }
			set { array[e.prevFace.index] = value; }
		}

		public T this[Topology.FaceEdge e]
		{
			get { return array[e.farFace.index]; }
			set { array[e.farFace.index] = value; }
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

	public abstract class FaceAttribute<T> : ScriptableObject, IFaceAttribute<T>
	{
		public abstract T this[int i] { get; set; }
		public abstract T this[Topology.Face f] { get; set; }
		public abstract T this[Topology.VertexEdge e] { get; set; }
		public abstract T this[Topology.FaceEdge e] { get; set; }

		public virtual int Count { get { throw new NotSupportedException(); } }
		public virtual bool IsReadOnly { get { return true; } }
		public virtual void Add(T item) { throw new NotSupportedException(); }
		public virtual void Clear() { throw new NotSupportedException(); }
		public virtual bool Contains(T item) { throw new NotSupportedException(); }
		public virtual void CopyTo(T[] array, int arrayIndex) { throw new NotSupportedException(); }
		public virtual IEnumerator<T> GetEnumerator() { throw new NotSupportedException(); }
		public virtual int IndexOf(T item) { throw new NotSupportedException(); }
		public virtual void Insert(int index, T item) { throw new NotSupportedException(); }
		public virtual bool Remove(T item) { throw new NotSupportedException(); }
		public virtual void RemoveAt(int index) { throw new NotSupportedException(); }
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	public class FaceConstantAttribute<T> : FaceAttribute<T> where T : new()
	{
		public T constant;

		protected static TDerived CreateDerivedInstance<TDerived>() where TDerived : FaceConstantAttribute<T>
		{
			return CreateInstance<TDerived>();
		}

		protected static TDerived CreateDerivedInstance<TDerived>(T constant) where TDerived : FaceConstantAttribute<T>
		{
			var instance = CreateInstance<TDerived>();
			instance.constant = constant;
			return instance;
		}

		protected static TDerived CreateDerivedInstance<TDerived>(T constant, string name) where TDerived : FaceConstantAttribute<T>
		{
			var instance = CreateInstance<TDerived>();
			instance.constant = constant;
			instance.name = name;
			return instance;
		}

		protected static TDerived CreateDerivedInstance<TDerived>(string name) where TDerived : FaceConstantAttribute<T>
		{
			var instance = CreateInstance<TDerived>();
			instance.name = name;
			return instance;
		}

		protected TDerived CloneDerived<TDerived>() where TDerived : FaceConstantAttribute<T>
		{
			var clone = CreateInstance<TDerived>();
			clone.constant = constant;
			clone.name = name;
			clone.hideFlags = hideFlags;
			return clone;
		}

		public override T this[int i]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant face attribute cannot be changed."); }
		}

		public override T this[Topology.Face f]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant face attribute cannot be changed."); }
		}

		public override T this[Topology.VertexEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant face attribute cannot be changed."); }
		}

		public override T this[Topology.FaceEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant face attribute cannot be changed."); }
		}
	}

	public class FaceArrayAttribute<T> : FaceAttribute<T> where T : new()
	{
		public T[] array;

		protected static TDerived CreateDerivedInstance<TDerived>() where TDerived : FaceArrayAttribute<T>
		{
			return CreateInstance<TDerived>();
		}

		protected static TDerived CreateDerivedInstance<TDerived>(T[] array) where TDerived : FaceArrayAttribute<T>
		{
			var instance = CreateInstance<TDerived>();
			instance.array = array;
			return instance;
		}

		protected static TDerived CreateDerivedInstance<TDerived>(T[] array, string name) where TDerived : FaceArrayAttribute<T>
		{
			var instance = CreateInstance<TDerived>();
			instance.array = array;
			instance.name = name;
			return instance;
		}

		protected static TDerived CreateDerivedInstance<TDerived>(string name) where TDerived : FaceArrayAttribute<T>
		{
			var instance = CreateInstance<TDerived>();
			instance.name = name;
			return instance;
		}

		protected TDerived CloneDerived<TDerived>() where TDerived : FaceArrayAttribute<T>
		{
			var clone = CreateInstance<TDerived>();
			clone.array = (T[])array.Clone();
			clone.name = name;
			clone.hideFlags = hideFlags;
			return clone;
		}

		public override T this[int i]
		{
			get { return array[i]; }
			set { array[i] = value; }
		}

		public override T this[Topology.Face f]
		{
			get { return array[f.index]; }
			set { array[f.index] = value; }
		}

		public override T this[Topology.VertexEdge e]
		{
			get { return array[e.prevFace.index]; }
			set { array[e.prevFace.index] = value; }
		}

		public override T this[Topology.FaceEdge e]
		{
			get { return array[e.farFace.index]; }
			set { array[e.farFace.index] = value; }
		}

		public override int Count { get { return array.Length; } }
		public override bool Contains(T item) { return ((IList<T>)array).Contains(item); }
		public override void CopyTo(T[] array, int arrayIndex) { this.array.CopyTo(array, arrayIndex); }
		public override IEnumerator<T> GetEnumerator() { return ((IList<T>)array).GetEnumerator(); }
		public override int IndexOf(T item) { return ((IList<T>)array).IndexOf(item); }
	}

	public abstract class FaceGroupLookupFaceAttribute<T, TFaceGroupAttribute> : FaceAttribute<T> where TFaceGroupAttribute : FaceGroupAttribute<T>
	{
		public IntFaceAttribute faceGroupIndices;
		public TFaceGroupAttribute faceGroupData;

		public T GetAttribute(Topology.Face face)
		{
			return faceGroupData[faceGroupIndices[face]];
		}

		public override T this[int i]
		{
			get { return faceGroupData[faceGroupIndices[i]]; }
			set { throw new System.NotSupportedException("A face group lookup face attribute is read only and cannot be modified."); }
		}

		public override T this[Topology.Face f]
		{
			get { return faceGroupData[faceGroupIndices[f]]; }
			set { throw new System.NotSupportedException("A face group lookup face attribute is read only and cannot be modified."); }
		}

		public override T this[Topology.VertexEdge e]
		{
			get { return faceGroupData[faceGroupIndices[e.prevFace]]; }
			set { throw new System.NotSupportedException("A face group lookup face attribute is read only and cannot be modified."); }
		}

		public override T this[Topology.FaceEdge e]
		{
			get { return faceGroupData[faceGroupIndices[e.farFace]]; }
			set { throw new System.NotSupportedException("A face group lookup face attribute is read only and cannot be modified."); }
		}
	}

	public static class FaceExtensions
	{
		public static FaceAttributeArrayWrapper<T> AsFaceAttribute<T>(this T[] array)
		{
			return new FaceAttributeArrayWrapper<T>(array);
		}
	}
}
