using UnityEngine;
using System;

namespace Experilous.Topological
{
	public class WrappedRowMajorQuadGridFaceNeighborIndexer : FaceNeighborIndexer
	{
		[SerializeField] private int _faceColumnCount;
		[SerializeField] private int _faceRowCount;
		[SerializeField] private bool _isWrappedHorizontally;
		[SerializeField] private bool _isWrappedVertically;

		[SerializeField] private int _vertexColumnCount;
		[SerializeField] private int _vertexRowCount;

		[SerializeField] private int _vertexCount;
		[SerializeField] private int _edgeCount;
		[SerializeField] private int _internalEdgeCount;
		[SerializeField] private int _externalEdgeCount;
		[SerializeField] private int _verticalEdgeOffset;
		[SerializeField] private int _faceCount;
		[SerializeField] private int _internalFaceCount;
		[SerializeField] private int _externalFaceCount;

		[SerializeField] private int _horizontalEdgesPerColumn;
		[SerializeField] private int _verticalEdgesPerRow;

		public int faceColumnCount { get { return _faceColumnCount; } }
		public int faceRowCount { get { return _faceRowCount; } }

		public int vertexColumnCount { get { return _vertexColumnCount; } }
		public int vertexRowCount { get { return _vertexRowCount; } }

		public bool isWrappedHorizontally { get { return _isWrappedHorizontally; } }
		public bool isWrappedVertically { get { return _isWrappedVertically; } }

		public override int vertexCount { get { return _vertexCount; } }
		public override int edgeCount { get { return _edgeCount; } }
		public override int internalEdgeCount { get { return _internalEdgeCount; } }
		public override int externalEdgeCount { get { return _externalEdgeCount; } }
		public override int faceCount { get { return _faceCount; } }
		public override int internalFaceCount { get { return _internalFaceCount; } }
		public override int externalFaceCount { get { return _externalFaceCount; } }

		public static WrappedRowMajorQuadGridFaceNeighborIndexer CreateInstance(int faceColumnCount, int faceRowCount, bool isWrappedHorizontally, bool isWrappedVertically)
		{
			if (faceColumnCount < 1) throw new System.ArgumentOutOfRangeException("faceColumnCount");
			if (faceRowCount < 1) throw new System.ArgumentOutOfRangeException("faceRowCount");
			if (!isWrappedHorizontally && !isWrappedVertically) throw new System.ArgumentException("isWrappedHorizontally");
			var instance = CreateInstance<WrappedRowMajorQuadGridFaceNeighborIndexer>();
			instance._faceColumnCount = faceColumnCount;
			instance._faceRowCount = faceRowCount;
			instance._isWrappedHorizontally = isWrappedHorizontally;
			instance._isWrappedVertically = isWrappedVertically;
			instance.Initialize();
			return instance;
		}

		public static WrappedRowMajorQuadGridFaceNeighborIndexer CreateInstance(int faceColumnCount, int faceRowCount, bool isWrappedHorizontally, bool isWrappedVertically, string name)
		{
			var instance = CreateInstance(faceColumnCount, faceRowCount, isWrappedHorizontally, isWrappedVertically);
			instance.name = name;
			return instance;
		}

		private void Initialize()
		{
			_vertexColumnCount = _faceColumnCount + (_isWrappedHorizontally ? 0 : 1);
			_vertexRowCount = _faceRowCount + (_isWrappedVertically ? 0 : 1);
			_vertexCount = _vertexColumnCount * _vertexRowCount;
			_horizontalEdgesPerColumn = _vertexRowCount * 2;
			_verticalEdgesPerRow = _vertexColumnCount * 2;
			_verticalEdgeOffset = _horizontalEdgesPerColumn * _faceColumnCount;
			_edgeCount = _verticalEdgeOffset + _verticalEdgesPerRow * _faceRowCount;
			_internalFaceCount = _faceColumnCount * _faceRowCount;
			_externalFaceCount = (_isWrappedHorizontally && _isWrappedVertically) ? 0 : 2;
			_faceCount = _internalFaceCount + _externalFaceCount;
			_internalEdgeCount = _internalFaceCount * 4;
			_externalEdgeCount = _edgeCount - _internalEdgeCount;
		}

