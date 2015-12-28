using UnityEngine;
using System;

namespace Experilous.Topological
{
	[Serializable]
	public partial class Topology
	{
		public Topology()
		{
		}

		public Topology(Topology original)
		{
			CloneFields(original);
		}

		protected void CloneFields(Topology original)
		{
			_vertexNeighborCounts = original._vertexNeighborCounts.Clone() as ushort[];
			_vertexFirstEdgeIndices = original._vertexFirstEdgeIndices.Clone() as int[];
			_edgeData = original._edgeData.Clone() as EdgeData[];
			_faceNeighborCounts = original._faceNeighborCounts.Clone() as ushort[];
			_faceFirstEdgeIndices = original._faceFirstEdgeIndices.Clone() as int[];
			_firstExternalEdgeIndex = original._firstExternalEdgeIndex;
			_firstExternalFaceIndex = original._firstExternalFaceIndex;
		}

		public Topology Clone()
		{
			return new Topology(this);
		}

		public virtual void MakeDual()
		{
			if (_firstExternalFaceIndex < _faceFirstEdgeIndices.Length) throw new InvalidOperationException("A dual topology cannot be derived from a topology with external faces.");

			Utility.Swap(ref _vertexNeighborCounts, ref _faceNeighborCounts);
			Utility.Swap(ref _vertexFirstEdgeIndices, ref _faceFirstEdgeIndices);

			_firstExternalFaceIndex = _faceFirstEdgeIndices.Length;

			var dualEdgeData = new EdgeData[_edgeData.Length];
			for (int i = 0; i < _edgeData.Length; ++i)
			{
				// Edges rotate clockwise to point at next faces becoming far vertices, with their
				// side toward far vertices becoming prev faces.
				dualEdgeData[i] = new EdgeData(
					_edgeData[i]._twin, // twin remains the same
					_edgeData[_edgeData[_edgeData[i]._twin]._fNext]._twin, // vNext becomes twin of vPrev, where vPrev is the fNext of twin
					_edgeData[i]._vNext, // fNext becomes what vNext had been
					_edgeData[_edgeData[i]._twin]._face, // far vertex becomes what had been next face
					_edgeData[i]._vertex); // prev face becomes what had been far vertex
			}
			_edgeData = dualEdgeData;

			// Due to rotations, face data (which had been vertex data) still points to the same edges,
			// but vertex data (which had been face data) is now backwards, pointing to edges which
			// point back at the vertex; this needs to be reversed by setting first edges to their twins.
			for (int i = 0; i < _vertexFirstEdgeIndices.Length; ++i)
			{
				_vertexFirstEdgeIndices[i] = _edgeData[_vertexFirstEdgeIndices[i]]._twin;
			}
		}

