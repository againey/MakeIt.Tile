using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[ExecuteInEditMode]
	public class PolyhedronGenerator : ManifoldGenerator
	{
		public int SubdivisionDegree = 0;
		public int AlterationDegree = 0;
		public int MinVertexNeighbors = 3;
		public int MaxVertexNeighbors = 8;
		public int MinFaceNeighbors = 3;
		public int MaxFaceNeighbors = 8;
		public int MaxRelaxIterations = 20;
		public int MaxRepairIterations = 20;
		public float AlterationFrequency = 0.1f;
		public int RandomSeed = 0;
		public float RelaxationRegularity = 0.5f;

		public RegularPolyhedron BasePolyhedron = RegularPolyhedron.Icosahedron;
		public bool UseDualPolyhedron = false;

		public enum RegularPolyhedron
		{
			Tetrahedron,
			Hexahedron,
			Octahedron,
			Icosahedron,
		}

		public void IncreaseSubdivision()
		{
			SubdivisionDegree += 1;
			Invalidate();
		}

		public void DecreaseSubdivision()
		{
			if (SubdivisionDegree > 0)
			{
				SubdivisionDegree -= 1;
				Invalidate();
			}
		}

		protected override Manifold RebuildManifold()
		{
			var stopwatch = new System.Diagnostics.Stopwatch();

			Manifold polyhedron;
			switch (BasePolyhedron)
			{
				case RegularPolyhedron.Tetrahedron: polyhedron = SphericalManifold.CreateTetrahedron(); break;
				case RegularPolyhedron.Hexahedron: polyhedron = SphericalManifold.CreateHexahedron(); break;
				case RegularPolyhedron.Octahedron: polyhedron = SphericalManifold.CreateOctahedron(); break;
				case RegularPolyhedron.Icosahedron: polyhedron = SphericalManifold.CreateIcosahedron(); break;
				default: throw new System.ArgumentException("A valid base polyhedron must be selected.");
			}

			if (SubdivisionDegree > 0)
			{
				stopwatch.Start();
				polyhedron = SphericalManifold.Subdivide(polyhedron, SubdivisionDegree);
				stopwatch.Stop();
				Debug.LogFormat("Subdivision Elapsed Time:  {0}", stopwatch.ElapsedMilliseconds);
				stopwatch.Reset();
			}

			if (AlterationDegree > 0 && AlterationFrequency > 0f)
			{
				var vertexPositions = polyhedron.vertexPositions;

				/*stopwatch.Start();
				var optimizedVertexPositions = new VertexAttribute<Vector3>(vertexPositions.Count);
				polyhedron.topology.OptimizeForVertexNeighborTraversal((int oldVertexIndex, int newVertexIndex) =>
				{
					optimizedVertexPositions[newVertexIndex] = vertexPositions[oldVertexIndex];
				},
				(int oldEdgeIndex, int newEdgeIndex) =>
				{
				});
				vertexPositions = polyhedron.vertexPositions = optimizedVertexPositions;
				stopwatch.Stop();
				Debug.LogFormat("Topology Optimizatoin Pass Time:  {0}", stopwatch.ElapsedMilliseconds);
				stopwatch.Reset();*/

				var random = RandomSeed > 0 ? new System.Random(RandomSeed) : new System.Random();

				var iterationAlterationFrequency = AlterationFrequency / AlterationDegree;

				var minVertex = UseDualPolyhedron ? MinFaceNeighbors : MinVertexNeighbors;
				var maxVertex = UseDualPolyhedron ? MaxFaceNeighbors : MaxVertexNeighbors;
				//var minFace = UseDualPolyhedron ? MinVertexNeighbors : MinFaceNeighbors;
				//var maxFace = UseDualPolyhedron ? MaxVertexNeighbors : MaxFaceNeighbors;

				var regularityRelaxedPositions = new VertexAttribute<Vector3>(vertexPositions.Count);
				var equalAreaRelaxedPositions = new VertexAttribute<Vector3>(vertexPositions.Count);
				var centroidsBuffer = new FaceAttribute<Vector3>(polyhedron.topology.faces.Count);

				var regularityWeight = RelaxationRegularity;
				var equalAreaWeight = (1f - RelaxationRegularity);

				for (int alterationPass = 0; alterationPass < AlterationDegree; ++alterationPass)
				{
					stopwatch.Start();
					foreach (var edge in polyhedron.topology.vertexEdges)
					{
						if (random.NextDouble() < iterationAlterationFrequency &&
							edge.nearVertex.neighborCount > minVertex && edge.farVertex.neighborCount > minVertex &&
							edge.prev.farVertex.neighborCount < maxVertex && edge.twin.prev.farVertex.neighborCount < maxVertex)
						{
							polyhedron.topology.SpinEdgeForward(edge);
						}
					}
					stopwatch.Stop();
					Debug.LogFormat("Alteration Pass Time:  {0}", stopwatch.ElapsedMilliseconds);
					stopwatch.Reset();

					float priorRelaxationAmount = 0f;
					for (int i = 0; i < MaxRelaxIterations; ++i)
					{
						stopwatch.Start();
						SphericalManifold.RelaxForRegularity(polyhedron, regularityRelaxedPositions);
						stopwatch.Stop();
						Debug.LogFormat("Relax for Regularity Pass Time:  {0}", stopwatch.ElapsedMilliseconds);
						stopwatch.Reset();

						stopwatch.Start();
						SphericalManifold.RelaxForEqualArea(polyhedron, equalAreaRelaxedPositions, centroidsBuffer);
						stopwatch.Stop();
						Debug.LogFormat("Relax for Equal Area Pass Time:  {0}", stopwatch.ElapsedMilliseconds);
						stopwatch.Reset();

						stopwatch.Start();
						float relaxationAmount = 0f;
						var weightedRelaxedPositions = regularityRelaxedPositions;
						for (int j = 0; j < vertexPositions.Count; ++j)
						{
							var weightedRelaxedPosition = regularityRelaxedPositions[j] * regularityWeight + equalAreaRelaxedPositions[j] * equalAreaWeight;
							relaxationAmount += (vertexPositions[j] - weightedRelaxedPosition).magnitude;
							weightedRelaxedPositions[j] = weightedRelaxedPosition;
						}
						stopwatch.Stop();
						Debug.LogFormat("Relax Average Pass Time:  {0}", stopwatch.ElapsedMilliseconds);
						stopwatch.Reset();

						if (relaxationAmount == 0f || (priorRelaxationAmount != 0f && relaxationAmount / priorRelaxationAmount > 0.95f))
						{
							break;
						}

						regularityRelaxedPositions = vertexPositions;
						vertexPositions = weightedRelaxedPositions;

						polyhedron.vertexPositions = vertexPositions;

						stopwatch.Start();
						for (int j = 0; j < MaxRepairIterations; ++j)
						{
							if (SphericalManifold.ValidateAndRepair(polyhedron, 0.5f))
							{
								break;
							}
						}
						stopwatch.Stop();
						Debug.LogFormat("Repair Time:  {0}", stopwatch.ElapsedMilliseconds);
						stopwatch.Reset();
					}
				}
			}

			Manifold manifold;

			if (!UseDualPolyhedron)
			{
				manifold = polyhedron;
			}
			else
			{
				stopwatch.Start();
				var topology = polyhedron.topology.GetDualTopology();
				var vertexPositions = new VertexAttribute<Vector3>(topology.vertices.Count);

				foreach (var face in polyhedron.topology.faces)
				{
					var average = new Vector3();
					foreach (var edge in face.edges)
					{
						average += polyhedron.vertexPositions[edge.prevVertex];
					}
					vertexPositions[face.index] = average.normalized;
				}

				manifold = new Manifold(topology, vertexPositions);
				stopwatch.Stop();
				Debug.LogFormat("Dual Polyhedron Time:  {0}", stopwatch.ElapsedMilliseconds);
				stopwatch.Reset();
			}

			return manifold;
		}
	}
}
