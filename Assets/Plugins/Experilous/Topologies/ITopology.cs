/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;

namespace Experilous.Topologies
{
	public interface ITopology : IGraph
	{
		new TopologyNode.NodesIndexer nodes { get; }

		int faceCount { get; }

		int GetFaceNeighborCount(int faceIndex);
		int GetFaceFirstEdgeIndex(int faceIndex);
		bool IsFaceInternal(int faceIndex);

		TopologyFace.FacesIndexer faces { get; }
		TopologyFace.FilteredFacesIndexer internalFaces { get; }
		TopologyFace.FilteredFacesIndexer externalFaces { get; }

		int GetEdgeTargetFaceIndex(int edgeIndex);
		EdgeWrap GetEdgeWrap(int edgeIndex);

		new TopologyEdge.EdgesIndexer edges { get; }
		TopologyNodeEdge.EdgesIndexer nodeEdges { get; }
		TopologyFaceEdge.EdgesIndexer faceEdges { get; }
	}

	public struct TopologyNode
	{
		private ITopology _topology;
		private int _index;

		public TopologyNode(ITopology topology, int index)
		{
			_topology = topology;
			_index = index;
		}

		public static TopologyNode none { get { return new TopologyNode(); } }

		public ITopology topology { get { return _topology; } }
		public int index { get { return _index; } }
		public static implicit operator bool(TopologyNode node) { return node._topology != null && node._index >= 0; }

		public int neighborCount { get { return _topology.GetNodeNeighborCount(_index); } }

		public TopologyNodeEdge firstEdge { get { return new TopologyNodeEdge(_topology, _topology.GetNodeFirstEdgeIndex(_index)); } }

		public struct EdgesIndexer : IEnumerable<TopologyNodeEdge>
		{
			private ITopology _topology;
			private int _index;

			public EdgesIndexer(ITopology topology, int index)
			{
				_topology = topology;
				_index = index;
			}

			public int Count { get { return _topology.GetNodeNeighborCount(_index); } }

			public struct EdgeEnumerator : IEnumerator<TopologyNodeEdge>
			{
				private ITopology _topology;
				private int _firstEdgeIndex;
				private int _currentEdgeIndex;
				private int _nextEdgeIndex;

				public EdgeEnumerator(ITopology topology, int firstEdgeIndex)
				{
					_topology = topology;
					_firstEdgeIndex = firstEdgeIndex;
					_currentEdgeIndex = -1;
					_nextEdgeIndex = firstEdgeIndex;
				}

				public TopologyNodeEdge Current { get { return new TopologyNodeEdge(_topology, _currentEdgeIndex); } }

