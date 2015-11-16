﻿using System;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public partial class Topology
	{
		public struct EdgeData
		{
			// \             /
			//  P     R     /
			//   \         /
			//    r---E-->v
			//   /         \
			//  N     F     \
			// /             \
			//
			// E:  This edge
			// r:  Implicit near vertex
			// v:  Explicit far vertex
			// R:  Implicit near face
			// F:  Explicit far face
			// P:  Previous edge (also the next edge when going around the implicit near face clockwise)
			// N:  Next edge

			public int _twin; // The other edge between the same two vertices and same two faces, but going the opposite direction.
			public int _prev; // The next edge after this one when going clockwise around the implicit near vertex.
			public int _next; // The next edge this one when going clockwise around the implicit near face.
			public int _vertex; // The vertex at the far end of this edge that preceeds the below face when going clockwise around the implicit near vertex.
			public int _face; // The face on the far side of this edge that follows after the above vertex when going clockwise around the implicit near vertex.

			public EdgeData(int twin, int prev, int next, int vertex, int face)
			{
				_twin = twin;
				_prev = prev;
				_next = next;
				_vertex = vertex;
				_face = face;
			}

			public override string ToString()
			{
				return string.Format("EdgeData ({0}, {1}, {2}, {3}, {4})", _twin, _prev, _next, _vertex, _face);
			}
		}

		public EdgeData[] _edgeData;

		public struct VertexEdge : IEquatable<VertexEdge>, IEquatable<FaceEdge>, IComparable<VertexEdge>, IComparable<FaceEdge>
		{
			private Topology _topology;
			private int _index;

			public VertexEdge(Topology topology, int index)
			{
				_topology = topology;
				_index = index;
			}

			public VertexEdge(FaceEdge faceEdge)
			{
				_topology = faceEdge.topology;
				_index = faceEdge.index;
			}

			public Topology topology { get { return _topology; } }

			public int index { get { return _index; } }

			public int twinIndex { get { return _topology._edgeData[_index]._twin; } }
			public int prevIndex { get { return _topology._edgeData[_index]._prev; } }
			public int nextIndex { get { return _topology._edgeData[_index]._next; } }
			public int nearVertexIndex { get { return _topology._edgeData[twinIndex]._vertex; } }
			public int farVertexIndex { get { return _topology._edgeData[_index]._vertex; } }
			public int prevFaceIndex { get { return _topology._edgeData[twinIndex]._face; } }
			public int nextFaceIndex { get { return _topology._edgeData[_index]._face; } }

			public VertexEdge twin { get { return new VertexEdge(_topology, twinIndex); } }
			public VertexEdge prev { get { return new VertexEdge(_topology, prevIndex); } }
			public VertexEdge next { get { return new VertexEdge(_topology, nextIndex); } }
			public Vertex nearVertex { get { return new Vertex(_topology, nearVertexIndex); } }
			public Vertex farVertex { get { return new Vertex(_topology, farVertexIndex); } }
			public Face prevFace { get { return new Face(_topology, prevFaceIndex); } }
			public Face nextFace { get { return new Face(_topology, nextFaceIndex); } }

			public FaceEdge faceEdge { get { return new FaceEdge(_topology, _index); } }

			public override bool Equals(object other) { return other is VertexEdge && _index == ((VertexEdge)other)._index || other is FaceEdge && _index == ((FaceEdge)other).index; }
			public override int GetHashCode() { return _index.GetHashCode(); }

			public bool Equals(VertexEdge other) { return _index == other._index; }
			public int CompareTo(VertexEdge other) { return _index - other._index; }
			public static bool operator ==(VertexEdge lhs, VertexEdge rhs) { return lhs._index == rhs._index; }
			public static bool operator !=(VertexEdge lhs, VertexEdge rhs) { return lhs._index != rhs._index; }
			public static bool operator < (VertexEdge lhs, VertexEdge rhs) { return lhs._index <  rhs._index; }
			public static bool operator > (VertexEdge lhs, VertexEdge rhs) { return lhs._index >  rhs._index; }
			public static bool operator <=(VertexEdge lhs, VertexEdge rhs) { return lhs._index <= rhs._index; }
			public static bool operator >=(VertexEdge lhs, VertexEdge rhs) { return lhs._index >= rhs._index; }

			public bool Equals(FaceEdge other) { return _index == other.index; }
			public int CompareTo(FaceEdge other) { return _index - other.index; }
			public static bool operator ==(VertexEdge lhs, FaceEdge rhs) { return lhs._index == rhs.index; }
			public static bool operator !=(VertexEdge lhs, FaceEdge rhs) { return lhs._index != rhs.index; }
			public static bool operator < (VertexEdge lhs, FaceEdge rhs) { return lhs._index <  rhs.index; }
			public static bool operator > (VertexEdge lhs, FaceEdge rhs) { return lhs._index >  rhs.index; }
			public static bool operator <=(VertexEdge lhs, FaceEdge rhs) { return lhs._index <= rhs.index; }
			public static bool operator >=(VertexEdge lhs, FaceEdge rhs) { return lhs._index >= rhs.index; }

			public override string ToString()
			{
				return string.Format("Vertex Edge {0} ({1}, {2}, {3}), ({4}, {5}), ({6}, {7})", _index, prev.index, next.index, twinIndex, nearVertex.index, farVertex.index, prevFace.index, nextFace.index);
			}
		}

		public struct FaceEdge : IEquatable<FaceEdge>, IEquatable<VertexEdge>, IComparable<FaceEdge>, IComparable<VertexEdge>
		{
			private Topology _topology;
			private int _index;

			public FaceEdge(Topology topology, int index)
			{
				_topology = topology;
				_index = index;
			}

			public FaceEdge(VertexEdge vertexEdge)
			{
				_topology = vertexEdge.topology;
				_index = vertexEdge.index;
			}

			public Topology topology { get { return _topology; } }

			public int index { get { return _index; } }

			public int twinIndex { get { return _topology._edgeData[_index]._twin; } }
			public int prevIndex { get { return _topology._edgeData[twinIndex]._next; } }
			public int nextIndex { get { return _topology._edgeData[_topology._edgeData[_index]._prev]._twin; } }
			public int prevVertexIndex { get { return _topology._edgeData[_index]._vertex; } }
			public int nextVertexIndex { get { return _topology._edgeData[twinIndex]._vertex; } }
			public int nearFaceIndex { get { return _topology._edgeData[twinIndex]._face; } }
			public int farFaceIndex { get { return _topology._edgeData[_index]._face; } }

			public FaceEdge twin { get { return new FaceEdge(_topology, twinIndex); } }
			public FaceEdge prev { get { return new FaceEdge(_topology, prevIndex); } }
			public FaceEdge next { get { return new FaceEdge(_topology, nextIndex); } }
			public Vertex prevVertex { get { return new Vertex(_topology, prevVertexIndex); } }
			public Vertex nextVertex { get { return new Vertex(_topology, nextVertexIndex); } }
			public Face nearFace { get { return new Face(_topology, nearFaceIndex); } }
			public Face farFace { get { return new Face(_topology, farFaceIndex); } }

			public VertexEdge vertexEdge { get { return new VertexEdge(_topology, _index); } }

			public override bool Equals(object other) { return other is FaceEdge && _index == ((FaceEdge)other)._index || other is VertexEdge && _index == ((VertexEdge)other).index; }
			public override int GetHashCode() { return _index.GetHashCode(); }

			public bool Equals(FaceEdge other) { return _index == other._index; }
			public int CompareTo(FaceEdge other) { return _index - other._index; }
			public static bool operator ==(FaceEdge lhs, FaceEdge rhs) { return lhs._index == rhs._index; }
			public static bool operator !=(FaceEdge lhs, FaceEdge rhs) { return lhs._index != rhs._index; }
			public static bool operator < (FaceEdge lhs, FaceEdge rhs) { return lhs._index <  rhs._index; }
			public static bool operator > (FaceEdge lhs, FaceEdge rhs) { return lhs._index >  rhs._index; }
			public static bool operator <=(FaceEdge lhs, FaceEdge rhs) { return lhs._index <= rhs._index; }
			public static bool operator >=(FaceEdge lhs, FaceEdge rhs) { return lhs._index >= rhs._index; }

			public bool Equals(VertexEdge other) { return _index == other.index; }
			public int CompareTo(VertexEdge other) { return _index - other.index; }
			public static bool operator ==(FaceEdge lhs, VertexEdge rhs) { return lhs._index == rhs.index; }
			public static bool operator !=(FaceEdge lhs, VertexEdge rhs) { return lhs._index != rhs.index; }
			public static bool operator < (FaceEdge lhs, VertexEdge rhs) { return lhs._index <  rhs.index; }
			public static bool operator > (FaceEdge lhs, VertexEdge rhs) { return lhs._index >  rhs.index; }
			public static bool operator <=(FaceEdge lhs, VertexEdge rhs) { return lhs._index <= rhs.index; }
			public static bool operator >=(FaceEdge lhs, VertexEdge rhs) { return lhs._index >= rhs.index; }

			public override string ToString()
			{
				return string.Format("Face Edge {0} ({1}, {2}, {3}), ({4}, {5}), ({6}, {7})", _index, prev.index, next.index, twinIndex, nearFace.index, farFace.index, prevVertex.index, nextVertex.index);
			}
		}

		public struct VertexEdgesIndexer
		{
			private Topology _topology;

			public VertexEdgesIndexer(Topology topology){ _topology = topology; }
			public VertexEdge this[int i] { get { return new VertexEdge(_topology, i); } }
			public int Count { get { return _topology._edgeData.Length; } }
			public EdgeEnumerator GetEnumerator() { return new EdgeEnumerator(_topology); }

			public struct EdgeEnumerator
			{
				private Topology _topology;
				private EdgeData[] _edgeData;
				private int _index;

				public EdgeEnumerator(Topology topology) { _topology = topology; _edgeData = topology._edgeData; _index = -1; }
				public VertexEdge Current { get { return new VertexEdge(_topology, _index); } }
				public bool MoveNext() { return (++_index < _edgeData.Length); }
				public void Reset() { _index = -1; }
			}
		}

		public VertexEdgesIndexer vertexEdges { get { return new VertexEdgesIndexer(this); } }

		public struct FaceEdgesIndexer
		{
			private Topology _topology;

			public FaceEdgesIndexer(Topology topology){ _topology = topology; }
			public FaceEdge this[int i] { get { return new FaceEdge(_topology, i); } }
			public int Count { get { return _topology._edgeData.Length; } }
			public EdgeEnumerator GetEnumerator() { return new EdgeEnumerator(_topology); }

			public struct EdgeEnumerator
			{
				private Topology _topology;
				private int _index;

				public EdgeEnumerator(Topology topology) { _topology = topology; _index = -1; }
				public FaceEdge Current { get { return new FaceEdge(_topology, _index); } }
				public bool MoveNext() { return (++_index < _topology._edgeData.Length); }
				public void Reset() { _index = -1; }
			}
		}

		public FaceEdgesIndexer faceEdges { get { return new FaceEdgesIndexer(this); } }
	}
}
