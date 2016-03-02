/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System;

namespace Experilous.Topological
{
	[Serializable]
	public class DynamicMesh : ScriptableObject
	{
		[SerializeField] private VertexAttributes _vertexAttributes;
		[SerializeField] private Triangulation _triangulation;
		[SerializeField] private int _maxVerticesPerSubmesh;

		[SerializeField] private Submesh[] _submeshes;
		[SerializeField] private IntFaceAttribute _faceSubmeshIndices;
		[SerializeField] private IntFaceAttribute _faceFirstVertexIndices;

		public static DynamicMesh Create(Topology.FacesIndexer faces, IVertexBuilder vertexBuilder, VertexAttributes vertexAttributes, Triangulation triangulation, int maxVerticesPerSubmesh = 65534)
		{
			var dynamicMesh = CreateInstance<DynamicMesh>();
			dynamicMesh._vertexAttributes = vertexAttributes;
			dynamicMesh._triangulation = triangulation;
			dynamicMesh._maxVerticesPerSubmesh = maxVerticesPerSubmesh;
			dynamicMesh.Initialize(faces, vertexBuilder);
			return dynamicMesh;
		}

		private void Initialize(Topology.FacesIndexer faces, IVertexBuilder vertexBuilder)
		{
			_faceSubmeshIndices = IntFaceAttribute.Create(faces.Count);
			_faceFirstVertexIndices = IntFaceAttribute.Create(faces.Count);

			switch (_triangulation)
			{
				case Triangulation.Strip:
					BuildFaceStrips(faces, vertexBuilder);
					break;
				case Triangulation.EdgeFan:
					BuildFaceEdgeFans(faces, vertexBuilder);
					break;
				case Triangulation.CenterFan:
					BuildFaceCenterFans(faces, vertexBuilder);
					break;
				case Triangulation.Umbrella:
					BuildFaceUmbrellas(faces, vertexBuilder);
					break;
				default:
					throw new NotImplementedException();
			}
		}

		public void RebuildFace(Topology.Face face, IVertexBuilder vertexBuilder)
		{
			var submesh = _submeshes[_faceSubmeshIndices[face]];
			var vertexAttributeArrays = new IndexedVertexAttributeArrays(_faceFirstVertexIndices[face], submesh);

			switch (_triangulation)
			{
				case Triangulation.Strip:
					RebuildFaceStrip(face, vertexBuilder, vertexAttributeArrays);
					break;
				case Triangulation.EdgeFan:
					RebuildFaceEdgeFan(face, vertexBuilder, vertexAttributeArrays);
					break;
				case Triangulation.CenterFan:
					RebuildFaceCenterFan(face, vertexBuilder, vertexAttributeArrays);
					break;
				case Triangulation.Umbrella:
					RebuildFaceUmbrella(face, vertexBuilder, vertexAttributeArrays);
					break;
				default:
					throw new NotImplementedException();
			}

			submesh.isDirty = true;
		}

		public void RebuildMesh(VertexAttributes dirtyVertexAttributes)
		{
			foreach (var submesh in _submeshes)
			{
				if (submesh.isDirty) submesh.Rebuild(dirtyVertexAttributes);
			}
		}

		public int submeshCount { get { return _submeshes.Length; } }

		public IEnumerable<Mesh> submeshes
		{
			get
			{
				foreach (var submesh in _submeshes)
				{
					yield return submesh.mesh;
				}
			}
		}

		#region Public Types

		[Flags] public enum VertexAttributes
		{
			None = 0x000,
			Position = 0x001,
			Normal = 0x002,
			Color = 0x004,
			Color32 = 0x008,
			UV = 0x010,
			UV1 = 0x010,
			UV2 = 0x020,
			UV3 = 0x040,
			UV4 = 0x080,
			Tangent = 0x100,
			All = Position | Normal | Color | Color32 | UV1 | UV2 | UV3 | UV4 | Tangent,
		}

		public interface IVertexAttributes
		{
			Vector3 position { set; }
			Vector3 normal { set; }
			Color color { set; }
			Color32 color32 { set; }
			Vector2 uv { set; }
			Vector2 uv1 { set; }
			Vector2 uv2 { set; }
			Vector2 uv3 { set; }
			Vector2 uv4 { set; }
			Vector4 tangent { set; }
		}

