using UnityEngine;
using System;

namespace Experilous.Topological
{
	public class RowMajorQuadGridFaceNeighborIndexer : FaceNeighborIndexer
	{
		[SerializeField] private int _faceColumnCount;
		[SerializeField] private int _faceRowCount;

		[SerializeField] private int _vertexColumnCount;
		[SerializeField] private int _vertexRowCount;

		[SerializeField] private int _vertexCount;
		[SerializeField] private int _edgeCount;
		[SerializeField] private int _internalEdgeCount;
		[SerializeField] private int _externalEdgeCount;
		[SerializeField] private int _verticalEdgeOffset;
		[SerializeField] private int _internalFaceCount;

		[SerializeField] private int _horizontalEdgesPerColumn;
		[SerializeField] private int _verticalEdgesPerRow;

		public int faceColumnCount { get { return _faceColumnCount; } }
		public int faceRowCount { get { return _faceRowCount; } }

		public int vertexColumnCount { get { return _vertexColumnCount; } }
		public int vertexRowCount { get { return _vertexRowCount; } }

		public override int vertexCount { get { return _vertexCount; } }
		public override int edgeCount { get { return _edgeCount; } }
		public override int internalEdgeCount { get { return _internalEdgeCount; } }
		public override int externalEdgeCount { get { return _externalEdgeCount; } }
		public override int faceCount { get { return _internalFaceCount + 1; } }
		public override int internalFaceCount { get { return _internalFaceCount; } }
		public override int externalFaceCount { get { return 1; } }

		public static RowMajorQuadGridFaceNeighborIndexer CreateInstance(int faceColumnCount, int faceRowCount)
		{
			if (faceColumnCount < 1) throw new System.ArgumentOutOfRangeException("faceColumnCount");
			if (faceRowCount < 1) throw new System.ArgumentOutOfRangeException("faceRowCount");
			var instance = CreateInstance<RowMajorQuadGridFaceNeighborIndexer>();
			instance._faceColumnCount = faceColumnCount;
			instance._faceRowCount = faceRowCount;
			instance.Initialize();
			return instance;
		}

		public static RowMajorQuadGridFaceNeighborIndexer CreateInstance(int faceColumnCount, int faceRowCount, string name)
		{
			var instance = CreateInstance(faceColumnCount, faceRowCount);
			instance.name = name;
			return instance;
		}

		private void Initialize()
		{
			_vertexColumnCount = _faceColumnCount + 1;
			_vertexRowCount = _faceRowCount + 1;
			_vertexCount = _vertexColumnCount * _vertexRowCount;
			_horizontalEdgesPerColumn = _vertexRowCount * 2;
			_verticalEdgesPerRow = _vertexColumnCount * 2;
			_verticalEdgeOffset = _horizontalEdgesPerColumn * _faceColumnCount;
			_edgeCount = _verticalEdgeOffset + _verticalEdgesPerRow * _faceRowCount;
			_internalFaceCount = _faceColumnCount * _faceRowCount;
			_internalEdgeCount = _internalFaceCount * 4;
			_externalEdgeCount = _edgeCount - _internalEdgeCount;
		}

		public override object Clone()
		{
			var clone = CreateInstance(_faceColumnCount, _faceRowCount, name);
			clone.hideFlags = hideFlags;
			return clone;
		}

		public override ushort GetNeighborCount(int faceIndex)
		{
			if (faceIndex < 0 || faceIndex >= faceCount) throw new ArgumentOutOfRangeException("faceIndex");
			if (faceIndex < internalFaceCount)
			{
				return 4;
			}
			else
			{
				return (ushort)_externalEdgeCount;
			}
		}

		public override int GetNeighborVertexIndex(int faceIndex, int neighborIndex)
		{
			if (faceIndex < 0 || faceIndex >= faceCount) throw new ArgumentOutOfRangeException("faceIndex");
			if (faceIndex < _internalFaceCount)
			{
				var x = faceIndex % _faceColumnCount;
				var y = faceIndex / _faceColumnCount;
				var neighbor0 = x + y * _vertexColumnCount;
				switch (neighborIndex)
				{
					case 0: return neighbor0;
					case 1: return neighbor0 + _vertexColumnCount;
					case 2: return neighbor0 + _vertexColumnCount + 1;
					case 3: return neighbor0 + 1;
					default: throw new ArgumentOutOfRangeException("neighborIndex");
				}
			}
			else
			{
				// Return the perimeter vertices in counter-clockwise order (since the external face is back-facing).
				if (neighborIndex < 0) throw new ArgumentOutOfRangeException("neighborIndex");
				if (neighborIndex <= _faceColumnCount) return neighborIndex;
				neighborIndex -= _faceColumnCount;
				if (neighborIndex <= _faceRowCount) return _vertexColumnCount * (neighborIndex + 1) - 1;
				neighborIndex -= _faceRowCount;
				if (neighborIndex <= _faceColumnCount) return _vertexCount - neighborIndex - 1;
				neighborIndex -= _faceColumnCount;
				if (neighborIndex < _faceRowCount) return _vertexColumnCount * (_faceRowCount - neighborIndex);
				throw new ArgumentOutOfRangeException("neighborIndex");
			}
		}

