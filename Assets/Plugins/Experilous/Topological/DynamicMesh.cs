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
		[SerializeField] private int _maxVerticesPerSubmesh;

		[SerializeField] private Submesh[] _submeshes;
		[SerializeField] private IntFaceAttribute _faceSubmeshIndices;
		[SerializeField] private IntFaceAttribute _faceFirstVertexIndices;

		private IndexedVertexAttributeArrays _cachedIndexedVertexAttributeArrays;

		public static DynamicMesh Create(Topology.FacesIndexer faces, VertexAttributes vertexAttributes, ITriangulation triangulation, int maxVerticesPerSubmesh = 65534)
		{
			var dynamicMesh = CreateInstance<DynamicMesh>();
			dynamicMesh._vertexAttributes = vertexAttributes;
			dynamicMesh._maxVerticesPerSubmesh = maxVerticesPerSubmesh;
			dynamicMesh.Initialize(faces, triangulation);
			return dynamicMesh;
		}

		private void Initialize(Topology.FacesIndexer faces, ITriangulation triangulation)
		{
			_faceSubmeshIndices = IntFaceAttribute.Create(faces.Count);
			_faceFirstVertexIndices = IntFaceAttribute.Create(faces.Count);

			_cachedIndexedVertexAttributeArrays = new IndexedVertexAttributeArrays();

			var submeshList = new List<Submesh>();

			var initialVertexCount = Mathf.Min(_maxVerticesPerSubmesh, faces.Count * 3); // Very conservative estimate.
			var vertexAttributeArrays = new DynamicVertexAttributeArrays(_vertexAttributes, initialVertexCount);
			var triangleIndices = new List<int>();

			foreach (var face in faces)
			{
				var vertexCount = triangulation.GetVertexCount(face);

				if (vertexAttributeArrays.index + vertexCount > _maxVerticesPerSubmesh)
				{
					triangulation.FinalizeSubmesh(submeshList.Count);
					submeshList.Add(new Submesh(vertexAttributeArrays, triangleIndices));
					vertexAttributeArrays.Reset();
					triangleIndices.Clear();
				}

				_faceSubmeshIndices[face] = submeshList.Count;
				_faceFirstVertexIndices[face] = vertexAttributeArrays.index;

				vertexAttributeArrays.Grow(vertexCount);

				triangulation.BuildFace(face, vertexAttributeArrays, triangleIndices);
			}

			if (vertexAttributeArrays.index > 0)
			{
				triangulation.FinalizeSubmesh(submeshList.Count);
				submeshList.Add(new Submesh(vertexAttributeArrays, triangleIndices));
			}

			_submeshes = submeshList.ToArray();
		}

		public void RebuildFace(Topology.Face face, ITriangulation triangulation)
		{
			var submesh = _submeshes[_faceSubmeshIndices[face]];
			var vertexAttributeArrays = GetIndexedVertexAttributeArrays(submesh, _faceFirstVertexIndices[face]);

			triangulation.RebuildFace(face, vertexAttributeArrays);

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

		private IndexedVertexAttributeArrays GetIndexedVertexAttributeArrays(Submesh submesh, int index)
		{
			if (ThreadUtility.isMainThread)
			{
				return _cachedIndexedVertexAttributeArrays.MoveTo(submesh, index);
			}
			else
			{
				return new IndexedVertexAttributeArrays(submesh, index);
			}
		}

		public Mesh GetSubmesh(int index)
		{
			return _submeshes[index].mesh;
		}

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
			Vector3 position { get; set; }
			Vector3 normal { get; set; }
			Color color { get; set; }
			Color32 color32 { get; set; }
			Vector2 uv { get; set; }
			Vector2 uv1 { get; set; }
			Vector2 uv2 { get; set; }
			Vector2 uv3 { get; set; }
			Vector2 uv4 { get; set; }
			Vector4 tangent { get; set; }
		}

		public interface IIndexedVertexAttributes : IVertexAttributes
		{
			int index { get; }
			void Advance();
		}

		public interface ITriangulation
		{
			int GetVertexCount(Topology.Face face);
			void BuildFace(Topology.Face face, IIndexedVertexAttributes vertexAttributes, IList<int> triangleIndices);
			void RebuildFace(Topology.Face face, IIndexedVertexAttributes vertexAttributes);
			void FinalizeSubmesh(int index);
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

		private class DynamicVertexAttributeArrays : IIndexedVertexAttributes
		{
			private int _index = 0;
			private int _capacity = 0;

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
				_capacity = capacity;

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

			public int index { get { return _index; } }

			public Vector3 position { get { return positions[_index]; } set { positions[_index] = value; } }
			public Vector3 normal { get { return normals[_index]; } set { normals[_index] = value; } }
			public Color color { get { return colors[_index]; } set { colors[_index] = value; } }
			public Color32 color32 { get { return colors32[_index]; } set { colors32[_index] = value; } }
			public Vector2 uv { get { return uvs[_index]; } set { uvs[_index] = value; } }
			public Vector2 uv1 { get { return uvs[_index]; } set { uvs[_index] = value; } }
			public Vector2 uv2 { get { return uvs2[_index]; } set { uvs2[_index] = value; } }
			public Vector2 uv3 { get { return uvs3[_index]; } set { uvs3[_index] = value; } }
			public Vector2 uv4 { get { return uvs4[_index]; } set { uvs4[_index] = value; } }
			public Vector4 tangent { get { return tangents[_index]; } set { tangents[_index] = value; } }

			public DynamicVertexAttributeArrays Grow(int vertexCount)
			{
				while (_index + vertexCount > _capacity)
				{
					_capacity = _capacity * 3 / 2;
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
					var newArray = new T[_capacity];
					Array.Copy(array, newArray, _index);
					array = newArray;
				}
			}

			public void Advance()
			{
				++_index;
			}

			public void Reset()
			{
				_index = 0;
			}
		}

		private class IndexedVertexAttributeArrays : IIndexedVertexAttributes
		{
			private Submesh _submesh;
			private int _index;

			public IndexedVertexAttributeArrays()
			{
				_submesh = null;
				_index = 0;
			}

			public IndexedVertexAttributeArrays(Submesh submesh)
			{
				_submesh = submesh;
				_index = 0;
			}

			public IndexedVertexAttributeArrays(Submesh submesh, int index)
			{
				_submesh = submesh;
				_index = index;
			}

			public int index { get { return _index; } }

			public Vector3 position { get { return _submesh.positions[_index]; } set { _submesh.positions[_index] = value; } }
			public Vector3 normal { get { return _submesh.normals[_index]; } set { _submesh.normals[_index] = value; } }
			public Color color { get { return _submesh.colors[_index]; } set { _submesh.colors[_index] = value; } }
			public Color32 color32 { get { return _submesh.colors32[_index]; } set { _submesh.colors32[_index] = value; } }
			public Vector2 uv { get { return _submesh.uvs[_index]; } set { _submesh.uvs[_index] = value; } }
			public Vector2 uv1 { get { return _submesh.uvs[_index]; } set { _submesh.uvs[_index] = value; } }
			public Vector2 uv2 { get { return _submesh.uvs2[_index]; } set { _submesh.uvs2[_index] = value; } }
			public Vector2 uv3 { get { return _submesh.uvs3[_index]; } set { _submesh.uvs3[_index] = value; } }
			public Vector2 uv4 { get { return _submesh.uvs4[_index]; } set { _submesh.uvs4[_index] = value; } }
			public Vector4 tangent { get { return _submesh.tangents[_index]; } set { _submesh.tangents[_index] = value; } }

			public void Advance()
			{
				++_index;
			}

			public IndexedVertexAttributeArrays MoveTo(Submesh submesh, int index)
			{
				_submesh = submesh;
				_index = index;
				return this;
			}
		}

		#endregion

		#region Triangulation

		public class SeparatedFacesStripTriangulation : ITriangulation
		{
			private int _ringDepth;

			private Action<Topology.FaceEdge, IIndexedVertexAttributes> _addRingVertices;

			public SeparatedFacesStripTriangulation(Action<Topology.FaceEdge, IIndexedVertexAttributes> addRingVertices)
			{
				_ringDepth = 1;
				_addRingVertices = addRingVertices;
			}

			public SeparatedFacesStripTriangulation(int ringDepth, Action<Topology.FaceEdge, IIndexedVertexAttributes> addRingVertices)
			{
				if (ringDepth < 1) throw new ArgumentOutOfRangeException("ringDepth");
				_ringDepth = ringDepth;
				_addRingVertices = addRingVertices;
			}

			public int GetVertexCount(Topology.Face face)
			{
				return face.neighborCount * _ringDepth;
			}

			public void BuildFace(Topology.Face face, IIndexedVertexAttributes vertexAttributes, IList<int> triangleIndices)
			{
				int neighborCount = face.neighborCount;
				int currentOuterVertexIndex = vertexAttributes.index;

				if (_ringDepth > 1)
				{
					int nextOuterVertexIndex = currentOuterVertexIndex + _ringDepth;

					for (int neighborIndex = 1; neighborIndex < neighborCount; ++neighborIndex)
					{
						int outerDepthIndex = 0;
						for (int innerDepthIndex = 1; innerDepthIndex < _ringDepth; ++innerDepthIndex)
						{
							triangleIndices.Add(currentOuterVertexIndex + outerDepthIndex);
							triangleIndices.Add(nextOuterVertexIndex + outerDepthIndex);
							triangleIndices.Add(currentOuterVertexIndex + innerDepthIndex);
							triangleIndices.Add(currentOuterVertexIndex + innerDepthIndex);
							triangleIndices.Add(nextOuterVertexIndex + outerDepthIndex);
							triangleIndices.Add(nextOuterVertexIndex + innerDepthIndex);
							outerDepthIndex = innerDepthIndex;
						}

						currentOuterVertexIndex = nextOuterVertexIndex;
						nextOuterVertexIndex += _ringDepth;
					}

					// Triangles that wrap-around and reuse the first column of vertices.
					{
						nextOuterVertexIndex = vertexAttributes.index;
						int outerDepthIndex = 0;
						for (int innerDepthIndex = 1; innerDepthIndex < _ringDepth; ++innerDepthIndex)
						{
							triangleIndices.Add(currentOuterVertexIndex + outerDepthIndex);
							triangleIndices.Add(nextOuterVertexIndex + outerDepthIndex);
							triangleIndices.Add(currentOuterVertexIndex + innerDepthIndex);
							triangleIndices.Add(currentOuterVertexIndex + innerDepthIndex);
							triangleIndices.Add(nextOuterVertexIndex + outerDepthIndex);
							triangleIndices.Add(nextOuterVertexIndex + innerDepthIndex);
							outerDepthIndex = innerDepthIndex;
						}
					}
				}

				int forwardVertexIndex = vertexAttributes.index + _ringDepth - 1;
				int backwardVertexIndex = vertexAttributes.index + neighborCount * _ringDepth - 1;

				int triangleIndex = 2;
				while (triangleIndex < neighborCount)
				{
					triangleIndices.Add(backwardVertexIndex);
					triangleIndices.Add(forwardVertexIndex);
					backwardVertexIndex -= _ringDepth;
					triangleIndices.Add(backwardVertexIndex);

					if (++triangleIndex == neighborCount) break;

					triangleIndices.Add(backwardVertexIndex);
					triangleIndices.Add(forwardVertexIndex);
					forwardVertexIndex += _ringDepth;
					triangleIndices.Add(forwardVertexIndex);

					++triangleIndex;
				}

				RebuildFace(face, vertexAttributes);
			}

			public void RebuildFace(Topology.Face face, IIndexedVertexAttributes vertexAttributes)
			{
				foreach (var edge in face.edges)
				{
					_addRingVertices(edge, vertexAttributes);
				}
			}

			public void FinalizeSubmesh(int index)
			{
			}
		}

		public class SeparatedFacesEdgeFanTriangulation : ITriangulation
		{
			private int _ringDepth;
			private Action<Topology.FaceEdge, IIndexedVertexAttributes> _addRingVertices;

			public SeparatedFacesEdgeFanTriangulation(Action<Topology.FaceEdge, IIndexedVertexAttributes> addRingVertices)
			{
				_ringDepth = 1;
				_addRingVertices = addRingVertices;
			}

			public SeparatedFacesEdgeFanTriangulation(int ringDepth, Action<Topology.FaceEdge, IIndexedVertexAttributes> addRingVertices)
			{
				if (ringDepth < 1) throw new ArgumentOutOfRangeException("ringDepth");
				_ringDepth = ringDepth;
				_addRingVertices = addRingVertices;
			}

			public int GetVertexCount(Topology.Face face)
			{
				return face.neighborCount * _ringDepth;
			}

			public void BuildFace(Topology.Face face, IIndexedVertexAttributes vertexAttributes, IList<int> triangleIndices)
			{
				int neighborCount = face.neighborCount;
				int currentOuterVertexIndex = vertexAttributes.index;

				if (_ringDepth > 1)
				{
					int nextOuterVertexIndex = currentOuterVertexIndex + _ringDepth;

					for (int neighborIndex = 1; neighborIndex < neighborCount; ++neighborIndex)
					{
						int outerDepthIndex = 0;
						for (int innerDepthIndex = 1; innerDepthIndex < _ringDepth; ++innerDepthIndex)
						{
							triangleIndices.Add(currentOuterVertexIndex + outerDepthIndex);
							triangleIndices.Add(nextOuterVertexIndex + outerDepthIndex);
							triangleIndices.Add(currentOuterVertexIndex + innerDepthIndex);
							triangleIndices.Add(currentOuterVertexIndex + innerDepthIndex);
							triangleIndices.Add(nextOuterVertexIndex + outerDepthIndex);
							triangleIndices.Add(nextOuterVertexIndex + innerDepthIndex);
							outerDepthIndex = innerDepthIndex;
						}

						currentOuterVertexIndex = nextOuterVertexIndex;
						nextOuterVertexIndex += _ringDepth;
					}

					// Triangles that wrap-around and reuse the first column of vertices.
					{
						nextOuterVertexIndex = vertexAttributes.index;
						int outerDepthIndex = 0;
						for (int innerDepthIndex = 1; innerDepthIndex < _ringDepth; ++innerDepthIndex)
						{
							triangleIndices.Add(currentOuterVertexIndex + outerDepthIndex);
							triangleIndices.Add(nextOuterVertexIndex + outerDepthIndex);
							triangleIndices.Add(currentOuterVertexIndex + innerDepthIndex);
							triangleIndices.Add(currentOuterVertexIndex + innerDepthIndex);
							triangleIndices.Add(nextOuterVertexIndex + outerDepthIndex);
							triangleIndices.Add(nextOuterVertexIndex + innerDepthIndex);
							outerDepthIndex = innerDepthIndex;
						}
					}
				}

				int innerRingFirstVertexIndex = vertexAttributes.index + _ringDepth - 1;
				int innerRingNextVertexIndex = innerRingFirstVertexIndex + _ringDepth;
				for (int triangleIndex = 2; triangleIndex < neighborCount; ++triangleIndex)
				{
					triangleIndices.Add(innerRingFirstVertexIndex);
					triangleIndices.Add(innerRingNextVertexIndex);
					innerRingNextVertexIndex += _ringDepth;
					triangleIndices.Add(innerRingNextVertexIndex);
				}

				RebuildFace(face, vertexAttributes);
			}

			public void RebuildFace(Topology.Face face, IIndexedVertexAttributes vertexAttributes)
			{
				foreach (var edge in face.edges)
				{
					_addRingVertices(edge, vertexAttributes);
				}
			}

			public void FinalizeSubmesh(int index)
			{
			}
		}

		public class SeparatedFacesCenterFanTriangulation : ITriangulation
		{
			private int _ringDepth;

			private Action<Topology.FaceEdge, IIndexedVertexAttributes> _addRingVertices;
			private Action<Topology.FaceEdge, IIndexedVertexAttributes> _addCenterVertex;
			private Action<Topology.Face, IIndexedVertexAttributes> _addDuplicateRingVertices;

			public SeparatedFacesCenterFanTriangulation(
				Action<Topology.FaceEdge, IIndexedVertexAttributes> addRingVertices,
				Action<Topology.FaceEdge, IIndexedVertexAttributes> addCenterVertex,
				Action<Topology.Face, IIndexedVertexAttributes> addDuplicateRingVertices)
			{
				_ringDepth = 1;
				_addRingVertices = addRingVertices;
				_addCenterVertex = addCenterVertex;
				_addDuplicateRingVertices = addDuplicateRingVertices;
			}

			public SeparatedFacesCenterFanTriangulation(
				int ringDepth,
				Action<Topology.FaceEdge, IIndexedVertexAttributes> addRingVertices,
				Action<Topology.FaceEdge, IIndexedVertexAttributes> addCenterVertex,
				Action<Topology.Face, IIndexedVertexAttributes> addDuplicateRingVertices)
			{
				if (ringDepth < 1) throw new ArgumentOutOfRangeException("ringDepth");
				_ringDepth = ringDepth;
				_addRingVertices = addRingVertices;
				_addCenterVertex = addCenterVertex;
				_addDuplicateRingVertices = addDuplicateRingVertices;
			}

			public int GetVertexCount(Topology.Face face)
			{
				return face.neighborCount * (_ringDepth + 1) + _ringDepth;
			}

			public void BuildFace(Topology.Face face, IIndexedVertexAttributes vertexAttributes, IList<int> triangleIndices)
			{
				int neighborCount = face.neighborCount;
				int currentOuterVertexIndex = vertexAttributes.index;
				int nextOuterVertexIndex = currentOuterVertexIndex + _ringDepth + 1;

				for (int neighborIndex = 0; neighborIndex < neighborCount; ++neighborIndex)
				{
					int outerDepthIndex = 0;
					for (int innerDepthIndex = 1; innerDepthIndex < _ringDepth; ++innerDepthIndex)
					{
						triangleIndices.Add(currentOuterVertexIndex + outerDepthIndex);
						triangleIndices.Add(nextOuterVertexIndex + outerDepthIndex);
						triangleIndices.Add(currentOuterVertexIndex + innerDepthIndex);
						triangleIndices.Add(currentOuterVertexIndex + innerDepthIndex);
						triangleIndices.Add(nextOuterVertexIndex + outerDepthIndex);
						triangleIndices.Add(nextOuterVertexIndex + innerDepthIndex);
						outerDepthIndex = innerDepthIndex;
					}

					triangleIndices.Add(currentOuterVertexIndex + outerDepthIndex);
					triangleIndices.Add(nextOuterVertexIndex + outerDepthIndex);
					triangleIndices.Add(currentOuterVertexIndex + _ringDepth);

					currentOuterVertexIndex = nextOuterVertexIndex;
					nextOuterVertexIndex += _ringDepth + 1;
				}

				RebuildFace(face, vertexAttributes);
			}

			public void RebuildFace(Topology.Face face, IIndexedVertexAttributes vertexAttributes)
			{
				var firstEdge = face.firstEdge;
				var edge = firstEdge;
				do
				{
					_addRingVertices(edge, vertexAttributes);
					edge = edge.next;
					_addCenterVertex(edge, vertexAttributes);
				} while (edge != firstEdge);

				_addDuplicateRingVertices(face, vertexAttributes);
			}

			public void FinalizeSubmesh(int index)
			{
			}
		}

		public class SeparatedFacesUmbrellaTriangulation : ITriangulation
		{
			private int _ringDepth;

			private Action<Topology.FaceEdge, IIndexedVertexAttributes> _addRingVertices;
			private Action<Topology.Face, IIndexedVertexAttributes> _addCenterVertex;

			public SeparatedFacesUmbrellaTriangulation(
				Action<Topology.FaceEdge, IIndexedVertexAttributes> addRingVertices,
				Action<Topology.Face, IIndexedVertexAttributes> addCenterVertex)
			{
				_ringDepth = 1;
				_addRingVertices = addRingVertices;
				_addCenterVertex = addCenterVertex;
			}

			public SeparatedFacesUmbrellaTriangulation(
				int ringDepth,
				Action<Topology.FaceEdge, IIndexedVertexAttributes> addRingVertices,
				Action<Topology.Face, IIndexedVertexAttributes> addCenterVertex)
			{
				if (ringDepth < 1) throw new ArgumentOutOfRangeException("ringDepth");
				_ringDepth = ringDepth;
				_addRingVertices = addRingVertices;
				_addCenterVertex = addCenterVertex;
			}

			public int GetVertexCount(Topology.Face face)
			{
				return face.neighborCount * _ringDepth + 1;
			}

			public void BuildFace(Topology.Face face, IIndexedVertexAttributes vertexAttributes, IList<int> triangleIndices)
			{
				int neighborCount = face.neighborCount;
				int currentOuterVertexIndex = vertexAttributes.index;
				int nextOuterVertexIndex = currentOuterVertexIndex + _ringDepth;
				int centerVertexIndex = currentOuterVertexIndex + neighborCount * _ringDepth;

				for (int neighborIndex = 1; neighborIndex < neighborCount; ++neighborIndex)
				{
					int outerDepthIndex = 0;
					for (int innerDepthIndex = 1; innerDepthIndex < _ringDepth; ++innerDepthIndex)
					{
						triangleIndices.Add(currentOuterVertexIndex + outerDepthIndex);
						triangleIndices.Add(nextOuterVertexIndex + outerDepthIndex);
						triangleIndices.Add(currentOuterVertexIndex + innerDepthIndex);
						triangleIndices.Add(currentOuterVertexIndex + innerDepthIndex);
						triangleIndices.Add(nextOuterVertexIndex + outerDepthIndex);
						triangleIndices.Add(nextOuterVertexIndex + innerDepthIndex);
						outerDepthIndex = innerDepthIndex;
					}

					triangleIndices.Add(currentOuterVertexIndex + outerDepthIndex);
					triangleIndices.Add(nextOuterVertexIndex + outerDepthIndex);
					triangleIndices.Add(centerVertexIndex);

					currentOuterVertexIndex = nextOuterVertexIndex;
					nextOuterVertexIndex += _ringDepth;
				}

				// Triangles that wrap-around and reuse the first column of vertices.
				{
					nextOuterVertexIndex = vertexAttributes.index;
					int outerDepthIndex = 0;
					for (int innerDepthIndex = 1; innerDepthIndex < _ringDepth; ++innerDepthIndex)
					{
						triangleIndices.Add(currentOuterVertexIndex + outerDepthIndex);
						triangleIndices.Add(nextOuterVertexIndex + outerDepthIndex);
						triangleIndices.Add(currentOuterVertexIndex + innerDepthIndex);
						triangleIndices.Add(currentOuterVertexIndex + innerDepthIndex);
						triangleIndices.Add(nextOuterVertexIndex + outerDepthIndex);
						triangleIndices.Add(nextOuterVertexIndex + innerDepthIndex);
						outerDepthIndex = innerDepthIndex;
					}

					triangleIndices.Add(currentOuterVertexIndex + outerDepthIndex);
					triangleIndices.Add(nextOuterVertexIndex + outerDepthIndex);
					triangleIndices.Add(centerVertexIndex);
				}

				RebuildFace(face, vertexAttributes);
			}

			public void RebuildFace(Topology.Face face, IIndexedVertexAttributes vertexAttributes)
			{
				foreach (var edge in face.edges)
				{
					_addRingVertices(edge, vertexAttributes);
				}

				_addCenterVertex(face, vertexAttributes);
			}

			public void FinalizeSubmesh(int index)
			{
			}
		}

		#endregion
	}
}
