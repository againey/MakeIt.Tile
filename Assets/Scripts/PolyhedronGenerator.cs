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

		protected override Manifold RebuildManifold()
		{
			Manifold polyhedron;
			switch (BasePolyhedron)
			{
				case RegularPolyhedron.Tetrahedron: polyhedron = SphericalManifold.CreateTetrahedron(); break;
				case RegularPolyhedron.Hexahedron: polyhedron = SphericalManifold.CreateCube(); break;
				case RegularPolyhedron.Octahedron: polyhedron = SphericalManifold.CreateOctahedron(); break;
				case RegularPolyhedron.Icosahedron: polyhedron = SphericalManifold.CreateIcosahedron(); break;
				default: throw new System.ArgumentException("A valid base polyhedron must be selected.");
			}

			if (SubdivisionDegree > 0)
			{
				polyhedron = SphericalManifold.Subdivide(polyhedron, SubdivisionDegree);
			}

			if (AlterationDegree > 0 && AlterationFrequency > 0f)
			{
				var random = RandomSeed > 0 ? new System.Random(RandomSeed) : new System.Random();

				var iterationAlterationFrequency = AlterationFrequency / AlterationDegree;

				var minVertex = UseDualPolyhedron ? MinFaceNeighbors : MinVertexNeighbors;
				var maxVertex = UseDualPolyhedron ? MaxFaceNeighbors : MaxVertexNeighbors;
				//var minFace = UseDualPolyhedron ? MinVertexNeighbors : MinFaceNeighbors;
				//var maxFace = UseDualPolyhedron ? MaxVertexNeighbors : MaxFaceNeighbors;

				var vertexPositions = polyhedron.vertexPositions;
				var regularityRelaxedPositions = new VertexAttribute<Vector3>(vertexPositions.Count);
				var equalAreaRelaxedPositions = new VertexAttribute<Vector3>(vertexPositions.Count);
				var centroidsBuffer = new FaceAttribute<Vector3>(polyhedron.topology.faces.Count);

				var regularityWeight = RelaxationRegularity;
				var equalAreaWeight = (1f - RelaxationRegularity);

				for (int alterationPass = 0; alterationPass < AlterationDegree; ++alterationPass)
				{
					foreach (var edge in polyhedron.topology.vertexEdges)
					{
						if (random.NextDouble() < iterationAlterationFrequency &&
							edge.nearVertex.neighborCount > minVertex && edge.farVertex.neighborCount > minVertex &&
							edge.prev.farVertex.neighborCount < maxVertex && edge.twin.prev.farVertex.neighborCount < maxVertex)
						{
							polyhedron.topology.SpinEdgeForward(edge);
						}
					}

					float priorRelaxationAmount = 0f;
					for (int i = 0; i < MaxRelaxIterations; ++i)
					{
						SphericalManifold.RelaxForRegularity(polyhedron, regularityRelaxedPositions);
						SphericalManifold.RelaxForEqualArea(polyhedron, equalAreaRelaxedPositions, centroidsBuffer);

						float relaxationAmount = 0f;
						var weightedRelaxedPositions = regularityRelaxedPositions;
						for (int j = 0; j < vertexPositions.Count; ++j)
						{
							var weightedRelaxedPosition = regularityRelaxedPositions[j] * regularityWeight + equalAreaRelaxedPositions[j] * equalAreaWeight;
							relaxationAmount += (vertexPositions[j] - weightedRelaxedPosition).magnitude;
							weightedRelaxedPositions[j] = weightedRelaxedPosition;
						}

						if (relaxationAmount == 0f || (priorRelaxationAmount != 0f && relaxationAmount / priorRelaxationAmount > 0.95f))
						{
							break;
						}

						regularityRelaxedPositions = vertexPositions;
						vertexPositions = weightedRelaxedPositions;

						polyhedron.vertexPositions = vertexPositions;

						for (int j = 0; j < MaxRepairIterations; ++j)
						{
							if (SphericalManifold.ValidateAndRepair(polyhedron, 0.5f))
							{
								break;
							}
						}
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
			}

			return manifold;
		}
	}
}
