using UnityEngine;
using System;

namespace Experilous.Topological
{
	public interface IFaceNeighborIndexer
	{
		int vertexCount { get; }
		int edgeCount { get; }
		int internalEdgeCount { get; }
		int externalEdgeCount { get; }
		int faceCount { get; }
		int internalFaceCount { get; }
		int externalFaceCount { get; }

		ushort GetNeighborCount(Topology.Face face);
		ushort GetNeighborCount(int faceIndex);

		Topology.Vertex GetNeighborVertex(Topology.Face face, int neighborIndex);
		Topology.Vertex GetNeighborVertex(int faceIndex, int neighborIndex);
		int GetNeighborVertexIndex(Topology.Face face, int neighborIndex);
		int GetNeighborVertexIndex(int faceIndex, int neighborIndex);

		Topology.FaceEdge GetNeighborEdge(Topology.Face face, int neighborIndex);
		Topology.FaceEdge GetNeighborEdge(int faceIndex, int neighborIndex);
		int GetNeighborEdgeIndex(Topology.Face face, int neighborIndex);
		int GetNeighborEdgeIndex(int faceIndex, int neighborIndex);

		Topology.Face GetNeighborFace(Topology.Face face, int neighborIndex);
		Topology.Face GetNeighborFace(int faceIndex, int neighborIndex);
		int GetNeighborFaceIndex(Topology.Face face, int neighborIndex);
		int GetNeighborFaceIndex(int faceIndex, int neighborIndex);

		int GetInverseNeighborIndex(Topology.Face face, int neighborIndex);
		int GetInverseNeighborIndex(int faceIndex, int neighborIndex);
	}

	public abstract class FaceNeighborIndexer : ScriptableObject, IFaceNeighborIndexer, ICloneable
	{
		public Topology topology;

		public abstract object Clone();

		public abstract int vertexCount { get; }
		public abstract int edgeCount { get; }
		public abstract int internalEdgeCount { get; }
		public abstract int externalEdgeCount { get; }
		public abstract int faceCount { get; }
		public abstract int internalFaceCount { get; }
		public abstract int externalFaceCount { get; }

		public abstract ushort GetNeighborCount(int faceIndex);
		public abstract int GetNeighborVertexIndex(int faceIndex, int neighborIndex);
		public abstract int GetNeighborEdgeIndex(int faceIndex, int neighborIndex);
		public abstract int GetNeighborFaceIndex(int faceIndex, int neighborIndex);
		public abstract int GetInverseNeighborIndex(int faceIndex, int neighborIndex);

		public ushort GetNeighborCount(Topology.Face face) { return GetNeighborCount(face.index); }

		public Topology.Vertex GetNeighborVertex(Topology.Face face, int neighborIndex) { return topology.vertices[GetNeighborVertexIndex(face.index, neighborIndex)]; }
		public Topology.Vertex GetNeighborVertex(int faceIndex, int neighborIndex) { return topology.vertices[GetNeighborVertexIndex(faceIndex, neighborIndex)]; }
		public int GetNeighborVertexIndex(Topology.Face face, int neighborIndex) { return GetNeighborVertexIndex(face.index, neighborIndex); }

		public Topology.FaceEdge GetNeighborEdge(Topology.Face face, int neighborIndex) { return topology.faceEdges[GetNeighborEdgeIndex(face.index, neighborIndex)]; }
		public Topology.FaceEdge GetNeighborEdge(int faceIndex, int neighborIndex) { return topology.faceEdges[GetNeighborEdgeIndex(faceIndex, neighborIndex)]; }
		public int GetNeighborEdgeIndex(Topology.Face face, int neighborIndex) { return GetNeighborEdgeIndex(face.index, neighborIndex); }

		public Topology.Face GetNeighborFace(Topology.Face face, int neighborIndex) { return topology.faces[GetNeighborFaceIndex(face.index, neighborIndex)]; }
		public Topology.Face GetNeighborFace(int faceIndex, int neighborIndex) { return topology.faces[GetNeighborFaceIndex(faceIndex, neighborIndex)]; }
		public int GetNeighborFaceIndex(Topology.Face face, int neighborIndex) { return GetNeighborFaceIndex(face.index, neighborIndex); }

		public int GetInverseNeighborIndex(Topology.Face face, int neighborIndex) { return GetInverseNeighborIndex(face.index, neighborIndex); }
	}
}
