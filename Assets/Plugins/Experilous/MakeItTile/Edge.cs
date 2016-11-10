/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System;
using System.Collections.Generic;

namespace Experilous.MakeItTile
{
	public partial class Topology
	{
		/// <summary>
		/// Data stored for each edge to track relations among vertices and faces.
		/// </summary>
		[Serializable]
		public struct EdgeData
		{
			// \             /
			//  \     F     /
			//   \         /
			//    r---E-->v
			//   /         \
			//  n     R     N
			// /             \
			//
			// E:  This edge
			// r:  Implicit near vertex
			// v:  Explicit far vertex
			// R:  Implicit near face
			// F:  Explicit far face
			// n:  Next edge when going around vertex r clockwise
			// N:  Next edge when going around face R clockwise

			/// <summary>
			/// The other edge between the same two vertices and same two faces, but going the opposite direction.
			/// </summary>
			public int twin;

			/// <summary>
			/// The next edge after this one when going clockwise around the implicit near vertex.
			/// </summary>
			public int vNext;

			/// <summary>
			/// The next edge this one when going clockwise around the implicit near face.
			/// </summary>
			public int fNext;

			/// <summary>
			/// The vertex at the far end of the current edge that preceeds the below face when going clockwise around the implicit near vertex.
			/// </summary>
			public int vertex;

			/// <summary>
			/// The face on the far side of the current edge that follows after the above vertex when going clockwise around the implicit near vertex.
			/// </summary>
			public int face;

			/// <summary>
			/// The wrap-around flags when transitioning among vertices, edges, and faces.
			/// </summary>
			public EdgeWrap wrap;

			/// <summary>
			/// Constructs an instance with the specified vertex and face relations.
			/// </summary>
			/// <param name="twin">The other edge between the same two vertices and same two faces, but going the opposite direction.</param>
			/// <param name="vNext">The next edge after this one when going clockwise around the implicit near vertex.</param>
			/// <param name="fNext">The next edge this one when going clockwise around the implicit near face.</param>
			/// <param name="vertex">The vertex at the far end of the current edge that preceeds the below face when going clockwise around the implicit near vertex.</param>
			/// <param name="face">The face on the far side of the current edge that follows after the above vertex when going clockwise around the implicit near vertex.</param>
			/// <param name="wrap">The wrap-around flags when transitioning among vertices, edges, and faces.</param>
			public EdgeData(int twin, int vNext, int fNext, int vertex, int face, EdgeWrap wrap = EdgeWrap.None)
			{
				this.twin = twin;
				this.vNext = vNext;
				this.fNext = fNext;
				this.vertex = vertex;
				this.face = face;
				this.wrap = wrap;
			}

			/// <summary>
			/// Converts the edge data to string representation, appropriate for diagnositic display.
			/// </summary>
			/// <returns>A string representation of the edge.</returns>
			public override string ToString()
			{
				return string.Format("EdgeData ({0}, {1}, {2}, {3}, {4})", twin, vNext, fNext, vertex, face);
			}
		}

		/// <summary>
		/// A wrapper for conveniently working with a topology edge, providing access to its core properties and enumeration of its neighbors.
		/// </summary>
		/// <remarks>
		/// <para>Edge wrappers come in three varieties, HalfEdge, VertexEdge, and FaceEdge.  The HalfEdge variety is focused on
		/// representing an edge neutrally, not relative to any of its neighboring vertices or faces.</para>
		/// <para>In addition to providing access to its two vertex and face neighbors, access is also provided to previous and next half-
		/// edges in the two circular linked lists that every half edge belongs to, the list of half edges around a near vertex, and the list
		/// of half edges around a near face.  Note that getting the next object requires one less lookup than getting the previous object,
		/// and is therefore recommended for the majority of cases.</para>
		/// </remarks>
		public struct HalfEdge : IEquatable<HalfEdge>, IEquatable<VertexEdge>, IEquatable<FaceEdge>, IComparable<HalfEdge>, IComparable<VertexEdge>, IComparable<FaceEdge>
		{
			private Topology _topology;
			private int _index;

			/// <summary>
			/// Constructs a half edge wrapper for a given topology and edge index.
			/// </summary>
			/// <param name="topology">The topology containing the edge.</param>
			/// <param name="index">The index of the edge.</param>
			public HalfEdge(Topology topology, int index)
			{
				_topology = topology;
				_index = index;
			}

			/// <summary>
			/// Implicitly converts a half edge wrapper to a vertex edge wrapper around the same underlying edge.
			/// </summary>
			/// <param name="edge">The half edge wrapper to convert.</param>
			/// <returns>The vertex edge wrapper that represents the same edge as the converted half edge wrapper.</returns>
			public static implicit operator VertexEdge(HalfEdge edge)
			{
				return new VertexEdge(edge._topology, edge._index);
			}

			/// <summary>
			/// Implicitly converts a half edge wrapper to a face edge wrapper around the same underlying edge.
			/// </summary>
			/// <param name="edge">The half edge wrapper to convert.</param>
			/// <returns>The face edge wrapper that represents the same edge as the converted half edge wrapper.</returns>
			public static implicit operator FaceEdge(HalfEdge edge)
			{
				return new FaceEdge(edge._topology, edge._index);
			}

			#region Properties

			/// <summary>
			/// An empty half edge wrapper that does not reference any topology or edge.
			/// </summary>
			public static HalfEdge none { get { return new HalfEdge(); } }

			/// <summary>
			/// The topology to which the current face belongs.
			/// </summary>
			public Topology topology { get { return _topology; } }

			/// <summary>
			/// The integer index of the current edge, used by edge references and access into edge attribute collections.
			/// </summary>
			public int index { get { return _index; } }

			/// <summary>
			/// The index of the other edge between the same two vertices and same two faces, but going the opposite direction.
			/// </summary>
			public int twinIndex { get { return _topology.edgeData[_index].twin; } }

			/// <summary>
			/// Indicates whether the current half edge is the earlier of itself and its twin, useful in cases needs only apply to a single half edge of every twin pair.
			/// </summary>
			public bool isFirstTwin { get { return index < twinIndex; } }

			/// <summary>
			/// Indicates whether the current half edge is the later of itself and its twin, useful in cases needs only apply to a single half edge of every twin pair.
			/// </summary>
			public bool isSecondTwin { get { return index > twinIndex; } }

			/// <summary>
			/// The other edge between the same two vertices and same two faces, but going the opposite direction.
			/// </summary>
			public HalfEdge twin { get { return new HalfEdge(_topology, _topology.edgeData[_index].twin); } }

			/// <summary>
			/// The first edge of the current edge's twin pair, either the current edge itself or its twin, whichever is first in sequence.
			/// </summary>
			public HalfEdge firstTwin { get { return isFirstTwin ? this : twin; } }

			/// <summary>
			/// The second edge of the current edge's twin pair, either the current edge itself or its twin, whichever is first in sequence.
			/// </summary>
			public HalfEdge secondTwin { get { return isSecondTwin ? this : twin; } }

			/// <summary>
			/// The previous half edge, counter-clockwise from the current half edge if enumerating the edges pivoting around the near vertex.
			/// </summary>
			public HalfEdge prevAroundVertex { get { return new HalfEdge(_topology, _topology.edgeData[_topology.edgeData[_index].twin].fNext); } }

			/// <summary>
			/// The next half edge, clockwise from the current half edge if enumerating the edges pivoting around the near vertex.
			/// </summary>
			public HalfEdge nextAroundVertex { get { return new HalfEdge(_topology, _topology.edgeData[_index].vNext); } }

			/// <summary>
			/// The previous half edge, counter-clockwise from the current half edge if enumerating the edges circling around the near face.
			/// </summary>
			public HalfEdge prevAroundFace { get { return new HalfEdge(_topology, _topology.edgeData[_topology.edgeData[_index].vNext].twin); } }

			/// <summary>
			/// The next half edge, clockwise from the current half edge if enumerating the edges circling around the near face.
			/// </summary>
			public HalfEdge nextAroundFace { get { return new HalfEdge(_topology, _topology.edgeData[_index].fNext); } }

			/// <summary>
			/// A vertex wrapper of the vertex directly referenced by the current half edge; identical to <see cref="farVertex"/>.
			/// </summary>
			/// <seealso cref="farVertex"/>
			public Vertex vertex { get { return new Vertex(_topology, _topology.edgeData[_index].vertex); } }

			/// <summary>
			/// A vertex wrapper of the vertex at the source end of the current half edge.
			/// </summary>
			public Vertex nearVertex { get { return new Vertex(_topology, _topology.edgeData[_topology.edgeData[_index].twin].vertex); } }

			/// <summary>
			/// A vertex wrapper of the vertex at the target end of the current half edge.
			/// </summary>
			/// <seealso cref="vertex"/>
			public Vertex farVertex { get { return new Vertex(_topology, _topology.edgeData[_index].vertex); } }

			/// <summary>
			/// A face wrapper of the face directly referenced by the current half edge; identical to <see cref="farFace"/>.
			/// </summary>
			/// <seealso cref="farFace"/>
			public Face face { get { return new Face(_topology, _topology.edgeData[_index].face); } }

			/// <summary>
			/// A face wrapper of the face at the source end of the current half edge.
			/// </summary>
			public Face nearFace { get { return new Face(_topology, _topology.edgeData[_topology.edgeData[_index].twin].face); } }

			/// <summary>
			/// A face wrapper of the face at the target end of the current half edge.
			/// </summary>
			/// <seealso cref="face"/>
			public Face farFace { get { return new Face(_topology, _topology.edgeData[_index].face); } }

			/// <summary>
			/// The wrap-around flags when transitioning among vertices, edges, and faces.
			/// </summary>
			public EdgeWrap wrap { get { return _topology.edgeData[_index].wrap; } }

			/// <summary>
			/// Indicates if the current half edge is on the boundary between internal and external faces.
			/// </summary>
			public bool isBoundary { get { return farFace.isExternal != nearFace.isExternal; } }

			/// <summary>
			/// Indicates if the current half edge is not on the boundary between internal and external faces.
			/// </summary>
			public bool isNonBoundary { get { return farFace.isExternal == nearFace.isExternal; } }

			/// <summary>
			/// Indicates if the current edge is on the boundary between internal and external faces, with the far face being external.
			/// </summary>
			public bool isOuterBoundary { get { return farFace.isExternal && nearFace.isInternal; } }

			/// <summary>
			/// Indicates if the current edge is on the boundary between internal and external faces, with the far face being internal.
			/// </summary>
			public bool isInnerBoundary { get { return farFace.isInternal && nearFace.isExternal; } }

			/// <summary>
			/// Indicates if the current edge is internal, which is true if its near face is internal.
			/// </summary>
			/// <seealso cref="Face.isInternal"/>
			public bool isInternal { get { return nearFace.isInternal; } }

			/// <summary>
			/// Indicates if the current edge is external, which is true if its far face is external.
			/// </summary>
			/// <seealso cref="Face.isExternal"/>
			public bool isExternal { get { return farFace.isExternal; } }

			/// <summary>
			/// A vertex edge wrapper around the same underlying edge as the current half edge.
			/// </summary>
			public VertexEdge vertexEdge { get { return this; } }

			/// <summary>
			/// A face edge wrapper around the same underlying edge as the current half edge.
			/// </summary>
			public FaceEdge faceEdge { get { return this; } }

			#endregion

			/// <summary>
			/// Checks if the current edge wrapper represents a valid edge.  Converts to true if so, and false if the edge wrapper is empty.
			/// </summary>
			/// <param name="edge">The edge to check.</param>
			/// <returns>True if the current edge wrapper represents a valid edge, and false if the edge wrapper is empty.</returns>
			/// <seealso cref="none"/>
			public static implicit operator bool(HalfEdge edge) { return edge._topology != null; }