		public override object Clone()
		{
			var clone = CreateInstance(_faceColumnCount, _faceRowCount, _isWrappedHorizontally, _isWrappedVertically, name);
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
				if (_isWrappedHorizontally)
				{
					return (ushort)_faceColumnCount;
				}
				else //if (_isWrappedVertically)
				{
					return (ushort)_faceRowCount;
				}
			}
		}

		public override int GetNeighborVertexIndex(int faceIndex, int neighborIndex)
		{
			if (faceIndex < 0 || faceIndex >= faceCount) throw new ArgumentOutOfRangeException("faceIndex");
			if (faceIndex < _internalFaceCount)
			{
				var x = faceIndex % _faceColumnCount;
				var y = faceIndex / _faceColumnCount;
				var neighborRow = y * _vertexColumnCount;
				switch (neighborIndex)
				{
					case 0: return neighborRow + x;
					case 1: return (neighborRow + _vertexColumnCount) % _vertexCount + x;
					case 2: return (neighborRow + _vertexColumnCount) % _vertexCount + (x + 1) % _vertexColumnCount;
					case 3: return neighborRow + (x + 1) % _vertexColumnCount;
					default: throw new ArgumentOutOfRangeException("neighborIndex");
				}
			}
			else
			{
				if (_isWrappedHorizontally)
				{
					if (neighborIndex < 0 || neighborIndex >= _vertexColumnCount) throw new ArgumentOutOfRangeException("neighborIndex");
					if (faceIndex == _internalFaceCount) // first external face
					{
						return neighborIndex;
					}
					else // second external face
					{
						return _vertexCount - neighborIndex - 1;
					}
				}
				else //if (_isWrappedVertically)
				{
					if (neighborIndex < 0 || neighborIndex >= _vertexRowCount) throw new ArgumentOutOfRangeException("neighborIndex");
					if (faceIndex == _internalFaceCount) // first external face
					{
						return _vertexColumnCount * (_vertexRowCount - neighborIndex - 1);
					}
					else // second external face
					{
						return _vertexColumnCount * (neighborIndex + 1) - 1;
					}
				}
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
					case 2: return x * _horizontalEdgesPerColumn + (y * 2 + 2) % _horizontalEdgesPerColumn;
					case 3: return y * _verticalEdgesPerRow + (x * 2 + 2) % _verticalEdgesPerRow + _verticalEdgeOffset;
					default: throw new ArgumentOutOfRangeException("neighborIndex");
				}
			}
			else
			{
				if (_isWrappedHorizontally)
				{
					if (neighborIndex < 0 || neighborIndex >= _faceColumnCount) throw new ArgumentOutOfRangeException("neighborIndex");
					if (faceIndex == _internalFaceCount) // first external face
					{
						return _horizontalEdgesPerColumn * neighborIndex;
					}
					else // second external face
					{
						return _verticalEdgeOffset - _horizontalEdgesPerColumn * neighborIndex - 1;
					}
				}
				else //if (_isWrappedVertically)
				{
					if (neighborIndex < 0 || neighborIndex >= _vertexRowCount) throw new ArgumentOutOfRangeException("neighborIndex");
					if (faceIndex == _internalFaceCount) // first external face
					{
						return _verticalEdgeOffset + _verticalEdgesPerRow * (_faceRowCount - neighborIndex - 1);
					}
					else // second external face
					{
						return _verticalEdgeOffset + _verticalEdgesPerRow * (neighborIndex + 1) - 1;
					}
				}
			}
		}

		public override int GetNeighborFaceIndex(int faceIndex, int neighborIndex)
		{
			if (faceIndex < 0 || faceIndex >= faceCount) throw new ArgumentOutOfRangeException("faceIndex");
			if (faceIndex < _internalFaceCount)
			{
				switch (neighborIndex)
				{
					case 0: return (faceIndex >= _faceColumnCount) ? faceIndex - _faceColumnCount : (_isWrappedVertically ? faceIndex + _internalFaceCount - _faceColumnCount : _internalFaceCount);
					case 1: return (faceIndex % _faceColumnCount > 0) ? faceIndex - 1 : (_isWrappedHorizontally ? faceIndex + _faceColumnCount - 1 : _internalFaceCount);
					case 2: return (faceIndex < _internalFaceCount - _faceColumnCount) ? faceIndex + _faceColumnCount : (_isWrappedVertically ? faceIndex - _internalFaceCount + _faceColumnCount : _internalFaceCount + 1);
					case 3: return (faceIndex % _faceColumnCount < _faceColumnCount - 1) ? faceIndex + 1 : (_isWrappedHorizontally ? faceIndex - _faceColumnCount + 1 : _internalFaceCount + 1);
					default: throw new ArgumentOutOfRangeException("neighborIndex");
				}
			}
			else
			{
				if (_isWrappedHorizontally)
				{
					if (neighborIndex < 0 || neighborIndex >= _faceColumnCount) throw new ArgumentOutOfRangeException("neighborIndex");
					if (faceIndex == _internalFaceCount) // first external face
					{
						return neighborIndex;
					}
					else // second external face
					{
						return _internalFaceCount - neighborIndex - 1;
					}
				}
				else //if (_isWrappedVertically)
				{
					if (neighborIndex < 0 || neighborIndex >= _vertexRowCount) throw new ArgumentOutOfRangeException("neighborIndex");
					if (faceIndex == _internalFaceCount) // first external face
					{
						return _faceColumnCount * (_faceRowCount - neighborIndex - 1);
					}
					else // second external face
					{
						return _faceColumnCount * (neighborIndex + 1) - 1;
					}
				}
			}
		}

		public override int GetInverseNeighborIndex(int faceIndex, int neighborIndex)
		{
			if (faceIndex < 0 || faceIndex >= faceCount) throw new ArgumentOutOfRangeException("faceIndex");
			if (faceIndex < _internalFaceCount)
			{
				switch (neighborIndex)
				{
					case 0: return (_isWrappedVertically || faceIndex >= _faceColumnCount) ? 2 : faceIndex;
					case 1: return (_isWrappedHorizontally || faceIndex % _faceColumnCount > 0) ? 3 : _faceRowCount - faceIndex / _faceColumnCount - 1;
					case 2: return (_isWrappedVertically || faceIndex < _internalFaceCount - _faceColumnCount) ? 0 : _internalFaceCount - faceIndex - 1;
					case 3: return (_isWrappedHorizontally || faceIndex % _faceColumnCount < _faceColumnCount - 1) ? 1 : faceIndex / _faceColumnCount;
					default: throw new ArgumentOutOfRangeException("neighborIndex");
				}
			}
			else
			{
				if (_isWrappedHorizontally)
				{
					if (neighborIndex < 0 || neighborIndex >= _faceColumnCount) throw new ArgumentOutOfRangeException("neighborIndex");
					if (faceIndex == _internalFaceCount) // first external face
					{
						return 0;
					}
					else // second external face
					{
						return 2;
					}
				}
				else //if (_isWrappedVertically)
				{
					if (neighborIndex < 0 || neighborIndex >= _vertexRowCount) throw new ArgumentOutOfRangeException("neighborIndex");
					if (faceIndex == _internalFaceCount) // first external face
					{
						return 1;
					}
					else // second external face
					{
						return 3;
					}
				}
			}
		}

		public EdgeWrap GetEdgeWrapData(int faceIndex, int neighborIndex)
		{
			if (faceIndex < 0 || faceIndex >= faceCount) throw new ArgumentOutOfRangeException("faceIndex");
			if (faceIndex < _internalFaceCount)
			{
				switch (neighborIndex)
				{
					case 0: return (!_isWrappedVertically || faceIndex >= _faceColumnCount) ? EdgeWrap.None : EdgeWrap.NegativeAxis1;
					case 1: return (!_isWrappedHorizontally || faceIndex % _faceColumnCount > 0) ? EdgeWrap.None : EdgeWrap.NegativeAxis0;
					case 2: return (!_isWrappedVertically || faceIndex < _internalFaceCount - _faceColumnCount) ? EdgeWrap.None : EdgeWrap.PositiveAxis1;
					case 3: return (!_isWrappedHorizontally || faceIndex % _faceColumnCount < _faceColumnCount - 1) ? EdgeWrap.None : EdgeWrap.PositiveAxis0;
					default: throw new ArgumentOutOfRangeException("neighborIndex");
				}
			}
			else
			{
				return new EdgeWrap();
			}
		}
	}
}
