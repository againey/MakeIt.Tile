/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

namespace Experilous.Topologies
{
	public class FixedSizeGraph : IGraph
	{
		protected int[] _nodeNeighborCounts;
		protected int[] _nodeFirstEdgeIndices;
		protected int[] _edgeNextChainedEdgeIndices;
		protected int[] _edgeNextLateralEdgeIndices;
		protected int[] _edgeTargetNodeIndices;

		protected FixedSizeGraph()
		{
		}

		public FixedSizeGraph(int nodeCount, int edgeCount)
		{
			_nodeNeighborCounts = new int[nodeCount];
			_nodeFirstEdgeIndices = new int[nodeCount];
			_edgeNextChainedEdgeIndices = new int[edgeCount];
			_edgeNextLateralEdgeIndices = new int[edgeCount];
			_edgeTargetNodeIndices = new int[edgeCount];

			for (int i = 0; i < nodeCount; ++i)
			{
				_nodeNeighborCounts[i] = 0;
				_nodeFirstEdgeIndices[i] = -1;
			}

			for (int i = 0; i < edgeCount; ++i)
			{
				_edgeNextChainedEdgeIndices[i] = -1;
				_edgeNextLateralEdgeIndices[i] = -1;
				_edgeTargetNodeIndices[i] = -1;
			}
		}

		public FixedSizeGraph(int[] nodeNeighborCounts, int[] nodeFirstEdgeIndices, int[] edgeNextChainedEdgeIndices, int[] edgeNextLateralEdgeIndices, int[] edgeTargetNodeIndices)
		{
			_nodeNeighborCounts = nodeNeighborCounts;
			_nodeFirstEdgeIndices = nodeFirstEdgeIndices;
			_edgeNextChainedEdgeIndices = edgeNextChainedEdgeIndices;
			_edgeNextLateralEdgeIndices = edgeNextLateralEdgeIndices;
			_edgeTargetNodeIndices = edgeTargetNodeIndices;

			UnityEngine.Debug.Assert(_nodeNeighborCounts.Length == _nodeFirstEdgeIndices.Length);
			UnityEngine.Debug.Assert(_edgeNextChainedEdgeIndices.Length == _edgeNextLateralEdgeIndices.Length);
			UnityEngine.Debug.Assert(_edgeNextChainedEdgeIndices.Length == _edgeTargetNodeIndices.Length);
		}

		public FixedSizeGraph(IGraph graph)
		{
			_nodeNeighborCounts = new int[graph.nodeCount];
			_nodeFirstEdgeIndices = new int[graph.nodeCount];
			_edgeNextChainedEdgeIndices = new int[graph.edgeCount];
			_edgeNextLateralEdgeIndices = new int[graph.edgeCount];
			_edgeTargetNodeIndices = new int[graph.edgeCount];

			for (int i = 0; i < graph.nodeCount; ++i)
			{
				_nodeNeighborCounts[i] = graph.GetNodeNeighborCount(i);
				_nodeFirstEdgeIndices[i] = graph.GetNodeFirstEdgeIndex(i);
			}

			for (int i = 0; i < graph.edgeCount; ++i)
			{
				_edgeNextChainedEdgeIndices[i] = graph.GetEdgeNextChainedEdgeIndex(i);
				_edgeNextLateralEdgeIndices[i] = graph.GetEdgeNextLateralEdgeIndex(i);
				_edgeTargetNodeIndices[i] = graph.GetEdgeTargetNodeIndex(i);
			}
		}

		public virtual object Clone()
		{
			var clone = new FixedSizeGraph();
			clone._nodeNeighborCounts = _nodeNeighborCounts.Clone() as int[];
			clone._nodeFirstEdgeIndices = _nodeFirstEdgeIndices.Clone() as int[];
			clone._edgeNextChainedEdgeIndices = _edgeNextChainedEdgeIndices.Clone() as int[];
			clone._edgeNextLateralEdgeIndices = _edgeNextLateralEdgeIndices.Clone() as int[];
			clone._edgeTargetNodeIndices = _edgeTargetNodeIndices.Clone() as int[];
			return clone;
		}

		public int nodeCount
		{
			get
			{
				return _nodeFirstEdgeIndices.Length;
			}
		}

		public int edgeCount
		{
			get
			{
				return _edgeNextChainedEdgeIndices.Length;
			}
		}

		public GraphNode.NodesIndexer nodes
		{
			get
			{
				return new GraphNode.NodesIndexer(this);
			}
		}

		public GraphEdge.EdgesIndexer edges
		{
			get
			{
				return new GraphEdge.EdgesIndexer(this);
			}
		}

		public int GetNodeNeighborCount(int nodeIndex)
		{
			if (nodeIndex == -1) return 0;
			return _nodeNeighborCounts[nodeIndex];
		}

		public int GetNodeFirstEdgeIndex(int nodeIndex)
		{
			if (nodeIndex == -1) return -1;
			return _nodeFirstEdgeIndices[nodeIndex];
		}

		public int GetEdgeNextChainedEdgeIndex(int edgeIndex)
		{
			if (edgeIndex == -1) return -1;
			return _edgeNextChainedEdgeIndices[edgeIndex];
		}

		public int GetEdgeNextLateralEdgeIndex(int edgeIndex)
		{
			if (edgeIndex == -1) return -1;
			return _edgeNextLateralEdgeIndices[edgeIndex];
		}

		public int GetEdgeTargetNodeIndex(int edgeIndex)
		{
			if (edgeIndex == -1) return -1;
			return _edgeTargetNodeIndices[edgeIndex];
		}
	}
}
