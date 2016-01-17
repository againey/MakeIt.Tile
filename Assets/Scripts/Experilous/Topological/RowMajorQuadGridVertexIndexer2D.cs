using UnityEngine;

namespace Experilous.Topological
{
	public class RowMajorQuadGridVertexIndexer2D : VertexIndexer2D
	{
		[SerializeField] private int _vertexColumnCount;
		[SerializeField] private int _vertexRowCount;

		[SerializeField] private Index2D _lowestIndex;

		[SerializeField] private int _originVertexIndex;
		[SerializeField] private int _originVertexOffset;

		public int vertexColumnCount { get { return _vertexColumnCount; } }
		public int vertexRowCount { get { return _vertexRowCount; } }

		public Index2D lowestIndex { get { return _lowestIndex; } }

		public override int vertexCount { get { return _vertexColumnCount * _vertexRowCount; } }

		public static RowMajorQuadGridVertexIndexer2D CreateInstance(int vertexColumnCount, int vertexRowCount)
		{
			return CreateInstance(vertexColumnCount, vertexRowCount, new Index2D(0, 0));
		}

		public static RowMajorQuadGridVertexIndexer2D CreateInstance(int vertexColumnCount, int vertexRowCount, string name)
		{
			return SetName(CreateInstance(vertexColumnCount, vertexRowCount), name);
		}

		public static RowMajorQuadGridVertexIndexer2D CreateInstance(int vertexColumnCount, int vertexRowCount, Index2D lowestIndex)
		{
			var instance = CreateInstance<RowMajorQuadGridVertexIndexer2D>();
			instance._vertexColumnCount = vertexColumnCount;
			instance._vertexRowCount = vertexRowCount;
			instance._lowestIndex = lowestIndex;
			instance.Initialize();
			return instance;
		}

		public static RowMajorQuadGridVertexIndexer2D CreateInstance(int vertexColumnCount, int vertexRowCount, Index2D lowestIndex, string name)
		{
			return SetName(CreateInstance(vertexColumnCount, vertexRowCount, lowestIndex), name);
		}

		private static RowMajorQuadGridVertexIndexer2D SetName(RowMajorQuadGridVertexIndexer2D instance, string name)
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
			var instance = CreateInstance(_vertexColumnCount, _vertexRowCount, _lowestIndex, name);
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
			return x + y * _vertexColumnCount + _originVertexIndex;
		}
	}
}
