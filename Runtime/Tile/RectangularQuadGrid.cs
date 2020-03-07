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

namespace MakeIt.Tile
{
	/// <summary>
	/// A surface with a grid of discrete quadrilateral tiles.
	/// </summary>
	public class RectangularQuadGrid : QuadrilateralSurface, IFaceNeighborIndexer, IFaceIndexer2D, IVertexIndexer2D
	{
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

		[SerializeField] private int _faceAxis0Count;
		[SerializeField] private int _faceAxis1Count;

		[SerializeField] private int _vertexAxis0Count;
		[SerializeField] private int _vertexAxis1Count;

		[SerializeField] private int _vertexCount;
		[SerializeField] private int _edgeCount;
		[SerializeField] private int _internalFaceCount;
		[SerializeField] private int _externalFaceCount;

		[SerializeField] private bool _isInverted;

		/// <summary>
		/// Creates a quadrilaterally tiled grid surface instance with the given axes, origin, orientation, wrapping behavior, and grid size.
		/// </summary>
		/// <param name="tileAxis0">The first axis, with a length corresponding to the size of a single tile.</param>
		/// <param name="tileAxis1">The second axis, with a length corresponding to the size of a single tile.</param>
		/// <param name="origin">The origin of the plane.</param>
		/// <param name="orientation">The orientation of the plane.</param>
		/// <param name="isAxis0Wrapped">Indicates whether the first axis exhibits wrap-around behavior at the grid boundaries.</param>
		/// <param name="isAxis1Wrapped">Indicates whether the second axis exhibits wrap-around behavior at the grid boundaries.</param>
		/// <param name="size">The size of the grid, in terms of the number of tiles along the first and second axes of the surface..</param>
		/// <returns>A quadrilaterally tiled grid surface.</returns>
		public static RectangularQuadGrid Create(Vector2 tileAxis0, Vector2 tileAxis1, Vector3 origin, Quaternion orientation, bool isAxis0Wrapped, bool isAxis1Wrapped, IntVector2 size)
		{
			return CreateInstance<RectangularQuadGrid>().Reset(tileAxis0, tileAxis1, origin, orientation, isAxis0Wrapped, isAxis1Wrapped, size);
		}

		/// <summary>
		/// Resets the current grid surface with new values for the axes, origin, orientation, wrapping behavior, and grid size.
		/// </summary>
		/// <param name="tileAxis0">The first axis, with a length corresponding to the size of a single tile.</param>
		/// <param name="tileAxis1">The second axis, with a length corresponding to the size of a single tile.</param>
		/// <param name="origin">The origin of the plane.</param>
		/// <param name="orientation">The orientation of the plane.</param>
		/// <param name="isAxis0Wrapped">Indicates whether the first axis exhibits wrap-around behavior at the grid boundaries.</param>
		/// <param name="isAxis1Wrapped">Indicates whether the second axis exhibits wrap-around behavior at the grid boundaries.</param>
		/// <param name="size">The size of the grid, in terms of the number of tiles along the first and second axes of the surface..</param>
		/// <returns>A reference to the current surface.</returns>
		public RectangularQuadGrid Reset(Vector2 tileAxis0, Vector2 tileAxis1, Vector3 origin, Quaternion orientation, bool isAxis0Wrapped, bool isAxis1Wrapped, IntVector2 size)
		{
			Reset(
				new WrappableAxis2(tileAxis0 * size.x, isAxis0Wrapped),
				new WrappableAxis2(tileAxis1 * size.y, isAxis1Wrapped),
				origin, orientation);
			this.size = size;
			topology = null;
			Initialize();
			return this;
		}

		private void Initialize()
		{
			if (size.x <= 0 || size.y <= 0)
				throw new InvalidTopologyException("A rectangular quad grid cannot have an axis size less than or equal to zero.");

			_faceAxis0Count = size.x;
			_faceAxis1Count = size.y;
			_vertexAxis0Count = _faceAxis0Count + (axis0.isWrapped ? 0 : 1);
			_vertexAxis1Count = _faceAxis1Count + (axis1.isWrapped ? 0 : 1);

			_vertexCount = _vertexAxis0Count * _vertexAxis1Count;
			_edgeCount = (_vertexAxis1Count * _faceAxis0Count + vertexAxis0Count * _faceAxis1Count) * 2;
			_internalFaceCount = _faceAxis0Count * _faceAxis1Count;
			_externalFaceCount = (axis0.isWrapped && axis1.isWrapped) ? 0 : (axis0.isWrapped || axis1.isWrapped) ? 2 : 1;

			_isInverted = Vector3.Dot(Vector3.Cross(axis1.vector, axis0.vector), Vector3.back) < 0f;
		}

