/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;

namespace Experilous.Topologies
{
	public interface IGraph : ICloneable
	{
		int nodeCount { get; }
		int edgeCount { get; }

		int GetNodeNeighborCount(int nodeIndex);
		int GetNodeFirstEdgeIndex(int nodeIndex);

		GraphNode.NodesIndexer nodes { get; }

		int GetEdgeNextChainedEdgeIndex(int edgeIndex);
		int GetEdgeNextLateralEdgeIndex(int edgeIndex);
		int GetEdgeTargetNodeIndex(int edgeIndex);

		GraphEdge.EdgesIndexer edges { get; }
	}

	public struct GraphNode
	{
		private IGraph _graph;
		private int _index;

		public GraphNode(IGraph graph, int index)
		{
			_graph = graph;
			_index = index;
		}

		public GraphNode(IGraph graph)
		{
			_graph = graph;
			_index = -1;
		}

		public static GraphNode none { get { return new GraphNode(); } }

		public IGraph graph { get { return _graph; } }
		public int index { get { return _index; } }
		public static implicit operator bool(GraphNode node) { return node._graph != null && node._index >= 0; }

		public int neighborCount { get { return _graph.GetNodeNeighborCount(_index); } }

		public GraphEdge firstEdge { get { return new GraphEdge(_graph, _graph.GetNodeFirstEdgeIndex(_index)); } }

		public struct EdgesIndexer : IEnumerable<GraphEdge>
		{
			private IGraph _graph;
			private int _index;

			public EdgesIndexer(IGraph graph, int index)
			{
				_graph = graph;
				_index = index;
			}

			public int Count { get { return _graph.GetNodeNeighborCount(_index); } }

			public struct EdgeEnumerator : IEnumerator<GraphEdge>
			{
				private IGraph _graph;
				private int _firstEdgeIndex;
				private int _currentEdgeIndex;
				private int _nextEdgeIndex;

				public EdgeEnumerator(IGraph graph, int firstEdgeIndex)
				{
					_graph = graph;
					_firstEdgeIndex = firstEdgeIndex;
					_currentEdgeIndex = -1;
					_nextEdgeIndex = firstEdgeIndex;
				}

				public GraphEdge Current { get { return new GraphEdge(_graph, _currentEdgeIndex); } }

				public bool MoveNext()
				{
					if (_nextEdgeIndex != _firstEdgeIndex || _currentEdgeIndex == -1)
					{
						_currentEdgeIndex = _nextEdgeIndex;
						_nextEdgeIndex = _graph.GetEdgeNextLateralEdgeIndex(_currentEdgeIndex);
						return true;
					}
					else
					{
						return false;
					}
				}

				public void Reset()
				{
					_currentEdgeIndex = -1;
					_nextEdgeIndex = _firstEdgeIndex;
				}

				object IEnumerator.Current { get { return Current; } }
				bool IEnumerator.MoveNext() { return MoveNext(); }
				void IEnumerator.Reset() { Reset(); }
				void IDisposable.Dispose() { }
			}

			public EdgeEnumerator GetEnumerator()
			{
				return new EdgeEnumerator(_graph, _graph.GetNodeFirstEdgeIndex(_index));
			}

