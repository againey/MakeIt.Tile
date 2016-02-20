using UnityEngine;

namespace Experilous.Topological
{
	public class PlanarPositionallyWrappedTopology : Topology
	{
		public PlanarSurface surface;
		public EdgeWrap[] edgeWrap;

		public static PlanarPositionallyWrappedTopology Create(PlanarSurface surface, ushort[] vertexNeighborCounts, int[] vertexFirstEdgeIndices, EdgeData[] edgeData, EdgeWrap[] edgeWrap, ushort[] faceNeighborCounts, int[] faceFirstEdgeIndices)
		{
			return Create(surface, vertexNeighborCounts, vertexFirstEdgeIndices, edgeData, edgeWrap, faceNeighborCounts, faceFirstEdgeIndices, faceNeighborCounts.Length);
		}

		public static PlanarPositionallyWrappedTopology Create(PlanarSurface surface, ushort[] vertexNeighborCounts, int[] vertexFirstEdgeIndices, EdgeData[] edgeData, EdgeWrap[] edgeWrap, ushort[] faceNeighborCounts, int[] faceFirstEdgeIndices, int firstExternalFaceIndex)
		{
			var instance = CreateInstance<PlanarPositionallyWrappedTopology>();
			instance.surface = surface;
			instance.vertexNeighborCounts = vertexNeighborCounts;
			instance.vertexFirstEdgeIndices = vertexFirstEdgeIndices;
			instance.edgeData = edgeData;
			instance.faceNeighborCounts = faceNeighborCounts;
			instance.faceFirstEdgeIndices = faceFirstEdgeIndices;
			instance.firstExternalFaceIndex = firstExternalFaceIndex;
			instance.edgeWrap = edgeWrap;
			return instance;
		}

		public override object Clone()
		{
			var clone = Create(
				surface.Clone() as PlanarSurface,
				vertexNeighborCounts.Clone() as ushort[],
				vertexFirstEdgeIndices.Clone() as int[],
				edgeData.Clone() as EdgeData[],
				edgeWrap.Clone() as EdgeWrap[],
				faceNeighborCounts.Clone() as ushort[],
				faceFirstEdgeIndices.Clone() as int[],
				firstExternalFaceIndex);
			clone.name = name;
			clone.hideFlags = hideFlags;
			return clone;
		}

		public virtual Vector3 TransformVertexPointFromVertex(int edgeIndex, Vector3 point) { return point; }
		public virtual Vector3 TransformEdgePointFromVertex(int edgeIndex, Vector3 point) { return point; }
		public virtual Vector3 TransformFacePointFromVertex(int edgeIndex, Vector3 point) { return point; }
		public virtual Vector3 TransformVertexPointFromEdge(int edgeIndex, Vector3 point) { return point; }
		public virtual Vector3 TransformFacePointFromEdge(int edgeIndex, Vector3 point) { return point; }
		public virtual Vector3 TransformVertexPointFromFace(int edgeIndex, Vector3 point) { return point; }
		public virtual Vector3 TransformEdgePointFromFace(int edgeIndex, Vector3 point) { return point; }
		public virtual Vector3 TransformFacePointFromFace(int edgeIndex, Vector3 point) { return point; }

		public virtual Vector3 InverseTransformVertexPointFromVertex(int edgeIndex, Vector3 point) { return point; }
		public virtual Vector3 InverseTransformEdgePointFromVertex(int edgeIndex, Vector3 point) { return point; }
		public virtual Vector3 InverseTransformFacePointFromVertex(int edgeIndex, Vector3 point) { return point; }
		public virtual Vector3 InverseTransformVertexPointFromEdge(int edgeIndex, Vector3 point) { return point; }
		public virtual Vector3 InverseTransformFacePointFromEdge(int edgeIndex, Vector3 point) { return point; }
		public virtual Vector3 InverseTransformVertexPointFromFace(int edgeIndex, Vector3 point) { return point; }
		public virtual Vector3 InverseTransformEdgePointFromFace(int edgeIndex, Vector3 point) { return point; }
		public virtual Vector3 InverseTransformFacePointFromFace(int edgeIndex, Vector3 point) { return point; }

