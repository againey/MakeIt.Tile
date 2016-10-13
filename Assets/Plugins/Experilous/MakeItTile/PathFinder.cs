/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using Experilous.Containers;
using Geometry = Experilous.Numerics.Geometry;

namespace Experilous.MakeItTile
{
	public interface IPath
	{
		bool isComplete { get; }
	}

	public interface IVertexPath : IList<Topology.Vertex>, IPath
	{
	}

	public interface IVertexEdgePath : IList<Topology.VertexEdge>, IPath
	{
		Topology.Vertex source { get; }
		Topology.Vertex target { get; }

		IVertexPath AsVertexPath();
	}

	public interface IFacePath : IList<Topology.Face>, IPath
	{
	}

	public interface IFaceEdgePath : IList<Topology.FaceEdge>, IPath
	{
		Topology.Face source { get; }
		Topology.Face target { get; }

		IFacePath AsFacePath();
	}

	public class PathFinder
	{
		public delegate float VertexCostHeuristicDelegate(Topology.Vertex source, Topology.Vertex target, int pathLength);
		public delegate float VertexCostDelegate(Topology.VertexEdge edge, int pathLength);

		public delegate float FaceCostHeuristicDelegate(Topology.Face source, Topology.Face target, int pathLength);
		public delegate float FaceCostDelegate(Topology.FaceEdge edge, int pathLength);

		#region Private A* Types and Data

		private DelegateOrderedPriorityQueue<Node> _queue;
		private Dictionary<int, Node> _openSet;
		private Dictionary<int, Node> _closedSet;

		private struct Node : IEquatable<Node>
		{
			public float _f;
			public float _g;
			public float _h;
			public int _elementIndex;
			public int _edgeIndex;
			public int _fromIndex;
			public int _length;

			public Node(int elementIndex, int edgeIndex, int fromIndex)
			{
				_f = 0f;
				_g = 0f;
				_h = 0f;
				_elementIndex = elementIndex;
				_edgeIndex = edgeIndex;
				_fromIndex = fromIndex;
				_length = 0;
			}

			public Node(float f, float g, float h, int elementIndex, int edgeIndex, int fromIndex, int length)
			{
				_f = f;
				_g = g;
				_h = h;
				_elementIndex = elementIndex;
				_edgeIndex = edgeIndex;
				_fromIndex = fromIndex;
				_length = length;
			}

			public static bool AreOrdered(Node lhs, Node rhs)
			{
				return lhs._f <= rhs._f;
			}

			public bool Equals(Node other) { return _elementIndex == other._elementIndex; }
			public override bool Equals(object other) { return other is Node && _elementIndex == ((Node)other)._elementIndex; }
			public static bool operator ==(Node lhs, Node rhs) { return lhs._elementIndex == rhs._elementIndex; }
			public static bool operator !=(Node lhs, Node rhs) { return lhs._elementIndex != rhs._elementIndex; }

			public override int GetHashCode()
			{
				return _elementIndex.GetHashCode();
			}
		}

		#endregion

		#region Path Implementations

		private class EdgePath
		{
			protected Topology _topology;
			protected int _sourceElementIndex;
			protected int _targetElementIndex;
			protected int[] _edgeIndices;
			protected int _length;

			public bool isComplete
			{
				get
				{
					return _length > 0 || _sourceElementIndex == _targetElementIndex;
				}
			}

			public void Rebuild(Topology topology, int sourceElementIndex, int targetElementIndex)
			{
				_topology = topology;
				_sourceElementIndex = sourceElementIndex;
				_targetElementIndex = targetElementIndex;
				_length = 0;
			}

			public void Rebuild(Topology topology, int sourceElementIndex, int targetElementIndex, Node lastNode, Dictionary<int, Node> closedSet)
			{
				_topology = topology;
				_sourceElementIndex = sourceElementIndex;
				_targetElementIndex = targetElementIndex;

				var newLength = lastNode._length;

				if (_length > newLength)
				{
					_length = newLength;
				}
				else if (_length < newLength)
				{
					_edgeIndices = new int[newLength];
					_length = newLength;
				}

				if (newLength > 0)
				{
					var node = lastNode;

					for (int i = newLength - 1; i > 0; --i)
					{
						_edgeIndices[i] = node._edgeIndex;
						node = closedSet[node._fromIndex];
					}

					_edgeIndices[0] = node._edgeIndex;
				}
			}
		}

