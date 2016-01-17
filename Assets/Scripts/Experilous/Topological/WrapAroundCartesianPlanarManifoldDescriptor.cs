using UnityEngine;
using System;

namespace Experilous.Topological
{
#if false
	public class WrapAroundCartesianPlanarManifoldDescriptor : ScriptableObject
	{
		[Serializable]
		public enum Orientation
		{
			RowMajor,
			ColumnMajor,
		}

		[Serializable]
		public struct Coordinate
		{
			public int x;
			public int y;

			public Coordinate(int x, int y)
			{
				this.x = x;
				this.y = y;
			}

			public Coordinate Offset(int dx, int dy)
			{
				return new Coordinate(x + dx, y + dy);
			}
		}

		public int columnCount;
		public int rowCount;

		public bool isHorizontallyWrapped;
		public bool isVerticallyWrapped;

		public Orientation orientation;

		public Coordinate lowestCoordinate;

		public Vector3 horizontalAxis;
		public Vector3 verticalAxis;

		public Vector3[] repetitionAxes { get { return _repetitionAxes; } }
		public int nullRepetitionAxisIndex { get { return 0; } }
		public int horizontalRepetitionAxisIndex { get { return isHorizontallyWrapped ? 1 : 0; } }
		public int verticalRepetitionAxisIndex { get { return isVerticallyWrapped ? isHorizontallyWrapped ? 2 : 1 : 0; } }

		[SerializeField] private Vector3[] _repetitionAxes;

		[SerializeField] private int _horizontalVertexStride;
		[SerializeField] private int _verticalVertexStride;
		[SerializeField] private int _originVertexIndex;
		[SerializeField] private int _originVertexOffset;

		[SerializeField] private int _horizontalFaceStride;
		[SerializeField] private int _verticalFaceStride;
		[SerializeField] private int _originFaceIndex;
		[SerializeField] private int _originFaceOffset;

		public static WrapAroundCartesianPlanarManifoldDescriptor CreateInstance(int columnCount, int rowCount, bool isHorizontallyWrapped, bool isVerticallyWrapped, string name)
		{
			var instance = CreateInstance(columnCount, rowCount, isHorizontallyWrapped, isVerticallyWrapped);
			instance.name = name;
			return instance;
		}

		public static WrapAroundCartesianPlanarManifoldDescriptor CreateInstance(int columnCount, int rowCount, bool isHorizontallyWrapped, bool isVerticallyWrapped)
		{
			return CreateInstance(columnCount, rowCount, isHorizontallyWrapped, isVerticallyWrapped, Orientation.RowMajor, new Coordinate(0, 0));
		}

		public static WrapAroundCartesianPlanarManifoldDescriptor CreateInstance(int columnCount, int rowCount, bool isHorizontallyWrapped, bool isVerticallyWrapped, Orientation orientation, Coordinate lowestCoordinate, string name)
		{
			var instance = CreateInstance(columnCount, rowCount, isHorizontallyWrapped, isVerticallyWrapped, orientation, lowestCoordinate);
			instance.name = name;
			return instance;
		}

		public static WrapAroundCartesianPlanarManifoldDescriptor CreateInstance(int columnCount, int rowCount, bool isHorizontallyWrapped, bool isVerticallyWrapped, Orientation orientation, Coordinate lowestCoordinate)
		{
			return CreateInstance(columnCount, rowCount, isHorizontallyWrapped, isVerticallyWrapped, orientation, lowestCoordinate, new Vector3(1f, 0f, 0f), new Vector3(0f, 1f, 0f));
		}

		public static WrapAroundCartesianPlanarManifoldDescriptor CreateInstance(int columnCount, int rowCount, bool isHorizontallyWrapped, bool isVerticallyWrapped, Orientation orientation, Coordinate lowestCoordinate, Vector3 horizontalAxis, Vector3 verticalAxis, string name)
		{
			var instance = CreateInstance(columnCount, rowCount, isHorizontallyWrapped, isVerticallyWrapped, orientation, lowestCoordinate, horizontalAxis, verticalAxis);
			instance.name = name;
			return instance;
		}

