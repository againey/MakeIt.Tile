﻿/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public partial class Topology
	{
		/// <summary>
		/// A wrapper for conveniently working with a topology face, providing access to its core properties and enumeration of its neighbors.
		/// </summary>
		public struct Face : IEquatable<Face>, IComparable<Face>
		{
			private Topology _topology;
			private int _index;

			public Face(Topology topology, int index)
			{
				_topology = topology;
				_index = index;
			}

			public static Face none { get { return new Face(); } }

			public Topology topology { get { return _topology; } }

			public int index { get { return _index; } }
			public bool isInternal { get { return _index < _topology.firstExternalFaceIndex; } }
			public bool isExternal { get { return _index >= _topology.firstExternalFaceIndex; } }
			public int neighborCount { get { return (int)_topology.faceNeighborCounts[_index]; } }
			public FaceEdge firstEdge { get { return new FaceEdge(_topology, _topology.faceFirstEdgeIndices[_index]); } }

			public static implicit operator bool(Face face) { return face._topology != null; }

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
						if (_nextEdgeIndex != _firstEdgeIndex || _currentEdgeIndex == -1)
						{
							_currentEdgeIndex = _nextEdgeIndex;
							_nextEdgeIndex = _topology.edgeData[_currentEdgeIndex].fNext;
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

			public struct ExtendedFaceEdgesIndexer
			{
				private Topology _topology;
				private int _index;

				public ExtendedFaceEdgesIndexer(Topology topology, int index)
				{
					_topology = topology;
					_index = index;
				}

				public struct ExtendedFaceEdgeEnumerator
				{
					private Topology _topology;
					private int _firstEdgeIndex;
					private int _currentExtendedEdgeIndex;
					private int _nextExtendedEdgeIndex;
					private int _nextEdgeIndex;

					public ExtendedFaceEdgeEnumerator(Topology topology, int firstEdgeIndex)
					{
						_topology = topology;
						_firstEdgeIndex = firstEdgeIndex;
						_currentExtendedEdgeIndex = -1;
						_nextExtendedEdgeIndex = firstEdgeIndex;
						_nextEdgeIndex = firstEdgeIndex;
					}

					public FaceEdge Current { get { return new FaceEdge(_topology, _currentExtendedEdgeIndex); } }

					public bool MoveNext()
					{
						if (_nextExtendedEdgeIndex != _nextEdgeIndex)
						{
							_currentExtendedEdgeIndex = _nextExtendedEdgeIndex;
							_nextExtendedEdgeIndex = _topology.edgeData[_currentExtendedEdgeIndex].vNext;
							return true;
						}
						else if (_nextEdgeIndex != _firstEdgeIndex || _currentExtendedEdgeIndex == -1)
						{
							do
							{
								_currentExtendedEdgeIndex = _topology.edgeData[_topology.edgeData[_nextEdgeIndex].twin].vNext;
								_nextEdgeIndex = _topology.edgeData[_nextEdgeIndex].fNext;
								if (_currentExtendedEdgeIndex == _firstEdgeIndex)
								{
									_nextExtendedEdgeIndex = _nextEdgeIndex = _firstEdgeIndex;
									return false;
								}
							} while (_currentExtendedEdgeIndex == _nextEdgeIndex);
							_nextExtendedEdgeIndex = _topology.edgeData[_currentExtendedEdgeIndex].vNext;
							return true;
						}
						else
						{
							return false;
						}
					}

					public void Reset()
					{
						_currentExtendedEdgeIndex = -1;
						_nextExtendedEdgeIndex = -1;
						_nextEdgeIndex = _firstEdgeIndex;
					}
				}

				public ExtendedFaceEdgeEnumerator GetEnumerator()
				{
					return new ExtendedFaceEdgeEnumerator(_topology, _topology.faceFirstEdgeIndices[_index]);
				}
			}

			public ExtendedFaceEdgesIndexer extendedEdges { get { return new ExtendedFaceEdgesIndexer(_topology, _index); } }

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

	/// <summary>
	/// Generic interface for accessing attribute values of topology faces.
	/// </summary>
	/// <typeparam name="T">The type of the attribute values.</typeparam>
	/// <remarks>
	/// <para>Instead of working with integer indices everywhere, this interface allows attributes to be
	/// indexed by instances of the Face structure directly.</para>
	/// 
	/// <para>The three indexers that take an edge as an index permit the possibility of altering the face attribute
	/// lookup dependent upon the context of how the face is being accessed.  For most implementations, these three
	/// indexers are expected to simply defer to the primary indexer using the far/prev face of the edge.</para>
	/// </remarks>
	public interface IFaceAttribute<T> : IEdgeAttribute<T>
	{
		/// <summary>
		/// Lookup the attribute value for the face indicated.
		/// </summary>
		/// <param name="i">The index of the face whose attribute value is desired.</param>
		/// <returns>The attribute value for the face indicated.</returns>
		new T this[int i] { get; set; }

		/// <summary>
		/// Lookup the attribute value for the face indicated.
		/// </summary>
		/// <param name="f">The face whose attribute value is desired.</param>
		/// <returns>The attribute value for the face indicated.</returns>
		T this[Topology.Face f] { get; set; }

		/// <summary>
		/// Lookup the attribute value for a face relative to an edge.
		/// </summary>
		/// <param name="e">The edge that points at the face whose attribute value is desired.</param>
		/// <returns>The attribute value for the far face of the indicated edge, relative to that edge.</returns>
		//T this[Topology.HalfEdge e] { get; set; }

		/// <summary>
		/// Lookup the attribute value for a face relative to a neighboring vertex.
		/// </summary>
		/// <param name="e">The edge that points at the face whose attribute value is desired.</param>
		/// <returns>The attribute value for the prev face of the indicated edge, relative to that edge's near vertex.</returns>
		//T this[Topology.VertexEdge e] { get; set; }

		/// <summary>
		/// Lookup the attribute value for a face relative to a neighboring face.
		/// </summary>
		/// <param name="e">The edge that points at the face whose attribute value is desired.</param>
		/// <returns>The attribute value for the far face of the indicated edge, relative to that edge's near face.</returns>
		//T this[Topology.FaceEdge e] { get; set; }
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

		public T this[Topology.HalfEdge e]
		{
			get { return array[e.farFace.index]; }
			set { array[e.farFace.index] = value; }
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

		/// <summary>
		/// Lookup the attribute value for the face indicated.
		/// </summary>
		/// <param name="i">The index of the face whose attribute value is desired.</param>
		/// <returns>The attribute value for the face indicated.</returns>
		T IFaceAttribute<T>.this[int i]
		{
			get { return this[i]; }
			set { this[i] = value; }
		}

		/// <summary>
		/// Lookup the attribute value for the edge indicated.
		/// </summary>
		/// <param name="i">The index of the edge whose attribute value is desired.</param>
		/// <returns>The attribute value for the edge indicated.</returns>
		/// <remarks><para>Any code utilizing this indexer through the <c>IEdgeAttribute&lt;T&gt;</c> interface is likely to
		/// be treating the index as an edge index, but without access to the full edge data, nothing useful can be done with
		/// just an edge index.  Therefore, this indexer throws in order to identify any code that attempts to misuse a
		/// face attribute in this way.</para></remarks>
		T IEdgeAttribute<T>.this[int i]
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
	}

	public abstract class FaceAttribute<T> : ScriptableObject, IFaceAttribute<T>
	{
		public abstract T this[int i] { get; set; }
		public abstract T this[Topology.Face f] { get; set; }
		public abstract T this[Topology.HalfEdge e] { get; set; }
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

		/// <summary>
		/// Lookup the attribute value for the face indicated.
		/// </summary>
		/// <param name="i">The index of the face whose attribute value is desired.</param>
		/// <returns>The attribute value for the face indicated.</returns>
		T IFaceAttribute<T>.this[int i]
		{
			get { return this[i]; }
			set { this[i] = value; }
		}

		/// <summary>
		/// Lookup the attribute value for the edge indicated.
		/// </summary>
		/// <param name="i">The index of the edge whose attribute value is desired.</param>
		/// <returns>The attribute value for the edge indicated.</returns>
		/// <remarks><para>Any code utilizing this indexer through the <c>IEdgeAttribute&lt;T&gt;</c> interface is likely to
		/// be treating the index as an edge index, but without access to the full edge data, nothing useful can be done with
		/// just an edge index.  Therefore, this indexer throws in order to identify any code that attempts to misuse a
		/// face attribute in this way.</para></remarks>
		T IEdgeAttribute<T>.this[int i]
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
	}

	public class FaceConstantAttribute<T> : FaceAttribute<T> where T : new()
	{
		public T constant;

		protected static TDerived CreateDerived<TDerived>(T constant) where TDerived : FaceConstantAttribute<T>
		{
			var instance = CreateInstance<TDerived>();
			instance.constant = constant;
			return instance;
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

		public override T this[Topology.HalfEdge e]
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

		protected static TDerived CreateDerived<TDerived>(T[] array) where TDerived : FaceArrayAttribute<T>
		{
			var instance = CreateInstance<TDerived>();
			instance.array = array;
			return instance;
		}

		protected static TDerived CreateDerived<TDerived>(int faceCount) where TDerived : FaceArrayAttribute<T>
		{
			return CreateDerived<TDerived>(new T[faceCount]);
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

		public override T this[Topology.HalfEdge e]
		{
			get { return array[e.farFace.index]; }
			set { array[e.farFace.index] = value; }
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

		public override T this[int i]
		{
			get { return faceGroupData[faceGroupIndices[i]]; }
			set { throw new NotSupportedException("A face group lookup face attribute is read only and cannot be modified."); }
		}

		public override T this[Topology.Face f]
		{
			get { return faceGroupData[faceGroupIndices[f]]; }
			set { throw new NotSupportedException("A face group lookup face attribute is read only and cannot be modified."); }
		}

		public override T this[Topology.HalfEdge e]
		{
			get { return faceGroupData[faceGroupIndices[e.farFace]]; }
			set { throw new NotSupportedException("A face group lookup face attribute is read only and cannot be modified."); }
		}

		public override T this[Topology.VertexEdge e]
		{
			get { return faceGroupData[faceGroupIndices[e.prevFace]]; }
			set { throw new NotSupportedException("A face group lookup face attribute is read only and cannot be modified."); }
		}

		public override T this[Topology.FaceEdge e]
		{
			get { return faceGroupData[faceGroupIndices[e.farFace]]; }
			set { throw new NotSupportedException("A face group lookup face attribute is read only and cannot be modified."); }
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
