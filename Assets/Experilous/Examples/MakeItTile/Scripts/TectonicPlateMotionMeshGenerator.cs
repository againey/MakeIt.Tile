using UnityEngine;
using System.Collections.Generic;
using Experilous.MakeItRandom;
using Experilous.Numerics;
using Experilous.MakeItTile;

namespace Experilous.Examples.MakeItTile
{
#if UNITY_EDITOR
	[ExecuteInEditMode]
	[System.Serializable]
	[RequireComponent(typeof(GeneratorExecutive))]
	[RequireComponent(typeof(SphereGenerator))]
	[RequireComponent(typeof(PlanetGenerator))]
	public class TectonicPlateMotionMeshGenerator : MonoBehaviour, IGenerator
	{
		public Material material;
		[Range(0.1f, 4f)]
		public float scale = 1f;
		
		[Range(0f, 1f)]
		public float slice = 0.5f;

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
			
			var faceTectonicPlateMotions = _planetGenerator.faceTectonicPlateMotions;
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

			var meshTriangles = new List<int>(Mathf.Max(topology.internalFaces.Count * 18, 65534));
			var meshVertexPositions = new List<Vector3>(Mathf.Max(topology.internalFaces.Count * 8, 65534));
			var meshVertexNormals = new List<Vector3>(meshVertexPositions.Capacity);
			var meshVertexUVs = new List<Vector2>(meshVertexPositions.Capacity);

			var totalArea = 4f * surface.radius * surface.radius;
			var averageAreaPerFace = totalArea / topology.internalFaces.Count;
			var scaledAverageDiameter = Mathf.Sqrt(averageAreaPerFace) * scale;
			var minLength = scaledAverageDiameter / 2f;
			var maxLength = minLength * 4f;
			var minWidth = scaledAverageDiameter / 4f;
			var maxWidth = minWidth * 2f;

			float minMotionMagnitude = float.PositiveInfinity;
			float maxMotionMagnitude = 0f;

			foreach (var face in topology.internalFaces)
			{
				var faceMotionMagnitude = faceTectonicPlateMotions[face].sqrMagnitude;
				minMotionMagnitude = Mathf.Min(minMotionMagnitude, faceMotionMagnitude);
				maxMotionMagnitude = Mathf.Max(maxMotionMagnitude, faceMotionMagnitude);
			}

			minMotionMagnitude = Mathf.Sqrt(minMotionMagnitude);
			maxMotionMagnitude = Mathf.Sqrt(maxMotionMagnitude);
			float motionMagnitudeRange = maxMotionMagnitude - minMotionMagnitude;

			float aspectRatio = 2f;
			float tail = aspectRatio * slice;
			float head = aspectRatio * (1f - slice);

