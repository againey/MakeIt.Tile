using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Tiling
{
	public partial class Topology
	{
		public class Builder
		{
			private struct VertexNeighbor
			{
				public int _next;
				public int _vertex;

				public VertexNeighbor(int vertex) { _next = -1; _vertex = vertex; }
				public VertexNeighbor(int next, int vertex) { _next = next; _vertex = vertex; }
			}

			private readonly List<int> _vertexRoots = new List<int>();
			private readonly List<VertexNeighbor> _vertexNeighbors = new List<VertexNeighbor>();

			public Builder()
			{
			}

			public Builder(int vertexCount, int edgeCount, int faceCount)
			{
				_vertexRoots.Capacity = vertexCount;
				_vertexNeighbors.Capacity = vertexCount + edgeCount * 3 / 2;
			}

			public int vertexCount { get { return _vertexRoots.Count; } }

			public int AddVertex()
			{
				var vertexIndex = _vertexRoots.Count;
				_vertexRoots.Add(-1);
				return vertexIndex;
			}

			public int AddVertex(int neighbor0, int neighbor1, int neighbor2)
			{
				var vertexIndex = _vertexRoots.Count;
				_vertexRoots.Add(_vertexNeighbors.Count);
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor0));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor1));
				_vertexNeighbors.Add(new VertexNeighbor(neighbor2));
				return vertexIndex;
			}

			public int AddVertex(int neighbor0, int neighbor1, int neighbor2, int neighbor3)
			{
				var vertexIndex = _vertexRoots.Count;
				_vertexRoots.Add(_vertexNeighbors.Count);
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor0));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor1));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor2));
				_vertexNeighbors.Add(new VertexNeighbor(neighbor3));
				return vertexIndex;
			}

			public int AddVertex(int neighbor0, int neighbor1, int neighbor2, int neighbor3, int neighbor4)
			{
				var vertexIndex = _vertexRoots.Count;
				_vertexRoots.Add(_vertexNeighbors.Count);
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor0));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor1));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor2));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor3));
				_vertexNeighbors.Add(new VertexNeighbor(neighbor4));
				return vertexIndex;
			}

			public int AddVertex(int neighbor0, int neighbor1, int neighbor2, int neighbor3, int neighbor4, int neighbor5)
			{
				var vertexIndex = _vertexRoots.Count;
				_vertexRoots.Add(_vertexNeighbors.Count);
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor0));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor1));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor2));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor3));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor4));
				_vertexNeighbors.Add(new VertexNeighbor(neighbor5));
				return vertexIndex;
			}

			public int AddVertex(params int[] neighbors)
			{
				return AddVertex(neighbors.Length, neighbors);
			}

			public int AddVertex(int neighborCount, int[] neighbors)
			{
				var vertexIndex = _vertexRoots.Count;
				_vertexRoots.Add(_vertexNeighbors.Count);
				int i = 0;
				int iEnd = neighborCount - 1;
				while (i < iEnd)
				{
					_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbors[i]));
					++i;
				}
				_vertexNeighbors.Add(new VertexNeighbor(neighbors[i]));
				return vertexIndex;
			}

			public void ExtendVertex(int vertex, int neighbor0)
			{
				var originalIndex = _vertexRoots[vertex];
				_vertexRoots[vertex] = _vertexNeighbors.Count;
				_vertexNeighbors.Add(new VertexNeighbor(originalIndex, neighbor0));
			}

			public void ExtendVertex(int vertex, int neighbor0, int neighbor1)
			{
				var originalIndex = _vertexRoots[vertex];
				_vertexRoots[vertex] = _vertexNeighbors.Count;
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor0));
				_vertexNeighbors.Add(new VertexNeighbor(originalIndex, neighbor1));
			}

			public void ExtendVertex(int vertex, int neighbor0, int neighbor1, int neighbor2)
			{
				var originalIndex = _vertexRoots[vertex];
				_vertexRoots[vertex] = _vertexNeighbors.Count;
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor0));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor1));
				_vertexNeighbors.Add(new VertexNeighbor(originalIndex, neighbor2));
			}

			public void ExtendVertex(int vertex, int neighbor0, int neighbor1, int neighbor2, int neighbor3)
			{
				var originalIndex = _vertexRoots[vertex];
				_vertexRoots[vertex] = _vertexNeighbors.Count;
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor0));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor1));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor2));
				_vertexNeighbors.Add(new VertexNeighbor(originalIndex, neighbor3));
			}

			public void ExtendVertex(int vertex, int neighbor0, int neighbor1, int neighbor2, int neighbor3, int neighbor4)
			{
				var originalIndex = _vertexRoots[vertex];
				_vertexRoots[vertex] = _vertexNeighbors.Count;
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor0));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor1));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor2));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor3));
				_vertexNeighbors.Add(new VertexNeighbor(originalIndex, neighbor4));
			}

			public void ExtendVertex(int vertex, int neighbor0, int neighbor1, int neighbor2, int neighbor3, int neighbor4, int neighbor5)
			{
				var originalIndex = _vertexRoots[vertex];
				_vertexRoots[vertex] = _vertexNeighbors.Count;
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor0));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor1));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor2));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor3));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor4));
				_vertexNeighbors.Add(new VertexNeighbor(originalIndex, neighbor5));
			}

			public void ExtendVertex(int vertex, params int[] neighbors)
			{
				ExtendVertex(vertex, neighbors.Length, neighbors);
			}

			public void ExtendVertex(int vertex, int neighborCount, int[] neighbors)
			{
				var originalIndex = _vertexRoots[vertex];
				_vertexRoots[vertex] = _vertexNeighbors.Count;
				int iEnd = neighborCount - 1;
				for (int i = 0; i < iEnd; ++i)
				{
					_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbors[i]));
				}
				_vertexNeighbors.Add(new VertexNeighbor(originalIndex, neighbors[iEnd]));
			}

			private int FindVertexNeighborIndex(int vertexIndex, int neighborVertexIndex)
			{
				var neighborIndex = _vertexRoots[vertexIndex];
				while (neighborIndex != -1)
				{
					if (_vertexNeighbors[neighborIndex]._vertex == neighborVertexIndex) return neighborIndex;
					neighborIndex = _vertexNeighbors[neighborIndex]._next;
				}
				throw new System.ArgumentException("The vertex after which the new neighbors were to be inserted is not already recorded as a neighbor vertex.");
			}

			public void ExtendVertexAfter(int vertexIndex, int insertAfterVertexIndex, int neighbor0)
			{
				var neighborIndex = FindVertexNeighborIndex(vertexIndex, insertAfterVertexIndex);
				var nextneighborIndex = _vertexNeighbors[neighborIndex]._next;
				_vertexNeighbors[neighborIndex] = new VertexNeighbor(_vertexNeighbors.Count, insertAfterVertexIndex);
				_vertexNeighbors.Add(new VertexNeighbor(nextneighborIndex, neighbor0));
			}

			public void ExtendVertexAfter(int vertexIndex, int insertAfterVertexIndex, int neighbor0, int neighbor1)
			{
				var neighborIndex = FindVertexNeighborIndex(vertexIndex, insertAfterVertexIndex);
				var nextneighborIndex = _vertexNeighbors[neighborIndex]._next;
				_vertexNeighbors[neighborIndex] = new VertexNeighbor(_vertexNeighbors.Count, insertAfterVertexIndex);
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor0));
				_vertexNeighbors.Add(new VertexNeighbor(nextneighborIndex, neighbor1));
			}

			public void ExtendVertexAfter(int vertexIndex, int insertAfterVertexIndex, int neighbor0, int neighbor1, int neighbor2)
			{
				var neighborIndex = FindVertexNeighborIndex(vertexIndex, insertAfterVertexIndex);
				var nextneighborIndex = _vertexNeighbors[neighborIndex]._next;
				_vertexNeighbors[neighborIndex] = new VertexNeighbor(_vertexNeighbors.Count, insertAfterVertexIndex);
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor0));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor1));
				_vertexNeighbors.Add(new VertexNeighbor(nextneighborIndex, neighbor2));
			}

			public void ExtendVertexAfter(int vertexIndex, int insertAfterVertexIndex, int neighbor0, int neighbor1, int neighbor2, int neighbor3)
			{
				var neighborIndex = FindVertexNeighborIndex(vertexIndex, insertAfterVertexIndex);
				var nextneighborIndex = _vertexNeighbors[neighborIndex]._next;
				_vertexNeighbors[neighborIndex] = new VertexNeighbor(_vertexNeighbors.Count, insertAfterVertexIndex);
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor0));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor1));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor2));
				_vertexNeighbors.Add(new VertexNeighbor(nextneighborIndex, neighbor3));
			}

			public void ExtendVertexAfter(int vertexIndex, int insertAfterVertexIndex, int neighbor0, int neighbor1, int neighbor2, int neighbor3, int neighbor4)
			{
				var neighborIndex = FindVertexNeighborIndex(vertexIndex, insertAfterVertexIndex);
				var nextneighborIndex = _vertexNeighbors[neighborIndex]._next;
				_vertexNeighbors[neighborIndex] = new VertexNeighbor(_vertexNeighbors.Count, insertAfterVertexIndex);
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor0));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor1));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor2));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor3));
				_vertexNeighbors.Add(new VertexNeighbor(nextneighborIndex, neighbor4));
			}

			public void ExtendVertexAfter(int vertexIndex, int insertAfterVertexIndex, int neighbor0, int neighbor1, int neighbor2, int neighbor3, int neighbor4, int neighbor5)
			{
				var neighborIndex = FindVertexNeighborIndex(vertexIndex, insertAfterVertexIndex);
				var nextneighborIndex = _vertexNeighbors[neighborIndex]._next;
				_vertexNeighbors[neighborIndex] = new VertexNeighbor(_vertexNeighbors.Count, insertAfterVertexIndex);
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor0));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor1));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor2));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor3));
				_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbor4));
				_vertexNeighbors.Add(new VertexNeighbor(nextneighborIndex, neighbor5));
			}

			public void ExtendVertexAfter(int vertexIndex, int insertAfterVertexIndex, params int[] neighbors)
			{
				ExtendVertexAfter(vertexIndex, insertAfterVertexIndex, neighbors.Length, neighbors);
			}

			public void ExtendVertexAfter(int vertexIndex, int insertAfterVertexIndex, int neighborCount, int[] neighbors)
			{
				var neighborIndex = FindVertexNeighborIndex(vertexIndex, insertAfterVertexIndex);
				var nextneighborIndex = _vertexNeighbors[neighborIndex]._next;
				int iEnd = neighborCount - 1;
				for (int i = 0; i < iEnd; ++i)
				{
					_vertexNeighbors.Add(new VertexNeighbor(_vertexNeighbors.Count + 1, neighbors[i]));
				}
				_vertexNeighbors.Add(new VertexNeighbor(nextneighborIndex, neighbors[iEnd]));
			}

			public Topology BuildTopology()
			{
				var topology = new Topology();

				topology._vertexData = new NodeData[_vertexRoots.Count];
				topology._edgeData = new EdgeData[_vertexNeighbors.Count];

				int edgeIndex = 0;

				for (int vertexIndex = 0; vertexIndex < _vertexRoots.Count; ++vertexIndex)
				{
					var bufferIndex = _vertexRoots[vertexIndex];
					var firstEdgeIndex = edgeIndex;
					while (bufferIndex != -1)
					{
						var neighborVertexIndex = _vertexNeighbors[bufferIndex]._vertex;
						topology._edgeData[edgeIndex]._vertex = neighborVertexIndex;
						topology._edgeData[edgeIndex]._face = -1;

						if (neighborVertexIndex < vertexIndex)
						{
							var firstNeighborEdgeIndex = topology._vertexData[neighborVertexIndex].firstEdge;
							var neighborEdgeIndex = firstNeighborEdgeIndex;
							while (topology._edgeData[neighborEdgeIndex]._vertex != vertexIndex)
							{
								neighborEdgeIndex = topology._edgeData[neighborEdgeIndex]._next;
								if (neighborEdgeIndex == firstNeighborEdgeIndex) throw new System.InvalidOperationException("Two vertices that were set as neighbors in one direction were not also set as neighbors in the opposite direction.");
							}
							topology._edgeData[neighborEdgeIndex]._twin = edgeIndex;
							topology._edgeData[edgeIndex]._twin = neighborEdgeIndex;
						}

						var nextEdgeIndex = edgeIndex + 1;
						topology._edgeData[edgeIndex]._next = nextEdgeIndex;
						topology._edgeData[nextEdgeIndex]._prev = edgeIndex;
						edgeIndex = nextEdgeIndex;
						bufferIndex = _vertexNeighbors[bufferIndex]._next;
					}

					var lastEdgeIndex = edgeIndex - 1;
					topology._edgeData[firstEdgeIndex]._prev = lastEdgeIndex;
					topology._edgeData[lastEdgeIndex]._next = firstEdgeIndex;
					topology._vertexData[vertexIndex] = new NodeData(edgeIndex - firstEdgeIndex, firstEdgeIndex);
				}

				int faceIndex = 0;

				for (edgeIndex = 0; edgeIndex < _vertexNeighbors.Count; ++edgeIndex)
				{
					if (topology._edgeData[edgeIndex]._face == -1)
					{
						var faceEdgeIndex = edgeIndex;
						do
						{
							topology._edgeData[edgeIndex]._face = faceIndex;
							faceEdgeIndex = topology._edgeData[topology._edgeData[faceEdgeIndex]._twin]._prev;
						} while (faceEdgeIndex != edgeIndex);
					}
				}

				return topology;
			}
		}
	}
}
