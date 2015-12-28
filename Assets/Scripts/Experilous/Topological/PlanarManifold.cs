using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public static class PlanarManifold
	{
		public static Manifold CreatePointyTopTriGrid(int columnCount, int rowCount, bool horizontalWrapAround, bool verticalWrapAround)
		{
			return null;
		}

		public static Manifold CreateFlatTopTriGrid(int columnCount, int rowCount, bool horizontalWrapAround, bool verticalWrapAround)
		{
			return null;
		}

		public static Manifold CreateQuadGrid(int columnCount, int rowCount, bool horizontalWrapAround, bool verticalWrapAround, out Vector3[] repetitionAxes, out EdgeWrapData[] edgeWrapData)
		{
			var builder = new Topology.FaceVerticesBuilder(columnCount * rowCount, columnCount * rowCount * 4 + columnCount * 2);

			var vertexColumnCount = columnCount + (horizontalWrapAround ? 0 : 1);
			var vertexRowCount = rowCount + (verticalWrapAround ? 0 : 1);

			var vertexPositions = new Vector3[vertexColumnCount * vertexRowCount];

			var internalEdgeCount = columnCount * rowCount * 4;
			var externalEdgeCount = (horizontalWrapAround ? 0 : rowCount * 2) + (verticalWrapAround ? 0 : columnCount * 2);
			edgeWrapData = new EdgeWrapData[internalEdgeCount + externalEdgeCount];

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

			var topology = builder.BuildTopology();

			if (horizontalWrapAround && verticalWrapAround)
			{
				repetitionAxes = new Vector3[3]
				{
					new Vector3(0f, 0f, 0f),
					new Vector3(0f, +rowCount, 0f),
					new Vector3(+columnCount, 0f, 0f),
				};
			}
			else if (horizontalWrapAround)
			{
				repetitionAxes = new Vector3[2]
				{
					new Vector3(0f, 0f, 0f),
					new Vector3(+columnCount, 0f, 0f),
				};
			}
			else if (verticalWrapAround)
			{
				repetitionAxes = new Vector3[2]
				{
					new Vector3(0f, 0f, 0f),
					new Vector3(0f, +rowCount, 0f),
				};
			}
			else
			{
				repetitionAxes = null;
			}

			if (horizontalWrapAround)
			{
				var axis = verticalWrapAround ? 2 : 1;
				var faceIndexOffset = columnCount - 1;
				for (int y = 0; y < rowCount; ++y)
				{
					var faceIndex = y * columnCount;
					foreach (var edge in topology.internalFaces[faceIndex].edges)
					{
						if (edge.farFace.index == faceIndex + faceIndexOffset)
						{
							edgeWrapData[edge] = new EdgeWrapData(axis, true);
							edgeWrapData[edge.twin] = new EdgeWrapData(axis, false);
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
					foreach (var edge in topology.internalFaces[faceIndex].edges)
					{
						if (edge.farFace.index == faceIndex + faceIndexOffset)
						{
							edgeWrapData[edge] = new EdgeWrapData(axis, true);
							edgeWrapData[edge.twin] = new EdgeWrapData(axis, false);
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

			return new Manifold(topology, vertexPositions);
		}

		public static Manifold CreateQuadGrid(int columnCount, int rowCount, bool horizontalWrapAround, bool verticalWrapAround, out Vector3[] repetitionAxes, out EdgeWrapData[] edgeWrapData, out CartesianQuadFaceMapper mapper)
		{
			mapper = new CartesianQuadFaceMapper(columnCount);
			return CreateQuadGrid(columnCount, rowCount, horizontalWrapAround, verticalWrapAround, out repetitionAxes, out edgeWrapData);
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

		public static Vector3 GetFaceVertexPosition(Topology.FaceEdge prevEdge, Vector3[] vertexPositions, EdgeWrapData[] edgeWrapData, Vector3[] repetitionAxes)
		{
			return GetFaceVertexPosition(edgeWrapData[prevEdge], edgeWrapData[prevEdge.next], vertexPositions[prevEdge.nextVertex], repetitionAxes);
		}

		public static Vector3 GetFaceVertexPosition(Topology.FaceEdge prevEdge, Vector3 canonicalPosition, EdgeWrapData[] edgeWrapData, Vector3[] repetitionAxes)
		{
			return GetFaceVertexPosition(edgeWrapData[prevEdge], edgeWrapData[prevEdge.next], canonicalPosition, repetitionAxes);
		}

		public static Vector3 GetFaceVertexPosition(EdgeWrapData prevEdgeWrapData, EdgeWrapData nextEdgeWrapData, Vector3 canonicalPosition, Vector3[] repetitionAxes)
		{
			if (!prevEdgeWrapData.isWrapped || prevEdgeWrapData.isNegatedAxis)
			{
				if (!nextEdgeWrapData.isWrapped || nextEdgeWrapData.isNegatedAxis)
				{
					return canonicalPosition;
				}
				else
				{
					return canonicalPosition + repetitionAxes[nextEdgeWrapData.repetitionAxis];
				}
			}
			else
			{
				if (!nextEdgeWrapData.isWrapped || nextEdgeWrapData.isNegatedAxis)
				{
					return canonicalPosition + repetitionAxes[prevEdgeWrapData.repetitionAxis];
				}
				else
				{
					return canonicalPosition + repetitionAxes[prevEdgeWrapData.repetitionAxis] + repetitionAxes[nextEdgeWrapData.repetitionAxis];
				}
			}
		}
	}

	public struct EdgeWrapData
	{
		private byte _data;

		[System.Flags]
		public enum Flags : byte
		{
			None = (byte)0x00u,
			RepetitionAxisBits = (byte)0x03u, //0b00000011
			UnusedBits = (byte)0x38u, //0b01111100
			IsNegatedAxis = (byte)0x80u, //0b10000000
		}

		public EdgeWrapData(int repetitionAxis, bool isNegatedAxis)
		{
			_data = (byte)
			(
				(repetitionAxis & (int)Flags.RepetitionAxisBits) |
				(isNegatedAxis ? (int)Flags.IsNegatedAxis : 0)
			);
		}

		public bool isWrapped
		{
			get { return (_data & (int)Flags.RepetitionAxisBits) != 0; }
		}

		public int repetitionAxis
		{
			get { return _data & (int)Flags.RepetitionAxisBits; }
			set { _data = (byte)((_data & ~(int)Flags.RepetitionAxisBits) | (value & (int)Flags.RepetitionAxisBits)); }
		}

		public bool isNegatedAxis
		{
			get { return (_data & (int)Flags.IsNegatedAxis) != 0; }
			set { _data = (byte)((_data & ~(int)Flags.IsNegatedAxis) | (value ? (int)Flags.IsNegatedAxis : 0)); }
		}
	}
}
