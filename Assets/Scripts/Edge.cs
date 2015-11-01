using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Tiling
{
	public partial class Topology
	{
		private struct EdgeNeighbor
		{
			public int _vertex; // The vertex that preceeds the below face when going around the edge clockwise.
			public int _face; // The face that follows after the above vertex when going around the vertex clockwise.

			public EdgeNeighbor(int vertex, int face) { _vertex = vertex; _face = face; }
		}

		private EdgeNeighbor[,] _edgeNeighbors;

		public struct Edge : IEquatable<Edge>
		{
			private Topology _topology;
			private int _index;

			public Edge(Topology topology, int index)
			{
				_topology = topology;
				_index = index;
			}

			public int index { get { return _index; } }

			public struct Neighbor
			{
				private Topology _topology;
				private int _edgeIndex;
				private int _neighborIndex;

				public Neighbor(Topology topology, int edgeIndex, int neighborIndex)
				{
					_topology = topology;
					_edgeIndex = edgeIndex;
					_neighborIndex = neighborIndex;
				}

				public int index { get { return _neighborIndex; } }

				public Vertex vertex { get { return new Vertex(_topology, _topology._edgeNeighbors[_edgeIndex, _neighborIndex]._vertex); } }
				public Face face { get { return new Face(_topology, _topology._edgeNeighbors[_edgeIndex, _neighborIndex]._face); } }

				public Neighbor prev { get { return new Neighbor(_topology, _edgeIndex, (_neighborIndex + 1) & 1); } }
				public Neighbor next { get { return new Neighbor(_topology, _edgeIndex, (_neighborIndex + 1) & 1); } }
			}

			public struct NeighborsIndexer
			{
				private Topology _topology;
				private int _index;

				public NeighborsIndexer(Topology topology, int index)
				{
					_topology = topology;
					_index = index;
				}

				public int Count { get { return 2; } }
				
				public struct NeighborEnumerator
				{
					private Topology _topology;
					private int _edgeIndex;
					private int _neighborIndex;

					public NeighborEnumerator(Topology topology, int edgeIndex)
					{
						_topology = topology;
						_edgeIndex = edgeIndex;
						_neighborIndex = -1;
					}

					public Neighbor Current { get { return new Neighbor(_topology, _edgeIndex, _neighborIndex); } }

					public bool MoveNext()
					{
						return ++_neighborIndex < 2;
					}

					public void Reset()
					{
						_neighborIndex = -1;
					}
				}

				public NeighborEnumerator GetEnumerator()
				{
					return new NeighborEnumerator(_topology, _index);
				}
			}

			public NeighborsIndexer Neighbors { get { return new NeighborsIndexer(_topology, _index); } }

			public Neighbor FindNeighbor(Vertex vertex)
			{
				Neighbor neighbor;
				if (!TryFindNeighbor(vertex, out neighbor)) throw new InvalidOperationException("The specified vertex is not a neighbor of this edge.");
				return neighbor;
			}

			public Neighbor FindNeighbor(Face face)
			{
				Neighbor neighbor;
				if (!TryFindNeighbor(face, out neighbor)) throw new InvalidOperationException("The specified face is not a neighbor of this edge.");
				return neighbor;
			}

			public bool TryFindNeighbor(Vertex vertex, out Neighbor neighbor)
			{
				if (_topology._edgeNeighbors[_index, 0]._vertex == vertex.index)
				{
					neighbor = new Neighbor(_topology, _index, 0);
					return true;
				}
				else if (_topology._edgeNeighbors[_index, 1]._vertex == vertex.index)
				{
					neighbor = new Neighbor(_topology, _index, 1);
					return true;
				}
				else
				{
					neighbor = new Neighbor();
					return false;
				}
			}

			public bool TryFindNeighbor(Face face, out Neighbor neighbor)
			{
				if (_topology._edgeNeighbors[_index, 0]._face == face.index)
				{
					neighbor = new Neighbor(_topology, _index, 0);
					return true;
				}
				else if (_topology._edgeNeighbors[_index, 1]._face == face.index)
				{
					neighbor = new Neighbor(_topology, _index, 1);
					return true;
				}
				else
				{
					neighbor = new Neighbor();
					return false;
				}
			}

			public Vertex GetOpposite(Vertex vertex) { return FindNeighbor(vertex).next.vertex; }
			public Face GetOpposite(Face face) { return FindNeighbor(face).next.face; }

			public override bool Equals(object other) { return other is Edge && _index == ((Edge)other)._index; }
			public bool Equals(Edge other) { return _index == other._index; }
			public static bool operator ==(Edge lhs, Edge rhs) { return lhs._index == rhs._index; }
			public static bool operator !=(Edge lhs, Edge rhs) { return lhs._index != rhs._index; }
			public override int GetHashCode() { return _index.GetHashCode(); }
		}

		public struct EdgesIndexer : IEnumerable<Edge>
		{
			private Topology _topology;
			public EdgesIndexer(Topology topology) { _topology = topology; }
			public Edge this[int i] { get { return new Edge(_topology, i); } }
			public int Count { get { return _topology._edgeNeighbors.Length; } }
			public IEnumerator<Edge> GetEnumerator() { for (int i = 0; i < _topology._edgeNeighbors.Length; ++i) yield return new Edge(_topology, i); }
			IEnumerator IEnumerable.GetEnumerator() { return (this as IEnumerable<Edge>).GetEnumerator(); }
		}

		public EdgesIndexer edges { get { return new EdgesIndexer(this); } }

		public struct EdgeAttribute<T> where T : new()
		{
			public T[] _values;

			public EdgeAttribute(int edgeCount)
			{
				_values = new T[edgeCount];
			}

			public T this[int i]
			{
				get {  return _values[i]; }
				set {  _values[i] = value; }
			}

			public T this[Edge edge]
			{
				get {  return _values[edge.index]; }
				set { _values[edge.index] = value; }
			}

			public int Count
			{
				get { return _values.Length; }
			}
		}
	}
}
