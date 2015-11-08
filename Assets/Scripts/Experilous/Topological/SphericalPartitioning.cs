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
			public int _underPartitionIndex;
			public int _overPartitionIndex;

			public Partition(Vector3 normal, int underFaceIndex, int overFaceIndex)
			{
				_normal = normal;
				_underFaceIndex = underFaceIndex;
				_overFaceIndex = overFaceIndex;
				_underPartitionIndex = 0;
				_overPartitionIndex = 0;
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
			_partitionBinaryTree = new Partition[edges.Count];

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
					_partitionBinaryTree[0] = new Partition(edges[edgeIndex], positions);
					edgeIndex = (edgeIndex + increment) % edges.Count;
					break;
				}
				edgeIndex = (edgeIndex + increment) % edges.Count;
			}

			int nextPartitionIndex = 1;

			while (edgesProcessed < edges.Count)
			{
				++edgesProcessed;
				if (edges[edgeIndex] < edges[edgeIndex].twin)
				{
					PartitionEdge(edges[edgeIndex], positions, ref nextPartitionIndex);
				}
				edgeIndex = (edgeIndex + increment) % edges.Count;
			}

			if (nextPartitionIndex < _partitionBinaryTree.Length)
			{
				TruncateBinaryTree(nextPartitionIndex);
			}
			
			//Debug.LogFormat("{0:n0}, {1}, {2}, {3:n0}", edges.Count / 2, Mathf.CeilToInt(Mathf.Log(edges.Count / 2) / Mathf.Log(2f)), GetHeight(0), nextPartitionIndex);
		}

		private int GetHeight(int partitionIndex)
		{
			var partition = _partitionBinaryTree[partitionIndex];
			return 1 + Mathf.Max(
				partition._underPartitionIndex != 0 ? GetHeight(partition._underPartitionIndex) : 0,
				partition._overPartitionIndex != 0 ? GetHeight(partition._overPartitionIndex) : 0);
		}

		private void PartitionEdge(Topology.VertexEdge edge, VertexAttribute<Vector3> vertexPositions, ref int nextPartitionIndex)
		{
			PartitionEdge(0, vertexPositions[edge.nearVertex], vertexPositions[edge.farVertex], edge.index, edge.prevFace.index, edge.nextFace.index, ref nextPartitionIndex);
		}

		private void PartitionEdge(int partitionIndex, Vector3 p0, Vector3 p1, int edgeIndex, int underFaceIndex, int overFaceIndex, ref int nextPartitionIndex)
		{
			var partition = _partitionBinaryTree[partitionIndex];
			var relation = partition.Compare(p0, 0.0001f) * 3 + partition.Compare(p1, 0.0001f);
			Vector3 underIntersection;
			Vector3 overIntersection;
			switch (relation)
			{
				case -4: //both points are under
				case -3:
				case -1:
					PartitionEdgeUnder(partitionIndex, p0, p1, edgeIndex, underFaceIndex, overFaceIndex, ref nextPartitionIndex);
					break;

				case -2: //p0 is under and p1 is over
					partition.Intersect(p0, p1, out underIntersection, out overIntersection);
					PartitionEdgeUnder(partitionIndex, p0, overIntersection, edgeIndex, underFaceIndex, overFaceIndex, ref nextPartitionIndex);
					PartitionEdgeOver(partitionIndex, underIntersection, p1, edgeIndex, underFaceIndex, overFaceIndex, ref nextPartitionIndex);
					break;

				case 0: //both points are directly on the partition plane
					break;

				case 2: //p0 is over and p1 is under
					partition.Intersect(p1, p0, out underIntersection, out overIntersection);
					PartitionEdgeUnder(partitionIndex, overIntersection, p1, edgeIndex, underFaceIndex, overFaceIndex, ref nextPartitionIndex);
					PartitionEdgeOver(partitionIndex, p0, underIntersection, edgeIndex, underFaceIndex, overFaceIndex, ref nextPartitionIndex);
					break;

				case 1: //both points are over
				case 3:
				case 4:
					PartitionEdgeOver(partitionIndex, p0, p1, edgeIndex, underFaceIndex, overFaceIndex, ref nextPartitionIndex);
					break;

				default: throw new ApplicationException("An unexpected program state was encountered while comparing the relation of an edge to a planar partition.");
			}
		}

		private void PartitionEdgeUnder(int parentIndex, Vector3 p0, Vector3 p1, int edgeIndex, int underFaceIndex, int overFaceIndex, ref int nextPartitionIndex)
		{
			if (_partitionBinaryTree[parentIndex]._underPartitionIndex != 0)
			{
				PartitionEdge(_partitionBinaryTree[parentIndex]._underPartitionIndex, p0, p1, edgeIndex, underFaceIndex, overFaceIndex, ref nextPartitionIndex);
			}
			else
			{
				_partitionBinaryTree[parentIndex]._underPartitionIndex = nextPartitionIndex;
				if (nextPartitionIndex >= _partitionBinaryTree.Length) ExtendBinaryTree();
				_partitionBinaryTree[nextPartitionIndex] = new Partition(p0, p1, underFaceIndex, overFaceIndex);
				++nextPartitionIndex;
			}
		}

		private void PartitionEdgeOver(int parentIndex, Vector3 p0, Vector3 p1, int edgeIndex, int underFaceIndex, int overFaceIndex, ref int nextPartitionIndex)
		{
			if (_partitionBinaryTree[parentIndex]._overPartitionIndex != 0)
			{
				PartitionEdge(_partitionBinaryTree[parentIndex]._overPartitionIndex, p0, p1, edgeIndex, underFaceIndex, overFaceIndex, ref nextPartitionIndex);
			}
			else
			{
				_partitionBinaryTree[parentIndex]._overPartitionIndex = nextPartitionIndex;
				if (nextPartitionIndex >= _partitionBinaryTree.Length) ExtendBinaryTree();
				_partitionBinaryTree[nextPartitionIndex] = new Partition(p0, p1, underFaceIndex, overFaceIndex);
				++nextPartitionIndex;
			}
		}

		private void ExtendBinaryTree()
		{
			var extendedPartitionBinaryTree = new Partition[_partitionBinaryTree.Length * 2];
			System.Array.Copy(_partitionBinaryTree, extendedPartitionBinaryTree, _partitionBinaryTree.Length);
			_partitionBinaryTree = extendedPartitionBinaryTree;
		}

		private void TruncateBinaryTree(int length)
		{
			var truncatedPartitionBinaryTree = new Partition[length];
			System.Array.Copy(_partitionBinaryTree, truncatedPartitionBinaryTree, length);
			_partitionBinaryTree = truncatedPartitionBinaryTree;
		}

		private Topology.Face Intersect(Vector3 centerRay, int partitionIndex)
		{
			var partition = _partitionBinaryTree[partitionIndex];
			if (partition.IsUnder(centerRay))
			{
				if (partition._underPartitionIndex != 0) return Intersect(centerRay, partition._underPartitionIndex);
				else return _manifold.topology.faces[partition._underFaceIndex];
			}
			else
			{
				if (partition._overPartitionIndex != 0) return Intersect(centerRay, partition._overPartitionIndex);
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
