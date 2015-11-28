using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public class FaceMeshGenerator : UniqueMesh
	{
		/*public ManifoldGenerator ManifoldGenerator;
		public Color Color;

		private void AddFace(Topology.Face face, VertexAttribute<Vector3> vertexPositions, Vector3[] vertices, Color[] colors, int[] triangles, ref int meshVertex, ref int meshTriangle)
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

		public void BuildMeshForFace(Topology.Face face)
		{
			var manifold = ManifoldGenerator.manifold;

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

		public void BuildMeshForFaces(Topology.Face face0, Topology.Face face1)
		{
			var manifold = ManifoldGenerator.manifold;

			Vector3[] vertices = new Vector3[face0.neighborCount + face1.neighborCount + 2];
			Color[] colors = new Color[face0.neighborCount + face1.neighborCount + 2];
			int[] triangles = new int[(face0.neighborCount * + face1.neighborCount) * 3];

			int meshVertex = 0;
			int meshTriangle = 0;

			AddFace(face0, manifold.vertexPositions, vertices, colors, triangles, ref meshVertex, ref meshTriangle);
			AddFace(face1, manifold.vertexPositions, vertices, colors, triangles, ref meshVertex, ref meshTriangle);

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
		}*/
	}
}
