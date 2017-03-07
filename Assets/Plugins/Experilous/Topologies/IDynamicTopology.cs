/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;

namespace Experilous.Topologies
{
	public interface IDynamicTopology : ITopology
	{
		int faceCount { get; }
		int internalFaceCount { get; }
		int externalFaceCount { get; }

		int GetFaceNeighborCount(int faceIndex);
		int GetFaceFirstEdgeIndex(int faceIndex);
		bool IsFaceInternal(int faceIndex);

		TopologyFace.FacesIndexer faces { get; }

		int GetEdgeFaceIndex(int edgeIndex);
		int GetEdgePrevFaceEdgeIndex(int edgeIndex);
		int GetEdgeNextFaceEdgeIndex(int edgeIndex);
		EdgeWrap GetEdgeWrap(int edgeIndex);

		TopologyHalfEdge.EdgesIndexer halfEdges { get; }
		TopologyNodeEdge.EdgesIndexer nodeEdges { get; }
		TopologyFaceEdge.EdgesIndexer faceEdges { get; }
	}
}
