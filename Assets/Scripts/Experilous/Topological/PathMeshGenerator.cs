using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public class PathMeshGenerator : UniqueMesh
	{
		public TopologyGenerator TopologyGenerator;
		public Color Color;

		private void AddFace(Topology.Face face, Vector3[] vertexPositions, Vector3[] vertices, Color[] colors, int[] triangles, ref int meshVertex, ref int meshTriangle)
		{
			var neighborCount = face.neighborCount;
			var firstEdge = face.firstEdge;

			var centroid = new Vector3();
			var edge = firstEdge;
			do
			{
				centroid += vertexPositions[edge.nextVertex];
				edge = edge.next;
			} while (edge != firstEdge);
			centroid /= neighborCount;

			vertices[meshVertex] = centroid * 1.001f;
			colors[meshVertex] = Color;
			int i = 0;
			do
			{
				vertices[meshVertex + i + 1] = vertexPositions[edge.nextVertex] * 1.001f;
				colors[meshVertex + i + 1] = Color * 0.5f;
				triangles[meshTriangle + i * 3 + 0] = meshVertex;
				triangles[meshTriangle + i * 3 + 1] = meshVertex + 1 + i;
				triangles[meshTriangle + i * 3 + 2] = meshVertex + 1 + (i + 1) % neighborCount;
				edge = edge.next;
				++i;
			} while (edge != firstEdge);

			meshVertex += neighborCount + 1;
			meshTriangle += neighborCount * 3;
		}

		public void BuildPathMeshForFace(Topology.Face face)
		{
			var manifold = TopologyGenerator.manifold;

			Vector3[] vertices = new Vector3[face.neighborCount + 1];
			Color[] colors = new Color[face.neighborCount + 1];
			int[] triangles = new int[face.neighborCount * 3];

			int meshVertex = 0;
			int meshTriangle = 0;

			AddFace(face, manifold.vertexPositions, vertices, colors, triangles, ref meshVertex, ref meshTriangle);

			mesh.vertices = vertices;
			mesh.colors = colors;
			mesh.triangles = triangles;

			mesh.RecalculateBounds();
		}

		public void BuildPathMeshBetweenFaces(Topology.Face face0, Topology.Face face1)
		{
			var manifold = TopologyGenerator.manifold;

			int[] path = null;
			if (TopologyGenerator.CalculateCentroids == true)
			{
				if (TopologyGenerator.GenerateRegions == false)
				{
					if (TopologyGenerator.Projection != TopologyProjection.Spherical)
					{
						path = PathFinder.FindEuclideanPath(face0, face1, TopologyGenerator.centroids);
					}
					else
					{
						path = PathFinder.FindSphericalEuclideanPath(face0, face1, TopologyGenerator.centroids);
					}
				}
				else
				{
					float largestDistanceSquared = 0f;
					foreach (var face in manifold.topology.internalFaces)
					{
						var facePosition = TopologyGenerator.centroids[face];
						foreach (var edge in face.edges)
						{
							if (face.index < edge.farFace.index && edge.farFace.isInternal)
							{
								var delta = facePosition - TopologyGenerator.centroids[edge.farFace];
								largestDistanceSquared = Mathf.Max(largestDistanceSquared, delta.sqrMagnitude);
							}
						}
					}

					float smallestInternalTravelCost = float.PositiveInfinity;
					for (int i = 0; i < TopologyGenerator.regionCount; ++i)
					{
						smallestInternalTravelCost = Mathf.Min(smallestInternalTravelCost, TopologyGenerator.regionInternalTravelCosts[i]);
					}

					float optimisticHeuristicScale = smallestInternalTravelCost / Mathf.Sqrt(largestDistanceSquared);

					path = PathFinder.FindPath(face0, face1, 
						(Topology.Face s, Topology.Face t, int pathLength) =>
						{
							return (TopologyGenerator.centroids[s] - TopologyGenerator.centroids[t]).magnitude * optimisticHeuristicScale;
						},
						(Topology.FaceEdge edge, int pathLength) =>
						{
							if (edge.isOuterBoundary) return float.PositiveInfinity;
							var sourceRegion = TopologyGenerator.faceRegions[edge.nearFace];
							var targetRegion = TopologyGenerator.faceRegions[edge.farFace];
							if (sourceRegion == targetRegion)
							{
								return TopologyGenerator.regionInternalTravelCosts[targetRegion];
							}
							else
							{
								return Mathf.Max(TopologyGenerator.regionBorderTravelCosts[sourceRegion], TopologyGenerator.regionBorderTravelCosts[targetRegion]);
							}
						});
				}
			}

			Vector3[] vertices;
			Color[] colors;
			int[] triangles;

			if (path != null)
			{
				int meshVertex = 0;
				int meshTriangle = 0;

				foreach (var faceIndex in path)
				{
					var neighborCount = manifold.topology.internalFaces[faceIndex].neighborCount;
					meshVertex += neighborCount + 1;
					meshTriangle += neighborCount * 3;
				}
			
				vertices = new Vector3[meshVertex];
				colors = new Color[meshVertex];
				triangles = new int[meshTriangle];

				meshVertex = 0;
				meshTriangle = 0;

				foreach (var faceIndex in path)
				{
					AddFace(manifold.topology.internalFaces[faceIndex], manifold.vertexPositions, vertices, colors, triangles, ref meshVertex, ref meshTriangle);
				}
			}
			else
			{
				int meshVertex = 0;
				int meshTriangle = 0;
			
				vertices = new Vector3[face0.neighborCount + face1.neighborCount + 2];
				colors = new Color[face0.neighborCount + face1.neighborCount + 2];
				triangles = new int[(face0.neighborCount * + face1.neighborCount) * 3];

				AddFace(face0, manifold.vertexPositions, vertices, colors, triangles, ref meshVertex, ref meshTriangle);
				AddFace(face1, manifold.vertexPositions, vertices, colors, triangles, ref meshVertex, ref meshTriangle);
			}

			mesh.Clear();

			mesh.vertices = vertices;
			mesh.colors = colors;
			mesh.triangles = triangles;

			mesh.RecalculateBounds();
		}

		public void Clear()
		{
			mesh.Clear();
			mesh.RecalculateBounds();
		}
	}
}
