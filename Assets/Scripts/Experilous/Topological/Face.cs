using UnityEngine;
using System;

namespace Experilous.Topological
{
	public partial class Topology
	{
		[SerializeField]
		private NodeData[] _faceData;

		[SerializeField]
		private int _firstExternalFaceIndex;

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
			public bool isInternal { get { return !_topology._faceData[_index].isExternal; } }
			public bool isExternal { get { return _topology._faceData[_index].isExternal; } }
			public int neighborCount { get { return _topology._faceData[_index].neighborCount; } }
			public FaceEdge firstEdge { get { return new FaceEdge(_topology, _topology._faceData[_index].firstEdge); } }

			public bool isInitialized { get { return _topology != null; } }

			public bool hasExternalVertexNeighbor
			{
				get
				{
					foreach (var edge in edges)
						if (edge.nextVertex.isExternal)
							return true;
					return false;
				}
			}

			public bool hasExternalFaceNeighbor
			{
				get
				{
					foreach (var edge in edges)
						if (edge.farFace.isExternal)
							return true;
					return false;
				}
			}

			public static implicit operator int(Face face)
			{
				return face._index;
			}

			public T Attribute<T>(T[] attributeArray)
			{
				return attributeArray[_index];
			}

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
							_nextEdgeIndex = _topology._edgeData[_currentEdgeIndex]._fNext;
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
			private int _first;
			private int _last;

			public FacesIndexer(Topology topology) { _topology = topology; _first = 0; _last = _topology._faceData.Length; }
			public FacesIndexer(Topology topology, int first, int last) { _topology = topology; _first = first; _last = last; }
			public Face this[int i] { get { return new Face(_topology, _first + i); } }
			public int Count { get { return _last - _first; } }
			public FaceEnumerator GetEnumerator() { return new FaceEnumerator(_topology, _first, _last); }

			public struct FaceEnumerator
			{
				private Topology _topology;
				private int _index;
				private int _next;
				private int _last;

				public FaceEnumerator(Topology topology, int first, int last) { _topology = topology; _index = 0; _next = first; _last = last; }
				public Face Current { get { return new Face(_topology, _index); } }
				public bool MoveNext() { return (_index = _next++) != _last; }
				public void Reset() { throw new NotSupportedException(); }
			}
		}

		public FacesIndexer faces { get { return new FacesIndexer(this); } }
		public FacesIndexer internalFaces { get { return new FacesIndexer(this, 0, _firstExternalFaceIndex); } }
		public FacesIndexer externalFaces { get { return new FacesIndexer(this, _firstExternalFaceIndex, _faceData.Length); } }
	}

	public static class FaceExtensions
	{
		public static T Of<T>(this T[] attributArray, Topology.Face face)
		{
			return attributArray[face.index];
		}
	}
}