			/// <summary>
			/// Compares the current edge to the specified object to find if they are wrappers around the same edge.
			/// </summary>
			/// <param name="other">The object to be compared to the current edge.</param>
			/// <returns>Returns true if the specified object is an instance of <see cref="HalfEdge"/>,
			/// <see cref="VertexEdge"/>, or <see cref="FaceEdge"/>, and both it and the current edge are
			/// wrappers around the same edge, and false otherwise.</returns>
			public override bool Equals(object other)
			{
				return
					other is HalfEdge && _index == ((HalfEdge)other)._index && _topology == ((HalfEdge)other)._topology ||
					other is VertexEdge && _index == ((VertexEdge)other).index && _topology == ((VertexEdge)other).topology ||
					other is FaceEdge && _index == ((FaceEdge)other).index && _topology == ((FaceEdge)other).topology;
			}

			/// <summary>
			/// Calculates a 32-bit integer hash code for the current edge.
			/// </summary>
			/// <returns>A 32-bit signed integer hash code based on the owning topology and index of the current edge.</returns>
			public override int GetHashCode() { return _topology.GetHashCode() ^ _index.GetHashCode(); }

			#region Half Edge / Half Edge Comparisons

			/// <summary>
			/// Compares the current half edge to the specified half edge to find if they are wrappers around the same edge.
			/// </summary>
			/// <param name="other">The half edge to be compared to the current half edge.</param>
			/// <returns>Returns true if the specified half edge and the current half edge are both wrappers around the same edge, and false otherwise.</returns>
			public bool Equals(HalfEdge other) { return _index == other._index && _topology == other._topology; }

			/// <summary>
			/// Compares the current half edge to the specified half edge.
			/// </summary>
			/// <param name="other">The other half edge to compare to the current half edge.</param>
			/// <returns>Returns a negative value if the current half edge is ordered before the specified half edge, a positive
			/// value if it is ordered after the specified half edge, and zero if they are wrappers around the same edge.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public int CompareTo(HalfEdge other) { return _index - other._index; }

			/// <summary>
			/// Compares two half edge wrappers for equality.
			/// </summary>
			/// <param name="lhs">The first half edge to compare.</param>
			/// <param name="rhs">The second half edge to compare.</param>
			/// <returns>Returns true if the two edges are wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator ==(HalfEdge lhs, HalfEdge rhs) { return lhs._index == rhs._index; }

			/// <summary>
			/// Compares two half edge wrappers for inequality.
			/// </summary>
			/// <param name="lhs">The first half edge to compare.</param>
			/// <param name="rhs">The second half edge to compare.</param>
			/// <returns>Returns true if the two edges are wrappers around two different edges, and false if they are wrappers around the same edge.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator !=(HalfEdge lhs, HalfEdge rhs) { return lhs._index != rhs._index; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered before the second.
			/// </summary>
			/// <param name="lhs">The first half edge to compare.</param>
			/// <param name="rhs">The second half edge to compare.</param>
			/// <returns>True if the current half edge is ordered before the specified half edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator < (HalfEdge lhs, HalfEdge rhs) { return lhs._index <  rhs._index; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered before the second or if they both are wrappers around the same edge.
			/// </summary>
			/// <param name="lhs">The first half edge to compare.</param>
			/// <param name="rhs">The second half edge to compare.</param>
			/// <returns>True if the current half edge is ordered before the specified half edge or if
			/// they are both wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator <=(HalfEdge lhs, HalfEdge rhs) { return lhs._index <= rhs._index; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered after the second.
			/// </summary>
			/// <param name="lhs">The first half edge to compare.</param>
			/// <param name="rhs">The second half edge to compare.</param>
			/// <returns>True if the current half edge is ordered after the specified half edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator > (HalfEdge lhs, HalfEdge rhs) { return lhs._index >  rhs._index; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered after the second or if they both are wrappers around the same edge.
			/// </summary>
			/// <param name="lhs">The first half edge to compare.</param>
			/// <param name="rhs">The second half edge to compare.</param>
			/// <returns>True if the current half edge is ordered after the specified half edge or if
			/// they are both wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator >=(HalfEdge lhs, HalfEdge rhs) { return lhs._index >= rhs._index; }

			#endregion

			#region Half Edge / Vertex Edge Comparisons

			/// <summary>
			/// Compares the current half edge to the specified vertex edge to find if they are wrappers around the same edge.
			/// </summary>
			/// <param name="other">The vertex edge to be compared to the current half edge.</param>
			/// <returns>Returns true if the specified vertex edge and the current half edge are both wrappers around the same edge, and false otherwise.</returns>
			public bool Equals(VertexEdge other) { return _index == other.index && _topology == other.topology; }

			/// <summary>
			/// Compares the current half edge to the specified vertex edge.
			/// </summary>
			/// <param name="other">The other vertex edge to compare to the current half edge.</param>
			/// <returns>Returns a negative value if the current half edge is ordered before the specified vertex edge, a positive
			/// value if it is ordered after the specified vertex edge, and zero if they are wrappers around the same edge.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public int CompareTo(VertexEdge other) { return _index - other.index; }

			/// <summary>
			/// Compares two half edge wrappers for equality.
			/// </summary>
			/// <param name="lhs">The first half edge to compare.</param>
			/// <param name="rhs">The second vertex edge to compare.</param>
			/// <returns>Returns true if the two edges are wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator ==(HalfEdge lhs, VertexEdge rhs) { return lhs._index == rhs.index; }

			/// <summary>
			/// Compares two half edge wrappers for inequality.
			/// </summary>
			/// <param name="lhs">The first half edge to compare.</param>
			/// <param name="rhs">The second vertex edge to compare.</param>
			/// <returns>Returns true if the two edges are wrappers around two different edges, and false if they are wrappers around the same edge.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator !=(HalfEdge lhs, VertexEdge rhs) { return lhs._index != rhs.index; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered before the second.
			/// </summary>
			/// <param name="lhs">The first half edge to compare.</param>
			/// <param name="rhs">The second vertex edge to compare.</param>
			/// <returns>True if the current half edge is ordered before the specified vertex edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator < (HalfEdge lhs, VertexEdge rhs) { return lhs._index <  rhs.index; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered before the second or if they both are wrappers around the same edge.
			/// </summary>
			/// <param name="lhs">The first half edge to compare.</param>
			/// <param name="rhs">The second vertex edge to compare.</param>
			/// <returns>True if the current half edge is ordered before the specified vertex edge or if
			/// they are both wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator <=(HalfEdge lhs, VertexEdge rhs) { return lhs._index <= rhs.index; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered after the second.
			/// </summary>
			/// <param name="lhs">The first half edge to compare.</param>
			/// <param name="rhs">The second half edge to compare.</param>
			/// <returns>True if the current vertex edge is ordered after the specified vertex edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator > (HalfEdge lhs, VertexEdge rhs) { return lhs._index >  rhs.index; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered after the second or if they both are wrappers around the same edge.
			/// </summary>
			/// <param name="lhs">The first half edge to compare.</param>
			/// <param name="rhs">The second vertex edge to compare.</param>
			/// <returns>True if the current half edge is ordered after the specified vertex edge or if
			/// they are both wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator >=(HalfEdge lhs, VertexEdge rhs) { return lhs._index >= rhs.index; }

			#endregion

			#region Half Edge / Face Edge Comparisons

			/// <summary>
			/// Compares the current half edge to the specified face edge to find if they are wrappers around the same edge.
			/// </summary>
			/// <param name="other">The face edge to be compared to the current half edge.</param>
			/// <returns>Returns true if the specified face edge and the current half edge are both wrappers around the same edge, and false otherwise.</returns>
			public bool Equals(FaceEdge other) { return _index == other.index && _topology == other.topology; }

			/// <summary>
			/// Compares the current half edge to the specified face edge.
			/// </summary>
			/// <param name="other">The other face edge to compare to the current half edge.</param>
			/// <returns>Returns a negative value if the current half edge is ordered before the specified face edge, a positive
			/// value if it is ordered after the specified face edge, and zero if they are wrappers around the same edge.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public int CompareTo(FaceEdge other) { return _index - other.index; }

			/// <summary>
			/// Compares two half edge wrappers for equality.
			/// </summary>
			/// <param name="lhs">The first half edge to compare.</param>
			/// <param name="rhs">The second face edge to compare.</param>
			/// <returns>Returns true if the two edges are wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator ==(HalfEdge lhs, FaceEdge rhs) { return lhs._index == rhs.index; }

			/// <summary>
			/// Compares two half edge wrappers for inequality.
			/// </summary>
			/// <param name="lhs">The first half edge to compare.</param>
			/// <param name="rhs">The second face edge to compare.</param>
			/// <returns>Returns true if the two edges are wrappers around two different edges, and false if they are wrappers around the same edge.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator !=(HalfEdge lhs, FaceEdge rhs) { return lhs._index != rhs.index; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered before the second.
			/// </summary>
			/// <param name="lhs">The first half edge to compare.</param>
			/// <param name="rhs">The second face edge to compare.</param>
			/// <returns>True if the current half edge is ordered before the specified face edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator < (HalfEdge lhs, FaceEdge rhs) { return lhs._index <  rhs.index; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered before the second or if they both are wrappers around the same edge.
			/// </summary>
			/// <param name="lhs">The first half edge to compare.</param>
			/// <param name="rhs">The second face edge to compare.</param>
			/// <returns>True if the current half edge is ordered before the specified face edge or if
			/// they are both wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator <=(HalfEdge lhs, FaceEdge rhs) { return lhs._index <= rhs.index; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered after the second.
			/// </summary>
			/// <param name="lhs">The first half edge to compare.</param>
			/// <param name="rhs">The second half edge to compare.</param>
			/// <returns>True if the current face edge is ordered after the specified face edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator > (HalfEdge lhs, FaceEdge rhs) { return lhs._index >  rhs.index; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered after the second or if they both are wrappers around the same edge.
			/// </summary>
			/// <param name="lhs">The first half edge to compare.</param>
			/// <param name="rhs">The second face edge to compare.</param>
			/// <returns>True if the current half edge is ordered after the specified face edge or if
			/// they are both wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator >=(HalfEdge lhs, FaceEdge rhs) { return lhs._index >= rhs.index; }

			#endregion

			/// <summary>
			/// Converts the edge to string representation, appropriate for diagnositic display.
			/// </summary>
			/// <returns>A string representation of the edge.</returns>
			public override string ToString()
			{
				return string.Format("Edge {0} (Twin Edge: {1}, Vertex Edges p/n: {2}/{3}, Face Edges p/n: {4}/{5}), (Vertices p/n: {6}/{7}), (Faces p/n: {8}/{9})", _index, twinIndex, prevAroundVertex.index, nextAroundVertex.index, prevAroundFace.index, nextAroundFace.index, nearVertex.index, farVertex.index, farFace.index, nearFace.index);
			}
		}

		/// <summary>
		/// A wrapper for conveniently working with a topology edge, providing access to its core properties and enumeration of its neighbors.
		/// </summary>
		/// <remarks>
		/// <para>Edge wrappers come in three varieties, HalfEdge, VertexEdge, and FaceEdge.  The VertexEdge variety is focused on
		/// representing an edge relative to its near vertex, and properties are named relative to rotating clockwise around the near
		/// vertex where relevant.</para>
		/// <para>In addition to providing access to its two vertex and face neighbors, access is also provided to previous and next half-
		/// edges in the circular linked list around this half edge's near vertex.  Note that getting the next half edge requires one less
		/// lookup than getting the previous half edge, and is therefore recommended for the majority of cases.</para>
		/// </remarks>
		public struct VertexEdge : IEquatable<VertexEdge>, IEquatable<FaceEdge>, IComparable<VertexEdge>, IComparable<FaceEdge>
		{
			private HalfEdge _halfEdge;

