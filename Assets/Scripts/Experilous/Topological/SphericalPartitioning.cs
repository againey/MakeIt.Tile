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

		private Manifold _manifold;
		private Partition[] _partitionBinaryTree;

		public SphericalPartitioning(Manifold manifold)
		{
			_manifold = manifold;

			var edges = manifold.topology.vertexEdges;
			var positions = manifold.vertexPositions;
			Mathf.NextPowerOfTwo(edges.Count);
			_partitionBinaryTree = new Partition[Mathf.NextPowerOfTwo(edges.Count)];

			int i = 0;
			while (i < edges.Count)
			{
				if (edges[i] < edges[i].twin)
				{
					_partitionBinaryTree[0] = new Partition(edges[i], positions);
					++i;
					break;
				}
				++i;
			}

			while (i < edges.Count)
			{
				if (edges[i] < edges[i].twin)
				{
					PartitionEdge(edges[i], positions);
				}
				++i;
			}
		}

		private int GetUnderIndex(int partitionIndex)
		{
			return partitionIndex * 2 + 1;
		}

		private int GetOverIndex(int partitionIndex)
		{
			return partitionIndex * 2 + 2;
		}

		private void PartitionEdge(Topology.VertexEdge edge, VertexAttribute<Vector3> vertexPositions)
		{
			PartitionEdge(0, vertexPositions[edge.nearVertex], vertexPositions[edge.farVertex], edge.prevFace.index, edge.nextFace.index);
		}

		private void PartitionEdge(int parentIndex, Vector3 p0, Vector3 p1, int underFaceIndex, int overFaceIndex)
		{
			var parent = _partitionBinaryTree[parentIndex];
			var relation = parent.Compare(p0, 0.0001f) * 3 + parent.Compare(p1, 0.0001f);
			Vector3 underIntersection;
			Vector3 overIntersection;
			switch (relation)
			{
				case -4: //both points are under
				case -3:
				case -1:
					if (parent._underFaceIndex == -1)
					{
						PartitionEdge(GetUnderIndex(parentIndex), p0, p1, underFaceIndex, overFaceIndex);
					}
					else
					{
						PartitionEdgeUnder(parentIndex, p0, p1, underFaceIndex, overFaceIndex);
					}
					break;

				case -2: //p0 is under and p1 is over
					parent.Intersect(p0, p1, out underIntersection, out overIntersection);
					if (parent._underFaceIndex == -1)
					{
						PartitionEdge(GetUnderIndex(parentIndex), p0, overIntersection, underFaceIndex, overFaceIndex);
					}
					else
					{
						PartitionEdgeUnder(parentIndex, p0, overIntersection, underFaceIndex, overFaceIndex);
					}

					if (parent._overFaceIndex == -1)
					{
						PartitionEdge(GetOverIndex(parentIndex), underIntersection, p1, underFaceIndex, overFaceIndex);
					}
					else
					{
						PartitionEdgeOver(parentIndex, underIntersection, p1, underFaceIndex, overFaceIndex);
					}
					break;

				case 2: //p0 is over and p1 is under
					parent.Intersect(p1, p0, out underIntersection, out overIntersection);
					if (parent._underFaceIndex == -1)
					{
						PartitionEdge(GetUnderIndex(parentIndex), overIntersection, p1, underFaceIndex, overFaceIndex);
					}
					else
					{
						PartitionEdgeUnder(parentIndex, overIntersection, p1, underFaceIndex, overFaceIndex);
					}

					if (parent._overFaceIndex == -1)
					{
						PartitionEdge(GetOverIndex(parentIndex), p0, underIntersection, underFaceIndex, overFaceIndex);
					}
					else
					{
						PartitionEdgeOver(parentIndex, p0, underIntersection, underFaceIndex, overFaceIndex);
					}
					break;

				case 1: //both points are over
				case 3:
				case 4:
					if (parent._overFaceIndex == -1)
					{
						PartitionEdge(GetOverIndex(parentIndex), p0, p1, underFaceIndex, overFaceIndex);
					}
					else
					{
						PartitionEdgeOver(parentIndex, p0, p1, underFaceIndex, overFaceIndex);
					}
					break;
			}
		}

		private void PartitionEdgeUnder(int parentIndex, Vector3 p0, Vector3 p1, int underFaceIndex, int overFaceIndex)
		{
			_partitionBinaryTree[parentIndex]._underFaceIndex = -1;
			var childIndex = GetUnderIndex(parentIndex);
			if (childIndex >= _partitionBinaryTree.Length) ExtendBinaryTree();
			_partitionBinaryTree[childIndex] = new Partition(p0, p1, underFaceIndex, overFaceIndex);
		}

		private void PartitionEdgeOver(int parentIndex, Vector3 p0, Vector3 p1, int underFaceIndex, int overFaceIndex)
		{
			_partitionBinaryTree[parentIndex]._overFaceIndex = -1;
			var childIndex = GetOverIndex(parentIndex);
			if (childIndex >= _partitionBinaryTree.Length) ExtendBinaryTree();
			_partitionBinaryTree[childIndex] = new Partition(p0, p1, underFaceIndex, overFaceIndex);
		}

		private void ExtendBinaryTree()
		{
			var extendedPartitionBinaryTree = new Partition[_partitionBinaryTree.Length * 2];
			System.Array.Copy(_partitionBinaryTree, extendedPartitionBinaryTree, _partitionBinaryTree.Length);
			_partitionBinaryTree = extendedPartitionBinaryTree;
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
