using UnityEngine;
using System;

namespace Experilous.Topological
{
	[Serializable]
	public class Manifold
	{
		[SerializeField]
		private Topology _topology;

		[SerializeField]
		private Vector3[] _vertexPositions;

		public Manifold(Topology topology, Vector3[] vertexPositions)
		{
			_topology = topology;
			_vertexPositions = vertexPositions;
		}

		public Manifold Clone()
		{
			return new Manifold(_topology.Clone(), (Vector3[])_vertexPositions.Clone());
		}

		public Topology topology { get { return _topology; } set { _topology = value; } }
		public Vector3[] vertexPositions { get { return _vertexPositions; } set { _vertexPositions = value; } }

		private static Topology.FacesIndexer GetFaces(Manifold manifold, bool includeExternalFaces)
		{
			return !includeExternalFaces ? manifold.topology.internalFaces : manifold.topology.faces;
		}

		private static Topology.FaceEdgesIndexer GetFaceEdges(Manifold manifold, bool includeExternalFaces)
		{
			return !includeExternalFaces ? manifold.topology.internalFaceEdges : manifold.topology.faceEdges;
		}

		public static Vector3[] CalculateFaceCentroids(Manifold manifold)
		{
			return CalculateFaceCentroids(manifold, false);
		}
		
		public static Vector3[] CalculateFaceCentroids(Manifold manifold, bool includeExternalFaces)
		{
			var faces = GetFaces(manifold, includeExternalFaces);
			var centroids = new Vector3[faces.Count];
			var vertexPositions = manifold.vertexPositions;

			foreach (var face in faces)
			{
				var centroid = new Vector3(0f, 0f, 0f);
				foreach (var edge in face.edges)
				{
					centroid += vertexPositions[edge.nextVertex];
				}
				centroids[face] = centroid / face.neighborCount;
			}

			return centroids;
		}

		public static Vector3[] CalculateFaceNormals(Manifold manifold)
		{
			return CalculateFaceNormals(manifold, false);
		}
		
		public static Vector3[] CalculateFaceNormals(Manifold manifold, bool includeExternalFaces)
		{
			var faces = GetFaces(manifold, includeExternalFaces);
			var normals = new Vector3[faces.Count];
			var vertexPositions = manifold.vertexPositions;

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
					normals[face] = Vector3.Cross(p1 - p0, p2 - p0).normalized;
				}
				else
				{
					var normalSum = new Vector3(0f, 0f, 0f);
					foreach (var edge in face.edges)
					{
						normalSum += vertexPositions[edge.nextVertex];
					}
					normals[face] = normalSum.normalized;
				}
			}

