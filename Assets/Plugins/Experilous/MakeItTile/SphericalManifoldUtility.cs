/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System;
using Experilous.Numerics;

namespace Experilous.MakeItTile
{
	public static class SphericalManifoldUtility
	{
		#region Generation

		public static void CreateTetrahedron(SphericalSurface surface, out Topology topology, out Vector3[] vertexPositions)
		{
			var y = surface.radius * -1f / 3f;
			var z0 = surface.radius * 2f / 3f * Mathf.Sqrt(2f);
			var z1 = surface.radius * -Mathf.Sqrt(2f / 9f);
			var x = surface.radius * Mathf.Sqrt(2f / 3f);

			vertexPositions = new Vector3[4];
			vertexPositions[ 0] = new Vector3( 0, surface.radius,  0);
			vertexPositions[ 1] = new Vector3( 0,  y, z0);
			vertexPositions[ 2] = new Vector3(+x,  y, z1);
			vertexPositions[ 3] = new Vector3(-x,  y, z1);

			var orientation = surface.orientation;
			for (int i = 0; i < vertexPositions.Length; ++i)
			{
				vertexPositions[i] = orientation * vertexPositions[i];
			}

			var indexer = new ManualFaceNeighborIndexer(4, 12, 4);

			if (!surface.isInverted)
			{
				indexer.AddFace(0, 1, 2);
				indexer.AddFace(0, 2, 3);
				indexer.AddFace(0, 3, 1);
				indexer.AddFace(3, 2, 1);
			}
			else
			{
				indexer.AddFace(2, 1, 0);
				indexer.AddFace(3, 2, 0);
				indexer.AddFace(1, 3, 0);
				indexer.AddFace(1, 2, 3);
			}

			topology = TopologyUtility.BuildTopology(indexer);
		}

		public static void CreateCube(SphericalSurface surface, out Topology topology, out Vector3[] vertexPositions)
		{
			var a = surface.radius / Mathf.Sqrt(3f);

			vertexPositions = new Vector3[8];
			vertexPositions[ 0] = new Vector3(+a, +a, +a);
			vertexPositions[ 1] = new Vector3(+a, +a, -a);
			vertexPositions[ 2] = new Vector3(-a, +a, -a);
			vertexPositions[ 3] = new Vector3(-a, +a, +a);
			vertexPositions[ 4] = new Vector3(+a, -a, +a);
			vertexPositions[ 5] = new Vector3(+a, -a, -a);
			vertexPositions[ 6] = new Vector3(-a, -a, -a);
			vertexPositions[ 7] = new Vector3(-a, -a, +a);

			var orientation = surface.orientation;
			for (int i = 0; i < vertexPositions.Length; ++i)
			{
				vertexPositions[i] = orientation * vertexPositions[i];
			}

			var indexer = new ManualFaceNeighborIndexer(8, 24, 6);

			if (!surface.isInverted)
			{
				indexer.AddFace(0, 1, 2, 3);
				indexer.AddFace(0, 4, 5, 1);
				indexer.AddFace(1, 5, 6, 2);
				indexer.AddFace(2, 6, 7, 3);
				indexer.AddFace(3, 7, 4, 0);
				indexer.AddFace(7, 6, 5, 4);
			}
			else
			{
				indexer.AddFace(3, 2, 1, 0);
				indexer.AddFace(1, 5, 4, 0);
				indexer.AddFace(2, 6, 5, 1);
				indexer.AddFace(3, 7, 6, 2);
				indexer.AddFace(0, 4, 7, 3);
				indexer.AddFace(4, 5, 6, 7);
			}

			topology = TopologyUtility.BuildTopology(indexer);
		}

