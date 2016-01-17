using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public static class PlanarManifoldUtility
	{
		#region Generation

		public static void Generate(RowMajorQuadGridFaceNeighborIndexer neighborIndexer, RowMajorQuadGridVertexIndexer2D vertexIndexer, Vector3 rowAxis, Vector3 columnAxis, Vector3 origin, out Topology topology, out Vector3[] vertexPositions)
		{
			topology = TopologyBuilder.BuildTopoogy(neighborIndexer, "Topology");

			vertexPositions = new Vector3[vertexIndexer.vertexCount];

			for (int i = 0; i < vertexIndexer.vertexCount; ++i)
			{
				var index2D = vertexIndexer.GetVertexIndex2D(i);
				vertexPositions[i] = index2D.x * rowAxis + index2D.y * columnAxis + origin;
			}
		}

		#endregion

		#region Modification

		public static void MakeDual(Topology topology, ref IList<Vector3> vertexPositions)
		{
			ManifoldUtility.MakeDual(topology, FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.faces, vertexPositions), out vertexPositions);
		}

		public static void GetDualManifold(Topology topology, IList<Vector3> vertexPositions, out Topology dualTopology, out IList<Vector3> dualVertexPositions)
		{
			dualTopology = (Topology)topology.Clone();
			dualVertexPositions = vertexPositions;
			MakeDual(dualTopology, ref dualVertexPositions);
		}

		#endregion
	}
}
