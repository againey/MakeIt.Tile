using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Experilous.Topologies;

namespace Experilous.Examples.MakeItTile
{
	public class ContourTriangulatorController : MonoBehaviour
	{
		public Material material;

		public Vector3[] polygonVertices;
		public float[] contourDistances;
		public Color[] contourColors;

		protected void Awake()
		{
			if (polygonVertices.Length >= 3 && contourDistances.Length >= 2 && contourColors.Length == contourDistances.Length)
			{
				var graph = new DynamicGraph();
				int prevNodeIndex;
				graph.AddNode(out prevNodeIndex);
				for (int i = 1; i < polygonVertices.Length; ++i)
				{
					int nextNodeIndex;
					graph.AddNode(out nextNodeIndex);
					graph.ConnectNodes(prevNodeIndex, nextNodeIndex);
					prevNodeIndex = nextNodeIndex;
				}
				graph.ConnectNodes(prevNodeIndex, 0);

				var voronoiGenerator = new PlanarVoronoiGenerator(0.0001f);
				voronoiGenerator.SetSites(graph, new GraphNodeDataArray<Vector3>(polygonVertices), Vector3.zero, Vector3.right, Vector3.up);
				var diagram = voronoiGenerator.Generate();

				var vertexIndexMap = new Dictionary<ContourTriangulator.PositionId, int>();
				var vertexPositions = new List<Vector3>();
				var vertexColors = new List<Color>();
				var triangleIndices = new List<int>();

				ContourTriangulator.OnVertexDelegate onVertex =
					(ContourTriangulator.PositionId positionId, Vector3 position, VoronoiSiteType siteType, int siteIndex, int contourIndex, float distance) =>
					{
						if (!vertexIndexMap.ContainsKey(positionId))
						{
							vertexIndexMap.Add(positionId, vertexPositions.Count);
							vertexPositions.Add(position);
							if (contourIndex > 0)
							{
								vertexColors.Add(Color.Lerp(contourColors[contourIndex - 1], contourColors[contourIndex], (distance - contourDistances[contourIndex - 1]) / (contourDistances[contourIndex] - contourDistances[contourIndex - 1])));
							}
							else
							{
								vertexColors.Add(contourColors[0]);
							}
							//Debug.LogFormat("Add Vertex({0}):  {1}, {2}, {3}, {4}, {5}, {6:F4}, {7}", vertexPositions.Count - 1, positionId, position.ToString("F2"), siteType, siteIndex, contourIndex, distance, vertexColors[vertexColors.Count - 1]);
						}
						else
						{
							//Debug.LogFormat("Reuse Vertex({0}):  {1}, {2}, {3}, {4}, {5}, {6:F4}, {7}", vertexIndexMap[positionId], positionId, position.ToString("F2"), siteType, siteIndex, contourIndex, distance, vertexColors[vertexColors.Count - 1]);
						}
					};

				ContourTriangulator.OnTriangleDelegate onTriangle =
					(ContourTriangulator.PositionId positionId0, ContourTriangulator.PositionId positionId1, ContourTriangulator.PositionId positionId2) =>
					{
						triangleIndices.Add(vertexIndexMap[positionId0]);
						triangleIndices.Add(vertexIndexMap[positionId1]);
						triangleIndices.Add(vertexIndexMap[positionId2]);
						//Debug.LogFormat("Triangle({0}, {1}, {2}):  {1}, {2}, {3}", vertexIndexMap[positionId0], vertexIndexMap[positionId1], vertexIndexMap[positionId2], vertexPositions[vertexIndexMap[positionId0]].ToString("F2"), vertexPositions[vertexIndexMap[positionId1]].ToString("F2"), vertexPositions[vertexIndexMap[positionId2]].ToString("F2"));
					};

				var triangulator = new ContourTriangulator(0.25f, 0.0001f);
				var edge = new TopologyEdge(diagram._voronoiTopology, diagram._siteEdgeFirstVoronoiEdgeIndices[0]);
				triangulator.Triangulate(diagram, edge.sourceFace, onVertex, onTriangle, Vector3.back, contourDistances);

				var gameObject = new GameObject();
				gameObject.name = "Generated Contour Mesh";

				var meshFilter = gameObject.AddComponent<MeshFilter>();
				var mesh = meshFilter.mesh;
				mesh.Clear();
				mesh.SetVertices(vertexPositions);
				mesh.SetColors(vertexColors);
				mesh.SetIndices(triangleIndices.ToArray(), MeshTopology.Triangles, 0);
				mesh.RecalculateBounds();
				mesh.RecalculateNormals();
				mesh.UploadMeshData(true);

				var meshRenderer = gameObject.AddComponent<MeshRenderer>();
				meshRenderer.material = material;
			}
		}

		protected void Start()
		{
		}

		protected void OnEnable()
		{
		}

		protected void OnDisable()
		{
		}

		protected void FixedUpdate()
		{
		}

		protected void Update()
		{
		}

		protected void LateUpdate()
		{
		}
	}
}