		/// <summary>
		/// The number of topology faces along the first axis.
		/// </summary>
		public int faceAxis0Count { get { return _faceAxis0Count; } }

		/// <summary>
		/// The number of topology faces along the second axis.
		/// </summary>
		public int faceAxis1Count { get { return _faceAxis1Count; } }

		/// <summary>
		/// The number of topology vertices along the first axis.
		/// </summary>
		public int vertexAxis0Count { get { return _vertexAxis0Count; } }

		/// <summary>
		/// The number of topology vertices along the second axis.
		/// </summary>
		public int vertexAxis1Count { get { return _vertexAxis1Count; } }

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

			for (int i = 0; i < vertexCount; ++i)
			{
				var index2D = GetVertexIndex2D(i);
				vertexPositions[i] =
					origin +
					index2D.x * _axis0Vector / size.x +
					index2D.y * _axis1Vector / size.y;
			}

			return CreateTopology();
		}

		#region IFaceNeighborIndexer Methods

		/// <inheritdoc/>
		public ushort GetNeighborCount(int faceIndex)
		{
			if (faceIndex < 0 || faceIndex >= _internalFaceCount) throw new ArgumentOutOfRangeException("faceIndex");

			return 4;
		}

		/// <inheritdoc/>
		public int GetNeighborVertexIndex(int faceIndex, int neighborIndex)
		{
			if (faceIndex < 0 || faceIndex >= _internalFaceCount) throw new ArgumentOutOfRangeException("faceIndex");

			var index2D = GetFaceIndex2D(faceIndex);
			var basisIndex = index2D.y * _vertexAxis0Count;
			switch (ConditionalInvert(neighborIndex, 4, _isInverted))
			{
				case 0: return basisIndex + index2D.x;
				case 1: return (basisIndex + _vertexAxis0Count) % vertexCount + index2D.x;
				case 2: return (basisIndex + _vertexAxis0Count) % vertexCount + (index2D.x + 1) % _vertexAxis0Count;
				case 3: return basisIndex + (index2D.x + 1) % _vertexAxis0Count;
				default: throw new ArgumentOutOfRangeException("neighborIndex");
			}
		}

		/// <inheritdoc/>
		public EdgeWrap GetEdgeWrap(int faceIndex, int neighborIndex)
		{
			if (faceIndex < 0 || faceIndex >= _internalFaceCount) throw new ArgumentOutOfRangeException("faceIndex");

			if (!axis0.isWrapped && !axis1.isWrapped) return EdgeWrap.None;

			if (!_isInverted)
			{
				switch (neighborIndex)
				{
					case 0: return EdgeWrapUtility.FromEdgeRelations(
						(WrapsOnSidePosAxis0(faceIndex) ? EdgeWrap.NegVertToEdgeAxis0 : EdgeWrap.None) |
						(WrapsOnSideNegAxis1(faceIndex) ? EdgeWrap.NegEdgeToFaceAxis1 : EdgeWrap.None));
					case 1: return EdgeWrapUtility.FromEdgeRelations(
						(WrapsOnSideNegAxis0(faceIndex) ? EdgeWrap.NegEdgeToFaceAxis0 : EdgeWrap.None) |
						(WrapsOnSidePosAxis1(faceIndex) ? EdgeWrap.PosEdgeToVertAxis1 : EdgeWrap.None));
					case 2: return EdgeWrapUtility.FromEdgeRelations(
						(WrapsOnSidePosAxis0(faceIndex) ? EdgeWrap.PosEdgeToVertAxis0 : EdgeWrap.None) |
						(WrapsOnSidePosAxis1(faceIndex) ? EdgeWrap.PosFaceToEdgeAxis1 : EdgeWrap.None));
					case 3: return EdgeWrapUtility.FromEdgeRelations(
						(WrapsOnSidePosAxis0(faceIndex) ? EdgeWrap.PosFaceToEdgeAxis0 : EdgeWrap.None) |
						(WrapsOnSidePosAxis1(faceIndex) ? EdgeWrap.NegVertToEdgeAxis1 : EdgeWrap.None));
					default: throw new ArgumentOutOfRangeException("neighborIndex");
				}
			}
			else
			{
				switch (neighborIndex)
				{
					case 0: return EdgeWrapUtility.FromEdgeRelations(
						(WrapsOnSidePosAxis0(faceIndex) ? EdgeWrap.PosEdgeToVertAxis0 : EdgeWrap.None) |
						(WrapsOnSideNegAxis1(faceIndex) ? EdgeWrap.NegEdgeToFaceAxis1 : EdgeWrap.None));
					case 1: return EdgeWrapUtility.FromEdgeRelations(
						(WrapsOnSideNegAxis0(faceIndex) ? EdgeWrap.PosFaceToEdgeAxis0 : EdgeWrap.None) |
						(WrapsOnSidePosAxis1(faceIndex) ? EdgeWrap.PosEdgeToVertAxis1 : EdgeWrap.None));
					case 2: return EdgeWrapUtility.FromEdgeRelations(
						(WrapsOnSidePosAxis0(faceIndex) ? EdgeWrap.NegVertToEdgeAxis0 : EdgeWrap.None) |
						(WrapsOnSidePosAxis1(faceIndex) ? EdgeWrap.PosFaceToEdgeAxis1 : EdgeWrap.None));
					case 3: return EdgeWrapUtility.FromEdgeRelations(
						(WrapsOnSidePosAxis0(faceIndex) ? EdgeWrap.NegEdgeToFaceAxis0 : EdgeWrap.None) |
						(WrapsOnSidePosAxis1(faceIndex) ? EdgeWrap.NegVertToEdgeAxis1 : EdgeWrap.None));
					default: throw new ArgumentOutOfRangeException("neighborIndex");
				}
			}
		}

