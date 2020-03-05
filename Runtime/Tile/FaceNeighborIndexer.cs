/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System;

namespace MakeIt.Tile
{
	/// <summary>
	/// An interface for providing a fairly minimal and simple description of a topology
	/// from which the full topology data structure can be generated.
	/// </summary>
	/// <seealso cref="TopologyUtility"/>
	/// <seealso cref="ManualFaceNeighborIndexer"/>
	public interface IFaceNeighborIndexer
	{
		/// <summary>
		/// The number of vertices in the topology described by this indexer.
		/// </summary>
		int vertexCount { get; }

		/// <summary>
		/// The number of edges in the topology described by this indexer.
		/// </summary>
		/// <remarks><para>This refers to half edges, so it is double the quantity of bidirectional edge pairs.</para></remarks>
		int edgeCount { get; }

		/// <summary>
		/// The number of faces in the topology described by this indexer.
		/// </summary>
		int faceCount { get; }

		/// <summary>
		/// The number of internal faces in the topoloy described by this indexer.
		/// </summary>
		int internalFaceCount { get; }

		/// <summary>
		/// The number of external faces in the topology described by this indexer.
		/// </summary>
		int externalFaceCount { get; }

		/// <summary>
		/// Returns how many neighbors the face with the given index has.
		/// </summary>
		/// <param name="faceIndex">The index of the face whose neighbor count is to be returned.</param>
		/// <returns>The number of neighbors the face with the given index has.</returns>
		ushort GetNeighborCount(int faceIndex);

		/// <summary>
		/// Returns the vertex index of a particular neighbor vertex of the face with the given index.
		/// </summary>
		/// <param name="faceIndex">The index of the face whose neighbor vertex index is to be returned.</param>
		/// <param name="neighborIndex">The index of the neighbor around the face whose vertex index is to be returned.  The first neighbor has an index of zero, and the last has an index one less than the neighbor count.</param>
		/// <returns>The vertex index of the specified neighbor vertex of the face with the given index.</returns>
		int GetNeighborVertexIndex(int faceIndex, int neighborIndex);

		// For a given internal face, return the edge wrap data of the specified neighbor.
		// The specific details required depend on the consumer of the face neighbor indexer.
		// Some consumers only require knowledge of specific relations, and can infer all
		// other relations from this more narrow description.
		/// <summary>
		/// Returns the edge wrap data of a particular neighbor edge of the face with the given index.
		/// </summary>
		/// <param name="faceIndex">The index of the face whose neighbor edge wrap data is to be returned.</param>
		/// <param name="neighborIndex">The index of the neighbor around the face whose edge wrap data is to be returned.  The first neighbor has an index of zero, and the last has an index one less than the neighbor count.</param>
		/// <returns>The edge wrap data of a particular neighbor edge of the face with the given index.</returns>
		EdgeWrap GetEdgeWrap(int faceIndex, int neighborIndex);
	}

	/// <summary>
	/// A class for manually providing a fairly minimal and simple description of
	/// a topology from which the full topology data structure can be generated.
	/// </summary>
	/// <seealso cref="TopologyUtility"/>
	public class ManualFaceNeighborIndexer : IFaceNeighborIndexer
	{
		private int _vertexCount;
		private int _edgeCount;
		private int _internalFaceCount;
		private int _externalFaceCount;

		/// <summary>
		/// A structure to hold minimal relation information about a singl neighbor around a face.
		/// </summary>
		public struct NeighborData
		{
			/// <summary>
			/// The index of a neighbor vertex.
			/// </summary>
			public int vertexIndex;

			/// <summary>
			/// The edge wrap data of a neighbor edge.
			/// </summary>
			public EdgeWrap edgeWrap;

			/// <summary>
			/// Constructs an instance of neighbor data using the given vertex index and edge wrap data.
			/// </summary>
			/// <param name="vertexIndex">The index of the neighbor vertex.</param>
			/// <param name="edgeWrap">The edge wrap data of the neighbor edge.</param>
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

