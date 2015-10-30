using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

namespace Tiling
{
	public class MinimalTopology
	{
		public int[,] _cornerTiles; // [CornerCount, 3]
		public int[,] _edgeTiles; // [EdgeCount, 2]
	}

	public class Topology : MinimalTopology
	{
		public int[,] _cornerCorners; // [CornerCount, 3]
		public int[,] _cornerEdges; // [CornerCount, 3]
		public int[,] _edgeCorners; // [EdgeCount, 2]
		public int[] _tileNeighborOffsets; // [TileCount + 1]
		public int[] _tileCorners; // [TileCount * AverageNeighborCount]
		public int[] _tileEdges; // [TileCount * AverageNeighborCount]
		public int[] _tileTiles; // [TileCount * AverageNeighborCount]

		public struct TilesIndexer : IEnumerable<Tile>
		{
			private Topology _topology;
			public TilesIndexer(Topology topology) { _topology = topology; }
			public Tile this[int i] { get { return new Tile(_topology, i); } }
			public int Count { get { return _topology._tileNeighborOffsets.Length - 1; } }
			public IEnumerator<Tile> GetEnumerator() { for (int i = 0; i < _topology._tileNeighborOffsets.Length - 1; ++i) yield return new Tile(_topology, i); }
			IEnumerator IEnumerable.GetEnumerator() { return (this as IEnumerable<Tile>).GetEnumerator(); }
		}

		public TilesIndexer Tiles { get { return new TilesIndexer(this); } }

		public struct EdgesIndexer : IEnumerable<Edge>
		{
			private Topology _topology;
			public EdgesIndexer(Topology topology) { _topology = topology; }
			public Edge this[int i] { get { return new Edge(_topology, i); } }
			public int Count { get { return _topology._edgeTiles.GetLength(0); } }
			public IEnumerator<Edge> GetEnumerator() { for (int i = 0; i < _topology._edgeTiles.GetLength(0); ++i) yield return new Edge(_topology, i); }
			IEnumerator IEnumerable.GetEnumerator() { return (this as IEnumerable<Edge>).GetEnumerator(); }
		}

		public EdgesIndexer Edges { get { return new EdgesIndexer(this); } }

		public struct CornersIndexer : IEnumerable<Corner>
		{
			private Topology _topology;
			public CornersIndexer(Topology topology) { _topology = topology; }
			public Corner this[int i] { get { return new Corner(_topology, i); } }
			public int Count { get { return _topology._cornerTiles.GetLength(0); } }
			public IEnumerator<Corner> GetEnumerator() { for (int i = 0; i < _topology._cornerTiles.GetLength(0); ++i) yield return new Corner(_topology, i); }
			IEnumerator IEnumerable.GetEnumerator() { return (this as IEnumerable<Corner>).GetEnumerator(); }
		}

		public CornersIndexer Corners { get { return new CornersIndexer(this); } }

		public Topology()
		{
		}

