using UnityEngine;
using System.Collections.Generic;
using Tiling;

[ExecuteInEditMode]
public class SubdividedDodecahdron : TilingGenerator
{
	public int SubdivisionDegree = 0;
	public int AlterationDegree = 0;
	public int MinimumPolygonSize = 3;
	public int MaximumPolygonSize = 8;
	public float AlterationFrequency = 0.1f;
	public int RandomSeed = 0;
	public float RelaxationRegularity = 0.5f;

	public RegularPolyhedron BasePolyhedron = RegularPolyhedron.Icosahedron;
	public bool UseDualPolyhedron = false;

	protected override void RebuildTiling(out Topology topology, out Topology.FaceAttribute<Vector3> facePositions, out Topology.VertexAttribute<Vector3> vertexPositions)
	{
		Manifold polyhedron;
		switch (BasePolyhedron)
		{
			case RegularPolyhedron.Tetrahedron: polyhedron = SphereTopology.CreateTetrahedron(); break;
			case RegularPolyhedron.Hexahedron: polyhedron = SphereTopology.CreateCube(); break;
			case RegularPolyhedron.Octahedron: polyhedron = SphereTopology.CreateOctahedron(); break;
			case RegularPolyhedron.Dodecahedron: throw new System.NotSupportedException("Using a dodecahedron as a base polyhedron is not currently supported.");
			case RegularPolyhedron.Icosahedron: polyhedron = SphereTopology.CreateIcosahedron(); break;
			default: throw new System.ArgumentException("A valid base polyhedron must be selected.");
		}

		if (SubdivisionDegree > 0)
		{
			polyhedron = SphereTopology.Subdivide(polyhedron, SubdivisionDegree);
		}

		if (!UseDualPolyhedron)
		{
			topology = polyhedron.topology;
			vertexPositions = polyhedron.vertexPositions;
		}
		else
		{
			topology = polyhedron.topology.GetDualTopology();
			vertexPositions = new Topology.VertexAttribute<Vector3>(topology.vertices.Count);

			foreach (var face in polyhedron.topology.faces)
			{
				var average = new Vector3();
				foreach (var edge in face.edges)
				{
					average += polyhedron.vertexPositions[edge.prevVertex];
				}
				vertexPositions[face.index] = average.normalized;
			}
		}

		facePositions = new Topology.FaceAttribute<Vector3>(topology.faces.Count);
		foreach (var face in topology.faces)
		{
			var average = new Vector3();
			foreach (var edge in face.edges)
			{
				average += vertexPositions[edge.nextVertex];
			}
			facePositions[face] = average / face.edges.Count;
		}

		/*if (AlterationDegree > 0 && AlterationFrequency > 0f)
		{
			var random = new System.Random(RandomSeed);
			TileAttribute<Vector3> regularityRelaxedPositions = new TileAttribute<Vector3>(localTilePositions.Count);
			TileAttribute<Vector3> areaRelaxedPositions = new TileAttribute<Vector3>(localTilePositions.Count);
			TileAttribute<Vector3> relaxedPositions = new TileAttribute<Vector3>(localTilePositions.Count);
			var idealArea = 4f * Mathf.PI;
			topology = topology.AlterTopology(AlterationDegree,
				delegate(Topology altered, Edge edge)
				{
					if (edge.Tiles[0].NeighborCount <= MinimumPolygonSize || edge.Tiles[1].NeighborCount <= MinimumPolygonSize) return false;
					if (edge.Corners[0].OppositeTile(edge).NeighborCount >= MaximumPolygonSize || edge.Corners[1].OppositeTile(edge).NeighborCount >= MaximumPolygonSize) return false;
					return random.NextDouble() < AlterationFrequency / AlterationDegree;
				},
				delegate(Topology altered)
				{
					float priorRelaxationAmount = 0f;
					for (int i = 0; i < 20; ++i)
					{
						TilingUtility.RelaxTilePositionsForRegularity(altered, localTilePositions, regularityRelaxedPositions);
						TilingUtility.RelaxTilePositionsForEqualArea(altered, localTilePositions, areaRelaxedPositions, idealArea / localTilePositions.Count);

						for (int j = 0; j < localTilePositions.Count; ++j)
						{
							relaxedPositions[j] = regularityRelaxedPositions[j] * RelaxationRegularity + areaRelaxedPositions[j] * (1f - RelaxationRegularity);
						}

						float relaxationAmount = 0f;
						for (int j = 0; j < localTilePositions.Count; ++j)
						{
							relaxationAmount += (localTilePositions[j] - relaxedPositions[j]).magnitude;
						}

						if (relaxationAmount == 0f || (priorRelaxationAmount != 0f && relaxationAmount / priorRelaxationAmount > 0.95f))
						{
							break;
						}

						Utility.Swap(ref localTilePositions, ref relaxedPositions);

						for (int j = 0; j < 20; ++j)
						{
							if (TilingUtility.ValidateAndRepairTilePositions(altered, localTilePositions, 0.5f))
							{
								break;
							}
						}
					}
				});
		}

		tilePositions = localTilePositions;

		cornerPositions = new CornerAttribute<Vector3>(topology.Corners.Count);
		foreach (var corner in topology.Corners)
		{
			cornerPositions[corner] = (
				tilePositions[corner.Tiles[0]] +
				tilePositions[corner.Tiles[1]] +
				tilePositions[corner.Tiles[2]]
			).normalized;
		}*/
	}
}