			/// <summary>
			/// Constructs a vertex edge wrapper for a given topology and edge index.
			/// </summary>
			/// <param name="topology">The topology containing the edge.</param>
			/// <param name="index">The index of the edge.</param>
			public VertexEdge(Topology topology, int index)
			{
				_halfEdge = new HalfEdge(topology, index);
			}

			/// <summary>
			/// Implicitly converts a vertex edge wrapper to a half edge wrapper around the same underlying edge.
			/// </summary>
			/// <param name="edge">The vertex edge wrapper to convert.</param>
			/// <returns>The half edge wrapper that represents the same edge as the converted vertex edge wrapper.</returns>
			public static implicit operator HalfEdge(VertexEdge edge)
			{
				return edge._halfEdge;
			}

			/// <summary>
			/// Implicitly converts a vertex edge wrapper to a face edge wrapper around the same underlying edge.
			/// </summary>
			/// <param name="edge">The vertex edge wrapper to convert.</param>
			/// <returns>The face edge wrapper that represents the same edge as the converted vertex edge wrapper.</returns>
			public static implicit operator FaceEdge(VertexEdge edge)
			{
				return edge._halfEdge;
			}

			#region Properties

			/// <summary>
			/// An empty vertex edge wrapper that does not reference any topology or edge.
			/// </summary>
			public static VertexEdge none { get { return new VertexEdge(); } }

			/// <summary>
			/// The topology to which the current face belongs.
			/// </summary>
			public Topology topology { get { return _halfEdge.topology; } }

			/// <summary>
			/// The integer index of the current edge, used by edge references and access into edge attribute collections.
			/// </summary>
			public int index { get { return _halfEdge.index; } }

			/// <summary>
			/// The index of the other edge between the same two vertices and same two faces, but going the opposite direction.
			/// </summary>
			public int twinIndex { get { return _halfEdge.twinIndex; } }

			/// <summary>
			/// Indicates whether the current vertex edge is the earlier of itself and its twin, useful in cases needs only apply to a single vertex edge of every twin pair.
			/// </summary>
			public bool isFirstTwin { get { return index < twinIndex; } }

			/// <summary>
			/// Indicates whether the current vertex edge is the later of itself and its twin, useful in cases needs only apply to a single vertex edge of every twin pair.
			/// </summary>
			public bool isSecondTwin { get { return index > twinIndex; } }

			/// <summary>
			/// The other edge between the same two vertices and same two faces, but going the opposite direction.
			/// </summary>
			public VertexEdge twin { get { return _halfEdge.twin; } }

			/// <summary>
			/// The first edge of the current edge's twin pair, either the current edge itself or its twin, whichever is first in sequence.
			/// </summary>
			public VertexEdge firstTwin { get { return isFirstTwin ? this : twin; } }

			/// <summary>
			/// The second edge of the current edge's twin pair, either the current edge itself or its twin, whichever is first in sequence.
			/// </summary>
			public VertexEdge secondTwin { get { return isSecondTwin ? this : twin; } }

			/// <summary>
			/// The previous vertex edge, counter-clockwise from the current vertex edge if enumerating the edges pivoting around the near vertex.
			/// </summary>
			public VertexEdge prev { get { return _halfEdge.prevAroundVertex; } }

			/// <summary>
			/// The next vertex edge, clockwise from the current vertex edge if enumerating the edges pivoting around the near vertex.
			/// </summary>
			public VertexEdge next { get { return _halfEdge.nextAroundVertex; } }

			/// <summary>
			/// A vertex wrapper of the vertex directly referenced by the current vertex edge; identical to <see cref="farVertex"/>.
			/// </summary>
			/// <seealso cref="farVertex"/>
			public Vertex vertex { get { return _halfEdge.farVertex; } }

			/// <summary>
			/// A vertex wrapper of the vertex at the source end of the current vertex edge.
			/// </summary>
			public Vertex nearVertex { get { return _halfEdge.nearVertex; } }

			/// <summary>
			/// A vertex wrapper of the vertex at the target end of the current vertex edge.
			/// </summary>
			/// <seealso cref="vertex"/>
			public Vertex farVertex { get { return _halfEdge.farVertex; } }

			/// <summary>
			/// A face wrapper of the face directly referenced by the current vertex edge; identical to <see cref="prevFace"/>.
			/// </summary>
			/// <seealso cref="prevFace"/>
			public Face face { get { return _halfEdge.farFace; } }

			/// <summary>
			/// A face wrapper of the face on the counter-clockwise side of the current edge if enumerating the edges pivoting around the near vertex.
			/// </summary>
			public Face prevFace { get { return _halfEdge.farFace; } }

			/// <summary>
			/// A face wrapper of the face on the clockwise side of the current edge if enumerating the edges pivoting around the near vertex.
			/// </summary>
			/// <seealso cref="face"/>
			public Face nextFace { get { return _halfEdge.nearFace; } }

			/// <summary>
			/// The wrap-around flags when transitioning among vertices, edges, and faces.
			/// </summary>
			public EdgeWrap wrap { get { return _halfEdge.wrap; } }

			/// <summary>
			/// Indicates if the current vertex edge is on the boundary between internal and external faces.
			/// </summary>
			public bool isBoundary { get { return _halfEdge.isBoundary; } }

			/// <summary>
			/// Indicates if the current vertex edge is not on the boundary between internal and external faces.
			/// </summary>
			public bool isNonBoundary { get { return _halfEdge.isNonBoundary; } }

			/// <summary>
			/// Indicates if the current edge is on the boundary between internal and external faces, with the previous face being external.
			/// </summary>
			/// <seealso cref="Face.isInternal"/>
			/// <seealso cref="Face.isExternal"/>
			public bool isOuterBoundary { get { return _halfEdge.isOuterBoundary; } }

			/// <summary>
			/// Indicates if the current edge is on the boundary between internal and external faces, with the previous face being internal.
			/// </summary>
			/// <seealso cref="Face.isInternal"/>
			/// <seealso cref="Face.isExternal"/>
			public bool isInnerBoundary { get { return _halfEdge.isInnerBoundary; } }

			/// <summary>
			/// Indicates if the current edge is internal, which is true if its near face is internal.
			/// </summary>
			/// <seealso cref="Face.isInternal"/>
			public bool isInternal { get { return _halfEdge.isInternal; } }

			/// <summary>
			/// Indicates if the current edge is external, which is true if its previous face is external.
			/// </summary>
			/// <seealso cref="Face.isExternal"/>
			public bool isExternal { get { return _halfEdge.isExternal; } }

			/// <summary>
			/// A half edge wrapper around the same underlying edge as the current vertex edge.
			/// </summary>
			public HalfEdge halfEdge { get { return _halfEdge; } }

			/// <summary>
			/// A face edge wrapper around the same underlying edge as the current vertex edge.
			/// </summary>
			public FaceEdge faceEdge { get { return _halfEdge; } }

			#endregion

			/// <summary>
			/// Checks if the current edge wrapper represents a valid edge.  Converts to true if so, and false if the edge wrapper is empty.
			/// </summary>
			/// <param name="edge">The edge to check.</param>
			/// <returns>True if the current edge wrapper represents a valid edge, and false if the edge wrapper is empty.</returns>
			/// <seealso cref="none"/>
			public static implicit operator bool(VertexEdge edge) { return edge._halfEdge; }

			/// <summary>
			/// Compares the current edge to the specified object to find if they are wrappers around the same edge.
			/// </summary>
			/// <param name="other">The object to be compared to the current edge.</param>
			/// <returns>Returns true if the specified object is an instance of <see cref="HalfEdge"/>,
			/// <see cref="VertexEdge"/>, or <see cref="FaceEdge"/>, and both it and the current edge are
			/// wrappers around the same edge, and false otherwise.</returns>
			public override bool Equals(object other) { return _halfEdge.Equals(other); }

			/// <summary>
			/// Calculates a 32-bit integer hash code for the current edge.
			/// </summary>
			/// <returns>A 32-bit signed integer hash code based on the owning topology and index of the current edge.</returns>
			public override int GetHashCode() { return _halfEdge.GetHashCode(); }

			#region Vertex Edge / Half Edge Comparisons

			/// <summary>
			/// Compares the current vertex edge to the specified half edge to find if they are wrappers around the same edge.
			/// </summary>
			/// <param name="other">The half edge to be compared to the current vertex edge.</param>
			/// <returns>Returns true if the specified half edge and the current vertex edge are both wrappers around the same edge, and false otherwise.</returns>
			public bool Equals(HalfEdge other) { return _halfEdge.Equals(other); }

			/// <summary>
			/// Compares the current vertex edge to the specified half edge.
			/// </summary>
			/// <param name="other">The other half edge to compare to the current vertex edge.</param>
			/// <returns>Returns a negative value if the current vertex edge is ordered before the specified half edge, a positive
			/// value if it is ordered after the specified half edge, and zero if they are wrappers around the same edge.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public int CompareTo(HalfEdge other) { return _halfEdge.CompareTo(other); }

			/// <summary>
			/// Compares two vertex edge wrappers for equality.
			/// </summary>
			/// <param name="lhs">The first vertex edge to compare.</param>
			/// <param name="rhs">The second half edge to compare.</param>
			/// <returns>Returns true if the two edges are wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator ==(VertexEdge lhs, HalfEdge rhs) { return lhs._halfEdge == rhs; }

			/// <summary>
			/// Compares two vertex edge wrappers for inequality.
			/// </summary>
			/// <param name="lhs">The first vertex edge to compare.</param>
			/// <param name="rhs">The second half edge to compare.</param>
			/// <returns>Returns true if the two edges are wrappers around two different edges, and false if they are wrappers around the same edge.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator !=(VertexEdge lhs, HalfEdge rhs) { return lhs._halfEdge != rhs; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered before the second.
			/// </summary>
			/// <param name="lhs">The first vertex edge to compare.</param>
			/// <param name="rhs">The second half edge to compare.</param>
			/// <returns>True if the current vertex edge is ordered before the specified half edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator < (VertexEdge lhs, HalfEdge rhs) { return lhs._halfEdge <  rhs; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered before the second or if they both are wrappers around the same edge.
			/// </summary>
			/// <param name="lhs">The first vertex edge to compare.</param>
			/// <param name="rhs">The second half edge to compare.</param>
			/// <returns>True if the current vertex edge is ordered before the specified half edge or if
			/// they are both wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator <=(VertexEdge lhs, HalfEdge rhs) { return lhs._halfEdge <= rhs; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered after the second.
			/// </summary>
			/// <param name="lhs">The first vertex edge to compare.</param>
			/// <param name="rhs">The second vertex edge to compare.</param>
			/// <returns>True if the current half edge is ordered after the specified half edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator > (VertexEdge lhs, HalfEdge rhs) { return lhs._halfEdge >  rhs; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered after the second or if they both are wrappers around the same edge.
			/// </summary>
			/// <param name="lhs">The first vertex edge to compare.</param>
			/// <param name="rhs">The second half edge to compare.</param>
			/// <returns>True if the current vertex edge is ordered after the specified half edge or if
			/// they are both wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator >=(VertexEdge lhs, HalfEdge rhs) { return lhs._halfEdge >= rhs; }

