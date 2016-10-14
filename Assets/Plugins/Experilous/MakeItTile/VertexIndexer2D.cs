/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using Experilous.Numerics;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// Interface for accessing the vertices of a topology using two-dimensional indices.
	/// </summary>
	public interface IVertexIndexer2D
	{
		/// <summary>
		/// The number of vertices in the topology.
		/// </summary>
		int vertexCount { get; }

		/// <summary>
		/// Gets the vertex corresponding to the given two-dimensional index.
		/// </summary>
		/// <param name="index">The two-dimensional index used for lookup.</param>
		/// <returns>The vertex corresponding to the given two-dimensional index.</returns>
		Topology.Vertex GetVertex(IntVector2 index);

		/// <summary>
		/// Gets the vertex corresponding to the given two-dimensional index.
		/// </summary>
		/// <param name="x">The first component of the two-dimensional index.</param>
		/// <param name="y">The second component of the two-dimensional index.</param>
		/// <returns>The vertex corresponding to the given two-dimensional index.</returns>
		Topology.Vertex GetVertex(int x, int y);

		/// <summary>
		/// Gets the standard one-dimensional vertex index corresponding to the given two-dimensional index.
		/// </summary>
		/// <param name="index">The two-dimensional index used for lookup.</param>
		/// <returns>The standard one-dimensional vertex index corresponding to the given two-dimensional index.</returns>
		/// <seealso cref="Topology.Vertex.index"/>
		int GetVertexIndex(IntVector2 index);

		/// <summary>
		/// Gets the standard one-dimensional vertex index corresponding to the given two-dimensional index.
		/// </summary>
		/// <param name="x">The first component of the two-dimensional index.</param>
		/// <param name="y">The second component of the two-dimensional index.</param>
		/// <returns>The standard one-dimensional vertex index corresponding to the given two-dimensional index.</returns>
		/// <seealso cref="Topology.Vertex.index"/>
		int GetVertexIndex(int x, int y);

		/// <summary>
		/// Gets the two-dimensional index of the given vertex.
		/// </summary>
		/// <param name="vertex">The vertex whose two-dimensional index is to be returned.</param>
		/// <returns>The two-dimensional index of the given vertex.</returns>
		IntVector2 GetVertexIndex2D(Topology.Vertex vertex);

		/// <summary>
		/// Gets the two-dimensional index of the vertex with the given standard one-dimesional index.
		/// </summary>
		/// <param name="vertex">The vertex whose two-dimensional index is to be returned.</param>
		/// <returns>The two-dimensional index of the vertex with the given standard one-dimesional index.</returns>
		IntVector2 GetVertexIndex2D(int vertexIndex);

		/// <summary>
		/// Wraps the first component of a two-dimensional vertex index if the topology wraps appropriately.
		/// </summary>
		/// <param name="x">The first component of a two-dimensional vertex index.  Can be outside the normal bounds for this component.</param>
		/// <returns>The first component of a two-dimensional vertex index, wrapped to be within the normal bounds for this component if the topology wraps along this dimension, or the unmodified input value if not.</returns>
		int VertexWrapX(int x);

		/// <summary>
		/// Wraps the second component of a two-dimensional vertex index if the topology wraps appropriately.
		/// </summary>
		/// <param name="y">The second component of a two-dimensional vertex index.  Can be outside the normal bounds for this component.</param>
		/// <returns>The second component of a two-dimensional vertex index, wrapped to be within the normal bounds for this component if the topology wraps along this dimension, or the unmodified input value if not.</returns>
		int VertexWrapY(int y);

		/// <summary>
		/// Wraps a two-dimensional vertex index if the topology wraps appropriately.
		/// </summary>
		/// <param name="x">The first component of a two-dimensional vertex index.  Can be outside the normal bounds for this component.</param>
		/// <param name="y">The second component of a two-dimensional vertex index.  Can be outside the normal bounds for this component.</param>
		/// <returns>The two-dimensional vertex index, wrapped to be within the normal bounds for each component if the topology wraps along the corresponding dimension, or the unmodified input value if not.</returns>
		IntVector2 VertexWrap(int x, int y);

		/// <summary>
		/// Wraps a two-dimensional vertex index if the topology wraps appropriately.
		/// </summary>
		/// <param name="index">The two-dimensional index to be wrapped.</param>
		/// <returns>The two-dimensional vertex index, wrapped to be within the normal bounds for each component if the topology wraps along the corresponding dimension, or the unmodified input value if not.</returns>
		IntVector2 VertexWrap(IntVector2 index);

		/// <summary>
		/// Gets the vertex corresponding to the given two-dimensional index, first wrapping the two-dimensional index if necessary.
		/// </summary>
		/// <param name="index">The two-dimensional index used for lookup.</param>
		/// <returns>The vertex corresponding to the given two-dimensional index after being wrapped.</returns>
		Topology.Vertex GetWrappedVertex(IntVector2 index);

		/// <summary>
		/// Gets the vertex corresponding to the given two-dimensional index, first wrapping the two-dimensional index if necessary.
		/// </summary>
		/// <param name="x">The first component of the two-dimensional index.</param>
		/// <param name="y">The second component of the two-dimensional index.</param>
		/// <returns>The vertex corresponding to the given two-dimensional index after being wrapped.</returns>
		Topology.Vertex GetWrappedVertex(int x, int y);

		/// <summary>
		/// Gets the standard one-dimensional vertex index corresponding to the given two-dimensional index, first wrapping the two-dimensional index if necessary.
		/// </summary>
		/// <param name="index">The two-dimensional index used for lookup.</param>
		/// <returns>The standard one-dimensional vertex index corresponding to the given two-dimensional index after being wrapped.</returns>
		/// <seealso cref="Topology.Vertex.index"/>
		int GetWrappedVertexIndex(IntVector2 index);

		/// <summary>
		/// Gets the standard one-dimensional vertex index corresponding to the given two-dimensional index, first wrapping the two-dimensional index if necessary.
		/// </summary>
		/// <param name="x">The first component of the two-dimensional index.</param>
		/// <param name="y">The second component of the two-dimensional index.</param>
		/// <returns>The standard one-dimensional vertex index corresponding to the given two-dimensional index after being wrapped.</returns>
		/// <seealso cref="Topology.Vertex.index"/>
		int GetWrappedVertexIndex(int x, int y);
	}

	/// <summary>
	/// Abstract base class for accessing the vertices of a topology using two-dimensional indices, implementing some of the methods in terms of other methods.
	/// </summary>
	public abstract class VertexIndexer2D : ScriptableObject, IVertexIndexer2D
	{
		/// <summary>
		/// The topology containing the vertices with which this object works.
		/// </summary>
		public Topology topology;

		/// <inheritdoc/>
		public abstract int vertexCount { get; }

		/// <inheritdoc/>
		public abstract int GetVertexIndex(int x, int y);
		/// <inheritdoc/>
		public abstract IntVector2 GetVertexIndex2D(int vertexIndex);

		/// <inheritdoc/>
		public abstract int VertexWrapX(int x);
		/// <inheritdoc/>
		public abstract int VertexWrapY(int y);

		/// <inheritdoc/>
		public Topology.Vertex GetVertex(IntVector2 index) { return topology.vertices[GetVertexIndex(index.x, index.y)]; }
		/// <inheritdoc/>
		public Topology.Vertex GetVertex(int x, int y) { return topology.vertices[GetVertexIndex(x, y)]; }
		/// <inheritdoc/>
		public int GetVertexIndex(IntVector2 index) { return GetVertexIndex(index.x, index.y); }

		/// <inheritdoc/>
		public IntVector2 GetVertexIndex2D(Topology.Vertex vertex) { return GetVertexIndex2D(vertex.index); }

		/// <inheritdoc/>
		public IntVector2 VertexWrap(int x, int y) { return new IntVector2(VertexWrapX(x), VertexWrapY(y)); }
		/// <inheritdoc/>
		public IntVector2 VertexWrap(IntVector2 index) { return new IntVector2(VertexWrapX(index.x), VertexWrapY(index.y)); }

		/// <inheritdoc/>
		public Topology.Vertex GetWrappedVertex(IntVector2 index) { return GetVertex(VertexWrap(index)); }
		/// <inheritdoc/>
		public Topology.Vertex GetWrappedVertex(int x, int y) { return GetVertex(VertexWrap(x, y)); }
		/// <inheritdoc/>
		public int GetWrappedVertexIndex(IntVector2 index) { return GetVertexIndex(VertexWrap(index)); }
		/// <inheritdoc/>
		public int GetWrappedVertexIndex(int x, int y) { return GetVertexIndex(VertexWrap(x, y)); }
	}
}
