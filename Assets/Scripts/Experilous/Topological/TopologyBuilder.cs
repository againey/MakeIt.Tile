using System;

namespace Experilous.Topological
{
	public static class TopologyBuilder
	{
		public static Topology BuildTopoogy(IFaceNeighborIndexer indexer, string name)
		{
			var topology = BuildTopoogy(indexer);
			topology.name = name;
			return topology;
		}

		public static Topology BuildTopoogy(IFaceNeighborIndexer indexer)
		{
			var vertexNeighborCounts = new ushort[indexer.vertexCount];
			var vertexFirstEdgeIndices = new int[indexer.vertexCount];
			var edgeData = new Topology.EdgeData[indexer.edgeCount];
			var faceNeighborCounts = new ushort[indexer.faceCount];
			var faceFirstEdgeIndices = new int[indexer.faceCount];

			for (int vertexIndex = 0; vertexIndex < indexer.vertexCount; ++vertexIndex)
			{
				vertexFirstEdgeIndices[vertexIndex] = indexer.edgeCount;
			}

			for (int edgeIndex = 0; edgeIndex < indexer.edgeCount; ++edgeIndex)
			{
				edgeData[edgeIndex]._twin = indexer.edgeCount;
			}

			for (int faceIndex = 0; faceIndex < indexer.faceCount; ++faceIndex)
			{
				var neighborCount = indexer.GetNeighborCount(faceIndex);
				faceNeighborCounts[faceIndex] = neighborCount;
				var nextEdgeIndex = indexer.GetNeighborEdgeIndex(faceIndex, 0);

				faceFirstEdgeIndices[faceIndex] = nextEdgeIndex;

				var priorTwinEdgeIndex = indexer.GetNeighborEdgeIndex(
					indexer.GetNeighborFaceIndex(faceIndex, neighborCount - 1),
					indexer.GetInverseNeighborIndex(faceIndex, neighborCount - 1));

				for (int neighborIndex = 0; neighborIndex < neighborCount; ++neighborIndex)
				{
					var neighborEdgeIndex = nextEdgeIndex;
					nextEdgeIndex = indexer.GetNeighborEdgeIndex(faceIndex, (neighborIndex + 1) % neighborCount);

					var neighborVertexIndex = indexer.GetNeighborVertexIndex(faceIndex, neighborIndex);
					var neighborFaceIndex = indexer.GetNeighborFaceIndex(faceIndex, neighborIndex);
					var inverseNeighborIndex = indexer.GetInverseNeighborIndex(faceIndex, neighborIndex);
					var twinEdgeIndex = indexer.GetNeighborEdgeIndex(neighborFaceIndex, inverseNeighborIndex);

					edgeData[neighborEdgeIndex] = new Topology.EdgeData(twinEdgeIndex, priorTwinEdgeIndex, nextEdgeIndex, neighborVertexIndex, neighborFaceIndex);

					if (vertexFirstEdgeIndices[neighborVertexIndex] > twinEdgeIndex)
					{
						vertexFirstEdgeIndices[neighborVertexIndex] = twinEdgeIndex;
					}

					priorTwinEdgeIndex = twinEdgeIndex;
				}
			}

			for (int edgeIndex = 0; edgeIndex < indexer.edgeCount; ++edgeIndex)
			{
				if (edgeData[edgeIndex]._twin == indexer.edgeCount) throw new InvalidOperationException("Not all edges were referenced by a face.");
			}

			for (int vertexIndex = 0; vertexIndex < indexer.vertexCount; ++vertexIndex)
			{
				var firstEdgeIndex = vertexFirstEdgeIndices[vertexIndex];
				if (firstEdgeIndex == indexer.edgeCount) throw new InvalidOperationException("Not all vertices were referenced by a face.");

				ushort neighborCount = 1;
				var edgeIndex = edgeData[firstEdgeIndex]._vNext;
				while (edgeIndex != firstEdgeIndex)
				{
					++neighborCount;
					edgeIndex = edgeData[edgeIndex]._vNext;
					if (neighborCount > vertexFirstEdgeIndices.Length)
					{
						throw new InvalidOperationException("Face neighbors were specified such that a face was misconfigured.");
					}
				}

				vertexNeighborCounts[vertexIndex] = neighborCount;
			}

			return Topology.CreateInstance(vertexNeighborCounts, vertexFirstEdgeIndices, edgeData, faceNeighborCounts, faceFirstEdgeIndices, indexer.internalEdgeCount, indexer.internalFaceCount);
		}
	}
}
