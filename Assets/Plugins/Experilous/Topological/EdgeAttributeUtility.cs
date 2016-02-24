/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using UnityEngine;

namespace Experilous.Topological
{
	public static class EdgeAttributeUtility
	{
		#region T[] Calculate[VertexEdgeAttribute]...(...)

		public static IEdgeAttribute<float> CalculateVertexAnglesFromVertexPositions(Topology.VertexEdgesIndexer vertexEdges, IVertexAttribute<Vector3> vertexPositions)
		{
			return CalculateVertexAnglesFromVertexPositions(vertexEdges, vertexPositions, new float[vertexEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<float> CalculateVertexAnglesFromVertexPositions(Topology.VertexEdgesIndexer vertexEdges, IVertexAttribute<Vector3> vertexPositions, IEdgeAttribute<float> vertexAngles)
		{
			foreach (var vertexEdge in vertexEdges)
			{
				var p0 = vertexPositions[vertexEdge.next.farVertex];
				var p1 = vertexPositions[vertexEdge.nearVertex];
				var p2 = vertexPositions[vertexEdge.farVertex];
				vertexAngles[vertexEdge] = GeometryUtility.AngleBetweenVectors(p1 - p0, p2 - p0);
			}

			return vertexAngles;
		}

		public static IEdgeAttribute<float> CalculateVertexEdgeLengthsFromVertexPositions(Topology.VertexEdgesIndexer vertexEdges, IVertexAttribute<Vector3> vertexPositions)
		{
			return CalculateVertexEdgeLengthsFromVertexPositions(vertexEdges, vertexPositions, new float[vertexEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<float> CalculateVertexEdgeLengthsFromVertexPositions(Topology.VertexEdgesIndexer vertexEdges, IVertexAttribute<Vector3> vertexPositions, IEdgeAttribute<float> vertexEdgeLengths)
		{
			foreach (var vertexEdge in vertexEdges)
			{
				vertexEdgeLengths[vertexEdge] = Vector3.Distance(vertexPositions[vertexEdge.nearVertex], vertexPositions[vertexEdge.farVertex]);
			}

			return vertexEdgeLengths;
		}

		public static IEdgeAttribute<Vector3> CalculateVertexEdgeMidpointsFromVertexPositions(Topology.VertexEdgesIndexer vertexEdges, IVertexAttribute<Vector3> vertexPositions)
		{
			return CalculateVertexEdgeMidpointsFromVertexPositions(vertexEdges, vertexPositions, new Vector3[vertexEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector3> CalculateVertexEdgeMidpointsFromVertexPositions(Topology.VertexEdgesIndexer vertexEdges, IVertexAttribute<Vector3> vertexPositions, IEdgeAttribute<Vector3> vertexEdgeMidpoints)
		{
			foreach (var vertexEdge in vertexEdges)
			{
				vertexEdgeMidpoints[vertexEdge] = (vertexPositions[vertexEdge.nearVertex] + vertexPositions[vertexEdge.farVertex]) * 0.5f;
			}

			return vertexEdgeMidpoints;
		}

		#endregion

		#region T[] Calculate[FaceEdgeAttribute]...(...)

		public static IEdgeAttribute<float> CalculateFaceAnglesFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IVertexAttribute<Vector3> vertexPositions)
		{
			return CalculateFaceAnglesFromVertexPositions(faceEdges, vertexPositions, new float[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<float> CalculateFaceAnglesFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IVertexAttribute<Vector3> vertexPositions, IEdgeAttribute<float> faceAngles)
		{
			foreach (var faceEdge in faceEdges)
			{
				var p0 = vertexPositions[faceEdge.prev.prevVertex];
				var p1 = vertexPositions[faceEdge.prevVertex];
				var p2 = vertexPositions[faceEdge.nextVertex];
				faceAngles[faceEdge] = GeometryUtility.AngleBetweenVectors(p1 - p0, p2 - p0);
			}

			return faceAngles;
		}

		public static IEdgeAttribute<float> CalculateFaceCentroidAnglesFromFaceCentroids(Topology.FaceEdgesIndexer faceEdges, IFaceAttribute<Vector3> faceCentroids)
		{
			return CalculateFaceCentroidAnglesFromFaceCentroids(faceEdges, faceCentroids, new float[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<float> CalculateFaceCentroidAnglesFromFaceCentroids(Topology.FaceEdgesIndexer faceEdges, IFaceAttribute<Vector3> faceCentroids, IEdgeAttribute<float> faceAngles)
		{
			foreach (var faceEdge in faceEdges)
			{
				var p0 = faceCentroids[faceEdge.prev.farFace];
				var p1 = faceCentroids[faceEdge.nearFace];
				var p2 = faceCentroids[faceEdge.farFace];
				faceAngles[faceEdge] = GeometryUtility.AngleBetweenVectors(p0 - p1, p2 - p1);
			}

			return faceAngles;
		}

		public static IEdgeAttribute<float> CalculateSphericalFaceCentroidAnglesFromFaceCentroids(Topology.FaceEdgesIndexer faceEdges, IFaceAttribute<Vector3> faceCentroids)
		{
			return CalculateSphericalFaceCentroidAnglesFromFaceCentroids(faceEdges, faceCentroids, new float[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<float> CalculateSphericalFaceCentroidAnglesFromFaceCentroids(Topology.FaceEdgesIndexer faceEdges, IFaceAttribute<Vector3> faceCentroids, IEdgeAttribute<float> faceAngles)
		{
			foreach (var faceEdge in faceEdges)
			{
				var p0 = faceCentroids[faceEdge.prev.farFace];
				var p1 = faceCentroids[faceEdge.nearFace];
				var p2 = faceCentroids[faceEdge.farFace];
				var v10 = Vector3.Cross(Vector3.Cross(p0 - p1, p1), p1);
				var v12 = Vector3.Cross(Vector3.Cross(p2 - p1, p1), p1);

				faceAngles[faceEdge] = GeometryUtility.AngleBetweenVectors(v10, v12);
			}

			return faceAngles;
		}

		public static IEdgeAttribute<float> CalculateFaceEdgeLengthsFromFaceCentroids(Topology.FaceEdgesIndexer faceEdges, IFaceAttribute<Vector3> faceCentroids, float boundaryLength = float.PositiveInfinity)
		{
			return CalculateFaceEdgeLengthsFromFaceCentroids(faceEdges, faceCentroids, boundaryLength, new float[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<float> CalculateFaceEdgeLengthsFromFaceCentroids(Topology.FaceEdgesIndexer faceEdges, IFaceAttribute<Vector3> faceCentroids, float boundaryLength, IEdgeAttribute<float> faceEdgeLengths)
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

		public static IEdgeAttribute<Vector3> CalculateFaceEdgeMidpointsFromFaceCentroids(Topology.FaceEdgesIndexer faceEdges, IFaceAttribute<Vector3> faceCentroids)
		{
			return CalculateFaceEdgeMidpointsFromFaceCentroids(faceEdges, faceCentroids, new Vector3[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector3> CalculateFaceEdgeMidpointsFromFaceCentroids(Topology.FaceEdgesIndexer faceEdges, IFaceAttribute<Vector3> faceCentroids, IEdgeAttribute<Vector3> faceEdgeMidpoints)
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