		public static void CreateOctahedron(SphericalSurface surface, out Topology topology, out Vector3[] vertexPositions)
		{
			vertexPositions = new Vector3[6];
			vertexPositions[ 0] = new Vector3( 0, +surface.radius,  0);
			vertexPositions[ 1] = new Vector3(+surface.radius,  0,  0);
			vertexPositions[ 2] = new Vector3( 0,  0, -surface.radius);
			vertexPositions[ 3] = new Vector3(-surface.radius,  0,  0);
			vertexPositions[ 4] = new Vector3( 0,  0, +surface.radius);
			vertexPositions[ 5] = new Vector3( 0, -surface.radius,  0);

			var orientation = surface.orientation;
			for (int i = 0; i < vertexPositions.Length; ++i)
			{
				vertexPositions[i] = orientation * vertexPositions[i];
			}

			var indexer = new ManualFaceNeighborIndexer(6, 24, 8);

			if (!surface.isInverted)
			{
				indexer.AddFace(0, 1, 2);
				indexer.AddFace(0, 2, 3);
				indexer.AddFace(0, 3, 4);
				indexer.AddFace(0, 4, 1);
				indexer.AddFace(2, 1, 5);
				indexer.AddFace(3, 2, 5);
				indexer.AddFace(4, 3, 5);
				indexer.AddFace(1, 4, 5);
			}
			else
			{
				indexer.AddFace(2, 1, 0);
				indexer.AddFace(3, 2, 0);
				indexer.AddFace(4, 3, 0);
				indexer.AddFace(1, 4, 0);
				indexer.AddFace(5, 1, 2);
				indexer.AddFace(5, 2, 3);
				indexer.AddFace(5, 3, 4);
				indexer.AddFace(5, 4, 1);
			}

			topology = TopologyUtility.BuildTopology(indexer);
		}

		public static void CreateDodecahedron(SphericalSurface surface, out Topology topology, out Vector3[] vertexPositions)
		{
			CreateIcosahedron(surface, out topology, out vertexPositions);
			MakeDual(surface, topology, ref vertexPositions);
		}

		public static void CreateIcosahedron(SphericalSurface surface, out Topology topology, out Vector3[] vertexPositions)
		{
			var latitude = Mathf.Atan2(1, 2);
			var longitude = Mathf.PI * 0.2f;
			var cosLat = Mathf.Cos(latitude);
			var scaledCosLat = surface.radius * cosLat;

			var x0 = 0.0f;
			var x1 = scaledCosLat * Mathf.Sin(longitude);
			var x2 = scaledCosLat * Mathf.Sin(longitude * 2.0f);
			var y0 = +surface.radius;
			var y1 = surface.radius * Mathf.Sin(latitude);
			var y2 = -surface.radius;
			var z0 = scaledCosLat;
			var z1 = scaledCosLat * Mathf.Cos(longitude);
			var z2 = scaledCosLat * Mathf.Cos(longitude * 2.0f);

			vertexPositions = new Vector3[12];
			vertexPositions[ 0] = new Vector3( x0,  y0,  0f);
			vertexPositions[ 1] = new Vector3( x0, +y1, -z0);
			vertexPositions[ 2] = new Vector3(-x2, +y1, -z2);
			vertexPositions[ 3] = new Vector3(-x1, +y1, +z1);
			vertexPositions[ 4] = new Vector3(+x1, +y1, +z1);
			vertexPositions[ 5] = new Vector3(+x2, +y1, -z2);
			vertexPositions[ 6] = new Vector3( x0, -y1, +z0);
			vertexPositions[ 7] = new Vector3(-x2, -y1, +z2);
			vertexPositions[ 8] = new Vector3(-x1, -y1, -z1);
			vertexPositions[ 9] = new Vector3(+x1, -y1, -z1);
			vertexPositions[10] = new Vector3(+x2, -y1, +z2);
			vertexPositions[11] = new Vector3( x0,  y2,  0f);

			var orientation = surface.orientation;
			for (int i = 0; i < vertexPositions.Length; ++i)
			{
				vertexPositions[i] = orientation * vertexPositions[i];
			}

			var indexer = new ManualFaceNeighborIndexer(12, 60, 20);

			if (!surface.isInverted)
			{
				indexer.AddFace( 0,  1,  2);
				indexer.AddFace( 0,  2,  3);
				indexer.AddFace( 0,  3,  4);
				indexer.AddFace( 0,  4,  5);
				indexer.AddFace( 0,  5,  1);
				indexer.AddFace( 1,  8,  2);
				indexer.AddFace( 2,  8,  7);
				indexer.AddFace( 2,  7,  3);
				indexer.AddFace( 3,  7,  6);
				indexer.AddFace( 3,  6,  4);
				indexer.AddFace( 4,  6, 10);
				indexer.AddFace( 4, 10,  5);
				indexer.AddFace( 5, 10,  9);
				indexer.AddFace( 5,  9,  1);
				indexer.AddFace( 1,  9,  8);
				indexer.AddFace(11,  6,  7);
				indexer.AddFace(11,  7,  8);
				indexer.AddFace(11,  8,  9);
				indexer.AddFace(11,  9, 10);
				indexer.AddFace(11, 10,  6);
			}
			else
			{
				indexer.AddFace( 2,  1,  0);
				indexer.AddFace( 3,  2,  0);
				indexer.AddFace( 4,  3,  0);
				indexer.AddFace( 5,  4,  0);
				indexer.AddFace( 1,  5,  0);
				indexer.AddFace( 2,  8,  1);
				indexer.AddFace( 7,  8,  2);
				indexer.AddFace( 3,  7,  2);
				indexer.AddFace( 6,  7,  3);
				indexer.AddFace( 4,  6,  3);
				indexer.AddFace(10,  6,  4);
				indexer.AddFace( 5, 10,  4);
				indexer.AddFace( 9, 10,  5);
				indexer.AddFace( 1,  9,  5);
				indexer.AddFace( 8,  9,  1);
				indexer.AddFace( 7,  6, 11);
				indexer.AddFace( 8,  7, 11);
				indexer.AddFace( 9,  8, 11);
				indexer.AddFace(10,  9, 11);
				indexer.AddFace( 6, 10, 11);
			}

			topology = TopologyUtility.BuildTopology(indexer);
		}