		/// <summary>
		/// Constructs a partially specified instance of a manual face neighbor indexer with the given element counts.  Actual neighbor relations need to also be specified before the indexer is fully specified.
		/// </summary>
		/// <param name="vertexCount">The number of vertices in the topology described by this indexer.</param>
		/// <param name="edgeCount">The number of edges in the topology described by this indexer.</param>
		/// <param name="internalFaceCount">The number of internal faces in the topoloy described by this indexer.</param>
		/// <param name="externalFaceCount">The number of external faces in the topology described by this indexer.</param>
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

		/// <inheritdoc/>
		public int vertexCount { get { return _vertexCount; } }
		/// <inheritdoc/>
		public int edgeCount { get { return _edgeCount; } }
		/// <inheritdoc/>
		public int faceCount { get { return _internalFaceCount + _externalFaceCount; } }
		/// <inheritdoc/>
		public int internalFaceCount { get { return _internalFaceCount; } }
		/// <inheritdoc/>
		public int externalFaceCount { get { return _externalFaceCount; } }

		/// <inheritdoc/>
		public ushort GetNeighborCount(int faceIndex)
		{
			if (faceIndex < 0 || faceIndex >= _internalFaceCount) throw new ArgumentOutOfRangeException("faceIndex");
			return _faceNeighborCounts[faceIndex];
		}

		/// <inheritdoc/>
		public int GetNeighborVertexIndex(int faceIndex, int neighborIndex)
		{
			if (faceIndex < 0 || faceIndex >= _internalFaceCount) throw new ArgumentOutOfRangeException("faceIndex");
			if (neighborIndex < 0 || neighborIndex >= _faceNeighborCounts[faceIndex]) throw new ArgumentOutOfRangeException("neighborIndex");
			return _neighborData[_faceFirstNeighborIndices[faceIndex] + neighborIndex].vertexIndex;
		}

		/// <inheritdoc/>
		public EdgeWrap GetEdgeWrap(int faceIndex, int neighborIndex)
		{
			if (faceIndex < 0 || faceIndex >= _internalFaceCount) throw new ArgumentOutOfRangeException("faceIndex");
			if (neighborIndex < 0 || neighborIndex >= _faceNeighborCounts[faceIndex]) throw new ArgumentOutOfRangeException("neighborIndex");
			return _neighborData[_faceFirstNeighborIndices[faceIndex] + neighborIndex].edgeWrap;
		}

		/// <summary>
		/// Adds a face with the given vertex indices as neighbors.
		/// </summary>
		/// <param name="vertex0Index">The index of the first neighbor vertex.</param>
		/// <param name="vertex1Index">The index of the second neighbor vertex.</param>
		/// <param name="vertex2Index">The index of the third neighbor vertex.</param>
		/// <returns>The index of the face just added.</returns>
		public int AddFace(int vertex0Index, int vertex1Index, int vertex2Index)
		{
			_faceNeighborCounts[_faceIndex] = 3;
			_faceFirstNeighborIndices[_faceIndex] = _neighborDataIndex;
			_neighborData[_neighborDataIndex++] = new NeighborData(vertex0Index);
			_neighborData[_neighborDataIndex++] = new NeighborData(vertex1Index);
			_neighborData[_neighborDataIndex++] = new NeighborData(vertex2Index);
			return _faceIndex++;
		}

		/// <summary>
		/// Adds a face with the given vertex indices as neighbors.
		/// </summary>
		/// <param name="vertex0Index">The index of the first neighbor vertex.</param>
		/// <param name="vertex1Index">The index of the second neighbor vertex.</param>
		/// <param name="vertex2Index">The index of the third neighbor vertex.</param>
		/// <param name="vertex3Index">The index of the fourth neighbor vertex.</param>
		/// <returns>The index of the face just added.</returns>
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

