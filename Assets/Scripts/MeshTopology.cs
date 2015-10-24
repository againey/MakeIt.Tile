using UnityEngine;
using System.Collections.Generic;

public class BasicMeshTopology
{
	public int[,] _triangleVertices; //Vn x 3
	public int[,] _edgeVertices; //En x 2
}

public class MeshTopology : BasicMeshTopology
{
	public int[,] _triangleTriangles; //Vn x 3
	public int[,] _triangleEdges; //Vn x 3
	public int[,] _edgeTriangles; //En x 2
	public int[] _vertexNeighborOffsets;
	public int[] _vertexTriangles;
	public int[] _vertexEdges;
	public int[] _vertexVertices;

	public MeshTopology()
	{
	}

	public MeshTopology(BasicMeshTopology basicTopology)
	{
		_triangleVertices = new int[basicTopology._triangleVertices.GetLength(0), 3];
		System.Array.Copy(basicTopology._triangleVertices, 0, _triangleVertices, 0, basicTopology._triangleVertices.Length);
		_edgeVertices = new int[basicTopology._edgeVertices.GetLength(0), 2];
		System.Array.Copy(basicTopology._edgeVertices, 0, _edgeVertices, 0, basicTopology._edgeVertices.Length);

		// Determine the total number of tiles by finding the maximum tile index specified and adding 1.
		int vertexCount = 0;
		for (int i = 0; i < _edgeVertices.GetLength(0); ++i)
		{
			vertexCount = Mathf.Max(vertexCount, Mathf.Max(_edgeVertices[i, 0], _edgeVertices[i, 1]));
		}
		++vertexCount;

		// Determine the number of neighbors each tile has by examining all the edges and incrementing
		// the tile neighbor count for both neighboring tiles of every edge.
		var vertexNeighborCounts = new byte[vertexCount];
		for (int i = 0; i < _edgeVertices.GetLength(0); ++i)
		{
			++vertexNeighborCounts[_edgeVertices[i, 0]];
			++vertexNeighborCounts[_edgeVertices[i, 1]];
		}

		// Determine the offsets in the tile neighbor arrays by tracking a running sum of neighbor counts.
		_vertexNeighborOffsets = new int[vertexCount + 1];
		int totalNeighborCount = 0;
		for (int i = 0; i < vertexNeighborCounts.Length; ++i)
		{
			_vertexNeighborOffsets[i] = totalNeighborCount;
			totalNeighborCount += vertexNeighborCounts[i];
		}
		_vertexNeighborOffsets[_vertexNeighborOffsets.Length - 1] = totalNeighborCount;

		// Reset the tile neighbor counts, and then add the neighboring edges to the tiles one by one.
		System.Array.Clear(vertexNeighborCounts, 0, vertexNeighborCounts.Length);
		_vertexEdges = new int[totalNeighborCount];
		for (int i = 0; i < _edgeVertices.GetLength(0); ++i)
		{
			_vertexEdges[_vertexNeighborOffsets[_edgeVertices[i, 0]] + vertexNeighborCounts[_edgeVertices[i, 0]]++] = i;
			_vertexEdges[_vertexNeighborOffsets[_edgeVertices[i, 1]] + vertexNeighborCounts[_edgeVertices[i, 1]]++] = i;
		}

		// Reset the tile neighbor counts, and then add the neighboring vertices to the tiles one by one.
		System.Array.Clear(vertexNeighborCounts, 0, vertexNeighborCounts.Length);
		_vertexTriangles = new int[totalNeighborCount];
		for (int i = 0; i < _triangleVertices.GetLength(0); ++i)
		{
			_vertexTriangles[_vertexNeighborOffsets[_triangleVertices[i, 0]] + vertexNeighborCounts[_triangleVertices[i, 0]]++] = i;
			_vertexTriangles[_vertexNeighborOffsets[_triangleVertices[i, 1]] + vertexNeighborCounts[_triangleVertices[i, 1]]++] = i;
			_vertexTriangles[_vertexNeighborOffsets[_triangleVertices[i, 2]] + vertexNeighborCounts[_triangleVertices[i, 2]]++] = i;
		}

		// Reorder tile neighbors based on the order of vertices to their neighbor tiles.
		_vertexVertices = new int[totalNeighborCount];
		for (int i = 0; i < vertexNeighborCounts.Length; ++i)
		{
			var nextTriangle = _vertexTriangles[_vertexNeighborOffsets[i]];
			var triangleVertex = _triangleVertices[nextTriangle, 0] == i ? 0 : _triangleVertices[nextTriangle, 1] == i ? 1 : 2;
			var priorVertex = _triangleVertices[nextTriangle, (triangleVertex + 1) % 3];
			var nextVertex = _triangleVertices[nextTriangle, (triangleVertex + 2) % 3];

			// Reorder the tile's first neighbor edge to match the first neighbor vertex.
			for (int k = 0; k < vertexNeighborCounts[i]; ++k)
			{
				var vertexEdge = _vertexEdges[_vertexNeighborOffsets[i] + k];
				if (_edgeVertices[vertexEdge, 0] == priorVertex || _edgeVertices[vertexEdge, 1] == priorVertex)
				{
					if (k > 0)
					{
						var swapIntermediate = _vertexEdges[_vertexNeighborOffsets[i]];
						_vertexEdges[_vertexNeighborOffsets[i]] = _vertexEdges[_vertexNeighborOffsets[i] + k];
						_vertexEdges[_vertexNeighborOffsets[i] + k] = swapIntermediate;
					}
					break;
				}
			}

			// Set the first neighbor tile to match the first neighbor edge and vertex.
			_vertexVertices[_vertexNeighborOffsets[i]] = priorVertex;

			// Find each following vertex, given the preceding vertex, and then line up the edges and neighbor tiles to match.
			// The very last neighbor doesn't need to be reordered, since there will be nothing left to swap with.
			for (int j = 1; j < vertexNeighborCounts[i] - 1; ++j)
			{
				// Check each following vertex to see if it is the correct vertex to come next in the ordering.
				for (int k = j; k < vertexNeighborCounts[i]; ++k)
				{
					nextTriangle = _vertexTriangles[_vertexNeighborOffsets[i] + k];
					triangleVertex = _triangleVertices[nextTriangle, 0] == i ? 0 : _triangleVertices[nextTriangle, 1] == i ? 1 : 2;
					if (_triangleVertices[nextTriangle, (triangleVertex + 1) % 3] == nextVertex)
					{
						priorVertex = nextVertex;
						nextVertex = _triangleVertices[nextTriangle, (triangleVertex + 2) % 3];
						if (k > j)
						{
							var swapIntermediate = _vertexTriangles[_vertexNeighborOffsets[i] + j];
							_vertexTriangles[_vertexNeighborOffsets[i] + j] = _vertexTriangles[_vertexNeighborOffsets[i] + k];
							_vertexTriangles[_vertexNeighborOffsets[i] + k] = swapIntermediate;
						}
						break;
					}
				}

				// Reorder the tile's j-th neighbor edge to match the j-th neighbor vertex.
				for (int k = j; k < vertexNeighborCounts[i]; ++k)
				{
					var vertexEdge = _vertexEdges[_vertexNeighborOffsets[i] + k];
					if (_edgeVertices[vertexEdge, 0] == priorVertex || _edgeVertices[vertexEdge, 1] == priorVertex)
					{
						if (k > j)
						{
							var swapIntermediate = _vertexEdges[_vertexNeighborOffsets[i] + j];
							_vertexEdges[_vertexNeighborOffsets[i] + j] = _vertexEdges[_vertexNeighborOffsets[i] + k];
							_vertexEdges[_vertexNeighborOffsets[i] + k] = swapIntermediate;
						}
						break;
					}
				}

				// Set the j-th neighbor tile to match the j-th neighbor edge and vertex.
				_vertexVertices[_vertexNeighborOffsets[i] + j] = priorVertex;
			}

			// Set the last neighbor tile to match the last neighbor edge and vertex.
			_vertexVertices[_vertexNeighborOffsets[i + 1] - 1] = nextVertex;
		}

		// Build edge neighbors.
		_edgeTriangles = new int[_edgeVertices.GetLength(0), 2];

		for (int i = 0; i < _edgeVertices.GetLength(0); ++i)
		{
			var edgeVertex = _edgeVertices[i, 0];
			var vertexEdge = 0;
			for (; vertexEdge < vertexNeighborCounts[edgeVertex]; ++vertexEdge)
			{
				if (_vertexEdges[_vertexNeighborOffsets[edgeVertex] + vertexEdge] == i)
				{
					break;
				}
			}

			_edgeTriangles[i, 0] = _vertexTriangles[_vertexNeighborOffsets[edgeVertex] + vertexEdge];
			_edgeTriangles[i, 1] = _vertexTriangles[_vertexNeighborOffsets[edgeVertex] + (vertexEdge + vertexNeighborCounts[edgeVertex] - 1) % vertexNeighborCounts[edgeVertex]];
		}

		// Build vertex neighbors.
		_triangleEdges = new int[_triangleVertices.GetLength(0), 3];
		_triangleTriangles = new int[_triangleVertices.GetLength(0), 3];

		for (int i = 0; i < _triangleVertices.GetLength(0); ++i)
		{
			for (int j = 0; j < 3; ++j)
			{
				var triangleVertex = _triangleVertices[i, j];
				var vertexTriangle = 0;
				for (; vertexTriangle < vertexNeighborCounts[triangleVertex]; ++vertexTriangle)
				{
					if (_vertexTriangles[_vertexNeighborOffsets[triangleVertex] + vertexTriangle] == i)
					{
						break;
					}
				}
				var triangleEdge = _vertexEdges[_vertexNeighborOffsets[triangleVertex] + vertexTriangle];
				_triangleEdges[i, j] = triangleEdge;
				_triangleTriangles[i, j] = _vertexTriangles[_vertexNeighborOffsets[triangleVertex] + (vertexTriangle + vertexNeighborCounts[triangleVertex] - 1) % vertexNeighborCounts[triangleVertex]];
			}
		}
	}

