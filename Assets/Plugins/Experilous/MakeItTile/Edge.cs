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

			public int twin; // The other edge between the same two vertices and same two faces, but going the opposite direction.
			public int vNext; // The next edge after this one when going clockwise around the implicit near vertex.
			public int fNext; // The next edge this one when going clockwise around the implicit near face.
			public int vertex; // The vertex at the far end of this edge that preceeds the below face when going clockwise around the implicit near vertex.
			public int face; // The face on the far side of this edge that follows after the above vertex when going clockwise around the implicit near vertex.
			public EdgeWrap wrap; // The wrap-around flags when transitioning among vertices, edges, and faces.

			public EdgeData(int twin, int vNext, int fNext, int vertex, int face, EdgeWrap wrap = EdgeWrap.None)
			{
				this.twin = twin;
				this.vNext = vNext;
				this.fNext = fNext;
				this.vertex = vertex;
				this.face = face;
				this.wrap = wrap;
			}

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
		/// 
		/// <para>In addition to providing access to its two vertex and face neighbors, access is also provided to previous and next half-
		/// edges in the two circular linked lists that every half-edge belongs to, the list of half-edges around a near vertex, and the list
		/// of half-edges around a near face.  Note that getting the next object requires one less lookup than getting the previous object,
		/// and is therefore recommended for the majority of cases.</para>
		/// </remarks>
		public struct HalfEdge : IEquatable<HalfEdge>, IEquatable<VertexEdge>, IEquatable<FaceEdge>, IComparable<HalfEdge>, IComparable<VertexEdge>, IComparable<FaceEdge>
		{
			private Topology _topology;
			private int _index;

			public HalfEdge(Topology topology, int index)
			{
				_topology = topology;
				_index = index;
			}

			public HalfEdge(HalfEdge edge)
			{
				_topology = edge._topology;
				_index = edge._index;
			}

			public HalfEdge(VertexEdge vertexEdge)
			{
				_topology = vertexEdge.topology;
				_index = vertexEdge.index;
			}

			public HalfEdge(FaceEdge faceEdge)
			{
				_topology = faceEdge.topology;
				_index = faceEdge.index;
			}

			public static HalfEdge none { get { return new HalfEdge(); } }

			public Topology topology { get { return _topology; } }

			public int index { get { return _index; } }
			public int twinIndex { get { return _topology.edgeData[_index].twin; } }
			public bool isFirstTwin { get { return index < twinIndex; } }
			public bool isSecondTwin { get { return index > twinIndex; } }

			public HalfEdge twin { get { return new HalfEdge(_topology, _topology.edgeData[_index].twin); } }
			public HalfEdge firstTwin { get { return isFirstTwin ? this : twin; } }
			public HalfEdge secondTwin { get { return isSecondTwin ? this : twin; } }
			public HalfEdge prevAroundVertex { get { return new HalfEdge(_topology, _topology.edgeData[_topology.edgeData[_index].twin].fNext); } }
			public HalfEdge nextAroundVertex { get { return new HalfEdge(_topology, _topology.edgeData[_index].vNext); } }
			public HalfEdge prevAroundFace { get { return new HalfEdge(_topology, _topology.edgeData[_topology.edgeData[_index].vNext].twin); } }
			public HalfEdge nextAroundFace { get { return new HalfEdge(_topology, _topology.edgeData[_index].fNext); } }
			public Vertex vertex { get { return new Vertex(_topology, _topology.edgeData[_index].vertex); } }
			public Vertex nearVertex { get { return new Vertex(_topology, _topology.edgeData[_topology.edgeData[_index].twin].vertex); } }
			public Vertex farVertex { get { return new Vertex(_topology, _topology.edgeData[_index].vertex); } }
			public Face face { get { return new Face(_topology, _topology.edgeData[_index].face); } }
			public Face nearFace { get { return new Face(_topology, _topology.edgeData[_topology.edgeData[_index].twin].face); } }
			public Face farFace { get { return new Face(_topology, _topology.edgeData[_index].face); } }

			public EdgeWrap wrap { get { return _topology.edgeData[_index].wrap; } }

			public bool isBoundary { get { return farFace.isExternal != nearFace.isExternal; } }
			public bool isNonBoundary { get { return farFace.isExternal == nearFace.isExternal; } }
			public bool isOuterBoundary { get { return farFace.isExternal; } }
			public bool isInnerBoundary { get { return nearFace.isExternal; } }
			public bool isInternal { get { return nearFace.isInternal; } }
			public bool isExternal { get { return farFace.isExternal; } }

			public VertexEdge vertexEdge { get { return new VertexEdge(_topology, _index); } }
			public FaceEdge faceEdge { get { return new FaceEdge(_topology, _index); } }

			public static implicit operator bool(HalfEdge edge) { return edge._topology != null; }

			public override bool Equals(object other)
			{
				return
					other is HalfEdge && _index == ((HalfEdge)other)._index ||
					other is VertexEdge && _index == ((VertexEdge)other).index ||
					other is FaceEdge && _index == ((FaceEdge)other).index;
			}

			public override int GetHashCode() { return _index.GetHashCode(); }

			public bool Equals(HalfEdge other) { return _index == other._index; }
			public int CompareTo(HalfEdge other) { return _index - other._index; }
			public static bool operator ==(HalfEdge lhs, HalfEdge rhs) { return lhs._index == rhs._index; }
			public static bool operator !=(HalfEdge lhs, HalfEdge rhs) { return lhs._index != rhs._index; }
			public static bool operator < (HalfEdge lhs, HalfEdge rhs) { return lhs._index <  rhs._index; }
			public static bool operator > (HalfEdge lhs, HalfEdge rhs) { return lhs._index >  rhs._index; }
			public static bool operator <=(HalfEdge lhs, HalfEdge rhs) { return lhs._index <= rhs._index; }
			public static bool operator >=(HalfEdge lhs, HalfEdge rhs) { return lhs._index >= rhs._index; }

			public bool Equals(VertexEdge other) { return _index == other.index; }
			public int CompareTo(VertexEdge other) { return _index - other.index; }
			public static bool operator ==(HalfEdge lhs, VertexEdge rhs) { return lhs._index == rhs.index; }
			public static bool operator !=(HalfEdge lhs, VertexEdge rhs) { return lhs._index != rhs.index; }
			public static bool operator < (HalfEdge lhs, VertexEdge rhs) { return lhs._index <  rhs.index; }
			public static bool operator > (HalfEdge lhs, VertexEdge rhs) { return lhs._index >  rhs.index; }
			public static bool operator <=(HalfEdge lhs, VertexEdge rhs) { return lhs._index <= rhs.index; }
			public static bool operator >=(HalfEdge lhs, VertexEdge rhs) { return lhs._index >= rhs.index; }

			public bool Equals(FaceEdge other) { return _index == other.index; }
			public int CompareTo(FaceEdge other) { return _index - other.index; }
			public static bool operator ==(HalfEdge lhs, FaceEdge rhs) { return lhs._index == rhs.index; }
			public static bool operator !=(HalfEdge lhs, FaceEdge rhs) { return lhs._index != rhs.index; }
			public static bool operator < (HalfEdge lhs, FaceEdge rhs) { return lhs._index <  rhs.index; }
			public static bool operator > (HalfEdge lhs, FaceEdge rhs) { return lhs._index >  rhs.index; }
			public static bool operator <=(HalfEdge lhs, FaceEdge rhs) { return lhs._index <= rhs.index; }
			public static bool operator >=(HalfEdge lhs, FaceEdge rhs) { return lhs._index >= rhs.index; }

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
		/// 
		/// <para>In addition to providing access to its two vertex and face neighbors, access is also provided to previous and next half-
		/// edges in the circular linked list around this half-edge's near vertex.  Note that getting the next half-edge requires one less
		/// lookup than getting the previous half-edge, and is therefore recommended for the majority of cases.</para>
		/// </remarks>
		public struct VertexEdge : IEquatable<VertexEdge>, IEquatable<FaceEdge>, IComparable<VertexEdge>, IComparable<FaceEdge>
		{
			private HalfEdge _halfEdge;

			public VertexEdge(Topology topology, int index)
			{
				_halfEdge = new HalfEdge(topology, index);
			}

			public VertexEdge(HalfEdge halfEdge)
			{
				_halfEdge = halfEdge;
			}

			public VertexEdge(VertexEdge vertexEdge)
			{
				_halfEdge = vertexEdge._halfEdge;
			}

			public VertexEdge(FaceEdge faceEdge)
			{
				_halfEdge = faceEdge.halfEdge;
			}

			public static VertexEdge none { get { return new VertexEdge(); } }

			public Topology topology { get { return _halfEdge.topology; } }

			public int index { get { return _halfEdge.index; } }
			public int twinIndex { get { return _halfEdge.twinIndex; } }
			public bool isFirstTwin { get { return index < twinIndex; } }
			public bool isSecondTwin { get { return index > twinIndex; } }

			public VertexEdge twin { get { return new VertexEdge(_halfEdge.twin); } }
			public VertexEdge firstTwin { get { return isFirstTwin ? this : twin; } }
			public VertexEdge secondTwin { get { return isSecondTwin ? this : twin; } }
			public VertexEdge prev { get { return new VertexEdge(_halfEdge.prevAroundVertex); } }
			public VertexEdge next { get { return new VertexEdge(_halfEdge.nextAroundVertex); } }
			public Vertex vertex { get { return _halfEdge.farVertex; } }
			public Vertex nearVertex { get { return _halfEdge.nearVertex; } }
			public Vertex farVertex { get { return _halfEdge.farVertex; } }
			public Face face { get { return _halfEdge.farFace; } }
			public Face prevFace { get { return _halfEdge.farFace; } }
			public Face nextFace { get { return _halfEdge.nearFace; } }

			public EdgeWrap wrap { get { return _halfEdge.wrap; } }

			public bool isBoundary { get { return _halfEdge.isBoundary; } }
			public bool isNonBoundary { get { return _halfEdge.isNonBoundary; } }
			public bool isOuterBoundary { get { return _halfEdge.isOuterBoundary; } }
			public bool isInnerBoundary { get { return _halfEdge.isInnerBoundary; } }
			public bool isInternal { get { return _halfEdge.isInternal; } }
			public bool isExternal { get { return _halfEdge.isExternal; } }

			public HalfEdge halfEdge { get { return _halfEdge; } }
			public FaceEdge faceEdge { get { return new FaceEdge(_halfEdge); } }

			public static implicit operator bool(VertexEdge edge) { return edge._halfEdge; }

			public override bool Equals(object other)
			{
				return
					other is HalfEdge && index == ((HalfEdge)other).index ||
					other is VertexEdge && index == ((VertexEdge)other).index ||
					other is FaceEdge && index == ((FaceEdge)other).index;
			}

			public override int GetHashCode() { return _halfEdge.GetHashCode(); }

			public bool Equals(HalfEdge other) { return _halfEdge.Equals(other); }
			public int CompareTo(HalfEdge other) { return _halfEdge.CompareTo(other); }
			public static bool operator ==(VertexEdge lhs, HalfEdge rhs) { return lhs._halfEdge == rhs; }
			public static bool operator !=(VertexEdge lhs, HalfEdge rhs) { return lhs._halfEdge != rhs; }
			public static bool operator < (VertexEdge lhs, HalfEdge rhs) { return lhs._halfEdge <  rhs; }
			public static bool operator > (VertexEdge lhs, HalfEdge rhs) { return lhs._halfEdge >  rhs; }
			public static bool operator <=(VertexEdge lhs, HalfEdge rhs) { return lhs._halfEdge <= rhs; }
			public static bool operator >=(VertexEdge lhs, HalfEdge rhs) { return lhs._halfEdge >= rhs; }

			public bool Equals(VertexEdge other) { return _halfEdge.Equals(other._halfEdge); }
			public int CompareTo(VertexEdge other) { return _halfEdge.CompareTo(other._halfEdge); }
			public static bool operator ==(VertexEdge lhs, VertexEdge rhs) { return lhs._halfEdge == rhs._halfEdge; }
			public static bool operator !=(VertexEdge lhs, VertexEdge rhs) { return lhs._halfEdge != rhs._halfEdge; }
			public static bool operator < (VertexEdge lhs, VertexEdge rhs) { return lhs._halfEdge <  rhs._halfEdge; }
			public static bool operator > (VertexEdge lhs, VertexEdge rhs) { return lhs._halfEdge >  rhs._halfEdge; }
			public static bool operator <=(VertexEdge lhs, VertexEdge rhs) { return lhs._halfEdge <= rhs._halfEdge; }
			public static bool operator >=(VertexEdge lhs, VertexEdge rhs) { return lhs._halfEdge >= rhs._halfEdge; }

			public bool Equals(FaceEdge other) { return _halfEdge.Equals(other); }
			public int CompareTo(FaceEdge other) { return _halfEdge.CompareTo(other); }
			public static bool operator ==(VertexEdge lhs, FaceEdge rhs) { return lhs._halfEdge == rhs; }
			public static bool operator !=(VertexEdge lhs, FaceEdge rhs) { return lhs._halfEdge != rhs; }
			public static bool operator < (VertexEdge lhs, FaceEdge rhs) { return lhs._halfEdge <  rhs; }
			public static bool operator > (VertexEdge lhs, FaceEdge rhs) { return lhs._halfEdge >  rhs; }
			public static bool operator <=(VertexEdge lhs, FaceEdge rhs) { return lhs._halfEdge <= rhs; }
			public static bool operator >=(VertexEdge lhs, FaceEdge rhs) { return lhs._halfEdge >= rhs; }

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
		/// 
		/// <para>In addition to providing access to its two vertex and face neighbors, access is also provided to previous and next half-
		/// edges in the circular linked list around this half-edge's near face.  Note that getting the next half-edge requires one less
		/// lookup than getting the previous half-edge, and is therefore recommended for the majority of cases.</para>
		/// </remarks>
		public struct FaceEdge : IEquatable<FaceEdge>, IEquatable<VertexEdge>, IComparable<FaceEdge>, IComparable<VertexEdge>
		{
			private HalfEdge _halfEdge;

			public FaceEdge(Topology topology, int index)
			{
				_halfEdge = new HalfEdge(topology, index);
			}

			public FaceEdge(HalfEdge halfEdge)
			{
				_halfEdge = halfEdge;
			}

			public FaceEdge(VertexEdge vertexEdge)
			{
				_halfEdge = vertexEdge.halfEdge;
			}

			public FaceEdge(FaceEdge faceEdge)
			{
				_halfEdge = faceEdge._halfEdge;
			}

			public static FaceEdge none { get { return new FaceEdge(); } }

			public Topology topology { get { return _halfEdge.topology; } }

			public int index { get { return _halfEdge.index; } }
			public int twinIndex { get { return _halfEdge.twinIndex; } }
			public bool isFirstTwin { get { return index < twinIndex; } }
			public bool isSecondTwin { get { return index > twinIndex; } }

			public FaceEdge twin { get { return new FaceEdge(_halfEdge.twin); } }
			public FaceEdge firstTwin { get { return isFirstTwin ? this : twin; } }
			public FaceEdge secondTwin { get { return isSecondTwin ? this : twin; } }
			public FaceEdge prev { get { return new FaceEdge(_halfEdge.prevAroundFace); } }
			public FaceEdge next { get { return new FaceEdge(_halfEdge.nextAroundFace); } }
			public Vertex vertex { get { return _halfEdge.farVertex; } }
			public Vertex prevVertex { get { return _halfEdge.nearVertex; } }
			public Vertex nextVertex { get { return _halfEdge.farVertex; } }
			public Face face { get { return _halfEdge.farFace; } }
			public Face nearFace { get { return _halfEdge.nearFace; } }
			public Face farFace { get { return _halfEdge.farFace; } }

			public EdgeWrap wrap { get { return _halfEdge.wrap; } }

			public bool isBoundary { get { return _halfEdge.isBoundary; } }
			public bool isNonBoundary { get { return _halfEdge.isNonBoundary; } }
			public bool isOuterBoundary { get { return _halfEdge.isOuterBoundary; } }
			public bool isInnerBoundary { get { return _halfEdge.isInnerBoundary; } }
			public bool isInternal { get { return _halfEdge.isInternal; } }
			public bool isExternal { get { return _halfEdge.isExternal; } }

			public HalfEdge halfEdge { get { return _halfEdge; } }
			public VertexEdge vertexEdge { get { return new VertexEdge(_halfEdge); } }

			public static implicit operator bool(FaceEdge edge) { return edge._halfEdge; }

			public override bool Equals(object other)
			{
				return
					other is HalfEdge && index == ((HalfEdge)other).index ||
					other is VertexEdge && index == ((VertexEdge)other).index ||
					other is FaceEdge && index == ((FaceEdge)other).index;
			}

			public override int GetHashCode() { return _halfEdge.GetHashCode(); }

			public bool Equals(HalfEdge other) { return _halfEdge.Equals(other); }
			public int CompareTo(HalfEdge other) { return _halfEdge.CompareTo(other); }
			public static bool operator ==(FaceEdge lhs, HalfEdge rhs) { return lhs._halfEdge == rhs; }
			public static bool operator !=(FaceEdge lhs, HalfEdge rhs) { return lhs._halfEdge != rhs; }
			public static bool operator < (FaceEdge lhs, HalfEdge rhs) { return lhs._halfEdge <  rhs; }
			public static bool operator > (FaceEdge lhs, HalfEdge rhs) { return lhs._halfEdge >  rhs; }
			public static bool operator <=(FaceEdge lhs, HalfEdge rhs) { return lhs._halfEdge <= rhs; }
			public static bool operator >=(FaceEdge lhs, HalfEdge rhs) { return lhs._halfEdge >= rhs; }

			public bool Equals(VertexEdge other) { return _halfEdge.Equals(other); }
			public int CompareTo(VertexEdge other) { return _halfEdge.CompareTo(other); }
			public static bool operator ==(FaceEdge lhs, VertexEdge rhs) { return lhs._halfEdge == rhs; }
			public static bool operator !=(FaceEdge lhs, VertexEdge rhs) { return lhs._halfEdge != rhs; }
			public static bool operator < (FaceEdge lhs, VertexEdge rhs) { return lhs._halfEdge <  rhs; }
			public static bool operator > (FaceEdge lhs, VertexEdge rhs) { return lhs._halfEdge >  rhs; }
			public static bool operator <=(FaceEdge lhs, VertexEdge rhs) { return lhs._halfEdge <= rhs; }
			public static bool operator >=(FaceEdge lhs, VertexEdge rhs) { return lhs._halfEdge >= rhs; }

			public bool Equals(FaceEdge other) { return _halfEdge.Equals(other._halfEdge); }
			public int CompareTo(FaceEdge other) { return _halfEdge.CompareTo(other._halfEdge); }
			public static bool operator ==(FaceEdge lhs, FaceEdge rhs) { return lhs._halfEdge == rhs._halfEdge; }
			public static bool operator !=(FaceEdge lhs, FaceEdge rhs) { return lhs._halfEdge != rhs._halfEdge; }
			public static bool operator < (FaceEdge lhs, FaceEdge rhs) { return lhs._halfEdge <  rhs._halfEdge; }
			public static bool operator > (FaceEdge lhs, FaceEdge rhs) { return lhs._halfEdge >  rhs._halfEdge; }
			public static bool operator <=(FaceEdge lhs, FaceEdge rhs) { return lhs._halfEdge <= rhs._halfEdge; }
			public static bool operator >=(FaceEdge lhs, FaceEdge rhs) { return lhs._halfEdge >= rhs._halfEdge; }

			public override string ToString()
			{
				return string.Format("Face Edge {0} (Twin Edge: {1}, Edges p/n: {2}/{3}), (Faces n/f: {6}/{7}), (Vertices p/n: {4}/{5})", index, twinIndex, prev.index, next.index, nearFace.index, farFace.index, prevVertex.index, nextVertex.index);
			}
		}

		public struct HalfEdgesIndexer
		{
			private Topology _topology;

			public HalfEdgesIndexer(Topology topology) { _topology = topology; }
			public HalfEdge this[int i] { get { return new HalfEdge(_topology, i); } }
			public int Count { get { return _topology.edgeData.Length; } }
			public EdgeEnumerator GetEnumerator() { return new EdgeEnumerator(_topology); }

			public struct EdgeEnumerator
			{
				private Topology _topology;
				private int _current;

				public EdgeEnumerator(Topology topology) { _topology = topology; _current = -1; }
				public HalfEdge Current { get { return new HalfEdge(_topology, _current); } }
				public bool MoveNext() { return ++_current < _topology.edgeData.Length; }
				public void Reset() { throw new NotSupportedException(); }
			}
		}

		public HalfEdgesIndexer halfEdges { get { return new HalfEdgesIndexer(this); } }

		public struct VertexEdgesIndexer
		{
			private Topology _topology;

			public VertexEdgesIndexer(Topology topology) { _topology = topology; }
			public VertexEdge this[int i] { get { return new VertexEdge(_topology, i); } }
			public int Count { get { return _topology.edgeData.Length; } }
			public EdgeEnumerator GetEnumerator() { return new EdgeEnumerator(_topology); }

			public struct EdgeEnumerator
			{
				private Topology _topology;
				private int _current;

				public EdgeEnumerator(Topology topology) { _topology = topology; _current = -1; }
				public VertexEdge Current { get { return new VertexEdge(_topology, _current); } }
				public bool MoveNext() { return ++_current < _topology.edgeData.Length; }
				public void Reset() { throw new NotSupportedException(); }
			}
		}

		public VertexEdgesIndexer vertexEdges { get { return new VertexEdgesIndexer(this); } }

		public struct FaceEdgesIndexer
		{
			private Topology _topology;

			public FaceEdgesIndexer(Topology topology) { _topology = topology; }
			public FaceEdge this[int i] { get { return new FaceEdge(_topology, i); } }
			public int Count { get { return _topology.edgeData.Length; } }
			public EdgeEnumerator GetEnumerator() { return new EdgeEnumerator(_topology); }

			public struct EdgeEnumerator
			{
				private Topology _topology;
				private int _current;

				public EdgeEnumerator(Topology topology) { _topology = topology; _current = -1; }
				public FaceEdge Current { get { return new FaceEdge(_topology, _current); } }
				public bool MoveNext() { return ++_current < _topology.edgeData.Length; }
				public void Reset() { throw new NotSupportedException(); }
			}
		}

		public FaceEdgesIndexer faceEdges { get { return new FaceEdgesIndexer(this); } }
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
		/// Lookup the attribute value for the half-edge indicated.
		/// </summary>
		/// <param name="i">The index of the half-edge whose attribute value is desired.</param>
		/// <returns>The attribute value for the half-edge indicated.</returns>
		new T this[int i] { get; set; }

		/// <summary>
		/// Lookup the attribute value for the half-edge indicated.
		/// </summary>
		/// <param name="e">The half-edge whose attribute value is desired.</param>
		/// <returns>The attribute value for the half-edge indicated.</returns>
		T this[Topology.HalfEdge e] { get; set; }

		/// <summary>
		/// Lookup the attribute value for a half-edge relative to a neighboring vertex.
		/// </summary>
		/// <param name="e">The half-edge (represented as a vertex edge) whose attribute value is desired.</param>
		/// <returns>The attribute value for the indicated half-edge, relative to its near vertex.</returns>
		T this[Topology.VertexEdge e] { get; set; }

		/// <summary>
		/// Lookup the attribute value for a half-edge relative to a neighboring face.
		/// </summary>
		/// <param name="e">The half-edge (represented as a face edge) whose attribute value is desired.</param>
		/// <returns>The attribute value for the indicated half-edge, relative to its near face.</returns>
		T this[Topology.FaceEdge e] { get; set; }
	}

	public struct EdgeAttributeConstantWrapper<T> : IEdgeAttribute<T>
	{
		public T constant;

		public EdgeAttributeConstantWrapper(T constant)
		{
			this.constant = constant;
		}

		public T this[int i]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant edge attribute cannot be changed."); }
		}

		public T this[Topology.HalfEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant edge attribute cannot be changed."); }
		}

		public T this[Topology.VertexEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant edge attribute cannot be changed."); }
		}

		public T this[Topology.FaceEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant edge attribute cannot be changed."); }
		}

		public int Count { get { throw new NotSupportedException(); } }
		public bool IsReadOnly { get { return true; } }
		public void Add(T item) { throw new NotSupportedException(); }
		public void Clear() { throw new NotSupportedException(); }
		public bool Contains(T item) { throw new NotSupportedException(); }
		public void CopyTo(T[] array, int arrayIndex) { throw new NotSupportedException(); }
		public IEnumerator<T> GetEnumerator() { throw new NotSupportedException(); }
		public int IndexOf(T item) { throw new NotSupportedException(); }
		public void Insert(int index, T item) { throw new NotSupportedException(); }
		public bool Remove(T item) { throw new NotSupportedException(); }
		public void RemoveAt(int index) { throw new NotSupportedException(); }
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	public struct EdgeAttributeArrayWrapper<T> : IEdgeAttribute<T>
	{
		public T[] array;

		public EdgeAttributeArrayWrapper(T[] array)
		{
			this.array = array;
		}

		public EdgeAttributeArrayWrapper(int elementCount)
		{
			array = new T[elementCount];
		}

		public T this[int i]
		{
			get { return array[i]; }
			set { array[i] = value; }
		}

		public T this[Topology.HalfEdge e]
		{
			get { return array[e.index]; }
			set { array[e.index] = value; }
		}

		public T this[Topology.VertexEdge e]
		{
			get { return array[e.index]; }
			set { array[e.index] = value; }
		}

		public T this[Topology.FaceEdge e]
		{
			get { return array[e.index]; }
			set { array[e.index] = value; }
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

	public struct TwinEdgeAttributeWrapper<T> : IEdgeAttribute<T>
	{
		public Topology topology;
		public IEdgeAttribute<T> underlyingAttribute;

		public TwinEdgeAttributeWrapper(Topology topology, IEdgeAttribute<T> underlyingAttribute)
		{
			this.topology = topology;
			this.underlyingAttribute = underlyingAttribute;
		}

		public T this[int i]
		{
			get { return underlyingAttribute[topology.halfEdges[i].twinIndex]; }
			set { underlyingAttribute[topology.halfEdges[i].twinIndex] = value; }
		}

		public T this[Topology.HalfEdge e]
		{
			get { return underlyingAttribute[e.twin]; }
			set { underlyingAttribute[e.twin] = value; }
		}

		public T this[Topology.VertexEdge e]
		{
			get { return underlyingAttribute[e.twin]; }
			set { underlyingAttribute[e.twin] = value; }
		}

		public T this[Topology.FaceEdge e]
		{
			get { return underlyingAttribute[e.twin]; }
			set { underlyingAttribute[e.twin] = value; }
		}

		public int Count { get { return underlyingAttribute.Count; } }
		public bool IsReadOnly { get { return underlyingAttribute.IsReadOnly; } }
		public void Add(T item) { underlyingAttribute.Add(item); }
		public void Clear() { underlyingAttribute.Clear(); }
		public bool Contains(T item) { return underlyingAttribute.Contains(item); }
		public void CopyTo(T[] array, int arrayIndex) { underlyingAttribute.CopyTo(array, arrayIndex); }
		public IEnumerator<T> GetEnumerator() { return underlyingAttribute.GetEnumerator(); }
		public int IndexOf(T item) { return underlyingAttribute.IndexOf(item); }
		public void Insert(int index, T item) { underlyingAttribute.Insert(index, item); }
		public bool Remove(T item) { return underlyingAttribute.Remove(item); }
		public void RemoveAt(int index) { underlyingAttribute.RemoveAt(index); }
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	public abstract class EdgeAttribute<T> : ScriptableObject, IEdgeAttribute<T>
	{
		public abstract T this[int i] { get; set; }
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

	public class EdgeConstantAttribute<T> : EdgeAttribute<T> where T : new()
	{
		public T constant;

		protected static TDerived CreateDerived<TDerived>(T constant) where TDerived : EdgeConstantAttribute<T>
		{
			var instance = CreateInstance<TDerived>();
			instance.constant = constant;
			return instance;
		}

		public override T this[int i]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant edge attribute cannot be changed."); }
		}

		public override T this[Topology.HalfEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant edge attribute cannot be changed."); }
		}

		public override T this[Topology.VertexEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant edge attribute cannot be changed."); }
		}

		public override T this[Topology.FaceEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant edge attribute cannot be changed."); }
		}
	}

	public class EdgeArrayAttribute<T> : EdgeAttribute<T> where T : new()
	{
		public T[] array;

		protected static TDerived CreateDerived<TDerived>(T[] array) where TDerived : EdgeArrayAttribute<T>
		{
			var instance = CreateInstance<TDerived>();
			instance.array = array;
			return instance;
		}

		protected static TDerived CreateDerived<TDerived>(int faceCount) where TDerived : EdgeArrayAttribute<T>
		{
			return CreateDerived<TDerived>(new T[faceCount]);
		}

		public override T this[int i]
		{
			get { return array[i]; }
			set { array[i] = value; }
		}

		public override T this[Topology.HalfEdge e]
		{
			get { return array[e.index]; }
			set { array[e.index] = value; }
		}

		public override T this[Topology.VertexEdge e]
		{
			get { return array[e.index]; }
			set { array[e.index] = value; }
		}

		public override T this[Topology.FaceEdge e]
		{
			get { return array[e.index]; }
			set { array[e.index] = value; }
		}

		public override int Count { get { return array.Length; } }
		public override bool Contains(T item) { return ((IList<T>)array).Contains(item); }
		public override void CopyTo(T[] array, int arrayIndex) { this.array.CopyTo(array, arrayIndex); }
		public override IEnumerator<T> GetEnumerator() { return ((IList<T>)array).GetEnumerator(); }
		public override int IndexOf(T item) { return ((IList<T>)array).IndexOf(item); }
	}

	public static class EdgeExtensions
	{
		public static EdgeAttributeArrayWrapper<T> AsEdgeAttribute<T>(this T[] array)
		{
			return new EdgeAttributeArrayWrapper<T>(array);
		}
	}
}
