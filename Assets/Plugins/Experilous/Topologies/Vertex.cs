/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

#if false

using UnityEngine;
using System;
using System.Collections.Generic;

namespace Experilous.Topologies
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

			/// <summary>
			/// Constructs a vertex wrapper for a given topology and vertex index.
			/// </summary>
			/// <param name="topology">The topology containing the vertex.</param>
			/// <param name="index">The index of the vertex.</param>
			public Vertex(Topology topology, int index)
			{
				_topology = topology;
				_index = index;
			}

			/// <summary>
			/// An empty vertex wrapper that does not reference any topology or vertex.
			/// </summary>
			public static Vertex none { get { return new Vertex(); } }

			/// <summary>
			/// The topology to which the current vertex belongs.
			/// </summary>
			public Topology topology { get { return _topology; } }

			/// <summary>
			/// The integer index of the current vertex, used by edge references and access into vertex attribute collections.
			/// </summary>
			public int index { get { return _index; } }

			/// <summary>
			/// The number of neighboring vertices or neighboring faces that a vertex has.
			/// </summary>
			/// <remarks><para>A vertex is guaranteed to have an equal number of neighboring vertices and faces.
			/// So a vertex with three neighbors means that it has three neighboring vertices, and also three
			/// neighboring faces.</para></remarks>
			public int neighborCount { get { return _topology.vertexNeighborCounts[_index]; } }

			/// <summary>
			/// A vertex edge wrapper around the first edge of this vertex, pointing to the vertex's first vertex/face pair of neighbors.
			/// </summary>
			/// <remarks><para>This edge wrapper can be used to manually iterate around the neighbors of the vertex, by
			/// using <see cref="VertexEdge.next"/> repeatedly until coming back around to the first edge.</para></remarks>
			public VertexEdge firstEdge { get { return new VertexEdge(_topology, _topology.vertexFirstEdgeIndices[_index]); } }

			/// <summary>
			/// Checks if this vertex wrapper represents a valid vertex.  Converts to true if so, and false if the vertex wrapper is empty.
			/// </summary>
			/// <param name="vertex">The vertex to check.</param>
			/// <returns>True if the current vertex wrapper represents a valid vertex, and false if the vertex wrapper is empty.</returns>
			/// <seealso cref="none"/>
			public static implicit operator bool(Vertex vertex) { return vertex._topology != null; }

			/// <summary>
			/// Indicates if any of the vertex's neighboring faces are external.
			/// </summary>
			public bool hasExternalFaceNeighbor
			{
				get
				{
					foreach (var edge in edges)
						if (edge.face.isExternal)
							return true;
					return false;
				}
			}

			/// <summary>
			/// An indexer of a vertex's edges, used for enumerating its vertex edges.  Satisfies the concept
			/// of <see cref="IEnumerable{VertexEdge}"/>, enabling it to be used in <c>foreach</c> loops.
			/// </summary>
			public struct VertexEdgesIndexer
			{
				private Topology _topology;
				private int _index;

				/// <summary>
				/// Constructs an indexer for the vertex in the given topology and with the given index.
				/// </summary>
				/// <param name="topology">The topology to which the vertex belongs.</param>
				/// <param name="index">The index of the vertex.</param>
				public VertexEdgesIndexer(Topology topology, int index)
				{
					_topology = topology;
					_index = index;
				}

				/// <summary>
				/// The number of edges around the vertex to be enumerated.
				/// </summary>
				public int Count { get { return _topology.vertexNeighborCounts[_index]; } }
				
				/// <summary>
				/// An enumerator of a vertex's edges.  Satisfies the concept of <see cref="IEnumerator{VertexEdge}"/>,
				/// enabling it to be used in <c>foreach</c> loops.
				/// </summary>
				public struct VertexEdgeEnumerator
				{
					private Topology _topology;
					private int _firstEdgeIndex;
					private int _currentEdgeIndex;
					private int _nextEdgeIndex;

					/// <summary>
					/// Constructs an instance of a vertex edge enumerator, given a topology and the first edge in the cycle.
					/// </summary>
					/// <param name="topology">The topology to which the enumerated vertex edges belong.</param>
					/// <param name="firstEdgeIndex">The index of the first edge within a cycle to enumerate.</param>
					public VertexEdgeEnumerator(Topology topology, int firstEdgeIndex)
					{
						_topology = topology;
						_firstEdgeIndex = firstEdgeIndex;
						_currentEdgeIndex = -1;
						_nextEdgeIndex = firstEdgeIndex;
					}

					/// <summary>
					/// A vertex edge wrapper around the current edge being enumerated.
					/// </summary>
					public VertexEdge Current { get { return new VertexEdge(_topology, _currentEdgeIndex); } }

					/// <summary>
					/// Updates the enumerator to the next edge in the cycle around the near vertex.
					/// </summary>
					/// <returns>True if it moved to the next valid edge, or false if there are no more edges to be enumerated.</returns>
					public bool MoveNext()
					{
						if (_nextEdgeIndex != _firstEdgeIndex || _currentEdgeIndex == -1)
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

					/// <summary>
					/// Resets the enumerator back to its original state, so that another call to <see cref="MoveNext"/>
					/// will have <see cref="Current"/> return the first vertex edge of the enumerated sequence.
					/// </summary>
					public void Reset()
					{
						_currentEdgeIndex = -1;
						_nextEdgeIndex = _firstEdgeIndex;
					}
				}

				/// <summary>
				/// Creates an enumerator for the vertex edges of the current indexer.
				/// </summary>
				/// <returns>An enumerator for the vertex edges of the current indexer.</returns>
				public VertexEdgeEnumerator GetEnumerator()
				{
					return new VertexEdgeEnumerator(_topology, _topology.vertexFirstEdgeIndices[_index]);
				}
			}

			/// <summary>
			/// Returns an indexer of all the vertex edges around the current vertex, which all have the
			/// current vertex as their near vertex.  Can be used in <c>foreach</c> loops.
			/// </summary>
			/// <seealso cref="enumerableEdges"/>
			public VertexEdgesIndexer edges { get { return new VertexEdgesIndexer(_topology, _index); } }

			/// <summary>
			/// Returns an enumerable sequence of all the vertex edges around the current vertex, which have
			/// the current vertex as their near vertex.  Implements <see cref="IEnumerable{VertexEdge}"/>,
			/// for circumstances where the actual interface and not just the concept is needed.
			/// </summary>
			/// <seealso cref="edges"/>
			public IEnumerable<VertexEdge> enumerableEdges
			{
				get
				{
					foreach (var edge in edges)
					{
						yield return edge;
					}
				}
			}

			/// <summary>
			/// An indexer of a vertex's outer edges, used for enumerating those edges.  Satisfies the concept
			/// of <see cref="IEnumerable{VertexEdge}"/>, enabling it to be used in <c>foreach</c> loops.
			/// </summary>
			/// <remarks><para>A vertex's outer edges provide convenient enumeration of all the edges that
			/// share a face with the vertex but do not come directly from the vertex.  This is particularly useful
			/// in the situation where the concept of a vertex's neighbors includes every other  vertex that is
			/// exactly one face away from the vertex, such as the case with square grids which allow diagonal
			/// movement, where each vertex is treated as though it has eight neighbors.</para></remarks>
			public struct OuterEdgesIndexer
			{
				private Topology _topology;
				private int _index;

				/// <summary>
				/// Constructs an indexer for the vertex in the given topology and with the given index.
				/// </summary>
				/// <param name="topology">The topology to which the vertex belongs.</param>
				/// <param name="index">The index of the vertex.</param>
				public OuterEdgesIndexer(Topology topology, int index)
				{
					_topology = topology;
					_index = index;
				}

				/// <summary>
				/// An enumerator of a vertex's outer edges.  Satisfies the concept of <see cref="IEnumerator{VertexEdge}"/>,
				/// enabling it to be used in <c>foreach</c> loops.
				/// </summary>
				public struct OuterEdgeEnumerator
				{
					private Topology _topology;
					private int _firstVertexEdgeIndex;
					private int _currentVertexEdgeIndex;
					private int _nextFaceEdgeIndex;
					private int _nextVertexEdgeIndex;

					/// <summary>
					/// Constructs an instance of an outer edge enumerator, given a topology and a vertex's first ordinary (non-outer) edge.
					/// </summary>
					/// <param name="topology">The topology to which the enumerated edges belong.</param>
					/// <param name="firstEdgeIndex">The index of the first ordinary (non-outer) edge within a cycle to enumerate.</param>
					public OuterEdgeEnumerator(Topology topology, int firstEdgeIndex)
					{
						_topology = topology;
						_firstVertexEdgeIndex = topology.edgeData[firstEdgeIndex].twin;
						_currentVertexEdgeIndex = -1;
						_nextFaceEdgeIndex = _firstVertexEdgeIndex;
						_nextVertexEdgeIndex = _firstVertexEdgeIndex;
					}

					/// <summary>
					/// A vertex edge wrapper around the current outer edge being enumerated.
					/// </summary>
					public VertexEdge Current { get { return new VertexEdge(_topology, _currentVertexEdgeIndex); } }

					/// <summary>
					/// Updates the enumerator to the next outer edge in the cycle around the vertex.
					/// </summary>
					/// <returns>True if it moved to the next valid edge, or false if there are no more edges to be enumerated.</returns>
					public bool MoveNext()
					{
						if (_nextFaceEdgeIndex != _nextVertexEdgeIndex)
						{
							_currentVertexEdgeIndex = _nextFaceEdgeIndex;
							_nextFaceEdgeIndex = _topology.edgeData[_currentVertexEdgeIndex].fNext;
							return true;
						}
						else if (_nextVertexEdgeIndex != _firstVertexEdgeIndex || _currentVertexEdgeIndex == -1)
						{
							do
							{
								var twinIndex = _topology.edgeData[_nextVertexEdgeIndex].twin;
								_currentVertexEdgeIndex = _topology.edgeData[twinIndex].fNext;
								_nextVertexEdgeIndex = _topology.edgeData[_topology.edgeData[twinIndex].vNext].twin;
								if (_currentVertexEdgeIndex == _firstVertexEdgeIndex)
								{
									_nextFaceEdgeIndex = _nextVertexEdgeIndex = _firstVertexEdgeIndex;
									return false;
								}
							} while (_currentVertexEdgeIndex == _nextVertexEdgeIndex);
							_nextFaceEdgeIndex = _topology.edgeData[_currentVertexEdgeIndex].fNext;
							return true;
						}
						else
						{
							return false;
						}
					}

					/// <summary>
					/// Resets the enumerator back to its original state, so that another call to <see cref="MoveNext"/>
					/// will have <see cref="Current"/> return the first outer edge of the enumerated sequence.
					/// </summary>
					public void Reset()
					{
						_currentVertexEdgeIndex = -1;
						_nextFaceEdgeIndex = -1;
						_nextVertexEdgeIndex = _firstVertexEdgeIndex;
					}
				}

				/// <summary>
				/// Creates an enumerator for the outer edges of the current indexer.
				/// </summary>
				/// <returns>An enumerator for the outer edges of the current indexer.</returns>
				public OuterEdgeEnumerator GetEnumerator()
				{
					return new OuterEdgeEnumerator(_topology, _topology.vertexFirstEdgeIndices[_index]);
				}
			}

			/// <summary>
			/// Returns an indexer of all the outer edges around the current vertex, which all share a
			/// near face with the current vertex, but do not have the current vertex as their near vertex.
			/// Can be used in <c>foreach</c> loops.
			/// </summary>
			/// <remarks><para>A vertex's outer edges provide convenient enumeration of all the edges that
			/// share a face with the vertex but do not come directly from the vertex.  This is particularly useful
			/// in the situation where the concept of a vertex's neighbors includes every other  vertex that is
			/// exactly one face away from the vertex, such as the case with square grids which allow diagonal
			/// movement, where each vertex is treated as though it has eight neighbors.</para></remarks>
			public OuterEdgesIndexer outerEdges { get { return new OuterEdgesIndexer(_topology, _index); } }

			/// <summary>
			/// Returns an enumerable sequence of all the outer edges around the current vertex, which all share
			/// a near face with the current vertex, but do not have the current vertex as their near vertex.
			/// Implements <see cref="IEnumerable{FaceEdge}"/>, for circumstances where the actual interface
			/// and not just the concept is needed.
			/// </summary>
			/// <remarks><para>A vertex's outer edges provide convenient enumeration of all the edges that
			/// share a face with the vertex but do not come directly from the vertex.  This is particularly useful
			/// in the situation where the concept of a vertex's neighbors includes every other  vertex that is
			/// exactly one face away from the vertex, such as the case with square grids which allow diagonal
			/// movement, where each vertex is treated as though it has eight neighbors.</para></remarks>
			public IEnumerable<VertexEdge> enumerableOuterEdges
			{
				get
				{
					foreach (var edge in edges)
					{
						yield return edge;
					}
				}
			}

			/// <summary>
			/// Finds the vertex edge around the current vertex that corresponds to the given face.
			/// </summary>
			/// <param name="face">The face for which the corresponding edge is to be found.</param>
			/// <returns>The vertex edge whose face is the given face, or an empty edge if the given face is not a neighbor of the current vertex.</returns>
			public VertexEdge FindEdge(Face face)
			{
				foreach (var vertexEdge in edges)
				{
					if (vertexEdge.face == face)
					{
						return vertexEdge;
					}
				}
				return VertexEdge.none;
			}

			/// <summary>
			/// Finds the vertex edge around the current vertex that points to the given vertex.
			/// </summary>
			/// <param name="vertex">The vertex for which the corresponding edge is to be found.</param>
			/// <returns>The vertex edge whose vertex is the given vertex, or an empty edge if the given vertex is not a neighbor of the current vertex.</returns>
			public VertexEdge FindEdge(Vertex vertex)
			{
				foreach (var vertexEdge in edges)
				{
					if (vertexEdge.vertex == vertex)
					{
						return vertexEdge;
					}
				}
				return VertexEdge.none;
			}

			/// <summary>
			/// Tries to find the vertex edge around the current vertex that corresponds to the given face.
			/// </summary>
			/// <param name="face">The face for which the corresponding edge is to be found.</param>
			/// <param name="edge">The vertex edge whose face is the given face, or an empty edge if the given face is not a neighbor of the current vertex.</param>
			/// <returns>True if the corresponding edge was found, and false if the given face is not a neighbor of the current vertex.</returns>
			public bool TryFindEdge(Face face, out VertexEdge edge)
			{
				return edge = FindEdge(face);
			}

			/// <summary>
			/// Tries to find the vertex edge around the current vertex that points to the given vertex.
			/// </summary>
			/// <param name="vertex">The vertex for which the corresponding edge is to be found.</param>
			/// <param name="edge">The vertex edge whose vertex is the given vertex, or an empty edge if the given vertex is not a neighbor of the current vertex.</param>
			/// <returns>True if the corresponding edge was found, and false if the given vertex is not a neighbor of the current vertex.</returns>
			public bool TryFindEdge(Vertex vertex, out VertexEdge edge)
			{
				return edge = FindEdge(vertex);
			}

			/// <summary>
			/// Finds the outer edge around the current vertex that corresponds to the given face.
			/// </summary>
			/// <param name="face">The face for which the corresponding edge is to be found.</param>
			/// <returns>The outer edge whose face is the given face, or an empty edge if no matching edge is found.</returns>
			public VertexEdge FindOuterEdge(Face face)
			{
				foreach (var vertexEdge in outerEdges)
				{
					if (vertexEdge.face == face)
					{
						return vertexEdge;
					}
				}
				return VertexEdge.none;
			}

			/// <summary>
			/// Finds the outer edge around the current vertex that points to the given vertex.
			/// </summary>
			/// <param name="vertex">The vertex for which the corresponding edge is to be found.</param>
			/// <returns>The outer edge whose vertex is the given vertex, or an empty edge if no matching edge is found.</returns>
			public VertexEdge FindOuterEdge(Vertex vertex)
			{
				foreach (var vertexEdge in outerEdges)
				{
					if (vertexEdge.vertex == vertex)
					{
						return vertexEdge;
					}
				}
				return VertexEdge.none;
			}

			/// <summary>
			/// Tries to find the outer edge around the current vertex that corresponds to the given face.
			/// </summary>
			/// <param name="face">The face for which the corresponding edge is to be found.</param>
			/// <param name="edge">The outer edge whose face is the given face, or an empty edge if no matching edge is found.</param>
			/// <returns>True if the corresponding edge was found, and false if no matching edge is found.</returns>
			public bool TryFindOuterEdge(Face face, out VertexEdge edge)
			{
				return edge = FindOuterEdge(face);
			}

			/// <summary>
			/// Tries to find the outer edge around the current vertex that points to the given vertex.
			/// </summary>
			/// <param name="vertex">The vertex for which the corresponding edge is to be found.</param>
			/// <param name="edge">The outer edge whose vertex is the given vertex, or an empty edge if no matching edge is found.</param>
			/// <returns>True if the corresponding edge was found, and false if no matching edge is found.</returns>
			public bool TryFindOuterEdge(Vertex vertex, out VertexEdge edge)
			{
				return edge = FindOuterEdge(vertex);
			}

			/// <summary>
			/// Compares the current vertex to the specified object to find if they are wrappers around the same vertex.
			/// </summary>
			/// <param name="other">The object to be compared to the current vertex.</param>
			/// <returns>Returns true if the specified object is an instance of <see cref="Vertex"/>, and both it and the current vertex are wrappers around the same vertex, and false otherwise.</returns>
			public override bool Equals(object other) { return other is Vertex && _index == ((Vertex)other)._index && _topology == ((Vertex)other)._topology; }

			/// <summary>
			/// Compares the current vertex to the specified vertex to find if they are wrappers around the same vertex.
			/// </summary>
			/// <param name="other">The vertex to be compared to the current vertex.</param>
			/// <returns>Returns true if the specified vertex and the current vertex are both wrappers around the same vertex, and false otherwise.</returns>
			public bool Equals(Vertex other) { return _index == other._index && _topology == other._topology; }

			/// <summary>
			/// Compares the current vertex to the specified vertex.
			/// </summary>
			/// <param name="other">The other vertex to compare to the current vertex.</param>
			/// <returns>Returns a negative value if the current vertex is ordered before the specified vertex, a positive
			/// value if it is ordered after the specified vertex, and zero if they are wrappers around the same vertex.</returns>
			/// <remarks><para>It is assumed that both vertices belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public int CompareTo(Vertex other) { return _index - other._index; }

			/// <summary>
			/// Compares two vertex wrappers for equality.
			/// </summary>
			/// <param name="lhs">The first vertex to compare.</param>
			/// <param name="rhs">The second vertex to compare.</param>
			/// <returns>Returns true if the two vertices are wrappers around the same vertex, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both vertices belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator ==(Vertex lhs, Vertex rhs) { return lhs._index == rhs._index; }

			/// <summary>
			/// Compares two vertex wrappers for inequality.
			/// </summary>
			/// <param name="lhs">The first vertex to compare.</param>
			/// <param name="rhs">The second vertex to compare.</param>
			/// <returns>Returns true if the two vertices are wrappers around two different vertices, and false if they are wrappers around the same vertex.</returns>
			/// <remarks><para>It is assumed that both vertices belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator !=(Vertex lhs, Vertex rhs) { return lhs._index != rhs._index; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered before the second.
			/// </summary>
			/// <param name="lhs">The first vertex to compare.</param>
			/// <param name="rhs">The second vertex to compare.</param>
			/// <returns>True if the current vertex is ordered before the specified vertex, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both vertices belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator < (Vertex lhs, Vertex rhs) { return lhs._index <  rhs._index; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered before the second or if they both are wrappers around the same vertex.
			/// </summary>
			/// <param name="lhs">The first vertex to compare.</param>
			/// <param name="rhs">The second vertex to compare.</param>
			/// <returns>True if the current vertex is ordered before the specified vertex or if
			/// they are both wrappers around the same vertex, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both vertices belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator <=(Vertex lhs, Vertex rhs) { return lhs._index <= rhs._index; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered after the second.
			/// </summary>
			/// <param name="lhs">The first vertex to compare.</param>
			/// <param name="rhs">The second vertex to compare.</param>
			/// <returns>True if the current vertex is ordered after the specified vertex, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both vertices belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator > (Vertex lhs, Vertex rhs) { return lhs._index >  rhs._index; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered after the second or if they both are wrappers around the same vertex.
			/// </summary>
			/// <param name="lhs">The first vertex to compare.</param>
			/// <param name="rhs">The second vertex to compare.</param>
			/// <returns>True if the current vertex is ordered after the specified vertex or if
			/// they are both wrappers around the same vertex, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both vertices belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator >=(Vertex lhs, Vertex rhs) { return lhs._index >= rhs._index; }

			/// <summary>
			/// Calculates a 32-bit integer hash code for the current vertex.
			/// </summary>
			/// <returns>A 32-bit signed integer hash code based on the owning topology and index of the current vertex.</returns>
			public override int GetHashCode() { return _topology.GetHashCode() ^ _index.GetHashCode(); }

			/// <summary>
			/// Converts the vertex to string representation, appropriate for diagnositic display.  Includes the vertex index, and the indices of all neighbor vertices and faces.
			/// </summary>
			/// <returns>A string representation of the vertex.</returns>
			public override string ToString()
			{
				var sb = new System.Text.StringBuilder();
				sb.AppendFormat("Vertex {0}", _index);

				var edge = firstEdge;
				sb.AppendFormat("; Neighbor Vertices = {{ {0}", edge.vertex.index);
				for (int i = 1; i < neighborCount; ++i)
				{
					edge = edge.next;
					sb.AppendFormat(", {0}", edge.vertex.index);
				}

				edge = firstEdge;
				sb.AppendFormat("; Neighbor Faces = {{ {0}", edge.face.index);
				for (int i = 1; i < neighborCount; ++i)
				{
					edge = edge.next;
					sb.AppendFormat(", {0}", edge.face.index);
				}

				return sb.ToString();
			}
		}

		/// <summary>
		/// An indexer of the vertices in a topology, used for enumerating vertices.  Satisfies the concept
		/// of <see cref="IEnumerable{Vertex}"/>, enabling it to be used in <c>foreach</c> loops.
		/// </summary>
		public struct VerticesIndexer
		{
			private Topology _topology;

			/// <summary>
			/// Constructs an indexer for the vertices in the given topology.
			/// </summary>
			/// <param name="topology">The topology whose vertices are to be enumerated.</param>
			public VerticesIndexer(Topology topology) { _topology = topology; }

			/// <summary>
			/// Accesses a vertex by its integer index.
			/// </summary>
			/// <param name="i">The index of the vertex to access.</param>
			/// <returns>The vertex with the given index.</returns>
			public Vertex this[int i] { get { return new Vertex(_topology, i); } }

			/// <summary>
			/// The number of vertices accessible by this indexer.
			/// </summary>
			public int Count { get { return _topology.vertexFirstEdgeIndices.Length; } }

			/// <summary>
			/// Creates an enumerator for the vertices of the current indexer.
			/// </summary>
			/// <returns>An enumerator for the vertices of the current indexer.</returns>
			public VertexEnumerator GetEnumerator() { return new VertexEnumerator(_topology); }

			/// <summary>
			/// An enumerator of a topology's vertices.  Satisfies the concept of <see cref="IEnumerator{Vertex}"/>,
			/// enabling it to be used in <c>foreach</c> loops.
			/// </summary>
			public struct VertexEnumerator
			{
				private Topology _topology;
				private int _current;

				/// <summary>
				/// Constructs an instance of a vertex enumerator, given a topology containing the vertices.
				/// </summary>
				/// <param name="topology">The topology to which the enumerated vertices belong.</param>
				public VertexEnumerator(Topology topology) { _topology = topology; _current = -1; }

				/// <summary>
				/// A vertex wrapper around the current vertex being enumerated.
				/// </summary>
				public Vertex Current { get { return new Vertex(_topology, _current); } }

				/// <summary>
				/// Updates the enumerator to the next vertex in the topology.
				/// </summary>
				/// <returns>True if it moved to the next valid vertex, or false if there are no more vertices to be enumerated.</returns>
				public bool MoveNext() { return ++_current < _topology.vertexFirstEdgeIndices.Length; }

				/// <summary>
				/// Resets the enumerator back to its original state, so that another call to <see cref="MoveNext"/>
				/// will have <see cref="Current"/> return the first vertex of the enumerated sequence.
				/// </summary>
				public void Reset() { _current = -1; }
			}
		}

		/// <summary>
		/// Returns an indexer of all the vertices in the current topology.  Can be used in <c>foreach</c> loops.
		/// </summary>
		/// <seealso cref="enumerableVertices"/>
		public VerticesIndexer vertices { get { return new VerticesIndexer(this); } }

		/// <summary>
		/// Returns an enumerable sequence of all the vertices in the current topology.
		/// Implements <see cref="IEnumerable{Vertex}"/>, for circumstances where
		/// the actual interface and not just the concept is needed.
		/// </summary>
		/// <seealso cref="vertices"/>
		public IEnumerable<Vertex> enumerableVertices
		{
			get
			{
				foreach (var vertex in vertices)
				{
					yield return vertex;
				}
			}
		}
	}
}

#endif
