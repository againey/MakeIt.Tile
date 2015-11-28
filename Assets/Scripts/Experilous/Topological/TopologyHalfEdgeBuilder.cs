using System.Collections.Generic;

namespace Experilous.Topological
{
	public partial class Topology
	{
		public class HalfEdgeBuilder
		{
			private struct HalfEdge
			{
				public int _vertex;
				public int _face;

				public HalfEdge(int vertex)
				{
					_vertex = vertex;
					_face = -1;
				}

				public HalfEdge(int vertex, int face)
				{
					_vertex = vertex;
					_face = face;
				}
			}

			private struct HalfEdgeForwardLinks
			{
				public int _vNext;
				public int _fNext;

				public HalfEdgeForwardLinks(int vNext, int fNext)
				{
					_vNext = vNext;
					_fNext = fNext;
				}
			}

			private readonly List<HalfEdge> _halfEdges = new List<HalfEdge>();
			private readonly List<int> _twins = new List<int>();
			private readonly List<HalfEdgeForwardLinks> _forwardLinks = new List<HalfEdgeForwardLinks>();

			private int _maxVertexIndex = -1;
			private int _maxFaceIndex = -1;

			public HalfEdgeBuilder()
			{
			}

			public HalfEdgeBuilder(int edgeCount)
			{
				_halfEdges.Capacity = edgeCount * 2;
			}

			public int AddHalfEdge(int vertex)
			{
				return AddHalfEdge(vertex, -1);
			}

			public int AddHalfEdge(int vertex, int face)
			{
				var halfEdgeIndex = _halfEdges.Count;
				_halfEdges.Add(new HalfEdge(face, vertex));
				_twins.Add(-1);
				_forwardLinks.Add(new HalfEdgeForwardLinks(-1, -1));
				_maxVertexIndex = UnityEngine.Mathf.Max(_maxVertexIndex, vertex);
				_maxFaceIndex = UnityEngine.Mathf.Max(_maxFaceIndex, face);
				return halfEdgeIndex;
			}

			public void LinkTwins(int halfEdge0, int halfEdge1)
			{
				_twins[halfEdge0] = halfEdge1;
				_twins[halfEdge1] = halfEdge0;
			}

			public void LinkToNext(int halfEdge, int vNext, int fNext)
			{
				_forwardLinks[halfEdge] = new HalfEdgeForwardLinks(vNext, fNext);
			}

			public Topology BuildTopology()
			{
				var topology = new Topology();

				topology._vertexData = new NodeData[_maxVertexIndex + 1];
				topology._faceData = new NodeData[_maxFaceIndex + 1];
				topology._edgeData = new EdgeData[_halfEdges.Count];

				for (int halfEdge = 0; halfEdge < _halfEdges.Count; ++halfEdge)
				{
					var twin = _twins[halfEdge];
					var vertex = _halfEdges[halfEdge]._vertex;
					var face = _halfEdges[halfEdge]._face;
					topology._edgeData[halfEdge] = new EdgeData(twin, _forwardLinks[halfEdge]._vNext, _forwardLinks[halfEdge]._fNext, vertex, face);

					if (vertex != -1 && !topology._vertexData[vertex].isInitialized)
					{
						var neighborCount = 0;
						var vertexEdge = twin;
						do
						{
							vertexEdge = _forwardLinks[vertexEdge]._vNext;
							++neighborCount;
							if (neighborCount > _halfEdges.Count) throw new System.InvalidOperationException(string.Format("Half edge forward links were specified such that the edges surrounding a vertex ({0}, starting with edge {1}) were misconfigured.", vertex, twin));
						} while (vertexEdge != twin);
						topology._vertexData[vertex] = new NodeData(neighborCount, twin);
					}

					if (face != -1 && !topology._faceData[face].isInitialized)
					{
						var neighborCount = 0;
						var faceEdge = twin;
						do
						{
							faceEdge = _forwardLinks[faceEdge]._vNext;
							++neighborCount;
							if (neighborCount > _halfEdges.Count) throw new System.InvalidOperationException(string.Format("Half edge forward links were specified such that the edges surrounding a face ({0}, starting with edge {1}) were misconfigured.", face, twin));
						} while (faceEdge != twin);
						topology._faceData[face] = new NodeData(neighborCount, twin);
					}
				}

				return topology;
			}
		}
	}
}
