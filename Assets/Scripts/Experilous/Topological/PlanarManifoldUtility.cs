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

		#endregion
	}
}
