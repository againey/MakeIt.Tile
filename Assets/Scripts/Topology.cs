using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

namespace Tiling
{
	public partial class Topology
	{
		private struct NodeData
		{
			private uint _data;

			public NodeData(int neighborCount, int firstEdge)
			{
				_data = (((uint)neighborCount & 0xFF) << 24) & ((uint)firstEdge & 0xFFFFFF);
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

			public int firstEdge
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

		public Topology()
		{
		}

		public Topology(Topology original)
		{
			_vertexData = original._vertexData.Clone() as NodeData[];
			_edgeData = original._edgeData.Clone() as EdgeData[];
			_faceData = original._faceData.Clone() as NodeData[];
		}

		public Topology Clone()
		{
			return new Topology(this);
		}

		public Topology GetDualTopology()
		{
			var dual = new Topology();
			dual._vertexData = _faceData.Clone() as NodeData[];
			dual._faceData = _vertexData.Clone() as NodeData[];

			dual._edgeData = new EdgeData[_edgeData.Length];
			foreach (var edge in faceEdges)
			{
				dual._edgeData[edge.index] = new EdgeData(edge.twinIndex, edge.prev.index, edge.next.index, edge.farFace.index, edge.nextVertex.index);
			}

			return dual;
		}

		/*public MinimalTopology Subdivide(int degree, System.Action<int> tileAllocator, System.Action<int, int> tileCopier, System.Action<int, int, int, float> tileInterpolator)
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
		}*/

		private void RemoveEdgeFromFarVertex(VertexEdge edge)
		{
			var twin = edge.twin;
			var vertex = edge.farVertex;
			_edgeData[twin.prev.index]._next = twin.next.index;
			_edgeData[twin.next.index]._prev = twin.prev.index;
			_vertexData[vertex.index].neighborCount -= 1;
			if (vertex.firstEdge == twin) _vertexData[vertex.index].firstEdge = twin.next.index;
		}

		private void AddEdgeToFarVertex(Vertex vertex, VertexEdge edge, VertexEdge insertBefore)
		{
			_edgeData[edge.index]._vertex = vertex.index;
			_edgeData[edge.index]._prev = insertBefore.prev.index;
			_edgeData[insertBefore.prev.index]._next = edge.index;
			_edgeData[edge.index]._next = insertBefore.index;
			_edgeData[insertBefore.index]._prev = edge.index;
			_vertexData[vertex.index].neighborCount += 1;
		}

		// Pivot an edge counter-clockwise around its implicit near vertex.
		//
		//    \         /           \         /
		//     o-------o             o-------o
		//    /       / \           /        |\
		//   /       /   \         /         | \
		//  /   3   /     \       /         1|  \
		// o      1/2      o --> o      3    | 4 o
		//  \     /   4   /       \          |2 /
		//   A   /       B         A         | B
		//    \ /       /           \        |/
		//     5--7/8--6             5--7/8--6
		//    /         \           /         \
		//   /     9     \         /     9     \
		//
		// 1:  The edge that is pivoting
		// 2:  The twin of the pivoting edge
		// 3:  The next face from the pivoting edge
		// 4:  The prev face from the pivoting edge
		// 5:  The old far vertex of the pivoting edge
		// 6:  The new far vertex of the pivoting edge
		// 7:  The sliding edge facing outward toward the outside face
		// 8:  The twin of the sliding edge facing inward toward the prev face
		// 9:  The outside face
		// A:  An edge facing inward toward the next face
		// B:  An edge facing outward away from the prev face

		private void PivotEdgeBackwardUnchecked(VertexEdge edge)
		{
			var twinEdge = edge.twin;
			var slidingEdge = twinEdge.next;
			var twinSlidingEdge = slidingEdge.twin;
			var newVertex = slidingEdge.farVertex;

			// Outside face needs to point inward at the prev face.
			_edgeData[twinSlidingEdge.index]._face = edge.nextFace.index;

			RemoveEdgeFromFarVertex(edge);
			AddEdgeToFarVertex(newVertex, edge, twinSlidingEdge.next);
		}

		// Pivot an edge clockwise around its implicit near vertex.
		//
		//    \         /           \         /
		//     o-------o             o-------o
		//    / \       \           /|        \
		//   /   \       \         / |         \
		//  /     \   4   \       /  |2         \
		// o      1\2      o --> o 3 |    4      o
		//  \   3   \     /       \ 1|          /
		//   A       \   B         A |         B
		//    \       \ /           \|        /
		//     6--7/8--5             6--7/8--5
		//    /         \           /         \
		//   /     9     \         /     9     \
		//
		// 1:  The edge that is pivoting
		// 2:  The twin of the pivoting edge
		// 3:  The next face from the pivoting edge
		// 4:  The prev face from the pivoting edge
		// 5:  The old far vertex of the pivoting edge
		// 6:  The new far vertex of the pivoting edge
		// 7:  The sliding edge facing inward toward the next face
		// 8:  The twin of the sliding edge facing outward toward the outside face
		// 9:  The outside face
		// A:  An edge facing inward toward the next face
		// B:  An edge facing outward away from the prev face

		private void PivotEdgeForwardUnchecked(VertexEdge edge)
		{
			var twinEdge = edge.twin;
			var slidingEdge = twinEdge.prev;
			var twinSlidingEdge = slidingEdge.twin;
			var newVertex = slidingEdge.farVertex;

			// Outside face needs to point inward at the prev face.
			_edgeData[slidingEdge.index]._face = edge.prevFace.index;

			RemoveEdgeFromFarVertex(edge);
			AddEdgeToFarVertex(newVertex, edge, twinSlidingEdge);
		}

		public bool CanPivotEdgeBackward(VertexEdge edge)
		{
			return edge.prevFace.neighborCount > 3;
		}

		public bool CanPivotEdgeForward(VertexEdge edge)
		{
			return edge.nextFace.neighborCount > 3;
		}

		public void PivotEdgeBackward(VertexEdge edge)
		{
			if (!CanPivotEdgeBackward(edge)) throw new InvalidOperationException("Cannot pivot a vertex edge backward when it's previous face has only three sides.");
			PivotEdgeBackwardUnchecked(edge);
		}

		public void PivotEdgeForward(VertexEdge edge)
		{
			if (!CanPivotEdgeForward(edge)) throw new InvalidOperationException("Cannot pivot a vertex edge forward when it's next face has only three sides.");
			PivotEdgeForwardUnchecked(edge);
		}

		public void SpinEdgeBackward(VertexEdge edge)
		{
			PivotEdgeBackwardUnchecked(edge);
			PivotEdgeBackwardUnchecked(edge.twin);
		}

		public void SpinEdgeForward(VertexEdge edge)
		{
			PivotEdgeForwardUnchecked(edge);
			PivotEdgeForwardUnchecked(edge.twin);
		}
	}
}
