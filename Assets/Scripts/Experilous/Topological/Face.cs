using UnityEngine;
using System;

namespace Experilous.Topological
{
	public partial class Topology
	{
		[SerializeField]
		protected ushort[] _faceNeighborCounts;
		[SerializeField]
		protected int[] _faceFirstEdgeIndices;

		[SerializeField]
		protected int _firstExternalFaceIndex;

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
			public bool isInternal { get { return (_topology._faceNeighborCounts[_index] & 0x8000u) == 0x0000u; } }
			public bool isExternal { get { return (_topology._faceNeighborCounts[_index] & 0x8000u) == 0x8000u; } }
			public int neighborCount { get { return (int)(_topology._faceNeighborCounts[_index] & 0x7FFFu); } }
			public FaceEdge firstEdge { get { return new FaceEdge(_topology, _topology._faceFirstEdgeIndices[_index]); } }

			public bool isInitialized { get { return _topology != null; } }

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

				public int Count { get { return (int)(_topology._faceNeighborCounts[_index] & 0x7FFFu); } }
				
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
					return new FaceEdgeEnumerator(_topology, _topology._faceFirstEdgeIndices[_index]);
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

		public struct FacesIndexer : IFacesIndexer
		{
			private Topology _topology;
			private int _first;
			private int _last;

			public FacesIndexer(Topology topology) { _topology = topology; _first = 0; _last = _topology._faceFirstEdgeIndices.Length; }
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
		public FacesIndexer externalFaces { get { return new FacesIndexer(this, _firstExternalFaceIndex, _faceFirstEdgeIndices.Length); } }
	}

	public interface IFacesIndexer
	{
		Topology.Face this[int i] { get; }
		int Count { get; }
	}

	public struct CartesianIndex2D
	{
		public int x;
		public int y;

		public CartesianIndex2D(int x, int y)
		{
			this.x = x;
			this.y = y;
		}
	}

	public class CartesianQuadFaceMapper
	{
		private int _horizontalStride;
		private int _verticalStride;
		private int _originIndex;

		public CartesianQuadFaceMapper(int verticalStride)
		{
			_horizontalStride = 1;
			_verticalStride = verticalStride;
			_originIndex = 0;
		}

		public CartesianQuadFaceMapper(int horizontalStride, int verticalStride, int originIndex)
		{
			_horizontalStride = horizontalStride;
			_verticalStride = verticalStride;
			_originIndex = originIndex;
		}

		public CartesianIndex2D MapToCartesian(int faceIndex)
		{
			return new CartesianIndex2D(-1, -1); //TODO
		}

		public int MapFromCartesian(CartesianIndex2D cartesianIndex)
		{
			return _originIndex + cartesianIndex.x * _horizontalStride + cartesianIndex.y * _verticalStride;
		}

		public struct RangeIndexer : IFacesIndexer
		{
			private Topology _topology;
			private int _minorCount;
			private int _count;
			private int _minorStride;
			private int _majorStride;
			private int _originIndex;

			public RangeIndexer(Topology topology, int minorCount, int count, int minorStride, int majorStride, int originIndex)
			{
				_topology = topology;
				_minorCount = minorCount;
				_count = count;
				_minorStride = minorStride;
				_majorStride = majorStride;
				_originIndex = originIndex;
			}

			public Topology.Face this[int i] { get { return new Topology.Face(_topology, _originIndex + (i % _minorCount) * _minorStride + (i / _minorCount) * _majorStride); } }
			public int Count { get { return _count; } }
		}

		public IFacesIndexer GetRangeIndexer(Topology topology, CartesianIndex2D startCorner, CartesianIndex2D endCorner)
		{
			//TODO negatives
			var originIndex = MapFromCartesian(startCorner);
			var minorCount = endCorner.x - startCorner.x + 1;
			var count = minorCount * (endCorner.y - startCorner.y + 1);
			return new RangeIndexer(topology, minorCount, count, 1, _verticalStride, originIndex);
		}
	}

	public static class FaceExtensions
	{
		public static T Of<T>(this T[] attributeArray, Topology.Face face)
		{
			return attributeArray[face.index];
		}

		public static T Of<T>(this FaceAttribute<T> attribute, Topology.Face face) where T : new()
		{
			return attribute.array[face.index];
		}

		public static T Of<T>(this IFaceAttribute<T> attribute, Topology.Face face) where T : new()
		{
			return attribute[face.index];
		}

		public static Topology.Face GetFace(this Topology topology, CartesianIndex2D cartesianIndex, CartesianQuadFaceMapper mapper)
		{
			return topology.faces[mapper.MapFromCartesian(cartesianIndex)];
		}

		public static CartesianIndex2D GetCartesianIndex(this Topology.Face face, CartesianQuadFaceMapper mapper)
		{
			return mapper.MapToCartesian(face.index);
		}
	}

	public interface IFaceAttribute<T> where T : new()
	{
		T this[int i] { get; set; }
		T this[Topology.Face f] { get; set; }
		int Length { get; }
	}

	public class FaceAttribute<T> : ScriptableObject, IFaceAttribute<T> where T : new()
	{
		public T[] array;

		protected FaceAttribute()
		{
		}

		protected static TDerived CreateDerivedInstance<TDerived>() where TDerived : FaceAttribute<T>
		{
			return CreateInstance<TDerived>();
		}

		protected static TDerived CreateDerivedInstance<TDerived>(T[] array) where TDerived : FaceAttribute<T>
		{
			var instance = CreateInstance<TDerived>();
			instance.array = array;
			return instance;
		}

		protected static TDerived CreateDerivedInstance<TDerived>(T[] array, string name) where TDerived : FaceAttribute<T>
		{
			var instance = CreateInstance<TDerived>();
			instance.array = array;
			instance.name = name;
			return instance;
		}

		protected static TDerived CreateDerivedInstance<TDerived>(string name) where TDerived : FaceAttribute<T>
		{
			var instance = CreateInstance<TDerived>();
			instance.name = name;
			return instance;
		}

		public T this[int i]
		{
			get { return array[i]; }
			set { array[i] = value; }
		}

		public T this[Topology.Face v]
		{
			get { return array[v.index]; }
			set { array[v.index] = value; }
		}

		public int Length
		{
			get { return array.Length; }
		}
	}

	public abstract class FaceIndexer2D : ScriptableObject
	{
		public struct Coordinate
		{
			public int x;
			public int y;

			public Coordinate(int x, int y)
			{
				this.x = x;
				this.y = y;
			}
		}

		public abstract Topology.Face GetFace(int x, int y);
		public abstract Topology.Face GetFace(Coordinate coordinate);
		public abstract int GetFaceIndex(int x, int y);
		public abstract int GetFaceIndex(Coordinate coordinate);
		public abstract Coordinate GetCoordinate(int faceIndex);
		public abstract Coordinate GetCoordinate(Topology.Face face);
	}
}
