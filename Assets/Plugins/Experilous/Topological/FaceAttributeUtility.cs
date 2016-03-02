/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
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
	}
}
