using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public static class PlanarManifoldUtility
	{
		public static Manifold CreatePointyTopTriGrid(int columnCount, int rowCount, bool horizontalWrapAround, bool verticalWrapAround)
		{
			return null;
		}

		public static Manifold CreateFlatTopTriGrid(int columnCount, int rowCount, bool horizontalWrapAround, bool verticalWrapAround)
		{
			return null;
		}

		public static Manifold CreateQuadGrid(int columnCount, int rowCount)
		{
			var builder = new Topology.FaceVerticesBuilder(columnCount * rowCount, columnCount * rowCount * 4 + columnCount * 2);

			var vertexColumnCount = columnCount + 1;
			var vertexRowCount = rowCount + 1;

			var vertexPositions = new Vector3[vertexColumnCount * vertexRowCount];

			for (int y = 0; y < rowCount; ++y)
			{
				var v0 = y * vertexColumnCount;
				var v1 = v0 + vertexColumnCount;
				for (int x = 1; x <= columnCount; ++x)
				{
					builder.AddFace(v0 + x - 1, v1 + x - 1, v1 + x, v0 + x);
				}
			}

			for (int y = 0; y < vertexRowCount; ++y)
			{
				var v = y * vertexColumnCount;
				for (int x = 0; x < vertexColumnCount; ++x)
				{
					vertexPositions[v + x] = new Vector3(x, y, 0f);
				}
			}

			var manifold = builder.BuildTopology<Manifold>();
			manifold.vertexPositions = Vector3VertexAttribute.CreateInstance(vertexPositions, "Vertex Positions");
			return manifold;
		}

		public static Manifold CreateQuadGrid(int columnCount, int rowCount, out CartesianQuadFaceMapper mapper)
		{
			mapper = new CartesianQuadFaceMapper(columnCount);
			return CreateQuadGrid(columnCount, rowCount);
		}

		private static PlanarWrapAroundManifold CreateQuadGrid(int columnCount, int rowCount, bool horizontalWrapAround, bool verticalWrapAround)
		{
			if (!horizontalWrapAround && !verticalWrapAround)
			{
				throw new System.ArgumentException("At least one of horizontal and vertical wrap-around must be enabled.  To create a grid with no wrap, use the function with no wrap-around parameters.", "horizontalWrapAround");
			}

			var builder = new Topology.FaceVerticesBuilder(columnCount * rowCount, columnCount * rowCount * 4 + columnCount * 2);

			var vertexColumnCount = columnCount + (horizontalWrapAround ? 0 : 1);
			var vertexRowCount = rowCount + (verticalWrapAround ? 0 : 1);

			var vertexPositions = new Vector3[vertexColumnCount * vertexRowCount];

			var internalEdgeCount = columnCount * rowCount * 4;
			var externalEdgeCount = (horizontalWrapAround ? 0 : rowCount * 2) + (verticalWrapAround ? 0 : columnCount * 2);

			System.Action<int, int> buildRow;
			if (horizontalWrapAround)
			{
				buildRow = (int v0, int v1) =>
				{
					for (int x = 1; x < columnCount; ++x)
					{
						builder.AddFace(v0 + x - 1, v1 + x - 1, v1 + x, v0 + x);
					}

					builder.AddFace(v0 + columnCount - 1, v1 + columnCount - 1, v1, v0);
				};
			}
			else
			{
				buildRow = (int v0, int v1) =>
				{
					for (int x = 1; x <= columnCount; ++x)
					{
						builder.AddFace(v0 + x - 1, v1 + x - 1, v1 + x, v0 + x);
					}
				};
			}

			if (verticalWrapAround)
			{
				for (int y = 1; y < rowCount; ++y)
				{
					var v1 = y * vertexColumnCount;
					var v0 = v1 - vertexColumnCount;
					buildRow(v0, v1);
				}

				buildRow((rowCount - 1) * vertexColumnCount, 0);
			}
			else
			{
				for (int y = 0; y < rowCount; ++y)
				{
					var v0 = y * vertexColumnCount;
					var v1 = v0 + vertexColumnCount;
					buildRow(v0, v1);
				}
			}

			for (int y = 0; y < vertexRowCount; ++y)
			{
				var v = y * vertexColumnCount;
				for (int x = 0; x < vertexColumnCount; ++x)
				{
					vertexPositions[v + x] = new Vector3(x, y, 0f);
				}
			}

			var manifold = builder.BuildTopology<PlanarWrapAroundManifold>();
			manifold.vertexPositions = Vector3VertexAttribute.CreateInstance(vertexPositions, "Vertex Positions");
			manifold.edgeWrapData = EdgeWrapDataEdgeAttribute.CreateInstance(new EdgeWrapData[internalEdgeCount + externalEdgeCount], "Edge Wrap Data");

			if (horizontalWrapAround && verticalWrapAround)
			{
				manifold.repetitionAxes = new Vector2[3]
				{
					new Vector2(0f, 0f),
					new Vector2(0f, +rowCount),
					new Vector2(+columnCount, 0f),
				};
			}
			else if (horizontalWrapAround)
			{
				manifold.repetitionAxes = new Vector2[2]
				{
					new Vector2(0f, 0f),
					new Vector2(+columnCount, 0f),
				};
			}
			else if (verticalWrapAround)
			{
				manifold.repetitionAxes = new Vector2[2]
				{
					new Vector2(0f, 0f),
					new Vector2(0f, +rowCount),
				};
			}

			if (horizontalWrapAround)
			{
				var axis = verticalWrapAround ? 2 : 1;
				var faceIndexOffset = columnCount - 1;
				for (int y = 0; y < rowCount; ++y)
				{
					var faceIndex = y * columnCount;
					foreach (var edge in manifold.internalFaces[faceIndex].edges)
					{
						if (edge.farFace.index == faceIndex + faceIndexOffset)
						{
							manifold.edgeWrapData[edge] = new EdgeWrapData(axis, true);
							manifold.edgeWrapData[edge.twin] = new EdgeWrapData(axis, false);
							break;
						}
					}
				}
			}

			if (verticalWrapAround)
			{
				var axis = 1;
				var faceIndexOffset = (rowCount - 1) * columnCount;
				for (int x = 0; x < columnCount; ++x)
				{
					var faceIndex = x;
					foreach (var edge in manifold.internalFaces[faceIndex].edges)
					{
						if (edge.farFace.index == faceIndex + faceIndexOffset)
						{
							manifold.edgeWrapData[edge] = new EdgeWrapData(axis, true);
							manifold.edgeWrapData[edge.twin] = new EdgeWrapData(axis, false);
							break;
						}
					}
				}
			}

			/*for (int y = 0; y < rowCount; ++y)
			{
				for (int x = 0; x < columnCount; ++x)
				{
					var f = y * columnCount + x;
					Debug.LogFormat("<b>Face {0} @ ({1}, {2})</b>", f, x, y);
					foreach (var edge in topology.internalFaces[f].edges)
					{
						var faceVertexPosition = GetFaceVertexPosition(edge, vertexPositions, edgeWrapData, repetitionAxes);
						Debug.LogFormat("Vertex {0} <b>{1}</b> {2}", edge.nextVertex.index, faceVertexPosition, vertexPositions[edge.nextVertex]);
					}
				}
			}*/

			return manifold;
		}

		private static PlanarWrapAroundManifold CreateQuadGrid(int columnCount, int rowCount, bool horizontalWrapAround, bool verticalWrapAround, out CartesianQuadFaceMapper mapper)
		{
			mapper = new CartesianQuadFaceMapper(columnCount); //TODO create mapper that includes wrap-around logic
			return CreateQuadGrid(columnCount, rowCount, horizontalWrapAround, verticalWrapAround);
		}

		public static PlanarWrapAroundManifold CreateHorizontalWrapAroundQuadGrid(int columnCount, int rowCount)
		{
			return CreateQuadGrid(columnCount, rowCount, true, false);
		}

		public static PlanarWrapAroundManifold CreateVerticalWrapAroundQuadGrid(int columnCount, int rowCount)
		{
			return CreateQuadGrid(columnCount, rowCount, false, true);
		}

		public static PlanarWrapAroundManifold CreateFullWrapAroundQuadGrid(int columnCount, int rowCount)
		{
			return CreateQuadGrid(columnCount, rowCount, true, true);
		}

		public static PlanarWrapAroundManifold CreateHorizontalWrapAroundQuadGrid(int columnCount, int rowCount, out CartesianQuadFaceMapper mapper)
		{
			return CreateQuadGrid(columnCount, rowCount, true, false, out mapper);
		}

		public static PlanarWrapAroundManifold CreateVerticalWrapAroundQuadGrid(int columnCount, int rowCount, out CartesianQuadFaceMapper mapper)
		{
			return CreateQuadGrid(columnCount, rowCount, false, true, out mapper);
		}

		public static PlanarWrapAroundManifold CreateFullWrapAroundQuadGrid(int columnCount, int rowCount, out CartesianQuadFaceMapper mapper)
		{
			return CreateQuadGrid(columnCount, rowCount, true, true, out mapper);
		}

		public static CartesianQuadFaceMapper CreateQuadGridFaceMapper(int columnCount, int rowCount)
		{
			return new CartesianQuadFaceMapper(columnCount);
		}

		public static Manifold CreatePointyTopHexGrid(int columnCount, int rowCount, bool horizontalWrapAround, bool verticalWrapAround)
		{
			return null;
		}

		public static Manifold CreateFlatTopHexGrid(int columnCount, int rowCount, bool horizontalWrapAround, bool verticalWrapAround)
		{
			return null;
		}
	}
}
