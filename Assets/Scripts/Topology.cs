using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

namespace Tiling
{
	public partial class Topology
	{
		public Topology()
		{
		}

		public Topology(Topology original)
		{
			_vertexNeighbors = _vertexNeighbors.Clone() as VertexNeighbor[];
			_vertexRoots = _vertexRoots.Clone() as VertexRoot[];
			_edgeNeighbors = _edgeNeighbors.Clone() as EdgeNeighbor[,];
			_faceNeighbors = _faceNeighbors.Clone() as FaceNeighbor[];
			_faceRoots = _faceRoots.Clone() as FaceRoot[];
		}

		public Topology Clone()
		{
			return new Topology(this);
		}

		public Topology GetDualTopology()
		{
			var dual = new Topology();

			dual._vertexNeighbors = new VertexNeighbor[_faceNeighbors.Length];
			dual._vertexRoots = new VertexRoot[_faceRoots.Length];
			dual._edgeNeighbors = new EdgeNeighbor[_edgeNeighbors.GetLength(0), 2];
			dual._faceNeighbors = new FaceNeighbor[_vertexNeighbors.Length];
			dual._faceRoots = new FaceRoot[_vertexRoots.Length];

			for (int i = 0; i < _faceRoots.Length; ++i)
			{
				dual._vertexRoots[i] = new VertexRoot(_faceRoots[i].neighborCount, _faceRoots[i].rootIndex);
			}

			for (int i = 0; i < _faceNeighbors.Length; ++i)
			{
				dual._vertexNeighbors[i] = new VertexNeighbor(_faceNeighbors[i]._prev, _faceNeighbors[i]._next, _faceNeighbors[i]._face, _faceNeighbors[i]._edge, _faceNeighbors[i]._vertex);
			}

			for (int i = 0; i < _edgeNeighbors.Length; ++i)
			{
				dual._edgeNeighbors[i, 0] = new EdgeNeighbor(_edgeNeighbors[i, 0]._face, _edgeNeighbors[i, 1]._vertex);
				dual._edgeNeighbors[i, 1] = new EdgeNeighbor(_edgeNeighbors[i, 1]._face, _edgeNeighbors[i, 0]._vertex);
			}

			for (int i = 0; i < _vertexRoots.Length; ++i)
			{
				dual._faceRoots[i] = new FaceRoot(_vertexRoots[i].neighborCount, _vertexRoots[i].rootIndex);
			}

			for (int i = 0; i < _vertexNeighbors.Length; ++i)
			{
				dual._faceNeighbors[i] = new FaceNeighbor(_vertexNeighbors[i]._prev, _vertexNeighbors[i]._next, _vertexNeighbors[i]._vertex, _vertexNeighbors[i]._edge, _vertexNeighbors[i]._face);
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

		private void MoveVertexNeighborBefore(int vertexIndex, int neighborIndex, int targetVertexIndex, int targetNextNeighbor)
		{
			_vertexRoots[vertexIndex].neighborCount -= 1;
			_vertexRoots[targetVertexIndex].neighborCount += 1;

			if (_vertexRoots[vertexIndex].rootIndex == neighborIndex) _vertexRoots[vertexIndex].rootIndex = _vertexNeighbors[neighborIndex]._next;

			// Remove element from its original list.
			_vertexNeighbors[_vertexNeighbors[neighborIndex]._next]._prev = _vertexNeighbors[neighborIndex]._prev;
			_vertexNeighbors[_vertexNeighbors[neighborIndex]._prev]._next = _vertexNeighbors[neighborIndex]._next;

			// Add element to the target list.
			_vertexNeighbors[neighborIndex]._prev = _vertexNeighbors[targetNextNeighbor]._prev;
			_vertexNeighbors[neighborIndex]._next = targetNextNeighbor;

			// Make target list aware of moved element.
			_vertexNeighbors[targetNextNeighbor]._prev = neighborIndex;
			_vertexNeighbors[_vertexNeighbors[neighborIndex]._prev]._next = neighborIndex;
		}

		private void MoveVertexNeighborAfter(int vertexIndex, int neighborIndex, int targetVertexIndex, int targetPrevNeighbor)
		{
			_vertexRoots[vertexIndex].neighborCount -= 1;
			_vertexRoots[targetVertexIndex].neighborCount += 1;

			if (_vertexRoots[vertexIndex].rootIndex == neighborIndex) _vertexRoots[vertexIndex].rootIndex = _vertexNeighbors[neighborIndex]._next;

			// Remove element from its original list.
			_vertexNeighbors[_vertexNeighbors[neighborIndex]._next]._prev = _vertexNeighbors[neighborIndex]._prev;
			_vertexNeighbors[_vertexNeighbors[neighborIndex]._prev]._next = _vertexNeighbors[neighborIndex]._next;

			// Add element to the target list.
			_vertexNeighbors[neighborIndex]._prev = targetPrevNeighbor;
			_vertexNeighbors[neighborIndex]._next = _vertexNeighbors[targetPrevNeighbor]._next;

			// Make target list aware of moved element.
			_vertexNeighbors[_vertexNeighbors[neighborIndex]._next]._prev = neighborIndex;
			_vertexNeighbors[targetPrevNeighbor]._next = neighborIndex;
		}

		private void MoveFaceNeighborBefore(int faceIndex, int neighborIndex, int targetFaceIndex, int targetNextNeighbor)
		{
			_faceRoots[faceIndex].neighborCount -= 1;
			_faceRoots[targetFaceIndex].neighborCount += 1;

			if (_faceRoots[faceIndex].rootIndex == neighborIndex) _faceRoots[faceIndex].rootIndex = _faceNeighbors[neighborIndex]._next;

			// Remove element from its original list.
			_faceNeighbors[_faceNeighbors[neighborIndex]._next]._prev = _faceNeighbors[neighborIndex]._prev;
			_faceNeighbors[_faceNeighbors[neighborIndex]._prev]._next = _faceNeighbors[neighborIndex]._next;

			// Add element to the target list.
			_faceNeighbors[neighborIndex]._prev = _faceNeighbors[targetNextNeighbor]._prev;
			_faceNeighbors[neighborIndex]._next = targetNextNeighbor;

			// Make target list aware of moved element.
			_faceNeighbors[targetNextNeighbor]._prev = neighborIndex;
			_faceNeighbors[_faceNeighbors[neighborIndex]._prev]._next = neighborIndex;
		}

		private void MoveFaceNeighborAfter(int faceIndex, int neighborIndex, int targetFaceIndex, int targetPrevNeighbor)
		{
			_faceRoots[faceIndex].neighborCount -= 1;
			_faceRoots[targetFaceIndex].neighborCount += 1;

			if (_faceRoots[faceIndex].rootIndex == neighborIndex) _faceRoots[faceIndex].rootIndex = _faceNeighbors[neighborIndex]._next;

			// Remove element from its original list.
			_faceNeighbors[_faceNeighbors[neighborIndex]._next]._prev = _faceNeighbors[neighborIndex]._prev;
			_faceNeighbors[_faceNeighbors[neighborIndex]._prev]._next = _faceNeighbors[neighborIndex]._next;

			// Add element to the target list.
			_faceNeighbors[neighborIndex]._prev = targetPrevNeighbor;
			_faceNeighbors[neighborIndex]._next = _faceNeighbors[targetPrevNeighbor]._next;

			// Make target list aware of moved element.
			_faceNeighbors[_faceNeighbors[neighborIndex]._next]._prev = neighborIndex;
			_faceNeighbors[targetPrevNeighbor]._next = neighborIndex;
		}

		// Shift a vertex's neighbor edge/vertex pair over to the next neighbor vertex.
		//
		//   o---o           o---o
		//  /   / \         /    |\
		// o   E   o  -->  o     E o
		//  \ /   /         \    |/
		//   V---N           V---N
		//  / --> \         /     \
		private void ShiftVertexNeighborPrev(Vertex vertex, Vertex.Neighbor neighbor)
		{
			var prevNeighbor = neighbor.prev;
			var prevPrevNeighbor = prevNeighbor.prev;

			var neighborVertex = neighbor.vertex;
			var pivotingVertex = prevNeighbor.vertex;
			var pivotingEdge = prevNeighbor.edge;
			var outsideEdge = neighbor.edge;
			var nextFace = prevNeighbor.face;
			var prevFace = prevPrevNeighbor.face;
			var outsideFace = neighbor.face;

			// Update the new vertex on the shifting end of the pivoting edge.
			_edgeNeighbors[pivotingEdge.index, pivotingEdge.FindNeighbor(vertex).index]._vertex = neighborVertex.index;
			// Update the new face next to the outside edge.
			_edgeNeighbors[outsideEdge.index, outsideEdge.FindNeighbor(nextFace).index]._face = prevFace.index;

			// Update the new neighbor vertex of the pivoting vertex.
			_vertexNeighbors[pivotingVertex.FindNeighbor(vertex).index]._vertex = neighborVertex.index;

			// Update the new neighbor face of the outside face.
			_faceNeighbors[outsideFace.FindNeighbor(outsideEdge).index]._face = prevFace.index;

			// Move the linked list element from the original vertex to the neighbor vertex.
			MoveVertexNeighborAfter(vertex.index, prevNeighbor.index, neighborVertex.index, neighborVertex.FindNeighbor(vertex).index);
			// Move the linked list element from the next face to the prev face.
			MoveFaceNeighborAfter(nextFace.index, nextFace.FindNeighbor(outsideEdge).index, prevFace.index, prevFace.FindNeighbor(pivotingEdge).index);
		}

		// Shift a vertex's neighbor edge/vertex pair over to the next neighbor vertex.
		//
		//   o---o           o---o
		//  / \   \         /|    \
		// o   E   o  -->  o E     o
		//  \   \ /         \|    /
		//   N---V           N---V
		//  / <-- \         /     \
		private void ShiftVertexNeighborNext(Vertex vertex, Vertex.Neighbor neighbor)
		{
			var nextNeighbor = neighbor.next;
			var prevNeighbor = neighbor.prev;

			var neighborVertex = neighbor.vertex;
			var pivotingVertex = nextNeighbor.vertex;
			var pivotingEdge = nextNeighbor.edge;
			var outsideEdge = neighbor.edge;
			var nextFace = nextNeighbor.face;
			var prevFace = neighbor.face;
			var outsideFace = prevNeighbor.face;

			// Update the new vertex on the shifting end of the pivoting edge.
			_edgeNeighbors[pivotingEdge.index, pivotingEdge.FindNeighbor(vertex).index]._vertex = neighborVertex.index;
			// Update the new face next to the outside edge.
			_edgeNeighbors[outsideEdge.index, outsideEdge.FindNeighbor(prevFace).index]._face = nextFace.index;

			// Update the new neighbor vertex of the pivoting vertex.
			_vertexNeighbors[pivotingVertex.FindNeighbor(vertex).index]._vertex = neighborVertex.index;

			// Update the new neighbor face of the outside face.
			_faceNeighbors[outsideFace.FindNeighbor(outsideEdge).index]._face = nextFace.index;

			// Move the linked list element from the original vertex to the neighbor vertex.
			MoveVertexNeighborBefore(vertex.index, nextNeighbor.index, neighborVertex.index, neighborVertex.FindNeighbor(vertex).index);
			// Move the linked list element from the prev face to the next face.
			MoveFaceNeighborBefore(prevFace.index, prevFace.FindNeighbor(outsideEdge).index, nextFace.index, nextFace.FindNeighbor(pivotingEdge).index);
		}

		// Rotate counter-clockwise around the pivot vertex until all that remains in the counter-clockwise
		// direction is a triangle.  The next pivot will then move the pivot point to the far vertex of that
		// triangle, and will connect the edge to the vertex that is next in the clockwise direction from the
		// pivot's prior vertex.
		//
		//   ---->               *
		//   o---o           o---o           o---o
		//  / \   \         /   /|\         /     \
		// o   E   o  -->  o   v E o  -->  o     _-P
		//  \   \ /         \    |/  ^      \  _- /
		//   o---P           o---P   |       o---o
		public void PivotEdgePrev(Edge edge, Edge.Neighbor pivot)
		{
			var nextFace = pivot.face;
			if (nextFace.Neighbors.Count > 3)
			{
				var shiftingVertex = pivot.next.vertex;
				ShiftVertexNeighborNext(shiftingVertex, shiftingVertex.FindNeighbor(edge).prev);
			}
			else
			{
				var firstShiftingVertex = pivot.vertex;
				var secondShiftingVertex = pivot.next.vertex;
				ShiftVertexNeighborNext(firstShiftingVertex, firstShiftingVertex.FindNeighbor(edge).prev);
				ShiftVertexNeighborNext(secondShiftingVertex, secondShiftingVertex.FindNeighbor(edge).prev);
				Utility.Swap(ref _edgeNeighbors[edge.index, 0], ref _edgeNeighbors[edge.index, 1]);
			}
		}

		// Rotate counter-clockwise around the pivot vertex until all that remains in the counter-clockwise
		// direction is a triangle.  The next pivot will then move the pivot point to the far vertex of that
		// triangle, and will connect the edge to the vertex that is next in the clockwise direction from the
		// pivot's prior vertex.
		//
		//   <----           *
		//   o---o           o---o           o---o
		//  /   / \         /|\   \         /     \
		// o   E   o  -->  o E v   o  -->  P-_     o
		//  \ /   /      ^  \|    /         \ -_  /
		//   P---o       |   P---o           o---o
		public void PivotEdgeNext(Edge edge, Edge.Neighbor pivot)
		{
			var nextFace = pivot.face;
			if (nextFace.Neighbors.Count > 3)
			{
				var shiftingVertex = pivot.next.vertex;
				ShiftVertexNeighborPrev(shiftingVertex, shiftingVertex.FindNeighbor(edge).next);
			}
			else
			{
				var firstShiftingVertex = pivot.vertex;
				var secondShiftingVertex = pivot.next.vertex;
				ShiftVertexNeighborPrev(firstShiftingVertex, firstShiftingVertex.FindNeighbor(edge).next);
				ShiftVertexNeighborPrev(secondShiftingVertex, secondShiftingVertex.FindNeighbor(edge).next);
				Utility.Swap(ref _edgeNeighbors[edge.index, 0], ref _edgeNeighbors[edge.index, 1]);
			}
		}

		// Pivot around the first neighbor by default.  Pivoting around the second neighbor would merey
		// reverse the direction of pivoting anyway.
		public void PivotEdgePrev(Edge edge)
		{
			PivotEdgePrev(edge, new Edge.Neighbor(this, edge.index, 0));
		}

		// Pivot around the first neighbor by default.  Pivoting around the second neighbor would merey
		// reverse the direction of pivoting anyway.
		public void PivotEdgeNext(Edge edge)
		{
			PivotEdgeNext(edge, new Edge.Neighbor(this, edge.index, 0));
		}

		// Shift both edge ends forward around the pair of faces.
		//
		// |   o---o           o---o
		// v  / \   \         /     \
		//   o   E   o  --> *o---E---o*
		//    \   \ /  ^      \     /
		//     o---o   |       o---o
		public void SpinEdgePrev(Edge edge)
		{
			var firstShiftingVertex = new Vertex(this, _edgeNeighbors[edge.index, 0]._vertex);
			var secondShiftingVertex = new Vertex(this, _edgeNeighbors[edge.index, 1]._vertex);
			ShiftVertexNeighborPrev(firstShiftingVertex, firstShiftingVertex.FindNeighbor(edge).next);
			ShiftVertexNeighborPrev(secondShiftingVertex, secondShiftingVertex.FindNeighbor(edge).next);
		}

		// Shift both edge ends forward around the pair of faces.
		//
		//     o---o   |       o---o
		//    /   / \  v      /     \
		//   o   E   o  --> *o---E---o*
		// ^  \ /   /         \     /
		// |   o---o           o---o
		public void SpinEdgeNext(Edge edge)
		{
			var firstShiftingVertex = new Vertex(this, _edgeNeighbors[edge.index, 0]._vertex);
			var secondShiftingVertex = new Vertex(this, _edgeNeighbors[edge.index, 1]._vertex);
			ShiftVertexNeighborPrev(firstShiftingVertex, firstShiftingVertex.FindNeighbor(edge).prev);
			ShiftVertexNeighborPrev(secondShiftingVertex, secondShiftingVertex.FindNeighbor(edge).prev);
		}

		// Reorder the vertex neighbors and face neighbors in physical memory so that all of the
		// embedded linked lists are essentially contiguous.
		public void Optimize()
		{
			var optimizedVertexNeighbors = new VertexNeighbor[_vertexNeighbors.Length];
			var nextNeighborIndex = 0;

			for (int vertexIndex = 0; vertexIndex < _vertexNeighbors.Length; ++vertexIndex)
			{
				var neighborCount = _vertexRoots[vertexIndex].neighborCount;
				if (neighborCount == 0) continue;

				var neighborIndex = _vertexRoots[vertexIndex].rootIndex;
				_vertexRoots[vertexIndex].rootIndex = nextNeighborIndex;
				while (neighborCount > 0)
				{
					optimizedVertexNeighbors[nextNeighborIndex]._prev = nextNeighborIndex - 1;
					optimizedVertexNeighbors[nextNeighborIndex]._next = nextNeighborIndex + 1;
					optimizedVertexNeighbors[nextNeighborIndex]._vertex = _vertexNeighbors[neighborIndex]._vertex;
					optimizedVertexNeighbors[nextNeighborIndex]._edge = _vertexNeighbors[neighborIndex]._edge;
					optimizedVertexNeighbors[nextNeighborIndex]._face = _vertexNeighbors[neighborIndex]._face;

					--neighborCount;
					++nextNeighborIndex;
					neighborIndex = _vertexNeighbors[neighborIndex]._next;
				}

				neighborCount = _vertexRoots[vertexIndex].neighborCount;
				optimizedVertexNeighbors[nextNeighborIndex - 1]._next = nextNeighborIndex - neighborCount;
				optimizedVertexNeighbors[nextNeighborIndex - neighborCount]._prev = nextNeighborIndex - 1;
			}

			_vertexNeighbors = optimizedVertexNeighbors;

			var optimizedFaceNeighbors = new FaceNeighbor[_faceNeighbors.Length];
			nextNeighborIndex = 0;

			for (int faceIndex = 0; faceIndex < _faceNeighbors.Length; ++faceIndex)
			{
				var neighborCount = _faceRoots[faceIndex].neighborCount;
				if (neighborCount == 0) continue;

				var neighborIndex = _faceRoots[faceIndex].rootIndex;
				_faceRoots[faceIndex].rootIndex = nextNeighborIndex;
				while (neighborCount > 0)
				{
					optimizedFaceNeighbors[nextNeighborIndex]._prev = nextNeighborIndex - 1;
					optimizedFaceNeighbors[nextNeighborIndex]._next = nextNeighborIndex + 1;
					optimizedFaceNeighbors[nextNeighborIndex]._vertex = _faceNeighbors[neighborIndex]._vertex;
					optimizedFaceNeighbors[nextNeighborIndex]._edge = _faceNeighbors[neighborIndex]._edge;
					optimizedFaceNeighbors[nextNeighborIndex]._face = _faceNeighbors[neighborIndex]._face;

					--neighborCount;
					++nextNeighborIndex;
					neighborIndex = _faceNeighbors[neighborIndex]._next;
				}

				neighborCount = _faceRoots[faceIndex].neighborCount;
				optimizedFaceNeighbors[nextNeighborIndex - 1]._next = nextNeighborIndex - neighborCount;
				optimizedFaceNeighbors[nextNeighborIndex - neighborCount]._prev = nextNeighborIndex - 1;
			}

			_faceNeighbors = optimizedFaceNeighbors;
		}
	}
}
