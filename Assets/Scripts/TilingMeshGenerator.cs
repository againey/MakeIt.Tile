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
		else
		{
			if (_invalidated)
			{
				RebuildMeshes();
			}
		}
	}
#else
	void Update()
	{
		if (_invalidated)
		{
		}
	}
#endif

	void RebuildMeshes()
	{
		var meshes = gameObject.GetComponentsInChildren<UniqueMesh>();
		var meshIndex = 0;

		if (TilingGenerator != null && TilingGenerator.Topology != null)
		{
			var faceIndex = 0;
			var faceCount = TilingGenerator.Topology.faces.Count;
			while (faceIndex < faceCount)
			{
				var endFaceIndex = faceIndex;
				var meshVertexCount = 0;
				var meshTriangleCount = 0;
				while (endFaceIndex < faceCount)
				{
					var face = TilingGenerator.Topology.faces[endFaceIndex];
					var neighborCount = face.neighborCount;
					var faceVertexCount = neighborCount + 1;
					if (meshVertexCount + faceVertexCount > 65534) break;
					++endFaceIndex;
					meshVertexCount += faceVertexCount;
					meshTriangleCount += neighborCount;
				}

				Vector3[] vertices = new Vector3[meshVertexCount];
				Color[] colors = new Color[meshVertexCount];
				int[] triangles = new int[meshTriangleCount * 3];

				int meshVertex = 0;
				int meshTriangle = 0;

				while (faceIndex < endFaceIndex)
				{
					var face = TilingGenerator.Topology.faces[faceIndex];
					var edge = face.firstEdge;
					var neighborCount = face.neighborCount;
					vertices[meshVertex] = TilingGenerator.FacePositions[faceIndex];
					colors[meshVertex] = new Color(1, 1, 1);
					for (int j = 0; j < neighborCount; ++j, edge = edge.next)
					{
						vertices[meshVertex + j + 1] = TilingGenerator.VertexPositions[edge.nextVertex];
						colors[meshVertex + j + 1] = new Color(0, 0, 0);
						triangles[meshTriangle + j * 3 + 0] = meshVertex;
						triangles[meshTriangle + j * 3 + 1] = meshVertex + 1 + j;
						triangles[meshTriangle + j * 3 + 2] = meshVertex + 1 + (j + 1) % neighborCount;
					}
					meshVertex += neighborCount + 1;
					meshTriangle += neighborCount * 3;
					++faceIndex;
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
			if (this != null && gameObject != null)
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
