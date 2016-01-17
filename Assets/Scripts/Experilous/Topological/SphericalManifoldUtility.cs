using UnityEngine;
using System;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public static class SphericalManifoldUtility
	{
		#region Generation

		public static void CreateTetrahedron(float circumsphereRadius, out Topology topology, out IList<Vector3> vertexPositions)
		{
			var builder = new VertexVerticesBuilder(4, 12, 4);

			var y = circumsphereRadius * -1f / 3f;
			var z0 = circumsphereRadius * 2f / 3f * Mathf.Sqrt(2f);
			var z1 = circumsphereRadius * -Mathf.Sqrt(2f / 9f);
			var x = circumsphereRadius * Mathf.Sqrt(2f / 3f);

			vertexPositions = new Vector3[4];
			vertexPositions[ 0] = new Vector3( 0, circumsphereRadius,  0);
			vertexPositions[ 1] = new Vector3( 0,  y, z0);
			vertexPositions[ 2] = new Vector3(+x,  y, z1);
			vertexPositions[ 3] = new Vector3(-x,  y, z1);

			builder.AddVertex(1, 2, 3);
			builder.AddVertex(0, 3, 2);
			builder.AddVertex(0, 1, 3);
			builder.AddVertex(0, 2, 1);

			topology = builder.BuildTopology();
		}

		public static void CreateCube(float circumsphereRadius, out Topology topology, out IList<Vector3> vertexPositions)
		{
			var builder = new VertexVerticesBuilder(8, 24, 6);

			var a = circumsphereRadius / Mathf.Sqrt(3f);

			vertexPositions = new Vector3[8];
			vertexPositions[ 0] = new Vector3(+a, +a, +a);
			vertexPositions[ 1] = new Vector3(+a, +a, -a);
			vertexPositions[ 2] = new Vector3(-a, +a, -a);
			vertexPositions[ 3] = new Vector3(-a, +a, +a);
			vertexPositions[ 4] = new Vector3(+a, -a, +a);
			vertexPositions[ 5] = new Vector3(+a, -a, -a);
			vertexPositions[ 6] = new Vector3(-a, -a, -a);
			vertexPositions[ 7] = new Vector3(-a, -a, +a);

			builder.AddVertex(1, 3, 4);
			builder.AddVertex(0, 5, 2);
			builder.AddVertex(1, 6, 3);
			builder.AddVertex(0, 2, 7);
			builder.AddVertex(0, 7, 5);
			builder.AddVertex(1, 4, 6);
			builder.AddVertex(2, 5, 7);
			builder.AddVertex(3, 6, 4);

			topology = builder.BuildTopology();
		}

		public static void CreateOctahedron(float circumsphereRadius, out Topology topology, out IList<Vector3> vertexPositions)
		{
			var builder = new VertexVerticesBuilder(6, 24, 8);

			vertexPositions = new Vector3[6];
			vertexPositions[ 0] = new Vector3( 0, +circumsphereRadius,  0);
			vertexPositions[ 1] = new Vector3(+circumsphereRadius,  0,  0);
			vertexPositions[ 2] = new Vector3( 0,  0, -circumsphereRadius);
			vertexPositions[ 3] = new Vector3(-circumsphereRadius,  0,  0);
			vertexPositions[ 4] = new Vector3( 0,  0, +circumsphereRadius);
			vertexPositions[ 5] = new Vector3( 0, -circumsphereRadius,  0);

			builder.AddVertex(1, 2, 3, 4);
			builder.AddVertex(0, 4, 5, 2);
			builder.AddVertex(0, 1, 5, 3);
			builder.AddVertex(0, 2, 5, 4);
			builder.AddVertex(0, 3, 5, 1);
			builder.AddVertex(1, 4, 3, 2);

			topology = builder.BuildTopology();
		}

		public static void CreateDodecahedron(float circumsphereRadius, out Topology topology, out IList<Vector3> vertexPositions)
		{
			CreateIcosahedron(circumsphereRadius, out topology, out vertexPositions);
			MakeDual(topology, ref vertexPositions, circumsphereRadius);
		}

		public static void CreateIcosahedron(float circumsphereRadius, out Topology topology, out IList<Vector3> vertexPositions)
		{
			var builder = new VertexVerticesBuilder(12, 60, 20);

			var latitude = Mathf.Atan2(1, 2);
			var longitude = Mathf.PI * 0.2f;
			var cosLat = Mathf.Cos(latitude);
			var scaledCosLat = circumsphereRadius * cosLat;

			var x0 = scaledCosLat * Mathf.Sin(longitude * 2.0f);
			var x1 = scaledCosLat * Mathf.Sin(longitude);
			var x2 = 0.0f;
			var y0 = +circumsphereRadius;
			var y1 = circumsphereRadius * Mathf.Sin(latitude);
			var y2 = -circumsphereRadius;
			var z0 = scaledCosLat;
			var z1 = scaledCosLat * Mathf.Cos(longitude);
			var z2 = scaledCosLat * Mathf.Cos(longitude * 2.0f);

			vertexPositions = new Vector3[12];
			vertexPositions[ 0] = new Vector3(+x2,  y0,  0f);
			vertexPositions[ 1] = new Vector3(+x2, +y1, +z0);
			vertexPositions[ 2] = new Vector3(+x2, -y1, -z0);
			vertexPositions[ 3] = new Vector3(+x2,  y2,  0f);
			vertexPositions[ 4] = new Vector3(+x1, +y1, -z1);
			vertexPositions[ 5] = new Vector3(-x1, +y1, -z1);
			vertexPositions[ 6] = new Vector3(+x1, -y1, +z1);
			vertexPositions[ 7] = new Vector3(-x1, -y1, +z1);
			vertexPositions[ 8] = new Vector3(+x0, +y1, +z2);
			vertexPositions[ 9] = new Vector3(+x0, -y1, -z2);
			vertexPositions[10] = new Vector3(-x0, +y1, +z2);
			vertexPositions[11] = new Vector3(-x0, -y1, -z2);

			builder.AddVertex( 1,  8,  4,  5, 10);
			builder.AddVertex( 0, 10,  7,  6,  8);
			builder.AddVertex( 3, 11,  5,  4,  9);
			builder.AddVertex( 2,  9,  6,  7, 11);
			builder.AddVertex( 0,  8,  9,  2,  5);
			builder.AddVertex( 0,  4,  2, 11, 10);
			builder.AddVertex( 1,  7,  3,  9,  8);
			builder.AddVertex( 1, 10, 11,  3,  6);
			builder.AddVertex( 0,  1,  6,  9,  4);
			builder.AddVertex( 2,  4,  8,  6,  3);
			builder.AddVertex( 0,  5, 11,  7,  1);
			builder.AddVertex( 2,  3,  7, 10,  5);

			topology = builder.BuildTopology();
		}

		#endregion

		#region Modification

		public static void MakeDual(Topology topology, ref IList<Vector3> vertexPositions, float circumsphereRadius)
		{
			ManifoldUtility.MakeDual(topology, FaceAttributeUtility.CalculateSphericalFaceCentroidsFromVertexPositions(topology.faces, vertexPositions, circumsphereRadius), out vertexPositions);
		}

		public static void GetDualManifold(Topology topology, IList<Vector3> vertexPositions, float circumsphereRadius, out Topology dualTopology, out IList<Vector3> dualVertexPositions)
		{
			dualTopology = (Topology)topology.Clone();
			dualVertexPositions = vertexPositions;
			MakeDual(dualTopology, ref dualVertexPositions, circumsphereRadius);
		}

		public static void Subdivide(Topology topology, IList<Vector3> vertexPositions, int degree, float circumsphereRadius, out Topology subdividedTopology, out IList<Vector3> subdividedVertexPositions)
		{
			Func<Vector3, Vector3, float, Vector3> interpolator;
			if (circumsphereRadius == 1f)
			{
				interpolator = (Vector3 p0, Vector3 p1, float t) => { return MathUtility.SlerpUnitVectors(p0, p1, t); };
			}
			else
			{
				interpolator = (Vector3 p0, Vector3 p1, float t) => { return MathUtility.SlerpSphericalVectors(p0, p1, t, circumsphereRadius); };
			}
			ManifoldUtility.Subdivide(topology, vertexPositions, degree, interpolator, out subdividedTopology, out subdividedVertexPositions);
		}

		public static IList<Vector3> RelaxVertexPositionsForRegularity(Topology topology, IList<Vector3> vertexPositions, float circumsphereRadius, bool lockBoundaryPositions)
		{
			return RelaxVertexPositionsForRegularity(topology, vertexPositions, circumsphereRadius, lockBoundaryPositions, new Vector3[topology.vertices.Count]);
		}

		public static IList<Vector3> RelaxVertexPositionsForRegularity(Topology topology, IList<Vector3> vertexPositions, float circumsphereRadius, bool lockBoundaryPositions, IList<Vector3> relaxedVertexPositions)
		{
			foreach (var vertex in topology.vertices)
			{
				if (!lockBoundaryPositions || !vertex.hasExternalFaceNeighbor)
				{
					var firstEdge = vertex.firstEdge;
					var relaxedVertexPosition = vertexPositions[firstEdge.farVertex];
					var edge = firstEdge.next;
					while (edge != firstEdge)
					{
						relaxedVertexPosition += vertexPositions[edge.farVertex];
						edge = edge.next;
					}
					
					relaxedVertexPositions[vertex] = relaxedVertexPosition.Scaled(circumsphereRadius);
				}
				else
				{
					relaxedVertexPositions[vertex] = vertexPositions[vertex];
				}
			}

			return relaxedVertexPositions;
		}

		public static IList<Vector3> RelaxForEqualArea(Topology topology, IList<Vector3> vertexPositions, float circumsphereRadius, bool lockBoundaryPositions)
		{
			return RelaxForEqualArea(topology, vertexPositions, circumsphereRadius, lockBoundaryPositions, new Vector3[topology.vertices.Count]);
		}

		public static IList<Vector3> RelaxForEqualArea(Topology topology, IList<Vector3> vertexPositions, float circumsphereRadius, bool lockBoundaryPositions, IList<Vector3> relaxedVertexPositions)
		{
			return RelaxForEqualArea(topology, vertexPositions, circumsphereRadius, lockBoundaryPositions, relaxedVertexPositions, new Vector3[topology.vertices.Count], new float[topology.faceEdges.Count], new float[topology.vertices.Count]);
		}

		public static IList<Vector3> RelaxForEqualArea(Topology topology, IList<Vector3> vertexPositions, float circumsphereRadius, bool lockBoundaryPositions, IList<Vector3> relaxedVertexPositions, IList<Vector3> faceCentroids, IList<float> faceCentroidAngles, IList<float> vertexAreas)
		{
			var idealArea = 4f * Mathf.PI / topology.vertices.Count;

			FaceAttributeUtility.CalculateSphericalFaceCentroidsFromVertexPositions(topology.faces, vertexPositions, circumsphereRadius, faceCentroids);
			EdgeAttributeUtility.CalculateFaceCentroidAnglesFromFaceCentroids(topology.faceEdges, faceCentroids, faceCentroidAngles); //TODO: spherical angles?
			VertexAttributeUtility.CalculateSphericalVertexAreasFromFaceCentroidAngles(topology.vertices, faceCentroidAngles, circumsphereRadius, vertexAreas);

			for (int i = 0; i < topology.vertices.Count; ++i)
			{
				relaxedVertexPositions[i] = new Vector3(0f, 0f, 0f);
			}

			foreach (var vertex in topology.vertices)
			{
				var center = vertexPositions[vertex];
				var multiplier = Mathf.Sqrt(idealArea / vertexAreas[vertex]);
				foreach (var edge in vertex.edges)
				{
					relaxedVertexPositions[edge.farVertex] += (vertexPositions[edge.farVertex] - center) * multiplier + center;
				}
			}

			foreach (var vertex in topology.vertices)
			{
				if (!lockBoundaryPositions || !vertex.hasExternalFaceNeighbor)
				{
					relaxedVertexPositions[vertex] = relaxedVertexPositions[vertex].Scaled(circumsphereRadius);
				}
				else
				{
					relaxedVertexPositions[vertex] = vertexPositions[vertex];
				}
			}

			return relaxedVertexPositions;
		}

		public static bool ValidateAndRepair(Topology topology, IList<Vector3> vertexPositions, float circumsphereRadius, float adjustmentWeight, bool lockBoundaryPositions)
		{
			bool repaired = false;
			float originalWeight = 1f - adjustmentWeight;
			foreach (var vertex in topology.vertices)
			{
				if (!lockBoundaryPositions || !vertex.hasExternalFaceNeighbor)
				{
					var center = vertexPositions[vertex] * 3f; // Multiply by 3 to not have to divide centroid sums by 3 below.
					var edge = vertex.firstEdge;
					var p0 = vertexPositions[edge.farVertex];
					edge = edge.next;
					var p1 = vertexPositions[edge.farVertex];
					edge = edge.next;
					var centroid0 = (center + p0 + p1);
					var firstEdge = edge;
					do
					{
						var p2 = vertexPositions[edge.farVertex];
						var centroid1 = (center + p1 + p2);
						var normal = Vector3.Cross(centroid0 - center, centroid1 - center);
						if (Vector3.Dot(normal, center) < 0f) goto repair;
						p0 = p1;
						p1 = p2;
						centroid0 = centroid1;
						edge = edge.next;
					} while (edge != firstEdge);

					continue;

					repair: repaired = true;
					var average = new Vector3();
					edge = firstEdge;
					do
					{
						average += vertexPositions[edge.farVertex];
						edge = edge.next;
					} while (edge != firstEdge);
					average /= vertex.neighborCount;
					vertexPositions[vertex] = (center * originalWeight + average * adjustmentWeight).Scaled(circumsphereRadius);
				}
			}

			return !repaired;
		}

		#endregion
	}
}
