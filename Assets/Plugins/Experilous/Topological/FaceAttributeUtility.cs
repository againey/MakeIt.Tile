/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public static class FaceAttributeUtility
	{
		#region IList<Vector3> CacluateFaceCentroids...(...)

		public static IFaceAttribute<Vector3> CalculateFaceCentroidsFromVertexPositions(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions)
		{
			return CalculateFaceCentroidsFromVertexPositions(faces, vertexPositions, new Vector3[faces.Count].AsFaceAttribute());
		}

		public static IFaceAttribute<Vector3> CalculateFaceCentroidsFromVertexPositions(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> faceCentroids)
		{
			foreach (var face in faces)
			{
				var centroid = new Vector3(0f, 0f, 0f);
				foreach (var edge in face.edges)
				{
					centroid += vertexPositions[edge];
				}
				faceCentroids[face] = centroid / face.neighborCount;
			}

			return faceCentroids;
		}

		public static IFaceAttribute<Vector3> CalculateSphericalFaceCentroidsFromVertexPositions(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, float circumsphereRadius = 1f)
		{
			return CalculateSphericalFaceCentroidsFromVertexPositions(faces, vertexPositions, circumsphereRadius, new Vector3[faces.Count].AsFaceAttribute());
		}

		public static IFaceAttribute<Vector3> CalculateSphericalFaceCentroidsFromVertexPositions(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, float circumsphereRadius, IFaceAttribute<Vector3> faceCentroids)
		{
			foreach (var face in faces)
			{
				var centroid = new Vector3(0f, 0f, 0f);
				foreach (var edge in face.edges)
				{
					centroid += vertexPositions[edge];
				}
				faceCentroids[face] = centroid * (circumsphereRadius / centroid.magnitude);
			}

			return faceCentroids;
		}

		#endregion

		#region IList<float> CalculateFaceNormals...(...)

		public static IFaceAttribute<Vector3> CalculateFaceNormalsFromSurface(Topology.FacesIndexer faces, Surface surface, IFaceAttribute<Vector3> facePositions)
		{
			return CalculateFaceNormalsFromSurface(faces, surface, facePositions, new Vector3[faces.Count].AsFaceAttribute());
		}

		public static IFaceAttribute<Vector3> CalculateFaceNormalsFromSurface(Topology.FacesIndexer faces, Surface surface, IFaceAttribute<Vector3> facePositions, IFaceAttribute<Vector3> faceNormals)
		{
			foreach (var face in faces)
			{
				faceNormals[face] = surface.GetNormal(facePositions[face]);
			}

			return faceNormals;
		}

		public static IFaceAttribute<Vector3> CalculateFaceNormalsFromFacePositions(Topology.FacesIndexer faces, IFaceAttribute<Vector3> facePositions)
		{
			return CalculateFaceNormalsFromFacePositions(faces, facePositions, new Vector3[faces.Count].AsFaceAttribute());
		}

		public static IFaceAttribute<Vector3> CalculateFaceNormalsFromFacePositions(Topology.FacesIndexer faces, IFaceAttribute<Vector3> facePositions, IFaceAttribute<Vector3> faceNormals)
		{
			foreach (var face in faces)
			{
				var normalSum = new Vector3(0f, 0f, 0f);
				var facePosition = facePositions[face];

				var edge = face.firstEdge;
				var p0 = facePositions[edge.farFace];
				edge = edge.next;
				var firstEdge = edge;
				do
				{
					var p1 = facePositions[edge.farFace];
					normalSum += Vector3.Cross(p0 - facePosition, p1 - facePosition);
					edge = edge.next;
				} while (edge != firstEdge);

				faceNormals[face] = normalSum.normalized;
			}

			return faceNormals;
		}

		public static IFaceAttribute<Vector3> CalculateFaceNormalsFromVertexPositions(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions)
		{
			return CalculateFaceNormalsFromVertexPositions(faces, vertexPositions, new Vector3[faces.Count].AsFaceAttribute());
		}

		public static IFaceAttribute<Vector3> CalculateFaceNormalsFromVertexPositions(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> faceNormals)
		{
			foreach (var face in faces)
			{
				var normalSum = new Vector3(0f, 0f, 0f);

				var edge = face.firstEdge;
				var p0 = vertexPositions[edge];
				edge = edge.next;
				var p1 = vertexPositions[edge];
				edge = edge.next;
				var firstEdge = edge;
				do
				{
					var p2 = vertexPositions[edge];
					normalSum += Vector3.Cross(p0 - p1, p2 - p1);
					edge = edge.next;
				} while (edge != firstEdge);

				faceNormals[face] = normalSum.normalized;
			}

			return faceNormals;
		}

		public static IFaceAttribute<Vector3> CalculateFaceNormalsFromVertexNormals(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexNormals)
		{
			return CalculateFaceNormalsFromVertexNormals(faces, vertexNormals, new Vector3[faces.Count].AsFaceAttribute());
		}

		public static IFaceAttribute<Vector3> CalculateFaceNormalsFromVertexNormals(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexNormals, IFaceAttribute<Vector3> faceNormals)
		{
			foreach (var face in faces)
			{
				var normalSum = new Vector3(0f, 0f, 0f);

				foreach (var edge in face.edges)
				{
					normalSum += vertexNormals[edge.nextVertex];
				}

				faceNormals[face] = normalSum.normalized;
			}

			return faceNormals;
		}

		public static IFaceAttribute<Vector3> CalculateSphericalFaceNormalsFromVertexPositions(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions)
		{
			return CalculateSphericalFaceNormalsFromVertexPositions(faces, vertexPositions);
		}
		
		public static IFaceAttribute<Vector3> CalculateSphericalFaceNormalsFromVertexPositions(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> faceNormals)
		{
			return CalculateSphericalFaceCentroidsFromVertexPositions(faces, vertexPositions, 1f, faceNormals);
		}
		
		public static IFaceAttribute<Vector3> CalculateSphericalFaceNormalsFromFacePositions(Topology.FacesIndexer faces, IFaceAttribute<Vector3> facePositions)
		{
			return CalculateSphericalFaceNormalsFromFacePositions(faces, facePositions, new Vector3[faces.Count].AsFaceAttribute());
		}

		public static IFaceAttribute<Vector3> CalculateSphericalFaceNormalsFromFacePositions(Topology.FacesIndexer faces, IFaceAttribute<Vector3> facePositions, IFaceAttribute<Vector3> faceNormals)
		{
			foreach (var face in faces)
			{
				faceNormals[face] = facePositions[face].normalized;
			}
			return faceNormals;
		}

		#endregion

		#region IList<float> CalculateFaceAreas...(...)

		public static IFaceAttribute<float> CalculatePlanarFaceAreasFromVertexPositions(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions)
		{
			return CalculatePlanarFaceAreasFromVertexPositions(faces, vertexPositions, new float[faces.Count].AsFaceAttribute());
		}

		public static IFaceAttribute<float> CalculatePlanarFaceAreasFromVertexPositions(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, IFaceAttribute<float> faceAreas)
		{
			foreach (var face in faces)
			{
				if (face.neighborCount == 3)
				{
					var edge = face.firstEdge;
					var p0 = vertexPositions[edge.nextVertex];
					edge = edge.next;
					var p1 = vertexPositions[edge.nextVertex];
					edge = edge.next;
					var p2 = vertexPositions[edge.nextVertex];
					faceAreas[face] = Vector3.Cross(p1 - p0, p2 - p0).magnitude * 0.5f;
				}
				else
				{
					throw new System.NotImplementedException();
				}
			}

			return faceAreas;
		}

		public static IFaceAttribute<float> CalculateSphericalFaceAreasFromFaceCentroidAngles(Topology.FacesIndexer faces, IEdgeAttribute<float> faceCentroidAngles, float circumsphereRadius = 1f)
		{
			return CalculateSphericalFaceAreasFromFaceCentroidAngles(faces, faceCentroidAngles, circumsphereRadius, new float[faces.Count].AsFaceAttribute());
		}

		public static IFaceAttribute<float> CalculateSphericalFaceAreasFromFaceCentroidAngles(Topology.FacesIndexer faces, IEdgeAttribute<float> faceCentroidAngles, float circumsphereRadius, IFaceAttribute<float> faceAreas)
		{
			var radiusSquared = circumsphereRadius * circumsphereRadius;

			foreach (var face in faces)
			{
				float angleSum = 0f;
				foreach (var edge in face.edges)
				{
					angleSum += faceCentroidAngles[edge];
				}
				faceAreas[face] = (angleSum - Mathf.PI) * radiusSquared;
			}

			return faceAreas;
		}

		#endregion

		public static IFaceAttribute<float> CalculateFaceCircumferencesFromVertexPositions(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, IFaceAttribute<float> faceCircumferences)
		{
			foreach (var face in faces)
			{
				float circumference = 0f;
				var firstEdge = face.firstEdge;
				var priorPosition = vertexPositions[firstEdge];
				firstEdge = firstEdge.next;
				var nextEdge = firstEdge;
				do
				{
					var currentPosition = vertexPositions[nextEdge];
					circumference += Vector3.Distance(priorPosition, currentPosition);
					priorPosition = currentPosition;
					nextEdge = nextEdge.next;
				} while (nextEdge != firstEdge);

				faceCircumferences[face] = circumference;
			}

			return faceCircumferences;
		}

		public static IFaceAttribute<float> CalculateFaceCircumferencesFromEdgeLengths(Topology.FacesIndexer faces, IEdgeAttribute<float> edgeLengths, IFaceAttribute<float> faceCircumferences)
		{
			foreach (var face in faces)
			{
				float circumference = 0f;
				foreach (var edge in face.edges)
				{
					circumference += edgeLengths[edge];
				}
				faceCircumferences[face] = circumference;
			}

			return faceCircumferences;
		}

		#region CalculateMinAndRangeValues...(...)

		public static void CalculateFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IVertexAttribute<float> vertexValues, IFaceAttribute<float> minValues, IFaceAttribute<float> ranges)
		{
			foreach (var face in faces)
			{
				var min = float.MaxValue;
				var max = float.MinValue;
				foreach (var edge in face.edges)
				{
					min = Mathf.Min(min, vertexValues[edge]);
					max = Mathf.Max(max, vertexValues[edge]);
				}
				minValues[face] = min;
				ranges[face] = max - min;
			}
		}

		public static void CalculateFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IVertexAttribute<Vector2> vertexValues, IFaceAttribute<Vector2> minValues, IFaceAttribute<Vector2> ranges)
		{
			foreach (var face in faces)
			{
				var min = new Vector2(float.MaxValue, float.MaxValue);
				var max = new Vector2(float.MinValue, float.MinValue);
				foreach (var edge in face.edges)
				{
					min = GeometryUtility.AxisAlignedMin(min, vertexValues[edge]);
					max = GeometryUtility.AxisAlignedMax(max, vertexValues[edge]);
				}
				minValues[face] = min;
				ranges[face] = max - min;
			}
		}

		public static void CalculateFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexValues, IFaceAttribute<Vector3> minValues, IFaceAttribute<Vector3> ranges)
		{
			foreach (var face in faces)
			{
				var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
				var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
				foreach (var edge in face.edges)
				{
					min = GeometryUtility.AxisAlignedMin(min, vertexValues[edge]);
					max = GeometryUtility.AxisAlignedMax(max, vertexValues[edge]);
				}
				minValues[face] = min;
				ranges[face] = max - min;
			}
		}

		public static void CalculateFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IVertexAttribute<Vector4> vertexValues, IFaceAttribute<Vector4> minValues, IFaceAttribute<Vector4> ranges)
		{
			foreach (var face in faces)
			{
				var min = new Vector4(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);
				var max = new Vector4(float.MinValue, float.MinValue, float.MinValue, float.MinValue);
				foreach (var edge in face.edges)
				{
					min = GeometryUtility.AxisAlignedMin(min, vertexValues[edge]);
					max = GeometryUtility.AxisAlignedMax(max, vertexValues[edge]);
				}
				minValues[face] = min;
				ranges[face] = max - min;
			}
		}

		public static void CalculateFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<float> edgeValues, IFaceAttribute<float> minValues, IFaceAttribute<float> ranges)
		{
			foreach (var face in faces)
			{
				var min = float.MaxValue;
				var max = float.MinValue;
				foreach (var edge in face.edges)
				{
					min = Mathf.Min(min, edgeValues[edge]);
					max = Mathf.Max(max, edgeValues[edge]);
				}
				minValues[face] = min;
				ranges[face] = max - min;
			}
		}

		public static void CalculateFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<Vector2> edgeValues, IFaceAttribute<Vector2> minValues, IFaceAttribute<Vector2> ranges)
		{
			foreach (var face in faces)
			{
				var min = new Vector2(float.MaxValue, float.MaxValue);
				var max = new Vector2(float.MinValue, float.MinValue);
				foreach (var edge in face.edges)
				{
					min = GeometryUtility.AxisAlignedMin(min, edgeValues[edge]);
					max = GeometryUtility.AxisAlignedMax(max, edgeValues[edge]);
				}
				minValues[face] = min;
				ranges[face] = max - min;
			}
		}

		public static void CalculateFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> edgeValues, IFaceAttribute<Vector3> minValues, IFaceAttribute<Vector3> ranges)
		{
			foreach (var face in faces)
			{
				var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
				var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
				foreach (var edge in face.edges)
				{
					min = GeometryUtility.AxisAlignedMin(min, edgeValues[edge]);
					max = GeometryUtility.AxisAlignedMax(max, edgeValues[edge]);
				}
				minValues[face] = min;
				ranges[face] = max - min;
			}
		}

		public static void CalculateFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<Vector4> edgeValues, IFaceAttribute<Vector4> minValues, IFaceAttribute<Vector4> ranges)
		{
			foreach (var face in faces)
			{
				var min = new Vector4(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);
				var max = new Vector4(float.MinValue, float.MinValue, float.MinValue, float.MinValue);
				foreach (var edge in face.edges)
				{
					min = GeometryUtility.AxisAlignedMin(min, edgeValues[edge]);
					max = GeometryUtility.AxisAlignedMax(max, edgeValues[edge]);
				}
				minValues[face] = min;
				ranges[face] = max - min;
			}
		}

		public static void ExpandFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IVertexAttribute<float> vertexValues, IFaceAttribute<float> minValues, IFaceAttribute<float> ranges)
		{
			foreach (var face in faces)
			{
				var min = minValues[face];
				var max = min + ranges[face];
				foreach (var edge in face.edges)
				{
					min = Mathf.Min(min, vertexValues[edge]);
					max = Mathf.Max(max, vertexValues[edge]);
				}
				minValues[face] = min;
				ranges[face] = max - min;
			}
		}

		public static void ExpandFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IVertexAttribute<Vector2> vertexValues, IFaceAttribute<Vector2> minValues, IFaceAttribute<Vector2> ranges)
		{
			foreach (var face in faces)
			{
				var min = minValues[face];
				var max = min + ranges[face];
				foreach (var edge in face.edges)
				{
					min = GeometryUtility.AxisAlignedMin(min, vertexValues[edge]);
					max = GeometryUtility.AxisAlignedMax(max, vertexValues[edge]);
				}
				minValues[face] = min;
				ranges[face] = max - min;
			}
		}

		public static void ExpandFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexValues, IFaceAttribute<Vector3> minValues, IFaceAttribute<Vector3> ranges)
		{
			foreach (var face in faces)
			{
				var min = minValues[face];
				var max = min + ranges[face];
				foreach (var edge in face.edges)
				{
					min = GeometryUtility.AxisAlignedMin(min, vertexValues[edge]);
					max = GeometryUtility.AxisAlignedMax(max, vertexValues[edge]);
				}
				minValues[face] = min;
				ranges[face] = max - min;
			}
		}

		public static void ExpandFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IVertexAttribute<Vector4> vertexValues, IFaceAttribute<Vector4> minValues, IFaceAttribute<Vector4> ranges)
		{
			foreach (var face in faces)
			{
				var min = minValues[face];
				var max = min + ranges[face];
				foreach (var edge in face.edges)
				{
					min = GeometryUtility.AxisAlignedMin(min, vertexValues[edge]);
					max = GeometryUtility.AxisAlignedMax(max, vertexValues[edge]);
				}
				minValues[face] = min;
				ranges[face] = max - min;
			}
		}

		public static void ExpandFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<float> edgeValues, IFaceAttribute<float> minValues, IFaceAttribute<float> ranges)
		{
			foreach (var face in faces)
			{
				var min = minValues[face];
				var max = min + ranges[face];
				foreach (var edge in face.edges)
				{
					min = Mathf.Min(min, edgeValues[edge]);
					max = Mathf.Max(max, edgeValues[edge]);
				}
				minValues[face] = min;
				ranges[face] = max - min;
			}
		}

		public static void ExpandFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<Vector2> edgeValues, IFaceAttribute<Vector2> minValues, IFaceAttribute<Vector2> ranges)
		{
			foreach (var face in faces)
			{
				var min = minValues[face];
				var max = min + ranges[face];
				foreach (var edge in face.edges)
				{
					min = GeometryUtility.AxisAlignedMin(min, edgeValues[edge]);
					max = GeometryUtility.AxisAlignedMax(max, edgeValues[edge]);
				}
				minValues[face] = min;
				ranges[face] = max - min;
			}
		}

		public static void ExpandFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> edgeValues, IFaceAttribute<Vector3> minValues, IFaceAttribute<Vector3> ranges)
		{
			foreach (var face in faces)
			{
				var min = minValues[face];
				var max = min + ranges[face];
				foreach (var edge in face.edges)
				{
					min = GeometryUtility.AxisAlignedMin(min, edgeValues[edge]);
					max = GeometryUtility.AxisAlignedMax(max, edgeValues[edge]);
				}
				minValues[face] = min;
				ranges[face] = max - min;
			}
		}

		public static void ExpandFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<Vector4> edgeValues, IFaceAttribute<Vector4> minValues, IFaceAttribute<Vector4> ranges)
		{
			foreach (var face in faces)
			{
				var min = minValues[face];
				var max = min + ranges[face];
				foreach (var edge in face.edges)
				{
					min = GeometryUtility.AxisAlignedMin(min, edgeValues[edge]);
					max = GeometryUtility.AxisAlignedMax(max, edgeValues[edge]);
				}
				minValues[face] = min;
				ranges[face] = max - min;
			}
		}

		#endregion

		#region CalculateCenteredMinAndRangeValues...(...)

		public static void CalculateCenteredFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IVertexAttribute<float> vertexValues, IFaceAttribute<float> centerValues, IFaceAttribute<float> minValues, IFaceAttribute<float> ranges)
		{
			foreach (var face in faces)
			{
				var min = float.MaxValue;
				var max = float.MinValue;
				foreach (var edge in face.edges)
				{
					min = Mathf.Min(min, vertexValues[edge]);
					max = Mathf.Max(max, vertexValues[edge]);
				}
				var center = centerValues[face];
				var extent = Mathf.Max(max - center, center - min);
				minValues[face] = center - extent;
				ranges[face] = extent * 2f;
			}
		}

		public static void CalculateCenteredFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IVertexAttribute<Vector2> vertexValues, IFaceAttribute<Vector2> centerValues, IFaceAttribute<Vector2> minValues, IFaceAttribute<Vector2> ranges)
		{
			foreach (var face in faces)
			{
				var min = new Vector2(float.MaxValue, float.MaxValue);
				var max = new Vector2(float.MinValue, float.MinValue);
				foreach (var edge in face.edges)
				{
					min = GeometryUtility.AxisAlignedMin(min, vertexValues[edge]);
					max = GeometryUtility.AxisAlignedMax(max, vertexValues[edge]);
				}
				var center = centerValues[face];
				var extent = GeometryUtility.AxisAlignedMax(max - center, center - min);
				minValues[face] = center - extent;
				ranges[face] = extent * 2f;
			}
		}

		public static void CalculateCenteredFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexValues, IFaceAttribute<Vector3> centerValues, IFaceAttribute<Vector3> minValues, IFaceAttribute<Vector3> ranges)
		{
			foreach (var face in faces)
			{
				var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
				var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
				foreach (var edge in face.edges)
				{
					min = GeometryUtility.AxisAlignedMin(min, vertexValues[edge]);
					max = GeometryUtility.AxisAlignedMax(max, vertexValues[edge]);
				}
				var center = centerValues[face];
				var extent = GeometryUtility.AxisAlignedMax(max - center, center - min);
				minValues[face] = center - extent;
				ranges[face] = extent * 2f;
			}
		}

		public static void CalculateCenteredFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IVertexAttribute<Vector4> vertexValues, IFaceAttribute<Vector4> centerValues, IFaceAttribute<Vector4> minValues, IFaceAttribute<Vector4> ranges)
		{
			foreach (var face in faces)
			{
				var min = new Vector4(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);
				var max = new Vector4(float.MinValue, float.MinValue, float.MinValue, float.MinValue);
				foreach (var edge in face.edges)
				{
					min = GeometryUtility.AxisAlignedMin(min, vertexValues[edge]);
					max = GeometryUtility.AxisAlignedMax(max, vertexValues[edge]);
				}
				var center = centerValues[face];
				var extent = GeometryUtility.AxisAlignedMax(max - center, center - min);
				minValues[face] = center - extent;
				ranges[face] = extent * 2f;
			}
		}

		public static void CalculateCenteredFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<float> edgeValues, IFaceAttribute<float> centerValues, IFaceAttribute<float> minValues, IFaceAttribute<float> ranges)
		{
			foreach (var face in faces)
			{
				var min = float.MaxValue;
				var max = float.MinValue;
				foreach (var edge in face.edges)
				{
					min = Mathf.Min(min, edgeValues[edge]);
					max = Mathf.Max(max, edgeValues[edge]);
				}
				var center = centerValues[face];
				var extent = Mathf.Max(max - center, center - min);
				minValues[face] = center - extent;
				ranges[face] = extent * 2f;
			}
		}

		public static void CalculateCenteredFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<Vector2> edgeValues, IFaceAttribute<Vector2> centerValues, IFaceAttribute<Vector2> minValues, IFaceAttribute<Vector2> ranges)
		{
			foreach (var face in faces)
			{
				var min = new Vector2(float.MaxValue, float.MaxValue);
				var max = new Vector2(float.MinValue, float.MinValue);
				foreach (var edge in face.edges)
				{
					min = GeometryUtility.AxisAlignedMin(min, edgeValues[edge]);
					max = GeometryUtility.AxisAlignedMax(max, edgeValues[edge]);
				}
				var center = centerValues[face];
				var extent = GeometryUtility.AxisAlignedMax(max - center, center - min);
				minValues[face] = center - extent;
				ranges[face] = extent * 2f;
			}
		}

		public static void CalculateCenteredFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> edgeValues, IFaceAttribute<Vector3> centerValues, IFaceAttribute<Vector3> minValues, IFaceAttribute<Vector3> ranges)
		{
			foreach (var face in faces)
			{
				var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
				var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
				foreach (var edge in face.edges)
				{
					min = GeometryUtility.AxisAlignedMin(min, edgeValues[edge]);
					max = GeometryUtility.AxisAlignedMax(max, edgeValues[edge]);
				}
				var center = centerValues[face];
				var extent = GeometryUtility.AxisAlignedMax(max - center, center - min);
				minValues[face] = center - extent;
				ranges[face] = extent * 2f;
			}
		}

		public static void CalculateCenteredFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<Vector4> edgeValues, IFaceAttribute<Vector4> centerValues, IFaceAttribute<Vector4> minValues, IFaceAttribute<Vector4> ranges)
		{
			foreach (var face in faces)
			{
				var min = new Vector4(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);
				var max = new Vector4(float.MinValue, float.MinValue, float.MinValue, float.MinValue);
				foreach (var edge in face.edges)
				{
					min = GeometryUtility.AxisAlignedMin(min, edgeValues[edge]);
					max = GeometryUtility.AxisAlignedMax(max, edgeValues[edge]);
				}
				var center = centerValues[face];
				var extent = GeometryUtility.AxisAlignedMax(max - center, center - min);
				minValues[face] = center - extent;
				ranges[face] = extent * 2f;
			}
		}

		public static void ExpandCenteredFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IVertexAttribute<float> vertexValues, IFaceAttribute<float> centerValues, IFaceAttribute<float> minValues, IFaceAttribute<float> ranges)
		{
			foreach (var face in faces)
			{
				var min = minValues[face];
				var max = min + ranges[face];
				foreach (var edge in face.edges)
				{
					min = Mathf.Min(min, vertexValues[edge]);
					max = Mathf.Max(max, vertexValues[edge]);
				}
				var center = centerValues[face];
				var extent = Mathf.Max(max - center, center - min);
				minValues[face] = center - extent;
				ranges[face] = extent * 2f;
			}
		}

		public static void ExpandCenteredFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IVertexAttribute<Vector2> vertexValues, IFaceAttribute<Vector2> centerValues, IFaceAttribute<Vector2> minValues, IFaceAttribute<Vector2> ranges)
		{
			foreach (var face in faces)
			{
				var min = minValues[face];
				var max = min + ranges[face];
				foreach (var edge in face.edges)
				{
					min = GeometryUtility.AxisAlignedMin(min, vertexValues[edge]);
					max = GeometryUtility.AxisAlignedMax(max, vertexValues[edge]);
				}
				var center = centerValues[face];
				var extent = GeometryUtility.AxisAlignedMax(max - center, center - min);
				minValues[face] = center - extent;
				ranges[face] = extent * 2f;
			}
		}

		public static void ExpandCenteredFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexValues, IFaceAttribute<Vector3> centerValues, IFaceAttribute<Vector3> minValues, IFaceAttribute<Vector3> ranges)
		{
			foreach (var face in faces)
			{
				var min = minValues[face];
				var max = min + ranges[face];
				foreach (var edge in face.edges)
				{
					min = GeometryUtility.AxisAlignedMin(min, vertexValues[edge]);
					max = GeometryUtility.AxisAlignedMax(max, vertexValues[edge]);
				}
				var center = centerValues[face];
				var extent = GeometryUtility.AxisAlignedMax(max - center, center - min);
				minValues[face] = center - extent;
				ranges[face] = extent * 2f;
			}
		}

		public static void ExpandCenteredFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IVertexAttribute<Vector4> vertexValues, IFaceAttribute<Vector4> centerValues, IFaceAttribute<Vector4> minValues, IFaceAttribute<Vector4> ranges)
		{
			foreach (var face in faces)
			{
				var min = minValues[face];
				var max = min + ranges[face];
				foreach (var edge in face.edges)
				{
					min = GeometryUtility.AxisAlignedMin(min, vertexValues[edge]);
					max = GeometryUtility.AxisAlignedMax(max, vertexValues[edge]);
				}
				var center = centerValues[face];
				var extent = GeometryUtility.AxisAlignedMax(max - center, center - min);
				minValues[face] = center - extent;
				ranges[face] = extent * 2f;
			}
		}

		public static void ExpandCenteredFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<float> edgeValues, IFaceAttribute<float> centerValues, IFaceAttribute<float> minValues, IFaceAttribute<float> ranges)
		{
			foreach (var face in faces)
			{
				var min = minValues[face];
				var max = min + ranges[face];
				foreach (var edge in face.edges)
				{
					min = Mathf.Min(min, edgeValues[edge]);
					max = Mathf.Max(max, edgeValues[edge]);
				}
				var center = centerValues[face];
				var extent = Mathf.Max(max - center, center - min);
				minValues[face] = center - extent;
				ranges[face] = extent * 2f;
			}
		}

		public static void ExpandCenteredFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<Vector2> edgeValues, IFaceAttribute<Vector2> centerValues, IFaceAttribute<Vector2> minValues, IFaceAttribute<Vector2> ranges)
		{
			foreach (var face in faces)
			{
				var min = minValues[face];
				var max = min + ranges[face];
				foreach (var edge in face.edges)
				{
					min = GeometryUtility.AxisAlignedMin(min, edgeValues[edge]);
					max = GeometryUtility.AxisAlignedMax(max, edgeValues[edge]);
				}
				var center = centerValues[face];
				var extent = GeometryUtility.AxisAlignedMax(max - center, center - min);
				minValues[face] = center - extent;
				ranges[face] = extent * 2f;
			}
		}

		public static void ExpandCenteredFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> edgeValues, IFaceAttribute<Vector3> centerValues, IFaceAttribute<Vector3> minValues, IFaceAttribute<Vector3> ranges)
		{
			foreach (var face in faces)
			{
				var min = minValues[face];
				var max = min + ranges[face];
				foreach (var edge in face.edges)
				{
					min = GeometryUtility.AxisAlignedMin(min, edgeValues[edge]);
					max = GeometryUtility.AxisAlignedMax(max, edgeValues[edge]);
				}
				var center = centerValues[face];
				var extent = GeometryUtility.AxisAlignedMax(max - center, center - min);
				minValues[face] = center - extent;
				ranges[face] = extent * 2f;
			}
		}

		public static void ExpandCenteredFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<Vector4> edgeValues, IFaceAttribute<Vector4> centerValues, IFaceAttribute<Vector4> minValues, IFaceAttribute<Vector4> ranges)
		{
			foreach (var face in faces)
			{
				var min = minValues[face];
				var max = min + ranges[face];
				foreach (var edge in face.edges)
				{
					min = GeometryUtility.AxisAlignedMin(min, edgeValues[edge]);
					max = GeometryUtility.AxisAlignedMax(max, edgeValues[edge]);
				}
				var center = centerValues[face];
				var extent = GeometryUtility.AxisAlignedMax(max - center, center - min);
				minValues[face] = center - extent;
				ranges[face] = extent * 2f;
			}
		}

		#endregion

		#region CalculateGlobalUVs...(...)

		#region CalculateGlobalPlanarUVs...(...)

		public static IFaceAttribute<Vector2> CalculateGlobalPlanarUnnormalizedUVsFromFacePositions(Topology.FacesIndexer faces, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculateGlobalPlanarUnnormalizedUVsFromFacePositions(faces, facePositions, Vector3.zero, uAxis, vAxis, new Vector2[faces.Count].AsFaceAttribute());
		}

		public static IFaceAttribute<Vector2> CalculateGlobalPlanarUnnormalizedUVsFromFacePositions(Topology.FacesIndexer faces, IFaceAttribute<Vector3> facePositions, Vector3 origin, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculateGlobalPlanarUnnormalizedUVsFromFacePositions(faces, facePositions, origin, uAxis, vAxis, new Vector2[faces.Count].AsFaceAttribute());
		}

		public static IFaceAttribute<Vector2> CalculateGlobalPlanarUnnormalizedUVsFromFacePositions(Topology.FacesIndexer faces, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis, IFaceAttribute<Vector2> uvs)
		{
			return CalculateGlobalPlanarUnnormalizedUVsFromFacePositions(faces, facePositions, Vector3.zero, uAxis, vAxis, uvs);
		}

		public static IFaceAttribute<Vector2> CalculateGlobalPlanarUnnormalizedUVsFromFacePositions(Topology.FacesIndexer faces, IFaceAttribute<Vector3> facePositions, Vector3 origin, Vector3 uAxis, Vector3 vAxis, IFaceAttribute<Vector2> uvs)
		{
			var normal = Vector3.Cross(vAxis, uAxis).normalized;
			var uPlane = new Plane(origin, uAxis + origin, normal + origin);
			var vPlane = new Plane(origin, normal + origin, vAxis + origin);

			var uAxisNeg = -uAxis;
			var vAxisNeg = -vAxis;

			foreach (var face in faces)
			{
				var facePosition = facePositions[face];

				uvs[face] = new Vector2(
					GeometryUtility.GetIntersectionParameter(vPlane, new ScaledRay(facePosition, uAxisNeg)),
					GeometryUtility.GetIntersectionParameter(uPlane, new ScaledRay(facePosition, vAxisNeg)));
			}

			return uvs;
		}

		#endregion

		#region CalculateGlobalSphericalUVs...(...)

		public static IFaceAttribute<Vector2> CalculateGlobalSphericalUVsFromFacePositions(Topology.FacesIndexer faces, IFaceAttribute<Vector3> facePositions, Quaternion orientation)
		{
			return CalculateGlobalSphericalUVsFromFacePositions(faces, facePositions, orientation, new Vector2[faces.Count].AsFaceAttribute());
		}

		public static IFaceAttribute<Vector2> CalculateGlobalSphericalUVsFromFacePositions(Topology.FacesIndexer faces, IFaceAttribute<Vector3> facePositions, Quaternion orientation, IFaceAttribute<Vector2> uvs)
		{
			var twoPi = Mathf.PI * 2f;
			foreach (var face in faces)
			{
				var normal = (orientation * facePositions[face]).normalized;
				uvs[face] = new Vector2(
					Mathf.Atan2(normal.z, normal.x) / twoPi,
					Mathf.Acos(normal.y) / twoPi);
			}

			return uvs;
		}

		#endregion

		#endregion

		public static IFaceAttribute<Vector2> CalculateUnnormalizedUVsFromFacePositions(Topology.FacesIndexer faces, IFaceAttribute<Vector3> facePositions, IFaceAttribute<UVFrame3> uvFrames)
		{
			return CalculateUnnormalizedUVsFromFacePositions(faces, facePositions, uvFrames, new Vector2[faces.Count].AsFaceAttribute());
		}

		public static IFaceAttribute<Vector2> CalculateUnnormalizedUVsFromFacePositions(Topology.FacesIndexer faces, IFaceAttribute<Vector3> facePositions, IFaceAttribute<UVFrame3> uvFrames, IFaceAttribute<Vector2> uvs)
		{
			foreach (var face in faces)
			{
				var uvFrame = uvFrames[face];
				var position = facePositions[face];

				uvs[face] = uvFrame.GetUV(position);
			}

			return uvs;
		}

		#region CalculatePerFace...UVsFromFaceUVMinAndRange(...)

		public static IFaceAttribute<Vector2> CalculateUniformlyNormalizedUVsFromFaceUVMinAndRange(Topology.FacesIndexer faces, IFaceAttribute<Vector2> faceMinUVs, IFaceAttribute<Vector2> faceRangeUVs, IFaceAttribute<Vector2> uvs)
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
				uvs[face] = Vector2.Scale(uvs[face] - faceOffsetUV, faceMaxUVRangeReciprocal);
			}

			return uvs;
		}

		#endregion

		public static IFaceAttribute<UVFrame3> CalculatePerFaceSphericalUVFramesFromFaceNormals(Topology.FacesIndexer faces, IFaceAttribute<Vector3> faceNormals, Quaternion orientation)
		{
			return CalculatePerFaceSphericalUVFramesFromFaceNormals(faces, faceNormals, orientation, new UVFrame3[faces.Count].AsFaceAttribute());
		}

		public static IFaceAttribute<UVFrame3> CalculatePerFaceSphericalUVFramesFromFaceNormals(Topology.FacesIndexer faces, IFaceAttribute<Vector3> faceNormals, Quaternion orientation, IFaceAttribute<UVFrame3> uvFrames)
		{
			var globalUp = orientation * Vector3.up;
			var globalRight = orientation * Vector3.right;
			foreach (var face in faces)
			{
				var faceNormal = faceNormals[face];
				var faceRight = Vector3.Cross(faceNormal, globalUp);
				var faceRightSqrMag = faceRight.sqrMagnitude;
				Vector3 faceUp;
				if (faceRightSqrMag > 0.01f)
				{
					faceRight /= Mathf.Sqrt(faceRightSqrMag);
					faceUp = Vector3.Cross(faceRight, faceNormal);
				}
				else
				{
					faceUp = Vector3.Cross(globalRight, faceNormal).normalized;
					faceRight = Vector3.Cross(faceNormal, faceUp);
				}

				uvFrames[face] = new UVFrame3(
					new Plane(Vector3.zero, faceRight, faceNormal),
					new Plane(Vector3.zero, faceNormal, faceUp),
					-faceRight,
					-faceUp);
			}

			return uvFrames;
		}
	}
}