			#endregion

			#region Vertex Edge / Vertex Edge Comparisons

			/// <summary>
			/// Compares the current vertex edge to the specified vertex edge to find if they are wrappers around the same edge.
			/// </summary>
			/// <param name="other">The vertex edge to be compared to the current vertex edge.</param>
			/// <returns>Returns true if the specified vertex edge and the current vertex edge are both wrappers around the same edge, and false otherwise.</returns>
			public bool Equals(VertexEdge other) { return _halfEdge.Equals(other._halfEdge); }

			/// <summary>
			/// Compares the current vertex edge to the specified vertex edge.
			/// </summary>
			/// <param name="other">The other vertex edge to compare to the current vertex edge.</param>
			/// <returns>Returns a negative value if the current vertex edge is ordered before the specified vertex edge, a positive
			/// value if it is ordered after the specified vertex edge, and zero if they are wrappers around the same edge.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public int CompareTo(VertexEdge other) { return _halfEdge.CompareTo(other._halfEdge); }

			/// <summary>
			/// Compares two vertex edge wrappers for equality.
			/// </summary>
			/// <param name="lhs">The first vertex edge to compare.</param>
			/// <param name="rhs">The second vertex edge to compare.</param>
			/// <returns>Returns true if the two edges are wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator ==(VertexEdge lhs, VertexEdge rhs) { return lhs._halfEdge == rhs._halfEdge; }

			/// <summary>
			/// Compares two vertex edge wrappers for inequality.
			/// </summary>
			/// <param name="lhs">The first vertex edge to compare.</param>
			/// <param name="rhs">The second vertex edge to compare.</param>
			/// <returns>Returns true if the two edges are wrappers around two different edges, and false if they are wrappers around the same edge.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator !=(VertexEdge lhs, VertexEdge rhs) { return lhs._halfEdge != rhs._halfEdge; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered before the second.
			/// </summary>
			/// <param name="lhs">The first vertex edge to compare.</param>
			/// <param name="rhs">The second vertex edge to compare.</param>
			/// <returns>True if the current vertex edge is ordered before the specified vertex edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator < (VertexEdge lhs, VertexEdge rhs) { return lhs._halfEdge <  rhs._halfEdge; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered before the second or if they both are wrappers around the same edge.
			/// </summary>
			/// <param name="lhs">The first vertex edge to compare.</param>
			/// <param name="rhs">The second vertex edge to compare.</param>
			/// <returns>True if the current vertex edge is ordered before the specified vertex edge or if
			/// they are both wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator <=(VertexEdge lhs, VertexEdge rhs) { return lhs._halfEdge <= rhs._halfEdge; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered after the second.
			/// </summary>
			/// <param name="lhs">The first vertex edge to compare.</param>
			/// <param name="rhs">The second vertex edge to compare.</param>
			/// <returns>True if the current vertex edge is ordered after the specified vertex edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator > (VertexEdge lhs, VertexEdge rhs) { return lhs._halfEdge >  rhs._halfEdge; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered after the second or if they both are wrappers around the same edge.
			/// </summary>
			/// <param name="lhs">The first vertex edge to compare.</param>
			/// <param name="rhs">The second vertex edge to compare.</param>
			/// <returns>True if the current vertex edge is ordered after the specified vertex edge or if
			/// they are both wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator >=(VertexEdge lhs, VertexEdge rhs) { return lhs._halfEdge >= rhs._halfEdge; }

			#endregion

			#region Vertex Edge / Face Edge Comparisons

			/// <summary>
			/// Compares the current vertex edge to the specified face edge to find if they are wrappers around the same edge.
			/// </summary>
			/// <param name="other">The face edge to be compared to the current vertex edge.</param>
			/// <returns>Returns true if the specified face edge and the current vertex edge are both wrappers around the same edge, and false otherwise.</returns>
			public bool Equals(FaceEdge other) { return _halfEdge.Equals(other); }

			/// <summary>
			/// Compares the current vertex edge to the specified face edge.
			/// </summary>
			/// <param name="other">The other face edge to compare to the current vertex edge.</param>
			/// <returns>Returns a negative value if the current vertex edge is ordered before the specified face edge, a positive
			/// value if it is ordered after the specified face edge, and zero if they are wrappers around the same edge.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public int CompareTo(FaceEdge other) { return _halfEdge.CompareTo(other); }

			/// <summary>
			/// Compares two vertex edge wrappers for equality.
			/// </summary>
			/// <param name="lhs">The first vertex edge to compare.</param>
			/// <param name="rhs">The second face edge to compare.</param>
			/// <returns>Returns true if the two edges are wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator ==(VertexEdge lhs, FaceEdge rhs) { return lhs._halfEdge == rhs; }

			/// <summary>
			/// Compares two vertex edge wrappers for inequality.
			/// </summary>
			/// <param name="lhs">The first vertex edge to compare.</param>
			/// <param name="rhs">The second face edge to compare.</param>
			/// <returns>Returns true if the two edges are wrappers around two different edges, and false if they are wrappers around the same edge.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator !=(VertexEdge lhs, FaceEdge rhs) { return lhs._halfEdge != rhs; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered before the second.
			/// </summary>
			/// <param name="lhs">The first vertex edge to compare.</param>
			/// <param name="rhs">The second face edge to compare.</param>
			/// <returns>True if the current vertex edge is ordered before the specified face edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator < (VertexEdge lhs, FaceEdge rhs) { return lhs._halfEdge <  rhs; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered before the second or if they both are wrappers around the same edge.
			/// </summary>
			/// <param name="lhs">The first vertex edge to compare.</param>
			/// <param name="rhs">The second face edge to compare.</param>
			/// <returns>True if the current vertex edge is ordered before the specified face edge or if
			/// they are both wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator <=(VertexEdge lhs, FaceEdge rhs) { return lhs._halfEdge <= rhs; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered after the second.
			/// </summary>
			/// <param name="lhs">The first vertex edge to compare.</param>
			/// <param name="rhs">The second vertex edge to compare.</param>
			/// <returns>True if the current face edge is ordered after the specified face edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator > (VertexEdge lhs, FaceEdge rhs) { return lhs._halfEdge >  rhs; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered after the second or if they both are wrappers around the same edge.
			/// </summary>
			/// <param name="lhs">The first vertex edge to compare.</param>
			/// <param name="rhs">The second face edge to compare.</param>
			/// <returns>True if the current vertex edge is ordered after the specified face edge or if
			/// they are both wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator >=(VertexEdge lhs, FaceEdge rhs) { return lhs._halfEdge >= rhs; }

			#endregion

			/// <summary>
			/// Converts the edge to string representation, appropriate for diagnositic display.
			/// </summary>
			/// <returns>A string representation of the edge.</returns>
			public override string ToString()
			{
				return string.Format("Vertex Edge {0} (Twin Edge: {1}, Edges p/n: {2}/{3}), (Vertices n/f: {4}/{5}), (Faces p/n: {6}/{7})", index, twinIndex, prev.index, next.index, nearVertex.index, farVertex.index, prevFace.index, nextFace.index);
			}
		}

		/// <summary>
		/// A wrapper for conveniently working with a topology face, providing access to its core properties and enumeration of its neighbors.
		/// </summary>
		/// <remarks>
		/// <para>Edge wrappers come in three varieties, HalfEdge, VertexEdge, and FaceEdge.  The FaceEdge variety is focused on
		/// representing an edge relative to its near face, and properties are named relative to rotating clockwise around the near
		/// face where relevant.</para>
		/// <para>In addition to providing access to its two vertex and face neighbors, access is also provided to previous and next half-
		/// edges in the circular linked list around this half edge's near face.  Note that getting the next half edge requires one less
		/// lookup than getting the previous half edge, and is therefore recommended for the majority of cases.</para>
		/// </remarks>
		public struct FaceEdge : IEquatable<FaceEdge>, IEquatable<VertexEdge>, IComparable<FaceEdge>, IComparable<VertexEdge>
		{
			private HalfEdge _halfEdge;

			/// <summary>
			/// Constructs a face edge wrapper for a given topology and edge index.
			/// </summary>
			/// <param name="topology">The topology containing the edge.</param>
			/// <param name="index">The index of the edge.</param>
			public FaceEdge(Topology topology, int index)
			{
				_halfEdge = new HalfEdge(topology, index);
			}

			/// <summary>
			/// Implicitly converts a face edge wrapper to a half edge wrapper around the same underlying edge.
			/// </summary>
			/// <param name="edge">The face edge wrapper to convert.</param>
			/// <returns>The half edge wrapper that represents the same edge as the converted face edge wrapper.</returns>
			public static implicit operator HalfEdge(FaceEdge edge)
			{
				return edge._halfEdge;
			}

			/// <summary>
			/// Implicitly converts a face edge wrapper to a vertex edge wrapper around the same underlying edge.
			/// </summary>
			/// <param name="edge">The face edge wrapper to convert.</param>
			/// <returns>The vertex edge wrapper that represents the same edge as the converted face edge wrapper.</returns>
			public static implicit operator VertexEdge(FaceEdge edge)
			{
				return edge._halfEdge;
			}

			#region Properties

			/// <summary>
			/// An empty face edge wrapper that does not reference any topology or edge.
			/// </summary>
			public static FaceEdge none { get { return new FaceEdge(); } }

			/// <summary>
			/// The topology to which the current face belongs.
			/// </summary>
			public Topology topology { get { return _halfEdge.topology; } }

			/// <summary>
			/// The integer index of the current edge, used by edge references and access into edge attribute collections.
			/// </summary>
			public int index { get { return _halfEdge.index; } }

			/// <summary>
			/// The index of the other edge between the same two vertices and same two faces, but going the opposite direction.
			/// </summary>
			public int twinIndex { get { return _halfEdge.twinIndex; } }

			/// <summary>
			/// Indicates whether the current face edge is the earlier of itself and its twin, useful in cases needs only apply to a single face edge of every twin pair.
			/// </summary>
			public bool isFirstTwin { get { return index < twinIndex; } }

			/// <summary>
			/// Indicates whether the current face edge is the later of itself and its twin, useful in cases needs only apply to a single face edge of every twin pair.
			/// </summary>
			public bool isSecondTwin { get { return index > twinIndex; } }

			/// <summary>
			/// The other edge between the same two vertices and same two faces, but going the opposite direction.
			/// </summary>
			public FaceEdge twin { get { return _halfEdge.twin; } }

			/// <summary>
			/// The first edge of the current edge's twin pair, either the current edge itself or its twin, whichever is first in sequence.
			/// </summary>
			public FaceEdge firstTwin { get { return isFirstTwin ? this : twin; } }

			/// <summary>
			/// The second edge of the current edge's twin pair, either the current edge itself or its twin, whichever is first in sequence.
			/// </summary>
			public FaceEdge secondTwin { get { return isSecondTwin ? this : twin; } }

			/// <summary>
			/// The previous face edge, counter-clockwise from the current face edge if enumerating the edges circling around the near face.
			/// </summary>
			public FaceEdge prev { get { return _halfEdge.prevAroundFace; } }

			/// <summary>
			/// The next face edge, clockwise from the current face edge if enumerating the edges circling around the near face.
			/// </summary>
			public FaceEdge next { get { return _halfEdge.nextAroundFace; } }

			/// <summary>
			/// A vertex wrapper of the vertex directly referenced by the current face edge; identical to <see cref="nextVertex"/>.
			/// </summary>
			/// <seealso cref="nextVertex"/>
			public Vertex vertex { get { return _halfEdge.farVertex; } }