		public static WrapAroundCartesianPlanarManifoldDescriptor CreateInstance(int columnCount, int rowCount, bool isHorizontallyWrapped, bool isVerticallyWrapped, Orientation orientation, Coordinate lowestCoordinate, Vector3 horizontalAxis, Vector3 verticalAxis)
		{
			var instance = CreateInstance<WrapAroundCartesianPlanarManifoldDescriptor>();
			instance.columnCount = columnCount;
			instance.rowCount = rowCount;
			instance.isHorizontallyWrapped = isHorizontallyWrapped;
			instance.isVerticallyWrapped = isVerticallyWrapped;
			instance.orientation = orientation;
			instance.lowestCoordinate = lowestCoordinate;
			instance.horizontalAxis = horizontalAxis;
			instance.verticalAxis = verticalAxis;

			switch (orientation)
			{
				case Orientation.RowMajor:
					instance._horizontalVertexStride = 1;
					instance._horizontalFaceStride = 1;
					instance._verticalVertexStride = columnCount + (isVerticallyWrapped ? 0 : 1);
					instance._verticalFaceStride = columnCount;
					instance._originVertexIndex = -lowestCoordinate.x - lowestCoordinate.y * instance._verticalVertexStride;
					instance._originFaceIndex = -lowestCoordinate.x - lowestCoordinate.y * instance._verticalFaceStride;
					instance._originVertexOffset = instance._originVertexIndex % instance._verticalVertexStride;
					instance._originFaceOffset = instance._originFaceIndex % instance._verticalFaceStride;
					break;
				case Orientation.ColumnMajor:
					instance._horizontalVertexStride = rowCount + (isHorizontallyWrapped ? 0 : 1);
					instance._horizontalFaceStride = rowCount;
					instance._verticalVertexStride = 1;
					instance._verticalFaceStride = 1;
					instance._originVertexIndex = -lowestCoordinate.y - lowestCoordinate.x * instance._horizontalVertexStride;
					instance._originFaceIndex = -lowestCoordinate.y - lowestCoordinate.x * instance._horizontalFaceStride;
					instance._originVertexOffset = instance._originVertexIndex % instance._verticalVertexStride;
					instance._originFaceOffset = instance._originFaceIndex % instance._verticalFaceStride;
					instance._originVertexOffset = instance._originVertexIndex % instance._horizontalVertexStride;
					instance._originFaceOffset = instance._originFaceIndex % instance._horizontalFaceStride;
					break;
				default:
					throw new NotImplementedException();
			}

			if (isHorizontallyWrapped && isVerticallyWrapped)
			{
				instance._repetitionAxes = new Vector3[3] { new Vector3(0f, 0f, 0f), horizontalAxis, verticalAxis };
			}
			else if (isHorizontallyWrapped)
			{
				instance._repetitionAxes = new Vector3[2] { new Vector3(0f, 0f, 0f), horizontalAxis };
			}
			else if (isVerticallyWrapped)
			{
				instance._repetitionAxes = new Vector3[2] { new Vector3(0f, 0f, 0f), verticalAxis };
			}
			else
			{
				throw new InvalidOperationException("A wrap-around Cartesian planar manifold must wrap either horizontally or vertically (or both), but both were specified as not wrapping.");
			}

			return instance;
		}

		public int WrapX(int x)
		{
			return isHorizontallyWrapped ? (x + lowestCoordinate.x) % columnCount - lowestCoordinate.x : x;
		}

		public int WrapY(int y)
		{
			return isVerticallyWrapped ? (y + lowestCoordinate.y) % rowCount - lowestCoordinate.y : y;
		}

		public Coordinate WrapCoordinate(int x, int y)
		{
			return new Coordinate(WrapX(x), WrapY(y));
		}

		public Coordinate WrapCoordinate(Coordinate coordinate)
		{
			return new Coordinate(WrapX(coordinate.x), WrapY(coordinate.y));
		}

		public int GetVertexIndex(int x, int y)
		{
			return x * _horizontalVertexStride + y * _verticalVertexStride + _originVertexIndex;
		}

		public int GetVertexIndex(Coordinate coordinate)
		{
			return coordinate.x * _horizontalVertexStride + coordinate.y * _verticalVertexStride + _originVertexIndex;
		}

		public int GetWrappedVertexIndex(int x, int y)
		{
			return GetVertexIndex(WrapCoordinate(x, y));
		}

		public int GetWrappedVertexIndex(Coordinate coordinate)
		{
			return GetVertexIndex(WrapCoordinate(coordinate));
		}

		public Coordinate GetVertexCoordinate(int vertexIndex)
		{
			var adjustedIndex = vertexIndex + _originVertexOffset - _originVertexIndex;
			switch (orientation)
			{
				case Orientation.RowMajor:
					return new Coordinate(
						adjustedIndex % _verticalVertexStride - _originFaceOffset,
						adjustedIndex / _verticalVertexStride);
				case Orientation.ColumnMajor:
					return new Coordinate(
						adjustedIndex / _horizontalVertexStride,
						adjustedIndex % _horizontalVertexStride - _originFaceOffset);
				default:
					throw new NotImplementedException();
			}
		}

		public int GetFaceIndex(int x, int y)
		{
			return x * _horizontalFaceStride + y * _verticalFaceStride + _originFaceIndex;
		}

		public int GetFaceIndex(Coordinate coordinate)
		{
			return coordinate.x * _horizontalFaceStride + coordinate.y * _verticalFaceStride + _originFaceIndex;
		}

		public int GetWrappedFaceIndex(int x, int y)
		{
			return GetFaceIndex(WrapCoordinate(x, y));
		}

		public int GetWrappedFaceIndex(Coordinate coordinate)
		{
			return GetFaceIndex(WrapCoordinate(coordinate));
		}

		public Coordinate GetFaceCoordinate(int faceIndex)
		{
			var adjustedIndex = faceIndex + _originFaceOffset - _originFaceIndex;
			switch (orientation)
			{
				case Orientation.RowMajor:
					return new Coordinate(
						adjustedIndex % _verticalFaceStride - _originFaceOffset,
						adjustedIndex / _verticalFaceStride);
				case Orientation.ColumnMajor:
					return new Coordinate(
						adjustedIndex / _horizontalFaceStride,
						adjustedIndex % _horizontalFaceStride - _originFaceOffset);
				default:
					throw new NotImplementedException();
			}
		}
	}
#endif
}