		public virtual Topology GetDualTopology()
		{
			var clone = Clone();
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

		protected void PivotVertexEdgeBackwardUnchecked(int edgeIndex, int twinEdgeIndex, int outerEdgeIndex1, int innerEdgeIndex1)
		{
			var innerEdgeIndex0 = _edgeData[outerEdgeIndex1]._vNext;
			var outerEdgeIndex0 = _edgeData[innerEdgeIndex0]._twin;
			var outerEdgeIndex2 = _edgeData[edgeIndex]._fNext;

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
			if (oldVertexIndex != -1) _vertexFirstEdgeIndices[oldVertexIndex] = innerEdgeIndex1;
			if (prevFaceIndex != -1) _faceFirstEdgeIndices[prevFaceIndex] = twinEdgeIndex;

			// Adjust neighbor counts.
			if (oldVertexIndex != -1) _vertexNeighborCounts[oldVertexIndex] -= 1;
			if (newVertexIndex != -1) _vertexNeighborCounts[newVertexIndex] += 1;
			if (prevFaceIndex != -1) _faceNeighborCounts[prevFaceIndex] -= 1; // Dropping below 0 is undefined behavior; it better not ever happen.
			if (nextFaceIndex != -1) _faceNeighborCounts[nextFaceIndex] +=1; // Surpassing 32767 is undefined behavior; it better not ever happen.
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

		protected void PivotVertexEdgeForwardUnchecked(int edgeIndex, int twinEdgeIndex, int outerEdgeIndex1, int innerEdgeIndex1)
		{
			var innerEdgeIndex0 = _edgeData[twinEdgeIndex]._vNext;
			var outerEdgeIndex0 = _edgeData[innerEdgeIndex0]._twin;
			var outerEdgeIndex2 = _edgeData[outerEdgeIndex1]._fNext;

			var prevFaceIndex = _edgeData[edgeIndex]._face;
			var nextFaceIndex = _edgeData[twinEdgeIndex]._face;
			var oldVertexIndex = _edgeData[edgeIndex]._vertex;
			var newVertexIndex = _edgeData[outerEdgeIndex1]._vertex;

			// The edge was pointing at the old vertex, will now point at the new vertex.
			_edgeData[edgeIndex]._vertex = newVertexIndex;

			// The second inner edge was pointing at the next face, will now point at the previous face.
			_edgeData[innerEdgeIndex1]._face = prevFaceIndex;

			// Remove twin edge from old vertex linked list by skipping over it.
			_edgeData[outerEdgeIndex1]._vNext = innerEdgeIndex0;

			// Insert twin edge into new vertex linked list.
			_edgeData[outerEdgeIndex2]._vNext = twinEdgeIndex;
			_edgeData[twinEdgeIndex]._vNext = innerEdgeIndex1;

			// Remove second outer edge from next face linked list by skipping over it.
			_edgeData[edgeIndex]._fNext = outerEdgeIndex2;

			// Insert second outer edge into the previous face linked list.
			_edgeData[outerEdgeIndex0]._fNext = outerEdgeIndex1;
			_edgeData[outerEdgeIndex1]._fNext = twinEdgeIndex;

			// Reroot the vertex and face that just lost edges with edges guaranteed to still belong.
			if (oldVertexIndex != -1) _vertexFirstEdgeIndices[oldVertexIndex] = outerEdgeIndex1;
			if (nextFaceIndex != -1) _faceFirstEdgeIndices[nextFaceIndex] = edgeIndex;

			// Adjust neighbor counts.
			if (oldVertexIndex != -1) _vertexNeighborCounts[oldVertexIndex] -= 1;
			if (newVertexIndex != -1) _vertexNeighborCounts[newVertexIndex] += 1;
			if (nextFaceIndex != -1) _faceNeighborCounts[nextFaceIndex] -= 1; // Dropping below 0 is undefined behavior; it better not ever happen.
			if (prevFaceIndex != -1) _faceNeighborCounts[prevFaceIndex] +=1; // Surpassing 32767 is undefined behavior; it better not ever happen.
		}

		protected void PivotEdgeBackwardUnchecked(VertexEdge edge)
		{
			
			int edgeIndex = edge.index;
			var twinEdgeIndex = _edgeData[edgeIndex]._twin;
			var innerEdgeIndex1 = _edgeData[twinEdgeIndex]._vNext;
			var outerEdgeIndex1 = _edgeData[innerEdgeIndex1]._twin;
			PivotVertexEdgeBackwardUnchecked(edgeIndex, twinEdgeIndex, outerEdgeIndex1, innerEdgeIndex1);
		}

		protected void PivotEdgeForwardUnchecked(VertexEdge edge)
		{
			int edgeIndex = edge.index;
			int twinEdgeIndex = _edgeData[edgeIndex]._twin;
			var outerEdgeIndex1 = _edgeData[edgeIndex]._fNext;
			var innerEdgeIndex1 = _edgeData[outerEdgeIndex1]._twin;
			PivotVertexEdgeForwardUnchecked(edgeIndex, twinEdgeIndex, outerEdgeIndex1, innerEdgeIndex1);
		}

		// Pivoting around the near face of an edge ultimately results in a simple
		// pivot of an edge around a vertex, except it's a different edge pivoting
		// around a different vertex.  The original face edge becomes the edge
		// along which the vertex edge slides its far vertex.

		protected void PivotEdgeBackwardUnchecked(FaceEdge edge)
		{
			var innerEdgeIndex1 = edge.index;
			var outerEdgeIndex1 = _edgeData[innerEdgeIndex1]._twin;
			var twinEdgeIndex = _edgeData[outerEdgeIndex1]._fNext;
			var edgeIndex = _edgeData[twinEdgeIndex]._twin;
			PivotVertexEdgeBackwardUnchecked(edgeIndex, twinEdgeIndex, outerEdgeIndex1, innerEdgeIndex1);
		}

		protected void PivotEdgeForwardUnchecked(FaceEdge edge)
		{
			var innerEdgeIndex1 = edge.index;
			var outerEdgeIndex1 = _edgeData[innerEdgeIndex1]._twin;
			var twinEdgeIndex = _edgeData[outerEdgeIndex1]._vNext;
			var edgeIndex = _edgeData[twinEdgeIndex]._twin;
			PivotVertexEdgeForwardUnchecked(edgeIndex, twinEdgeIndex, outerEdgeIndex1, innerEdgeIndex1);
		}

		public bool CanPivotEdgeBackward(VertexEdge edge)
		{
			// After pivoting, the edge's old far vertex and previous face will both have their
			// neighbor counts reduced by one.  Neither of these counts can fall below 2.
			return edge.farVertex.neighborCount > 2 && edge.prevFace.neighborCount > 2;
		}

		public bool CanPivotEdgeForward(VertexEdge edge)
		{
			// After pivoting, the edge's old far vertex and next face will both have their
			// neighbor counts reduced by one.  Neither of these counts can fall below 2.
			return edge.farVertex.neighborCount > 2 && edge.nextFace.neighborCount > 2;
		}

		public void PivotEdgeBackward(VertexEdge edge)
		{
			if (!CanPivotEdgeBackward(edge)) throw new InvalidOperationException("Cannot pivot a vertex edge backward when either its far vertex or previous face has only two neighbors.");
			PivotEdgeBackwardUnchecked(edge);
		}

		public void PivotEdgeForward(VertexEdge edge)
		{
			if (!CanPivotEdgeForward(edge)) throw new InvalidOperationException("Cannot pivot a vertex edge forward when either its far vertex or next face has only two neighbors.");
			PivotEdgeForwardUnchecked(edge);
		}

		public bool CanSpinEdgeBackward(VertexEdge edge)
		{
			// After spinning, the edge's old near and far vertices will both have their
			// neighbor counts reduced by one.  Neither of these counts can fall below 2.
			// The previous and next faces must also have more than 2 neighbors, or else
			// things get really weird.
			// Note that all face neighbor counts will remain stable.
			return edge.farVertex.neighborCount > 2 && edge.nearVertex.neighborCount > 2 && edge.prevFace.neighborCount > 2 && edge.nextFace.neighborCount > 2;
		}

		public bool CanSpinEdgeForward(VertexEdge edge)
		{
			// After spinning, the edge's old near and far vertices will both have their
			// neighbor counts reduced by one.  Neither of these counts can fall below 2.
			// The previous and next faces must also have more than 2 neighbors, or else
			// things get really weird.
			// Note that all face neighbor counts will remain stable.
			return edge.farVertex.neighborCount > 2 && edge.nearVertex.neighborCount > 2 && edge.prevFace.neighborCount > 2 && edge.nextFace.neighborCount > 2;
		}

		public void SpinEdgeBackward(VertexEdge edge)
		{
			if (!CanSpinEdgeBackward(edge)) throw new InvalidOperationException("Cannot spin a vertex edge backward when either its far vertex or near vertex has only two neighbors.");
			PivotEdgeBackwardUnchecked(edge);
			PivotEdgeBackwardUnchecked(edge.twin);
		}

		public void SpinEdgeForward(VertexEdge edge)
		{
			if (!CanSpinEdgeForward(edge)) throw new InvalidOperationException("Cannot spin a vertex edge forward when either its far vertex or near vertex has only two neighbors.");
			PivotEdgeForwardUnchecked(edge);
			PivotEdgeForwardUnchecked(edge.twin);
		}

		public bool CanPivotEdgeBackward(FaceEdge edge)
		{
			// After pivoting, the edge's old far face and prev vertex will both have their
			// neighbor counts reduced by one.  Neither of these counts can fall below 2.
			return edge.farFace.neighborCount > 2 && edge.prevVertex.neighborCount > 2;
		}

		public bool CanPivotEdgeForward(FaceEdge edge)
		{
			// After pivoting, the edge's old far face and next vertex will both have their
			// neighbor counts reduced by one.  Neither of these counts can fall below 2.
			return edge.farFace.neighborCount > 2 && edge.nextVertex.neighborCount > 2;
		}

		public void PivotEdgeBackward(FaceEdge edge)
		{
			if (!CanPivotEdgeBackward(edge)) throw new InvalidOperationException("Cannot pivot a face edge backward when either its far face or previous vertex has only two neighbors.");
			PivotEdgeBackwardUnchecked(edge);
		}

		public void PivotEdgeForward(FaceEdge edge)
		{
			if (!CanPivotEdgeForward(edge)) throw new InvalidOperationException("Cannot pivot a face edge backward when either its far face or next vertex has only two neighbors.");
			PivotEdgeForwardUnchecked(edge);
		}

		public bool CanSpinEdgeBackward(FaceEdge edge)
		{
			// After spinning, the edge's old near and far faces will both have their
			// neighbor counts reduced by one.  Neither of these counts can fall below 2.
			// The previous and next vertices must also have more than 2 neighbors, or
			// else things get really weird.
			// Note that all vertex neighbor counts will remain stable.
			return edge.farFace.neighborCount > 2 && edge.nearFace.neighborCount > 2 && edge.prevVertex.neighborCount > 2 && edge.nextVertex.neighborCount > 2;
		}

		public bool CanSpinEdgeForward(FaceEdge edge)
		{
			// After spinning, the edge's old near and far faces will both have their
			// neighbor counts reduced by one.  Neither of these counts can fall below 2.
			// The previous and next vertices must also have more than 2 neighbors, or
			// else things get really weird.
			// Note that all vertex neighbor counts will remain stable.
			return edge.farFace.neighborCount > 2 && edge.nearFace.neighborCount > 2 && edge.prevVertex.neighborCount > 2 && edge.nextVertex.neighborCount > 2;
		}

		public void SpinEdgeBackward(FaceEdge edge)
		{
			if (!CanSpinEdgeBackward(edge)) throw new InvalidOperationException("Cannot spin a face edge backward when either its far face or near face has only two neighbors.");
			PivotEdgeBackwardUnchecked(edge);
			PivotEdgeBackwardUnchecked(edge.twin);
		}

		public void SpinEdgeForward(FaceEdge edge)
		{
			if (!CanSpinEdgeForward(edge)) throw new InvalidOperationException("Cannot spin a face edge forward when either its far face or near face has only two neighbors.");
			PivotEdgeForwardUnchecked(edge);
			PivotEdgeForwardUnchecked(edge.twin);
		}

		public void CheckVerticesForInvalidEdgeCycles()
		{
			foreach (var vertex in vertices)
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

		public void CheckFacesForInvalidEdgeCycles()
		{
			foreach (var face in faces)
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