		/// <summary>
		/// Adds a face with the given vertex indices as neighbors.
		/// </summary>
		/// <param name="vertex0Index">The index of the first neighbor vertex.</param>
		/// <param name="vertex1Index">The index of the second neighbor vertex.</param>
		/// <param name="vertex2Index">The index of the third neighbor vertex.</param>
		/// <param name="vertex3Index">The index of the fourth neighbor vertex.</param>
		/// <param name="vertex4Index">The index of the fifth neighbor vertex.</param>
		/// <returns>The index of the face just added.</returns>
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

		/// <summary>
		/// Adds a face with the given vertex indices as neighbors.
		/// </summary>
		/// <param name="vertex0Index">The index of the first neighbor vertex.</param>
		/// <param name="vertex1Index">The index of the second neighbor vertex.</param>
		/// <param name="vertex2Index">The index of the third neighbor vertex.</param>
		/// <param name="vertex3Index">The index of the fourth neighbor vertex.</param>
		/// <param name="vertex4Index">The index of the fifth neighbor vertex.</param>
		/// <param name="vertex5Index">The index of the sixth neighbor vertex.</param>
		/// <returns>The index of the face just added.</returns>
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

		/// <summary>
		/// Adds a face with the given vertex indices as neighbors.
		/// </summary>
		/// <param name="vertexIndices">The indices of the neighbor vertices.</param>
		/// <returns>The index of the face just added.</returns>
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

		/// <summary>
		/// Adds a face with the given neighbor data instances as neighbors.
		/// </summary>
		/// <param name="neighbor0">The neighbor data of the first neighbor.</param>
		/// <param name="neighbor1">The neighbor data of the second neighbor.</param>
		/// <param name="neighbor2">The neighbor data of the third neighbor.</param>
		/// <returns>The index of the face just added.</returns>
		public int AddFace(NeighborData neighbor0, NeighborData neighbor1, NeighborData neighbor2)
		{
			_faceNeighborCounts[_faceIndex] = 3;
			_faceFirstNeighborIndices[_faceIndex] = _neighborDataIndex;
			_neighborData[_neighborDataIndex++] = neighbor0;
			_neighborData[_neighborDataIndex++] = neighbor1;
			_neighborData[_neighborDataIndex++] = neighbor2;
			return _faceIndex++;
		}

		/// <summary>
		/// Adds a face with the given neighbor data instances as neighbors.
		/// </summary>
		/// <param name="neighbor0">The neighbor data of the first neighbor.</param>
		/// <param name="neighbor1">The neighbor data of the second neighbor.</param>
		/// <param name="neighbor2">The neighbor data of the third neighbor.</param>
		/// <param name="neighbor3">The neighbor data of the fourth neighbor.</param>
		/// <returns>The index of the face just added.</returns>
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

		/// <summary>
		/// Adds a face with the given neighbor data instances as neighbors.
		/// </summary>
		/// <param name="neighbor0">The neighbor data of the first neighbor.</param>
		/// <param name="neighbor1">The neighbor data of the second neighbor.</param>
		/// <param name="neighbor2">The neighbor data of the third neighbor.</param>
		/// <param name="neighbor3">The neighbor data of the fourth neighbor.</param>
		/// <param name="neighbor4">The neighbor data of the fifth neighbor.</param>
		/// <returns>The index of the face just added.</returns>
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

		/// <summary>
		/// Adds a face with the given neighbor data instances as neighbors.
		/// </summary>
		/// <param name="neighbor0">The neighbor data of the first neighbor.</param>
		/// <param name="neighbor1">The neighbor data of the second neighbor.</param>
		/// <param name="neighbor2">The neighbor data of the third neighbor.</param>
		/// <param name="neighbor3">The neighbor data of the fourth neighbor.</param>
		/// <param name="neighbor4">The neighbor data of the fifth neighbor.</param>
		/// <param name="neighbor5">The neighbor data of the sixth neighbor.</param>
		/// <returns>The index of the face just added.</returns>
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

		/// <summary>
		/// Adds a face with the given neighbor data instances as neighbors.
		/// </summary>
		/// <param name="neighbors">The neighbor data instances of the neighbors.</param>
		/// <returns>The index of the face just added.</returns>
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
