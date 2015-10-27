using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Tiling
{
	public struct Edge : IEquatable<Edge>
	{
		private Topology _topology;
		private int _index;

		public Edge(Topology topology, int index)
		{
			_topology = topology;
			_index = index;
		}

		public int Index { get { return _index; } }

		public int NeighborCount { get { return 2; } }

		public struct TilesIndexer : IEnumerable<Tile>
		{
			private Topology _topology;
			private int _index;

			public TilesIndexer(Topology topology, int index)
			{
				_topology = topology;
				_index = index;
			}

			public Tile this[int i] { get { return new Tile(_topology, _topology._edgeTiles[_index, i]); } }
			public int Count { get { return 2; } }
			public IEnumerator<Tile> GetEnumerator() { for (int i = 0; i < 2; ++i) yield return this[i]; }
			IEnumerator IEnumerable.GetEnumerator() { return (this as IEnumerable<Tile>).GetEnumerator(); }
			public Tile First { get { return this[0]; } }
			public Tile Last { get { return this[Count - 1]; } }
		}

		public TilesIndexer Tiles { get { return new TilesIndexer(_topology, _index); } }

		public struct CornersIndexer : IEnumerable<Corner>
		{
			private Topology _topology;
			private int _index;

			public CornersIndexer(Topology topology, int index)
			{
				_topology = topology;
				_index = index;
			}

			public Corner this[int i] { get { return new Corner(_topology, _topology._edgeCorners[_index, i]); } }
			public int Count { get { return 2; } }
			public IEnumerator<Corner> GetEnumerator() { for (int i = 0; i < 2; ++i) yield return this[i]; }
			IEnumerator IEnumerable.GetEnumerator() { return (this as IEnumerable<Corner>).GetEnumerator(); }
			public Corner First { get { return this[0]; } }
			public Corner Last { get { return this[Count - 1]; } }
		}

		public CornersIndexer Corners { get { return new CornersIndexer(_topology, _index); } }

		public int NeighborIndexOf(Tile tile)
		{
			if (_topology._edgeTiles[_index, 0] == tile.Index) return 0;
			else if (_topology._edgeTiles[_index, 1] == tile.Index) return 1;
			throw new ApplicationException("The specified tile is not a neighbor of this edge.");
		}

		public int NeighborIndexOf(Corner corner)
		{
			if (_topology._edgeCorners[_index, 0] == corner.Index) return 0;
			else if (_topology._edgeCorners[_index, 1] == corner.Index) return 1;
			throw new ApplicationException("The specified corner is not a neighbor of this edge.");
		}

		public int RotateNeighborIndex(int index, int distance)
		{
			return (index + 2 + distance) % 2;
		}

		public Tile OppositeTile(Tile neighbor) { return Tiles[RotateNeighborIndex(NeighborIndexOf(neighbor), +1)]; }
		public Corner OppositeCorner(Corner neighbor) { return Corners[RotateNeighborIndex(NeighborIndexOf(neighbor), +1)]; }

		public Corner PrevCorner(Tile neighbor) { return Corners[RotateNeighborIndex(NeighborIndexOf(neighbor), -1)]; }
		public Corner NextCorner(Tile neighbor) { return Corners[NeighborIndexOf(neighbor)]; }
		public Tile PrevTile(Corner neighbor) { return Tiles[NeighborIndexOf(neighbor)]; }
		public Tile NextTile(Corner neighbor) { return Tiles[RotateNeighborIndex(NeighborIndexOf(neighbor), +1)]; }

		public override bool Equals(object other) { return other is Edge && _index == ((Edge)other)._index; }
		public bool Equals(Edge other) { return _index == other._index; }
		public static bool operator ==(Edge lhs, Edge rhs) { return lhs._index == rhs._index; }
		public static bool operator !=(Edge lhs, Edge rhs) { return lhs._index != rhs._index; }
		public override int GetHashCode() { return _index.GetHashCode(); }
	}

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
			get {  return _values[edge.Index]; }
			set { _values[edge.Index] = value; }
		}

		public int Count
		{
			get { return _values.Length; }
		}
	}
}