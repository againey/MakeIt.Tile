/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// An interface for providing a fairly minimal and simple description of a topology
	/// from which the full topology data structure can be generated.
	/// </summary>
	/// <seealso cref="TopologyUtility"/>
	/// <seealso cref="ManualFaceNeighborIndexer"/>
	public interface IFaceNeighborIndexer
	{
		int vertexCount { get; }
		int edgeCount { get; }
		int faceCount { get; }
		int internalFaceCount { get; }
		int externalFaceCount { get; }

		// For a given internal face, return how many neighbors it has.
		ushort GetNeighborCount(int faceIndex);

		// For a given internal face, return the vertex index of the specified neighbor.
		int GetNeighborVertexIndex(int faceIndex, int neighborIndex);

		// For a given internal face, return the edge wrap data of the specified neighbor.
		// The specific details required depend on the consumer of the face neighbor indexer.
		// Some consumers only require knowledge of specific relations, and can infer all
		// other relations from this more narrow description.
		EdgeWrap GetEdgeWrap(int faceIndex, int neighborIndex);
	}

	public class ManualFaceNeighborIndexer : IFaceNeighborIndexer
	{
		private int _vertexCount;
		private int _edgeCount;
		private int _internalFaceCount;
		private int _externalFaceCount;

		public struct NeighborData
		{
			public int vertexIndex;
			public EdgeWrap edgeWrap;

			public NeighborData(int vertexIndex, EdgeWrap edgeWrap = EdgeWrap.None)
			{
				this.vertexIndex = vertexIndex;
				this.edgeWrap = edgeWrap;
			}
		}

		private ushort[] _faceNeighborCounts;
		private int[] _faceFirstNeighborIndices;
		private NeighborData[] _neighborData;

		private int _faceIndex;
		private int _neighborDataIndex;

		public ManualFaceNeighborIndexer(int vertexCount, int edgeCount, int internalFaceCount, int externalFaceCount = 0)
		{
			_vertexCount = vertexCount;
			_edgeCount = edgeCount;
			_internalFaceCount = internalFaceCount;
			_externalFaceCount = externalFaceCount;

			_faceNeighborCounts = new ushort[_internalFaceCount];
			_faceFirstNeighborIndices = new int[_internalFaceCount];
			_neighborData = new NeighborData[_edgeCount];
		}

		public int vertexCount { get { return _vertexCount; } }
		public int edgeCount { get { return _edgeCount; } }
		public int faceCount { get { return _internalFaceCount + _externalFaceCount; } }
		public int internalFaceCount { get { return _internalFaceCount; } }
		public int externalFaceCount { get { return _externalFaceCount; } }

		public ushort GetNeighborCount(int faceIndex)
		{
			if (faceIndex < 0 || faceIndex >= _internalFaceCount) throw new ArgumentOutOfRangeException("faceIndex");
			return _faceNeighborCounts[faceIndex];
		}

		public int GetNeighborVertexIndex(int faceIndex, int neighborIndex)
		{
			if (faceIndex < 0 || faceIndex >= _internalFaceCount) throw new ArgumentOutOfRangeException("faceIndex");
			if (neighborIndex < 0 || neighborIndex >= _faceNeighborCounts[faceIndex]) throw new ArgumentOutOfRangeException("neighborIndex");
			return _neighborData[_faceFirstNeighborIndices[faceIndex] + neighborIndex].vertexIndex;
		}

		public EdgeWrap GetEdgeWrap(int faceIndex, int neighborIndex)
		{
			if (faceIndex < 0 || faceIndex >= _internalFaceCount) throw new ArgumentOutOfRangeException("faceIndex");
			if (neighborIndex < 0 || neighborIndex >= _faceNeighborCounts[faceIndex]) throw new ArgumentOutOfRangeException("neighborIndex");
			return _neighborData[_faceFirstNeighborIndices[faceIndex] + neighborIndex].edgeWrap;
		}

		public int AddFace(int vertex0Index, int vertex1Index, int vertex2Index)
		{
			_faceNeighborCounts[_faceIndex] = 3;
			_faceFirstNeighborIndices[_faceIndex] = _neighborDataIndex;
			_neighborData[_neighborDataIndex++] = new NeighborData(vertex0Index);
			_neighborData[_neighborDataIndex++] = new NeighborData(vertex1Index);
			_neighborData[_neighborDataIndex++] = new NeighborData(vertex2Index);
			return _faceIndex++;
		}

		public int AddFace(int vertex0Index, int vertex1Index, int vertex2Index, int vertex3Index)
		{
			_faceNeighborCounts[_faceIndex] = 4;
			_faceFirstNeighborIndices[_faceIndex] = _neighborDataIndex;
			_neighborData[_neighborDataIndex++] = new NeighborData(vertex0Index);
			_neighborData[_neighborDataIndex++] = new NeighborData(vertex1Index);
			_neighborData[_neighborDataIndex++] = new NeighborData(vertex2Index);
			_neighborData[_neighborDataIndex++] = new NeighborData(vertex3Index);
			return _faceIndex++;
		}

		public int AddFace(int vertex0Index, int vertex1Index, int vertex2Index, int vertex3Index, int vertex4Index)
		{
			_faceNeighborCounts[_faceIndex] = 5;
			_faceFirstNeighborIndices[_faceIndex] = _neighborDataIndex;
			_neighborData[_neighborDataIndex++] = new NeighborData(vertex0Index);
			_neighborData[_neighborDataIndex++] = new NeighborData(vertex1Index);
			_neighborData[_neighborDataIndex++] = new NeighborData(vertex2Index);
			_neighborData[_neighborDataIndex++] = new NeighborData(vertex3Index);
			_neighborData[_neighborDataIndex++] = new NeighborData(vertex4Index);
			return _faceIndex++;
		}

		public int AddFace(int vertex0Index, int vertex1Index, int vertex2Index, int vertex3Index, int vertex4Index, int vertex5Index)
		{
			_faceNeighborCounts[_faceIndex] = 6;
			_faceFirstNeighborIndices[_faceIndex] = _neighborDataIndex;
			_neighborData[_neighborDataIndex++] = new NeighborData(vertex0Index);
			_neighborData[_neighborDataIndex++] = new NeighborData(vertex1Index);
			_neighborData[_neighborDataIndex++] = new NeighborData(vertex2Index);
			_neighborData[_neighborDataIndex++] = new NeighborData(vertex3Index);
			_neighborData[_neighborDataIndex++] = new NeighborData(vertex4Index);
			_neighborData[_neighborDataIndex++] = new NeighborData(vertex5Index);
			return _faceIndex++;
		}

		public int AddFace(params int[] vertexIndices)
		{
			if (vertexIndices.Length < 3) throw new ArgumentException("A face must have at least 3 neighbors.", "vertexIndices");
			_faceNeighborCounts[_faceIndex] = (ushort)vertexIndices.Length;
			_faceFirstNeighborIndices[_faceIndex] = _neighborDataIndex;
			foreach (var vertexIndex in vertexIndices)
			{
				_neighborData[_neighborDataIndex++] = new NeighborData(vertexIndex);
			}
			return _faceIndex++;
		}

		public int AddFace(NeighborData neighbor0, NeighborData neighbor1, NeighborData neighbor2)
		{
			_faceNeighborCounts[_faceIndex] = 3;
			_faceFirstNeighborIndices[_faceIndex] = _neighborDataIndex;
			_neighborData[_neighborDataIndex++] = neighbor0;
			_neighborData[_neighborDataIndex++] = neighbor1;
			_neighborData[_neighborDataIndex++] = neighbor2;
			return _faceIndex++;
		}

		public int AddFace(NeighborData neighbor0, NeighborData neighbor1, NeighborData neighbor2, NeighborData neighbor3)
		{
			_faceNeighborCounts[_faceIndex] = 4;
			_faceFirstNeighborIndices[_faceIndex] = _neighborDataIndex;
			_neighborData[_neighborDataIndex++] = neighbor0;
			_neighborData[_neighborDataIndex++] = neighbor1;
			_neighborData[_neighborDataIndex++] = neighbor2;
			_neighborData[_neighborDataIndex++] = neighbor3;
			return _faceIndex++;
		}

		public int AddFace(NeighborData neighbor0, NeighborData neighbor1, NeighborData neighbor2, NeighborData neighbor3, NeighborData neighbor4)
		{
			_faceNeighborCounts[_faceIndex] = 5;
			_faceFirstNeighborIndices[_faceIndex] = _neighborDataIndex;
			_neighborData[_neighborDataIndex++] = neighbor0;
			_neighborData[_neighborDataIndex++] = neighbor1;
			_neighborData[_neighborDataIndex++] = neighbor2;
			_neighborData[_neighborDataIndex++] = neighbor3;
			_neighborData[_neighborDataIndex++] = neighbor4;
			return _faceIndex++;
		}

		public int AddFace(NeighborData neighbor0, NeighborData neighbor1, NeighborData neighbor2, NeighborData neighbor3, NeighborData neighbor4, NeighborData neighbor5)
		{
			_faceNeighborCounts[_faceIndex] = 6;
			_faceFirstNeighborIndices[_faceIndex] = _neighborDataIndex;
			_neighborData[_neighborDataIndex++] = neighbor0;
			_neighborData[_neighborDataIndex++] = neighbor1;
			_neighborData[_neighborDataIndex++] = neighbor2;
			_neighborData[_neighborDataIndex++] = neighbor3;
			_neighborData[_neighborDataIndex++] = neighbor4;
			_neighborData[_neighborDataIndex++] = neighbor5;
			return _faceIndex++;
		}

		public int AddFace(params NeighborData[] neighbors)
		{
			if (neighbors.Length < 3) throw new ArgumentException("A face must have at least 3 neighbors.", "neighbors");
			_faceNeighborCounts[_faceIndex] = (ushort)neighbors.Length;
			_faceFirstNeighborIndices[_faceIndex] = _neighborDataIndex;
			foreach (var neighbor in neighbors)
			{
				_neighborData[_neighborDataIndex++] = neighbor;
			}
			return _faceIndex++;
		}
	}
}