			/// <summary>
			/// A vertex wrapper of the vertex to the counter-clockwise direction of the current edge if enumerating the edges circling around the near face.
			/// </summary>
			public Vertex prevVertex { get { return _halfEdge.nearVertex; } }

			/// <summary>
			/// A vertex wrapper of the vertex to the clockwise direction of the current edge if enumerating the edges circling around the near face.
			/// </summary>
			/// <seealso cref="vertex"/>
			public Vertex nextVertex { get { return _halfEdge.farVertex; } }

			/// <summary>
			/// A face wrapper of the face directly referenced by the current face edge; identical to <see cref="farFace"/>.
			/// </summary>
			/// <seealso cref="farFace"/>
			public Face face { get { return _halfEdge.farFace; } }

			/// <summary>
			/// A face wrapper of the face at the source end of the current face edge.
			/// </summary>
			public Face nearFace { get { return _halfEdge.nearFace; } }

			/// <summary>
			/// A face wrapper of the face at the target end of the current face edge.
			/// </summary>
			/// <seealso cref="face"/>
			public Face farFace { get { return _halfEdge.farFace; } }

			/// <summary>
			/// The wrap-around flags when transitioning among vertices, edges, and faces.
			/// </summary>
			public EdgeWrap wrap { get { return _halfEdge.wrap; } }

			/// <summary>
			/// Indicates if the current face edge is on the boundary between internal and external faces.
			/// </summary>
			public bool isBoundary { get { return _halfEdge.isBoundary; } }

			/// <summary>
			/// Indicates if the current face edge is not on the boundary between internal and external faces.
			/// </summary>
			public bool isNonBoundary { get { return _halfEdge.isNonBoundary; } }

			/// <summary>
			/// Indicates if the current edge is on the boundary between internal and external faces, with the far face being external.
			/// </summary>
			/// <seealso cref="Face.isInternal"/>
			/// <seealso cref="Face.isExternal"/>
			public bool isOuterBoundary { get { return _halfEdge.isOuterBoundary; } }

			/// <summary>
			/// Indicates if the current edge is on the boundary between internal and external faces, with the far face being internal.
			/// </summary>
			/// <seealso cref="Face.isInternal"/>
			/// <seealso cref="Face.isExternal"/>
			public bool isInnerBoundary { get { return _halfEdge.isInnerBoundary; } }

			/// <summary>
			/// Indicates if the current edge is internal, which is true if its near face is internal.
			/// </summary>
			/// <seealso cref="Face.isInternal"/>
			public bool isInternal { get { return _halfEdge.isInternal; } }

			/// <summary>
			/// Indicates if the current edge is external, which is true if its far face is external.
			/// </summary>
			/// <seealso cref="Face.isExternal"/>
			public bool isExternal { get { return _halfEdge.isExternal; } }

			/// <summary>
			/// A half edge wrapper around the same underlying edge as the current face edge.
			/// </summary>
			public HalfEdge halfEdge { get { return _halfEdge; } }

			/// <summary>
			/// A vertex edge wrapper around the same underlying edge as the current face edge.
			/// </summary>
			public VertexEdge vertexEdge { get { return _halfEdge; } }

			#endregion

			/// <summary>
			/// Checks if the current edge wrapper represents a valid edge.  Converts to true if so, and false if the edge wrapper is empty.
			/// </summary>
			/// <param name="edge">The edge to check.</param>
			/// <returns>True if the current edge wrapper represents a valid edge, and false if the edge wrapper is empty.</returns>
			/// <seealso cref="none"/>
			public static implicit operator bool(FaceEdge edge) { return edge._halfEdge; }

			/// <summary>
			/// Compares the current edge to the specified object to find if they are wrappers around the same edge.
			/// </summary>
			/// <param name="other">The object to be compared to the current edge.</param>
			/// <returns>Returns true if the specified object is an instance of <see cref="HalfEdge"/>,
			/// <see cref="VertexEdge"/>, or <see cref="FaceEdge"/>, and both it and the current edge are
			/// wrappers around the same edge, and false otherwise.</returns>
			public override bool Equals(object other) { return _halfEdge.Equals(other); }

			/// <summary>
			/// Calculates a 32-bit integer hash code for the current edge.
			/// </summary>
			/// <returns>A 32-bit signed integer hash code based on the owning topology and index of the current edge.</returns>
			public override int GetHashCode() { return _halfEdge.GetHashCode(); }

			#region Face Edge / Half Edge Comparisons

			/// <summary>
			/// Compares the current face edge to the specified half edge to find if they are wrappers around the same edge.
			/// </summary>
			/// <param name="other">The half edge to be compared to the current face edge.</param>
			/// <returns>Returns true if the specified half edge and the current face edge are both wrappers around the same edge, and false otherwise.</returns>
			public bool Equals(HalfEdge other) { return _halfEdge.Equals(other); }

			/// <summary>
			/// Compares the current face edge to the specified half edge.
			/// </summary>
			/// <param name="other">The other half edge to compare to the current face edge.</param>
			/// <returns>Returns a negative value if the current face edge is ordered before the specified half edge, a positive
			/// value if it is ordered after the specified half edge, and zero if they are wrappers around the same edge.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public int CompareTo(HalfEdge other) { return _halfEdge.CompareTo(other); }

			/// <summary>
			/// Compares two face edge wrappers for equality.
			/// </summary>
			/// <param name="lhs">The first face edge to compare.</param>
			/// <param name="rhs">The second half edge to compare.</param>
			/// <returns>Returns true if the two edges are wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator ==(FaceEdge lhs, HalfEdge rhs) { return lhs._halfEdge == rhs; }

			/// <summary>
			/// Compares two face edge wrappers for inequality.
			/// </summary>
			/// <param name="lhs">The first face edge to compare.</param>
			/// <param name="rhs">The second half edge to compare.</param>
			/// <returns>Returns true if the two edges are wrappers around two different edges, and false if they are wrappers around the same edge.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator !=(FaceEdge lhs, HalfEdge rhs) { return lhs._halfEdge != rhs; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered before the second.
			/// </summary>
			/// <param name="lhs">The first face edge to compare.</param>
			/// <param name="rhs">The second half edge to compare.</param>
			/// <returns>True if the current face edge is ordered before the specified half edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator < (FaceEdge lhs, HalfEdge rhs) { return lhs._halfEdge <  rhs; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered before the second or if they both are wrappers around the same edge.
			/// </summary>
			/// <param name="lhs">The first face edge to compare.</param>
			/// <param name="rhs">The second half edge to compare.</param>
			/// <returns>True if the current face edge is ordered before the specified half edge or if
			/// they are both wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator <=(FaceEdge lhs, HalfEdge rhs) { return lhs._halfEdge <= rhs; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered after the second.
			/// </summary>
			/// <param name="lhs">The first face edge to compare.</param>
			/// <param name="rhs">The second face edge to compare.</param>
			/// <returns>True if the current half edge is ordered after the specified half edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator > (FaceEdge lhs, HalfEdge rhs) { return lhs._halfEdge >  rhs; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered after the second or if they both are wrappers around the same edge.
			/// </summary>
			/// <param name="lhs">The first face edge to compare.</param>
			/// <param name="rhs">The second half edge to compare.</param>
			/// <returns>True if the current face edge is ordered after the specified half edge or if
			/// they are both wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator >=(FaceEdge lhs, HalfEdge rhs) { return lhs._halfEdge >= rhs; }

			#endregion

			#region Face Edge / Vertex Edge Comparisons

			/// <summary>
			/// Compares the current face edge to the specified vertex edge to find if they are wrappers around the same edge.
			/// </summary>
			/// <param name="other">The vertex edge to be compared to the current face edge.</param>
			/// <returns>Returns true if the specified vertex edge and the current face edge are both wrappers around the same edge, and false otherwise.</returns>
			public bool Equals(VertexEdge other) { return _halfEdge.Equals(other); }

			/// <summary>
			/// Compares the current face edge to the specified vertex edge.
			/// </summary>
			/// <param name="other">The other vertex edge to compare to the current face edge.</param>
			/// <returns>Returns a negative value if the current face edge is ordered before the specified vertex edge, a positive
			/// value if it is ordered after the specified vertex edge, and zero if they are wrappers around the same edge.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public int CompareTo(VertexEdge other) { return _halfEdge.CompareTo(other); }

			/// <summary>
			/// Compares two face edge wrappers for equality.
			/// </summary>
			/// <param name="lhs">The first face edge to compare.</param>
			/// <param name="rhs">The second vertex edge to compare.</param>
			/// <returns>Returns true if the two edges are wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator ==(FaceEdge lhs, VertexEdge rhs) { return lhs._halfEdge == rhs; }

			/// <summary>
			/// Compares two face edge wrappers for inequality.
			/// </summary>
			/// <param name="lhs">The first face edge to compare.</param>
			/// <param name="rhs">The second vertex edge to compare.</param>
			/// <returns>Returns true if the two edges are wrappers around two different edges, and false if they are wrappers around the same edge.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator !=(FaceEdge lhs, VertexEdge rhs) { return lhs._halfEdge != rhs; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered before the second.
			/// </summary>
			/// <param name="lhs">The first face edge to compare.</param>
			/// <param name="rhs">The second vertex edge to compare.</param>
			/// <returns>True if the current face edge is ordered before the specified vertex edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator < (FaceEdge lhs, VertexEdge rhs) { return lhs._halfEdge <  rhs; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered before the second or if they both are wrappers around the same edge.
			/// </summary>
			/// <param name="lhs">The first face edge to compare.</param>
			/// <param name="rhs">The second vertex edge to compare.</param>
			/// <returns>True if the current face edge is ordered before the specified vertex edge or if
			/// they are both wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator <=(FaceEdge lhs, VertexEdge rhs) { return lhs._halfEdge <= rhs; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered after the second.
			/// </summary>
			/// <param name="lhs">The first face edge to compare.</param>
			/// <param name="rhs">The second face edge to compare.</param>
			/// <returns>True if the current vertex edge is ordered after the specified vertex edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator > (FaceEdge lhs, VertexEdge rhs) { return lhs._halfEdge >  rhs; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered after the second or if they both are wrappers around the same edge.
			/// </summary>
			/// <param name="lhs">The first face edge to compare.</param>
			/// <param name="rhs">The second vertex edge to compare.</param>
			/// <returns>True if the current face edge is ordered after the specified vertex edge or if
			/// they are both wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator >=(FaceEdge lhs, VertexEdge rhs) { return lhs._halfEdge >= rhs; }

			#endregion

			#region Face Edge / Face Edge Comparisons

			/// <summary>
			/// Compares the current face edge to the specified face edge to find if they are wrappers around the same edge.
			/// </summary>
			/// <param name="other">The face edge to be compared to the current face edge.</param>
			/// <returns>Returns true if the specified face edge and the current face edge are both wrappers around the same edge, and false otherwise.</returns>
			public bool Equals(FaceEdge other) { return _halfEdge.Equals(other._halfEdge); }

			/// <summary>
			/// Compares the current face edge to the specified face edge.
			/// </summary>
			/// <param name="other">The other face edge to compare to the current face edge.</param>
			/// <returns>Returns a negative value if the current face edge is ordered before the specified face edge, a positive
			/// value if it is ordered after the specified face edge, and zero if they are wrappers around the same edge.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public int CompareTo(FaceEdge other) { return _halfEdge.CompareTo(other._halfEdge); }

