/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System;
using System.Collections.Generic;

namespace Experilous.Topologies
{
	public class DynamicGraph : IGraph
	{
		protected List<int> _nodeNeighborCounts;
		protected List<int> _nodeFirstEdgeIndices;
		protected List<int> _edgeNextChainedEdgeIndices;
		protected List<int> _edgeNextLateralEdgeIndices;
		protected List<int> _edgeTargetNodeIndices;

		#region Graph Creation

		public DynamicGraph()
		{
			_nodeNeighborCounts = new List<int>();
			_nodeFirstEdgeIndices = new List<int>();
			_edgeNextChainedEdgeIndices = new List<int>();
			_edgeNextLateralEdgeIndices = new List<int>();
			_edgeTargetNodeIndices = new List<int>();
		}

		public DynamicGraph(int nodeCount, int edgeCount)
		{
			_nodeNeighborCounts = new List<int>(nodeCount);
			_nodeFirstEdgeIndices = new List<int>(nodeCount);
			_edgeNextChainedEdgeIndices = new List<int>(edgeCount);
			_edgeNextLateralEdgeIndices = new List<int>(edgeCount);
			_edgeTargetNodeIndices = new List<int>(edgeCount);

			for (int i = 0; i < nodeCount; ++i)
			{
				_nodeNeighborCounts.Add(0);
				_nodeFirstEdgeIndices.Add(-1);
			}

			for (int i = 0; i < edgeCount; ++i)
			{
				_edgeNextChainedEdgeIndices.Add(-1);
				_edgeNextLateralEdgeIndices.Add(-1);
				_edgeTargetNodeIndices.Add(-1);
			}
		}

		public DynamicGraph(IGraph graph)
		{
			_nodeNeighborCounts = new List<int>(graph.nodeCount);
			_nodeFirstEdgeIndices = new List<int>(graph.nodeCount);
			_edgeNextChainedEdgeIndices = new List<int>(graph.edgeCount);
			_edgeNextLateralEdgeIndices = new List<int>(graph.edgeCount);
			_edgeTargetNodeIndices = new List<int>(graph.edgeCount);

			for (int i = 0; i < graph.nodeCount; ++i)
			{
				_nodeNeighborCounts.Add(graph.GetNodeNeighborCount(i));
				_nodeFirstEdgeIndices.Add(graph.GetNodeFirstEdgeIndex(i));
			}

			for (int i = 0; i < graph.edgeCount; ++i)
			{
				_edgeNextChainedEdgeIndices.Add(graph.GetEdgeNextChainedEdgeIndex(i));
				_edgeNextLateralEdgeIndices.Add(graph.GetEdgeNextLateralEdgeIndex(i));
				_edgeTargetNodeIndices.Add(graph.GetEdgeTargetNodeIndex(i));
			}
		}

		public object Clone()
		{
			var clone = new DynamicGraph();
			clone._nodeNeighborCounts = new List<int>(_nodeNeighborCounts);
			clone._nodeFirstEdgeIndices = new List<int>(_nodeFirstEdgeIndices);
			clone._edgeNextChainedEdgeIndices = new List<int>(_edgeNextChainedEdgeIndices);
			clone._edgeNextLateralEdgeIndices = new List<int>(_edgeNextLateralEdgeIndices);
			clone._edgeTargetNodeIndices = new List<int>(_edgeTargetNodeIndices);
			return clone;
		}

		#endregion

		#region IGraph Implementation

		public int nodeCount
		{
			get
			{
				return _nodeFirstEdgeIndices.Count;
			}
		}

