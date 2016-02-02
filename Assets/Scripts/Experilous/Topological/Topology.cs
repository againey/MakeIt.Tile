using UnityEngine;
using System;

namespace Experilous.Topological
{
	public partial class Topology : ScriptableObject, ICloneable
	{
		public ushort[] vertexNeighborCounts;
		public int[] vertexFirstEdgeIndices;

		public EdgeData[] edgeData;

		public ushort[] faceNeighborCounts;
		public int[] faceFirstEdgeIndices;

		public int firstExternalEdgeIndex;
		public int firstExternalFaceIndex;

		public static Topology Create(ushort[] vertexNeighborCounts, int[] vertexFirstEdgeIndices, EdgeData[] edgeData, ushort[] faceNeighborCounts, int[] faceFirstEdgeIndices)
		{
			return Create(vertexNeighborCounts, vertexFirstEdgeIndices, edgeData, faceNeighborCounts, faceFirstEdgeIndices, edgeData.Length, faceNeighborCounts.Length);
		}

		public static Topology Create(ushort[] vertexNeighborCounts, int[] vertexFirstEdgeIndices, EdgeData[] edgeData, ushort[] faceNeighborCounts, int[] faceFirstEdgeIndices, string name)
		{
			return Create(vertexNeighborCounts, vertexFirstEdgeIndices, edgeData, faceNeighborCounts, faceFirstEdgeIndices, edgeData.Length, faceNeighborCounts.Length, name);
		}

		public static Topology Create(ushort[] vertexNeighborCounts, int[] vertexFirstEdgeIndices, EdgeData[] edgeData, ushort[] faceNeighborCounts, int[] faceFirstEdgeIndices, int firstExternalEdgeIndex, int firstExternalFaceIndex, string name)
		{
			var instance = Create(vertexNeighborCounts, vertexFirstEdgeIndices, edgeData, faceNeighborCounts, faceFirstEdgeIndices, firstExternalEdgeIndex, firstExternalFaceIndex);
			instance.name = name;
			return instance;
		}

		public static Topology Create(ushort[] vertexNeighborCounts, int[] vertexFirstEdgeIndices, EdgeData[] edgeData, ushort[] faceNeighborCounts, int[] faceFirstEdgeIndices, int firstExternalEdgeIndex, int firstExternalFaceIndex)
		{
			var instance = CreateInstance<Topology>();
			instance.vertexNeighborCounts = vertexNeighborCounts;
			instance.vertexFirstEdgeIndices = vertexFirstEdgeIndices;
			instance.edgeData = edgeData;
			instance.faceNeighborCounts = faceNeighborCounts;
			instance.faceFirstEdgeIndices = faceFirstEdgeIndices;
			instance.firstExternalEdgeIndex = firstExternalEdgeIndex;
			instance.firstExternalFaceIndex = firstExternalFaceIndex;
			return instance;
		}

		public virtual object Clone()
		{
			var clone = Create(
				vertexNeighborCounts.Clone() as ushort[],
				vertexFirstEdgeIndices.Clone() as int[],
				edgeData.Clone() as EdgeData[],
				faceNeighborCounts.Clone() as ushort[],
				faceFirstEdgeIndices.Clone() as int[],
				firstExternalEdgeIndex,
				firstExternalFaceIndex);
			clone.name = name;
			clone.hideFlags = hideFlags;
			return clone;
		}

		public virtual void MakeDual()
		{
			if (firstExternalFaceIndex < faceFirstEdgeIndices.Length) throw new InvalidOperationException("A dual topology cannot be derived from a topology with external faces.");

			Utility.Swap(ref vertexNeighborCounts, ref faceNeighborCounts);
			Utility.Swap(ref vertexFirstEdgeIndices, ref faceFirstEdgeIndices);

			firstExternalFaceIndex = faceFirstEdgeIndices.Length;

			var dualEdgeData = new EdgeData[edgeData.Length];
			for (int i = 0; i < edgeData.Length; ++i)
			{
				// Edges rotate clockwise to point at next faces becoming far vertices, with their
				// side toward far vertices becoming prev faces.
				dualEdgeData[i] = new EdgeData(
					edgeData[i]._twin, // twin remains the same
					edgeData[edgeData[edgeData[i]._twin]._fNext]._twin, // vNext becomes twin of vPrev, where vPrev is the fNext of twin
					edgeData[i]._vNext, // fNext becomes what vNext had been
					edgeData[edgeData[i]._twin]._face, // far vertex becomes what had been next face
					edgeData[i]._vertex); // prev face becomes what had been far vertex
			}
			edgeData = dualEdgeData;

			// Due to rotations, face data (which had been vertex data) still points to the same edges,
			// but vertex data (which had been face data) is now backwards, pointing to edges which
			// point back at the vertex; this needs to be reversed by setting first edges to their twins.
			for (int i = 0; i < vertexFirstEdgeIndices.Length; ++i)
			{
				vertexFirstEdgeIndices[i] = edgeData[vertexFirstEdgeIndices[i]]._twin;
			}
		}

		public virtual Topology GetDualTopology()
		{
			var clone = (Topology)Clone();
			clone.MakeDual();
			return clone;
		}
	}
}
