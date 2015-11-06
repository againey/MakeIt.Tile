using System.Collections.Generic;

namespace Experilous.Topological
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

			private int _vertexCount = -1;
			private int _edgeCount = -1;
			private int _faceCount = -1;

			public Builder()
			{
			}

			public Builder(int vertexCount, int edgeCount, int faceCount)
			{
				_vertexRoots.Capacity = vertexCount;
				_vertexNeighbors.Capacity = vertexCount + edgeCount * 3 / 2;

				_vertexCount = vertexCount;
				_edgeCount = edgeCount;
				_faceCount = faceCount;
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

				if (_vertexCount != -1 && _vertexRoots.Count != _vertexCount) throw new System.InvalidOperationException("The actual number of vertices does not match the number specified when the topology builder was first constructed.");
				if (_edgeCount != -1 && _vertexNeighbors.Count != _edgeCount) throw new System.InvalidOperationException("The actual number of edges does not match the number specified when the topology builder was first constructed.");

				topology._vertexData = new NodeData[_vertexRoots.Count];
				topology._edgeData = new EdgeData[_vertexNeighbors.Count];

				// Build all of the vertex <-> edge relationships
				int edgeIndex = 0;
				for (int vertexIndex = 0; vertexIndex < _vertexRoots.Count; ++vertexIndex)
				{
					var bufferIndex = _vertexRoots[vertexIndex];
					var firstEdgeIndex = edgeIndex;
					while (bufferIndex != -1)
					{
						// Fill in edge details for the edge pointing from the current vertex to its neighbor.
						var neighborVertexIndex = _vertexNeighbors[bufferIndex]._vertex;
						topology._edgeData[edgeIndex]._vertex = neighborVertexIndex;
						topology._edgeData[edgeIndex]._face = -1;

						// Setup the linked list between this edge and the next, if this isn't the last neighbor of the vertex.
						bufferIndex = _vertexNeighbors[bufferIndex]._next;
						var nextEdgeIndex = edgeIndex + 1;
						if (bufferIndex != -1)
						{
							topology._edgeData[edgeIndex]._next = nextEdgeIndex;
							topology._edgeData[nextEdgeIndex]._prev = edgeIndex;
						}

						// If the neighbor has a smaller index than the current vertex, then the neighbor is known to already
						// have its neighbor relationships built.  Otherwise, the neighbor is not yet processed, and the
						// relationship between it and the current vertex must be processed later.
						if (neighborVertexIndex < vertexIndex)
						{
							// Search for which edge is the one pointing from the neighbor vertex to the current vertex, the
							// opposite direction from the edge configured above.
							var firstNeighborEdgeIndex = topology._vertexData[neighborVertexIndex].firstEdge;
							var neighborEdgeIndex = firstNeighborEdgeIndex;
							while (topology._edgeData[neighborEdgeIndex]._vertex != vertexIndex)
							{
								neighborEdgeIndex = topology._edgeData[neighborEdgeIndex]._next;
								if (neighborEdgeIndex == firstNeighborEdgeIndex) throw new System.InvalidOperationException("Two vertices that were set as neighbors in one direction were not also set as neighbors in the opposite direction.");
							}
							// Set the two edges to reference each either as twins.
							topology._edgeData[neighborEdgeIndex]._twin = edgeIndex;
							topology._edgeData[edgeIndex]._twin = neighborEdgeIndex;
						}

						edgeIndex = nextEdgeIndex;
					}

					// Finish the linked list by making it circular, wrapping around at the ends.
					var lastEdgeIndex = edgeIndex - 1;
					topology._edgeData[firstEdgeIndex]._prev = lastEdgeIndex;
					topology._edgeData[lastEdgeIndex]._next = firstEdgeIndex;
					// Store a link to the first edge created in this linked list, along with the number of edges.
					topology._vertexData[vertexIndex] = new NodeData(edgeIndex - firstEdgeIndex, firstEdgeIndex);
				}

				// If the face count wasn't given explicitly upfront, they must be counted first.
				if (_faceCount == -1)
				{
					// Now that all vertex and edge relationships are established, locate all edges that don't have a
					// face relationship specified (all of them at first) and spin around each face to establish those
					// edge -> face relationships and count the total number of faces.
					int faceIndex = 0;
					for (edgeIndex = 0; edgeIndex < _vertexNeighbors.Count; ++edgeIndex)
					{
						if (topology._edgeData[edgeIndex]._face == -1)
						{
							// Starting with the current edge, follow the edge links appropriatley to wind around the
							// implicit face clockwise and link the edges to the face.
							var neighborCount = 0;
							var faceEdgeIndex = edgeIndex;
							do
							{
								topology._edgeData[faceEdgeIndex]._face = faceIndex;
								faceEdgeIndex = topology._edgeData[topology._edgeData[faceEdgeIndex]._twin]._prev;
								++neighborCount;
								if (neighborCount > topology._vertexData.Length) throw new System.InvalidOperationException("Vertex neighbors were specified such that a face was misconfigured.");
							} while (faceEdgeIndex != edgeIndex);

							++faceIndex;
						}
					}

					// Allocate the array for face -> edge linkage, now that we know the number of faces, find the
					// first edge of every face and count the face's neighbors.
					topology._faceData = new NodeData[faceIndex];
					faceIndex = 0;
					for (edgeIndex = 0; edgeIndex < _vertexNeighbors.Count; ++edgeIndex)
					{
						// Give the above loop, the face index of edges in their current order are guaranteed to be
						// such that the all edges that refer to a face other than the re-incrementing face index
						// must have already been processed earlier in the loop and can be skipped.
						if (topology._edgeData[edgeIndex]._face == faceIndex)
						{
							// Starting with the current edge, follow the edge links appropriatley to wind around the
							// implicit face clockwise and link the edges to the face.
							var neighborCount = 0;
							var faceEdgeIndex = edgeIndex;
							do
							{
								faceEdgeIndex = topology._edgeData[topology._edgeData[faceEdgeIndex]._twin]._prev;
								++neighborCount;
								if (neighborCount > topology._vertexData.Length) throw new System.InvalidOperationException("Vertex neighbors were specified such that a face was misconfigured.");
							} while (faceEdgeIndex != edgeIndex);

							// Store the face count and link the face to the first edge.
							topology._faceData[faceIndex] = new NodeData(neighborCount, topology._edgeData[edgeIndex]._twin);
							++faceIndex;
						}
					}
				}
				else
				{
					// Now that all vertex and edge relationships are established, locate all edges that don't have a
					// face relationship specified (all of them at first) and spin around each face to establish those
					// edge -> face and face -> edge relationships.
					topology._faceData = new NodeData[_faceCount];
					int faceIndex = 0;
					for (edgeIndex = 0; edgeIndex < _vertexNeighbors.Count; ++edgeIndex)
					{
						if (topology._edgeData[edgeIndex]._face == -1)
						{
							if (faceIndex == _faceCount) throw new System.InvalidOperationException("The actual number of faces does not match the number specified when the topology builder was first constructed.");

							// Starting with the current edge, follow the edge links appropriatley to wind around the
							// implicit face clockwise and link the edges to the face.
							var neighborCount = 0;
							var faceEdgeIndex = edgeIndex;
							do
							{
								topology._edgeData[faceEdgeIndex]._face = faceIndex;
								faceEdgeIndex = topology._edgeData[topology._edgeData[faceEdgeIndex]._twin]._prev;
								++neighborCount;
								if (neighborCount > topology._vertexData.Length) throw new System.InvalidOperationException("Vertex neighbors were specified such that a face was misconfigured.");
							} while (faceEdgeIndex != edgeIndex);

							// Store the face count and link the face to the first edge.
							topology._faceData[faceIndex] = new NodeData(neighborCount, topology._edgeData[edgeIndex]._twin);
							++faceIndex;
						}
					}

					if (faceIndex != _faceCount) throw new System.InvalidOperationException("The actual number of faces does not match the number specified when the topology builder was first constructed.");
				}

				return topology;
			}
		}
	}
}
