using System;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public partial class Topology
	{
		private NodeData[] _faceData;

		public struct Face : IEquatable<Face>, IComparable<Face>
		{
			private Topology _topology;
			private int _index;

			public Face(Topology topology, int index)
			{
				_topology = topology;
				_index = index;
			}

			public Topology topology { get { return _topology; } }

			public int index { get { return _index; } }
			public int neighborCount { get { return _topology._faceData[_index].neighborCount; } }
			public FaceEdge firstEdge { get { return new FaceEdge(_topology, _topology._faceData[_index].firstEdge); } }

			public struct FaceEdgesIndexer
			{
				private Topology _topology;
				private int _index;

				public FaceEdgesIndexer(Topology topology, int index)
				{
					_topology = topology;
					_index = index;
				}

				public int Count { get { return _topology._faceData[_index].neighborCount; } }
				
				public struct FaceEdgeEnumerator
				{
					private Topology _topology;
					private int _firstEdgeIndex;
					private int _currentEdgeIndex;
					private int _nextEdgeIndex;

					public FaceEdgeEnumerator(Topology topology, int firstEdgeIndex)
					{
						_topology = topology;
						_firstEdgeIndex = firstEdgeIndex;
						_currentEdgeIndex = -1;
						_nextEdgeIndex = firstEdgeIndex;
					}

					public FaceEdge Current { get { return new FaceEdge(_topology, _currentEdgeIndex); } }

					public bool MoveNext()
					{
						if (_currentEdgeIndex == -1 || _nextEdgeIndex != _firstEdgeIndex)
						{
							_currentEdgeIndex = _nextEdgeIndex;
							_nextEdgeIndex = _topology._edgeData[_topology._edgeData[_currentEdgeIndex]._prev]._twin;
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

				public FaceEdgeEnumerator GetEnumerator()
				{
					return new FaceEdgeEnumerator(_topology, _topology._faceData[_index].firstEdge);
				}
			}

			public FaceEdgesIndexer edges { get { return new FaceEdgesIndexer(_topology, _index); } }

			public FaceEdge FindEdge(Vertex vertex)
			{
				FaceEdge neighbor;
				if (!TryFindEdge(vertex, out neighbor)) throw new InvalidOperationException("The specified vertex is not a neighbor of this face.");
				return neighbor;
			}

			public FaceEdge FindEdge(Face face)
			{
				FaceEdge edge;
				if (!TryFindEdge(face, out edge)) throw new InvalidOperationException("The specified face is not a neighbor of this face.");
				return edge;
			}

			public bool TryFindEdge(Vertex vertex, out FaceEdge edge)
			{
				foreach (var faceEdge in edges)
				{
					if (faceEdge.nextVertex == vertex)
					{
						edge = faceEdge;
						return true;
					}
				}
				edge = new FaceEdge();
				return false;
			}

			public bool TryFindEdge(Face face, out FaceEdge edge)
			{
				foreach (var faceEdge in edges)
				{
					if (faceEdge.farFace == face)
					{
						edge = faceEdge;
						return true;
					}
				}
				edge = new FaceEdge();
				return false;
			}

			public override bool Equals(object other) { return other is Face && _index == ((Face)other)._index; }
			public bool Equals(Face other) { return _index == other._index; }
			public int CompareTo(Face other) { return _index - other._index; }
			public static bool operator ==(Face lhs, Face rhs) { return lhs._index == rhs._index; }
			public static bool operator !=(Face lhs, Face rhs) { return lhs._index != rhs._index; }
			public static bool operator < (Face lhs, Face rhs) { return lhs._index <  rhs._index; }
			public static bool operator > (Face lhs, Face rhs) { return lhs._index >  rhs._index; }
			public static bool operator <=(Face lhs, Face rhs) { return lhs._index <= rhs._index; }
			public static bool operator >=(Face lhs, Face rhs) { return lhs._index >= rhs._index; }
			public override int GetHashCode() { return _index.GetHashCode(); }

			public override string ToString()
			{
				var sb = new System.Text.StringBuilder();
				sb.AppendFormat("Face {0} (", _index);
				foreach (var edge in edges)
					sb.AppendFormat(edge.next != firstEdge ? "{0}, " : "{0}), (", edge.farFace.index);
				foreach (var edge in edges)
					sb.AppendFormat(edge.next != firstEdge ? "{0}, " : "{0})", edge.nextVertex.index);
				return sb.ToString();
			}
		}

		public struct FacesIndexer
		{
			private Topology _topology;

			public FacesIndexer(Topology topology){ _topology = topology; }
			public Face this[int i] { get { return new Face(_topology, i); } }
			public int Count { get { return _topology._faceData.Length; } }
			public FaceEnumerator GetEnumerator() { return new FaceEnumerator(_topology); }

			public struct FaceEnumerator
			{
				private Topology _topology;
				private int _index;

				public FaceEnumerator(Topology topology) { _topology = topology; _index = -1; }
				public Face Current { get { return new Face(_topology, _index); } }
				public bool MoveNext() { return (++_index < _topology._faceData.Length); }
				public void Reset() { _index = -1; }
			}
		}

		public FacesIndexer faces { get { return new FacesIndexer(this); } }
	}
}
