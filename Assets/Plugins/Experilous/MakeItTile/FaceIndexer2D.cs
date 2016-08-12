/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using Experilous.MakeIt.Utilities;

namespace Experilous.MakeIt.Tile
{
	public interface IFaceIndexer2D
	{
		int internalFaceCount { get; }

		Topology.Face GetFace(IntVector2 index);
		Topology.Face GetFace(int x, int y);
		int GetFaceIndex(IntVector2 index);
		int GetFaceIndex(int x, int y);

		IntVector2 GetFaceIndex2D(Topology.Face internalFace);
		IntVector2 GetFaceIndex2D(int internalFaceIndex);

		int FaceWrapX(int x);
		int FaceWrapY(int y);
		IntVector2 FaceWrap(int x, int y);
		IntVector2 FaceWrap(IntVector2 index);

		Topology.Face GetWrappedFace(IntVector2 index);
		Topology.Face GetWrappedFace(int x, int y);
		int GetWrappedFaceIndex(IntVector2 index);
		int GetWrappedFaceIndex(int x, int y);
	}

	public abstract class FaceIndexer2D : ScriptableObject, IFaceIndexer2D
	{
		public Topology topology;

		public abstract int internalFaceCount { get; }

		public abstract int GetFaceIndex(int x, int y);
		public abstract IntVector2 GetFaceIndex2D(int internalFaceIndex);

		public abstract int FaceWrapX(int x);
		public abstract int FaceWrapY(int y);

		public Topology.Face GetFace(IntVector2 index) { return topology.faces[GetFaceIndex(index.x, index.y)]; }
		public Topology.Face GetFace(int x, int y) { return topology.faces[GetFaceIndex(x, y)]; }
		public int GetFaceIndex(IntVector2 index) { return GetFaceIndex(index.x, index.y); }

		public IntVector2 GetFaceIndex2D(Topology.Face internalFace) { return GetFaceIndex2D(internalFace.index); }

		public IntVector2 FaceWrap(int x, int y) { return new IntVector2(FaceWrapX(x), FaceWrapY(y)); }
		public IntVector2 FaceWrap(IntVector2 index) { return new IntVector2(FaceWrapX(index.x), FaceWrapY(index.y)); }

		public Topology.Face GetWrappedFace(IntVector2 index) { return GetFace(FaceWrap(index)); }
		public Topology.Face GetWrappedFace(int x, int y) { return GetFace(FaceWrap(x, y)); }
		public int GetWrappedFaceIndex(IntVector2 index) { return GetFaceIndex(FaceWrap(index)); }
		public int GetWrappedFaceIndex(int x, int y) { return GetFaceIndex(FaceWrap(x, y)); }
	}
}
