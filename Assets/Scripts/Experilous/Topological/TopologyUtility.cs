using System;

namespace Experilous.Topological
{
	public static class TopologyUtility
	{
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

		private static void PivotVertexEdgeBackwardUnchecked(Topology topology, int edgeIndex, int twinEdgeIndex, int outerEdgeIndex1, int innerEdgeIndex1)
		{
			var innerEdgeIndex0 = topology.edgeData[outerEdgeIndex1]._vNext;
			var outerEdgeIndex0 = topology.edgeData[innerEdgeIndex0]._twin;
			var outerEdgeIndex2 = topology.edgeData[edgeIndex]._fNext;

			var prevFaceIndex = topology.edgeData[edgeIndex]._face;
			var nextFaceIndex = topology.edgeData[twinEdgeIndex]._face;
			var oldVertexIndex = topology.edgeData[edgeIndex]._vertex;
			var newVertexIndex = topology.edgeData[innerEdgeIndex1]._vertex;

			// The edge was pointing at the old vertex, will now point at the new vertex.
			topology.edgeData[edgeIndex]._vertex = newVertexIndex;

			// The second inner edge was pointing at the previous face, will now point at the next face.
			topology.edgeData[innerEdgeIndex1]._face = prevFaceIndex;

			// Remove twin edge from old vertex linked list by skipping over it.
			topology.edgeData[outerEdgeIndex2]._vNext = innerEdgeIndex1;

			// Insert twin edge into new vertex linked list.
			topology.edgeData[outerEdgeIndex1]._vNext = twinEdgeIndex;
			topology.edgeData[twinEdgeIndex]._vNext = innerEdgeIndex0;

			// Remove second outer edge from previous face linked list by skipping over it.
			topology.edgeData[outerEdgeIndex0]._fNext = twinEdgeIndex;

			// Insert second outer edge into the previous face linked list.
			topology.edgeData[edgeIndex]._fNext = outerEdgeIndex1;
			topology.edgeData[outerEdgeIndex1]._fNext = outerEdgeIndex2;

			// Reroot the vertex and face that just lost edges with edges guaranteed to still belong.
			topology.vertexFirstEdgeIndices[oldVertexIndex] = innerEdgeIndex1;
			topology.faceFirstEdgeIndices[prevFaceIndex] = twinEdgeIndex;

			// Adjust neighbor counts.
			topology.vertexNeighborCounts[oldVertexIndex] -= 1;
			topology.vertexNeighborCounts[newVertexIndex] += 1;
			topology.faceNeighborCounts[prevFaceIndex] -= 1; // Dropping below 0 is undefined behavior; it better not ever happen.
			topology.faceNeighborCounts[nextFaceIndex] +=1; // Surpassing 32767 is undefined behavior; it better not ever happen.
		}

