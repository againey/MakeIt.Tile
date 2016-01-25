using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public static class PlanarManifoldUtility
	{
		#region Generation

		public static void Generate(RowMajorQuadGridFaceNeighborIndexer neighborIndexer, RowMajorQuadGridVertexIndexer2D vertexIndexer, PlanarSurfaceDescriptor surfaceDescriptor, Vector3 origin, out Topology topology, out Vector3[] vertexPositions)
		{
			topology = TopologyBuilder.BuildTopoogy(neighborIndexer, "Topology");

			vertexPositions = new Vector3[vertexIndexer.vertexCount];

			for (int i = 0; i < vertexIndexer.vertexCount; ++i)
			{
				var index2D = vertexIndexer.GetVertexIndex2D(i);
				vertexPositions[i] =
					index2D.x * surfaceDescriptor.axis0.vector / neighborIndexer.faceColumnCount +
					index2D.y * surfaceDescriptor.axis1.vector / neighborIndexer.faceRowCount + origin;
			}
		}

		public static void Generate(WrappedRowMajorQuadGridFaceNeighborIndexer neighborIndexer, WrappedRowMajorQuadGridVertexIndexer2D vertexIndexer, PlanarSurfaceDescriptor surfaceDescriptor, Vector3 origin, out Topology topology, out Vector3[] vertexPositions, out EdgeWrap[] edgeWrapData)
		{
			topology = TopologyBuilder.BuildTopoogy(neighborIndexer, "Topology");

			vertexPositions = new Vector3[vertexIndexer.vertexCount];

			for (int i = 0; i < vertexIndexer.vertexCount; ++i)
			{
				var index2D = vertexIndexer.GetVertexIndex2D(i);
				vertexPositions[i] =
					index2D.x * surfaceDescriptor.axis0.vector / neighborIndexer.faceColumnCount +
					index2D.y * surfaceDescriptor.axis1.vector / neighborIndexer.faceRowCount + origin;
			}

			edgeWrapData = new EdgeWrap[neighborIndexer.edgeCount];

			for (int i = 0; i < neighborIndexer.faceCount; ++i)
			{
				for (int j = 0; j < neighborIndexer.GetNeighborCount(i); ++j)
				{
					edgeWrapData[neighborIndexer.GetNeighborEdgeIndex(i, j)] = neighborIndexer.GetEdgeWrapData(i, j);
				}
			}
		}

		#endregion

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
					var relaxedVertexPosition = vertexPositions[firstEdge.farVertex];
					var edge = firstEdge.next;
					while (edge != firstEdge)
					{
						relaxedVertexPosition += vertexPositions[edge.farVertex];
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

		public static IVertexAttribute<Vector3> RelaxVertexPositionsForRegularity(Topology topology, IEdgeAttribute<Vector3> vertexPositions, bool lockBoundaryPositions)
		{
			return RelaxVertexPositionsForRegularity(topology, vertexPositions, lockBoundaryPositions, new Vector3[topology.vertices.Count].AsVertexAttribute());
		}

		public static IVertexAttribute<Vector3> RelaxVertexPositionsForRegularity(Topology topology, IEdgeAttribute<Vector3> vertexPositions, bool lockBoundaryPositions, IVertexAttribute<Vector3> relaxedVertexPositions)
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
					vertexPositions[vertex] = (center * originalWeight + average * adjustmentWeight);
				}
			}

			return !repaired;
		}


		public static bool ValidateAndRepair(Topology topology, IVertexAttribute<Vector3> vertexPositions, IEdgeAttribute<Vector3> vertexNeighborPositions, float adjustmentWeight, bool lockBoundaryPositions)
		{
			bool repaired = false;
			float originalWeight = 1f - adjustmentWeight;
			foreach (var vertex in topology.vertices)
			{
				if (!lockBoundaryPositions || !vertex.hasExternalFaceNeighbor)
				{
					var center = vertexPositions[vertex] * 3f; // Multiply by 3 to not have to divide centroid sums by 3 below.
					var edge = vertex.firstEdge;
					var p0 = vertexNeighborPositions[edge];
					edge = edge.next;
					var p1 = vertexNeighborPositions[edge];
					edge = edge.next;
					var centroid0 = (center + p0 + p1);
					var firstEdge = edge;
					do
					{
						var p2 = vertexNeighborPositions[edge];
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
						average += vertexNeighborPositions[edge];
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