		public Topology(MinimalTopology basicTopology)
		{
			_cornerTiles = new int[basicTopology._cornerTiles.GetLength(0), 3];
			System.Array.Copy(basicTopology._cornerTiles, 0, _cornerTiles, 0, basicTopology._cornerTiles.Length);
			_edgeTiles = new int[basicTopology._edgeTiles.GetLength(0), 2];
			System.Array.Copy(basicTopology._edgeTiles, 0, _edgeTiles, 0, basicTopology._edgeTiles.Length);

			// Determine the total number of tiles by finding the maximum tile index specified and adding 1.
			int tileCount = 0;
			for (int i = 0; i < _edgeTiles.GetLength(0); ++i)
			{
				tileCount = Mathf.Max(tileCount, Mathf.Max(_edgeTiles[i, 0], _edgeTiles[i, 1]));
			}
			++tileCount;

			// Determine the number of neighbors each tile has by examining all the edges and incrementing
			// the tile neighbor count for both neighboring tiles of every edge.
			var tileNeighborCounts = new byte[tileCount];
			for (int i = 0; i < _edgeTiles.GetLength(0); ++i)
			{
				++tileNeighborCounts[_edgeTiles[i, 0]];
				++tileNeighborCounts[_edgeTiles[i, 1]];
			}

			// Determine the offsets in the tile neighbor arrays by tracking a running sum of neighbor counts.
			_tileNeighborOffsets = new int[tileCount + 1];
			int totalNeighborCount = 0;
			for (int i = 0; i < tileNeighborCounts.Length; ++i)
			{
				_tileNeighborOffsets[i] = totalNeighborCount;
				totalNeighborCount += tileNeighborCounts[i];
			}
			_tileNeighborOffsets[_tileNeighborOffsets.Length - 1] = totalNeighborCount;

			// Reset the tile neighbor counts, and then add the neighboring edges to the tiles one by one.
			System.Array.Clear(tileNeighborCounts, 0, tileNeighborCounts.Length);
			_tileEdges = new int[totalNeighborCount];
			for (int i = 0; i < _edgeTiles.GetLength(0); ++i)
			{
				_tileEdges[_tileNeighborOffsets[_edgeTiles[i, 0]] + tileNeighborCounts[_edgeTiles[i, 0]]++] = i;
				_tileEdges[_tileNeighborOffsets[_edgeTiles[i, 1]] + tileNeighborCounts[_edgeTiles[i, 1]]++] = i;
			}

			// Reset the tile neighbor counts, and then add the neighboring tiles to the tiles one by one.
			System.Array.Clear(tileNeighborCounts, 0, tileNeighborCounts.Length);
			_tileCorners = new int[totalNeighborCount];
			for (int i = 0; i < _cornerTiles.GetLength(0); ++i)
			{
				_tileCorners[_tileNeighborOffsets[_cornerTiles[i, 0]] + tileNeighborCounts[_cornerTiles[i, 0]]++] = i;
				_tileCorners[_tileNeighborOffsets[_cornerTiles[i, 1]] + tileNeighborCounts[_cornerTiles[i, 1]]++] = i;
				_tileCorners[_tileNeighborOffsets[_cornerTiles[i, 2]] + tileNeighborCounts[_cornerTiles[i, 2]]++] = i;
			}

			// Reorder tile neighbors based on the order of tiles to their neighbor tiles.
			_tileTiles = new int[totalNeighborCount];
			for (int i = 0; i < tileNeighborCounts.Length; ++i)
			{
				var nextCorner = _tileCorners[_tileNeighborOffsets[i]];
				var cornerTile = _cornerTiles[nextCorner, 0] == i ? 0 : _cornerTiles[nextCorner, 1] == i ? 1 : 2;
				var priorTile = _cornerTiles[nextCorner, (cornerTile + 1) % 3];
				var nextTile = _cornerTiles[nextCorner, (cornerTile + 2) % 3];

				// Reorder the tile's first neighbor edge to match the first neighbor tile.
				for (int k = 0; k < tileNeighborCounts[i]; ++k)
				{
					var tileEdge = _tileEdges[_tileNeighborOffsets[i] + k];
					if (_edgeTiles[tileEdge, 0] == priorTile || _edgeTiles[tileEdge, 1] == priorTile)
					{
						if (k > 0)
						{
							var swapIntermediate = _tileEdges[_tileNeighborOffsets[i]];
							_tileEdges[_tileNeighborOffsets[i]] = _tileEdges[_tileNeighborOffsets[i] + k];
							_tileEdges[_tileNeighborOffsets[i] + k] = swapIntermediate;
						}
						break;
					}
				}

				// Set the first neighbor tile to match the first neighbor edge and tile.
				_tileTiles[_tileNeighborOffsets[i]] = priorTile;

				// Find each following tile, given the preceding tile, and then line up the edges and neighbor tiles to match.
				// The very last neighbor doesn't need to be reordered, since there will be nothing left to swap with.
				for (int j = 1; j < tileNeighborCounts[i] - 1; ++j)
				{
					// Check each following tile to see if it is the correct tile to come next in the ordering.
					for (int k = j; k < tileNeighborCounts[i]; ++k)
					{
						nextCorner = _tileCorners[_tileNeighborOffsets[i] + k];
						cornerTile = _cornerTiles[nextCorner, 0] == i ? 0 : _cornerTiles[nextCorner, 1] == i ? 1 : 2;
						if (_cornerTiles[nextCorner, (cornerTile + 1) % 3] == nextTile)
						{
							priorTile = nextTile;
							nextTile = _cornerTiles[nextCorner, (cornerTile + 2) % 3];
							if (k > j)
							{
								var swapIntermediate = _tileCorners[_tileNeighborOffsets[i] + j];
								_tileCorners[_tileNeighborOffsets[i] + j] = _tileCorners[_tileNeighborOffsets[i] + k];
								_tileCorners[_tileNeighborOffsets[i] + k] = swapIntermediate;
							}
							break;
						}
					}

					// Reorder the tile's j-th neighbor edge to match the j-th neighbor tile.
					for (int k = j; k < tileNeighborCounts[i]; ++k)
					{
						var tileEdge = _tileEdges[_tileNeighborOffsets[i] + k];
						if (_edgeTiles[tileEdge, 0] == priorTile || _edgeTiles[tileEdge, 1] == priorTile)
						{
							if (k > j)
							{
								var swapIntermediate = _tileEdges[_tileNeighborOffsets[i] + j];
								_tileEdges[_tileNeighborOffsets[i] + j] = _tileEdges[_tileNeighborOffsets[i] + k];
								_tileEdges[_tileNeighborOffsets[i] + k] = swapIntermediate;
							}
							break;
						}
					}

					// Set the j-th neighbor tile to match the j-th neighbor edge and tile.
					_tileTiles[_tileNeighborOffsets[i] + j] = priorTile;
				}

				// Set the last neighbor tile to match the last neighbor edge and tile.
				_tileTiles[_tileNeighborOffsets[i + 1] - 1] = nextTile;
			}

			// Build edge neighbors.
			_edgeCorners = new int[_edgeTiles.GetLength(0), 2];

			for (int i = 0; i < _edgeTiles.GetLength(0); ++i)
			{
				var edgeTile = _edgeTiles[i, 0];
				var tileEdge = 0;
				for (; tileEdge < tileNeighborCounts[edgeTile]; ++tileEdge)
				{
					if (_tileEdges[_tileNeighborOffsets[edgeTile] + tileEdge] == i)
					{
						break;
					}
				}

				_edgeCorners[i, 0] = _tileCorners[_tileNeighborOffsets[edgeTile] + tileEdge];
				_edgeCorners[i, 1] = _tileCorners[_tileNeighborOffsets[edgeTile] + (tileEdge + tileNeighborCounts[edgeTile] - 1) % tileNeighborCounts[edgeTile]];
			}

			// Build tile neighbors.
			_cornerEdges = new int[_cornerTiles.GetLength(0), 3];
			_cornerCorners = new int[_cornerTiles.GetLength(0), 3];

			for (int i = 0; i < _cornerTiles.GetLength(0); ++i)
			{
				for (int j = 0; j < 3; ++j)
				{
					var cornerTile = _cornerTiles[i, j];
					var tileCorner = 0;
					for (; tileCorner < tileNeighborCounts[cornerTile]; ++tileCorner)
					{
						if (_tileCorners[_tileNeighborOffsets[cornerTile] + tileCorner] == i)
						{
							break;
						}
					}
					var cornerEdge = _tileEdges[_tileNeighborOffsets[cornerTile] + tileCorner];
					_cornerEdges[i, j] = cornerEdge;
					_cornerCorners[i, j] = _tileCorners[_tileNeighborOffsets[cornerTile] + (tileCorner + tileNeighborCounts[cornerTile] - 1) % tileNeighborCounts[cornerTile]];
				}
			}
		}

