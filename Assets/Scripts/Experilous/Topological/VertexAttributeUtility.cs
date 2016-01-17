using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public static class VertexAttributeUtility
	{
		#region IList<Vector3> CalculateVertexNormals...(...)

		public static IList<Vector3> CalculateVertexNormalsFromVertexPositions(Topology.VerticesIndexer vertices, IList<Vector3> vertexPositions)
		{
			return CalculateVertexNormalsFromVertexPositions(vertices, vertexPositions, new Vector3[vertices.Count]);
		}

		public static IList<Vector3> CalculateVertexNormalsFromVertexPositions(Topology.VerticesIndexer vertices, IList<Vector3> vertexPositions, IList<Vector3> vertexNormals)
		{
			foreach (var vertex in vertices)
			{
				var normalSum = new Vector3(0f, 0f, 0f);
				var vertexPosition = vertexPositions[vertex];

				var edge = vertex.firstEdge;
				var p0 = vertexPositions[edge.farVertex];
				edge = edge.next;
				var firstEdge = edge;
				do
				{
					var p1 = vertexPositions[edge.farVertex];
					normalSum += Vector3.Cross(p0 - vertexPosition, p1 - vertexPosition);
					edge = edge.next;
				} while (edge != firstEdge);

				vertexNormals[vertex] = normalSum.normalized;
			}

			return vertexNormals;
		}

		public static IList<Vector3> CalculateVertexNormalsFromFacePositions(Topology.VerticesIndexer vertices, IList<Vector3> facePositions)
		{
			return CalculateVertexNormalsFromFacePositions(vertices, facePositions, new Vector3[vertices.Count]);
		}

		public static IList<Vector3> CalculateVertexNormalsFromFacePositions(Topology.VerticesIndexer vertices, IList<Vector3> facePositions, IList<Vector3> vertexNormals)
		{
			foreach (var vertex in vertices)
			{
				var normalSum = new Vector3(0f, 0f, 0f);

				var edge = vertex.firstEdge;
				var p0 = facePositions[edge.prevFace];
				edge = edge.next;
				var p1 = facePositions[edge.prevFace];
				edge = edge.next;
				var firstEdge = edge;
				do
				{
					var p2 = facePositions[edge.prevFace];
					normalSum += Vector3.Cross(p0 - p1, p2 - p1);
					edge = edge.next;
				} while (edge != firstEdge);

				vertexNormals[vertex] = normalSum.normalized;
			}

			return vertexNormals;
		}

		public static IList<Vector3> CalculateVertexNormalsFromFaceNormals(Topology.VerticesIndexer vertices, IList<Vector3> faceNormals)
		{
			return CalculateVertexNormalsFromFaceNormals(vertices, faceNormals, new Vector3[vertices.Count]);
		}

		public static IList<Vector3> CalculateVertexNormalsFromFaceNormals(Topology.VerticesIndexer vertices, IList<Vector3> faceNormals, IList<Vector3> vertexNormals)
		{
			foreach (var vertex in vertices)
			{
				var normalSum = new Vector3(0f, 0f, 0f);

				foreach (var edge in vertex.edges)
				{
					normalSum += faceNormals[edge.prevFace];
				}

				vertexNormals[vertex] = normalSum.normalized;
			}

			return vertexNormals;
		}

		public static IList<Vector3> CalculateSphericalVertexNormalsFromVertexPositions(Topology.VerticesIndexer vertices, IList<Vector3> vertexPositions)
		{
			return CalculateSphericalVertexNormalsFromVertexPositions(vertices, vertexPositions, new Vector3[vertices.Count]);
		}

		public static IList<Vector3> CalculateSphericalVertexNormalsFromVertexPositions(Topology.VerticesIndexer vertices, IList<Vector3> vertexPositions, IList<Vector3> vertexNormals)
		{
			foreach (var vertex in vertices)
			{
				vertexNormals[vertex] = vertexPositions[vertex].normalized;
			}

			return vertexNormals;
		}

		#endregion

		#region IList<float> CalculateVertexAreas...(...)

		public static IList<float> CalculatePlanarVertexAreasFromFaceCentroids(Topology.VerticesIndexer vertices, IList<Vector3> faceCentroids)
		{
			return CalculatePlanarVertexAreasFromFaceCentroids(vertices, faceCentroids, new float[vertices.Count]);
		}

		public static IList<float> CalculatePlanarVertexAreasFromFaceCentroids(Topology.VerticesIndexer vertices, IList<Vector3> faceCentroids, IList<float> vertexAreas)
		{
			foreach (var vertex in vertices)
			{
				if (vertex.neighborCount == 3)
				{
					var edge = vertex.firstEdge;
					var p0 = faceCentroids[edge.prevFace];
					edge = edge.next;
					var p1 = faceCentroids[edge.prevFace];
					edge = edge.next;
					var p2 = faceCentroids[edge.prevFace];
					vertexAreas[vertex] = Vector3.Cross(p1 - p0, p2 - p0).magnitude * 0.5f;
				}
				else
				{
					throw new System.NotImplementedException();
				}
			}

			return vertexAreas;
		}

		public static IList<float> CalculateVertexAreasFromVertexPositionsAndFaceCentroids(Topology.VerticesIndexer vertices, IList<Vector3> vertexPositions, IList<Vector3> faceCentroids)
		{
			return CalculateVertexAreasFromVertexPositionsAndFaceCentroids(vertices, faceCentroids, vertexPositions, new float[vertices.Count]);
		}

		public static IList<float> CalculateVertexAreasFromVertexPositionsAndFaceCentroids(Topology.VerticesIndexer vertices, IList<Vector3> vertexPositions, IList<Vector3> faceCentroids, IList<float> vertexAreas)
		{
			foreach (var vertex in vertices)
			{
				if (vertex.neighborCount == 3)
				{
					var edge = vertex.firstEdge;
					var p0 = faceCentroids[edge.prevFace];
					edge = edge.next;
					var p1 = faceCentroids[edge.prevFace];
					edge = edge.next;
					var p2 = faceCentroids[edge.prevFace];
					vertexAreas[vertex] = Vector3.Cross(p1 - p0, p2 - p0).magnitude * 0.5f;
				}
				else
				{
					var p0 = vertexPositions[vertex];
					float doubleAreaSum = 0f;
					var edge = vertex.firstEdge;
					var p1 = faceCentroids[edge.prevFace];
					edge = edge.next;
					var firstEdge = edge;
					do
					{
						var p2 = faceCentroids[edge.prevFace];
						doubleAreaSum += Vector3.Cross(p1 - p0, p2 - p0).magnitude;
						p1 = p2;
						edge = edge.next;
					} while (edge != firstEdge);
					vertexAreas[vertex] = doubleAreaSum / (2 * vertex.neighborCount);
				}
			}

			return vertexAreas;
		}

		public static IList<float> CalculateSphericalVertexAreasFromFaceCentroidAngles(Topology.VerticesIndexer vertices, IList<float> faceCentroidAngles, float sphereRadius = 1f)
		{
			return CalculateSphericalVertexAreasFromFaceCentroidAngles(vertices, faceCentroidAngles, sphereRadius, new float[vertices.Count]);
		}

		public static IList<float> CalculateSphericalVertexAreasFromFaceCentroidAngles(Topology.VerticesIndexer vertices, IList<float> faceCentroidAngles, float sphereRadius, IList<float> vertexAreas)
		{
			var radiusSquared = sphereRadius * sphereRadius;

			foreach (var vertex in vertices)
			{
				float angleSum = 0f;
				foreach (var edge in vertex.edges)
				{
					angleSum += faceCentroidAngles[edge];
				}
				vertexAreas[vertex] = (angleSum - Mathf.PI) * radiusSquared;
			}

			return vertexAreas;
		}

		#endregion
	}
}
