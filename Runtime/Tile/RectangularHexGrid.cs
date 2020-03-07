/******************************************************************************\
* Copyright Andy Gainey                                                        *
*                                                                              *
* Licensed under the Apache License, Version 2.0 (the "License");              *
* you may not use this file except in compliance with the License.             *
* You may obtain a copy of the License at                                      *
*                                                                              *
*     http://www.apache.org/licenses/LICENSE-2.0                               *
*                                                                              *
* Unless required by applicable law or agreed to in writing, software          *
* distributed under the License is distributed on an "AS IS" BASIS,            *
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.     *
* See the License for the specific language governing permissions and          *
* limitations under the License.                                               *
\******************************************************************************/

using UnityEngine;
using System;
using MakeIt.Numerics;
using GeneralUtility = MakeIt.Core.GeneralUtility;

namespace MakeIt.Tile
{
	/// <summary>
	/// A surface with a grid of discrete hexagonal tiles.
	/// </summary>
	public class RectangularHexGrid : QuadrilateralSurface, IFaceNeighborIndexer, IFaceIndexer2D, IVertexIndexer2D
	{
		/// <summary>
		/// The vector from the center of each hexagonal tile to the midpoint of one of the edges.
		/// </summary>
		public Vector2 midpoint;

		/// <summary>
		/// The vector from the center of each hexagonal tile to the corner one and a half edges away from the defined edge midpoint.
		/// </summary>
		public Vector2 majorCorner;

		/// <summary>
		/// The vector from the center of each hexagonal tile to one of the major corner's neighboring corners.
		/// </summary>
		public Vector2 minorCorner;

		/// <summary>
		/// Indicates if the midpoint vector is to be used as the first axis, or is instead the second axis.
		/// </summary>
		public bool midpointIsFirstAxis;

		/// <summary>
		/// The styles with which the axes of the hexagonal grid are arranged.
		/// </summary>
		public HexGridAxisStyles axisStyle;

		/// <summary>
		/// The size of the grid, in terms of the number of tiles along the first and second axes of the surface.
		/// </summary>
		public IntVector2 size;

		/// <summary>
		/// The topology defining the vertices, edges, and faces of the grid.
		/// </summary>
		/// <remarks><note type="important">This field is not automatically set, and must be filled in manually.
		/// If not set, most surface functions will still work, but any that return topology objects such as
		/// <see cref="Topology.Vertex"/> or <see cref="Topology.Face"/> will fail.</note></remarks>
		public Topology topology;

		[SerializeField] private bool _originIsObtuse;

		[SerializeField] private Vector2 _faceAxis0;
		[SerializeField] private Vector2 _faceAxis1;

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

		[SerializeField] private bool _isInverted;

		/// <summary>
		/// Creates a hexagonally tiled grid surface instance with the given tile shape and arrangement, origin, orientation, wrapping behavior, and grid size.
		/// </summary>
		/// <param name="hexDescriptor">The descriptor structure with details about the shape and arrangement of hexagonal tiles.</param>
		/// <param name="origin">The origin of the plane.</param>
		/// <param name="orientation">The orientation of the plane.</param>
		/// <param name="isAxis0Wrapped">Indicates whether the first axis exhibits wrap-around behavior at the grid boundaries.</param>
		/// <param name="isAxis1Wrapped">Indicates whether the second axis exhibits wrap-around behavior at the grid boundaries.</param>
		/// <param name="size">The size of the grid, in terms of the number of tiles along the first and second axes of the surface..</param>
		/// <returns>A hexagonally tiled grid surface.</returns>
		public static RectangularHexGrid Create(HexGridDescriptor hexDescriptor, Vector3 origin, Quaternion orientation, bool isAxis0Wrapped, bool isAxis1Wrapped, IntVector2 size)
		{
			return CreateInstance<RectangularHexGrid>().Reset(hexDescriptor, origin, orientation, isAxis0Wrapped, isAxis1Wrapped, size);
		}

