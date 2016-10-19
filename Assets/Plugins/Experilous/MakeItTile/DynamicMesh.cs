/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System;
using ThreadUtility = Experilous.Core.ThreadUtility;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// Manages the creation and modification of a topology-based mesh which may need to be split into several smaller submeshes.
	/// </summary>
	/// <remarks>
	/// <para>Meshes generated from topologies are likely to come in many different formats,
	/// highly dependent upon the particular rendering needs of each use.  This class is aimed
	/// at simplifying much of this complexity, making it easier to plug in the unique needs
	/// of each use, while maintaining a common interface for working with the mesh.</para>
	/// 
	/// <para>One of the main common behaviors for dynamic meshes is the need to split any
	/// mesh that goes beyond Unity's maximum vertex count limit per mesh.  This class will
	/// automatically create and manage multiple submeshes to remain under that limit, or any
	/// other lower limit specified during creation.</para>
	/// 
	/// <para>Another common behavior is the process of modifying and updating some aspects of
	/// a mesh, whether that be a subset of vertices, or a subset of the vertex attributes in
	/// use.  Any attribute and any submesh can be touched, and then a single call can be made
	/// to apply the changes only for the vertex attributes and submeshes affected.</para>
	/// 
	/// <para>One major behavior that needs to be adaptable for each unique situation is the
	/// way in which topology faces are converted to triangles, and how vertices are generated
	/// and indexed by these triangles.  The ITriangulation interface offers a flexible
	/// customization point in this regard, and some common triangulation schemes are also
	/// provided.</para>
	/// 
	/// <para>Another aspect of a dynamic mesh that needs to be adaptable to each situation is
	/// the set of vertex attributes that are used to describe the mesh.  Unity's mesh class
	/// exposes nine different attributes:  position, normal, color/color32, uv 1/2/3/4, and
	/// tangents.  Not every mesh will need all attributes defined; this depends on the
	/// shader(s) that will be used to render the mesh.  These are thus fully specifiable, and
	/// the triangulation classes make it easy to customize how these attributes are assigned
	/// values from external sources.</para>
	/// </remarks>
	[Serializable]
	public class DynamicMesh : ScriptableObject
	{
		[SerializeField] private VertexAttributes _vertexAttributes;
		[SerializeField] private int _maxVerticesPerSubmesh;

		[SerializeField] private Submesh[] _submeshes;
		[SerializeField] private int[] _faceSubmeshIndices;
		[SerializeField] private int[] _faceFirstVertexIndices;

		private IndexedVertexAttributeArrays _cachedIndexedVertexAttributeArrays;

		/// <summary>
		/// Creates a dynamic mesh instance for the given face groups and faces, using the specified vertex attributes and triangulation object to construct the vertex data and triangles.
		/// </summary>
		/// <param name="faceGroups">The face groups for which to generate triangle meshes.  Each group will map to at least one submesh.</param>
		/// <param name="vertexAttributes">The types of vertex data to include in the meshes, such as position, color, and normal.</param>
		/// <param name="triangulation">The object which is responsible for turning each individual face into triangles with a mesh.</param>
		/// <param name="maxVerticesPerSubmesh">The maximum number of vertices allowed per submesh.  Defaults to Unity's built-in maximum of 65534.</param>
		/// <returns>A dynamic mesh instance created from the given face groups and triangulation.</returns>
		public static DynamicMesh Create(IEnumerable<IEnumerable<Topology.Face>> faceGroups, VertexAttributes vertexAttributes, ITriangulation triangulation, int maxVerticesPerSubmesh = 65534)
		{
			var dynamicMesh = CreateInstance<DynamicMesh>();
			dynamicMesh._vertexAttributes = vertexAttributes;
			dynamicMesh._maxVerticesPerSubmesh = maxVerticesPerSubmesh;
			dynamicMesh.Initialize(faceGroups, triangulation);
			return dynamicMesh;
		}

		/// <summary>
		/// Creates a dynamic mesh instance for the given faces, using the specified vertex attributes and triangulation object to construct the vertex data and triangles.
		/// </summary>
		/// <param name="faces">The faces for which to generate triangle meshes.</param>
		/// <param name="vertexAttributes">The types of vertex data to include in the meshes, such as position, color, and normal.</param>
		/// <param name="triangulation">The object which is responsible for turning each individual face into triangles with a mesh.</param>
		/// <param name="maxVerticesPerSubmesh">The maximum number of vertices allowed per submesh.  Defaults to Unity's built-in maximum of 65534.</param>
		/// <returns>A dynamic mesh instance created from the given faces and triangulation.</returns>
		public static DynamicMesh Create(IEnumerable<Topology.Face> faces, VertexAttributes vertexAttributes, ITriangulation triangulation, int maxVerticesPerSubmesh = 65534)
		{
			var dynamicMesh = CreateInstance<DynamicMesh>();
			dynamicMesh._vertexAttributes = vertexAttributes;
			dynamicMesh._maxVerticesPerSubmesh = maxVerticesPerSubmesh;
			dynamicMesh.Initialize(new IEnumerable<Topology.Face>[] { faces }, triangulation);
			return dynamicMesh;
		}

		private void Initialize(IEnumerable<IEnumerable<Topology.Face>> faceGroups, ITriangulation triangulation)
		{
			int maxFaceIndex = -1;
			var dynamicFaceSubmeshIndicesArray = new int[65536];
			var dynamicFaceFirstVertexIndicesArray = new int[65536];

			_cachedIndexedVertexAttributeArrays = new IndexedVertexAttributeArrays();

			var submeshList = new List<Submesh>();

			var vertexAttributeArrays = new DynamicVertexAttributeArrays(_vertexAttributes, _maxVerticesPerSubmesh);
			var triangleIndices = new List<int>();

			foreach (var faceGroup in faceGroups)
			{
				foreach (var face in faceGroup)
				{
					var vertexCount = triangulation.GetVertexCount(face);

					if (vertexAttributeArrays.index + vertexCount > _maxVerticesPerSubmesh)
					{
						triangulation.FinalizeSubmesh(submeshList.Count);
						submeshList.Add(new Submesh(vertexAttributeArrays, triangleIndices));
						vertexAttributeArrays.Reset();
						triangleIndices.Clear();
					}

					maxFaceIndex = Mathf.Max(maxFaceIndex, face.index);
					SetGrowableArrayElement(ref dynamicFaceSubmeshIndicesArray, face.index, submeshList.Count);
					SetGrowableArrayElement(ref dynamicFaceFirstVertexIndicesArray, face.index, vertexAttributeArrays.index);

					vertexAttributeArrays.Grow(vertexCount);

					triangulation.BuildFace(face, vertexAttributeArrays, triangleIndices);
				}

				if (vertexAttributeArrays.index > 0)
				{
					triangulation.FinalizeSubmesh(submeshList.Count);
					submeshList.Add(new Submesh(vertexAttributeArrays, triangleIndices));
					vertexAttributeArrays.Reset();
					triangleIndices.Clear();
				}
			}

			_submeshes = submeshList.ToArray();

			var faceCount = maxFaceIndex + 1;
			_faceSubmeshIndices = new int[faceCount];
			_faceFirstVertexIndices = new int[faceCount];
			Array.Copy(dynamicFaceSubmeshIndicesArray, _faceSubmeshIndices, faceCount);
			Array.Copy(dynamicFaceFirstVertexIndicesArray, _faceFirstVertexIndices, faceCount);
		}

		private void SetGrowableArrayElement<T>(ref T[] array, int index, T value)
		{
			if (array.Length <= index)
			{
				var capacity = array.Length;
				do
				{
					capacity += capacity * 3 / 2;
				} while (capacity <= index);
				var newArray = new T[capacity];
				Array.Copy(array, newArray, array.Length);
				array = newArray;
			}
			array[index] = value;
		}

		/// <summary>
		/// Provide new values for the vertices that constitute the triangles of the specified face.
		/// </summary>
		/// <param name="face">The face whose triangle vertices should be udpated.</param>
		/// <param name="triangulation">The triangulation method used for converting faces to triangles.</param>
		/// <remarks>
		/// <para>This method will update the vertex attribute values for this face, but will not
		/// immediately push these changes to the mesh itself.  Instead, it will simply mark the
		/// relevant submesh as dirty.  Call <see cref="RebuildMesh"/>() in order to commit all
		/// changes and apply them to the mesh.</para>
		/// 
		/// <para>The <paramref name="triangulation"/> parameter is expected to match the kind of
		/// triangulation used when first creating the dynamic mesh, but is permitted to set the
		/// vertex attributes however it likes.  That is, the number of triangles generated for
		/// each face and the vertex indices used by each of these triangles cannot change, only
		/// vertex attributes.  Not all vertex attributes need or are always expected to be
		/// changed either.  In many cases, simply changing normals or uvs, for example, is
		/// all that is needed, and all other vertex attribute values can remain the same.</para>
		/// </remarks>
		public void RebuildFace(Topology.Face face, ITriangulation triangulation)
		{
			var submesh = _submeshes[_faceSubmeshIndices[face.index]];
			var vertexAttributeArrays = GetIndexedVertexAttributeArrays(submesh, _faceFirstVertexIndices[face.index]);

			triangulation.RebuildFace(face, vertexAttributeArrays);

			submesh.isDirty = true;
		}

		/// <summary>
		/// Applies and uploads the specified vertex attribute arrays of all dirty submeshes to the Unity meshes themselves.
		/// </summary>
		/// <param name="dirtyVertexAttributes">The vertex attributes that should be uploaded.</param>
		public void RebuildMesh(VertexAttributes dirtyVertexAttributes)
		{
			foreach (var submesh in _submeshes)
			{
				if (submesh.isDirty) submesh.Rebuild(dirtyVertexAttributes);
			}
		}

		/// <summary>
		/// The number of submeshes contained within the current dynamic mesh.
		/// </summary>
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

		/// <summary>
		/// Returns the submesh at the given index.
		/// </summary>
		/// <param name="index">The zero-based index of the submesh to return.  Must a non-negative integer less than <see cref="submeshCount"/>.</param>
		/// <returns>The submesh at the given index.</returns>
		public Mesh GetSubmesh(int index)
		{
			if (index < 0 || index >= _submeshes.Length) throw new ArgumentOutOfRangeException("index");
			return _submeshes[index].mesh;
		}

		/// <summary>
		/// Replaces an internally stored submesh with the given submesh.  Necessary in some asset management and serialization scenarios.
		/// </summary>
		/// <param name="index">The index of the submesh to replace.</param>
		/// <param name="replacementSubmesh">The new mesh to replace an existing mesh.  Ideally it is an exact copy, different only by identity.</param>
		public void ReplaceSubmesh(int index, Mesh replacementSubmesh)
		{
			if (index < 0 || index >= _submeshes.Length) throw new ArgumentOutOfRangeException("index");
			if (replacementSubmesh == null) throw new ArgumentNullException("index");
			if (ReferenceEquals(_submeshes[index].mesh, replacementSubmesh)) return;
			_submeshes[index].mesh = replacementSubmesh;
		}

		/// <summary>
		/// An enumerable collection of all the submeshes.
		/// </summary>
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

		/// <summary>
		/// Flags for all the vertex attributes that can be used with a <see cref="UnityEngine.Mesh"/>.
		/// </summary>
		[Flags] public enum VertexAttributes
		{
			/// <summary>A value representing the selection of no vertex attributes.</summary>
			None = 0x000,
			/// <summary>A flag representing the position of a vertex.</summary>
			Position = 0x001,
			/// <summary>A flag representing the surface normal of a vertex.</summary>
			Normal = 0x002,
			/// <summary>A flag representing the color of a vertex.</summary>
			Color = 0x004,
			/// <summary>A flag representing the 32-bit color of a vertex.</summary>
			Color32 = 0x008,
			/// <summary>A flag representing the first UV coordinate of a vertex.</summary>
			UV = 0x010,
			/// <summary>A flag representing the first UV coordinate of a vertex.</summary>
			UV1 = 0x010,
			/// <summary>A flag representing the second UV coordinate of a vertex.</summary>
			UV2 = 0x020,
			/// <summary>A flag representing the third UV coordinate of a vertex.</summary>
			UV3 = 0x040,
			/// <summary>A flag representing the fourth UV coordinate of a vertex.</summary>
			UV4 = 0x080,
			/// <summary>A flag representing the tangent of a vertex.</summary>
			Tangent = 0x100,
			/// <summary>A value representing the selection of all the vertex attributes.</summary>
			All = Position | Normal | Color | Color32 | UV1 | UV2 | UV3 | UV4 | Tangent,
		}

		/// <summary>
		/// Interface for accessing the vertex attributes of a single vertex.
		/// </summary>
		public interface IVertexAttributes
		{
			/// <summary>The position of the current vertex.</summary>
			Vector3 position { get; set; }
			/// <summary>The surface normal of the current vertex.</summary>
			Vector3 normal { get; set; }
			/// <summary>The color of the current vertex.</summary>
			Color color { get; set; }
			/// <summary>The 32-bit color of the current vertex.</summary>
			Color32 color32 { get; set; }
			/// <summary>The first UV coordinate of the current vertex.</summary>
			Vector2 uv { get; set; }
			/// <summary>The first UV coordinate of the current vertex.</summary>
			Vector2 uv1 { get; set; }
			/// <summary>The second UV coordinate of the current vertex.</summary>
			Vector2 uv2 { get; set; }
			/// <summary>The third UV coordinate of the current vertex.</summary>
			Vector2 uv3 { get; set; }
			/// <summary>The fourth UV coordinate of the current vertex.</summary>
			Vector2 uv4 { get; set; }
			/// <summary>The tangent of the current vertex.</summary>
			Vector4 tangent { get; set; }
		}

		/// <summary>
		/// Interface for accessing the vertex attributes and index of a single vertex, as well advancing to the next vertex.
		/// </summary>
		public interface IIndexedVertexAttributes : IVertexAttributes
		{
			/// <summary>
			/// The index of the current vertex.
			/// </summary>
			int index { get; }

			/// <summary>
			/// Move this instance to the next vertex of the mesh, so that any access to vertex attributes affect that next vertex instead of the previous one.
			/// </summary>
			void Advance();
		}

		/// <summary>
		/// Interface for converting faces into collections of triangles.
		/// </summary>
		public interface ITriangulation
		{
			/// <summary>
			/// Returns the number of vertices needed for triangulating the face.
			/// </summary>
			/// <param name="face">The face whose vertex count is requested.</param>
			/// <returns>The number of vertices needed for triangulating the face.</returns>
			int GetVertexCount(Topology.Face face);

			/// <summary>
			/// Builds triangulation data for the given face, assigning it to the vertex attributes and triangle indices provided.
			/// </summary>
			/// <param name="face">The face to be triangulated.</param>
			/// <param name="vertexAttributes">The vertex attributes that will receive the values for the vertices triangulated for the face.</param>
			/// <param name="triangleIndices">The triangle indices list that will have the face's triangle indices appended to it.</param>
			void BuildFace(Topology.Face face, IIndexedVertexAttributes vertexAttributes, IList<int> triangleIndices);

			/// <summary>
			/// Rebuilds vertex attribute triangulation data for the given face, for when some or all of the vertex attributes have changed.
			/// </summary>
			/// <param name="face">The face to be re-triangulated.</param>
			/// <param name="vertexAttributes">The vertex attributes that will receive the new values for the vertices triangulated for the face.</param>
			void RebuildFace(Topology.Face face, IIndexedVertexAttributes vertexAttributes);

			/// <summary>
			/// Performs any final actions necessary when all the faces that will be triangulated to a submesh have been processed.
			/// </summary>
			/// <param name="index">The index of the submesh being finalized.</param>
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

					if (tangents != null)
					{
						// Since setting the tangents is necessary for updating binormals, then
						// we need to set the tangents if either the tangents themselves are
						// dirty or the normals which are used to compute the binormals are dirty.
						if ((dirtyVertexAttributes & VertexAttributes.Tangent) != 0 || normals != null && (dirtyVertexAttributes & VertexAttributes.Normal) != 0)
						{
							mesh.tangents = tangents;
						}
					}

					// If the positions have changed, recalculate the bounds.
					if (positions != null && (dirtyVertexAttributes & VertexAttributes.Position) != 0) { mesh.RecalculateBounds(); }

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
	}
}