		public MinimalTopology Subdivide(int degree, System.Action<int> tileAllocator, System.Action<int, int> tileCopier, System.Action<int, int, int, float> tileInterpolator)
		{
			var subdivided = new MinimalTopology();

			if (degree == 0)
			{
				subdivided._cornerTiles = new int[_cornerTiles.GetLength(0), 3];
				System.Array.Copy(_cornerTiles, 0, subdivided._cornerTiles, 0, _cornerTiles.Length);
				subdivided._edgeTiles = new int[_edgeTiles.GetLength(0), 2];
				System.Array.Copy(_edgeTiles, 0, subdivided._edgeTiles, 0, _edgeTiles.Length);
				tileAllocator(_tileNeighborOffsets.Length - 1);
				for (int i = 0; i < _tileNeighborOffsets.Length - 1; ++i)
				{
					tileCopier(i, i);
				}
				return subdivided;
			}

			var tileCount = _tileNeighborOffsets.Length - 1;
			var edgeCount = _edgeTiles.GetLength(0);
			var cornerCount = _cornerTiles.GetLength(0);

			var innerTilesPerEdge = degree + 0;
			var innerEdgesPerEdge = degree + 1;
			var tilesPerCorner = (degree + 2) * (degree + 3) / 2;
			var innerTilesPerCorner = (degree - 1) * degree / 2;
			var innerEdgesPerCorner = degree * (degree + 1) * 3 / 2;
			var innerCornersPerCorner = (degree + 1) * (degree + 1);

			int totalSubdividedTileCount = innerTilesPerCorner * cornerCount + innerTilesPerEdge * edgeCount + tileCount;
			int totalSubdividedEdgeCount = innerEdgesPerCorner * cornerCount + innerEdgesPerEdge * edgeCount;
			int totalSubdividedCornerCount = innerCornersPerCorner * cornerCount;

			tileAllocator(totalSubdividedTileCount);
			subdivided._edgeTiles = new int[totalSubdividedEdgeCount, 2];
			subdivided._cornerTiles = new int[totalSubdividedCornerCount, 3];

			int subdividedTileCount = 0;
			int subdividedEdgeCount = 0;
			int subdividedCornerCount = 0;

			for (int i = 0; i < _tileNeighborOffsets.Length - 1; ++i)
			{
				tileCopier(i, i);
			}
			subdividedTileCount = _tileNeighborOffsets.Length - 1;

			System.Action<int, int, int> SubdivideLine = delegate (int i0, int i1, int count)
			{
				var dt = 1.0f / (float)(count + 1);
				var t = dt;
				var tEnd = 1f - dt * 0.5f;
				while (t < tEnd)
				{
					tileInterpolator(i0, i1, subdividedTileCount++, t);
					t += dt;
				}
			};

			var firstSubdividedEdgeTile = subdividedTileCount;
			for (int i = 0; i < edgeCount; ++i)
			{
				int firstEdgeTile = subdividedTileCount;

				SubdivideLine(_edgeTiles[i, 0], _edgeTiles[i, 1], innerTilesPerEdge);

				subdivided._edgeTiles[subdividedEdgeCount, 0] = _edgeTiles[i, 0];
				subdivided._edgeTiles[subdividedEdgeCount++, 1] = firstEdgeTile;
				for (int j = 1; j < degree; ++j)
				{
					subdivided._edgeTiles[subdividedEdgeCount, 0] = firstEdgeTile + j - 1;
					subdivided._edgeTiles[subdividedEdgeCount++, 1] = firstEdgeTile + j;
				}
				subdivided._edgeTiles[subdividedEdgeCount, 0] = firstEdgeTile + degree - 1;
				subdivided._edgeTiles[subdividedEdgeCount++, 1] = _edgeTiles[i, 1];
			}

			var cornerTileLookup = new int[tilesPerCorner];

			for (int i = 0; i < cornerCount; ++i)
			{
				var edge0 = _cornerEdges[i, 0];
				var edge1 = _cornerEdges[i, 1];
				var edge2 = _cornerEdges[i, 2];
				var firstEdgeTile0 = firstSubdividedEdgeTile + edge0 * innerTilesPerEdge;
				var firstEdgeTile1 = firstSubdividedEdgeTile + edge1 * innerTilesPerEdge;
				var firstEdgeTile2 = firstSubdividedEdgeTile + edge2 * innerTilesPerEdge;
				var tileDelta0 = _edgeTiles[edge0, 0] == _cornerTiles[i, 1] ? 1 : -1;
				var tileDelta1 = _edgeTiles[edge1, 0] == _cornerTiles[i, 1] ? 1 : -1;
				var tileDelta2 = _edgeTiles[edge2, 0] == _cornerTiles[i, 0] ? 1 : -1;
				if (tileDelta0 == -1) firstEdgeTile0 += innerTilesPerEdge - 1;
				if (tileDelta1 == -1) firstEdgeTile1 += innerTilesPerEdge - 1;
				if (tileDelta2 == -1) firstEdgeTile2 += innerTilesPerEdge - 1;

				cornerTileLookup[0] = _cornerTiles[i, 1];
				cornerTileLookup[1] = firstEdgeTile0;
				cornerTileLookup[2] = firstEdgeTile1;
				var cornerTileLookupCount = 3;

				var edgeTile0 = firstEdgeTile0 + tileDelta0;
				var edgeTile1 = firstEdgeTile1 + tileDelta1;

				for (int j = 1; j < innerTilesPerEdge; ++j)
				{
					var firstRowTile = subdividedTileCount;
					SubdivideLine(edgeTile0, edgeTile1, j);

					cornerTileLookup[cornerTileLookupCount++] = edgeTile0;
					for (int k = 0; k < j; ++k)
					{
						cornerTileLookup[cornerTileLookupCount++] = firstRowTile + k;
					}
					cornerTileLookup[cornerTileLookupCount++] = edgeTile1;

					edgeTile0 += tileDelta0;
					edgeTile1 += tileDelta1;
				}

				cornerTileLookup[cornerTileLookupCount++] = _cornerTiles[i, 0];
				var edgeTile2 = firstEdgeTile2;
				for (int k = 0; k < innerTilesPerEdge; ++k)
				{
					cornerTileLookup[cornerTileLookupCount++] = edgeTile2++;
				}
				cornerTileLookup[cornerTileLookupCount++] = _cornerTiles[i, 2];

				subdivided._cornerTiles[subdividedCornerCount, 0] = cornerTileLookup[0];
				subdivided._cornerTiles[subdividedCornerCount, 1] = cornerTileLookup[1];
				subdivided._cornerTiles[subdividedCornerCount++, 2] = cornerTileLookup[2];

				var cornerTile = 1;
				for (int j = 1; j <= innerTilesPerEdge; ++j)
				{
					for (int k = 0; k < j; ++k)
					{
						subdivided._edgeTiles[subdividedEdgeCount, 0] = cornerTileLookup[cornerTile];
						subdivided._edgeTiles[subdividedEdgeCount++, 1] = cornerTileLookup[cornerTile + 1];

						subdivided._edgeTiles[subdividedEdgeCount, 0] = cornerTileLookup[cornerTile];
						subdivided._edgeTiles[subdividedEdgeCount++, 1] = cornerTileLookup[cornerTile + j + 2];

						subdivided._edgeTiles[subdividedEdgeCount, 0] = cornerTileLookup[cornerTile + 1];
						subdivided._edgeTiles[subdividedEdgeCount++, 1] = cornerTileLookup[cornerTile + j + 2];

						subdivided._cornerTiles[subdividedCornerCount, 0] = cornerTileLookup[cornerTile];
						subdivided._cornerTiles[subdividedCornerCount, 1] = cornerTileLookup[cornerTile + j + 1];
						subdivided._cornerTiles[subdividedCornerCount++, 2] = cornerTileLookup[cornerTile + j + 2];

						subdivided._cornerTiles[subdividedCornerCount, 0] = cornerTileLookup[cornerTile];
						subdivided._cornerTiles[subdividedCornerCount, 1] = cornerTileLookup[cornerTile + j + 2];
						subdivided._cornerTiles[subdividedCornerCount++, 2] = cornerTileLookup[cornerTile + 1];

						++cornerTile;
					}

					subdivided._cornerTiles[subdividedCornerCount, 0] = cornerTileLookup[cornerTile];
					subdivided._cornerTiles[subdividedCornerCount, 1] = cornerTileLookup[cornerTile + j + 1];
					subdivided._cornerTiles[subdividedCornerCount++, 2] = cornerTileLookup[cornerTile + j + 2];

					++cornerTile;
				}
			}

			return subdivided;
		}

