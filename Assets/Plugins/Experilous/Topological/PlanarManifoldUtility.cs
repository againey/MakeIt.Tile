using UnityEngine;

namespace Experilous.Topological
{
	public static class PlanarManifoldUtility
	{
		#region Modification

		public static void MakeDual(Topology topology, IVertexAttribute<Vector3> vertexPositions, out Vector3[] dualVertexPositions)
		{
			ManifoldUtility.MakeDual(topology, FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.faces, vertexPositions), out dualVertexPositions);
		}

		public static void MakeDual(Topology topology, ref Vector3[] vertexPositions)
		{
			ManifoldUtility.MakeDual(topology, FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.faces, vertexPositions.AsVertexAttribute()), out vertexPositions);
		}

		public static void GetDualManifold(Topology topology, IVertexAttribute<Vector3> vertexPositions, out Topology dualTopology, out Vector3[] dualVertexPositions)
		{
			dualTopology = (Topology)topology.Clone();
			MakeDual(dualTopology, vertexPositions, out dualVertexPositions);
		}

		public static IVertexAttribute<Vector3> RelaxVertexPositionsForRegularity(Topology topology, IVertexAttribute<Vector3> vertexPositions, bool lockBoundaryPositions)
		{
			return RelaxVertexPositionsForRegularity(topology, vertexPositions, lockBoundaryPositions, new Vector3[topology.vertices.Count].AsVertexAttribute());
		}

		public static IVertexAttribute<Vector3> RelaxVertexPositionsForRegularity(Topology topology, IVertexAttribute<Vector3> vertexPositions, bool lockBoundaryPositions, IVertexAttribute<Vector3> relaxedVertexPositions)
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
					
					relaxedVertexPositions[vertex] = relaxedVertexPosition / vertex.neighborCount;
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

		public static IVertexAttribute<Vector3> RelaxForEqualArea(Topology topology, IVertexAttribute<Vector3> vertexPositions, float totalArea, bool lockBoundaryPositions)
		{
			return RelaxForEqualArea(topology, vertexPositions, totalArea, lockBoundaryPositions, new Vector3[topology.vertices.Count].AsVertexAttribute());
		}

		public static IVertexAttribute<Vector3> RelaxForEqualArea(Topology topology, IVertexAttribute<Vector3> vertexPositions, float totalArea, bool lockBoundaryPositions, IVertexAttribute<Vector3> relaxedVertexPositions)
		{
			return RelaxVertexPositionsForEqualArea(topology, vertexPositions, totalArea, lockBoundaryPositions, relaxedVertexPositions, new Vector3[topology.internalFaces.Count].AsFaceAttribute(), new float[topology.vertices.Count].AsVertexAttribute());
		}

		public static IVertexAttribute<Vector3> RelaxVertexPositionsForEqualArea(Topology topology, IVertexAttribute<Vector3> vertexPositions, float totalArea, bool lockBoundaryPositions, IVertexAttribute<Vector3> relaxedVertexPositions, IFaceAttribute<Vector3> faceCentroids, IVertexAttribute<float> vertexAreas)
		{
			var idealArea = totalArea / topology.vertices.Count;

			FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions, faceCentroids);
			VertexAttributeUtility.CalculateVertexAreasFromVertexPositionsAndFaceCentroids(topology.vertices, vertexPositions, faceCentroids, vertexAreas);

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
					relaxedVertexPositions[vertex] /= vertex.neighborCount;
				}
				else
				{
					relaxedVertexPositions[vertex] = vertexPositions[vertex];
				}
			}

			return relaxedVertexPositions;
		}

		public static bool ValidateAndRepair(Topology topology, IVertexAttribute<Vector3> vertexPositions, float adjustmentWeight, bool lockBoundaryPositions)
		{
			bool repaired = false;
			float originalWeight = 1f - adjustmentWeight;
			foreach (var vertex in topology.vertices)
			{
				if (!lockBoundaryPositions || !vertex.hasExternalFaceNeighbor)
				{
					var center = vertexPositions[vertex] * 3f; // Multiply by 3 to not have to divide centroid sums by 3 below.
					var edge = vertex.firstEdge;
					var p0 = vertexPositions[edge];
					edge = edge.next;
					var p1 = vertexPositions[edge];
					edge = edge.next;
					var centroid0 = (center + p0 + p1);
					var firstEdge = edge;
					do
					{
						var p2 = vertexPositions[edge];
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
						average += vertexPositions[edge];
						edge = edge.next;
					} while (edge != firstEdge);
					average /= vertex.neighborCount;
					vertexPositions[vertex] = (center * originalWeight + average * adjustmentWeight);
				}
			}

			return !repaired;
		}

		#endregion
	}
}