		public interface IVertexBuilder
		{
			void AddEdgeVertex(Topology.FaceEdge edge, IVertexAttributes vertexAttributes);
			void AddCenterVertex(Topology.Face face, IVertexAttributes vertexAttributes);
			void AddDuplicateFirstEdgeVertex(Topology.Face face, IVertexAttributes vertexAttributes);
		}

		public enum Triangulation
		{
			Strip,
			EdgeFan,
			CenterFan,
			Umbrella,
		}

		public class CallbackVertexBuilder : IVertexBuilder
		{
			private Action<Topology.FaceEdge, IVertexAttributes> _addEdgeVertexCallback;
			private Action<Topology.Face, IVertexAttributes> _addCenterVertexCallback;
			private Action<Topology.Face, IVertexAttributes> _addDuplicateFirstEdgeVertexCallback;

			public CallbackVertexBuilder(
				Action<Topology.FaceEdge, IVertexAttributes> addEdgeVertexCallback,
				Action<Topology.Face, IVertexAttributes> addCenterVertexCallback = null,
				Action<Topology.Face, IVertexAttributes> addDuplicateFirstEdgeVertexCallback = null)
			{
				if (addEdgeVertexCallback != null)
				{
					_addEdgeVertexCallback = addEdgeVertexCallback;
				}
				else
				{
					_addEdgeVertexCallback = (Topology.FaceEdge edge, IVertexAttributes vertexAttributes) =>
					{
						throw new NotSupportedException("A triangulation strategy needs to add an edge vertex, but no action for building such a vertex was provided.");
					};
				}

				if (addCenterVertexCallback != null)
				{
					_addCenterVertexCallback = addCenterVertexCallback;
				}
				else
				{
					_addCenterVertexCallback = (Topology.Face face, IVertexAttributes vertexAttributes) =>
					{
						throw new NotSupportedException("A triangulation strategy needs to add a center vertex, but no action for building such a vertex was provided.");
					};
				}

				if (addDuplicateFirstEdgeVertexCallback != null)
				{
					_addDuplicateFirstEdgeVertexCallback = addDuplicateFirstEdgeVertexCallback;
				}
				else
				{
					_addDuplicateFirstEdgeVertexCallback = (Topology.Face face, IVertexAttributes vertexAttributes) =>
					{
						throw new NotSupportedException("A triangulation strategy needs to add a duplicated first edge vertex, but no action for building such a vertex was provided.");
					};
				}
			}

			public void AddEdgeVertex(Topology.FaceEdge edge, IVertexAttributes vertexAttributes)
			{
				_addEdgeVertexCallback(edge, vertexAttributes);
			}

			public void AddCenterVertex(Topology.Face face, IVertexAttributes vertexAttributes)
			{
				_addCenterVertexCallback(face, vertexAttributes);
			}

			public void AddDuplicateFirstEdgeVertex(Topology.Face face, IVertexAttributes vertexAttributes)
			{
				_addDuplicateFirstEdgeVertexCallback(face, vertexAttributes);
			}
		}

		#endregion

		#region Private Types

		[Serializable]
		private class Submesh
		{
			public Mesh mesh;
			public bool isDirty;

			public Vector3[] positions;
			public Vector3[] normals;
			public Color[] colors;
			public Color32[] colors32;
			public Vector2[] uvs;
			public Vector2[] uvs2;
			public Vector2[] uvs3;
			public Vector2[] uvs4;
			public Vector4[] tangents;