		private struct TileAlteration
		{
			public int _neighbor;
			public int _tile;
			public int _edge;
			public int _corner;

			public TileAlteration(int neighbor)
			{
				_neighbor = neighbor;
				_tile = -1;
				_edge = -1;
				_corner = -1;
			}

			public TileAlteration(int neighbor, int tile, int edge, int corner)
			{
				_neighbor = neighbor;
				_tile = tile;
				_edge = edge;
				_corner = corner;
			}
		}

		private int GetTileTileNeighborIndex(int tile, int neighborTile)
		{
			for (int i = _tileNeighborOffsets[tile]; i < _tileNeighborOffsets[tile + 1]; ++i)
			{
				if (_tileTiles[i] == neighborTile) return i - _tileNeighborOffsets[tile];
			}
			throw new ApplicationException("The provided neighbor tile was not actually a neighbor of the specified tile.");
		}

		private int GetTileEdgeNeighborIndex(int tile, int edge)
		{
			for (int i = _tileNeighborOffsets[tile]; i < _tileNeighborOffsets[tile + 1]; ++i)
			{
				if (_tileEdges[i] == edge) return i - _tileNeighborOffsets[tile];
			}
			throw new ApplicationException("The provided edge was not a neighbor of the specified tile.");
		}

		private int GetEdgeCornerNeighborIndex(int edge, int corner)
		{
			if (_edgeCorners[edge, 0] == corner) return 0;
			else if (_edgeCorners[edge, 1] == corner) return 1;
			else throw new ApplicationException("The provided corner was not a neighbor of the specified edge.");
		}

