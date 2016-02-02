using UnityEngine;

namespace Experilous.Topological
{
	public class RowMajorQuadGridFaceIndexer2D : FaceIndexer2D
	{
		[SerializeField] private int _faceColumnCount;
		[SerializeField] private int _faceRowCount;

		[SerializeField] private Index2D _lowestIndex;

		[SerializeField] private int _originFaceIndex;
		[SerializeField] private int _originFaceOffset;

		public int faceColumnCount { get { return _faceColumnCount; } }
		public int faceRowCount { get { return _faceRowCount; } }

		public Index2D lowestIndex { get { return _lowestIndex; } }

		public override int internalFaceCount { get { return _faceColumnCount * _faceRowCount; } }

		public static RowMajorQuadGridFaceIndexer2D CreateInstance(int faceColumnCount, int faceRowCount)
		{
			return CreateInstance(faceColumnCount, faceRowCount, new Index2D(0, 0));
		}

		public static RowMajorQuadGridFaceIndexer2D CreateInstance(int faceColumnCount, int faceRowCount, string name)
		{
			return SetName(CreateInstance(faceColumnCount, faceRowCount), name);
		}

		public static RowMajorQuadGridFaceIndexer2D CreateInstance(int faceColumnCount, int faceRowCount, Index2D lowestIndex)
		{
			if (faceColumnCount < 1) throw new System.ArgumentOutOfRangeException("faceColumnCount");
			if (faceRowCount < 1) throw new System.ArgumentOutOfRangeException("faceRowCount");
			var instance = CreateInstance<RowMajorQuadGridFaceIndexer2D>();
			instance._faceColumnCount = faceColumnCount;
			instance._faceRowCount = faceRowCount;
			instance._lowestIndex = lowestIndex;
			instance.Initialize();
			return instance;
		}

		public static RowMajorQuadGridFaceIndexer2D CreateInstance(int faceColumnCount, int faceRowCount, Index2D lowestIndex, string name)
		{
			return SetName(CreateInstance(faceColumnCount, faceRowCount, lowestIndex), name);
		}

		private static RowMajorQuadGridFaceIndexer2D SetName(RowMajorQuadGridFaceIndexer2D instance, string name)
		{
			instance.name = name;
			return instance;
		}

		private void Initialize()
		{
			_originFaceIndex = -_lowestIndex.x - _lowestIndex.y * _faceColumnCount;
			_originFaceOffset = _originFaceIndex % _faceColumnCount;
		}

		public override object Clone()
		{
			var instance = CreateInstance(_faceColumnCount, _faceRowCount, _lowestIndex, name);
			instance.hideFlags = hideFlags;
			return instance;
		}

		public override Index2D GetFaceIndex2D(int internalFaceIndex)
		{
			var adjustedIndex = internalFaceIndex + _originFaceOffset - _originFaceIndex;
			return new Index2D(
				adjustedIndex % _faceColumnCount - _originFaceOffset,
				adjustedIndex / _faceColumnCount);
		}

		public override int GetFaceIndex(int x, int y)
		{
			return x + y * _faceColumnCount + _originFaceIndex;
		}
	}
}
