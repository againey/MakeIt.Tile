using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
#if false
	public class PlanarWrapAroundManifold : Manifold
	{
		[SerializeField]
		protected Vector2[] _repetitionAxes;

		[SerializeField]
		protected EdgeWrapDataEdgeAttribute _edgeWrapData;

		public PlanarWrapAroundManifold()
		{
		}

		public PlanarWrapAroundManifold(PlanarWrapAroundManifold original)
		{
			CloneFields(original);
		}

		protected void CloneFields(PlanarWrapAroundManifold original)
		{
			base.CloneFields(original);
			_repetitionAxes = original._repetitionAxes.Clone() as Vector2[];
			_edgeWrapData = EdgeWrapDataEdgeAttribute.CreateInstance(original._edgeWrapData.array, original._edgeWrapData.name);
		}

		public new PlanarWrapAroundManifold Clone()
		{
			return new PlanarWrapAroundManifold(this);
		}

		public Vector2[] repetitionAxes { get { return _repetitionAxes; } set { _repetitionAxes = value; } }
		public EdgeWrapDataEdgeAttribute edgeWrapData { get { return _edgeWrapData; } set { _edgeWrapData = value; } }

		private Vector3 AddRepetitionAxis(Vector3 position, int repetitionAxis)
		{
			position.x += _repetitionAxes[repetitionAxis].x;
			position.y += _repetitionAxes[repetitionAxis].y;
			return position;
		}

		private Vector3 AddRepetitionAxis(Vector3 position, int repetitionAxis0, int repetitionAxis1)
		{
			position.x += _repetitionAxes[repetitionAxis0].x + _repetitionAxes[repetitionAxis1].x;
			position.y += _repetitionAxes[repetitionAxis0].y + _repetitionAxes[repetitionAxis1].y;
			return position;
		}

		public Vector3 GetFaceVertexPosition(FaceEdge prevEdge)
		{
			var prevEdgeWrapData = _edgeWrapData[prevEdge];
			var nextEdgeWrapData = _edgeWrapData[prevEdge.next];
			var canonicalPosition = vertexPositions[prevEdge.nextVertex];

			if (!prevEdgeWrapData.isWrapped || prevEdgeWrapData.isNegatedAxis)
			{
				if (!nextEdgeWrapData.isWrapped || nextEdgeWrapData.isNegatedAxis)
				{
					return canonicalPosition;
				}
				else
				{
					return AddRepetitionAxis(canonicalPosition, nextEdgeWrapData.repetitionAxis);
				}
			}
			else
			{
				if (!nextEdgeWrapData.isWrapped || nextEdgeWrapData.isNegatedAxis)
				{
					return AddRepetitionAxis(canonicalPosition, prevEdgeWrapData.repetitionAxis);
				}
				else
				{
					return AddRepetitionAxis(canonicalPosition, prevEdgeWrapData.repetitionAxis, nextEdgeWrapData.repetitionAxis);
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
#endif
}