		private int GetCornerEdgeNeighborIndex(int corner, int edge)
		{
			if (_cornerEdges[corner, 0] == edge) return 0;
			else if (_cornerEdges[corner, 1] == edge) return 1;
			else if (_cornerEdges[corner, 2] == edge) return 2;
			else throw new ApplicationException("The provided edge was not a neighbor of the specified corner.");
		}

		private int GetCornerCornerNeighborIndex(int corner, int neighborCorner)
		{
			if (_cornerCorners[corner, 0] == neighborCorner) return 0;
			else if (_cornerCorners[corner, 1] == neighborCorner) return 1;
			else if (_cornerCorners[corner, 2] == neighborCorner) return 2;
			else throw new ApplicationException("The provided corner was not a neighbor of the specified corner.");
		}

		private int RotateTileNeighborIndex(int tile, int neighborIndex, int distance)
		{
			var neighborCount = _tileNeighborOffsets[tile + 1] - _tileNeighborOffsets[tile];
			return (neighborIndex + neighborCount + distance) % neighborCount;
		}

		private int RotateCornerNeighborIndex(int neighborIndex, int distance)
		{
			return (neighborIndex + 3 + distance) % 3;
		}

		private void InsertTileAlteration(int tile, TileAlteration alteration, List<int> tileAlterationIndexQueue, List<TileAlteration> tileAlterationQueue)
		{
			var insertionIndex = tileAlterationIndexQueue.BinarySearch(tile);
			if (insertionIndex >= 0) throw new ApplicationException("The specified tile already has an alteration queued, and cannot be altered a second time in a single pass.");
			insertionIndex = ~insertionIndex;

			tileAlterationIndexQueue.Insert(insertionIndex, tile);
			tileAlterationQueue.Insert(insertionIndex, alteration);
		}

