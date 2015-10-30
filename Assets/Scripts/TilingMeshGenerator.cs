using UnityEngine;
using System.Collections.Generic;
using Tiling;

[ExecuteInEditMode]
public class TilingMeshGenerator : MonoBehaviour
{
	public Material Material;
	public TilingGenerator TilingGenerator;

#if UNITY_EDITOR
	private Topology _topology = null;
#endif

	private bool _invalidated = true;

	public void Invalidate()
	{
		_invalidated = true;
	}

	void Start()
	{
	}

	void OnValidate()
	{
		Invalidate();
	}

#if UNITY_EDITOR
	void LateUpdate()
	{
		if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
		{
			if (TilingGenerator.Topology != _topology)
			{
				RebuildMeshes();
				_topology = TilingGenerator.Topology;
			}
		}
	}
#endif

	void RebuildMeshes()
	{
		var meshes = gameObject.GetComponentsInChildren<UniqueMesh>();
		var meshIndex = 0;

		if (TilingGenerator != null && TilingGenerator.Topology != null)
		{
			var flattenedTilePositions = new TileAttribute<Vector3>(TilingGenerator.Topology.Tiles.Count);
			foreach (var tile in TilingGenerator.Topology.Tiles)
			{
				foreach (var corner in tile.Corners)
				{
					flattenedTilePositions[tile] += TilingGenerator.CornerPositions[corner];
				}
				flattenedTilePositions[tile] /= tile.NeighborCount;
			}

			var tileIndex = 0;
			var tileCount = TilingGenerator.Topology.Tiles.Count;
			while (tileIndex < tileCount)
			{
				var endTileIndex = tileIndex;
				var vertexCount = 0;
				var triangleCount = 0;
				while (endTileIndex < tileCount)
				{
					var tile = TilingGenerator.Topology.Tiles[endTileIndex];
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
					var tile = TilingGenerator.Topology.Tiles[tileIndex];
					var corners = tile.Corners;
					var neighborCount = tile.NeighborCount;
					vertices[vertex] = flattenedTilePositions[tileIndex];
					colors[vertex] = new Color(1, 1, 1);
					for (int j = 0; j < neighborCount; ++j)
					{
						vertices[vertex + j + 1] = TilingGenerator.CornerPositions[corners[j]];
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
		}

#if UNITY_EDITOR
		UnityEditor.EditorApplication.delayCall += () =>
		{
			if (gameObject != null)
			{
#endif
				while (meshIndex < meshes.Length)
				{
					DestroyImmediate(meshes[meshIndex++].gameObject);
				}
#if UNITY_EDITOR
			}
		};
#endif

		_invalidated = false;
	}
}
