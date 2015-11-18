using System.Collections.Generic;

namespace Experilous.Topological
{
	public partial class Topology
	{
		public class FaceVerticesBuilder
		{
			private struct FaceVertex
			{
				public int _next;
				public int _vertex;

				public FaceVertex(int vertex) { _next = -1; _vertex = vertex; }
				public FaceVertex(int next, int vertex) { _next = next; _vertex = vertex; }
			}

			private readonly List<int> _faceRoots = new List<int>();
			private readonly List<FaceVertex> _faceVertices = new List<FaceVertex>();

			private int _maxVertexIndex = -1;

			public FaceVerticesBuilder()
			{
			}

			public FaceVerticesBuilder(int faceCount)
			{
				_faceRoots.Capacity = faceCount;
			}

			public FaceVerticesBuilder(int faceCount, int edgeCount)
			{
				_faceRoots.Capacity = faceCount;
				_faceVertices.Capacity = edgeCount * 2;
			}

			public int faceCount { get { return _faceRoots.Count; } }

			public int AddFace()
			{
				var faceIndex = _faceRoots.Count;
				_faceRoots.Add(-1);
				return faceIndex;
			}

			public int AddFace(int vertex0, int vertex1, int vertex2)
			{
				var faceIndex = _faceRoots.Count;
				var rootIndex = _faceVertices.Count;
				_faceRoots.Add(rootIndex);
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex0));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex1));
				_faceVertices.Add(new FaceVertex(rootIndex, vertex2));
				_maxVertexIndex = UnityEngine.Mathf.Max(_maxVertexIndex, vertex0, vertex1, vertex2);
				return faceIndex;
			}

			public int AddFace(int vertex0, int vertex1, int vertex2, int vertex3)
			{
				var faceIndex = _faceRoots.Count;
				var rootIndex = _faceVertices.Count;
				_faceRoots.Add(rootIndex);
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex0));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex1));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex2));
				_faceVertices.Add(new FaceVertex(rootIndex, vertex3));
				_maxVertexIndex = UnityEngine.Mathf.Max(_maxVertexIndex, vertex0, vertex1, vertex2, vertex3);
				return faceIndex;
			}

			public int AddFace(int vertex0, int vertex1, int vertex2, int vertex3, int vertex4)
			{
				var faceIndex = _faceRoots.Count;
				var rootIndex = _faceVertices.Count;
				_faceRoots.Add(rootIndex);
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex0));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex1));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex2));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex3));
				_faceVertices.Add(new FaceVertex(rootIndex, vertex4));
				_maxVertexIndex = UnityEngine.Mathf.Max(_maxVertexIndex, vertex0, vertex1, vertex2, vertex3, vertex4);
				return faceIndex;
			}

			public int AddFace(int vertex0, int vertex1, int vertex2, int vertex3, int vertex4, int vertex5)
			{
				var faceIndex = _faceRoots.Count;
				var rootIndex = _faceVertices.Count;
				_faceRoots.Add(rootIndex);
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex0));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex1));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex2));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex3));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex4));
				_faceVertices.Add(new FaceVertex(rootIndex, vertex5));
				_maxVertexIndex = UnityEngine.Mathf.Max(_maxVertexIndex, vertex0, vertex1, vertex2, vertex3, vertex4, vertex5);
				return faceIndex;
			}

			public int AddFace(params int[] vertices)
			{
				return AddFace(vertices.Length, vertices);
			}

			public int AddFace(int vertexCount, int[] vertices)
			{
				var faceIndex = _faceRoots.Count;
				var rootIndex = _faceVertices.Count;
				_faceRoots.Add(rootIndex);
				int i = 0;
				int iEnd = vertexCount - 1;
				while (i < iEnd)
				{
					_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertices[i]));
					++i;
				}
				_faceVertices.Add(new FaceVertex(rootIndex, vertices[i]));
				_maxVertexIndex = UnityEngine.Mathf.Max(_maxVertexIndex, UnityEngine.Mathf.Max(vertices));
				return faceIndex;
			}

			private int FindLastIndex(int originalIndex)
			{
				var index = originalIndex;
				var nextIndex = _faceVertices[index]._next;
				while (nextIndex != originalIndex)
				{
					index = nextIndex;
					nextIndex = _faceVertices[index]._next;
				}
				return index;
			}

			private int PrepareToExtendFace(int face)
			{
				var rootIndex = _faceRoots[face];
				if (rootIndex != -1)
				{
					var index = FindLastIndex(rootIndex);
					_faceVertices[index] = new FaceVertex(_faceVertices.Count, _faceVertices[index]._vertex);
				}
				else
				{
					rootIndex = _faceVertices.Count;
					_faceRoots[face] = rootIndex;
				}
				return rootIndex;
			}

			public void ExtendFace(int face, int vertex0)
			{
				var originalIndex = PrepareToExtendFace(face);
				_faceVertices.Add(new FaceVertex(originalIndex, vertex0));
				_maxVertexIndex = UnityEngine.Mathf.Max(_maxVertexIndex, vertex0);
			}

			public void ExtendFace(int face, int vertex0, int vertex1)
			{
				var originalIndex = PrepareToExtendFace(face);
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex0));
				_faceVertices.Add(new FaceVertex(originalIndex, vertex1));
				_maxVertexIndex = UnityEngine.Mathf.Max(_maxVertexIndex, vertex0, vertex1);
			}

			public void ExtendFace(int face, int vertex0, int vertex1, int vertex2)
			{
				var originalIndex = PrepareToExtendFace(face);
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex0));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex1));
				_faceVertices.Add(new FaceVertex(originalIndex, vertex2));
				_maxVertexIndex = UnityEngine.Mathf.Max(_maxVertexIndex, vertex0, vertex1, vertex2);
			}

			public void ExtendFace(int face, int vertex0, int vertex1, int vertex2, int vertex3)
			{
				var originalIndex = PrepareToExtendFace(face);
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex0));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex1));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex2));
				_faceVertices.Add(new FaceVertex(originalIndex, vertex3));
				_maxVertexIndex = UnityEngine.Mathf.Max(_maxVertexIndex, vertex0, vertex1, vertex2, vertex3);
			}

			public void ExtendFace(int face, int vertex0, int vertex1, int vertex2, int vertex3, int vertex4)
			{
				var originalIndex = PrepareToExtendFace(face);
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex0));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex1));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex2));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex3));
				_faceVertices.Add(new FaceVertex(originalIndex, vertex4));
				_maxVertexIndex = UnityEngine.Mathf.Max(_maxVertexIndex, vertex0, vertex1, vertex2, vertex3, vertex4);
			}

			public void ExtendFace(int face, int vertex0, int vertex1, int vertex2, int vertex3, int vertex4, int vertex5)
			{
				var originalIndex = PrepareToExtendFace(face);
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex0));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex1));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex2));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex3));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex4));
				_faceVertices.Add(new FaceVertex(originalIndex, vertex5));
				_maxVertexIndex = UnityEngine.Mathf.Max(_maxVertexIndex, vertex0, vertex1, vertex2, vertex3, vertex4, vertex5);
			}

			public void ExtendFace(int face, params int[] vertices)
			{
				ExtendFace(face, vertices.Length, vertices);
			}

			public void ExtendFace(int face, int vertexCount, int[] vertices)
			{
				var originalIndex = PrepareToExtendFace(face);
				int iEnd = vertexCount - 1;
				for (int i = 0; i < iEnd; ++i)
				{
					_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertices[i]));
				}
				_faceVertices.Add(new FaceVertex(originalIndex, vertices[iEnd]));
				_maxVertexIndex = UnityEngine.Mathf.Max(_maxVertexIndex, UnityEngine.Mathf.Max(vertices));
			}

			private int FindFaceVertexIndex(int faceVertex, int targetVertex)
			{
				var firstFaceVertex = faceVertex;
				do
				{
					if (_faceVertices[faceVertex]._vertex == targetVertex) return faceVertex;
					faceVertex = _faceVertices[faceVertex]._next;
				} while (faceVertex != firstFaceVertex);
				throw new System.ArgumentException("The vertex specified was not yet added to the face.");
			}

			private int PrepareToExtendFace(int face, int insertAfterVertex)
			{
				var rootIndex = _faceRoots[face];
				if (rootIndex != -1)
				{
					var index = FindFaceVertexIndex(rootIndex, insertAfterVertex);
					var nextIndex = _faceVertices[index]._next;
					_faceVertices[index] = new FaceVertex(_faceVertices.Count, insertAfterVertex);
					return nextIndex;
				}
				else
				{
					rootIndex = _faceVertices.Count;
					_faceRoots[face] = rootIndex;
					return rootIndex;
				}
			}

			public void ExtendFaceAfter(int face, int insertAfterVertex, int vertex0)
			{
				var nextFaceVertex = PrepareToExtendFace(face, insertAfterVertex);
				_faceVertices.Add(new FaceVertex(nextFaceVertex, vertex0));
				_maxVertexIndex = UnityEngine.Mathf.Max(_maxVertexIndex, vertex0);
			}

			public void ExtendFaceAfter(int face, int insertAfterVertex, int vertex0, int vertex1)
			{
				var nextFaceVertex = PrepareToExtendFace(face, insertAfterVertex);
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex0));
				_faceVertices.Add(new FaceVertex(nextFaceVertex, vertex1));
				_maxVertexIndex = UnityEngine.Mathf.Max(_maxVertexIndex, vertex0, vertex1);
			}

			public void ExtendFaceAfter(int face, int insertAfterVertex, int vertex0, int vertex1, int vertex2)
			{
				var nextFaceVertex = PrepareToExtendFace(face, insertAfterVertex);
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex0));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex1));
				_faceVertices.Add(new FaceVertex(nextFaceVertex, vertex2));
				_maxVertexIndex = UnityEngine.Mathf.Max(_maxVertexIndex, vertex0, vertex1, vertex2);
			}

			public void ExtendFaceAfter(int face, int insertAfterVertex, int vertex0, int vertex1, int vertex2, int vertex3)
			{
				var nextFaceVertex = PrepareToExtendFace(face, insertAfterVertex);
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex0));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex1));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex2));
				_faceVertices.Add(new FaceVertex(nextFaceVertex, vertex3));
				_maxVertexIndex = UnityEngine.Mathf.Max(_maxVertexIndex, vertex0, vertex1, vertex2, vertex3);
			}

			public void ExtendFaceAfter(int face, int insertAfterVertex, int vertex0, int vertex1, int vertex2, int vertex3, int vertex4)
			{
				var nextFaceVertex = PrepareToExtendFace(face, insertAfterVertex);
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex0));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex1));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex2));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex3));
				_faceVertices.Add(new FaceVertex(nextFaceVertex, vertex4));
				_maxVertexIndex = UnityEngine.Mathf.Max(_maxVertexIndex, vertex0, vertex1, vertex2, vertex3, vertex4);
			}

			public void ExtendFaceAfter(int face, int insertAfterVertex, int vertex0, int vertex1, int vertex2, int vertex3, int vertex4, int vertex5)
			{
				var nextFaceVertex = PrepareToExtendFace(face, insertAfterVertex);
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex0));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex1));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex2));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex3));
				_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertex4));
				_faceVertices.Add(new FaceVertex(nextFaceVertex, vertex5));
				_maxVertexIndex = UnityEngine.Mathf.Max(_maxVertexIndex, vertex0, vertex1, vertex2, vertex3, vertex4, vertex5);
			}

			public void ExtendFaceAfter(int face, int insertAfterVertex, params int[] vertices)
			{
				ExtendFaceAfter(face, insertAfterVertex, vertices.Length, vertices);
			}

			public void ExtendFaceAfter(int face, int insertAfterVertex, int vertexCount, int[] vertices)
			{
				var nextFaceVertex = PrepareToExtendFace(face, insertAfterVertex);
				int iEnd = vertexCount - 1;
				for (int i = 0; i < iEnd; ++i)
				{
					_faceVertices.Add(new FaceVertex(_faceVertices.Count + 1, vertices[i]));
				}
				_faceVertices.Add(new FaceVertex(nextFaceVertex, vertices[iEnd]));
				_maxVertexIndex = UnityEngine.Mathf.Max(_maxVertexIndex, UnityEngine.Mathf.Max(vertices));
			}

			private void AddEdgeToVertex(NodeData[] vertexData, EdgeData[] edgeData, int vertex, int edge)
			{
				if (!vertexData[vertex].isEmpty)
				{
					var firstVertexEdge = vertexData[vertex].firstEdge;
					var vertexEdge = firstVertexEdge;
					var nextVertexEdge = edgeData[vertexEdge]._vNext;
					while (nextVertexEdge != firstVertexEdge)
					{
						vertexEdge = nextVertexEdge;
						nextVertexEdge = edgeData[vertexEdge]._vNext;
					}
					edgeData[vertexEdge]._vNext = edge;
					edgeData[edge]._vNext = firstVertexEdge;
					vertexData[vertex].neighborCount += 1;
				}
				else
				{
					vertexData[vertex] = new NodeData(1, edge);
					edgeData[edge]._vNext = edge;
				}
			}

			private void PutEdgesInSequence(EdgeData[] edgeData, int edge0, int edge1)
			{
				var firstEdge = edgeData[edge0]._vNext;
				if (firstEdge != edge1)
				{
					var edge = firstEdge;
					var nextEdge = edgeData[edge]._vNext;
					while (nextEdge != edge1)
					{
						edge = nextEdge;
						nextEdge = edgeData[edge]._vNext;
					}
					edgeData[edge]._vNext = edgeData[nextEdge]._vNext;
					edgeData[nextEdge]._vNext = firstEdge;
					edgeData[edge0]._vNext = edge1;
				}
			}

			public Topology BuildTopology()
			{
				var topology = new Topology();

				var vertexData = new NodeData[_maxVertexIndex + 1];
				var interiorEdgeData = new EdgeData[_faceVertices.Count];
				var faceData = new NodeData[_faceRoots.Count];

				// Create all the edges with full vertex and face references, correct but
				// unspecified-order vNext references, invalid twin references, and unset
				// fNext references.
				int nextEdge = 0;
				for (int face = 0; face < _faceRoots.Count; ++face)
				{
					var faceVertex0 = _faceRoots[face];
					var faceVertex1 = _faceVertices[faceVertex0]._next;
					var firstFaceVertex = faceVertex0;
					do
					{
						interiorEdgeData[nextEdge]._vertex = _faceVertices[faceVertex0]._vertex;
						interiorEdgeData[nextEdge]._face = face;
						interiorEdgeData[nextEdge]._twin = -1;

						AddEdgeToVertex(vertexData, interiorEdgeData, _faceVertices[faceVertex1]._vertex, nextEdge);

						++nextEdge;
						faceVertex0 = faceVertex1;
						faceVertex1 = _faceVertices[faceVertex0]._next;
					} while (faceVertex0 != firstFaceVertex);
				}

				int boundaryEdgeCount = 0;

				// Use the limited information constructed in the prior loop to determine
				// interior edge twins and count boundary edges.
				int edge = 0;
				for (int face = 0; face < _faceRoots.Count; ++face)
				{
					var faceVertex0 = _faceRoots[face];
					var faceVertex1 = _faceVertices[faceVertex0]._next;

					var firstFaceVertex = faceVertex0;
					do
					{
						if (interiorEdgeData[edge]._twin == -1)
						{
							var firstVertexEdge = vertexData[_faceVertices[faceVertex0]._vertex].firstEdge;
							var vertexEdge = firstVertexEdge;
							var targetVertex = _faceVertices[faceVertex1]._vertex;
							while (interiorEdgeData[vertexEdge]._vertex != targetVertex)
							{
								vertexEdge = interiorEdgeData[vertexEdge]._vNext;
								if (vertexEdge == firstVertexEdge)
								{
									++boundaryEdgeCount;
									goto nextFaceVertex;
								}
							}
							interiorEdgeData[vertexEdge]._twin = edge;
							interiorEdgeData[edge]._twin = vertexEdge;
						}

						nextFaceVertex:  ++edge;
						faceVertex0 = faceVertex1;
						faceVertex1 = _faceVertices[faceVertex0]._next;
					} while (faceVertex0 != firstFaceVertex);
				}

				EdgeData[] edgeData;

				if (boundaryEdgeCount == 0)
				{
					edgeData = interiorEdgeData;
				}
				else
				{
					// Finish the edge twin process by creating the boundary edges and
					// setting their twins.
					edgeData = new EdgeData[interiorEdgeData.Length + boundaryEdgeCount];
					System.Array.Copy(interiorEdgeData, edgeData, interiorEdgeData.Length);
					edge = 0;
					for (int face = 0; face < _faceRoots.Count; ++face)
					{
						var faceVertex0 = _faceRoots[face];
						var faceVertex1 = _faceVertices[faceVertex0]._next;

						var firstFaceVertex = faceVertex0;
						do
						{
							if (edgeData[edge]._twin == -1)
							{
								// This edge is twinned with a boundary edge that hasn't been created yet.
								edgeData[nextEdge]._vertex = _faceVertices[faceVertex1]._vertex;
								edgeData[nextEdge]._face = -1;

								AddEdgeToVertex(vertexData, edgeData, _faceVertices[faceVertex0]._vertex, nextEdge);

								edgeData[nextEdge]._twin = edge;
								edgeData[edge]._twin = nextEdge;

								++nextEdge;
							}

							++edge;
							faceVertex0 = faceVertex1;
							faceVertex1 = _faceVertices[faceVertex0]._next;
						} while (faceVertex0 != firstFaceVertex);
					}
				}

				// Correct the order of the vNext linked lists, and construct the
				// fNext linked lists and set the face roots and neighbor counts.
				edge = 0;
				for (int face = 0; face < _faceRoots.Count; ++face)
				{
					int neighborCount = 0;

					var faceVertex0 = _faceRoots[face];
					var faceVertex1 = _faceVertices[faceVertex0]._next;
					var faceVertex2 = _faceVertices[faceVertex1]._next;

					var firstEdge = edge;
					nextEdge = edge + 1;

					var firstFaceVertex = faceVertex0;
					do
					{
						PutEdgesInSequence(edgeData, edgeData[nextEdge]._twin, edge);

						edgeData[edgeData[edge]._twin]._fNext = edgeData[nextEdge]._twin;
						++neighborCount;

						edge = nextEdge;
						nextEdge = (faceVertex2 != firstFaceVertex) ? edge + 1 : firstEdge;
						faceVertex0 = faceVertex1;
						faceVertex1 = faceVertex2;
						faceVertex2 = _faceVertices[faceVertex1]._next;
					} while (faceVertex0 != firstFaceVertex);

					faceData[face] = new NodeData(neighborCount, edgeData[firstEdge]._twin);
					edge = firstEdge + neighborCount;
				}

				topology._vertexData = vertexData;
				topology._edgeData = edgeData;
				topology._faceData = faceData;

				return topology;
			}
		}
	}
}
