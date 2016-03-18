/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;

namespace Experilous.Topological
{
	public interface IVertexIndexer2D
	{
		int vertexCount { get; }

		Topology.Vertex GetVertex(Index2D index);
		Topology.Vertex GetVertex(int x, int y);
		int GetVertexIndex(Index2D index);
		int GetVertexIndex(int x, int y);

		Index2D GetVertexIndex2D(Topology.Vertex vertex);
		Index2D GetVertexIndex2D(int vertexIndex);

		int VertexWrapX(int x);
		int VertexWrapY(int y);
		Index2D VertexWrap(int x, int y);
		Index2D VertexWrap(Index2D index);

		Topology.Vertex GetWrappedVertex(Index2D index);
		Topology.Vertex GetWrappedVertex(int x, int y);
		int GetWrappedVertexIndex(Index2D index);
		int GetWrappedVertexIndex(int x, int y);
	}

	public abstract class VertexIndexer2D : ScriptableObject, IVertexIndexer2D
	{
		public Topology topology;

		public abstract int vertexCount { get; }

		public abstract int GetVertexIndex(int x, int y);
		public abstract Index2D GetVertexIndex2D(int vertexIndex);

		public abstract int VertexWrapX(int x);
		public abstract int VertexWrapY(int y);

		public Topology.Vertex GetVertex(Index2D index) { return topology.vertices[GetVertexIndex(index.x, index.y)]; }
		public Topology.Vertex GetVertex(int x, int y) { return topology.vertices[GetVertexIndex(x, y)]; }
		public int GetVertexIndex(Index2D index) { return GetVertexIndex(index.x, index.y); }

		public Index2D GetVertexIndex2D(Topology.Vertex vertex) { return GetVertexIndex2D(vertex.index); }

		public Index2D VertexWrap(int x, int y) { return new Index2D(VertexWrapX(x), VertexWrapY(y)); }
		public Index2D VertexWrap(Index2D index) { return new Index2D(VertexWrapX(index.x), VertexWrapY(index.y)); }

		public Topology.Vertex GetWrappedVertex(Index2D index) { return GetVertex(VertexWrap(index)); }
		public Topology.Vertex GetWrappedVertex(int x, int y) { return GetVertex(VertexWrap(x, y)); }
		public int GetWrappedVertexIndex(Index2D index) { return GetVertexIndex(VertexWrap(index)); }
		public int GetWrappedVertexIndex(int x, int y) { return GetVertexIndex(VertexWrap(x, y)); }
	}
}