		private void ApplyTileAlteration(List<int> tileAlterationIndexQueue, List<TileAlteration> tileAlterationQueue, int[] newTileNeighborOffsets, int[] newTileTiles, int[] newTileEdges, int[] newTileCorners, ref int newOffset, ref int tileAlterationQueuePosition)
		{
			var tile = tileAlterationIndexQueue[tileAlterationQueuePosition];
			var firstOffset = _tileNeighborOffsets[tile];
			var neighborCount = _tileNeighborOffsets[tile + 1] - firstOffset;
			newTileNeighborOffsets[tile] = newOffset;

			var neighborIndex = tileAlterationQueue[tileAlterationQueuePosition]._neighbor;
			if (tileAlterationQueue[tileAlterationQueuePosition]._tile >= 0)
			{
				if (neighborIndex > 0)
				{
					System.Array.Copy(_tileTiles, firstOffset, newTileTiles, newOffset, neighborIndex);
					System.Array.Copy(_tileEdges, firstOffset, newTileEdges, newOffset, neighborIndex);
					System.Array.Copy(_tileCorners, firstOffset, newTileCorners, newOffset, neighborIndex);
				}

				newTileTiles[newOffset + neighborIndex] = tileAlterationQueue[tileAlterationQueuePosition]._tile;
				newTileEdges[newOffset + neighborIndex] = tileAlterationQueue[tileAlterationQueuePosition]._edge;
				newTileCorners[newOffset + neighborIndex] = _tileCorners[firstOffset + (neighborIndex + neighborCount - 1) % neighborCount];

				if (neighborIndex < neighborCount)
				{
					System.Array.Copy(_tileTiles, firstOffset + neighborIndex, newTileTiles, newOffset + neighborIndex + 1, neighborCount - neighborIndex);
					System.Array.Copy(_tileEdges, firstOffset + neighborIndex, newTileEdges, newOffset + neighborIndex + 1, neighborCount - neighborIndex);
					System.Array.Copy(_tileCorners, firstOffset + neighborIndex, newTileCorners, newOffset + neighborIndex + 1, neighborCount - neighborIndex);
				}

				newTileCorners[newOffset + (neighborIndex + neighborCount) % (neighborCount + 1)] = tileAlterationQueue[tileAlterationQueuePosition]._corner;

				newOffset += neighborCount + 1;
			}
			else
			{
				if (neighborIndex > 0)
				{
					System.Array.Copy(_tileTiles, firstOffset, newTileTiles, newOffset, neighborIndex);
					System.Array.Copy(_tileEdges, firstOffset, newTileEdges, newOffset, neighborIndex);
					System.Array.Copy(_tileCorners, firstOffset, newTileCorners, newOffset, neighborIndex);
				}

				if (neighborIndex < neighborCount - 1)
				{
					System.Array.Copy(_tileTiles, firstOffset + neighborIndex + 1, newTileTiles, newOffset + neighborIndex, neighborCount - neighborIndex - 1);
					System.Array.Copy(_tileEdges, firstOffset + neighborIndex + 1, newTileEdges, newOffset + neighborIndex, neighborCount - neighborIndex - 1);
					System.Array.Copy(_tileCorners, firstOffset + neighborIndex + 1, newTileCorners, newOffset + neighborIndex, neighborCount - neighborIndex - 1);
				}

				newOffset += neighborCount - 1;
			}

			++tileAlterationQueuePosition;
		}

