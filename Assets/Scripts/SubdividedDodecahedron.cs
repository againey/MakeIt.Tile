using UnityEngine;
using System.Collections.Generic;
using Tiling;

public class SubdividedDodecahedron : MonoBehaviour
{
	public int SubdivisionDegree = 0;
	public int AlterationDegree = 0;
	public int MinimumPolygonSize = 3;
	public int MaximumPolygonSize = 8;
	public float AlterationFrequency = 0.1f;
	public int RandomSeed = 0;
	public float RelaxationRegularity = 0.5f;

	public Material Material;

	private readonly Topology _icosahedronTopology;
	private readonly TileAttribute<Vector3> _icosahedronTilePositions;

	public SubdividedDodecahedron()
	{
		MinimalTopology basicIcosahedron;
		SphereTopology.Icosahedron(out basicIcosahedron, out _icosahedronTilePositions);
		_icosahedronTopology = new Topology(basicIcosahedron);
	}

	void OnValidate()
	{
		UnityEditor.EditorApplication.delayCall += () =>
		{
			if (this != null && gameObject != null) RebuildMeshes();
		};
	}

	void RebuildMeshes()
	{
		TileAttribute<Vector3> tilePositions = new TileAttribute<Vector3>();
		var topology = new Topology(_icosahedronTopology.Subdivide(SubdivisionDegree,
			delegate (int subdividedTileCount)
			{
				tilePositions = new TileAttribute<Vector3>(subdividedTileCount);
			},
			delegate (int i0, int i1)
			{
				tilePositions[i1] = _icosahedronTilePositions[i0];
			},
			delegate (int i0, int i1, int i2, float t)
			{
				var p0 = tilePositions[i0];
				var p1 = tilePositions[i1];
				var omega = Mathf.Acos(Vector3.Dot(p0, p1));
				var d = Mathf.Sin(omega);
				var s0 = Mathf.Sin((1f - t) * omega);
				var s1 = Mathf.Sin(t * omega);
				tilePositions[i2] = (p0 * s0 + p1 * s1) / d;
			}));

		var random = new System.Random(RandomSeed);
		TileAttribute<Vector3> regularityRelaxedPositions = new TileAttribute<Vector3>(tilePositions.Count);
		TileAttribute<Vector3> areaRelaxedPositions = new TileAttribute<Vector3>(tilePositions.Count);
		TileAttribute<Vector3> relaxedPositions = new TileAttribute<Vector3>(tilePositions.Count);
		var idealArea = 4f * Mathf.PI;
		int pass = 0;
		topology = topology.AlterTopology(AlterationDegree,
			delegate(Topology altered, Edge edge)
			{
				if (edge.Tiles[0].NeighborCount <= MinimumPolygonSize || edge.Tiles[1].NeighborCount <= MinimumPolygonSize) return false;
				if (edge.Corners[0].OppositeTile(edge).NeighborCount >= MaximumPolygonSize || edge.Corners[1].OppositeTile(edge).NeighborCount >= MaximumPolygonSize) return false;
				return random.NextDouble() < AlterationFrequency / AlterationDegree;
			},
			delegate(Topology altered)
			{
				//return;
				++pass;
				float priorRelaxationAmount = 0f;
				for (int i = 0; i < 20; ++i)
				{
					TilingUtility.RelaxTilePositionsForRegularity(altered, tilePositions, regularityRelaxedPositions);
					TilingUtility.RelaxTilePositionsForEqualArea(altered, tilePositions, areaRelaxedPositions, idealArea / tilePositions.Count);

					for (int j = 0; j < tilePositions.Count; ++j)
					{
						relaxedPositions[j] = regularityRelaxedPositions[j] * RelaxationRegularity + areaRelaxedPositions[j] * (1f - RelaxationRegularity);
					}

					float relaxationAmount = 0f;
					for (int j = 0; j < tilePositions.Count; ++j)
					{
						relaxationAmount += (tilePositions[j] - relaxedPositions[j]).magnitude;
					}

					if (relaxationAmount == 0f || (priorRelaxationAmount != 0f && relaxationAmount / priorRelaxationAmount > 0.95f))
					{
						break;
					}

					Utility.Swap(ref tilePositions, ref relaxedPositions);

					for (int j = 0; j < 20; ++j)
					{
						if (TilingUtility.ValidateAndRepairTilePositions(altered, tilePositions, 0.5f))
						{
							break;
						}
					}
				}
			});

		var cornerPositions = new CornerAttribute<Vector3>(topology.Corners.Count);
		foreach (var corner in topology.Corners)
		{
			cornerPositions[corner] = (
				tilePositions[corner.Tiles[0]] +
				tilePositions[corner.Tiles[1]] +
				tilePositions[corner.Tiles[2]]
			).normalized;
		}

		var flattenedTilePositions = new TileAttribute<Vector3>(topology.Tiles.Count);
		foreach (var tile in topology.Tiles)
		{
			foreach (var corner in tile.Corners)
			{
				flattenedTilePositions[tile] += cornerPositions[corner];
			}
			flattenedTilePositions[tile] /= tile.NeighborCount;
		}

		var meshes = gameObject.GetComponentsInChildren<UniqueMesh>();

		var tileIndex = 0;
		var meshIndex = 0;
		var tileCount = topology.Tiles.Count;
		while (tileIndex < tileCount)
		{
			var endTileIndex = tileIndex;
			var vertexCount = 0;
			var triangleCount = 0;
			while (endTileIndex < tileCount)
			{
				var tile = topology.Tiles[endTileIndex];
				var tileNeighborCount = tile.NeighborCount;
				var tileVertexCount = tileNeighborCount + 1;
				if (vertexCount + tileVertexCount > 65534) break;
				++endTileIndex;
				vertexCount += tileVertexCount;
				triangleCount += tileNeighborCount;
			}

			Vector3[] vertices = new Vector3[vertexCount];
			Color[] colors = new Color[vertexCount];
			int[] triangles = new int[triangleCount * 3];

			int vertex = 0;
			int triangle = 0;

			while (tileIndex < endTileIndex)
			{
				var tile = topology.Tiles[tileIndex];
				var corners = tile.Corners;
				var neighborCount = tile.NeighborCount;
				vertices[vertex] = flattenedTilePositions[tileIndex];
				colors[vertex] = new Color(1, 1, 1);
				for (int j = 0; j < neighborCount; ++j)
				{
					vertices[vertex + j + 1] = cornerPositions[corners[j]];
					colors[vertex + j + 1] = new Color(0, 0, 0);
					triangles[triangle + j * 3 + 0] = vertex;
					triangles[triangle + j * 3 + 1] = vertex + 1 + j;
					triangles[triangle + j * 3 + 2] = vertex + 1 + (j + 1) % neighborCount;
				}
				vertex += neighborCount + 1;
				triangle += neighborCount * 3;
				++tileIndex;
			}

			Mesh mesh;

			if (meshIndex >= meshes.Length)
			{
				var meshObject = new GameObject("Mesh [" + meshIndex.ToString() + "]");
				meshObject.transform.parent = transform;
				meshObject.AddComponent<MeshFilter>();
				var renderer = meshObject.AddComponent<MeshRenderer>();
				renderer.material = Material;
				var uniqueMesh = meshObject.AddComponent<UniqueMesh>();
				mesh = uniqueMesh.mesh;
			}
			else
			{
				mesh = meshes[meshIndex].mesh;
				mesh.Clear();
			}

			mesh.vertices = vertices;
			mesh.colors = colors;
			mesh.triangles = triangles;

			mesh.RecalculateBounds();

			++meshIndex;
		}

		while (meshIndex < meshes.Length)
		{
			DestroyImmediate(meshes[meshIndex++].gameObject);
		}
	}
}
