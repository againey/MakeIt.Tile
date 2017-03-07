/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Experilous.Topologies
{
	public enum VoronoiSiteType
	{
		None = 0,
		Point,
		Line,
	}

	public class PlanarVoronoiGenerator
	{
		private class SplitEvent : IEquatable<SplitEvent>
		{
			public Vector2 position;
			public int nodeIndex;

			public SplitEvent(float x, float y, int nodeIndex)
			{
				position.x = x;
				position.y = y;
				this.nodeIndex = nodeIndex;
			}

			public bool Equals(SplitEvent other)
			{
				return nodeIndex == other.nodeIndex;
			}
		}

		private class MergeEvent : IEquatable<MergeEvent>
		{
			public Vector2 position;
			public Vector2 mergePosition;
			public BeachSegment segment;

			public MergeEvent(float x, float y, Vector2 mergePosition, BeachSegment segment)
			{
				position.x = x;
				position.y = y;
				this.mergePosition = mergePosition;
				this.segment = segment;
			}

			public bool Equals(MergeEvent other)
			{
				return ReferenceEquals(segment, other.segment);
			}

			public bool isValid
			{
				get
				{
					return ReferenceEquals(this, segment.mergeEvent);
				}
			}
		}

		private class BeachSegment
		{
			public BeachSegment prevSegment;
			public BeachSegment nextSegment;
			public int prevEdgeIndex;
			public int nextEdgeIndex;
			public VoronoiSiteType siteType;
			public int siteIndex;
			public MergeEvent mergeEvent;

			public BeachSegment(BeachSegment prevSegment, BeachSegment nextSegment, int prevEdgeIndex, int nextEdgeIndex)
			{
				this.prevSegment = prevSegment;
				this.nextSegment = nextSegment;
				this.prevEdgeIndex = prevEdgeIndex;
				this.nextEdgeIndex = nextEdgeIndex;
				siteType = VoronoiSiteType.None;
				siteIndex = -1;
				mergeEvent = null;
			}

			public static bool SitesAreEqual(BeachSegment lhs, BeachSegment rhs)
			{
				return lhs.siteType == rhs.siteType && lhs.siteIndex == rhs.siteIndex;
			}

			public bool SiteEquals(GraphNode pointSite)
			{
				return siteType == VoronoiSiteType.Point && siteIndex == pointSite.index;
			}

			public bool SiteEquals(GraphEdge edgeSite)
			{
				return siteType == VoronoiSiteType.Line && siteIndex == edgeSite.index;
			}
		}

		private class DirectedEdge
		{
			public Vector2 direction;
			public float order;
			public int edgeIndex;

			public DirectedEdge(Vector2 direction, int edgeIndex)
			{
				this.direction = direction;
				order = ComputeOrder(direction);
				this.edgeIndex = edgeIndex;
			}

			public static float ComputeOrder(Vector2 direction)
			{
				var sum = direction.x + direction.y;
				var sub = direction.x - direction.y;
				var mul = sum * sub;
				if (direction.x <= 0f)
				{
					if (direction.y <= 0f)
					{
						return -3f + mul; // bottom-left quadrant, -4 to -2
					}
					else
					{
						return -1f - mul; // top-left quadrant, -2 to 0
					}
				}
				else
				{
					if (direction.y >= 0f)
					{
						return 1f + mul; // top-right quadrant, 0 to +2
					}
					else
					{
						return 3f - mul; // bottom-right quadrant, +2 to +4
					}
				}
			}
		}

		private static bool CompareEventPositions(Vector2 lhs, Vector2 rhs, float errorMargin)
		{
			float xDelta = rhs.x - lhs.x;
			return xDelta >= errorMargin || xDelta > -errorMargin && rhs.y - lhs.y > xDelta;
		}

		private class SplitEventPriorityQueue : Containers.PriorityQueue<SplitEvent>
		{
			private float _errorMargin = 0.0001f;

			public SplitEventPriorityQueue(float errorMargin)
			{
				_errorMargin = errorMargin;
			}

			protected override bool AreOrdered(SplitEvent lhs, SplitEvent rhs)
			{
				return CompareEventPositions(lhs.position, rhs.position, _errorMargin);
			}
		}

		private class MergeEventPriorityQueue : Containers.PriorityQueue<MergeEvent>
		{
			private float _errorMargin = 0.0001f;

			public MergeEventPriorityQueue(float errorMargin)
			{
				_errorMargin = errorMargin;
			}

			protected override bool AreOrdered(MergeEvent lhs, MergeEvent rhs)
			{
				return CompareEventPositions(lhs.position, rhs.position, _errorMargin);
			}
		}

		private abstract class EdgeGeometry
		{
			public float tStart;

			protected EdgeGeometry(float tStart)
			{
				this.tStart = tStart;
			}

			public abstract Vector2 GetPosition(float t);
			public abstract float GetDistance(float t);
			public abstract bool Intersect(EdgeGeometry other, out float tThis, out float tOther);
			public abstract bool Intersect(StraightEdgeGeometry other, out float tThis, out float tOther);
			public abstract bool Intersect(ParabolicEdgeGeometry other, out float tThis, out float tOther);
		}

		private abstract class StraightEdgeGeometry : EdgeGeometry
		{
			public Vector2 p;
			public Vector2 v;

			protected StraightEdgeGeometry(Vector2 p, Vector2 v, float tStart = 0f)
				: base(tStart)
			{
				this.p = p;
				this.v = v;
			}

			public override Vector2 GetPosition(float t)
			{
				return v * t + p;
			}

			public override bool Intersect(EdgeGeometry other, out float tThis, out float tOther)
			{
				return other.Intersect(this, out tOther, out tThis);
			}

			public override bool Intersect(StraightEdgeGeometry other, out float tThis, out float tOther)
			{
				float denom = Numerics.Geometry.DotPerpendicularCCW(v, other.v);
				if (denom == 0f)
				{
					tThis = tOther = float.NaN;
					return false;
				}
				
				var delta = other.p - p;
				tThis = Numerics.Geometry.DotPerpendicularCCW(delta, other.v) / denom;
				tOther = Numerics.Geometry.DotPerpendicularCCW(delta, v) / denom;
				return true;
			}

			public override bool Intersect(ParabolicEdgeGeometry other,out float tThis, out float tOther)
			{
				return other.Intersect(this, out tOther, out tThis);
			}
		}

		private class ScaledStraightEdgeGeometry : StraightEdgeGeometry
		{
			public ScaledStraightEdgeGeometry(Vector2 p, Vector2 v, float tStart = 0f)
				: base(p, v, tStart)
			{
			}

			public override float GetDistance(float t)
			{
				return t;
			}
		}

		private class OffsetStraightEdgeGeometry : StraightEdgeGeometry
		{
			public float offsetSqr;

			public OffsetStraightEdgeGeometry(Vector2 p, Vector2 v, float offsetSqr, float tStart = 0f)
				: base(p, v, tStart)
			{
				this.offsetSqr = offsetSqr;
			}

			public override float GetDistance(float t)
			{
				return Mathf.Sqrt(t * t + offsetSqr);
			}
		}

		private class ParabolicEdgeGeometry : EdgeGeometry
		{
			public Vector2 p;
			public Vector2 v;
			public float s;
			public float d;

			public ParabolicEdgeGeometry(Vector2 p, Vector2 u, Vector2 v, float tStart = 0f)
				: base(tStart)
			{
				this.p = p;
				this.u = u;
				this.v = v;
			}

			public override Vector2 GetPosition(float t)
			{
				return (Mathf.Abs(s) * v * t + s * Numerics.Geometry.PerpendicularCW(v)) * t + p;
			}

			public override float GetDistance(float t)
			{
				return Mathf.Abs(s) * (t + 1f) * t + d;
			}

			public override bool Intersect(EdgeGeometry other, out float tThis, out float tOther)
			{
				return other.Intersect(this, out tOther, out tThis);
			}

			public override bool Intersect(StraightEdgeGeometry other, out float tThis, out float tOther)
			{
				var delta = other.p - p;
				var uSqrMagnitude = u.sqrMagnitude;
				float a = Vector2.Dot(delta, u) / uSqrMagnitude;
				float b = Vector2.Dot(delta, v) / uSqrMagnitude;
				float c = Vector2.Dot(other.v, u) / uSqrMagnitude;
				float d = Vector2.Dot(other.v, v) / uSqrMagnitude;

				float denom = Numerics.Geometry.DotPerpendicularCCW(v, other.v);
				if (denom == 0f)
				{
					tThis = tOther = float.NaN;
					return false;
				}
				
				var delta = other.p - p;
				tThis = Numerics.Geometry.DotPerpendicularCCW(delta, other.v) / denom;
				tOther = Numerics.Geometry.DotPerpendicularCCW(delta, v) / denom;
				return true;
			}

			public override bool Intersect(ParabolicEdgeGeometry other,out float tThis, out float tOther)
			{
				return other.Intersect(this, out tOther, out tThis);
			}
		}

		private IGraph _siteGraph;
		private IGraphNodeData<Vector3> _pointSitePositions;
		//TODO: private IGraphEdgeData<???> _edgeSiteArcData;

		private SplitEventPriorityQueue _splitEventQueue;
		private MergeEventPriorityQueue _mergeEventQueue;
		private List<SplitEvent> _splitEventPool = new List<SplitEvent>();
		private List<MergeEvent> _mergeEventPool = new List<MergeEvent>();

		private List<BeachSegment> _beachSegmentPool = new List<BeachSegment>();
		private BeachSegment _firstBeachSegment;
		private BeachSegment _lastBeachSegment;

		private List<DirectedEdge> _orderedLineSites = new List<DirectedEdge>();
		private List<DirectedEdge> _directedEdgePool = new List<DirectedEdge>();

		private Vector3 _origin;
		private Vector3 _forward;
		private Vector3 _up;

		private float _sweep;

		private DynamicGraph _voronoiGraph = new DynamicGraph();
		private GraphNodeDataList<Vector2> _voronoiNodePositions = new GraphNodeDataList<Vector2>();
		private GraphEdgeDataList<Vector3> _voronoiEdgeStuff;
		private GraphEdgeDataList<EdgeGeometry> _voronoiEdgeGeometry = new GraphEdgeDataList<EdgeGeometry>();

		private float _errorMargin;

		public PlanarVoronoiGenerator(float errorMargin)
		{
			_errorMargin = errorMargin;

			_splitEventQueue = new SplitEventPriorityQueue(_errorMargin);
			_mergeEventQueue = new MergeEventPriorityQueue(_errorMargin);
		}

		public void SetSites(IGraph siteGraph, IGraphNodeData<Vector3> pointSitePositions, Vector3 origin, Vector3 forward, Vector3 up)
		{
			_siteGraph = siteGraph;
			_pointSitePositions = pointSitePositions;

			_origin = origin;
			_forward = forward.normalized;
			_up = Vector3.Cross(forward, Vector3.Cross(up, forward)).normalized;

			_splitEventQueue.Clear();
			_mergeEventQueue.Clear();

			_firstBeachSegment = _lastBeachSegment = CreateBeachSegment(null, null, -1, -1);

			_sweep = float.NegativeInfinity;

			_voronoiGraph.Clear();
			_voronoiNodePositions.Clear();

			foreach (var pointSite in _siteGraph.nodes)
			{
				var position = _pointSitePositions[pointSite];
				float x = Vector3.Dot(_forward, position);
				float y = Vector3.Dot(_up, position);
				_splitEventQueue.Push(new SplitEvent(x, y, pointSite.index));
			}
		}

		public VoronoiDiagram Generate()
		{
			while (Step()) { }

			var convertedNodePositions = new TopologyNodeDataArray<Vector3>(_voronoiNodePositions.Count);
			for (int i = 0; i < _voronoiNodePositions.Count; ++i)
			{
				var position = _voronoiNodePositions[i];
				convertedNodePositions[i] = position.x * _forward + position.y * _up;
			}

			return new VoronoiDiagram(new FixedSizeTopology(_voronoiGraph), convertedNodePositions);
		}

		private BeachSegment CreateBeachSegment(BeachSegment prevSegment, BeachSegment nextSegment, int prevEdgeIndex, int nextEdgeIndex)
		{
			if (_beachSegmentPool.Count > 0)
			{
				int lastIndex = _beachSegmentPool.Count - 1;
				var segment = _beachSegmentPool[lastIndex];
				_beachSegmentPool.RemoveAt(lastIndex);
				segment.prevSegment = prevSegment;
				segment.nextSegment = nextSegment;
				segment.prevEdgeIndex = prevEdgeIndex;
				segment.nextEdgeIndex = nextEdgeIndex;
				segment.siteType = VoronoiSiteType.None;
				segment.siteIndex = -1;
				segment.mergeEvent = null;
				return segment;
			}
			else
			{
				return new BeachSegment(prevSegment, nextSegment, prevEdgeIndex, nextEdgeIndex);
			}
		}

		private void DestroyBeachSegment(BeachSegment segment)
		{
			_beachSegmentPool.Add(segment);
		}

		private SplitEvent CreateSplitEvent(float x, float y, int nodeIndex)
		{
			if (_splitEventPool.Count > 0)
			{
				int lastIndex = _splitEventPool.Count - 1;
				var ev = _splitEventPool[lastIndex];
				_splitEventPool.RemoveAt(lastIndex);
				ev.position.x = x;
				ev.position.y = y;
				ev.nodeIndex = nodeIndex;
				return ev;
			}
			else
			{
				return new SplitEvent(x, y, nodeIndex);
			}
		}

		private void DestroySplitEvent(SplitEvent ev)
		{
			_splitEventPool.Add(ev);
		}

		private MergeEvent CreateMergeEvent(float x, float y, Vector2 mergePosition, BeachSegment segment)
		{
			if (_mergeEventPool.Count > 0)
			{
				int lastIndex = _mergeEventPool.Count - 1;
				var ev = _mergeEventPool[lastIndex];
				_mergeEventPool.RemoveAt(lastIndex);
				ev.position.x = x;
				ev.position.y = y;
				ev.mergePosition = mergePosition;
				ev.segment = segment;
				return ev;
			}
			else
			{
				return new MergeEvent(x, y, mergePosition, segment);
			}
		}

		private MergeEvent CreateMergeEvent(float x, float y, BeachSegment segment)
		{
			return CreateMergeEvent(x, y, new Vector2(float.NaN, float.NaN), segment);
		}

		private void DestroyMergeEvent(MergeEvent ev)
		{
			_mergeEventPool.Add(ev);
		}

		private DirectedEdge CreateDirectedEdge(Vector2 direction, int edgeIndex)
		{
			if (_directedEdgePool.Count > 0)
			{
				int lastIndex = _directedEdgePool.Count - 1;
				var directedEdge = _directedEdgePool[lastIndex];
				_directedEdgePool.RemoveAt(lastIndex);
				directedEdge.direction = direction;
				directedEdge.order = DirectedEdge.ComputeOrder(direction);
				directedEdge.edgeIndex = edgeIndex;
				return directedEdge;
			}
			else
			{
				return new DirectedEdge(direction, edgeIndex);
			}
		}

		private void DestroyDirectedEdge(DirectedEdge directedEdge)
		{
			_directedEdgePool.Add(directedEdge);
		}

		private void ClearOrderedLineSites()
		{
			_directedEdgePool.AddRange(_orderedLineSites);
			_orderedLineSites.Clear();
		}

		private bool Step()
		{
			if (!_splitEventQueue.isEmpty)
			{
				if (!_mergeEventQueue.isEmpty)
				{
					if (CompareEventPositions(_splitEventQueue.Peek().position, _mergeEventQueue.Peek().position, _errorMargin))
					{
						ProcessEvent(_splitEventQueue.Pop());
					}
					else
					{
						ProcessEvent(_mergeEventQueue.Pop());
					}
				}
				else
				{
					ProcessEvent(_splitEventQueue.Pop());
				}
			}
			else if (!_mergeEventQueue.isEmpty)
			{
				ProcessEvent(_mergeEventQueue.Pop());
			}
			else
			{
				return false;
			}

			return true;
		}

		private void ProcessEvent(SplitEvent ev)
		{
			_sweep = ev.position.x;

			GraphNode voronoiNode = GraphNode.none;
			int incomingLineCount = 0;
			int outgoingLineCount = 0;

			var pointSite = new GraphNode(_siteGraph, ev.nodeIndex);

			if (pointSite.neighborCount > 0)
			{
				voronoiNode = _voronoiGraph.AddNode();
				_voronoiNodePositions.Add(_pointSitePositions[ev.nodeIndex]);

				foreach (var edge in pointSite.edges)
				{
					InsertOrdered(_orderedLineSites, CreateDirectedEdge(GetReframedVector(edge).normalized, edge.index));
				}

				incomingLineCount = _orderedLineSites.Count;
				for (int i = 0; i < _orderedLineSites.Count; ++i)
				{
					if (_orderedLineSites[i].order >= 0f)
					{
						incomingLineCount = i;
						outgoingLineCount = _orderedLineSites.Count - i;
						break;
					}
				}
			}

			var prevSegment = SearchBeach(ev.position);
			var nextSegment = prevSegment;

			bool prevConcavity = false;
			bool nextConcavity = false;

			Vector2 splitPoint = new Vector2(float.NaN, float.NaN);

			if (incomingLineCount == 0)
			{
				// This point site does not have any incoming line segments from point sites already processed.
				// Assert(prevSegment.siteType != VoronoiSiteType.Line || new GraphEdge(_siteGraph, prevSegment.siteIndex).sourceNode != ev.siteIndex);

				splitPoint = ProjectOntoBeachSegment(prevSegment, ev.position);
				nextSegment = SplitBeachSegment(prevSegment);
			}
			else
			{
				// This point site has one or more incoming line segments from point sites already processed.
				// Assert(prevSegment.siteType == VoronoiSiteType.Line && new GraphEdge(_siteGraph, prevSegment.siteIndex).sourceNode == ev.siteIndex);

				splitPoint = Vector2.zero;

				var lastIncomingLine = _orderedLineSites[incomingLineCount - 1];
				while (nextSegment.siteType != VoronoiSiteType.Line || nextSegment.siteIndex != lastIncomingLine.edgeIndex)
				{
					nextSegment = nextSegment.nextSegment;
				}

				nextSegment = nextSegment.nextSegment;

				if (outgoingLineCount > 0)
				{
					var lastOutgoingDirection = _orderedLineSites[_orderedLineSites.Count - 1].direction;
					var firstIncomingDirection = _orderedLineSites[0].direction;
					prevConcavity = Vector2.Dot(lastOutgoingDirection, firstIncomingDirection) < _errorMargin;

					var lastIncomingDirection = _orderedLineSites[incomingLineCount - 1].direction;
					var firstOutgoingDirection = _orderedLineSites[incomingLineCount].direction;
					nextConcavity = Vector2.Dot(lastIncomingDirection, firstOutgoingDirection) < _errorMargin;
				}
			}

			if (outgoingLineCount == 0)
			{
				if (prevConcavity && nextConcavity)
				{
					DivideAdjacentSegments(prevSegment);
					_voronoiGraph.AttachEdgeToSourceNodeBefore(prevSegment.nextEdgeIndex, voronoiNode.firstEdge.index);
				}
				else
				{
					var newSegment = InsertBeachSegmentAfter(pointSite, prevSegment);
					DivideSeparatedSegments(prevSegment, nextSegment, splitPoint);

					if (incomingLineCount > 0)
					{
						_voronoiGraph.AttachEdgeToSourceNodeBefore(prevSegment.nextEdgeIndex, voronoiNode.firstEdge.index);
						_voronoiGraph.AttachEdgeToSourceNodeBefore(newSegment.nextEdgeIndex, prevSegment.nextEdgeIndex);
					}
				}
			}
			else
			{
				var insertAfterSegment = prevSegment;
				int insertBeforeEdgeIndex = voronoiNode.firstEdge.index;

				if (!prevConcavity)
				{
					insertAfterSegment = InsertBeachSegmentAfter(pointSite, insertAfterSegment);
				}

				for (int i = incomingLineCount; i < _orderedLineSites.Count; ++i)
				{
					var outgoingEdge = new GraphEdge(_siteGraph, _orderedLineSites[i].edgeIndex);

					bool wasFirst = ReferenceEquals(insertAfterSegment, prevSegment);
					insertAfterSegment = InsertBeachSegmentAfter(outgoingEdge.twin, insertAfterSegment);
					if (!wasFirst)
					{
						DivideAdjacentSegments(insertAfterSegment);
						_voronoiGraph.AttachEdgeToSourceNodeBefore(insertAfterSegment.nextEdgeIndex, insertBeforeEdgeIndex);
						insertBeforeEdgeIndex = insertAfterSegment.nextEdgeIndex;
					}

					insertAfterSegment = InsertBeachSegmentAfter(outgoingEdge, insertAfterSegment);

					DivideAdjacentSegments(insertAfterSegment);
					_voronoiGraph.AttachEdgeToSourceNodeBefore(insertAfterSegment.nextEdgeIndex, insertBeforeEdgeIndex);
					insertBeforeEdgeIndex = insertAfterSegment.nextEdgeIndex;
				}

				if (!nextConcavity)
				{
					insertAfterSegment = InsertBeachSegmentAfter(pointSite, insertAfterSegment);
					DivideAdjacentSegments(insertAfterSegment);
					_voronoiGraph.AttachEdgeToSourceNodeBefore(insertAfterSegment.nextEdgeIndex, insertBeforeEdgeIndex);
					insertBeforeEdgeIndex = insertAfterSegment.nextEdgeIndex;
				}

				DivideSeparatedSegments(prevSegment, nextSegment, splitPoint);

				if (incomingLineCount > 0)
				{
					_voronoiGraph.AttachEdgeToSourceNodeBefore(prevSegment.nextEdgeIndex, insertBeforeEdgeIndex);
					_voronoiGraph.AttachEdgeToSourceNodeBefore(insertAfterSegment.nextEdgeIndex, insertBeforeEdgeIndex);
				}
			}

			CheckForMergeEvent(prevSegment);
			if (incomingLineCount == 0)
			{
				if (prevSegment.nextSegment.siteType == VoronoiSiteType.Point && prevSegment.nextSegment.siteIndex == pointSite.index)
				{
					CheckForMergeEvent(prevSegment.nextSegment);
				}

				if (nextSegment.prevSegment.siteType == VoronoiSiteType.Point && nextSegment.prevSegment.siteIndex == pointSite.index && !ReferenceEquals(prevSegment.nextSegment, nextSegment.prevSegment))
				{
					CheckForMergeEvent(nextSegment.prevSegment);
				}
			}
			CheckForMergeEvent(nextSegment);

			DestroySplitEvent(ev);
			ClearOrderedLineSites();
		}

		private void ProcessEvent(MergeEvent ev)
		{
			if (ev.isValid)
			{
				_sweep = ev.position.x;

				var prevSegment = ev.segment.prevSegment;
				var nextSegment = ev.segment.nextSegment;

				CollapseBeachSegment(ev.segment, ev.position);

				CheckForMergeEvent(prevSegment);
				CheckForMergeEvent(nextSegment);
			}

			DestroyMergeEvent(ev);
		}

		private BeachSegment SearchBeach(Vector2 position)
		{
			//TODO: Convert to a balanced binary tree search.

			BeachSegment segment = _firstBeachSegment;
			while (!ReferenceEquals(segment, _lastBeachSegment))
			{
				var nextSegment = segment.nextSegment;

				if (segment.siteType == VoronoiSiteType.Line && nextSegment.siteType == VoronoiSiteType.Line && segment.siteIndex == nextSegment.siteIndex)
				{
					return segment;
				}

				float y = GetSegmentUpperBound(segment);

				if (!float.IsNaN(y) && position.y <= y)
				{
					return segment;
				}

				segment = segment.nextSegment;
			}

			return segment;
		}

		private float GetSegmentUpperBound(BeachSegment segment)
		{
			var nextSegment = segment.nextSegment;
			switch (segment.siteType)
			{
				case VoronoiSiteType.Point:
					switch (nextSegment.siteType)
					{
						case VoronoiSiteType.Point:
							return GetSegmentUpperBound_PointPoint(
								GetReframedPosition(segment.siteIndex),
								GetReframedPosition(segment.siteIndex));
						case VoronoiSiteType.Line:
							return GetSegmentUpperBound_PointLine(
								GetReframedPosition(segment.siteIndex),
								GetReframedSourcePosition(nextSegment.siteIndex),
								GetReframedTargetPosition(nextSegment.siteIndex));
						case VoronoiSiteType.None:
						default: throw new NotImplementedException();
					}
				case VoronoiSiteType.Line:
					switch (nextSegment.siteType)
					{
						case VoronoiSiteType.Point:
							return GetSegmentUpperBound_PointLine(
								GetReframedPosition(nextSegment.siteIndex),
								GetReframedSourcePosition(segment.siteIndex),
								GetReframedTargetPosition(segment.siteIndex));
						case VoronoiSiteType.Line:
							return GetSegmentUpperBound_LineLine(
								GetReframedSourcePosition(segment.siteIndex),
								GetReframedTargetPosition(segment.siteIndex),
								GetReframedSourcePosition(nextSegment.siteIndex),
								GetReframedTargetPosition(nextSegment.siteIndex));
						case VoronoiSiteType.None:
						default: throw new NotImplementedException();
					}
				case VoronoiSiteType.None:
					switch (nextSegment.siteType)
					{
						case VoronoiSiteType.Point:
						case VoronoiSiteType.Line:
						case VoronoiSiteType.None:
						default: throw new NotImplementedException();
					}
				default: throw new NotImplementedException();
			}
		}

		private float GetSegmentUpperBound_PointPoint(Vector2 position0, Vector2 position1)
		{
		}

		private float GetSegmentUpperBound_PointLine(Vector2 position0, Vector2 position1a, Vector2 position1b)
		{
		}

		private float GetSegmentUpperBound_LineLine(Vector2 position0a, Vector2 position0b, Vector2 position1a, Vector2 position1b)
		{
		}

		private Vector2 ProjectOntoBeachSegment(BeachSegment segment, Vector2 position)
		{
			switch (segment.siteType)
			{
				case VoronoiSiteType.Point:
				case VoronoiSiteType.Line:
				case VoronoiSiteType.None:
				default: throw new NotImplementedException();
			}
		}

		private void DivideAdjacentSegments(BeachSegment prevSegment)
		{
			DivideAdjacentSegments(prevSegment, new Vector2(float.NaN, float.NaN));
		}

		private void DivideAdjacentSegments(BeachSegment prevSegment, Vector2 position)
		{
		}

		private void DivideSeparatedSegments(BeachSegment prevSegment, BeachSegment nextSegment, Vector2 position)
		{
			if (BeachSegment.SitesAreEqual(prevSegment, nextSegment))
			{
				DivideAdjacentSegments(prevSegment, position);
				nextSegment.prevEdgeIndex = prevSegment.nextEdgeIndex;
				nextSegment.prevSegment.nextEdgeIndex = nextSegment.prevEdgeIndex ^ 1;
			}
			else
			{
				DivideAdjacentSegments(prevSegment);
				DivideAdjacentSegments(nextSegment.prevSegment);
			}
		}

		private BeachSegment SplitBeachSegment(BeachSegment segment)
		{
			var nextSegment = CreateBeachSegment(segment, segment.nextSegment, -1, segment.nextEdgeIndex);

			nextSegment.siteType = segment.siteType;
			nextSegment.siteIndex = segment.siteIndex;

			segment.nextSegment = nextSegment;
			segment.nextEdgeIndex = -1;

			if (ReferenceEquals(_lastBeachSegment, segment))
			{
				_lastBeachSegment = nextSegment;
			}
			else
			{
				nextSegment.nextSegment.prevSegment = nextSegment;
			}

			return nextSegment;
		}

		private BeachSegment InsertBeachSegmentAfter(VoronoiSiteType siteType, int siteIndex, BeachSegment segment)
		{
			var nextSegment = CreateBeachSegment(segment, segment.nextSegment, -1, -1);

			nextSegment.siteType = siteType;
			nextSegment.siteIndex = siteIndex;

			segment.nextSegment = nextSegment;

			if (ReferenceEquals(_lastBeachSegment, segment))
			{
				_lastBeachSegment = nextSegment;
			}
			else
			{
				nextSegment.nextSegment.prevSegment = nextSegment;
			}

			return nextSegment;
		}

		private BeachSegment InsertBeachSegmentAfter(GraphNode pointSite, BeachSegment segment)
		{
			return InsertBeachSegmentAfter(VoronoiSiteType.Point, pointSite.index, segment);
		}

		private BeachSegment InsertBeachSegmentAfter(GraphEdge edgeSite, BeachSegment segment)
		{
			return InsertBeachSegmentAfter(VoronoiSiteType.Line, edgeSite.index, segment);
		}

		private void CheckForMergeEvent(BeachSegment segment)
		{
			if (segment == null) return;

			segment.mergeEvent = null; // Implicitly invalidates any existing merge event for this segment.

			if (segment.prevSegment == null || segment.nextSegment == null) return;

			if (segment.siteType == VoronoiSiteType.None)
			{
				segment.mergeEvent = CreateMergeEvent(_sweep, float.NegativeInfinity, new Vector2(float.PositiveInfinity, float.PositiveInfinity), segment);
				_mergeEventQueue.Push(segment.mergeEvent);
				return;
			}

			if (ReferenceEquals(segment.prevSegment, _firstBeachSegment))
			{
				segment.mergeEvent = CreateMergeEvent(float.PositiveInfinity, float.NegativeInfinity, segment);
				_mergeEventQueue.Push(segment.mergeEvent);
				return;
			}

			if (ReferenceEquals(segment.prevSegment, segment.nextSegment)) return;
			if (segment.prevEdgeIndex == -1 || segment.nextEdgeIndex == -1) return;

			var prevEdgeGeometry = _voronoiEdgeGeometry[segment.prevEdgeIndex];
			var nextEdgeGeometry = _voronoiEdgeGeometry[segment.nextEdgeIndex];

			if (prevEdgeGeometry != null && nextEdgeGeometry != null)
			{
				float tPrev, tNext;
				if (prevEdgeGeometry.Intersect(nextEdgeGeometry, out tPrev, out tNext))
				{
					if (tPrev - prevEdgeGeometry.tStart > _errorMargin && tNext - nextEdgeGeometry.tStart < -_errorMargin)
					{
						Vector2 position = prevEdgeGeometry.GetPosition(tPrev);
						float distance = prevEdgeGeometry.GetDistance(tPrev);
						float mergeSweep = position.x + distance;
						if (mergeSweep - _sweep > -_errorMargin)
						{
							segment.mergeEvent = CreateMergeEvent(position.x, position.y, position, segment);
							_mergeEventQueue.Push(segment.mergeEvent);
						}
					}
				}
			}
			else if (prevEdgeGeometry is StraightEdgeGeometry)
			{
				var straightEdgeGeometry = (StraightEdgeGeometry)prevEdgeGeometry;
				if (Vector2.Dot(straightEdgeGeometry.v.normalized, _forward) > 1f - _errorMargin)
				{
					segment.mergeEvent = CreateMergeEvent(_sweep, straightEdgeGeometry.p.y, segment);
					_mergeEventQueue.Push(segment.mergeEvent);
				}
			}
			else if (nextEdgeGeometry is StraightEdgeGeometry)
			{
				var straightEdgeGeometry = (StraightEdgeGeometry)nextEdgeGeometry;
				if (Vector2.Dot(straightEdgeGeometry.v.normalized, _forward) > 1f - _errorMargin)
				{
					segment.mergeEvent = CreateMergeEvent(_sweep, straightEdgeGeometry.p.y, segment);
					_mergeEventQueue.Push(segment.mergeEvent);
				}
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		private void CollapseBeachSegment(BeachSegment segment, Vector2 position)
		{
			var prevSegment = segment.prevSegment;
			var nextSegment = segment.nextSegment;

			int edgeIndex0 = segment.prevEdgeIndex;
			int edgeIndex1 = segment.nextEdgeIndex ^ 1;
			int edgeIndex2;

			if (ReferenceEquals(prevSegment, _firstBeachSegment) && ReferenceEquals(nextSegment, _lastBeachSegment.prevSegment))
			{
				_firstBeachSegment = _lastBeachSegment = null;
				edgeIndex2 = nextSegment.nextEdgeIndex ^ 1;
			}
			else
			{
				nextSegment.prevSegment = prevSegment;
				prevSegment.nextSegment = nextSegment;
				DivideAdjacentSegments(prevSegment, position);
				edgeIndex2 = prevSegment.nextEdgeIndex;
			}

			int nodeIndex;
			_voronoiGraph.AddNode(out nodeIndex);
			_voronoiNodePositions.Add(position);

			_voronoiGraph.AttachEdgeToSourceNode(edgeIndex0, nodeIndex);
			_voronoiGraph.AttachEdgeToSourceNodeAfter(edgeIndex1, edgeIndex0);
			_voronoiGraph.AttachEdgeToSourceNodeAfter(edgeIndex2, edgeIndex1);

			if (!float.IsNaN(position.x))
			{
				int neighborNodeIndex = _voronoiGraph.GetEdgeTargetNodeIndex(edgeIndex0);
				if (neighborNodeIndex != -1)
				{
					var neighborPosition = _voronoiNodePositions[neighborNodeIndex];
					if (!float.IsNaN(neighborPosition.x) && (neighborPosition - position).sqrMagnitude < _errorMargin)
					{
						MergeVoronoiNodes(edgeIndex0);
					}
				}

				neighborNodeIndex = _voronoiGraph.GetEdgeTargetNodeIndex(edgeIndex1);
				if (neighborNodeIndex != -1)
				{
					var neighborPosition = _voronoiNodePositions[neighborNodeIndex];
					if (!float.IsNaN(neighborPosition.x) && (neighborPosition - position).sqrMagnitude < _errorMargin)
					{
						MergeVoronoiNodes(edgeIndex1);
					}
				}
			}

			DestroyBeachSegment(segment);
		}

		private void MergeVoronoiNodes(int edgeIndex)
		{
			_voronoiGraph.CollapseEdge(edgeIndex, RemapNodeData, RemapEdgeData);
		}

		private void RemapNodeData(int toNodeIndex, int fromNodeIndex)
		{
			_voronoiNodePositions[toNodeIndex] = _voronoiNodePositions[fromNodeIndex];
			_voronoiNodePositions.RemoveAt(fromNodeIndex);
		}

		private void RemapEdgeData(int toEdgeIndex, int toTwinIndex, int fromEdgeIndex, int fromTwinIndex)
		{
			_voronoiEdgeStuff[toEdgeIndex] = _voronoiEdgeStuff[fromEdgeIndex];
			_voronoiEdgeStuff[toTwinIndex] = _voronoiEdgeStuff[fromTwinIndex];
			_voronoiEdgeStuff.RemoveRange(fromEdgeIndex & ~1, 2);
		}

		private static void InsertOrdered(List<DirectedEdge> list, DirectedEdge item)
		{
			for (int i = 0; i < list.Count; ++i)
			{
				if (list[i].order > item.order)
				{
					list.Insert(i, item);
					return;
				}
			}
			list.Add(item);
		}

		private Vector2 GetReframedPosition(int nodeIndex)
		{
			var position = _pointSitePositions[nodeIndex] - _origin;
			return new Vector2(
				Vector3.Dot(_forward, position),
				Vector3.Dot(_up, position));
		}

		private Vector2 GetReframedSourcePosition(int edgeIndex)
		{
			return GetReframedPosition(new GraphEdge(_siteGraph, edgeIndex).sourceNode.index);
		}

		private Vector2 GetReframedTargetPosition(int edgeIndex)
		{
			return GetReframedPosition(new GraphEdge(_siteGraph, edgeIndex).targetNode.index);
		}

		private Vector2 GetReframedVector(GraphEdge edge)
		{
			return GetReframedPosition(edge.targetNode.index) - GetReframedPosition(edge.sourceNode.index);
		}
	}

	public class VoronoiDiagram
	{
		public VoronoiDiagram(ITopology topology, ITopologyNodeData<Vector3> vertexPositions)
		{
		}
	}
}
