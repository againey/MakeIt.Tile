using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public static class PlanarManifoldUtility
	{
		#region Generation

		public static void Generate(RowMajorQuadGridFaceNeighborIndexer neighborIndexer, RowMajorQuadGridVertexIndexer2D vertexIndexer, Vector3 rowAxis, Vector3 columnAxis, Vector3 origin, out Topology topology, out IList<Vector3> vertexPositions)
		{
			topology = TopologyBuilder.BuildTopoogy(neighborIndexer, "Topology");

			vertexPositions = new Vector3[vertexIndexer.vertexCount];

			for (int i = 0; i < vertexIndexer.vertexCount; ++i)
			{
				var index2D = vertexIndexer.GetVertexIndex2D(i);
				vertexPositions[i] = index2D.x * rowAxis + index2D.y * columnAxis + origin;
			}
		}

		public static void Generate(WrappedRowMajorQuadGridFaceNeighborIndexer neighborIndexer, WrappedRowMajorQuadGridVertexIndexer2D vertexIndexer, Vector3 rowAxis, Vector3 columnAxis, Vector3 origin, out Topology topology, out IList<Vector3> vertexPositions, out IList<EdgeWrapData> edgeWrapData, out Vector3[] repetitionAxes)
		{
			topology = TopologyBuilder.BuildTopoogy(neighborIndexer, "Topology");

			vertexPositions = new Vector3[vertexIndexer.vertexCount];

			for (int i = 0; i < vertexIndexer.vertexCount; ++i)
			{
				var index2D = vertexIndexer.GetVertexIndex2D(i);
				vertexPositions[i] = index2D.x * rowAxis + index2D.y * columnAxis + origin;
			}

			edgeWrapData = new EdgeWrapData[neighborIndexer.edgeCount];

			for (int i = 0; i < neighborIndexer.faceCount; ++i)
			{
				for (int j = 0; j < neighborIndexer.GetNeighborCount(i); ++j)
				{
					edgeWrapData[neighborIndexer.GetNeighborEdgeIndex(i, j)] = neighborIndexer.GetEdgeWrapData(i, j);
				}
			}

			if (neighborIndexer.isWrappedHorizontally && neighborIndexer.isWrappedVertically)
			{
				repetitionAxes = new Vector3[3] { new Vector3(0f, 0f, 0f), rowAxis * neighborIndexer.vertexColumnCount, columnAxis * neighborIndexer.vertexRowCount };
			}
			else if (neighborIndexer.isWrappedHorizontally)
			{
				repetitionAxes = new Vector3[2] { new Vector3(0f, 0f, 0f), rowAxis * neighborIndexer.vertexColumnCount };
			}
			else //if (neighborIndexer.isWrappedVertically)
			{
				repetitionAxes = new Vector3[2] { new Vector3(0f, 0f, 0f), columnAxis * neighborIndexer.vertexRowCount };
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

		#endregion
	}
}