				public bool MoveNext()
				{
					if (_nextEdgeIndex != _firstEdgeIndex || _currentEdgeIndex == -1)
					{
						_currentEdgeIndex = _nextEdgeIndex;
						_nextEdgeIndex = _topology.GetEdgeNextLateralEdgeIndex(_currentEdgeIndex);
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
				return new EdgeEnumerator(_topology, _topology.GetNodeFirstEdgeIndex(_index));
			}

			IEnumerator<TopologyNodeEdge> IEnumerable<TopologyNodeEdge>.GetEnumerator()
			{
				return GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public EdgesIndexer edges { get { return new EdgesIndexer(_topology, _index); } }

		public struct OuterEdgesIndexer : IEnumerable<TopologyNodeEdge>
		{
			private ITopology _topology;
			private int _index;

			public OuterEdgesIndexer(ITopology topology, int index)
			{
				_topology = topology;
				_index = index;
			}

			public struct OuterEdgeEnumerator : IEnumerator<TopologyNodeEdge>
			{
				private ITopology _topology;
				private int _firstNodeEdgeIndex;
				private int _currentNodeEdgeIndex;
				private int _nextFaceEdgeIndex;
				private int _nextNodeEdgeIndex;

				public OuterEdgeEnumerator(ITopology topology, int firstEdgeIndex)
				{
					_topology = topology;
					_firstNodeEdgeIndex = firstEdgeIndex ^ 1;
					_currentNodeEdgeIndex = -1;
					_nextFaceEdgeIndex = _firstNodeEdgeIndex;
					_nextNodeEdgeIndex = _firstNodeEdgeIndex;
				}

				public TopologyNodeEdge Current { get { return new TopologyNodeEdge(_topology, _currentNodeEdgeIndex); } }

				public bool MoveNext()
				{
					if (_nextFaceEdgeIndex != _nextNodeEdgeIndex)
					{
						_currentNodeEdgeIndex = _nextFaceEdgeIndex;
						_nextFaceEdgeIndex = _topology.GetEdgeNextChainedEdgeIndex(_currentNodeEdgeIndex);
						return true;
					}
					else if (_nextNodeEdgeIndex != _firstNodeEdgeIndex || _currentNodeEdgeIndex == -1)
					{
						do
						{
							var twinIndex = _nextNodeEdgeIndex ^ 1;
							_currentNodeEdgeIndex = _topology.GetEdgeNextChainedEdgeIndex(twinIndex);
							_nextNodeEdgeIndex = _topology.GetEdgeNextLateralEdgeIndex(twinIndex) ^ 1;
							if (_currentNodeEdgeIndex == _firstNodeEdgeIndex)
							{
								_nextFaceEdgeIndex = _nextNodeEdgeIndex = _firstNodeEdgeIndex;
								return false;
							}
						} while (_currentNodeEdgeIndex == _nextNodeEdgeIndex);
						_nextFaceEdgeIndex = _topology.GetEdgeNextChainedEdgeIndex(_currentNodeEdgeIndex);
						return true;
					}
					else
					{
						return false;
					}
				}

				public void Reset()
				{
					_currentNodeEdgeIndex = -1;
					_nextFaceEdgeIndex = -1;
					_nextNodeEdgeIndex = _firstNodeEdgeIndex;
				}

				object IEnumerator.Current { get { return Current; } }
				bool IEnumerator.MoveNext() { return MoveNext(); }
				void IEnumerator.Reset() { Reset(); }
				void IDisposable.Dispose() { }
			}

			public OuterEdgeEnumerator GetEnumerator()
			{
				return new OuterEdgeEnumerator(_topology, _topology.GetFaceFirstEdgeIndex(_index));
			}

			IEnumerator<TopologyNodeEdge> IEnumerable<TopologyNodeEdge>.GetEnumerator()
			{
				return GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public OuterEdgesIndexer outerEdges { get { return new OuterEdgesIndexer(_topology, _index); } }

		public struct NodesIndexer : IEnumerable<TopologyNode>
		{
			private ITopology _topology;

			public NodesIndexer(ITopology topology)
			{
				_topology = topology;
			}

			public int Count { get { return _topology.faceCount; } }

			public struct NodeEnumerator : IEnumerator<TopologyNode>
			{
				private ITopology _topology;
				private int _currentNodeIndex;

				public NodeEnumerator(ITopology topology)
				{
					_topology = topology;
					_currentNodeIndex = -1;
				}

				public TopologyNode Current { get { return new TopologyNode(_topology, _currentNodeIndex); } }

				public bool MoveNext()
				{
					if (_currentNodeIndex < _topology.faceCount)
					{
						return ++_currentNodeIndex < _topology.faceCount;
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
				return new NodeEnumerator(_topology);
			}

			IEnumerator<TopologyNode> IEnumerable<TopologyNode>.GetEnumerator()
			{
				return GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public override bool Equals(object other) { return other is TopologyNode && _index == ((TopologyNode)other)._index && ReferenceEquals(_topology, ((TopologyNode)other)._topology); }
		public override int GetHashCode() { return _topology.GetHashCode() ^ _index.GetHashCode(); }

		public bool Equals(TopologyNode other) { return _index == other._index && ReferenceEquals(_topology,  other._topology); }
		public int CompareTo(TopologyNode other) { return _index - other._index; }

		public static bool operator ==(TopologyNode lhs, TopologyNode rhs) { return lhs._index == rhs._index; }
		public static bool operator !=(TopologyNode lhs, TopologyNode rhs) { return lhs._index != rhs._index; }
		public static bool operator < (TopologyNode lhs, TopologyNode rhs) { return lhs._index <  rhs._index; }
		public static bool operator <=(TopologyNode lhs, TopologyNode rhs) { return lhs._index <= rhs._index; }
		public static bool operator > (TopologyNode lhs, TopologyNode rhs) { return lhs._index >  rhs._index; }
		public static bool operator >=(TopologyNode lhs, TopologyNode rhs) { return lhs._index >= rhs._index; }

		public override string ToString()
		{
			var sb = new System.Text.StringBuilder();
			sb.AppendFormat("Topology Node {0}", _index);

			if (neighborCount > 0)
			{
				var edge = firstEdge;
				sb.AppendFormat("; Neighbors ({0}) {{ Node {1} and Face {2} by Edge {3}", neighborCount, edge.node.index, edge.prevFace.index, edge.index);
				for (int i = 1; i < neighborCount; ++i)
				{
					edge = edge.next;
					sb.AppendFormat(", Node {0} and Face {1} by Edge {2}", edge.node.index, edge.prevFace.index, edge.index);
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

	public struct TopologyFace
	{
		private ITopology _topology;
		private int _index;

		public TopologyFace(ITopology topology, int index)
		{
			_topology = topology;
			_index = index;
		}

		public static TopologyFace none { get { return new TopologyFace(); } }

		public ITopology topology { get { return _topology; } }
		public int index { get { return _index; } }
		public static implicit operator bool(TopologyFace face) { return face._topology != null && face._index >= 0; }

		public int neighborCount { get { return _topology.GetFaceNeighborCount(_index); } }

		public TopologyFaceEdge firstEdge { get { return new TopologyFaceEdge(_topology, _topology.GetFaceFirstEdgeIndex(_index)); } }

		public bool isInternal { get { return _topology.IsFaceInternal(_index); } }
		public bool isExternal { get { return !_topology.IsFaceInternal(_index); } }

		public struct EdgesIndexer : IEnumerable<TopologyFaceEdge>
		{
			private ITopology _topology;
			private int _index;

			public EdgesIndexer(ITopology topology, int index)
			{
				_topology = topology;
				_index = index;
			}

			public int Count { get { return _topology.GetFaceNeighborCount(_index); } }

			public struct EdgeEnumerator : IEnumerator<TopologyFaceEdge>
			{
				private ITopology _topology;
				private int _firstEdgeIndex;
				private int _currentEdgeIndex;
				private int _nextEdgeIndex;

				public EdgeEnumerator(ITopology topology, int firstEdgeIndex)
				{
					_topology = topology;
					_firstEdgeIndex = firstEdgeIndex;
					_currentEdgeIndex = -1;
					_nextEdgeIndex = firstEdgeIndex;
				}

				public TopologyFaceEdge Current { get { return new TopologyFaceEdge(_topology, _currentEdgeIndex); } }

				public bool MoveNext()
				{
					if (_nextEdgeIndex != _firstEdgeIndex || _currentEdgeIndex == -1)
					{
						_currentEdgeIndex = _nextEdgeIndex;
						_nextEdgeIndex = _topology.GetEdgeNextChainedEdgeIndex(_currentEdgeIndex);
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
				return new EdgeEnumerator(_topology, _topology.GetFaceFirstEdgeIndex(_index));
			}

			IEnumerator<TopologyFaceEdge> IEnumerable<TopologyFaceEdge>.GetEnumerator()
			{
				return GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public EdgesIndexer edges { get { return new EdgesIndexer(_topology, _index); } }

		public struct OuterEdgesIndexer : IEnumerable<TopologyFaceEdge>
		{
			private ITopology _topology;
			private int _index;

			public OuterEdgesIndexer(ITopology topology, int index)
			{
				_topology = topology;
				_index = index;
			}

			public struct OuterEdgeEnumerator : IEnumerator<TopologyFaceEdge>
			{
				private ITopology _topology;
				private int _firstFaceEdgeIndex;
				private int _currentFaceEdgeIndex;
				private int _nextNodeEdgeIndex;
				private int _nextFaceEdgeIndex;

				public OuterEdgeEnumerator(ITopology topology, int firstEdgeIndex)
				{
					_topology = topology;
					_firstFaceEdgeIndex = firstEdgeIndex;
					_currentFaceEdgeIndex = -1;
					_nextNodeEdgeIndex = _firstFaceEdgeIndex;
					_nextFaceEdgeIndex = _firstFaceEdgeIndex;
				}

				public TopologyFaceEdge Current { get { return new TopologyFaceEdge(_topology, _currentFaceEdgeIndex); } }

				public bool MoveNext()
				{
					if (_nextNodeEdgeIndex != _nextFaceEdgeIndex)
					{
						_currentFaceEdgeIndex = _nextNodeEdgeIndex;
						_nextNodeEdgeIndex = _topology.GetEdgeNextLateralEdgeIndex(_currentFaceEdgeIndex);
						return true;
					}
					else if (_nextFaceEdgeIndex != _firstFaceEdgeIndex || _currentFaceEdgeIndex == -1)
					{
						do
						{
							_currentFaceEdgeIndex = _topology.GetEdgeNextLateralEdgeIndex(_nextFaceEdgeIndex ^ 1);
							_nextFaceEdgeIndex = _topology.GetEdgeNextChainedEdgeIndex(_nextFaceEdgeIndex);
							if (_currentFaceEdgeIndex == _firstFaceEdgeIndex)
							{
								_nextNodeEdgeIndex = _nextFaceEdgeIndex = _firstFaceEdgeIndex;
								return false;
							}
						} while (_currentFaceEdgeIndex == _nextFaceEdgeIndex);
						_nextNodeEdgeIndex = _topology.GetEdgeNextLateralEdgeIndex(_currentFaceEdgeIndex);
						return true;
					}
					else
					{
						return false;
					}
				}

				public void Reset()
				{
					_currentFaceEdgeIndex = -1;
					_nextNodeEdgeIndex = -1;
					_nextFaceEdgeIndex = _firstFaceEdgeIndex;
				}

				object IEnumerator.Current { get { return Current; } }
				bool IEnumerator.MoveNext() { return MoveNext(); }
				void IEnumerator.Reset() { Reset(); }
				void IDisposable.Dispose() { }
			}

			public OuterEdgeEnumerator GetEnumerator()
			{
				return new OuterEdgeEnumerator(_topology, _topology.GetNodeFirstEdgeIndex(_index));
			}

			IEnumerator<TopologyFaceEdge> IEnumerable<TopologyFaceEdge>.GetEnumerator()
			{
				return GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public OuterEdgesIndexer outerEdges { get { return new OuterEdgesIndexer(_topology, _index); } }

		public struct FacesIndexer : IEnumerable<TopologyFace>
		{
			private ITopology _topology;

			public FacesIndexer(ITopology topology)
			{
				_topology = topology;
			}

			public int Count { get { return _topology.faceCount; } }

			public struct FaceEnumerator : IEnumerator<TopologyFace>
			{
				private ITopology _topology;
				private int _currentFaceIndex;

				public FaceEnumerator(ITopology topology)
				{
					_topology = topology;
					_currentFaceIndex = -1;
				}

				public TopologyFace Current { get { return new TopologyFace(_topology, _currentFaceIndex); } }

				public bool MoveNext()
				{
					if (_currentFaceIndex < _topology.faceCount)
					{
						return ++_currentFaceIndex < _topology.faceCount;
					}
					else
					{
						return false;
					}
				}

				public void Reset()
				{
					_currentFaceIndex = -1;
				}

				object IEnumerator.Current { get { return Current; } }
				bool IEnumerator.MoveNext() { return MoveNext(); }
				void IEnumerator.Reset() { Reset(); }
				void IDisposable.Dispose() { }
			}

			public FaceEnumerator GetEnumerator()
			{
				return new FaceEnumerator(_topology);
			}

			IEnumerator<TopologyFace> IEnumerable<TopologyFace>.GetEnumerator()
			{
				return GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public struct FilteredFacesIndexer : IEnumerable<TopologyFace>
		{
			private ITopology _topology;
			private bool _isInternalMatch;

			public FilteredFacesIndexer(ITopology topology, bool isInternalMatch)
			{
				_topology = topology;
				_isInternalMatch = isInternalMatch;
			}

			public int Count { get { return _topology.faceCount; } }

			public struct FaceEnumerator : IEnumerator<TopologyFace>
			{
				private ITopology _topology;
				private int _currentFaceIndex;
				private bool _isInternalMatch;

				public FaceEnumerator(ITopology topology, bool isInternalMatch)
				{
					_topology = topology;
					_currentFaceIndex = -1;
					_isInternalMatch = isInternalMatch;
				}

				public TopologyFace Current { get { return new TopologyFace(_topology, _currentFaceIndex); } }

				public bool MoveNext()
				{
					if (_currentFaceIndex < _topology.faceCount)
					{
						do
						{
							if (++_currentFaceIndex >= _topology.faceCount) return false;
						} while (_topology.IsFaceInternal(_currentFaceIndex) != _isInternalMatch);
						return true;
					}
					else
					{
						return false;
					}
				}

				public void Reset()
				{
					_currentFaceIndex = -1;
				}

				object IEnumerator.Current { get { return Current; } }
				bool IEnumerator.MoveNext() { return MoveNext(); }
				void IEnumerator.Reset() { Reset(); }
				void IDisposable.Dispose() { }
			}

			public FaceEnumerator GetEnumerator()
			{
				return new FaceEnumerator(_topology, _isInternalMatch);
			}

			IEnumerator<TopologyFace> IEnumerable<TopologyFace>.GetEnumerator()
			{
				return GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public override bool Equals(object other) { return other is TopologyFace && _index == ((TopologyFace)other)._index && ReferenceEquals(_topology, ((TopologyFace)other)._topology); }
		public override int GetHashCode() { return _topology.GetHashCode() ^ _index.GetHashCode(); }

		public bool Equals(TopologyFace other) { return _index == other._index && ReferenceEquals(_topology,  other._topology); }
		public int CompareTo(TopologyFace other) { return _index - other._index; }

		public static bool operator ==(TopologyFace lhs, TopologyFace rhs) { return lhs._index == rhs._index; }
		public static bool operator !=(TopologyFace lhs, TopologyFace rhs) { return lhs._index != rhs._index; }
		public static bool operator < (TopologyFace lhs, TopologyFace rhs) { return lhs._index <  rhs._index; }
		public static bool operator <=(TopologyFace lhs, TopologyFace rhs) { return lhs._index <= rhs._index; }
		public static bool operator > (TopologyFace lhs, TopologyFace rhs) { return lhs._index >  rhs._index; }
		public static bool operator >=(TopologyFace lhs, TopologyFace rhs) { return lhs._index >= rhs._index; }

		public override string ToString()
		{
			var sb = new System.Text.StringBuilder();
			sb.AppendFormat(isInternal ? "Topology Face {0}" : "Face (external) {0}", _index);

			if (neighborCount > 0)
			{
				var edge = firstEdge;
				sb.AppendFormat("; Neighbors ({0}) {{ Face {1} and Node {2} by Edge {3}", neighborCount, edge.face.index, edge.nextNode.index, edge.index);
				for (int i = 1; i < neighborCount; ++i)
				{
					edge = edge.next;
					sb.AppendFormat(", Face {0} and Node {1} by Edge {2}", edge.face.index, edge.nextNode.index, edge.index);
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

	public struct TopologyEdge
	{
		private ITopology _topology;
		private int _index;

		public TopologyEdge(ITopology topology, int index)
		{
			_topology = topology;
			_index = index;
		}

		public static TopologyEdge none { get { return new TopologyEdge(); } }

		public ITopology topology { get { return _topology; } }
		public int index { get { return _index; } }
		public static implicit operator bool(TopologyEdge edge) { return edge._topology != null && edge._index >= 0; }

		public static explicit operator TopologyNodeEdge(TopologyEdge edge) { return new TopologyNodeEdge(edge._topology, edge._index); }
		public static explicit operator TopologyFaceEdge(TopologyEdge edge) { return new TopologyFaceEdge(edge._topology, edge._index); }

		public TopologyEdge twin { get { return new TopologyEdge(_topology, _index ^ 1); } }
		public TopologyEdge firstTwin { get { return new TopologyEdge(_topology, _index & ~1); } }
		public TopologyEdge secondTwin { get { return new TopologyEdge(_topology, _index | 1); } }

		public bool isFirstTwin { get { return (_index & 1) == 0; } }
		public bool isSecondTwin { get { return (_index & 1) == 1; } }

		public TopologyEdge left { get { return new TopologyEdge(_topology, _topology.GetEdgeNextChainedEdgeIndex(_index ^ 1)); } }
		public TopologyEdge right { get { return new TopologyEdge(_topology, _topology.GetEdgeNextLateralEdgeIndex(_index)); } }

		public TopologyEdge backward { get { return new TopologyEdge(_topology, _topology.GetEdgeNextLateralEdgeIndex(_index) ^ 1); } }
		public TopologyEdge forward { get { return new TopologyEdge(_topology, _topology.GetEdgeNextChainedEdgeIndex(_index)); } }

		public TopologyNode node { get { return new TopologyNode(_topology, _topology.GetEdgeTargetNodeIndex(_index)); } }

		public TopologyNode sourceNode { get { return new TopologyNode(_topology, _topology.GetEdgeTargetNodeIndex(_index ^ 1)); } }
		public TopologyNode targetNode { get { return new TopologyNode(_topology, _topology.GetEdgeTargetNodeIndex(_index)); } }

		public TopologyFace face { get { return new TopologyFace(_topology, _topology.GetEdgeTargetFaceIndex(_index)); } }

		public TopologyFace sourceFace { get { return new TopologyFace(_topology, _topology.GetEdgeTargetFaceIndex(_index ^ 1)); } }
		public TopologyFace targetFace { get { return new TopologyFace(_topology, _topology.GetEdgeTargetFaceIndex(_index)); } }

		public EdgeWrap wrap { get { return _topology.GetEdgeWrap(_index); } }

		public bool isBoundary { get { return targetFace.isExternal != sourceFace.isExternal; } }
		public bool isNonBoundary { get { return targetFace.isExternal == sourceFace.isExternal; } }
		public bool isOuterBoundary { get { return targetFace.isExternal && sourceFace.isInternal; } }
		public bool isInnerBoundary { get { return targetFace.isInternal && sourceFace.isExternal; } }

		//TODO: This seems messed up, especially since a boundary edge will be either simultaneously both internal and external, or neither.
		public bool isInternal { get { return sourceFace.isInternal; } }
		public bool isExternal { get { return targetFace.isExternal; } }

		public struct EdgesIndexer : IEnumerable<TopologyEdge>
		{
			private ITopology _topology;

			public EdgesIndexer(ITopology topology)
			{
				_topology = topology;
			}

			public int Count { get { return _topology.edgeCount; } }

			public struct EdgeEnumerator : IEnumerator<TopologyEdge>
			{
				private ITopology _topology;
				private int _currentEdgeIndex;

				public EdgeEnumerator(ITopology topology)
				{
					_topology = topology;
					_currentEdgeIndex = -1;
				}

				public TopologyEdge Current { get { return new TopologyEdge(_topology, _currentEdgeIndex); } }

				public bool MoveNext()
				{
					if (_currentEdgeIndex < _topology.edgeCount)
					{
						return ++_currentEdgeIndex < _topology.edgeCount;
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
				return new EdgeEnumerator(_topology);
			}

			IEnumerator<TopologyEdge> IEnumerable<TopologyEdge>.GetEnumerator()
			{
				return GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public override bool Equals(object other)
		{
			return
				other is TopologyEdge && Equals((TopologyEdge)other) ||
				other is TopologyNodeEdge && Equals((TopologyNodeEdge)other) ||
				other is TopologyFaceEdge && Equals((TopologyFaceEdge)other);
		}

		public override int GetHashCode() { return _topology.GetHashCode() ^ _index.GetHashCode(); }

		public bool Equals(TopologyEdge other) { return _index == other._index && ReferenceEquals(_topology,  other._topology); }
		public int CompareTo(TopologyEdge other) { return _index - other._index; }

		public static bool operator ==(TopologyEdge lhs, TopologyEdge rhs) { return lhs._index == rhs._index; }
		public static bool operator !=(TopologyEdge lhs, TopologyEdge rhs) { return lhs._index != rhs._index; }
		public static bool operator < (TopologyEdge lhs, TopologyEdge rhs) { return lhs._index <  rhs._index; }
		public static bool operator <=(TopologyEdge lhs, TopologyEdge rhs) { return lhs._index <= rhs._index; }
		public static bool operator > (TopologyEdge lhs, TopologyEdge rhs) { return lhs._index >  rhs._index; }
		public static bool operator >=(TopologyEdge lhs, TopologyEdge rhs) { return lhs._index >= rhs._index; }

		public bool Equals(TopologyNodeEdge other) { return _index == other.index && ReferenceEquals(_topology,  other.topology); }
		public int CompareTo(TopologyNodeEdge other) { return _index - other.index; }

		public static bool operator ==(TopologyEdge lhs, TopologyNodeEdge rhs) { return lhs._index == rhs.index; }
		public static bool operator !=(TopologyEdge lhs, TopologyNodeEdge rhs) { return lhs._index != rhs.index; }
		public static bool operator < (TopologyEdge lhs, TopologyNodeEdge rhs) { return lhs._index <  rhs.index; }
		public static bool operator <=(TopologyEdge lhs, TopologyNodeEdge rhs) { return lhs._index <= rhs.index; }
		public static bool operator > (TopologyEdge lhs, TopologyNodeEdge rhs) { return lhs._index >  rhs.index; }
		public static bool operator >=(TopologyEdge lhs, TopologyNodeEdge rhs) { return lhs._index >= rhs.index; }

		public bool Equals(TopologyFaceEdge other) { return _index == other.index && ReferenceEquals(_topology,  other.topology); }
		public int CompareTo(TopologyFaceEdge other) { return _index - other.index; }

		public static bool operator ==(TopologyEdge lhs, TopologyFaceEdge rhs) { return lhs._index == rhs.index; }
		public static bool operator !=(TopologyEdge lhs, TopologyFaceEdge rhs) { return lhs._index != rhs.index; }
		public static bool operator < (TopologyEdge lhs, TopologyFaceEdge rhs) { return lhs._index <  rhs.index; }
		public static bool operator <=(TopologyEdge lhs, TopologyFaceEdge rhs) { return lhs._index <= rhs.index; }
		public static bool operator > (TopologyEdge lhs, TopologyFaceEdge rhs) { return lhs._index >  rhs.index; }
		public static bool operator >=(TopologyEdge lhs, TopologyFaceEdge rhs) { return lhs._index >= rhs.index; }

		public override string ToString()
		{
			return string.Format("Topology Edge {0}; Twin {1}; from Node {2} to Node {3}; from Face {4} to Face {5}; Forward {6}; Right {7}", _index, _index ^ 1, sourceNode.index, targetNode.index, sourceFace.index, targetFace.index, forward.index, right.index);
		}
	}

	public struct TopologyNodeEdge
	{
		private ITopology _topology;
		private int _index;

		public TopologyNodeEdge(ITopology topology, int index)
		{
			_topology = topology;
			_index = index;
		}

		public static TopologyNodeEdge none { get { return new TopologyNodeEdge(); } }

		public ITopology topology { get { return _topology; } }
		public int index { get { return _index; } }
		public static implicit operator bool(TopologyNodeEdge edge) { return edge._topology != null && edge._index >= 0; }

		public static explicit operator TopologyEdge(TopologyNodeEdge edge) { return new TopologyEdge(edge._topology, edge._index); }
		public static explicit operator TopologyFaceEdge(TopologyNodeEdge edge) { return new TopologyFaceEdge(edge._topology, edge._index); }

		public TopologyNodeEdge twin { get { return new TopologyNodeEdge(_topology, _index ^ 1); } }
		public TopologyNodeEdge firstTwin { get { return new TopologyNodeEdge(_topology, _index & ~1); } }
		public TopologyNodeEdge secondTwin { get { return new TopologyNodeEdge(_topology, _index | 1); } }

		public bool isFirstTwin { get { return (_index & 1) == 0; } }
		public bool isSecondTwin { get { return (_index & 1) == 1; } }

		public TopologyNodeEdge prev { get { return new TopologyNodeEdge(_topology, _topology.GetEdgeNextChainedEdgeIndex(_index ^ 1)); } }
		public TopologyNodeEdge next { get { return new TopologyNodeEdge(_topology, _topology.GetEdgeNextLateralEdgeIndex(_index)); } }

		public TopologyEdge left { get { return new TopologyEdge(_topology, _topology.GetEdgeNextChainedEdgeIndex(_index ^ 1)); } }
		public TopologyEdge right { get { return new TopologyEdge(_topology, _topology.GetEdgeNextLateralEdgeIndex(_index)); } }

		public TopologyEdge backward { get { return new TopologyEdge(_topology, _topology.GetEdgeNextLateralEdgeIndex(_index) ^ 1); } }
		public TopologyEdge forward { get { return new TopologyEdge(_topology, _topology.GetEdgeNextChainedEdgeIndex(_index)); } }

		public TopologyNode node { get { return new TopologyNode(_topology, _topology.GetEdgeTargetNodeIndex(_index)); } }

		public TopologyNode sourceNode { get { return new TopologyNode(_topology, _topology.GetEdgeTargetNodeIndex(_index ^ 1)); } }
		public TopologyNode targetNode { get { return new TopologyNode(_topology, _topology.GetEdgeTargetNodeIndex(_index)); } }

		public TopologyFace prevFace { get { return new TopologyFace(_topology, _topology.GetEdgeTargetFaceIndex(_index)); } }
		public TopologyFace nextFace { get { return new TopologyFace(_topology, _topology.GetEdgeTargetFaceIndex(_index ^ 1)); } }

		public EdgeWrap wrap { get { return _topology.GetEdgeWrap(_index); } }

		public bool isBoundary { get { return prevFace.isExternal != nextFace.isExternal; } }
		public bool isNonBoundary { get { return prevFace.isExternal == nextFace.isExternal; } }
		public bool isOuterBoundary { get { return prevFace.isExternal && nextFace.isInternal; } }
		public bool isInnerBoundary { get { return prevFace.isInternal && nextFace.isExternal; } }

		public bool isInternal { get { return nextFace.isInternal; } }
		public bool isExternal { get { return prevFace.isExternal; } }

		public struct EdgesIndexer : IEnumerable<TopologyNodeEdge>
		{
			private ITopology _topology;

			public EdgesIndexer(ITopology topology)
			{
				_topology = topology;
			}

			public int Count { get { return _topology.edgeCount; } }

			public struct EdgeEnumerator : IEnumerator<TopologyNodeEdge>
			{
				private ITopology _topology;
				private int _currentEdgeIndex;

				public EdgeEnumerator(ITopology topology)
				{
					_topology = topology;
					_currentEdgeIndex = -1;
				}

				public TopologyNodeEdge Current { get { return new TopologyNodeEdge(_topology, _currentEdgeIndex); } }

				public bool MoveNext()
				{
					if (_currentEdgeIndex < _topology.edgeCount)
					{
						return ++_currentEdgeIndex < _topology.edgeCount;
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
				return new EdgeEnumerator(_topology);
			}

			IEnumerator<TopologyNodeEdge> IEnumerable<TopologyNodeEdge>.GetEnumerator()
			{
				return GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public override bool Equals(object other)
		{
			return
				other is TopologyEdge && Equals((TopologyEdge)other) ||
				other is TopologyNodeEdge && Equals((TopologyNodeEdge)other) ||
				other is TopologyFaceEdge && Equals((TopologyFaceEdge)other);
		}

		public override int GetHashCode() { return _topology.GetHashCode() ^ _index.GetHashCode(); }

		public bool Equals(TopologyEdge other) { return _index == other.index && ReferenceEquals(_topology,  other.topology); }
		public int CompareTo(TopologyEdge other) { return _index - other.index; }

		public static bool operator ==(TopologyNodeEdge lhs, TopologyEdge rhs) { return lhs._index == rhs.index; }
		public static bool operator !=(TopologyNodeEdge lhs, TopologyEdge rhs) { return lhs._index != rhs.index; }
		public static bool operator < (TopologyNodeEdge lhs, TopologyEdge rhs) { return lhs._index <  rhs.index; }
		public static bool operator <=(TopologyNodeEdge lhs, TopologyEdge rhs) { return lhs._index <= rhs.index; }
		public static bool operator > (TopologyNodeEdge lhs, TopologyEdge rhs) { return lhs._index >  rhs.index; }
		public static bool operator >=(TopologyNodeEdge lhs, TopologyEdge rhs) { return lhs._index >= rhs.index; }

		public bool Equals(TopologyNodeEdge other) { return _index == other._index && ReferenceEquals(_topology,  other._topology); }
		public int CompareTo(TopologyNodeEdge other) { return _index - other._index; }

		public static bool operator ==(TopologyNodeEdge lhs, TopologyNodeEdge rhs) { return lhs._index == rhs._index; }
		public static bool operator !=(TopologyNodeEdge lhs, TopologyNodeEdge rhs) { return lhs._index != rhs._index; }
		public static bool operator < (TopologyNodeEdge lhs, TopologyNodeEdge rhs) { return lhs._index <  rhs._index; }
		public static bool operator <=(TopologyNodeEdge lhs, TopologyNodeEdge rhs) { return lhs._index <= rhs._index; }
		public static bool operator > (TopologyNodeEdge lhs, TopologyNodeEdge rhs) { return lhs._index >  rhs._index; }
		public static bool operator >=(TopologyNodeEdge lhs, TopologyNodeEdge rhs) { return lhs._index >= rhs._index; }

		public bool Equals(TopologyFaceEdge other) { return _index == other.index && ReferenceEquals(_topology,  other.topology); }
		public int CompareTo(TopologyFaceEdge other) { return _index - other.index; }

		public static bool operator ==(TopologyNodeEdge lhs, TopologyFaceEdge rhs) { return lhs._index == rhs.index; }
		public static bool operator !=(TopologyNodeEdge lhs, TopologyFaceEdge rhs) { return lhs._index != rhs.index; }
		public static bool operator < (TopologyNodeEdge lhs, TopologyFaceEdge rhs) { return lhs._index <  rhs.index; }
		public static bool operator <=(TopologyNodeEdge lhs, TopologyFaceEdge rhs) { return lhs._index <= rhs.index; }
		public static bool operator > (TopologyNodeEdge lhs, TopologyFaceEdge rhs) { return lhs._index >  rhs.index; }
		public static bool operator >=(TopologyNodeEdge lhs, TopologyFaceEdge rhs) { return lhs._index >= rhs.index; }

		public override string ToString()
		{
			return string.Format("Topology Node Edge {0}; Twin {1}; from Node {2} to Node {3}; from Face {4} to Face {5}; Forward {6}; Right {7}", _index, _index ^ 1, sourceNode.index, targetNode.index, prevFace.index, nextFace.index, forward.index, right.index);
		}
	}

	public struct TopologyFaceEdge
	{
		private ITopology _topology;
		private int _index;

		public TopologyFaceEdge(ITopology topology, int index)
		{
			_topology = topology;
			_index = index;
		}

		public static TopologyFaceEdge none { get { return new TopologyFaceEdge(); } }

		public ITopology topology { get { return _topology; } }
		public int index { get { return _index; } }
		public static implicit operator bool(TopologyFaceEdge edge) { return edge._topology != null && edge._index >= 0; }

		public static explicit operator TopologyEdge(TopologyFaceEdge edge) { return new TopologyEdge(edge._topology, edge._index); }
		public static explicit operator TopologyNodeEdge(TopologyFaceEdge edge) { return new TopologyNodeEdge(edge._topology, edge._index); }

		public TopologyFaceEdge twin { get { return new TopologyFaceEdge(_topology, _index ^ 1); } }
		public TopologyFaceEdge firstTwin { get { return new TopologyFaceEdge(_topology, _index & ~1); } }
		public TopologyFaceEdge secondTwin { get { return new TopologyFaceEdge(_topology, _index | 1); } }

		public bool isFirstTwin { get { return (_index & 1) == 0; } }
		public bool isSecondTwin { get { return (_index & 1) == 1; } }

		public TopologyFaceEdge prev { get { return new TopologyFaceEdge(_topology, _topology.GetEdgeNextLateralEdgeIndex(_index) ^ 1); } }
		public TopologyFaceEdge next { get { return new TopologyFaceEdge(_topology, _topology.GetEdgeNextChainedEdgeIndex(_index)); } }

		public TopologyEdge left { get { return new TopologyEdge(_topology, _topology.GetEdgeNextChainedEdgeIndex(_index ^ 1)); } }
		public TopologyEdge right { get { return new TopologyEdge(_topology, _topology.GetEdgeNextLateralEdgeIndex(_index)); } }

		public TopologyEdge backward { get { return new TopologyEdge(_topology, _topology.GetEdgeNextLateralEdgeIndex(_index) ^ 1); } }
		public TopologyEdge forward { get { return new TopologyEdge(_topology, _topology.GetEdgeNextChainedEdgeIndex(_index)); } }

		public TopologyFace face { get { return new TopologyFace(_topology, _topology.GetEdgeTargetFaceIndex(_index)); } }

		public TopologyFace sourceFace { get { return new TopologyFace(_topology, _topology.GetEdgeTargetFaceIndex(_index ^ 1)); } }
		public TopologyFace targetFace { get { return new TopologyFace(_topology, _topology.GetEdgeTargetFaceIndex(_index)); } }

		public TopologyNode prevNode { get { return new TopologyNode(_topology, _topology.GetEdgeTargetNodeIndex(_index ^ 1)); } }
		public TopologyNode nextNode { get { return new TopologyNode(_topology, _topology.GetEdgeTargetNodeIndex(_index)); } }

		public EdgeWrap wrap { get { return _topology.GetEdgeWrap(_index); } }

		public bool isBoundary { get { return targetFace.isExternal != sourceFace.isExternal; } }
		public bool isNonBoundary { get { return targetFace.isExternal == sourceFace.isExternal; } }
		public bool isOuterBoundary { get { return targetFace.isExternal && sourceFace.isInternal; } }
		public bool isInnerBoundary { get { return targetFace.isInternal && sourceFace.isExternal; } }

		public bool isInternal { get { return sourceFace.isInternal; } }
		public bool isExternal { get { return targetFace.isExternal; } }

		//TODO: Name?  cutsFace?
		public bool isBridge { get { return _topology.GetEdgeTargetFaceIndex(_index) == _topology.GetEdgeTargetFaceIndex(_index ^ 1); } }

		public struct EdgesIndexer : IEnumerable<TopologyFaceEdge>
		{
			private ITopology _topology;

			public EdgesIndexer(ITopology topology)
			{
				_topology = topology;
			}

			public int Count { get { return _topology.edgeCount; } }

			public struct EdgeEnumerator : IEnumerator<TopologyFaceEdge>
			{
				private ITopology _topology;
				private int _currentEdgeIndex;

				public EdgeEnumerator(ITopology topology)
				{
					_topology = topology;
					_currentEdgeIndex = -1;
				}

				public TopologyFaceEdge Current { get { return new TopologyFaceEdge(_topology, _currentEdgeIndex); } }

				public bool MoveNext()
				{
					if (_currentEdgeIndex < _topology.edgeCount)
					{
						return ++_currentEdgeIndex < _topology.edgeCount;
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
				return new EdgeEnumerator(_topology);
			}

			IEnumerator<TopologyFaceEdge> IEnumerable<TopologyFaceEdge>.GetEnumerator()
			{
				return GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public override bool Equals(object other)
		{
			return
				other is TopologyEdge && Equals((TopologyEdge)other) ||
				other is TopologyNodeEdge && Equals((TopologyNodeEdge)other) ||
				other is TopologyFaceEdge && Equals((TopologyFaceEdge)other);
		}

		public override int GetHashCode() { return _topology.GetHashCode() ^ _index.GetHashCode(); }

		public bool Equals(TopologyEdge other) { return _index == other.index && ReferenceEquals(_topology,  other.topology); }
		public int CompareTo(TopologyEdge other) { return _index - other.index; }

		public static bool operator ==(TopologyFaceEdge lhs, TopologyEdge rhs) { return lhs._index == rhs.index; }
		public static bool operator !=(TopologyFaceEdge lhs, TopologyEdge rhs) { return lhs._index != rhs.index; }
		public static bool operator < (TopologyFaceEdge lhs, TopologyEdge rhs) { return lhs._index <  rhs.index; }
		public static bool operator <=(TopologyFaceEdge lhs, TopologyEdge rhs) { return lhs._index <= rhs.index; }
		public static bool operator > (TopologyFaceEdge lhs, TopologyEdge rhs) { return lhs._index >  rhs.index; }
		public static bool operator >=(TopologyFaceEdge lhs, TopologyEdge rhs) { return lhs._index >= rhs.index; }

		public bool Equals(TopologyNodeEdge other) { return _index == other.index && ReferenceEquals(_topology,  other.topology); }
		public int CompareTo(TopologyNodeEdge other) { return _index - other.index; }

		public static bool operator ==(TopologyFaceEdge lhs, TopologyNodeEdge rhs) { return lhs._index == rhs.index; }
		public static bool operator !=(TopologyFaceEdge lhs, TopologyNodeEdge rhs) { return lhs._index != rhs.index; }
		public static bool operator < (TopologyFaceEdge lhs, TopologyNodeEdge rhs) { return lhs._index <  rhs.index; }
		public static bool operator <=(TopologyFaceEdge lhs, TopologyNodeEdge rhs) { return lhs._index <= rhs.index; }
		public static bool operator > (TopologyFaceEdge lhs, TopologyNodeEdge rhs) { return lhs._index >  rhs.index; }
		public static bool operator >=(TopologyFaceEdge lhs, TopologyNodeEdge rhs) { return lhs._index >= rhs.index; }

		public bool Equals(TopologyFaceEdge other) { return _index == other._index && ReferenceEquals(_topology,  other._topology); }
		public int CompareTo(TopologyFaceEdge other) { return _index - other._index; }

		public static bool operator ==(TopologyFaceEdge lhs, TopologyFaceEdge rhs) { return lhs._index == rhs._index; }
		public static bool operator !=(TopologyFaceEdge lhs, TopologyFaceEdge rhs) { return lhs._index != rhs._index; }
		public static bool operator < (TopologyFaceEdge lhs, TopologyFaceEdge rhs) { return lhs._index <  rhs._index; }
		public static bool operator <=(TopologyFaceEdge lhs, TopologyFaceEdge rhs) { return lhs._index <= rhs._index; }
		public static bool operator > (TopologyFaceEdge lhs, TopologyFaceEdge rhs) { return lhs._index >  rhs._index; }
		public static bool operator >=(TopologyFaceEdge lhs, TopologyFaceEdge rhs) { return lhs._index >= rhs._index; }

		public override string ToString()
		{
			return string.Format("Topology Face Edge {0}; Twin {1}; from Face {2} to Face {3}; from Node {4} to Node {5}; Forward {6}; Right {7}", _index, _index ^ 1, sourceFace.index, targetFace.index, prevNode.index, nextNode.index, forward.index, right.index);
		}
	}
}
