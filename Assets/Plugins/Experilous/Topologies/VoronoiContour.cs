/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Experilous.Topologies
{
	public class ContourTriangulator
	{
		public struct PositionId : IEquatable<PositionId>
		{
			private const ulong NODE = 0x0000000000000000U;
			private const ulong EDGE = 0x1000000000000000U;
			private const ulong SUBEDGE_ENTER = 0x2000000000000000U;
			private const ulong SUBEDGE_CONTINUE = 0x3000000000000000U;
			private const ulong SUBEDGE_EXIT = 0x4000000000000000U;
			private const ulong FACE = 0x5000000000000000U;

			private const ulong INDEX_MASK = 0x00000000FFFFFFFFU;
			private const ulong CONTOUR_MASK = 0x00000FFF00000000U;
			private const int CONTOUR_SHIFT = 32;
			private const ulong SEGMENT_MASK = 0x00FFF00000000000U;
			private const int SEGMENT_SHIFT = 44;

			private ulong _value;

			private PositionId(ulong value)
			{
				_value = value;
			}

			public PositionId(TopologyNode node)
			{
				_value = NODE | ((ulong)node.index & INDEX_MASK);
			}

			public PositionId(TopologyEdge edge, int contour)
			{
				_value = EDGE | ((ulong)(edge.index ^ 1) & INDEX_MASK) | (((ulong)contour << CONTOUR_SHIFT) & CONTOUR_MASK);
			}

			public PositionId(TopologyEdge edge, int contour, int segment, int segmentCount, bool enter, bool exit)
			{
				if (edge.isSecondTwin)
				{
					segment = segmentCount - segment;
					enter = !enter;
					exit = !exit;
				}
				ulong type = (enter != exit) ? (enter ? SUBEDGE_ENTER : SUBEDGE_EXIT) : SUBEDGE_CONTINUE;
				_value = type | ((ulong)(edge.index ^ 1) & INDEX_MASK) | (((ulong)contour << CONTOUR_SHIFT) & CONTOUR_MASK) | (((ulong)segment << SEGMENT_SHIFT) & SEGMENT_MASK);
			}

			public PositionId(TopologyFace face, int contour, int segment)
			{
				_value = FACE | ((ulong)face.index & INDEX_MASK) | (((ulong)contour << CONTOUR_SHIFT) & CONTOUR_MASK) | (((ulong)segment << SEGMENT_SHIFT) & SEGMENT_MASK);
			}

			public bool Equals(PositionId other)
			{
				return _value == other._value;
			}

			public override bool Equals(object other)
			{
				return other is PositionId && Equals((PositionId)other);
			}

			public override int GetHashCode()
			{
				return _value.GetHashCode();
			}

			public override string ToString()
			{
				return "0x" + _value.ToString("X16");
			}
		}

		private struct Vertex
		{
			public PositionId positionId;
			public Vector3 position;
			public float distance;

			public Vertex(PositionId positionId, Vector3 position, float distance)
			{
				this.positionId = positionId;
				this.position = position;
				this.distance = distance;
			}
		}

		private struct Contour
		{
			public int index;
			public int nextSegmentIndex;
			public float distance;
			public List<Vertex> vertices;

			public class Comparer : IComparer<Contour>
			{
				public int Compare(Contour lhs, Contour rhs)
				{
					return (int)Numerics.Math.ZeroInclusiveSign(lhs.distance - rhs.distance);
				}
			}
		}

		public delegate void OnVertexDelegate(PositionId positionId, Vector3 position, VoronoiSiteType siteType, int siteIndex, int contourIndex, float distance);
		public delegate void OnTriangleDelegate(PositionId positionId0, PositionId positionId1, PositionId positionId2);

		private VoronoiDiagram _voronoiDiagram;
		private OnVertexDelegate _onVertex;
		private OnTriangleDelegate _onTriangle;

		private float _maxCurvaturePerSegment;
		private float _errorMargin;

		private Vector3 _normal;

		private TopologyFace _initialFace;
		private TopologyFace _nextFace;

		private Contour[] _contours = new Contour[0];
		private int _contourCount;
		private readonly Contour.Comparer _contourComparer = new Contour.Comparer();

		public ContourTriangulator(float maxCurvaturePerSegment = 0.25f, float errorMargin = 0.0001f)
		{
			_maxCurvaturePerSegment = maxCurvaturePerSegment;
			_errorMargin = errorMargin;
		}

		public float maxCurvaturePerSegment
		{
			get
			{
				return _maxCurvaturePerSegment;
			}
			set
			{
				if (value <= 0f) throw new ArgumentOutOfRangeException("value", "The maximum curvature per segment must be greater than zero.");
				_maxCurvaturePerSegment = value;
			}
		}

		public float errorMargin
		{
			get
			{
				return _errorMargin;
			}
			set
			{
				if (value <= 0f) throw new ArgumentOutOfRangeException("value", "The error margin must be greater than zero.");
				_errorMargin = value;
			}
		}

		public void Triangulate(VoronoiDiagram voronoiDiagram, TopologyFace initialFace, OnVertexDelegate onVertex, OnTriangleDelegate onTriangle, Vector3 normal, params float[] contourDistances)
		{
			BeginTriangulation(voronoiDiagram, initialFace, onVertex, onTriangle, normal, contourDistances);
			while (TriangulateSegment()) { }
		}

		public void BeginTriangulation(VoronoiDiagram voronoiDiagram, TopologyFace initialFace, OnVertexDelegate onVertex, OnTriangleDelegate onTriangle, Vector3 normal, params float[] contourDistances)
		{
			_voronoiDiagram = voronoiDiagram;
			_onVertex = onVertex;
			_onTriangle = onTriangle;
			_normal = normal;

			_initialFace = TopologyFace.none;
			_nextFace = initialFace;

			_contourCount = contourDistances.Length;

			if (_contours.Length < _contourCount)
			{
				var newContours = new Contour[_contourCount];

				Array.Copy(_contours, newContours, _contours.Length);
				for (int i = _contours.Length; i < _contourCount; ++i)
				{
					newContours[i].vertices = new List<Vertex>();
				}

				_contours = newContours;
			}

			for (int i = 0; i < _contourCount; ++i)
			{
				_contours[i].index = i;
				_contours[i].distance = contourDistances[i];
				_contours[i].vertices.Clear();
			}

			Array.Sort(_contours, 0, _contourCount, _contourComparer);
		}

		public bool TriangulateSegment()
		{
			if (_nextFace == _initialFace) return false;

			var currentFace = _nextFace;

			if (!_initialFace)
			{
				_initialFace = _nextFace;
			}
			else
			{
				_nextFace = _initialFace;
			}

			int siteIndex = _voronoiDiagram._voronoiFaceSiteIndices[currentFace];
			var siteType = _voronoiDiagram._voronoiFaceSiteTypes[currentFace];

			TopologyFaceEdge baseVoronoiEdge;
			TopologyFaceEdge currentVoronoiEdge;
			switch (siteType)
			{
				case VoronoiSiteType.Point:
					baseVoronoiEdge = new TopologyFaceEdge(_voronoiDiagram._voronoiTopology, _voronoiDiagram._siteNodeFirstVoronoiEdgeIndices[siteIndex]);
					currentVoronoiEdge = baseVoronoiEdge;
					break;
				case VoronoiSiteType.Line:
					baseVoronoiEdge = new TopologyFaceEdge(_voronoiDiagram._voronoiTopology, _voronoiDiagram._siteEdgeFirstVoronoiEdgeIndices[siteIndex]);
					currentVoronoiEdge = baseVoronoiEdge.next;
					break;
				case VoronoiSiteType.None:
					throw new InvalidOperationException();
				default:
					throw new NotImplementedException();
			}

			VoronoiEdgeShape currentEdgeShape = _voronoiDiagram._voronoiEdgeShapes[currentVoronoiEdge];
			float tPrev = float.NaN;
			float tNext = 0f;

			int innerContourIndex = -1;
			int outerContourIndex = 0;

			for (int i = 0; i < _contourCount; ++i)
			{
				_contours[i].nextSegmentIndex = 0;
			}

			while (true)
			{
				float tNextEntrance = float.NaN;
				float tNextExit = float.NaN;

				bool moveToNextEdge = false;
				bool moveToNextContour = false;
				bool moveToPrevContour = false;

				if (outerContourIndex < _contourCount)
				{
					tNextEntrance = currentEdgeShape.IntersectEntranceUnclamped(_contours[outerContourIndex].distance);
					if (tNextEntrance - currentEdgeShape.t1 > _errorMargin)
					{
						// Next contour enters after end of current edge; ignore it for now.
						tNextEntrance = float.NaN;
					}
					else if (currentEdgeShape.t1 - tNextEntrance < _errorMargin)
					{
						// Next contour enters right at end of current edge.
						if (currentEdgeShape.GetDistanceDerivative(tNextEntrance) <= _errorMargin && currentEdgeShape.GetDistanceSecondDerivative(tNextEntrance) <= _errorMargin)
						{
							// Neither distance 1st deriv or 2nd deriv are positive, so ignore it for now.
							tNextEntrance = float.NaN;
						}
					}
				}

				if (innerContourIndex >= 0)
				{
					tNextExit = currentEdgeShape.IntersectExitUnclamped(_contours[innerContourIndex].distance);
					if (tNextExit - currentEdgeShape.t1 > _errorMargin)
					{
						// Current contour exits after end of current edge; ignore it for now.
						tNextExit = float.NaN;
					}
					else if (tNextExit < tPrev)
					{
						// Concave curve with an irrelevant earlier exit and subsequent entrance.
						tNextExit = float.NaN;
					}
				}

				if (!float.IsNaN(tNextEntrance) && !float.IsNaN(tNextExit))
				{
					// If both the next entrance and exit are valid, figure out which happens first, and invalidate the other one.
					if (tNextEntrance < tNextExit)
					{
						tNextExit = float.NaN;
					}
					else
					{
						tNextEntrance = float.NaN;
					}
				}

				PositionId positionId;
				Vector3 position;
				float distance;

				if (!float.IsNaN(tNextEntrance))
				{
					moveToNextContour = true;
					distance = _contours[outerContourIndex].distance;

					if (currentEdgeShape.t1 - tNextEntrance > _errorMargin)
					{
						// Next contour enters before end of current edge.
						tNext = tNextEntrance;

						moveToNextEdge = false;

						if (tNextEntrance - currentEdgeShape.t0 < _errorMargin)
						{
							// Next contour enters before or right at start of current edge.
							positionId = new PositionId(currentVoronoiEdge.prevNode);
							position = _voronoiDiagram._voronoiNodePositions[currentVoronoiEdge.prevNode];
						}
						else
						{
							// Next contour enters in middle of current edge.
							positionId = new PositionId(currentVoronoiEdge, outerContourIndex);
							position = currentEdgeShape.Evaluate(tNext);
						}
					}
					else
					{
						// Next contour enters right at end of current edge.
						tNext = currentEdgeShape.t1;

						moveToNextEdge = true;

						positionId = new PositionId(currentVoronoiEdge.nextNode);
						position = _voronoiDiagram._voronoiNodePositions[currentVoronoiEdge.nextNode];
					}
				}
				else if (!float.IsNaN(tNextExit))
				{
					moveToPrevContour = true;
					distance = _contours[innerContourIndex].distance;

					if (currentEdgeShape.t1 - tNextExit > _errorMargin)
					{
						// Current contour exits before end of current edge.
						tNext = tNextExit;

						moveToNextEdge = false;

						positionId = new PositionId(currentVoronoiEdge, innerContourIndex);
						position = currentEdgeShape.Evaluate(tNext);
					}
					else
					{
						// Current contour exits right at end of current edge.
						tNext = currentEdgeShape.t1;

						moveToNextEdge = true;

						positionId = new PositionId(currentVoronoiEdge.nextNode);
						position = _voronoiDiagram._voronoiNodePositions[currentVoronoiEdge.nextNode];
					}
				}
				else
				{
					tNext = currentEdgeShape.t1;

					moveToNextEdge = true;

					if (innerContourIndex >= 0 && outerContourIndex < _contourCount)
					{
						positionId = new PositionId(currentVoronoiEdge.nextNode);
						position = _voronoiDiagram._voronoiNodePositions[currentVoronoiEdge.nextNode];
						distance = currentEdgeShape.GetDistance(tNext);
					}
					else
					{
						positionId = new PositionId();
						position = Vector3.zero;
						distance = float.NaN;
					}
				}

				if ((moveToNextContour || innerContourIndex >= 0) && outerContourIndex < _contourCount)
				{
					// Subdivide current edge from tPrev to tNext
					float cumulativeCurvature = Mathf.Abs(currentEdgeShape.GetCurvatureSum(tPrev, tNext));
					int segmentCount = Mathf.CeilToInt(cumulativeCurvature / _maxCurvaturePerSegment);

					for (int i = 1; i < segmentCount; ++i)
					{
						float tSegment = currentEdgeShape.GetCurvatureSumOffset(tPrev, (cumulativeCurvature * i) / segmentCount);

						var segmentVertexPositionId = new PositionId(currentVoronoiEdge, outerContourIndex, i, segmentCount, moveToNextContour, moveToPrevContour);
						var segmentVertexPosition = currentEdgeShape.Evaluate(tSegment);
						var segmentVertexDistance = currentEdgeShape.GetDistance(tSegment);
						_contours[outerContourIndex].vertices.Add(new Vertex(segmentVertexPositionId, segmentVertexPosition, segmentVertexDistance));
						_onVertex(segmentVertexPositionId, segmentVertexPosition, siteType, siteIndex, outerContourIndex, segmentVertexDistance);

						tPrev = tSegment;
					}
				}

				if (moveToPrevContour)
				{
					// Subdivide current contour from its last position to the new position.
					var contourVertices = _contours[innerContourIndex].vertices;
					if (contourVertices.Count > 0)
					{
						switch (siteType)
						{
							case VoronoiSiteType.Point:
								{
									var pivotPosition = _voronoiDiagram._pointSitePositions[siteIndex];

									var prevPosition = contourVertices[contourVertices.Count - 1].position;
									var prevVector = prevPosition - pivotPosition;
									var nextVector = position - pivotPosition;
									var prevDirection = prevVector.normalized;
									var nextDirection = nextVector.normalized;
									float angleCosine = Vector3.Dot(prevDirection, nextDirection);
									float angleSine = Vector3.Dot(Vector3.Cross(prevDirection, nextDirection), _normal);
									float cumulativeCurvature = Mathf.Repeat(Mathf.Atan2(angleSine, angleCosine), Mathf.PI * 2f);
									int segmentCount = Mathf.CeilToInt(cumulativeCurvature / _maxCurvaturePerSegment);

									if (segmentCount > 1)
									{
										int nextSegmentIndex = _contours[innerContourIndex].nextSegmentIndex;
										var rightVector = Vector3.Cross(_normal, prevVector);
										for (int i = 1; i < segmentCount; ++i)
										{
											float tSegment = (cumulativeCurvature * i) / segmentCount;

											var segmentVertexPositionId = new PositionId(currentFace, innerContourIndex, nextSegmentIndex++); //TODO: When distance is 0, make this position id be assigned to the edge rather than face.
											var segmentVertexPosition = pivotPosition + prevVector * Mathf.Cos(tSegment) + rightVector * Mathf.Sin(tSegment);
											_contours[innerContourIndex].vertices.Add(new Vertex(segmentVertexPositionId, segmentVertexPosition, distance));
											_onVertex(segmentVertexPositionId, segmentVertexPosition, siteType, siteIndex, innerContourIndex, distance);

											tPrev = tSegment;
										}
										_contours[innerContourIndex].nextSegmentIndex = nextSegmentIndex;
									}
								}
								break;
							case VoronoiSiteType.Line:
								// No straight line subdivision supported at this time; do nothing.
								break;
						}
					}

					_contours[innerContourIndex].vertices.Add(new Vertex(positionId, position, distance));
					_onVertex(positionId, position, siteType, siteIndex, innerContourIndex, distance);

					if (outerContourIndex < _contourCount)
					{
						CreateTriangles(outerContourIndex);
					}

					if (innerContourIndex == 0)
					{
						_nextFace = currentVoronoiEdge.face;
					}
				}
				else if ((moveToNextContour || innerContourIndex >= 0) && outerContourIndex < _contourCount)
				{
					_contours[outerContourIndex].vertices.Add(new Vertex(positionId, position, distance));
					_onVertex(positionId, position, siteType, siteIndex, outerContourIndex, distance);
				}

				if (moveToNextEdge)
				{
					currentVoronoiEdge = currentVoronoiEdge.next;

					if (currentVoronoiEdge == baseVoronoiEdge) break;

					currentEdgeShape = _voronoiDiagram._voronoiEdgeShapes[currentVoronoiEdge];
					tPrev = currentEdgeShape.t0;
				}
				else
				{
					tPrev = tNext;
				}

				if (moveToNextContour)
				{
					// Move to next contour.
					++innerContourIndex;
					++outerContourIndex;
				}
				else if (moveToPrevContour)
				{
					// Move to previous contour.
					--innerContourIndex;
					--outerContourIndex;
				}
			}

			_contours[0].vertices.Clear();

			return true;
		}

		private void CreateTriangles(int outerContourIndex)
		{
			int innerContourIndex = outerContourIndex - 1;
			var innerVertices = _contours[innerContourIndex].vertices;
			var outerVertices = _contours[outerContourIndex].vertices;

			int innerVertexIndex = 1;
			int outerVertexIndex = 1;

			while (innerVertexIndex < innerVertices.Count && outerVertexIndex < outerVertices.Count)
			{
				var innerHypotenuse = innerVertices[innerVertexIndex].position - outerVertices[outerVertexIndex - 1].position;
				var outerHypotenuse = outerVertices[outerVertexIndex].position - innerVertices[innerVertexIndex - 1].position;

				if (innerHypotenuse.sqrMagnitude < outerHypotenuse.sqrMagnitude)
				{
					_onTriangle(
						innerVertices[innerVertexIndex - 1].positionId,
						outerVertices[outerVertexIndex - 1].positionId,
						innerVertices[innerVertexIndex].positionId);
					++innerVertexIndex;
				}
				else
				{
					_onTriangle(
						innerVertices[innerVertexIndex - 1].positionId,
						outerVertices[outerVertexIndex - 1].positionId,
						outerVertices[outerVertexIndex].positionId);
					++outerVertexIndex;
				}
			}

			while (innerVertexIndex < innerVertices.Count)
			{
				_onTriangle(
					innerVertices[innerVertexIndex - 1].positionId,
					outerVertices[outerVertexIndex - 1].positionId,
					innerVertices[innerVertexIndex].positionId);
				++innerVertexIndex;
			}

			while (outerVertexIndex < outerVertices.Count)
			{
				_onTriangle(
					innerVertices[innerVertexIndex - 1].positionId,
					outerVertices[outerVertexIndex - 1].positionId,
					outerVertices[outerVertexIndex].positionId);
				++outerVertexIndex;
			}

			outerVertices.Clear();
		}
	}
}
