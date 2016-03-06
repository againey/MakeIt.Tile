/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using UnityEngine;
using System;

namespace Experilous.Topological
{
	public class RectangularQuadGrid : PlanarSurface, IFaceNeighborIndexer, IFaceIndexer2D, IVertexIndexer2D
	{
		public Index2D size;
		public Topology topology;

		public Vector3 faceAxis0;
		public Vector3 faceAxis1;

		[SerializeField] private int _faceAxis0Count;
		[SerializeField] private int _faceAxis1Count;

		[SerializeField] private int _vertexAxis0Count;
		[SerializeField] private int _vertexAxis1Count;

		[SerializeField] private int _vertexCount;
		[SerializeField] private int _edgeCount;
		[SerializeField] private int _internalFaceCount;
		[SerializeField] private int _externalFaceCount;

		public static RectangularQuadGrid Create(PlanarDescriptor planarDescriptor, Index2D size)
		{
			return CreateInstance<RectangularQuadGrid>().Reset(planarDescriptor, size);
		}

		public RectangularQuadGrid Reset(PlanarDescriptor planarDescriptor, Index2D size)
		{
			Reset(planarDescriptor);
			faceAxis0 = planarDescriptor.axis0.vector;
			faceAxis1 = planarDescriptor.axis1.vector;
			this.size = size;
			topology = null;
			Initialize();
			return this;
		}

		protected void Initialize()
		{
			if (size.x <= 0 || size.y <= 0)
				throw new InvalidTopologyException("A rectangular quad grid cannot have an axis size less than or equal to zero.");

			axis0.vector *= size.x;
			axis1.vector *= size.y;

			_faceAxis0Count = size.x;
			_faceAxis1Count = size.y;
			_vertexAxis0Count = _faceAxis0Count + (axis0.isWrapped ? 0 : 1);
			_vertexAxis1Count = _faceAxis1Count + (axis1.isWrapped ? 0 : 1);

			_vertexCount = _vertexAxis0Count * _vertexAxis1Count;
			_edgeCount = (_vertexAxis1Count * _faceAxis0Count + vertexAxis0Count * _faceAxis1Count) * 2;
			_internalFaceCount = _faceAxis0Count * _faceAxis1Count;
			_externalFaceCount = (axis0.isWrapped && axis1.isWrapped) ? 0 : (axis0.isWrapped || axis1.isWrapped) ? 2 : 1;
		}

		public int faceAxis0Count { get { return _faceAxis0Count; } }
		public int faceAxis1Count { get { return _faceAxis1Count; } }

		public int vertexAxis0Count { get { return _vertexAxis0Count; } }
		public int vertexAxis1Count { get { return _vertexAxis1Count; } }

		public int vertexCount { get { return _vertexCount; } }
		public int edgeCount { get { return _edgeCount; } }
		public int faceCount { get { return _internalFaceCount + _externalFaceCount; } }
		public int internalFaceCount { get { return _internalFaceCount; } }
		public int externalFaceCount { get { return _externalFaceCount; } }

		public Topology CreateTopology()
		{
			return TopologyUtility.BuildTopology(this);
		}

		public Topology CreateManifold(out Vector3[] vertexPositions)
		{
			vertexPositions = new Vector3[vertexCount];

			for (int i = 0; i < vertexCount; ++i)
			{
				var index2D = GetVertexIndex2D(i);
				vertexPositions[i] =
					origin +
					index2D.x * faceAxis0 +
					index2D.y * faceAxis1;
			}

			return CreateTopology();
		}

		#region IFaceNeighborIndexer Methods

		public ushort GetNeighborCount(int faceIndex)
		{
			if (faceIndex < 0 || faceIndex >= _internalFaceCount) throw new ArgumentOutOfRangeException("faceIndex");

			return 4;
		}

		public int GetNeighborVertexIndex(int faceIndex, int neighborIndex)
		{
			if (faceIndex < 0 || faceIndex >= _internalFaceCount) throw new ArgumentOutOfRangeException("faceIndex");

			var index2D = GetFaceIndex2D(faceIndex);
			var basisIndex = index2D.y * _vertexAxis0Count;
			switch (ConditionalInvert(neighborIndex, 3, isInverted))
			{
				case 0: return basisIndex + index2D.x;
				case 1: return (basisIndex + _vertexAxis0Count) % vertexCount + index2D.x;
				case 2: return (basisIndex + _vertexAxis0Count) % vertexCount + (index2D.x + 1) % _vertexAxis0Count;
				case 3: return basisIndex + (index2D.x + 1) % _vertexAxis0Count;
				default: throw new ArgumentOutOfRangeException("neighborIndex");
			}
		}

		public EdgeWrap GetEdgeWrap(int faceIndex, int neighborIndex)
		{
			if (faceIndex < 0 || faceIndex >= _internalFaceCount) throw new ArgumentOutOfRangeException("faceIndex");

			if (!axis0.isWrapped && !axis1.isWrapped) return EdgeWrap.None;

			if (!isInverted)
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

		public Index2D GetFaceIndex2D(int internalFaceIndex)
		{
			return new Index2D(
				internalFaceIndex % _faceAxis0Count,
				internalFaceIndex / _faceAxis0Count);
		}

		public int GetFaceIndex(int x, int y)
		{
			return x + y * _faceAxis0Count;
		}

		public int FaceWrapX(int x)
		{
			return axis0.isWrapped ? MathUtility.Modulo(x, _faceAxis0Count) : x;
		}

		public int FaceWrapY(int y)
		{
			return axis1.isWrapped ? MathUtility.Modulo(y, _faceAxis1Count) : y;
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

		public Index2D GetVertexIndex2D(int vertexIndex)
		{
			return new Index2D(
				vertexIndex % _vertexAxis0Count,
				vertexIndex / _vertexAxis0Count);
		}

		public int GetVertexIndex(int x, int y)
		{
			return x + y * _vertexAxis0Count;
		}

		public int VertexWrapX(int x)
		{
			return axis0.isWrapped ? MathUtility.Modulo(x, _vertexAxis0Count) : x;
		}

		public int VertexWrapY(int y)
		{
			return axis1.isWrapped ? MathUtility.Modulo(y, _vertexAxis1Count) : y;
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
