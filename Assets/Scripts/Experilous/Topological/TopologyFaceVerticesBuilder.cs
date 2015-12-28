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
				_faceVertices.Capacity = edgeCount;
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

			private void AddEdgeToVertex(ushort[] vertexNeighborCounts, int[] vertexFirstEdgeIndices, EdgeData[] edgeData, int vertex, int edge)
			{
				if (vertexNeighborCounts[vertex] != 0)
				{
					var firstVertexEdge = vertexFirstEdgeIndices[vertex];
					var vertexEdge = firstVertexEdge;
					var nextVertexEdge = edgeData[vertexEdge]._vNext;
					while (nextVertexEdge != firstVertexEdge)
					{
						vertexEdge = nextVertexEdge;
						nextVertexEdge = edgeData[vertexEdge]._vNext;
					}
					edgeData[vertexEdge]._vNext = edge;
					edgeData[edge]._vNext = firstVertexEdge;
					vertexNeighborCounts[vertex] += 1;
				}
				else
				{
					vertexNeighborCounts[vertex] = 1;
					vertexFirstEdgeIndices[vertex] = edge;
					edgeData[edge]._vNext = edge;
				}
			}

			private void PutEdgesInSequence(EdgeData[] edgeData, int edge0, int edge1)
			{
				// Find out what comes immediately after edge0.
				var afterEdge0 = edgeData[edge0]._vNext;
				if (afterEdge0 != edge1)
				{
					// If it's not edge1, then search for edge1 and the edge that immediately preceeds it.
					var beforeEdge1 = afterEdge0;
					var nextEdge = edgeData[beforeEdge1]._vNext;
					while (nextEdge != edge1)
					{
						beforeEdge1 = nextEdge;
						nextEdge = edgeData[beforeEdge1]._vNext;
					}
					// The decision must be made whether to move edge0 to just before edge1, or move edge1
					// to just after edge0.  The choice must avoid breaking an ordered relationship that
					// has already been established previously.  It is known that any edge pointing to an
					// edge that has an index smaller than edge1 has already been processed.  It is also
					// known that the only two relations we care about are between beforeEdge0 and edge0,
					// and edge1 and the edge after edge1.  The other two potential relations are not
					// actually possible because we're in the process of establishing those relationships
					// right now; they couldn't possibly have been established differently before.
					// What that boils down to is that if edge0 is less than edge1, then we cannot move
					// edge0 and must move edge1 instead; otherwise, it is safe to move edge0 (and
					// undetermined and irrelevant whether it is safe to move edge1).
					if (edge0 < edge1)
					{
						// Move edge1 to the location immediately after edge0.
						edgeData[beforeEdge1]._vNext = edgeData[edge1]._vNext;
						edgeData[edge1]._vNext = afterEdge0;
						edgeData[edge0]._vNext = edge1;
					}
					else
					{
						// Move edge0 to the location immediately before edge1.
						// To do so, find the edge that immediately preceeds it.
						var beforeEdge0 = edge1;
						nextEdge = edgeData[beforeEdge0]._vNext;
						while (nextEdge != edge0)
						{
							beforeEdge0 = nextEdge;
							nextEdge = edgeData[beforeEdge0]._vNext;
						}
						edgeData[beforeEdge0]._vNext = afterEdge0;
						edgeData[beforeEdge1]._vNext = edge0;
						edgeData[edge0]._vNext = edge1;
					}
				}
			}

			public TTopology BuildTopology<TTopology>() where TTopology : Topology, new()
			{
				var vertexNeighborCounts = new ushort[_maxVertexIndex + 1];
				var vertexFirstEdgeIndices = new int[_maxVertexIndex + 1];
				var internalEdgeData = new EdgeData[_faceVertices.Count];
				var internalFaceNeighborCounts = new ushort[_faceRoots.Count];
				var internalFaceFirstEdgeIndices = new int[_faceRoots.Count];

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
						internalEdgeData[nextEdge]._vertex = _faceVertices[faceVertex0]._vertex;
						internalEdgeData[nextEdge]._face = face;
						internalEdgeData[nextEdge]._twin = -1;

						AddEdgeToVertex(vertexNeighborCounts, vertexFirstEdgeIndices, internalEdgeData, _faceVertices[faceVertex1]._vertex, nextEdge);

						++nextEdge;
						faceVertex0 = faceVertex1;
						faceVertex1 = _faceVertices[faceVertex0]._next;
					} while (faceVertex0 != firstFaceVertex);
				}

				int boundaryEdgeCount = 0;

				// Use the limited information constructed in the prior loop to determine
				// internal edge twins and count boundary edges.
				int edge = 0;
				for (int face = 0; face < _faceRoots.Count; ++face)
				{
					var faceVertex0 = _faceRoots[face];
					var faceVertex1 = _faceVertices[faceVertex0]._next;

					var firstFaceVertex = faceVertex0;
					do
					{
						if (internalEdgeData[edge]._twin == -1)
						{
							var firstVertexEdge = vertexFirstEdgeIndices[_faceVertices[faceVertex0]._vertex];
							var vertexEdge = firstVertexEdge;
							var targetVertex = _faceVertices[faceVertex1]._vertex;
							while (internalEdgeData[vertexEdge]._vertex != targetVertex)
							{
								vertexEdge = internalEdgeData[vertexEdge]._vNext;
								if (vertexEdge == firstVertexEdge)
								{
									++boundaryEdgeCount;
									goto nextFaceVertex;
								}
							}
							internalEdgeData[vertexEdge]._twin = edge;
							internalEdgeData[edge]._twin = vertexEdge;
						}

						nextFaceVertex:  ++edge;
						faceVertex0 = faceVertex1;
						faceVertex1 = _faceVertices[faceVertex0]._next;
					} while (faceVertex0 != firstFaceVertex);
				}

				EdgeData[] edgeData;

				if (boundaryEdgeCount == 0)
				{
					edgeData = internalEdgeData;
				}
				else
				{
					// Finish the edge twin process by creating the boundary edges and
					// setting their twins.
					edgeData = new EdgeData[internalEdgeData.Length + boundaryEdgeCount];
					System.Array.Copy(internalEdgeData, edgeData, internalEdgeData.Length);
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

								AddEdgeToVertex(vertexNeighborCounts, vertexFirstEdgeIndices, edgeData, _faceVertices[faceVertex0]._vertex, nextEdge);

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

					internalFaceNeighborCounts[face] = (ushort)(0x0000u | (neighborCount & 0x7FFFu));
					internalFaceFirstEdgeIndices[face] = edgeData[firstEdge]._twin;
					edge = firstEdge + neighborCount;
				}

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
						if (edgeData[edgeData[nextEdge]._twin]._vNext != edge)
							throw new System.InvalidOperationException();

						++neighborCount;

						edge = nextEdge;
						nextEdge = (faceVertex2 != firstFaceVertex) ? edge + 1 : firstEdge;
						faceVertex0 = faceVertex1;
						faceVertex1 = faceVertex2;
						faceVertex2 = _faceVertices[faceVertex1]._next;
					} while (faceVertex0 != firstFaceVertex);

					edge = firstEdge + neighborCount;
				}

				ushort[] faceNeighborCounts;
				int[] faceFirstEdgeIndices;

				if (boundaryEdgeCount == 0)
				{
					faceNeighborCounts = internalFaceNeighborCounts;
					faceFirstEdgeIndices = internalFaceFirstEdgeIndices;
				}
				else
				{
					// Now that all vertex and edge relationships are established, locate all edges that don't have a
					// face relationship specified (the boundary edges) and spin around each face to establish those
					// edge -> face relationships and count the total number of external faces.
					int faceIndex = internalFaceFirstEdgeIndices.Length;
					for (int edgeIndex = 0; edgeIndex < edgeData.Length; ++edgeIndex)
					{
						if (edgeData[edgeIndex]._face == -1)
						{
							// Starting with the current edge, follow the edge links appropriately to wind around the
							// implicit face counter-clockwise and link the edges to the face.
							var neighborCount = 0;
							var twinEdgeIndex = edgeIndex;
							var faceEdgeIndex = edgeData[edgeIndex]._twin;
							do
							{
								edgeData[twinEdgeIndex]._face = faceIndex;
								twinEdgeIndex = edgeData[faceEdgeIndex]._vNext;
								var nextFaceEdgeIndex = edgeData[twinEdgeIndex]._twin;
								edgeData[nextFaceEdgeIndex]._fNext = faceEdgeIndex;
								faceEdgeIndex = nextFaceEdgeIndex;
								++neighborCount;
								if (neighborCount > vertexFirstEdgeIndices.Length) throw new System.InvalidOperationException("Face neighbors were specified such that an external face was misconfigured.");
							} while (twinEdgeIndex != edgeIndex);

							++faceIndex;
						}
					}

					// Allocate the array for face -> edge linkage, now that we know the number of faces, find the
					// first edge of every face and count the face's neighbors.
					faceNeighborCounts = new ushort[faceIndex];
					faceFirstEdgeIndices = new int[faceIndex];
					System.Array.Copy(internalFaceNeighborCounts, faceNeighborCounts, internalFaceNeighborCounts.Length);
					System.Array.Copy(internalFaceFirstEdgeIndices, faceFirstEdgeIndices, internalFaceFirstEdgeIndices.Length);
					faceIndex = internalFaceFirstEdgeIndices.Length;
					for (int edgeIndex = 0; edgeIndex < edgeData.Length; ++edgeIndex)
					{
						// Give the above loop, the face index of edges in their current order are guaranteed to be
						// such that the all edges that refer to a face other than the re-incrementing face index
						// must have already been processed earlier in the loop and can be skipped.
						if (edgeData[edgeIndex]._face == faceIndex)
						{
							// Starting with the current edge, follow the edge links appropriately to wind around the
							// implicit face clockwise and link the edges to the face.
							var neighborCount = 0;
							var faceEdgeIndex = edgeData[edgeIndex]._twin;
							var firstFaceEdgeIndex = faceEdgeIndex;
							do
							{
								faceEdgeIndex = edgeData[faceEdgeIndex]._fNext;
								++neighborCount;
								if (neighborCount > vertexFirstEdgeIndices.Length) throw new System.InvalidOperationException("Face neighbors were specified such that an external face was misconfigured.");
							} while (faceEdgeIndex != firstFaceEdgeIndex);

							// Store the face count and link the face to the first edge.
							faceNeighborCounts[faceIndex] = (ushort)(0x8000u | (neighborCount & 0x7FFFu));
							faceFirstEdgeIndices[faceIndex] = firstFaceEdgeIndex;
							++faceIndex;
						}
					}
				}

				var topology = new TTopology();

				topology._vertexNeighborCounts = vertexNeighborCounts;
				topology._vertexFirstEdgeIndices = vertexFirstEdgeIndices;
				topology._edgeData = edgeData;
				topology._faceNeighborCounts = faceNeighborCounts;
				topology._faceFirstEdgeIndices = faceFirstEdgeIndices;
				topology._firstExternalEdgeIndex = internalEdgeData.Length;
				topology._firstExternalFaceIndex = internalFaceFirstEdgeIndices.Length;

				return topology;
			}
		}
	}
}
