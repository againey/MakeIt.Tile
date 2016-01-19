using UnityEngine;
using System;

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
	}

	public abstract class VertexIndexer2D : ScriptableObject, IVertexIndexer2D, ICloneable
	{
		public Topology topology;

		public abstract object Clone();

		public abstract int vertexCount { get; }

		public abstract int GetVertexIndex(int x, int y);
		public abstract Index2D GetVertexIndex2D(int vertexIndex);

		public Topology.Vertex GetVertex(Index2D index) { return topology.vertices[GetVertexIndex(index.x, index.y)]; }
		public Topology.Vertex GetVertex(int x, int y) { return topology.vertices[GetVertexIndex(x, y)]; }
		public int GetVertexIndex(Index2D index) { return GetVertexIndex(index.x, index.y); }

		public Index2D GetVertexIndex2D(Topology.Vertex vertex) { return GetVertexIndex2D(vertex.index); }
	}
}