			/// <summary>
			/// Compares two face edge wrappers for equality.
			/// </summary>
			/// <param name="lhs">The first face edge to compare.</param>
			/// <param name="rhs">The second face edge to compare.</param>
			/// <returns>Returns true if the two edges are wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator ==(FaceEdge lhs, FaceEdge rhs) { return lhs._halfEdge == rhs._halfEdge; }

			/// <summary>
			/// Compares two face edge wrappers for inequality.
			/// </summary>
			/// <param name="lhs">The first face edge to compare.</param>
			/// <param name="rhs">The second face edge to compare.</param>
			/// <returns>Returns true if the two edges are wrappers around two different edges, and false if they are wrappers around the same edge.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator !=(FaceEdge lhs, FaceEdge rhs) { return lhs._halfEdge != rhs._halfEdge; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered before the second.
			/// </summary>
			/// <param name="lhs">The first face edge to compare.</param>
			/// <param name="rhs">The second face edge to compare.</param>
			/// <returns>True if the current face edge is ordered before the specified face edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator < (FaceEdge lhs, FaceEdge rhs) { return lhs._halfEdge <  rhs._halfEdge; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered before the second or if they both are wrappers around the same edge.
			/// </summary>
			/// <param name="lhs">The first face edge to compare.</param>
			/// <param name="rhs">The second face edge to compare.</param>
			/// <returns>True if the current face edge is ordered before the specified face edge or if
			/// they are both wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator <=(FaceEdge lhs, FaceEdge rhs) { return lhs._halfEdge <= rhs._halfEdge; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered after the second.
			/// </summary>
			/// <param name="lhs">The first face edge to compare.</param>
			/// <param name="rhs">The second face edge to compare.</param>
			/// <returns>True if the current face edge is ordered after the specified face edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator > (FaceEdge lhs, FaceEdge rhs) { return lhs._halfEdge >  rhs._halfEdge; }

			/// <summary>
			/// Compares the two vertices to determine if the first is ordered after the second or if they both are wrappers around the same edge.
			/// </summary>
			/// <param name="lhs">The first face edge to compare.</param>
			/// <param name="rhs">The second face edge to compare.</param>
			/// <returns>True if the current face edge is ordered after the specified face edge or if
			/// they are both wrappers around the same edge, and false otherwise.</returns>
			/// <remarks><para>It is assumed that both edges belong to the same topology.
			/// If they do not, then the behavior of this function is undefined.</para></remarks>
			public static bool operator >=(FaceEdge lhs, FaceEdge rhs) { return lhs._halfEdge >= rhs._halfEdge; }

			#endregion

			/// <summary>
			/// Converts the edge to string representation, appropriate for diagnositic display.
			/// </summary>
			/// <returns>A string representation of the edge.</returns>
			public override string ToString()
			{
				return string.Format("Face Edge {0} (Twin Edge: {1}, Edges p/n: {2}/{3}), (Faces n/f: {6}/{7}), (Vertices p/n: {4}/{5})", index, twinIndex, prev.index, next.index, nearFace.index, farFace.index, prevVertex.index, nextVertex.index);
			}
		}

		/// <summary>
		/// An indexer of the half edges in a topology, used for enumerating half edges.  Satisfies the
		/// concept of <see cref="IEnumerable{HalfEdge}"/>, enabling it to be used in <c>foreach</c> loops.
		/// </summary>
		public struct HalfEdgesIndexer
		{
			private Topology _topology;

			/// <summary>
			/// Constructs an indexer for the half edges in the given topology.
			/// </summary>
			/// <param name="topology">The topology whose half edges are to be enumerated.</param>
			public HalfEdgesIndexer(Topology topology) { _topology = topology; }

			/// <summary>
			/// Accesses a half edge by its integer index.
			/// </summary>
			/// <param name="i">The index of the half edge to access.</param>
			/// <returns>The half edge with the given index.</returns>
			public HalfEdge this[int i] { get { return new HalfEdge(_topology, i); } }

			/// <summary>
			/// The number of half edges accessible by this indexer.
			/// </summary>
			public int Count { get { return _topology.edgeData.Length; } }

			/// <summary>
			/// Creates an enumerator for the half edges of the current indexer.
			/// </summary>
			/// <returns>An enumerator for the half edges of the current indexer.</returns>
			public EdgeEnumerator GetEnumerator() { return new EdgeEnumerator(_topology); }

			/// <summary>
			/// An enumerator of a topology's half edges.  Satisfies the concept of <see cref="IEnumerator{HalfEdge}"/>,
			/// enabling it to be used in <c>foreach</c> loops.
			/// </summary>
			public struct EdgeEnumerator
			{
				private Topology _topology;
				private int _current;

				/// <summary>
				/// Constructs an instance of a half edge enumerator, given a topology containing the half edges.
				/// </summary>
				/// <param name="topology">The topology to which the enumerated half edges belong.</param>
				public EdgeEnumerator(Topology topology) { _topology = topology; _current = -1; }

				/// <summary>
				/// A half edge wrapper around the current half edge being enumerated.
				/// </summary>
				public HalfEdge Current { get { return new HalfEdge(_topology, _current); } }

				/// <summary>
				/// Updates the enumerator to the next half edge in the topology.
				/// </summary>
				/// <returns>True if it moved to the next valid half edge, or false if there are no more half edges to be enumerated.</returns>
				public bool MoveNext() { return ++_current < _topology.edgeData.Length; }

				/// <summary>
				/// Resets the enumerator back to its original state, so that another call to <see cref="MoveNext"/>
				/// will have <see cref="Current"/> return the first half edge of the enumerated sequence.
				/// </summary>
				public void Reset() { throw new NotSupportedException(); }
			}
		}

		/// <summary>
		/// Returns an indexer of all the half edges in the current topology.  Can be used in <c>foreach</c> loops.
		/// </summary>
		/// <seealso cref="enumerableHalfEdges"/>
		public HalfEdgesIndexer halfEdges { get { return new HalfEdgesIndexer(this); } }

		/// <summary>
		/// Returns an enumerable sequence of all the half edges in the current topology.
		/// Implements <see cref="IEnumerable{HalfEdge}"/>, for circumstances where
		/// the actual interface and not just the concept is needed.
		/// </summary>
		/// <seealso cref="halfEdges"/>
		public IEnumerable<HalfEdge> enumerableHalfEdges
		{
			get
			{
				foreach (var halfEdge in halfEdges)
				{
					yield return halfEdge;
				}
			}
		}

		/// <summary>
		/// An indexer of the vertex edges in a topology, used for enumerating vertex edges.  Satisfies the
		/// concept of <see cref="IEnumerable{VertexEdge}"/>, enabling it to be used in <c>foreach</c> loops.
		/// </summary>
		public struct VertexEdgesIndexer
		{
			private Topology _topology;

			/// <summary>
			/// Constructs an indexer for the vertex edges in the given topology.
			/// </summary>
			/// <param name="topology">The topology whose vertex edges are to be enumerated.</param>
			public VertexEdgesIndexer(Topology topology) { _topology = topology; }

			/// <summary>
			/// Accesses a vertex edge by its integer index.
			/// </summary>
			/// <param name="i">The index of the vertex edge to access.</param>
			/// <returns>The vertex edge with the given index.</returns>
			public VertexEdge this[int i] { get { return new VertexEdge(_topology, i); } }

			/// <summary>
			/// The number of vertex edges accessible by this indexer.
			/// </summary>
			public int Count { get { return _topology.edgeData.Length; } }

			/// <summary>
			/// Creates an enumerator for the vertex edges of the current indexer.
			/// </summary>
			/// <returns>An enumerator for the vertex edges of the current indexer.</returns>
			public EdgeEnumerator GetEnumerator() { return new EdgeEnumerator(_topology); }

			/// <summary>
			/// An enumerator of a topology's vertex edges.  Satisfies the concept of <see cref="IEnumerator{VertexEdge}"/>,
			/// enabling it to be used in <c>foreach</c> loops.
			/// </summary>
			public struct EdgeEnumerator
			{
				private Topology _topology;
				private int _current;

				/// <summary>
				/// Constructs an instance of a vertex edge enumerator, given a topology containing the vertex edges.
				/// </summary>
				/// <param name="topology">The topology to which the enumerated vertex edges belong.</param>
				public EdgeEnumerator(Topology topology) { _topology = topology; _current = -1; }

				/// <summary>
				/// A vertex edge wrapper around the current vertex edge being enumerated.
				/// </summary>
				public VertexEdge Current { get { return new VertexEdge(_topology, _current); } }

				/// <summary>
				/// Updates the enumerator to the next vertex edge in the topology.
				/// </summary>
				/// <returns>True if it moved to the next valid vertex edge, or false if there are no more vertex edges to be enumerated.</returns>
				public bool MoveNext() { return ++_current < _topology.edgeData.Length; }

				/// <summary>
				/// Resets the enumerator back to its original state, so that another call to <see cref="MoveNext"/>
				/// will have <see cref="Current"/> return the first vertex edge of the enumerated sequence.
				/// </summary>
				public void Reset() { throw new NotSupportedException(); }
			}
		}

		/// <summary>
		/// Returns an indexer of all the vertex edges in the current topology.  Can be used in <c>foreach</c> loops.
		/// </summary>
		/// <seealso cref="enumerableVertexEdges"/>
		public VertexEdgesIndexer vertexEdges { get { return new VertexEdgesIndexer(this); } }

		/// <summary>
		/// Returns an enumerable sequence of all the vertex edges in the current topology.
		/// Implements <see cref="IEnumerable{VertexEdge}"/>, for circumstances where
		/// the actual interface and not just the concept is needed.
		/// </summary>
		/// <seealso cref="vertexEdges"/>
		public IEnumerable<VertexEdge> enumerableVertexEdges
		{
			get
			{
				foreach (var vertexEdge in vertexEdges)
				{
					yield return vertexEdge;
				}
			}
		}

		/// <summary>
		/// An indexer of the face edges in a topology, used for enumerating face edges.  Satisfies the
		/// concept of <see cref="IEnumerable{FaceEdge}"/>, enabling it to be used in <c>foreach</c> loops.
		/// </summary>
		public struct FaceEdgesIndexer
		{
			private Topology _topology;

			/// <summary>
			/// Constructs an indexer for the face edges in the given topology.
			/// </summary>
			/// <param name="topology">The topology whose face edges are to be enumerated.</param>
			public FaceEdgesIndexer(Topology topology) { _topology = topology; }

			/// <summary>
			/// Accesses a face edge by its integer index.
			/// </summary>
			/// <param name="i">The index of the face edge to access.</param>
			/// <returns>The face edge with the given index.</returns>
			public FaceEdge this[int i] { get { return new FaceEdge(_topology, i); } }

			/// <summary>
			/// The number of face edges accessible by this indexer.
			/// </summary>
			public int Count { get { return _topology.edgeData.Length; } }

			/// <summary>
			/// Creates an enumerator for the face edges of the current indexer.
			/// </summary>
			/// <returns>An enumerator for the face edges of the current indexer.</returns>
			public EdgeEnumerator GetEnumerator() { return new EdgeEnumerator(_topology); }

			/// <summary>
			/// An enumerator of a topology's face edges.  Satisfies the concept of <see cref="IEnumerator{FaceEdge}"/>,
			/// enabling it to be used in <c>foreach</c> loops.
			/// </summary>
			public struct EdgeEnumerator
			{
				private Topology _topology;
				private int _current;

				/// <summary>
				/// Constructs an instance of a face edge enumerator, given a topology containing the face edges.
				/// </summary>
				/// <param name="topology">The topology to which the enumerated face edges belong.</param>
				public EdgeEnumerator(Topology topology) { _topology = topology; _current = -1; }

