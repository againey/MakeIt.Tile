using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Tiling
{
	public struct Corner : IEquatable<Corner>
	{
		private Topology _topology;
		private int _index;

		public Corner(Topology topology, int index)
		{
			_topology = topology;
			_index = index;
		}

		public int Index { get { return _index; } }

		public int NeighborCount { get { return 3; } }

		public struct TilesIndexer : IEnumerable<Tile>
		{
			private Topology _topology;
			private int _index;

			public TilesIndexer(Topology topology, int index)
			{
				_topology = topology;
				_index = index;
			}

			public Tile this[int i] { get { return new Tile(_topology, _topology._cornerTiles[_index, i]); } }
			public int Count { get { return 3; } }
			public IEnumerator<Tile> GetEnumerator() { for (int i = 0; i < 3; ++i) yield return this[i]; }
			IEnumerator IEnumerable.GetEnumerator() { return (this as IEnumerable<Tile>).GetEnumerator(); }
			public Tile First { get { return this[0]; } }
			public Tile Last { get { return this[Count - 1]; } }
		}

		public TilesIndexer Tiles { get { return new TilesIndexer(_topology, _index); } }

		public struct EdgesIndexer : IEnumerable<Edge>
		{
			private Topology _topology;
			private int _index;

			public EdgesIndexer(Topology topology, int index)
			{
				_topology = topology;
				_index = index;
			}

			public Edge this[int i] { get { return new Edge(_topology, _topology._cornerEdges[_index, i]); } }
			public int Count { get { return 3; } }
			public IEnumerator<Edge> GetEnumerator() { for (int i = 0; i < 3; ++i) yield return this[i]; }
			IEnumerator IEnumerable.GetEnumerator() { return (this as IEnumerable<Edge>).GetEnumerator(); }
			public Edge First { get { return this[0]; } }
			public Edge Last { get { return this[Count - 1]; } }
		}

		public EdgesIndexer Edges { get { return new EdgesIndexer(_topology, _index); } }

		public struct CornersIndexer : IEnumerable<Corner>
		{
			private Topology _topology;
			private int _index;

			public CornersIndexer(Topology topology, int index)
			{
				_topology = topology;
				_index = index;
			}

			public Corner this[int i] { get { return new Corner(_topology, _topology._cornerCorners[_index, i]); } }
			public int Count { get { return 3; } }
			public IEnumerator<Corner> GetEnumerator() { for (int i = 0; i < 3; ++i) yield return this[i]; }
			IEnumerator IEnumerable.GetEnumerator() { return (this as IEnumerable<Corner>).GetEnumerator(); }
			public Corner First { get { return this[0]; } }
			public Corner Last { get { return this[Count - 1]; } }
		}

		public CornersIndexer Corners { get { return new CornersIndexer(_topology, _index); } }

		public int NeighborIndexOf(Tile tile)
		{
			if (_topology._cornerTiles[_index, 0] == tile.Index) return 0;
			else if (_topology._cornerTiles[_index, 1] == tile.Index) return 1;
			else if (_topology._cornerTiles[_index, 2] == tile.Index) return 2;
			throw new ApplicationException("The specified tile is not a neighbor of this corner.");
		}

		public int NeighborIndexOf(Edge edge)
		{
			if (_topology._cornerEdges[_index, 0] == edge.Index) return 0;
			else if (_topology._cornerEdges[_index, 1] == edge.Index) return 1;
			else if (_topology._cornerEdges[_index, 2] == edge.Index) return 2;
			throw new ApplicationException("The specified edge is not a neighbor of this corner.");
		}

		public int NeighborIndexOf(Corner corner)
		{
			if (_topology._cornerCorners[_index, 0] == corner.Index) return 0;
			else if (_topology._cornerCorners[_index, 1] == corner.Index) return 1;
			else if (_topology._cornerCorners[_index, 2] == corner.Index) return 2;
			throw new ApplicationException("The specified corner is not a neighbor of this corner.");
		}

		public int RotateNeighborIndex(int index, int distance)
		{
			return (index + 3 + distance) % 3;
		}

		public Tile PrevTile(Tile neighbor) { return Tiles[RotateNeighborIndex(NeighborIndexOf(neighbor), -1)]; }
		public Edge PrevEdge(Edge neighbor) { return Edges[RotateNeighborIndex(NeighborIndexOf(neighbor), -1)]; }
		public Corner PrevCorner(Corner neighbor) { return Corners[RotateNeighborIndex(NeighborIndexOf(neighbor), -1)]; }

		public Tile NextTile(Tile neighbor) { return Tiles[RotateNeighborIndex(NeighborIndexOf(neighbor), +1)]; }
		public Edge NextEdge(Edge neighbor) { return Edges[RotateNeighborIndex(NeighborIndexOf(neighbor), 1)]; }
		public Corner NextCorner(Corner neighbor) { return Corners[RotateNeighborIndex(NeighborIndexOf(neighbor), +1)]; }

		public Tile PrevTile(Edge neighbor) { return Tiles[NeighborIndexOf(neighbor)]; }
		public Tile NextTile(Edge neighbor) { return Tiles[RotateNeighborIndex(NeighborIndexOf(neighbor), +1)]; }
		public Tile OppositeTile(Edge neighbor) { return Tiles[RotateNeighborIndex(NeighborIndexOf(neighbor), -1)]; }
		public Tile PrevTile(Corner neighbor) { return Tiles[NeighborIndexOf(neighbor)]; }
		public Tile NextTile(Corner neighbor) { return Tiles[RotateNeighborIndex(NeighborIndexOf(neighbor), +1)]; }

		public Edge PrevEdge(Tile neighbor) { return Edges[RotateNeighborIndex(NeighborIndexOf(neighbor), -1)]; }
		public Edge NextEdge(Tile neighbor) { return Edges[NeighborIndexOf(neighbor)]; }
		public Edge OppositeEdge(Tile neighbor) { return Edges[RotateNeighborIndex(NeighborIndexOf(neighbor), +1)]; }
		public Edge AssociatedEdge(Corner neighbor) { return Edges[NeighborIndexOf(neighbor)]; }

		public Corner PrevCorner(Tile neighbor) { return Corners[RotateNeighborIndex(NeighborIndexOf(neighbor), -1)]; }
		public Corner NextCorner(Tile neighbor) { return Corners[NeighborIndexOf(neighbor)]; }
		public Corner OppositeCorner(Tile neighbor) { return Corners[RotateNeighborIndex(NeighborIndexOf(neighbor), +1)]; }
		public Corner AssociatedCorner(Edge neighbor) { return Corners[NeighborIndexOf(neighbor)]; }

		public override bool Equals(object other) { return other is Corner && _index == ((Corner)other)._index; }
		public bool Equals(Corner other) { return _index == other._index; }
		public static bool operator ==(Corner lhs, Corner rhs) { return lhs._index == rhs._index; }
		public static bool operator !=(Corner lhs, Corner rhs) { return lhs._index != rhs._index; }
		public override int GetHashCode() { return _index.GetHashCode(); }
	}

	public struct CornerAttribute<T> where T : new()
	{
		public T[] _values;

		public CornerAttribute(int cornerCount)
		{
			_values = new T[cornerCount];
		}

		public T this[int i]
		{
			get {  return _values[i]; }
			set {  _values[i] = value; }
		}

		public T this[Corner corner]
		{
			get {  return _values[corner.Index]; }
			set { _values[corner.Index] = value; }
		}

		public int Count
		{
			get { return _values.Length; }
		}
	}
}