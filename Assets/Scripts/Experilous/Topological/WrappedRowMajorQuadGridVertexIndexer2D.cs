using UnityEngine;

namespace Experilous.Topological
{
	public class WrappedRowMajorQuadGridVertexIndexer2D : VertexIndexer2D
	{
		[SerializeField] private int _vertexColumnCount;
		[SerializeField] private int _vertexRowCount;
		[SerializeField] private bool _isWrappedHorizontally;
		[SerializeField] private bool _isWrappedVertically;

		[SerializeField] private Index2D _lowestIndex;

		[SerializeField] private int _originVertexIndex;
		[SerializeField] private int _originVertexOffset;

		public int vertexColumnCount { get { return _vertexColumnCount; } }
		public int vertexRowCount { get { return _vertexRowCount; } }

		public Index2D lowestIndex { get { return _lowestIndex; } }

		public override int vertexCount { get { return _vertexColumnCount * _vertexRowCount; } }

		public static WrappedRowMajorQuadGridVertexIndexer2D CreateInstance(int faceColumnCount, int faceRowCount, bool isWrappedHorizontally, bool isWrappedVertically)
		{
			return CreateInstance(faceColumnCount, faceRowCount, isWrappedHorizontally, isWrappedVertically, new Index2D(0, 0));
		}

		public static WrappedRowMajorQuadGridVertexIndexer2D CreateInstance(int faceColumnCount, int faceRowCount, bool isWrappedHorizontally, bool isWrappedVertically, string name)
		{
			return SetName(CreateInstance(faceColumnCount, faceRowCount, isWrappedHorizontally, isWrappedVertically), name);
		}

		public static WrappedRowMajorQuadGridVertexIndexer2D CreateInstance(int faceColumnCount, int faceRowCount, bool isWrappedHorizontally, bool isWrappedVertically, Index2D lowestIndex)
		{
			if (faceColumnCount < 1) throw new System.ArgumentOutOfRangeException("faceColumnCount");
			if (faceRowCount < 1) throw new System.ArgumentOutOfRangeException("faceRowCount");
			if (!isWrappedHorizontally && !isWrappedVertically) throw new System.ArgumentException("isWrappedHorizontally");
			var instance = CreateInstance<WrappedRowMajorQuadGridVertexIndexer2D>();
			instance._vertexColumnCount = faceColumnCount + (isWrappedHorizontally ? 0 : 1);
			instance._vertexRowCount = faceRowCount + (isWrappedVertically ? 0 : 1);
			instance._isWrappedHorizontally = isWrappedHorizontally;
			instance._isWrappedVertically = isWrappedVertically;
			instance._lowestIndex = lowestIndex;
			instance.Initialize();
			return instance;
		}

		public static WrappedRowMajorQuadGridVertexIndexer2D CreateInstance(int faceColumnCount, int faceRowCount, bool isWrappedHorizontally, bool isWrappedVertically, Index2D lowestIndex, string name)
		{
			return SetName(CreateInstance(faceColumnCount, faceRowCount, isWrappedHorizontally, isWrappedVertically, lowestIndex), name);
		}

		private static WrappedRowMajorQuadGridVertexIndexer2D SetName(WrappedRowMajorQuadGridVertexIndexer2D instance, string name)
		{
			instance.name = name;
			return instance;
		}

		private void Initialize()
		{
			_originVertexIndex = -_lowestIndex.x - _lowestIndex.y * _vertexColumnCount;
			_originVertexOffset = _originVertexIndex % _vertexColumnCount;
		}

		public override object Clone()
		{
			var instance = CreateInstance(_vertexColumnCount - (_isWrappedHorizontally ? 0 : 1), _vertexRowCount - (_isWrappedVertically ? 0 : 1), _isWrappedHorizontally, _isWrappedVertically, _lowestIndex, name);
			instance.hideFlags = hideFlags;
			return instance;
		}

		public override Index2D GetVertexIndex2D(int vertexIndex)
		{
			var adjustedIndex = vertexIndex + _originVertexOffset - _originVertexIndex;
			return new Index2D(
				adjustedIndex % _vertexColumnCount - _originVertexOffset,
				adjustedIndex / _vertexColumnCount);
		}

		public override int GetVertexIndex(int x, int y)
		{
			return WrapX(x) + WrapY(y) * _vertexColumnCount + _originVertexIndex;
		}

		public int WrapX(int x)
		{
			return _isWrappedHorizontally ? MathUtility.Modulo(x, _vertexColumnCount) : x;
		}

		public int WrapY(int y)
		{
			return _isWrappedVertically ? MathUtility.Modulo(y, _vertexRowCount) : y;
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
