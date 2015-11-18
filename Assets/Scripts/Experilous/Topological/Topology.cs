using System;

namespace Experilous.Topological
{
	public partial class Topology
	{
		private struct NodeData
		{
			private uint _data;

			public NodeData(int neighborCount, int firstEdge)
			{
				_data = (((uint)neighborCount & 0xFF) << 24) | ((uint)firstEdge & 0xFFFFFF);
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

			public bool isEmpty
			{
				get
				{
					return _data == 0;
				}
			}

			public override string ToString()
			{
				return string.Format("NodeData ({0}, {1})", neighborCount, firstEdge);
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
			foreach (var edge in vertexEdges)
			{
				dual._edgeData[edge.index] = new EdgeData(edge.twinIndex, edge.twin.next.index, edge.prev.twin.index, edge.nextFace.index, edge.nearVertex.index);
			}

			for (int i = 0; i < dual._faceData.Length; ++i)
			{
				dual._faceData[i].firstEdge = dual._edgeData[dual._faceData[i].firstEdge]._twin;
			}

			return dual;
		}

		// Pivot an edge counter-clockwise around its implicit near vertex.
		//
		// \         /          \         /
		//  o-------o            o-------o
		//  |       |            |       |
		//  2   N   |            2   N   |
		//  |       |            |       |
		//  A<--E---V      -->   A       V
		// / \       \          / \     / \
		//    1       \            1   E   \
		//     \       \ /          \ v     \ /
		//      B       o            B       o
		//      |   P   |            |   P   |
		//      0       |            0       |
		//      |       |            |       |
		//      o-------o            o-------o
		//     /         \          /         \
		//
		// E:  edge passed in as parameter
		// V:  vertex to pivot E around clockwise
		// A:  old vertex that E is originally pointing at
		// B:  new vertex that E will now point at
		// P:  previous face on the counter-clockwise side of E
		// N:  next face on the clockwise side of E
		// 0:  outer edge 0 points at B, inner edge 0 points away from B
		// 1:  outer edge 1 points at A, inner edge 1 points at B, at P before pivot, at N after pivot
		// 2:  outer edge 2 points away from A
		// Note:  inner edges point downward, outer edges point upward

		private void PivotEdgeBackwardUnchecked(int edgeIndex)
		{
			var twinEdgeIndex = _edgeData[edgeIndex]._twin;
			var outerEdgeIndex2 = _edgeData[edgeIndex]._fNext;
			var innerEdgeIndex1 = _edgeData[twinEdgeIndex]._vNext;
			var outerEdgeIndex1 = _edgeData[innerEdgeIndex1]._twin;
			var innerEdgeIndex0 = _edgeData[outerEdgeIndex1]._vNext;
			var outerEdgeIndex0 = _edgeData[innerEdgeIndex0]._twin;

			var prevFaceIndex = _edgeData[edgeIndex]._face;
			var nextFaceIndex = _edgeData[twinEdgeIndex]._face;
			var oldVertexIndex = _edgeData[edgeIndex]._vertex;
			var newVertexIndex = _edgeData[innerEdgeIndex1]._vertex;

			// The edge was pointing at the old vertex, will now point at the new vertex.
			_edgeData[edgeIndex]._vertex = newVertexIndex;

			// The second inner edge was pointing at the previous face, will now point at the next face.
			_edgeData[innerEdgeIndex1]._face = prevFaceIndex;

			// Remove twin edge from old vertex linked list by skipping over it.
			_edgeData[outerEdgeIndex2]._vNext = innerEdgeIndex1;

			// Insert twin edge into new vertex linked list.
			_edgeData[outerEdgeIndex1]._vNext = twinEdgeIndex;
			_edgeData[twinEdgeIndex]._vNext = innerEdgeIndex0;

			// Remove second outer edge from previous face linked list by skipping over it.
			_edgeData[outerEdgeIndex0]._fNext = twinEdgeIndex;

			// Insert second outer edge into the previous face linked list.
			_edgeData[edgeIndex]._fNext = outerEdgeIndex1;
			_edgeData[outerEdgeIndex1]._fNext = outerEdgeIndex2;

			// Reroot the vertex and face that just lost edges with edges guaranteed to still belong.
			_vertexData[oldVertexIndex].firstEdge = innerEdgeIndex1;
			_faceData[prevFaceIndex].firstEdge = twinEdgeIndex;

			// Adjust neighbor counts.
			_vertexData[oldVertexIndex].neighborCount -= 1;
			_vertexData[newVertexIndex].neighborCount += 1;
			_faceData[prevFaceIndex].neighborCount -= 1;
			_faceData[nextFaceIndex].neighborCount += 1;
		}

		// Pivot an edge clockwise around its implicit near vertex.
		//
		// \         /          \         /
		//  o-------o            o-------o
		//  |       |            |       |
		//  2   N   |            2   N   |
		//  |       |            |       |
		//  B       V      -->   B<--E---V
		// / \     / \          / \       \
		//    1   E   \            1       \
		//     \ v     \ /          \       \ /
		//      A       o            A       o
		//      |   P   |            |   P   |
		//      0       |            0       |
		//      |       |            |       |
		//      o-------o            o-------o
		//     /         \          /         \
		//
		// E:  edge passed in as parameter
		// V:  vertex to pivot E around clockwise
		// A:  old vertex that E is originally pointing at
		// B:  new vertex that E will now point at
		// P:  previous face on the counter-clockwise side of E
		// N:  next face on the clockwise side of E
		// 0:  outer edge 0 points at A, inner edge 0 points away from A
		// 1:  outer edge 1 points at B, inner edge 1 points at A, at N before pivot, at P after pivot
		// 2:  outer edge 2 points away from B
		// Note:  inner edges point downward, outer edges point upward

		private void PivotEdgeForwardUnchecked(int edgeIndex)
		{
			var twinEdgeIndex = _edgeData[edgeIndex]._twin;
			var outerEdgeIndex1 = _edgeData[edgeIndex]._fNext;
			var outerEdgeIndex2 = _edgeData[outerEdgeIndex1]._fNext;
			var innerEdgeIndex0 = _edgeData[twinEdgeIndex]._vNext;
			var outerEdgeIndex0 = _edgeData[innerEdgeIndex0]._twin;
			var innerEdgeIndex1 = _edgeData[outerEdgeIndex1]._twin;

			var prevFaceIndex = _edgeData[edgeIndex]._face;
			var nextFaceIndex = _edgeData[twinEdgeIndex]._face;
			var oldVertexIndex = _edgeData[edgeIndex]._vertex;
			var newVertexIndex = _edgeData[outerEdgeIndex1]._vertex;

			// The edge was pointing at the old vertex, will now point at the new vertex.
			_edgeData[edgeIndex]._vertex = newVertexIndex;

			// The second inner edge was pointing at the next face, will now point at the previous face.
			_edgeData[innerEdgeIndex1]._face = prevFaceIndex;

			// Remove twin edge from old vertex linked list by skipping over it.
			_edgeData[outerEdgeIndex1]._vNext = outerEdgeIndex0;

			// Insert twin edge into new vertex linked list.
			_edgeData[outerEdgeIndex2]._vNext = twinEdgeIndex;
			_edgeData[twinEdgeIndex]._vNext = innerEdgeIndex1;

			// Remove second outer edge from next face linked list by skipping over it.
			_edgeData[edgeIndex]._fNext = outerEdgeIndex2;

			// Insert second outer edge into the previous face linked list.
			_edgeData[outerEdgeIndex0]._fNext = outerEdgeIndex1;
			_edgeData[outerEdgeIndex1]._fNext = twinEdgeIndex;

			// Reroot the vertex and face that just lost edges with edges guaranteed to still belong.
			_vertexData[oldVertexIndex].firstEdge = outerEdgeIndex1;
			_faceData[nextFaceIndex].firstEdge = edgeIndex;

			// Adjust neighbor counts.
			_vertexData[oldVertexIndex].neighborCount -= 1;
			_vertexData[newVertexIndex].neighborCount += 1;
			_faceData[nextFaceIndex].neighborCount -= 1;
			_faceData[prevFaceIndex].neighborCount += 1;
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
			if (!CanPivotEdgeBackward(edge)) throw new InvalidOperationException("Cannot pivot a vertex edge backward when its previous face has only three sides.");
			PivotEdgeBackwardUnchecked(edge.index);
		}

		public void PivotEdgeForward(VertexEdge edge)
		{
			if (!CanPivotEdgeForward(edge)) throw new InvalidOperationException("Cannot pivot a vertex edge forward when its next face has only three sides.");
			PivotEdgeForwardUnchecked(edge.index);
		}

		public bool CanSpinEdgeBackward(VertexEdge edge)
		{
			return edge.farVertex.neighborCount > 3 && edge.nearVertex.neighborCount > 3;
		}

		public bool CanSpinEdgeForward(VertexEdge edge)
		{
			return edge.farVertex.neighborCount > 3 && edge.nearVertex.neighborCount > 3;
		}

		public void SpinEdgeBackward(VertexEdge edge)
		{
			if (!CanSpinEdgeBackward(edge)) throw new InvalidOperationException("Cannot spin a vertex edge backward when one of its vertices has only three neighbors.");
			PivotEdgeBackwardUnchecked(edge.index);
			PivotEdgeBackwardUnchecked(edge.twinIndex);
		}

		public void SpinEdgeForward(VertexEdge edge)
		{
			if (!CanSpinEdgeForward(edge)) throw new InvalidOperationException("Cannot spin a vertex edge forward when one of its vertices has only three neighbors.");
			PivotEdgeForwardUnchecked(edge.index);
			PivotEdgeForwardUnchecked(edge.twinIndex);
		}
	}
}
