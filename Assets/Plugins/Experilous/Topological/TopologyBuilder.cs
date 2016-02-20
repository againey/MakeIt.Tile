﻿using System;

namespace Experilous.Topological
{
	/// <summary>
	/// Static utility class providing functions which can build a full topology data structure from input data.
	/// </summary>
	public static class TopologyBuilder
	{
		private static void AddEdgeToVertexUnordered(int edgeIndex, int vertexIndex, ushort[] vertexNeighborCounts, int[] vertexFirstEdgeIndices, Topology.EdgeData[] edgeData)
		{
			if (vertexNeighborCounts[vertexIndex] != 0)
			{
				var firstVertexEdge = vertexFirstEdgeIndices[vertexIndex];
				var vertexEdge = firstVertexEdge;
				var nextVertexEdge = edgeData[vertexEdge].vNext;
				while (nextVertexEdge != firstVertexEdge)
				{
					vertexEdge = nextVertexEdge;
					nextVertexEdge = edgeData[vertexEdge].vNext;
				}
				edgeData[vertexEdge].vNext = edgeIndex;
				edgeData[edgeIndex].vNext = firstVertexEdge;
				vertexNeighborCounts[vertexIndex] += 1;
			}
			else
			{
				vertexNeighborCounts[vertexIndex] = 1;
				vertexFirstEdgeIndices[vertexIndex] = edgeIndex;
				edgeData[edgeIndex].vNext = edgeIndex;
			}
		}

		private static void PutEdgesInSequence(int edge0Index, int edge1Index, Topology.EdgeData[] edgeData)
		{
			// Find out what comes immediately after edge0.
			var afterEdge0Index = edgeData[edge0Index].vNext;
			if (afterEdge0Index != edge1Index)
			{
				// If it's not edge1, then search for edge1 and the edge that immediately preceeds it.
				var beforeEdge1Index = afterEdge0Index;
				var nextEdgeIndex = edgeData[beforeEdge1Index].vNext;
				while (nextEdgeIndex != edge1Index)
				{
					beforeEdge1Index = nextEdgeIndex;
					nextEdgeIndex = edgeData[beforeEdge1Index].vNext;
				}
				// The decision must be made whether to move edge0 to just before edge1, or move edge1
				// to just after edge0.  The choice must avoid breaking an ordered relationship that
				// has already been established previously.  It is known that any edge pointing to an
				// edge that has an index smaller than edge1 has already been processed.  It is also
				// known that the only two relations we care about are between beforeEdge0 and edge0,
				// and edge1 and the edge after edge1.  The other two potential relations are not
				// actually possible because we're in the process of establishing those relationships
				// right now; they couldn't possibly have been established differently before.
				// What that boils down to is that if edge0 is less than edge1, then we cannot move
				// edge0 and must move edge1 instead; otherwise, it is safe to move edge0 (and
				// undetermined and irrelevant whether it is safe to move edge1).
				if (edge0Index < edge1Index)
				{
					// Move edge1 to the location immediately after edge0.
					edgeData[beforeEdge1Index].vNext = edgeData[edge1Index].vNext;
					edgeData[edge1Index].vNext = afterEdge0Index;
					edgeData[edge0Index].vNext = edge1Index;
				}
				else
				{
					// Move edge0 to the location immediately before edge1.
					// To do so, find the edge that immediately preceeds it.
					var beforeEdge0Index = edge1Index;
					nextEdgeIndex = edgeData[beforeEdge0Index].vNext;
					while (nextEdgeIndex != edge0Index)
					{
						beforeEdge0Index = nextEdgeIndex;
						nextEdgeIndex = edgeData[beforeEdge0Index].vNext;
					}
					edgeData[beforeEdge0Index].vNext = afterEdge0Index;
					edgeData[beforeEdge1Index].vNext = edge0Index;
					edgeData[edge0Index].vNext = edge1Index;
				}
			}
		}