		private void RotateEdge(int edge, List<int> tileAlterationIndexQueue, List<TileAlteration> tileAlterationQueue)
		{
			var oldTile0 = _edgeTiles[edge, 0];
			var oldTile1 = _edgeTiles[edge, 1];
			var corner0 = _edgeCorners[edge, 0];
			var corner1 = _edgeCorners[edge, 1];
			var oldTileNeighborIndex0 = GetTileEdgeNeighborIndex(oldTile0, edge);
			var oldTileNeighborIndex1 = GetTileEdgeNeighborIndex(oldTile1, edge);
			var newTile0 = _tileTiles[_tileNeighborOffsets[oldTile0] + RotateTileNeighborIndex(oldTile0, oldTileNeighborIndex0, 1)];
			var newTile1 = _tileTiles[_tileNeighborOffsets[oldTile1] + RotateTileNeighborIndex(oldTile1, oldTileNeighborIndex1, 1)];

			_cornerTiles[corner0, 0] = newTile0;
			_cornerTiles[corner0, 1] = newTile1;
			_cornerTiles[corner0, 2] = oldTile1;
			_cornerTiles[corner1, 0] = newTile1;
			_cornerTiles[corner1, 1] = newTile0;
			_cornerTiles[corner1, 2] = oldTile0;

			var cornerNeighborIndex0 = GetCornerEdgeNeighborIndex(corner0, edge);
			var cornerNeighborIndex1 = GetCornerEdgeNeighborIndex(corner1, edge);
			var cornerNeighborNextIndex0 = RotateCornerNeighborIndex(cornerNeighborIndex0, 1);
			var cornerNeighborNextIndex1 = RotateCornerNeighborIndex(cornerNeighborIndex1, 1);
			var cornerOuterNeighborIndex0 = RotateCornerNeighborIndex(cornerNeighborNextIndex0, 1);
			var cornerOuterNeighborIndex1 = RotateCornerNeighborIndex(cornerNeighborNextIndex1, 1);

			var cornerOuterEdge00 = _cornerEdges[corner0, cornerNeighborNextIndex0];
			var cornerOuterEdge01 = _cornerEdges[corner0, cornerOuterNeighborIndex0];
			var cornerOuterEdge10 = _cornerEdges[corner1, cornerNeighborNextIndex1];
			var cornerOuterEdge11 = _cornerEdges[corner1, cornerOuterNeighborIndex1];

			_cornerEdges[corner0, 0] = edge;
			_cornerEdges[corner0, 1] = cornerOuterEdge11;
			_cornerEdges[corner0, 2] = cornerOuterEdge00;
			_cornerEdges[corner1, 0] = edge;
			_cornerEdges[corner1, 1] = cornerOuterEdge01;
			_cornerEdges[corner1, 2] = cornerOuterEdge10;

			var cornerOuterCorner00 = _cornerCorners[corner0, cornerNeighborNextIndex0];
			var cornerOuterCorner01 = _cornerCorners[corner0, cornerOuterNeighborIndex0];
			var cornerOuterCorner10 = _cornerCorners[corner1, cornerNeighborNextIndex1];
			var cornerOuterCorner11 = _cornerCorners[corner1, cornerOuterNeighborIndex1];

			_cornerCorners[corner0, 0] = corner1;
			_cornerCorners[corner0, 1] = cornerOuterCorner11;
			_cornerCorners[corner0, 2] = cornerOuterCorner00;
			_cornerCorners[corner1, 0] = corner0;
			_cornerCorners[corner1, 1] = cornerOuterCorner01;
			_cornerCorners[corner1, 2] = cornerOuterCorner10;

			_edgeTiles[edge, 0] = newTile0;
			_edgeTiles[edge, 1] = newTile1;

			_edgeCorners[cornerOuterEdge01, GetEdgeCornerNeighborIndex(cornerOuterEdge01, corner0)] = corner1;
			_edgeCorners[cornerOuterEdge11, GetEdgeCornerNeighborIndex(cornerOuterEdge11, corner1)] = corner0;

			_cornerCorners[cornerOuterCorner01, GetCornerCornerNeighborIndex(cornerOuterCorner01, corner0)] = corner1;
			_cornerCorners[cornerOuterCorner11, GetCornerCornerNeighborIndex(cornerOuterCorner11, corner1)] = corner0;

			InsertTileAlteration(oldTile0, new TileAlteration(oldTileNeighborIndex0), tileAlterationIndexQueue, tileAlterationQueue);
			InsertTileAlteration(oldTile1, new TileAlteration(oldTileNeighborIndex1), tileAlterationIndexQueue, tileAlterationQueue);

			var newTileNeighborIndex0 = GetTileEdgeNeighborIndex(newTile0, cornerOuterEdge00);
			var newTileNeighborIndex1 = GetTileEdgeNeighborIndex(newTile1, cornerOuterEdge10);
			InsertTileAlteration(newTile0, new TileAlteration(newTileNeighborIndex0, newTile1, edge, corner1), tileAlterationIndexQueue, tileAlterationQueue);
			InsertTileAlteration(newTile1, new TileAlteration(newTileNeighborIndex1, newTile0, edge, corner0), tileAlterationIndexQueue, tileAlterationQueue);
		}

