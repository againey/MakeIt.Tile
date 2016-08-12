/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using Experilous.Numerics;

namespace Experilous.MakeIt.Tile
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
				vertexAngles[vertexEdge] = Geometry.AngleBetweenVectors(p1 - p0, p2 - p0);
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
				faceAngles[faceEdge] = Geometry.AngleBetweenVectors(p1 - p0, p2 - p0);
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
				faceAngles[faceEdge] = Geometry.AngleBetweenVectors(p0 - p1, p2 - p1);
			}

			return faceAngles;
		}

		public static IEdgeAttribute<Vector3> CalculateFaceEdgeBisectorsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IEdgeAttribute<Vector3> vertexPositions)
		{
			return CalculateFaceEdgeBisectorsFromVertexPositions(faceEdges, vertexPositions, new Vector3[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector3> CalculateFaceEdgeBisectorsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IEdgeAttribute<Vector3> vertexPositions, IEdgeAttribute<Vector3> bisectors)
		{
			foreach (var edge in faceEdges)
			{
				var p0 = vertexPositions[edge.prev];
				var p1 = vertexPositions[edge];
				var p2 = vertexPositions[edge.next];
				var v1to0 = (p0 - p1).normalized;
				var v1to2 = (p2 - p1).normalized;
				var sum = v1to0 + v1to2;
				bisectors[edge] = sum.normalized;
			}

			return bisectors;
		}

		public static IEdgeAttribute<Vector3> CalculateFaceEdgeBisectorsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions)
		{
			return CalculateFaceEdgeBisectorsFromVertexPositions(faceEdges, faces, vertexPositions, facePositions, new Vector3[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector3> CalculateFaceEdgeBisectorsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, IEdgeAttribute<Vector3> bisectors)
		{
			foreach (var face in faces)
			{
				var center = facePositions[face];
				foreach (var edge in face.edges)
				{
					var p0 = vertexPositions[edge.prev];
					var p1 = vertexPositions[edge];
					var p2 = vertexPositions[edge.next];
					var v1to0 = (p0 - p1).normalized;
					var v1to2 = (p2 - p1).normalized;
					var v1toCenter = center - p1;
					var sum = v1to0 + v1to2;
					if (sum != Vector3.zero)
					{
						if (Vector3.Dot(sum, v1toCenter) >= 0f)
						{
							bisectors[edge] = sum.normalized;
						}
						else
						{
							bisectors[edge] = -sum.normalized;
						}
					}
					else
					{
						bisectors[edge] = v1toCenter.normalized;
					}
				}
			}

			return bisectors;
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

				faceAngles[faceEdge] = Geometry.AngleBetweenVectors(v10, v12);
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

		public static void CalculateFaceEdgeCornerKitePositionsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IEdgeAttribute<Vector3> vertexPositions, float kiteLongEdgeLength, out IEdgeAttribute<Vector3> leadingPositions, out IEdgeAttribute<Vector3> followingPositions, out IEdgeAttribute<Vector3> interiorPositions)
		{
			leadingPositions = new Vector3[faceEdges.Count].AsEdgeAttribute();
			followingPositions = new Vector3[faceEdges.Count].AsEdgeAttribute();
			interiorPositions = new Vector3[faceEdges.Count].AsEdgeAttribute();
			CalculateFaceEdgeCornerKitePositionsFromVertexPositions(faceEdges, vertexPositions, kiteLongEdgeLength, leadingPositions, followingPositions, interiorPositions);
		}

		public static void CalculateFaceEdgeCornerKitePositionsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IEdgeAttribute<Vector3> vertexPositions, float kiteLongEdgeLength, IEdgeAttribute<Vector3> leadingPositions, IEdgeAttribute<Vector3> followingPositions, IEdgeAttribute<Vector3> interiorPositions)
		{
			foreach (var edge in faceEdges)
			{
				var p0 = vertexPositions[edge.prev];
				var p1 = vertexPositions[edge];
				var p2 = vertexPositions[edge.next];
				var v1to0 = (p0 - p1).normalized;
				var v1to2 = (p2 - p1).normalized;
				var bisector = v1to0 + v1to2;
				if (bisector != Vector3.zero)
				{
					bisector.Normalize();
					var interiorHalfAngleSine = Vector3.Cross(bisector, v1to2).magnitude;
					var kiteAxis = bisector * (kiteLongEdgeLength / interiorHalfAngleSine);
					interiorPositions[edge] = p1 + kiteAxis;
					leadingPositions[edge] = p1 + kiteAxis.ProjectOntoUnit(v1to0);
					followingPositions[edge] = p1 + kiteAxis.ProjectOntoUnit(v1to2);
				}
				else
				{
					interiorPositions[edge] = p1;
					leadingPositions[edge] = p1;
					followingPositions[edge] = p1;
				}
			}
		}

		#region CalculateUVs...(...)

		#region CalculateGlobalUVs...(...)

		#region CalculateGlobalPanarUVs...(...)

		public static IEdgeAttribute<Vector2> CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IEdgeAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, Vector3.zero, uAxis, vAxis, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IEdgeAttribute<Vector3> vertexPositions, Vector3 origin, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, origin, uAxis, vAxis, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IEdgeAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, IEdgeAttribute<Vector2> uvs)
		{
			return CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, Vector3.zero, uAxis, vAxis, uvs);
		}

		public static IEdgeAttribute<Vector2> CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IEdgeAttribute<Vector3> vertexPositions, Vector3 origin, Vector3 uAxis, Vector3 vAxis, IEdgeAttribute<Vector2> uvs)
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
					Geometry.GetIntersectionParameter(vPlane, new ScaledRay(vertexPosition, uAxisNeg)),
					Geometry.GetIntersectionParameter(uPlane, new ScaledRay(vertexPosition, vAxisNeg)));
			}

			return uvs;
		}

		public static IEdgeAttribute<Vector2> CalculateGlobalPlanarNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IEdgeAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculateGlobalPlanarNormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculateGlobalPlanarNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IEdgeAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, IEdgeAttribute<Vector2> uvs)
		{
			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);

			Vector2 uvMin = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
			Vector2 uvMax = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

			foreach (var edge in faceEdges)
			{
				uvMin = Geometry.AxisAlignedMin(uvMin, uvs[edge]);
				uvMax = Geometry.AxisAlignedMax(uvMax, uvs[edge]);
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

		#endregion

		#region CalculatePerFaceUVs...(...)

		#region CalculatePerFaceUnnormalizedUVs...(...)

		public static IEdgeAttribute<Vector2> CalculatePerFaceUnnormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<UVFrame3> faceUVFrames)
		{
			return CalculatePerFaceUnnormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, faceUVFrames, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculatePerFaceUnnormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<UVFrame3> faceUVFrames, IEdgeAttribute<Vector2> uvs)
		{
			foreach (var face in faces)
			{
				var uvFrame = faceUVFrames[face];
				foreach (var edge in face.edges)
				{
					var vertexPosition = vertexPositions[edge];
					uvs[edge] = uvFrame.GetUV(vertexPosition);
				}
			}

			return uvs;
		}

		#endregion

		#region CalculatePerFace...UVsFromUnnormalizedUVs(...)

		public static IEdgeAttribute<Vector2> CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(Topology.FacesIndexer faces, IEdgeAttribute<Vector2> unnormalizedUVs, IEdgeAttribute<Vector2> uvs)
		{
			var faceMinUVs = new Vector2[faces.Count].AsFaceAttribute();
			var faceRangeUVs = new Vector2[faces.Count].AsFaceAttribute();

			FaceAttributeUtility.CalculateFaceEdgeMinAndRangeValues(faces, unnormalizedUVs, faceMinUVs, faceRangeUVs);

			return CalculatePerFaceUniformlyNormalizedUVsFromFaceUVMinAndRange(faces, faceMinUVs, faceRangeUVs, uvs);
		}

		public static IEdgeAttribute<Vector2> CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(Topology.FacesIndexer faces, IEdgeAttribute<Vector2> unnormalizedUVs, float targetAspectRatio, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> uvs)
		{
			if (aspectRatioPreservation == AspectRatioPreservation.None) return CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(faces, unnormalizedUVs, uvs);

			var faceMinUVs = new Vector2[faces.Count].AsFaceAttribute();
			var faceRangeUVs = new Vector2[faces.Count].AsFaceAttribute();

			FaceAttributeUtility.CalculateFaceEdgeMinAndRangeValues(faces, unnormalizedUVs, faceMinUVs, faceRangeUVs);

			var aspectRatioPreservationDelegate = AspectRatioUtility.GetMinAndSizeAdjustmentDelegate(aspectRatioPreservation, targetAspectRatio);

			foreach (var face in faces)
			{
				Vector2 uvMin = faceMinUVs[face];
				Vector2 uvRange = faceRangeUVs[face];
				aspectRatioPreservationDelegate(ref uvMin, ref uvRange);
				faceMinUVs[face] = uvMin;
				faceRangeUVs[face] = uvRange;
			}

			return CalculatePerFaceUniformlyNormalizedUVsFromFaceUVMinAndRange(faces, faceMinUVs, faceRangeUVs, uvs);
		}

		public static IEdgeAttribute<Vector2> CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(Topology.FacesIndexer faces, IEdgeAttribute<Vector2> unnormalizedEdgeUVs, IFaceAttribute<Vector2> unnormalizedFaceUVs, IEdgeAttribute<Vector2> edgeUVs)
		{
			var faceMinUVs = new Vector2[faces.Count].AsFaceAttribute();
			var faceRangeUVs = new Vector2[faces.Count].AsFaceAttribute();

			FaceAttributeUtility.CalculateCenteredFaceEdgeMinAndRangeValues(faces, unnormalizedEdgeUVs, unnormalizedFaceUVs, faceMinUVs, faceRangeUVs);

			return CalculatePerFaceUniformlyNormalizedUVsFromFaceUVMinAndRange(faces, faceMinUVs, faceRangeUVs, edgeUVs);
		}

		public static IEdgeAttribute<Vector2> CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(Topology.FacesIndexer faces, IEdgeAttribute<Vector2> unnormalizedEdgeUVs, IFaceAttribute<Vector2> unnormalizedFaceUVs, float targetAspectRatio, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> edgeUVs)
		{
			if (aspectRatioPreservation == AspectRatioPreservation.None) return CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(faces, unnormalizedEdgeUVs, unnormalizedFaceUVs, edgeUVs);

			var faceMinUVs = new Vector2[faces.Count].AsFaceAttribute();
			var faceRangeUVs = new Vector2[faces.Count].AsFaceAttribute();

			FaceAttributeUtility.CalculateCenteredFaceEdgeMinAndRangeValues(faces, unnormalizedEdgeUVs, unnormalizedFaceUVs, faceMinUVs, faceRangeUVs);

			var aspectRatioPreservationDelegate = AspectRatioUtility.GetMinAndSizeAdjustmentDelegate(aspectRatioPreservation, targetAspectRatio);

			foreach (var face in faces)
			{
				Vector2 uvMin = faceMinUVs[face];
				Vector2 uvRange = faceRangeUVs[face];
				aspectRatioPreservationDelegate(ref uvMin, ref uvRange);
				faceMinUVs[face] = uvMin;
				faceRangeUVs[face] = uvRange;
			}

			return CalculatePerFaceUniformlyNormalizedUVsFromFaceUVMinAndRange(faces, faceMinUVs, faceRangeUVs, edgeUVs);
		}

		public static IEdgeAttribute<Vector2> CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(Topology.FacesIndexer faces, IEdgeAttribute<Vector2> unnormalizedUVs, IEdgeAttribute<Vector2> uvs)
		{
			foreach (var face in faces)
			{
				Vector2 uvMin = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
				Vector2 uvMax = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

				foreach (var edge in face.edges)
				{
					uvMin = Geometry.AxisAlignedMin(uvMin, unnormalizedUVs[edge]);
					uvMax = Geometry.AxisAlignedMax(uvMax, unnormalizedUVs[edge]);
				}

				var uvRange = uvMax - uvMin;
				var uvRangeReciprocal = new Vector2(1f / uvRange.x, 1f / uvRange.y);

				foreach (var edge in face.edges)
				{
					uvs[edge] = Vector2.Scale(unnormalizedUVs[edge] - uvMin, uvRangeReciprocal);
				}
			}

			return uvs;
		}

		public static IEdgeAttribute<Vector2> CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(Topology.FacesIndexer faces, IEdgeAttribute<Vector2> unnormalizedUVs, float targetAspectRatio, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> uvs)
		{
			if (aspectRatioPreservation == AspectRatioPreservation.None) return CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(faces, unnormalizedUVs, uvs);

			var aspectRatioPreservationDelegate = AspectRatioUtility.GetMinAndSizeAdjustmentDelegate(aspectRatioPreservation, targetAspectRatio);

			foreach (var face in faces)
			{
				Vector2 uvMin = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
				Vector2 uvMax = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

				foreach (var edge in face.edges)
				{
					uvMin = Geometry.AxisAlignedMin(uvMin, unnormalizedUVs[edge]);
					uvMax = Geometry.AxisAlignedMax(uvMax, unnormalizedUVs[edge]);
				}

				var uvRange = uvMax - uvMin;
				aspectRatioPreservationDelegate(ref uvMin, ref uvRange);
				var uvRangeReciprocal = new Vector2(1f / uvRange.x, 1f / uvRange.y);

				foreach (var edge in face.edges)
				{
					uvs[edge] = Vector2.Scale(unnormalizedUVs[edge] - uvMin, uvRangeReciprocal);
				}
			}

			return uvs;
		}

		public static IEdgeAttribute<Vector2> CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(Topology.FacesIndexer faces, IEdgeAttribute<Vector2> unnormalizedEdgeUVs, IFaceAttribute<Vector2> unnormalizedFaceUVs, IEdgeAttribute<Vector2> edgeUVs)
		{
			foreach (var face in faces)
			{
				Vector2 uvMin = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
				Vector2 uvMax = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

				foreach (var edge in face.edges)
				{
					uvMin = Geometry.AxisAlignedMin(uvMin, unnormalizedEdgeUVs[edge]);
					uvMax = Geometry.AxisAlignedMax(uvMax, unnormalizedEdgeUVs[edge]);
				}

				var center = unnormalizedFaceUVs[face];
				var extent = Geometry.AxisAlignedMax(uvMax - center, center - uvMin);
				uvMin = center - extent;
				var uvRange = extent * 2f;
				var uvRangeReciprocal = new Vector2(1f / uvRange.x, 1f / uvRange.y);

				foreach (var edge in face.edges)
				{
					edgeUVs[edge] = Vector2.Scale(unnormalizedEdgeUVs[edge] - uvMin, uvRangeReciprocal);
				}
			}

			return edgeUVs;
		}

		public static IEdgeAttribute<Vector2> CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(Topology.FacesIndexer faces, IEdgeAttribute<Vector2> unnormalizedEdgeUVs, IFaceAttribute<Vector2> unnormalizedFaceUVs, float targetAspectRatio, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> edgeUVs)
		{
			if (aspectRatioPreservation == AspectRatioPreservation.None) return CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(faces, unnormalizedEdgeUVs, edgeUVs);

			var aspectRatioPreservationDelegate = AspectRatioUtility.GetMinAndSizeAdjustmentDelegate(aspectRatioPreservation, targetAspectRatio);

			foreach (var face in faces)
			{
				Vector2 uvMin = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
				Vector2 uvMax = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

				foreach (var edge in face.edges)
				{
					uvMin = Geometry.AxisAlignedMin(uvMin, unnormalizedEdgeUVs[edge]);
					uvMax = Geometry.AxisAlignedMax(uvMax, unnormalizedEdgeUVs[edge]);
				}

				var center = unnormalizedFaceUVs[face];
				var extent = Geometry.AxisAlignedMax(uvMax - center, center - uvMin);
				uvMin = center - extent;
				var uvRange = extent * 2f;
				aspectRatioPreservationDelegate(ref uvMin, ref uvRange);
				var uvRangeReciprocal = new Vector2(1f / uvRange.x, 1f / uvRange.y);

				foreach (var edge in face.edges)
				{
					edgeUVs[edge] = Vector2.Scale(unnormalizedEdgeUVs[edge] - uvMin, uvRangeReciprocal);
				}
			}

			return edgeUVs;
		}

		#endregion

		#region CalculatePerFace...UVsFromFaceUVMinAndRange(...)

		public static IEdgeAttribute<Vector2> CalculatePerFaceUniformlyNormalizedUVsFromFaceUVMinAndRange(Topology.FacesIndexer faces, IFaceAttribute<Vector2> faceMinUVs, IFaceAttribute<Vector2> faceRangeUVs, IEdgeAttribute<Vector2> uvs)
		{
			var faceMaxUVRange = Vector2.zero;

			foreach (var face in faces)
			{
				faceMaxUVRange = Geometry.AxisAlignedMax(faceMaxUVRange, faceRangeUVs[face]);
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

		#region CalculatePerFacePlanarUVs...(...)

		#region CaclulatePerFacePlanarUniformlyNormalizedUVs...(...)

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, uAxis, vAxis, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, IEdgeAttribute<Vector2> uvs)
		{
			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);
			return CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(faces, uvs, uvs);
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, AspectRatioPreservation aspectRatioPreservation)
		{
			return CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, uAxis, vAxis, aspectRatioPreservation, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> uvs)
		{
			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);
			return CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(faces, uvs, Mathf.Sqrt(uAxis.sqrMagnitude / vAxis.sqrMagnitude), aspectRatioPreservation, uvs);
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, facePositions, uAxis, vAxis, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis, IEdgeAttribute<Vector2> uvs)
		{
			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);
			var faceUVs = new Vector2[faces.Count].AsFaceAttribute();
			FaceAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromFacePositions(faces, facePositions, uAxis, vAxis, faceUVs);
			return CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(faces, uvs, faceUVs, uvs);
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis, AspectRatioPreservation aspectRatioPreservation)
		{
			return CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, facePositions, uAxis, vAxis, aspectRatioPreservation, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> uvs)
		{
			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);
			var faceUVs = new Vector2[faces.Count].AsFaceAttribute();
			FaceAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromFacePositions(faces, facePositions, uAxis, vAxis, faceUVs);
			return CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(faces, uvs, faceUVs, Mathf.Sqrt(uAxis.sqrMagnitude / vAxis.sqrMagnitude), aspectRatioPreservation, uvs);
		}

		#endregion

		#region CalculatePerFacePlanarVariablyNormalizedUVs...(...)

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, uAxis, vAxis, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, IEdgeAttribute<Vector2> uvs)
		{
			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);
			return CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(faces, uvs, uvs);
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, AspectRatioPreservation aspectRatioPreservation)
		{
			return CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, uAxis, vAxis, aspectRatioPreservation, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> uvs)
		{
			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);
			return CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(faces, uvs, Mathf.Sqrt(uAxis.sqrMagnitude / vAxis.sqrMagnitude), aspectRatioPreservation, uvs);
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, facePositions, uAxis, vAxis, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis, IEdgeAttribute<Vector2> uvs)
		{
			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);
			var faceUVs = new Vector2[faces.Count].AsFaceAttribute();
			FaceAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromFacePositions(faces, facePositions, uAxis, vAxis, faceUVs);
			return CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(faces, uvs, faceUVs, uvs);
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis, AspectRatioPreservation aspectRatioPreservation)
		{
			return CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, facePositions, uAxis, vAxis, aspectRatioPreservation, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> uvs)
		{
			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);
			var faceUVs = new Vector2[faces.Count].AsFaceAttribute();
			FaceAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromFacePositions(faces, facePositions, uAxis, vAxis, faceUVs);
			return CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(faces, uvs, faceUVs, Mathf.Sqrt(uAxis.sqrMagnitude / vAxis.sqrMagnitude), aspectRatioPreservation, uvs);
		}

		#endregion

		#endregion

		#region CalculatePerFaceUniformlyNormalizedUVs...(...)

		public static IEdgeAttribute<Vector2> CalculatePerFaceUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<UVFrame3> faceUVFrames)
		{
			return CalculatePerFaceUniformlyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, faceUVFrames, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculatePerFaceUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<UVFrame3> faceUVFrames, IEdgeAttribute<Vector2> uvs)
		{
			CalculatePerFaceUnnormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, faceUVFrames, uvs);
			return CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(faces, uvs, uvs);
		}

		public static IEdgeAttribute<Vector2> CalculatePerFaceUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, IFaceAttribute<UVFrame3> faceUVFrames)
		{
			return CalculatePerFaceUniformlyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, facePositions, faceUVFrames, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculatePerFaceUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, IFaceAttribute<UVFrame3> faceUVFrames, IEdgeAttribute<Vector2> uvs)
		{
			CalculatePerFaceUnnormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, faceUVFrames, uvs);
			var faceUVs = new Vector2[faces.Count].AsFaceAttribute();
			FaceAttributeUtility.CalculateUnnormalizedUVsFromFacePositions(faces, facePositions, faceUVFrames, faceUVs);
			return CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(faces, uvs, faceUVs, uvs);
		}

		public static IEdgeAttribute<Vector2> CalculatePerFaceUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<UVFrame3> faceUVFrames, float targetAspectRatio, AspectRatioPreservation aspectRatioPreservation)
		{
			return CalculatePerFaceUniformlyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, faceUVFrames, targetAspectRatio, aspectRatioPreservation, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculatePerFaceUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<UVFrame3> faceUVFrames, float targetAspectRatio, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> uvs)
		{
			CalculatePerFaceUnnormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, faceUVFrames, uvs);
			return CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(faces, uvs, targetAspectRatio, aspectRatioPreservation, uvs);
		}

		public static IEdgeAttribute<Vector2> CalculatePerFaceUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, IFaceAttribute<UVFrame3> faceUVFrames, float targetAspectRatio, AspectRatioPreservation aspectRatioPreservation)
		{
			return CalculatePerFaceUniformlyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, facePositions, faceUVFrames, targetAspectRatio, aspectRatioPreservation, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculatePerFaceUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, IFaceAttribute<UVFrame3> faceUVFrames, float targetAspectRatio, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> uvs)
		{
			CalculatePerFaceUnnormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, faceUVFrames, uvs);
			var faceUVs = new Vector2[faces.Count].AsFaceAttribute();
			FaceAttributeUtility.CalculateUnnormalizedUVsFromFacePositions(faces, facePositions, faceUVFrames, faceUVs);
			return CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(faces, uvs, faceUVs, targetAspectRatio, aspectRatioPreservation, uvs);
		}

		#endregion

		#region CalculatePerFaceVariablyNormalizedUVs...(...)

		public static IEdgeAttribute<Vector2> CalculatePerFaceVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<UVFrame3> faceUVFrames)
		{
			return CalculatePerFaceVariablyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, faceUVFrames, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculatePerFaceVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<UVFrame3> faceUVFrames, IEdgeAttribute<Vector2> uvs)
		{
			CalculatePerFaceUnnormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, faceUVFrames, uvs);
			return CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(faces, uvs, uvs);
		}

		public static IEdgeAttribute<Vector2> CalculatePerFaceVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, IFaceAttribute<UVFrame3> faceUVFrames)
		{
			return CalculatePerFaceVariablyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, facePositions, faceUVFrames, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculatePerFaceVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, IFaceAttribute<UVFrame3> faceUVFrames, IEdgeAttribute<Vector2> uvs)
		{
			CalculatePerFaceUnnormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, faceUVFrames, uvs);
			var faceUVs = new Vector2[faces.Count].AsFaceAttribute();
			FaceAttributeUtility.CalculateUnnormalizedUVsFromFacePositions(faces, facePositions, faceUVFrames, faceUVs);
			return CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(faces, uvs, faceUVs, uvs);
		}

		public static IEdgeAttribute<Vector2> CalculatePerFaceVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<UVFrame3> faceUVFrames, float targetAspectRatio, AspectRatioPreservation aspectRatioPreservation)
		{
			return CalculatePerFaceVariablyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, faceUVFrames, targetAspectRatio, aspectRatioPreservation, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculatePerFaceVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<UVFrame3> faceUVFrames, float targetAspectRatio, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> uvs)
		{
			CalculatePerFaceUnnormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, faceUVFrames, uvs);
			return CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(faces, uvs, targetAspectRatio, aspectRatioPreservation, uvs);
		}

		public static IEdgeAttribute<Vector2> CalculatePerFaceVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, IFaceAttribute<UVFrame3> faceUVFrames, float targetAspectRatio, AspectRatioPreservation aspectRatioPreservation)
		{
			return CalculatePerFaceVariablyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, facePositions, faceUVFrames, targetAspectRatio, aspectRatioPreservation, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		public static IEdgeAttribute<Vector2> CalculatePerFaceVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, IFaceAttribute<UVFrame3> faceUVFrames, float targetAspectRatio, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> uvs)
		{
			CalculatePerFaceUnnormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, faceUVFrames, uvs);
			var faceUVs = new Vector2[faces.Count].AsFaceAttribute();
			FaceAttributeUtility.CalculateUnnormalizedUVsFromFacePositions(faces, facePositions, faceUVFrames, faceUVs);
			return CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(faces, uvs, faceUVs, targetAspectRatio, aspectRatioPreservation, uvs);
		}

		#endregion

		#endregion

		#endregion

		#endregion
	}
}
