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

		private static void SubdivideTriangle(VertexVerticesBuilder builder, Topology.Face face, int degree, Func<Vector3, Vector3, float, Vector3> interpolator, List<Vector3> subdividedVertexPositions, int[] subdividedEdgeVertices)
		{
			var rightEdge = face.firstEdge;
			var bottomEdge = rightEdge.next;
			var leftEdge = bottomEdge.next;

			var topVertex = leftEdge.nextVertex;
			var bottomRightVertex = rightEdge.nextVertex;
			var bottomLeftVertex = bottomEdge.nextVertex;

			int rightVertices = rightEdge.index * degree;
			int bottomVertices = bottomEdge.twinIndex * degree;
			int leftVertices = leftEdge.twinIndex * degree;

			if (degree > 2)
			{
				int firstVertexIndex = builder.vertexCount;
				int nextVertexIndex = firstVertexIndex;

				int yEnd = degree - 2;

				// Top inner subdivided vertex
				builder.AddVertex(
					subdividedEdgeVertices[leftVertices + 1],
					subdividedEdgeVertices[leftVertices],
					subdividedEdgeVertices[rightVertices],
					subdividedEdgeVertices[rightVertices + 1],
					nextVertexIndex + 2,
					nextVertexIndex + 1);
				subdividedVertexPositions.Add(interpolator(subdividedVertexPositions[subdividedEdgeVertices[leftVertices + 1]], subdividedVertexPositions[subdividedEdgeVertices[rightVertices + 1]], 0.5f));
				++nextVertexIndex;

				float t;
				float dt;
				Vector3 p0;
				Vector3 p1;

				// Middle rows of inner subdivided vertices
				for (int y = 1; y < yEnd; ++y)
				{
					t = dt = 1f / (y + 2);
					p0 = subdividedVertexPositions[subdividedEdgeVertices[leftVertices + y + 1]];
					p1 = subdividedVertexPositions[subdividedEdgeVertices[rightVertices + y + 1]];

					builder.AddVertex(
						subdividedEdgeVertices[leftVertices + y + 1],
						subdividedEdgeVertices[leftVertices + y],
						nextVertexIndex - y,
						nextVertexIndex + 1,
						nextVertexIndex + y + 2,
						nextVertexIndex + y + 1);
					subdividedVertexPositions.Add(interpolator(p0, p1, t));
					++nextVertexIndex;
					t += dt;

					for (int x = 1; x < y; ++x)
					{
						builder.AddVertex(
							nextVertexIndex - 1,
							nextVertexIndex - y - 1,
							nextVertexIndex - y,
							nextVertexIndex + 1,
							nextVertexIndex + y + 2,
							nextVertexIndex + y + 1);
						subdividedVertexPositions.Add(interpolator(p0, p1, t));
						++nextVertexIndex;
						t += dt;
					}

					builder.AddVertex(
						nextVertexIndex - 1,
						nextVertexIndex - y - 1,
						subdividedEdgeVertices[rightVertices + y],
						subdividedEdgeVertices[rightVertices + y + 1],
						nextVertexIndex + y + 2,
						nextVertexIndex + y + 1);
					subdividedVertexPositions.Add(interpolator(p0, p1, t));
					++nextVertexIndex;
				}

				// Bottom row of inner subdivided vertices
				t = dt = 1f / (yEnd + 2);
				p0 = subdividedVertexPositions[subdividedEdgeVertices[leftVertices + yEnd + 1]];
				p1 = subdividedVertexPositions[subdividedEdgeVertices[rightVertices + yEnd + 1]];

				builder.AddVertex(
					subdividedEdgeVertices[leftVertices + yEnd + 1],
					subdividedEdgeVertices[leftVertices + yEnd],
					nextVertexIndex - yEnd,
					nextVertexIndex + 1,
					subdividedEdgeVertices[bottomVertices + 1],
					subdividedEdgeVertices[bottomVertices]);
				subdividedVertexPositions.Add(interpolator(p0, p1, t));
				++nextVertexIndex;
				t += dt;

				for (int x = 1; x < yEnd; ++x)
				{
					builder.AddVertex(
						nextVertexIndex - 1,
						nextVertexIndex - yEnd - 1,
						nextVertexIndex - yEnd,
						nextVertexIndex + 1,
						subdividedEdgeVertices[bottomVertices + x + 1],
						subdividedEdgeVertices[bottomVertices + x]);
					subdividedVertexPositions.Add(interpolator(p0, p1, t));
					++nextVertexIndex;
					t += dt;
				}

				builder.AddVertex(
					nextVertexIndex - 1,
					nextVertexIndex - yEnd - 1,
					subdividedEdgeVertices[rightVertices + yEnd],
					subdividedEdgeVertices[rightVertices + yEnd + 1],
					subdividedEdgeVertices[bottomVertices + yEnd + 1],
					subdividedEdgeVertices[bottomVertices + yEnd]);
				subdividedVertexPositions.Add(interpolator(p0, p1, t));

				var lastRowFirstVertexIndex = firstVertexIndex + (yEnd * yEnd + yEnd) / 2;

				yEnd = degree - 1;
				var xEnd = yEnd;

				// Right outside edge vertices
				builder.ExtendVertexAfter(subdividedEdgeVertices[rightVertices], subdividedEdgeVertices[rightVertices + 1], firstVertexIndex, subdividedEdgeVertices[leftVertices]);
				for (int y = 1; y < yEnd; ++y)
				{
					var rowFirstVertexIndex = firstVertexIndex + (y * y + y) / 2;
					builder.ExtendVertexAfter(subdividedEdgeVertices[rightVertices + y], subdividedEdgeVertices[rightVertices + y + 1], rowFirstVertexIndex + y, rowFirstVertexIndex - 1);
				}
				builder.ExtendVertexAfter(subdividedEdgeVertices[rightVertices + yEnd], bottomRightVertex.index, subdividedEdgeVertices[bottomVertices + yEnd], lastRowFirstVertexIndex + yEnd - 1);

				// Bottom outside edge vertices
				builder.ExtendVertexAfter(subdividedEdgeVertices[bottomVertices], bottomLeftVertex.index, subdividedEdgeVertices[leftVertices + yEnd], lastRowFirstVertexIndex);
				for (int x = 1; x < xEnd; ++x)
				{
					builder.ExtendVertexAfter(subdividedEdgeVertices[bottomVertices + x], subdividedEdgeVertices[bottomVertices + x - 1], lastRowFirstVertexIndex + x - 1, lastRowFirstVertexIndex + x);
				}
				builder.ExtendVertexAfter(subdividedEdgeVertices[bottomVertices + xEnd], subdividedEdgeVertices[bottomVertices + xEnd - 1], lastRowFirstVertexIndex + yEnd - 1, subdividedEdgeVertices[rightVertices + yEnd]);

				// Left outside edge vertices
				builder.ExtendVertexAfter(subdividedEdgeVertices[leftVertices], topVertex.index, subdividedEdgeVertices[rightVertices], firstVertexIndex);
				for (int y = 1; y < yEnd; ++y)
				{
					var nextRowFirstVertexIndex = firstVertexIndex + (y * y + y) / 2;
					builder.ExtendVertexAfter(subdividedEdgeVertices[leftVertices + y], subdividedEdgeVertices[leftVertices + y - 1], nextRowFirstVertexIndex - y, nextRowFirstVertexIndex);
				}
				builder.ExtendVertexAfter(subdividedEdgeVertices[leftVertices + yEnd], subdividedEdgeVertices[leftVertices + yEnd - 1], lastRowFirstVertexIndex, subdividedEdgeVertices[bottomVertices]);
			}
			else if (degree == 2)
			{
				int innerVertexIndex = builder.AddVertex(
					subdividedEdgeVertices[rightVertices],
					subdividedEdgeVertices[rightVertices + 1],
					subdividedEdgeVertices[bottomVertices + 1],
					subdividedEdgeVertices[bottomVertices],
					subdividedEdgeVertices[leftVertices + 1],
					subdividedEdgeVertices[leftVertices]);
				subdividedVertexPositions.Add(interpolator(subdividedVertexPositions[subdividedEdgeVertices[leftVertices + 1]], subdividedVertexPositions[subdividedEdgeVertices[rightVertices + 1]], 0.5f));

				builder.ExtendVertexAfter(subdividedEdgeVertices[rightVertices], subdividedEdgeVertices[rightVertices + 1], innerVertexIndex, subdividedEdgeVertices[leftVertices]);
				builder.ExtendVertexAfter(subdividedEdgeVertices[rightVertices + 1], bottomRightVertex.index, subdividedEdgeVertices[bottomVertices + 1], innerVertexIndex);
				builder.ExtendVertexAfter(subdividedEdgeVertices[bottomVertices], bottomLeftVertex.index, subdividedEdgeVertices[leftVertices + 1], innerVertexIndex);
				builder.ExtendVertexAfter(subdividedEdgeVertices[bottomVertices + 1], subdividedEdgeVertices[bottomVertices], innerVertexIndex, subdividedEdgeVertices[rightVertices + 1]);
				builder.ExtendVertexAfter(subdividedEdgeVertices[leftVertices], topVertex.index, subdividedEdgeVertices[rightVertices], innerVertexIndex);
				builder.ExtendVertexAfter(subdividedEdgeVertices[leftVertices + 1], subdividedEdgeVertices[leftVertices], innerVertexIndex, subdividedEdgeVertices[bottomVertices]);
			}
			else if (degree == 1)
			{
				builder.ExtendVertexAfter(subdividedEdgeVertices[rightVertices], bottomRightVertex.index, subdividedEdgeVertices[bottomVertices], subdividedEdgeVertices[leftVertices]);
				builder.ExtendVertexAfter(subdividedEdgeVertices[bottomVertices], bottomLeftVertex.index, subdividedEdgeVertices[leftVertices], subdividedEdgeVertices[rightVertices]);
				builder.ExtendVertexAfter(subdividedEdgeVertices[leftVertices], topVertex.index, subdividedEdgeVertices[rightVertices], subdividedEdgeVertices[bottomVertices]);
			}
		}

		private static void SubdivideQuadrilateral(VertexVerticesBuilder builder, Topology.Face face, int degree, Func<Vector3, Vector3, float, Vector3> interpolator, List<Vector3> subdividedVertexPositions, int[] subdividedEdgeVertices)
		{
			var topEdge = face.firstEdge;
			var rightEdge = topEdge.next;
			var bottomEdge = rightEdge.next;
			var leftEdge = bottomEdge.next;

			var topLeftVertex = leftEdge.nextVertex;
			var topRightVertex = topEdge.nextVertex;
			var bottomRightVertex = rightEdge.nextVertex;
			var bottomLeftVertex = bottomEdge.nextVertex;

			int topVertices = topEdge.index * degree;
			int bottomVertices = bottomEdge.twinIndex * degree;
			int rightVertices = rightEdge.index * degree;
			int leftVertices = leftEdge.twinIndex * degree;

			var dt = 1f / (degree + 1);

			if (degree > 2)
			{
				int firstVertexIndex = builder.vertexCount;
				int nextVertexIndex = firstVertexIndex;

				int yEnd = degree - 1;
				int xEnd = degree - 1;

				float t;
				Vector3 p0;
				Vector3 p1;

				// Top row of inner subdivided vertices
				t = dt;
				p0 = subdividedVertexPositions[subdividedEdgeVertices[leftVertices]];
				p1 = subdividedVertexPositions[subdividedEdgeVertices[rightVertices]];

				builder.AddVertex(subdividedEdgeVertices[topVertices], nextVertexIndex + 1, nextVertexIndex + degree, subdividedEdgeVertices[leftVertices]);
				subdividedVertexPositions.Add(interpolator(p0, p1, t));
				++nextVertexIndex;
				t += dt;
				for (int x = 1; x < xEnd; ++x)
				{
					builder.AddVertex(subdividedEdgeVertices[topVertices + x], nextVertexIndex + 1, nextVertexIndex + degree, nextVertexIndex - 1);
					subdividedVertexPositions.Add(interpolator(p0, p1, t));
					++nextVertexIndex;
					t += dt;
				}
				builder.AddVertex(subdividedEdgeVertices[topVertices + xEnd], subdividedEdgeVertices[rightVertices], nextVertexIndex + degree, nextVertexIndex - 1);
				subdividedVertexPositions.Add(interpolator(p0, p1, t));
				++nextVertexIndex;

				// Middle rows of inner subdivided vertices
				for (int y = 1; y < yEnd; ++y)
				{
					t = dt;
					p0 = subdividedVertexPositions[subdividedEdgeVertices[leftVertices + y]];
					p1 = subdividedVertexPositions[subdividedEdgeVertices[rightVertices + y]];

					builder.AddVertex(nextVertexIndex - degree, nextVertexIndex + 1, nextVertexIndex + degree, subdividedEdgeVertices[leftVertices + y]);
					subdividedVertexPositions.Add(interpolator(p0, p1, t));
					++nextVertexIndex;
					t += dt;
					for (int x = 1; x < xEnd; ++x)
					{
						builder.AddVertex(nextVertexIndex - degree, nextVertexIndex + 1, nextVertexIndex + degree, nextVertexIndex - 1);
						subdividedVertexPositions.Add(interpolator(p0, p1, t));
						++nextVertexIndex;
						t += dt;
					}
					builder.AddVertex(nextVertexIndex - degree, subdividedEdgeVertices[rightVertices + y], nextVertexIndex + degree, nextVertexIndex - 1);
					subdividedVertexPositions.Add(interpolator(p0, p1, t));
					++nextVertexIndex;
				}

				// Bottom row of inner subdivided vertices
				t = dt;
				p0 = subdividedVertexPositions[subdividedEdgeVertices[leftVertices + yEnd]];
				p1 = subdividedVertexPositions[subdividedEdgeVertices[rightVertices + yEnd]];

				builder.AddVertex(nextVertexIndex - degree, nextVertexIndex + 1, subdividedEdgeVertices[bottomVertices], subdividedEdgeVertices[leftVertices + yEnd]);
				subdividedVertexPositions.Add(interpolator(p0, p1, t));
				++nextVertexIndex;
				t += dt;
				for (int x = 1; x < xEnd; ++x)
				{
					builder.AddVertex(nextVertexIndex - degree, nextVertexIndex + 1, subdividedEdgeVertices[bottomVertices + x], nextVertexIndex - 1);
					subdividedVertexPositions.Add(interpolator(p0, p1, t));
					++nextVertexIndex;
					t += dt;
				}
				builder.AddVertex(nextVertexIndex - degree, subdividedEdgeVertices[rightVertices + yEnd], subdividedEdgeVertices[bottomVertices + xEnd], nextVertexIndex - 1);
				subdividedVertexPositions.Add(interpolator(p0, p1, t));

				// Top outside edge vertices
				for (int x = 0; x < xEnd; ++x)
				{
					builder.ExtendVertexAfter(subdividedEdgeVertices[topVertices + x], subdividedEdgeVertices[topVertices + x + 1], firstVertexIndex + x);
				}
				builder.ExtendVertexAfter(subdividedEdgeVertices[topVertices + xEnd], topRightVertex.index, firstVertexIndex + xEnd);

				// Bottom outside edge vertices
				builder.ExtendVertexAfter(subdividedEdgeVertices[bottomVertices], bottomLeftVertex.index, firstVertexIndex + yEnd * degree);
				for (int x = 1; x <= xEnd; ++x)
				{
					builder.ExtendVertexAfter(subdividedEdgeVertices[bottomVertices + x], subdividedEdgeVertices[bottomVertices + x - 1], firstVertexIndex + yEnd * degree + x);
				}

				// Right outside edge vertices
				for (int y = 0; y < yEnd; ++y)
				{
					builder.ExtendVertexAfter(subdividedEdgeVertices[rightVertices + y], subdividedEdgeVertices[rightVertices + y + 1], firstVertexIndex + y * degree + xEnd);
				}
				builder.ExtendVertexAfter(subdividedEdgeVertices[rightVertices + yEnd], bottomRightVertex.index, firstVertexIndex + yEnd * degree + xEnd);

				// Left outside edge vertices
				builder.ExtendVertexAfter(subdividedEdgeVertices[leftVertices], topLeftVertex.index, firstVertexIndex);
				for (int y = 1; y <= yEnd; ++y)
				{
					builder.ExtendVertexAfter(subdividedEdgeVertices[leftVertices + y], subdividedEdgeVertices[leftVertices + y - 1], firstVertexIndex + y * degree);
				}
			}
			else if (degree == 2)
			{
				int firstVertexIndex = builder.vertexCount;

				builder.AddVertex(subdividedEdgeVertices[topVertices], firstVertexIndex + 1, firstVertexIndex + 2, subdividedEdgeVertices[leftVertices]);
				builder.AddVertex(subdividedEdgeVertices[topVertices + 1], subdividedEdgeVertices[rightVertices], firstVertexIndex + 3, firstVertexIndex);
				builder.AddVertex(firstVertexIndex, firstVertexIndex + 3, subdividedEdgeVertices[bottomVertices], subdividedEdgeVertices[leftVertices + 1]);
				builder.AddVertex(firstVertexIndex + 1, subdividedEdgeVertices[rightVertices + 1], subdividedEdgeVertices[bottomVertices + 1], firstVertexIndex + 2);

				subdividedVertexPositions.Add(interpolator(subdividedVertexPositions[subdividedEdgeVertices[leftVertices]], subdividedVertexPositions[subdividedEdgeVertices[rightVertices]], dt));
				subdividedVertexPositions.Add(interpolator(subdividedVertexPositions[subdividedEdgeVertices[leftVertices]], subdividedVertexPositions[subdividedEdgeVertices[rightVertices]], dt + dt));
				subdividedVertexPositions.Add(interpolator(subdividedVertexPositions[subdividedEdgeVertices[leftVertices + 1]], subdividedVertexPositions[subdividedEdgeVertices[rightVertices + 1]], dt));
				subdividedVertexPositions.Add(interpolator(subdividedVertexPositions[subdividedEdgeVertices[leftVertices + 1]], subdividedVertexPositions[subdividedEdgeVertices[rightVertices + 1]], dt + dt));

				builder.ExtendVertexAfter(subdividedEdgeVertices[topVertices], subdividedEdgeVertices[topVertices + 1], firstVertexIndex);
				builder.ExtendVertexAfter(subdividedEdgeVertices[topVertices + 1], topRightVertex.index, firstVertexIndex + 1);
				builder.ExtendVertexAfter(subdividedEdgeVertices[bottomVertices], bottomLeftVertex.index, firstVertexIndex + 2);
				builder.ExtendVertexAfter(subdividedEdgeVertices[bottomVertices + 1], subdividedEdgeVertices[bottomVertices], firstVertexIndex + 3);
				builder.ExtendVertexAfter(subdividedEdgeVertices[rightVertices], subdividedEdgeVertices[rightVertices + 1], firstVertexIndex + 1);
				builder.ExtendVertexAfter(subdividedEdgeVertices[rightVertices + 1], bottomRightVertex.index, firstVertexIndex + 3);
				builder.ExtendVertexAfter(subdividedEdgeVertices[leftVertices], topLeftVertex.index, firstVertexIndex);
				builder.ExtendVertexAfter(subdividedEdgeVertices[leftVertices + 1], subdividedEdgeVertices[leftVertices], firstVertexIndex + 2);
			}
			else if (degree == 1)
			{
				int innerVertexIndex = builder.AddVertex(subdividedEdgeVertices[topVertices], subdividedEdgeVertices[rightVertices], subdividedEdgeVertices[bottomVertices], subdividedEdgeVertices[leftVertices]);

				subdividedVertexPositions.Add(interpolator(subdividedVertexPositions[subdividedEdgeVertices[leftVertices]], subdividedVertexPositions[subdividedEdgeVertices[rightVertices]], dt));

				builder.ExtendVertexAfter(subdividedEdgeVertices[topVertices], topRightVertex.index, innerVertexIndex);
				builder.ExtendVertexAfter(subdividedEdgeVertices[bottomVertices], bottomLeftVertex.index, innerVertexIndex);
				builder.ExtendVertexAfter(subdividedEdgeVertices[rightVertices], bottomRightVertex.index, innerVertexIndex);
				builder.ExtendVertexAfter(subdividedEdgeVertices[leftVertices], topLeftVertex.index, innerVertexIndex);
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

			var builder = new VertexVerticesBuilder();
			var subdividedVertexPositionsList = new List<Vector3>();

			foreach (var vertex in topology.vertices)
			{
				builder.AddVertex();
				subdividedVertexPositionsList.Add(vertexPositions[vertex]);
			}

			var dt = 1f / (degree + 1);
			var tEnd = 1f - dt * 0.5f;

			var subdividedEdgeVertices = new int[topology.vertexEdges.Count * degree];

			foreach (var edge in topology.vertexEdges)
			{
				if (edge.nearVertex < edge.farVertex)
				{
					var p0 = vertexPositions[edge.nearVertex];
					var p1 = vertexPositions[edge.farVertex];
					var t = dt;
					var subdividedEdgeVertexIndex = edge.index * degree;
					while (t < tEnd)
					{
						subdividedEdgeVertices[subdividedEdgeVertexIndex++] = builder.AddVertex();
						subdividedVertexPositionsList.Add(interpolator(p0, p1, t));
						t += dt;
					}

					subdividedEdgeVertexIndex = edge.index * degree;
					var prevVertexIndex = edge.nearVertex.index;
					var vertexIndex = subdividedEdgeVertices[subdividedEdgeVertexIndex];
					for (int i = 1; i < degree; ++i)
					{
						var nextVertexIndex = subdividedEdgeVertices[subdividedEdgeVertexIndex + i];
						builder.ExtendVertex(vertexIndex, prevVertexIndex, nextVertexIndex);
						prevVertexIndex = vertexIndex;
						vertexIndex = nextVertexIndex;
					}
					builder.ExtendVertex(vertexIndex, prevVertexIndex, edge.farVertex.index);
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

			int maxNeighborCount = 0;
			foreach (var vertex in topology.vertices)
			{
				maxNeighborCount = Mathf.Max(maxNeighborCount, vertex.neighborCount);
			}

			var neighbors = new int[maxNeighborCount];
			foreach (var vertex in topology.vertices)
			{
				int i = 0;
				foreach (var edge in vertex.edges)
				{
					neighbors[i++] = subdividedEdgeVertices[edge.index * degree];
				}
				builder.ExtendVertex(vertex.index, i, neighbors);
			}

			foreach (var face in topology.internalFaces)
			{
				if (face.neighborCount == 3)
				{
					SubdivideTriangle(builder, face, degree, interpolator, subdividedVertexPositionsList, subdividedEdgeVertices);
				}
				else if (face.neighborCount == 4)
				{
					SubdivideQuadrilateral(builder, face, degree, interpolator, subdividedVertexPositionsList, subdividedEdgeVertices);
				}
				else
				{
					throw new System.InvalidOperationException("Cannot subdivide a face with anything other than 3 or 4 sides.");
				}
			}

			subdividedTopology = builder.BuildTopology();
			subdividedVertexPositions = subdividedVertexPositionsList.ToArray();
		}

		public static void Subdivide(Topology topology, IVertexAttribute<Vector3> vertexPositions, int degree, out Topology subdividedTopology, out Vector3[] subdividedVertexPositions)
		{
			Subdivide(topology, vertexPositions, degree, (Vector3 p0, Vector3 p1, float t) => { return Vector3.LerpUnclamped(p0, p1, t); }, out subdividedTopology, out subdividedVertexPositions);
		}
	}
}
