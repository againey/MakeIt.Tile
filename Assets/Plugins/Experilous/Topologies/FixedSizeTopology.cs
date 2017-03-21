/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

namespace Experilous.Topologies
{
	public class FixedSizeTopology : FixedSizeGraph, ITopology
	{
		protected int[] _faceNeighborCounts;
		protected int[] _faceFirstEdgeIndices;
		protected int[] _edgeTargetFaceIndices;
		protected EdgeWrap[] _edgeWrapData;

		protected FixedSizeTopology()
		{
		}

		public FixedSizeTopology(int nodeCount, int faceCount, int edgeCount)
			: base(nodeCount, edgeCount)
		{
			_faceNeighborCounts = new int[faceCount];
			_faceFirstEdgeIndices = new int[faceCount];
			_edgeTargetFaceIndices = new int[edgeCount];
			_edgeWrapData = new EdgeWrap[edgeCount];

			for (int i = 0; i < faceCount; ++i)
			{
				_faceNeighborCounts[i] = 0;
				_faceFirstEdgeIndices[i] = -1;
			}

			for (int i = 0; i < edgeCount; ++i)
			{
				_edgeTargetFaceIndices[i] = -1;
				_edgeWrapData[i] = EdgeWrap.None;
			}
		}

		public FixedSizeTopology(int[] nodeNeighborCounts, int[] nodeFirstEdgeIndices, int[] faceNeighborCounts, int[] faceFirstEdgeIndices, int[] edgeNextChainedEdgeIndices, int[] edgeNextLateralEdgeIndices, int[] edgeTargetNodeIndices, int[] edgeTargetFaceIndices, EdgeWrap[] edgeWrapData)
			: base(nodeNeighborCounts, nodeFirstEdgeIndices, edgeNextChainedEdgeIndices, edgeNextLateralEdgeIndices, edgeTargetNodeIndices)
		{
			_faceNeighborCounts = faceNeighborCounts;
			_faceFirstEdgeIndices = faceFirstEdgeIndices;
			_edgeTargetFaceIndices = edgeTargetFaceIndices;
			_edgeWrapData = edgeWrapData;

			UnityEngine.Debug.Assert(_faceNeighborCounts.Length == _faceFirstEdgeIndices.Length);
			UnityEngine.Debug.Assert(_edgeNextChainedEdgeIndices.Length == _edgeTargetFaceIndices.Length);
			UnityEngine.Debug.Assert(_edgeNextChainedEdgeIndices.Length == _edgeWrapData.Length);
		}

		public FixedSizeTopology(ITopology topology)
			: base(topology)
		{
			_faceNeighborCounts = new int[topology.faceCount];
			_faceFirstEdgeIndices = new int[topology.faceCount];
			_edgeTargetFaceIndices = new int[topology.edgeCount];
			_edgeWrapData = new EdgeWrap[topology.edgeCount];

			for (int i = 0; i < topology.faceCount; ++i)
			{
				_faceNeighborCounts[i] = topology.GetFaceNeighborCount(i) + (topology.IsFaceInternal(i) ? 0 : -0x80000000);
				_faceFirstEdgeIndices[i] = topology.GetFaceFirstEdgeIndex(i);
			}

			for (int i = 0; i < topology.edgeCount; ++i)
			{
				_edgeTargetFaceIndices[i] = topology.GetEdgeTargetFaceIndex(i);
				_edgeWrapData[i] = topology.GetEdgeWrap(i);
			}
		}

		public override object Clone()
		{
			var clone = new FixedSizeTopology();
			clone._nodeNeighborCounts = _nodeNeighborCounts.Clone() as int[];
			clone._nodeFirstEdgeIndices = _nodeFirstEdgeIndices.Clone() as int[];
			clone._faceNeighborCounts = _faceNeighborCounts.Clone() as int[];
			clone._faceFirstEdgeIndices = _faceFirstEdgeIndices.Clone() as int[];
			clone._edgeNextChainedEdgeIndices = _edgeNextChainedEdgeIndices.Clone() as int[];
			clone._edgeNextLateralEdgeIndices = _edgeNextLateralEdgeIndices.Clone() as int[];
			clone._edgeTargetNodeIndices = _edgeTargetNodeIndices.Clone() as int[];
			clone._edgeTargetFaceIndices = _edgeTargetFaceIndices.Clone() as int[];
			clone._edgeWrapData = _edgeWrapData.Clone() as EdgeWrap[];
			return clone;
		}

		public int faceCount
		{
			get
			{
				return _faceFirstEdgeIndices.Length;
			}
		}

		public new TopologyNode.NodesIndexer nodes
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

		public new TopologyEdge.EdgesIndexer edges
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
	}
}