		public Topology AlterTopology(int passCount, System.Func<Topology, Edge, bool> edgeRotationPredicate, System.Action<Topology> afterPassAction)
		{
			Topology altered = new Topology();

			altered._cornerCorners = _cornerCorners.Clone() as int[,];
			altered._cornerEdges = _cornerEdges.Clone() as int[,];
			altered._cornerTiles = _cornerTiles.Clone() as int[,];
			altered._edgeCorners = _edgeCorners.Clone() as int[,];
			altered._edgeTiles = _edgeTiles.Clone() as int[,];
			altered._tileNeighborOffsets = _tileNeighborOffsets.Clone() as int[];
			altered._tileCorners = _tileCorners.Clone() as int[];
			altered._tileEdges = _tileEdges.Clone() as int[];
			altered._tileTiles = _tileTiles.Clone() as int[];

			if (passCount == 0) return altered;

			//Begin Loop
				//Do Alter Pass
					//For each tile V
						//If tile V is at the beginning of alterations queue
							//Write tile V alterations to the new buffer
						//Else
							//For each neighbor edge E
								//If tile V has the smallest index of the four associated tiles
									//If none of the other three tiles are in the alterations queue (binary search)
										//If random chance is true
											//If edge E is in judged to be reasonably shaped for a topology rotation
												//   o          o   <- newTileIndex1
												//  / \        /|\
												// o---o  ->  o | o <- oldTileIndex0/1
												//  \ /        \|/
												//   o          o   <- newTileIndex0
												//Alter all relevant corners
												//Alter all relevant edges
												//Write tile V alterations to the new buffer
												//Store alterations to the other three tiles in the alterations queue (insert sorted)
												//Skip remaining neighbor edges of tile V
							//If tile did not get altered
								//Write existing tile data to the new buffer
				//Do Relax Pass
			//End Loop

			var newTileNeighborOffsets = new int[_tileNeighborOffsets.Length];
			var newTileTiles = new int[_tileTiles.Length];
			var newTileEdges = new int[_tileEdges.Length];
			var newTileCorners = new int[_tileCorners.Length];

			var tileAlterationIndexQueue = new List<int>();
			var tileAlterationQueue = new List<TileAlteration>();

			for (int pass = 0; pass < passCount; ++pass)
			{
				var newOffset = 0;
				int tileAlterationQueuePosition = 0;

				for (int tile = 0; tile < altered._tileNeighborOffsets.Length - 1; ++tile)
				{
					bool tileAltered = false;

					var firstOffset = altered._tileNeighborOffsets[tile];
					var neighborCount = altered._tileNeighborOffsets[tile + 1] - firstOffset;
					if (tileAlterationQueuePosition == tileAlterationIndexQueue.Count || tileAlterationIndexQueue[tileAlterationQueuePosition] != tile)
					{
						for (int neighbor = 0; neighbor < neighborCount; ++neighbor)
						{
							var neighborLeft = (neighbor + neighborCount - 1) % neighborCount;
							var neighborRight = (neighbor + 1) % neighborCount;

							var tileLeft = altered._tileTiles[firstOffset + neighborLeft];
							var tileMiddle = altered._tileTiles[firstOffset + neighbor];
							var tileRight = altered._tileTiles[firstOffset + neighborRight];

							if (tile > tileLeft || tile > tileMiddle) continue;
							if (tileAlterationIndexQueue.BinarySearch(tileLeft) >= 0) continue;
							if (tileAlterationIndexQueue.BinarySearch(tileMiddle) >= 0) continue;
							if (altered._tileNeighborOffsets[tileMiddle + 1] - altered._tileNeighborOffsets[tileMiddle] <= 3) continue;

							int edgeToRotate;

							if (tile > tileRight) goto tryFarEdge;
							if (tileAlterationIndexQueue.BinarySearch(tileRight) >= 0) goto tryFarEdge;
							if (neighborCount <= 3) goto tryFarEdge;
							edgeToRotate = altered._tileEdges[firstOffset + neighbor];
							if (!edgeRotationPredicate(altered, new Edge(altered, edgeToRotate))) goto tryFarEdge;
							goto rotate;

							tryFarEdge:
							var neighborLeftMiddle = altered.GetTileTileNeighborIndex(tileLeft, tileMiddle);
							var neighborLeftLeft = altered.RotateTileNeighborIndex(tileLeft, neighborLeftMiddle, -1);
							var leftFirstOffset = altered._tileNeighborOffsets[tileLeft];
							var tileLeftLeft = altered._tileTiles[leftFirstOffset + neighborLeftLeft];

							if (tile > tileLeftLeft) continue;
							if (tileAlterationIndexQueue.BinarySearch(tileLeftLeft) >= 0) continue;
							if (altered._tileNeighborOffsets[tileLeft + 1] - altered._tileNeighborOffsets[tileLeft] <= 3) goto tryFarEdge;
							edgeToRotate = altered._tileEdges[leftFirstOffset + neighborLeftMiddle];
							if (!edgeRotationPredicate(altered, new Edge(altered, edgeToRotate))) continue;

							rotate: altered.RotateEdge(edgeToRotate, tileAlterationIndexQueue, tileAlterationQueue);

							tileAltered = true;
							break;
						}
					}
					else
					{
						tileAltered = true;
					}

					if (!tileAltered)
					{
						newTileNeighborOffsets[tile] = newOffset;
						System.Array.Copy(altered._tileTiles, firstOffset, newTileTiles, newOffset, neighborCount);
						System.Array.Copy(altered._tileEdges, firstOffset, newTileEdges, newOffset, neighborCount);
						System.Array.Copy(altered._tileCorners, firstOffset, newTileCorners, newOffset, neighborCount);

						newOffset += neighborCount;
					}
					else
					{
						altered.ApplyTileAlteration(tileAlterationIndexQueue, tileAlterationQueue, newTileNeighborOffsets, newTileTiles, newTileEdges, newTileCorners, ref newOffset, ref tileAlterationQueuePosition);
					}
				}

				newTileNeighborOffsets[newTileNeighborOffsets.Length - 1] = newOffset;

				Utility.Swap(ref altered._tileNeighborOffsets, ref newTileNeighborOffsets);
				Utility.Swap(ref altered._tileTiles, ref newTileTiles);
				Utility.Swap(ref altered._tileEdges, ref newTileEdges);
				Utility.Swap(ref altered._tileCorners, ref newTileCorners);

				tileAlterationIndexQueue.Clear();
				tileAlterationQueue.Clear();

				afterPassAction(altered);
			}

			return altered;
		}
	}
}