		#endregion

		#region IFaceIndexer2D Methods

		/// <inheritdoc/>
		public IntVector2 GetFaceIndex2D(int internalFaceIndex)
		{
			return new IntVector2(
				internalFaceIndex % _faceAxis0Count,
				internalFaceIndex / _faceAxis0Count);
		}

		/// <inheritdoc/>
		public int GetFaceIndex(int x, int y)
		{
			return x + y * _faceAxis0Count;
		}

		/// <inheritdoc/>
		public int FaceWrapX(int x)
		{
			return axis0.isWrapped ? Numerics.Math.Modulo(x, _faceAxis0Count) : x;
		}

		/// <inheritdoc/>
		public int FaceWrapY(int y)
		{
			return axis1.isWrapped ? Numerics.Math.Modulo(y, _faceAxis1Count) : y;
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

		/// <inheritdoc/>
		public IntVector2 GetVertexIndex2D(int vertexIndex)
		{
			return new IntVector2(
				vertexIndex % _vertexAxis0Count,
				vertexIndex / _vertexAxis0Count);
		}

		/// <inheritdoc/>
		public int GetVertexIndex(int x, int y)
		{
			return x + y * _vertexAxis0Count;
		}

		/// <inheritdoc/>
		public int VertexWrapX(int x)
		{
			return axis0.isWrapped ? Numerics.Math.Modulo(x, _vertexAxis0Count) : x;
		}

		/// <inheritdoc/>
		public int VertexWrapY(int y)
		{
			return axis1.isWrapped ? Numerics.Math.Modulo(y, _vertexAxis1Count) : y;
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

		private bool IsOnSideNegAxis0(int internalFaceIndex)
		{
			return internalFaceIndex % _faceAxis0Count == 0;
		}

		private bool IsOnSidePosAxis0(int internalFaceIndex)
		{
			return internalFaceIndex % _faceAxis0Count == _faceAxis0Count - 1;
		}

		private bool IsOnSideNegAxis1(int internalFaceIndex)
		{
			return internalFaceIndex < _faceAxis0Count;
		}

		private bool IsOnSidePosAxis1(int internalFaceIndex)
		{
			return internalFaceIndex >= _internalFaceCount - _faceAxis0Count;
		}

		private bool ExitsOnSideNegAxis0(int internalFaceIndex)
		{
			return !axis0.isWrapped && IsOnSideNegAxis0(internalFaceIndex);
		}

		private bool ExitsOnSidePosAxis0(int internalFaceIndex)
		{
			return !axis0.isWrapped && IsOnSidePosAxis0(internalFaceIndex);
		}

		private bool ExitsOnSideNegAxis1(int internalFaceIndex)
		{
			return !axis1.isWrapped && IsOnSideNegAxis1(internalFaceIndex);
		}

		private bool ExitsOnSidePosAxis1(int internalFaceIndex)
		{
			return !axis1.isWrapped && IsOnSidePosAxis1(internalFaceIndex);
		}

		private bool WrapsOnSideNegAxis0(int internalFaceIndex)
		{
			return axis0.isWrapped && IsOnSideNegAxis0(internalFaceIndex);
		}

		private bool WrapsOnSidePosAxis0(int internalFaceIndex)
		{
			return axis0.isWrapped && IsOnSidePosAxis0(internalFaceIndex);
		}

		private bool WrapsOnSideNegAxis1(int internalFaceIndex)
		{
			return axis1.isWrapped && IsOnSideNegAxis1(internalFaceIndex);
		}

		private bool WrapsOnSidePosAxis1(int internalFaceIndex)
		{
			return axis1.isWrapped && IsOnSidePosAxis1(internalFaceIndex);
		}

		#endregion
	}
}