			public Submesh(DynamicVertexAttributeArrays vertexAttributeArrays, List<int> triangleIndices)
			{
				mesh = new Mesh();
				isDirty = false;

				var length = vertexAttributeArrays.index;
				if (CopyArray(ref positions, vertexAttributeArrays.positions, length)) { mesh.vertices = positions; }
				if (CopyArray(ref normals, vertexAttributeArrays.normals, length)) { mesh.normals = normals; }
				if (CopyArray(ref colors, vertexAttributeArrays.colors, length)) { mesh.colors = colors; }
				if (CopyArray(ref colors32, vertexAttributeArrays.colors32, length)) { mesh.colors32 = colors32; }
				if (CopyArray(ref uvs, vertexAttributeArrays.uvs, length)) { mesh.uv = uvs; }
				if (CopyArray(ref uvs2, vertexAttributeArrays.uvs2, length)) { mesh.uv2 = uvs2; }
				if (CopyArray(ref uvs3, vertexAttributeArrays.uvs3, length)) { mesh.uv3 = uvs3; }
				if (CopyArray(ref uvs4, vertexAttributeArrays.uvs4, length)) { mesh.uv4 = uvs4; }
				if (CopyArray(ref tangents, vertexAttributeArrays.tangents, length)) { mesh.tangents = tangents; }

				mesh.triangles = triangleIndices.ToArray();
				mesh.RecalculateBounds();
			}

			private bool CopyArray<T>(ref T[] array, T[] sourceArray, int length)
			{
				if (sourceArray != null)
				{
					array = new T[length];
					Array.Copy(sourceArray, array, length);
					return true;
				}
				else
				{
					return false;
				}
			}

			public void Rebuild(VertexAttributes dirtyVertexAttributes)
			{
				if (isDirty)
				{
					if (positions != null && (dirtyVertexAttributes & VertexAttributes.Position) != 0) { mesh.vertices = positions; }
					if (normals != null && (dirtyVertexAttributes & VertexAttributes.Normal) != 0) { mesh.normals = normals; }
					if (colors != null && (dirtyVertexAttributes & VertexAttributes.Color) != 0) { mesh.colors = colors; }
					if (colors32 != null && (dirtyVertexAttributes & VertexAttributes.Color32) != 0) { mesh.colors32 = colors32; }
					if (uvs != null && (dirtyVertexAttributes & VertexAttributes.UV) != 0) { mesh.uv = uvs; }
					if (uvs2 != null && (dirtyVertexAttributes & VertexAttributes.UV2) != 0) { mesh.uv2 = uvs2; }
					if (uvs3 != null && (dirtyVertexAttributes & VertexAttributes.UV3) != 0) { mesh.uv3 = uvs3; }
					if (uvs4 != null && (dirtyVertexAttributes & VertexAttributes.UV4) != 0) { mesh.uv4 = uvs4; }
					if (tangents != null && (dirtyVertexAttributes & VertexAttributes.Tangent) != 0) { mesh.tangents = tangents; }

					isDirty = false;
				}
			}
		}

		private interface IIndexedVertexAttributes : IVertexAttributes
		{
			void Advance();
		}

		private class DynamicVertexAttributeArrays : IIndexedVertexAttributes
		{
			public int index = 0;
			public int capacity = 0;

			public Vector3[] positions;
			public Vector3[] normals;
			public Color[] colors;
			public Color32[] colors32;
			public Vector2[] uvs;
			public Vector2[] uvs2;
			public Vector2[] uvs3;
			public Vector2[] uvs4;
			public Vector4[] tangents;

			public DynamicVertexAttributeArrays(VertexAttributes attributes, int capacity)
			{
				this.capacity = capacity;

				if ((attributes & VertexAttributes.Position) != 0) positions = new Vector3[capacity];
				if ((attributes & VertexAttributes.Normal) != 0) normals = new Vector3[capacity];
				if ((attributes & VertexAttributes.Color) != 0) colors = new Color[capacity];
				if ((attributes & VertexAttributes.Color32) != 0) colors32 = new Color32[capacity];
				if ((attributes & VertexAttributes.UV) != 0) uvs = new Vector2[capacity];
				if ((attributes & VertexAttributes.UV2) != 0) uvs2 = new Vector2[capacity];
				if ((attributes & VertexAttributes.UV3) != 0) uvs3 = new Vector2[capacity];
				if ((attributes & VertexAttributes.UV4) != 0) uvs4 = new Vector2[capacity];
				if ((attributes & VertexAttributes.Tangent) != 0) tangents = new Vector4[capacity];
			}

