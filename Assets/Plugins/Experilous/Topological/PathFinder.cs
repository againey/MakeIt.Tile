/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

namespace Experilous.Topological
{
	public interface IFacePath : IList<Topology.Face>
	{
	}

	public interface IFaceEdgePath : IList<Topology.FaceEdge>
	{
		IFacePath AsFacePath();
	}

	public class PathFinder
	{
		public delegate float FaceCostHeuristicDelegate(Topology.Face source, Topology.Face target, int pathLength);
		public delegate float FaceCostDelegate(Topology.FaceEdge edge, int pathLength);

		private PriorityQueue<Node> _queue;
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

			public Node(int faceIndex, int edgeIndex, int fromIndex)
			{
				_f = 0f;
				_g = 0f;
				_h = 0f;
				_elementIndex = faceIndex;
				_edgeIndex = edgeIndex;
				_fromIndex = fromIndex;
				_length = 0;
			}

			public Node(float f, float g, float h, int faceIndex, int edgeIndex, int fromIndex, int length)
			{
				_f = f;
				_g = g;
				_h = h;
				_elementIndex = faceIndex;
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

		private class FaceEdgePath : IFaceEdgePath
		{
			private Topology _topology;
			private int[] _edgeIndices;
			private int _length;

			public FaceEdgePath(Topology topology)
			{
				_topology = topology;
			}

			#region IList<Topology.FaceEdge> Implementation

			public bool IsReadOnly { get { return true; } }
			public int Count { get { return _length; } }

			public Topology.FaceEdge this[int index]
			{
				get { return _topology.faceEdges[_edgeIndices[index]]; }
				set { _edgeIndices[index] = value.index; }
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
				var faceEdges = _topology.faceEdges;
				for (int i = 0; i < _length; ++i)
				{
					array[arrayIndex++] = faceEdges[_edgeIndices[i]];
				}
			}

			public IEnumerator<Topology.FaceEdge> GetEnumerator()
			{
				var faceEdges = _topology.faceEdges;
				for (int i = 0; i < _length; ++i)
				{
					yield return faceEdges[_edgeIndices[i]];
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

			private class FacePathAdapter : IFacePath
			{
				private FaceEdgePath _faceEdgePath;

				public FacePathAdapter(FaceEdgePath faceEdgePath)
				{
					_faceEdgePath = faceEdgePath;
				}

				#region IList<Topology.Face> Implementation

				public bool IsReadOnly { get { return true; } }
				public int Count { get { return _faceEdgePath._length > 0 ? _faceEdgePath._length + 1 : 0; } }

				public Topology.Face this[int index]
				{
					get
					{
						if (index != 0)
						{
							return _faceEdgePath._topology.faceEdges[_faceEdgePath._edgeIndices[index - 1]].farFace;
						}
						else
						{
							return _faceEdgePath._topology.faceEdges[_faceEdgePath._edgeIndices[0]].nearFace;
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
					var length = _faceEdgePath._length;
					if (length == 0) return -1;
					var faceEdges = _faceEdgePath._topology.faceEdges;
					var edgeIndices = _faceEdgePath._edgeIndices;
					if (faceEdges[edgeIndices[0]].nearFace == item) return 0;
					for (int i = 0; i < length; ++i)
					{
						if (faceEdges[edgeIndices[i]].farFace == item) return i + 1;
					}
					return -1;
				}

				public void CopyTo(Topology.Face[] array, int arrayIndex)
				{
					var length = _faceEdgePath._length;
					if (length == 0) return;
					var faceEdges = _faceEdgePath._topology.faceEdges;
					var edgeIndices = _faceEdgePath._edgeIndices;
					array[arrayIndex++] = faceEdges[edgeIndices[0]].nearFace;
					for (int i = 0; i < length; ++i)
					{
						array[arrayIndex++] = faceEdges[edgeIndices[i]].farFace;
					}
				}

				public IEnumerator<Topology.Face> GetEnumerator()
				{
					var length = _faceEdgePath._length;
					if (length == 0) yield break;
					var faceEdges = _faceEdgePath._topology.faceEdges;
					var edgeIndices = _faceEdgePath._edgeIndices;
					yield return faceEdges[edgeIndices[0]].nearFace;
					for (int i = 0; i < length; ++i)
					{
						yield return faceEdges[edgeIndices[i]].farFace;
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

			public FaceEdgePath Rebuild()
			{
				_length = 0;
				return this;
			}

			public FaceEdgePath Rebuild(Dictionary<int, Node> closedSet, Node lastNode)
			{
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

				var node = lastNode;

				for (int i = newLength - 1; i > 0; --i)
				{
					_edgeIndices[i] = node._edgeIndex;
					node = closedSet[node._fromIndex];
				}
				_edgeIndices[0] = node._edgeIndex;

				return this;
			}
		}

		public IFaceEdgePath FindPath(Topology.Face source, Topology.Face target, FaceCostHeuristicDelegate costHeuristic, FaceCostDelegate cost, IFaceEdgePath path = null)
		{
			if (!source) throw new ArgumentException("The source face must be a valid face.", "source");
			if (!target) throw new ArgumentException("The target face must be a valid face.", "target");

			var topology = source.topology;

			var concretePath = path as FaceEdgePath;
			if (concretePath == null)
			{
				concretePath = new FaceEdgePath(topology);
			}

			if (target.isExternal) return concretePath.Rebuild();

			if (_queue == null)
			{
				_queue = new PriorityQueue<Node>(Node.AreOrdered, Mathf.CeilToInt(Mathf.Sqrt(source.topology.internalFaces.Count)));
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
					return concretePath.Rebuild(_closedSet, node);
				}

				_closedSet.Add(node._elementIndex, node);
				var face = topology.internalFaces[node._elementIndex];

				foreach (var edge in face.edges)
				{
					if (_closedSet.ContainsKey(edge.farFace.index)) continue;

					var g = node._g + cost(edge, node._length);

					if (!float.IsPositiveInfinity(g))
					{
						Node neighborNode;
						if (!_openSet.TryGetValue(edge.farFace.index, out neighborNode))
						{
							var h = costHeuristic(edge.farFace, target, node._length);
							neighborNode = new Node(g + h, g, h, edge.farFace.index, edge.index, node._elementIndex, node._length + 1);
							_queue.Push(neighborNode);
							_openSet.Add(edge.farFace.index, neighborNode);
						}
						else if (g < neighborNode._g)
						{
							var h = costHeuristic(edge.farFace, target, node._length);
							neighborNode = new Node(g + h, g, h, edge.farFace.index, edge.index, node._elementIndex, node._length + 1);
							_openSet[edge.farFace.index] = neighborNode;
							_queue.Reprioritize(neighborNode);
						}
					}
				}
			}

			return concretePath.Rebuild();
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

		public IFaceEdgePath FindSphericalEuclideanPath(Topology.Face source, Topology.Face target, IFaceAttribute<Vector3> facePositions, float sphereRadius, IFaceEdgePath path = null)
		{
			return FindPath(source, target,
				(Topology.Face s, Topology.Face t, int pathLength) =>
				{
					var sourcePosition = facePositions[s];
					var targetPosition = facePositions[t];
					return GeometryUtility.SphericalArcLength(sourcePosition, targetPosition, sphereRadius);
				},
				(Topology.FaceEdge edge, int pathLength) =>
				{
					if (edge.isOuterBoundary) return float.PositiveInfinity;
					var sourcePosition = facePositions[edge.nearFace];
					var targetPosition = facePositions[edge.farFace];
					return GeometryUtility.SphericalArcLength(sourcePosition, targetPosition, sphereRadius);
				},
				path);
		}
	}
}
