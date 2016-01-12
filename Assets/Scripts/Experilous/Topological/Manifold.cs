using UnityEngine;
using System;

namespace Experilous.Topological
{
	[Serializable]
	public class Manifold : Topology
	{
		[SerializeField]
		protected Vector3VertexAttribute _vertexPositions;

		public Manifold()
		{
		}

		public Manifold(Manifold original)
		{
			CloneFields(original);
		}

		protected void CloneFields(Manifold original)
		{
			base.CloneFields(original);
			_vertexPositions = Vector3VertexAttribute.CreateInstance(original._vertexPositions.array, original._vertexPositions.name);
		}

		public new Manifold Clone()
		{
			return new Manifold(this);
		}

		public Vector3VertexAttribute vertexPositions { get { return _vertexPositions; } set { _vertexPositions = value; } }

		protected FacesIndexer GetFaces(bool includeExternalFaces)
		{
			return !includeExternalFaces ? internalFaces : faces;
		}

		protected FaceEdgesIndexer GetFaceEdges(bool includeExternalFaces)
		{
			return !includeExternalFaces ? internalFaceEdges : faceEdges;
		}

		public static Vector3[] CalculateFaceCentroids(FacesIndexer faces, Vector3[] vertexPositions)
		{
			var centroids = new Vector3[faces.Count];

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

		public Vector3[] CalculateFaceCentroids()
		{
			return CalculateFaceCentroids(false);
		}
		
		public virtual Vector3[] CalculateFaceCentroids(bool includeExternalFaces)
		{
			return CalculateFaceCentroids(GetFaces(includeExternalFaces), _vertexPositions.array);
		}

		public static Vector3[] CalculateVertexNormalsFromVertexPositions(VerticesIndexer vertices, Vector3[] vertexPositions)
		{
			var normals = new Vector3[vertices.Count];

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

				normals[vertex] = normalSum.normalized;
			}

			return normals;
		}

		public static Vector3[] CalculateVertexNormalsFromFacePositions(VerticesIndexer vertices, Vector3[] facePositions)
		{
			var normals = new Vector3[vertices.Count];

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

				normals[vertex] = normalSum.normalized;
			}

			return normals;
		}

		public static Vector3[] CalculateVertexNormalsFromFaceNormals(VerticesIndexer vertices, Vector3[] faceNormals)
		{
			var normals = new Vector3[vertices.Count];

			foreach (var vertex in vertices)
			{
				var normalSum = new Vector3(0f, 0f, 0f);

				foreach (var edge in vertex.edges)
				{
					normalSum += faceNormals[edge.prevFace];
				}

				normals[vertex] = normalSum.normalized;
			}

			return normals;
		}

		public static Vector3[] CalculateFaceNormalsFromFacePositions(FacesIndexer faces, Vector3[] facePositions)
		{
			var normals = new Vector3[faces.Count];

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

				normals[face] = normalSum.normalized;
			}