		public int edgeCount
		{
			get
			{
				return _edgeNextChainedEdgeIndices.Count;
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

		#endregion

		#region Graph Modification

		public void AddNode(out int nodeIndex)
		{
			nodeIndex = _nodeNeighborCounts.Count;
			_nodeNeighborCounts.Add(0);
			_nodeFirstEdgeIndices.Add(-1);
		}

		public GraphNode AddNode()
		{
			int nodeIndex = _nodeNeighborCounts.Count;
			_nodeNeighborCounts.Add(0);
			_nodeFirstEdgeIndices.Add(-1);
			return new GraphNode(this, nodeIndex);
		}

		public void RemoveNode(int nodeIndex, Action<int, int> remap = null)
		{
			if (nodeIndex < 0 || nodeIndex >= _nodeNeighborCounts.Count) throw new ArgumentOutOfRangeException("nodeIndex");

			DetachNodeFromAllEdges(nodeIndex);

			int lastNodeIndex = _nodeNeighborCounts.Count - 1;

			for (int i = 0; i < _edgeTargetNodeIndices.Count; ++i)
			{
				if (_edgeTargetNodeIndices[i] == lastNodeIndex) _edgeTargetNodeIndices[i] = nodeIndex;
			}

			_nodeNeighborCounts[nodeIndex] = _nodeNeighborCounts[lastNodeIndex];
			_nodeFirstEdgeIndices[nodeIndex] = _nodeFirstEdgeIndices[lastNodeIndex];

			_nodeNeighborCounts.RemoveAt(lastNodeIndex);
			_nodeFirstEdgeIndices.RemoveAt(lastNodeIndex);

			if (remap != null) remap(nodeIndex, lastNodeIndex);
		}

		public void Remove(GraphNode node)
		{
			if (!ReferenceEquals(node.graph, this)) throw new ArgumentException("The provided node does not belong to this graph.", "node");
			RemoveNode(node.index);
		}

		public int ConnectNodes(int firstNodeIndex, int secondNodeIndex)
		{
			if (firstNodeIndex < 0 || firstNodeIndex >= _nodeNeighborCounts.Count) throw new ArgumentOutOfRangeException("firstNodeIndex");
			if (secondNodeIndex < 0 || secondNodeIndex >= _nodeNeighborCounts.Count) throw new ArgumentOutOfRangeException("secondNodeIndex");

			int edgeIndex;
			AddEdgePair(out edgeIndex);
			AttachEdgeToSourceNode(edgeIndex, firstNodeIndex);
			AttachEdgeToSourceNode(edgeIndex ^ 1, secondNodeIndex);
			return edgeIndex;
		}

		public GraphEdge Connect(GraphNode firstNode, GraphNode secondNode)
		{
			if (!ReferenceEquals(firstNode.graph, this)) throw new ArgumentException("The provided node does not belong to this graph.", "firstNode");
			if (!ReferenceEquals(secondNode.graph, this)) throw new ArgumentException("The provided node does not belong to this graph.", "secondNode");
			return new GraphEdge(this, ConnectNodes(firstNode.index, secondNode.index));
		}

		public void AddEdgePair(out int edgeIndex)
		{
			edgeIndex = _edgeNextChainedEdgeIndices.Count;
			_edgeNextChainedEdgeIndices.Add(-1);
			_edgeNextChainedEdgeIndices.Add(-1);
			_edgeNextLateralEdgeIndices.Add(-1);
			_edgeNextLateralEdgeIndices.Add(-1);
			_edgeTargetNodeIndices.Add(-1);
			_edgeTargetNodeIndices.Add(-1);
		}

		public GraphEdge AddEdgePair()
		{
			int edgeIndex;
			AddEdgePair(out edgeIndex);
			return new GraphEdge(this, edgeIndex);
		}

		public void AttachEdgeToSourceNode(int edgeIndex, int nodeIndex)
		{
			if (edgeIndex < 0 || edgeIndex >= _edgeNextChainedEdgeIndices.Count) throw new ArgumentOutOfRangeException("edgeIndex");
			if (nodeIndex < 0 || nodeIndex >= _nodeNeighborCounts.Count) throw new ArgumentOutOfRangeException("nodeIndex");

			DetachEdgeFromSourceNode(edgeIndex);

			int twinEdgeIndex = edgeIndex ^ 1;
			int nextEdgeIndex = _nodeFirstEdgeIndices[nodeIndex];
			if (nextEdgeIndex == -1)
			{
				_nodeFirstEdgeIndices[nodeIndex] = edgeIndex;
				_edgeNextLateralEdgeIndices[edgeIndex] = edgeIndex;
				_edgeNextChainedEdgeIndices[twinEdgeIndex] = edgeIndex;
			}
			else
			{
				int prevEdgeIndex = _edgeNextChainedEdgeIndices[nextEdgeIndex ^ 1];

				_edgeNextLateralEdgeIndices[edgeIndex] = nextEdgeIndex;
				_edgeNextChainedEdgeIndices[twinEdgeIndex] = prevEdgeIndex;

				_edgeNextLateralEdgeIndices[prevEdgeIndex] = edgeIndex;
				_edgeNextChainedEdgeIndices[nextEdgeIndex ^ 1] = edgeIndex;
			}

			_edgeTargetNodeIndices[twinEdgeIndex] = nodeIndex;

			_nodeNeighborCounts[nodeIndex] += 1;

			if (_nodeFirstEdgeIndices[nodeIndex] == -1)
			{
				_nodeFirstEdgeIndices[nodeIndex] = edgeIndex;
			}
		}

		public void AttachToSource(GraphEdge edge, GraphNode node)
		{
			if (!ReferenceEquals(edge.graph, this)) throw new ArgumentException("The provided edge does not belong to this graph.", "edge");
			if (!ReferenceEquals(node.graph, this)) throw new ArgumentException("The provided node does not belong to this graph.", "node");
			AttachEdgeToSourceNode(edge.index, node.index);
		}

		public void AttachEdgeToSourceNodeBefore(int edgeIndex, int nextEdgeIndex)
		{
			if (edgeIndex < 0 || edgeIndex >= _edgeNextChainedEdgeIndices.Count) throw new ArgumentOutOfRangeException("edgeIndex");
			if (nextEdgeIndex < 0 || nextEdgeIndex >= _edgeNextChainedEdgeIndices.Count) throw new ArgumentOutOfRangeException("nextEdgeIndex");

			DetachEdgeFromSourceNode(edgeIndex);

			int twinEdgeIndex = edgeIndex ^ 1;
			int prevEdgeIndex = _edgeNextChainedEdgeIndices[nextEdgeIndex ^ 1];

			_edgeNextLateralEdgeIndices[edgeIndex] = nextEdgeIndex;
			_edgeNextChainedEdgeIndices[twinEdgeIndex] = prevEdgeIndex;

			_edgeNextLateralEdgeIndices[prevEdgeIndex] = edgeIndex;
			_edgeNextChainedEdgeIndices[nextEdgeIndex ^ 1] = edgeIndex;

			int nodeIndex = _edgeTargetNodeIndices[nextEdgeIndex ^ 1];
			_edgeTargetNodeIndices[twinEdgeIndex] = nodeIndex;

			_nodeNeighborCounts[nodeIndex] += 1;
		}

		public void AttachBefore(GraphEdge edge, GraphEdge nextEdge)
		{
			if (!ReferenceEquals(edge.graph, this)) throw new ArgumentException("The provided edge does not belong to this graph.", "edge");
			if (!ReferenceEquals(nextEdge.graph, this)) throw new ArgumentException("The provided edge does not belong to this graph.", "nextEdge");
			AttachEdgeToSourceNodeBefore(edge.index, nextEdge.index);
		}

		public void AttachEdgeToSourceNodeAfter(int edgeIndex, int prevEdgeIndex)
		{
			if (edgeIndex < 0 || edgeIndex >= _edgeNextChainedEdgeIndices.Count) throw new ArgumentOutOfRangeException("edgeIndex");
			if (prevEdgeIndex < 0 || prevEdgeIndex >= _edgeNextChainedEdgeIndices.Count) throw new ArgumentOutOfRangeException("prevEdgeIndex");

			DetachEdgeFromSourceNode(edgeIndex);

			int twinEdgeIndex = edgeIndex ^ 1;
			int nextEdgeIndex = _edgeNextLateralEdgeIndices[prevEdgeIndex];

			_edgeNextLateralEdgeIndices[edgeIndex] = nextEdgeIndex;
			_edgeNextChainedEdgeIndices[twinEdgeIndex] = prevEdgeIndex;

			_edgeNextLateralEdgeIndices[prevEdgeIndex] = edgeIndex;
			_edgeNextChainedEdgeIndices[nextEdgeIndex ^ 1] = edgeIndex;

			int nodeIndex = _edgeTargetNodeIndices[prevEdgeIndex ^ 1];
			_edgeTargetNodeIndices[twinEdgeIndex] = nodeIndex;

			_nodeNeighborCounts[nodeIndex] += 1;
		}

		public void AttachAfter(GraphEdge edge, GraphEdge prevEdge)
		{
			if (!ReferenceEquals(edge.graph, this)) throw new ArgumentException("The provided edge does not belong to this graph.", "edge");
			if (!ReferenceEquals(prevEdge.graph, this)) throw new ArgumentException("The provided edge does not belong to this graph.", "prevEdge");
			AttachEdgeToSourceNodeAfter(edge.index, prevEdge.index);
		}

		public void RemoveEdge(int edgeIndex, Action<int, int, int, int> remap = null)
		{
			if (edgeIndex < 0 || edgeIndex >= _edgeNextChainedEdgeIndices.Count) throw new ArgumentOutOfRangeException("edgeIndex");

			int twinIndex = edgeIndex ^ 1;

			DetachEdgeFromSourceNode(edgeIndex);
			DetachEdgeFromSourceNode(twinIndex);

			int lastEdgeIndex = (_edgeNextChainedEdgeIndices.Count - 2) | (edgeIndex & 1);
			int lastTwinIndex = lastEdgeIndex ^ 1;

			for (int i = 0; i < _nodeFirstEdgeIndices.Count; ++i)
			{
				if (_nodeFirstEdgeIndices[i] == lastEdgeIndex) _nodeFirstEdgeIndices[i] = edgeIndex;
				else if (_nodeFirstEdgeIndices[i] == lastTwinIndex) _nodeFirstEdgeIndices[i] = twinIndex;
			}

			for (int i = 0; i < _edgeNextChainedEdgeIndices.Count; ++i)
			{
				if (_edgeNextChainedEdgeIndices[i] == lastEdgeIndex) _edgeNextChainedEdgeIndices[i] = edgeIndex;
				else if (_edgeNextChainedEdgeIndices[i] == lastTwinIndex) _edgeNextChainedEdgeIndices[i] = twinIndex;

				if (_edgeNextLateralEdgeIndices[i] == lastEdgeIndex) _edgeNextLateralEdgeIndices[i] = edgeIndex;
				else if (_edgeNextLateralEdgeIndices[i] == lastTwinIndex) _edgeNextLateralEdgeIndices[i] = twinIndex;
			}

			_edgeNextChainedEdgeIndices[edgeIndex] = _edgeNextChainedEdgeIndices[lastEdgeIndex];
			_edgeNextChainedEdgeIndices[twinIndex] = _edgeNextChainedEdgeIndices[lastTwinIndex];

			_edgeNextLateralEdgeIndices[edgeIndex] = _edgeNextLateralEdgeIndices[lastEdgeIndex];
			_edgeNextLateralEdgeIndices[twinIndex] = _edgeNextLateralEdgeIndices[lastTwinIndex];

			_edgeTargetNodeIndices[edgeIndex] = _edgeTargetNodeIndices[lastEdgeIndex];
			_edgeTargetNodeIndices[twinIndex] = _edgeTargetNodeIndices[lastTwinIndex];

			lastEdgeIndex = lastEdgeIndex & ~1;
			_edgeNextChainedEdgeIndices.RemoveRange(lastEdgeIndex, 2);
			_edgeNextLateralEdgeIndices.RemoveRange(lastEdgeIndex, 2);
			_edgeTargetNodeIndices.RemoveRange(lastEdgeIndex, 2);

			if (remap != null) remap(edgeIndex, twinIndex, lastEdgeIndex, lastTwinIndex);
		}

		public void Remove(GraphEdge edge)
		{
			if (!ReferenceEquals(edge.graph, this)) throw new ArgumentException("The provided edge does not belong to this graph.", "edge");
			RemoveEdge(edge.index);
		}

		public void DetachEdgeFromSourceNode(int edgeIndex)
		{
			if (edgeIndex < 0 || edgeIndex >= _edgeNextChainedEdgeIndices.Count) throw new ArgumentOutOfRangeException("edgeIndex");

			int twinEdgeIndex = edgeIndex ^ 1;
			int nodeIndex = _edgeTargetNodeIndices[twinEdgeIndex];
			if (nodeIndex == -1) return;

			_edgeTargetNodeIndices[twinEdgeIndex] = -1;

			int prevEdgeIndex = _edgeNextChainedEdgeIndices[twinEdgeIndex];
			int nextEdgeIndex = _edgeNextLateralEdgeIndices[edgeIndex];

			_edgeNextChainedEdgeIndices[twinEdgeIndex] = -1;
			_edgeNextLateralEdgeIndices[edgeIndex] = -1;

			if (prevEdgeIndex != edgeIndex)
			{
				_edgeNextChainedEdgeIndices[nextEdgeIndex ^ 1] = prevEdgeIndex;
				_edgeNextLateralEdgeIndices[prevEdgeIndex] = nextEdgeIndex;

				if (_nodeFirstEdgeIndices[nodeIndex] == edgeIndex)
				{
					_nodeFirstEdgeIndices[nodeIndex] = nextEdgeIndex;
				}

				_nodeNeighborCounts[nodeIndex] -= 1;
			}
			else
			{
				_nodeNeighborCounts[nodeIndex] = 0;
				_nodeFirstEdgeIndices[nodeIndex] = -1;
			}
		}

		public void DetachFromSourceNode(GraphEdge edge)
		{
			if (!ReferenceEquals(edge.graph, this)) throw new ArgumentException("The provided edge does not belong to this graph.", "edge");
			DetachEdgeFromSourceNode(edge.index);
		}

		public void DetachNodeFromAllEdges(int nodeIndex)
		{
			if (nodeIndex < 0 || nodeIndex >= _nodeNeighborCounts.Count) throw new ArgumentOutOfRangeException("nodeIndex");

			int firstEdgeIndex = _nodeFirstEdgeIndices[nodeIndex];
			int edgeIndex = firstEdgeIndex;
			while (edgeIndex != firstEdgeIndex)
			{
				int nextEdgeIndex = _edgeNextLateralEdgeIndices[edgeIndex];

				_edgeNextChainedEdgeIndices[edgeIndex ^ 1] = -1;
				_edgeNextLateralEdgeIndices[edgeIndex] = -1;

				edgeIndex = nextEdgeIndex;
			}

			_nodeNeighborCounts[nodeIndex] = 0;
			_nodeFirstEdgeIndices[nodeIndex] = -1;
		}

		public void DetachFromAllEdges(GraphNode node)
		{
			if (!ReferenceEquals(node.graph, this)) throw new ArgumentException("The provided node does not belong to this graph.", "node");
			DetachNodeFromAllEdges(node.index);
		}

		public void DetachAllEdges()
		{
			for (int i = 0; i < _nodeNeighborCounts.Count; ++i)
			{
				_nodeNeighborCounts[i] = 0;
				_nodeFirstEdgeIndices[i] = -1;
			}

			for (int i = 0; i < _edgeNextChainedEdgeIndices.Count; ++i)
			{
				_edgeNextChainedEdgeIndices[i] = -1;
				_edgeNextLateralEdgeIndices[i] = -1;
				_edgeTargetNodeIndices[i] = -1;
			}
		}

		public void CollapseEdge(int edgeIndex, Action<int, int> remapNode = null, Action<int, int, int, int> remapEdges = null)
		{
			int sourceNodeIndex = _edgeTargetNodeIndices[edgeIndex ^ 1];
			int targetNodeIndex = _edgeTargetNodeIndices[edgeIndex];

			int retargetingEdgeIndex = _edgeNextLateralEdgeIndices[edgeIndex];
			while (retargetingEdgeIndex != edgeIndex)
			{
				_edgeTargetNodeIndices[retargetingEdgeIndex] = targetNodeIndex;
				retargetingEdgeIndex = _edgeNextLateralEdgeIndices[retargetingEdgeIndex];
			}

			int twinIndex = edgeIndex ^ 1;

			_edgeNextChainedEdgeIndices[_edgeNextLateralEdgeIndices[edgeIndex] ^ 1] = _edgeNextChainedEdgeIndices[edgeIndex];
			_edgeNextChainedEdgeIndices[_edgeNextLateralEdgeIndices[twinIndex] ^ 1] = _edgeNextChainedEdgeIndices[twinIndex];

			_edgeNextLateralEdgeIndices[_edgeNextChainedEdgeIndices[edgeIndex]] = _edgeNextLateralEdgeIndices[edgeIndex];
			_edgeNextLateralEdgeIndices[_edgeNextChainedEdgeIndices[twinIndex]] = _edgeNextLateralEdgeIndices[twinIndex];

			RemoveEdge(edgeIndex, remapEdges);
			RemoveNode(sourceNodeIndex, remapNode);
		}

		public void Collapse(GraphEdge edge, Action<int, int> remapNode = null, Action<int, int, int, int> remapEdges = null)
		{
			if (!ReferenceEquals(edge.graph, this)) throw new ArgumentException("The provided edge does not belong to this graph.", "edge");
			CollapseEdge(edge.index, remapNode, remapEdges);
		}

		public void ShiftEdgeForwardAroundSourceNode(int edgeIndex)
		{
			if (edgeIndex < 0 || edgeIndex >= _edgeNextChainedEdgeIndices.Count) throw new ArgumentOutOfRangeException("edgeIndex");

			int nextEdgeIndex = _edgeNextLateralEdgeIndices[edgeIndex];
			if (nextEdgeIndex == -1) throw new ArgumentException("The provided edge is not attached to a source node.", "edgeIndex");
			if (nextEdgeIndex == edgeIndex) return;

			int prevEdgeIndex = _edgeNextChainedEdgeIndices[edgeIndex ^ 1];

			_edgeNextChainedEdgeIndices[nextEdgeIndex ^ 1] = prevEdgeIndex;
			_edgeNextLateralEdgeIndices[prevEdgeIndex] = nextEdgeIndex;

			prevEdgeIndex = nextEdgeIndex;
			nextEdgeIndex = _edgeNextLateralEdgeIndices[nextEdgeIndex];

			_edgeNextChainedEdgeIndices[nextEdgeIndex ^ 1] = edgeIndex;
			_edgeNextLateralEdgeIndices[prevEdgeIndex] = edgeIndex;

			_edgeNextChainedEdgeIndices[edgeIndex ^ 1] = prevEdgeIndex;
			_edgeNextLateralEdgeIndices[edgeIndex] = nextEdgeIndex;
		}

		public void ShiftEdgeForwardAroundSourceNode(GraphEdge edge)
		{
			if (!ReferenceEquals(edge.graph, this)) throw new ArgumentException("The provided edge does not belong to this graph.", "edge");
			ShiftEdgeForwardAroundSourceNode(edge.index);
		}

		public void ShiftEdgeBackwardAroundSourceNode(int edgeIndex)
		{
			if (edgeIndex < 0 || edgeIndex >= _edgeNextChainedEdgeIndices.Count) throw new ArgumentOutOfRangeException("edgeIndex");

			int nextEdgeIndex = _edgeNextLateralEdgeIndices[edgeIndex];
			if (nextEdgeIndex == -1) throw new ArgumentException("The provided edge is not attached to a source node.", "edgeIndex");
			if (nextEdgeIndex == edgeIndex) return;

			int prevEdgeIndex = _edgeNextChainedEdgeIndices[edgeIndex ^ 1];

			_edgeNextChainedEdgeIndices[nextEdgeIndex ^ 1] = prevEdgeIndex;
			_edgeNextLateralEdgeIndices[prevEdgeIndex] = nextEdgeIndex;

			nextEdgeIndex = prevEdgeIndex;
			prevEdgeIndex = _edgeNextChainedEdgeIndices[prevEdgeIndex ^ 1];

			_edgeNextChainedEdgeIndices[nextEdgeIndex ^ 1] = edgeIndex;
			_edgeNextLateralEdgeIndices[prevEdgeIndex] = edgeIndex;

			_edgeNextChainedEdgeIndices[edgeIndex ^ 1] = prevEdgeIndex;
			_edgeNextLateralEdgeIndices[edgeIndex] = nextEdgeIndex;
		}

		public void ShiftEdgeBackwardAroundSourceNode(GraphEdge edge)
		{
			if (!ReferenceEquals(edge.graph, this)) throw new ArgumentException("The provided edge does not belong to this graph.", "edge");
			ShiftEdgeBackwardAroundSourceNode(edge.index);
		}

		public void Clear()
		{
			_nodeNeighborCounts.Clear();
			_nodeFirstEdgeIndices.Clear();
			_edgeNextChainedEdgeIndices.Clear();
			_edgeNextLateralEdgeIndices.Clear();
			_edgeTargetNodeIndices.Clear();
		}

		public void ClearEdges()
		{
			for (int i = 0; i < _nodeNeighborCounts.Count; ++i)
			{
				_nodeNeighborCounts[i] = 0;
				_nodeFirstEdgeIndices[i] = -1;
			}

			_edgeNextChainedEdgeIndices.Clear();
			_edgeNextLateralEdgeIndices.Clear();
			_edgeTargetNodeIndices.Clear();
		}

		#endregion

		#region Graph Data Copy

		public int[] GetNodeNeighborCounts()
		{
			return _nodeNeighborCounts.ToArray();
		}

		public int[] GetNodeFirstEdgeIndices()
		{
			return _nodeFirstEdgeIndices.ToArray();
		}

		public int[] GetEdgeNextChainedEdgeIndices()
		{
			return _edgeNextChainedEdgeIndices.ToArray();
		}

		public int[] GetEdgeNextLateralEdgeIndices()
		{
			return _edgeNextLateralEdgeIndices.ToArray();
		}

		public int[] GetEdgeTargetNodeIndices()
		{
			return _edgeTargetNodeIndices.ToArray();
		}

		#endregion
	}
}
