using UnityEngine;
using System;
using System.Collections.Generic;

namespace Experilous.Topological
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

			public int _twin; // The other edge between the same two vertices and same two faces, but going the opposite direction.
			public int _vNext; // The next edge after this one when going clockwise around the implicit near vertex.
			public int _fNext; // The next edge this one when going clockwise around the implicit near face.
			public int _vertex; // The vertex at the far end of this edge that preceeds the below face when going clockwise around the implicit near vertex.
			public int _face; // The face on the far side of this edge that follows after the above vertex when going clockwise around the implicit near vertex.

			public EdgeData(int twin, int vNext, int fNext, int vertex, int face)
			{
				_twin = twin;
				_vNext = vNext;
				_fNext = fNext;
				_vertex = vertex;
				_face = face;
			}

			public override string ToString()
			{
				return string.Format("EdgeData ({0}, {1}, {2}, {3}, {4})", _twin, _vNext, _fNext, _vertex, _face);
			}
		}

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

			public Topology topology { get { return _topology; } }

			public int index { get { return _index; } }
			public int twinIndex { get { return _topology.edgeData[_index]._twin; } }

			public HalfEdge twin { get { return new HalfEdge(_topology, _topology.edgeData[_index]._twin); } }
			public HalfEdge prevAroundVertex { get { return new HalfEdge(_topology, _topology.edgeData[_topology.edgeData[_index]._twin]._fNext); } }
			public HalfEdge nextAroundVertex { get { return new HalfEdge(_topology, _topology.edgeData[_index]._vNext); } }
			public HalfEdge prevAroundFace { get { return new HalfEdge(_topology, _topology.edgeData[_topology.edgeData[_index]._vNext]._twin); } }
			public HalfEdge nextAroundFace { get { return new HalfEdge(_topology, _topology.edgeData[_index]._fNext); } }
			public Vertex nearVertex { get { return new Vertex(_topology, _topology.edgeData[_topology.edgeData[_index]._twin]._vertex); } }
			public Vertex farVertex { get { return new Vertex(_topology, _topology.edgeData[_index]._vertex); } }
			public Face nearFace { get { return new Face(_topology, _topology.edgeData[_topology.edgeData[_index]._twin]._face); } }
			public Face farFace { get { return new Face(_topology, _topology.edgeData[_index]._face); } }

			public bool isBoundary { get { return farFace.isExternal != nearFace.isExternal; } }
			public bool isOuterBoundary { get { return farFace.isExternal; } }
			public bool isInnerBoundary { get { return nearFace.isExternal; } }
			public bool isInternal { get { return nearFace.isInternal; } }
			public bool isExternal { get { return farFace.isExternal; } }

			public VertexEdge vertexEdge { get { return new VertexEdge(_topology, _index); } }
			public FaceEdge faceEdge { get { return new FaceEdge(_topology, _index); } }

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

			public Topology topology { get { return _halfEdge.topology; } }

			public int index { get { return _halfEdge.index; } }
			public int twinIndex { get { return _halfEdge.twinIndex; } }

			public VertexEdge twin { get { return new VertexEdge(_halfEdge.twin); } }
			public VertexEdge prev { get { return new VertexEdge(_halfEdge.prevAroundVertex); } }
			public VertexEdge next { get { return new VertexEdge(_halfEdge.nextAroundVertex); } }
			public Vertex nearVertex { get { return _halfEdge.nearVertex; } }
			public Vertex farVertex { get { return _halfEdge.farVertex; } }
			public Face prevFace { get { return _halfEdge.farFace; } }
			public Face nextFace { get { return _halfEdge.nearFace; } }

			public bool isBoundary { get { return _halfEdge.isBoundary; } }
			public bool isOuterBoundary { get { return _halfEdge.isOuterBoundary; } }
			public bool isInnerBoundary { get { return _halfEdge.isInnerBoundary; } }
			public bool isInternal { get { return _halfEdge.isInternal; } }
			public bool isExternal { get { return _halfEdge.isExternal; } }

			public HalfEdge halfEdge { get { return _halfEdge; } }
			public FaceEdge faceEdge { get { return new FaceEdge(_halfEdge); } }

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

			public Topology topology { get { return _halfEdge.topology; } }

			public int index { get { return _halfEdge.index; } }
			public int twinIndex { get { return _halfEdge.twinIndex; } }

			public FaceEdge twin { get { return new FaceEdge(_halfEdge.twin); } }
			public FaceEdge prev { get { return new FaceEdge(_halfEdge.prevAroundFace); } }
			public FaceEdge next { get { return new FaceEdge(_halfEdge.nextAroundFace); } }
			public Vertex prevVertex { get { return _halfEdge.nearVertex; } }
			public Vertex nextVertex { get { return _halfEdge.farVertex; } }
			public Face nearFace { get { return _halfEdge.nearFace; } }
			public Face farFace { get { return _halfEdge.farFace; } }

			public bool isBoundary { get { return _halfEdge.isBoundary; } }
			public bool isOuterBoundary { get { return _halfEdge.isOuterBoundary; } }
			public bool isInnerBoundary { get { return _halfEdge.isInnerBoundary; } }
			public bool isInternal { get { return _halfEdge.isInternal; } }
			public bool isExternal { get { return _halfEdge.isExternal; } }

			public HalfEdge halfEdge { get { return _halfEdge; } }
			public VertexEdge vertexEdge { get { return new VertexEdge(_halfEdge); } }

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
			private int _first;
			private int _last;

			public HalfEdgesIndexer(Topology topology){ _topology = topology; _first = 0; _last = _topology.edgeData.Length; }
			public HalfEdgesIndexer(Topology topology, int first, int last) { _topology = topology; _first = first; _last = last; }
			public HalfEdge this[int i] { get { return new HalfEdge(_topology, _first + i); } }
			public int Count { get { return _last - _first; } }
			public EdgeEnumerator GetEnumerator() { return new EdgeEnumerator(_topology, _first, _last); }

			public struct EdgeEnumerator
			{
				private Topology _topology;
				private int _index;
				private int _next;
				private int _last;

				public EdgeEnumerator(Topology topology, int first, int last) { _topology = topology; _index = 0; _next = first; _last = last; }
				public HalfEdge Current { get { return new HalfEdge(_topology, _index); } }
				public bool MoveNext() { return (_index = _next++) != _last; }
				public void Reset() { throw new NotSupportedException(); }
			}
		}

		public HalfEdgesIndexer halfEdges { get { return new HalfEdgesIndexer(this); } }
		public HalfEdgesIndexer internalHalfEdges { get { return new HalfEdgesIndexer(this, 0, firstExternalEdgeIndex); } }
		public HalfEdgesIndexer externalHalfEdges { get { return new HalfEdgesIndexer(this, firstExternalEdgeIndex, edgeData.Length); } }

		public struct VertexEdgesIndexer
		{
			private Topology _topology;
			private int _first;
			private int _last;

			public VertexEdgesIndexer(Topology topology){ _topology = topology; _first = 0; _last = _topology.edgeData.Length; }
			public VertexEdgesIndexer(Topology topology, int first, int last) { _topology = topology; _first = first; _last = last; }
			public VertexEdge this[int i] { get { return new VertexEdge(_topology, _first + i); } }
			public int Count { get { return _last - _first; } }
			public EdgeEnumerator GetEnumerator() { return new EdgeEnumerator(_topology, _first, _last); }

			public struct EdgeEnumerator
			{
				private Topology _topology;
				private int _index;
				private int _next;
				private int _last;

				public EdgeEnumerator(Topology topology, int first, int last) { _topology = topology; _index = 0; _next = first; _last = last; }
				public VertexEdge Current { get { return new VertexEdge(_topology, _index); } }
				public bool MoveNext() { return (_index = _next++) != _last; }
				public void Reset() { throw new NotSupportedException(); }
			}
		}

		public VertexEdgesIndexer vertexEdges { get { return new VertexEdgesIndexer(this); } }
		public VertexEdgesIndexer internalVertexEdges { get { return new VertexEdgesIndexer(this, 0, firstExternalEdgeIndex); } }
		public VertexEdgesIndexer externalVertexEdges { get { return new VertexEdgesIndexer(this, firstExternalEdgeIndex, edgeData.Length); } }

		public struct FaceEdgesIndexer
		{
			private Topology _topology;
			private int _first;
			private int _last;

			public FaceEdgesIndexer(Topology topology){ _topology = topology; _first = 0; _last = _topology.edgeData.Length; }
			public FaceEdgesIndexer(Topology topology, int first, int last) { _topology = topology; _first = first; _last = last; }
			public FaceEdge this[int i] { get { return new FaceEdge(_topology, _first + i); } }
			public int Count { get { return _last - _first; } }
			public EdgeEnumerator GetEnumerator() { return new EdgeEnumerator(_topology, _first, _last); }

			public struct EdgeEnumerator
			{
				private Topology _topology;
				private int _index;
				private int _next;
				private int _last;

				public EdgeEnumerator(Topology topology, int first, int last) { _topology = topology; _index = 0; _next = first; _last = last; }
				public FaceEdge Current { get { return new FaceEdge(_topology, _index); } }
				public bool MoveNext() { return (_index = _next++) != _last; }
				public void Reset() { throw new NotSupportedException(); }
			}
		}

		public FaceEdgesIndexer faceEdges { get { return new FaceEdgesIndexer(this); } }
		public FaceEdgesIndexer internalFaceEdges { get { return new FaceEdgesIndexer(this, 0, firstExternalEdgeIndex); } }
		public FaceEdgesIndexer externalFaceEdges { get { return new FaceEdgesIndexer(this, firstExternalEdgeIndex, edgeData.Length); } }
	}

	public interface IEdgeAttribute<T> : IList<T>
	{
		T this[Topology.HalfEdge e] { get; set; }
		T this[Topology.VertexEdge e] { get; set; }
		T this[Topology.FaceEdge e] { get; set; }
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

		protected static TDerived CreateDerivedInstance<TDerived>() where TDerived : EdgeConstantAttribute<T>
		{
			return CreateInstance<TDerived>();
		}

		protected static TDerived CreateDerivedInstance<TDerived>(T constant) where TDerived : EdgeConstantAttribute<T>
		{
			var instance = CreateInstance<TDerived>();
			instance.constant = constant;
			return instance;
		}

		protected static TDerived CreateDerivedInstance<TDerived>(T constant, string name) where TDerived : EdgeConstantAttribute<T>
		{
			var instance = CreateInstance<TDerived>();
			instance.constant = constant;
			instance.name = name;
			return instance;
		}

		protected static TDerived CreateDerivedInstance<TDerived>(string name) where TDerived : EdgeConstantAttribute<T>
		{
			var instance = CreateInstance<TDerived>();
			instance.name = name;
			return instance;
		}

		protected TDerived CloneDerived<TDerived>() where TDerived : EdgeConstantAttribute<T>
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

		protected static TDerived CreateDerivedInstance<TDerived>() where TDerived : EdgeArrayAttribute<T>
		{
			return CreateInstance<TDerived>();
		}

		protected static TDerived CreateDerivedInstance<TDerived>(T[] array) where TDerived : EdgeArrayAttribute<T>
		{
			var attribute = CreateInstance<TDerived>();
			attribute.array = array;
			return attribute;
		}

		protected static TDerived CreateDerivedInstance<TDerived>(T[] array, string name) where TDerived : EdgeArrayAttribute<T>
		{
			var attribute = CreateInstance<TDerived>();
			attribute.array = array;
			attribute.name = name;
			return attribute;
		}

		protected static TDerived CreateDerivedInstance<TDerived>(string name) where TDerived : EdgeArrayAttribute<T>
		{
			var attribute = CreateInstance<TDerived>();
			attribute.name = name;
			return attribute;
		}

		protected TDerived CloneDerived<TDerived>() where TDerived : EdgeArrayAttribute<T>
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
