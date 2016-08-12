/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using Experilous.Numerics;

namespace Experilous.MakeIt.Tile
{
	public interface IVertexIndexer2D
	{
		int vertexCount { get; }

		Topology.Vertex GetVertex(IntVector2 index);
		Topology.Vertex GetVertex(int x, int y);
		int GetVertexIndex(IntVector2 index);
		int GetVertexIndex(int x, int y);

		IntVector2 GetVertexIndex2D(Topology.Vertex vertex);
		IntVector2 GetVertexIndex2D(int vertexIndex);

		int VertexWrapX(int x);
		int VertexWrapY(int y);
		IntVector2 VertexWrap(int x, int y);
		IntVector2 VertexWrap(IntVector2 index);

		Topology.Vertex GetWrappedVertex(IntVector2 index);
		Topology.Vertex GetWrappedVertex(int x, int y);
		int GetWrappedVertexIndex(IntVector2 index);
		int GetWrappedVertexIndex(int x, int y);
	}

	public abstract class VertexIndexer2D : ScriptableObject, IVertexIndexer2D
	{
		public Topology topology;

		public abstract int vertexCount { get; }

		public abstract int GetVertexIndex(int x, int y);
		public abstract IntVector2 GetVertexIndex2D(int vertexIndex);

		public abstract int VertexWrapX(int x);
		public abstract int VertexWrapY(int y);

		public Topology.Vertex GetVertex(IntVector2 index) { return topology.vertices[GetVertexIndex(index.x, index.y)]; }
		public Topology.Vertex GetVertex(int x, int y) { return topology.vertices[GetVertexIndex(x, y)]; }
		public int GetVertexIndex(IntVector2 index) { return GetVertexIndex(index.x, index.y); }

		public IntVector2 GetVertexIndex2D(Topology.Vertex vertex) { return GetVertexIndex2D(vertex.index); }

		public IntVector2 VertexWrap(int x, int y) { return new IntVector2(VertexWrapX(x), VertexWrapY(y)); }
		public IntVector2 VertexWrap(IntVector2 index) { return new IntVector2(VertexWrapX(index.x), VertexWrapY(index.y)); }

		public Topology.Vertex GetWrappedVertex(IntVector2 index) { return GetVertex(VertexWrap(index)); }
		public Topology.Vertex GetWrappedVertex(int x, int y) { return GetVertex(VertexWrap(x, y)); }
		public int GetWrappedVertexIndex(IntVector2 index) { return GetVertexIndex(VertexWrap(index)); }
		public int GetWrappedVertexIndex(int x, int y) { return GetVertexIndex(VertexWrap(x, y)); }
	}
}
