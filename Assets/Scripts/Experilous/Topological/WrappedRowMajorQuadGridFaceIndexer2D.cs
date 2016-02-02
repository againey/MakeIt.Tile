using UnityEngine;

namespace Experilous.Topological
{
	public class WrappedRowMajorQuadGridFaceIndexer2D : FaceIndexer2D
	{
		[SerializeField] private int _faceColumnCount;
		[SerializeField] private int _faceRowCount;
		[SerializeField] private bool _isWrappedHorizontally;
		[SerializeField] private bool _isWrappedVertically;

		[SerializeField] private Index2D _lowestIndex;

		[SerializeField] private int _originFaceIndex;
		[SerializeField] private int _originFaceOffset;

		public int faceColumnCount { get { return _faceColumnCount; } }
		public int faceRowCount { get { return _faceRowCount; } }

		public Index2D lowestIndex { get { return _lowestIndex; } }

		public override int internalFaceCount { get { return _faceColumnCount * _faceRowCount; } }

		public static WrappedRowMajorQuadGridFaceIndexer2D CreateInstance(int faceColumnCount, int faceRowCount, bool isWrappedHorizontally, bool isWrappedVertically)
		{
			return CreateInstance(faceColumnCount, faceRowCount, isWrappedHorizontally, isWrappedVertically, new Index2D(0, 0));
		}

		public static WrappedRowMajorQuadGridFaceIndexer2D CreateInstance(int faceColumnCount, int faceRowCount, bool isWrappedHorizontally, bool isWrappedVertically, string name)
		{
			return SetName(CreateInstance(faceColumnCount, faceRowCount, isWrappedHorizontally, isWrappedVertically), name);
		}

		public static WrappedRowMajorQuadGridFaceIndexer2D CreateInstance(int faceColumnCount, int faceRowCount, bool isWrappedHorizontally, bool isWrappedVertically, Index2D lowestIndex)
		{
			if (faceColumnCount < 1) throw new System.ArgumentOutOfRangeException("faceColumnCount");
			if (faceRowCount < 1) throw new System.ArgumentOutOfRangeException("faceRowCount");
			if (!isWrappedHorizontally && !isWrappedVertically) throw new System.ArgumentException("isWrappedHorizontally");
			var instance = CreateInstance<WrappedRowMajorQuadGridFaceIndexer2D>();
			instance._faceColumnCount = faceColumnCount;
			instance._faceRowCount = faceRowCount;
			instance._isWrappedHorizontally = isWrappedHorizontally;
			instance._isWrappedVertically = isWrappedVertically;
			instance._lowestIndex = lowestIndex;
			instance.Initialize();
			return instance;
		}

		public static WrappedRowMajorQuadGridFaceIndexer2D CreateInstance(int faceColumnCount, int faceRowCount, bool isWrappedHorizontally, bool isWrappedVertically, Index2D lowestIndex, string name)
		{
			return SetName(CreateInstance(faceColumnCount, faceRowCount, isWrappedHorizontally, isWrappedVertically, lowestIndex), name);
		}

		private static WrappedRowMajorQuadGridFaceIndexer2D SetName(WrappedRowMajorQuadGridFaceIndexer2D instance, string name)
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
			var instance = CreateInstance(_faceColumnCount, _faceRowCount, _isWrappedHorizontally, _isWrappedVertically, _lowestIndex, name);
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
			return WrapX(x) + WrapY(y) * _faceColumnCount + _originFaceIndex;
		}

		public int WrapX(int x)
		{
			return _isWrappedHorizontally ? MathUtility.Modulo(x, _faceColumnCount) : x;
		}

		public int WrapY(int y)
		{
			return _isWrappedVertically ? MathUtility.Modulo(y, _faceRowCount) : y;
		}

		public Index2D Wrap(int x, int y)
		{
			return new Index2D(WrapX(x), WrapY(y));
		}

		public Index2D Wrap(Index2D index)
		{
			return Wrap(index.x, index.y);
		}
	}
}
