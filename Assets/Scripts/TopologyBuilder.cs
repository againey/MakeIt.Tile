using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Tiling
{
	public partial class Topology
	{
		public class Builder
		{
			private int _vertexCount = 0;
			private readonly List<int> _vertexNeighbors = new List<int>();

			public Builder()
			{
			}

			public Builder(int vertexCount, int edgeCount, int faceCount)
			{
				_vertexNeighbors.Capacity = vertexCount + edgeCount * 2;
			}

			public void AddVertex(int neighbor0, int neighbor1, int neighbor2)
			{
				++_vertexCount;
				_vertexNeighbors.Add(3);
				_vertexNeighbors.Add(neighbor0);
				_vertexNeighbors.Add(neighbor1);
				_vertexNeighbors.Add(neighbor2);
			}

			public void AddVertex(int neighbor0, int neighbor1, int neighbor2, int neighbor3)
			{
				++_vertexCount;
				_vertexNeighbors.Add(4);
				_vertexNeighbors.Add(neighbor0);
				_vertexNeighbors.Add(neighbor1);
				_vertexNeighbors.Add(neighbor2);
				_vertexNeighbors.Add(neighbor3);
			}

			public void AddVertex(int neighbor0, int neighbor1, int neighbor2, int neighbor3, int neighbor4)
			{
				++_vertexCount;
				_vertexNeighbors.Add(5);
				_vertexNeighbors.Add(neighbor0);
				_vertexNeighbors.Add(neighbor1);
				_vertexNeighbors.Add(neighbor2);
				_vertexNeighbors.Add(neighbor3);
				_vertexNeighbors.Add(neighbor4);
			}

			public void AddVertex(int neighbor0, int neighbor1, int neighbor2, int neighbor3, int neighbor4, int neighbor5)
			{
				++_vertexCount;
				_vertexNeighbors.Add(6);
				_vertexNeighbors.Add(neighbor0);
				_vertexNeighbors.Add(neighbor1);
				_vertexNeighbors.Add(neighbor2);
				_vertexNeighbors.Add(neighbor3);
				_vertexNeighbors.Add(neighbor4);
				_vertexNeighbors.Add(neighbor5);
			}

			public void AddVertex(params int[] neighbors)
			{
				++_vertexCount;
				_vertexNeighbors.Add(neighbors.Length);
				_vertexNeighbors.AddRange(neighbors);
			}

			private void InitializeVertexNeighbor(Topology topology, int vertexIndex, int vertexNeighborIndex, ref int edgeIndex, ref int inputIndex)
			{
				var neighborVertexIndex = _vertexNeighbors[inputIndex++];
				topology._vertexNeighbors[vertexNeighborIndex]._vertex = neighborVertexIndex;
				topology._vertexNeighbors[vertexNeighborIndex]._face = -1;

				if (vertexIndex < neighborVertexIndex)
				{
					topology._vertexNeighbors[vertexNeighborIndex]._edge = edgeIndex;
					topology._edgeNeighbors[edgeIndex, 0] = new EdgeNeighbor(vertexIndex, -1);
					topology._edgeNeighbors[edgeIndex, 1] = new EdgeNeighbor(neighborVertexIndex, -1);
				}
				else
				{
					var neighborNeighborIndex = topology._vertexRoots[neighborVertexIndex].rootIndex;
					while (topology._vertexNeighbors[neighborNeighborIndex]._vertex != vertexIndex)
					{
						neighborNeighborIndex = topology._vertexNeighbors[neighborNeighborIndex]._next;
					}
					topology._vertexNeighbors[vertexNeighborIndex]._edge = topology._vertexNeighbors[neighborNeighborIndex]._edge;
					++edgeIndex;
				}
			}

			private void InitializeFace(Topology topology, int vertexIndex, int vertexNeighborIndex, ref int faceIndex, ref int faceNeighborIndex)
			{
				var neighborCount = 0;
				var rootIndex = faceNeighborIndex;

				var prevVertexIndex = vertexIndex;

				do
				{
					var nextVertexIndex = topology._vertexNeighbors[vertexNeighborIndex]._vertex;
					var edgeIndex = topology._vertexNeighbors[vertexNeighborIndex]._edge;

					topology._vertexNeighbors[vertexNeighborIndex]._face = faceIndex;

					var edgeNeighborIndex = (topology._edgeNeighbors[edgeIndex, 0]._vertex == prevVertexIndex ? 0 : 1);
					var edgeOppositeNeighborIndex = -edgeNeighborIndex + 1;
					topology._edgeNeighbors[edgeIndex, edgeNeighborIndex]._face = faceIndex;

					topology._faceNeighbors[faceNeighborIndex]._prev = faceNeighborIndex - 1;
					topology._faceNeighbors[faceNeighborIndex]._next = faceNeighborIndex + 1;
					topology._faceNeighbors[faceNeighborIndex]._vertex = nextVertexIndex;
					topology._faceNeighbors[faceNeighborIndex]._edge = edgeIndex;

					var oppositeFaceIndex = topology._edgeNeighbors[edgeIndex, edgeOppositeNeighborIndex]._face;
					topology._faceNeighbors[faceNeighborIndex]._face = oppositeFaceIndex;
					if (oppositeFaceIndex != -1)
					{
						var neighborNeighborIndex = topology._faceRoots[oppositeFaceIndex].rootIndex;
						while (topology._faceNeighbors[neighborNeighborIndex]._edge != edgeIndex)
						{
							neighborNeighborIndex = topology._faceNeighbors[neighborNeighborIndex]._next;
						}
						topology._faceNeighbors[neighborNeighborIndex]._face = faceIndex;
					}

					vertexNeighborIndex = topology._vertexRoots[nextVertexIndex].rootIndex;
					while (topology._vertexNeighbors[vertexNeighborIndex]._vertex != prevVertexIndex)
					{
						vertexNeighborIndex = topology._vertexNeighbors[vertexNeighborIndex]._next;
					}
					vertexNeighborIndex = topology._vertexNeighbors[vertexNeighborIndex]._prev;
					prevVertexIndex = nextVertexIndex;

					++faceNeighborIndex;
					++neighborCount;
				} while (prevVertexIndex != vertexIndex);

				topology._faceRoots[faceIndex] = new FaceRoot(neighborCount, rootIndex);

				++faceIndex;
			}

			public Topology BuildTopology()
			{
				var topology = new Topology();

				topology._vertexRoots = new VertexRoot[_vertexCount];
				topology._vertexNeighbors = new VertexNeighbor[_vertexNeighbors.Count - _vertexCount];
				topology._edgeNeighbors = new EdgeNeighbor[(_vertexNeighbors.Count - _vertexCount) / 2, 2];

				int vertexIndex = 0;
				int vertexNeighborIndex = 0;
				int edgeIndex = 0;
				int inputIndex = 0;
				while (inputIndex < _vertexNeighbors.Count)
				{
					var neighborCount = _vertexNeighbors[inputIndex++];
					var lastIndex = vertexNeighborIndex + neighborCount - 1;
					topology._vertexRoots[vertexIndex] = new VertexRoot(neighborCount, vertexNeighborIndex);

					topology._vertexNeighbors[lastIndex]._next = vertexNeighborIndex;
					topology._vertexNeighbors[vertexNeighborIndex]._prev = lastIndex;
					while (vertexNeighborIndex < lastIndex)
					{
						InitializeVertexNeighbor(topology, vertexIndex, vertexNeighborIndex, ref edgeIndex, ref inputIndex);

						var nextNeighborIndex = vertexNeighborIndex + 1;
						topology._vertexNeighbors[vertexNeighborIndex]._next = nextNeighborIndex;
						topology._vertexNeighbors[nextNeighborIndex]._prev = vertexNeighborIndex;
						vertexNeighborIndex = nextNeighborIndex;
					}

					InitializeVertexNeighbor(topology, vertexIndex, vertexNeighborIndex, ref edgeIndex, ref inputIndex);

					++vertexNeighborIndex;
					++vertexIndex;
				}

				if (vertexIndex != topology._vertexRoots.Length) throw new System.InvalidOperationException("The input vertex data did not generate the expected number of final vertices.");
				if (vertexNeighborIndex != topology._vertexNeighbors.Length) throw new System.InvalidOperationException("The input vertex data did not generate the expected number of final vertex neighbors.");
				if (edgeIndex != topology._edgeNeighbors.GetLength(0)) throw new System.InvalidOperationException("The input vertex data did not generate the expected number of final edges.");

				int faceIndex = 0;
				int faceNeighborIndex = 0;

				for (vertexIndex = 0; vertexIndex < topology._vertexRoots.Length; ++vertexIndex)
				{
					var neighborCount = topology._vertexRoots[vertexIndex].neighborCount;
					var neighborIndex = topology._vertexRoots[vertexIndex].rootIndex;
					while (neighborCount > 0)
					{
						if (topology._vertexNeighbors[neighborIndex]._face == -1)
						{
							InitializeFace(topology, vertexIndex, neighborIndex, ref faceIndex, ref faceNeighborIndex);
						}

						neighborIndex = topology._vertexNeighbors[neighborIndex]._next;
						--neighborCount;
					}
				}

				return topology;
			}
		}
	}
}
