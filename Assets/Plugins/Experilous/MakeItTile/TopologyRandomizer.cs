/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using Experilous.MakeItRandom;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// A static utiltiy class for randomly mutating a topology.
	/// </summary>
	public static class TopologyRandomizer
	{
		/// <summary>
		/// Randomly mutates the given topology by spinning randomly chosen edges.
		/// </summary>
		/// <param name="topology">The topology to be randomly mutated.</param>
		/// <param name="passCount">The number of randomization passes to execute.</param>
		/// <param name="frequency">The probability that a particular edge will be spun during a single randomization pass.</param>
		/// <param name="minVertexNeighbors">The minimum allowable number of neighbors a vertex can have.  No spin will be performed on an edge if it would violate this constraint.</param>
		/// <param name="maxVertexNeighbors">The maximum allowable number of neighbors a vertex can have.  No spin will be performed on an edge if it would violate this constraint.</param>
		/// <param name="minFaceNeighbors">The minimum allowable number of neighbors a face can have.  No spin will be performed on an edge if it would violate this constraint.</param>
		/// <param name="maxFaceNeighbors">The maximum allowable number of neighbors a face can have.  No spin will be performed on an edge if it would violate this constraint.</param>
		/// <param name="lockBoundaryVertices">Restricts vertices which touch an external face from being part of any edge mutations.</param>
		/// <param name="random">The random engine that will be used to randomly select the edges to spin and how to spin them.</param>
		/// <param name="relaxFunction">The relaxation and repair function to execute once after every randomization pass, to keep the vertex positions sane.</param>
		/// <seealso cref="O:Experilous.MakeItTile.Topology.SpinEdgeBackward"/>
		/// <seealso cref="O:Experilous.MakeItTile.Topology.SpinEdgeForward"/>
		public static void Randomize(Topology topology, int passCount, float frequency, int minVertexNeighbors, int maxVertexNeighbors, int minFaceNeighbors, int maxFaceNeighbors, bool lockBoundaryVertices, IRandom random, System.Action relaxFunction)
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
						var randomValue = random.FloatCO();
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
						if (random.FloatCO() < perPassRandomizationFrequency)
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

		/// <summary>
		/// Creates a relaxation loop function to be used by <see cref="Randomize"/> after each randomization pass to relax and repair vertex positions to account for topology mutations.
		/// </summary>
		/// <param name="maxRelaxPassCount">The maximum number of relaxation passes to perform each time the returned function object is invoked.</param>
		/// <param name="maxRepairPassCount">The maximum number of repair passes to perform each time the returned function object is invoked.</param>
		/// <param name="relativePrecision">The minimum amount of excessive relaxation change.  If the amount of change by a relaxation pass is under this threshold, relaxation will terminate early.</param>
		/// <param name="relaxIterationFunction">The function to call to perform a single relaxation pass.  It should return the amount of change produced by the relaxation.</param>
		/// <param name="repairFunction">The repair function to call to perform a single repair pass.  It should return true as long as at least one vertex needed to be repaired.  If this function returns false, repair will terminate early.</param>
		/// <returns>A function object that will perform multiple relaxation passes in a loop when invoked, and after each relaxation pass, multiple repair loops will also be executed in a nested loop.</returns>
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