		public override int GetNeighborEdgeIndex(int faceIndex, int neighborIndex)
		{
			if (faceIndex < 0 || faceIndex >= faceCount) throw new ArgumentOutOfRangeException("faceIndex");
			if (faceIndex < _internalFaceCount)
			{
				var x = faceIndex % _faceColumnCount;
				var y = faceIndex / _faceColumnCount;
				switch (neighborIndex)
				{
					case 0: return x * _horizontalEdgesPerColumn + y * 2 + 1;
					case 1: return y * _verticalEdgesPerRow + x * 2 + 1 + _verticalEdgeOffset;
					case 2: return x * _horizontalEdgesPerColumn + y * 2 + 2;
					case 3: return y * _verticalEdgesPerRow + x * 2 + 2 + _verticalEdgeOffset;
					default: throw new ArgumentOutOfRangeException("neighborIndex");
				}
			}
			else
			{
				// Return the perimeter edges in counter-clockwise order (since the external face is back-facing).
				if (neighborIndex < 0) throw new ArgumentOutOfRangeException("neighborIndex");
				if (neighborIndex < _faceColumnCount) return neighborIndex * _horizontalEdgesPerColumn;
				neighborIndex -= _faceColumnCount;
				if (neighborIndex < _faceRowCount) return _verticalEdgeOffset + _verticalEdgesPerRow * (neighborIndex + 1) - 1;
				neighborIndex -= _faceRowCount;
				if (neighborIndex < _faceColumnCount) return _verticalEdgeOffset - neighborIndex * _horizontalEdgesPerColumn - 1;
				neighborIndex -= _faceColumnCount;
				if (neighborIndex < _faceRowCount) return _verticalEdgeOffset + _verticalEdgesPerRow * (_faceRowCount - neighborIndex - 1);
				throw new ArgumentOutOfRangeException("neighborIndex");
			}
		}

		public override int GetNeighborFaceIndex(int faceIndex, int neighborIndex)
		{
			if (faceIndex < 0 || faceIndex >= faceCount) throw new ArgumentOutOfRangeException("faceIndex");
			if (faceIndex < _internalFaceCount)
			{
				switch (neighborIndex)
				{
					case 0: return (faceIndex >= _faceColumnCount) ? faceIndex - _faceColumnCount : _internalFaceCount;
					case 1: return (faceIndex % _faceColumnCount > 0) ? faceIndex - 1 : _internalFaceCount;
					case 2: return (faceIndex < _internalFaceCount - _faceColumnCount) ? faceIndex + _faceColumnCount : _internalFaceCount;
					case 3: return (faceIndex % _faceColumnCount < _faceColumnCount - 1) ? faceIndex + 1 : _internalFaceCount;
					default: throw new ArgumentOutOfRangeException("neighborIndex");
				}
			}
			else
			{
				// Return the perimeter faces in counter-clockwise order (since the external face is back-facing).
				if (neighborIndex < 0) throw new ArgumentOutOfRangeException("neighborIndex");
				if (neighborIndex < _faceColumnCount) return neighborIndex;
				neighborIndex -= _faceColumnCount;
				if (neighborIndex < _faceRowCount) return (neighborIndex + 1) * _faceColumnCount - 1;
				neighborIndex -= _faceRowCount;
				if (neighborIndex < _faceColumnCount) return _internalFaceCount - neighborIndex - 1;
				neighborIndex -= _faceColumnCount;
				if (neighborIndex < _faceRowCount) return _faceColumnCount * (_faceRowCount - neighborIndex - 1);
				throw new ArgumentOutOfRangeException("neighborIndex");
			}
		}

		public override int GetInverseNeighborIndex(int faceIndex, int neighborIndex)
		{
			if (faceIndex < 0 || faceIndex >= faceCount) throw new ArgumentOutOfRangeException("faceIndex");
			if (faceIndex < _internalFaceCount)
			{
				switch (neighborIndex)
				{
					case 0: return (faceIndex >= _faceColumnCount) ? 2 : faceIndex;
					case 1: return (faceIndex % _faceColumnCount > 0) ? 3 : _externalEdgeCount - faceIndex / _faceColumnCount - 1;
					case 2: return (faceIndex < _internalFaceCount - _faceColumnCount) ? 0 : _externalEdgeCount - _faceRowCount - faceIndex % _faceColumnCount - 1;
					case 3: return (faceIndex % _faceColumnCount < _faceColumnCount - 1) ? 1 : _faceColumnCount + faceIndex / _faceColumnCount;
					default: throw new ArgumentOutOfRangeException("neighborIndex");
				}
			}
			else
			{
				// Return the perimeter faces in counter-clockwise order (since the external face is back-facing).
				if (neighborIndex < 0) throw new ArgumentOutOfRangeException("neighborIndex");
				if (neighborIndex < _faceColumnCount) return 0;
				neighborIndex -= _faceColumnCount;
				if (neighborIndex < _faceRowCount) return 3;
				neighborIndex -= _faceRowCount;
				if (neighborIndex < _faceColumnCount) return 2;
				neighborIndex -= _faceColumnCount;
				if (neighborIndex < _faceRowCount) return 1;
				throw new ArgumentOutOfRangeException("neighborIndex");
			}
		}
	}
}
