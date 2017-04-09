/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace Experilous.Topologies
{
	public class VoronoiTracer
	{
		private VoronoiDiagram _voronoiDiagram;
		private readonly List<Trace> _traces = new List<Trace>();
		private readonly List<Trace> _tracePool = new List<Trace>();
		private TraceData[] _traceData;

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
		}

		private struct TraceData
		{
			public Trace.State state;
			public float distance;
		}

		public void BeginTracing(VoronoiDiagram voronoiDiagram, TopologyEdge startingEdge, params float[] distances)
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

		public Trace GetTrace(int index)
		{
			return _traces[index];
		}

		public bool ContinueTrace()
		{
			throw new System.NotImplementedException();
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