		#endregion

		#region Modification

		public static void MakeDual(SphericalSurface surface, Topology topology, IVertexAttribute<Vector3> vertexPositions, out Vector3[] dualVertexPositions)
		{
			ManifoldUtility.MakeDual(topology, FaceAttributeUtility.CalculateSphericalFaceCentroidsFromVertexPositions(topology.faces, vertexPositions, surface.radius), out dualVertexPositions);
		}

		public static void MakeDual(SphericalSurface surface, Topology topology, ref Vector3[] vertexPositions)
		{
			ManifoldUtility.MakeDual(topology, FaceAttributeUtility.CalculateSphericalFaceCentroidsFromVertexPositions(topology.faces, vertexPositions.AsVertexAttribute(), surface.radius), out vertexPositions);
		}

		public static void GetDualManifold(SphericalSurface surface, Topology topology, IVertexAttribute<Vector3> vertexPositions, out Topology dualTopology, out Vector3[] dualVertexPositions)
		{
			dualTopology = (Topology)topology.Clone();
			MakeDual(surface, dualTopology, vertexPositions, out dualVertexPositions);
		}

		public static void Subdivide(SphericalSurface surface, Topology topology, IVertexAttribute<Vector3> vertexPositions, int degree, out Topology subdividedTopology, out Vector3[] subdividedVertexPositions)
		{
			Func<Vector3, Vector3, float, Vector3> interpolator;
			if (surface.radius == 1f)
			{
				interpolator = (Vector3 p0, Vector3 p1, float t) => { return Geometry.SlerpUnitVectors(p0, p1, t); };
			}
			else
			{
				interpolator = (Vector3 p0, Vector3 p1, float t) => { return Geometry.SlerpUnitVectors(p0 / surface.radius, p1 / surface.radius, t) * surface.radius; };
			}
			ManifoldUtility.Subdivide(topology, vertexPositions, degree, interpolator, out subdividedTopology, out subdividedVertexPositions);
		}

		public static IVertexAttribute<Vector3> RelaxVertexPositionsForRegularity(SphericalSurface surface, Topology topology, IVertexAttribute<Vector3> vertexPositions, bool lockBoundaryPositions)
		{
			return RelaxVertexPositionsForRegularity(surface, topology, vertexPositions, lockBoundaryPositions, new Vector3[topology.vertices.Count].AsVertexAttribute());
		}

		public static IVertexAttribute<Vector3> RelaxVertexPositionsForRegularity(SphericalSurface surface, Topology topology, IVertexAttribute<Vector3> vertexPositions, bool lockBoundaryPositions, IVertexAttribute<Vector3> relaxedVertexPositions)
		{
			foreach (var vertex in topology.vertices)
			{
				if (!lockBoundaryPositions || !vertex.hasExternalFaceNeighbor)
				{
					var firstEdge = vertex.firstEdge;
					var relaxedVertexPosition = vertexPositions[firstEdge];
					var edge = firstEdge.next;
					while (edge != firstEdge)
					{
						relaxedVertexPosition += vertexPositions[edge];
						edge = edge.next;
					}
					
					relaxedVertexPositions[vertex] = relaxedVertexPosition.WithMagnitude(surface.radius);
				}
				else
				{
					relaxedVertexPositions[vertex] = vertexPositions[vertex];
				}
			}

			return relaxedVertexPositions;
		}

		public static IVertexAttribute<Vector3> RelaxForEqualArea(SphericalSurface surface, Topology topology, IVertexAttribute<Vector3> vertexPositions, bool lockBoundaryPositions)
		{
			return RelaxForEqualArea(surface, topology, vertexPositions, lockBoundaryPositions, new Vector3[topology.vertices.Count].AsVertexAttribute());
		}

