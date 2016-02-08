using UnityEngine;
using System;

namespace Experilous.Topological
{
	public interface IFaceIndexer2D
	{
		int internalFaceCount { get; }

		Topology.Face GetFace(Index2D index);
		Topology.Face GetFace(int x, int y);
		int GetFaceIndex(Index2D index);
		int GetFaceIndex(int x, int y);

		Index2D GetFaceIndex2D(Topology.Face internalFace);
		Index2D GetFaceIndex2D(int internalFaceIndex);

		int FaceWrapX(int x);
		int FaceWrapY(int y);
		Index2D FaceWrap(int x, int y);
		Index2D FaceWrap(Index2D index);

		Topology.Face GetWrappedFace(Index2D index);
		Topology.Face GetWrappedFace(int x, int y);
		int GetWrappedFaceIndex(Index2D index);
		int GetWrappedFaceIndex(int x, int y);
	}

	public abstract class FaceIndexer2D : ScriptableObject, IFaceIndexer2D
	{
		public Topology topology;

		public abstract int internalFaceCount { get; }

		public abstract int GetFaceIndex(int x, int y);
		public abstract Index2D GetFaceIndex2D(int internalFaceIndex);

		public abstract int FaceWrapX(int x);
		public abstract int FaceWrapY(int y);

		public Topology.Face GetFace(Index2D index) { return topology.faces[GetFaceIndex(index.x, index.y)]; }
		public Topology.Face GetFace(int x, int y) { return topology.faces[GetFaceIndex(x, y)]; }
		public int GetFaceIndex(Index2D index) { return GetFaceIndex(index.x, index.y); }

		public Index2D GetFaceIndex2D(Topology.Face internalFace) { return GetFaceIndex2D(internalFace.index); }

		public Index2D FaceWrap(int x, int y) { return new Index2D(FaceWrapX(x), FaceWrapY(y)); }
		public Index2D FaceWrap(Index2D index) { return new Index2D(FaceWrapX(index.x), FaceWrapY(index.y)); }

		public Topology.Face GetWrappedFace(Index2D index) { return GetFace(FaceWrap(index)); }
		public Topology.Face GetWrappedFace(int x, int y) { return GetFace(FaceWrap(x, y)); }
		public int GetWrappedFaceIndex(Index2D index) { return GetFaceIndex(FaceWrap(index)); }
		public int GetWrappedFaceIndex(int x, int y) { return GetFaceIndex(FaceWrap(x, y)); }
	}
}
