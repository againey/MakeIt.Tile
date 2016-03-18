/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public static class ManifoldUtility
	{
		public static void MakeDual(Topology topology, IList<Vector3> facePositions, out Vector3[] vertexPositions)
		{
			var faceCount = topology.faces.Count;
			topology.MakeDual();
			vertexPositions = new Vector3[faceCount];
			facePositions.CopyTo(vertexPositions, 0);
		}

		public static void GetDualManifold(Topology topology, IList<Vector3> facePositions, out Topology dualTopology, out Vector3[] vertexPositions)
		{
			dualTopology = (Topology)topology.Clone();
			MakeDual(dualTopology, facePositions, out vertexPositions);
		}

		private static void SubdivideEdge(Vector3 p0, Vector3 p1, int count, Func<Vector3, Vector3, float, Vector3> interpolator, List<Vector3> positions)
		{
			var dt = 1.0f / (float)(count + 1);
			var t = dt;
			var tEnd = 1f - dt * 0.5f;
			while (t < tEnd)
			{
				positions.Add(interpolator(p0, p1, t));
				t += dt;
			}
		}

		private static void SubdivideTriangle(ManualFaceNeighborIndexer indexer, Topology.Face face, int degree, Func<Vector3, Vector3, float, Vector3> interpolator, List<Vector3> subdividedVertexPositions, int[] subdividedEdgeVertices, ref int currentVertexCount)
		{
			var rightEdge = face.firstEdge;
			var bottomEdge = rightEdge.next;
			var leftEdge = bottomEdge.next;

			var topVertex = leftEdge.nextVertex;
			var bottomRightVertex = rightEdge.nextVertex;
			var bottomLeftVertex = bottomEdge.nextVertex;

			int rightVertices = rightEdge.index * degree; // Progresses from the top vertex down to the bottom right vertex.
			int bottomVertices = bottomEdge.twinIndex * degree; // Progresses from the bottom left vertex over to the bottom right vertex.
			int leftVertices = leftEdge.twinIndex * degree; // Progresses from the top vertex down to the bottom left vertex.

			if (degree > 2)
			{
				// Top triangle
				indexer.AddFace(subdividedEdgeVertices[leftVertices], topVertex.index, subdividedEdgeVertices[rightVertices]);

				// Next three triangles above the top inner subdivided vertex
				indexer.AddFace(subdividedEdgeVertices[leftVertices + 1], subdividedEdgeVertices[leftVertices], currentVertexCount);
				indexer.AddFace(currentVertexCount, subdividedEdgeVertices[leftVertices], subdividedEdgeVertices[rightVertices]);
				indexer.AddFace(currentVertexCount, subdividedEdgeVertices[rightVertices], subdividedEdgeVertices[rightVertices + 1]);

				// Top inner subdivided vertex
				subdividedVertexPositions.Add(interpolator(subdividedVertexPositions[subdividedEdgeVertices[leftVertices + 1]], subdividedVertexPositions[subdividedEdgeVertices[rightVertices + 1]], 0.5f));
				++currentVertexCount;

				float t;
				float dt;
				Vector3 p0;
				Vector3 p1;

				int yEnd = degree - 1;

				// Middle rows of inner subdivided vertices
				for (int y = 1; y < yEnd; ++y)
				{
					t = dt = 1f / (y + 2);
					p0 = subdividedVertexPositions[subdividedEdgeVertices[leftVertices + y + 1]];
					p1 = subdividedVertexPositions[subdividedEdgeVertices[rightVertices + y + 1]];

					// First two triangles of the row of faces above this row of inner subdivided vertices
					indexer.AddFace(subdividedEdgeVertices[leftVertices + y + 1], subdividedEdgeVertices[leftVertices + y], currentVertexCount);
					indexer.AddFace(currentVertexCount, subdividedEdgeVertices[leftVertices + y], currentVertexCount - y);

					subdividedVertexPositions.Add(interpolator(p0, p1, t));
					++currentVertexCount;
					t += dt;

					for (int x = 1; x < y; ++x)
					{
						// Next two triangles above the vertex being added
						indexer.AddFace(currentVertexCount - 1, currentVertexCount - y - 1, currentVertexCount);
						indexer.AddFace(currentVertexCount, currentVertexCount - y - 1, currentVertexCount - y);
						subdividedVertexPositions.Add(interpolator(p0, p1, t));
						++currentVertexCount;
						t += dt;
					}

					// Last three triangles of the row of faces above this row of inner subdivided vertices
					indexer.AddFace(currentVertexCount - 1, currentVertexCount - y - 1, currentVertexCount);
					indexer.AddFace(currentVertexCount, currentVertexCount - y - 1, subdividedEdgeVertices[rightVertices + y]);
					indexer.AddFace(currentVertexCount, subdividedEdgeVertices[rightVertices + y], subdividedEdgeVertices[rightVertices + y + 1]);

					subdividedVertexPositions.Add(interpolator(p0, p1, t));
					++currentVertexCount;
				}

				// First two triangles of the last row of faces
				indexer.AddFace(bottomLeftVertex.index, subdividedEdgeVertices[leftVertices + yEnd], subdividedEdgeVertices[bottomVertices]);
				indexer.AddFace(subdividedEdgeVertices[bottomVertices], subdividedEdgeVertices[leftVertices + yEnd], currentVertexCount - yEnd);

				for (int x = 1; x < yEnd; ++x)
				{
					// Next two triangles of the last row of faces
					indexer.AddFace(subdividedEdgeVertices[bottomVertices + x - 1], currentVertexCount - yEnd + x - 1, subdividedEdgeVertices[bottomVertices + x]);
					indexer.AddFace(subdividedEdgeVertices[bottomVertices + x], currentVertexCount - yEnd + x - 1, currentVertexCount - yEnd + x);
				}

				// Last three triangles of the last row of faces
				indexer.AddFace(subdividedEdgeVertices[bottomVertices + yEnd - 1], currentVertexCount - 1, subdividedEdgeVertices[bottomVertices + yEnd]);
				indexer.AddFace(subdividedEdgeVertices[bottomVertices + yEnd], currentVertexCount - 1, subdividedEdgeVertices[rightVertices + yEnd]);
				indexer.AddFace(subdividedEdgeVertices[bottomVertices + yEnd], subdividedEdgeVertices[rightVertices + yEnd], bottomRightVertex.index);
			}
			else if (degree == 2)
			{
				indexer.AddFace(subdividedEdgeVertices[leftVertices], topVertex.index, subdividedEdgeVertices[rightVertices]);
				indexer.AddFace(subdividedEdgeVertices[leftVertices + 1], subdividedEdgeVertices[leftVertices], currentVertexCount);
				indexer.AddFace(currentVertexCount, subdividedEdgeVertices[leftVertices], subdividedEdgeVertices[rightVertices]);
				indexer.AddFace(currentVertexCount, subdividedEdgeVertices[rightVertices], subdividedEdgeVertices[rightVertices + 1]);
				indexer.AddFace(bottomLeftVertex.index, subdividedEdgeVertices[leftVertices + 1], subdividedEdgeVertices[bottomVertices]);
				indexer.AddFace(subdividedEdgeVertices[bottomVertices], subdividedEdgeVertices[leftVertices + 1], currentVertexCount);
				indexer.AddFace(subdividedEdgeVertices[bottomVertices], currentVertexCount, subdividedEdgeVertices[bottomVertices + 1]);
				indexer.AddFace(subdividedEdgeVertices[bottomVertices + 1], currentVertexCount, subdividedEdgeVertices[rightVertices + 1]);
				indexer.AddFace(subdividedEdgeVertices[bottomVertices + 1], subdividedEdgeVertices[rightVertices + 1], bottomRightVertex.index);

				subdividedVertexPositions.Add(interpolator(subdividedVertexPositions[subdividedEdgeVertices[leftVertices + 1]], subdividedVertexPositions[subdividedEdgeVertices[rightVertices + 1]], 0.5f));
				++currentVertexCount;
			}
			else if (degree == 1)
			{
				indexer.AddFace(subdividedEdgeVertices[leftVertices], topVertex.index, subdividedEdgeVertices[rightVertices]);
				indexer.AddFace(bottomLeftVertex.index, subdividedEdgeVertices[leftVertices], subdividedEdgeVertices[bottomVertices]);
				indexer.AddFace(subdividedEdgeVertices[bottomVertices], subdividedEdgeVertices[leftVertices], subdividedEdgeVertices[rightVertices]);
				indexer.AddFace(subdividedEdgeVertices[bottomVertices], subdividedEdgeVertices[rightVertices], bottomRightVertex.index);
			}
		}

		private static void SubdivideQuadrilateral(ManualFaceNeighborIndexer indexer, Topology.Face face, int degree, Func<Vector3, Vector3, float, Vector3> interpolator, List<Vector3> subdividedVertexPositions, int[] subdividedEdgeVertices, ref int currentVertexCount)
		{
			var topEdge = face.firstEdge;
			var rightEdge = topEdge.next;
			var bottomEdge = rightEdge.next;
			var leftEdge = bottomEdge.next;

			var topLeftVertex = leftEdge.nextVertex;
			var topRightVertex = topEdge.nextVertex;
			var bottomRightVertex = rightEdge.nextVertex;
			var bottomLeftVertex = bottomEdge.nextVertex;

			int topVertices = topEdge.index * degree; // Progresses from top left vertex over to top right vertex.
			int bottomVertices = bottomEdge.twinIndex * degree; // Progresses from bottom left vertex over to bottom right vertex.
			int rightVertices = rightEdge.index * degree; // Progresses from top right vertex down to bottom right vertex.
			int leftVertices = leftEdge.twinIndex * degree; // Progresses from top left vertex down to bottom left vertex.

			var dt = 1f / (degree + 1);

			if (degree > 2)
			{
				int yEnd = degree - 1;
				int xEnd = degree - 1;

				float t;
				Vector3 p0;
				Vector3 p1;

				// Top row of subdivided faces
				indexer.AddFace(topLeftVertex.index, subdividedEdgeVertices[topVertices], currentVertexCount, subdividedEdgeVertices[leftVertices]);
				for (int x = 0; x < xEnd; ++x)
				{
					indexer.AddFace(subdividedEdgeVertices[topVertices + x], subdividedEdgeVertices[topVertices + x + 1], currentVertexCount + x + 1, currentVertexCount + x);
				}
				indexer.AddFace(subdividedEdgeVertices[topVertices + xEnd], topRightVertex.index, subdividedEdgeVertices[rightVertices], currentVertexCount + xEnd);

				// Middle rows of subdivided faces
				for (int y = 0; y < yEnd; ++y)
				{
					var rowFirstVertexIndex = currentVertexCount + y * degree;
					indexer.AddFace(subdividedEdgeVertices[leftVertices + y], rowFirstVertexIndex, rowFirstVertexIndex + degree, subdividedEdgeVertices[leftVertices + y + 1]);
					for (int x = 0; x < xEnd; ++x)
					{
						indexer.AddFace(rowFirstVertexIndex + x, rowFirstVertexIndex + x + 1, rowFirstVertexIndex + degree + x + 1, rowFirstVertexIndex + degree + x);
					}
					indexer.AddFace(rowFirstVertexIndex + xEnd, subdividedEdgeVertices[rightVertices + y], subdividedEdgeVertices[rightVertices + y + 1], rowFirstVertexIndex + degree + xEnd);
				}

				// Bottom row of subdivided faces
				var lastRowFirstVertexIndex = currentVertexCount + yEnd * degree;
				indexer.AddFace(subdividedEdgeVertices[leftVertices + yEnd], lastRowFirstVertexIndex, subdividedEdgeVertices[bottomVertices], bottomLeftVertex.index);
				for (int x = 0; x < xEnd; ++x)
				{
					indexer.AddFace(lastRowFirstVertexIndex + x, lastRowFirstVertexIndex + x + 1, subdividedEdgeVertices[bottomVertices + x + 1], subdividedEdgeVertices[bottomVertices + x]);
				}
				indexer.AddFace(lastRowFirstVertexIndex + xEnd, subdividedEdgeVertices[rightVertices + yEnd], bottomRightVertex.index, subdividedEdgeVertices[bottomVertices + xEnd]);

				// Subdivided vertices
				for (int y = 0; y <= yEnd; ++y)
				{
					t = dt;
					p0 = subdividedVertexPositions[subdividedEdgeVertices[leftVertices + y]];
					p1 = subdividedVertexPositions[subdividedEdgeVertices[rightVertices + y]];

					for (int x = 0; x <= xEnd; ++x)
					{
						subdividedVertexPositions.Add(interpolator(p0, p1, t));
						t += dt;
					}
				}
				currentVertexCount += degree * degree;
			}
			else if (degree == 2)
			{
				indexer.AddFace(topLeftVertex.index, subdividedEdgeVertices[topVertices], currentVertexCount, subdividedEdgeVertices[leftVertices]);
				indexer.AddFace(subdividedEdgeVertices[topVertices], subdividedEdgeVertices[topVertices + 1], currentVertexCount + 1, currentVertexCount);
				indexer.AddFace(subdividedEdgeVertices[topVertices + 1], topRightVertex.index, subdividedEdgeVertices[rightVertices], currentVertexCount + 1);

				indexer.AddFace(subdividedEdgeVertices[leftVertices], currentVertexCount, currentVertexCount + 2, subdividedEdgeVertices[leftVertices + 1]);
				indexer.AddFace(currentVertexCount, currentVertexCount + 1, currentVertexCount + 3, currentVertexCount + 2);
				indexer.AddFace(currentVertexCount + 1, subdividedEdgeVertices[rightVertices], subdividedEdgeVertices[rightVertices + 1], currentVertexCount + 3);

				indexer.AddFace(subdividedEdgeVertices[leftVertices + 1], currentVertexCount + 2, subdividedEdgeVertices[bottomVertices], bottomLeftVertex.index);
				indexer.AddFace(currentVertexCount + 2, currentVertexCount + 3, subdividedEdgeVertices[bottomVertices + 1], subdividedEdgeVertices[bottomVertices]);
				indexer.AddFace(currentVertexCount + 3, subdividedEdgeVertices[rightVertices + 1], bottomRightVertex.index, subdividedEdgeVertices[bottomVertices + 1]);

				subdividedVertexPositions.Add(interpolator(subdividedVertexPositions[subdividedEdgeVertices[leftVertices]], subdividedVertexPositions[subdividedEdgeVertices[rightVertices]], dt));
				subdividedVertexPositions.Add(interpolator(subdividedVertexPositions[subdividedEdgeVertices[leftVertices]], subdividedVertexPositions[subdividedEdgeVertices[rightVertices]], dt + dt));
				subdividedVertexPositions.Add(interpolator(subdividedVertexPositions[subdividedEdgeVertices[leftVertices + 1]], subdividedVertexPositions[subdividedEdgeVertices[rightVertices + 1]], dt));
				subdividedVertexPositions.Add(interpolator(subdividedVertexPositions[subdividedEdgeVertices[leftVertices + 1]], subdividedVertexPositions[subdividedEdgeVertices[rightVertices + 1]], dt + dt));
				currentVertexCount += 4;
			}
			else if (degree == 1)
			{
				indexer.AddFace(topLeftVertex.index, subdividedEdgeVertices[topVertices], currentVertexCount, subdividedEdgeVertices[leftVertices]);
				indexer.AddFace(subdividedEdgeVertices[topVertices], topRightVertex.index, subdividedEdgeVertices[rightVertices], currentVertexCount);
				indexer.AddFace(subdividedEdgeVertices[leftVertices], currentVertexCount, subdividedEdgeVertices[bottomVertices], bottomLeftVertex.index);
				indexer.AddFace(currentVertexCount, subdividedEdgeVertices[rightVertices], bottomRightVertex.index, subdividedEdgeVertices[bottomVertices]);

				subdividedVertexPositions.Add(interpolator(subdividedVertexPositions[subdividedEdgeVertices[leftVertices]], subdividedVertexPositions[subdividedEdgeVertices[rightVertices]], dt));
				++currentVertexCount;
			}
		}

		public static void Subdivide(Topology topology, IVertexAttribute<Vector3> vertexPositions, int degree, Func<Vector3, Vector3, float, Vector3> interpolator, out Topology subdividedTopology, out Vector3[] subdividedVertexPositions)
		{
			if (degree < 0) throw new ArgumentOutOfRangeException("Topology subdivision degree cannot be negative.");
			if (degree == 0)
			{
				subdividedTopology = (Topology)topology.Clone();
				var subdividedVertexPositionsArray = new Vector3[topology.vertices.Count];
				vertexPositions.CopyTo(subdividedVertexPositionsArray, 0);
				subdividedVertexPositions = subdividedVertexPositionsArray;
				return;
			}

			int vertexCount = 0;
			int edgeCount = 0;
			int internalFaceCount = 0;

			vertexCount += topology.vertices.Count + (topology.halfEdges.Count / 2) * degree;

			foreach (var face in topology.internalFaces)
			{
				switch (face.neighborCount)
				{
					case 3:
						vertexCount += (degree * (degree - 1)) / 2;
						edgeCount += (degree + 1) * (degree + 1) * 3;
						internalFaceCount += (degree + 1) * (degree + 1);
						break;
					case 4:
						vertexCount += degree * degree;
						edgeCount += (degree + 1) * (degree + 1) * 4;
						internalFaceCount += (degree + 1) * (degree + 1);
						break;
					default:
						throw new InvalidOperationException("Cannot subdivide a face with anything other than 3 or 4 sides.");
				}
			}

			foreach (var face in topology.externalFaces)
			{
				edgeCount += face.neighborCount * (degree + 1);
			}

			var indexer = new ManualFaceNeighborIndexer(vertexCount, edgeCount, internalFaceCount, topology.externalFaces.Count);
			var subdividedVertexPositionsList = new List<Vector3>();

			foreach (var vertex in topology.vertices)
			{
				subdividedVertexPositionsList.Add(vertexPositions[vertex]);
			}

			var currentVertexCount = topology.vertices.Count;

			var dt = 1f / (degree + 1);
			var tEnd = 1f - dt * 0.5f;

			var subdividedEdgeVertices = new int[topology.halfEdges.Count * degree];

			foreach (var edge in topology.vertexEdges)
			{
				if (edge.isFirstTwin)
				{
					var p0 = vertexPositions[edge.nearVertex];
					var p1 = vertexPositions[edge.farVertex];
					var t = dt;
					var subdividedEdgeVertexIndex = edge.index * degree;
					while (t < tEnd)
					{
						subdividedEdgeVertices[subdividedEdgeVertexIndex++] = currentVertexCount++;
						subdividedVertexPositionsList.Add(interpolator(p0, p1, t));
						t += dt;
					}
				}
				else
				{
					var subdividedEdgeVertexIndex = edge.index * degree;
					var subdividedTwinEdgeVertexIndex = (edge.twinIndex + 1) * degree - 1;
					for (int i = 0; i < degree; ++i)
					{
						subdividedEdgeVertices[subdividedEdgeVertexIndex + i] = subdividedEdgeVertices[subdividedTwinEdgeVertexIndex - i];
					}
				}
			}

			foreach (var face in topology.internalFaces)
			{
				switch (face.neighborCount)
				{
					case 3:
						SubdivideTriangle(indexer, face, degree, interpolator, subdividedVertexPositionsList, subdividedEdgeVertices, ref currentVertexCount);
						break;
					case 4:
						SubdivideQuadrilateral(indexer, face, degree, interpolator, subdividedVertexPositionsList, subdividedEdgeVertices, ref currentVertexCount);
						break;
					default:
						throw new InvalidOperationException("Cannot subdivide a face with anything other than 3 or 4 sides.");
				}
			}

			subdividedTopology = TopologyUtility.BuildTopology(indexer);
			subdividedVertexPositions = subdividedVertexPositionsList.ToArray();
		}

		public static void Subdivide(Topology topology, IVertexAttribute<Vector3> vertexPositions, int degree, out Topology subdividedTopology, out Vector3[] subdividedVertexPositions)
		{
			Subdivide(topology, vertexPositions, degree, (Vector3 p0, Vector3 p1, float t) => { return GeometryUtility.LerpUnclamped(p0, p1, t); }, out subdividedTopology, out subdividedVertexPositions);
		}
	}
}
