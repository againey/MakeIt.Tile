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

		public static IEdgeAttribute<Vector2> CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IVertexAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, IEdgeAttribute<Vector2> uvs)
		{
			return CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, Vector3.zero, uAxis, vAxis, uvs);
		}

		public static IEdgeAttribute<Vector2> CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IVertexAttribute<Vector3> vertexPositions, Vector3 origin, Vector3 uAxis, Vector3 vAxis, IEdgeAttribute<Vector2> uvs)
		{
			var normal = Vector3.Cross(vAxis, uAxis).normalized;
			var uPlane = new Plane(origin, uAxis + origin, normal + origin);
			var vPlane = new Plane(origin, normal + origin, vAxis + origin);

			var uAxisNeg = -uAxis;
			var vAxisNeg = -vAxis;

			foreach (var edge in faceEdges)
			{
				var vertexPosition = vertexPositions[edge];

				uvs[edge] = new Vector2(
					GeometryUtility.GetIntersectionParameter(vPlane, new ScaledRay(vertexPosition, uAxisNeg)),
					GeometryUtility.GetIntersectionParameter(uPlane, new ScaledRay(vertexPosition, vAxisNeg)));
			}

			return uvs;
		}

		public static IEdgeAttribute<Vector2> CalculateGlobalPlanarNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IVertexAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculateGlobalPlanarNormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculateGlobalPlanarNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IVertexAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, IEdgeAttribute<Vector2> uvs)
		{
			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);

			Vector2 uvMin = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
			Vector2 uvMax = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

			foreach (var edge in faceEdges)
			{
				uvMin = GeometryUtility.AxisAlignedMin(uvMin, uvs[edge]);
				uvMax = GeometryUtility.AxisAlignedMax(uvMax, uvs[edge]);
			}

			var uvRange = uvMax - uvMin;
			var uvRangeReciprocal = new Vector2(1f / uvRange.x, 1f / uvRange.y);

			foreach (var edge in faceEdges)
			{
				uvs[edge] -= uvMin;
				uvs[edge].Scale(uvRangeReciprocal);
			}

			return uvs;
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarEquallyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, IEdgeAttribute<Vector2> uvs)
		{
			throw new System.NotImplementedException();
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarEquallyNormalizedCeneteredUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis, IEdgeAttribute<Vector2> uvs)
		{
			throw new System.NotImplementedException();
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, IEdgeAttribute<Vector2> uvs)
		{
			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);

			foreach (var face in faces)
			{
				Vector2 uvMin = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
				Vector2 uvMax = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

				foreach (var edge in face.edges)
				{
					uvMin = GeometryUtility.AxisAlignedMin(uvMin, uvs[edge]);
					uvMax = GeometryUtility.AxisAlignedMax(uvMax, uvs[edge]);
				}

				var uvRange = uvMax - uvMin;
				var uvRangeReciprocal = new Vector2(1f / uvRange.x, 1f / uvRange.y);

				foreach (var edge in face.edges)
				{
					uvs[edge] -= uvMin;
					uvs[edge].Scale(uvRangeReciprocal);
				}
			}

			return uvs;
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarVariablyNormalizedCenteredUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis, IEdgeAttribute<Vector2> uvs)
		{
			throw new System.NotImplementedException();
		}

		public static IEdgeAttribute<Vector2> CalculatePerFaceRadialUVsFromEdgeIndices(Topology.FacesIndexer faces, IEdgeAttribute<float> vValues, IEdgeAttribute<Vector2> uvs)
		{
			foreach (var face in faces)
			{
				float index = 0f;
				float neighborCount = face.neighborCount;
				foreach (var edge in face.edges)
				{
					uvs[edge] = new Vector2(index / neighborCount, vValues[edge]);
					++index;
				}
			}

			return uvs;
		}

		public static IEdgeAttribute<Vector2> CalculatePerFaceRadialUVsFromEdgeLengths(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<float> vValues, IEdgeAttribute<float> edgeLengths, IEdgeAttribute<Vector2> uvs)
		{
			foreach (var face in faces)
			{
				float circumference = 0f;
				foreach (var edge in face.edges)
				{
					circumference += edgeLengths[edge];
				}
				float partialCircumference = 0f;
				foreach (var edge in face.edges)
				{
					partialCircumference += edgeLengths[edge];
					uvs[edge] = new Vector2(partialCircumference / circumference, vValues[edge]);
				}
			}

			return uvs;
		}

		public static IEdgeAttribute<Vector2> CalculatePerFaceRadialUVsFromEdgeLengths(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<float> vValues, IEdgeAttribute<float> edgeLengths, IFaceAttribute<float> faceCircumferences, IEdgeAttribute<Vector2> uvs)
		{
			foreach (var face in faces)
			{
				float circumference = faceCircumferences[face];
				float partialCircumference = 0f;
				foreach (var edge in face.edges)
				{
					uvs[edge] = new Vector2(partialCircumference / circumference, vValues[edge]);
					partialCircumference += edgeLengths[edge];
				}
			}

			return uvs;
		}

		public static IEdgeAttribute<Vector2> CalculatePerFaceRadialUVsFromEdgeAngles(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<float> vValues, IEdgeAttribute<float> edgeAngles, IEdgeAttribute<Vector2> uvs)
		{
			foreach (var face in faces)
			{
				float angleSum = 0f;
				foreach (var edge in face.edges)
				{
					uvs[edge] = new Vector2(angleSum, vValues[edge]);
					angleSum += edgeAngles[edge];
				}
			}

			return uvs;
		}

		#endregion
	}
}