			public Vector3 position { set { positions[index] = value; } }
			public Vector3 normal { set { normals[index] = value; } }
			public Color color { set { colors[index] = value; } }
			public Color32 color32 { set { colors32[index] = value; } }
			public Vector2 uv { set { uvs[index] = value; } }
			public Vector2 uv1 { set { uvs[index] = value; } }
			public Vector2 uv2 { set { uvs2[index] = value; } }
			public Vector2 uv3 { set { uvs3[index] = value; } }
			public Vector2 uv4 { set { uvs4[index] = value; } }
			public Vector4 tangent { set { tangents[index] = value; } }

			public DynamicVertexAttributeArrays Grow(int vertexCount)
			{
				while (index + vertexCount > capacity)
				{
					capacity = capacity * 3 / 2;
					GrowArray(ref positions);
					GrowArray(ref normals);
					GrowArray(ref colors);
					GrowArray(ref colors32);
					GrowArray(ref uvs);
					GrowArray(ref uvs2);
					GrowArray(ref uvs3);
					GrowArray(ref uvs4);
					GrowArray(ref tangents);
				}
				return this;
			}

			private void GrowArray<T>(ref T[] array)
			{
				if (array != null)
				{
					var newArray = new T[capacity];
					Array.Copy(array, newArray, index);
					array = newArray;
				}
			}

			public void Advance()
			{
				++index;
			}

			public void Reset()
			{
				index = 0;
			}
		}

		private class IndexedVertexAttributeArrays : IIndexedVertexAttributes
		{
			public int index;

			private Submesh _submesh;

			public IndexedVertexAttributeArrays(Submesh submesh)
			{
				index = 0;
				_submesh = submesh;
			}

			public IndexedVertexAttributeArrays(int index, Submesh submesh)
			{
				this.index = index;
				_submesh = submesh;
			}

			public Vector3 position { set { _submesh.positions[index] = value; } }
			public Vector3 normal { set { _submesh.normals[index] = value; } }
			public Color color { set { _submesh.colors[index] = value; } }
			public Color32 color32 { set { _submesh.colors32[index] = value; } }
			public Vector2 uv { set { _submesh.uvs[index] = value; } }
			public Vector2 uv1 { set { _submesh.uvs[index] = value; } }
			public Vector2 uv2 { set { _submesh.uvs2[index] = value; } }
			public Vector2 uv3 { set { _submesh.uvs3[index] = value; } }
			public Vector2 uv4 { set { _submesh.uvs4[index] = value; } }
			public Vector4 tangent { set { _submesh.tangents[index] = value; } }

			public void Advance()
			{
				++index;
			}
		}

		#endregion

		#region Triangulation

		private void BuildFaceStrips(Topology.FacesIndexer faces, IVertexBuilder vertexBuilder)
		{
			var submeshList = new List<Submesh>();

			var vertexAttributeArrays = new DynamicVertexAttributeArrays(_vertexAttributes, Mathf.Min(_maxVerticesPerSubmesh, faces.Count * 6));
			var triangleIndices = new List<int>();

			foreach (var face in faces)
			{
				var vertexCount = face.neighborCount;

				if (vertexAttributeArrays.index + vertexCount > _maxVerticesPerSubmesh)
				{
					submeshList.Add(new Submesh(vertexAttributeArrays, triangleIndices));
					vertexAttributeArrays.Reset();
					triangleIndices.Clear();
				}

				_faceSubmeshIndices[face] = submeshList.Count;
				_faceFirstVertexIndices[face] = vertexAttributeArrays.index;

				vertexAttributeArrays.Grow(vertexCount);

				int firstVertexIndex = vertexAttributeArrays.index;
				int forwardVertexIndex = firstVertexIndex + 1;
				int backwardVertexIndex = firstVertexIndex + 2;

				triangleIndices.Add(firstVertexIndex);
				triangleIndices.Add(forwardVertexIndex);
				triangleIndices.Add(backwardVertexIndex);

				int triangleCount = face.neighborCount - 2;
				int triangleIndex = 1;
				while (triangleIndex < triangleCount)
				{
					triangleIndices.Add(backwardVertexIndex);
					triangleIndices.Add(forwardVertexIndex);
					triangleIndices.Add(forwardVertexIndex += 2);

					if (++triangleIndex == triangleCount) break;

					triangleIndices.Add(backwardVertexIndex);
					triangleIndices.Add(forwardVertexIndex);
					triangleIndices.Add(backwardVertexIndex += 2);

					++triangleIndex;
				}

				RebuildFaceStrip(face, vertexBuilder, vertexAttributeArrays);
			}

			if (vertexAttributeArrays.index > 0)
			{
				submeshList.Add(new Submesh(vertexAttributeArrays, triangleIndices));
			}

			_submeshes = submeshList.ToArray();
		}

