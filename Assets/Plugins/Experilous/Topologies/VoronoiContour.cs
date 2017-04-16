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

			public PositionId(TopologyFace face, int segment)
			{
				_value = FACE | ((ulong)face.index & INDEX_MASK) | (((ulong)segment << SEGMENT_SHIFT) & SEGMENT_MASK);
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
			public float distance;
			public List<Vertex> vertices;
		}

		public delegate void OnVertexDelegate(PositionId positionId, Vector3 position, VoronoiSiteType siteType, int siteIndex, int contourIndex, float distance);
		public delegate void OnTriangleDelegate(PositionId positionId0, PositionId positionId1, PositionId positionId2);

		private VoronoiDiagram _voronoiDiagram;
		private OnVertexDelegate _onVertex;
		private OnTriangleDelegate _onTriangle;

		private float _maxCurvaturePerSegment;
		private float _errorMargin;

		private TopologyFace _initialFace;
		private TopologyFace _nextFace;

		private Contour[] _contours;

		public ContourTriangulator(float maxCurvaturePerSegment = 0.25f, float errorMargin = 0.0001f)
		{
			_maxCurvaturePerSegment = maxCurvaturePerSegment;
			_errorMargin = errorMargin;
		}

		public void Triangulate(VoronoiDiagram voronoiDiagram, TopologyFace initialFace, OnVertexDelegate onVertex, OnTriangleDelegate onTriangle, params float[] contourDistances)
		{
			BeginTriangulation(voronoiDiagram, initialFace, onVertex, onTriangle, contourDistances);
			while (TriangulateSegment()) { }
		}

		public void BeginTriangulation(VoronoiDiagram voronoiDiagram, TopologyFace initialFace, OnVertexDelegate onVertex, OnTriangleDelegate onTriangle, params float[] contourDistances)
		{
			_voronoiDiagram = voronoiDiagram;
			_onVertex = onVertex;
			_onTriangle = onTriangle;

			_initialFace = TopologyFace.none;
			_nextFace = initialFace;

			if (_contours == null || _contours.Length < contourDistances.Length)
			{
				_contours = new Contour[contourDistances.Length];
			}

			for (int i = 0; i < contourDistances.Length; ++i)
			{
				_contours[i].index = i;
				_contours[i].distance = contourDistances[i];
				_contours[i].vertices.Clear();
			}

			Array.Sort(_contours, (Contour lhs, Contour rhs) => (int)Numerics.Math.ZeroInclusiveSign(lhs.distance - rhs.distance));
		}

		public bool TriangulateSegment()
		{
			if (_nextFace == _initialFace) return false;

			switch (_voronoiDiagram._voronoiFaceSiteTypes[_nextFace])
			{
				case VoronoiSiteType.Point:
					TriangulatePointSegment();
					break;
				case VoronoiSiteType.Line:
					TriangulateLineSegment();
					break;
				case VoronoiSiteType.None:
					throw new InvalidOperationException();
				default:
					throw new NotImplementedException();
			}

			if (!_initialFace) _initialFace = _nextFace;

			return true;
		}

		private void TriangulatePointSegment()
		{

		}

		private void TriangulateLineSegment()
		{
			int siteIndex = _voronoiDiagram._voronoiFaceSiteIndices[_nextFace];
			var baseVoronoiEdge = new TopologyFaceEdge(_voronoiDiagram._voronoiTopology, _voronoiDiagram._siteEdgeFirstVoronoiEdgeIndices[siteIndex]);
			var currentVoronoiEdge = baseVoronoiEdge.next;
			VoronoiEdgeShape currentEdgeShape = _voronoiDiagram._voronoiEdgeShapes[currentVoronoiEdge];
			int currentEdgeSegmentIndex = 0;
			float tPrev = float.NaN;
			float tNext = 0f;

			int innerContourIndex = -1;
			int outerContourIndex = 0;

			while (currentVoronoiEdge != baseVoronoiEdge)
			{
				// Actions:
				//   1) Subdivide edge if appropriate, add vertex at end of edge, go to next edge.
				//   2) Subdivide edge if appropriate, add vertex at end of edge, go to next edge, go to next contour.
				//   3) Subdivide edge if appropriate, add vertex in middle of edge, go to next contour.
				//   4) Subdivide edge if appropriate, subdivide current contour from its last vertex if appropriate, add vertex at end of edge, go to next edge.
				//   5) Subdivide edge if appropriate, subdivide current contour from its last vertex if appropriate, add vertex at end of edge, go to next edge, go to previous contour.
				//   6) Subdivide edge if appropriate, subdivide current contour from its last vertex if appropriate, add vertex in middle of edge, go to previous contour.

				// Broken up actions:
				//   a) subdivide edge (prompts performing d possibly multiple times)
				//   b) subdivide contour (prompts performing e possibly multiple times)
				//   c) add vertex at end of edge (prompts a check for performing f)
				//   d) add vertex in middle of edge (prompts a check for performing f)
				//   e) add vertex in middle of face (prompts a check for performing f)
				//   f) [conditionally] add triangle
				//   g) go to next edge
				//   h) go to next contour
				//   i) go to previous contour

				// Cases:
				//   If there is a next contour:
				//     Next contour enters after end of current edge:  1; (a(df?)*)(cf?)g
				//     Next contour enters right at end of current edge, but neither distance 1st deriv or 2nd deriv are positive:  1; (a(df?)*)(cf?)g
				//     Next contour enters right at end of current edge, and either distance 1st deriv or 2nd deriv is positive:  2; (a(df?)*)(cf?)gh
				//     Next contour enters before end of current edge:  3; (a(df?)*)(df?)h
				//   Current contour exits after end of current edge:  4; (a(df?)*)(cf?)g
				//   Current contour exits right at end of current edge:  5; (a(df?)*)(b(ef?)?)(cf?)gi
				//   Current contour exits before end of current edge:  6; (a(df?)*)(b(ef?)?)(df?)i

				// To subdivide edge, need to know the previous and current t values on the edge.
				// To subdivide contour, need to know the previous and current t values along the contour.

				// Every action represents a progress along the edges of the current voronoi face, so once the final vertex of the step is determined, always do (a(df?)*).
				// Information that needs to be determined before taking action:
				//   The unclamped position of the next vertex along the edge.
				//   The clamped position of the next vertex along the edge.
				//   If at least one of the distance 1st deriv or 2nd deriv is positive.
				//   If it's a next-contour-enters or a current-contour-exits situation.

				float tNextEntrance = float.NaN;
				float tNextExit = float.NaN;

				bool moveToNextEdge = false;
				bool moveToNextContour = false;
				bool moveToPrevContour = false;

				if (outerContourIndex < _contours.Length)
				{
					tNextEntrance = currentEdgeShape.IntersectEntranceUnclamped(_contours[outerContourIndex].distance);
					if (currentEdgeShape.t1 - tNextEntrance > _errorMargin)
					{
						// Next contour enters after end of current edge; ignore it for now.
						tNextEntrance = float.NaN;
					}
					else if (tNextEntrance - currentEdgeShape.t1 < _errorMargin)
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
					if (currentEdgeShape.t1 - tNextExit > _errorMargin)
					{
						// Current contour exits after end of current edge; ignore it for now.
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
							positionId = new PositionId(currentVoronoiEdge, innerContourIndex);
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

					positionId = new PositionId(currentVoronoiEdge.nextNode);
					position = _voronoiDiagram._voronoiNodePositions[currentVoronoiEdge.nextNode];
					distance = currentEdgeShape.GetDistance(tNextExit);
				}

				if (outerContourIndex < _contours.Length)
				{
					// Subdivide current edge from tPrev to tNext
					var cumulativeCurvature = Mathf.Abs(currentEdgeShape.GetCurvatureSum(tPrev, tNext));
					int segmentCount = Mathf.CeilToInt(cumulativeCurvature / _maxCurvaturePerSegment);

					for (int i = 1; i < segmentCount; ++i)
					{
						float tSegment = currentEdgeShape.GetCurvatureSumOffset(tPrev, (cumulativeCurvature * i) / segmentCount);

						var segmentVertexPositionId = new PositionId(currentVoronoiEdge, outerContourIndex, i, segmentCount, moveToNextContour, moveToPrevContour); //TODO: Needs fixing.
						var segmentVertexPosition = currentEdgeShape.Evaluate(tSegment);
						var segmentVertexDistance = currentEdgeShape.GetDistance(tSegment);
						_contours[outerContourIndex].vertices.Add(new Vertex(segmentVertexPositionId, segmentVertexPosition, segmentVertexDistance));
						_onVertex(segmentVertexPositionId, segmentVertexPosition, VoronoiSiteType.Line, siteIndex, innerContourIndex, segmentVertexDistance);

						tPrev = tSegment;
					}
				}

				if (moveToPrevContour)
				{
					// Subdivide current contour from ??? to ???
					var contourVertices = _contours[innerContourIndex].vertices;
					if (contourVertices.Count > 0)
					{
						Vector3 prevPosition = contourVertices[contourVertices.Count - 1].position;
						Vector3 nextPosition;
						// No subdivision implemented yet for straight line contours.

						_contours[innerContourIndex].vertices.Add(new Vertex(positionId, position, distance));
						_onVertex(positionId, position, VoronoiSiteType.Line, siteIndex, innerContourIndex, distance);
					}
				}
				else if (outerContourIndex < _contours.Length)
				{
					_contours[outerContourIndex].vertices.Add(new Vertex(positionId, position, distance));
					_onVertex(positionId, position, VoronoiSiteType.Line, siteIndex, outerContourIndex, distance);
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
		}
	}

	public class VoronoiTracer
	{
		private VoronoiDiagram _voronoiDiagram;
		private readonly List<Trace> _traces = new List<Trace>();
		private readonly List<Trace> _tracePool = new List<Trace>();
		private TraceData[] _traceData;
		private TopologyFace _currentFace;
		private float _currentFaceProgress;

		public class Trace
		{
			public enum State
			{
				Normal,
				Breaking,
				Broken,
				Resuming,
			}

			private VoronoiTracer _tracer;
			private int _index;

			public Trace(VoronoiTracer tracer, int index)
			{
				_tracer = tracer;
				_index = index;
			}

			public State state { get { return _tracer._traceData[_index].state; } }
			public Vector3 position { get { return _tracer._traceData[_index].currentPosition; } }
			public TracePositionId positionId { get { return _tracer._traceData[_index].currentPositionId; } }
		}

		public struct TracePositionId : IEquatable<TracePositionId>
		{
			private enum PositionType
			{
				Node,
				Edge,
				Face,
			}

			private PositionType _type;
			private int _index;
			private int _segment;

			private TracePositionId(PositionType type, int index, int segment)
			{
				_type = type;
				_index = index;
				_segment = segment;
			}

			public TracePositionId(TopologyNode node, int segment)
			{
				_type = PositionType.Node;
				_index = node.index;
				_segment = segment;
			}

			public TracePositionId(TopologyEdge edge, int segment)
			{
				_type = PositionType.Edge;
				_index = edge.index; //TODO: handle twin, always take lower, negate/shift segment
				_segment = segment;
			}

			public TracePositionId(TopologyFace face, int segment)
			{
				_type = PositionType.Face;
				_index = face.index;
				_segment = segment;
			}

			public TracePositionId FromNode(int index, int segment)
			{
				return new TracePositionId(PositionType.Node, index, segment);
			}

			public TracePositionId FromEdge(int index, int segment)
			{
				return new TracePositionId(PositionType.Edge, index, segment);
			}

			public TracePositionId FromFace(int index, int segment)
			{
				return new TracePositionId(PositionType.Face, index, segment);
			}

			public bool Equals(TracePositionId other)
			{
				return _type == other._type && _index == other._index && _segment == other._segment;
			}

			public override bool Equals(object other)
			{
				return other is TracePositionId && Equals((TracePositionId)other);
			}

			public override int GetHashCode()
			{
				return _type.GetHashCode() ^ _index.GetHashCode() ^ _segment.GetHashCode();
			}

			public override string ToString()
			{
				return string.Format("{0} {1}.{2}", _type, _index, _segment);
			}
		}

		private struct TraceData
		{
			public Trace.State state;
			public float distance;
			public Vector3 currentPosition;
			public TracePositionId currentPositionId;
		}

		public void BeginTracing(VoronoiDiagram voronoiDiagram, TopologyFace startingFace, params float[] distances)
		{
			_voronoiDiagram = voronoiDiagram;
			_traces.Clear();
			while (_tracePool.Count < distances.Length)
			{
				_tracePool.Add(new Trace(this, _tracePool.Count));
			}

			if (_traceData == null || _traceData.Length < distances.Length)
			{
				_traceData = new TraceData[distances.Length];
			}

			for (int i = 0; i < distances.Length; ++i)
			{
				_traceData[i].state = Trace.State.Normal;

				_traces.Add(_tracePool[i]);
			}
		}

		private void InitializePrimaryTrace(TopologyFace face, float distance)
		{
			foreach (var edge in face.edges)
			{
				var edgeShape = _voronoiDiagram._voronoiEdgeShapes[edge];
				float t0, t1;
				//if (edgeShape.Intersect(distance, out t0, out t1) && !float.IsNaN(t0))
			}
		}

		public Trace primaryTrace { get { return _traces[0]; } }

		public Trace GetTrace(int index)
		{
			return _traces[index];
		}

		public bool ContinueTrace()
		{
			throw new NotImplementedException();
		}
	}
	
	/*
	public struct VoronoiContour
	{
		public enum State
		{
			Normal,
			Breaking,
			Broken,
			Resuming,
		}

		public VoronoiDiagram voronoiDiagram;
		public float contourDistance;
		public float referenceDistance;

		public VoronoiSiteType siteType;
		public int siteIndex;
		public State state;

		public bool Reset(VoronoiDiagram voronoiDiagram, GraphNode startingNode, float contourDistance, float referenceDistance = 0f)
		{
			this.voronoiDiagram = voronoiDiagram;
			this.contourDistance = contourDistance;
			this.referenceDistance = referenceDistance;
			siteType = VoronoiSiteType.Point;
			siteIndex = startingNode.index;
		}

		public bool Reset(VoronoiDiagram voronoiDiagram, GraphEdge startingEdge, float contourDistance, float referenceDistance = 0f)
		{
			this.voronoiDiagram = voronoiDiagram;
			this.contourDistance = contourDistance;
			this.referenceDistance = referenceDistance;
			siteType = VoronoiSiteType.Line;
			siteIndex = startingEdge.index;

			return MoveToStartOfLineSite();
		}

		private bool MoveToStartOfLineSite()
		{
			var edge = new TopologyEdge(voronoiDiagram._voronoiTopology, voronoiDiagram._siteEdgeFirstVoronoiEdgeIndices[siteIndex]);

			var prevSiteType = voronoiDiagram._voronoiFaceSiteTypes[edge];
			int prevSiteIndex = voronoiDiagram._voronoiFaceSiteIndices[edge];

			if (prevSiteType == VoronoiSiteType.Line)
			{
			}
			else if (prevSiteType == VoronoiSiteType.Point)
			{
				if (voronoiDiagram._siteGraph.GetEdgeTargetNodeIndex(siteIndex ^ 1) == prevSiteIndex)
				{
				}
				else
				{
				}
			}
		}
	}
	*/
}