		private static void PivotVertexEdgeBackwardUnchecked(Topology topology, IEdgeAttribute<EdgeWrap> edgeWrap, int edgeIndex, int twinEdgeIndex, int outerEdgeIndex1, int innerEdgeIndex1)
		{
			var innerEdgeIndex0 = topology.edgeData[outerEdgeIndex1]._vNext;
			var outerEdgeIndex0 = topology.edgeData[innerEdgeIndex0]._twin;
			var outerEdgeIndex2 = topology.edgeData[edgeIndex]._fNext;

			var prevFaceIndex = topology.edgeData[edgeIndex]._face;
			var nextFaceIndex = topology.edgeData[twinEdgeIndex]._face;
			var oldVertexIndex = topology.edgeData[edgeIndex]._vertex;
			var newVertexIndex = topology.edgeData[innerEdgeIndex1]._vertex;

			// The edge was pointing at the old vertex, will now point at the new vertex.
			topology.edgeData[edgeIndex]._vertex = newVertexIndex;

			// The second inner edge was pointing at the previous face, will now point at the next face.
			topology.edgeData[innerEdgeIndex1]._face = prevFaceIndex;

			// Remove twin edge from old vertex linked list by skipping over it.
			topology.edgeData[outerEdgeIndex2]._vNext = innerEdgeIndex1;

			// Insert twin edge into new vertex linked list.
			topology.edgeData[outerEdgeIndex1]._vNext = twinEdgeIndex;
			topology.edgeData[twinEdgeIndex]._vNext = innerEdgeIndex0;

			// Remove second outer edge from previous face linked list by skipping over it.
			topology.edgeData[outerEdgeIndex0]._fNext = twinEdgeIndex;

			// Insert second outer edge into the previous face linked list.
			topology.edgeData[edgeIndex]._fNext = outerEdgeIndex1;
			topology.edgeData[outerEdgeIndex1]._fNext = outerEdgeIndex2;

			// Reroot the vertex and face that just lost edges with edges guaranteed to still belong.
			topology.vertexFirstEdgeIndices[oldVertexIndex] = innerEdgeIndex1;
			topology.faceFirstEdgeIndices[prevFaceIndex] = twinEdgeIndex;

			// Adjust neighbor counts.
			topology.vertexNeighborCounts[oldVertexIndex] -= 1;
			topology.vertexNeighborCounts[newVertexIndex] += 1;
			topology.faceNeighborCounts[prevFaceIndex] -= 1; // Dropping below 0 is undefined behavior; it better not ever happen.
			topology.faceNeighborCounts[nextFaceIndex] +=1; // Surpassing 32767 is undefined behavior; it better not ever happen.

			edgeWrap[edgeIndex] =
				EdgeWrapUtility.ChainVertToVertToVert(edgeWrap[edgeIndex], edgeWrap[innerEdgeIndex1]) | //vert-to-vert
				EdgeWrapUtility.ChainFaceToVertToVert(edgeWrap[edgeIndex], edgeWrap[innerEdgeIndex1]) | //face-to-vert
				edgeWrap[edgeIndex] & ~EdgeWrap.ToVert;

			edgeWrap[twinEdgeIndex] =
				EdgeWrapUtility.ChainVertToVertToVert(edgeWrap[outerEdgeIndex1], edgeWrap[twinEdgeIndex]) | //vert-to-vert
				EdgeWrapUtility.ChainVertToVertToFace(edgeWrap[outerEdgeIndex1], edgeWrap[twinEdgeIndex]) | //vert-to-face
				edgeWrap[twinEdgeIndex] & ~EdgeWrap.VertTo;

			edgeWrap[outerEdgeIndex1] =
				EdgeWrapUtility.ChainFaceToFaceToFace(edgeWrap[edgeIndex], edgeWrap[outerEdgeIndex1]) | //face-to-face
				EdgeWrapUtility.ChainFaceToFaceToVert(edgeWrap[edgeIndex], edgeWrap[outerEdgeIndex1]) | //face-to-vert
				edgeWrap[outerEdgeIndex1] & ~EdgeWrap.FaceTo;

			edgeWrap[innerEdgeIndex1] =
				EdgeWrapUtility.ChainFaceToFaceToFace(edgeWrap[innerEdgeIndex1], edgeWrap[twinEdgeIndex]) | //face-to-face
				EdgeWrapUtility.ChainVertToFaceToFace(edgeWrap[innerEdgeIndex1], edgeWrap[twinEdgeIndex]) | //vert-to-face
				edgeWrap[innerEdgeIndex1] & ~EdgeWrap.ToFace;
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

		private static void PivotVertexEdgeForwardUnchecked(Topology topology, int edgeIndex, int twinEdgeIndex, int outerEdgeIndex1, int innerEdgeIndex1)
		{
			var innerEdgeIndex0 = topology.edgeData[twinEdgeIndex]._vNext;
			var outerEdgeIndex0 = topology.edgeData[innerEdgeIndex0]._twin;
			var outerEdgeIndex2 = topology.edgeData[outerEdgeIndex1]._fNext;

			var prevFaceIndex = topology.edgeData[edgeIndex]._face;
			var nextFaceIndex = topology.edgeData[twinEdgeIndex]._face;
			var oldVertexIndex = topology.edgeData[edgeIndex]._vertex;
			var newVertexIndex = topology.edgeData[outerEdgeIndex1]._vertex;

			// The edge was pointing at the old vertex, will now point at the new vertex.
			topology.edgeData[edgeIndex]._vertex = newVertexIndex;

			// The second inner edge was pointing at the next face, will now point at the previous face.
			topology.edgeData[innerEdgeIndex1]._face = prevFaceIndex;

			// Remove twin edge from old vertex linked list by skipping over it.
			topology.edgeData[outerEdgeIndex1]._vNext = innerEdgeIndex0;

			// Insert twin edge into new vertex linked list.
			topology.edgeData[outerEdgeIndex2]._vNext = twinEdgeIndex;
			topology.edgeData[twinEdgeIndex]._vNext = innerEdgeIndex1;

			// Remove second outer edge from next face linked list by skipping over it.
			topology.edgeData[edgeIndex]._fNext = outerEdgeIndex2;

			// Insert second outer edge into the previous face linked list.
			topology.edgeData[outerEdgeIndex0]._fNext = outerEdgeIndex1;
			topology.edgeData[outerEdgeIndex1]._fNext = twinEdgeIndex;

			// Reroot the vertex and face that just lost edges with edges guaranteed to still belong.
			topology.vertexFirstEdgeIndices[oldVertexIndex] = outerEdgeIndex1;
			topology.faceFirstEdgeIndices[nextFaceIndex] = edgeIndex;

			// Adjust neighbor counts.
			topology.vertexNeighborCounts[oldVertexIndex] -= 1;
			topology.vertexNeighborCounts[newVertexIndex] += 1;
			topology.faceNeighborCounts[nextFaceIndex] -= 1; // Dropping below 0 is undefined behavior; it better not ever happen.
			topology.faceNeighborCounts[prevFaceIndex] +=1; // Surpassing 32767 is undefined behavior; it better not ever happen.
		}

		private static void PivotVertexEdgeForwardUnchecked(Topology topology, IEdgeAttribute<EdgeWrap> edgeWrap, int edgeIndex, int twinEdgeIndex, int outerEdgeIndex1, int innerEdgeIndex1)
		{
			var innerEdgeIndex0 = topology.edgeData[twinEdgeIndex]._vNext;
			var outerEdgeIndex0 = topology.edgeData[innerEdgeIndex0]._twin;
			var outerEdgeIndex2 = topology.edgeData[outerEdgeIndex1]._fNext;

			var prevFaceIndex = topology.edgeData[edgeIndex]._face;
			var nextFaceIndex = topology.edgeData[twinEdgeIndex]._face;
			var oldVertexIndex = topology.edgeData[edgeIndex]._vertex;
			var newVertexIndex = topology.edgeData[outerEdgeIndex1]._vertex;

			// The edge was pointing at the old vertex, will now point at the new vertex.
			topology.edgeData[edgeIndex]._vertex = newVertexIndex;

			// The second inner edge was pointing at the next face, will now point at the previous face.
			topology.edgeData[innerEdgeIndex1]._face = prevFaceIndex;

			// Remove twin edge from old vertex linked list by skipping over it.
			topology.edgeData[outerEdgeIndex1]._vNext = innerEdgeIndex0;

			// Insert twin edge into new vertex linked list.
			topology.edgeData[outerEdgeIndex2]._vNext = twinEdgeIndex;
			topology.edgeData[twinEdgeIndex]._vNext = innerEdgeIndex1;

			// Remove second outer edge from next face linked list by skipping over it.
			topology.edgeData[edgeIndex]._fNext = outerEdgeIndex2;

			// Insert second outer edge into the previous face linked list.
			topology.edgeData[outerEdgeIndex0]._fNext = outerEdgeIndex1;
			topology.edgeData[outerEdgeIndex1]._fNext = twinEdgeIndex;

			// Reroot the vertex and face that just lost edges with edges guaranteed to still belong.
			topology.vertexFirstEdgeIndices[oldVertexIndex] = outerEdgeIndex1;
			topology.faceFirstEdgeIndices[nextFaceIndex] = edgeIndex;

			// Adjust neighbor counts.
			topology.vertexNeighborCounts[oldVertexIndex] -= 1;
			topology.vertexNeighborCounts[newVertexIndex] += 1;
			topology.faceNeighborCounts[nextFaceIndex] -= 1; // Dropping below 0 is undefined behavior; it better not ever happen.
			topology.faceNeighborCounts[prevFaceIndex] +=1; // Surpassing 32767 is undefined behavior; it better not ever happen.

			edgeWrap[edgeIndex] =
				EdgeWrapUtility.ChainVertToVertToVert(edgeWrap[edgeIndex], edgeWrap[outerEdgeIndex1]) | //vert-to-vert
				EdgeWrapUtility.ChainFaceToVertToVert(edgeWrap[edgeIndex], edgeWrap[outerEdgeIndex1]) | //face-to-vert
				edgeWrap[edgeIndex] & ~EdgeWrap.ToVert;

			edgeWrap[twinEdgeIndex] =
				EdgeWrapUtility.ChainVertToVertToVert(edgeWrap[innerEdgeIndex1], edgeWrap[twinEdgeIndex]) | //vert-to-vert
				EdgeWrapUtility.ChainVertToVertToFace(edgeWrap[innerEdgeIndex1], edgeWrap[twinEdgeIndex]) | //vert-to-face
				edgeWrap[twinEdgeIndex] & ~EdgeWrap.VertTo;

			edgeWrap[outerEdgeIndex1] =
				EdgeWrapUtility.ChainFaceToFaceToFace(edgeWrap[twinEdgeIndex], edgeWrap[outerEdgeIndex1]) | //face-to-face
				EdgeWrapUtility.ChainFaceToFaceToVert(edgeWrap[twinEdgeIndex], edgeWrap[outerEdgeIndex1]) | //face-to-vert
				edgeWrap[outerEdgeIndex1] & ~EdgeWrap.FaceTo;

			edgeWrap[innerEdgeIndex1] =
				EdgeWrapUtility.ChainFaceToFaceToFace(edgeWrap[innerEdgeIndex1], edgeWrap[edgeIndex]) | //face-to-face
				EdgeWrapUtility.ChainVertToFaceToFace(edgeWrap[innerEdgeIndex1], edgeWrap[edgeIndex]) | //vert-to-face
				edgeWrap[innerEdgeIndex1] & ~EdgeWrap.ToFace;
		}

		private static void PivotEdgeBackwardUnchecked(Topology.VertexEdge edge)
		{
			var topology = edge.topology;
			int edgeIndex = edge.index;
			var twinEdgeIndex = topology.edgeData[edgeIndex]._twin;
			var innerEdgeIndex1 = topology.edgeData[twinEdgeIndex]._vNext;
			var outerEdgeIndex1 = topology.edgeData[innerEdgeIndex1]._twin;
			PivotVertexEdgeBackwardUnchecked(topology, edgeIndex, twinEdgeIndex, outerEdgeIndex1, innerEdgeIndex1);
		}

		private static void PivotEdgeBackwardUnchecked(Topology.VertexEdge edge, IEdgeAttribute<EdgeWrap> edgeWrap)
		{
			var topology = edge.topology;
			int edgeIndex = edge.index;
			var twinEdgeIndex = topology.edgeData[edgeIndex]._twin;
			var innerEdgeIndex1 = topology.edgeData[twinEdgeIndex]._vNext;
			var outerEdgeIndex1 = topology.edgeData[innerEdgeIndex1]._twin;
			PivotVertexEdgeBackwardUnchecked(topology, edgeWrap, edgeIndex, twinEdgeIndex, outerEdgeIndex1, innerEdgeIndex1);
		}

		private static void PivotEdgeForwardUnchecked(Topology.VertexEdge edge)
		{
			var topology = edge.topology;
			int edgeIndex = edge.index;
			int twinEdgeIndex = topology.edgeData[edgeIndex]._twin;
			var outerEdgeIndex1 = topology.edgeData[edgeIndex]._fNext;
			var innerEdgeIndex1 = topology.edgeData[outerEdgeIndex1]._twin;
			PivotVertexEdgeForwardUnchecked(topology, edgeIndex, twinEdgeIndex, outerEdgeIndex1, innerEdgeIndex1);
		}

		private static void PivotEdgeForwardUnchecked(Topology.VertexEdge edge, IEdgeAttribute<EdgeWrap> edgeWrap)
		{
			var topology = edge.topology;
			int edgeIndex = edge.index;
			int twinEdgeIndex = topology.edgeData[edgeIndex]._twin;
			var outerEdgeIndex1 = topology.edgeData[edgeIndex]._fNext;
			var innerEdgeIndex1 = topology.edgeData[outerEdgeIndex1]._twin;
			PivotVertexEdgeForwardUnchecked(topology, edgeWrap, edgeIndex, twinEdgeIndex, outerEdgeIndex1, innerEdgeIndex1);
		}

		// Pivoting around the near face of an edge ultimately results in a simple
		// pivot of an edge around a vertex, except it's a different edge pivoting
		// around a different vertex.  The original face edge becomes the edge
		// along which the vertex edge slides its far vertex.

		private static void PivotEdgeBackwardUnchecked(Topology.FaceEdge edge)
		{
			var topology = edge.topology;
			var innerEdgeIndex1 = edge.index;
			var outerEdgeIndex1 = topology.edgeData[innerEdgeIndex1]._twin;
			var twinEdgeIndex = topology.edgeData[outerEdgeIndex1]._fNext;
			var edgeIndex = topology.edgeData[twinEdgeIndex]._twin;
			PivotVertexEdgeBackwardUnchecked(topology, edgeIndex, twinEdgeIndex, outerEdgeIndex1, innerEdgeIndex1);
		}

		private static void PivotEdgeBackwardUnchecked(Topology.FaceEdge edge, IEdgeAttribute<EdgeWrap> edgeWrap)
		{
			var topology = edge.topology;
			var innerEdgeIndex1 = edge.index;
			var outerEdgeIndex1 = topology.edgeData[innerEdgeIndex1]._twin;
			var twinEdgeIndex = topology.edgeData[outerEdgeIndex1]._fNext;
			var edgeIndex = topology.edgeData[twinEdgeIndex]._twin;
			PivotVertexEdgeBackwardUnchecked(topology, edgeWrap, edgeIndex, twinEdgeIndex, outerEdgeIndex1, innerEdgeIndex1);
		}

		private static void PivotEdgeForwardUnchecked(Topology.FaceEdge edge)
		{
			var topology = edge.topology;
			var innerEdgeIndex1 = edge.index;
			var outerEdgeIndex1 = topology.edgeData[innerEdgeIndex1]._twin;
			var twinEdgeIndex = topology.edgeData[outerEdgeIndex1]._vNext;
			var edgeIndex = topology.edgeData[twinEdgeIndex]._twin;
			PivotVertexEdgeForwardUnchecked(topology, edgeIndex, twinEdgeIndex, outerEdgeIndex1, innerEdgeIndex1);
		}

		private static void PivotEdgeForwardUnchecked(Topology.FaceEdge edge, IEdgeAttribute<EdgeWrap> edgeWrap)
		{
			var topology = edge.topology;
			var innerEdgeIndex1 = edge.index;
			var outerEdgeIndex1 = topology.edgeData[innerEdgeIndex1]._twin;
			var twinEdgeIndex = topology.edgeData[outerEdgeIndex1]._vNext;
			var edgeIndex = topology.edgeData[twinEdgeIndex]._twin;
			PivotVertexEdgeForwardUnchecked(topology, edgeWrap, edgeIndex, twinEdgeIndex, outerEdgeIndex1, innerEdgeIndex1);
		}

		public static bool CanPivotEdgeBackward(Topology.VertexEdge edge)
		{
			// After pivoting, the edge's old far vertex and previous face will both have their
			// neighbor counts reduced by one.  Neither of these counts can fall below 2.
			return edge.farVertex.neighborCount > 2 && edge.prevFace.neighborCount > 2;
		}

		public static bool CanPivotEdgeForward(Topology.VertexEdge edge)
		{
			// After pivoting, the edge's old far vertex and next face will both have their
			// neighbor counts reduced by one.  Neither of these counts can fall below 2.
			return edge.farVertex.neighborCount > 2 && edge.nextFace.neighborCount > 2;
		}

		public static void PivotEdgeBackward(Topology.VertexEdge edge)
		{
			if (!CanPivotEdgeBackward(edge)) throw new InvalidOperationException("Cannot pivot a vertex edge backward when either its far vertex or previous face has only two neighbors.");
			PivotEdgeBackwardUnchecked(edge);
		}

		public static void PivotEdgeBackward(Topology.VertexEdge edge, IEdgeAttribute<EdgeWrap> edgeWrap)
		{
			if (!CanPivotEdgeBackward(edge)) throw new InvalidOperationException("Cannot pivot a vertex edge backward when either its far vertex or previous face has only two neighbors.");
			PivotEdgeBackwardUnchecked(edge, edgeWrap);
		}

		public static void PivotEdgeForward(Topology.VertexEdge edge)
		{
			if (!CanPivotEdgeForward(edge)) throw new InvalidOperationException("Cannot pivot a vertex edge forward when either its far vertex or next face has only two neighbors.");
			PivotEdgeForwardUnchecked(edge);
		}

		public static void PivotEdgeForward(Topology.VertexEdge edge, IEdgeAttribute<EdgeWrap> edgeWrap)
		{
			if (!CanPivotEdgeForward(edge)) throw new InvalidOperationException("Cannot pivot a vertex edge forward when either its far vertex or next face has only two neighbors.");
			PivotEdgeForwardUnchecked(edge, edgeWrap);
		}

		public static bool CanSpinEdgeBackward(Topology.VertexEdge edge)
		{
			// After spinning, the edge's old near and far vertices will both have their
			// neighbor counts reduced by one.  Neither of these counts can fall below 2.
			// The previous and next faces must also have more than 2 neighbors, or else
			// things get really weird.
			// Note that all face neighbor counts will remain stable.
			return edge.farVertex.neighborCount > 2 && edge.nearVertex.neighborCount > 2 && edge.prevFace.neighborCount > 2 && edge.nextFace.neighborCount > 2;
		}

		public static bool CanSpinEdgeForward(Topology.VertexEdge edge)
		{
			// After spinning, the edge's old near and far vertices will both have their
			// neighbor counts reduced by one.  Neither of these counts can fall below 2.
			// The previous and next faces must also have more than 2 neighbors, or else
			// things get really weird.
			// Note that all face neighbor counts will remain stable.
			return edge.farVertex.neighborCount > 2 && edge.nearVertex.neighborCount > 2 && edge.prevFace.neighborCount > 2 && edge.nextFace.neighborCount > 2;
		}

		public static void SpinEdgeBackward(Topology.VertexEdge edge)
		{
			if (!CanSpinEdgeBackward(edge)) throw new InvalidOperationException("Cannot spin a vertex edge backward when either its far vertex or near vertex has only two neighbors.");
			PivotEdgeBackwardUnchecked(edge);
			PivotEdgeBackwardUnchecked(edge.twin);
		}

		public static void SpinEdgeBackward(Topology.VertexEdge edge, IEdgeAttribute<EdgeWrap> edgeWrap)
		{
			if (!CanSpinEdgeBackward(edge)) throw new InvalidOperationException("Cannot spin a vertex edge backward when either its far vertex or near vertex has only two neighbors.");
			PivotEdgeBackwardUnchecked(edge, edgeWrap);
			PivotEdgeBackwardUnchecked(edge.twin, edgeWrap);
		}

		public static void SpinEdgeForward(Topology.VertexEdge edge)
		{
			if (!CanSpinEdgeForward(edge)) throw new InvalidOperationException("Cannot spin a vertex edge forward when either its far vertex or near vertex has only two neighbors.");
			PivotEdgeForwardUnchecked(edge);
			PivotEdgeForwardUnchecked(edge.twin);
		}

		public static void SpinEdgeForward(Topology.VertexEdge edge, IEdgeAttribute<EdgeWrap> edgeWrap)
		{
			if (!CanSpinEdgeForward(edge)) throw new InvalidOperationException("Cannot spin a vertex edge forward when either its far vertex or near vertex has only two neighbors.");
			PivotEdgeForwardUnchecked(edge, edgeWrap);
			PivotEdgeForwardUnchecked(edge.twin, edgeWrap);
		}

		public static bool CanPivotEdgeBackward(Topology.FaceEdge edge)
		{
			// After pivoting, the edge's old far face and prev vertex will both have their
			// neighbor counts reduced by one.  Neither of these counts can fall below 2.
			return edge.farFace.neighborCount > 2 && edge.prevVertex.neighborCount > 2;
		}

		public static bool CanPivotEdgeForward(Topology.FaceEdge edge)
		{
			// After pivoting, the edge's old far face and next vertex will both have their
			// neighbor counts reduced by one.  Neither of these counts can fall below 2.
			return edge.farFace.neighborCount > 2 && edge.nextVertex.neighborCount > 2;
		}

		public static void PivotEdgeBackward(Topology.FaceEdge edge)
		{
			if (!CanPivotEdgeBackward(edge)) throw new InvalidOperationException("Cannot pivot a face edge backward when either its far face or previous vertex has only two neighbors.");
			PivotEdgeBackwardUnchecked(edge);
		}

		public static void PivotEdgeBackward(Topology.FaceEdge edge, IEdgeAttribute<EdgeWrap> edgeWrap)
		{
			if (!CanPivotEdgeBackward(edge)) throw new InvalidOperationException("Cannot pivot a face edge backward when either its far face or previous vertex has only two neighbors.");
			PivotEdgeBackwardUnchecked(edge, edgeWrap);
		}

		public static void PivotEdgeForward(Topology.FaceEdge edge)
		{
			if (!CanPivotEdgeForward(edge)) throw new InvalidOperationException("Cannot pivot a face edge backward when either its far face or next vertex has only two neighbors.");
			PivotEdgeForwardUnchecked(edge);
		}

		public static void PivotEdgeForward(Topology.FaceEdge edge, IEdgeAttribute<EdgeWrap> edgeWrap)
		{
			if (!CanPivotEdgeForward(edge)) throw new InvalidOperationException("Cannot pivot a face edge backward when either its far face or next vertex has only two neighbors.");
			PivotEdgeForwardUnchecked(edge, edgeWrap);
		}

		public static bool CanSpinEdgeBackward(Topology.FaceEdge edge)
		{
			// After spinning, the edge's old near and far faces will both have their
			// neighbor counts reduced by one.  Neither of these counts can fall below 2.
			// The previous and next vertices must also have more than 2 neighbors, or
			// else things get really weird.
			// Note that all vertex neighbor counts will remain stable.
			return edge.farFace.neighborCount > 2 && edge.nearFace.neighborCount > 2 && edge.prevVertex.neighborCount > 2 && edge.nextVertex.neighborCount > 2;
		}

		public static bool CanSpinEdgeForward(Topology.FaceEdge edge)
		{
			// After spinning, the edge's old near and far faces will both have their
			// neighbor counts reduced by one.  Neither of these counts can fall below 2.
			// The previous and next vertices must also have more than 2 neighbors, or
			// else things get really weird.
			// Note that all vertex neighbor counts will remain stable.
			return edge.farFace.neighborCount > 2 && edge.nearFace.neighborCount > 2 && edge.prevVertex.neighborCount > 2 && edge.nextVertex.neighborCount > 2;
		}

		public static void SpinEdgeBackward(Topology.FaceEdge edge)
		{
			if (!CanSpinEdgeBackward(edge)) throw new InvalidOperationException("Cannot spin a face edge backward when either its far face or near face has only two neighbors.");
			PivotEdgeBackwardUnchecked(edge);
			PivotEdgeBackwardUnchecked(edge.twin);
		}

		public static void SpinEdgeBackward(Topology.FaceEdge edge, IEdgeAttribute<EdgeWrap> edgeWrap)
		{
			if (!CanSpinEdgeBackward(edge)) throw new InvalidOperationException("Cannot spin a face edge backward when either its far face or near face has only two neighbors.");
			PivotEdgeBackwardUnchecked(edge, edgeWrap);
			PivotEdgeBackwardUnchecked(edge.twin, edgeWrap);
		}

		public static void SpinEdgeForward(Topology.FaceEdge edge)
		{
			if (!CanSpinEdgeForward(edge)) throw new InvalidOperationException("Cannot spin a face edge forward when either its far face or near face has only two neighbors.");
			PivotEdgeForwardUnchecked(edge);
			PivotEdgeForwardUnchecked(edge.twin);
		}

		public static void SpinEdgeForward(Topology.FaceEdge edge, IEdgeAttribute<EdgeWrap> edgeWrap)
		{
			if (!CanSpinEdgeForward(edge)) throw new InvalidOperationException("Cannot spin a face edge forward when either its far face or near face has only two neighbors.");
			PivotEdgeForwardUnchecked(edge, edgeWrap);
			PivotEdgeForwardUnchecked(edge.twin, edgeWrap);
		}

		public static void CheckVerticesForInvalidEdgeCycles(Topology topology)
		{
			foreach (var vertex in topology.vertices)
			{
				var edge = vertex.firstEdge.next;
				for (int i = 1; i < vertex.neighborCount; ++i)
				{
					if (vertex.firstEdge == edge)
						throw new System.InvalidOperationException(string.Format("The cycle of edges around vertex {0} returned back to the first edge earlier than was expected, {1} iterations rather than {2}.", vertex.index, i, vertex.neighborCount));
					edge = edge.next;
				}
				if (vertex.firstEdge != edge)
					throw new System.InvalidOperationException(string.Format("The cycle of edges around vertex {0}  did not return back to the first edge in the {1} iterations expected.", vertex.index, vertex.neighborCount));
			}
		}

		public static void CheckFacesForInvalidEdgeCycles(Topology topology)
		{
			foreach (var face in topology.faces)
			{
				var edge = face.firstEdge.next;
				for (int i = 1; i < face.neighborCount; ++i)
				{
					if (face.firstEdge == edge)
						throw new System.InvalidOperationException(string.Format("The cycle of edges around face {0} returned back to the first edge earlier than was expected, {1} iterations rather than {2}.", face.index, i, face.neighborCount));
					edge = edge.next;
				}
				if (face.firstEdge != edge)
					throw new System.InvalidOperationException(string.Format("The cycle of edges around face {0} did not return back to the first edge in the {1} iterations expected.", face.index, face.neighborCount));
			}
		}
	}
}
