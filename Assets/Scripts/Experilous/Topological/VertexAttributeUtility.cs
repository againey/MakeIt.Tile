using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public static class VertexAttributeUtility
	{
		#region IList<Vector3> CalculateVertexNormals...(...)

		public static IVertexAttribute<Vector3> CalculateVertexNormalsFromVertexPositions(Topology.VerticesIndexer vertices, IVertexAttribute<Vector3> vertexPositions)
		{
			return CalculateVertexNormalsFromVertexPositions(vertices, vertexPositions, new Vector3[vertices.Count].AsVertexAttribute());
		}

		public static IVertexAttribute<Vector3> CalculateVertexNormalsFromVertexPositions(Topology.VerticesIndexer vertices, IVertexAttribute<Vector3> vertexPositions, IVertexAttribute<Vector3> vertexNormals)
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

		public static IVertexAttribute<Vector3> CalculateVertexNormalsFromFacePositions(Topology.VerticesIndexer vertices, IFaceAttribute<Vector3> facePositions)
		{
			return CalculateVertexNormalsFromFacePositions(vertices, facePositions, new Vector3[vertices.Count].AsVertexAttribute());
		}

		public static IVertexAttribute<Vector3> CalculateVertexNormalsFromFacePositions(Topology.VerticesIndexer vertices, IFaceAttribute<Vector3> facePositions, IVertexAttribute<Vector3> vertexNormals)
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

		public static IVertexAttribute<Vector3> CalculateVertexNormalsFromFaceNormals(Topology.VerticesIndexer vertices, IFaceAttribute<Vector3> faceNormals)
		{
			return CalculateVertexNormalsFromFaceNormals(vertices, faceNormals, new Vector3[vertices.Count].AsVertexAttribute());
		}

		public static IVertexAttribute<Vector3> CalculateVertexNormalsFromFaceNormals(Topology.VerticesIndexer vertices, IFaceAttribute<Vector3> faceNormals, IVertexAttribute<Vector3> vertexNormals)
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

		public static IVertexAttribute<Vector3> CalculateSphericalVertexNormalsFromVertexPositions(Topology.VerticesIndexer vertices, IVertexAttribute<Vector3> vertexPositions)
		{
			return CalculateSphericalVertexNormalsFromVertexPositions(vertices, vertexPositions, new Vector3[vertices.Count].AsVertexAttribute());
		}

		public static IVertexAttribute<Vector3> CalculateSphericalVertexNormalsFromVertexPositions(Topology.VerticesIndexer vertices, IVertexAttribute<Vector3> vertexPositions, IVertexAttribute<Vector3> vertexNormals)
		{
			foreach (var vertex in vertices)
			{
				vertexNormals[vertex] = vertexPositions[vertex].normalized;
			}

			return vertexNormals;
		}

		#endregion

		#region IList<float> CalculateVertexAreas...(...)

		public static IVertexAttribute<float> CalculatePlanarVertexAreasFromFaceCentroids(Topology.VerticesIndexer vertices, IFaceAttribute<Vector3> faceCentroids)
		{
			return CalculatePlanarVertexAreasFromFaceCentroids(vertices, faceCentroids, new float[vertices.Count].AsVertexAttribute());
		}

		public static IVertexAttribute<float> CalculatePlanarVertexAreasFromFaceCentroids(Topology.VerticesIndexer vertices, IFaceAttribute<Vector3> faceCentroids, IVertexAttribute<float> vertexAreas)
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

		public static IVertexAttribute<float> CalculateVertexAreasFromVertexPositionsAndFaceCentroids(Topology.VerticesIndexer vertices, IVertexAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> faceCentroids)
		{
			return CalculateVertexAreasFromVertexPositionsAndFaceCentroids(vertices, vertexPositions, faceCentroids, new float[vertices.Count].AsVertexAttribute());
		}

		public static IVertexAttribute<float> CalculateVertexAreasFromVertexPositionsAndFaceCentroids(Topology.VerticesIndexer vertices, IVertexAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> faceCentroids, IVertexAttribute<float> vertexAreas)
		{
			foreach (var vertex in vertices)
			{
				var center = vertexPositions[vertex];
				var quadAreaSum = 0f;
				var edge = vertex.firstEdge;
				var v0 = vertexPositions[edge] - center;
				edge = edge.next;
				var firstEdge = edge;
				do
				{
					var v2 = vertexPositions[edge] - center;
					if (!edge.isOuterBoundary)
					{
						var v1 = faceCentroids[edge] - center;
						quadAreaSum += Vector3.Cross(v0, v1).magnitude + Vector3.Cross(v1, v2).magnitude;
					}
					v0 = v2;
					edge = edge.next;
				} while (edge != firstEdge);

				vertexAreas[vertex] = quadAreaSum / 4;
			}

			return vertexAreas;
		}

		public static IVertexAttribute<float> CalculateSphericalVertexAreasFromFaceCentroidAngles(Topology.VerticesIndexer vertices, IEdgeAttribute<float> faceCentroidAngles, float sphereRadius = 1f)
		{
			return CalculateSphericalVertexAreasFromFaceCentroidAngles(vertices, faceCentroidAngles, sphereRadius, new float[vertices.Count].AsVertexAttribute());
		}

		public static IVertexAttribute<float> CalculateSphericalVertexAreasFromFaceCentroidAngles(Topology.VerticesIndexer vertices, IEdgeAttribute<float> faceCentroidAngles, float sphereRadius, IVertexAttribute<float> vertexAreas)
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
