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

			public VertexEdge twin { get { return new VertexEdge(_topology, _topology._edgeData[_index]._twin); } }
			public VertexEdge prev { get { return new VertexEdge(_topology, _topology._edgeData[_topology._edgeData[_index]._twin]._fNext); } }
			public VertexEdge next { get { return new VertexEdge(_topology, _topology._edgeData[_index]._vNext); } }
			public Vertex nearVertex { get { return new Vertex(_topology, _topology._edgeData[_topology._edgeData[_index]._twin]._vertex); } }
			public Vertex farVertex { get { return new Vertex(_topology, _topology._edgeData[_index]._vertex); } }
			public Face prevFace { get { return new Face(_topology, _topology._edgeData[_index]._face); } }
			public Face nextFace { get { return new Face(_topology, _topology._edgeData[_topology._edgeData[_index]._twin]._face); } }

			public bool isBoundary { get { return prevFace.isExternal != nextFace.isExternal; } }
			public bool isOuterBoundary { get { return prevFace.isExternal; } }
			public bool isInnerBoundary { get { return nextFace.isExternal; } }
			public bool isInternal { get { return nextFace.isInternal; } }
			public bool isExternal { get { return prevFace.isExternal; } }

			public FaceEdge faceEdge { get { return new FaceEdge(_topology, _index); } }

			public static implicit operator int(VertexEdge edge)
			{
				return edge._index;
			}

			public T Attribute<T>(T[] attributeArray)
			{
				return attributeArray[_index];
			}

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

			public FaceEdge twin { get { return new FaceEdge(_topology, _topology._edgeData[_index]._twin); } }
			public FaceEdge prev { get { return new FaceEdge(_topology, _topology._edgeData[_topology._edgeData[_index]._vNext]._twin); } }
			public FaceEdge next { get { return new FaceEdge(_topology, _topology._edgeData[_index]._fNext); } }
			public Vertex prevVertex { get { return new Vertex(_topology, _topology._edgeData[_topology._edgeData[_index]._twin]._vertex); } }
			public Vertex nextVertex { get { return new Vertex(_topology, _topology._edgeData[_index]._vertex); } }
			public Face nearFace { get { return new Face(_topology, _topology._edgeData[_topology._edgeData[_index]._twin]._face); } }
			public Face farFace { get { return new Face(_topology, _topology._edgeData[_index]._face); } }

			public bool isBoundary { get { return farFace.isExternal != nearFace.isExternal; } }
			public bool isOuterBoundary { get { return farFace.isExternal; } }
			public bool isInnerBoundary { get { return nearFace.isExternal; } }
			public bool isInternal { get { return nearFace.isInternal; } }
			public bool isExternal { get { return farFace.isExternal; } }

			public VertexEdge vertexEdge { get { return new VertexEdge(_topology, _index); } }

			public static implicit operator int(FaceEdge edge)
			{
				return edge._index;
			}

			public T Attribute<T>(T[] attributeArray)
			{
				return attributeArray[_index];
			}

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
			private int _first;
			private int _last;

			public VertexEdgesIndexer(Topology topology){ _topology = topology; _first = 0; _last = _topology._edgeData.Length; }
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
		public VertexEdgesIndexer internalVertexEdges { get { return new VertexEdgesIndexer(this, 0, _firstExternalEdgeIndex); } }
		public VertexEdgesIndexer externalVertexEdges { get { return new VertexEdgesIndexer(this, _firstExternalEdgeIndex, _edgeData.Length); } }

		public struct FaceEdgesIndexer
		{
			private Topology _topology;
			private int _first;
			private int _last;

			public FaceEdgesIndexer(Topology topology){ _topology = topology; _first = 0; _last = _topology._edgeData.Length; }
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
		public FaceEdgesIndexer internalFaceEdges { get { return new FaceEdgesIndexer(this, 0, _firstExternalEdgeIndex); } }
		public FaceEdgesIndexer externalFaceEdges { get { return new FaceEdgesIndexer(this, _firstExternalEdgeIndex, _edgeData.Length); } }
	}

	public interface IEdgeAttribute<T> : IList<T>
	{
		T this[Topology.VertexEdge e] { get; set; }
		T this[Topology.FaceEdge e] { get; set; }
	}

	public abstract class EdgeAttribute<T> : ScriptableObject, IEdgeAttribute<T>
	{
		public abstract T this[int i] { get; set; }
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
}
