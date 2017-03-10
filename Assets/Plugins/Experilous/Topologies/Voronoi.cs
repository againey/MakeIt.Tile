/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using Experilous.Numerics;

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

		private Vector3 _origin;
		private Vector3 _right;
		private Vector3 _up;

		private IGraph _siteGraph;
		//TODO: private IGraphEdgeData<???> _edgeSiteArcData;

		private GraphNodeDataList<Vector2> _pointSitePositions = new GraphNodeDataList<Vector2>();

		private SplitEventPriorityQueue _splitEventQueue;
		private MergeEventPriorityQueue _mergeEventQueue;
		private List<SplitEvent> _splitEventPool = new List<SplitEvent>();
		private List<MergeEvent> _mergeEventPool = new List<MergeEvent>();

		private List<BeachSegment> _beachSegmentPool = new List<BeachSegment>();
		private BeachSegment _firstBeachSegment;
		private BeachSegment _lastBeachSegment;

		private List<DirectedEdge> _orderedLineSites = new List<DirectedEdge>();
		private List<DirectedEdge> _directedEdgePool = new List<DirectedEdge>();

		private float _sweep;

		private DynamicGraph _voronoiGraph = new DynamicGraph();
		private GraphNodeDataList<Vector2> _voronoiNodePositions = new GraphNodeDataList<Vector2>();

		private float _errorMargin;

		public PlanarVoronoiGenerator(float errorMargin)
		{
			_errorMargin = errorMargin;

			_splitEventQueue = new SplitEventPriorityQueue(_errorMargin);
			_mergeEventQueue = new MergeEventPriorityQueue(_errorMargin);
		}

		public void SetSites(IGraph siteGraph, IGraphNodeData<Vector3> pointSitePositions, Vector3 origin, Vector3 right, Vector3 up)
		{
			_siteGraph = siteGraph;

			_origin = origin;
			_right = right.normalized;
			_up = Vector3.Cross(right, Vector3.Cross(up, right)).normalized;

			_splitEventQueue.Clear();
			_mergeEventQueue.Clear();

			_firstBeachSegment = _lastBeachSegment = CreateBeachSegment(null, null, -1, -1);

			_sweep = float.NegativeInfinity;

			_voronoiGraph.Clear();
			_voronoiNodePositions.Clear();

			_pointSitePositions.Clear();

			foreach (var pointSite in _siteGraph.nodes)
			{
				var position = pointSitePositions[pointSite];
				float x = Vector3.Dot(_right, position);
				float y = Vector3.Dot(_up, position);
				_pointSitePositions.Add(new Vector2(x, y));
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
				convertedNodePositions[i] = _origin + position.x * _right + position.y * _up;
			}

			throw new NotImplementedException();
			//return new VoronoiDiagram(new FixedSizeTopology(_voronoiGraph), convertedNodePositions);
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
					var direction = (_pointSitePositions[edge.targetNode] - _pointSitePositions[edge.sourceNode]).normalized;
					InsertOrdered(_orderedLineSites, CreateDirectedEdge(direction, edge.index));
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

				float x = GetSegmentRightBound(segment);

				if (!float.IsNaN(x) && position.x <= x)
				{
					return segment;
				}

				segment = segment.nextSegment;
			}

			return segment;
		}

		private float GetSegmentRightBound(BeachSegment segment)
		{
			var nextSegment = segment.nextSegment;
			switch (segment.siteType)
			{
				case VoronoiSiteType.Point:
					switch (nextSegment.siteType)
					{
						case VoronoiSiteType.Point:
							return VoronoiUtility.GetSegmentRightBound_PointPoint(
								_pointSitePositions[segment.siteIndex],
								_pointSitePositions[nextSegment.siteIndex],
								_sweep, _errorMargin);
						case VoronoiSiteType.Line:
							return VoronoiUtility.GetSegmentRightBound_PointLine(
								_pointSitePositions[segment.siteIndex],
								_pointSitePositions[_siteGraph.GetEdgeTargetNodeIndex(nextSegment.siteIndex ^ 1)],
								_pointSitePositions[_siteGraph.GetEdgeTargetNodeIndex(nextSegment.siteIndex)],
								_sweep, _errorMargin);
						case VoronoiSiteType.None:
						default: throw new NotImplementedException();
					}
				case VoronoiSiteType.Line:
					switch (nextSegment.siteType)
					{
						case VoronoiSiteType.Point:
							return VoronoiUtility.GetSegmentRightBound_PointLine(
								_pointSitePositions[nextSegment.siteIndex],
								_pointSitePositions[_siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex ^ 1)],
								_pointSitePositions[_siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex)],
								_sweep, _errorMargin); //TODO: Reverse this?  Somehow get the other intersection.
						case VoronoiSiteType.Line:
							return VoronoiUtility.GetSegmentRightBound_LineLine(
								_pointSitePositions[_siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex ^ 1)],
								_pointSitePositions[_siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex)],
								_pointSitePositions[_siteGraph.GetEdgeTargetNodeIndex(nextSegment.siteIndex ^ 1)],
								_pointSitePositions[_siteGraph.GetEdgeTargetNodeIndex(nextSegment.siteIndex)],
								_sweep, _errorMargin);
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

			switch (segment.prevSegment.siteType)
			{
				case VoronoiSiteType.Point:
					switch (segment.siteType)
					{
						case VoronoiSiteType.Point:
							switch (segment.nextSegment.siteType)
							{
								case VoronoiSiteType.Point:
									CheckForMergeEvent_PPP(segment);
									break;
								case VoronoiSiteType.Line:
									CheckForMergeEvent_PPL(segment);
									break;
							}
							break;
						case VoronoiSiteType.Line:
							switch (segment.nextSegment.siteType)
							{
								case VoronoiSiteType.Point:
									CheckForMergeEvent_PLP(segment);
									break;
								case VoronoiSiteType.Line:
									CheckForMergeEvent_PLL(segment);
									break;
							}
							break;
					}
					break;
				case VoronoiSiteType.Line:
					switch (segment.siteType)
					{
						case VoronoiSiteType.Point:
							switch (segment.nextSegment.siteType)
							{
								case VoronoiSiteType.Point:
									CheckForMergeEvent_LPP(segment);
									break;
								case VoronoiSiteType.Line:
									CheckForMergeEvent_LPL(segment);
									break;
							}
							break;
						case VoronoiSiteType.Line:
							switch (segment.nextSegment.siteType)
							{
								case VoronoiSiteType.Point:
									CheckForMergeEvent_LLP(segment);
									break;
								case VoronoiSiteType.Line:
									CheckForMergeEvent_LLL(segment);
									break;
							}
							break;
					}
					break;
			}
		}

		private void CheckForMergeEvent_PPP(BeachSegment segment)
		{
			var p0 = _pointSitePositions[segment.prevSegment.siteIndex];
			var p1 = _pointSitePositions[segment.siteIndex];
			var p2 = _pointSitePositions[segment.nextSegment.siteIndex];

			var v0 = p1 - p0;
			var v2 = p1 - p2;

			float determinant = Geometry.DotPerpendicularCCW(v0, v2);
			if (determinant > 0f)
			{
				float lenSqr0 = v0.sqrMagnitude;
				float lenSqr2 = v2.sqrMagnitude;
				float cx = 0.5f * (v2.y * lenSqr0 - v0.y * lenSqr2) / determinant;
				float cy = 0.5f * (v0.x * lenSqr2 - v2.x * lenSqr0) / determinant;

				var mergeOffset = new Vector2(cx, cy);
				float radius = mergeOffset.magnitude;
				var mergePosition = p1 + mergeOffset;

				_mergeEventQueue.Push(CreateMergeEvent(mergePosition.x + radius, mergePosition.y, mergePosition, segment));
			}
		}

		private void CheckForMergeEvent_PPL(BeachSegment segment)
		{
			int targetNodeIndex = _siteGraph.GetEdgeTargetNodeIndex(segment.nextSegment.siteIndex);
			if (segment.prevSegment.siteIndex == targetNodeIndex)
			{
			}
			else if (segment.siteIndex == targetNodeIndex)
			{
			}
			else
			{
			}
		}

		private void CheckForMergeEvent_PLP(BeachSegment segment)
		{
			int targetNodeIndex = _siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex);
			int sourceNodeIndex = _siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex ^ 1);

			var p1 = _pointSitePositions[sourceNodeIndex];
			var v1 = _pointSitePositions[targetNodeIndex] - p1;
			var n1 = v1.PerpendicularCCW();
			var vSqrLen = v1.sqrMagnitude;

			var p0 = _pointSitePositions[segment.nextSegment.siteIndex];
			var p2 = _pointSitePositions[segment.prevSegment.siteIndex];
			var d0 = p0 - p1;
			var d2 = p2 - p1;

			var q0 = new Vector2(Vector2.Dot(d0, v1), Vector2.Dot(d0, n1)) / vSqrLen;
			var q2 = new Vector2(Vector2.Dot(d2, v1), Vector2.Dot(d2, n1)) / vSqrLen;

			float qDeltaY = q2.y - q0.y;

			float x, y;

			if (segment.prevSegment.siteIndex == sourceNodeIndex)
			{
				if (segment.nextSegment.siteIndex != targetNodeIndex)
				{
					x = Geometry.DotPerpendicularCW(q0, q2) / qDeltaY;
					var dx = x - q2.x;
					y = (dx * dx / q2.y + q2.y) / 2f;
				}
				else
				{
					return;
				}
			}
			else if (segment.nextSegment.siteIndex == targetNodeIndex)
			{
				x = Geometry.DotPerpendicularCW(q0, q2) / qDeltaY;
				var dx = x - q0.x;
				y = (dx * dx / q0.y + q0.y) / 2f;
			}
			else
			{
				if (Mathf.Abs(qDeltaY) >= _errorMargin)
				{
					float qProductY = q0.y * q2.y;
					if (qProductY >= _errorMargin)
					{
						var dq = q2 - q0;
						x = (Geometry.DotPerpendicularCW(q0, q2) + Mathf.Sqrt(dq.sqrMagnitude * qProductY) * Mathf.Sign(qDeltaY)) / qDeltaY;
						y = (((q0.y + q2.y) * x + (q0.x * q2.y + q0.y * q2.x)) * x + (q0.sqrMagnitude * q2.y + q2.sqrMagnitude * q0.y)) / (4f * qProductY);
					}
					else if (qProductY > -_errorMargin)
					{
						x = Geometry.DotPerpendicularCW(q0, q2) / qDeltaY;

						float absY0 = Mathf.Abs(q0.y);
						float absY2 = Mathf.Abs(q2.y);
						if (absY0 > _errorMargin && absY0 > absY2)
						{
							var dx = x - q0.x;
							y = (dx * dx / q0.y + q0.y) / 2f;
						}
						else if (absY2 > _errorMargin && absY2 > absY0)
						{
							var dx = x - q2.x;
							y = (dx * dx / q2.y + q2.y) / 2f;
						}
						else
						{
							return;
						}
					}
					else
					{
						return;
					}
				}
				else
				{
					x = (q0.x + q2.x) / 2f;
					y = ((2f * x + q0.x + q2.x) * x + q0.sqrMagnitude + q2.sqrMagnitude) / (2f * (q0.y + q2.y));
				}
			}

			var radius = Mathf.Sqrt(vSqrLen) * y;
			var mergePosition = p1 + v1 * x + n1 * y;
			_mergeEventQueue.Push(CreateMergeEvent(mergePosition.x + radius, mergePosition.y, mergePosition, segment));
		}

		private Vector2 IntersectParabolaWithVertical(Vector2 directrix, Vector2 normal, Vector2 endPointToFocus)
		{
			float a = Vector2.Dot(directrix, directrix);
			float b = Vector2.Dot(endPointToFocus, directrix);
			float c = Vector2.Dot(endPointToFocus, normal);
			float t = (b * b / c + c) / (2f * a);
			return normal * t;
		}

		private void CheckForMergeEvent_PLL(BeachSegment segment)
		{
			int targetNodeIndex = _siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex);
			if (segment.prevSegment.siteIndex == targetNodeIndex)
			{
			}
			else
			{
			}
		}

		private void CheckForMergeEvent_LPP(BeachSegment segment)
		{
			int sourceNodeIndex = _siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex ^ 1);
			if (segment.siteIndex == sourceNodeIndex)
			{
			}
			else if (segment.nextSegment.siteIndex == sourceNodeIndex)
			{
			}
			else
			{
			}
		}

		private void CheckForMergeEvent_LPL(BeachSegment segment)
		{
			int sourceNodeIndex = _siteGraph.GetEdgeTargetNodeIndex(segment.prevSegment.siteIndex ^ 1);
			int targetNodeIndex = _siteGraph.GetEdgeTargetNodeIndex(segment.nextSegment.siteIndex);
			if (segment.siteIndex == sourceNodeIndex)
			{
				if (segment.siteIndex == targetNodeIndex)
				{
				}
				else
				{
				}
			}
			else if (segment.siteIndex == targetNodeIndex)
			{
			}
			else
			{
			}
		}

		private void CheckForMergeEvent_LLP(BeachSegment segment)
		{
			int sourceNodeIndex = _siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex ^ 1);
			if (segment.nextSegment.siteIndex == sourceNodeIndex)
			{
			}
			else
			{
			}
		}

		private void CheckForMergeEvent_LLL(BeachSegment segment)
		{

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
			//_voronoiEdgeStuff[toEdgeIndex] = _voronoiEdgeStuff[fromEdgeIndex];
			//_voronoiEdgeStuff[toTwinIndex] = _voronoiEdgeStuff[fromTwinIndex];
			//_voronoiEdgeStuff.RemoveRange(fromEdgeIndex & ~1, 2);
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
	}

	public class VoronoiDiagram
	{
		public VoronoiDiagram(ITopology topology, ITopologyNodeData<Vector3> vertexPositions)
		{
		}
	}
}