		/// <summary>
		/// Resets the current grid surface with new values for the tile shape and arrangement, origin, orientation, wrapping behavior, and grid size.
		/// </summary>
		/// <param name="hexDescriptor">The descriptor structure with details about the shape and arrangement of hexagonal tiles.</param>
		/// <param name="origin">The origin of the plane.</param>
		/// <param name="orientation">The orientation of the plane.</param>
		/// <param name="isAxis0Wrapped">Indicates whether the first axis exhibits wrap-around behavior at the grid boundaries.</param>
		/// <param name="isAxis1Wrapped">Indicates whether the second axis exhibits wrap-around behavior at the grid boundaries.</param>
		/// <param name="size">The size of the grid, in terms of the number of tiles along the first and second axes of the surface..</param>
		/// <returns>A reference to the current surface.</returns>
		public RectangularHexGrid Reset(HexGridDescriptor hexDescriptor, Vector3 origin, Quaternion orientation, bool isAxis0Wrapped, bool isAxis1Wrapped, IntVector2 size)
		{
			midpoint = hexDescriptor.midpoint;
			majorCorner = hexDescriptor.majorCorner;
			minorCorner = hexDescriptor.minorCorner;

			midpointIsFirstAxis = hexDescriptor.midpointIsFirstAxis;
			axisStyle = hexDescriptor.axisStyle;

			_originIsObtuse = Vector2.Dot(midpoint, minorCorner) < Vector2.Dot(midpoint, majorCorner);

			if (axisStyle == HexGridAxisStyles.Straight)
			{
				_faceAxis0 = midpoint * 2f;
				_faceAxis1 = majorCorner + minorCorner;
			}
			else
			{
				_faceAxis0 = midpoint * 2f;

				if (_originIsObtuse)
				{
					_faceAxis1 = majorCorner + minorCorner + midpoint;
				}
				else
				{
					_faceAxis1 = majorCorner + minorCorner - midpoint;
				}
			}

			if (!midpointIsFirstAxis)
			{
				GeneralUtility.Swap(ref _faceAxis0, ref _faceAxis1);
			}

			Reset(new WrappableAxis2(_faceAxis0 * size.x, isAxis0Wrapped), new WrappableAxis2(_faceAxis1 * size.y, isAxis1Wrapped), origin, orientation);

			this.size = size;
			topology = null;
			Initialize();
			return this;
		}

		private void Initialize()
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

			if (axisStyle != HexGridAxisStyles.Straight && (axis0.isWrapped && !midpointIsFirstAxis && (size.x % 2) != 0 || axis1.isWrapped && midpointIsFirstAxis && (size.y % 2) != 0))
				throw new InvalidTopologyException("A rectangular hexagonal grid cannot have a wrapped staggered axis with an odd size.");
			if (axisStyle == HexGridAxisStyles.StaggeredSymmetric && (axis0.isWrapped && midpointIsFirstAxis || axis1.isWrapped && !midpointIsFirstAxis))
				throw new InvalidTopologyException("A rectangular hexagonal grid cannot have a wrapped straight symmetric axis when the other axis is staggered.");

			if (Vector3.Cross(axis0.vector, axis1.vector).sqrMagnitude < 0.001f)
				throw new InvalidTopologyException("A planar surface cannot have nearly colinear axes.");

			if (Mathf.Abs(Geometry.SinMagnitude(midpoint, majorCorner)) < 0.001f ||
				Mathf.Abs(Geometry.SinMagnitude(midpoint, minorCorner)) < 0.001f ||
				Mathf.Abs(Geometry.SinMagnitude(minorCorner, majorCorner)) < 0.001f)
				throw new InvalidTopologyException("A hexagonal grid cell shape cannot have nearly colinear defining vectors.");
		}

		private void InitializeCoreMetrics()
		{
			if (midpointIsFirstAxis)
			{
				_isInverted = Vector3.Dot(Vector3.Cross(majorCorner, midpoint), Vector3.back) < 0f;

				_faceColumnCount = size.x;
				_faceRowCount = size.y;

				_wrapRows = axis0.isWrapped;
				_wrapCols = axis1.isWrapped;

				_reverseColumnsRows = false;
				_reverseWindingOrder = _isInverted;
			}
			else
			{
				_isInverted = Vector3.Dot(Vector3.Cross(midpoint, majorCorner), Vector3.back) < 0f;

				_faceColumnCount = size.y;
				_faceRowCount = size.x;

				_wrapRows = axis1.isWrapped;
				_wrapCols = axis0.isWrapped;

				_reverseColumnsRows = true;
				_reverseWindingOrder = !_isInverted;
			}
		}

