﻿using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public class WrapAroundManifold : Manifold
	{
		[SerializeField]
		protected Vector3[] _repetitionAxes;

		[SerializeField]
		protected EdgeWrapData[] _edgeWrapData;

		public WrapAroundManifold()
		{
		}

		public WrapAroundManifold(WrapAroundManifold original)
		{
			CloneFields(original);
		}

		protected void CloneFields(WrapAroundManifold original)
		{
			base.CloneFields(original);
			_repetitionAxes = original._repetitionAxes.Clone() as Vector3[];
			_edgeWrapData = original._edgeWrapData.Clone() as EdgeWrapData[];
		}

		public new WrapAroundManifold Clone()
		{
			return new WrapAroundManifold(this);
		}

		public Vector3[] repetitionAxes { get { return _repetitionAxes; } set { _repetitionAxes = value; } }
		public EdgeWrapData[] edgeWrapData { get { return _edgeWrapData; } set { _edgeWrapData = value; } }

		public Vector3 GetFaceVertexPosition(FaceEdge prevEdge)
		{
			var prevEdgeWrapData = _edgeWrapData[prevEdge];
			var nextEdgeWrapData = _edgeWrapData[prevEdge.next];
			var canonicalPosition = _vertexPositions[prevEdge.nextVertex];

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

		public override Vector3[] CalculateFaceCentroids(bool includeExternalFaces)
		{
			var faces = GetFaces(includeExternalFaces);
			var centroids = new Vector3[faces.Count];

			foreach (var face in faces)
			{
				var centroid = new Vector3(0f, 0f, 0f);
				foreach (var edge in face.edges)
				{
					centroid += GetFaceVertexPosition(edge);
				}
				centroids[face] = centroid / face.neighborCount;
			}

			return centroids;
		}

		public override Vector3[] CalculateFaceNormals(bool includeExternalFaces)
		{
			var faces = GetFaces(includeExternalFaces);
			var normals = new Vector3[faces.Count];

			foreach (var face in faces)
			{
				if (face.neighborCount == 3)
				{
					var edge = face.firstEdge;
					var p0 = GetFaceVertexPosition(edge);
					edge = edge.next;
					var p1 = GetFaceVertexPosition(edge);
					edge = edge.next;
					var p2 = GetFaceVertexPosition(edge);
					normals[face] = Vector3.Cross(p1 - p0, p2 - p0).normalized;
				}
				else
				{
					var normalSum = new Vector3(0f, 0f, 0f);
					foreach (var edge in face.edges)
					{
						normalSum += GetFaceVertexPosition(edge);
					}
					normals[face] = normalSum.normalized;
				}
			}

			return normals;
		}

		public override float[] CalculateFaceAreas(Vector3[] centroids, bool includeExternalFaces)
		{
			var faces = GetFaces(includeExternalFaces);
			var areas = new float[faces.Count];

			foreach (var face in faces)
			{
				if (face.neighborCount == 3)
				{
					var edge = face.firstEdge;
					var p0 = GetFaceVertexPosition(edge);
					edge = edge.next;
					var p1 = GetFaceVertexPosition(edge);
					edge = edge.next;
					var p2 = GetFaceVertexPosition(edge);
					areas[face] = Vector3.Cross(p1 - p0, p2 - p0).magnitude * 0.5f;
				}
				else
				{
					var p0 = centroids[face];
					float doubleAreaSum = 0f;
					var edge = face.firstEdge;
					var p1 = GetFaceVertexPosition(edge);
					edge = edge.next;
					var firstEdge = edge;
					do
					{
						var p2 = GetFaceVertexPosition(edge);
						doubleAreaSum += Vector3.Cross(p1 - p0, p2 - p0).magnitude;
						p1 = p2;
						edge = edge.next;
					} while (edge != firstEdge);
					areas[face] = doubleAreaSum / (2 * face.neighborCount);
				}
			}

			return areas;
		}

		public override float[] CalculateTriangularFaceAreas(bool includeExternalFaces)
		{
			var faces = GetFaces(includeExternalFaces);
			var areas = new float[faces.Count];

			foreach (var face in faces)
			{
				if (face.neighborCount == 3)
				{
					var edge = face.firstEdge;
					var p0 = GetFaceVertexPosition(edge);
					edge = edge.next;
					var p1 = GetFaceVertexPosition(edge);
					edge = edge.next;
					var p2 = GetFaceVertexPosition(edge);
					areas[face] = Vector3.Cross(p1 - p0, p2 - p0).magnitude * 0.5f;
				}
				else
				{
					throw new System.InvalidOperationException("Unexpected non-triangular face encountered while calculating face areas without centroids.");
				}
			}

			return areas;
		}

		//TODO
		//public override float[] CalculateVertexAreas(Vector3[] centroids)
		//public override float[] CalculateVertexAngles()

		public override float[] CalculateFaceAngles(bool includeExternalFaces)
		{
			var edges = GetFaceEdges(includeExternalFaces);
			var angles = new float[edges.Count];

			foreach (var edge in edges)
			{
				var p0 = GetFaceVertexPosition(edge.prev.prev);
				var p1 = GetFaceVertexPosition(edge.prev);
				var p2 = GetFaceVertexPosition(edge);
				var v1 = (p1 - p0).normalized;
				var v2 = (p2 - p0).normalized;
				angles[edge] = SphericalManifoldUtility.AngleBetweenUnitVectors(v1, v2);
			}

			return angles;
		}

		//TODO
		//public override float[] CalculateVertexEdgeLengths()
		//public override Vector3[] CalculateVertexEdgeMidpoints()
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
