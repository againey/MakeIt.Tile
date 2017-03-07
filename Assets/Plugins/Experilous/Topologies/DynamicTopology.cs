/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System.Collections.Generic;

namespace Experilous.Topologies
{
	public class DynamicTopology : ITopology
	{
		protected List<int> _nodeNeighborCounts;
		protected List<int> _nodeFirstEdgeIndices;
		protected List<int> _faceNeighborCounts;
		protected List<int> _faceFirstEdgeIndices;
		protected List<int> _edgeNextChainedEdgeIndices;
		protected List<int> _edgeNextLateralEdgeIndices;
		protected List<int> _edgeTargetNodeIndices;
		protected List<int> _edgeTargetFaceIndices;
		protected List<EdgeWrap> _edgeWrapData;

		#region Topology Creation

		public DynamicTopology()
		{
			_nodeNeighborCounts = new List<int>();
			_nodeFirstEdgeIndices = new List<int>();
			_faceNeighborCounts = new List<int>();
			_faceFirstEdgeIndices = new List<int>();
			_edgeNextChainedEdgeIndices = new List<int>();
			_edgeNextLateralEdgeIndices = new List<int>();
			_edgeTargetNodeIndices = new List<int>();
			_edgeTargetFaceIndices = new List<int>();
			_edgeWrapData = new List<EdgeWrap>();
		}

		public DynamicTopology(int nodeCount, int faceCount, int edgeCount)
		{
			_nodeNeighborCounts = new List<int>(nodeCount);
			_nodeFirstEdgeIndices = new List<int>(nodeCount);
			_faceNeighborCounts = new List<int>(faceCount);
			_faceFirstEdgeIndices = new List<int>(faceCount);
			_edgeNextChainedEdgeIndices = new List<int>(edgeCount);
			_edgeNextLateralEdgeIndices = new List<int>(edgeCount);
			_edgeTargetNodeIndices = new List<int>(edgeCount);
			_edgeTargetFaceIndices = new List<int>(edgeCount);
			_edgeWrapData = new List<EdgeWrap>(edgeCount);

			for (int i = 0; i < nodeCount; ++i)
			{
				_nodeNeighborCounts.Add(0);
				_nodeFirstEdgeIndices.Add(-1);
			}

			for (int i = 0; i < faceCount; ++i)
			{
				_faceNeighborCounts.Add(0);
				_faceFirstEdgeIndices.Add(-1);
			}

			for (int i = 0; i < edgeCount; ++i)
			{
				_edgeNextChainedEdgeIndices.Add(-1);
				_edgeNextLateralEdgeIndices.Add(-1);
				_edgeTargetNodeIndices.Add(-1);
				_edgeTargetFaceIndices.Add(-1);
				_edgeWrapData.Add(EdgeWrap.None);
			}
		}

		public DynamicTopology(ITopology topology)
		{
			_nodeNeighborCounts = new List<int>(topology.nodeCount);
			_nodeFirstEdgeIndices = new List<int>(topology.nodeCount);
			_faceNeighborCounts = new List<int>(topology.faceCount);
			_faceFirstEdgeIndices = new List<int>(topology.faceCount);
			_edgeNextChainedEdgeIndices = new List<int>(topology.edgeCount);
			_edgeNextLateralEdgeIndices = new List<int>(topology.edgeCount);
			_edgeTargetNodeIndices = new List<int>(topology.edgeCount);
			_edgeTargetFaceIndices = new List<int>(topology.edgeCount);
			_edgeWrapData = new List<EdgeWrap>(topology.edgeCount);

			for (int i = 0; i < topology.nodeCount; ++i)
			{
				_nodeNeighborCounts.Add(topology.GetNodeNeighborCount(i));
				_nodeFirstEdgeIndices.Add(topology.GetNodeFirstEdgeIndex(i));
			}

			for (int i = 0; i < topology.faceCount; ++i)
			{
				_faceNeighborCounts.Add(topology.GetFaceNeighborCount(i) + (topology.IsFaceInternal(i) ? 0 : -0x80000000));
				_faceFirstEdgeIndices.Add(topology.GetFaceFirstEdgeIndex(i));
			}

			for (int i = 0; i < topology.edgeCount; ++i)
			{
				_edgeNextChainedEdgeIndices.Add(topology.GetEdgeNextChainedEdgeIndex(i));
				_edgeNextLateralEdgeIndices.Add(topology.GetEdgeNextLateralEdgeIndex(i));
				_edgeTargetNodeIndices.Add(topology.GetEdgeTargetNodeIndex(i));
				_edgeTargetFaceIndices.Add(topology.GetEdgeTargetFaceIndex(i));
				_edgeWrapData.Add(topology.GetEdgeWrap(i));
			}
		}

