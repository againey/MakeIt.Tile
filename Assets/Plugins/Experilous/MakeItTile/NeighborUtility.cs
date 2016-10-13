﻿using System.Collections.Generic;

namespace Experilous.MakeItTile
{
	public static class NeighborUtility
	{
		public delegate IEnumerator<Topology.Vertex> GetVertexNeighborVertexEnumeratorDelegate(Topology.Vertex vertex);
		public delegate IEnumerator<Topology.Face> GetVertexNeighborFaceEnumeratorDelegate(Topology.Vertex vertex);
		public delegate IEnumerator<Topology.Vertex> GetFaceNeighborVertexEnumeratorDelegate(Topology.Face face);
		public delegate IEnumerator<Topology.Face> GetFaceNeighborFaceEnumeratorDelegate(Topology.Face face);

		public static IEnumerator<Topology.Vertex> AdjacentVertices(Topology.Vertex vertex)
		{
			foreach (var edge in vertex.edges)
			{
				yield return edge.vertex;
			}
		}

		public static IEnumerator<Topology.Face> AdjacentFaces(Topology.Vertex vertex)
		{
			foreach (var edge in vertex.edges)
			{
				yield return edge.face;
			}
		}

		public static IEnumerator<Topology.Vertex> FaceAdjacentVertices(Topology.Vertex vertex)
		{
			foreach (var edge in vertex.outerVertexEdges)
			{
				yield return edge.vertex;
			}
		}

		public static IEnumerator<Topology.Face> AdjacentFaces(Topology.Face face)
		{
			foreach (var edge in face.edges)
			{
				yield return edge.face;
			}
		}

		public static IEnumerator<Topology.Vertex> AdjacentVertices(Topology.Face face)
		{
			foreach (var edge in face.edges)
			{
				yield return edge.vertex;
			}
		}

		public static IEnumerator<Topology.Face> VertexAdjacentFaces(Topology.Face face)
		{
			foreach (var edge in face.outerFaceEdges)
			{
				yield return edge.face;
			}
		}
	}
}
