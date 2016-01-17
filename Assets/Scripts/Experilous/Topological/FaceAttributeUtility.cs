using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public static class FaceAttributeUtility
	{
		#region IList<Vector3> CacluateFaceCentroids...(...)

		public static IList<Vector3> CalculateFaceCentroidsFromVertexPositions(Topology.FacesIndexer faces, IList<Vector3> vertexPositions)
		{
			return CalculateFaceCentroidsFromVertexPositions(faces, vertexPositions, new Vector3[faces.Count]);
		}

		public static IList<Vector3> CalculateFaceCentroidsFromVertexPositions(Topology.FacesIndexer faces, IList<Vector3> vertexPositions, IList<Vector3> faceCentroids)
		{
			foreach (var face in faces)
			{
				var centroid = new Vector3(0f, 0f, 0f);
				foreach (var edge in face.edges)
				{
					centroid += vertexPositions[edge.nextVertex];
				}
				faceCentroids[face] = centroid / face.neighborCount;
			}

			return faceCentroids;
		}

		public static IList<Vector3> CalculateSphericalFaceCentroidsFromVertexPositions(Topology.FacesIndexer faces, IList<Vector3> vertexPositions, float circumsphereRadius = 1f)
		{
			return CalculateSphericalFaceCentroidsFromVertexPositions(faces, vertexPositions, circumsphereRadius, new Vector3[faces.Count]);
		}

		public static IList<Vector3> CalculateSphericalFaceCentroidsFromVertexPositions(Topology.FacesIndexer faces, IList<Vector3> vertexPositions, float circumsphereRadius, IList<Vector3> faceCentroids)
		{
			foreach (var face in faces)
			{
				var centroid = new Vector3(0f, 0f, 0f);
				foreach (var edge in face.edges)
				{
					centroid += vertexPositions[edge.nextVertex];
				}
				faceCentroids[face] = centroid * (circumsphereRadius / centroid.magnitude);
			}

			return faceCentroids;
		}

		#endregion

		#region IList<float> CalculateFaceNormals...(...)

		public static IList<Vector3> CalculateFaceNormalsFromFacePositions(Topology.FacesIndexer faces, IList<Vector3> facePositions)
		{
			return CalculateFaceNormalsFromFacePositions(faces, facePositions, new Vector3[faces.Count]);
		}

		public static IList<Vector3> CalculateFaceNormalsFromFacePositions(Topology.FacesIndexer faces, IList<Vector3> facePositions, IList<Vector3> faceNormals)
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

		public static IList<Vector3> CalculateFaceNormalsFromVertexPositions(Topology.FacesIndexer faces, IList<Vector3> vertexPositions)
		{
			return CalculateFaceNormalsFromFacePositions(faces, vertexPositions, new Vector3[faces.Count]);
		}

		public static IList<Vector3> CalculateFaceNormalsFromVertexPositions(Topology.FacesIndexer faces, IList<Vector3> vertexPositions, IList<Vector3> faceNormals)
		{
			foreach (var face in faces)
			{
				var normalSum = new Vector3(0f, 0f, 0f);

				var edge = face.firstEdge;
				var p0 = vertexPositions[edge.nextVertex];
				edge = edge.next;
				var p1 = vertexPositions[edge.nextVertex];
				edge = edge.next;
				var firstEdge = edge;
				do
				{
					var p2 = vertexPositions[edge.nextVertex];
					normalSum += Vector3.Cross(p0 - p1, p2 - p1);
					edge = edge.next;
				} while (edge != firstEdge);

				faceNormals[face] = normalSum.normalized;
			}

			return faceNormals;
		}

		public static IList<Vector3> CalculateFaceNormalsFromVertexNormals(Topology.FacesIndexer faces, IList<Vector3> vertexNormals)
		{
			return CalculateFaceNormalsFromFacePositions(faces, vertexNormals, new Vector3[faces.Count]);
		}

		public static IList<Vector3> CalculateFaceNormalsFromVertexNormals(Topology.FacesIndexer faces, IList<Vector3> vertexNormals, IList<Vector3> faceNormals)
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

		public static IList<Vector3> CalculateSphericalFaceNormalsFromVertexPositions(Topology.FacesIndexer faces, IList<Vector3> vertexPositions)
		{
			return CalculateSphericalFaceNormalsFromVertexPositions(faces, vertexPositions);
		}
		
		public static IList<Vector3> CalculateSphericalFaceNormalsFromVertexPositions(Topology.FacesIndexer faces, IList<Vector3> vertexPositions, IList<Vector3> faceNormals)
		{
			return CalculateSphericalFaceCentroidsFromVertexPositions(faces, vertexPositions, 1f, faceNormals);
		}
		
		public static IList<Vector3> CalculateSphericalFaceNormalsFromFacePositions(Topology.FacesIndexer faces, IList<Vector3> facePositions)
		{
			return CalculateSphericalFaceNormalsFromFacePositions(faces, facePositions, new Vector3[faces.Count]);
		}

		public static IList<Vector3> CalculateSphericalFaceNormalsFromFacePositions(Topology.FacesIndexer faces, IList<Vector3> facePositions, IList<Vector3> faceNormals)
		{
			foreach (var face in faces)
			{
				faceNormals[face] = facePositions[face].normalized;
			}
			return faceNormals;
		}

		#endregion

		#region IList<float> CalculateFaceAreas...(...)

		public static IList<float> CalculatePlanarFaceAreasFromVertexPositions(Topology.FacesIndexer faces, IList<Vector3> vertexPositions)
		{
			return CalculatePlanarFaceAreasFromVertexPositions(faces, vertexPositions, new float[faces.Count]);
		}

		public static IList<float> CalculatePlanarFaceAreasFromVertexPositions(Topology.FacesIndexer faces, IList<Vector3> vertexPositions, IList<float> faceAreas)
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

		public static IList<float> CalculateSphericalFaceAreasFromFaceCentroidAngles(Topology.FacesIndexer faces, IList<float> faceCentroidAngles, float circumsphereRadius = 1f)
		{
			return CalculateSphericalFaceAreasFromFaceCentroidAngles(faces, faceCentroidAngles, circumsphereRadius, new float[faces.Count]);
		}

		public static IList<float> CalculateSphericalFaceAreasFromFaceCentroidAngles(Topology.FacesIndexer faces, IList<float> faceCentroidAngles, float circumsphereRadius, IList<float> faceAreas)
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
	}
}