		public static IVertexAttribute<Vector3> RelaxForEqualArea(SphericalSurface surface, Topology topology, IVertexAttribute<Vector3> vertexPositions, bool lockBoundaryPositions, IVertexAttribute<Vector3> relaxedVertexPositions)
		{
			return RelaxVertexPositionsForEqualArea(surface, topology, vertexPositions, lockBoundaryPositions, relaxedVertexPositions, new Vector3[topology.internalFaces.Count].AsFaceAttribute(), new float[topology.faceEdges.Count].AsEdgeAttribute(), new float[topology.vertices.Count].AsVertexAttribute());
		}

		public static IVertexAttribute<Vector3> RelaxVertexPositionsForEqualArea(SphericalSurface surface, Topology topology, IVertexAttribute<Vector3> vertexPositions, bool lockBoundaryPositions, IVertexAttribute<Vector3> relaxedVertexPositions, IFaceAttribute<Vector3> faceCentroids, IEdgeAttribute<float> faceCentroidAngles, IVertexAttribute<float> vertexAreas)
		{
			var idealArea = surface.radius * surface.radius * 4f * Mathf.PI / topology.vertices.Count;

			FaceAttributeUtility.CalculateSphericalFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions, surface.radius, faceCentroids);
			EdgeAttributeUtility.CalculateSphericalFaceCentroidAnglesFromFaceCentroids(topology.faceEdges, faceCentroids, faceCentroidAngles);
			VertexAttributeUtility.CalculateSphericalVertexAreasFromFaceCentroidAngles(topology.vertices, faceCentroidAngles, surface.radius, vertexAreas);

			for (int i = 0; i < topology.vertices.Count; ++i)
			{
				relaxedVertexPositions[i] = new Vector3(0f, 0f, 0f);
			}

			foreach (var vertex in topology.vertices)
			{
				var multiplier = Mathf.Sqrt(idealArea / vertexAreas[vertex]);
				foreach (var edge in vertex.edges)
				{
					var neighborVertex = edge.farVertex;
					var neighborRelativeCenter = vertexPositions[edge.twin];
					relaxedVertexPositions[neighborVertex] += (vertexPositions[neighborVertex] - neighborRelativeCenter) * multiplier + neighborRelativeCenter;
				}
			}

			foreach (var vertex in topology.vertices)
			{
				if (!lockBoundaryPositions || !vertex.hasExternalFaceNeighbor)
				{
					relaxedVertexPositions[vertex] = relaxedVertexPositions[vertex].WithMagnitude(surface.radius);
				}
				else
				{
					relaxedVertexPositions[vertex] = vertexPositions[vertex];
				}
			}

			return relaxedVertexPositions;
		}

		public static float CalculateRelaxationAmount(IVertexAttribute<Vector3> originalVertexPositions, IVertexAttribute<Vector3> relaxedVertexPositions)
		{
			float relaxationAmount = 0f;
			for (int i = 0; i < originalVertexPositions.Count; ++i)
			{
				relaxationAmount += (originalVertexPositions[i] - relaxedVertexPositions[i]).magnitude;
			}
			return relaxationAmount;
		}

		public static bool ValidateAndRepair(SphericalSurface surface, Topology topology, IVertexAttribute<Vector3> vertexPositions, float adjustmentWeight, bool lockBoundaryPositions)
		{
			bool repaired = false;
			float originalWeight = 1f - adjustmentWeight;
			foreach (var vertex in topology.vertices)
			{
				if (!lockBoundaryPositions || !vertex.hasExternalFaceNeighbor)
				{
					var center = vertexPositions[vertex];
					var surfaceNormal = surface.GetNormal(center);
					var edge = vertex.firstEdge;
					var p0 = vertexPositions[edge.farVertex];
					edge = edge.next;
					var p1 = vertexPositions[edge.farVertex];
					edge = edge.next;
					var centroid0 = (center + p0 + p1) / 3f;
					var firstEdge = edge;
					do
					{
						var p2 = vertexPositions[edge.farVertex];
						var centroid1 = (center + p1 + p2) / 3f;
						var normal = Vector3.Cross(centroid0 - center, centroid1 - center);
						if (Vector3.Dot(normal, surfaceNormal) < 0f) goto repair;
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
					vertexPositions[vertex] = (center * originalWeight + average * adjustmentWeight).WithMagnitude(surface.radius);
				}
			}

			return !repaired;
		}

		#endregion
	}
}
