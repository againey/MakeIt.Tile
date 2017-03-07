/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System.Collections.Generic;
using System;
using Experilous.Topologies;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// Triangulates a topology face using a triangle strip pattern, not sharing vertices among other faces.
	/// </summary>
	/// <remarks>
	/// <para>This triangulation method uses the triangle strip pattern when constructing triangles
	/// from a face, starting at the first vertex and moving both forward and backward around the
	/// circumference of the face, creating triangles stretching across both sides, until the
	/// forward and backward traversals meet on the far side of the face.</para>
	/// 
	/// <para>In the simplest case, each face can be made using exactly as many vertices as the
	/// face has neighbors.  However, inner rings can also be generated, multiplying the number
	/// of vertices needed for each additional ring.  Ring triangles are generated independently
	/// from the core triangle strip pattern.</para>
	/// 
	/// <para>All vertices on the outer ring of the face are duplicated rather than shared with
	/// adjacent faces.</para>
	/// </remarks>
	/// <seealso cref="DynamicMesh"/>
	/// <seealso cref="DynamicMesh.ITriangulation"/>
	public class SeparatedFacesStripTriangulation : DynamicMesh.ITriangulation
	{
		private int _ringDepth;

		private Action<Topology.FaceEdge, DynamicMesh.IIndexedVertexAttributes> _addRingVertices;

		/// <summary>
		/// Construct a strip triangulation instance with no inner rings using the supplied delegate to set vertex attribute values.
		/// </summary>
		/// <param name="addRingVertices">The delegate used to set vertex attribute values.</param>
		/// <remarks>The <paramref name="addRingVertices"/> delegate is expected to call <see cref="DynamicMesh.IIndexedVertexAttributes.Advance()"/>
		/// once after each vertex that is added by the delegate, which should also be equal to the ring depth.</remarks>
		/// <seealso cref="DynamicMesh"/>
		/// <seealso cref="DynamicMesh.IIndexedVertexAttributes"/>
		public SeparatedFacesStripTriangulation(Action<Topology.FaceEdge, DynamicMesh.IIndexedVertexAttributes> addRingVertices)
		{
			_ringDepth = 1;
			_addRingVertices = addRingVertices;
		}

		/// <summary>
		/// Construct a strip triangulation instance with the specified number of vertex rings using the supplied delegate to set vertex attribute values.
		/// </summary>
		/// <param name="ringDepth">The total number of vertex rings to generate.
		/// The minimum value of 1 specifies just the outer ring, while values of 2 or more generate inner vertex rings also.</param>
		/// <param name="addRingVertices">The delegate used to set vertex attribute values.</param>
		/// <remarks>The <paramref name="addRingVertices"/> delegate is expected to call <see cref="DynamicMesh.IIndexedVertexAttributes.Advance()"/>
		/// once after each vertex that is added by the delegate, which should also be equal to the ring depth.</remarks>
		/// <seealso cref="DynamicMesh"/>
		/// <seealso cref="DynamicMesh.IIndexedVertexAttributes"/>
		public SeparatedFacesStripTriangulation(int ringDepth, Action<Topology.FaceEdge, DynamicMesh.IIndexedVertexAttributes> addRingVertices)
		{
			if (ringDepth < 1) throw new ArgumentOutOfRangeException("ringDepth");
			_ringDepth = ringDepth;
			_addRingVertices = addRingVertices;
		}

		/// <inheritdoc/>
		public int GetVertexCount(Topology.Face face)
		{
			return face.neighborCount * _ringDepth;
		}

		/// <inheritdoc/>
		public void BuildFace(Topology.Face face, DynamicMesh.IIndexedVertexAttributes vertexAttributes, IList<int> triangleIndices)
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

		/// <inheritdoc/>
		public void RebuildFace(Topology.Face face, DynamicMesh.IIndexedVertexAttributes vertexAttributes)
		{
			foreach (var edge in face.edges)
			{
				_addRingVertices(edge, vertexAttributes);
			}
		}

		/// <inheritdoc/>
		public void FinalizeSubmesh(int index)
		{
		}
	}

	/// <summary>
	/// Triangulates a topology face using a triangle fan pattern, not sharing vertices among other faces.
	/// </summary>
	/// <remarks>
	/// <para>This triangulation method uses the triangle fan pattern when constructing triangles
	/// from a face, using the first vertex as the fan base and moving clockwise around the
	/// circumference of the face, creating triangles fanning out from the first vertex.</para>
	/// 
	/// <para>In the simplest case, each face can be made using exactly as many vertices as the
	/// face has neighbors.  However, inner rings can also be generated, multiplying the number
	/// of vertices needed for each additional ring.  Ring triangles are generated independently
	/// from the core triangle fan pattern.</para>
	/// 
	/// <para>All vertices on the outer ring of the face are duplicated rather than shared with
	/// adjacent faces.</para>
	/// </remarks>
	/// <seealso cref="DynamicMesh"/>
	/// <seealso cref="DynamicMesh.ITriangulation"/>
	public class SeparatedFacesFanTriangulation : DynamicMesh.ITriangulation
	{
		private int _ringDepth;
		private Action<Topology.FaceEdge, DynamicMesh.IIndexedVertexAttributes> _addRingVertices;

		/// <summary>
		/// Construct a fan triangulation instance with no inner rings using the supplied delegate to set vertex attribute values.
		/// </summary>
		/// <param name="addRingVertices">The delegate used to set vertex attribute values.</param>
		/// <remarks>The <paramref name="addRingVertices"/> delegate is expected to call <see cref="DynamicMesh.IIndexedVertexAttributes.Advance()"/>
		/// once after each vertex that is added by the delegate, which should also be equal to the ring depth.</remarks>
		/// <seealso cref="DynamicMesh"/>
		/// <seealso cref="DynamicMesh.IIndexedVertexAttributes"/>
		public SeparatedFacesFanTriangulation(Action<Topology.FaceEdge, DynamicMesh.IIndexedVertexAttributes> addRingVertices)
		{
			_ringDepth = 1;
			_addRingVertices = addRingVertices;
		}

		/// <summary>
		/// Construct a fan triangulation instance with the specified number of vertex rings using the supplied delegate to set vertex attribute values.
		/// </summary>
		/// <param name="ringDepth">The total number of vertex rings to generate.
		/// The minimum value of 1 specifies just the outer ring, while values of 2 or more generate inner vertex rings also.</param>
		/// <param name="addRingVertices">The delegate used to set vertex attribute values.</param>
		/// <remarks>The <paramref name="addRingVertices"/> delegate is expected to call <see cref="DynamicMesh.IIndexedVertexAttributes.Advance()"/>
		/// once after each vertex that is added by the delegate, which should also be equal to the ring depth.</remarks>
		/// <seealso cref="DynamicMesh"/>
		/// <seealso cref="DynamicMesh.IIndexedVertexAttributes"/>
		public SeparatedFacesFanTriangulation(int ringDepth, Action<Topology.FaceEdge, DynamicMesh.IIndexedVertexAttributes> addRingVertices)
		{
			if (ringDepth < 1) throw new ArgumentOutOfRangeException("ringDepth");
			_ringDepth = ringDepth;
			_addRingVertices = addRingVertices;
		}

		/// <inheritdoc/>
		public int GetVertexCount(Topology.Face face)
		{
			return face.neighborCount * _ringDepth;
		}

		/// <inheritdoc/>
		public void BuildFace(Topology.Face face, DynamicMesh.IIndexedVertexAttributes vertexAttributes, IList<int> triangleIndices)
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

		/// <inheritdoc/>
		public void RebuildFace(Topology.Face face, DynamicMesh.IIndexedVertexAttributes vertexAttributes)
		{
			foreach (var edge in face.edges)
			{
				_addRingVertices(edge, vertexAttributes);
			}
		}

		/// <inheritdoc/>
		public void FinalizeSubmesh(int index)
		{
		}
	}

	/// <summary>
	/// Triangulates a topology face using a triangle umbrella pattern, not sharing vertices among other faces.
	/// </summary>
	/// <remarks>
	/// <para>This triangulation method uses the triangle umbrella pattern when constructing triangles
	/// from a face, using a center vertex as a triangle fan base and moving clockwise around the
	/// circumference of the face, creating triangles fanning out from the center vertex.  The first
	/// outer vertex is reused for the very last triangle.</para>
	/// 
	/// <para>In the simplest case, each face can be made using exactly as many vertices as the face
	/// has neighbors, plus one for the center vertex.  However, inner rings can also be generated,
	/// multiplying the number of vertices needed for each additional ring.</para>
	/// 
	/// <para>All vertices on the outer ring of the face are duplicated rather than shared with
	/// adjacent faces.</para>
	/// </remarks>
	/// <seealso cref="DynamicMesh"/>
	/// <seealso cref="DynamicMesh.ITriangulation"/>
	public class SeparatedFacesUmbrellaTriangulation : DynamicMesh.ITriangulation
	{
		private int _ringDepth;

		private Action<Topology.FaceEdge, DynamicMesh.IIndexedVertexAttributes> _addRingVertices;
		private Action<Topology.Face, DynamicMesh.IIndexedVertexAttributes> _addCenterVertex;

		/// <summary>
		/// Construct a fan triangulation instance with no inner rings using the supplied delegate to set vertex attribute values.
		/// </summary>
		/// <param name="addRingVertices">The delegate used to set vertex attribute values on all ring vertices.</param>
		/// <param name="addCenterVertex">The delegate used to set vertex attribute for the center vertex.</param>
		/// <remarks>The <paramref name="addRingVertices"/> delegate is expected to call <see cref="DynamicMesh.IIndexedVertexAttributes.Advance()"/>
		/// once after each vertex that is added by the delegate, which should also be equal to the ring depth.  Similarly, the
		/// <paramref name="addCenterVertex"/> delegate is expected to call <see cref="DynamicMesh.IIndexedVertexAttributes.Advance()"/> exactly once.</remarks>
		/// <seealso cref="DynamicMesh"/>
		/// <seealso cref="DynamicMesh.IIndexedVertexAttributes"/>
		public SeparatedFacesUmbrellaTriangulation(
			Action<Topology.FaceEdge, DynamicMesh.IIndexedVertexAttributes> addRingVertices,
			Action<Topology.Face, DynamicMesh.IIndexedVertexAttributes> addCenterVertex)
		{
			_ringDepth = 1;
			_addRingVertices = addRingVertices;
			_addCenterVertex = addCenterVertex;
		}

		/// <summary>
		/// Construct a fan triangulation instance with the specified number of vertex rings using the supplied delegate to set vertex attribute values.
		/// </summary>
		/// <param name="ringDepth">The total number of vertex rings to generate.
		/// The minimum value of 1 specifies just the outer ring, while values of 2 or more generate inner vertex rings also.</param>
		/// <param name="addRingVertices">The delegate used to set vertex attribute values on all ring vertices.</param>
		/// <param name="addCenterVertex">The delegate used to set vertex attribute for the center vertex.</param>
		/// <remarks>The <paramref name="addRingVertices"/> delegate is expected to call <see cref="DynamicMesh.IIndexedVertexAttributes.Advance()"/>
		/// once after each vertex that is added by the delegate, which should also be equal to the ring depth.  Similarly, the
		/// <paramref name="addCenterVertex"/> delegate is expected to call <see cref="DynamicMesh.IIndexedVertexAttributes.Advance()"/> exactly once.</remarks>
		/// <seealso cref="DynamicMesh"/>
		/// <seealso cref="DynamicMesh.IIndexedVertexAttributes"/>
		public SeparatedFacesUmbrellaTriangulation(
			int ringDepth,
			Action<Topology.FaceEdge, DynamicMesh.IIndexedVertexAttributes> addRingVertices,
			Action<Topology.Face, DynamicMesh.IIndexedVertexAttributes> addCenterVertex)
		{
			if (ringDepth < 1) throw new ArgumentOutOfRangeException("ringDepth");
			_ringDepth = ringDepth;
			_addRingVertices = addRingVertices;
			_addCenterVertex = addCenterVertex;
		}

		/// <inheritdoc/>
		public int GetVertexCount(Topology.Face face)
		{
			return face.neighborCount * _ringDepth + 1;
		}

		/// <inheritdoc/>
		public void BuildFace(Topology.Face face, DynamicMesh.IIndexedVertexAttributes vertexAttributes, IList<int> triangleIndices)
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

		/// <inheritdoc/>
		public void RebuildFace(Topology.Face face, DynamicMesh.IIndexedVertexAttributes vertexAttributes)
		{
			foreach (var edge in face.edges)
			{
				_addRingVertices(edge, vertexAttributes);
			}

			_addCenterVertex(face, vertexAttributes);
		}

		/// <inheritdoc/>
		public void FinalizeSubmesh(int index)
		{
		}
	}

	/// <summary>
	/// Triangulates a topology face using a kite pattern at the corners, not sharing vertices among other faces.
	/// </summary>
	/// <remarks>
	/// <para>This triangulation method creates two rings of vertices around each face, using a
	/// kite pattern at each corner, with three vertices per corner on the outside ring, and one
	/// vertex per corner on the inner ring.</para>
	/// 
	/// <para>All vertices on the outer ring of the face are duplicated rather than shared with
	/// adjacent faces.</para>
	/// </remarks>
	/// <seealso cref="DynamicMesh"/>
	/// <seealso cref="DynamicMesh.ITriangulation"/>
	public class SeparatedFacesKiteCornerStripTriangulation : DynamicMesh.ITriangulation
	{
		private Action<Topology.FaceEdge, DynamicMesh.IIndexedVertexAttributes> _addCornerVertices;

		/// <summary>
		/// Construct a kite triangulation instance using the supplied delegate to set vertex attribute values.
		/// </summary>
		/// <param name="addCornerVertices">The delegate used to set vertex attribute values on all corner kite vertices.</param>
		/// <remarks>The <paramref name="addCornerVertices"/> delegate is expected to call <see cref="DynamicMesh.IIndexedVertexAttributes.Advance()"/> four times each time it is invoked.</remarks>
		/// <seealso cref="DynamicMesh"/>
		/// <seealso cref="DynamicMesh.IIndexedVertexAttributes"/>
		public SeparatedFacesKiteCornerStripTriangulation(Action<Topology.FaceEdge, DynamicMesh.IIndexedVertexAttributes> addCornerVertices)
		{
			_addCornerVertices = addCornerVertices;
		}

		/// <inheritdoc/>
		public int GetVertexCount(Topology.Face face)
		{
			return face.neighborCount * 4;
		}

		/// <inheritdoc/>
		public void BuildFace(Topology.Face face, DynamicMesh.IIndexedVertexAttributes vertexAttributes, IList<int> triangleIndices)
		{
			int neighborCount = face.neighborCount;
			int currentVertexIndex = vertexAttributes.index;

			//        __(0)__
			//    __--   |   --__
			// (1)       |       (2)__
			//   \       |       / \  --__
			//    \   A  |  B   /   \     --__
			//     \     |     /     \        --__
			//      \    |    /       \           --__
			//       \   |   /         \              --(5)
			//        \  |  /           \               /
			//         \ | /       C     \     D       /
			//          (3)__             \           /
			//               --__          \         /
			//                   --__       \       /
			//                       --__    \     /
			//                           --__ \   /
			//                               --(7)

			for (int neighborIndex = 0; neighborIndex < neighborCount; ++neighborIndex)
			{
				// A (1->0->3)
				triangleIndices.Add(currentVertexIndex + 1);
				triangleIndices.Add(currentVertexIndex + 0);
				triangleIndices.Add(currentVertexIndex + 3);
				// B (3->0->2)
				triangleIndices.Add(currentVertexIndex + 3);
				triangleIndices.Add(currentVertexIndex + 0);
				triangleIndices.Add(currentVertexIndex + 2);
				// C (3->2->7)
				triangleIndices.Add(currentVertexIndex + 3);
				triangleIndices.Add(currentVertexIndex + 2);
				triangleIndices.Add(currentVertexIndex + 7);
				// D (7->2->1)
				triangleIndices.Add(currentVertexIndex + 7);
				triangleIndices.Add(currentVertexIndex + 2);
				triangleIndices.Add(currentVertexIndex + 5);

				currentVertexIndex += 4;
			}

			triangleIndices[triangleIndices.Count - 4] = vertexAttributes.index + 3;
			triangleIndices[triangleIndices.Count - 3] = vertexAttributes.index + 3;
			triangleIndices[triangleIndices.Count - 1] = vertexAttributes.index + 1;

			int forwardVertexIndex = vertexAttributes.index + 3;
			int backwardVertexIndex = vertexAttributes.index + neighborCount * 4 - 1;

			int triangleIndex = 2;
			while (triangleIndex < neighborCount)
			{
				triangleIndices.Add(backwardVertexIndex);
				triangleIndices.Add(forwardVertexIndex);
				backwardVertexIndex -= 4;
				triangleIndices.Add(backwardVertexIndex);

				if (++triangleIndex == neighborCount) break;

				triangleIndices.Add(backwardVertexIndex);
				triangleIndices.Add(forwardVertexIndex);
				forwardVertexIndex += 4;
				triangleIndices.Add(forwardVertexIndex);

				++triangleIndex;
			}

			RebuildFace(face, vertexAttributes);
		}

		/// <inheritdoc/>
		public void RebuildFace(Topology.Face face, DynamicMesh.IIndexedVertexAttributes vertexAttributes)
		{
			foreach (var edge in face.edges)
			{
				_addCornerVertices(edge, vertexAttributes);
			}
		}

		/// <inheritdoc/>
		public void FinalizeSubmesh(int index)
		{
		}
	}
}
