using UnityEngine;
using System;

namespace Experilous.Topological
{
	public partial class Topology : ScriptableObject, ICloneable
	{
		public ushort[] vertexNeighborCounts;
		public int[] vertexFirstEdgeIndices;

		public EdgeData[] edgeData;

		public ushort[] faceNeighborCounts;
		public int[] faceFirstEdgeIndices;

		public int firstExternalFaceIndex;

		public static Topology Create(ushort[] vertexNeighborCounts, int[] vertexFirstEdgeIndices, EdgeData[] edgeData, ushort[] faceNeighborCounts, int[] faceFirstEdgeIndices)
		{
			return Create(vertexNeighborCounts, vertexFirstEdgeIndices, edgeData, faceNeighborCounts, faceFirstEdgeIndices, faceNeighborCounts.Length);
		}

		public static Topology Create(ushort[] vertexNeighborCounts, int[] vertexFirstEdgeIndices, EdgeData[] edgeData, ushort[] faceNeighborCounts, int[] faceFirstEdgeIndices, int firstExternalFaceIndex)
		{
			var instance = CreateInstance<Topology>();
			instance.vertexNeighborCounts = vertexNeighborCounts;
			instance.vertexFirstEdgeIndices = vertexFirstEdgeIndices;
			instance.edgeData = edgeData;
			instance.faceNeighborCounts = faceNeighborCounts;
			instance.faceFirstEdgeIndices = faceFirstEdgeIndices;
			instance.firstExternalFaceIndex = firstExternalFaceIndex;
			return instance;
		}

		public virtual object Clone()
		{
			var clone = Create(
				vertexNeighborCounts.Clone() as ushort[],
				vertexFirstEdgeIndices.Clone() as int[],
				edgeData.Clone() as EdgeData[],
				faceNeighborCounts.Clone() as ushort[],
				faceFirstEdgeIndices.Clone() as int[],
				firstExternalFaceIndex);
			clone.name = name;
			clone.hideFlags = hideFlags;
			return clone;
		}

		public virtual void MakeDual()
		{
			if (firstExternalFaceIndex < faceFirstEdgeIndices.Length) throw new InvalidOperationException("A dual topology cannot be derived from a topology with external faces.");

			Utility.Swap(ref vertexNeighborCounts, ref faceNeighborCounts);
			Utility.Swap(ref vertexFirstEdgeIndices, ref faceFirstEdgeIndices);

			firstExternalFaceIndex = faceFirstEdgeIndices.Length;

			var dualEdgeData = new EdgeData[edgeData.Length];
			for (int i = 0; i < edgeData.Length; ++i)
			{
				// Edges rotate clockwise to point at next faces becoming far vertices, with their
				// side toward far vertices becoming prev faces.
				dualEdgeData[i] = new EdgeData(
					edgeData[i].twin, // twin remains the same
					edgeData[edgeData[edgeData[i].twin].fNext].twin, // vNext becomes twin of vPrev, where vPrev is the fNext of twin
					edgeData[i].vNext, // fNext becomes what vNext had been
					edgeData[edgeData[i].twin].face, // far vertex becomes what had been next face
					edgeData[i].vertex); // prev face becomes what had been far vertex
			}
			edgeData = dualEdgeData;

			// Due to rotations, face data (which had been vertex data) still points to the same edges,
			// but vertex data (which had been face data) is now backwards, pointing to edges which
			// point back at the vertex; this needs to be reversed by setting first edges to their twins.
			for (int i = 0; i < vertexFirstEdgeIndices.Length; ++i)
			{
				vertexFirstEdgeIndices[i] = edgeData[vertexFirstEdgeIndices[i]].twin;
			}
		}

