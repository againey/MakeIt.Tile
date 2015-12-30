using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public static class MeshGenerator
	{
		public static Mesh[] GenerateSubmeshes(IFacesIndexer faces, Vector3[] vertexPositions, Vector3[] centroidPositions, Vector3[] faceNormals, System.Func<int, Color> faceColorGenerator)
		{
			List<Mesh> meshes = new List<Mesh>();

			var i = 0;
			var faceCount = faces.Count;
			while (i < faceCount)
			{
				var endFaceIndex = i;
				var meshVertexCount = 0;
				var meshTriangleCount = 0;
				while (endFaceIndex < faceCount)
				{
					var face = faces[endFaceIndex];
					var neighborCount = face.neighborCount;
					var faceVertexCount = neighborCount + 1;
					if (meshVertexCount + faceVertexCount > 65534) break;
					++endFaceIndex;
					meshVertexCount += faceVertexCount;
					meshTriangleCount += neighborCount;
				}

				Vector3[] vertices = new Vector3[meshVertexCount];
				Color[] colors = new Color[meshVertexCount];
				Vector3[] normals = new Vector3[meshVertexCount];
				int[] triangles = new int[meshTriangleCount * 3];

				int meshVertex = 0;
				int meshTriangle = 0;

				while (i < endFaceIndex)
				{
					var face = faces[i];
					var edge = face.firstEdge;
					var neighborCount = face.neighborCount;
					vertices[meshVertex] = centroidPositions[face];
					colors[meshVertex] = faceColorGenerator(face.index);
					normals[meshVertex] = faceNormals[face];
					for (int j = 0; j < neighborCount; ++j, edge = edge.next)
					{
						vertices[meshVertex + j + 1] = vertexPositions[edge.nextVertex];
						colors[meshVertex + j + 1] = new Color(0, 0, 0);
						normals[meshVertex + j + 1] = faceNormals[face];
						triangles[meshTriangle + j * 3 + 0] = meshVertex;
						triangles[meshTriangle + j * 3 + 1] = meshVertex + 1 + j;
						triangles[meshTriangle + j * 3 + 2] = meshVertex + 1 + (j + 1) % neighborCount;
					}
					meshVertex += neighborCount + 1;
					meshTriangle += neighborCount * 3;
					++i;
				}

				var mesh = new Mesh();

				mesh.vertices = vertices;
				mesh.colors = colors;
				mesh.normals = normals;
				mesh.triangles = triangles;

				mesh.RecalculateBounds();

				meshes.Add(mesh);
			}

			return meshes.ToArray();
		}

		public static Mesh[] GenerateSubmeshes(Manifold manifold, IFacesIndexer faces, Vector3[] centroidPositions, Vector3[] faceNormals, System.Func<int, Color> faceColorGenerator)
		{
			return GenerateSubmeshes(faces, manifold.vertexPositions, centroidPositions, faceNormals, faceColorGenerator);
		}

		public static Mesh[] GenerateSubmeshes(WrapAroundManifold manifold, IFacesIndexer faces, Vector3[] centroidPositions, Vector3[] faceNormals, System.Func<int, Color> faceColorGenerator)
		{
			List<Mesh> meshes = new List<Mesh>();

			var i = 0;
			var faceCount = faces.Count;
			while (i < faceCount)
			{
				var endFaceIndex = i;
				var meshVertexCount = 0;
				var meshTriangleCount = 0;
				while (endFaceIndex < faceCount)
				{
					var face = faces[endFaceIndex];
					var neighborCount = face.neighborCount;
					var faceVertexCount = neighborCount + 1;
					if (meshVertexCount + faceVertexCount > 65534) break;
					++endFaceIndex;
					meshVertexCount += faceVertexCount;
					meshTriangleCount += neighborCount;
				}

				Vector3[] vertices = new Vector3[meshVertexCount];
				Color[] colors = new Color[meshVertexCount];
				Vector3[] normals = new Vector3[meshVertexCount];
				int[] triangles = new int[meshTriangleCount * 3];

				int meshVertex = 0;
				int meshTriangle = 0;

				while (i < endFaceIndex)
				{
					var face = faces[i];
					var edge = face.firstEdge;
					var neighborCount = face.neighborCount;
					vertices[meshVertex] = centroidPositions[face];
					colors[meshVertex] = faceColorGenerator(face.index);
					normals[meshVertex] = faceNormals[face];
					for (int j = 0; j < neighborCount; ++j, edge = edge.next)
					{
						vertices[meshVertex + j + 1] = manifold.GetFaceVertexPosition(edge);
						colors[meshVertex + j + 1] = new Color(0, 0, 0);
						normals[meshVertex + j + 1] = faceNormals[face];
						triangles[meshTriangle + j * 3 + 0] = meshVertex;
						triangles[meshTriangle + j * 3 + 1] = meshVertex + 1 + j;
						triangles[meshTriangle + j * 3 + 2] = meshVertex + 1 + (j + 1) % neighborCount;
					}
					meshVertex += neighborCount + 1;
					meshTriangle += neighborCount * 3;
					++i;
				}

				var mesh = new Mesh();

				mesh.vertices = vertices;
				mesh.colors = colors;
				mesh.normals = normals;
				mesh.triangles = triangles;

				mesh.RecalculateBounds();

				meshes.Add(mesh);
			}

			return meshes.ToArray();
		}
	}
}
