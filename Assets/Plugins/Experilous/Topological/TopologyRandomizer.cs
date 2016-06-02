/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using Experilous.Randomization;

namespace Experilous.Topological
{
	public static class TopologyRandomizer
	{
		public static void Randomize(Topology topology, int passCount, float frequency, int minVertexNeighbors, int maxVertexNeighbors, int minFaceNeighbors, int maxFaceNeighbors, bool lockBoundaryVertices, IRandomEngine random, System.Action relaxFunction)
		{
			var perPassRandomizationFrequency = frequency / passCount;

			for (int pass = 0; pass < passCount; ++pass)
			{
				foreach (var vertexEdge in topology.vertexEdges)
				{
					var twinVertexEdge = vertexEdge.twin;
					var faceEdge = vertexEdge.faceEdge;

					var verticesCanChange =
						minVertexNeighbors < maxVertexNeighbors &&
						topology.CanSpinEdgeForward(vertexEdge) &&
						(!lockBoundaryVertices || !vertexEdge.isBoundary && !faceEdge.isBoundary && !vertexEdge.nearVertex.hasExternalFaceNeighbor && !vertexEdge.farVertex.hasExternalFaceNeighbor) &&
						vertexEdge.farVertex.neighborCount > minVertexNeighbors &&
						twinVertexEdge.farVertex.neighborCount > minVertexNeighbors &&
						vertexEdge.faceEdge.next.nextVertex.neighborCount < maxVertexNeighbors &&
						twinVertexEdge.faceEdge.next.nextVertex.neighborCount < maxVertexNeighbors;

					var facesCanChange =
						minFaceNeighbors < maxFaceNeighbors &&
						topology.CanSpinEdgeForward(faceEdge) &&
						(!lockBoundaryVertices || !vertexEdge.isBoundary && !faceEdge.isBoundary && !vertexEdge.nearVertex.hasExternalFaceNeighbor && !vertexEdge.farVertex.hasExternalFaceNeighbor) &&
						(vertexEdge.prevFace.isExternal || vertexEdge.prevFace.neighborCount > minFaceNeighbors) &&
						(twinVertexEdge.prevFace.isExternal || twinVertexEdge.prevFace.neighborCount > minFaceNeighbors) &&
						(vertexEdge.next.nextFace.isExternal || vertexEdge.next.nextFace.neighborCount < maxFaceNeighbors) &&
						(twinVertexEdge.next.nextFace.isExternal || twinVertexEdge.next.nextFace.neighborCount < maxFaceNeighbors);

					if (verticesCanChange && facesCanChange)
					{
						var randomValue = RandomUnit.HalfOpenFloat(random);
						if (randomValue < perPassRandomizationFrequency)
						{
							if (randomValue < perPassRandomizationFrequency * 0.5f)
							{
								topology.SpinEdgeForward(vertexEdge);
							}
							else
							{
								topology.SpinEdgeForward(faceEdge);
							}
						}
					}
					else if (verticesCanChange || facesCanChange)
					{
						if (RandomUnit.HalfOpenFloat(random) < perPassRandomizationFrequency)
						{
							if (verticesCanChange)
							{
								topology.SpinEdgeForward(vertexEdge);
							}
							else
							{
								topology.SpinEdgeForward(faceEdge);
							}
						}
					}
				}

				if (relaxFunction != null) relaxFunction();
			}
		}

		public static System.Action CreateRelaxationLoopFunction(int maxRelaxPassCount, int maxRepairPassCount, float relativePrecision, System.Func<float> relaxIterationFunction, System.Func<bool> repairFunction)
		{
			return () =>
			{
				float priorRelaxationAmount = 0f;
				for (int i = 0; i < maxRelaxPassCount; ++i)
				{
					var relaxationAmount = relaxIterationFunction();

					if (relaxationAmount == 0f || (priorRelaxationAmount != 0f && 1f - relaxationAmount / priorRelaxationAmount < relativePrecision))
					{
						break;
					}

					priorRelaxationAmount = relaxationAmount;

					for (int j = 0; j < maxRepairPassCount; ++j)
					{
						if (repairFunction())
						{
							break;
						}
					}
				}
			};
		}
	}
}
