using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public static class MeshBuilder
	{
		public static IEnumerable<Mesh> BuildMeshes(
			IEnumerable<Topology.Face> faces,
			IVertexAttribute<Vector3> vertexPositions,
			IFaceAttribute<Vector3> centroidPositions = null,
			IFaceAttribute<Vector3> faceNormals = null,
			IFaceAttribute<Color> faceColors = null,
			int maxVerticesPerSubmesh = 65534)
		{
			var vertices = new List<Vector3>();
			var colors = new List<Color>();
			var normals = new List<Vector3>();
			var triangles = new List<int>();

			foreach (var face in faces)
			{
				if (vertices.Count + face.neighborCount + 1 > maxVerticesPerSubmesh)
				{
					yield return ConvertToMesh(vertices, colors, normals, triangles);
				}

				AddToMesh(face, vertexPositions, centroidPositions, faceNormals, faceColors, vertices, colors, normals, triangles);
			}

			if (vertices.Count > 0)
			{
				yield return ConvertToMesh(vertices, colors, normals, triangles);
			}
		}

		public static IEnumerable<Mesh> BuildMeshes(
			IEnumerable<Topology.Face> faces,
			Vector3 offset,
			IVertexAttribute<Vector3> vertexPositions,
			IFaceAttribute<Vector3> centroidPositions = null,
			IFaceAttribute<Vector3> faceNormals = null,
			IFaceAttribute<Color> faceColors = null,
			int maxVerticesPerSubmesh = 65534)
		{
			var vertices = new List<Vector3>();
			var colors = new List<Color>();
			var normals = new List<Vector3>();
			var triangles = new List<int>();

			foreach (var face in faces)
			{
				if (vertices.Count + face.neighborCount + 1 > maxVerticesPerSubmesh)
				{
					for (int i = 0; i < vertices.Count; ++i)
					{
						vertices[i] += offset;
					}

					yield return ConvertToMesh(vertices, colors, normals, triangles);
				}

				AddToMesh(face, vertexPositions, centroidPositions, faceNormals, faceColors, vertices, colors, normals, triangles);
			}

			if (vertices.Count > 0)
			{
				for (int i = 0; i < vertices.Count; ++i)
				{
					vertices[i] += offset;
				}

				yield return ConvertToMesh(vertices, colors, normals, triangles);
			}
		}

		#region Private Helper Functions

		private static void AddToMesh(Topology.Face face,
			IVertexAttribute<Vector3> vertexPositions,
			IFaceAttribute<Vector3> centroidPositions,
			IFaceAttribute<Vector3> faceNormals,
			IFaceAttribute<Color> faceColors,
			List<Vector3> vertices, List<Color> colors, List<Vector3> normals, List<int> triangles)
		{
			var firstVertexIndex = vertices.Count;

			vertices.Add(centroidPositions[face]);
			colors.Add(faceColors[face]);
			normals.Add(faceNormals[face]);

			var neighborCount = face.neighborCount;
			var edge = face.firstEdge;
			for (int j = 0; j < neighborCount; ++j, edge = edge.next)
			{
				vertices.Add(vertexPositions[edge]);
				colors.Add(new Color(0, 0, 0));
				normals.Add(faceNormals[face]);
				triangles.Add(firstVertexIndex);
				triangles.Add(firstVertexIndex + 1 + j);
				triangles.Add(firstVertexIndex + 1 + (j + 1) % neighborCount);
			}
		}

		private static Mesh ConvertToMesh(List<Vector3> vertices, List<Color> colors, List<Vector3> normals, List<int> triangles)
		{
			var mesh = new Mesh();

			mesh.vertices = vertices.ToArray();
			mesh.colors = colors.ToArray();
			mesh.normals = normals.ToArray();
			mesh.triangles = triangles.ToArray();

			vertices.Clear();
			colors.Clear();
			normals.Clear();
			triangles.Clear();

			mesh.RecalculateBounds();

			return mesh;
		}

		#endregion
	}
}