		public virtual Topology GetDualTopology()
		{
			var clone = (Topology)Clone();
			clone.MakeDual();
			return clone;
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

		private void PivotVertexEdgeBackwardUnchecked(int edgeIndex, int twinEdgeIndex, int outerEdgeIndex1, int innerEdgeIndex1)
		{
			var innerEdgeIndex0 = edgeData[outerEdgeIndex1].vNext;
			var outerEdgeIndex0 = edgeData[innerEdgeIndex0].twin;
			var outerEdgeIndex2 = edgeData[edgeIndex].fNext;

			var prevFaceIndex = edgeData[edgeIndex].face;
			var nextFaceIndex = edgeData[twinEdgeIndex].face;
			var oldVertexIndex = edgeData[edgeIndex].vertex;
			var newVertexIndex = edgeData[innerEdgeIndex1].vertex;

			// The edge was pointing at the old vertex, will now point at the new vertex.
			edgeData[edgeIndex].vertex = newVertexIndex;

			// The second inner edge was pointing at the previous face, will now point at the next face.
			edgeData[innerEdgeIndex1].face = prevFaceIndex;

			// Remove twin edge from old vertex linked list by skipping over it.
			edgeData[outerEdgeIndex2].vNext = innerEdgeIndex1;

			// Insert twin edge into new vertex linked list.
			edgeData[outerEdgeIndex1].vNext = twinEdgeIndex;
			edgeData[twinEdgeIndex].vNext = innerEdgeIndex0;

			// Remove second outer edge from previous face linked list by skipping over it.
			edgeData[outerEdgeIndex0].fNext = twinEdgeIndex;

			// Insert second outer edge into the previous face linked list.
			edgeData[edgeIndex].fNext = outerEdgeIndex1;
			edgeData[outerEdgeIndex1].fNext = outerEdgeIndex2;

			// Reroot the vertex and face that just lost edges with edges guaranteed to still belong.
			vertexFirstEdgeIndices[oldVertexIndex] = innerEdgeIndex1;
			faceFirstEdgeIndices[prevFaceIndex] = twinEdgeIndex;

			// Adjust neighbor counts.
			vertexNeighborCounts[oldVertexIndex] -= 1;
			vertexNeighborCounts[newVertexIndex] += 1;
			faceNeighborCounts[prevFaceIndex] -= 1; // Dropping below 0 is undefined behavior; it better not ever happen.
			faceNeighborCounts[nextFaceIndex] +=1; // Surpassing 32767 is undefined behavior; it better not ever happen.

			// Adjust edge wrap information, coallescing multiple edges together as appropriate.
			edgeData[edgeIndex].wrap = EdgeWrapUtility.ModifyTargetVertEdgeRelations(edgeData[edgeIndex].wrap, edgeData[innerEdgeIndex1].wrap); // Edge's target vertex changed, according to Inner Edge 1.
			edgeData[twinEdgeIndex].wrap = EdgeWrapUtility.ModifySourceVertEdgeRelations(edgeData[twinEdgeIndex].wrap, edgeData[outerEdgeIndex1].wrap); // Twin Edge's source vertex changed, according to Outer Edge 1.
			edgeData[innerEdgeIndex1].wrap = EdgeWrapUtility.ModifyTargetFaceEdgeRelations(edgeData[innerEdgeIndex1].wrap, edgeData[twinEdgeIndex].wrap); // Inner Edge 1's target face changed, according to Twin Edge.
			edgeData[outerEdgeIndex1].wrap = EdgeWrapUtility.ModifySourceFaceEdgeRelations(edgeData[outerEdgeIndex1].wrap, edgeData[edgeIndex].wrap); // Outer Edge 1's source face changed, according to Edge.
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

		private void PivotVertexEdgeForwardUnchecked(int edgeIndex, int twinEdgeIndex, int outerEdgeIndex1, int innerEdgeIndex1)
		{
			var innerEdgeIndex0 = edgeData[twinEdgeIndex].vNext;
			var outerEdgeIndex0 = edgeData[innerEdgeIndex0].twin;
			var outerEdgeIndex2 = edgeData[outerEdgeIndex1].fNext;

			var prevFaceIndex = edgeData[edgeIndex].face;
			var nextFaceIndex = edgeData[twinEdgeIndex].face;
			var oldVertexIndex = edgeData[edgeIndex].vertex;
			var newVertexIndex = edgeData[outerEdgeIndex1].vertex;

			// The edge was pointing at the old vertex, will now point at the new vertex.
			edgeData[edgeIndex].vertex = newVertexIndex;

			// The second inner edge was pointing at the next face, will now point at the previous face.
			edgeData[innerEdgeIndex1].face = prevFaceIndex;

			// Remove twin edge from old vertex linked list by skipping over it.
			edgeData[outerEdgeIndex1].vNext = innerEdgeIndex0;

			// Insert twin edge into new vertex linked list.
			edgeData[outerEdgeIndex2].vNext = twinEdgeIndex;
			edgeData[twinEdgeIndex].vNext = innerEdgeIndex1;

			// Remove second outer edge from next face linked list by skipping over it.
			edgeData[edgeIndex].fNext = outerEdgeIndex2;

			// Insert second outer edge into the previous face linked list.
			edgeData[outerEdgeIndex0].fNext = outerEdgeIndex1;
			edgeData[outerEdgeIndex1].fNext = twinEdgeIndex;

			// Reroot the vertex and face that just lost edges with edges guaranteed to still belong.
			vertexFirstEdgeIndices[oldVertexIndex] = outerEdgeIndex1;
			faceFirstEdgeIndices[nextFaceIndex] = edgeIndex;

			// Adjust neighbor counts.
			vertexNeighborCounts[oldVertexIndex] -= 1;
			vertexNeighborCounts[newVertexIndex] += 1;
			faceNeighborCounts[nextFaceIndex] -= 1; // Dropping below 0 is undefined behavior; it better not ever happen.
			faceNeighborCounts[prevFaceIndex] +=1; // Surpassing 32767 is undefined behavior; it better not ever happen.

			// Adjust edge wrap information, coallescing multiple edges together as appropriate.
			edgeData[edgeIndex].wrap = EdgeWrapUtility.ModifyTargetVertEdgeRelations(edgeData[edgeIndex].wrap, edgeData[outerEdgeIndex1].wrap); // Edge's target vertex changed, according to Inner Edge 1.
			edgeData[twinEdgeIndex].wrap = EdgeWrapUtility.ModifySourceVertEdgeRelations(edgeData[twinEdgeIndex].wrap, edgeData[innerEdgeIndex1].wrap); // Twin Edge's source vertex changed, according to Outer Edge 1.
			edgeData[innerEdgeIndex1].wrap = EdgeWrapUtility.ModifyTargetFaceEdgeRelations(edgeData[innerEdgeIndex1].wrap, edgeData[edgeIndex].wrap); // Inner Edge 1's target face changed, according to Twin Edge.
			edgeData[outerEdgeIndex1].wrap = EdgeWrapUtility.ModifySourceFaceEdgeRelations(edgeData[outerEdgeIndex1].wrap, edgeData[twinEdgeIndex].wrap); // Outer Edge 1's source face changed, according to Edge.
		}

		private void PivotEdgeBackwardUnchecked(Topology.VertexEdge edge)
		{
			int edgeIndex = edge.index;
			var twinEdgeIndex = edgeData[edgeIndex].twin;
			var innerEdgeIndex1 = edgeData[twinEdgeIndex].vNext;
			var outerEdgeIndex1 = edgeData[innerEdgeIndex1].twin;
			PivotVertexEdgeBackwardUnchecked(edgeIndex, twinEdgeIndex, outerEdgeIndex1, innerEdgeIndex1);
		}

		private void PivotEdgeForwardUnchecked(Topology.VertexEdge edge)
		{
			int edgeIndex = edge.index;
			int twinEdgeIndex = edgeData[edgeIndex].twin;
			var outerEdgeIndex1 = edgeData[edgeIndex].fNext;
			var innerEdgeIndex1 = edgeData[outerEdgeIndex1].twin;
			PivotVertexEdgeForwardUnchecked(edgeIndex, twinEdgeIndex, outerEdgeIndex1, innerEdgeIndex1);
		}

		// Pivoting around the near face of an edge ultimately results in a simple
		// pivot of an edge around a vertex, except it's a different edge pivoting
		// around a different vertex.  The original face edge becomes the edge
		// along which the vertex edge slides its far vertex.

		private void PivotEdgeBackwardUnchecked(Topology.FaceEdge edge)
		{
			var innerEdgeIndex1 = edge.index;
			var outerEdgeIndex1 = edgeData[innerEdgeIndex1].twin;
			var twinEdgeIndex = edgeData[outerEdgeIndex1].fNext;
			var edgeIndex = edgeData[twinEdgeIndex].twin;
			PivotVertexEdgeBackwardUnchecked(edgeIndex, twinEdgeIndex, outerEdgeIndex1, innerEdgeIndex1);
		}

		private void PivotEdgeForwardUnchecked(Topology.FaceEdge edge)
		{
			var innerEdgeIndex1 = edge.index;
			var outerEdgeIndex1 = edgeData[innerEdgeIndex1].twin;
			var twinEdgeIndex = edgeData[outerEdgeIndex1].vNext;
			var edgeIndex = edgeData[twinEdgeIndex].twin;
			PivotVertexEdgeForwardUnchecked(edgeIndex, twinEdgeIndex, outerEdgeIndex1, innerEdgeIndex1);
		}

		public bool CanPivotEdgeBackward(Topology.VertexEdge edge)
		{
			// After pivoting, the edge's old far vertex and previous face will both have their
			// neighbor counts reduced by one.  Neither of these counts can fall below 2.
			return edge.farVertex.neighborCount > 2 && edge.prevFace.neighborCount > 2;
		}

		public bool CanPivotEdgeForward(Topology.VertexEdge edge)
		{
			// After pivoting, the edge's old far vertex and next face will both have their
			// neighbor counts reduced by one.  Neither of these counts can fall below 2.
			return edge.farVertex.neighborCount > 2 && edge.nextFace.neighborCount > 2;
		}

		public void PivotEdgeBackward(Topology.VertexEdge edge)
		{
			if (!CanPivotEdgeBackward(edge)) throw new InvalidOperationException("Cannot pivot a vertex edge backward when either its far vertex or previous face has only two neighbors.");
			PivotEdgeBackwardUnchecked(edge);
		}

		public void PivotEdgeForward(Topology.VertexEdge edge)
		{
			if (!CanPivotEdgeForward(edge)) throw new InvalidOperationException("Cannot pivot a vertex edge forward when either its far vertex or next face has only two neighbors.");
			PivotEdgeForwardUnchecked(edge);
		}

		public bool CanSpinEdgeBackward(Topology.VertexEdge edge)
		{
			// After spinning, the edge's old near and far vertices will both have their
			// neighbor counts reduced by one.  Neither of these counts can fall below 2.
			// The previous and next faces must also have more than 2 neighbors, or else
			// things get really weird.
			// Note that all face neighbor counts will remain stable.
			return edge.farVertex.neighborCount > 2 && edge.nearVertex.neighborCount > 2 && edge.prevFace.neighborCount > 2 && edge.nextFace.neighborCount > 2;
		}

		public bool CanSpinEdgeForward(Topology.VertexEdge edge)
		{
			// After spinning, the edge's old near and far vertices will both have their
			// neighbor counts reduced by one.  Neither of these counts can fall below 2.
			// The previous and next faces must also have more than 2 neighbors, or else
			// things get really weird.
			// Note that all face neighbor counts will remain stable.
			return edge.farVertex.neighborCount > 2 && edge.nearVertex.neighborCount > 2 && edge.prevFace.neighborCount > 2 && edge.nextFace.neighborCount > 2;
		}

		public void SpinEdgeBackward(Topology.VertexEdge edge)
		{
			if (!CanSpinEdgeBackward(edge)) throw new InvalidOperationException("Cannot spin a vertex edge backward when either its far vertex or near vertex has only two neighbors.");
			PivotEdgeBackwardUnchecked(edge);
			PivotEdgeBackwardUnchecked(edge.twin);
		}

		public void SpinEdgeForward(Topology.VertexEdge edge)
		{
			if (!CanSpinEdgeForward(edge)) throw new InvalidOperationException("Cannot spin a vertex edge forward when either its far vertex or near vertex has only two neighbors.");
			PivotEdgeForwardUnchecked(edge);
			PivotEdgeForwardUnchecked(edge.twin);
		}

		public bool CanPivotEdgeBackward(Topology.FaceEdge edge)
		{
			// After pivoting, the edge's old far face and prev vertex will both have their
			// neighbor counts reduced by one.  Neither of these counts can fall below 2.
			return edge.farFace.neighborCount > 2 && edge.prevVertex.neighborCount > 2;
		}

		public bool CanPivotEdgeForward(Topology.FaceEdge edge)
		{
			// After pivoting, the edge's old far face and next vertex will both have their
			// neighbor counts reduced by one.  Neither of these counts can fall below 2.
			return edge.farFace.neighborCount > 2 && edge.nextVertex.neighborCount > 2;
		}

		public void PivotEdgeBackward(Topology.FaceEdge edge)
		{
			if (!CanPivotEdgeBackward(edge)) throw new InvalidOperationException("Cannot pivot a face edge backward when either its far face or previous vertex has only two neighbors.");
			PivotEdgeBackwardUnchecked(edge);
		}

		public void PivotEdgeForward(Topology.FaceEdge edge)
		{
			if (!CanPivotEdgeForward(edge)) throw new InvalidOperationException("Cannot pivot a face edge backward when either its far face or next vertex has only two neighbors.");
			PivotEdgeForwardUnchecked(edge);
		}

		public bool CanSpinEdgeBackward(Topology.FaceEdge edge)
		{
			// After spinning, the edge's old near and far faces will both have their
			// neighbor counts reduced by one.  Neither of these counts can fall below 2.
			// The previous and next vertices must also have more than 2 neighbors, or
			// else things get really weird.
			// Note that all vertex neighbor counts will remain stable.
			return edge.farFace.neighborCount > 2 && edge.nearFace.neighborCount > 2 && edge.prevVertex.neighborCount > 2 && edge.nextVertex.neighborCount > 2;
		}

		public bool CanSpinEdgeForward(Topology.FaceEdge edge)
		{
			// After spinning, the edge's old near and far faces will both have their
			// neighbor counts reduced by one.  Neither of these counts can fall below 2.
			// The previous and next vertices must also have more than 2 neighbors, or
			// else things get really weird.
			// Note that all vertex neighbor counts will remain stable.
			return edge.farFace.neighborCount > 2 && edge.nearFace.neighborCount > 2 && edge.prevVertex.neighborCount > 2 && edge.nextVertex.neighborCount > 2;
		}

		public void SpinEdgeBackward(Topology.FaceEdge edge)
		{
			if (!CanSpinEdgeBackward(edge)) throw new InvalidOperationException("Cannot spin a face edge backward when either its far face or near face has only two neighbors.");
			PivotEdgeBackwardUnchecked(edge);
			PivotEdgeBackwardUnchecked(edge.twin);
		}

		public void SpinEdgeForward(Topology.FaceEdge edge)
		{
			if (!CanSpinEdgeForward(edge)) throw new InvalidOperationException("Cannot spin a face edge forward when either its far face or near face has only two neighbors.");
			PivotEdgeForwardUnchecked(edge);
			PivotEdgeForwardUnchecked(edge.twin);
		}
	}
}