	public BasicMeshTopology Subdivide(int degree, System.Action<int> vertexAllocator, System.Action<int, int> vertexCopier, System.Action<int, int, int, float> vertexInterpolator)
	{
		var subdivided = new BasicMeshTopology();

		if (degree == 0)
		{
			subdivided._triangleVertices = new int[_triangleVertices.GetLength(0), 3];
			System.Array.Copy(_triangleVertices, 0, subdivided._triangleVertices, 0, _triangleVertices.Length);
			subdivided._edgeVertices = new int[_edgeVertices.GetLength(0), 2];
			System.Array.Copy(_edgeVertices, 0, subdivided._edgeVertices, 0, _edgeVertices.Length);
			vertexAllocator(_vertexNeighborOffsets.Length - 1);
			for (int i = 0; i < _vertexNeighborOffsets.Length - 1; ++i)
			{
				vertexCopier(i, i);
			}
			return subdivided;
		}

		var vertexCount = _vertexNeighborOffsets.Length - 1;
		var edgeCount = _edgeVertices.GetLength(0);
		var triangleCount = _triangleVertices.GetLength(0);

		var innerVerticesPerEdge = degree + 0;
		var innerEdgesPerEdge = degree + 1;
		var verticesPerTriangle = (degree + 2) * (degree + 3) / 2;
		var innerVerticesPerTriangle = (degree - 1) * degree / 2;
		var innerEdgesPerTriangle = degree * (degree + 1) * 3 / 2;
		var innerTrianglesPerTriangle = (degree + 1) * (degree + 1);

		int totalSubdividedVertexCount = innerVerticesPerTriangle * triangleCount + innerVerticesPerEdge * edgeCount + vertexCount;
		int totalSubdividedEdgeCount = innerEdgesPerTriangle * triangleCount + innerEdgesPerEdge * edgeCount;
		int totalSubdividedTriangleCount = innerTrianglesPerTriangle * triangleCount;

		vertexAllocator(totalSubdividedVertexCount);
		subdivided._edgeVertices = new int[totalSubdividedEdgeCount, 2];
		subdivided._triangleVertices = new int[totalSubdividedTriangleCount, 3];

		int subdividedVertexCount = 0;
		int subdividedEdgeCount = 0;
		int subdividedTriangleCount = 0;

		for (int i = 0; i < _vertexNeighborOffsets.Length - 1; ++i)
		{
			vertexCopier(i, i);
		}
		subdividedVertexCount = _vertexNeighborOffsets.Length - 1;

		System.Action<int, int, int> SubdivideLine = delegate (int i0, int i1, int count)
		{
			var dt = 1.0f / (float)(count + 1);
			var t = dt;
			var tEnd = 1f - dt * 0.5f;
			while (t < tEnd)
			{
				vertexInterpolator(i0, i1, subdividedVertexCount++, t);
				t += dt;
			}
		};

		var firstSubdividedEdgeVertex = subdividedVertexCount;
		for (int i = 0; i < edgeCount; ++i)
		{
			int firstEdgeVertex = subdividedVertexCount;

			SubdivideLine(_edgeVertices[i, 0], _edgeVertices[i, 1], innerVerticesPerEdge);

			subdivided._edgeVertices[subdividedEdgeCount, 0] = _edgeVertices[i, 0];
			subdivided._edgeVertices[subdividedEdgeCount++, 1] = firstEdgeVertex;
			for (int j = 1; j < degree; ++j)
			{
				subdivided._edgeVertices[subdividedEdgeCount, 0] = firstEdgeVertex + j - 1;
				subdivided._edgeVertices[subdividedEdgeCount++, 1] = firstEdgeVertex + j;
			}
			subdivided._edgeVertices[subdividedEdgeCount, 0] = firstEdgeVertex + degree - 1;
			subdivided._edgeVertices[subdividedEdgeCount++, 1] = _edgeVertices[i, 1];
		}

		var triangleVertexLookup = new int[verticesPerTriangle];

		for (int i = 0; i < triangleCount; ++i)
		{
			var edge0 = _triangleEdges[i, 0];
			var edge1 = _triangleEdges[i, 1];
			var edge2 = _triangleEdges[i, 2];
			var firstEdgeVertex0 = firstSubdividedEdgeVertex + edge0 * innerVerticesPerEdge;
			var firstEdgeVertex1 = firstSubdividedEdgeVertex + edge1 * innerVerticesPerEdge;
			var firstEdgeVertex2 = firstSubdividedEdgeVertex + edge2 * innerVerticesPerEdge;
			var vertexDelta0 = _edgeVertices[edge0, 0] == _triangleVertices[i, 1] ? 1 : -1;
			var vertexDelta1 = _edgeVertices[edge1, 0] == _triangleVertices[i, 1] ? 1 : -1;
			var vertexDelta2 = _edgeVertices[edge2, 0] == _triangleVertices[i, 0] ? 1 : -1;
			if (vertexDelta0 == -1) firstEdgeVertex0 += innerVerticesPerEdge - 1;
			if (vertexDelta1 == -1) firstEdgeVertex1 += innerVerticesPerEdge - 1;
			if (vertexDelta2 == -1) firstEdgeVertex2 += innerVerticesPerEdge - 1;

			triangleVertexLookup[0] = _triangleVertices[i, 1];
			triangleVertexLookup[1] = firstEdgeVertex0;
			triangleVertexLookup[2] = firstEdgeVertex1;
			var triangleVertexLookupCount = 3;

			var edgeVertex0 = firstEdgeVertex0 + vertexDelta0;
			var edgeVertex1 = firstEdgeVertex1 + vertexDelta1;

			for (int j = 1; j < innerVerticesPerEdge; ++j)
			{
				var firstRowVertex = subdividedVertexCount;
				SubdivideLine(edgeVertex0, edgeVertex1, j);

				triangleVertexLookup[triangleVertexLookupCount++] = edgeVertex0;
				for (int k = 0; k < j; ++k)
				{
					triangleVertexLookup[triangleVertexLookupCount++] = firstRowVertex + k;
				}
				triangleVertexLookup[triangleVertexLookupCount++] = edgeVertex1;

				edgeVertex0 += vertexDelta0;
				edgeVertex1 += vertexDelta1;
			}

			triangleVertexLookup[triangleVertexLookupCount++] = _triangleVertices[i, 0];
			var edgeVertex2 = firstEdgeVertex2;
			for (int k = 0; k < innerVerticesPerEdge; ++k)
			{
				triangleVertexLookup[triangleVertexLookupCount++] = edgeVertex2++;
			}
			triangleVertexLookup[triangleVertexLookupCount++] = _triangleVertices[i, 2];

			subdivided._triangleVertices[subdividedTriangleCount, 0] = triangleVertexLookup[0];
			subdivided._triangleVertices[subdividedTriangleCount, 1] = triangleVertexLookup[1];
			subdivided._triangleVertices[subdividedTriangleCount++, 2] = triangleVertexLookup[2];

			var triangleVertex = 1;
			for (int j = 1; j <= innerVerticesPerEdge; ++j)
			{
				for (int k = 0; k < j; ++k)
				{
					subdivided._edgeVertices[subdividedEdgeCount, 0] = triangleVertexLookup[triangleVertex];
					subdivided._edgeVertices[subdividedEdgeCount++, 1] = triangleVertexLookup[triangleVertex + 1];

					subdivided._edgeVertices[subdividedEdgeCount, 0] = triangleVertexLookup[triangleVertex];
					subdivided._edgeVertices[subdividedEdgeCount++, 1] = triangleVertexLookup[triangleVertex + j + 2];

					subdivided._edgeVertices[subdividedEdgeCount, 0] = triangleVertexLookup[triangleVertex + 1];
					subdivided._edgeVertices[subdividedEdgeCount++, 1] = triangleVertexLookup[triangleVertex + j + 2];

					subdivided._triangleVertices[subdividedTriangleCount, 0] = triangleVertexLookup[triangleVertex];
					subdivided._triangleVertices[subdividedTriangleCount, 1] = triangleVertexLookup[triangleVertex + j + 1];
					subdivided._triangleVertices[subdividedTriangleCount++, 2] = triangleVertexLookup[triangleVertex + j + 2];

					subdivided._triangleVertices[subdividedTriangleCount, 0] = triangleVertexLookup[triangleVertex];
					subdivided._triangleVertices[subdividedTriangleCount, 1] = triangleVertexLookup[triangleVertex + j + 2];
					subdivided._triangleVertices[subdividedTriangleCount++, 2] = triangleVertexLookup[triangleVertex + 1];

					++triangleVertex;
				}

				subdivided._triangleVertices[subdividedTriangleCount, 0] = triangleVertexLookup[triangleVertex];
				subdivided._triangleVertices[subdividedTriangleCount, 1] = triangleVertexLookup[triangleVertex + j + 1];
				subdivided._triangleVertices[subdividedTriangleCount++, 2] = triangleVertexLookup[triangleVertex + j + 2];

				++triangleVertex;
			}
		}

		return subdivided;
	}
}

