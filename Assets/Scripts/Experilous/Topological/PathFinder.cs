using UnityEngine;
using System;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public static class PathFinder
	{
		public delegate float CostHeuristicDelegate(Topology.Face source, Topology.Face target, int pathLength);
		public delegate float CostDelegate(Topology.FaceEdge edge, int pathLength);

		private struct Node : IEquatable<Node>
		{
			public float _f;
			public float _g;
			public float _h;
			public int _faceIndex;
			public int _fromIndex;
			public int _length;

			public Node(int faceIndex, int fromIndex)
			{
				_f = 0f;
				_g = 0f;
				_h = 0f;
				_faceIndex = faceIndex;
				_fromIndex = fromIndex;
				_length = 0;
			}

			public Node(float f, float g, float h, int faceIndex, int fromIndex, int length)
			{
				_f = f;
				_g = g;
				_h = h;
				_faceIndex = faceIndex;
				_fromIndex = fromIndex;
				_length = length;
			}

			public static bool AreOrdered(Node lhs, Node rhs)
			{
				return lhs._f <= rhs._f;
			}

			public bool Equals(Node other) { return _faceIndex == other._faceIndex; }
			public override bool Equals(object other) { return other is Node && _faceIndex == ((Node)other)._faceIndex; }
			public static bool operator ==(Node lhs, Node rhs) { return lhs._faceIndex == rhs._faceIndex; }
			public static bool operator !=(Node lhs, Node rhs) { return lhs._faceIndex != rhs._faceIndex; }

			public override int GetHashCode()
			{
				return _faceIndex.GetHashCode();
			}
		}

		public static int[] FindPath(Topology.Face source, Topology.Face target, CostHeuristicDelegate costHeuristic, CostDelegate cost)
		{
			if (target.isExternal) return new int[0];

			var topology = source.topology;
			var queue = new PriorityQueue<Node>(Node.AreOrdered, Mathf.CeilToInt(Mathf.Sqrt(source.topology.internalFaces.Count)));
			var openSet = new Dictionary<int, Node>();
			var closedSet = new Dictionary<int, Node>();

			queue.Push(new Node(0f, 0f, costHeuristic(source, target, 0), source.index, -1, 0));
			openSet.Add(source.index, queue.front);

			while (queue.Count > 0)
			{
				var node = queue.front;
				queue.Pop();

				if (node._faceIndex == target.index)
				{
					var path = new int[node._length + 1];
					for (int i = node._length; i > 0; --i)
					{
						path[i] = node._faceIndex;
						node = closedSet[node._fromIndex];
					}
					path[0] = node._faceIndex;
					return path;
				}

				closedSet.Add(node._faceIndex, node);
				var face = topology.internalFaces[node._faceIndex];

				foreach (var edge in face.edges)
				{
					if (closedSet.ContainsKey(edge.farFace.index)) continue;

					var g = node._g + cost(edge, node._length);

					if (!float.IsPositiveInfinity(g))
					{
						Node neighborNode;
						if (!openSet.TryGetValue(edge.farFace.index, out neighborNode))
						{
							var h = costHeuristic(edge.farFace, target, node._length);
							neighborNode = new Node(g + h, g, h, edge.farFace.index, node._faceIndex, node._length + 1);
							queue.Push(neighborNode);
							openSet.Add(edge.farFace.index, neighborNode);
						}
						else if (g < neighborNode._g)
						{
							var h = costHeuristic(edge.farFace, target, node._length);
							neighborNode = new Node(g + h, g, h, edge.farFace.index, node._faceIndex, node._length + 1);
							openSet[edge.farFace.index] = neighborNode;
							queue.Reprioritize(neighborNode);
						}
					}
				}
			}

			return null;
		}

		public static int[] FindEuclideanPath(Topology.Face source, Topology.Face target, Vector3[] facePositions)
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
				});
		}

		public static int[] FindSphericalEuclideanPath(Topology.Face source, Topology.Face target, Vector3[] facePositions)
		{
			return FindPath(source, target,
				(Topology.Face s, Topology.Face t, int pathLength) =>
				{
					var sourcePosition = facePositions[s];
					var targetPosition = facePositions[t];
					return SphericalManifold.ArcLength(sourcePosition, targetPosition);
				},
				(Topology.FaceEdge edge, int pathLength) =>
				{
					if (edge.isOuterBoundary) return float.PositiveInfinity;
					var sourcePosition = facePositions[edge.nearFace];
					var targetPosition = facePositions[edge.farFace];
					return SphericalManifold.ArcLength(sourcePosition, targetPosition);
				});
		}
	}
}
