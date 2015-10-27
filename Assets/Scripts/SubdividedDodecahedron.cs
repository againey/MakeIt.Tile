using UnityEngine;
using System.Collections.Generic;

public class SubdividedDodecahedron : MonoBehaviour
{
	public int SubdivisionDegree = 0;

	public Material Material;

	private readonly MeshTopology _icosahedronTopology;
	private readonly Vector3[] _icosahedronVertexPositions;

	public SubdividedDodecahedron()
	{
		BasicMeshTopology basicIcosahedron;
		SphereTopology.Icosahedron(out basicIcosahedron, out _icosahedronVertexPositions);
		_icosahedronTopology = new MeshTopology(basicIcosahedron);
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
		Vector3[] vertexPositions = null;
		var topology = new MeshTopology(_icosahedronTopology.Subdivide(SubdivisionDegree,
			delegate (int count)
			{
				vertexPositions = new Vector3[count];
			},
			delegate (int i0, int i1)
			{
				vertexPositions[i1] = _icosahedronVertexPositions[i0];
			},
			delegate (int i0, int i1, int i2, float t)
			{
				var p0 = vertexPositions[i0];
				var p1 = vertexPositions[i1];
				var omega = Mathf.Acos(Vector3.Dot(p0, p1));
				var d = Mathf.Sin(omega);
				var s0 = Mathf.Sin((1f - t) * omega);
				var s1 = Mathf.Sin(t * omega);
				vertexPositions[i2] = (p0 * s0 + p1 * s1) / d;
			}));

		Vector3[] relaxedPositions = new Vector3[vertexPositions.Length];
		int pass = 0;
		topology = topology.AlterTopology(3,
			delegate(MeshTopology altered, int edge)
			{
				//return Random.Range(0, 10) == 0;
				return ((edge + 1 + pass * 5) % 41 == 0 || (edge + 1 + pass * 5) % 43 == 0);
			},
			delegate(MeshTopology altered)
			{
				//return;
				++pass;
				float priorRelaxationAmount = 0f;
				for (int i = 0; i < 20; ++i)
				{
					altered.RelaxForRegularity(vertexPositions, relaxedPositions);

					float relaxationAmount = 0f;
					for (int j = 0; j < vertexPositions.Length; ++j)
					{
						relaxationAmount += (vertexPositions[j] - relaxedPositions[j]).magnitude;
					}

					if (relaxationAmount == 0f || (priorRelaxationAmount != 0f && relaxationAmount / priorRelaxationAmount > 0.95f))
					{
						break;
					}

					Utility.Swap(ref vertexPositions, ref relaxedPositions);

					for (int j = 0; j < 20; ++j)
					{
						if (altered.ValidateAndRepairPositions(vertexPositions, 0.5f))
						{
							break;
						}
					}
				}
			});

		var tileCount = topology._vertexNeighborOffsets.Length - 1;

		var centroids = new Vector3[topology._triangleVertices.GetLength(0)];
		for (int i = 0; i < centroids.Length; ++i)
		{
			centroids[i] = (
				vertexPositions[topology._triangleVertices[i, 0]] +
				vertexPositions[topology._triangleVertices[i, 1]] +
				vertexPositions[topology._triangleVertices[i, 2]]
			).normalized;
		}

		var tileCenters = new Vector3[tileCount];
		for (int i = 0; i < tileCount; ++i)
		{
			for (int j = topology._vertexNeighborOffsets[i]; j < topology._vertexNeighborOffsets[i + 1]; ++j)
			{
				tileCenters[i] += centroids[topology._vertexTriangles[j]];
			}
			tileCenters[i] /= topology._vertexNeighborOffsets[i + 1] - topology._vertexNeighborOffsets[i];
		}

		var meshes = gameObject.GetComponentsInChildren<UniqueMesh>();

		var tile = 0;
		var meshIndex = 0;
		while (tile < tileCount)
		{
			var endTile = tile;
			var vertexCount = 0;
			var triangleCount = 0;
			while (endTile < tileCount)
			{
				var tileNeighborCount = topology._vertexNeighborOffsets[endTile + 1] - topology._vertexNeighborOffsets[endTile];
				var tileVertexCount = tileNeighborCount + 1;
				if (vertexCount + tileVertexCount > 65534) break;
				++endTile;
				vertexCount += tileVertexCount;
				triangleCount += tileNeighborCount;
			}

			Vector3[] vertices = new Vector3[vertexCount];
			Color[] colors = new Color[vertexCount];
			int[] triangles = new int[triangleCount * 3];

			int vertex = 0;
			int triangle = 0;

			while (tile < endTile)
			{
				vertices[vertex] = tileCenters[tile];
				colors[vertex] = new Color(1, 1, 1);
				var neighborOffset = topology._vertexNeighborOffsets[tile];
				var neighborCount = topology._vertexNeighborOffsets[tile + 1] - neighborOffset;
				for (int j = 0; j < neighborCount; ++j)
				{
					vertices[vertex + j + 1] = centroids[topology._vertexTriangles[neighborOffset + j]];
					colors[vertex + j + 1] = new Color(0, 0, 0);
					triangles[triangle + j * 3 + 0] = vertex;
					triangles[triangle + j * 3 + 1] = vertex + 1 + j;
					triangles[triangle + j * 3 + 2] = vertex + 1 + (j + 1) % neighborCount;
				}
				vertex += neighborCount + 1;
				triangle += neighborCount * 3;
				++tile;
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