		public object Clone()
		{
			var clone = new DynamicTopology();
			clone._nodeNeighborCounts = new List<int>(_nodeNeighborCounts);
			clone._nodeFirstEdgeIndices = new List<int>(_nodeFirstEdgeIndices);
			clone._faceNeighborCounts = new List<int>(_faceNeighborCounts);
			clone._faceFirstEdgeIndices = new List<int>(_faceFirstEdgeIndices);
			clone._edgeNextChainedEdgeIndices = new List<int>(_edgeNextChainedEdgeIndices);
			clone._edgeNextLateralEdgeIndices = new List<int>(_edgeNextLateralEdgeIndices);
			clone._edgeTargetNodeIndices = new List<int>(_edgeTargetNodeIndices);
			clone._edgeTargetFaceIndices = new List<int>(_edgeTargetFaceIndices);
			clone._edgeWrapData = new List<EdgeWrap>(_edgeWrapData);
			return clone;
		}

		#endregion

		#region ITopology Implementation

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

		public int faceCount
		{
			get
			{
				return _faceFirstEdgeIndices.Count;
			}
		}

		GraphNode.NodesIndexer IGraph.nodes
		{
			get
			{
				return new GraphNode.NodesIndexer(this);
			}
		}

		public TopologyNode.NodesIndexer nodes
		{
			get
			{
				return new TopologyNode.NodesIndexer(this);
			}
		}

		public TopologyFace.FacesIndexer faces
		{
			get
			{
				return new TopologyFace.FacesIndexer(this);
			}
		}

		public TopologyFace.FilteredFacesIndexer internalFaces
		{
			get
			{
				return new TopologyFace.FilteredFacesIndexer(this, true);
			}
		}

		public TopologyFace.FilteredFacesIndexer externalFaces
		{
			get
			{
				return new TopologyFace.FilteredFacesIndexer(this, false);
			}
		}

		GraphEdge.EdgesIndexer IGraph.edges
		{
			get
			{
				return new GraphEdge.EdgesIndexer(this);
			}
		}

		public TopologyEdge.EdgesIndexer edges
		{
			get
			{
				return new TopologyEdge.EdgesIndexer(this);
			}
		}

		public TopologyNodeEdge.EdgesIndexer nodeEdges
		{
			get
			{
				return new TopologyNodeEdge.EdgesIndexer(this);
			}
		}

		public TopologyFaceEdge.EdgesIndexer faceEdges
		{
			get
			{
				return new TopologyFaceEdge.EdgesIndexer(this);
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

		public int GetFaceNeighborCount(int faceIndex)
		{
			if (faceIndex == -1) return 0;
			int neighborCount = _faceNeighborCounts[faceIndex];
			return neighborCount - (neighborCount & -0x80000000);
		}

		public int GetFaceFirstEdgeIndex(int faceIndex)
		{
			if (faceIndex == -1) return -1;
			return _faceFirstEdgeIndices[faceIndex];
		}

		public bool IsFaceInternal(int faceIndex)
		{
			if (faceIndex == -1) return false;
			return _faceNeighborCounts[faceIndex] >= 0;
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

		public int GetEdgeTargetFaceIndex(int edgeIndex)
		{
			if (edgeIndex == -1) return -1;
			return _edgeTargetFaceIndices[edgeIndex];
		}

		public EdgeWrap GetEdgeWrap(int edgeIndex)
		{
			if (edgeIndex == -1) return EdgeWrap.None;
			return _edgeWrapData[edgeIndex];
		}

		#endregion

		#region Topology Modification

		//TODO: Not sure of a good interface for this, as I can't easily allow disconnected edges and free-floating nodes the way I do for graphs.

		#endregion
	}
}