		private void RebuildFaceStrip(Topology.Face face, IVertexBuilder vertexBuilder, IIndexedVertexAttributes vertexAttributes)
		{
			var firstEdge = face.firstEdge;

			vertexBuilder.AddEdgeVertex(firstEdge, vertexAttributes);
			vertexAttributes.Advance();

			var forwardEdge = firstEdge.next;
			vertexBuilder.AddEdgeVertex(forwardEdge, vertexAttributes);
			vertexAttributes.Advance();

			var backwardEdge = firstEdge.prev;
			vertexBuilder.AddEdgeVertex(backwardEdge, vertexAttributes);
			vertexAttributes.Advance();

			forwardEdge = forwardEdge.next;
			while (forwardEdge != backwardEdge)
			{
				vertexBuilder.AddEdgeVertex(forwardEdge, vertexAttributes);
				vertexAttributes.Advance();

				backwardEdge = backwardEdge.prev;
				if (forwardEdge == backwardEdge) break;

				vertexBuilder.AddEdgeVertex(backwardEdge, vertexAttributes);
				vertexAttributes.Advance();

				forwardEdge = forwardEdge.next;
			}
		}

		private void BuildFaceEdgeFans(Topology.FacesIndexer faces, IVertexBuilder vertexBuilder)
		{
			var submeshList = new List<Submesh>();

			var vertexAttributeArrays = new DynamicVertexAttributeArrays(_vertexAttributes, Mathf.Min(_maxVerticesPerSubmesh, faces.Count * 6));
			var triangleIndices = new List<int>();

			foreach (var face in faces)
			{
				var vertexCount = face.neighborCount;

				if (vertexAttributeArrays.index + vertexCount > _maxVerticesPerSubmesh)
				{
					submeshList.Add(new Submesh(vertexAttributeArrays, triangleIndices));
					vertexAttributeArrays.Reset();
					triangleIndices.Clear();
				}

				_faceSubmeshIndices[face] = submeshList.Count;
				_faceFirstVertexIndices[face] = vertexAttributeArrays.index;

				vertexAttributeArrays.Grow(vertexCount);

				int firstVertexIndex = vertexAttributeArrays.index;
				int nextVertexIndex = firstVertexIndex + 1;

				int triangleCount = face.neighborCount - 2;
				for (int triangleIndex = 0; triangleIndex < triangleCount; ++triangleIndex)
				{
					triangleIndices.Add(firstVertexIndex);
					triangleIndices.Add(nextVertexIndex);
					triangleIndices.Add(++nextVertexIndex);
				}

				RebuildFaceEdgeFan(face, vertexBuilder, vertexAttributeArrays);
			}

			if (vertexAttributeArrays.index > 0)
			{
				submeshList.Add(new Submesh(vertexAttributeArrays, triangleIndices));
			}

			_submeshes = submeshList.ToArray();
		}

		private void RebuildFaceEdgeFan(Topology.Face face, IVertexBuilder vertexBuilder, IIndexedVertexAttributes vertexAttributes)
		{
			foreach (var edge in face.edges)
			{
				vertexBuilder.AddEdgeVertex(edge, vertexAttributes);
				vertexAttributes.Advance();
			}
		}