		private static void BuildTopology(
			IFaceNeighborIndexer indexer,
			out ushort[] vertexNeighborCounts,
			out int[] vertexFirstEdgeIndices,
			out Topology.EdgeData[] edgeData,
			out ushort[] faceNeighborCounts,
			out int[] faceFirstEdgeIndices)
		{
			vertexNeighborCounts = new ushort[indexer.vertexCount];
			vertexFirstEdgeIndices = new int[indexer.vertexCount];
			edgeData = new Topology.EdgeData[indexer.edgeCount];
			faceNeighborCounts = new ushort[indexer.faceCount];
			faceFirstEdgeIndices = new int[indexer.faceCount];

			// Initialize the face roots and neighbor counts, and the next vertex, fNext, vNext, and wrap
			// fields of the internal edges.  The vNext fields will result in linked lists that contain the
			// correct members (aside from external edges), but in an unspecified order.
			int edgeIndex = 0;
			for (int faceIndex = 0; faceIndex < indexer.internalFaceCount; ++faceIndex)
			{
				var neighborCount = indexer.GetNeighborCount(faceIndex);
				faceNeighborCounts[faceIndex] = neighborCount;
				faceFirstEdgeIndices[faceIndex] = edgeIndex;

				var priorVertexIndex = indexer.GetNeighborVertexIndex(faceIndex, neighborCount - 1);
				for (int neighborIndex = 0; neighborIndex < neighborCount; ++neighborIndex)
				{
					var vertexIndex = indexer.GetNeighborVertexIndex(faceIndex, neighborIndex);
					edgeData[edgeIndex].vertex = vertexIndex;
					edgeData[edgeIndex].fNext = edgeIndex + 1;
					edgeData[edgeIndex].face = -1;
					edgeData[edgeIndex].twin = -1;

					AddEdgeToVertexUnordered(edgeIndex, priorVertexIndex, vertexNeighborCounts, vertexFirstEdgeIndices, edgeData);
					priorVertexIndex = vertexIndex;
					++edgeIndex;
				}

				// Correct the face's last edge to refer back to the face's first edge.
				edgeData[edgeIndex - 1].fNext = edgeIndex - neighborCount;
			}

			var internalEdgeCount = edgeIndex;

			// Use the partial information constructed in the prior loop to determine edge twins, and set
			// the vertex, fNext, vNext, and wrap fields for external edges too.
			edgeIndex = 0;
			var externalEdgeIndex = internalEdgeCount;
			for (int faceIndex = 0; faceIndex < indexer.internalFaceCount; ++faceIndex)
			{
				var neighborCount = indexer.GetNeighborCount(faceIndex);
				var priorVertexIndex = indexer.GetNeighborVertexIndex(faceIndex, neighborCount - 1);
				for (int neighborIndex = 0; neighborIndex < neighborCount; ++neighborIndex)
				{
					var vertexIndex = indexer.GetNeighborVertexIndex(faceIndex, neighborIndex);

					if (edgeData[edgeIndex].twin == -1)
					{
						// The current edge is pointing at the current vertex index.  Its twin will be one
						// of the edges pointing out from the current vertex, and will be pointing at the
						// prior vertex.  Search for this edge.
						var vertexFirstEdgeIndex = vertexFirstEdgeIndices[vertexIndex];
						var vertexEdgeIndex = vertexFirstEdgeIndex;
						while (edgeData[vertexEdgeIndex].vertex != priorVertexIndex)
						{
							vertexEdgeIndex = edgeData[vertexEdgeIndex].vNext;
							if (vertexEdgeIndex == vertexFirstEdgeIndex)
							{
								// This edge's twin is an external edge which needs to be initialized.
								edgeData[externalEdgeIndex].vertex = priorVertexIndex;
								edgeData[externalEdgeIndex].fNext = -1; // Cannot be determined until the vNext linked lists are in the correct order.
								edgeData[externalEdgeIndex].face = -1;

								AddEdgeToVertexUnordered(externalEdgeIndex, vertexIndex, vertexNeighborCounts, vertexFirstEdgeIndices, edgeData);

								vertexEdgeIndex = externalEdgeIndex;
								++externalEdgeIndex;
								break;
							}
						}
						edgeData[vertexEdgeIndex].twin = edgeIndex;
						edgeData[edgeIndex].twin = vertexEdgeIndex;
					}

					++edgeIndex;
					priorVertexIndex = vertexIndex;
				}
			}

			// Correct the order of the vNext linked lists, and set the face indices of the twins of all internal edges.
			edgeIndex = 0;
			for (int faceIndex = 0; faceIndex < indexer.internalFaceCount; ++faceIndex)
			{
				var neighborCount = indexer.GetNeighborCount(faceIndex);
				var priorEdgeIndex = edgeIndex + neighborCount - 1;
				for (int neighborIndex = 0; neighborIndex < neighborCount; ++neighborIndex)
				{
					var twinEdgeIndex = edgeData[priorEdgeIndex].twin;
					PutEdgesInSequence(edgeIndex, twinEdgeIndex, edgeData);

					edgeData[twinEdgeIndex].face = faceIndex;

					priorEdgeIndex = edgeIndex;
					++edgeIndex;
				}
			}

			if (indexer.externalFaceCount > 0)
			{
				// Now that all vertex and edge relationships are established, locate all edges that don't have a
				// face relationship specified (those which are pointing out toward external faces) and spin around
				// each face to establish those edge -> face relationships and count external face neighbors.
				var faceIndex = indexer.internalFaceCount;
				for (edgeIndex = 0; edgeIndex < indexer.edgeCount; ++edgeIndex)
				{
					if (edgeData[edgeIndex].face == -1)
					{
						// Starting with the current edge, follow the edge links appropriately to wind around the
						// implicit face counter-clockwise and link the edges to the face.
						var neighborCount = 0;
						var twinEdgeIndex = edgeIndex;
						var faceEdgeIndex = edgeData[edgeIndex].twin;
						var faceFirstEdgeIndex = faceEdgeIndex;
						do
						{
							edgeData[twinEdgeIndex].face = faceIndex;
							twinEdgeIndex = edgeData[faceEdgeIndex].vNext;
							var nextFaceEdgeIndex = edgeData[twinEdgeIndex].twin;
							edgeData[nextFaceEdgeIndex].fNext = faceEdgeIndex;
							faceEdgeIndex = nextFaceEdgeIndex;
							++neighborCount;
							if (neighborCount > vertexFirstEdgeIndices.Length) throw new InvalidOperationException("Face neighbors were specified such that an external face was misconfigured.");
						} while (twinEdgeIndex != edgeIndex);

						faceNeighborCounts[faceIndex] = (ushort)neighborCount;
						faceFirstEdgeIndices[faceIndex] = faceFirstEdgeIndex;
						++faceIndex;
					}
				}
			}
		}

