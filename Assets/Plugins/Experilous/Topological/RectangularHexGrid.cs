using UnityEngine;
using System;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public enum HexGridAxisStyles
	{
		Straight,
		Staggered,
	}

	public enum HexGridAxisRelations
	{
		Acute,
		Obtuse,
	}

	public struct HexGridDescriptor
	{
		public HexGridAxisStyles axisStyle0;
		public HexGridAxisStyles axisStyle1;
		public HexGridAxisRelations axisRelation;
		public bool variableRowLength;

		public HexGridDescriptor(HexGridAxisStyles axisStyle0, HexGridAxisStyles axisStyle1, HexGridAxisRelations axisRelation, bool variableRowLength = false)
		{
			this.axisStyle0 = axisStyle0;
			this.axisStyle1 = axisStyle1;
			this.axisRelation = axisRelation;
			this.variableRowLength = variableRowLength;
		}
	}

	public class RectangularHexGrid : PlanarSurface, IFaceNeighborIndexer, IFaceIndexer2D, IVertexIndexer2D
	{
		public HexGridAxisStyles axis0Style;
		public HexGridAxisStyles axis1Style;
		public HexGridAxisRelations axisRelation;
		public bool variableRowLength;

		public Index2D size;
		public Topology topology;

		public Vector3 faceAxis0;
		public Vector3 faceAxis1;

		public const float halfSqrtThree = 0.8660254038f;

		[SerializeField] private int _vertexCount;
		[SerializeField] private int _edgeCount;
		[SerializeField] private int _internalFaceCount;
		[SerializeField] private int _externalFaceCount;

		[SerializeField] private bool _vertexRowEvenIndentNextFront = false;
		[SerializeField] private bool _vertexRowOddIndentNextFront = false;
		[SerializeField] private bool _vertexRowFirstIndentNextFront = false;
		[SerializeField] private bool _vertexRowLastIndentNextFront = false;

		[SerializeField] private bool _vertexRowEvenIndentNextBack = false;
		[SerializeField] private bool _vertexRowOddIndentNextBack = false;
		[SerializeField] private bool _vertexRowFirstIndentNextBack = false;
		//[SerializeField] private bool _vertexRowLastIndentNextBack = false;

		[SerializeField] private bool _wrapRows = false;
		[SerializeField] private bool _wrapCols = false;

		[SerializeField] private bool _reverseColumnsRows = false;
		[SerializeField] private bool _reverseWindingOrder = false;

		[SerializeField] private int _faceColumnCount;
		[SerializeField] private int _faceColumnCountFirst;
		[SerializeField] private int _faceColumnCountEven;
		[SerializeField] private int _faceColumnCountOdd;
		[SerializeField] private int _faceColumnCountLast;
		[SerializeField] private int _faceColumnCountPair;
		[SerializeField] private int _faceRowCount;

		[SerializeField] private int _faceOffset;

		[SerializeField] private int _vertexColumnCount;
		[SerializeField] private int _vertexColumnCountFirst;
		[SerializeField] private int _vertexColumnCountLast;
		[SerializeField] private int _vertexRowCount;

		[SerializeField] private int _vertexLowerOffsetFirst;
		[SerializeField] private int _vertexLowerOffsetEven;
		[SerializeField] private int _vertexUpperOffsetEven;
		[SerializeField] private int _vertexLowerOffsetOdd;
		[SerializeField] private int _vertexUpperOffsetOdd;
		[SerializeField] private int _vertexUpperOffsetLast;

		[SerializeField] private int _vertexInterceptOffsetFirst;
		[SerializeField] private int _vertexInterceptOffsetEven;
		[SerializeField] private int _vertexInterceptOffsetOdd;
		[SerializeField] private int _vertexInterceptOffsetLast;

		public static RectangularHexGrid Create(PlanarDescriptor planarDescriptor, HexGridDescriptor hexDescriptor, Index2D size)
		{
			return CreateInstance<RectangularHexGrid>().Reset(planarDescriptor, hexDescriptor, size);
		}

		public RectangularHexGrid Reset(PlanarDescriptor planarDescriptor, HexGridDescriptor hexDescriptor, Index2D size)
		{
			Reset(planarDescriptor);
			faceAxis0 = planarDescriptor.axis0.vector;
			faceAxis1 = planarDescriptor.axis1.vector;
			axis0Style = hexDescriptor.axisStyle0;
			axis1Style = hexDescriptor.axisStyle1;
			axisRelation = hexDescriptor.axisRelation;
			variableRowLength = hexDescriptor.variableRowLength;
			this.size = size;
			Initialize();
			return this;
		}

		protected void Initialize()
		{
			Validate();
			InitializeCoreMetrics();
			InitializeIndentation();
			InitializeFaceMetrics();
			InitializeVertexMetrics();
			InitializeEdgeMetrics();
		}

		private void Validate()
		{
			if (size.x <= 0 || size.y <= 0)
				throw new InvalidTopologyException("A rectangular hexagonal grid cannot have an axis size less than or equal to zero.");

			if (axis0Style == HexGridAxisStyles.Staggered && axis1Style == HexGridAxisStyles.Staggered)
				throw new InvalidTopologyException("A rectangular hexagonal grid cannot have both axis styles set to staggered.");
			if (axis0Style == HexGridAxisStyles.Straight && axis1Style == HexGridAxisStyles.Straight && variableRowLength)
				throw new InvalidTopologyException("A rectangular hexagonal grid cannot have a variable row length if neither axis style is set to staggered.");

			if (axis0.isWrapped && axis1Style == HexGridAxisStyles.Staggered && variableRowLength ||
				axis1.isWrapped && axis0Style == HexGridAxisStyles.Staggered && variableRowLength)
				throw new InvalidTopologyException("A rectangular hexagonal grid cannot have a variable row length and a wrapped straight axis when the other axis is staggered.");
			if (axis0.isWrapped && axis0Style == HexGridAxisStyles.Staggered && (size.x % 2) != 0 ||
				axis1.isWrapped && axis1Style == HexGridAxisStyles.Staggered && (size.y % 2) != 0)
				throw new InvalidTopologyException("A rectangular hexagonal grid cannot have a wrapped staggered axis with an odd size.");
		}

		private void InitializeCoreMetrics()
		{
			axis0.vector *= size.x;
			axis1.vector *= size.y;

			if (axis0Style == HexGridAxisStyles.Staggered) axis0.vector *= halfSqrtThree;
			if (axis1Style == HexGridAxisStyles.Staggered) axis1.vector *= halfSqrtThree;

			if (axis0Style != HexGridAxisStyles.Staggered)
			{
				_faceColumnCount = size.x;
				_faceRowCount = size.y;

				_wrapRows = axis0.isWrapped;
				_wrapCols = axis1.isWrapped;

				_reverseColumnsRows = false;
				_reverseWindingOrder = isInverted;
			}
			else
			{
				_faceColumnCount = size.y;
				_faceRowCount = size.x;

				_wrapRows = axis1.isWrapped;
				_wrapCols = axis0.isWrapped;

				_reverseColumnsRows = true;
				_reverseWindingOrder = !isInverted;
			}
		}

		private void InitializeIndentation()
		{
			_vertexRowEvenIndentNextFront = false;
			_vertexRowOddIndentNextFront = false;

			if (axis0Style == HexGridAxisStyles.Straight && axis1Style == HexGridAxisStyles.Straight)
			{
				switch (axisRelation)
				{
					case HexGridAxisRelations.Acute:
						_vertexRowEvenIndentNextFront = true;
						_vertexRowOddIndentNextFront = true;
						break;
					case HexGridAxisRelations.Obtuse:
						_vertexRowEvenIndentNextFront = false;
						_vertexRowOddIndentNextFront = false;
						break;
					default: throw new NotImplementedException();
				}
			}
			else
			{
				switch (axisRelation)
				{
					case HexGridAxisRelations.Acute:
						_vertexRowEvenIndentNextFront = false;
						_vertexRowOddIndentNextFront = true;
						break;
					case HexGridAxisRelations.Obtuse:
						_vertexRowEvenIndentNextFront = true;
						_vertexRowOddIndentNextFront = false;
						break;
					default: throw new NotImplementedException();
				}
			}

			_vertexRowFirstIndentNextFront = _vertexRowEvenIndentNextFront;
			_vertexRowLastIndentNextFront = MathUtility.IsEven(_faceRowCount) ? _vertexRowEvenIndentNextFront : _vertexRowOddIndentNextFront;

			// If row lengths are variable so that there's a symmetry on both ends of the rows, then
			// the indentation on the back end is identical to that on the front end.
			if (variableRowLength)
			{
				_vertexRowEvenIndentNextBack = _vertexRowEvenIndentNextFront;
				_vertexRowOddIndentNextBack = _vertexRowOddIndentNextFront;
				_vertexRowFirstIndentNextBack = _vertexRowFirstIndentNextFront;
				//_vertexRowLastIndentNextBack = _vertexRowLastIndentNextFront;
			}
			// Otherwise, the shifted nature of the rows causes the back side indentation to be the
			// opposite of the front side indentation.
			else
			{
				_vertexRowEvenIndentNextBack = !_vertexRowEvenIndentNextFront;
				_vertexRowOddIndentNextBack = !_vertexRowOddIndentNextFront;
				_vertexRowFirstIndentNextBack = !_vertexRowFirstIndentNextFront;
				//_vertexRowLastIndentNextBack = !_vertexRowLastIndentNextFront;
			}
		}

		private void InitializeFaceMetrics()
		{
			_faceColumnCountEven = (variableRowLength && _vertexRowEvenIndentNextFront && !_vertexRowOddIndentNextFront) ? _faceColumnCount - 1 : _faceColumnCount;
			_faceColumnCountOdd = (variableRowLength && _vertexRowOddIndentNextFront && !_vertexRowEvenIndentNextFront) ? _faceColumnCount - 1 : _faceColumnCount;
			_faceColumnCountFirst = _faceColumnCountEven;
			_faceColumnCountLast = MathUtility.IsEven(_faceRowCount) ? _faceColumnCountOdd : _faceColumnCountEven;
			_faceColumnCountPair = _faceColumnCountEven + _faceColumnCountOdd;

			_faceOffset = _faceColumnCount - _faceColumnCountEven;

			_internalFaceCount = _faceColumnCountPair * (_faceRowCount / 2) + (MathUtility.IsEven(_faceRowCount) ? 0 : _faceColumnCountLast);

			if (axis0.isWrapped && axis1.isWrapped)
			{
				_externalFaceCount = 0;
			}
			else if (axis0.isWrapped || axis1.isWrapped)
			{
				_externalFaceCount = 2;
			}
			else
			{
				_externalFaceCount = 1;
			}
		}

		private void InitializeVertexMetrics()
		{
			// If our rows have varying lengths, then rows can't be wrapping, and there'll be an odd number of vertices per row.
			if (variableRowLength)
			{
				_vertexColumnCount = _faceColumnCount * 2 + 1;
			}
			// Otherwise, if rows aren't wrapping, there will be a pair of vertices for each, face, plus an extra pair due to row offsets.
			else if (!_wrapRows)
			{
				_vertexColumnCount = (_faceColumnCount + 1) * 2;
			}
			// If rows are wrapping, then the extra pair is gone, taken care of by the wrapped vertices from the other side.
			else
			{
				_vertexColumnCount = _faceColumnCount * 2;
			}

			// If columns aren't wrapping, then the vertex row count is one greater than the face row count.
			if (!_wrapCols)
			{
				_vertexRowCount = _faceRowCount + 1;
			}
			// Otherwise, the last row of vertices is taken care of by the wrapped vertices from the other side.
			else
			{
				_vertexRowCount = _faceRowCount;
			}

			// If no wrapping is involved at all, then the first and last rows of vertices might have unique lengths.
			if (!_wrapRows && !_wrapCols)
			{
				_vertexColumnCountFirst = _faceColumnCountFirst * 2 + 1;
				_vertexColumnCountLast = _faceColumnCountLast * 2 + 1;
				_vertexCount = _vertexColumnCountFirst + _vertexColumnCount * (_vertexRowCount - 2) + _vertexColumnCountLast;
			}
			// Otherwise, the first and last rows are guaranteed to have the same lengths as all other rows.
			else
			{
				_vertexColumnCountFirst = _vertexColumnCount;
				_vertexColumnCountLast = _vertexColumnCount;
				_vertexCount = _vertexColumnCount * _vertexRowCount;
			}

			// The length of the first row relative to the ordinary row affects the offsets of all other rows after it.
			var offsetAdjustment = _vertexColumnCount - _vertexColumnCountFirst;

			// Offsets for determining the first lower and first upper vertex of each face are further determined
			// by how each row is indented relative to the row befor or after it.
			_vertexLowerOffsetEven = (_vertexRowEvenIndentNextFront ? 1 : 0) - offsetAdjustment;
			_vertexUpperOffsetEven = (!_vertexRowOddIndentNextFront ? 1 : 0) - offsetAdjustment;
			_vertexLowerOffsetOdd = (_vertexRowOddIndentNextFront ? 1 : 0) - offsetAdjustment;
			_vertexUpperOffsetOdd = (!_vertexRowEvenIndentNextFront ? 1 : 0) - offsetAdjustment;

			// Offsets for determining the vertex in each row that intercepts the column axis behave likewise.
			_vertexInterceptOffsetEven = (_vertexRowEvenIndentNextFront ? 1 : 0) - offsetAdjustment;
			_vertexInterceptOffsetOdd = (_vertexRowOddIndentNextFront ? 1 : 0) - offsetAdjustment;

			// By default, the first and last row offsets are the same as the rest.
			_vertexLowerOffsetFirst = _vertexLowerOffsetEven;
			_vertexInterceptOffsetFirst = _vertexInterceptOffsetEven;

			// Note that the upper offset of the last row is based on the number of face rows, and thus ignores
			// wrapping around column ends, while the intercept offset is instead based on the number of vertex
			// rows, thus taking into account wrapping around column ends.  This is due to how each of these is
			// used in various functions.
			_vertexUpperOffsetLast = MathUtility.IsEven(_faceRowCount) ? _vertexUpperOffsetOdd : _vertexUpperOffsetEven;
			_vertexInterceptOffsetLast = MathUtility.IsEven(_vertexRowCount) ? _vertexInterceptOffsetOdd : _vertexInterceptOffsetEven;

			// If not wrapping at all, then the first and last row offsets might need adjustment,
			// since the first and last rows might have a different arrangement of verticies than
			// all the normal rows.
			if (!_wrapRows && !_wrapCols)
			{
				// If there is a missing vertex at the back of the first row, indicated by an
				// indentation on the next side, then the first row offsets need to be increased.
				if (_vertexRowFirstIndentNextBack)
				{
					_vertexLowerOffsetFirst += 1;
					_vertexInterceptOffsetFirst += 1;
				}

				// If there is a missing vertex at the front of the last row, indicated by an
				// indentation on the prev side, then the last row offsets need to be decreased.
				if (!_vertexRowLastIndentNextFront)
				{
					_vertexUpperOffsetLast -= 1;
					_vertexInterceptOffsetLast -= 1;
				}
			}
		}

		private void InitializeEdgeMetrics()
		{
			if (_wrapRows && _wrapCols)
			{
				_edgeCount = _internalFaceCount * 6;
			}
			else if (_wrapRows)
			{
				_edgeCount = _internalFaceCount * 6 + _faceRowCount * 4;
			}
			else if (_wrapCols)
			{
				_edgeCount = _internalFaceCount * 6 + _faceColumnCount * 4;
			}
			else
			{
				_edgeCount = _internalFaceCount * 6 + _faceRowCount * 4 + _faceColumnCountFirst * 2 + _faceColumnCountLast * 2 - 2;
			}
		}

		public int vertexCount { get { return _vertexCount; } }
		public int edgeCount { get { return _edgeCount; } }
		public int faceCount { get { return _internalFaceCount + _externalFaceCount; } }
		public int internalFaceCount { get { return _internalFaceCount; } }
		public int externalFaceCount { get { return _externalFaceCount; } }

		public Topology CreateTopology()
		{
			return TopologyBuilder.BuildTopoogy(this);
		}

		public Topology CreateManifold(out Vector3[] vertexPositions)
		{
			vertexPositions = new Vector3[vertexCount];

			Vector3 rowAxisBasis;
			Vector3 columnAxisBasis;
			HexGridAxisStyles columnAxisStyle;

			Vector3 rowAxis;
			Vector3 rowAxisOddOffset;
			Vector3 columnAxisDouble;
			Vector3 columnAxisOddOffset;

			if (!_reverseColumnsRows)
			{
				rowAxisBasis = faceAxis0;
				columnAxisBasis = faceAxis1;
				columnAxisStyle = axis1Style;
			}
			else
			{
				rowAxisBasis = faceAxis1;
				columnAxisBasis = faceAxis0;
				columnAxisStyle = axis0Style;
			}

			if (columnAxisStyle != HexGridAxisStyles.Staggered)
			{
				rowAxis = rowAxisBasis * 0.5f;
				columnAxisDouble = columnAxisBasis * 2f;
				columnAxisOddOffset = columnAxisBasis;
			}
			else
			{
				rowAxis = rowAxisBasis * 0.5f;
				columnAxisDouble = columnAxisBasis * halfSqrtThree * 2f;

				switch (axisRelation)
				{
					case HexGridAxisRelations.Acute: columnAxisOddOffset = columnAxisBasis * halfSqrtThree + rowAxis; break;
					case HexGridAxisRelations.Obtuse: columnAxisOddOffset = columnAxisBasis * halfSqrtThree - rowAxis; break;
					default: throw new NotImplementedException();
				}
			}

			switch (axisRelation)
			{
				case HexGridAxisRelations.Acute: rowAxisOddOffset = (4f * rowAxis - columnAxisOddOffset) / 3f - rowAxis; break;
				case HexGridAxisRelations.Obtuse: rowAxisOddOffset = (2f * rowAxis - columnAxisOddOffset) / 3f - rowAxis; break;
				default: throw new NotImplementedException();
			}

			for (int i = 0; i < vertexCount; ++i)
			{
				int col, row;
				GetVertexColRow(i, out col, out row);
				var vertexPosition = origin + col * rowAxis + (row / 2) * columnAxisDouble;
				if (MathUtility.IsOdd(col)) vertexPosition += rowAxisOddOffset;
				if (MathUtility.IsOdd(row)) vertexPosition += columnAxisOddOffset;
				vertexPositions[i] = vertexPosition;
			}

			return CreateTopology();
		}

		#region IFaceNeighborIndexer Methods

		public ushort GetNeighborCount(int faceIndex)
		{
			if (faceIndex < 0 || faceIndex >= _internalFaceCount) throw new ArgumentOutOfRangeException("faceIndex");

			return 6;
		}

		public int GetNeighborVertexIndex(int faceIndex, int neighborIndex)
		{
			if (faceIndex < 0 || faceIndex >= _internalFaceCount) throw new ArgumentOutOfRangeException("faceIndex");

			int col, row;
			GetFaceColRow(faceIndex, out col, out row);

			switch (ConditionalInvert(neighborIndex, 6, _reverseWindingOrder))
			{
				case 0: return GetLowerVertexIndex(col, row, 0);
				case 1: return GetUpperVertexIndex(col, row, 0);
				case 2: return GetUpperVertexIndex(col, row, 1);
				case 3: return GetUpperVertexIndex(col, row, 2);
				case 4: return GetLowerVertexIndex(col, row, 2);
				case 5: return GetLowerVertexIndex(col, row, 1);
				default: throw new ArgumentOutOfRangeException("neighborIndex");
			}
		}

		public EdgeWrap GetEdgeWrap(int faceIndex, int neighborIndex)
		{
			if (faceIndex < 0 || faceIndex >= _internalFaceCount) throw new ArgumentOutOfRangeException("faceIndex");

			if (!_wrapRows && !_wrapCols) return EdgeWrap.None;

			EdgeWrap wrap;

			switch (!_reverseWindingOrder ? neighborIndex : (6 - neighborIndex) % 6)
			{
				case 0: wrap =
					(WrapsOnSidePosRow(faceIndex) && !IsBackIndentedLower(faceIndex) ? EdgeWrap.NegVertToEdgeAxis0 : EdgeWrap.None);
					break;
				case 1: wrap =
					(WrapsOnSidePosCol(faceIndex) ? EdgeWrap.PosEdgeToVertAxis1 : EdgeWrap.None);
					break;
				case 2: wrap =
					(WrapsOnSidePosCol(faceIndex) ? EdgeWrap.PosFaceToEdgeAxis1 : EdgeWrap.None) |
					(WrapsOnSidePosRow(faceIndex) && !IsBackIndentedUpper(faceIndex) ? EdgeWrap.PosEdgeToVertAxis0 : EdgeWrap.None);
					break;
				case 3: wrap =
					(WrapsOnSidePosCol(faceIndex) ? EdgeWrap.PosFaceToEdgeAxis1 : EdgeWrap.None) |
					(WrapsOnSidePosRow(faceIndex) && !IsBackIndentedUpper(faceIndex) ? EdgeWrap.PosFaceToEdgeAxis0 : EdgeWrap.None) |
					(WrapsOnSidePosRow(faceIndex) && IsBackIndentedUpper(faceIndex) ? EdgeWrap.PosEdgeToVertAxis0 : EdgeWrap.None);
					break;
				case 4: wrap =
					(WrapsOnSidePosRow(faceIndex) ? EdgeWrap.PosFaceToEdgeAxis0 : EdgeWrap.None) |
					(WrapsOnSidePosCol(faceIndex) ? EdgeWrap.NegVertToEdgeAxis1 : EdgeWrap.None);
					break;
				case 5: wrap =
					(WrapsOnSidePosRow(faceIndex) && !IsBackIndentedLower(faceIndex) ? EdgeWrap.PosFaceToEdgeAxis0 : EdgeWrap.None) |
					(WrapsOnSidePosRow(faceIndex) && IsBackIndentedLower(faceIndex) ? EdgeWrap.NegVertToEdgeAxis0 : EdgeWrap.None);
					break;
				default:
					throw new ArgumentOutOfRangeException("neighborIndex");
			}

			if (_reverseWindingOrder)
			{
				wrap = EdgeWrapUtility.InvertVertexEdgeRelations(wrap);
			}

			if (_reverseColumnsRows)
			{
				wrap = EdgeWrapUtility.SwapAxes(wrap);
			}

			return wrap;
		}

		#endregion

		#region IFaceIndexer2D Methods

		private void GetFaceColRow(int internalFaceIndex, out int col, out int row)
		{
			if (!variableRowLength)
			{
				col = internalFaceIndex % _faceColumnCount;
				row = internalFaceIndex / _faceColumnCount;
			}
			else
			{
				var adjustedIndex = internalFaceIndex * 2 + _faceOffset;
				col = (adjustedIndex % _faceColumnCountPair) / 2;
				row = adjustedIndex / _faceColumnCountPair;
			}
		}

		public Index2D GetFaceIndex2D(int internalFaceIndex)
		{
			Index2D index2D;
			if (!_reverseColumnsRows)
			{
				GetFaceColRow(internalFaceIndex, out index2D.x, out index2D.y);
			}
			else
			{
				GetFaceColRow(internalFaceIndex, out index2D.y, out index2D.x);
			}
			return index2D;
		}

		public int GetFaceIndex(int x, int y)
		{
			throw new NotImplementedException();
		}

		public int FaceWrapX(int x)
		{
			throw new NotImplementedException();
		}

		public int FaceWrapY(int y)
		{
			throw new NotImplementedException();
		}

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

		#endregion

		#region IVertexIndexer2D Methods

		private void GetVertexColRow(int vertexIndex, out int col, out int row)
		{
			if (vertexIndex < _vertexColumnCountFirst)
			{
				col = vertexIndex - _vertexInterceptOffsetFirst;
				row = 0;
			}
			else if (vertexIndex < _vertexCount - _vertexColumnCountLast)
			{
				row = (vertexIndex - _vertexColumnCountFirst) / _vertexColumnCount + 1;
				col = vertexIndex - (_vertexColumnCount * row + (MathUtility.IsEven(row) ? _vertexInterceptOffsetEven : _vertexInterceptOffsetOdd));
			}
			else
			{
				row = _vertexRowCount - 1;
				col = vertexIndex - (_vertexColumnCount * row + _vertexInterceptOffsetLast);
			}
		}

		public Index2D GetVertexIndex2D(int vertexIndex)
		{
			Index2D index2D;
			if (!_reverseColumnsRows)
			{
				GetVertexColRow(vertexIndex, out index2D.x, out index2D.y);
			}
			else
			{
				GetVertexColRow(vertexIndex, out index2D.y, out index2D.x);
			}
			return index2D;
		}

		public int GetVertexIndex(int x, int y)
		{
			throw new NotImplementedException();
		}

		public int VertexWrapX(int x)
		{
			throw new NotImplementedException();
		}

		public int VertexWrapY(int y)
		{
			throw new NotImplementedException();
		}

		public Topology.Vertex GetVertex(Index2D index) { return topology.vertices[GetVertexIndex(index.x, index.y)]; }
		public Topology.Vertex GetVertex(int x, int y) { return topology.vertices[GetVertexIndex(x, y)]; }
		public int GetVertexIndex(Index2D index) { return GetVertexIndex(index.x, index.y); }

		public Index2D GetVertexIndex2D(Topology.Vertex vertex) { return GetVertexIndex2D(vertex.index); }

		public Index2D VertexWrap(int x, int y) { return new Index2D(VertexWrapX(x), VertexWrapY(y)); }
		public Index2D VertexWrap(Index2D index) { return new Index2D(VertexWrapX(index.x), VertexWrapY(index.y)); }

		public Topology.Vertex GetWrappedVertex(Index2D index) { return GetVertex(VertexWrap(index)); }
		public Topology.Vertex GetWrappedVertex(int x, int y) { return GetVertex(VertexWrap(x, y)); }
		public int GetWrappedVertexIndex(Index2D index) { return GetVertexIndex(VertexWrap(index)); }
		public int GetWrappedVertexIndex(int x, int y) { return GetVertexIndex(VertexWrap(x, y)); }

		#endregion

		#region Private Helper Methods

		private int ConditionalInvert(int neighborIndex, int neighborCount, bool invert)
		{
			return invert ? neighborCount - neighborIndex - 1 : neighborIndex;
		}

		private bool IsInEvenRow(int faceIndex)
		{
			return faceIndex % _faceColumnCountPair < _faceColumnCountEven;
		}

		private bool IsInOddRow(int faceIndex)
		{
			return faceIndex % _faceColumnCountPair >= _faceColumnCountEven;
		}

		private bool IsFrontIndentedLower(int faceIndex)
		{
			return IsInEvenRow(faceIndex) ? _vertexRowEvenIndentNextFront : _vertexRowOddIndentNextFront;
		}

		private bool IsFrontIndentedUpper(int faceIndex)
		{
			return IsInEvenRow(faceIndex) ? !_vertexRowOddIndentNextFront : !_vertexRowEvenIndentNextFront;
		}

		private bool IsBackIndentedLower(int faceIndex)
		{
			return IsInEvenRow(faceIndex) ? _vertexRowEvenIndentNextBack : _vertexRowOddIndentNextBack;
		}

		private bool IsBackIndentedUpper(int faceIndex)
		{
			return IsInEvenRow(faceIndex) ? !_vertexRowOddIndentNextBack : !_vertexRowEvenIndentNextBack;
		}

		private int GetLowerVertexIndex(int col, int row, int offset)
		{
			return _vertexColumnCount * row + (col * 2 + GetFirstLowerVertexOffset(row) + offset) % _vertexColumnCount;
		}

		private int GetUpperVertexIndex(int col, int row, int offset)
		{
			return _vertexColumnCount * (row + 1) % _vertexCount + (col * 2 + GetFirstUpperVertexOffset(row) + offset) % _vertexColumnCount;
		}

		private int GetFirstLowerVertexOffset(int row)
		{
			if (row == 0)
			{
				return _vertexLowerOffsetFirst;
			}
			else if (MathUtility.IsEven(row))
			{
				return _vertexLowerOffsetEven;
			}
			else
			{
				return _vertexLowerOffsetOdd;
			}
		}

		private int GetFirstUpperVertexOffset(int row)
		{
			if (row == _faceRowCount - 1)
			{
				return _vertexUpperOffsetLast;
			}
			else if (MathUtility.IsEven(row))
			{
				return _vertexUpperOffsetEven;
			}
			else
			{
				return _vertexUpperOffsetOdd;
			}
		}

		private bool IsOnSideNegCol(int internalFaceIndex)
		{
			return internalFaceIndex < _faceColumnCountFirst;
		}

		private bool IsOnSidePosCol(int internalFaceIndex)
		{
			return internalFaceIndex >= _internalFaceCount - _faceColumnCountLast;
		}

		private bool IsOnSideNegRow(int internalFaceIndex)
		{
			return (internalFaceIndex * 2 + _faceOffset) % _faceColumnCountPair <= 1;
		}

		private bool IsOnSidePosRow(int internalFaceIndex)
		{
			return (internalFaceIndex * 2 + _faceOffset) % _faceColumnCountPair + 2 >= _faceColumnCountPair;
		}

		private bool ExitsOnSideNegCol(int internalFaceIndex)
		{
			return !_wrapCols && IsOnSideNegCol(internalFaceIndex);
		}

		private bool ExitsOnSidePosCol(int internalFaceIndex)
		{
			return !_wrapCols && IsOnSidePosCol(internalFaceIndex);
		}

		private bool ExitsOnSideNegRow(int internalFaceIndex)
		{
			return !_wrapRows && IsOnSideNegRow(internalFaceIndex);
		}

		private bool ExitsOnSidePosRow(int internalFaceIndex)
		{
			return !_wrapRows && IsOnSidePosRow(internalFaceIndex);
		}

		private bool WrapsOnSideNegCol(int internalFaceIndex)
		{
			return _wrapCols && IsOnSideNegCol(internalFaceIndex);
		}

		private bool WrapsOnSidePosCol(int internalFaceIndex)
		{
			return _wrapCols && IsOnSidePosCol(internalFaceIndex);
		}

		private bool WrapsOnSideNegRow(int internalFaceIndex)
		{
			return _wrapRows && IsOnSideNegRow(internalFaceIndex);
		}

		private bool WrapsOnSidePosRow(int internalFaceIndex)
		{
			return _wrapRows && IsOnSidePosRow(internalFaceIndex);
		}

		#endregion
	}
}