			IEnumerator<GraphEdge> IEnumerable<GraphEdge>.GetEnumerator()
			{
				return GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public EdgesIndexer edges { get { return new EdgesIndexer(_graph, _index); } }

		public struct NodesIndexer : IEnumerable<GraphNode>
		{
			private IGraph _graph;

			public NodesIndexer(IGraph graph)
			{
				_graph = graph;
			}

			public int Count { get { return _graph.nodeCount; } }

			public struct NodeEnumerator : IEnumerator<GraphNode>
			{
				private IGraph _graph;
				private int _currentNodeIndex;

				public NodeEnumerator(IGraph graph)
				{
					_graph = graph;
					_currentNodeIndex = -1;
				}

				public GraphNode Current { get { return new GraphNode(_graph, _currentNodeIndex); } }

				public bool MoveNext()
				{
					if (_currentNodeIndex < _graph.nodeCount)
					{
						return ++_currentNodeIndex < _graph.nodeCount;
					}
					else
					{
						return false;
					}
				}

				public void Reset()
				{
					_currentNodeIndex = -1;
				}

				object IEnumerator.Current { get { return Current; } }
				bool IEnumerator.MoveNext() { return MoveNext(); }
				void IEnumerator.Reset() { Reset(); }
				void IDisposable.Dispose() { }
			}

			public NodeEnumerator GetEnumerator()
			{
				return new NodeEnumerator(_graph);
			}

			IEnumerator<GraphNode> IEnumerable<GraphNode>.GetEnumerator()
			{
				return GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public GraphEdge FindEdge(GraphNode node)
		{
			if (!ReferenceEquals(_graph, node.graph)) throw new ArgumentException("The provided node does not belong to the same graph as this edge.", "node");

			int nodeIndex = node.index;

			foreach (var edge in edges)
			{
				if (edge.node.index == nodeIndex)
				{
					return edge;
				}
			}

			return new GraphEdge(_graph);
		}

		public bool TryFindEdge(GraphNode node, out GraphEdge edge)
		{
			if (!ReferenceEquals(_graph, node.graph)) throw new ArgumentException("The provided node does not belong to the same graph as this edge.", "node");

			int nodeIndex = node.index;

			foreach (var nodeEdge in edges)
			{
				if (nodeEdge.node.index == nodeIndex)
				{
					edge = nodeEdge;
					return true;
				}
			}

			edge = new GraphEdge(_graph);
			return false;
		}

		public override bool Equals(object other) { return other is GraphNode && _index == ((GraphNode)other)._index && ReferenceEquals(_graph, ((GraphNode)other)._graph); }
		public override int GetHashCode() { return _graph.GetHashCode() ^ _index.GetHashCode(); }

		public bool Equals(GraphNode other) { return _index == other._index && ReferenceEquals(_graph,  other._graph); }
		public int CompareTo(GraphNode other) { return _index - other._index; }

		public static bool operator ==(GraphNode lhs, GraphNode rhs) { return lhs._index == rhs._index; }
		public static bool operator !=(GraphNode lhs, GraphNode rhs) { return lhs._index != rhs._index; }
		public static bool operator < (GraphNode lhs, GraphNode rhs) { return lhs._index <  rhs._index; }
		public static bool operator <=(GraphNode lhs, GraphNode rhs) { return lhs._index <= rhs._index; }
		public static bool operator > (GraphNode lhs, GraphNode rhs) { return lhs._index >  rhs._index; }
		public static bool operator >=(GraphNode lhs, GraphNode rhs) { return lhs._index >= rhs._index; }

		public override string ToString()
		{
			var sb = new System.Text.StringBuilder();
			sb.AppendFormat("Graph Node {0}", _index);

			if (neighborCount > 0)
			{
				var edge = firstEdge;
				sb.AppendFormat("; Neighbors ({0}) {{ Node {1}", neighborCount, edge.node.index);
				for (int i = 1; i < neighborCount; ++i)
				{
					edge = edge.right;
					sb.AppendFormat(", Node {0}", edge.node.index);
				}

				sb.Append(" }");
			}
			else
			{
				sb.Append("; Neighbors (0) {}");
			}

			return sb.ToString();
		}
	}

	public struct GraphEdge
	{
		private IGraph _graph;
		private int _index;

		public GraphEdge(IGraph graph)
		{
			_graph = graph;
			_index = -1;
		}

		public GraphEdge(IGraph graph, int index)
		{
			_graph = graph;
			_index = index;
		}

		public static GraphEdge none { get { return new GraphEdge(); } }

		public IGraph graph { get { return _graph; } }
		public int index { get { return _index; } }
		public static implicit operator bool(GraphEdge edge) { return edge._graph != null && edge._index >= 0; }

		public GraphEdge twin { get { return new GraphEdge(_graph, _index ^ 1); } }
		public GraphEdge firstTwin { get { return new GraphEdge(_graph, _index & ~1); } }
		public GraphEdge secondTwin { get { return new GraphEdge(_graph, _index | 1); } }

		public bool isFirstTwin { get { return (_index & 1) == 0; } }
		public bool isSecondTwin { get { return (_index & 1) == 1; } }

		public GraphEdge left { get { return new GraphEdge(_graph, _graph.GetEdgeNextChainedEdgeIndex(_index ^ 1)); } }
		public GraphEdge right { get { return new GraphEdge(_graph, _graph.GetEdgeNextLateralEdgeIndex(_index)); } }

		public GraphEdge forward { get { return new GraphEdge(_graph, _graph.GetEdgeNextChainedEdgeIndex(_index)); } }
		public GraphEdge backward { get { return new GraphEdge(_graph, _graph.GetEdgeNextLateralEdgeIndex(_index) ^ 1); } }

		public GraphNode node { get { return new GraphNode(_graph, _graph.GetEdgeTargetNodeIndex(_index)); } }
		public GraphNode targetNode { get { return new GraphNode(_graph, _graph.GetEdgeTargetNodeIndex(_index)); } }
		public GraphNode sourceNode { get { return new GraphNode(_graph, _graph.GetEdgeTargetNodeIndex(_index ^ 1)); } }

		public struct EdgesIndexer : IEnumerable<GraphEdge>
		{
			private IGraph _graph;

			public EdgesIndexer(IGraph graph)
			{
				_graph = graph;
			}

			public int Count { get { return _graph.edgeCount; } }

			public struct EdgeEnumerator : IEnumerator<GraphEdge>
			{
				private IGraph _graph;
				private int _currentEdgeIndex;

				public EdgeEnumerator(IGraph graph)
				{
					_graph = graph;
					_currentEdgeIndex = -1;
				}

				public GraphEdge Current { get { return new GraphEdge(_graph, _currentEdgeIndex); } }

				public bool MoveNext()
				{
					if (_currentEdgeIndex < _graph.edgeCount)
					{
						return ++_currentEdgeIndex < _graph.edgeCount;
					}
					else
					{
						return false;
					}
				}

				public void Reset()
				{
					_currentEdgeIndex = -1;
				}

				object IEnumerator.Current { get { return Current; } }
				bool IEnumerator.MoveNext() { return MoveNext(); }
				void IEnumerator.Reset() { Reset(); }
				void IDisposable.Dispose() { }
			}

			public EdgeEnumerator GetEnumerator()
			{
				return new EdgeEnumerator(_graph);
			}

			IEnumerator<GraphEdge> IEnumerable<GraphEdge>.GetEnumerator()
			{
				return GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public override bool Equals(object other) { return other is GraphEdge && _index == ((GraphEdge)other)._index && ReferenceEquals(_graph, ((GraphEdge)other)._graph); }
		public override int GetHashCode() { return _graph.GetHashCode() ^ _index.GetHashCode(); }

		public bool Equals(GraphEdge other) { return _index == other._index && ReferenceEquals(_graph,  other._graph); }
		public int CompareTo(GraphEdge other) { return _index - other._index; }

		public static bool operator ==(GraphEdge lhs, GraphEdge rhs) { return lhs._index == rhs._index; }
		public static bool operator !=(GraphEdge lhs, GraphEdge rhs) { return lhs._index != rhs._index; }
		public static bool operator < (GraphEdge lhs, GraphEdge rhs) { return lhs._index <  rhs._index; }
		public static bool operator <=(GraphEdge lhs, GraphEdge rhs) { return lhs._index <= rhs._index; }
		public static bool operator > (GraphEdge lhs, GraphEdge rhs) { return lhs._index >  rhs._index; }
		public static bool operator >=(GraphEdge lhs, GraphEdge rhs) { return lhs._index >= rhs._index; }

		public override string ToString()
		{
			return string.Format("Graph Edge {0}; Twin {1}; from Node {2} to Node {3}; Forward {4}; Right {5}", _index, _index ^ 1, sourceNode.index, targetNode.index, forward.index, right.index);
		}
	}
}
