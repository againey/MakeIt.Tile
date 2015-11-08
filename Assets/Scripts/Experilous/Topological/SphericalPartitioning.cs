using UnityEngine;
using System;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public class SphericalPartitioning
	{
		private struct Partition
		{
			public Vector3 _normal;
			public int _underFaceIndex;
			public int _overFaceIndex;

			public Partition(Vector3 normal, int underFaceIndex, int overFaceIndex)
			{
				_normal = normal;
				_underFaceIndex = underFaceIndex;
				_overFaceIndex = overFaceIndex;
			}

			public Partition(Vector3 p0, Vector3 p1, int underFaceIndex, int overFaceIndex)
				: this(Vector3.Cross(p0 - p1, p0).normalized, underFaceIndex, overFaceIndex)
			{
			}

			public Partition(Topology.VertexEdge edge, VertexAttribute<Vector3> vertexPositions)
				: this(vertexPositions[edge.nearVertex], vertexPositions[edge.farVertex], edge.prevFace.index, edge.nextFace.index)
			{
			}

			public bool IsUnder(Vector3 p)
			{
				return Vector3.Dot(_normal, p) < 0f;
			}

			public bool IsOver(Vector3 p)
			{
				return Vector3.Dot(_normal, p) >= 0f;
			}

			public int Compare(Vector3 p, float margin)
			{
				var cosine = Vector3.Dot(_normal, p);
				if (cosine > margin) return 1;
				else if (cosine < -margin) return -1;
				else return 0;
			}

			public void Intersect(Vector3 p0, Vector3 p1, out Vector3 underIntersection, out Vector3 overIntersection)
			{
				var delta = p1 - p0;
				var denominator = Vector3.Dot(_normal, delta);
				if (denominator != 0f)
				{
					var numerator = Vector3.Dot(_normal, p0);
					var t = -numerator / denominator;
					underIntersection = overIntersection = delta * t + p0;
				}
				else
				{
					underIntersection = p0;
					overIntersection = p1;
				}
			}

			public override string ToString()
			{
				return string.Format("{0} ({1}, {2})", _normal, _underFaceIndex, _overFaceIndex);
			}
		}

		private class PartitionNode
		{
			public int _height;
			public int _edgeIndex;
			public Partition _partition;
			public PartitionNode _parent;
			public PartitionNode _under;
			public PartitionNode _over;

			public PartitionNode()
			{
				_height = 0;
				_edgeIndex = -1;
				_partition = new Partition();
				_parent = null;
				_under = null;
				_over = null;
			}

			private PartitionNode(PartitionNode parent, Vector3 normal, int edgeIndex, int underFaceIndex, int overFaceIndex)
			{
				_height = 1;
				_edgeIndex = edgeIndex;
				_partition = new Partition(normal, underFaceIndex, overFaceIndex);
				_parent = parent;
				_under = null;
				_over = null;
			}

			private PartitionNode(PartitionNode parent, Vector3 p0, Vector3 p1, int edgeIndex, int underFaceIndex, int overFaceIndex)
				: this(parent, Vector3.Cross(p0 - p1, p0).normalized, edgeIndex, underFaceIndex, overFaceIndex)
			{
			}

			private PartitionNode(PartitionNode parent, Topology.VertexEdge edge, VertexAttribute<Vector3> vertexPositions)
				: this(parent, vertexPositions[edge.nearVertex], vertexPositions[edge.farVertex], edge.index, edge.prevFace.index, edge.nextFace.index)
			{
			}

			public PartitionNode AddUnder(Topology.VertexEdge edge, VertexAttribute<Vector3> vertexPositions)
			{
				_under = new PartitionNode(this, edge, vertexPositions);
				return _under;
			}

			public PartitionNode AddUnder(Vector3 p0, Vector3 p1, int edgeIndex, int underFaceIndex, int overFaceIndex)
			{
				_under = new PartitionNode(this, p0, p1, edgeIndex, underFaceIndex, overFaceIndex);
				return _under;
			}

			public PartitionNode AddOver(Topology.VertexEdge edge, VertexAttribute<Vector3> vertexPositions)
			{
				_over = new PartitionNode(this, edge, vertexPositions);
				return _over;
			}

			public PartitionNode AddOver(Vector3 p0, Vector3 p1, int edgeIndex, int underFaceIndex, int overFaceIndex)
			{
				_over = new PartitionNode(this, p0, p1, edgeIndex, underFaceIndex, overFaceIndex);
				return _over;
			}

			public void RotateLeft()
			{
				var child = _over;
				var grandchild = _over._under;
				_parent.ReplaceChild(this, child);
				_over = grandchild;
				if (grandchild != null) grandchild._parent = this;
				_parent = child;
				child._under = this;
				RecomputeHeight();
				child.RecomputeHeight();
			}

			public void RotateRight()
			{
				var child = _under;
				var grandchild = _under._over;
				_parent.ReplaceChild(this, child);
				_under = grandchild;
				if (grandchild != null) grandchild._parent = this;
				_parent = child;
				child._over = this;
				RecomputeHeight();
				child.RecomputeHeight();
			}

			public void RotateUnderLeft()
			{
				var parent = _under;
				var child = parent._over;
				var grandchild = child._under;
				_under = child;
				child._parent = this;
				parent._over = grandchild;
				if (grandchild != null) grandchild._parent = parent;
				parent._parent = child;
				child._under = parent;
				parent.RecomputeHeight();
				child.RecomputeHeight();
			}

			public void RotateOverRight()
			{
				var parent = _over;
				var child = parent._under;
				var grandchild = child._over;
				_over = child;
				child._parent = this;
				parent._under = grandchild;
				if (grandchild != null) grandchild._parent = parent;
				parent._parent = child;
				child._over = parent;
				parent.RecomputeHeight();
				child.RecomputeHeight();
			}

			public int balanceFactor
			{
				get
				{
					return (_under != null ? _under._height : 0) - (_over != null ? _over._height : 0);
				}
			}

			public void RecomputeHeight()
			{
				_height = Mathf.Max(_under != null ? _under._height : 0, _over != null ? _over._height : 0) + 1;
			}

			public void ReplaceChild(PartitionNode oldChild, PartitionNode newChild)
			{
				if (ReferenceEquals(_under, oldChild))
				{
					_under = newChild;
				}
				else
				{
					_over = newChild;
				}
				newChild._parent = this;
			}

			public override string ToString()
			{
				return string.Format("PartitionNode ({0}, {1}, {2}), {3}", _height, balanceFactor, _edgeIndex, _partition);
			}
		}

		private Manifold _manifold;
		private Partition[] _partitionBinaryTree;

		public SphericalPartitioning(Manifold manifold)
		{
			_manifold = manifold;

			var edges = manifold.topology.vertexEdges;
			var positions = manifold.vertexPositions;

			PartitionNode partitionRoot = new PartitionNode();

			int edgesProcessed = 0;
			int increment = 1;
			if (edges.Count % 23 != 0) increment = 23;
			else if (edges.Count % 19 != 0) increment = 19;
			else if (edges.Count % 17 != 0) increment = 17;
			else if (edges.Count % 13 != 0) increment = 13;
			else if (edges.Count % 11 != 0) increment = 11;
			else if (edges.Count % 7 != 0) increment = 7;
			else if (edges.Count % 5 != 0) increment = 5;
			else if (edges.Count % 3 != 0) increment = 3;
			else if (edges.Count % 2 != 0) increment = 2;

			int edgeIndex = 0;
			while (edgesProcessed < edges.Count)
			{
				++edgesProcessed;
				if (edges[edgeIndex] < edges[edgeIndex].twin)
				{
					partitionRoot.AddUnder(edges[edgeIndex], positions);
					edgeIndex = (edgeIndex + increment) % edges.Count;
					break;
				}
				edgeIndex = (edgeIndex + increment) % edges.Count;
			}

			while (edgesProcessed < edges.Count)
			{
				++edgesProcessed;
				if (edges[edgeIndex] < edges[edgeIndex].twin)
				{
					PartitionEdge(partitionRoot._under, edges[edgeIndex], positions);
				}
				edgeIndex = (edgeIndex + increment) % edges.Count;
			}

			var deepProbe = partitionRoot._under;
			var deepestIndex = 0;
			while (true)
			{
				if (deepProbe.balanceFactor > 0)
				{
					deepProbe = deepProbe._under;
					deepestIndex = GetUnderIndex(deepestIndex);
				}
				else if (deepProbe._over != null)
				{
					deepProbe = deepProbe._over;
					deepestIndex = GetOverIndex(deepestIndex);
				}
				else if (deepProbe._under != null)
				{
					deepProbe = deepProbe._under;
					deepestIndex = GetUnderIndex(deepestIndex);
				}
				else
				{
					break;
				}
			}

			_partitionBinaryTree = new Partition[deepestIndex + 1];
			var count = BuildCompactTree(partitionRoot._under, 0);

			Debug.LogFormat("{0:n0}, {1}, {2:n0}, {3}, {4}, {5:n0}", edges.Count / 2, Mathf.CeilToInt(Mathf.Log(edges.Count / 2) / Mathf.Log(2f)), _partitionBinaryTree.Length, Mathf.CeilToInt(Mathf.Log(_partitionBinaryTree.Length) / Mathf.Log(2f)), increment, count);
		}

		private void PartitionEdge(PartitionNode parent, Topology.VertexEdge edge, VertexAttribute<Vector3> vertexPositions)
		{
			PartitionEdge(parent, vertexPositions[edge.nearVertex], vertexPositions[edge.farVertex], edge.index, edge.prevFace.index, edge.nextFace.index);
		}

		private bool PartitionEdge(PartitionNode parent, Vector3 p0, Vector3 p1, int edgeIndex, int underFaceIndex, int overFaceIndex)
		{
			var relation = parent._partition.Compare(p0, 0.0001f) * 3 + parent._partition.Compare(p1, 0.0001f);
			Vector3 underIntersection;
			Vector3 overIntersection;
			bool balanced;
			switch (relation)
			{
				case -4: //both points are under
				case -3:
				case -1:
					balanced = PartitionEdgeUnder(parent, p0, p1, edgeIndex, underFaceIndex, overFaceIndex);
					break;

				case -2: //p0 is under and p1 is over
					parent._partition.Intersect(p0, p1, out underIntersection, out overIntersection);
					balanced =
						PartitionEdgeUnder(parent, p0, overIntersection, edgeIndex, underFaceIndex, overFaceIndex) &
						PartitionEdgeOver(parent, underIntersection, p1, edgeIndex, underFaceIndex, overFaceIndex);
					break;

				case 0: //both points are directly on the partition plane
					return true;

				case 2: //p0 is over and p1 is under
					parent._partition.Intersect(p1, p0, out underIntersection, out overIntersection);
					balanced =
						PartitionEdgeUnder(parent, overIntersection, p1, edgeIndex, underFaceIndex, overFaceIndex) &
						PartitionEdgeOver(parent, p0, underIntersection, edgeIndex, underFaceIndex, overFaceIndex);
					break;

				case 1: //both points are over
				case 3:
				case 4:
					balanced = PartitionEdgeOver(parent, p0, p1, edgeIndex, underFaceIndex, overFaceIndex);
					break;

				default: throw new ApplicationException("An unexpected program state was encountered while comparing the relation of an edge to a planar partition.");
			}

			//parent.RecomputeHeight();
			//return true;

			if (!balanced)
			{
				var balanceFactor = parent.balanceFactor;
				if (balanceFactor == 2)
				{
					if (parent._under.balanceFactor == -1)
						parent.RotateUnderLeft();
					parent.RotateRight();
					return true;
				}
				else if (balanceFactor == -2)
				{
					if (parent._over.balanceFactor == 1)
						parent.RotateOverRight();
					parent.RotateLeft();
					return true;
				}
				else
				{
					parent.RecomputeHeight();
					return false;
				}
			}
			else
			{
				return true;
			}
		}

		private bool PartitionEdgeUnder(PartitionNode parent, Vector3 p0, Vector3 p1, int edgeIndex, int underFaceIndex, int overFaceIndex)
		{
			if (parent._under != null)
			{
				return PartitionEdge(parent._under, p0, p1, edgeIndex, underFaceIndex, overFaceIndex);
			}
			else
			{
				parent.AddUnder(p0, p1, edgeIndex, underFaceIndex, overFaceIndex);
				return false;
			}
		}

		private bool PartitionEdgeOver(PartitionNode parent, Vector3 p0, Vector3 p1, int edgeIndex, int underFaceIndex, int overFaceIndex)
		{
			if (parent._over != null)
			{
				return PartitionEdge(parent._over, p0, p1, edgeIndex, underFaceIndex, overFaceIndex);
			}
			else
			{
				parent.AddOver(p0, p1, edgeIndex, underFaceIndex, overFaceIndex);
				return false;
			}
		}

		private int GetParentIndex(int partitionIndex)
		{
			return (partitionIndex - 1) / 2;
		}

		private int GetUnderIndex(int partitionIndex)
		{
			return partitionIndex * 2 + 1;
		}

		private int GetOverIndex(int partitionIndex)
		{
			return partitionIndex * 2 + 2;
		}

		private int GetOverIndexFromUnderIndex(int partitionIndex)
		{
			return partitionIndex + 1;
		}

		private int BuildCompactTree(PartitionNode partitionNode, int partitionIndex)
		{
			int count = 1;
			int underFaceIndex;
			int overFaceIndex;
			if (partitionNode._under != null)
			{
				count += BuildCompactTree(partitionNode._under, GetUnderIndex(partitionIndex));
				underFaceIndex = -1;
			}
			else
			{
				underFaceIndex = partitionNode._partition._underFaceIndex;
			}

			if (partitionNode._over != null)
			{
				count += BuildCompactTree(partitionNode._over, GetOverIndex(partitionIndex));
				overFaceIndex = -1;
			}
			else
			{
				overFaceIndex = partitionNode._partition._overFaceIndex;
			}

			_partitionBinaryTree[partitionIndex] = new Partition(partitionNode._partition._normal, underFaceIndex, overFaceIndex);
			return count;
		}

		private Topology.Face Intersect(Vector3 centerRay, int partitionIndex)
		{
			var partition = _partitionBinaryTree[partitionIndex];
			if (partition.IsUnder(centerRay))
			{
				if (partition._underFaceIndex == -1) return Intersect(centerRay, GetUnderIndex(partitionIndex));
				else return _manifold.topology.faces[partition._underFaceIndex];
			}
			else
			{
				if (partition._overFaceIndex == -1) return Intersect(centerRay, GetOverIndex(partitionIndex));
				else return _manifold.topology.faces[partition._overFaceIndex];
			}
		}

		public Topology.Face Intersect(Vector3 centerRay)
		{
			return Intersect(centerRay, 0);
		}

		public Topology.Face Intersect(Vector3 surfacePoint, Vector3 sphereCenter)
		{
			return Intersect(surfacePoint - sphereCenter, 0);
		}

		public bool Intersect(Ray ray, Vector3 sphereCenter, float sphereRadius, out Topology.Face face)
		{
			var delta = ray.origin - sphereCenter;
			var cosine = Vector3.Dot(ray.direction, delta);
			var deltaLength = delta.magnitude;
			var square = cosine * cosine - deltaLength * deltaLength + sphereRadius * sphereRadius;
			if (square > 0f)
			{
				var squareRoot = Mathf.Sqrt(square);
				var distance0 = -cosine - squareRoot;
				var distance1 = -cosine + squareRoot;
				if (distance0 >= 0f && distance1 >= 0f)
				{
					face = Intersect(delta + ray.direction * Mathf.Min(distance0, distance1));
					return true;
				}
				else if (distance0 >= 0f)
				{
					face = Intersect(delta + ray.direction * distance0);
					return true;
				}
				else if (distance1 >= 0f)
				{
					face = Intersect(delta + ray.direction * distance1);
					return true;
				}
				else
				{
					face = new Topology.Face();
					return false;
				}
			}
			else if (square < 0f)
			{
				face = new Topology.Face();
				return false;
			}
			else
			{
				face = Intersect(delta + ray.direction * -cosine);
				return true;
			}
		}
	}
}
