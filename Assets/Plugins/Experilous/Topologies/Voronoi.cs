/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using Experilous.Numerics;
using Experilous.Topologies.Detail;

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
			public float distance;
			public BeachSegment segment;

			public MergeEvent(Vector2 position, float distance, BeachSegment segment)
			{
				this.position = position;
				this.distance = distance;
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
				order = VoronoiUtility.ComputeDirectionalOrder(direction);
				this.edgeIndex = edgeIndex;
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

		private GraphNodeDataArray<int> _siteNodeFirstVoronoiEdgeIndices;
		private GraphEdgeDataArray<int> _siteEdgeFirstVoronoiEdgeIndices;
		private int _externalSiteFirstVoronoiEdgeIndex;

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

			_siteNodeFirstVoronoiEdgeIndices = new GraphNodeDataArray<int>(_siteGraph.nodeCount);
			for (int i = 0; i < _siteNodeFirstVoronoiEdgeIndices.Count; ++i)
			{
				_siteNodeFirstVoronoiEdgeIndices[i] = -1;
			}

			_siteEdgeFirstVoronoiEdgeIndices = new GraphEdgeDataArray<int>(_siteGraph.edgeCount);
			for (int i = 0; i < _siteEdgeFirstVoronoiEdgeIndices.Count; ++i)
			{
				_siteEdgeFirstVoronoiEdgeIndices[i] = -1;
			}

			_externalSiteFirstVoronoiEdgeIndex = -1;

			_origin = origin;
			_right = right.normalized;
			_up = Vector3.Cross(right, Vector3.Cross(up, right)).normalized;

			_firstBeachSegment = _lastBeachSegment = CreateBeachSegment(null, null, -1, -1);

			_sweep = float.NegativeInfinity;

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

			var voronoiDiagram = FinalizeVoronoiDiagram();

			_firstBeachSegment = _lastBeachSegment = null;

			_siteNodeFirstVoronoiEdgeIndices = null;
			_siteEdgeFirstVoronoiEdgeIndices = null;

			_splitEventQueue.Clear();
			_mergeEventQueue.Clear();

			_voronoiGraph.Clear();
			_voronoiNodePositions.Clear();

			_pointSitePositions.Clear();

			return voronoiDiagram;
		}

		private VoronoiDiagram FinalizeVoronoiDiagram()
		{
			int voronoiPointSiteFaceCount = 0;
			for (int i = 0; i < _siteNodeFirstVoronoiEdgeIndices.Count; ++i)
			{
				if (_siteNodeFirstVoronoiEdgeIndices[i] != -1) ++voronoiPointSiteFaceCount;
			}

			int voronoiLineSiteFaceCount = 0;
			for (int i = 0; i < _siteEdgeFirstVoronoiEdgeIndices.Count; ++i)
			{
				if (_siteEdgeFirstVoronoiEdgeIndices[i] != -1) ++voronoiLineSiteFaceCount;
			}

			int faceCount = voronoiPointSiteFaceCount + voronoiLineSiteFaceCount + 1;
			var faceNeighborCounts = new int[faceCount];
			var faceFirstEdgeIndices = new int[faceCount];
			var edgeTargetFaceIndices = new int[_voronoiGraph.edgeCount];

			int faceIndex = 0;

			Action<int> initializeVoronoiFace = (int firstEdgeIndex) =>
			{
				if (firstEdgeIndex != -1)
				{
					faceFirstEdgeIndices[faceIndex] = firstEdgeIndex;

					int neighborCount = 0;
					var edgeIndex = firstEdgeIndex;
					do
					{
						edgeTargetFaceIndices[edgeIndex ^ 1] = faceIndex;
						++neighborCount;
						edgeIndex = _voronoiGraph.GetEdgeNextChainedEdgeIndex(edgeIndex);
					} while (edgeIndex != firstEdgeIndex);

					faceNeighborCounts[faceIndex] = neighborCount;

					++faceIndex;
				}
			};

			for (int i = 0; i < _siteNodeFirstVoronoiEdgeIndices.Count; ++i)
			{
				initializeVoronoiFace(_siteNodeFirstVoronoiEdgeIndices[i]);
			}

			for (int i = 0; i < _siteEdgeFirstVoronoiEdgeIndices.Count; ++i)
			{
				initializeVoronoiFace(_siteEdgeFirstVoronoiEdgeIndices[i]);
			}

			initializeVoronoiFace(_externalSiteFirstVoronoiEdgeIndex);

			var voronoiTopology = new FixedSizeTopology(
				_voronoiGraph.GetNodeNeighborCounts(),
				_voronoiGraph.GetNodeFirstEdgeIndices(),
				faceNeighborCounts,
				faceFirstEdgeIndices,
				_voronoiGraph.GetEdgeNextChainedEdgeIndices(),
				_voronoiGraph.GetEdgeNextLateralEdgeIndices(),
				_voronoiGraph.GetEdgeTargetNodeIndices(),
				edgeTargetFaceIndices,
				new EdgeWrap[_voronoiGraph.edgeCount]);

			var finalVoronoiPositions = new TopologyNodeDataArray<Vector3>(_voronoiNodePositions.Count);
			for (int i = 0; i < _voronoiNodePositions.Count; ++i)
			{
				finalVoronoiPositions[i] = _voronoiNodePositions[i].x * _right + _voronoiNodePositions[i].y * _up;
			}

			var voronoiFaceSiteTypes = new TopologyFaceDataArray<VoronoiSiteType>(voronoiTopology.faceCount);
			var voronoiFaceSiteIndices = new TopologyFaceDataArray<int>(voronoiTopology.faceCount);

			for (int i = 0; i < voronoiTopology.faceCount; ++i)
			{
				voronoiFaceSiteTypes[i] = VoronoiSiteType.None;
				voronoiFaceSiteIndices[i] = -1;
			}

			for (int i = 0; i < _siteGraph.nodeCount; ++i)
			{
				int edgeIndex = _siteNodeFirstVoronoiEdgeIndices[i];
				if (edgeIndex != -1)
				{
					int siteFaceEdge = voronoiTopology.GetEdgeTargetFaceIndex(edgeIndex ^ 1);
					voronoiFaceSiteTypes[siteFaceEdge] = VoronoiSiteType.Point;
					voronoiFaceSiteIndices[siteFaceEdge] = i;
				}
			}

			for (int i = 0; i < _siteGraph.edgeCount; ++i)
			{
				int edgeIndex = _siteEdgeFirstVoronoiEdgeIndices[i];
				if (edgeIndex != -1)
				{
					int siteFaceEdge = voronoiTopology.GetEdgeTargetFaceIndex(edgeIndex ^ 1);
					voronoiFaceSiteTypes[siteFaceEdge] = VoronoiSiteType.Line;
					voronoiFaceSiteIndices[siteFaceEdge] = i;
				}
			}

			return new VoronoiDiagram(_siteGraph, voronoiTopology, _siteNodeFirstVoronoiEdgeIndices, _siteEdgeFirstVoronoiEdgeIndices, finalVoronoiPositions, voronoiFaceSiteTypes, voronoiFaceSiteIndices);
		}

		#region Pooled Object Management

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

		private MergeEvent CreateMergeEvent(Vector2 mergePosition, float distance, BeachSegment segment)
		{
			if (_mergeEventPool.Count > 0)
			{
				int lastIndex = _mergeEventPool.Count - 1;
				var ev = _mergeEventPool[lastIndex];
				_mergeEventPool.RemoveAt(lastIndex);
				ev.position.x = mergePosition.x;
				ev.position.y = mergePosition.y + distance;
				ev.distance = distance;
				ev.segment = segment;
				return ev;
			}
			else
			{
				return new MergeEvent(new Vector2(mergePosition.x, mergePosition.y + distance), distance, segment);
			}
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
				directedEdge.order = VoronoiUtility.ComputeDirectionalOrder(direction);
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

		#endregion

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

			if (incomingLineCount == 0)
			{
				// This point site does not have any incoming line segments from point sites already processed.
				// Assert(prevSegment.siteType != VoronoiSiteType.Line || new GraphEdge(_siteGraph, prevSegment.siteIndex).sourceNode != ev.siteIndex);

				nextSegment = SplitBeachSegment(prevSegment);
			}
			else
			{
				// This point site has one or more incoming line segments from point sites already processed.
				// Assert(prevSegment.siteType == VoronoiSiteType.Line && new GraphEdge(_siteGraph, prevSegment.siteIndex).sourceNode == ev.siteIndex);

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
					_siteNodeFirstVoronoiEdgeIndices[pointSite] = prevSegment.nextEdgeIndex ^ 1;
				}
				else
				{
					var newSegment = InsertBeachSegmentAfter(pointSite, prevSegment);
					DivideSeparatedSegments(prevSegment, nextSegment);

					if (incomingLineCount > 0)
					{
						_voronoiGraph.AttachEdgeToSourceNodeBefore(prevSegment.nextEdgeIndex, voronoiNode.firstEdge.index);
						_voronoiGraph.AttachEdgeToSourceNodeBefore(newSegment.nextEdgeIndex, prevSegment.nextEdgeIndex);
					}

					_siteNodeFirstVoronoiEdgeIndices[pointSite] = prevSegment.nextEdgeIndex ^ 1;
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
					var insertedSegment = InsertBeachSegmentAfter(outgoingEdge.twin, insertAfterSegment);
					if (!wasFirst)
					{
						DivideAdjacentSegments(insertAfterSegment);
						_voronoiGraph.AttachEdgeToSourceNodeBefore(insertAfterSegment.nextEdgeIndex, insertBeforeEdgeIndex);
						insertBeforeEdgeIndex = insertAfterSegment.nextEdgeIndex;
					}

					insertAfterSegment = insertedSegment;

					insertedSegment = InsertBeachSegmentAfter(outgoingEdge, insertAfterSegment);

					DivideAdjacentSegments(insertAfterSegment);
					_voronoiGraph.AttachEdgeToSourceNodeBefore(insertAfterSegment.nextEdgeIndex, insertBeforeEdgeIndex);
					insertBeforeEdgeIndex = insertAfterSegment.nextEdgeIndex;
					insertAfterSegment = insertedSegment;

					_siteEdgeFirstVoronoiEdgeIndices[outgoingEdge.twin] = insertedSegment.prevEdgeIndex;
					_siteEdgeFirstVoronoiEdgeIndices[outgoingEdge] = insertedSegment.prevEdgeIndex ^ 1;
				}

				if (!nextConcavity)
				{
					var insertedSegment = InsertBeachSegmentAfter(pointSite, insertAfterSegment);
					DivideAdjacentSegments(insertAfterSegment.prevSegment);
					_voronoiGraph.AttachEdgeToSourceNodeBefore(insertAfterSegment.nextEdgeIndex, insertBeforeEdgeIndex);
					insertBeforeEdgeIndex = insertAfterSegment.nextEdgeIndex;
					insertAfterSegment = insertedSegment;
				}

				DivideSeparatedSegments(prevSegment, nextSegment);

				if (incomingLineCount > 0)
				{
					_voronoiGraph.AttachEdgeToSourceNodeBefore(prevSegment.nextEdgeIndex, insertBeforeEdgeIndex);
					_voronoiGraph.AttachEdgeToSourceNodeBefore(insertAfterSegment.nextEdgeIndex, insertBeforeEdgeIndex);

					if (prevConcavity)
					{
						_siteNodeFirstVoronoiEdgeIndices[pointSite] = prevSegment.nextSegment.nextEdgeIndex;
					}
					else if (nextConcavity)
					{
						_siteNodeFirstVoronoiEdgeIndices[pointSite] = _voronoiGraph.GetEdgeNextChainedEdgeIndex(nextSegment.prevSegment.prevEdgeIndex);
					}
				}
				else if (prevConcavity || nextConcavity)
				{
					_siteNodeFirstVoronoiEdgeIndices[pointSite] = prevSegment.nextSegment.nextEdgeIndex;
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

		private void DivideAdjacentSegments(BeachSegment prevSegment)
		{
			int edgeIndex;
			_voronoiGraph.AddEdgePair(out edgeIndex);
			prevSegment.nextEdgeIndex = edgeIndex ^ 1;
			prevSegment.nextSegment.prevEdgeIndex = edgeIndex;
		}

		private void DivideSeparatedSegments(BeachSegment prevSegment, BeachSegment nextSegment)
		{
			if (BeachSegment.SitesAreEqual(prevSegment, nextSegment))
			{
				DivideAdjacentSegments(prevSegment);
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
				segment.mergeEvent = CreateMergeEvent(new Vector2(float.NegativeInfinity, _sweep), float.PositiveInfinity, segment);
				_mergeEventQueue.Push(segment.mergeEvent);
				return;
			}

			if (ReferenceEquals(segment.prevSegment, _firstBeachSegment))
			{
				segment.mergeEvent = CreateMergeEvent(new Vector2(float.NegativeInfinity, float.PositiveInfinity), float.NaN, segment);
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
									CheckForMergeEvent_PointPointPoint(segment);
									break;
								case VoronoiSiteType.Line:
									CheckForMergeEvent_PointPointLine(segment);
									break;
							}
							break;
						case VoronoiSiteType.Line:
							switch (segment.nextSegment.siteType)
							{
								case VoronoiSiteType.Point:
									CheckForMergeEvent_PointLinePoint(segment);
									break;
								case VoronoiSiteType.Line:
									CheckForMergeEvent_PointLineLine(segment);
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
									CheckForMergeEvent_LinePointPoint(segment);
									break;
								case VoronoiSiteType.Line:
									CheckForMergeEvent_LinePointLine(segment);
									break;
							}
							break;
						case VoronoiSiteType.Line:
							switch (segment.nextSegment.siteType)
							{
								case VoronoiSiteType.Point:
									CheckForMergeEvent_LineLinePoint(segment);
									break;
								case VoronoiSiteType.Line:
									CheckForMergeEvent_LineLineLine(segment);
									break;
							}
							break;
					}
					break;
			}
		}

		private void CheckForMergeEvent_PointPointPoint(BeachSegment segment)
		{
			var p0 = _pointSitePositions[segment.prevSegment.siteIndex];
			var p1 = _pointSitePositions[segment.siteIndex];
			var p2 = _pointSitePositions[segment.nextSegment.siteIndex];

			Vector2 mergePosition;
			float distance;
			if (VoronoiUtility.CheckForMergeEvent_PointPointPoint(p0, p1, p2, out mergePosition, out distance))
			{
				_mergeEventQueue.Push(CreateMergeEvent(mergePosition, distance, segment));
			}
		}

		private void CheckForMergeEvent_PointPointLine(BeachSegment segment)
		{
			int targetNodeIndex = _siteGraph.GetEdgeTargetNodeIndex(segment.nextSegment.siteIndex);
			int sourceNodeIndex = _siteGraph.GetEdgeTargetNodeIndex(segment.nextSegment.siteIndex ^ 1);

			var p0 = _pointSitePositions[segment.prevSegment.siteIndex];
			var p1 = _pointSitePositions[segment.siteIndex];
			var p2a = _pointSitePositions[sourceNodeIndex];
			var p2b = _pointSitePositions[targetNodeIndex];

			Vector2 mergePosition;
			float distance;

			if (segment.siteIndex == targetNodeIndex)
			{
				if (VoronoiUtility.CheckForMergeEvent_PointTargetPointLine(p0, p2a, p2b, out mergePosition, out distance))
				{
					_mergeEventQueue.Push(CreateMergeEvent(mergePosition, distance, segment));
				}
			}
			else
			{
				if (VoronoiUtility.CheckForMergeEvent_PointPointLine(p0, p1, p2a, p2b, _errorMargin, out mergePosition, out distance))
				{
					_mergeEventQueue.Push(CreateMergeEvent(mergePosition, distance, segment));
				}
			}
		}

		private void CheckForMergeEvent_PointLinePoint(BeachSegment segment)
		{
			int targetNodeIndex = _siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex);
			int sourceNodeIndex = _siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex ^ 1);

			var p0 = _pointSitePositions[segment.prevSegment.siteIndex];
			var p1a = _pointSitePositions[sourceNodeIndex];
			var p1b = _pointSitePositions[targetNodeIndex];
			var p2 = _pointSitePositions[segment.nextSegment.siteIndex];

			Vector2 mergePosition;
			float distance;

			if (segment.prevSegment.siteIndex == sourceNodeIndex)
			{
				if (segment.nextSegment.siteIndex != targetNodeIndex)
				{
					if (VoronoiUtility.CheckForMergeEvent_SourcePointLinePoint(p1a, p1b, p2, out mergePosition, out distance))
					{
						_mergeEventQueue.Push(CreateMergeEvent(mergePosition, distance, segment));
					}
				}
			}
			else if (segment.nextSegment.siteIndex == targetNodeIndex)
			{
				if (VoronoiUtility.CheckForMergeEvent_PointLineTargetPoint(p0, p1a, p1b, out mergePosition, out distance))
				{
					_mergeEventQueue.Push(CreateMergeEvent(mergePosition, distance, segment));
				}
			}
			else
			{
				if (VoronoiUtility.CheckForMergeEvent_PointLinePoint(p0, p1a, p1b, p2, _errorMargin, out mergePosition, out distance))
				{
					_mergeEventQueue.Push(CreateMergeEvent(mergePosition, distance, segment));
				}
			}
		}

		private void CheckForMergeEvent_PointLineLine(BeachSegment segment)
		{
			int targetNodeIndex1 = _siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex);
			int sourceNodeIndex1 = _siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex ^ 1);
			int targetNodeIndex2 = _siteGraph.GetEdgeTargetNodeIndex(segment.nextSegment.siteIndex);
			int sourceNodeIndex2 = _siteGraph.GetEdgeTargetNodeIndex(segment.nextSegment.siteIndex ^ 1);

			var p0 = _pointSitePositions[segment.prevSegment.siteIndex];
			var p1a = _pointSitePositions[sourceNodeIndex1];
			var p1b = _pointSitePositions[targetNodeIndex1];
			var p2a = _pointSitePositions[sourceNodeIndex2];
			var p2b = _pointSitePositions[targetNodeIndex2];

			Vector2 mergePosition;
			float distance;

			if (segment.prevSegment.siteIndex == sourceNodeIndex1)
			{
				if (VoronoiUtility.CheckForMergeEvent_SourcePointLineLine(p1a, p1b, p2a, p2b, out mergePosition, out distance))
				{
					_mergeEventQueue.Push(CreateMergeEvent(mergePosition, distance, segment));
				}
			}
			else
			{
				if (VoronoiUtility.CheckForMergeEvent_PointLineLine(p0, p1a, p1b, p2a, p2b, _errorMargin, out mergePosition, out distance))
				{
					_mergeEventQueue.Push(CreateMergeEvent(mergePosition, distance, segment));
				}
			}
		}

		private void CheckForMergeEvent_LinePointPoint(BeachSegment segment)
		{
			int targetNodeIndex = _siteGraph.GetEdgeTargetNodeIndex(segment.prevSegment.siteIndex);
			int sourceNodeIndex = _siteGraph.GetEdgeTargetNodeIndex(segment.prevSegment.siteIndex ^ 1);

			var p0a = _pointSitePositions[sourceNodeIndex];
			var p0b = _pointSitePositions[targetNodeIndex];
			var p1 = _pointSitePositions[segment.siteIndex];
			var p2 = _pointSitePositions[segment.nextSegment.siteIndex];

			Vector2 mergePosition;
			float distance;

			if (segment.siteIndex == sourceNodeIndex)
			{
				if (VoronoiUtility.CheckForMergeEvent_LineSourcePointPoint(p0a, p0b, p2, out mergePosition, out distance))
				{
					_mergeEventQueue.Push(CreateMergeEvent(mergePosition, distance, segment));
				}
			}
			else
			{
				if (VoronoiUtility.CheckForMergeEvent_LinePointPoint(p0a, p0b, p1, p2, _errorMargin, out mergePosition, out distance))
				{
					_mergeEventQueue.Push(CreateMergeEvent(mergePosition, distance, segment));
				}
			}
		}

		private void CheckForMergeEvent_LinePointLine(BeachSegment segment)
		{
			int targetNodeIndex0 = _siteGraph.GetEdgeTargetNodeIndex(segment.prevSegment.siteIndex);
			int sourceNodeIndex0 = _siteGraph.GetEdgeTargetNodeIndex(segment.prevSegment.siteIndex ^ 1);
			int targetNodeIndex2 = _siteGraph.GetEdgeTargetNodeIndex(segment.nextSegment.siteIndex);
			int sourceNodeIndex2 = _siteGraph.GetEdgeTargetNodeIndex(segment.nextSegment.siteIndex ^ 1);

			var p0a = _pointSitePositions[sourceNodeIndex0];
			var p0b = _pointSitePositions[targetNodeIndex0];
			var p1 = _pointSitePositions[segment.siteIndex];
			var p2a = _pointSitePositions[sourceNodeIndex2];
			var p2b = _pointSitePositions[targetNodeIndex2];

			Vector2 mergePosition;
			float distance;

			if (targetNodeIndex0 == segment.siteIndex)
			{
				if (segment.siteIndex != sourceNodeIndex2)
				{
					if (VoronoiUtility.CheckForMergeEvent_LineTargetPointLine(p0a, p0b, p2a, p2b, out mergePosition, out distance))
					{
						_mergeEventQueue.Push(CreateMergeEvent(mergePosition, distance, segment));
					}
				}
			}
			else if (segment.siteIndex == sourceNodeIndex2)
			{
				if (VoronoiUtility.CheckForMergeEvent_LineSourcePointLine(p0a, p0b, p2a, p2b, out mergePosition, out distance))
				{
					_mergeEventQueue.Push(CreateMergeEvent(mergePosition, distance, segment));
				}
			}
			else
			{
				if (VoronoiUtility.CheckForMergeEvent_LinePointLine(p0a, p0b, p1, p2a, p2b, _errorMargin, out mergePosition, out distance))
				{
					_mergeEventQueue.Push(CreateMergeEvent(mergePosition, distance, segment));
				}
			}
		}

		private void CheckForMergeEvent_LineLinePoint(BeachSegment segment)
		{
			int targetNodeIndex0 = _siteGraph.GetEdgeTargetNodeIndex(segment.prevSegment.siteIndex);
			int sourceNodeIndex0 = _siteGraph.GetEdgeTargetNodeIndex(segment.prevSegment.siteIndex ^ 1);
			int targetNodeIndex1 = _siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex);
			int sourceNodeIndex1 = _siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex ^ 1);

			var p0a = _pointSitePositions[sourceNodeIndex0];
			var p0b = _pointSitePositions[targetNodeIndex0];
			var p1a = _pointSitePositions[sourceNodeIndex1];
			var p1b = _pointSitePositions[targetNodeIndex1];
			var p2 = _pointSitePositions[segment.nextSegment.siteIndex];

			Vector2 mergePosition;
			float distance;

			if (targetNodeIndex1 == segment.nextSegment.siteIndex)
			{
				if (VoronoiUtility.CheckForMergeEvent_LineLineTargetPoint(p0a, p0b, p1a, p1b, out mergePosition, out distance))
				{
					_mergeEventQueue.Push(CreateMergeEvent(mergePosition, distance, segment));
				}
			}
			else
			{
				if (VoronoiUtility.CheckForMergeEvent_LineLinePoint(p0a, p0b, p1a, p1b, p2, _errorMargin, out mergePosition, out distance))
				{
					_mergeEventQueue.Push(CreateMergeEvent(mergePosition, distance, segment));
				}
			}
		}

		private void CheckForMergeEvent_LineLineLine(BeachSegment segment)
		{
			int targetNodeIndex0 = _siteGraph.GetEdgeTargetNodeIndex(segment.prevSegment.siteIndex);
			int sourceNodeIndex0 = _siteGraph.GetEdgeTargetNodeIndex(segment.prevSegment.siteIndex ^ 1);
			int targetNodeIndex1 = _siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex);
			int sourceNodeIndex1 = _siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex ^ 1);
			int targetNodeIndex2 = _siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex);
			int sourceNodeIndex2 = _siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex ^ 1);

			var p0a = _pointSitePositions[sourceNodeIndex0];
			var p0b = _pointSitePositions[targetNodeIndex0];
			var p1a = _pointSitePositions[sourceNodeIndex1];
			var p1b = _pointSitePositions[targetNodeIndex1];
			var p2a = _pointSitePositions[sourceNodeIndex2];
			var p2b = _pointSitePositions[targetNodeIndex2];

			Vector2 mergePosition;
			float distance;

			if (VoronoiUtility.CheckForMergeEvent_LineLineLine(p0a, p0b, p1a, p1b, p2a, p2b, _errorMargin, out mergePosition, out distance))
			{
				_mergeEventQueue.Push(CreateMergeEvent(mergePosition, distance, segment));
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
				DivideAdjacentSegments(prevSegment);
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
		private IGraph _siteGraph;
		private ITopology _voronoiTopology;
		private IGraphNodeData<int> _siteNodeFirstVoronoiEdgeIndices;
		private IGraphEdgeData<int> _siteEdgeFirstVoronoiEdgeIndices;
		private ITopologyNodeData<Vector3> _voronoiNodePositions;
		private ITopologyFaceData<VoronoiSiteType> _voronoiFaceSiteTypes;
		private ITopologyFaceData<int> _voronoiFaceSiteIndices;

		public VoronoiDiagram(IGraph siteGraph, ITopology voronoiTopology, IGraphNodeData<int> siteNodeFirstVoronoiEdgeIndices, IGraphEdgeData<int> siteEdgeFirstVoronoiEdgeIndices, ITopologyNodeData<Vector3> voronoiNodePositions, ITopologyFaceData<VoronoiSiteType> voronoiFaceSiteTypes, ITopologyFaceData<int> voronoiFaceSiteIndices)
		{
			_siteGraph = siteGraph;
			_voronoiTopology = voronoiTopology;
			_siteNodeFirstVoronoiEdgeIndices = siteNodeFirstVoronoiEdgeIndices;
			_siteEdgeFirstVoronoiEdgeIndices = siteEdgeFirstVoronoiEdgeIndices;
			_voronoiNodePositions = voronoiNodePositions;
			_voronoiFaceSiteTypes = voronoiFaceSiteTypes;
			_voronoiFaceSiteIndices = voronoiFaceSiteIndices;
		}
	}
}
