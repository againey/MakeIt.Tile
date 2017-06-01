/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Experilous.Topologies
{
	[Serializable]
	public struct ContourDescriptor
	{
		public float distance;
		public List<int> entranceEdgeIndices;
		public List<float> entranceParameters;
		public bool loop;

		public ContourDescriptor(float distance, int entranceEdgeIndex, float entranceParameter)
		{
			this.distance = distance;
			entranceEdgeIndices = new List<int>();
			entranceParameters = new List<float>();
			entranceEdgeIndices.Add(entranceEdgeIndex);
			entranceParameters.Add(entranceParameter);
			loop = false;
		}

		public ContourDescriptor(float distance, List<int> entranceEdgeIndices, List<float> entranceParameters, bool loop)
		{
			this.distance = distance;
			this.entranceEdgeIndices = entranceEdgeIndices;
			this.entranceParameters = entranceParameters;
			this.loop = loop;
		}

		public void Clear()
		{
			entranceEdgeIndices.Clear();
			entranceParameters.Clear();
		}
	}

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

		private struct ContourLayer
		{
			public int index;
			public int nextSegmentIndex;
			public float distance;
			public List<Vertex> vertices;

			public class Comparer : IComparer<ContourLayer>
			{
				public int Compare(ContourLayer lhs, ContourLayer rhs)
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

		private float _maxAngleChangePerSegment;
		private float _errorMargin;

		private Vector3 _normal;

		private ContourLayer[] _contourLayers = new ContourLayer[0];
		private int _contourLayerCount;
		private readonly ContourLayer.Comparer _contourComparer = new ContourLayer.Comparer();

		public ContourTriangulator(float maxAngleChangePerSegment = 0.25f, float errorMargin = 0.0001f)
		{
			_maxAngleChangePerSegment = maxAngleChangePerSegment;
			_errorMargin = errorMargin;
		}

		public float maxAngleChangePerSegment
		{
			get
			{
				return _maxAngleChangePerSegment;
			}
			set
			{
				if (value <= 0f) throw new ArgumentOutOfRangeException("value", "The maximum curvature per segment must be greater than zero.");
				_maxAngleChangePerSegment = value;
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

		private ContourDescriptor _baseContour;
		private int _baseContourCurrentIndex;

		public void Triangulate(VoronoiDiagram voronoiDiagram, OnVertexDelegate onVertex, OnTriangleDelegate onTriangle, Vector3 normal, ContourDescriptor baseContour, params float[] contourDistances)
		{
			BeginTriangulation(voronoiDiagram, onVertex, onTriangle, normal, baseContour, contourDistances);
			while (TriangulateSegment()) { }
		}

		public void BeginTriangulation(VoronoiDiagram voronoiDiagram, OnVertexDelegate onVertex, OnTriangleDelegate onTriangle, Vector3 normal, ContourDescriptor baseContour, params float[] contourDistances)
		{
			_voronoiDiagram = voronoiDiagram;
			_onVertex = onVertex;
			_onTriangle = onTriangle;
			_normal = normal;
			_baseContour = baseContour;
			_baseContourCurrentIndex = 0;

			_contourLayerCount = contourDistances.Length;

			if (_contourLayers.Length < _contourLayerCount)
			{
				var newContourLayers = new ContourLayer[_contourLayerCount];

				Array.Copy(_contourLayers, newContourLayers, _contourLayers.Length);
				for (int i = _contourLayers.Length; i < _contourLayerCount; ++i)
				{
					newContourLayers[i].vertices = new List<Vertex>();
				}

				_contourLayers = newContourLayers;
			}

			for (int i = 0; i < _contourLayerCount; ++i)
			{
				_contourLayers[i].index = i;
				_contourLayers[i].vertices.Clear();
				_contourLayers[i].distance = contourDistances[i];
			}

			Array.Sort(_contourLayers, 0, _contourLayerCount, _contourComparer);
		}

		public bool TriangulateSegment()
		{
			TopologyFaceEdge currentVoronoiEdge = new TopologyFaceEdge(_voronoiDiagram._voronoiTopology, _baseContour.entranceEdgeIndices[_baseContourCurrentIndex]);
			VoronoiEdgeShape currentEdgeShape = _voronoiDiagram._voronoiEdgeShapes[currentVoronoiEdge];
			float tPrev = _baseContour.entranceParameters[_baseContourCurrentIndex];
			float tNext;

			TopologyFace currentFace = currentVoronoiEdge.sourceFace;
			int siteIndex = _voronoiDiagram._voronoiFaceSiteIndices[currentFace];
			var siteType = _voronoiDiagram._voronoiFaceSiteTypes[currentFace];

			int innerContourIndex = 0;
			int outerContourIndex = 1;

			for (int i = 0; i < _contourLayerCount; ++i)
			{
				_contourLayers[i].nextSegmentIndex = 0;
			}

			// Initial vertex at the entrance of the base contour.
			{
				PositionId positionId;
				Vector3 position;

				if (Numerics.Math.ApproximateLessOrEqual(tPrev, currentEdgeShape.t0, _errorMargin))
				{
					// Next contour enters before or right at start of current edge.
					positionId = new PositionId(currentVoronoiEdge.prevNode);
					position = _voronoiDiagram._voronoiNodePositions[currentVoronoiEdge.prevNode];
				}
				else
				{
					// Next contour enters in middle of current edge.
					positionId = new PositionId(currentVoronoiEdge, innerContourIndex);
					position = currentEdgeShape.Evaluate(tPrev);
				}

				_contourLayers[innerContourIndex].vertices.Add(new Vertex(positionId, position, _contourLayers[innerContourIndex].distance));
				_onVertex(positionId, position, siteType, siteIndex, innerContourIndex, _contourLayers[innerContourIndex].distance);
			}

			while (innerContourIndex >= 0)
			{
				float tNextEntrance = float.NaN;
				float tNextExit = float.NaN;

				bool moveToNextEdge = false;
				bool moveToNextContour = false;
				bool moveToPrevContour = false;

				if (outerContourIndex < _contourLayerCount)
				{
					tNextEntrance = currentEdgeShape.IntersectEntranceUnclamped(_contourLayers[outerContourIndex].distance);
					if (Numerics.Math.ApproximateGreaterThan(tNextEntrance, currentEdgeShape.t1, _errorMargin))
					{
						// Next contour enters after end of current edge; ignore it for now.
						tNextEntrance = float.NaN;
					}
					else if (Numerics.Math.ApproximateGreaterOrEqual(tNextEntrance, currentEdgeShape.t1, _errorMargin))
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
					tNextExit = currentEdgeShape.IntersectExitUnclamped(_contourLayers[innerContourIndex].distance);
					if (Numerics.Math.ApproximateGreaterThan(tNextExit, currentEdgeShape.t1, _errorMargin))
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
					distance = _contourLayers[outerContourIndex].distance;

					if (Numerics.Math.ApproximateLessThan(tNextEntrance, currentEdgeShape.t1, _errorMargin))
					{
						// Next contour enters before end of current edge.
						tNext = tNextEntrance;

						moveToNextEdge = false;

						if (Numerics.Math.ApproximateLessOrEqual(tNextEntrance, currentEdgeShape.t0, _errorMargin))
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
					distance = _contourLayers[innerContourIndex].distance;

					if (Numerics.Math.ApproximateLessThan(tNextExit, currentEdgeShape.t1, _errorMargin))
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

					if (innerContourIndex >= 0 && outerContourIndex < _contourLayerCount)
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

				if ((moveToNextContour || innerContourIndex >= 0) && outerContourIndex < _contourLayerCount)
				{
					// Subdivide current edge from tPrev to tNext
					float angleChange = Mathf.Abs(currentEdgeShape.GetAngleChange(tPrev, tNext));
					int segmentCount = Mathf.CeilToInt(angleChange / _maxAngleChangePerSegment);

					float tFirst = tPrev;
					for (int i = 1; i < segmentCount; ++i)
					{
						float tSegment = currentEdgeShape.GetAngleChangeOffset(tFirst, (angleChange * i) / segmentCount);

						var segmentVertexPositionId = new PositionId(currentVoronoiEdge, outerContourIndex, i, segmentCount, moveToNextContour, moveToPrevContour);
						var segmentVertexPosition = currentEdgeShape.Evaluate(tSegment);
						var segmentVertexDistance = currentEdgeShape.GetDistance(tSegment);
						_contourLayers[outerContourIndex].vertices.Add(new Vertex(segmentVertexPositionId, segmentVertexPosition, segmentVertexDistance));
						_onVertex(segmentVertexPositionId, segmentVertexPosition, siteType, siteIndex, outerContourIndex, segmentVertexDistance);

						tPrev = tSegment;
					}
				}

				if (moveToPrevContour)
				{
					// Subdivide current contour from its last position to the new position.
					var contourVertices = _contourLayers[innerContourIndex].vertices;
					int firstContourVertexIndex = contourVertices.Count - 1;
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
									float angleChange = Mathf.Repeat(Mathf.Atan2(angleSine, angleCosine), Mathf.PI * 2f);
									int segmentCount = Mathf.CeilToInt(angleChange / _maxAngleChangePerSegment);

									if (segmentCount > 1)
									{
										int nextSegmentIndex = _contourLayers[innerContourIndex].nextSegmentIndex;
										var rightVector = Vector3.Cross(_normal, prevVector);
										for (int i = 1; i < segmentCount; ++i)
										{
											float tSegment = (angleChange * i) / segmentCount;

											//TODO: Is this insufficient when a contour exits and later re-enters the same face?
											var segmentVertexPositionId = new PositionId(currentFace, innerContourIndex, nextSegmentIndex++); //TODO: When distance is 0, make this position id be assigned to the edge rather than face.
											var segmentVertexPosition = pivotPosition + prevVector * Mathf.Cos(tSegment) + rightVector * Mathf.Sin(tSegment);
											_contourLayers[innerContourIndex].vertices.Add(new Vertex(segmentVertexPositionId, segmentVertexPosition, distance));
											_onVertex(segmentVertexPositionId, segmentVertexPosition, siteType, siteIndex, innerContourIndex, distance);

											tPrev = tSegment;
										}
										_contourLayers[innerContourIndex].nextSegmentIndex = nextSegmentIndex;
									}
								}
								break;
							case VoronoiSiteType.Line:
								// No straight line subdivision supported at this time; do nothing.
								break;
						}
					}

					_contourLayers[innerContourIndex].vertices.Add(new Vertex(positionId, position, distance));
					_onVertex(positionId, position, siteType, siteIndex, innerContourIndex, distance);

					if (outerContourIndex < _contourLayerCount)
					{
						CreateTriangles(outerContourIndex, firstContourVertexIndex);
					}
				}
				else if ((moveToNextContour || innerContourIndex >= 0) && outerContourIndex < _contourLayerCount)
				{
					_contourLayers[outerContourIndex].vertices.Add(new Vertex(positionId, position, distance));
					_onVertex(positionId, position, siteType, siteIndex, outerContourIndex, distance);
				}

				if (moveToNextEdge)
				{
					currentVoronoiEdge = currentVoronoiEdge.next;

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

			_contourLayers[0].vertices.Clear();

			return ++_baseContourCurrentIndex < _baseContour.entranceEdgeIndices.Count;
		}

		private void CreateTriangles(int outerContourIndex, int firstInnerContourVertexIndex)
		{
			int innerContourIndex = outerContourIndex - 1;
			var innerVertices = _contourLayers[innerContourIndex].vertices;
			var outerVertices = _contourLayers[outerContourIndex].vertices;

			int innerVertexIndex = firstInnerContourVertexIndex + 1;
			int outerVertexIndex = 1;

			while (innerVertexIndex < innerVertices.Count && outerVertexIndex < outerVertices.Count)
			{
				var innerBase = innerVertices[innerVertexIndex].position - innerVertices[innerVertexIndex - 1].position;
				var outerBase = outerVertices[outerVertexIndex].position - outerVertices[outerVertexIndex - 1].position;

				var innerHypotenuse = innerVertices[innerVertexIndex].position - outerVertices[outerVertexIndex - 1].position;
				var outerHypotenuse = outerVertices[outerVertexIndex].position - innerVertices[innerVertexIndex - 1].position;

				float innerSine = Vector3.Cross(innerBase.normalized, innerHypotenuse.normalized).magnitude;
				float outerSine = Vector3.Cross(outerBase.normalized, outerHypotenuse.normalized).magnitude;

				if (innerSine > outerSine)
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