				/// <summary>
				/// A face edge wrapper around the current face edge being enumerated.
				/// </summary>
				public FaceEdge Current { get { return new FaceEdge(_topology, _current); } }

				/// <summary>
				/// Updates the enumerator to the next face edge in the topology.
				/// </summary>
				/// <returns>True if it moved to the next valid face edge, or false if there are no more face edges to be enumerated.</returns>
				public bool MoveNext() { return ++_current < _topology.edgeData.Length; }

				/// <summary>
				/// Resets the enumerator back to its original state, so that another call to <see cref="MoveNext"/>
				/// will have <see cref="Current"/> return the first face edge of the enumerated sequence.
				/// </summary>
				public void Reset() { throw new NotSupportedException(); }
			}
		}

		/// <summary>
		/// Returns an indexer of all the face edges in the current topology.  Can be used in <c>foreach</c> loops.
		/// </summary>
		/// <seealso cref="enumerableFaceEdges"/>
		public FaceEdgesIndexer faceEdges { get { return new FaceEdgesIndexer(this); } }

		/// <summary>
		/// Returns an enumerable sequence of all the face edges in the current topology.
		/// Implements <see cref="IEnumerable{FaceEdge}"/>, for circumstances where
		/// the actual interface and not just the concept is needed.
		/// </summary>
		/// <seealso cref="faceEdges"/>
		public IEnumerable<FaceEdge> enumerableFaceEdges
		{
			get
			{
				foreach (var faceEdge in faceEdges)
				{
					yield return faceEdge;
				}
			}
		}
	}

	/// <summary>
	/// Generic interface for accessing attribute values of topology edges.
	/// </summary>
	/// <typeparam name="T">The type of the attribute values.</typeparam>
	/// <remarks>
	/// <para>Instead of working with integer indices everywhere, this interface allows attributes to be
	/// indexed by instances of one of the three edge structures directly.</para>
	/// </remarks>
	public interface IEdgeAttribute<T> : IList<T>
	{
		/// <summary>
		/// Lookup the attribute value for the half edge indicated.
		/// </summary>
		/// <param name="i">The index of the half edge whose attribute value is desired.</param>
		/// <returns>The attribute value for the half edge indicated.</returns>
		new T this[int i] { get; set; }

		/// <summary>
		/// Lookup the attribute value for the half edge indicated.
		/// </summary>
		/// <param name="e">The half edge whose attribute value is desired.</param>
		/// <returns>The attribute value for the half edge indicated.</returns>
		T this[Topology.HalfEdge e] { get; set; }

		/// <summary>
		/// Lookup the attribute value for a half edge relative to a neighboring vertex.
		/// </summary>
		/// <param name="e">The half edge (represented as a vertex edge) whose attribute value is desired.</param>
		/// <returns>The attribute value for the indicated half edge, relative to its near vertex.</returns>
		T this[Topology.VertexEdge e] { get; set; }

		/// <summary>
		/// Lookup the attribute value for a half edge relative to a neighboring face.
		/// </summary>
		/// <param name="e">The half edge (represented as a face edge) whose attribute value is desired.</param>
		/// <returns>The attribute value for the indicated half edge, relative to its near face.</returns>
		T this[Topology.FaceEdge e] { get; set; }
	}

	/// <summary>
	/// A simple low-storage edge attribute class for situations when all edges have the exact same value for a particular attribute.
	/// </summary>
	/// <typeparam name="T">The type of the constant attribute value.</typeparam>
	public struct EdgeAttributeConstantWrapper<T> : IEdgeAttribute<T>
	{
		/// <summary>
		/// The attribute value shared by all edges.
		/// </summary>
		public T constant;

		/// <summary>
		/// Constructs an edge attribute constant wrapper with the given constant value.
		/// </summary>
		/// <param name="constant">The constant attribute value shared by all edges.</param>
		public EdgeAttributeConstantWrapper(T constant)
		{
			this.constant = constant;
		}

		/// <inheritdoc/>
		public T this[int i]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant edge attribute cannot be changed."); }
		}

		/// <inheritdoc/>
		public T this[Topology.HalfEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant edge attribute cannot be changed."); }
		}

		/// <inheritdoc/>
		public T this[Topology.VertexEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant edge attribute cannot be changed."); }
		}

		/// <inheritdoc/>
		public T this[Topology.FaceEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant edge attribute cannot be changed."); }
		}