			for (int tectonicPlateIndex = 0; tectonicPlateIndex < _planetGenerator.tectonicPlateCount; ++tectonicPlateIndex)
			{
				Vector3 positionSum = Vector3.zero;
				int faceCount = 0;
				var firstFace = new Topology.Face(topology, tectonicPlateFirstFaceIndices[tectonicPlateIndex]);

				var face = firstFace;
				do
				{
					Vector3 faceMotion = faceTectonicPlateMotions[face];
					if (faceMotion == Vector3.zero) continue;

					if (meshVertexPositions.Count + 8 > 65534)
					{
						CreateMesh(positionSum / faceCount, meshTriangles, meshVertexPositions, meshVertexNormals, meshVertexUVs);
						positionSum = Vector3.zero;
						faceCount = 0;
					}

					var center = faceCentroids[face];
					var normal = surface.GetNormal(center);

					positionSum += center;

					int firstVertexIndex = meshVertexPositions.Count;

					float normalizedMotionMagnitude = (faceMotion.magnitude - minMotionMagnitude) / motionMagnitudeRange;
					float length = Math.LerpUnclamped(minLength, maxLength, normalizedMotionMagnitude);
					float width = Math.LerpUnclamped(minWidth, maxWidth, normalizedMotionMagnitude);
					float lengthTail = width * tail;
					float lengthHead = width * head;

					var forward = faceMotion.normalized;
					var forwardTail = forward * lengthTail;
					var forwardTailMid = forward * (length - lengthHead);
					var forwardTailMidHead = forward * length;
					var right = Vector3.Cross(normal, forward).normalized;
					var rightWidth = right * width;
					var backLeft = center - forwardTailMidHead * 0.5f - rightWidth * 0.5f;

					meshVertexPositions.Add(backLeft);
					meshVertexPositions.Add(backLeft + rightWidth);
					meshVertexPositions.Add(backLeft + forwardTail);
					meshVertexPositions.Add(backLeft + forwardTail + rightWidth);
					meshVertexPositions.Add(backLeft + forwardTailMid);
					meshVertexPositions.Add(backLeft + forwardTailMid + rightWidth);
					meshVertexPositions.Add(backLeft + forwardTailMidHead);
					meshVertexPositions.Add(backLeft + forwardTailMidHead + rightWidth);

					meshVertexNormals.Add(normal);
					meshVertexNormals.Add(normal);
					meshVertexNormals.Add(normal);
					meshVertexNormals.Add(normal);
					meshVertexNormals.Add(normal);
					meshVertexNormals.Add(normal);
					meshVertexNormals.Add(normal);
					meshVertexNormals.Add(normal);

					meshVertexUVs.Add(new Vector2(0f, 0f));
					meshVertexUVs.Add(new Vector2(0f, 1f));
					meshVertexUVs.Add(new Vector2(slice, 0f));
					meshVertexUVs.Add(new Vector2(slice, 1f));
					meshVertexUVs.Add(new Vector2(slice, 0f));
					meshVertexUVs.Add(new Vector2(slice, 1f));
					meshVertexUVs.Add(new Vector2(1f, 0f));
					meshVertexUVs.Add(new Vector2(1f, 1f));

					meshTriangles.Add(firstVertexIndex + 0);
					meshTriangles.Add(firstVertexIndex + 2);
					meshTriangles.Add(firstVertexIndex + 1);
					meshTriangles.Add(firstVertexIndex + 1);
					meshTriangles.Add(firstVertexIndex + 2);
					meshTriangles.Add(firstVertexIndex + 3);

					meshTriangles.Add(firstVertexIndex + 2);
					meshTriangles.Add(firstVertexIndex + 4);
					meshTriangles.Add(firstVertexIndex + 3);
					meshTriangles.Add(firstVertexIndex + 3);
					meshTriangles.Add(firstVertexIndex + 4);
					meshTriangles.Add(firstVertexIndex + 5);

					meshTriangles.Add(firstVertexIndex + 4);
					meshTriangles.Add(firstVertexIndex + 6);
					meshTriangles.Add(firstVertexIndex + 5);
					meshTriangles.Add(firstVertexIndex + 5);
					meshTriangles.Add(firstVertexIndex + 6);
					meshTriangles.Add(firstVertexIndex + 7);

					face = new Topology.Face(topology, faceNextTectonicPlateFaceIndices[face]);
					++faceCount;
				} while (face != firstFace);

				CreateMesh(positionSum / faceCount, meshTriangles, meshVertexPositions, meshVertexNormals, meshVertexUVs);
			}
		}

		private void CreateMesh(Vector3 meshCenter, List<int> triangles, List<Vector3> vertexPositions, List<Vector3> vertexNormals, List<Vector2> vertexUVs)
		{
			for (int i = 0; i < vertexPositions.Count; ++i)
			{
				vertexPositions[i] -= meshCenter;
			}

			var mesh = new Mesh();
			mesh.vertices = vertexPositions.ToArray();
			mesh.normals = vertexNormals.ToArray();
			mesh.uv = vertexUVs.ToArray();
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
				gameObject = new GameObject("Tectonic Plate Motion");
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
			vertexUVs.Clear();
		}
	}
#endif
}
