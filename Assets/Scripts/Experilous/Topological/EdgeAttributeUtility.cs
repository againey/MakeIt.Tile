using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public static class EdgeAttributeUtility
	{
		#region T[] Calculate[VertexEdgeAttribute]...(...)

		public static IList<float> CalculateVertexAnglesFromVertexPositions(Topology.VertexEdgesIndexer vertexEdges, IList<Vector3> vertexPositions)
		{
			return CalculateVertexAnglesFromVertexPositions(vertexEdges, vertexPositions, new float[vertexEdges.Count]);
		}

		public static IList<float> CalculateVertexAnglesFromVertexPositions(Topology.VertexEdgesIndexer vertexEdges, IList<Vector3> vertexPositions, IList<float> vertexAngles)
		{
			foreach (var vertexEdge in vertexEdges)
			{
				var p0 = vertexPositions[vertexEdge.next.farVertex];
				var p1 = vertexPositions[vertexEdge.nearVertex];
				var p2 = vertexPositions[vertexEdge.farVertex];
				vertexAngles[vertexEdge] = MathUtility.AngleBetweenVectors(p1 - p0, p2 - p0);
			}

			return vertexAngles;
		}

		public static IList<float> CalculateVertexEdgeLengthsFromVertexPositions(Topology.VertexEdgesIndexer vertexEdges, IList<Vector3> vertexPositions)
		{
			return CalculateVertexEdgeLengthsFromVertexPositions(vertexEdges, vertexPositions, new float[vertexEdges.Count]);
		}

		public static IList<float> CalculateVertexEdgeLengthsFromVertexPositions(Topology.VertexEdgesIndexer vertexEdges, IList<Vector3> vertexPositions, IList<float> vertexEdgeLengths)
		{
			foreach (var vertexEdge in vertexEdges)
			{
				vertexEdgeLengths[vertexEdge] = Vector3.Distance(vertexPositions[vertexEdge.nearVertex], vertexPositions[vertexEdge.farVertex]);
			}

			return vertexEdgeLengths;
		}

		public static IList<Vector3> CalculateVertexEdgeMidpointsFromVertexPositions(Topology.VertexEdgesIndexer vertexEdges, IList<Vector3> vertexPositions)
		{
			return CalculateVertexEdgeMidpointsFromVertexPositions(vertexEdges, vertexPositions, new Vector3[vertexEdges.Count]);
		}

		public static IList<Vector3> CalculateVertexEdgeMidpointsFromVertexPositions(Topology.VertexEdgesIndexer vertexEdges, IList<Vector3> vertexPositions, IList<Vector3> vertexEdgeMidpoints)
		{
			foreach (var vertexEdge in vertexEdges)
			{
				vertexEdgeMidpoints[vertexEdge] = (vertexPositions[vertexEdge.nearVertex] + vertexPositions[vertexEdge.farVertex]) * 0.5f;
			}

			return vertexEdgeMidpoints;
		}

		#endregion

		#region T[] Calculate[FaceEdgeAttribute]...(...)

		public static IList<float> CalculateFaceAnglesFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IList<Vector3> vertexPositions)
		{
			return CalculateFaceAnglesFromVertexPositions(faceEdges, vertexPositions, new float[faceEdges.Count]);
		}

		public static IList<float> CalculateFaceAnglesFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IList<Vector3> vertexPositions, IList<float> faceAngles)
		{
			foreach (var faceEdge in faceEdges)
			{
				var p0 = vertexPositions[faceEdge.prev.prevVertex];
				var p1 = vertexPositions[faceEdge.prevVertex];
				var p2 = vertexPositions[faceEdge.nextVertex];
				faceAngles[faceEdge] = MathUtility.AngleBetweenVectors(p1 - p0, p2 - p0);
			}

			return faceAngles;
		}

		public static IList<float> CalculateFaceCentroidAnglesFromFaceCentroids(Topology.FaceEdgesIndexer faceEdges, IList<Vector3> faceCentroids)
		{
			return CalculateFaceCentroidAnglesFromFaceCentroids(faceEdges, faceCentroids, new float[faceEdges.Count]);
		}

		public static IList<float> CalculateFaceCentroidAnglesFromFaceCentroids(Topology.FaceEdgesIndexer faceEdges, IList<Vector3> faceCentroids, IList<float> faceAngles)
		{
			foreach (var faceEdge in faceEdges)
			{
				var p0 = faceCentroids[faceEdge.farFace];
				var p1 = faceCentroids[faceEdge.nearFace];
				var p2 = faceCentroids[faceEdge.prev.farFace];
				faceAngles[faceEdge] = MathUtility.AngleBetweenVectors(p1 - p0, p2 - p0);
			}

			return faceAngles;
		}

		public static IList<float> CalculateFaceEdgeLengthsFromFaceCentroids(Topology.FaceEdgesIndexer faceEdges, IList<Vector3> faceCentroids, float boundaryLength = float.PositiveInfinity)
		{
			return CalculateFaceEdgeLengthsFromFaceCentroids(faceEdges, faceCentroids, boundaryLength, new float[faceEdges.Count]);
		}

		public static IList<float> CalculateFaceEdgeLengthsFromFaceCentroids(Topology.FaceEdgesIndexer faceEdges, IList<Vector3> faceCentroids, float boundaryLength, IList<float> faceEdgeLengths)
		{
			foreach (var faceEdge in faceEdges)
			{
				if (!faceEdge.isBoundary)
				{
					faceEdgeLengths[faceEdge] = Vector3.Distance(faceCentroids[faceEdge.nearFace], faceCentroids[faceEdge.farFace]);
				}
				else
				{
					faceEdgeLengths[faceEdge] = boundaryLength;
				}
			}

			return faceEdgeLengths;
		}

		public static IList<Vector3> CalculateFaceEdgeMidpointsFromFaceCentroids(Topology.FaceEdgesIndexer faceEdges, IList<Vector3> faceCentroids)
		{
			return CalculateFaceEdgeMidpointsFromFaceCentroids(faceEdges, faceCentroids, new Vector3[faceEdges.Count]);
		}

		public static IList<Vector3> CalculateFaceEdgeMidpointsFromFaceCentroids(Topology.FaceEdgesIndexer faceEdges, IList<Vector3> faceCentroids, IList<Vector3> faceEdgeMidpoints)
		{
			foreach (var faceEdge in faceEdges)
			{
				if (faceEdge.isInnerBoundary)
				{
					faceEdgeMidpoints[faceEdge] = faceCentroids[faceEdge.farFace];
				}
				else if (faceEdge.isOuterBoundary)
				{
					faceEdgeMidpoints[faceEdge] = faceCentroids[faceEdge.nearFace];
				}
				else
				{
					faceEdgeMidpoints[faceEdge] = (faceCentroids[faceEdge.nearFace] + faceCentroids[faceEdge.farFace]) * 0.5f;
				}
			}

			return faceEdgeMidpoints;
		}

		#endregion
	}
}
