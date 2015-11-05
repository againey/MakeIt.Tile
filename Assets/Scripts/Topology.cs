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