			return normals;
		}

		public static Vector3[] CalculateFaceNormalsFromVertexPositions(FacesIndexer faces, Vector3[] vertexPositions)
		{
			var normals = new Vector3[faces.Count];

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

				normals[face] = normalSum.normalized;
			}

			return normals;
		}

		public static Vector3[] CalculateFaceNormalsFromVertexNormals(FacesIndexer faces, Vector3[] vertexNormals)
		{
			var normals = new Vector3[faces.Count];

			foreach (var face in faces)
			{
				var normalSum = new Vector3(0f, 0f, 0f);

				foreach (var edge in face.edges)
				{
					normalSum += vertexNormals[edge.nextVertex];
				}

				normals[face] = normalSum.normalized;
			}

			return normals;
		}

		public Vector3[] CalculateFaceNormals()
		{
			return CalculateFaceNormals(false);
		}
		
		public virtual Vector3[] CalculateFaceNormals(bool includeExternalFaces)
		{
			var faces = GetFaces(includeExternalFaces);
			var normals = new Vector3[faces.Count];

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

		public float[] CalculateFaceAreas()
		{
			return CalculateFaceAreas(CalculateFaceCentroids(false), false);
		}
		
		public float[] CalculateFaceAreas(bool includeExternalFaces)
		{
			return CalculateFaceAreas(CalculateFaceCentroids(includeExternalFaces), includeExternalFaces);
		}

		public float[] CalculateFaceAreas(Vector3[] centroids)
		{
			return CalculateFaceAreas(centroids, false);
		}
		
		public virtual float[] CalculateFaceAreas(Vector3[] centroids, bool includeExternalFaces)
		{
			var faces = GetFaces(includeExternalFaces);
			var areas = new float[faces.Count];

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

		public virtual float[] CalculateTriangularFaceAreas(bool includeExternalFaces)
		{
			var faces = GetFaces(includeExternalFaces);
			var areas = new float[faces.Count];

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

		public float[] CalculateVertexAreas()
		{
			return CalculateVertexAreas(CalculateFaceCentroids(true));
		}
		
		public virtual float[] CalculateVertexAreas(Vector3[] centroids)
		{
			var areas = new float[vertices.Count];

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

		public virtual float[] CalculateVertexAngles()
		{
			var angles = new float[_edgeData.Length];

			foreach (var edge in vertexEdges)
			{
				var p0 = vertexPositions[edge.next.farVertex];
				var p1 = vertexPositions[edge.nearVertex];
				var p2 = vertexPositions[edge.farVertex];
				var v1 = (p1 - p0).normalized;
				var v2 = (p2 - p0).normalized;
				angles[edge] = SphericalManifoldUtility.AngleBetweenUnitVectors(v1, v2);
			}

			return angles;
		}

		public float[] CalculateFaceAngles()
		{
			return CalculateFaceAngles(false);
		}

		public virtual float[] CalculateFaceAngles(bool includeExternalFaces)
		{
			var edges = GetFaceEdges(includeExternalFaces);
			var angles = new float[edges.Count];

			foreach (var edge in edges)
			{
				var p0 = vertexPositions[edge.prev.prevVertex];
				var p1 = vertexPositions[edge.prevVertex];
				var p2 = vertexPositions[edge.nextVertex];
				var v1 = (p1 - p0).normalized;
				var v2 = (p2 - p0).normalized;
				angles[edge] = SphericalManifoldUtility.AngleBetweenUnitVectors(v1, v2);
			}

			return angles;
		}

		public float[] CalculateCentroidAngles()
		{
			return CalculateCentroidAngles(CalculateFaceCentroids(true));
		}

		public float[] CalculateCentroidAngles(Vector3[] centroids)
		{
			var angles = new float[_edgeData.Length];

			foreach (var edge in vertexEdges)
			{
				var p0 = centroids[edge.prevFace];
				var p1 = centroids[edge.nextFace];
				var p2 = centroids[edge.next.nextFace];
				var v1 = (p1 - p0).normalized;
				var v2 = (p2 - p0).normalized;
				angles[edge] = SphericalManifoldUtility.AngleBetweenUnitVectors(v1, v2);
			}

			return angles;
		}

		public virtual float[] CalculateVertexEdgeLengths()
		{
			var edgeLengths = new float[_edgeData.Length];

			foreach (var edge in vertexEdges)
			{
				edgeLengths[edge] = Vector3.Distance(vertexPositions[edge.nearVertex], _vertexPositions[edge.farVertex]);
			}

			return edgeLengths;
		}

		public virtual Vector3[] CalculateVertexEdgeMidpoints()
		{
			var midpoints = new Vector3[_edgeData.Length];

			foreach (var edge in vertexEdges)
			{
				midpoints[edge] = (vertexPositions[edge.nearVertex] + vertexPositions[edge.farVertex]) * 0.5f;
			}

			return midpoints;
		}

		public float[] CalculateFaceEdgeLengths()
		{
			return CalculateFaceEdgeLengths(CalculateFaceCentroids(false), Mathf.Infinity);
		}

		public float[] CalculateFaceEdgeLengths(float boundaryLength)
		{
			return CalculateFaceEdgeLengths(CalculateFaceCentroids(false), boundaryLength);
		}

		public float[] CalculateFaceEdgeLengths(Vector3[] centroids)
		{
			return CalculateFaceEdgeLengths(centroids, Mathf.Infinity);
		}

		public float[] CalculateFaceEdgeLengths(Vector3[] centroids, float boundaryLength)
		{
			var edgeLengths = new float[_edgeData.Length];

			foreach (var edge in faceEdges)
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

		public override void MakeDual()
		{
			MakeDual(CalculateFaceCentroids());
		}

		public virtual void MakeDual(Vector3[] facePositions)
		{
			base.MakeDual();
			_vertexPositions.array = facePositions;
		}

		public override Topology GetDualTopology()
		{
			return GetDualManifold();
		}

		public virtual Manifold GetDualManifold()
		{
			return GetDualManifold(CalculateFaceCentroids());
		}

		public virtual Manifold GetDualManifold(Vector3[] facePositions)
		{
			var clone = Clone();
			clone.MakeDual(facePositions);
			return clone;
		}
	}
}
