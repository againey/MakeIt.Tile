/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using UnityEngine;
using System;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public partial class Topology
	{
		/// <summary>
		/// A wrapper for conveniently working with a topology vertex, providing access to its core properties and enumeration of its neighbors.
		/// </summary>
		public struct Vertex : IEquatable<Vertex>, IComparable<Vertex>
		{
			private Topology _topology;
			private int _index;

			public Vertex(Topology topology, int index)
			{
				_topology = topology;
				_index = index;
			}

			public Topology topology { get { return _topology; } }

			public int index { get { return _index; } }
			public int neighborCount { get { return _topology.vertexNeighborCounts[_index]; } }
			public VertexEdge firstEdge { get { return new VertexEdge(_topology, _topology.vertexFirstEdgeIndices[_index]); } }

			public static implicit operator bool(Vertex vertex) { return vertex._topology != null; }

			public bool hasExternalFaceNeighbor
			{
				get
				{
					foreach (var edge in edges)
						if (edge.prevFace.isExternal)
							return true;
					return false;
				}
			}

			public struct VertexEdgesIndexer
			{
				private Topology _topology;
				private int _index;

				public VertexEdgesIndexer(Topology topology, int index)
				{
					_topology = topology;
					_index = index;
				}

				public int Count { get { return _topology.vertexNeighborCounts[_index]; } }
				
				public struct VertexEdgeEnumerator
				{
					private Topology _topology;
					private int _firstEdgeIndex;
					private int _currentEdgeIndex;
					private int _nextEdgeIndex;

					public VertexEdgeEnumerator(Topology topology, int firstEdgeIndex)
					{
						_topology = topology;
						_firstEdgeIndex = firstEdgeIndex;
						_currentEdgeIndex = -1;
						_nextEdgeIndex = firstEdgeIndex;
					}

					public VertexEdge Current { get { return new VertexEdge(_topology, _currentEdgeIndex); } }

					public bool MoveNext()
					{
						if (_currentEdgeIndex == -1 || _nextEdgeIndex != _firstEdgeIndex)
						{
							_currentEdgeIndex = _nextEdgeIndex;
							_nextEdgeIndex = _topology.edgeData[_currentEdgeIndex].vNext;
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

				public VertexEdgeEnumerator GetEnumerator()
				{
					return new VertexEdgeEnumerator(_topology, _topology.vertexFirstEdgeIndices[_index]);
				}
			}

			public VertexEdgesIndexer edges { get { return new VertexEdgesIndexer(_topology, _index); } }

			public VertexEdge FindEdge(Vertex vertex)
			{
				VertexEdge edge;
				if (!TryFindEdge(vertex, out edge)) throw new InvalidOperationException("The specified vertex is not a neighbor of this vertex.");
				return edge;
			}

			public VertexEdge FindEdge(Face face)
			{
				VertexEdge neighbor;
				if (!TryFindEdge(face, out neighbor)) throw new InvalidOperationException("The specified face is not a neighbor of this vertex.");
				return neighbor;
			}

			public bool TryFindEdge(Vertex vertex, out VertexEdge edge)
			{
				foreach (var vertexEdge in edges)
				{
					if (vertexEdge.farVertex == vertex)
					{
						edge = vertexEdge;
						return true;
					}
				}
				edge = new VertexEdge();
				return false;
			}

			public bool TryFindEdge(Face face, out VertexEdge edge)
			{
				foreach (var vertexEdge in edges)
				{
					if (vertexEdge.prevFace == face)
					{
						edge = vertexEdge;
						return true;
					}
				}
				edge = new VertexEdge();
				return false;
			}

			public override bool Equals(object other) { return other is Vertex && _index == ((Vertex)other)._index; }
			public bool Equals(Vertex other) { return _index == other._index; }
			public int CompareTo(Vertex other) { return _index - other._index; }
			public static bool operator ==(Vertex lhs, Vertex rhs) { return lhs._index == rhs._index; }
			public static bool operator !=(Vertex lhs, Vertex rhs) { return lhs._index != rhs._index; }
			public static bool operator < (Vertex lhs, Vertex rhs) { return lhs._index <  rhs._index; }
			public static bool operator > (Vertex lhs, Vertex rhs) { return lhs._index >  rhs._index; }
			public static bool operator <=(Vertex lhs, Vertex rhs) { return lhs._index <= rhs._index; }
			public static bool operator >=(Vertex lhs, Vertex rhs) { return lhs._index >= rhs._index; }
			public override int GetHashCode() { return _index.GetHashCode(); }

			public override string ToString()
			{
				var sb = new System.Text.StringBuilder();
				sb.AppendFormat("Vertex {0} (", _index);
				foreach (var edge in edges)
					sb.AppendFormat(edge.next != firstEdge ? "{0}, " : "{0}), (", edge.farVertex.index);
				foreach (var edge in edges)
					sb.AppendFormat(edge.next != firstEdge ? "{0}, " : "{0})", edge.prevFace.index);
				return sb.ToString();
			}
		}

		public struct VerticesIndexer
		{
			private Topology _topology;

			public VerticesIndexer(Topology topology) { _topology = topology; }
			public Vertex this[int i] { get { return new Vertex(_topology, i); } }
			public int Count { get { return _topology.vertexFirstEdgeIndices.Length; } }
			public VertexEnumerator GetEnumerator() { return new VertexEnumerator(_topology); }

			public struct VertexEnumerator
			{
				private Topology _topology;
				private int _current;

				public VertexEnumerator(Topology topology) { _topology = topology; _current = -1; }
				public Vertex Current { get { return new Vertex(_topology, _current); } }
				public bool MoveNext() { return ++_current < _topology.vertexFirstEdgeIndices.Length; }
				public void Reset() { throw new NotSupportedException(); }
			}
		}

		public VerticesIndexer vertices { get { return new VerticesIndexer(this); } }
	}

	/// <summary>
	/// Generic interface for accessing attribute values of topology vertices.
	/// </summary>
	/// <typeparam name="T">The type of the attribute values.</typeparam>
	/// <remarks>
	/// <para>Instead of working with integer indices everywhere, this interface allows attributes to be
	/// indexed by instances of the Vertex structure directly.</para>
	/// 
	/// <para>The three indexers that take an edge as an index permit the possibility of altering the vertex attribute
	/// lookup dependent upon the context of how the vertex is being accessed.  For most implementations, these three
	/// indexers are expected to simply defer to the primary indexer using the far/next vertex of the edge.</para>
	/// </remarks>
	public interface IVertexAttribute<T> : IList<T>
	{
		/// <summary>
		/// Lookup the attribute value for the vertex indicated.
		/// </summary>
		/// <param name="v">The vertex whose attribute value is desired.</param>
		/// <returns>The attribute value for the vertex indicated.</returns>
		T this[Topology.Vertex v] { get; set; }

		/// <summary>
		/// Lookup the attribute value for a vertex relative to an edge.
		/// </summary>
		/// <param name="e">The edge that points at the vertex whose attribute value is desired.</param>
		/// <returns>The attribute value for the far vertex of the indicated edge, relative to that edge.</returns>
		T this[Topology.HalfEdge e] { get; set; }

		/// <summary>
		/// Lookup the attribute value for a vertex relative to a neighboring vertex.
		/// </summary>
		/// <param name="e">The edge that points at the vertex whose attribute value is desired.</param>
		/// <returns>The attribute value for the far vertex of the indicated edge, relative to that edge's near vertex.</returns>
		T this[Topology.VertexEdge e] { get; set; }

		/// <summary>
		/// Lookup the attribute value for a vertex relative to a neighboring face.
		/// </summary>
		/// <param name="e">The edge that points at the vertex whose attribute value is desired.</param>
		/// <returns>The attribute value for the next vertex of the indicated edge, relative to that edge's near face.</returns>
		T this[Topology.FaceEdge e] { get; set; }
	}

	public struct VertexAttributeArrayWrapper<T> : IVertexAttribute<T>
	{
		public T[] array;

		public VertexAttributeArrayWrapper(T[] array)
		{
			this.array = array;
		}

		public VertexAttributeArrayWrapper(int elementCount)
		{
			array = new T[elementCount];
		}

		public T this[int i]
		{
			get { return array[i]; }
			set { array[i] = value; }
		}

		public T this[Topology.Vertex v]
		{
			get { return array[v.index]; }
			set { array[v.index] = value; }
		}

		public T this[Topology.HalfEdge e]
		{
			get { return array[e.farVertex.index]; }
			set { array[e.farVertex.index] = value; }
		}

		public T this[Topology.VertexEdge e]
		{
			get { return array[e.farVertex.index]; }
			set { array[e.farVertex.index] = value; }
		}

		public T this[Topology.FaceEdge e]
		{
			get { return array[e.nextVertex.index]; }
			set { array[e.nextVertex.index] = value; }
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

	public abstract class VertexAttribute<T> : ScriptableObject, IVertexAttribute<T>
	{
		public abstract T this[int i] { get; set; }
		public abstract T this[Topology.Vertex v] { get; set; }
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
	}

	public class VertexConstantAttribute<T> : VertexAttribute<T> where T : new()
	{
		public T constant;

		protected static TDerived CreateDerived<TDerived>(T constant) where TDerived : VertexConstantAttribute<T>
		{
			var instance = CreateInstance<TDerived>();
			instance.constant = constant;
			return instance;
		}

		public override T this[int i]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant vertex attribute cannot be changed."); }
		}

		public override T this[Topology.Vertex v]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant vertex attribute cannot be changed."); }
		}

		public override T this[Topology.HalfEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant vertex attribute cannot be changed."); }
		}

		public override T this[Topology.VertexEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant vertex attribute cannot be changed."); }
		}

		public override T this[Topology.FaceEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant vertex attribute cannot be changed."); }
		}
	}

	public class VertexArrayAttribute<T> : VertexAttribute<T> where T : new()
	{
		public T[] array;

		protected static TDerived CreateDerived<TDerived>(T[] array) where TDerived : VertexArrayAttribute<T>
		{
			var attribute = CreateInstance<TDerived>();
			attribute.array = array;
			return attribute;
		}

		protected static TDerived CreateDerived<TDerived>(int vertexCount) where TDerived : VertexArrayAttribute<T>
		{
			return CreateDerived<TDerived>(new T[vertexCount]);
		}

		public override T this[int i]
		{
			get { return array[i]; }
			set { array[i] = value; }
		}

		public override T this[Topology.Vertex v]
		{
			get { return array[v.index]; }
			set { array[v.index] = value; }
		}

		public override T this[Topology.HalfEdge e]
		{
			get { return array[e.farVertex.index]; }
			set { array[e.farVertex.index] = value; }
		}

		public override T this[Topology.VertexEdge e]
		{
			get { return array[e.farVertex.index]; }
			set { array[e.farVertex.index] = value; }
		}

		public override T this[Topology.FaceEdge e]
		{
			get { return array[e.nextVertex.index]; }
			set { array[e.nextVertex.index] = value; }
		}

		public override int Count { get { return array.Length; } }
		public override bool Contains(T item) { return ((IList<T>)array).Contains(item); }
		public override void CopyTo(T[] array, int arrayIndex) { this.array.CopyTo(array, arrayIndex); }
		public override IEnumerator<T> GetEnumerator() { return ((IList<T>)array).GetEnumerator(); }
		public override int IndexOf(T item) { return ((IList<T>)array).IndexOf(item); }
	}

	public static class VertexExtensions
	{
		public static VertexAttributeArrayWrapper<T> AsVertexAttribute<T>(this T[] array)
		{
			return new VertexAttributeArrayWrapper<T>(array);
		}
	}
}