		public virtual Vector3 TransformVertexDirectionFromVertex(int edgeIndex, Vector3 direction) { return direction; }
		public virtual Vector3 TransformEdgeDirectionFromVertex(int edgeIndex, Vector3 direction) { return direction; }
		public virtual Vector3 TransformFaceDirectionFromVertex(int edgeIndex, Vector3 direction) { return direction; }
		public virtual Vector3 TransformVertexDirectionFromEdge(int edgeIndex, Vector3 direction) { return direction; }
		public virtual Vector3 TransformFaceDirectionFromEdge(int edgeIndex, Vector3 direction) { return direction; }
		public virtual Vector3 TransformVertexDirectionFromFace(int edgeIndex, Vector3 direction) { return direction; }
		public virtual Vector3 TransformEdgeDirectionFromFace(int edgeIndex, Vector3 direction) { return direction; }
		public virtual Vector3 TransformFaceDirectionFromFace(int edgeIndex, Vector3 direction) { return direction; }

		public virtual Vector3 InverseTransformVertexDirectionFromVertex(int edgeIndex, Vector3 direction) { return direction; }
		public virtual Vector3 InverseTransformEdgeDirectionFromVertex(int edgeIndex, Vector3 direction) { return direction; }
		public virtual Vector3 InverseTransformFaceDirectionFromVertex(int edgeIndex, Vector3 direction) { return direction; }
		public virtual Vector3 InverseTransformVertexDirectionFromEdge(int edgeIndex, Vector3 direction) { return direction; }
		public virtual Vector3 InverseTransformFaceDirectionFromEdge(int edgeIndex, Vector3 direction) { return direction; }
		public virtual Vector3 InverseTransformVertexDirectionFromFace(int edgeIndex, Vector3 direction) { return direction; }
		public virtual Vector3 InverseTransformEdgeDirectionFromFace(int edgeIndex, Vector3 direction) { return direction; }
		public virtual Vector3 InverseTransformFaceDirectionFromFace(int edgeIndex, Vector3 direction) { return direction; }

		protected override void PivotVertexEdgeBackwardUnchecked(int edgeIndex, int twinEdgeIndex, int outerEdgeIndex1, int innerEdgeIndex1)
		{
			base.PivotVertexEdgeBackwardUnchecked(edgeIndex, twinEdgeIndex, outerEdgeIndex1, innerEdgeIndex1);

			// Adjust edge wrap information, coallescing multiple edges together as appropriate.
			edgeWrap[edgeIndex] = EdgeWrapUtility.ModifyTargetVertEdgeRelations(edgeWrap[edgeIndex], edgeWrap[innerEdgeIndex1]); // Edge's target vertex changed, according to Inner Edge 1.
			edgeWrap[twinEdgeIndex] = EdgeWrapUtility.ModifySourceVertEdgeRelations(edgeWrap[twinEdgeIndex], edgeWrap[outerEdgeIndex1]); // Twin Edge's source vertex changed, according to Outer Edge 1.
			edgeWrap[innerEdgeIndex1] = EdgeWrapUtility.ModifyTargetFaceEdgeRelations(edgeWrap[innerEdgeIndex1], edgeWrap[twinEdgeIndex]); // Inner Edge 1's target face changed, according to Twin Edge.
			edgeWrap[outerEdgeIndex1] = EdgeWrapUtility.ModifySourceFaceEdgeRelations(edgeWrap[outerEdgeIndex1], edgeWrap[edgeIndex]); // Outer Edge 1's source face changed, according to Edge.
		}

		protected override void PivotVertexEdgeForwardUnchecked(int edgeIndex, int twinEdgeIndex, int outerEdgeIndex1, int innerEdgeIndex1)
		{
			base.PivotVertexEdgeForwardUnchecked(edgeIndex, twinEdgeIndex, outerEdgeIndex1, innerEdgeIndex1);

			// Adjust edge wrap information, coallescing multiple edges together as appropriate.
			edgeWrap[edgeIndex] = EdgeWrapUtility.ModifyTargetVertEdgeRelations(edgeWrap[edgeIndex], edgeWrap[outerEdgeIndex1]); // Edge's target vertex changed, according to Inner Edge 1.
			edgeWrap[twinEdgeIndex] = EdgeWrapUtility.ModifySourceVertEdgeRelations(edgeWrap[twinEdgeIndex], edgeWrap[innerEdgeIndex1]); // Twin Edge's source vertex changed, according to Outer Edge 1.
			edgeWrap[innerEdgeIndex1] = EdgeWrapUtility.ModifyTargetFaceEdgeRelations(edgeWrap[innerEdgeIndex1], edgeWrap[edgeIndex]); // Inner Edge 1's target face changed, according to Twin Edge.
			edgeWrap[outerEdgeIndex1] = EdgeWrapUtility.ModifySourceFaceEdgeRelations(edgeWrap[outerEdgeIndex1], edgeWrap[twinEdgeIndex]); // Outer Edge 1's source face changed, according to Edge.
		}
	}
}