		private class VertexEdgePath : EdgePath, IVertexEdgePath
		{
			#region IList<Topology.VertexEdge> Implementation

			public bool IsReadOnly { get { return true; } }
			public int Count { get { return _length; } }

			public Topology.VertexEdge this[int index]
			{
				get { return new Topology.VertexEdge(_topology, _edgeIndices[index]); }
				set { throw new NotSupportedException(); }
			}

			public bool Contains(Topology.VertexEdge item)
			{
				return Array.IndexOf(_edgeIndices, item.index) != -1;
			}

			public int IndexOf(Topology.VertexEdge item)
			{
				return Array.IndexOf(_edgeIndices, item.index);
			}

			public void CopyTo(Topology.VertexEdge[] array, int arrayIndex)
			{
				for (int i = 0; i < _length; ++i)
				{
					array[arrayIndex++] = new Topology.VertexEdge(_topology, _edgeIndices[i]);
				}
			}

			public IEnumerator<Topology.VertexEdge> GetEnumerator()
			{
				for (int i = 0; i < _length; ++i)
				{
					yield return new Topology.VertexEdge(_topology, _edgeIndices[i]);
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public void Add(Topology.VertexEdge item) { throw new NotSupportedException(); }
			public void Insert(int index, Topology.VertexEdge item) { throw new NotSupportedException(); }
			public bool Remove(Topology.VertexEdge item) { throw new NotSupportedException(); }
			public void RemoveAt(int index) { throw new NotSupportedException(); }
			public void Clear() { throw new NotSupportedException(); }

			#endregion

			public Topology.Vertex source
			{
				get
				{
					return new Topology.Vertex(_topology, _sourceElementIndex);
				}
			}

			public Topology.Vertex target
			{
				get
				{
					return new Topology.Vertex(_topology, _targetElementIndex);
				}
			}

			private class VertexPathAdapter : IVertexPath
			{
				private VertexEdgePath _vertexEdgePath;

				public VertexPathAdapter(VertexEdgePath vertexEdgePath)
				{
					_vertexEdgePath = vertexEdgePath;
				}

				public bool isComplete { get { return _vertexEdgePath.isComplete; } }

				#region IList<Topology.Vertex> Implementation

				public bool IsReadOnly { get { return true; } }
				public int Count { get { return _vertexEdgePath.isComplete ? _vertexEdgePath._length + 1 : 0; } }

				public Topology.Vertex this[int index]
				{
					get
					{
						var topology = _vertexEdgePath._topology;
						if (index != 0)
						{
							var edgeIndex = _vertexEdgePath._edgeIndices[index - 1];
							var vertexIndex = topology.edgeData[edgeIndex].vertex;
							return new Topology.Vertex(topology, vertexIndex);
						}
						else
						{
							return new Topology.Vertex(topology, _vertexEdgePath._sourceElementIndex);
						}
					}
					set
					{
						throw new NotSupportedException();
					}
				}

				public bool Contains(Topology.Vertex item)
				{
					return IndexOf(item) != -1;
				}

				public int IndexOf(Topology.Vertex item)
				{
					if (!_vertexEdgePath.isComplete) return -1;
					var itemIndex = item.index;
					if (itemIndex == _vertexEdgePath._sourceElementIndex) return 0;
					var length = _vertexEdgePath._length;
					var edgeIndices = _vertexEdgePath._edgeIndices;
					var edgeData = _vertexEdgePath._topology.edgeData;
					for (int i = 0; i < length; ++i)
					{
						if (edgeData[edgeIndices[i]].vertex == itemIndex) return i + 1;
					}
					return -1;
				}

				public void CopyTo(Topology.Vertex[] array, int arrayIndex)
				{
					if (!_vertexEdgePath.isComplete) return;
					var topology = _vertexEdgePath._topology;
					array[arrayIndex++] = new Topology.Vertex(topology, _vertexEdgePath._sourceElementIndex);
					var length = _vertexEdgePath._length;
					var edgeIndices = _vertexEdgePath._edgeIndices;
					var edgeData = _vertexEdgePath._topology.edgeData;
					for (int i = 0; i < length; ++i)
					{
						array[arrayIndex++] = new Topology.Vertex(topology, edgeData[edgeIndices[i]].vertex);
					}
				}

				public IEnumerator<Topology.Vertex> GetEnumerator()
				{
					if (!_vertexEdgePath.isComplete) yield break;
					var topology = _vertexEdgePath._topology;
					yield return new Topology.Vertex(topology, _vertexEdgePath._sourceElementIndex);
					var length = _vertexEdgePath._length;
					var edgeIndices = _vertexEdgePath._edgeIndices;
					var edgeData = _vertexEdgePath._topology.edgeData;
					for (int i = 0; i < length; ++i)
					{
						yield return new Topology.Vertex(topology, edgeData[edgeIndices[i]].vertex);
					}
				}

				IEnumerator IEnumerable.GetEnumerator()
				{
					return GetEnumerator();
				}

				public void Add(Topology.Vertex item) { throw new NotSupportedException(); }
				public void Insert(int index, Topology.Vertex item) { throw new NotSupportedException(); }
				public bool Remove(Topology.Vertex item) { throw new NotSupportedException(); }
				public void RemoveAt(int index) { throw new NotSupportedException(); }
				public void Clear() { throw new NotSupportedException(); }

				#endregion
			}

			public IVertexPath AsVertexPath()
			{
				return new VertexPathAdapter(this);
			}

			public VertexEdgePath Rebuild(Topology.Vertex source, Topology.Vertex target)
			{
				Rebuild(source.topology, source.index, target.index);
				return this;
			}

			public VertexEdgePath Rebuild(Topology.Vertex source, Topology.Vertex target, Node lastNode, Dictionary<int, Node> closedSet)
			{
				Rebuild(source.topology, source.index, target.index, lastNode, closedSet);
				return this;
			}
		}

		private class FaceEdgePath : EdgePath, IFaceEdgePath
		{
			#region IList<Topology.FaceEdge> Implementation

			public bool IsReadOnly { get { return true; } }
			public int Count { get { return _length; } }

			public Topology.FaceEdge this[int index]
			{
				get { return new Topology.FaceEdge(_topology, _edgeIndices[index]); }
				set { throw new NotSupportedException(); }
			}

			public bool Contains(Topology.FaceEdge item)
			{
				return Array.IndexOf(_edgeIndices, item.index) != -1;
			}

			public int IndexOf(Topology.FaceEdge item)
			{
				return Array.IndexOf(_edgeIndices, item.index);
			}

			public void CopyTo(Topology.FaceEdge[] array, int arrayIndex)
			{
				for (int i = 0; i < _length; ++i)
				{
					array[arrayIndex++] = new Topology.FaceEdge(_topology, _edgeIndices[i]);
				}
			}

			public IEnumerator<Topology.FaceEdge> GetEnumerator()
			{
				for (int i = 0; i < _length; ++i)
				{
					yield return new Topology.FaceEdge(_topology, _edgeIndices[i]);
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public void Add(Topology.FaceEdge item) { throw new NotSupportedException(); }
			public void Insert(int index, Topology.FaceEdge item) { throw new NotSupportedException(); }
			public bool Remove(Topology.FaceEdge item) { throw new NotSupportedException(); }
			public void RemoveAt(int index) { throw new NotSupportedException(); }
			public void Clear() { throw new NotSupportedException(); }

			#endregion

			public Topology.Face source
			{
				get
				{
					return new Topology.Face(_topology, _sourceElementIndex);
				}
			}

			public Topology.Face target
			{
				get
				{
					return new Topology.Face(_topology, _targetElementIndex);
				}
			}

			private class FacePathAdapter : IFacePath
			{
				private FaceEdgePath _faceEdgePath;

				public FacePathAdapter(FaceEdgePath faceEdgePath)
				{
					_faceEdgePath = faceEdgePath;
				}

				public bool isComplete { get { return _faceEdgePath.isComplete; } }

				#region IList<Topology.Face> Implementation

				public bool IsReadOnly { get { return true; } }
				public int Count { get { return _faceEdgePath.isComplete ? _faceEdgePath._length + 1 : 0; } }

				public Topology.Face this[int index]
				{
					get
					{
						var topology = _faceEdgePath._topology;
						if (index != 0)
						{
							var edgeIndex = _faceEdgePath._edgeIndices[index - 1];
							var faceIndex = topology.edgeData[edgeIndex].face;
							return new Topology.Face(topology, faceIndex);
						}
						else
						{
							return new Topology.Face(topology, _faceEdgePath._sourceElementIndex);
						}
					}
					set
					{
						throw new NotSupportedException();
					}
				}

				public bool Contains(Topology.Face item)
				{
					return IndexOf(item) != -1;
				}

				public int IndexOf(Topology.Face item)
				{
					if (!_faceEdgePath.isComplete) return -1;
					var itemIndex = item.index;
					if (itemIndex == _faceEdgePath._sourceElementIndex) return 0;
					var length = _faceEdgePath._length;
					var edgeIndices = _faceEdgePath._edgeIndices;
					var edgeData = _faceEdgePath._topology.edgeData;
					for (int i = 0; i < length; ++i)
					{
						if (edgeData[edgeIndices[i]].face == itemIndex) return i + 1;
					}
					return -1;
				}

				public void CopyTo(Topology.Face[] array, int arrayIndex)
				{
					if (!_faceEdgePath.isComplete) return;
					var topology = _faceEdgePath._topology;
					array[arrayIndex++] = new Topology.Face(topology, _faceEdgePath._sourceElementIndex);
					var length = _faceEdgePath._length;
					var edgeIndices = _faceEdgePath._edgeIndices;
					var edgeData = _faceEdgePath._topology.edgeData;
					for (int i = 0; i < length; ++i)
					{
						array[arrayIndex++] = new Topology.Face(topology, edgeData[edgeIndices[i]].face);
					}
				}

				public IEnumerator<Topology.Face> GetEnumerator()
				{
					if (!_faceEdgePath.isComplete) yield break;
					var topology = _faceEdgePath._topology;
					yield return new Topology.Face(topology, _faceEdgePath._sourceElementIndex);
					var length = _faceEdgePath._length;
					var edgeIndices = _faceEdgePath._edgeIndices;
					var edgeData = _faceEdgePath._topology.edgeData;
					for (int i = 0; i < length; ++i)
					{
						yield return new Topology.Face(topology, edgeData[edgeIndices[i]].face);
					}
				}

				IEnumerator IEnumerable.GetEnumerator()
				{
					return GetEnumerator();
				}

				public void Add(Topology.Face item) { throw new NotSupportedException(); }
				public void Insert(int index, Topology.Face item) { throw new NotSupportedException(); }
				public bool Remove(Topology.Face item) { throw new NotSupportedException(); }
				public void RemoveAt(int index) { throw new NotSupportedException(); }
				public void Clear() { throw new NotSupportedException(); }

				#endregion
			}

			public IFacePath AsFacePath()
			{
				return new FacePathAdapter(this);
			}

			public FaceEdgePath Rebuild(Topology.Face source, Topology.Face target)
			{
				Rebuild(source.topology, source.index, target.index);
				return this;
			}

			public FaceEdgePath Rebuild(Topology.Face source, Topology.Face target, Node lastNode, Dictionary<int, Node> closedSet)
			{
				Rebuild(source.topology, source.index, target.index, lastNode, closedSet);
				return this;
			}
		}

		#endregion

		#region A* Implementations

		public IVertexEdgePath FindPath(Topology.Vertex source, Topology.Vertex target, VertexCostHeuristicDelegate costHeuristic, VertexCostDelegate cost, IVertexEdgePath path = null)
		{
			if (!source) throw new ArgumentException("The source vertex must be a valid vertex.", "source");
			if (!target) throw new ArgumentException("The target vertex must be a valid vertex.", "target");
			if (!source.topology != target.topology) throw new ArgumentException("The target vertex must belong to the same topology as the source vertex.", "target");

			var concretePath = path as VertexEdgePath;
			if (concretePath == null)
			{
				concretePath = new VertexEdgePath();
			}

			var topology = source.topology;

			if (source == target) return concretePath.Rebuild(source, target);

			if (_queue == null)
			{
				_queue = new DelegateOrderedPriorityQueue<Node>(Node.AreOrdered, Mathf.CeilToInt(Mathf.Sqrt(source.topology.vertices.Count)));
				_openSet = new Dictionary<int, Node>();
				_closedSet = new Dictionary<int, Node>();
			}
			else
			{
				_queue.Clear();
				_openSet.Clear();
				_closedSet.Clear();
			}

			_queue.Push(new Node(0f, 0f, costHeuristic(source, target, 0), source.index, -1, -1, 0));
			_openSet.Add(source.index, _queue.front);

			while (_queue.Count > 0)
			{
				var node = _queue.front;
				_queue.Pop();

				if (node._elementIndex == target.index)
				{
					return concretePath.Rebuild(source, target, node, _closedSet);
				}

				_closedSet.Add(node._elementIndex, node);
				var vertex = new Topology.Vertex(topology, node._elementIndex);

				foreach (var edge in vertex.edges)
				{
					if (_closedSet.ContainsKey(edge.vertex.index)) continue;

					var g = node._g + cost(edge, node._length);

					if (!float.IsPositiveInfinity(g))
					{
						Node neighborNode;
						if (!_openSet.TryGetValue(edge.vertex.index, out neighborNode))
						{
							var h = costHeuristic(edge.vertex, target, node._length);
							neighborNode = new Node(g + h, g, h, edge.vertex.index, edge.index, node._elementIndex, node._length + 1);
							_queue.Push(neighborNode);
							_openSet.Add(edge.vertex.index, neighborNode);
						}
						else if (g < neighborNode._g)
						{
							var h = costHeuristic(edge.vertex, target, node._length);
							neighborNode = new Node(g + h, g, h, edge.vertex.index, edge.index, node._elementIndex, node._length + 1);
							_openSet[edge.vertex.index] = neighborNode;
							_queue.Reprioritize(neighborNode);
						}
					}
				}
			}

			return concretePath.Rebuild(source, target);
		}

		public IFaceEdgePath FindPath(Topology.Face source, Topology.Face target, FaceCostHeuristicDelegate costHeuristic, FaceCostDelegate cost, IFaceEdgePath path = null)
		{
			if (!source) throw new ArgumentException("The source face must be a valid face.", "source");
			if (!target) throw new ArgumentException("The target face must be a valid face.", "target");
			if (source.topology != target.topology) throw new ArgumentException("The target face must belong to the same topology as the source face.", "target");

			var concretePath = path as FaceEdgePath;
			if (concretePath == null)
			{
				concretePath = new FaceEdgePath();
			}

			var topology = source.topology;

			if (source == target) return concretePath.Rebuild(source, target);

			if (_queue == null)
			{
				_queue = new DelegateOrderedPriorityQueue<Node>(Node.AreOrdered, Mathf.CeilToInt(Mathf.Sqrt(source.topology.faces.Count)));
				_openSet = new Dictionary<int, Node>();
				_closedSet = new Dictionary<int, Node>();
			}
			else
			{
				_queue.Clear();
				_openSet.Clear();
				_closedSet.Clear();
			}

			_queue.Push(new Node(0f, 0f, costHeuristic(source, target, 0), source.index, -1, -1, 0));
			_openSet.Add(source.index, _queue.front);

			while (_queue.Count > 0)
			{
				var node = _queue.front;
				_queue.Pop();

				if (node._elementIndex == target.index)
				{
					return concretePath.Rebuild(source, target, node, _closedSet);
				}

				_closedSet.Add(node._elementIndex, node);
				var face = new Topology.Face(topology, node._elementIndex);

				foreach (var edge in face.edges)
				{
					if (_closedSet.ContainsKey(edge.face.index)) continue;

					var g = node._g + cost(edge, node._length);

					if (!float.IsPositiveInfinity(g))
					{
						Node neighborNode;
						if (!_openSet.TryGetValue(edge.face.index, out neighborNode))
						{
							var h = costHeuristic(edge.face, target, node._length);
							neighborNode = new Node(g + h, g, h, edge.face.index, edge.index, node._elementIndex, node._length + 1);
							_queue.Push(neighborNode);
							_openSet.Add(edge.face.index, neighborNode);
						}
						else if (g < neighborNode._g)
						{
							var h = costHeuristic(edge.face, target, node._length);
							neighborNode = new Node(g + h, g, h, edge.face.index, edge.index, node._elementIndex, node._length + 1);
							_openSet[edge.face.index] = neighborNode;
							_queue.Reprioritize(neighborNode);
						}
					}
				}
			}

			return concretePath.Rebuild(source, target);
		}

		#endregion

		#region Helper Utilities

		public IVertexEdgePath FindEuclideanPath(Topology.Vertex source, Topology.Vertex target, IVertexAttribute<Vector3> vertexPositions, IVertexEdgePath path = null)
		{
			return FindPath(source, target,
				(Topology.Vertex s, Topology.Vertex t, int pathLength) =>
				{
					return (vertexPositions[s] - vertexPositions[t]).magnitude;
				},
				(Topology.VertexEdge edge, int pathLength) =>
				{
					return (vertexPositions[edge.nearVertex] - vertexPositions[edge.farVertex]).magnitude;
				},
				path);
		}

		public IFaceEdgePath FindEuclideanPath(Topology.Face source, Topology.Face target, IFaceAttribute<Vector3> facePositions, IFaceEdgePath path = null)
		{
			return FindPath(source, target,
				(Topology.Face s, Topology.Face t, int pathLength) =>
				{
					return (facePositions[s] - facePositions[t]).magnitude;
				},
				(Topology.FaceEdge edge, int pathLength) =>
				{
					if (edge.isOuterBoundary) return float.PositiveInfinity;
					return (facePositions[edge.nearFace] - facePositions[edge.farFace]).magnitude;
				},
				path);
		}

		public IVertexEdgePath FindSphericalEuclideanPath(Topology.Vertex source, Topology.Vertex target, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, IVertexEdgePath path = null)
		{
			return FindPath(source, target,
				(Topology.Vertex s, Topology.Vertex t, int pathLength) =>
				{
					var sourcePosition = vertexPositions[s];
					var targetPosition = vertexPositions[t];
					return Geometry.SphericalArcLength(sourcePosition, targetPosition, sphereRadius);
				},
				(Topology.VertexEdge edge, int pathLength) =>
				{
					var sourcePosition = vertexPositions[edge.nearVertex];
					var targetPosition = vertexPositions[edge.farVertex];
					return Geometry.SphericalArcLength(sourcePosition, targetPosition, sphereRadius);
				},
				path);
		}

		public IFaceEdgePath FindSphericalEuclideanPath(Topology.Face source, Topology.Face target, IFaceAttribute<Vector3> facePositions, float sphereRadius, IFaceEdgePath path = null)
		{
			return FindPath(source, target,
				(Topology.Face s, Topology.Face t, int pathLength) =>
				{
					var sourcePosition = facePositions[s];
					var targetPosition = facePositions[t];
					return Geometry.SphericalArcLength(sourcePosition, targetPosition, sphereRadius);
				},
				(Topology.FaceEdge edge, int pathLength) =>
				{
					if (edge.isOuterBoundary) return float.PositiveInfinity;
					var sourcePosition = facePositions[edge.nearFace];
					var targetPosition = facePositions[edge.farFace];
					return Geometry.SphericalArcLength(sourcePosition, targetPosition, sphereRadius);
				},
				path);
		}

		#endregion
	}
}
