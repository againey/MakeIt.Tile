using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[ExecuteInEditMode]
	public class ManifoldMeshGenerator : MonoBehaviour
	{
		public ManifoldGenerator ManifoldGenerator;

		public UniqueMesh MeshPrefab;

	#if UNITY_EDITOR
		private Manifold _manifold = null;
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
				if (ManifoldGenerator.manifold != _manifold)
				{
					RebuildMeshes();
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

			if (ManifoldGenerator != null && ManifoldGenerator.manifold != null)
			{
				_manifold = ManifoldGenerator.manifold;

				var facePositions = new FaceAttribute<Vector3>(_manifold.topology.faces.Count);
				foreach (var face in _manifold.topology.faces)
				{
					var average = new Vector3();
					foreach (var edge in face.edges)
					{
						average += _manifold.vertexPositions[edge.nextVertex];
					}
					facePositions[face] = average / face.edges.Count;
				}

				var faceIndex = 0;
				var faceCount = _manifold.topology.faces.Count;
				while (faceIndex < faceCount)
				{
					var endFaceIndex = faceIndex;
					var meshVertexCount = 0;
					var meshTriangleCount = 0;
					while (endFaceIndex < faceCount)
					{
						var face = _manifold.topology.faces[endFaceIndex];
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
						var face = _manifold.topology.faces[faceIndex];
						var edge = face.firstEdge;
						var neighborCount = face.neighborCount;
						vertices[meshVertex] = facePositions[faceIndex];
						colors[meshVertex] = new Color(1, 1, 1);
						for (int j = 0; j < neighborCount; ++j, edge = edge.next)
						{
							vertices[meshVertex + j + 1] = _manifold.vertexPositions[edge.nextVertex];
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
						var uniqueMesh = Instantiate(MeshPrefab);
						uniqueMesh.name = "Mesh [" + meshIndex.ToString() + "]";
						uniqueMesh.transform.parent = transform;
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
			else
			{
				_manifold = null;
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
}
