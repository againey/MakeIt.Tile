using UnityEngine;
using System;

namespace Experilous.Topological
{
	public partial class Topology
	{
		[SerializeField]
		private ushort[] _vertexNeighborCounts;
		[SerializeField]
		private int[] _vertexFirstEdgeIndices;

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
			public int neighborCount { get { return _topology._vertexNeighborCounts[_index]; } }
			public VertexEdge firstEdge { get { return new VertexEdge(_topology, _topology._vertexFirstEdgeIndices[_index]); } }

			public bool isInitialized { get { return _topology != null; } }

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

			public static implicit operator int(Vertex vertex)
			{
				return vertex._index;
			}

			public T Attribute<T>(T[] attributeArray)
			{
				return attributeArray[_index];
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

				public int Count { get { return _topology._vertexNeighborCounts[_index]; } }
				
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
							_nextEdgeIndex = _topology._edgeData[_currentEdgeIndex]._vNext;
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
					return new VertexEdgeEnumerator(_topology, _topology._vertexFirstEdgeIndices[_index]);
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
			public int Count { get { return _topology._vertexFirstEdgeIndices.Length; } }
			public VertexEnumerator GetEnumerator() { return new VertexEnumerator(_topology); }

			public struct VertexEnumerator
			{
				private Topology _topology;
				private int _current;

				public VertexEnumerator(Topology topology) { _topology = topology; _current = -1; }
				public Vertex Current { get { return new Vertex(_topology, _current); } }
				public bool MoveNext() { return ++_current < _topology._vertexFirstEdgeIndices.Length; }
				public void Reset() { throw new NotSupportedException(); }
			}
		}

		public VerticesIndexer vertices { get { return new VerticesIndexer(this); } }
	}

	public static class VertexExtensions
	{
		public static T Of<T>(this T[] attributArray, Topology.Vertex vertex)
		{
			return attributArray[vertex.index];
		}
	}

	public interface IVertexAttribute<T> where T : new()
	{
		T this[int i] { get; set; }
		T this[Topology.Vertex v] { get; set; }
	}
}