		/// <inheritdoc/>
		public int Count { get { throw new NotSupportedException(); } }
		/// <inheritdoc/>
		public bool IsReadOnly { get { return true; } }
		/// <inheritdoc/>
		public void Add(T item) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public void Clear() { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public bool Contains(T item) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public void CopyTo(T[] array, int arrayIndex) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public IEnumerator<T> GetEnumerator() { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public int IndexOf(T item) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public void Insert(int index, T item) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public bool Remove(T item) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public void RemoveAt(int index) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	/// <summary>
	/// An edge attribute wrapper around a raw array of edge attributes.
	/// </summary>
	/// <typeparam name="T">The type of the attribute values.</typeparam>
	public struct EdgeAttributeArrayWrapper<T> : IEdgeAttribute<T>
	{
		/// <summary>
		/// The array of edge attribute values that is being wrapped.
		/// </summary>
		public readonly T[] array;

		/// <summary>
		/// Constructs an edge attribute array wrapper around the given array.
		/// </summary>
		/// <param name="array">The array of edge attribute values to be wrapped.</param>
		public EdgeAttributeArrayWrapper(T[] array)
		{
			this.array = array;
		}

		/// <summary>
		/// Constructs an edge attribute array wrapper around an internally created array with the given number of elements.
		/// </summary>
		/// <param name="elementCount">The number of elements that the created array will contain.</param>
		public EdgeAttributeArrayWrapper(int elementCount)
		{
			array = new T[elementCount];
		}

		/// <summary>
		/// Implicitly converts an array wrapper to a raw array by simply returning the wrapped array.
		/// </summary>
		/// <param name="arrayWrapper">The array wrapper to convert.</param>
		/// <returns>The raw array wrapped by the converted array wrapper.</returns>
		public static implicit operator T[](EdgeAttributeArrayWrapper<T> arrayWrapper)
		{
			return arrayWrapper.array;
		}

		/// <summary>
		/// Implicitly converts a raw array to an array wrapper by constructing a wrapper around the array.
		/// </summary>
		/// <param name="array">The array to be converted.</param>
		/// <returns>An array wrapper around the converted array.</returns>
		public static implicit operator EdgeAttributeArrayWrapper<T>(T[] array)
		{
			return new EdgeAttributeArrayWrapper<T>(array);
		}

		/// <summary>
		/// Accesses the edge attribute value in the array corresponding to the edge with the given index.
		/// </summary>
		/// <param name="i">The index of the edge whose attribute value is to be accessed.</param>
		/// <returns>The edge attribute value corresponding to the edge with the given index.</returns>
		public T this[int i]
		{
			get { return array[i]; }
			set { array[i] = value; }
		}

		/// <summary>
		/// Accesses the edge attribute value in the array corresponding to the given edge.
		/// </summary>
		/// <param name="e">The edge whose attribute value is to be accessed.</param>
		/// <returns>The edge attribute value corresponding to the given edge.</returns>
		public T this[Topology.HalfEdge e]
		{
			get { return array[e.index]; }
			set { array[e.index] = value; }
		}

		/// <summary>
		/// Accesses the edge attribute value in the array corresponding to the given edge.
		/// </summary>
		/// <param name="e">The edge whose attribute value is to be accessed.</param>
		/// <returns>The edge attribute value corresponding to the given edge.</returns>
		public T this[Topology.VertexEdge e]
		{
			get { return array[e.index]; }
			set { array[e.index] = value; }
		}

		/// <summary>
		/// Accesses the edge attribute value in the array corresponding to the given edge.
		/// </summary>
		/// <param name="e">The edge whose attribute value is to be accessed.</param>
		/// <returns>The edge attribute value corresponding to the given edge.</returns>
		public T this[Topology.FaceEdge e]
		{
			get { return array[e.index]; }
			set { array[e.index] = value; }
		}

		/// <summary>
		/// The number of edge attribute values stored in the wrapped array.
		/// </summary>
		public int Count { get { return array.Length; } }

		/// <summary>
		/// Indicates if the collection is read-only and thus cannot have items inserted into or removed from it.  Always true.
		/// </summary>
		public bool IsReadOnly { get { return true; } }

		/// <summary>
		/// Not supported.
		/// </summary>
		/// <inheritdoc/>
		public void Add(T item) { throw new NotSupportedException(); }

		/// <summary>
		/// Not supported.
		/// </summary>
		/// <inheritdoc/>
		public void Clear() { throw new NotSupportedException(); }

		/// <summary>
		/// Checks whether the given edge attribute value is contained within the wrapped array.
		/// </summary>
		/// <param name="item">The edge attribute value to be searched for.</param>
		/// <returns>True if the given edge attribute value is stored within the wrapped array, and false if it is not.</returns>
		public bool Contains(T item) { return ((IList<T>)array).Contains(item); }

		/// <summary>
		/// Copies the edge attribute values of the wrapped array into the specified array.
		/// </summary>
		/// <param name="array">The target array into which edge attribute values will be copied.</param>
		/// <param name="arrayIndex">The starting index at which the copy of values will begin.</param>
		public void CopyTo(T[] array, int arrayIndex) { this.array.CopyTo(array, arrayIndex); }

		/// <summary>
		/// Gets an enumerator over all the edge attribute values in the wrapped array.
		/// </summary>
		/// <returns>An enumerator over all the edge attribute values in the wrapped array.</returns>
		public IEnumerator<T> GetEnumerator() { return ((IList<T>)array).GetEnumerator(); }

		/// <summary>
		/// Searches for the specified edge attribute value and returns the index at which the first occurrence is found.
		/// </summary>
		/// <param name="item">The edge attribute value to be searched for.</param>
		/// <returns>The index of the first occurrence of the given value, or -1 if no occurrence was found.</returns>
		public int IndexOf(T item) { return ((IList<T>)array).IndexOf(item); }

		/// <summary>
		/// Not supported.
		/// </summary>
		/// <inheritdoc/>
		public void Insert(int index, T item) { throw new NotSupportedException(); }

		/// <summary>
		/// Not supported.
		/// </summary>
		/// <inheritdoc/>
		public bool Remove(T item) { throw new NotSupportedException(); }

		/// <summary>
		/// Not supported.
		/// </summary>
		/// <inheritdoc/>
		public void RemoveAt(int index) { throw new NotSupportedException(); }

		/// <summary>
		/// Gets an enumerator over all the edge attribute values in the wrapped array.
		/// </summary>
		/// <returns>An enumerator over all the edge attribute values in the wrapped array.</returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	/// <summary>
	/// An edge attribute wrapper around another edge attribute collection, which accesses.
	/// the edge attributes of every edge's twin, instead of the edge's own attribute value.
	/// </summary>
	/// <typeparam name="T">The type of the attribute values.</typeparam>
	public struct TwinEdgeAttributeWrapper<T> : IEdgeAttribute<T>
	{
		/// <summary>
		/// The topology to which the edges belong.
		/// </summary>
		public Topology topology;

		/// <summary>
		/// The underlying edge attributes collection that is being wrapped.
		/// </summary>
		public IEdgeAttribute<T> underlyingAttribute;

		/// <summary>
		/// Constructs a twin edge attribute wrapper around the given underlying attribute collection.
		/// </summary>
		/// <param name="topology">The topology to which the edges belong.</param>
		/// <param name="underlyingAttribute">The underlying edge attributes to be wrapped.</param>
		public TwinEdgeAttributeWrapper(Topology topology, IEdgeAttribute<T> underlyingAttribute)
		{
			this.topology = topology;
			this.underlyingAttribute = underlyingAttribute;
		}

		/// <summary>
		/// Accesses the edge attribute value in the underling edge attributes collection corresponding to the twin of the edge with the given index.
		/// </summary>
		/// <param name="i">The index of the edge whose twin's attribute value is to be accessed.</param>
		/// <returns>The edge attribute value corresponding to the twin of the edge with the given index.</returns>
		public T this[int i]
		{
			get { return underlyingAttribute[topology.halfEdges[i].twinIndex]; }
			set { underlyingAttribute[topology.halfEdges[i].twinIndex] = value; }
		}

		/// <summary>
		/// Accesses the edge attribute value in the underling edge attributes collection corresponding to the given edge's twin.
		/// </summary>
		/// <param name="e">The edge whose twin's attribute value is to be accessed.</param>
		/// <returns>The underlying edge attribute value corresponding to the given edge's twin.</returns>
		public T this[Topology.HalfEdge e]
		{
			get { return underlyingAttribute[e.twin]; }
			set { underlyingAttribute[e.twin] = value; }
		}

		/// <summary>
		/// Accesses the edge attribute value in the underling edge attributes collection corresponding to the given edge's twin.
		/// </summary>
		/// <param name="e">The edge whose twin's attribute value is to be accessed.</param>
		/// <returns>The underlying edge attribute value corresponding to the given edge's twin.</returns>
		public T this[Topology.VertexEdge e]
		{
			get { return underlyingAttribute[e.twin]; }
			set { underlyingAttribute[e.twin] = value; }
		}

		/// <summary>
		/// Accesses the edge attribute value in the underling edge attributes collection corresponding to the given edge's twin.
		/// </summary>
		/// <param name="e">The edge whose twin's attribute value is to be accessed.</param>
		/// <returns>The underlying edge attribute value corresponding to the given edge's twin.</returns>
		public T this[Topology.FaceEdge e]
		{
			get { return underlyingAttribute[e.twin]; }
			set { underlyingAttribute[e.twin] = value; }
		}

		/// <summary>
		/// The number of edge attribute values stored in the wrapped attributes collection.
		/// </summary>
		public int Count { get { return underlyingAttribute.Count; } }

		/// <summary>
		/// Indicates if the collection is read-only and thus cannot have items inserted into or removed from it.  Always true.
		/// </summary>
		public bool IsReadOnly { get { return true; } }

		/// <summary>
		/// Not supported.
		/// </summary>
		/// <inheritdoc/>
		public void Add(T item) { throw new NotSupportedException(); }

		/// <summary>
		/// Not supported.
		/// </summary>
		/// <inheritdoc/>
		public void Clear() { throw new NotSupportedException(); }

		/// <summary>
		/// Checks whether the given edge attribute value is contained within the wrapped attributes collection.
		/// </summary>
		/// <param name="item">The edge attribute value to be searched for.</param>
		/// <returns>True if the given edge attribute value is stored within the wrapped attributes collection, and false if it is not.</returns>
		public bool Contains(T item) { return underlyingAttribute.Contains(item); }

		/// <summary>
		/// Copies the edge attribute values of the wrapped attributes collection into the specified array.
		/// </summary>
		/// <param name="array">The target array into which edge attribute values will be copied.</param>
		/// <param name="arrayIndex">The starting index at which the copy of values will begin.</param>
		public void CopyTo(T[] array, int arrayIndex) { underlyingAttribute.CopyTo(array, arrayIndex); }

		/// <summary>
		/// Gets an enumerator over all the edge attribute values in the wrapped attributes collection.
		/// </summary>
		/// <returns>An enumerator over all the edge attribute values in the wrapped attributes collection.</returns>
		public IEnumerator<T> GetEnumerator() { return underlyingAttribute.GetEnumerator(); }

		/// <summary>
		/// Searches for the specified edge attribute value and returns the index at which the first occurrence is found.
		/// </summary>
		/// <param name="item">The edge attribute value to be searched for.</param>
		/// <returns>The index of the first occurrence of the given value, or -1 if no occurrence was found.</returns>
		public int IndexOf(T item) { return underlyingAttribute.IndexOf(item); }

		/// <summary>
		/// Not supported.
		/// </summary>
		/// <inheritdoc/>
		public void Insert(int index, T item) { throw new NotSupportedException(); }

		/// <summary>
		/// Not supported.
		/// </summary>
		/// <inheritdoc/>
		public bool Remove(T item) { throw new NotSupportedException(); }

		/// <summary>
		/// Not supported.
		/// </summary>
		/// <inheritdoc/>
		public void RemoveAt(int index) { throw new NotSupportedException(); }

		/// <summary>
		/// Gets an enumerator over all the edge attribute values in the wrapped attributes collection.
		/// </summary>
		/// <returns>An enumerator over all the edge attribute values in the wrapped attributes collection.</returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	/// <summary>
	/// Abstract base class for edge attributes that also derives from <see cref="ScriptableObject"/>
	/// and can therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	/// <typeparam name="T">The type of the attribute values.</typeparam>
	public abstract class EdgeAttribute<T> : ScriptableObject, IEdgeAttribute<T>
	{
		/// <inheritdoc/>
		public abstract T this[int i] { get; set; }
		/// <inheritdoc/>
		public abstract T this[Topology.HalfEdge e] { get; set; }
		/// <inheritdoc/>
		public abstract T this[Topology.VertexEdge e] { get; set; }
		/// <inheritdoc/>
		public abstract T this[Topology.FaceEdge e] { get; set; }

		/// <inheritdoc/>
		public virtual int Count { get { throw new NotSupportedException(); } }
		/// <inheritdoc/>
		public virtual bool IsReadOnly { get { return true; } }
		/// <inheritdoc/>
		public virtual void Add(T item) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public virtual void Clear() { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public virtual bool Contains(T item) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public virtual void CopyTo(T[] array, int arrayIndex) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public virtual IEnumerator<T> GetEnumerator() { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public virtual int IndexOf(T item) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public virtual void Insert(int index, T item) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public virtual bool Remove(T item) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public virtual void RemoveAt(int index) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	/// <summary>
	/// A simple low-storage edge attribute class for situations when all edges have the exact same value for a particular attribute.
	/// </summary>
	/// <typeparam name="T">The type of the constant attribute value.</typeparam>
	public class EdgeConstantAttribute<T> : EdgeAttribute<T> where T : new()
	{
		/// <summary>
		/// The attribute value shared by all edges.
		/// </summary>
		public T constant;

		/// <summary>
		/// Creates an instance of a class derived from this one, and assigns it the constant value.
		/// </summary>
		/// <typeparam name="TDerived">The type of the derived class.</typeparam>
		/// <param name="constant">The constant attribute value shared by all edges.</param>
		/// <returns>An instance of the derived class using the constant value provided.</returns>
		/// <remarks><para>This is a convenience function for derived classes to use, which are needed
		/// because Unity is unable to serialize generic classes directly, but can serialize non-generic
		/// classes which are derived from generic classes but given a concrete type.</para></remarks>
		protected static TDerived CreateDerived<TDerived>(T constant) where TDerived : EdgeConstantAttribute<T>
		{
			var instance = CreateInstance<TDerived>();
			instance.constant = constant;
			return instance;
		}

		/// <inheritdoc/>
		public override T this[int i]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant edge attribute cannot be changed."); }
		}

		/// <inheritdoc/>
		public override T this[Topology.HalfEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant edge attribute cannot be changed."); }
		}

		/// <inheritdoc/>
		public override T this[Topology.VertexEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant edge attribute cannot be changed."); }
		}

		/// <inheritdoc/>
		public override T this[Topology.FaceEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant edge attribute cannot be changed."); }
		}
	}

	/// <summary>
	/// An edge attribute wrapper around a raw array of edge attributes, which also derives from
	/// <see cref="ScriptableObject"/> and can therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	/// <typeparam name="T">The type of the attribute values.</typeparam>
	/// <seealso cref="EdgeAttributeArrayWrapper{T}"/>
	public class EdgeArrayAttribute<T> : EdgeAttribute<T> where T : new()
	{
		/// <summary>
		/// The array of edge attribute values that is being wrapped.
		/// </summary>
		public T[] array;

		/// <summary>
		/// Creates an instance of a class derived from this one, and assigns it the array data provided.
		/// </summary>
		/// <typeparam name="TDerived">The type of the derived class.</typeparam>
		/// <param name="array">The array of edge attribute values to be wrapped.</param>
		/// <returns>An instance of the derived class using the array data provided.</returns>
		/// <remarks><para>This is a convenience function for derived classes to use, which are needed
		/// because Unity is unable to serialize generic classes directly, but can serialize non-generic
		/// classes which are derived from generic classes but given a concrete type.</para></remarks>
		protected static TDerived CreateDerived<TDerived>(T[] array) where TDerived : EdgeArrayAttribute<T>
		{
			var instance = CreateInstance<TDerived>();
			instance.array = array;
			return instance;
		}

		/// <summary>
		/// Creates an instance of a class derived from this one, and creates an array of the specified size.
		/// </summary>
		/// <typeparam name="TDerived">The type of the derived class.</typeparam>
		/// <param name="edgeCount">The number of elements that the created array will contain.</param>
		/// <returns>An instance of the derived class using an array created with the specified size.</returns>
		/// <remarks><para>This is a convenience function for derived classes to use, which are needed
		/// because Unity is unable to serialize generic classes directly, but can serialize non-generic
		/// classes which are derived from generic classes but given a concrete type.</para></remarks>
		protected static TDerived CreateDerived<TDerived>(int edgeCount) where TDerived : EdgeArrayAttribute<T>
		{
			return CreateDerived<TDerived>(new T[edgeCount]);
		}

		/// <inheritdoc/>
		public override T this[int i]
		{
			get { return array[i]; }
			set { array[i] = value; }
		}

		/// <inheritdoc/>
		public override T this[Topology.HalfEdge e]
		{
			get { return array[e.index]; }
			set { array[e.index] = value; }
		}

		/// <inheritdoc/>
		public override T this[Topology.VertexEdge e]
		{
			get { return array[e.index]; }
			set { array[e.index] = value; }
		}

		/// <inheritdoc/>
		public override T this[Topology.FaceEdge e]
		{
			get { return array[e.index]; }
			set { array[e.index] = value; }
		}

		/// <inheritdoc/>
		public override int Count { get { return array.Length; } }
		/// <inheritdoc/>
		public override bool Contains(T item) { return ((IList<T>)array).Contains(item); }
		/// <inheritdoc/>
		public override void CopyTo(T[] array, int arrayIndex) { this.array.CopyTo(array, arrayIndex); }
		/// <inheritdoc/>
		public override IEnumerator<T> GetEnumerator() { return ((IList<T>)array).GetEnumerator(); }
		/// <inheritdoc/>
		public override int IndexOf(T item) { return ((IList<T>)array).IndexOf(item); }
	}

	/// <summary>
	/// Extension methods involving edges and edge attributes.
	/// </summary>
	public static class EdgeExtensions
	{
		/// <summary>
		/// Creates an edgeattribute wrapper around an array.
		/// </summary>
		/// <typeparam name="T">The type of elements in the array.</typeparam>
		/// <param name="array">The array to be wrapped.</param>
		/// <returns>An edge attribute wrapper around the array.</returns>
		public static EdgeAttributeArrayWrapper<T> AsEdgeAttribute<T>(this T[] array)
		{
			return new EdgeAttributeArrayWrapper<T>(array);
		}
	}
}
