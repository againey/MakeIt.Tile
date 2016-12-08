using UnityEngine;
using System.Collections.Generic;
using Experilous.MakeItCurve;
using Experilous.MakeItTile;

namespace Experilous.Examples.MakeItTile
{
#if UNITY_EDITOR
	[ExecuteInEditMode]
	[System.Serializable]
	[RequireComponent(typeof(GeneratorExecutive))]
	[RequireComponent(typeof(HexGridGenerator))]
	[RequireComponent(typeof(RandomEngineGenerator))]
	public class RegionMeshGenerator : MonoBehaviour, IGenerator
	{
		public Material material;

		private HexGridGenerator _hexGridGenerator;
		private RandomEngineGenerator _randomEngineGenerator;

		public IEnumerable<IGenerator> dependencies
		{
			get
			{
				yield return _hexGridGenerator;
				yield return _randomEngineGenerator;
			}
		}

		[SerializeField] [HideInInspector] private List<GameObject> _tectonicPlateGameObjects = new List<GameObject>();
		[SerializeField] [HideInInspector] private List<GameObject> _gameObjectPool = new List<GameObject>();

		protected void OnEnable()
		{
			_hexGridGenerator = GetComponent<HexGridGenerator>();
			_randomEngineGenerator = GetComponent<RandomEngineGenerator>();
		}

		public void Generate()
		{
#if false
			var surface = _hexGridGenerator.surface;
			var topology = _hexGridGenerator.topology;
			var vertexPositions = _hexGridGenerator.vertexPositions;
			
			var random = _randomEngineGenerator.random;

			var rootFace = surface.GetFace(surface.size.x / 2, surface.size.y / 2);
			var targetFaceCount = topology.internalFaces.Count / 3;

			// Randomly build a blob region.
			var regionFaceIndices = new List<int>();
			TopologyVisitor.VisitFacesInRandomOrder(rootFace, (FaceVisitor visitor) =>
			{
				if (regionFaceIndices.Count < targetFaceCount)
				{
					regionFaceIndices.Add(visitor.face.index);
					visitor.VisitInternalNeighbors();
				}
				else
				{
					visitor.Break();
				}
			},
			random);

			// Find any single edge that is on the outer boundary of the region.
			Topology.FaceEdge firstBoundaryEdge = Topology.FaceEdge.none;
			TopologyVisitor.VisitFacesInDepthFirstOrder(rootFace.firstEdge, (FaceEdgeVisitor visitor) =>
			{
				if (regionFaceIndices.Contains(visitor.face.index))
				{
					visitor.VisitAllNeighborsExceptSource();
				}
				else
				{
					firstBoundaryEdge = visitor.edge;
					visitor.Break();
				}
			});

			// Find the ring of edges along the boundary of the region.
			var boundaryEdgeIndices = new List<int>();
			var boundaryEdge = firstBoundaryEdge;
			do
			{
				boundaryEdgeIndices.Add(boundaryEdge.index);

				var spinEdge = boundaryEdge.twin.vertexEdge;
				do
				{
					spinEdge = spinEdge.prev;
				} while (regionFaceIndices.Contains(spinEdge.face.index));

				boundaryEdge = spinEdge.faceEdge;
			} while (boundaryEdge != firstBoundaryEdge);

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

			Curve curve = new Curve();
			foreach (int edgeIndex in boundaryEdgeIndices)
			{
				curve.Add(vertexPositions[new Topology.FaceEdge(topology, edgeIndex)], 0.2f);
			}
			curve.Close();

			Vector3 positionSum = Vector3.zero;
			int positionCount = 0;

			var outerContour = curve.GetContour(0f, 10f /*max arc degree*/, float.PositiveInfinity /*max straight length*/);
			var innerContour = outerContour.Track(0.1f);

			var outerColor = new Color(0.5f, 0f, 0f, 1f);
			var innerColor = new Color(0.75f, 0.5f, 0.25f, 0f);

			int outerIndex;
			int innerIndex;

			if (!outerContour.MoveNext()) throw new System.InvalidOperationException();

			outerIndex = outerContour.SetIndex(meshVertexPositions.Count);
			meshVertexPositions.Add(outerContour.position);
			meshVertexColors.Add(outerColor);

			positionSum += (Vector3)outerContour.position;
			++positionCount;

			innerIndex = innerContour.SetIndex(meshVertexPositions.Count);
			meshVertexPositions.Add(innerContour.position);
			if (innerContour.state != ContourState.Broken)
			{
				meshVertexColors.Add(innerColor);
			}
			else
			{
				meshVertexColors.Add(Color.Lerp(outerColor, innerColor, innerContour.GetScaledDistance(outerContour)));
			}

			while (outerContour.MoveNext())
			{
				int nextOuterIndex;
				int nextInnerIndex;

				if (!outerContour.hasIndex)
				{
					nextOuterIndex = outerContour.SetIndex(meshVertexPositions.Count);
					meshVertexPositions.Add(outerContour.position);
					meshVertexColors.Add(outerColor);

					positionSum += (Vector3)outerContour.position;
					++positionCount;
				}
				else
				{
					nextOuterIndex = outerContour.index;
				}

				if (!innerContour.hasIndex)
				{
					nextInnerIndex = innerContour.SetIndex(meshVertexPositions.Count);
					meshVertexPositions.Add(innerContour.position);
					meshVertexColors.Add(innerColor);
				}
				else
				{
					nextInnerIndex = innerContour.index;
				}

				if (outerIndex != nextOuterIndex)
				{
					meshTriangles.Add(outerIndex);
					meshTriangles.Add(nextOuterIndex);
					meshTriangles.Add(innerIndex);
					outerIndex = nextOuterIndex;
				}

				if (innerIndex != nextInnerIndex)
				{
					meshTriangles.Add(innerIndex);
					meshTriangles.Add(outerIndex);
					meshTriangles.Add(nextInnerIndex);
					innerIndex = nextInnerIndex;
				}
			}

			CreateMesh(positionSum / positionCount, meshTriangles, meshVertexPositions, meshVertexColors);
#endif
		}

		private void CreateMesh(Vector3 meshCenter, List<int> triangles, List<Vector3> vertexPositions, List<Color> vertexColors)
		{
			for (int i = 0; i < vertexPositions.Count; ++i)
			{
				vertexPositions[i] -= meshCenter;
			}

			var mesh = new Mesh();
			mesh.vertices = vertexPositions.ToArray();
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
				gameObject = new GameObject("Region Boundary");
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
			vertexColors.Clear();
		}
	}
#endif
}
