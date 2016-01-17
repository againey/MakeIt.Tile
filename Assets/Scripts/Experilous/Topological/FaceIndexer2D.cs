using UnityEngine;
using System;

namespace Experilous.Topological
{
	public interface IFaceIndexer2D
	{
		int internalFaceCount { get; }

		Topology.Face GetFace(Index2D coordinate);
		Topology.Face GetFace(int x, int y);
		int GetFaceIndex(Index2D coordinate);
		int GetFaceIndex(int x, int y);

		Index2D GetFaceIndex2D(Topology.Face internalFace);
		Index2D GetFaceIndex2D(int internalFaceIndex);
	}

	public abstract class FaceIndexer2D : ScriptableObject, IFaceIndexer2D, ICloneable
	{
		public Topology topology;

		public abstract object Clone();

		public abstract int internalFaceCount { get; }

		public abstract int GetFaceIndex(int x, int y);
		public abstract Index2D GetFaceIndex2D(int internalFaceIndex);

		public Topology.Face GetFace(Index2D coordinate) { return topology.faces[GetFaceIndex(coordinate.x, coordinate.y)]; }
		public Topology.Face GetFace(int x, int y) { return topology.faces[GetFaceIndex(x, y)]; }
		public int GetFaceIndex(Index2D coordinate) { return GetFaceIndex(coordinate.x, coordinate.y); }

		public Index2D GetFaceIndex2D(Topology.Face internalFace) { return GetFaceIndex2D(internalFace.index); }
	}
}
