/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using Experilous.Numerics;
using Experilous.Topologies.Detail;
using InfiniteLoopGuard = Experilous.MakeItRandom.Detail.InfiniteLoopGuard;
using Math = Experilous.Numerics.Math;

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
		private Vector3 _origin;
		private Vector3 _right;
		private Vector3 _up;

		private IGraph _siteGraph;
		private IGraphNodeData<Vector3> _originalPointSitePositions;
		//TODO: private IGraphEdgeData<???> _edgeSiteArcData;

		private GraphNodeDataArray<int> _siteNodeFirstVoronoiEdgeIndices;
		private GraphEdgeDataArray<int> _siteEdgeFirstVoronoiEdgeIndices;
		private int _externalSiteFirstVoronoiEdgeIndex;

		private GraphNodeDataList<Vector2> _pointSitePositions = new GraphNodeDataList<Vector2>();

		private VoronoiUtility.SplitEventQueue _splitEventQueue;
		private VoronoiUtility.MergeEventQueue _mergeEventQueue;

		private VoronoiUtility.Beach _beach;

		private List<VoronoiUtility.DirectedEdge> _orderedLineSites = new List<VoronoiUtility.DirectedEdge>();
		private List<VoronoiUtility.DirectedEdge> _directedEdgePool = new List<VoronoiUtility.DirectedEdge>();

		private float _sweep;

		private DynamicGraph _voronoiGraph = new DynamicGraph();
		private GraphNodeDataList<Vector2> _voronoiNodePositions = new GraphNodeDataList<Vector2>();
		private GraphEdgeDataList<VoronoiEdgeShape> _voronoiEdgeShapes = new GraphEdgeDataList<VoronoiEdgeShape>();
		private GraphEdgeDataList<float> _voronoiEdgeShapeLowerBounds = new GraphEdgeDataList<float>();
		private GraphEdgeDataList<float> _voronoiEdgeShapeUpperBounds = new GraphEdgeDataList<float>();

		private float _errorMargin;

		public PlanarVoronoiGenerator(float errorMargin)
		{
			_errorMargin = errorMargin;

			_splitEventQueue = new VoronoiUtility.SplitEventQueue(_errorMargin);
			_mergeEventQueue = new VoronoiUtility.MergeEventQueue(_errorMargin);

			_beach = new VoronoiUtility.Beach();
		}

		public void SetSites(IGraph siteGraph, IGraphNodeData<Vector3> pointSitePositions, Vector3 origin, Vector3 right, Vector3 up)
		{
			_siteGraph = siteGraph;
			_originalPointSitePositions = pointSitePositions;

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

			_sweep = float.NegativeInfinity;

			foreach (var pointSite in _siteGraph.nodes)
			{
				var position = pointSitePositions[pointSite];
				var splitPosition = new Vector2(
					Vector3.Dot(_right, position),
					Vector3.Dot(_up, position));
				_pointSitePositions.Add(splitPosition);
				_splitEventQueue.Push(splitPosition, pointSite.index);
			}
		}

		public VoronoiDiagram Generate()
		{
			VoronoiDiagram voronoiDiagram;

			try
			{
				var guard = new InfiniteLoopGuard(1000);
				while (Step()) { guard.Iterate(); }

				var convertedNodePositions = new TopologyNodeDataArray<Vector3>(_voronoiNodePositions.Count);
				for (int i = 0; i < _voronoiNodePositions.Count; ++i)
				{
					var position = _voronoiNodePositions[i];
					convertedNodePositions[i] = _origin + position.x * _right + position.y * _up;
				}

				voronoiDiagram = FinalizeVoronoiDiagram();
			}
			finally
			{
				_siteGraph = null;
				_originalPointSitePositions = null;

				//_beach.Reset();
				_beach.Clear();

				_siteNodeFirstVoronoiEdgeIndices = null;
				_siteEdgeFirstVoronoiEdgeIndices = null;

				_splitEventQueue.Clear();
				_mergeEventQueue.Clear();

				_voronoiGraph.Clear();
				_voronoiNodePositions.Clear();
				_voronoiEdgeShapes.Clear();
				_voronoiEdgeShapeLowerBounds.Clear();
				_voronoiEdgeShapeUpperBounds.Clear();

				_pointSitePositions.Clear();
			}

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

			for (int i = 0; i < _voronoiGraph.edgeCount; ++i)
			{
				edgeTargetFaceIndices[i] = -1;
			}

			int faceIndex = 0;

			Action<int> initializeVoronoiFace = (int firstEdgeIndex) =>
			{
				if (firstEdgeIndex != -1)
				{
					faceFirstEdgeIndices[faceIndex] = firstEdgeIndex;

					int neighborCount = 0;
					var edgeIndex = firstEdgeIndex;
					var guard = new InfiniteLoopGuard(100);
					do
					{
						edgeTargetFaceIndices[edgeIndex ^ 1] = faceIndex;
						++neighborCount;
						edgeIndex = _voronoiGraph.GetEdgeNextChainedEdgeIndex(edgeIndex);
						guard.Iterate();
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

			for (int i = 0; i < _voronoiGraph.edgeCount; ++i)
			{
				if (edgeTargetFaceIndices[i] == -1)
				{
					initializeVoronoiFace(i ^ 1);
				}
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
				finalVoronoiPositions[i] = _origin + _voronoiNodePositions[i].x * _right + _voronoiNodePositions[i].y * _up;
			}

			var finalVoronoiEdgeShapes = new TopologyEdgeDataArray<VoronoiEdgeShape>(_voronoiEdgeShapes.Count);
			var finalVoronoiEdgeShapeLowerBounds = new TopologyEdgeDataArray<float>(_voronoiEdgeShapes.Count);
			var finalVoronoiEdgeShapeUpperBounds = new TopologyEdgeDataArray<float>(_voronoiEdgeShapes.Count);
			for (int i = 0; i < finalVoronoiEdgeShapes.Count; ++i)
			{
				finalVoronoiEdgeShapes[i] = _voronoiEdgeShapes[i].Transform(_origin, _right, _up);
				finalVoronoiEdgeShapeLowerBounds[i] = _voronoiEdgeShapeLowerBounds[i];
				finalVoronoiEdgeShapeUpperBounds[i] = _voronoiEdgeShapeUpperBounds[i];
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
					int siteFaceIndex = voronoiTopology.GetEdgeTargetFaceIndex(edgeIndex ^ 1);
					voronoiFaceSiteTypes[siteFaceIndex] = VoronoiSiteType.Point;
					voronoiFaceSiteIndices[siteFaceIndex] = i;
				}
			}

			for (int i = 0; i < _siteGraph.edgeCount; ++i)
			{
				int edgeIndex = _siteEdgeFirstVoronoiEdgeIndices[i];
				if (edgeIndex != -1)
				{
					int siteFaceIndex = voronoiTopology.GetEdgeTargetFaceIndex(edgeIndex ^ 1);
					voronoiFaceSiteTypes[siteFaceIndex] = VoronoiSiteType.Line;
					voronoiFaceSiteIndices[siteFaceIndex] = i;
				}
			}

			return new VoronoiDiagram(_siteGraph, _originalPointSitePositions, voronoiTopology, _siteNodeFirstVoronoiEdgeIndices, _siteEdgeFirstVoronoiEdgeIndices, finalVoronoiPositions, voronoiFaceSiteTypes, voronoiFaceSiteIndices, finalVoronoiEdgeShapes, finalVoronoiEdgeShapeLowerBounds, finalVoronoiEdgeShapeUpperBounds);
		}

		#region Pooled Object Management

		private VoronoiUtility.DirectedEdge CreateDirectedEdge(Vector2 direction, int edgeIndex)
		{
			if (_directedEdgePool.Count > 0)
			{
				int lastIndex = _directedEdgePool.Count - 1;
				var directedEdge = _directedEdgePool[lastIndex];
				_directedEdgePool.RemoveAt(lastIndex);
				directedEdge.direction = direction;
				directedEdge.order = VoronoiUtility.DirectedEdge.ComputeOrder(direction);
				directedEdge.edgeIndex = edgeIndex;
				return directedEdge;
			}
			else
			{
				return new VoronoiUtility.DirectedEdge(direction, edgeIndex);
			}
		}

		private void DestroyDirectedEdge(VoronoiUtility.DirectedEdge directedEdge)
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
					if (_splitEventQueue.Peek().IsBefore(_mergeEventQueue.Peek(), _errorMargin))
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

		private void AttachVoronoiEdgeToSourceNode(int edgeIndex, int nodeIndex)
		{
			_voronoiGraph.AttachEdgeToSourceNode(edgeIndex, nodeIndex);
			var p = _voronoiNodePositions[nodeIndex];
			float t = float.IsInfinity(p.x) ? float.NegativeInfinity : _voronoiEdgeShapes[edgeIndex].Project(p);
			_voronoiEdgeShapeLowerBounds[edgeIndex] = t;
			_voronoiEdgeShapeUpperBounds[edgeIndex ^ 1] = -t;
		}

		private void AttachVoronoiEdgeToSourceNodeBefore(int edgeIndex, int nodeIndex, int nextEdgeIndex)
		{
			_voronoiGraph.AttachEdgeToSourceNodeBefore(edgeIndex, nextEdgeIndex);
			var p = _voronoiNodePositions[nodeIndex];
			float t = float.IsInfinity(p.x) ? float.NegativeInfinity : _voronoiEdgeShapes[edgeIndex].Project(p);
			_voronoiEdgeShapeLowerBounds[edgeIndex] = t;
			_voronoiEdgeShapeUpperBounds[edgeIndex ^ 1] = -t;
		}

		private void AttachVoronoiEdgeToSourceNodeAfter(int edgeIndex, int nodeIndex, int prevEdgeIndex)
		{
			_voronoiGraph.AttachEdgeToSourceNodeAfter(edgeIndex, prevEdgeIndex);
			var p = _voronoiNodePositions[nodeIndex];
			float t = float.IsInfinity(p.x) ? float.NegativeInfinity : _voronoiEdgeShapes[edgeIndex].Project(p);
			_voronoiEdgeShapeLowerBounds[edgeIndex] = t;
			_voronoiEdgeShapeUpperBounds[edgeIndex ^ 1] = -t;
		}

		private void ProcessEvent(VoronoiUtility.SplitEvent ev)
		{
			_sweep = ev.position.y;

			GraphNode voronoiNode = GraphNode.none;
			int incomingLineCount = 0;
			int outgoingLineCount = 0;

			var pointSite = new GraphNode(_siteGraph, ev.nodeIndex);

			_orderedLineSites.Clear();

			if (pointSite.neighborCount > 0)
			{
				voronoiNode = _voronoiGraph.AddNode();
				_voronoiNodePositions.Add(_pointSitePositions[ev.nodeIndex]);

				var guard = new InfiniteLoopGuard(10);
				foreach (var edge in pointSite.edges)
				{
					var direction = (_pointSitePositions[edge.targetNode] - _pointSitePositions[edge.sourceNode]).normalized;
					InsertOrdered(_orderedLineSites, CreateDirectedEdge(direction, edge.index));
					guard.Iterate();
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

				nextSegment = _beach.Split(prevSegment);
			}
			else
			{
				// This point site has one or more incoming line segments from point sites already processed.
				// Assert(prevSegment.siteType == VoronoiSiteType.Line && new GraphEdge(_siteGraph, prevSegment.siteIndex).sourceNode == ev.siteIndex);

				var lastIncomingLine = _orderedLineSites[incomingLineCount - 1];
				var guard = new InfiniteLoopGuard(1000);
				while (nextSegment.siteType != VoronoiSiteType.Line || nextSegment.siteIndex != lastIncomingLine.edgeIndex)
				{
					nextSegment = nextSegment.nextSegment;
					guard.Iterate();
				}

				AttachVoronoiEdgeToSourceNode(nextSegment.prevEdgeIndex, voronoiNode.index);
				var segment = nextSegment.prevSegment;
				guard = new InfiniteLoopGuard(1000);
				while (!ReferenceEquals(segment, prevSegment))
				{
					AttachVoronoiEdgeToSourceNode(segment.prevEdgeIndex, voronoiNode.index);
					segment = segment.prevSegment;
					_beach.Remove(segment.nextSegment);
					guard.Iterate();
				}

				if (outgoingLineCount > 0)
				{
					var firstIncomingDirection = _orderedLineSites[0].direction;
					var firstOutgoingDirection = _orderedLineSites[_orderedLineSites.Count - 1].direction;
					prevConcavity = Geometry.DotPerpendicularCW(firstIncomingDirection, firstOutgoingDirection) < _errorMargin;

					var lastIncomingDirection = _orderedLineSites[incomingLineCount - 1].direction;
					var lastOutgoingDirection = _orderedLineSites[incomingLineCount].direction;
					nextConcavity = Geometry.DotPerpendicularCW(lastOutgoingDirection, lastIncomingDirection) < _errorMargin;
				}
			}

			if (outgoingLineCount == 0)
			{
				if (prevConcavity && nextConcavity)
				{
					DivideAdjacentSegments(prevSegment);
					AttachVoronoiEdgeToSourceNodeBefore(prevSegment.nextEdgeIndex, voronoiNode.index, voronoiNode.firstEdge.index);
					_siteNodeFirstVoronoiEdgeIndices[pointSite] = prevSegment.nextEdgeIndex;
				}
				else
				{
					var newSegment = _beach.InsertAfter(pointSite, prevSegment);
					DivideSeparatedSegments(prevSegment, nextSegment);

					if (incomingLineCount > 0)
					{
						AttachVoronoiEdgeToSourceNode(prevSegment.nextEdgeIndex, voronoiNode.index);
						AttachVoronoiEdgeToSourceNode(newSegment.nextEdgeIndex, voronoiNode.index);
					}

					_siteNodeFirstVoronoiEdgeIndices[pointSite] = prevSegment.nextEdgeIndex;
				}
			}
			else
			{
				var insertAfterSegment = prevSegment;

				if (!prevConcavity)
				{
					insertAfterSegment = _beach.InsertAfter(pointSite, insertAfterSegment);
				}

				for (int i = _orderedLineSites.Count - 1; i >= incomingLineCount; --i)
				{
					var outgoingEdge = new GraphEdge(_siteGraph, _orderedLineSites[i].edgeIndex);

					var insertedSegment = _beach.InsertAfter(outgoingEdge, insertAfterSegment);
					if (!ReferenceEquals(insertAfterSegment, prevSegment))
					{
						DivideAdjacentSegments(insertAfterSegment);
						AttachVoronoiEdgeToSourceNode(insertAfterSegment.nextEdgeIndex, voronoiNode.index);
					}

					insertAfterSegment = insertedSegment;

					insertedSegment = _beach.InsertAfter(outgoingEdge.twin, insertAfterSegment);

					DivideAdjacentSegments(insertAfterSegment);
					AttachVoronoiEdgeToSourceNode(insertAfterSegment.nextEdgeIndex, voronoiNode.index);

					_siteEdgeFirstVoronoiEdgeIndices[outgoingEdge] = insertedSegment.prevEdgeIndex;
					_siteEdgeFirstVoronoiEdgeIndices[outgoingEdge.twin] = insertAfterSegment.nextEdgeIndex;

					insertAfterSegment = insertedSegment;
				}

				if (!nextConcavity)
				{
					var insertedSegment = _beach.InsertAfter(pointSite, insertAfterSegment);
					DivideAdjacentSegments(insertAfterSegment);
					AttachVoronoiEdgeToSourceNode(insertAfterSegment.nextEdgeIndex, voronoiNode.index);
					insertAfterSegment = insertedSegment;
				}

				DivideSeparatedSegments(prevSegment, nextSegment);

				if (incomingLineCount > 0)
				{
					int prevInsertAfterEdgeIndex = _siteEdgeFirstVoronoiEdgeIndices[_orderedLineSites[0].edgeIndex] ^ 1;
					int nextInsertAfterEdgeIndex = _voronoiGraph.GetEdgeNextChainedEdgeIndex(_siteEdgeFirstVoronoiEdgeIndices[_orderedLineSites[incomingLineCount - 1].edgeIndex]);
					AttachVoronoiEdgeToSourceNodeAfter(prevSegment.nextEdgeIndex, voronoiNode.index, prevInsertAfterEdgeIndex);
					AttachVoronoiEdgeToSourceNodeAfter(insertAfterSegment.nextEdgeIndex, voronoiNode.index, nextInsertAfterEdgeIndex);

					if (prevConcavity)
					{
						_siteNodeFirstVoronoiEdgeIndices[pointSite] = nextSegment.prevSegment.prevEdgeIndex ^ 1;
					}
					else if (nextConcavity)
					{
						_siteNodeFirstVoronoiEdgeIndices[pointSite] = prevSegment.nextEdgeIndex;
					}
				}
				else if (prevConcavity || nextConcavity)
				{
					_siteNodeFirstVoronoiEdgeIndices[pointSite] = prevSegment.nextEdgeIndex;
				}
				else
				{
					_siteNodeFirstVoronoiEdgeIndices[pointSite] = nextSegment.prevSegment.prevEdgeIndex ^ 1;
				}
			}

#if UNITY_EDITOR
			if (voronoiNode)
			{
				_voronoiGraph.ValidateNodeEdgeRing(voronoiNode.index);
			}
#endif

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

			ClearOrderedLineSites();
		}

		private void ProcessEvent(VoronoiUtility.MergeEvent ev)
		{
			if (ev.isValid)
			{
				_sweep = ev.position.y;

				VoronoiUtility.BeachSegment prevSegment, nextSegment;
				CollapseBeachSegment(ev.segment, ev.mergePosition, out prevSegment, out nextSegment);

				ev.segment = null;

				CheckForMergeEvent(prevSegment);
				CheckForMergeEvent(nextSegment);
			}
		}

		private VoronoiUtility.BeachSegment SearchBeach(Vector2 position)
		{
			//TODO: Convert to a balanced binary tree search.

			VoronoiUtility.BeachSegment segment = _beach.head;
			var guard = new InfiniteLoopGuard(1000);
			while (segment.nextSegment != null)
			{
				var nextSegment = segment.nextSegment;

				if (segment.siteType == VoronoiSiteType.Line && nextSegment.siteType == VoronoiSiteType.Line && segment.siteIndex == nextSegment.siteIndex)
				{
					return segment;
				}

				float x = GetSegmentRightBound(segment);

				if (!float.IsNaN(x) && position.x - x < _errorMargin)
				{
					return segment;
				}

				segment = segment.nextSegment;
				guard.Iterate();
			}

			return segment;
		}

		private float GetSegmentLeftBound(VoronoiUtility.BeachSegment segment)
		{
			if (segment == null) return float.NegativeInfinity;
			return GetSegmentRightBound(segment.prevSegment, segment, _sweep);
		}

		private float GetSegmentRightBound(VoronoiUtility.BeachSegment segment)
		{
			if (segment == null) return float.PositiveInfinity;
			return GetSegmentRightBound(segment, segment.nextSegment, _sweep);
		}

		private float GetSegmentLeftBound(VoronoiUtility.BeachSegment segment, float sweep)
		{
			if (segment == null) return float.NegativeInfinity;
			return GetSegmentRightBound(segment.prevSegment, segment, sweep);
		}

		private float GetSegmentRightBound(VoronoiUtility.BeachSegment segment, float sweep)
		{
			if (segment == null) return float.PositiveInfinity;
			return GetSegmentRightBound(segment, segment.nextSegment, sweep);
		}

		private float GetSegmentRightBound(VoronoiUtility.BeachSegment prevSegment, VoronoiUtility.BeachSegment nextSegment, float sweep)
		{
			switch (prevSegment.siteType)
			{
				case VoronoiSiteType.Point:
					switch (nextSegment.siteType)
					{
						case VoronoiSiteType.Point:
							return VoronoiUtility.GetSegmentRightBound_PointPoint(
								_pointSitePositions[prevSegment.siteIndex],
								_pointSitePositions[nextSegment.siteIndex],
								sweep, _errorMargin);
						case VoronoiSiteType.Line:
							return VoronoiUtility.GetSegmentRightBound_PointLine(
								_pointSitePositions[prevSegment.siteIndex],
								_pointSitePositions[_siteGraph.GetEdgeTargetNodeIndex(nextSegment.siteIndex ^ 1)],
								_pointSitePositions[_siteGraph.GetEdgeTargetNodeIndex(nextSegment.siteIndex)],
								sweep, _errorMargin);
						case VoronoiSiteType.None:
							if (sweep >= _pointSitePositions[prevSegment.siteIndex].y + _errorMargin)
							{
								return float.PositiveInfinity;
							}
							else
							{
								return _pointSitePositions[prevSegment.siteIndex].x;
							}
						default: throw new NotImplementedException();
					}
				case VoronoiSiteType.Line:
					switch (nextSegment.siteType)
					{
						case VoronoiSiteType.Point:
							return VoronoiUtility.GetSegmentRightBound_PointLine(
								_pointSitePositions[nextSegment.siteIndex],
								_pointSitePositions[_siteGraph.GetEdgeTargetNodeIndex(prevSegment.siteIndex ^ 1)],
								_pointSitePositions[_siteGraph.GetEdgeTargetNodeIndex(prevSegment.siteIndex)],
								sweep, _errorMargin); //TODO: Reverse this?  Somehow get the other intersection.
						case VoronoiSiteType.Line:
							return VoronoiUtility.GetSegmentRightBound_LineLine(
								_pointSitePositions[_siteGraph.GetEdgeTargetNodeIndex(prevSegment.siteIndex ^ 1)],
								_pointSitePositions[_siteGraph.GetEdgeTargetNodeIndex(prevSegment.siteIndex)],
								_pointSitePositions[_siteGraph.GetEdgeTargetNodeIndex(nextSegment.siteIndex ^ 1)],
								_pointSitePositions[_siteGraph.GetEdgeTargetNodeIndex(nextSegment.siteIndex)],
								sweep, _errorMargin);
						case VoronoiSiteType.None:
							return float.PositiveInfinity;
						default: throw new NotImplementedException();
					}
				case VoronoiSiteType.None:
					switch (nextSegment.siteType)
					{
						case VoronoiSiteType.Point:
							if (sweep >= _pointSitePositions[nextSegment.siteIndex].y + _errorMargin)
							{
								return float.NegativeInfinity;
							}
							else
							{
								return _pointSitePositions[nextSegment.siteIndex].x;
							}
						case VoronoiSiteType.Line:
							return float.NegativeInfinity;
						case VoronoiSiteType.None:
						default: throw new NotImplementedException();
					}
				default: throw new NotImplementedException();
			}
		}

		private void DivideAdjacentSegments(VoronoiUtility.BeachSegment prevSegment)
		{
			int edgeIndex;
			_voronoiGraph.AddEdgePair(out edgeIndex);
			int twinIndex = edgeIndex ^ 1;
			var nextSegment = prevSegment.nextSegment;
			prevSegment.nextEdgeIndex = twinIndex;
			nextSegment.prevEdgeIndex = edgeIndex;

			_voronoiEdgeShapes.Add(VoronoiEdgeShape.FromVoronoiEdge(edgeIndex, _siteGraph, prevSegment.siteType, prevSegment.siteIndex, nextSegment.siteType, nextSegment.siteIndex, _originalPointSitePositions, Vector3.Cross(_up, _right)));
			_voronoiEdgeShapeLowerBounds.Add(float.NaN);
			_voronoiEdgeShapeUpperBounds.Add(float.NaN);

			_voronoiEdgeShapes.Add(_voronoiEdgeShapes[edgeIndex].reversed);
			_voronoiEdgeShapeLowerBounds.Add(float.NaN);
			_voronoiEdgeShapeUpperBounds.Add(float.NaN);
		}

		private void DivideSeparatedSegments(VoronoiUtility.BeachSegment prevSegment, VoronoiUtility.BeachSegment nextSegment)
		{
			if (VoronoiUtility.BeachSegment.SitesAreEqual(prevSegment, nextSegment))
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

		private void CheckForMergeEvent(VoronoiUtility.BeachSegment segment)
		{
			if (segment == null) return;

			segment.SetMergeEvent(null);

			if (segment.prevSegment == null || segment.nextSegment == null) return;

			if (segment.siteType == VoronoiSiteType.None)
			{
				segment.mergeEvent = _mergeEventQueue.Push(new Vector2(float.NegativeInfinity, _sweep), float.PositiveInfinity, segment);
				return;
			}

			if (segment.prevSegment.prevSegment == null)
			{
				segment.mergeEvent = _mergeEventQueue.Push(new Vector2(float.PositiveInfinity, float.PositiveInfinity), float.PositiveInfinity, segment);
				return;
			}

			if (ReferenceEquals(segment.prevSegment, segment.nextSegment)) return;
			if (segment.prevEdgeIndex == -1 || segment.nextEdgeIndex == -1) return;

			VoronoiUtility.MergeEvent ev = null;

			switch (segment.prevSegment.siteType)
			{
				case VoronoiSiteType.Point:
					switch (segment.siteType)
					{
						case VoronoiSiteType.Point:
							switch (segment.nextSegment.siteType)
							{
								case VoronoiSiteType.Point:
									ev = CheckForMergeEvent_PointPointPoint(segment);
									break;
								case VoronoiSiteType.Line:
									ev = CheckForMergeEvent_PointPointLine(segment);
									break;
							}
							break;
						case VoronoiSiteType.Line:
							switch (segment.nextSegment.siteType)
							{
								case VoronoiSiteType.Point:
									ev = CheckForMergeEvent_PointLinePoint(segment);
									break;
								case VoronoiSiteType.Line:
									ev = CheckForMergeEvent_PointLineLine(segment);
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
									ev = CheckForMergeEvent_LinePointPoint(segment);
									break;
								case VoronoiSiteType.Line:
									ev = CheckForMergeEvent_LinePointLine(segment);
									break;
							}
							break;
						case VoronoiSiteType.Line:
							switch (segment.nextSegment.siteType)
							{
								case VoronoiSiteType.Point:
									ev = CheckForMergeEvent_LineLinePoint(segment);
									break;
								case VoronoiSiteType.Line:
									ev = CheckForMergeEvent_LineLineLine(segment);
									break;
							}
							break;
					}
					break;
			}

			if (ev != null && ev.segment != null)
			{
				float prevLeftBound = GetSegmentLeftBound(ev.segment.prevSegment, ev.position.y);

				float nextRightBound = GetSegmentRightBound(ev.segment.nextSegment, ev.position.y);
				if (prevLeftBound < ev.position.x && nextRightBound > ev.position.x)
				{
					_mergeEventQueue.PushPrepared();
				}
			}
		}

		private VoronoiUtility.MergeEvent CheckForMergeEvent_PointPointPoint(VoronoiUtility.BeachSegment segment)
		{
			return VoronoiUtility.CheckForMergeEvent_PointPointPoint(segment.prevSegment.siteIndex, segment.siteIndex, segment.nextSegment.siteIndex, _pointSitePositions, _errorMargin, segment, _mergeEventQueue);
		}

		private VoronoiUtility.MergeEvent CheckForMergeEvent_PointPointLine(VoronoiUtility.BeachSegment segment)
		{
			int sourceNodeIndex = _siteGraph.GetEdgeTargetNodeIndex(segment.nextSegment.siteIndex ^ 1);
			int targetNodeIndex = _siteGraph.GetEdgeTargetNodeIndex(segment.nextSegment.siteIndex);

			return VoronoiUtility.CheckForMergeEvent_PointPointLine(segment.prevSegment.siteIndex, segment.siteIndex, sourceNodeIndex, targetNodeIndex, _pointSitePositions, _errorMargin, segment, _mergeEventQueue);
		}

		private VoronoiUtility.MergeEvent CheckForMergeEvent_PointLinePoint(VoronoiUtility.BeachSegment segment)
		{
			int sourceNodeIndex = _siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex ^ 1);
			int targetNodeIndex = _siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex);

			return VoronoiUtility.CheckForMergeEvent_PointLinePoint(segment.prevSegment.siteIndex, sourceNodeIndex, targetNodeIndex, segment.nextSegment.siteIndex, _pointSitePositions, _errorMargin, segment, _mergeEventQueue);
		}

		private VoronoiUtility.MergeEvent CheckForMergeEvent_LinePointPoint(VoronoiUtility.BeachSegment segment)
		{
			int sourceNodeIndex = _siteGraph.GetEdgeTargetNodeIndex(segment.prevSegment.siteIndex ^ 1);
			int targetNodeIndex = _siteGraph.GetEdgeTargetNodeIndex(segment.prevSegment.siteIndex);

			return VoronoiUtility.CheckForMergeEvent_LinePointPoint(sourceNodeIndex, targetNodeIndex, segment.siteIndex, segment.nextSegment.siteIndex, _pointSitePositions, _errorMargin, segment, _mergeEventQueue);
		}

		private VoronoiUtility.MergeEvent CheckForMergeEvent_PointLineLine(VoronoiUtility.BeachSegment segment)
		{
			int sourceNodeIndex1 = _siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex ^ 1);
			int targetNodeIndex1 = _siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex);
			int sourceNodeIndex2 = _siteGraph.GetEdgeTargetNodeIndex(segment.nextSegment.siteIndex ^ 1);
			int targetNodeIndex2 = _siteGraph.GetEdgeTargetNodeIndex(segment.nextSegment.siteIndex);

			return VoronoiUtility.CheckForMergeEvent_PointLineLine(segment.prevSegment.siteIndex, sourceNodeIndex1, targetNodeIndex1, sourceNodeIndex2, targetNodeIndex2, _pointSitePositions, _errorMargin, segment, _mergeEventQueue);
		}

		private VoronoiUtility.MergeEvent CheckForMergeEvent_LinePointLine(VoronoiUtility.BeachSegment segment)
		{
			int sourceNodeIndex0 = _siteGraph.GetEdgeTargetNodeIndex(segment.prevSegment.siteIndex ^ 1);
			int targetNodeIndex0 = _siteGraph.GetEdgeTargetNodeIndex(segment.prevSegment.siteIndex);
			int sourceNodeIndex2 = _siteGraph.GetEdgeTargetNodeIndex(segment.nextSegment.siteIndex ^ 1);
			int targetNodeIndex2 = _siteGraph.GetEdgeTargetNodeIndex(segment.nextSegment.siteIndex);

			return VoronoiUtility.CheckForMergeEvent_LinePointLine(sourceNodeIndex0, targetNodeIndex0, segment.siteIndex, sourceNodeIndex2, targetNodeIndex2, _pointSitePositions, _errorMargin, segment, _mergeEventQueue);
		}

		private VoronoiUtility.MergeEvent CheckForMergeEvent_LineLinePoint(VoronoiUtility.BeachSegment segment)
		{
			int sourceNodeIndex0 = _siteGraph.GetEdgeTargetNodeIndex(segment.prevSegment.siteIndex ^ 1);
			int targetNodeIndex0 = _siteGraph.GetEdgeTargetNodeIndex(segment.prevSegment.siteIndex);
			int sourceNodeIndex1 = _siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex ^ 1);
			int targetNodeIndex1 = _siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex);

			return VoronoiUtility.CheckForMergeEvent_LineLinePoint(sourceNodeIndex0, targetNodeIndex0, sourceNodeIndex1, targetNodeIndex1, segment.nextSegment.siteIndex, _pointSitePositions, _errorMargin, segment, _mergeEventQueue);
		}

		private VoronoiUtility.MergeEvent CheckForMergeEvent_LineLineLine(VoronoiUtility.BeachSegment segment)
		{
			int targetNodeIndex0 = _siteGraph.GetEdgeTargetNodeIndex(segment.prevSegment.siteIndex);
			int sourceNodeIndex0 = _siteGraph.GetEdgeTargetNodeIndex(segment.prevSegment.siteIndex ^ 1);
			int targetNodeIndex1 = _siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex);
			int sourceNodeIndex1 = _siteGraph.GetEdgeTargetNodeIndex(segment.siteIndex ^ 1);
			int targetNodeIndex2 = _siteGraph.GetEdgeTargetNodeIndex(segment.nextSegment.siteIndex);
			int sourceNodeIndex2 = _siteGraph.GetEdgeTargetNodeIndex(segment.nextSegment.siteIndex ^ 1);

			return VoronoiUtility.CheckForMergeEvent_LineLineLine(sourceNodeIndex0, targetNodeIndex0, sourceNodeIndex1, targetNodeIndex1, sourceNodeIndex2, targetNodeIndex2, _pointSitePositions, _errorMargin, segment, _mergeEventQueue);
		}

		private void CollapseBeachSegment(VoronoiUtility.BeachSegment segment, Vector2 position, out VoronoiUtility.BeachSegment prevSegment, out VoronoiUtility.BeachSegment nextSegment)
		{
			prevSegment = segment.prevSegment;
			nextSegment = segment.nextSegment;

			int edgeIndex0 = segment.nextEdgeIndex ^ 1;
			int edgeIndex1 = segment.prevEdgeIndex;
			int edgeIndex2;

			if (prevSegment.prevSegment == null && nextSegment.nextSegment != null && nextSegment.nextSegment.nextSegment == null)
			{
				_beach.Reset();
				edgeIndex2 = nextSegment.nextEdgeIndex ^ 1;
				prevSegment = nextSegment = null;
			}
			else
			{
				_beach.Remove(segment);
				DivideAdjacentSegments(prevSegment);
				edgeIndex2 = prevSegment.nextEdgeIndex;
			}

			int nodeIndex;
			_voronoiGraph.AddNode(out nodeIndex);
			_voronoiNodePositions.Add(position);

			AttachVoronoiEdgeToSourceNode(edgeIndex0, nodeIndex);
			AttachVoronoiEdgeToSourceNodeAfter(edgeIndex1, nodeIndex, edgeIndex0);
			AttachVoronoiEdgeToSourceNodeAfter(edgeIndex2, nodeIndex, edgeIndex1);

#if UNITY_EDITOR
			_voronoiGraph.ValidateNodeEdgeRing(nodeIndex);
#endif

			if (!float.IsNaN(position.x))
			{
				int neighborNodeIndex = _voronoiGraph.GetEdgeTargetNodeIndex(edgeIndex0);
				if (neighborNodeIndex != -1)
				{
					var neighborPosition = _voronoiNodePositions[neighborNodeIndex];
					if (!float.IsNaN(neighborPosition.x) && (neighborPosition - position).sqrMagnitude < _errorMargin)
					{
						MergeVoronoiNodes(edgeIndex0);
#if UNITY_EDITOR
						_voronoiGraph.ValidateNodeEdgeRing(neighborNodeIndex);
#endif
					}
				}

				neighborNodeIndex = _voronoiGraph.GetEdgeTargetNodeIndex(edgeIndex1);
				if (neighborNodeIndex != -1)
				{
					var neighborPosition = _voronoiNodePositions[neighborNodeIndex];
					if (!float.IsNaN(neighborPosition.x) && (neighborPosition - position).sqrMagnitude < _errorMargin)
					{
						MergeVoronoiNodes(edgeIndex1);
#if UNITY_EDITOR
						_voronoiGraph.ValidateNodeEdgeRing(neighborNodeIndex);
#endif
					}
				}
			}
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
			var segment = _beach.head.nextSegment;
			var guard = new InfiniteLoopGuard(1000);
			while (segment.nextSegment != null)
			{
				if (segment.prevEdgeIndex == fromEdgeIndex) segment.prevEdgeIndex = toEdgeIndex;
				if (segment.prevEdgeIndex == fromTwinIndex) segment.prevEdgeIndex = toTwinIndex;
				if (segment.nextEdgeIndex == fromEdgeIndex) segment.nextEdgeIndex = toEdgeIndex;
				if (segment.nextEdgeIndex == fromTwinIndex) segment.nextEdgeIndex = toTwinIndex;
				segment = segment.nextSegment;
				guard.Iterate();
			}

			_voronoiEdgeShapes[toEdgeIndex] = _voronoiEdgeShapes[fromEdgeIndex];
			_voronoiEdgeShapes[toTwinIndex] = _voronoiEdgeShapes[fromTwinIndex];
			_voronoiEdgeShapes.RemoveRange(fromEdgeIndex & ~1, 2);

			_voronoiEdgeShapeLowerBounds[toEdgeIndex] = _voronoiEdgeShapeLowerBounds[fromEdgeIndex];
			_voronoiEdgeShapeLowerBounds[toTwinIndex] = _voronoiEdgeShapeLowerBounds[fromTwinIndex];
			_voronoiEdgeShapeLowerBounds.RemoveRange(fromEdgeIndex & ~1, 2);

			_voronoiEdgeShapeUpperBounds[toEdgeIndex] = _voronoiEdgeShapeUpperBounds[fromEdgeIndex];
			_voronoiEdgeShapeUpperBounds[toTwinIndex] = _voronoiEdgeShapeUpperBounds[fromTwinIndex];
			_voronoiEdgeShapeUpperBounds.RemoveRange(fromEdgeIndex & ~1, 2);

			//_voronoiEdgeStuff[toEdgeIndex] = _voronoiEdgeStuff[fromEdgeIndex];
			//_voronoiEdgeStuff[toTwinIndex] = _voronoiEdgeStuff[fromTwinIndex];
			//_voronoiEdgeStuff.RemoveRange(fromEdgeIndex & ~1, 2);
		}

		private static void InsertOrdered(List<VoronoiUtility.DirectedEdge> list, VoronoiUtility.DirectedEdge item)
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

	public enum VoronoiEdgeShapeType
	{
		Infinite, // at the outer boundary of the voronoi diagram
		OrthogonalLine, // between line site and connected point site
		HalfAngleLine, // between two line sites
		ParallelLine, // between two parallel line sites
		PythagoreanLine, // between two point sites
		Parabola, // between line site and non-connected point site
	}

	public struct VoronoiEdgeShape
	{
		public float s;

		public Vector3 u;
		public Vector3 v;
		public Vector3 p;

		public VoronoiEdgeShapeType type;

		public VoronoiEdgeShape(VoronoiEdgeShapeType type)
		{
			this.type = type;

			u = Vector3.zero;
			v = Vector3.zero;
			p = Vector3.zero;
			s = 0f;
		}

		public VoronoiEdgeShape(VoronoiEdgeShapeType type, Vector3 v, Vector3 p)
		{
			this.type = type;

			u = Vector3.zero;
			this.v = v;
			this.p = p;
			s = 0f;
		}

		public VoronoiEdgeShape(VoronoiEdgeShapeType type, Vector3 v, Vector3 p, float s)
		{
			this.type = type;

			u = Vector3.zero;
			this.v = v;
			this.p = p;
			this.s = s;
		}

		public VoronoiEdgeShape(VoronoiEdgeShapeType type, Vector3 u, Vector3 v, Vector3 p, float s)
		{
			this.type = type;

			this.u = u;
			this.v = v;
			this.p = p;
			this.s = s;
		}

		public static VoronoiEdgeShape FromVoronoiEdge(int edgeIndex, IGraph siteGraph, VoronoiSiteType site0Type, int site0Index, VoronoiSiteType site1Type, int site1Index, IGraphNodeData<Vector3> pointSitePositions, Vector3 normal, float errorMargin = 0.0001f)
		{
			switch (site0Type)
			{
				case VoronoiSiteType.Point:
					{
						var p0 = pointSitePositions[site0Index];

						switch (site1Type)
						{
							case VoronoiSiteType.Point:
								{
									var p1 = pointSitePositions[site1Index];
									return FromPointPoint(p0, p1, normal);
								}
							case VoronoiSiteType.Line:
								{
									int site1aIndex = siteGraph.GetEdgeTargetNodeIndex(site1Index ^ 1);
									int site1bIndex = siteGraph.GetEdgeTargetNodeIndex(site1Index);
									var p1a = pointSitePositions[site1aIndex];
									var p1b = pointSitePositions[site1bIndex];

									if (site0Index == site1aIndex)
									{
										return FromSourcePointLine(p0, p1b - p0, normal);
									}
									else if(site0Index == site1bIndex)
									{
										return FromTargetPointLine(p0, p0 - p1a, normal);
									}
									else
									{
										return FromPointLine(p0, p1a, p1b - p1a, normal);
									}
								}
							case VoronoiSiteType.None:
								return new VoronoiEdgeShape(VoronoiEdgeShapeType.Infinite);
							default: throw new NotImplementedException();
						}
					}
				case VoronoiSiteType.Line:
					{
						int site0aIndex = siteGraph.GetEdgeTargetNodeIndex(site0Index ^ 1);
						int site0bIndex = siteGraph.GetEdgeTargetNodeIndex(site0Index);
						var p0a = pointSitePositions[site0aIndex];
						var p0b = pointSitePositions[site0bIndex];

						switch (site1Type)
						{
							case VoronoiSiteType.Point:
								{
									var p1 = pointSitePositions[site1Index];

									if (site0aIndex == site1Index)
									{
										return FromLineSourcePoint(p0b - p0a, p1, normal);
									}
									else if (site0bIndex == site1Index)
									{
										return FromLineTargetPoint(p0b - p0a, p1, normal);
									}
									else
									{
										return FromLinePoint(p0a, p0b - p0a, p1, normal);
									}
								}
							case VoronoiSiteType.Line:
								{
									if (site0Index == (site1Index ^ 1))
									{
										return FromTwinLines(p0a, p0b - p0a);
									}
									else
									{
										int site1aIndex = siteGraph.GetEdgeTargetNodeIndex(site1Index ^ 1);
										int site1bIndex = siteGraph.GetEdgeTargetNodeIndex(site1Index);
										var p1a = pointSitePositions[site1aIndex];
										var p1b = pointSitePositions[site1bIndex];

										return FromLineLine(p0a, p0b - p0a, p1a, p1b - p1a, normal, errorMargin);
									}
								}
							case VoronoiSiteType.None:
								return new VoronoiEdgeShape(VoronoiEdgeShapeType.Infinite);
							default: throw new NotImplementedException();
						}
					}
				case VoronoiSiteType.None:
					return new VoronoiEdgeShape(VoronoiEdgeShapeType.Infinite);
				default: throw new NotImplementedException();
			}
		}

		public static VoronoiEdgeShape FromPointPoint(Vector3 p0, Vector3 p1, Vector3 normal)
		{
			var u = p1 - p0;
			var v = Vector3.Cross(normal, u).normalized;
			var p = (p1 + p0) / 2f;
			var s = u.sqrMagnitude / 4f;
			return new VoronoiEdgeShape(VoronoiEdgeShapeType.PythagoreanLine, v, p, s);
		}

		public static VoronoiEdgeShape FromPointLine(Vector3 p0, Vector3 p1, Vector3 v1, Vector3 normal)
		{
			var q = p0.ProjectOnto(p1, v1);
			var u = (p0 - q) * 2f;
			var v = Vector3.Cross(u, normal);
			var p = (p0 + q) / 2f;
			var s = u.magnitude / 4f;
			return new VoronoiEdgeShape(VoronoiEdgeShapeType.Parabola, u, v, p, s);
		}

		public static VoronoiEdgeShape FromSourcePointLine(Vector3 p0, Vector3 v1, Vector3 normal)
		{
			var v = Vector3.Cross(normal, v1).normalized;
			return new VoronoiEdgeShape(VoronoiEdgeShapeType.OrthogonalLine, v, p0, -1f);
		}

		public static VoronoiEdgeShape FromTargetPointLine(Vector3 p0, Vector3 v1, Vector3 normal)
		{
			var v = Vector3.Cross(v1, normal).normalized;
			return new VoronoiEdgeShape(VoronoiEdgeShapeType.OrthogonalLine, v, p0, +1f);
		}

		public static VoronoiEdgeShape FromLinePoint(Vector3 p0, Vector3 v0, Vector3 p1, Vector3 normal)
		{
			var q = p1.ProjectOnto(p0, v0);
			var u = (p1 - q) * 2f;
			var v = Vector3.Cross(normal, u);
			var p = (p1 + q) / 2f;
			var s = u.magnitude / 4f;
			return new VoronoiEdgeShape(VoronoiEdgeShapeType.Parabola, u, v, p, s);
		}

		public static VoronoiEdgeShape FromLineSourcePoint(Vector3 v0, Vector3 p1, Vector3 normal)
		{
			var v = Vector3.Cross(v0, normal).normalized;
			return new VoronoiEdgeShape(VoronoiEdgeShapeType.OrthogonalLine, v, p1, +1f);
		}

		public static VoronoiEdgeShape FromLineTargetPoint(Vector3 v0, Vector3 p1, Vector3 normal)
		{
			var v = Vector3.Cross(normal, v0).normalized;
			return new VoronoiEdgeShape(VoronoiEdgeShapeType.OrthogonalLine, v, p1, -1f);
		}

		public static VoronoiEdgeShape FromLineLine(Vector3 p0, Vector3 v0, Vector3 p1, Vector3 v1, Vector3 normal, float errorMargin = 0.0001f)
		{
			var ray0 = new Ray(p0, v0);
			var ray1 = new Ray(p1, v1);

			var cosine = Vector2.Dot(ray0.direction, ray1.direction);

			if (1f - cosine < errorMargin)
			{
				// Parallel Lines, Same Direction
				var p = (p1 + p0) / 2f;
				var s = Math.ZeroExclusiveSign(Vector3.Dot(p0 - p1, ray1.direction));
				var v = Vector3.Cross(ray1.direction, normal) * s;

				return new VoronoiEdgeShape(VoronoiEdgeShapeType.OrthogonalLine, v, p, s);
			}
			else if (1f + cosine < errorMargin)
			{
				// Parallel Lines, Opposite Directions
				var p = (p1 + p0) / 2f;
				var v = ray0.direction;
				var delta = p1 - p0;
				var lineNormal = Vector3.Cross(v, normal);
				float normalSqrMagnitude = lineNormal.sqrMagnitude;
				float s = (normalSqrMagnitude >= errorMargin) ? Vector3.Dot(delta, lineNormal) / (2f * Mathf.Sqrt(normalSqrMagnitude)) : 0f; // Check for colinearity.

				return new VoronoiEdgeShape(VoronoiEdgeShapeType.ParallelLine, v, p, s);
			}
			else
			{
				var p = Geometry.GetNearestPoint(ray0, ray1);
				var v = (ray0.direction - ray1.direction).normalized;
				var shiftedCosine = 1f - cosine;
				var sine = Vector3.Dot(Vector3.Cross(ray0.direction, ray1.direction), normal);
				var s = Mathf.Sqrt(sine * sine + shiftedCosine * shiftedCosine) / sine;

				return new VoronoiEdgeShape(VoronoiEdgeShapeType.HalfAngleLine, v, p, s);
			}
		}

		public static VoronoiEdgeShape FromTwinLines(Vector3 p0, Vector3 v0)
		{
			var p = p0 + v0 / 2f;
			var v = -v0.normalized;
			return new VoronoiEdgeShape(VoronoiEdgeShapeType.ParallelLine, v, p, 0f);
		}

		public bool isStraight { get { return type != VoronoiEdgeShapeType.Infinite && type != VoronoiEdgeShapeType.Parabola; } }
		public bool isCurved { get { return type == VoronoiEdgeShapeType.Parabola; } }

		public bool isFinite { get { return type != VoronoiEdgeShapeType.Infinite; } }
		public bool isInfinite { get { return type == VoronoiEdgeShapeType.Infinite; } }

		public VoronoiEdgeShape reversed
		{
			get
			{
				switch (type)
				{
					case VoronoiEdgeShapeType.Infinite:
						return this;
					case VoronoiEdgeShapeType.OrthogonalLine:
					case VoronoiEdgeShapeType.HalfAngleLine:
						return new VoronoiEdgeShape(type, -v, p, -s);
					case VoronoiEdgeShapeType.ParallelLine:
						return new VoronoiEdgeShape(type, -v, p, s);
					case VoronoiEdgeShapeType.PythagoreanLine:
						return new VoronoiEdgeShape(type, -v, p, s);
					case VoronoiEdgeShapeType.Parabola:
						return new VoronoiEdgeShape(type, u, -v, p, s);
					default:
						throw new NotImplementedException();
				}
			}
		}

		public VoronoiEdgeShape Transform(Vector3 origin, Vector3 right, Vector3 up)
		{
			//TODO
			return new VoronoiEdgeShape(type, u, v, p, s);
		}

		public Vector3 Evaluate(float t)
		{
			switch (type)
			{
				case VoronoiEdgeShapeType.Infinite:
					return new Vector3(float.NaN, float.NaN, float.NaN);
				case VoronoiEdgeShapeType.OrthogonalLine:
				case VoronoiEdgeShapeType.HalfAngleLine:
				case VoronoiEdgeShapeType.ParallelLine:
				case VoronoiEdgeShapeType.PythagoreanLine:
					return v * t + p;
				case VoronoiEdgeShapeType.Parabola:
					return (u * t + v) * t + p;
				default:
					throw new NotImplementedException();
			}
		}

		public float GetDistance(float t)
		{
			switch (type)
			{
				case VoronoiEdgeShapeType.Infinite:
					return float.PositiveInfinity;
				case VoronoiEdgeShapeType.OrthogonalLine:
				case VoronoiEdgeShapeType.HalfAngleLine:
					return t / s;
				case VoronoiEdgeShapeType.ParallelLine:
					return s;
				case VoronoiEdgeShapeType.PythagoreanLine:
					return Mathf.Sqrt(t * t + s);
				case VoronoiEdgeShapeType.Parabola:
					return (t * t * 4f + 1f) * s;
				default:
					throw new NotImplementedException();
			}
		}

		public Vector3 GetDirection(float t)
		{
			switch (type)
			{
				case VoronoiEdgeShapeType.Infinite:
					return new Vector3(float.NaN, float.NaN, float.NaN);
				case VoronoiEdgeShapeType.OrthogonalLine:
				case VoronoiEdgeShapeType.HalfAngleLine:
				case VoronoiEdgeShapeType.ParallelLine:
				case VoronoiEdgeShapeType.PythagoreanLine:
					return v;
				case VoronoiEdgeShapeType.Parabola:
					return (2f * u * t + v).normalized;
				default:
					throw new NotImplementedException();
			}
		}

		public float GetAngleChange(float t0, float t1)
		{
			switch (type)
			{
				case VoronoiEdgeShapeType.Infinite:
					return float.NaN;
				case VoronoiEdgeShapeType.OrthogonalLine:
				case VoronoiEdgeShapeType.HalfAngleLine:
				case VoronoiEdgeShapeType.ParallelLine:
				case VoronoiEdgeShapeType.PythagoreanLine:
					return 0f;
				case VoronoiEdgeShapeType.Parabola:
					return Mathf.Atan(2f * t1) - Mathf.Atan(2f * t0);
				default:
					throw new NotImplementedException();
			}
		}

		public float GetAngleChangeOffset(float t0, float angle)
		{
			switch (type)
			{
				case VoronoiEdgeShapeType.Infinite:
					return float.NaN;
				case VoronoiEdgeShapeType.OrthogonalLine:
				case VoronoiEdgeShapeType.HalfAngleLine:
				case VoronoiEdgeShapeType.ParallelLine:
				case VoronoiEdgeShapeType.PythagoreanLine:
					return (angle == 0f) ? t0 : (angle > 0f) ? float.PositiveInfinity : float.NegativeInfinity;
				case VoronoiEdgeShapeType.Parabola:
					return Mathf.Tan(angle + Mathf.Atan(2f * t0)) / 2f;
				default:
					throw new NotImplementedException();
			}
		}

		public float GetCurvature(float t, Vector3 normal)
		{
			switch (type)
			{
				case VoronoiEdgeShapeType.Infinite:
					return float.NaN;
				case VoronoiEdgeShapeType.OrthogonalLine:
				case VoronoiEdgeShapeType.HalfAngleLine:
				case VoronoiEdgeShapeType.ParallelLine:
				case VoronoiEdgeShapeType.PythagoreanLine:
					return 0f;
				case VoronoiEdgeShapeType.Parabola:
					{
						float hSquared = t * t + 1f;
						return Math.ZeroExclusiveSign(Vector3.Dot(Vector3.Cross(v, u), normal)) / (2f * s * hSquared * Mathf.Sqrt(hSquared));
					}
				default:
					throw new NotImplementedException();
			}
		}

		public float GetArcLength(float t0, float t1)
		{
			switch (type)
			{
				case VoronoiEdgeShapeType.Infinite:
					return float.NaN;
				case VoronoiEdgeShapeType.OrthogonalLine:
				case VoronoiEdgeShapeType.HalfAngleLine:
				case VoronoiEdgeShapeType.ParallelLine:
				case VoronoiEdgeShapeType.PythagoreanLine:
					return t1 - t0;
				case VoronoiEdgeShapeType.Parabola:
					{
						float q0 = Mathf.Sqrt(s * s + t0 * t0 / 4f);
						float q1 = Mathf.Sqrt(s * s + t1 * t1 / 4f);
						return (t1 * q1 - t0 * q0) / (2f * s) + s * Mathf.Log((t1 / 2f + q1) / (t0 / 2f + q0));
					}
				default:
					throw new NotImplementedException();
			}
		}

		public float GetDistanceDerivative(float t)
		{
			switch (type)
			{
				case VoronoiEdgeShapeType.Infinite:
					return float.NaN;
				case VoronoiEdgeShapeType.OrthogonalLine:
					return s;
				case VoronoiEdgeShapeType.HalfAngleLine:
					return 1f / s;
				case VoronoiEdgeShapeType.ParallelLine:
					return 0f;
				case VoronoiEdgeShapeType.PythagoreanLine:
					return t / Mathf.Sqrt(s + t * t);
				case VoronoiEdgeShapeType.Parabola:
					return 8f * s * t;
				default:
					throw new NotImplementedException();
			}
		}

		public float GetDistanceSecondDerivative(float t)
		{
			switch (type)
			{
				case VoronoiEdgeShapeType.Infinite:
					return float.NaN;
				case VoronoiEdgeShapeType.OrthogonalLine:
				case VoronoiEdgeShapeType.HalfAngleLine:
				case VoronoiEdgeShapeType.ParallelLine:
					return 0f;
				case VoronoiEdgeShapeType.PythagoreanLine:
					{
						var hSquared = s + t * t;
						return s / (hSquared * Mathf.Sqrt(hSquared));
					}
				case VoronoiEdgeShapeType.Parabola:
					return 8f * s;
				default:
					throw new NotImplementedException();
			}
		}

		public float GetNearest(float t0 = float.NegativeInfinity, float t1 = float.PositiveInfinity)
		{
			switch (type)
			{
				case VoronoiEdgeShapeType.Infinite:
					return float.NaN;
				case VoronoiEdgeShapeType.OrthogonalLine:
				case VoronoiEdgeShapeType.HalfAngleLine:
					return s > 0f ? t0 : t1;
				case VoronoiEdgeShapeType.ParallelLine:
					return t0;
				case VoronoiEdgeShapeType.PythagoreanLine:
				case VoronoiEdgeShapeType.Parabola:
					return (t0 <= 0f) ? (t1 >= 0f) ? 0f : t1 : t0;
				default:
					throw new NotImplementedException();
			}
		}

		public float GetNearestDistance(float t0 = float.NegativeInfinity, float t1 = float.PositiveInfinity)
		{
			switch (type)
			{
				case VoronoiEdgeShapeType.Infinite:
					return float.PositiveInfinity;
				case VoronoiEdgeShapeType.OrthogonalLine:
				case VoronoiEdgeShapeType.HalfAngleLine:
					return s > 0f ? t0 / s : t1 / s;
				case VoronoiEdgeShapeType.ParallelLine:
					return s;
				case VoronoiEdgeShapeType.PythagoreanLine:
					if (t0 <= 0f && t1 >= 0f)
					{
						return Mathf.Sqrt(s);
					}
					else
					{
						float t = Mathf.Min(Mathf.Abs(t0), Mathf.Abs(t1));
						return Mathf.Sqrt(t * t + s);
					}
				case VoronoiEdgeShapeType.Parabola:
					if (t0 <= 0f && t1 >= 0f)
					{
						return s;
					}
					else
					{
						float t = Mathf.Min(Mathf.Abs(t0), Mathf.Abs(t1));
						return (t * t * 4f + 1f) * s;
					}
				default:
					throw new NotImplementedException();
			}
		}

		public void GetNearest(out float t, out float distance, float t0 = float.NegativeInfinity, float t1 = float.PositiveInfinity)
		{
			switch (type)
			{
				case VoronoiEdgeShapeType.Infinite:
					t = float.NaN;
					distance = float.PositiveInfinity;
					break;
				case VoronoiEdgeShapeType.OrthogonalLine:
				case VoronoiEdgeShapeType.HalfAngleLine:
					if (s > 0f)
					{
						t = t0;
						distance = t0 / s;
					}
					else
					{
						t = t1;
						distance = t1 / s;
					}
					break;
				case VoronoiEdgeShapeType.ParallelLine:
					t = t0;
					distance = s;
					break;
				case VoronoiEdgeShapeType.PythagoreanLine:
					if (t0 <= 0f)
					{
						if (t1 >= 0f)
						{
							t = 0f;
							distance = Mathf.Sqrt(s);
						}
						else if (t0 >= t1)
						{
							t = t0;
							distance = Mathf.Sqrt(t0 * t0 + s);
						}
						else
						{
							t = t1;
							distance = Mathf.Sqrt(t1 * t1 + s);
						}
					}
					else if (t0 <= t1)
					{
						t = t0;
						distance = Mathf.Sqrt(t0 * t0 + s);
					}
					else
					{
						t = t1;
						distance = Mathf.Sqrt(t1 * t1 + s);
					}
					break;
				case VoronoiEdgeShapeType.Parabola:
					if (t0 <= 0f)
					{
						if (t1 >= 0f)
						{
							t = 0f;
							distance = s;
						}
						else if (t0 >= t1)
						{
							t = t0;
							distance = (t0 * t0 * 4f + 1f) * s;
						}
						else
						{
							t = t1;
							distance = (t1 * t1 * 4f + 1f) * s;
						}
					}
					else if (t0 <= t1)
					{
						t = t0;
						distance = (t0 * t0 * 4f + 1f) * s;
					}
					else
					{
						t = t1;
						distance = (t1 * t1 * 4f + 1f) * s;
					}
					break;
				default:
					throw new NotImplementedException();
			}
		}

		public float GetFarthest(float t0 = float.NegativeInfinity, float t1 = float.PositiveInfinity)
		{
			switch (type)
			{
				case VoronoiEdgeShapeType.Infinite:
					return float.NaN;
				case VoronoiEdgeShapeType.OrthogonalLine:
				case VoronoiEdgeShapeType.HalfAngleLine:
					return s > 0f ? t1 : t0;
				case VoronoiEdgeShapeType.ParallelLine:
					return t1;
				case VoronoiEdgeShapeType.PythagoreanLine:
				case VoronoiEdgeShapeType.Parabola:
					return (Mathf.Abs(t0) > Mathf.Abs(t1)) ? t0 : t1;
				default:
					throw new NotImplementedException();
			}
		}

		public float GetFarthestDistance(float t0 = float.NegativeInfinity, float t1 = float.PositiveInfinity)
		{
			switch (type)
			{
				case VoronoiEdgeShapeType.Infinite:
					return float.PositiveInfinity;
				case VoronoiEdgeShapeType.OrthogonalLine:
				case VoronoiEdgeShapeType.HalfAngleLine:
					return s > 0f ? t1 / s : t0 / s;
				case VoronoiEdgeShapeType.ParallelLine:
					return s;
				case VoronoiEdgeShapeType.PythagoreanLine:
					{
						float t = Mathf.Max(Mathf.Abs(t0), Mathf.Abs(t1));
						return Mathf.Sqrt(t * t + s);
					}
				case VoronoiEdgeShapeType.Parabola:
					{
						float t = Mathf.Max(Mathf.Abs(t0), Mathf.Abs(t1));
						return (t * t * 4f + 1f) * s;
					}
				default:
					throw new NotImplementedException();
			}
		}

		public void GetFarthest(out float t, out float distance, float t0 = float.NegativeInfinity, float t1 = float.PositiveInfinity)
		{
			switch (type)
			{
				case VoronoiEdgeShapeType.Infinite:
					t = float.NaN;
					distance = float.PositiveInfinity;
					break;
				case VoronoiEdgeShapeType.OrthogonalLine:
				case VoronoiEdgeShapeType.HalfAngleLine:
					if (s <= 0f)
					{
						t = t0;
						distance = t0 / s;
					}
					else
					{
						t = t1;
						distance = t1 / s;
					}
					break;
				case VoronoiEdgeShapeType.ParallelLine:
					t = t1;
					distance = s;
					break;
				case VoronoiEdgeShapeType.PythagoreanLine:
					if (Mathf.Abs(t0) > Mathf.Abs(t1))
					{
						t = t0;
						distance = Mathf.Sqrt(t0 * t0 + s);
					}
					else
					{
						t = t1;
						distance = Mathf.Sqrt(t1 * t1 + s);
					}
					break;
				case VoronoiEdgeShapeType.Parabola:
					if (Mathf.Abs(t0) > Mathf.Abs(t1))
					{
						t = t0;
						distance = (t0 * t0 * 4f + 1f) * s;
					}
					else
					{
						t = t1;
						distance = (t1 * t1 * 4f + 1f) * s;
					}
					break;
				default:
					throw new NotImplementedException();
			}
		}

		private static float Project(Vector3 p, Vector3 v, Vector3 point)
		{
			return Vector3.Dot(point - p, v) / Vector3.Dot(v, v);
		}

		private static float ProjectTowardNegativeInfinity(Vector3 p, Vector3 v, Vector3 point)
		{
			var t = Vector3.Dot(point - p, v) / Vector3.Dot(v, v);
			return float.IsNaN(t) ? float.NegativeInfinity : t;
		}

		private static float ProjectTowardPositiveInfinity(Vector3 p, Vector3 v, Vector3 point)
		{
			var t = Vector3.Dot(point - p, v) / Vector3.Dot(v, v);
			return float.IsNaN(t) ? float.PositiveInfinity : t;
		}

		public float Project(Vector3 point)
		{
			return Vector3.Dot(point - p, v) / Vector3.Dot(v, v);
		}

#if false
		public bool Intersect(float distance, out float t0, out float t1)
		{
			switch (type)
			{
				case VoronoiEdgeShapeType.Infinite:
					t0 = t1 = float.NaN;
					return false;
				case VoronoiEdgeShapeType.OrthogonalLine:
				case VoronoiEdgeShapeType.HalfAngleLine:
					{
						float t = distance * s;
						if (t >= this.t0 && t <= this.t1)
						{
							if (s > 0f)
							{
								t0 = t;
								t1 = float.NaN;
							}
							else
							{
								t0 = float.NaN;
								t1 = t;
							}
							return true;
						}
						else
						{
							t0 = t1 = float.NaN;
							return false;
						}
					}
				case VoronoiEdgeShapeType.ParallelLine:
					if (distance == s)
					{
						t0 = this.t0;
						t1 = this.t1;
						return true;
					}
					else
					{
						t0 = t1 = float.NaN;
						return false;
					}
				case VoronoiEdgeShapeType.PythagoreanLine:
					{
						float distanceSquared = distance * distance;
						if (distanceSquared >= s)
						{
							float t = Mathf.Sqrt(distanceSquared - s);
							t0 = (-t >= this.t0) ? -t : float.NaN;
							t1 = (t <= this.t1) ? t : float.NaN;
							return !float.IsNaN(t0) || !float.IsNaN(t1);
						}
						else
						{
							t0 = t1 = float.NaN;
							return false;
						}
					}
				case VoronoiEdgeShapeType.Parabola:
					{
						float distanceOverS = distance / s;
						if (distanceOverS >= 1f)
						{
							float t = Mathf.Sqrt(distanceOverS - 1f) / 2f;
							t0 = (-t >= this.t0) ? -t : float.NaN;
							t1 = (t <= this.t1) ? t : float.NaN;
							return !float.IsNaN(t0) || !float.IsNaN(t1);
						}
						else
						{
							t0 = t1 = float.NaN;
							return false;
						}
					}
				default:
					throw new NotImplementedException();
			}
		}
#endif

		public float IntersectEntranceUnclamped(float distance)
		{
			switch (type)
			{
				case VoronoiEdgeShapeType.Infinite:
					break;
				case VoronoiEdgeShapeType.OrthogonalLine:
				case VoronoiEdgeShapeType.HalfAngleLine:
					if (s > 0f)
					{
						return distance * s;
					}
					break;
				case VoronoiEdgeShapeType.ParallelLine:
					if (distance == s)
					{
						return float.NegativeInfinity;
					}
					break;
				case VoronoiEdgeShapeType.PythagoreanLine:
					{
						float distanceSquared = distance * distance;
						if (distanceSquared >= s)
						{
							return Mathf.Sqrt(distanceSquared - s);
						}
						break;
					}
				case VoronoiEdgeShapeType.Parabola:
					{
						float distanceOverS = distance / s;
						if (distanceOverS >= 1f)
						{
							return Mathf.Sqrt(distanceOverS - 1f) / 2f;
						}
					}
					break;
				default:
					throw new NotImplementedException();
			}

			return float.NaN;
		}

		public float IntersectExitUnclamped(float distance)
		{
			switch (type)
			{
				case VoronoiEdgeShapeType.Infinite:
					break;
				case VoronoiEdgeShapeType.OrthogonalLine:
				case VoronoiEdgeShapeType.HalfAngleLine:
					if (s < 0f)
					{
						return distance * s;
					}
					break;
				case VoronoiEdgeShapeType.ParallelLine:
					if (distance == s)
					{
						return float.PositiveInfinity;
					}
					break;
				case VoronoiEdgeShapeType.PythagoreanLine:
					{
						float distanceSquared = distance * distance;
						if (distanceSquared >= s)
						{
							return -Mathf.Sqrt(distanceSquared - s);
						}
						break;
					}
				case VoronoiEdgeShapeType.Parabola:
					{
						float distanceOverS = distance / s;
						if (distanceOverS >= 1f)
						{
							return -Mathf.Sqrt(distanceOverS - 1f) / 2f;
						}
					}
					break;
				default:
					throw new NotImplementedException();
			}

			return float.NaN;
		}
	}

	public class VoronoiDiagram
	{
		public readonly IGraph _siteGraph;
		public readonly IGraphNodeData<Vector3> _pointSitePositions;
		public readonly ITopology _voronoiTopology;
		public readonly IGraphNodeData<int> _siteNodeFirstVoronoiEdgeIndices;
		public readonly IGraphEdgeData<int> _siteEdgeFirstVoronoiEdgeIndices;
		public readonly ITopologyNodeData<Vector3> _voronoiNodePositions;
		public readonly ITopologyFaceData<VoronoiSiteType> _voronoiFaceSiteTypes;
		public readonly ITopologyFaceData<int> _voronoiFaceSiteIndices;
		public readonly ITopologyEdgeData<VoronoiEdgeShape> _voronoiEdgeShapes;
		public readonly ITopologyEdgeData<float> _voronoiEdgeShapeLowerBounds;
		public readonly ITopologyEdgeData<float> _voronoiEdgeShapeUpperBounds;

		public VoronoiDiagram(IGraph siteGraph, IGraphNodeData<Vector3> pointSitePositions, ITopology voronoiTopology, IGraphNodeData<int> siteNodeFirstVoronoiEdgeIndices, IGraphEdgeData<int> siteEdgeFirstVoronoiEdgeIndices, ITopologyNodeData<Vector3> voronoiNodePositions, ITopologyFaceData<VoronoiSiteType> voronoiFaceSiteTypes, ITopologyFaceData<int> voronoiFaceSiteIndices, ITopologyEdgeData<VoronoiEdgeShape> voronoiEdgeShapes, ITopologyEdgeData<float> voronoiEdgeShapeLowerBounds, ITopologyEdgeData<float> voronoiEdgeShapeUpperBounds)
		{
			_siteGraph = siteGraph;
			_pointSitePositions = pointSitePositions;
			_voronoiTopology = voronoiTopology;
			_siteNodeFirstVoronoiEdgeIndices = siteNodeFirstVoronoiEdgeIndices;
			_siteEdgeFirstVoronoiEdgeIndices = siteEdgeFirstVoronoiEdgeIndices;
			_voronoiNodePositions = voronoiNodePositions;
			_voronoiFaceSiteTypes = voronoiFaceSiteTypes;
			_voronoiFaceSiteIndices = voronoiFaceSiteIndices;
			_voronoiEdgeShapes = voronoiEdgeShapes;
			_voronoiEdgeShapeLowerBounds = voronoiEdgeShapeLowerBounds;
			_voronoiEdgeShapeUpperBounds = voronoiEdgeShapeUpperBounds;
		}

		public List<ContourDescriptor> FindAllContourLoops(float distance, List<ContourDescriptor> loops = null, float errorMargin = 0.0001f)
		{
			if (loops == null)
			{
				loops = new List<ContourDescriptor>();
			}
			else
			{
				loops.Clear();
			}

			TopologyEdgeDataArray<int> edgeLoopIndices = new TopologyEdgeDataArray<int>(_voronoiTopology.edgeCount);

			for (int i = 0; i < _voronoiTopology.edgeCount; ++i)
			{
				edgeLoopIndices[i] = -1;
			}

			for (int i = 0; i < _voronoiTopology.edgeCount; ++i)
			{
				if (edgeLoopIndices[i] != -1) continue;

				var edgeShape = _voronoiEdgeShapes[i];
				float t0 = _voronoiEdgeShapeLowerBounds[i];
				float t1 = _voronoiEdgeShapeUpperBounds[i];

				var t = edgeShape.IntersectEntranceUnclamped(distance);

				if (Math.ApproximateGreaterOrEqual(t, t0, errorMargin) &&
					Math.ApproximateLessOrEqual(t, t1, errorMargin))
				{
					if (Math.ApproximateGreaterThan(t, t0, errorMargin))
					{
						if (Math.ApproximateLessThan(t, t1, errorMargin))
						{
							// Contour enters in middle of edge.
							loops.Add(BuildContourLoop(distance, new TopologyFaceEdge(_voronoiTopology, i), t, loops.Count, edgeLoopIndices, errorMargin));
						}
						else
						{
							// Contour enters right at end of edge.
							if (edgeShape.GetDistanceDerivative(t) > errorMargin || edgeShape.GetDistanceSecondDerivative(t) > errorMargin)
							{
								// Either distance 1st deriv or 2nd deriv is positive.
								loops.Add(BuildContourLoop(distance, new TopologyFaceEdge(_voronoiTopology, i), t, loops.Count, edgeLoopIndices, errorMargin));
							}
						}
					}
					else
					{
						if (Math.ApproximateLessThan(t, t1, errorMargin))
						{
							// Contour enters right at start of edge.
							loops.Add(BuildContourLoop(distance, new TopologyFaceEdge(_voronoiTopology, i), t, loops.Count, edgeLoopIndices, errorMargin));
						}
						else
						{
							// Contour enters right at both start and end of edge.
							throw new NotImplementedException();
						}
					}
				}
			}

			return loops;
		}

		private ContourDescriptor BuildContourLoop(float distance, TopologyFaceEdge firstEdge, float tFirst, int loopIndex, TopologyEdgeDataArray<int> edgeLoopIndices, float errorMargin = 0.0001f)
		{
			var edge = firstEdge;
			float tEntrance = tFirst;
			float tExit;

			var entranceEdgeIndices = new List<int>();
			var entranceParameters = new List<float>();

			var edgeShape = _voronoiEdgeShapes[edge];

			int outer = 0;

			do
			{
				if (++outer == 900) { edge = firstEdge; edgeShape = _voronoiEdgeShapes[edge]; tEntrance = tFirst; }
				if (outer > 900) Debug.LogFormat("firstEdge {0}, edge {1}, tFirst {2:F4}, tEntrance {3:F4}", firstEdge.index, edge.index, tFirst, tEntrance);
				if (outer > 1000) throw new InvalidOperationException();
				edgeLoopIndices[edge] = loopIndex;
				entranceEdgeIndices.Add(edge.index);
				entranceParameters.Add(tEntrance);

				tExit = edgeShape.IntersectExitUnclamped(distance);

				int inner = 0;

				while (float.IsNaN(tExit) || Math.ApproximateLessThan(tExit, tEntrance, errorMargin) || Math.ApproximateGreaterThan(tExit, _voronoiEdgeShapeUpperBounds[edge], errorMargin))
				{
					if (++inner > 1000) throw new InvalidOperationException();

					edge = edge.next;
					edgeShape = _voronoiEdgeShapes[edge];
					tEntrance = _voronoiEdgeShapeLowerBounds[edge];
					tExit = edgeShape.IntersectExitUnclamped(distance);
					if (outer > 900) Debug.LogFormat("nextEdge {0}, tEntrance {1:F4}, tExit {2:F4}", edge.index, tEntrance, tExit);
				}

				edge = edge.twin;
				edgeShape = _voronoiEdgeShapes[edge];
				tEntrance = -tExit;
				edgeLoopIndices[edge] = loopIndex;
			} while (edge != firstEdge || Math.ApproximateLessThan(tEntrance, tFirst, errorMargin));

			return new ContourDescriptor(distance, entranceEdgeIndices, entranceParameters, true);
		}
	}
}
