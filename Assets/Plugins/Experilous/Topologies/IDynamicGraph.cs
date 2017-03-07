/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;

namespace Experilous.Topologies
{
	public interface IDynamicGraph : IGraph
	{
		void AddNode(out int nodeIndex);
		GraphNode AddNode();

		void RemoveNode(int nodeIndex);
		void Remove(GraphNode node);

		int ConnectNodes(int firstNodeIndex, int secondNodeIndex);
		GraphEdge Connect(GraphNode firstNode, GraphNode secondNode);
		void ConnectNodes(int firstNodeIndex, int secondNodeIndex, out int firstEdgeIndex, out int secondEdgeIndex);
		void Connect(GraphNode firstNode, GraphNode secondNode, out GraphEdge firstEdge, out GraphEdge secondEdge);

		int ConnectNodes(int firstNodeIndex, int secondNodeIndex, int sourceNodePrevEdgeIndex, int targetNodePrevEdgeIndex);
		GraphEdge Connect(GraphNode firstNode, GraphNode secondNode, GraphEdge sourceNodePrevEdge, GraphEdge targetNodePrevEdge);
		void ConnectNodes(int firstNodeIndex, int secondNodeIndex, int sourceNodePrevEdgeIndex, int targetNodePrevEdgeIndex, out int firstEdgeIndex, out int secondEdgeIndex);
		void Connect(GraphNode firstNode, GraphNode secondNode, GraphEdge sourceNodePrevEdge, GraphEdge targetNodePrevEdge, out GraphEdge firstEdge, out GraphEdge secondEdge);

		void AddEdgePair(out int edgeIndex);
		GraphEdge AddEdgePair();
		void AddEdgePair(out int firstEdgeIndex, out int secondEdgeIndex);
		void AddEdgePair(out GraphEdge firstEdge, out GraphEdge secondEdge);

		void AttachEdgeToSourceNode(int edgeIndex, int nodeIndex);
		void AttachToSourceNode(GraphEdge edge, GraphNode node);

		void AttachEdgeAfter(int edgeIndex, int sourceNodePrevEdgeIndex);
		void AttachAfter(GraphEdge edge, GraphEdge sourceNodePrevEdge);

		void RemoveEdge(int edgeIndex);
		void Remove(GraphEdge edge);

		void DetachEdgeFromSourceNode(int edgeIndex);
		void DetachFromSourceNode(GraphEdge edge);

		void DetachNodeFromAllEdges(int nodeIndex);
		void DetachFromAllEdges(GraphNode node);

		void ShiftEdgeAroundSourceNodeCW(int edgeIndex);
		void ShiftEdgeAroundSourceNodeCW(GraphEdge edge);
		void ShiftEdgeAroundSourceNodeCCW(int edgeIndex);
		void ShiftEdgeAroundSourceNodeCCW(GraphEdge edge);

		void Clear();
	}
}
