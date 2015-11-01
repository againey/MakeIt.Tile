using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Tiling
{
	public partial class Topology
	{
		private struct VertexNeighbor
		{
			public int _prev; // The previous vertex neighbor.
			public int _next; // The next vertex neighbor.
			public int _vertex; // The vertex that is on the other end of the below edge, and which preceeds the face below when going around the vertex clockwise.
			public int _edge; // The edge that is between this vertex and the vertex above, and which preceeds the below face when going around the vertex clockwise.
			public int _face; // The face that follows after the above vertex and edge when going around the vertex clockwise.

			public VertexNeighbor(int prev, int next, int vertex, int edge, int face) { _prev = prev; _next = next; _vertex = vertex; _edge = edge; _face = face; }
		}

		private struct VertexRoot
		{
			private uint _data;

			public VertexRoot(int neighborCount, int rootIndex)
			{
				_data = (((uint)neighborCount & 0xFF) << 24) & ((uint)rootIndex & 0xFFFFFF);
			}

			public int neighborCount
			{
				get
				{
					return (int)((_data >> 24) & 0xFF);
				}
				set
				{
					_data = (_data & 0xFFFFFF) | (((uint)value & 0xFF) << 24);
				}
			}

			public int rootIndex
			{
				get
				{
					return (int)(_data & 0xFFFFFF);
				}
				set
				{
					_data = (_data & 0xFF000000) | ((uint)value & 0xFFFFFF);
				}
			}
		}

		private VertexRoot[] _vertexRoots;
		private VertexNeighbor[] _vertexNeighbors;

		public struct Vertex : IEquatable<Vertex>
		{
			private Topology _topology;
			private int _index;

			public Vertex(Topology topology, int index)
			{
				_topology = topology;
				_index = index;
			}

			public int index { get { return _index; } }

			public struct Neighbor
			{
				private Topology _topology;
				private int _index;

				public Neighbor(Topology topology, int index)
				{
					_topology = topology;
					_index = index;
				}

				public int index { get { return _index; } }

				public Vertex vertex { get { return new Vertex(_topology, _topology._vertexNeighbors[_index]._vertex); } }
				public Edge edge { get { return new Edge(_topology, _topology._vertexNeighbors[_index]._edge); } }
				public Face face { get { return new Face(_topology, _topology._vertexNeighbors[_index]._face); } }

				public Neighbor prev { get { return new Neighbor(_topology, _topology._vertexNeighbors[_index]._prev); } }
				public Neighbor next { get { return new Neighbor(_topology, _topology._vertexNeighbors[_index]._next); } }
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

				public int Count { get { return _topology._vertexRoots[_index].neighborCount; } }
				
				public struct NeighborEnumerator
				{
					private Topology _topology;
					private int _index;
					private int _first;

					public NeighborEnumerator(Topology topology, int first)
					{
						_topology = topology;
						_index = _topology._vertexNeighbors[first]._prev;
						_first = int.MinValue;
					}

					public Neighbor Current { get { return new Neighbor(_topology, _index); } }

					public bool MoveNext()
					{
						_index = _topology._vertexNeighbors[_index]._next;
						if (_first == int.MinValue)
						{
							_first = _index;
							return true;
						}
						else
						{
							return (_index != _first);
						}
					}

					public void Reset()
					{
						_index = _topology._vertexNeighbors[_first]._prev;
						_first = int.MinValue;
					}
				}

				public NeighborEnumerator GetEnumerator()
				{
					return new NeighborEnumerator(_topology, _topology._vertexRoots[_index].rootIndex);
				}
			}

			public NeighborsIndexer Neighbors { get { return new NeighborsIndexer(_topology, _index); } }

			public Neighbor FindNeighbor(Vertex vertex)
			{
				Neighbor neighbor;
				if (!TryFindNeighbor(vertex, out neighbor)) throw new InvalidOperationException("The specified vertex is not a neighbor of this vertex.");
				return neighbor;
			}

			public Neighbor FindNeighbor(Edge edge)
			{
				Neighbor neighbor;
				if (!TryFindNeighbor(edge, out neighbor)) throw new InvalidOperationException("The specified edge is not a neighbor of this vertex.");
				return neighbor;
			}

			public Neighbor FindNeighbor(Face face)
			{
				Neighbor neighbor;
				if (!TryFindNeighbor(face, out neighbor)) throw new InvalidOperationException("The specified face is not a neighbor of this vertex.");
				return neighbor;
			}

			public bool TryFindNeighbor(Vertex vertex, out Neighbor neighbor)
			{
				var vertexRoot = _topology._vertexRoots[_index];
				var neighborCount = vertexRoot.neighborCount;
				var neighborIndex = vertexRoot.rootIndex;
				while (neighborCount > 0)
				{
					if (_topology._vertexNeighbors[neighborIndex]._vertex == vertex.index)
					{
						neighbor = new Neighbor(_topology, neighborIndex);
						return true;
					}
					--neighborCount;
				}
				neighbor = new Neighbor();
				return false;
			}

			public bool TryFindNeighbor(Edge edge, out Neighbor neighbor)
			{
				var vertexRoot = _topology._vertexRoots[_index];
				var neighborCount = vertexRoot.neighborCount;
				var neighborIndex = vertexRoot.rootIndex;
				while (neighborCount > 0)
				{
					if (_topology._vertexNeighbors[neighborIndex]._edge == edge.index)
					{
						neighbor = new Neighbor(_topology, neighborIndex);
						return true;
					}
					--neighborCount;
				}
				neighbor = new Neighbor();
				return false;
			}

			public bool TryFindNeighbor(Face face, out Neighbor neighbor)
			{
				var vertexRoot = _topology._vertexRoots[_index];
				var neighborCount = vertexRoot.neighborCount;
				var neighborIndex = vertexRoot.rootIndex;
				while (neighborCount > 0)
				{
					if (_topology._vertexNeighbors[neighborIndex]._face == face.index)
					{
						neighbor = new Neighbor(_topology, neighborIndex);
						return true;
					}
					--neighborCount;
				}
				neighbor = new Neighbor();
				return false;
			}

			public override bool Equals(object other) { return other is Vertex && _index == ((Vertex)other)._index; }
			public bool Equals(Vertex other) { return _index == other._index; }
			public static bool operator ==(Vertex lhs, Vertex rhs) { return lhs._index == rhs._index; }
			public static bool operator !=(Vertex lhs, Vertex rhs) { return lhs._index != rhs._index; }
			public override int GetHashCode() { return _index.GetHashCode(); }
		}

		public struct VerticesIndexer : IEnumerable<Vertex>
		{
			private Topology _topology;
			public VerticesIndexer(Topology topology) { _topology = topology; }
			public Vertex this[int i] { get { return new Vertex(_topology, i); } }
			public int Count { get { return _topology._vertexRoots.Length; } }
			public IEnumerator<Vertex> GetEnumerator() { for (int i = 0; i < _topology._vertexRoots.Length; ++i) yield return new Vertex(_topology, i); }
			IEnumerator IEnumerable.GetEnumerator() { return (this as IEnumerable<Vertex>).GetEnumerator(); }
		}

		public VerticesIndexer vertices { get { return new VerticesIndexer(this); } }

		public struct VertexAttribute<T> where T : new()
		{
			public T[] _values;

			public VertexAttribute(int vertexCount)
			{
				_values = new T[vertexCount];
			}

			public T this[int i]
			{
				get {  return _values[i]; }
				set {  _values[i] = value; }
			}

			public T this[Vertex vertex]
			{
				get {  return _values[vertex.index]; }
				set { _values[vertex.index] = value; }
			}

			public int Count
			{
				get { return _values.Length; }
			}
		}
	}
}
