using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Tiling
{
	public struct Tile : IEquatable<Tile>
	{
		private Topology _topology;
		private int _index;

		private int _neighborOffset;
		private int _neighborCount;

		public Tile(Topology topology, int index)
		{
			_topology = topology;
			_index = index;
			_neighborOffset = _topology._tileNeighborOffsets[_index];
			_neighborCount = _topology._tileNeighborOffsets[_index + 1] - _neighborOffset;
		}

		public int Index { get { return _index; } }

		public int NeighborCount { get { return _neighborCount; } }

		public struct TilesIndexer : IEnumerable<Tile>
		{
			private Tile _tile;
			public TilesIndexer(Tile tile) { _tile = tile; }
			public Tile this[int i] { get { return new Tile(_tile._topology, _tile._topology._tileTiles[_tile._neighborOffset + i]); } }
			public int Count { get { return _tile._neighborCount; } }
			public IEnumerator<Tile> GetEnumerator() { for (int i = 0; i < _tile._neighborCount; ++i) yield return this[i]; }
			IEnumerator IEnumerable.GetEnumerator() { return (this as IEnumerable<Tile>).GetEnumerator(); }
			public Tile First { get { return this[0]; } }
			public Tile Last { get { return this[Count - 1]; } }
		}

		public TilesIndexer Tiles { get { return new TilesIndexer(this); } }

		public struct EdgesIndexer : IEnumerable<Edge>
		{
			private Tile _tile;
			public EdgesIndexer(Tile tile) { _tile = tile; }
			public Edge this[int i] { get { return new Edge(_tile._topology, _tile._topology._tileEdges[_tile._neighborOffset + i]); } }
			public int Count { get { return _tile._neighborCount; } }
			public IEnumerator<Edge> GetEnumerator() { for (int i = 0; i < _tile._neighborCount; ++i) yield return this[i]; }
			IEnumerator IEnumerable.GetEnumerator() { return (this as IEnumerable<Edge>).GetEnumerator(); }
			public Edge First { get { return this[0]; } }
			public Edge Last { get { return this[Count - 1]; } }
		}

		public EdgesIndexer Edges { get { return new EdgesIndexer(this); } }

		public struct CornersIndexer : IEnumerable<Corner>
		{
			private Tile _tile;
			public CornersIndexer(Tile tile) { _tile = tile; }
			public Corner this[int i] { get { return new Corner(_tile._topology, _tile._topology._tileCorners[_tile._neighborOffset + i]); } }
			public int Count { get { return _tile._neighborCount; } }
			public IEnumerator<Corner> GetEnumerator() { for (int i = 0; i < _tile._neighborCount; ++i) yield return this[i]; }
			IEnumerator IEnumerable.GetEnumerator() { return (this as IEnumerable<Corner>).GetEnumerator(); }
			public Corner First { get { return this[0]; } }
			public Corner Last { get { return this[Count - 1]; } }
		}

		public CornersIndexer Corners { get { return new CornersIndexer(this); } }

		public int NeighborIndexOf(Tile tile)
		{
			for (int i = _neighborOffset; i < _neighborOffset + _neighborCount; ++i)
			{
				if (_topology._tileTiles[i] == tile.Index) return i - _neighborOffset;
			}
			throw new ApplicationException("The specified tile is not a neighbor of this tile.");
		}

		public int NeighborIndexOf(Edge edge)
		{
			for (int i = _neighborOffset; i < _neighborOffset + _neighborCount; ++i)
			{
				if (_topology._tileEdges[i] == edge.Index) return i - _neighborOffset;
			}
			throw new ApplicationException("The specified edge is not a neighbor of this tile.");
		}

		public int NeighborIndexOf(Corner corner)
		{
			for (int i = _neighborOffset; i < _neighborOffset + _neighborCount; ++i)
			{
				if (_topology._tileCorners[i] == corner.Index) return i - _neighborOffset;
			}
			throw new ApplicationException("The specified corner is not a neighbor of this tile.");
		}

		public int RotateNeighborIndex(int index, int distance)
		{
			return (index + _neighborCount + distance) % _neighborCount;
		}

		public Tile PrevTile(Tile neighbor) { return Tiles[RotateNeighborIndex(NeighborIndexOf(neighbor), -1)]; }
		public Edge PrevEdge(Edge neighbor) { return Edges[RotateNeighborIndex(NeighborIndexOf(neighbor), -1)]; }
		public Corner PrevCorner(Corner neighbor) { return Corners[RotateNeighborIndex(NeighborIndexOf(neighbor), -1)]; }

		public Tile NextTile(Tile neighbor) { return Tiles[RotateNeighborIndex(NeighborIndexOf(neighbor), +1)]; }
		public Edge NextEdge(Edge neighbor) { return Edges[RotateNeighborIndex(NeighborIndexOf(neighbor), +1)]; }
		public Corner NextCorner(Corner neighbor) { return Corners[RotateNeighborIndex(NeighborIndexOf(neighbor), +1)]; }

		public Tile AssociatedTile(Edge neighbor) { return Tiles[NeighborIndexOf(neighbor)]; }
		public Tile PrevTile(Corner neighbor) { return Tiles[NeighborIndexOf(neighbor)]; }
		public Tile NextTile(Corner neighbor) { return Tiles[RotateNeighborIndex(NeighborIndexOf(neighbor), +1)]; }

		public Edge AssociatedEdge(Tile neighbor) { return Edges[NeighborIndexOf(neighbor)]; }
		public Edge PrevEdge(Corner neighbor) { return Edges[NeighborIndexOf(neighbor)]; }
		public Edge NextEdge(Corner neighbor) { return Edges[RotateNeighborIndex(NeighborIndexOf(neighbor), +1)]; }

		public Corner PrevCorner(Tile neighbor) { return Corners[RotateNeighborIndex(NeighborIndexOf(neighbor), -1)]; }
		public Corner NextCorner(Tile neighbor) { return Corners[NeighborIndexOf(neighbor)]; }
		public Corner PrevCorner(Edge neighbor) { return Corners[RotateNeighborIndex(NeighborIndexOf(neighbor), -1)]; }
		public Corner NextCorner(Edge neighbor) { return Corners[NeighborIndexOf(neighbor)]; }

		public override bool Equals(object other) { return other is Tile && _index == ((Tile)other)._index; }
		public bool Equals(Tile other) { return _index == other._index; }
		public static bool operator ==(Tile lhs, Tile rhs) { return lhs._index == rhs._index; }
		public static bool operator !=(Tile lhs, Tile rhs) { return lhs._index != rhs._index; }
		public override int GetHashCode() { return _index.GetHashCode(); }
	}

	public struct TileAttribute<T> where T : new()
	{
		public T[] _values;

		public TileAttribute(int tileCount)
		{
			_values = new T[tileCount];
		}

		public T this[int i]
		{
			get {  return _values[i]; }
			set {  _values[i] = value; }
		}

		public T this[Tile tile]
		{
			get {  return _values[tile.Index]; }
			set { _values[tile.Index] = value; }
		}

		public int Count
		{
			get { return _values.Length; }
		}
	}
}