		private void InitializeIndentation()
		{
			_vertexRowEvenIndentNextFront = false;
			_vertexRowOddIndentNextFront = false;

			if (axisStyle == HexGridAxisStyles.Straight)
			{
				if (!_originIsObtuse)
				{
					_vertexRowEvenIndentNextFront = true;
					_vertexRowOddIndentNextFront = true;
				}
				else
				{
					_vertexRowEvenIndentNextFront = false;
					_vertexRowOddIndentNextFront = false;
				}
			}
			else
			{
				if (!_originIsObtuse)
				{
					_vertexRowEvenIndentNextFront = false;
					_vertexRowOddIndentNextFront = true;
				}
				else
				{
					_vertexRowEvenIndentNextFront = true;
					_vertexRowOddIndentNextFront = false;
				}
			}

			_vertexRowFirstIndentNextFront = _vertexRowEvenIndentNextFront;
			_vertexRowLastIndentNextFront = Numerics.Math.IsEven(_faceRowCount) ? _vertexRowEvenIndentNextFront : _vertexRowOddIndentNextFront;

			// If row lengths are variable so that there's a symmetry on both ends of the rows, then
			// the indentation on the back end is identical to that on the front end.
			if (axisStyle == HexGridAxisStyles.StaggeredSymmetric)
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
			_faceColumnCountEven = (axisStyle == HexGridAxisStyles.StaggeredSymmetric && _vertexRowEvenIndentNextFront && !_vertexRowOddIndentNextFront) ? _faceColumnCount - 1 : _faceColumnCount;
			_faceColumnCountOdd = (axisStyle == HexGridAxisStyles.StaggeredSymmetric && _vertexRowOddIndentNextFront && !_vertexRowEvenIndentNextFront) ? _faceColumnCount - 1 : _faceColumnCount;
			_faceColumnCountFirst = _faceColumnCountEven;
			_faceColumnCountLast = Numerics.Math.IsEven(_faceRowCount) ? _faceColumnCountOdd : _faceColumnCountEven;
			_faceColumnCountPair = _faceColumnCountEven + _faceColumnCountOdd;

			_faceOffset = _faceColumnCount - _faceColumnCountEven;

			_internalFaceCount = _faceColumnCountPair * (_faceRowCount / 2) + (Numerics.Math.IsEven(_faceRowCount) ? 0 : _faceColumnCountLast);

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
			if (axisStyle == HexGridAxisStyles.StaggeredSymmetric)
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
			_vertexUpperOffsetLast = Numerics.Math.IsEven(_faceRowCount) ? _vertexUpperOffsetOdd : _vertexUpperOffsetEven;
			_vertexInterceptOffsetLast = Numerics.Math.IsEven(_vertexRowCount) ? _vertexInterceptOffsetOdd : _vertexInterceptOffsetEven;

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
				_edgeCount = _internalFaceCount * 6 + _faceColumnCount * 4;
			}
			else if (_wrapCols)
			{
				_edgeCount = _internalFaceCount * 6 + _faceRowCount * 4;
			}
			else
			{
				_edgeCount = _internalFaceCount * 6 + _faceRowCount * 4 + _faceColumnCountFirst * 2 + _faceColumnCountLast * 2 - 2;
			}
		}

		/// <summary>
		/// The number of topology vertices defined by the grid.
		/// </summary>
		public int vertexCount { get { return _vertexCount; } }

		/// <summary>
		/// The number of topology edges defined by the grid.
		/// </summary>
		public int edgeCount { get { return _edgeCount; } }

		/// <summary>
		/// The number of topology faces defined by the grid.
		/// </summary>
		public int faceCount { get { return _internalFaceCount + _externalFaceCount; } }

		/// <summary>
		/// The number of internal topology faces defined by the grid.
		/// </summary>
		public int internalFaceCount { get { return _internalFaceCount; } }

		/// <summary>
		/// The number of external topology faces defined by the grid.
		/// </summary>
		public int externalFaceCount { get { return _externalFaceCount; } }

		/// <summary>
		/// Creates a topology from the grid specifications of the current surface.
		/// </summary>
		/// <returns>A topology corresponding to the grid specifications of the surface.</returns>
		public Topology CreateTopology()
		{
			return TopologyUtility.BuildTopology(this);
		}

		/// <summary>
		/// Creates a manifold, consisting of a topology plus vertex positions, from the grid specifications of the current surface.
		/// </summary>
		/// <param name="vertexPositions">The vertex positions of the manifold corresponding to the grid specifications of the surface.</param>
		/// <returns>A topology corresponding to the grid specifications of the surface.</returns>
		public Topology CreateManifold(out Vector3[] vertexPositions)
		{
			vertexPositions = new Vector3[vertexCount];

			Vector3 rowAxis;
			Vector3 columnAxisDouble;

			Vector3 evenCorner = _originIsObtuse ? -minorCorner - midpoint  * 2f : -minorCorner;
			Vector3 oddCorner = -majorCorner;
			Vector3 oddRowOffset;

			if (axisStyle == HexGridAxisStyles.Straight)
			{
				rowAxis = midpoint * 2f;
				columnAxisDouble = (majorCorner + minorCorner) * 2f;
				oddRowOffset = majorCorner + minorCorner;
			}
			else
			{
				rowAxis = midpoint * 2f;

				if (_originIsObtuse)
				{
					columnAxisDouble = (majorCorner + minorCorner + midpoint) * 2f;
					oddRowOffset = majorCorner + minorCorner;
				}
				else
				{
					columnAxisDouble = (majorCorner + minorCorner - midpoint) * 2f;
					oddRowOffset = majorCorner + minorCorner;
				}
			}

			rowAxis = orientation * rowAxis;
			columnAxisDouble = orientation * columnAxisDouble;
			oddCorner = orientation * oddCorner;
			evenCorner = orientation * evenCorner;
			oddRowOffset = orientation * oddRowOffset;

			for (int i = 0; i < vertexCount; ++i)
			{
				int col, row;
				GetVertexColRow(i, out col, out row);
				var vertexPosition = origin + (col >> 1) * rowAxis + (row >> 1) * columnAxisDouble;
				if (Numerics.Math.IsOdd(col)) vertexPosition += oddCorner - evenCorner;
				if (Numerics.Math.IsOdd(row)) vertexPosition += oddRowOffset;
				vertexPositions[i] = vertexPosition;
			}

			return CreateTopology();
		}

		#region IFaceNeighborIndexer Methods

		/// <inheritdoc/>
		public ushort GetNeighborCount(int faceIndex)
		{
			if (faceIndex < 0 || faceIndex >= _internalFaceCount) throw new ArgumentOutOfRangeException("faceIndex");

			return 6;
		}

		/// <inheritdoc/>
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

		/// <inheritdoc/>
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
			if (axisStyle != HexGridAxisStyles.StaggeredSymmetric)
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

		private int GetFaceIndexFromColRow(int col, int row)
		{
			if (axisStyle != HexGridAxisStyles.StaggeredSymmetric)
			{
				return col + row * _faceColumnCount;
			}
			else
			{
				var adjustedIndex = col * 2 + row * _faceColumnCountPair;
				return (adjustedIndex - _faceOffset) / 2;
			}
		}

		/// <inheritdoc/>
		public IntVector2 GetFaceIndex2D(int internalFaceIndex)
		{
			IntVector2 index2D;
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

		/// <inheritdoc/>
		public int GetFaceIndex(int x, int y)
		{
			if (!_reverseColumnsRows)
			{
				return GetFaceIndexFromColRow(x, y);
			}
			else
			{
				return GetFaceIndexFromColRow(y, x);
			}
		}

		/// <inheritdoc/>
		public int FaceWrapX(int x)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public int FaceWrapY(int y)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public Topology.Face GetFace(IntVector2 index) { return topology.faces[GetFaceIndex(index.x, index.y)]; }
		/// <inheritdoc/>
		public Topology.Face GetFace(int x, int y) { return topology.faces[GetFaceIndex(x, y)]; }
		/// <inheritdoc/>
		public int GetFaceIndex(IntVector2 index) { return GetFaceIndex(index.x, index.y); }

		/// <inheritdoc/>
		public IntVector2 GetFaceIndex2D(Topology.Face internalFace) { return GetFaceIndex2D(internalFace.index); }

		/// <inheritdoc/>
		public IntVector2 FaceWrap(int x, int y) { return new IntVector2(FaceWrapX(x), FaceWrapY(y)); }
		/// <inheritdoc/>
		public IntVector2 FaceWrap(IntVector2 index) { return new IntVector2(FaceWrapX(index.x), FaceWrapY(index.y)); }

		/// <inheritdoc/>
		public Topology.Face GetWrappedFace(IntVector2 index) { return GetFace(FaceWrap(index)); }
		/// <inheritdoc/>
		public Topology.Face GetWrappedFace(int x, int y) { return GetFace(FaceWrap(x, y)); }
		/// <inheritdoc/>
		public int GetWrappedFaceIndex(IntVector2 index) { return GetFaceIndex(FaceWrap(index)); }
		/// <inheritdoc/>
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
				col = vertexIndex - (_vertexColumnCount * row + (Numerics.Math.IsEven(row) ? _vertexInterceptOffsetEven : _vertexInterceptOffsetOdd));
			}
			else
			{
				row = _vertexRowCount - 1;
				col = vertexIndex - (_vertexColumnCount * row + _vertexInterceptOffsetLast);
			}
		}

		private int GetVertexIndexFromColRow(int col, int row)
		{
			if (row == 0)
			{
				return col + _vertexInterceptOffsetFirst;
			}
			else if (row == _vertexRowCount - 1)
			{
				return col + (_vertexColumnCount * row + _vertexInterceptOffsetLast);
			}
			else
			{
				return col + (_vertexColumnCount * row + (Numerics.Math.IsEven(row) ? _vertexInterceptOffsetEven : _vertexInterceptOffsetOdd));
			}
		}

		/// <inheritdoc/>
		public IntVector2 GetVertexIndex2D(int vertexIndex)
		{
			IntVector2 index2D;
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

		/// <inheritdoc/>
		public int GetVertexIndex(int x, int y)
		{
			if (!_reverseColumnsRows)
			{
				return GetVertexIndexFromColRow(x, y);
			}
			else
			{
				return GetVertexIndexFromColRow(y, x);
			}
		}

		/// <inheritdoc/>
		public int VertexWrapX(int x)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public int VertexWrapY(int y)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public Topology.Vertex GetVertex(IntVector2 index) { return topology.vertices[GetVertexIndex(index.x, index.y)]; }
		/// <inheritdoc/>
		public Topology.Vertex GetVertex(int x, int y) { return topology.vertices[GetVertexIndex(x, y)]; }
		/// <inheritdoc/>
		public int GetVertexIndex(IntVector2 index) { return GetVertexIndex(index.x, index.y); }

		/// <inheritdoc/>
		public IntVector2 GetVertexIndex2D(Topology.Vertex vertex) { return GetVertexIndex2D(vertex.index); }

		/// <inheritdoc/>
		public IntVector2 VertexWrap(int x, int y) { return new IntVector2(VertexWrapX(x), VertexWrapY(y)); }
		/// <inheritdoc/>
		public IntVector2 VertexWrap(IntVector2 index) { return new IntVector2(VertexWrapX(index.x), VertexWrapY(index.y)); }

		/// <inheritdoc/>
		public Topology.Vertex GetWrappedVertex(IntVector2 index) { return GetVertex(VertexWrap(index)); }
		/// <inheritdoc/>
		public Topology.Vertex GetWrappedVertex(int x, int y) { return GetVertex(VertexWrap(x, y)); }
		/// <inheritdoc/>
		public int GetWrappedVertexIndex(IntVector2 index) { return GetVertexIndex(VertexWrap(index)); }
		/// <inheritdoc/>
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
			else if (Numerics.Math.IsEven(row))
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
			else if (Numerics.Math.IsEven(row))
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

	/// <summary>
	/// The styles with which the axes of a hexagonal grid can be arranged.
	/// </summary>
	public enum HexGridAxisStyles
	{
		/// <summary>
		/// Both axes follow a straight sequence of hexagonal tiles.  This leads to a skewed world if regular hexagons are used.
		/// </summary>
		Straight,
		/// <summary>
		/// One axis follows a staggered sequence of hexagonal tiles.  This allows for a rectangular world if regular hexagons are used.
		/// </summary>
		Staggered,
		/// <summary>
		/// One axis follws a staggered sequence of hexagonal tiles, and the number of tiles on the other axis alternates so that the staggered axis exhibits mirror symmetry.
		/// </summary>
		StaggeredSymmetric,
	}

	/// <summary>
	/// A struct for describing the shape and arrangement of hexagonal tiles.
	/// </summary>
	public struct HexGridDescriptor
	{
		/// <summary>
		/// The vector from the center of each hexagonal tile to the midpoint of one of the edges.
		/// </summary>
		public Vector2 midpoint;

		/// <summary>
		/// The vector from the center of each hexagonal tile to the corner one and a half edges away from the defined edge midpoint.
		/// </summary>
		public Vector2 majorCorner;

		/// <summary>
		/// The vector from the center of each hexagonal tile to one of the major corner's neighboring corners.
		/// </summary>
		public Vector2 minorCorner;

		/// <summary>
		/// Indicates if the midpoint vector is to be used as the first axis, or is instead the second axis.
		/// </summary>
		public bool midpointIsFirstAxis;

		/// <summary>
		/// The styles with which the axes of the hexagonal grid should be arranged.
		/// </summary>
		public HexGridAxisStyles axisStyle;

		/// <summary>
		/// Creates a descriptor of the shape and arrangment of hexagonal tiles.
		/// </summary>
		/// <param name="midpoint">The vector from the center of each hexagonal tile to the midpoint of one of the edges.</param>
		/// <param name="majorCorner">The vector from the center of each hexagonal tile to the corner one and a half edges away from the defined edge midpoint.</param>
		/// <param name="minorCorner">The vector from the center of each hexagonal tile to one of the major corner's neighboring corners</param>
		/// <param name="midpointIsFirstAxis">Indicates if the midpoint vector is to be used as the first axis, or is instead the second axis.</param>
		/// <param name="axisStyle">The styles with which the axes of the hexagonal grid should be arranged.</param>
		/// <returns>A hexagonal tile descriptor.</returns>
		public HexGridDescriptor(Vector2 midpoint, Vector2 majorCorner, Vector2 minorCorner, bool midpointIsFirstAxis, HexGridAxisStyles axisStyle)
		{
			this.midpoint = midpoint;
			this.majorCorner = majorCorner;
			this.minorCorner = minorCorner;
			this.midpointIsFirstAxis = midpointIsFirstAxis;
			this.axisStyle = axisStyle;
		}

		private const float _oneOverSqrtThree = 0.5773502692f;
		private const float _halfOverSqrtThree = 0.2886751346f;

		/// <summary>
		/// A standard descriptor for regular hexagonal tiles with a "pointy side up" orientation.
		/// </summary>
		public static HexGridDescriptor standardCornerUp { get { return CreateCornerUp(true, HexGridAxisStyles.Staggered); } }

		/// <summary>
		/// A standard descriptor for regular hexagonal tiles with a "flat side up" orientation.
		/// </summary>
		public static HexGridDescriptor standardSideUp { get { return CreateSideUp(true, HexGridAxisStyles.Staggered); } }

		/// <summary>
		/// Creates a descriptorfor regular hexagonal tiles with a "pointy side up" orientation, and the given additional arrangement details.
		/// </summary>
		/// <param name="originIsObtuse">Indicates if second tile along the first axis and the second tile along the second axis form an obtuse or acute angle relative to the first tile.</param>
		/// <param name="axisStyle">The styles with which the axes of the hexagonal grid should be arranged.</param>
		/// <returns>A hexagonal tile descriptor.</returns>
		public static HexGridDescriptor CreateCornerUp(bool originIsObtuse, HexGridAxisStyles axisStyle)
		{
			return new HexGridDescriptor(
				new Vector2(0.5f, 0f),
				new Vector2(0f, _oneOverSqrtThree),
				new Vector2(originIsObtuse ? -0.5f : 0.5f, _halfOverSqrtThree),
				true, axisStyle);
		}

		/// <summary>
		/// Creates a descriptorfor regular hexagonal tiles with a "flat side up" orientation, and the given additional arrangement details.
		/// </summary>
		/// <param name="originIsObtuse">Indicates if second tile along the first axis and the second tile along the second axis form an obtuse or acute angle relative to the first tile.</param>
		/// <param name="axisStyle">The styles with which the axes of the hexagonal grid should be arranged.</param>
		/// <returns>A hexagonal tile descriptor.</returns>
		public static HexGridDescriptor CreateSideUp(bool originIsObtuse, HexGridAxisStyles axisStyle)
		{
			return new HexGridDescriptor(
				new Vector2(0f, 0.5f),
				new Vector2(_oneOverSqrtThree, 0f),
				new Vector2(_halfOverSqrtThree, originIsObtuse ? -0.5f : 0.5f),
				false, axisStyle);
		}
	}
}