			return normals;
		}

		public static float[] CalculateFaceAreas(Manifold manifold)
		{
			return CalculateFaceAreas(manifold, CalculateFaceCentroids(manifold, false), false);
		}
		
		public static float[] CalculateFaceAreas(Manifold manifold, bool includeExternalFaces)
		{
			return CalculateFaceAreas(manifold, CalculateFaceCentroids(manifold, includeExternalFaces), includeExternalFaces);
		}

		public static float[] CalculateFaceAreas(Manifold manifold, Vector3[] centroids)
		{
			return CalculateFaceAreas(manifold, centroids, false);
		}
		
		public static float[] CalculateFaceAreas(Manifold manifold, Vector3[] centroids, bool includeExternalFaces)
		{
			var faces = GetFaces(manifold, includeExternalFaces);
			var areas = new float[faces.Count];
			var vertexPositions = manifold.vertexPositions;

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
					areas[face] = Vector3.Cross(p1 - p0, p2 - p0).magnitude * 0.5f;
				}
				else
				{
					var p0 = centroids[face];
					float doubleAreaSum = 0f;
					var edge = face.firstEdge;
					var p1 = vertexPositions[edge.nextVertex];
					edge = edge.next;
					var firstEdge = edge;
					do
					{
						var p2 = vertexPositions[edge.nextVertex];
						doubleAreaSum += Vector3.Cross(p1 - p0, p2 - p0).magnitude;
						p1 = p2;
						edge = edge.next;
					} while (edge != firstEdge);
					areas[face] = doubleAreaSum / (2 * face.neighborCount);
				}
			}

			return areas;
		}

		public static float[] CalculateTriangularFaceAreas(Manifold manifold, bool includeExternalFaces)
		{
			var faces = GetFaces(manifold, includeExternalFaces);
			var areas = new float[faces.Count];
			var vertexPositions = manifold.vertexPositions;

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
					areas[face] = Vector3.Cross(p1 - p0, p2 - p0).magnitude * 0.5f;
				}
				else
				{
					throw new InvalidOperationException("Unexpected non-triangular face encountered while calculating face areas without centroids.");
				}
			}

			return areas;
		}

		public static float[] CalculateVertexAreas(Manifold manifold)
		{
			return CalculateVertexAreas(manifold, CalculateFaceCentroids(manifold, true));
		}
		
		public static float[] CalculateVertexAreas(Manifold manifold, Vector3[] centroids)
		{
			var vertices = manifold.topology.vertices;
			var areas = new float[vertices.Count];
			var vertexPositions = manifold.vertexPositions;

			foreach (var vertex in vertices)
			{
				if (vertex.neighborCount == 3)
				{
					var edge = vertex.firstEdge;
					var p0 = centroids[edge.prevFace];
					edge = edge.next;
					var p1 = centroids[edge.prevFace];
					edge = edge.next;
					var p2 = centroids[edge.prevFace];
					areas[vertex] = Vector3.Cross(p1 - p0, p2 - p0).magnitude * 0.5f;
				}
				else
				{
					var p0 = vertexPositions[vertex];
					float doubleAreaSum = 0f;
					var edge = vertex.firstEdge;
					var p1 = centroids[edge.prevFace];
					edge = edge.next;
					var firstEdge = edge;
					do
					{
						var p2 = centroids[edge.prevFace];
						doubleAreaSum += Vector3.Cross(p1 - p0, p2 - p0).magnitude;
						p1 = p2;
						edge = edge.next;
					} while (edge != firstEdge);
					areas[vertex] = doubleAreaSum / (2 * vertex.neighborCount);
				}
			}

			return areas;
		}

		public static float[] CalculateVertexAngles(Manifold manifold)
		{
			var edges = manifold.topology.vertexEdges;
			var angles = new float[edges.Count];
			var vertexPositions = manifold.vertexPositions;

			foreach (var edge in edges)
			{
				var p0 = vertexPositions[edge.next.farVertex];
				var p1 = vertexPositions[edge.nearVertex];
				var p2 = vertexPositions[edge.farVertex];
				var v1 = (p1 - p0).normalized;
				var v2 = (p2 - p0).normalized;
				angles[edge] = SphericalManifold.AngleBetweenUnitVectors(v1, v2);
			}

			return angles;
		}

		public static float[] CalculateFaceAngles(Manifold manifold)
		{
			return CalculateFaceAngles(manifold, false);
		}

		public static float[] CalculateFaceAngles(Manifold manifold, bool includeExternalFaces)
		{
			var edges = GetFaceEdges(manifold, includeExternalFaces);
			var angles = new float[edges.Count];
			var vertexPositions = manifold.vertexPositions;

			foreach (var edge in edges)
			{
				var p0 = vertexPositions[edge.prev.prevVertex];
				var p1 = vertexPositions[edge.prevVertex];
				var p2 = vertexPositions[edge.nextVertex];
				var v1 = (p1 - p0).normalized;
				var v2 = (p2 - p0).normalized;
				angles[edge] = SphericalManifold.AngleBetweenUnitVectors(v1, v2);
			}

			return angles;
		}

		public static float[] CalculateCentroidAngles(Manifold manifold)
		{
			return CalculateCentroidAngles(manifold, CalculateFaceCentroids(manifold, true));
		}

		public static float[] CalculateCentroidAngles(Manifold manifold, Vector3[] centroids)
		{
			var edges = manifold.topology.vertexEdges;
			var angles = new float[edges.Count];

			foreach (var edge in edges)
			{
				var p0 = centroids[edge.prevFace];
				var p1 = centroids[edge.nextFace];
				var p2 = centroids[edge.next.nextFace];
				var v1 = (p1 - p0).normalized;
				var v2 = (p2 - p0).normalized;
				angles[edge] = SphericalManifold.AngleBetweenUnitVectors(v1, v2);
			}

			return angles;
		}

		public static float[] CalculateVertexEdgeLengths(Manifold manifold)
		{
			var edges = manifold.topology.vertexEdges;
			var edgeLengths = new float[edges.Count];
			var vertexPositions = manifold.vertexPositions;

			foreach (var edge in edges)
			{
				edgeLengths[edge] = Vector3.Distance(vertexPositions[edge.nearVertex], vertexPositions[edge.farVertex]);
			}

			return edgeLengths;
		}

		public static Vector3[] CalculateVertexEdgeMidpoints(Manifold manifold)
		{
			var edges = manifold.topology.vertexEdges;
			var midpoints = new Vector3[edges.Count];
			var vertexPositions = manifold.vertexPositions;

			foreach (var edge in edges)
			{
				midpoints[edge] = (vertexPositions[edge.nearVertex] + vertexPositions[edge.farVertex]) * 0.5f;
			}

			return midpoints;
		}

		public static float[] CalculateFaceEdgeLengths(Manifold manifold)
		{
			return CalculateFaceEdgeLengths(manifold, CalculateFaceCentroids(manifold, false), Mathf.Infinity);
		}

		public static float[] CalculateFaceEdgeLengths(Manifold manifold, float boundaryLength)
		{
			return CalculateFaceEdgeLengths(manifold, CalculateFaceCentroids(manifold, false), boundaryLength);
		}

		public static float[] CalculateFaceEdgeLengths(Manifold manifold, Vector3[] centroids)
		{
			return CalculateFaceEdgeLengths(manifold, centroids, Mathf.Infinity);
		}

		public static float[] CalculateFaceEdgeLengths(Manifold manifold, Vector3[] centroids, float boundaryLength)
		{
			var edges = manifold.topology.faceEdges;
			var edgeLengths = new float[edges.Count];

			foreach (var edge in edges)
			{
				if (!edge.isBoundary)
				{
					edgeLengths[edge] = Vector3.Distance(centroids[edge.nearFace], centroids[edge.farFace]);
				}
				else
				{
					edgeLengths[edge] = boundaryLength;
				}
			}

			return edgeLengths;
		}
	}
}