		/// <summary>
		/// Build a full topology data structure from the minimal data described by a face neighbor indexer.
		/// </summary>
		/// <param name="indexer">A minimal description of the faces and their neighbors constituting a topology.</param>
		/// <returns>A fully constructed topology matching the description of the provided face neighbor indexer.</returns>
		/// <remarks>
		/// <para>For the edge wrap data returned by the face neighbor indexer, only the vertex-to-edge, edge-to-vertex,
		/// and face-to-edge data needs to be supplied.  If there are no external faces, then the edge-to-vertex data is
		/// also unnecessary.</para>
		/// </remarks>
		public static Topology BuildTopology(IFaceNeighborIndexer indexer)
		{
			ushort[] vertexNeighborCounts;
			int[] vertexFirstEdgeIndices;
			Topology.EdgeData[] edgeData;
			ushort[] faceNeighborCounts;
			int[] faceFirstEdgeIndices;

			BuildTopology(indexer, out vertexNeighborCounts, out vertexFirstEdgeIndices, out edgeData, out faceNeighborCounts, out faceFirstEdgeIndices);

			return Topology.Create(vertexNeighborCounts, vertexFirstEdgeIndices, edgeData, faceNeighborCounts, faceFirstEdgeIndices, indexer.internalFaceCount);
		}

		/// <summary>
		/// Build a full topology data structure from the minimal data described by a face neighbor indexer.
		/// </summary>
		/// <param name="surface"></param>
		/// <param name="indexer">A minimal description of the faces and their neighbors constituting a topology.</param>
		/// <returns>A fully constructed topology matching the description of the provided face neighbor indexer.</returns>
		/// <remarks>
		/// <para>For the edge wrap data returned by the face neighbor indexer, only the vertex-to-edge, edge-to-vertex,
		/// and face-to-edge data needs to be supplied.  If there are no external faces, then the edge-to-vertex data is
		/// also unnecessary.</para>
		/// </remarks>
		public static Topology BuildTopology(PlanarSurface surface, IPositionallyWrappedFaceNeighborIndexer indexer)
		{
			ushort[] vertexNeighborCounts;
			int[] vertexFirstEdgeIndices;
			Topology.EdgeData[] edgeData;
			ushort[] faceNeighborCounts;
			int[] faceFirstEdgeIndices;

			BuildTopology(indexer, out vertexNeighborCounts, out vertexFirstEdgeIndices, out edgeData, out faceNeighborCounts, out faceFirstEdgeIndices);

			EdgeWrap[] edgeWrap = new EdgeWrap[indexer.edgeCount];

			for (int edgeIndex = 0; edgeIndex < indexer.edgeCount; ++edgeIndex)
			{
				var twinEdgeIndex = edgeData[edgeIndex].twin;
				var nearFaceIndex = edgeData[twinEdgeIndex].face;
				if (nearFaceIndex < indexer.internalFaceCount)
				{
					var farVertex = edgeData[edgeIndex].vertex;
					var neighborIndex = 0;
					while (indexer.GetNeighborVertexIndex(nearFaceIndex, neighborIndex) != farVertex)
					{
						++neighborIndex;
					}

					edgeWrap[edgeIndex] = indexer.GetEdgeWrap(nearFaceIndex, neighborIndex);
				}
				else
				{
					edgeWrap[edgeIndex] = EdgeWrapUtility.Invert(edgeWrap[edgeIndex]);
				}

				if (edgeIndex < twinEdgeIndex)
				{
					EdgeWrapUtility.CrossMergeTwins(ref edgeWrap[edgeIndex], ref edgeWrap[twinEdgeIndex]);
				}
			}

			return PlanarPositionallyWrappedTopology.Create(surface, vertexNeighborCounts, vertexFirstEdgeIndices, edgeData, edgeWrap, faceNeighborCounts, faceFirstEdgeIndices, indexer.internalFaceCount);
		}
	}
}
