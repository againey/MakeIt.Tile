﻿using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	using VertexPositions = VertexAttribute<Vector3>;

	public static class SphericalManifold
	{
		public static Manifold CreateTetrahedron()
		{
			var builder = new Topology.Builder(4, 12, 4);

			var y = -1f / 3f;
			var z0 = 2f / 3f * Mathf.Sqrt(2f);
			var z1 = -Mathf.Sqrt(2f / 9f);
			var x = Mathf.Sqrt(2f / 3f);

			var vertexPositions = new VertexPositions(4);
			vertexPositions[ 0] = new Vector3( 0, +1,  0);
			vertexPositions[ 1] = new Vector3( 0,  y, z0);
			vertexPositions[ 2] = new Vector3(+x,  y, z1);
			vertexPositions[ 3] = new Vector3(-x,  y, z1);

			builder.AddVertex(1, 2, 3);
			builder.AddVertex(0, 3, 2);
			builder.AddVertex(0, 1, 3);
			builder.AddVertex(0, 2, 1);

			return new Manifold(builder.BuildTopology(), vertexPositions);
		}

		public static Manifold CreateHexahedron()
		{
			var builder = new Topology.Builder(8, 24, 6);

			var a = 1f / Mathf.Sqrt(3f);

			var vertexPositions = new VertexPositions(8);
			vertexPositions[ 0] = new Vector3(+a, +a, +a);
			vertexPositions[ 1] = new Vector3(+a, +a, -a);
			vertexPositions[ 2] = new Vector3(-a, +a, -a);
			vertexPositions[ 3] = new Vector3(-a, +a, +a);
			vertexPositions[ 4] = new Vector3(+a, -a, +a);
			vertexPositions[ 5] = new Vector3(+a, -a, -a);
			vertexPositions[ 6] = new Vector3(-a, -a, -a);
			vertexPositions[ 7] = new Vector3(-a, -a, +a);

			builder.AddVertex(1, 3, 4);
			builder.AddVertex(0, 5, 2);
			builder.AddVertex(1, 6, 3);
			builder.AddVertex(0, 2, 7);
			builder.AddVertex(0, 7, 5);
			builder.AddVertex(1, 4, 6);
			builder.AddVertex(2, 5, 7);
			builder.AddVertex(3, 6, 4);

			return new Manifold(builder.BuildTopology(), vertexPositions);
		}

		public static Manifold CreateOctahedron()
		{
			var builder = new Topology.Builder(6, 24, 8);

			var vertexPositions = new VertexPositions(6);
			vertexPositions[ 0] = new Vector3( 0, +1,  0);
			vertexPositions[ 1] = new Vector3(+1,  0,  0);
			vertexPositions[ 2] = new Vector3( 0,  0, -1);
			vertexPositions[ 3] = new Vector3(-1,  0,  0);
			vertexPositions[ 4] = new Vector3( 0,  0, +1);
			vertexPositions[ 5] = new Vector3( 0, -1,  0);

			builder.AddVertex(1, 2, 3, 4);
			builder.AddVertex(0, 4, 5, 2);
			builder.AddVertex(0, 1, 5, 3);
			builder.AddVertex(0, 2, 5, 4);
			builder.AddVertex(0, 3, 5, 1);
			builder.AddVertex(1, 4, 3, 2);

			return new Manifold(builder.BuildTopology(), vertexPositions);
		}

		public static Manifold CreateDodecahedron()
		{
			return GetDualManifold(CreateIcosahedron());
		}

		public static Manifold CreateIcosahedron()
		{
			var builder = new Topology.Builder(12, 60, 20);

			var latitude = Mathf.Atan2(1, 2);
			var longitude = Mathf.PI * 0.2f;
			var cosLat = Mathf.Cos(latitude);

			var x0 = cosLat * Mathf.Sin(longitude * 2.0f);
			var x1 = cosLat * Mathf.Sin(longitude);
			var x2 = 0.0f;
			var y0 = +1.0f;
			var y1 = Mathf.Sin(latitude);
			var y2 = -1.0f;
			var z0 = cosLat;
			var z1 = cosLat * Mathf.Cos(longitude);
			var z2 = cosLat * Mathf.Cos(longitude * 2.0f);

			var vertexPositions = new VertexPositions(12);
			vertexPositions[ 0] = new Vector3(+x2,  y0, 0.0f);
			vertexPositions[ 1] = new Vector3(+x2, +y1, +z0);
			vertexPositions[ 2] = new Vector3(+x2, -y1, -z0);
			vertexPositions[ 3] = new Vector3(+x2,  y2, 0.0f);
			vertexPositions[ 4] = new Vector3(+x1, +y1, -z1);
			vertexPositions[ 5] = new Vector3(-x1, +y1, -z1);
			vertexPositions[ 6] = new Vector3(+x1, -y1, +z1);
			vertexPositions[ 7] = new Vector3(-x1, -y1, +z1);
			vertexPositions[ 8] = new Vector3(+x0, +y1, +z2);
			vertexPositions[ 9] = new Vector3(+x0, -y1, -z2);
			vertexPositions[10] = new Vector3(-x0, +y1, +z2);
			vertexPositions[11] = new Vector3(-x0, -y1, -z2);

			builder.AddVertex( 1,  8,  4,  5, 10);
			builder.AddVertex( 0, 10,  7,  6,  8);
			builder.AddVertex( 3, 11,  5,  4,  9);
			builder.AddVertex( 2,  9,  6,  7, 11);
			builder.AddVertex( 0,  8,  9,  2,  5);
			builder.AddVertex( 0,  4,  2, 11, 10);
			builder.AddVertex( 1,  7,  3,  9,  8);
			builder.AddVertex( 1, 10, 11,  3,  6);
			builder.AddVertex( 0,  1,  6,  9,  4);
			builder.AddVertex( 2,  4,  8,  6,  3);
			builder.AddVertex( 0,  5, 11,  7,  1);
			builder.AddVertex( 2,  3,  7, 10,  5);

			return new Manifold(builder.BuildTopology(), vertexPositions);
		}

		private static Vector3 Slerp(Vector3 p0, Vector3 p1, float t)
		{
			var omega = Mathf.Acos(Vector3.Dot(p0, p1));
			var d = Mathf.Sin(omega);
			var s0 = Mathf.Sin((1f - t) * omega);
			var s1 = Mathf.Sin(t * omega);
			return (p0 * s0 + p1 * s1) / d;
		}

		private static void SubdivideEdge(Vector3 p0, Vector3 p1, int count, ICollection<Vector3> positions)
		{
			var dt = 1.0f / (float)(count + 1);
			var t = dt;
			var tEnd = 1f - dt * 0.5f;
			while (t < tEnd)
			{
				positions.Add(Slerp(p0, p1, t));
				t += dt;
			}
		}

		private static void SubdivideTriangle(Topology.Builder builder, Topology.Face face, int degree, List<Vector3> subdividedPositions, int[] subdividedEdgeVertices)
		{
			var rightEdge = face.firstEdge;
			var bottomEdge = rightEdge.next;
			var leftEdge = bottomEdge.next;

			var topVertex = leftEdge.nextVertex;
			var bottomRightVertex = rightEdge.nextVertex;
			var bottomLeftVertex = bottomEdge.nextVertex;

			int rightVertices = rightEdge.twinIndex * degree;
			int bottomVertices = bottomEdge.index * degree;
			int leftVertices = leftEdge.index * degree;

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
				subdividedPositions.Add(Slerp(subdividedPositions[subdividedEdgeVertices[leftVertices + 1]], subdividedPositions[subdividedEdgeVertices[rightVertices + 1]], 0.5f));
				++nextVertexIndex;

				float t;
				float dt;
				Vector3 p0;
				Vector3 p1;

				// Middle rows of inner subdivided vertices
				for (int y = 1; y < yEnd; ++y)
				{
					t = dt = 1f / (y + 2);
					p0 = subdividedPositions[subdividedEdgeVertices[leftVertices + y + 1]];
					p1 = subdividedPositions[subdividedEdgeVertices[rightVertices + y + 1]];

					builder.AddVertex(
						subdividedEdgeVertices[leftVertices + y + 1],
						subdividedEdgeVertices[leftVertices + y],
						nextVertexIndex - y,
						nextVertexIndex + 1,
						nextVertexIndex + y + 2,
						nextVertexIndex + y + 1);
					subdividedPositions.Add(Slerp(p0, p1, t));
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
						subdividedPositions.Add(Slerp(p0, p1, t));
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
					subdividedPositions.Add(Slerp(p0, p1, t));
					++nextVertexIndex;
				}

				// Bottom row of inner subdivided vertices
				t = dt = 1f / (yEnd + 2);
				p0 = subdividedPositions[subdividedEdgeVertices[leftVertices + yEnd + 1]];
				p1 = subdividedPositions[subdividedEdgeVertices[rightVertices + yEnd + 1]];

				builder.AddVertex(
					subdividedEdgeVertices[leftVertices + yEnd + 1],
					subdividedEdgeVertices[leftVertices + yEnd],
					nextVertexIndex - yEnd,
					nextVertexIndex + 1,
					subdividedEdgeVertices[bottomVertices + 1],
					subdividedEdgeVertices[bottomVertices]);
				subdividedPositions.Add(Slerp(p0, p1, t));
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
					subdividedPositions.Add(Slerp(p0, p1, t));
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
				subdividedPositions.Add(Slerp(p0, p1, t));

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
				subdividedPositions.Add(Slerp(subdividedPositions[subdividedEdgeVertices[leftVertices + 1]], subdividedPositions[subdividedEdgeVertices[rightVertices + 1]], 0.5f));

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

		private static void SubdivideQuadrilateral(Topology.Builder builder, Topology.Face face, int degree, List<Vector3> subdividedPositions, int[] subdividedEdgeVertices)
		{
			var topEdge = face.firstEdge;
			var rightEdge = topEdge.next;
			var bottomEdge = rightEdge.next;
			var leftEdge = bottomEdge.next;

			var topLeftVertex = leftEdge.nextVertex;
			var topRightVertex = topEdge.nextVertex;
			var bottomRightVertex = rightEdge.nextVertex;
			var bottomLeftVertex = bottomEdge.nextVertex;

			int topVertices = topEdge.twinIndex * degree;
			int bottomVertices = bottomEdge.index * degree;
			int rightVertices = rightEdge.twinIndex * degree;
			int leftVertices = leftEdge.index * degree;

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
				p0 = subdividedPositions[subdividedEdgeVertices[leftVertices]];
				p1 = subdividedPositions[subdividedEdgeVertices[rightVertices]];

				builder.AddVertex(subdividedEdgeVertices[topVertices], nextVertexIndex + 1, nextVertexIndex + degree, subdividedEdgeVertices[leftVertices]);
				subdividedPositions.Add(Slerp(p0, p1, t));
				++nextVertexIndex;
				t += dt;
				for (int x = 1; x < xEnd; ++x)
				{
					builder.AddVertex(subdividedEdgeVertices[topVertices + x], nextVertexIndex + 1, nextVertexIndex + degree, nextVertexIndex - 1);
					subdividedPositions.Add(Slerp(p0, p1, t));
					++nextVertexIndex;
					t += dt;
				}
				builder.AddVertex(subdividedEdgeVertices[topVertices + xEnd], subdividedEdgeVertices[rightVertices], nextVertexIndex + degree, nextVertexIndex - 1);
				subdividedPositions.Add(Slerp(p0, p1, t));
				++nextVertexIndex;

				// Middle rows of inner subdivided vertices
				for (int y = 1; y < yEnd; ++y)
				{
					t = dt;
					p0 = subdividedPositions[subdividedEdgeVertices[leftVertices + y]];
					p1 = subdividedPositions[subdividedEdgeVertices[rightVertices + y]];

					builder.AddVertex(nextVertexIndex - degree, nextVertexIndex + 1, nextVertexIndex + degree, subdividedEdgeVertices[leftVertices + y]);
					subdividedPositions.Add(Slerp(p0, p1, t));
					++nextVertexIndex;
					t += dt;
					for (int x = 1; x < xEnd; ++x)
					{
						builder.AddVertex(nextVertexIndex - degree, nextVertexIndex + 1, nextVertexIndex + degree, nextVertexIndex - 1);
						subdividedPositions.Add(Slerp(p0, p1, t));
						++nextVertexIndex;
						t += dt;
					}
					builder.AddVertex(nextVertexIndex - degree, subdividedEdgeVertices[rightVertices + y], nextVertexIndex + degree, nextVertexIndex - 1);
					subdividedPositions.Add(Slerp(p0, p1, t));
					++nextVertexIndex;
				}

				// Bottom row of inner subdivided vertices
				t = dt;
				p0 = subdividedPositions[subdividedEdgeVertices[leftVertices + yEnd]];
				p1 = subdividedPositions[subdividedEdgeVertices[rightVertices + yEnd]];

				builder.AddVertex(nextVertexIndex - degree, nextVertexIndex + 1, subdividedEdgeVertices[bottomVertices], subdividedEdgeVertices[leftVertices + yEnd]);
				subdividedPositions.Add(Slerp(p0, p1, t));
				++nextVertexIndex;
				t += dt;
				for (int x = 1; x < xEnd; ++x)
				{
					builder.AddVertex(nextVertexIndex - degree, nextVertexIndex + 1, subdividedEdgeVertices[bottomVertices + x], nextVertexIndex - 1);
					subdividedPositions.Add(Slerp(p0, p1, t));
					++nextVertexIndex;
					t += dt;
				}
				builder.AddVertex(nextVertexIndex - degree, subdividedEdgeVertices[rightVertices + yEnd], subdividedEdgeVertices[bottomVertices + xEnd], nextVertexIndex - 1);
				subdividedPositions.Add(Slerp(p0, p1, t));

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

				subdividedPositions.Add(Slerp(subdividedPositions[subdividedEdgeVertices[leftVertices]], subdividedPositions[subdividedEdgeVertices[rightVertices]], dt));
				subdividedPositions.Add(Slerp(subdividedPositions[subdividedEdgeVertices[leftVertices]], subdividedPositions[subdividedEdgeVertices[rightVertices]], dt + dt));
				subdividedPositions.Add(Slerp(subdividedPositions[subdividedEdgeVertices[leftVertices + 1]], subdividedPositions[subdividedEdgeVertices[rightVertices + 1]], dt));
				subdividedPositions.Add(Slerp(subdividedPositions[subdividedEdgeVertices[leftVertices + 1]], subdividedPositions[subdividedEdgeVertices[rightVertices + 1]], dt + dt));

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

				subdividedPositions.Add(Slerp(subdividedPositions[subdividedEdgeVertices[leftVertices]], subdividedPositions[subdividedEdgeVertices[rightVertices]], dt));

				builder.ExtendVertexAfter(subdividedEdgeVertices[topVertices], topRightVertex.index, innerVertexIndex);
				builder.ExtendVertexAfter(subdividedEdgeVertices[bottomVertices], bottomLeftVertex.index, innerVertexIndex);
				builder.ExtendVertexAfter(subdividedEdgeVertices[rightVertices], bottomRightVertex.index, innerVertexIndex);
				builder.ExtendVertexAfter(subdividedEdgeVertices[leftVertices], topLeftVertex.index, innerVertexIndex);
			}
		}

		public static Manifold Subdivide(Manifold manifold, int degree)
		{
			if (degree < 0) throw new System.ArgumentOutOfRangeException("Topology subdivision degree cannot be negative.");
			if (degree == 0) return manifold.Clone();

			var topology = manifold.topology;
			var positions = manifold.vertexPositions;
			var builder = new Topology.Builder();
			var subdividedPositions = new List<Vector3>();

			foreach (var vertex in topology.vertices)
			{
				builder.AddVertex();
				subdividedPositions.Add(positions[vertex]);
			}

			var dt = 1f / (degree + 1);
			var tEnd = 1f - dt * 0.5f;

			var subdividedEdgeVertices = new int[topology.vertexEdges.Count * degree];

			foreach (var edge in topology.vertexEdges)
			{
				if (edge.nearVertex < edge.farVertex)
				{
					var p0 = positions[edge.nearVertex];
					var p1 = positions[edge.farVertex];
					var t = dt;
					var subdividedEdgeVertexIndex = edge.index * degree;
					while (t < tEnd)
					{
						subdividedEdgeVertices[subdividedEdgeVertexIndex++] = builder.AddVertex();
						subdividedPositions.Add(Slerp(p0, p1, t));
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

			foreach (var face in topology.faces)
			{
				if (face.neighborCount == 3)
				{
					SubdivideTriangle(builder, face, degree, subdividedPositions, subdividedEdgeVertices);
				}
				else if (face.neighborCount == 4)
				{
					SubdivideQuadrilateral(builder, face, degree, subdividedPositions, subdividedEdgeVertices);
				}
				else
				{
					throw new System.InvalidOperationException("Cannot subdivide a face with anything other than 3 or 4 sides.");
				}
			}

			return new Manifold(builder.BuildTopology(), new VertexPositions(subdividedPositions));
		}

		public static VertexPositions RelaxForRegularity(Manifold manifold)
		{
			return RelaxForRegularity(manifold, new VertexPositions(manifold.vertexPositions.Count));
		}

		public static VertexPositions RelaxForRegularity(Manifold manifold, VertexPositions relaxed)
		{
			var original = manifold.vertexPositions;
			if (relaxed.Count < original.Count) throw new System.ArgumentException("The buffer provided for relaxed vertex positions was not large enough given the number of vertices in the given manifold.");

			relaxed.Clear();

			foreach (var vertex in manifold.topology.vertices)
			{
				foreach (var edge in vertex.edges)
				{
					relaxed[vertex] += original[edge.farVertex];
				}
				relaxed[vertex] = relaxed[vertex].normalized;
			}

			return relaxed;
		}

		public static VertexPositions RelaxForEqualArea(Manifold manifold)
		{
			return RelaxForEqualArea(manifold, new VertexPositions(manifold.vertexPositions.Count));
		}

		public static VertexPositions RelaxForEqualArea(Manifold manifold, VertexPositions relaxed)
		{
			return RelaxForEqualArea(manifold, relaxed, new FaceAttribute<Vector3>(manifold.vertexPositions.Count));
		}

		public static VertexPositions RelaxForEqualArea(Manifold manifold, VertexPositions relaxed, FaceAttribute<Vector3> centroidsBuffer)
		{
			var original = manifold.vertexPositions;
			if (relaxed.Count < original.Count) throw new System.ArgumentException("The buffer provided for relaxed vertex positions was not large enough given the number of vertices in the given manifold.");

			var idealArea = 4f * Mathf.PI / manifold.topology.vertices.Count;

			centroidsBuffer.Clear();
			foreach (var face in manifold.topology.faces)
			{
				var sum = new Vector3();
				foreach (var edge in face.edges)
				{
					sum += original[edge.prevVertex];
				}
				centroidsBuffer[face] = sum.normalized;
			}

			relaxed.Clear();

			foreach (var vertex in manifold.topology.vertices)
			{
				var center = original[vertex];
				var firstEdge = vertex.firstEdge;
				var prevDelta = (original[firstEdge.prev.farVertex] - center) * 0.5f;
				var centroid = centroidsBuffer[firstEdge.prevFace];
				var edge = firstEdge;
				float surroundingArea = 0;
				do
				{
					var nextDelta = (original[edge.farVertex] - center) * 0.5f;
					var centroidDelta = centroid - center;
					surroundingArea += Vector3.Cross(prevDelta, centroidDelta).magnitude + Vector3.Cross(nextDelta, centroidDelta).magnitude;
					prevDelta = nextDelta;
					centroid = centroidsBuffer[edge.nextFace];
					edge = edge.next;
				} while (edge != firstEdge);
				var multiplier = idealArea / (surroundingArea * 0.5f);
				do
				{
					relaxed[edge.farVertex] += (original[edge.farVertex] - center) * multiplier + center;
					edge = edge.next;
				} while (edge != firstEdge);
			}

			for (int i = 0; i < relaxed.Count; ++i)
			{
				relaxed[i] = relaxed[i].normalized;
			}

			return relaxed;
		}

		public static bool ValidateAndRepair(Manifold manifold, float adjustmentWeight)
		{
			bool repaired = false;
			var vertexPositions = manifold.vertexPositions;
			float originalWeight = 1f - adjustmentWeight;
			foreach (var vertex in manifold.topology.vertices)
			{
				var center = vertexPositions[vertex] * 3f; // Multiply by 3 to not have to divide centroid sums by 3 below.
				var edge = vertex.firstEdge;
				var p0 = vertexPositions[edge.farVertex];
				edge = edge.next;
				var p1 = vertexPositions[edge.farVertex];
				edge = edge.next;
				var centroid0 = (center + p0 + p1);
				var firstEdge = edge;
				do
				{
					var p2 = vertexPositions[edge.farVertex];
					var centroid1 = (center + p1 + p2);
					var normal = Vector3.Cross(centroid0 - center, centroid1 - center);
					if (Vector3.Dot(normal, center) < 0f) goto repair;
					p0 = p1;
					p1 = p2;
					centroid0 = centroid1;
					edge = edge.next;
				} while (edge != firstEdge);

				continue;

				repair: repaired = true;
				var average = new Vector3();
				edge = firstEdge;
				do
				{
					average += vertexPositions[edge.farVertex];
					edge = edge.next;
				} while (edge != firstEdge);
				average /= vertex.neighborCount;
				vertexPositions[vertex] = (center * originalWeight + average * adjustmentWeight).normalized;
			}

			return !repaired;
		}

		public static Manifold GetDualManifold(Manifold manifold)
		{
			var topology = manifold.topology.GetDualTopology();
			var vertexPositions = new VertexAttribute<Vector3>(topology.vertices.Count);

			foreach (var face in manifold.topology.faces)
			{
				var average = new Vector3();
				foreach (var edge in face.edges)
				{
					average += manifold.vertexPositions[edge.prevVertex];
				}
				vertexPositions[face.index] = average.normalized;
			}

			return new Manifold(topology, vertexPositions);
		}
	}
}