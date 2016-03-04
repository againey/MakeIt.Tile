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

		public static IEdgeAttribute<float> CalculateFaceEdgeInteriorAnglesFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IVertexAttribute<Vector3> vertexPositions)
		{
			return CalculateFaceEdgeInteriorAnglesFromVertexPositions(faceEdges, vertexPositions, new float[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<float> CalculateFaceEdgeInteriorAnglesFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IVertexAttribute<Vector3> vertexPositions, IEdgeAttribute<float> faceAngles)
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

		public static IEdgeAttribute<float> CalculateFaceEdgeArcsFromFaceCentroids(Topology.FaceEdgesIndexer faceEdges, IFaceAttribute<Vector3> faceCentroids)
		{
			return CalculateFaceEdgeArcsFromFaceCentroids(faceEdges, faceCentroids, new float[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<float> CalculateFaceEdgeArcsFromFaceCentroids(Topology.FaceEdgesIndexer faceEdges, IFaceAttribute<Vector3> faceCentroids, IEdgeAttribute<float> faceAngles)
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

		public static IEdgeAttribute<Vector3> CalculateFaceEdgeBisectorsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IVertexAttribute<Vector3> vertexPositions)
		{
			return CalculateFaceEdgeBisectorsFromVertexPositions(faceEdges, vertexPositions, new Vector3[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector3> CalculateFaceEdgeBisectorsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IVertexAttribute<Vector3> vertexPositions, IEdgeAttribute<Vector3> bisectionVectors)
		{
			foreach (var edge in faceEdges)
			{
				var p0 = vertexPositions[edge.prev];
				var p1 = vertexPositions[edge];
				var p2 = vertexPositions[edge.next];
				var v1to0 = (p0 - p1).normalized;
				var v1to2 = (p2 - p1).normalized;
				bisectionVectors[edge] = (v1to0 + v1to2).normalized;
			}

			return bisectionVectors;
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

		#region CalculateUVs...(...)

		#region CalculateGlobalUVs...(..)

		public static IEdgeAttribute<Vector2> CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IVertexAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, Vector3.zero, uAxis, vAxis, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IVertexAttribute<Vector3> vertexPositions, Vector3 origin, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, origin, uAxis, vAxis, new Vector2[faceEdges.Count].AsEdgeAttribute());
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
				uvs[edge] = Vector2.Scale(uvs[edge] - uvMin, uvRangeReciprocal);
			}

			return uvs;
		}

		#endregion

		#region CalculatePerFaceUVs...(...)

		#region CaclulateUniformlyNormalizedUVs...(...)

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, uAxis, vAxis, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, IEdgeAttribute<Vector2> uvs)
		{
			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);

			var faceMinUVs = new Vector2[faces.Count].AsFaceAttribute();
			var faceRangeUVs = new Vector2[faces.Count].AsFaceAttribute();

			FaceAttributeUtility.CalculateFaceEdgeMinAndRangeValues(faces, uvs, faceMinUVs, faceRangeUVs);

			return CalculatePerFacePlanarUniformlyNormalizedUVsFromFaceUVMinAndRange(faces, faceMinUVs, faceRangeUVs, uvs);
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, AspectRatioPreservation aspectRatioPreservation)
		{
			return CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, uAxis, vAxis, aspectRatioPreservation, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> uvs)
		{
			if (aspectRatioPreservation == AspectRatioPreservation.None) return CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, uAxis, vAxis, uvs);

			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);

			var faceMinUVs = new Vector2[faces.Count].AsFaceAttribute();
			var faceRangeUVs = new Vector2[faces.Count].AsFaceAttribute();

			FaceAttributeUtility.CalculateFaceEdgeMinAndRangeValues(faces, uvs, faceMinUVs, faceRangeUVs);

			var targetAspectRatio = Mathf.Sqrt(uAxis.sqrMagnitude / vAxis.sqrMagnitude);
			var aspectRatioPreservationDelegate = AspectRatioUtility.GetMinAndSizeAdjustmentDelegate(aspectRatioPreservation, targetAspectRatio);

			foreach (var face in faces)
			{
				Vector2 uvMin = faceMinUVs[face];
				Vector2 uvRange = faceRangeUVs[face];
				aspectRatioPreservationDelegate(ref uvMin, ref uvRange);
				faceMinUVs[face] = uvMin;
				faceRangeUVs[face] = uvRange;
			}

			return CalculatePerFacePlanarUniformlyNormalizedUVsFromFaceUVMinAndRange(faces, faceMinUVs, faceRangeUVs, uvs);
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, facePositions, uAxis, vAxis, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis, IEdgeAttribute<Vector2> uvs)
		{
			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);

			var faceUVs = new Vector2[faces.Count].AsFaceAttribute();

			FaceAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromFacePositions(faces, facePositions, uAxis, vAxis, faceUVs);

			var faceMinUVs = new Vector2[faces.Count].AsFaceAttribute();
			var faceRangeUVs = new Vector2[faces.Count].AsFaceAttribute();

			FaceAttributeUtility.CalculateCenteredFaceEdgeMinAndRangeValues(faces, uvs, faceUVs, faceMinUVs, faceRangeUVs);

			return CalculatePerFacePlanarUniformlyNormalizedUVsFromFaceUVMinAndRange(faces, faceMinUVs, faceRangeUVs, uvs);
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis, AspectRatioPreservation aspectRatioPreservation)
		{
			return CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, facePositions, uAxis, vAxis, aspectRatioPreservation, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> uvs)
		{
			if (aspectRatioPreservation == AspectRatioPreservation.None) return CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, facePositions, uAxis, vAxis, uvs);

			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);

			var faceUVs = new Vector2[faces.Count].AsFaceAttribute();

			FaceAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromFacePositions(faces, facePositions, uAxis, vAxis, faceUVs);

			var faceMinUVs = new Vector2[faces.Count].AsFaceAttribute();
			var faceRangeUVs = new Vector2[faces.Count].AsFaceAttribute();

			FaceAttributeUtility.CalculateCenteredFaceEdgeMinAndRangeValues(faces, uvs, faceUVs, faceMinUVs, faceRangeUVs);

			var targetAspectRatio = Mathf.Sqrt(uAxis.sqrMagnitude / vAxis.sqrMagnitude);
			var aspectRatioPreservationDelegate = AspectRatioUtility.GetMinAndSizeAdjustmentDelegate(aspectRatioPreservation, targetAspectRatio);

			foreach (var face in faces)
			{
				Vector2 uvMin = faceMinUVs[face];
				Vector2 uvRange = faceRangeUVs[face];
				aspectRatioPreservationDelegate(ref uvMin, ref uvRange);
				faceMinUVs[face] = uvMin;
				faceRangeUVs[face] = uvRange;
			}

			return CalculatePerFacePlanarUniformlyNormalizedUVsFromFaceUVMinAndRange(faces, faceMinUVs, faceRangeUVs, uvs);
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarUniformlyNormalizedUVsFromFaceUVMinAndRange(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IFaceAttribute<Vector2> faceMinUVs, IFaceAttribute<Vector2> faceRangeUVs)
		{
			return CalculatePerFacePlanarUniformlyNormalizedUVsFromFaceUVMinAndRange(faces, faceMinUVs, faceRangeUVs, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarUniformlyNormalizedUVsFromFaceUVMinAndRange(Topology.FacesIndexer faces, IFaceAttribute<Vector2> faceMinUVs, IFaceAttribute<Vector2> faceRangeUVs, IEdgeAttribute<Vector2> uvs)
		{
			var faceMaxUVRange = Vector2.zero;

			foreach (var face in faces)
			{
				faceMaxUVRange = GeometryUtility.AxisAlignedMax(faceMaxUVRange, faceRangeUVs[face]);
			}

			var faceMaxUVExtent = faceMaxUVRange * 0.5f;
			var faceMaxUVRangeReciprocal = new Vector2(1f / faceMaxUVRange.x, 1f / faceMaxUVRange.y);

			foreach (var face in faces)
			{
				var faceOffsetUV = faceMinUVs[face] + faceRangeUVs[face] * 0.5f - faceMaxUVExtent;
				foreach (var edge in face.edges)
				{
					uvs[edge] = Vector2.Scale(uvs[edge] - faceOffsetUV, faceMaxUVRangeReciprocal);
				}
			}

			return uvs;
		}

		#endregion

		#region CalculateVariablyNormalizedUVs...(...)

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, uAxis, vAxis, new Vector2[faceEdges.Count].AsEdgeAttribute());
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
					uvs[edge] = Vector2.Scale(uvs[edge] - uvMin, uvRangeReciprocal);
				}
			}

			return uvs;
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, AspectRatioPreservation aspectRatioPreservation)
		{
			return CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, uAxis, vAxis, aspectRatioPreservation, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> uvs)
		{
			if (aspectRatioPreservation == AspectRatioPreservation.None) return CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, uAxis, vAxis, uvs);

			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);

			var targetAspectRatio = Mathf.Sqrt(uAxis.sqrMagnitude / vAxis.sqrMagnitude);
			var aspectRatioPreservationDelegate = AspectRatioUtility.GetMinAndSizeAdjustmentDelegate(aspectRatioPreservation, targetAspectRatio);

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
				aspectRatioPreservationDelegate(ref uvMin, ref uvRange);
				var uvRangeReciprocal = new Vector2(1f / uvRange.x, 1f / uvRange.y);

				foreach (var edge in face.edges)
				{
					uvs[edge] = Vector2.Scale(uvs[edge] - uvMin, uvRangeReciprocal);
				}
			}

			return uvs;
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, facePositions, uAxis, vAxis, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis, IEdgeAttribute<Vector2> uvs)
		{
			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);

			var faceUVs = new Vector2[faces.Count].AsFaceAttribute();

			FaceAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromFacePositions(faces, facePositions, uAxis, vAxis, faceUVs);

			foreach (var face in faces)
			{
				Vector2 uvMin = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
				Vector2 uvMax = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

				foreach (var edge in face.edges)
				{
					uvMin = GeometryUtility.AxisAlignedMin(uvMin, uvs[edge]);
					uvMax = GeometryUtility.AxisAlignedMax(uvMax, uvs[edge]);
				}

				var center = faceUVs[face];
				var extent = GeometryUtility.AxisAlignedMax(uvMax - center, center - uvMin);
				uvMin = center - extent;
				var uvRange = extent * 2f;
				var uvRangeReciprocal = new Vector2(1f / uvRange.x, 1f / uvRange.y);

				foreach (var edge in face.edges)
				{
					uvs[edge] = Vector2.Scale(uvs[edge] - uvMin, uvRangeReciprocal);
				}
			}

			return uvs;
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis, AspectRatioPreservation aspectRatioPreservation)
		{
			return CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, facePositions, uAxis, vAxis, aspectRatioPreservation, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> uvs)
		{
			if (aspectRatioPreservation == AspectRatioPreservation.None) return CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, facePositions, uAxis, vAxis, uvs);

			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);

			var faceUVs = new Vector2[faces.Count].AsFaceAttribute();

			FaceAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromFacePositions(faces, facePositions, uAxis, vAxis, faceUVs);

			var targetAspectRatio = Mathf.Sqrt(uAxis.sqrMagnitude / vAxis.sqrMagnitude);
			var aspectRatioPreservationDelegate = AspectRatioUtility.GetMinAndSizeAdjustmentDelegate(aspectRatioPreservation, targetAspectRatio);

			foreach (var face in faces)
			{
				Vector2 uvMin = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
				Vector2 uvMax = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

				foreach (var edge in face.edges)
				{
					uvMin = GeometryUtility.AxisAlignedMin(uvMin, uvs[edge]);
					uvMax = GeometryUtility.AxisAlignedMax(uvMax, uvs[edge]);
				}

				var center = faceUVs[face];
				var extent = GeometryUtility.AxisAlignedMax(uvMax - center, center - uvMin);
				uvMin = center - extent;
				var uvRange = extent * 2f;
				aspectRatioPreservationDelegate(ref uvMin, ref uvRange);
				var uvRangeReciprocal = new Vector2(1f / uvRange.x, 1f / uvRange.y);

				foreach (var edge in face.edges)
				{
					uvs[edge] = Vector2.Scale(uvs[edge] - uvMin, uvRangeReciprocal);
				}
			}

			return uvs;
		}

		#endregion

		#endregion

		#endregion

		#endregion
	}
}
