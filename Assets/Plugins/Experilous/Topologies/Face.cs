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
		/// A wrapper for conveniently working with a topology face, providing access to its core properties and enumeration of its neighbors.
		/// </summary>
		public struct Face : IEquatable<Face>, IComparable<Face>
		{
			private Topology _topology;
			private int _index;

			/// <summary>
			/// Constructs a face wrapper for a given topology and face index.
			/// </summary>
			/// <param name="topology">The topology containing the face.</param>
			/// <param name="index">The index of the face.</param>
			public Face(Topology topology, int index)
			{
				_topology = topology;
				_index = index;
			}

			/// <summary>
			/// An empty face wrapper that does not reference any topology or face.
			/// </summary>
			public static Face none { get { return new Face(); } }

			/// <summary>
			/// The topology to which the current face belongs.
			/// </summary>
			public Topology topology { get { return _topology; } }

			/// <summary>
			/// The integer index of the current face, used by edge references and access into face attribute collections.
			/// </summary>
			public int index { get { return _index; } }

			/// <summary>
			/// Checks whether the current face is considered to be part of or outside of the main topology surface.
			/// </summary>
			public bool isInternal { get { return _index < _topology.firstExternalFaceIndex; } }

			/// <summary>
			/// Checks whether the current face is considered to be outside of or part of the main topology surface.
			/// </summary>
			public bool isExternal { get { return _index >= _topology.firstExternalFaceIndex; } }

			/// <summary>
			/// The number of neighboring vertices or neighboring faces that a face has.
			/// </summary>
			/// <remarks><para>A face is guaranteed to have an equal number of neighboring vertices and faces.
			/// So a face with three neighbors means that it has three neighboring vertices, and also three
			/// neighboring faces.</para></remarks>
			public int neighborCount { get { return (int)_topology.faceNeighborCounts[_index]; } }

			/// <summary>
			/// A face edge wrapper around the first edge of this face, pointing to the face's first vertex/face pair of neighbors.
			/// </summary>
			/// <remarks><para>This edge wrapper can be used to manually iterate around the neighbors of the face, by
			/// using <see cref="FaceEdge.next"/> repeatedly until coming back around to the first edge.</para></remarks>
			public FaceEdge firstEdge { get { return new FaceEdge(_topology, _topology.faceFirstEdgeIndices[_index]); } }

			/// <summary>
			/// Checks if this face wrapper represents a valid face.  Converts to true if so, and false if the face wrapper is empty.
			/// </summary>
			/// <param name="face">The face to check.</param>
			/// <returns>True if the current face wrapper represents a valid face, and false if the face wrapper is empty.</returns>
			/// <seealso cref="none"/>
			public static implicit operator bool(Face face) { return face._topology != null; }

			/// <summary>
			/// Indicates if any of the face's neighboring faces are external.
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
			/// An indexer of a face's edges, used for enumerating its face edges.  Satisfies the concept
			/// of <see cref="IEnumerable{FaceEdge}"/>, enabling it to be used in <c>foreach</c> loops.
			/// </summary>
			public struct FaceEdgesIndexer
			{
				private Topology _topology;
				private int _index;

				/// <summary>
				/// Constructs an indexer for the face in the given topology and with the given index.
				/// </summary>
				/// <param name="topology">The topology to which the face belongs.</param>
				/// <param name="index">The index of the face.</param>
				public FaceEdgesIndexer(Topology topology, int index)
				{
					_topology = topology;
					_index = index;
				}

				/// <summary>
				/// The number of edges around the face to be enumerated.
				/// </summary>
				public int Count { get { return (int)(_topology.faceNeighborCounts[_index] & 0x7FFFu); } }
				
				/// <summary>
				/// An enumerator of a face's edges.  Satisfies the concept of <see cref="IEnumerator{FaceEdge}"/>,
				/// enabling it to be used in <c>foreach</c> loops.
				/// </summary>
				public struct FaceEdgeEnumerator
				{
					private Topology _topology;
					private int _firstEdgeIndex;
					private int _currentEdgeIndex;
					private int _nextEdgeIndex;

					/// <summary>
					/// Constructs an instance of a face edge enumerator, given a topology and the first edge in the cycle.
					/// </summary>
					/// <param name="topology">The topology to which the enumerated face edges belong.</param>
					/// <param name="firstEdgeIndex">The index of the first edge within a cycle to enumerate.</param>
					public FaceEdgeEnumerator(Topology topology, int firstEdgeIndex)
					{
						_topology = topology;
						_firstEdgeIndex = firstEdgeIndex;
						_currentEdgeIndex = -1;
						_nextEdgeIndex = firstEdgeIndex;
					}

					/// <summary>
					/// A face edge wrapper around the current edge being enumerated.
					/// </summary>
					public FaceEdge Current { get { return new FaceEdge(_topology, _currentEdgeIndex); } }

					/// <summary>
					/// Updates the enumerator to the next edge in the cycle around the near face.
					/// </summary>
					/// <returns>True if it moved to the next valid edge, or false if there are no more edges to be enumerated.</returns>
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

					/// <summary>
					/// Resets the enumerator back to its original state, so that another call to <see cref="MoveNext"/>
					/// will have <see cref="Current"/> return the first face edge of the enumerated sequence.
					/// </summary>
					public void Reset()
					{
						_currentEdgeIndex = -1;
						_nextEdgeIndex = _firstEdgeIndex;
					}
				}

				/// <summary>
				/// Creates an enumerator for the face edges of the current indexer.
				/// </summary>
				/// <returns>An enumerator for the face edges of the current indexer.</returns>
				public FaceEdgeEnumerator GetEnumerator()
				{
					return new FaceEdgeEnumerator(_topology, _topology.faceFirstEdgeIndices[_index]);
				}
			}

			/// <summary>
			/// Returns an indexer of all the face edges around the current face, which all have the
			/// current face as their near face.  Can be used in <c>foreach</c> loops.
			/// </summary>
			/// <seealso cref="enumerableEdges"/>
			public FaceEdgesIndexer edges { get { return new FaceEdgesIndexer(_topology, _index); } }

			/// <summary>
			/// Returns an enumerable sequence of all the face edges around the current face, which
			/// have the current face as their near face.  Implements <see cref="IEnumerable{FaceEdge}"/>,
			/// for circumstances where the actual interface and not just the concept is needed.
			/// </summary>
			/// <seealso cref="edges"/>
			public IEnumerable<FaceEdge> enumerableEdges
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
			/// An indexer of a face's outer edges, used for enumerating those edges.  Satisfies the concept
			/// of <see cref="IEnumerable{FaceEdge}"/>, enabling it to be used in <c>foreach</c> loops.
			/// </summary>
			/// <remarks><para>A face's outer edges provide convenient enumeration of all the edges that
			/// share a vertex with the face but do not border the face.  This is particularly useful in the
			/// situation where the concept of a face's neighbors includes every other face that that shares
			/// at least one vertex with the first face, such as the case with square grids which allow diagonal
			/// movement, where each face is treated as though it has eight neighbors.</para></remarks>
			public struct OuterEdgesIndexer
			{
				private Topology _topology;
				private int _index;

				/// <summary>
				/// Constructs an indexer for the face in the given topology and with the given index.
				/// </summary>
				/// <param name="topology">The topology to which the face belongs.</param>
				/// <param name="index">The index of the face.</param>
				public OuterEdgesIndexer(Topology topology, int index)
				{
					_topology = topology;
					_index = index;
				}

				/// <summary>
				/// An enumerator of a face's outer edges.  Satisfies the concept of <see cref="IEnumerator{FaceEdge}"/>,
				/// enabling it to be used in <c>foreach</c> loops.
				/// </summary>
				public struct OuterEdgesEnumerator
				{
					private Topology _topology;
					private int _firstFaceEdgeIndex;
					private int _currentFaceEdgeIndex;
					private int _nextVertexEdgeIndex;
					private int _nextFaceEdgeIndex;

					/// <summary>
					/// Constructs an instance of an outer edge enumerator, given a topology and a face's first ordinary (non-outer) edge.
					/// </summary>
					/// <param name="topology">The topology to which the enumerated edges belong.</param>
					/// <param name="firstEdgeIndex">The index of the first ordinary (non-outer) edge within a cycle to enumerate.</param>
					public OuterEdgesEnumerator(Topology topology, int firstEdgeIndex)
					{
						_topology = topology;
						_firstFaceEdgeIndex = firstEdgeIndex;
						_currentFaceEdgeIndex = -1;
						_nextVertexEdgeIndex = firstEdgeIndex;
						_nextFaceEdgeIndex = firstEdgeIndex;
					}

					/// <summary>
					/// A face edge wrapper around the current outer edge being enumerated.
					/// </summary>
					public FaceEdge Current { get { return new FaceEdge(_topology, _currentFaceEdgeIndex); } }

					/// <summary>
					/// Updates the enumerator to the next outer edge in the cycle around the face.
					/// </summary>
					/// <returns>True if it moved to the next valid edge, or false if there are no more edges to be enumerated.</returns>
					public bool MoveNext()
					{
						if (_nextVertexEdgeIndex != _nextFaceEdgeIndex)
						{
							_currentFaceEdgeIndex = _nextVertexEdgeIndex;
							_nextVertexEdgeIndex = _topology.edgeData[_currentFaceEdgeIndex].vNext;
							return true;
						}
						else if (_nextFaceEdgeIndex != _firstFaceEdgeIndex || _currentFaceEdgeIndex == -1)
						{
							do
							{
								_currentFaceEdgeIndex = _topology.edgeData[_topology.edgeData[_nextFaceEdgeIndex].twin].vNext;
								_nextFaceEdgeIndex = _topology.edgeData[_nextFaceEdgeIndex].fNext;
								if (_currentFaceEdgeIndex == _firstFaceEdgeIndex)
								{
									_nextVertexEdgeIndex = _nextFaceEdgeIndex = _firstFaceEdgeIndex;
									return false;
								}
							} while (_currentFaceEdgeIndex == _nextFaceEdgeIndex);
							_nextVertexEdgeIndex = _topology.edgeData[_currentFaceEdgeIndex].vNext;
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
						_currentFaceEdgeIndex = -1;
						_nextVertexEdgeIndex = -1;
						_nextFaceEdgeIndex = _firstFaceEdgeIndex;
					}
				}

				/// <summary>
				/// Creates an enumerator for the outer edges of the current indexer.
				/// </summary>
				/// <returns>An enumerator for the outer edges of the current indexer.</returns>
				public OuterEdgesEnumerator GetEnumerator()
				{
					return new OuterEdgesEnumerator(_topology, _topology.faceFirstEdgeIndices[_index]);
				}
			}

			/// <summary>
			/// Returns an indexer of all the outer edges around the current face, which all share a
			/// near vertex with the current face, but do not have the current face as their near face.
			/// Can be used in <c>foreach</c> loops.
			/// </summary>
			/// <remarks><para>A face's outer edges provide convenient enumeration of all the edges that
			/// share a vertex with the face but do not border the face.  This is particularly useful in the
			/// situation where the concept of a face's neighbors includes every other face that that shares
			/// at least one vertex with the first face, such as the case with square grids which allow diagonal
			/// movement, where each face is treated as though it has eight neighbors.</para></remarks>
			public OuterEdgesIndexer outerEdges { get { return new OuterEdgesIndexer(_topology, _index); } }

			/// <summary>
			/// Returns an enumerable sequence of all the outer edges around the current face, which
			/// all share a near vertex with the current face, but do not have the current face as their
			/// near face.  Implements <see cref="IEnumerable{FaceEdge}"/>, for circumstances where the
			/// actual interface and not just the concept is needed.
			/// </summary>
			/// <remarks><para>A face's outer edges provide convenient enumeration of all the edges that
			/// share a vertex with the face but do not border the face.  This is particularly useful in the
			/// situation where the concept of a face's neighbors includes every other face that that shares
			/// at least one vertex with the first face, such as the case with square grids which allow diagonal
			/// movement, where each face is treated as though it has eight neighbors.</para></remarks>
			/// <seealso cref="outerEdges"/>
			public IEnumerable<FaceEdge> enumerableOuterEdges
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
			/// Finds the face edge around the current face that corresponds to the given vertex.
			/// </summary>
			/// <param name="vertex">The vertex for which the corresponding edge is to be found.</param>
			/// <returns>The face edge whose vertex is the given vertex, or an empty edge if the given vertex is not a neighbor of the current face.</returns>
			public FaceEdge FindEdge(Vertex vertex)
			{
				foreach (var faceEdge in edges)
				{
					if (faceEdge.vertex == vertex)
					{
						return faceEdge;
					}
				}
				return FaceEdge.none;
			}

			/// <summary>
			/// Finds the face edge around the current face that points to the given face.
			/// </summary>
			/// <param name="face">The face for which the corresponding edge is to be found.</param>
			/// <returns>The face edge whose face is the given face, or an empty edge if the given face is not a neighbor of the current face.</returns>
			public FaceEdge FindEdge(Face face)
			{
				foreach (var faceEdge in edges)
				{
					if (faceEdge.face == face)
					{
						return faceEdge;
					}
				}
				return FaceEdge.none;
			}

			/// <summary>
			/// Tries to find the face edge around the current face that corresponds to the given vertex.
			/// </summary>
			/// <param name="vertex">The vertex for which the corresponding edge is to be found.</param>
			/// <param name="edge">The face edge whose vertex is the given vertex, or an empty edge if the given vertex is not a neighbor of the current face.</param>
			/// <returns>True if the corresponding edge was found, and false if the given vertex is not a neighbor of the current face.</returns>
			public bool TryFindEdge(Vertex vertex, out FaceEdge edge)
			{
				return edge = FindEdge(vertex);
			}

			/// <summary>
			/// Tries to find the face edge around the current face that corresponds to the given face.
			/// </summary>
			/// <param name="face">The face for which the corresponding edge is to be found.</param>
			/// <param name="edge">The face edge whose face is the given face, or an empty edge if the given face is not a neighbor of the current face.</param>
			/// <returns>True if the corresponding edge was found, and false if the given face is not a neighbor of the current face.</returns>
			public bool TryFindEdge(Face face, out FaceEdge edge)
			{
				return edge = FindEdge(face);
			}

			/// <summary>
			/// Finds the outer edge around the current face that points to the given vertex.
			/// </summary>
			/// <param name="vertex">The vertex for which the corresponding edge is to be found.</param>
			/// <returns>The outer edge whose vertex is the given vertex, or an empty edge if no matching edge is found.</returns>
			public FaceEdge FindOuterEdge(Vertex vertex)
			{
				foreach (var faceEdge in outerEdges)
				{
					if (faceEdge.vertex == vertex)
					{
						return faceEdge;
					}
				}
				return FaceEdge.none;
			}

			/// <summary>
			/// Finds the outer edge around the current face that corresponds to the given face.
			/// </summary>
			/// <param name="face">The face for which the corresponding edge is to be found.</param>
			/// <returns>The outer edge whose face is the given face, or an empty edge if no matching edge is found.</returns>
			public FaceEdge FindOuterEdge(Face face)
			{
				foreach (var faceEdge in outerEdges)
				{
					if (faceEdge.face == face)
					{
						return faceEdge;
					}
				}
				return FaceEdge.none;
			}

			/// <summary>
			/// Tries to find the outer edge around the current face that points to the given vertex.
			/// </summary>
			/// <param name="vertex">The vertex for which the corresponding edge is to be found.</param>
			/// <param name="edge">The outer edge whose vertex is the given vertex, or an empty edge if no matching edge is found.</param>
			/// <returns>True if the corresponding edge was found, and false if no matching edge is found.</returns>
			public bool TryFindOuterEdge(Vertex vertex, out FaceEdge edge)
			{
				return edge = FindOuterEdge(vertex);
			}

			/// <summary>
			/// Tries to find the outer edge around the current face that corresponds to the given face.
			/// </summary>
			/// <param name="face">The face for which the corresponding edge is to be found.</param>
			/// <param name="edge">The outer edge whose face is the given face, or an empty edge if no matching edge is found.</param>
			/// <returns>True if the corresponding edge was found, and false if no matching edge is found.</returns>
			public bool TryFindOuterEdge(Face face, out FaceEdge edge)
			{
				return edge = FindOuterEdge(face);
			}

			/// <summary>
			/// Compares the current face to the specified object to find if they are wrappers around the same face.
			/// </summary>
			/// <param name="other">The object to be compared to the current face.</param>
			/// <returns>Returns true if the specified object is an instance of <see cref="Face"/>, and both it and the current face are wrappers around the same face, and false otherwise.</returns>
			public override bool Equals(object other) { return other is Face && _index == ((Face)other)._index && _topology == ((Face)other)._topology; }

			/// <summary>
			/// Compares the current face to the specified face to find if they are wrappers around the same face.
			/// </summary>
			/// <param name="other">The face to be compared to the current face.</param>
			/// <returns>Returns true if the specified face and the current face are both wrappers around the same face, and false otherwise.</returns>
			public bool Equals(Face other) { return _index == other._index && _topology == other._topology; }

			/// <summary>
			/// Compares the current face to the specified face.
			/// </summary>
			/// <param name="other">The other face to compare to the current face.</param>
			/// <returns>Returns a negative value if the current face is ordered before the specified face, a positive
			/// value if it is ordered after the specified face, and zero if they are wrappers around the same face.</returns>
			/// <remarks><para>It is assumed that both faces belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public int CompareTo(Face other) { return _index - other._index; }

			/// <summary>
			/// Compares two face wrappers for equality.
			/// </summary>
			/// <param name="lhs">The first face to compare.</param>
			/// <param name="rhs">The second face to compare.</param>
			/// <returns>Returns true if the two faces are wrappers around the same face, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both faces belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator ==(Face lhs, Face rhs) { return lhs._index == rhs._index; }

			/// <summary>
			/// Compares two face wrappers for inequality.
			/// </summary>
			/// <param name="lhs">The first face to compare.</param>
			/// <param name="rhs">The second face to compare.</param>
			/// <returns>Returns true if the two faces are wrappers around two different faces, and false if they are wrappers around the same face.</returns>
			/// <remarks><para>It is assumed that both faces belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator !=(Face lhs, Face rhs) { return lhs._index != rhs._index; }

			/// <summary>
			/// Compares the two faces to determine if the first is ordered before the second.
			/// </summary>
			/// <param name="lhs">The first face to compare.</param>
			/// <param name="rhs">The second face to compare.</param>
			/// <returns>True if the current face is ordered before the specified face, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both faces belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator < (Face lhs, Face rhs) { return lhs._index <  rhs._index; }

			/// <summary>
			/// Compares the two faces to determine if the first is ordered before the second or if they both are wrappers around the same face.
			/// </summary>
			/// <param name="lhs">The first face to compare.</param>
			/// <param name="rhs">The second face to compare.</param>
			/// <returns>True if the current face is ordered before the specified face or if
			/// they are both wrappers around the same face, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both faces belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator <=(Face lhs, Face rhs) { return lhs._index <= rhs._index; }

			/// <summary>
			/// Compares the two faces to determine if the first is ordered after the second.
			/// </summary>
			/// <param name="lhs">The first face to compare.</param>
			/// <param name="rhs">The second face to compare.</param>
			/// <returns>True if the current face is ordered after the specified face, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both faces belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator > (Face lhs, Face rhs) { return lhs._index >  rhs._index; }

			/// <summary>
			/// Compares the two faces to determine if the first is ordered after the second or if they both are wrappers around the same face.
			/// </summary>
			/// <param name="lhs">The first face to compare.</param>
			/// <param name="rhs">The second face to compare.</param>
			/// <returns>True if the current face is ordered after the specified face or if
			/// they are both wrappers around the same face, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both faces belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator >=(Face lhs, Face rhs) { return lhs._index >= rhs._index; }

			/// <summary>
			/// Calculates a 32-bit integer hash code for the current face.
			/// </summary>
			/// <returns>A 32-bit signed integer hash code based on the owning topology and index of the current face.</returns>
			public override int GetHashCode() { return _topology.GetHashCode() ^ _index.GetHashCode(); }

			/// <summary>
			/// Converts the face to string representation, appropriate for diagnositic display.  Includes the face index, and the indices of all neighbor faces and vertices.
			/// </summary>
			/// <returns>A string representation of the face.</returns>
			public override string ToString()
			{
				var sb = new System.Text.StringBuilder();
				sb.AppendFormat("Face {0}", _index);

				var edge = firstEdge;
				sb.AppendFormat("; Neighbor Faces = {{ {0}", edge.face.index);
				for (int i = 1; i < neighborCount; ++i)
				{
					edge = edge.next;
					sb.AppendFormat(", {0}", edge.face.index);
				}

				edge = firstEdge;
				sb.AppendFormat("; Neighbor Vertices = {{ {0}", edge.vertex.index);
				for (int i = 1; i < neighborCount; ++i)
				{
					edge = edge.next;
					sb.AppendFormat(", {0}", edge.vertex.index);
				}

				return sb.ToString();
			}
		}

		/// <summary>
		/// An indexer of the faces in a topology, used for enumerating faces.  Satisfies the concept
		/// of <see cref="IEnumerable{Face}"/>, enabling it to be used in <c>foreach</c> loops.
		/// </summary>
		public struct FacesIndexer
		{
			private Topology _topology;
			private int _first;
			private int _last;

			/// <summary>
			/// Constructs an indexer for all the faces in the given topology.
			/// </summary>
			/// <param name="topology">The topology whose faces are to be indexed.</param>
			public FacesIndexer(Topology topology) { _topology = topology; _first = 0; _last = _topology.faceFirstEdgeIndices.Length; }

			/// <summary>
			/// Constructs an indexer for a range of faces in the given topology.
			/// </summary>
			/// <param name="topology">The topology whose faces are to be enumerated.</param>
			/// <param name="first">The index of the first face to be enumerated.</param>
			/// <param name="last">The index one past the last face to be enumerated.</param>
			public FacesIndexer(Topology topology, int first, int last) { _topology = topology; _first = first; _last = last; }

			/// <summary>
			/// Accesses a face by its integer index.
			/// </summary>
			/// <param name="i">The index of the face to access.</param>
			/// <returns>The face with the given index.</returns>
			/// <remarks><para>This index is relative to the range of the current indexer,
			/// not to the total set of faces in the topology.  If the range does not start
			/// at the beginning of the topology's sequence of faces, then the index used here
			/// will not match the returned face's index within the topology.</para></remarks>
			public Face this[int i] { get { return new Face(_topology, _first + i); } set { throw new NotSupportedException(); } }

			/// <summary>
			/// The number of faces accessible by this indexer.
			/// </summary>
			public int Count { get { return _last - _first; } }

			/// <summary>
			/// Creates an enumerator for the faces of the current indexer.
			/// </summary>
			/// <returns>An enumerator for the faces of the current indexer.</returns>
			public FaceEnumerator GetEnumerator() { return new FaceEnumerator(_topology, _first, _last); }

			/// <summary>
			/// An enumerator of a topology's faces.  Satisfies the concept of <see cref="IEnumerator{Face}"/>,
			/// enabling it to be used in <c>foreach</c> loops.
			/// </summary>
			public struct FaceEnumerator
			{
				private Topology _topology;
				private int _first;
				private int _last;
				private int _current;

				/// <summary>
				/// Constructs an instance of a face enumerator, given a topology containing the faces and the range of faces to enumerate.
				/// </summary>
				/// <param name="topology">The topology to which the enumerated faces belong.</param>
				/// <param name="first">The index of the first face to be enumerated.</param>
				/// <param name="last">The index one past the last face to be enumerated.</param>
				public FaceEnumerator(Topology topology, int first, int last) { _topology = topology; _first = first; _last = last; _current = _first - 1; }

				/// <summary>
				/// A face wrapper around the current face being enumerated.
				/// </summary>
				public Face Current { get { return new Face(_topology, _current); } }

				/// <summary>
				/// Updates the enumerator to the next face in the topology.
				/// </summary>
				/// <returns>True if it moved to the next valid face, or false if there are no more faces to be enumerated.</returns>
				public bool MoveNext() { return ++_current < _last; }

				/// <summary>
				/// Resets the enumerator back to its original state, so that another call to <see cref="MoveNext"/>
				/// will have <see cref="Current"/> return the first face of the enumerated sequence.
				/// </summary>
				public void Reset() { _current = _first - 1; }
			}
		}

		/// <summary>
		/// Returns an indexer of all the faces in the current topology.  Can be used in <c>foreach</c> loops.
		/// </summary>
		/// <seealso cref="internalFaces"/>
		/// <seealso cref="externalFaces"/>
		/// <seealso cref="enumerableFaces"/>
		public FacesIndexer faces { get { return new FacesIndexer(this); } }

		/// <summary>
		/// Returns an indexer of all the internal faces in the current topology, and excludes all the external faces.  Can be used in <c>foreach</c> loops.
		/// </summary>
		/// <seealso cref="faces"/>
		/// <seealso cref="externalFaces"/>
		/// <seealso cref="enumerableInternalFaces"/>
		public FacesIndexer internalFaces { get { return new FacesIndexer(this, 0, firstExternalFaceIndex); } }

		/// <summary>
		/// Returns an indexer of all the external faces in the current topology, and excludes all the internal faces.  Can be used in <c>foreach</c> loops.
		/// </summary>
		/// <seealso cref="faces"/>
		/// <seealso cref="internalFaces"/>
		/// <seealso cref="enumerableExternalFaces"/>
		public FacesIndexer externalFaces { get { return new FacesIndexer(this, firstExternalFaceIndex, faceFirstEdgeIndices.Length); } }

		/// <summary>
		/// Returns an enumerable sequence of all the faces in the current topology.
		/// Implements <see cref="IEnumerable{Face}"/>, for circumstances where
		/// the actual interface and not just the concept is needed.
		/// </summary>
		/// <seealso cref="faces"/>
		/// <seealso cref="enumerableInternalFaces"/>
		/// <seealso cref="enumerableExternalFaces"/>
		public IEnumerable<Face> enumerableFaces
		{
			get
			{
				foreach (var face in faces)
				{
					yield return face;
				}
			}
		}

		/// <summary>
		/// Returns an enumerable sequence of all the internal faces in the current
		/// topology, and excludes all the external faces. Implements
		/// <see cref="IEnumerable{Face}"/>, for circumstances where the actual
		/// interface and not just the concept is needed.
		/// </summary>
		/// <seealso cref="internalFaces"/>
		/// <seealso cref="enumerableFaces"/>
		/// <seealso cref="enumerableExternalFaces"/>
		public IEnumerable<Face> enumerableInternalFaces
		{
			get
			{
				foreach (var face in internalFaces)
				{
					yield return face;
				}
			}
		}

		/// <summary>
		/// Returns an enumerable sequence of all the external faces in the current
		/// topology, and excludes all the internal faces. Implements
		/// <see cref="IEnumerable{Face}"/>, for circumstances where the actual
		/// interface and not just the concept is needed.
		/// </summary>
		/// <seealso cref="internalFaces"/>
		/// <seealso cref="enumerableFaces"/>
		/// <seealso cref="enumerableInternalFaces"/>
		public IEnumerable<Face> enumerableExternalFaces
		{
			get
			{
				foreach (var face in externalFaces)
				{
					yield return face;
				}
			}
		}
	}
}

#endif
