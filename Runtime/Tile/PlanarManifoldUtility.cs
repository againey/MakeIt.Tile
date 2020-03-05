/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;

namespace MakeIt.Tile
{
	/// <summary>
	/// Static utility class for working with planar manifolds, topological surfaces shaped as planes in three-dimensional space.
	/// </summary>
	public static class PlanarManifoldUtility
	{
		#region Modification

		/// <summary>
		/// Reverses the roles of vertices and faces, as when taking the dual of a polyhedron.
		/// </summary>
		/// <param name="topology">The topology containing the vertices and faces to swap.</param>
		/// <param name="vertexPositions">The original positions of the vertices.</param>
		/// <param name="dualVertexPositions">The positions of the vertices after the swap, calculated as the face centroids of the original topology.</param>
		public static void MakeDual(Topology topology, IVertexAttribute<Vector3> vertexPositions, out Vector3[] dualVertexPositions)
		{
			ManifoldUtility.MakeDual(topology, FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.faces, vertexPositions), out dualVertexPositions);
		}

		/// <summary>
		/// Reverses the roles of vertices and faces, as when taking the dual of a polyhedron.
		/// </summary>
		/// <param name="topology">The topology containing the vertices and faces to swap.</param>
		/// <param name="vertexPositions">The positions of the vertices, which will become the new positions after the call is complete, calculated as the face centroids of the original topology.</param>
		public static void MakeDual(Topology topology, ref Vector3[] vertexPositions)
		{
			ManifoldUtility.MakeDual(topology, FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.faces, vertexPositions.AsVertexAttribute()), out vertexPositions);
		}

		/// <summary>
		/// Creates a copy of the specified topology, but with the roles of vertices and faces reversed, as when taking the dual of a polyhedron.
		/// </summary>
		/// <param name="topology">The original topology containing the vertices and faces to swap.</param>
		/// <param name="vertexPositions">The original positions of the vertices.</param>
		/// <param name="dualTopology">The copied topology with the vertices and faces swapped.</param>
		/// <param name="dualVertexPositions">The positions of the vertices after the swap, calculated as the face centroids of the original topology.</param>
		public static void GetDualManifold(Topology topology, IVertexAttribute<Vector3> vertexPositions, out Topology dualTopology, out Vector3[] dualVertexPositions)
		{
			dualTopology = (Topology)topology.Clone();
			MakeDual(dualTopology, vertexPositions, out dualVertexPositions);
		}

		/// <summary>
		/// Attempts to move the positions of vertices such that they have roughly uniform density, with a bias towards also making sure that the shapes of the faces become more regular.
		/// </summary>
		/// <param name="topology">The topology to relax.</param>
		/// <param name="vertexPositions">The original positions of the vertices to relax.</param>
		/// <param name="lockBoundaryPositions">Indicates that vertices with an external neighboring face should not have their positions altered.</param>
		/// <returns>The relaxed vertex positions.</returns>
		public static IVertexAttribute<Vector3> RelaxVertexPositionsForRegularity(Topology topology, IVertexAttribute<Vector3> vertexPositions, bool lockBoundaryPositions)
		{
			return RelaxVertexPositionsForRegularity(topology, vertexPositions, lockBoundaryPositions, new Vector3[topology.vertices.Count].AsVertexAttribute());
		}

		/// <summary>
		/// Attempts to move the positions of vertices such that they have roughly uniform density, with a bias towards also making sure that the shapes of the faces become more regular.
		/// </summary>
		/// <param name="topology">The topology to relax.</param>
		/// <param name="vertexPositions">The original positions of the vertices to relax.</param>
		/// <param name="lockBoundaryPositions">Indicates that vertices with an external neighboring face should not have their positions altered.</param>
		/// <param name="relaxedVertexPositions">A pre-allocated collection in which the relaxed vertex positions will be stored.  Should not be the same collection as <paramref name="vertexPositions"/>.</param>
		/// <returns>The relaxed vertex positions.</returns>
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

		/// <summary>
		/// Attempts to move the positions of vertices such that they have roughly uniform density, with a bias towards also making sure that the surface areas of the faces also become more uniform.
		/// </summary>
		/// <param name="topology">The topology to relax.</param>
		/// <param name="vertexPositions">The original positions of the vertices to relax.</param>
		/// <param name="totalArea">The total surface area of the internal faces of the topology.</param>
		/// <param name="lockBoundaryPositions">Indicates that vertices with an external neighboring face should not have their positions altered.</param>
		/// <returns>The relaxed vertex positions.</returns>
		public static IVertexAttribute<Vector3> RelaxForEqualArea(Topology topology, IVertexAttribute<Vector3> vertexPositions, float totalArea, bool lockBoundaryPositions)
		{
			return RelaxForEqualArea(topology, vertexPositions, totalArea, lockBoundaryPositions, new Vector3[topology.vertices.Count].AsVertexAttribute());
		}

		/// <summary>
		/// Attempts to move the positions of vertices such that they have roughly uniform density, with a bias towards also making sure that the surface areas of the faces also become more uniform.
		/// </summary>
		/// <param name="topology">The topology to relax.</param>
		/// <param name="vertexPositions">The original positions of the vertices to relax.</param>
		/// <param name="totalArea">The total surface area of the internal faces of the topology.</param>
		/// <param name="lockBoundaryPositions">Indicates that vertices with an external neighboring face should not have their positions altered.</param>
		/// <param name="relaxedVertexPositions">A pre-allocated collection in which the relaxed vertex positions will be stored.  Should not be the same collection as <paramref name="vertexPositions"/>.</param>
		/// <returns>The relaxed vertex positions.</returns>
		public static IVertexAttribute<Vector3> RelaxForEqualArea(Topology topology, IVertexAttribute<Vector3> vertexPositions, float totalArea, bool lockBoundaryPositions, IVertexAttribute<Vector3> relaxedVertexPositions)
		{
			return RelaxVertexPositionsForEqualArea(topology, vertexPositions, totalArea, lockBoundaryPositions, relaxedVertexPositions, new Vector3[topology.internalFaces.Count].AsFaceAttribute(), new float[topology.vertices.Count].AsVertexAttribute());
		}

		/// <summary>
		/// Attempts to move the positions of vertices such that they have roughly uniform density, with a bias towards also making sure that the surface areas of the faces also become more uniform.
		/// </summary>
		/// <param name="topology">The topology to relax.</param>
		/// <param name="vertexPositions">The original positions of the vertices to relax.</param>
		/// <param name="totalArea">The total surface area of the internal faces of the topology.</param>
		/// <param name="lockBoundaryPositions">Indicates that vertices with an external neighboring face should not have their positions altered.</param>
		/// <param name="relaxedVertexPositions">A pre-allocated collection in which the relaxed vertex positions will be stored.  Should not be the same collection as <paramref name="vertexPositions"/>.</param>
		/// <param name="faceCentroids">A pre-allocated collection in which the intermediate face centroid positions will be stored.</param>
		/// <param name="vertexAreas">A pre-allocated collection in which the intermediate nearby surface areas of vertices will be stored.</param>
		/// <returns>The relaxed vertex positions.</returns>
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
					var neighborVertex = edge.vertex;
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

		/// <summary>
		/// Determines the total amount of change between original vertex positions and relaxed vertex positions.
		/// </summary>
		/// <param name="originalVertexPositions">The original vertex positions, before being relaxed.</param>
		/// <param name="relaxedVertexPositions">The relaxed vertex positions.</param>
		/// <returns>The total amount of change between the original and relaxed vertex positions, as a sum of distance deltas.</returns>
		public static float CalculateRelaxationAmount(IVertexAttribute<Vector3> originalVertexPositions, IVertexAttribute<Vector3> relaxedVertexPositions)
		{
			float relaxationAmount = 0f;
			for (int i = 0; i < originalVertexPositions.Count; ++i)
			{
				relaxationAmount += (originalVertexPositions[i] - relaxedVertexPositions[i]).magnitude;
			}
			return relaxationAmount;
		}

		/// <summary>
		/// Scans the topology's vertices and their positions for any egregious anomolies, such as inverted triangles, and attempts to correct them when encountered.
		/// </summary>
		/// <param name="topology">The topology to validate and repair.</param>
		/// <param name="surfaceNormal">The surface normal of the manifold.</param>
		/// <param name="vertexPositions">The topology vertex positions, which will be modified by this function.</param>
		/// <param name="adjustmentWeight">The degree to which final repaired vertex positions should conform to the ideal computed positions, moving away from the original positions.</param>
		/// <param name="lockBoundaryPositions">Indicates that vertices with an external neighboring face should not have their positions altered.</param>
		/// <returns>True if all vertex positions were validated and thus not changed, and false if at least one vertex failed validation and had its position at least partially repaired.</returns>
		public static bool ValidateAndRepair(Topology topology, Vector3 surfaceNormal, IVertexAttribute<Vector3> vertexPositions, float adjustmentWeight, bool lockBoundaryPositions)
		{
			bool repaired = false;
			float originalWeight = 1f - adjustmentWeight;
			foreach (var vertex in topology.vertices)
			{
				if (!lockBoundaryPositions || !vertex.hasExternalFaceNeighbor)
				{
					var center = vertexPositions[vertex];
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
						var localNormal = Vector3.Cross(centroid0 - center, centroid1 - center);
						if (Vector3.Dot(localNormal, surfaceNormal) < 0f) goto repair;
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