		private void BuildFaceCenterFans(Topology.FacesIndexer faces, IVertexBuilder vertexBuilder)
		{
			var submeshList = new List<Submesh>();

			var vertexAttributeArrays = new DynamicVertexAttributeArrays(_vertexAttributes, Mathf.Min(_maxVerticesPerSubmesh, faces.Count * 8));
			var triangleIndices = new List<int>();

			foreach (var face in faces)
			{
				var vertexCount = face.neighborCount + 2;

				if (vertexAttributeArrays.index + vertexCount > _maxVerticesPerSubmesh)
				{
					submeshList.Add(new Submesh(vertexAttributeArrays, triangleIndices));
					vertexAttributeArrays.Reset();
					triangleIndices.Clear();
				}

				_faceSubmeshIndices[face] = submeshList.Count;
				_faceFirstVertexIndices[face] = vertexAttributeArrays.index;

				vertexAttributeArrays.Grow(vertexCount);

				int centerVertexIndex = vertexAttributeArrays.index;

				int nextVertexIndex = centerVertexIndex + 1;

				int triangleCount = face.neighborCount;
				for (int triangleIndex = 0; triangleIndex < triangleCount; ++triangleIndex)
				{
					triangleIndices.Add(centerVertexIndex);
					triangleIndices.Add(nextVertexIndex);
					triangleIndices.Add(++nextVertexIndex);
				}

				RebuildFaceCenterFan(face, vertexBuilder, vertexAttributeArrays);
			}

			if (vertexAttributeArrays.index > 0)
			{
				submeshList.Add(new Submesh(vertexAttributeArrays, triangleIndices));
			}

			_submeshes = submeshList.ToArray();
		}

		private void RebuildFaceCenterFan(Topology.Face face, IVertexBuilder vertexBuilder, IIndexedVertexAttributes vertexAttributes)
		{
			vertexBuilder.AddCenterVertex(face, vertexAttributes);
			vertexAttributes.Advance();

			foreach (var edge in face.edges)
			{
				vertexBuilder.AddEdgeVertex(edge, vertexAttributes);
				vertexAttributes.Advance();
			}

			vertexBuilder.AddDuplicateFirstEdgeVertex(face, vertexAttributes);
			vertexAttributes.Advance();
		}

		private void BuildFaceUmbrellas(Topology.FacesIndexer faces, IVertexBuilder vertexBuilder)
		{
			var submeshList = new List<Submesh>();

			var vertexAttributeArrays = new DynamicVertexAttributeArrays(_vertexAttributes, Mathf.Min(_maxVerticesPerSubmesh, faces.Count * 7));
			var triangleIndices = new List<int>();

			foreach (var face in faces)
			{
				var vertexCount = face.neighborCount + 1;

				if (vertexAttributeArrays.index + vertexCount > _maxVerticesPerSubmesh)
				{
					submeshList.Add(new Submesh(vertexAttributeArrays, triangleIndices));
					vertexAttributeArrays.Reset();
					triangleIndices.Clear();
				}

				_faceSubmeshIndices[face] = submeshList.Count;
				_faceFirstVertexIndices[face] = vertexAttributeArrays.index;

				vertexAttributeArrays.Grow(vertexCount);

				int centerVertexIndex = vertexAttributeArrays.index;

				int nextVertexIndex = centerVertexIndex + 1;

				int triangleCount = face.neighborCount;
				for (int triangleIndex = 0; triangleIndex < triangleCount; ++triangleIndex)
				{
					triangleIndices.Add(centerVertexIndex);
					triangleIndices.Add(nextVertexIndex);
					triangleIndices.Add(++nextVertexIndex);
				}

				triangleIndices[triangleIndices.Count - 1] = centerVertexIndex + 1;

				RebuildFaceUmbrella(face, vertexBuilder, vertexAttributeArrays);
			}

			if (vertexAttributeArrays.index > 0)
			{
				submeshList.Add(new Submesh(vertexAttributeArrays, triangleIndices));
			}

			_submeshes = submeshList.ToArray();
		}

		private void RebuildFaceUmbrella(Topology.Face face, IVertexBuilder vertexBuilder, IIndexedVertexAttributes vertexAttributes)
		{
			vertexBuilder.AddCenterVertex(face, vertexAttributes);
			vertexAttributes.Advance();

			foreach (var edge in face.edges)
			{
				vertexBuilder.AddEdgeVertex(edge, vertexAttributes);
				vertexAttributes.Advance();
			}
		}

		#endregion
	}
}
