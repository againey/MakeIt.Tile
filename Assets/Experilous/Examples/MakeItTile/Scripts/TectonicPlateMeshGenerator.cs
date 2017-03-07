using UnityEngine;
using System.Collections.Generic;
using Experilous.MakeItRandom;
using Experilous.Numerics;
using Experilous.MakeItTile;
using Experilous.Topologies;

namespace Experilous.Examples.MakeItTile
{
#if UNITY_EDITOR
	[ExecuteInEditMode]
	[System.Serializable]
	[RequireComponent(typeof(GeneratorExecutive))]
	[RequireComponent(typeof(SphereGenerator))]
	[RequireComponent(typeof(PlanetGenerator))]
	public class TectonicPlateMeshGenerator : MonoBehaviour, IGenerator
	{
		public Material material;

		private SphereGenerator _sphereGenerator;
		private PlanetGenerator _planetGenerator;

		public IEnumerable<IGenerator> dependencies
		{
			get
			{
				yield return _sphereGenerator;
				yield return _planetGenerator;
			}
		}

		[SerializeField] [HideInInspector] private List<GameObject> _tectonicPlateGameObjects = new List<GameObject>();
		[SerializeField] [HideInInspector] private List<GameObject> _gameObjectPool = new List<GameObject>();

		protected void OnEnable()
		{
			_sphereGenerator = GetComponent<SphereGenerator>();
			_planetGenerator = GetComponent<PlanetGenerator>();
		}

		public void Generate()
		{
			var surface = _sphereGenerator.surface;
			var topology = _sphereGenerator.topology;
			var vertexPositions = _sphereGenerator.vertexPositions;
			var faceCentroids = _sphereGenerator.faceCentroids;
			
			var tectonicPlateColors = _planetGenerator.tectonicPlateColors;
			var tectonicPlateFirstFaceIndices = _planetGenerator.tectonicPlateFirstFaceIndices;
			var faceNextTectonicPlateFaceIndices = _planetGenerator.faceNextTectonicPlateFaceIndices;

			foreach (var gameObject in _tectonicPlateGameObjects)
			{
				if (gameObject)
				{
					gameObject.hideFlags = HideFlags.HideAndDontSave;
					gameObject.SetActive(false);
					_gameObjectPool.Add(gameObject);
				}
			}
			_tectonicPlateGameObjects.Clear();

			var meshTriangles = new List<int>(Mathf.Max(topology.faceEdges.Count * 3, 65534));
			var meshVertexPositions = new List<Vector3>(Mathf.Max(topology.faceEdges.Count + topology.faces.Count, 65534));
			var meshVertexNormals = new List<Vector3>(meshVertexPositions.Capacity);
			var meshVertexColors = new List<Color>(meshVertexPositions.Capacity);

			for (int tectonicPlateIndex = 0; tectonicPlateIndex < _planetGenerator.tectonicPlateCount; ++tectonicPlateIndex)
			{
				Vector3 positionSum = Vector3.zero;
				int faceCount = 0;
				var tectonicPlateColor = tectonicPlateColors[tectonicPlateIndex];
				var firstFace = new Topology.Face(topology, tectonicPlateFirstFaceIndices[tectonicPlateIndex]);

				var face = firstFace;
				do
				{
					if (meshVertexPositions.Count + face.neighborCount + 1 > 65534)
					{
						CreateMesh(positionSum / faceCount, meshTriangles, meshVertexPositions, meshVertexNormals, meshVertexColors);
						positionSum = Vector3.zero;
						faceCount = 0;
					}

					positionSum += faceCentroids[face];

					int firstVertexIndex = meshVertexPositions.Count;
					meshVertexPositions.Add(faceCentroids[face]);
					meshVertexNormals.Add(surface.GetNormal(faceCentroids[face]));
					meshVertexColors.Add(tectonicPlateColor);

					var edge = face.firstEdge;
					for (int i = 1; i <= face.neighborCount; ++i)
					{
						meshTriangles.Add(firstVertexIndex);
						meshTriangles.Add(firstVertexIndex + i);
						meshTriangles.Add(firstVertexIndex + i + 1);

						meshVertexPositions.Add(vertexPositions[edge]);
						meshVertexNormals.Add(surface.GetNormal(vertexPositions[edge]));
						meshVertexColors.Add(tectonicPlateColor);

						edge = edge.next;
					}

					meshTriangles[meshTriangles.Count - 1] = firstVertexIndex + 1;

					face = new Topology.Face(topology, faceNextTectonicPlateFaceIndices[face]);
					++faceCount;
				} while (face != firstFace);

				CreateMesh(positionSum / faceCount, meshTriangles, meshVertexPositions, meshVertexNormals, meshVertexColors);
			}
		}

		private void CreateMesh(Vector3 meshCenter, List<int> triangles, List<Vector3> vertexPositions, List<Vector3> vertexNormals, List<Color> vertexColors)
		{
			for (int i = 0; i < vertexPositions.Count; ++i)
			{
				vertexPositions[i] -= meshCenter;
			}

			var mesh = new Mesh();
			mesh.vertices = vertexPositions.ToArray();
			mesh.normals = vertexNormals.ToArray();
			mesh.colors = vertexColors.ToArray();
			mesh.triangles = triangles.ToArray();
			mesh.RecalculateBounds();

			GameObject gameObject = null;
			foreach (var pooledGameObject in _gameObjectPool)
			{
				if (pooledGameObject)
				{
					gameObject = pooledGameObject;
					break;
				}
			}

			if (gameObject == null)
			{
				gameObject = new GameObject("Tectonic Plate");
				var transform = gameObject.transform;
				transform.SetParent(this.transform, false);
				transform.localPosition = meshCenter;
				var meshFilter = gameObject.AddComponent<MeshFilter>();
				meshFilter.mesh = mesh;
				var meshRenderer = gameObject.AddComponent<MeshRenderer>();
				meshRenderer.material = material;
			}
			else
			{
				_gameObjectPool.Remove(gameObject);
				gameObject.hideFlags = HideFlags.None;
				gameObject.SetActive(true);
				gameObject.transform.localPosition = meshCenter;
				var meshFilter = gameObject.GetComponent<MeshFilter>();
				meshFilter.mesh = mesh;
				var meshRenderer = gameObject.GetComponent<MeshRenderer>();
				meshRenderer.material = material;
			}

			_tectonicPlateGameObjects.Add(gameObject);

			triangles.Clear();
			vertexPositions.Clear();
			vertexNormals.Clear();
			vertexColors.Clear();
		}
	}
#endif
}
