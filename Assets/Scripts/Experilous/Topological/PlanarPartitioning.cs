using UnityEngine;
using System;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[Serializable]
	public class PlanarPartitioning
	{
		[Serializable]
		private struct Partition
		{
			public Vector2 _position;
			public Vector2 _normal;
			public int _underFaceIndex;
			public int _overFaceIndex;
			public int _underPartitionIndex;
			public int _overPartitionIndex;

			private static Vector2 GetNormal(Vector2 v)
			{
				v.Normalize();
				return new Vector2(v.y, -v.x);
			}

			private Partition(Vector2 position, Vector2 normal, int underFaceIndex, int overFaceIndex)
			{
				_position = position;
				_normal = normal;
				_underFaceIndex = underFaceIndex;
				_overFaceIndex = overFaceIndex;
				_underPartitionIndex = 0;
				_overPartitionIndex = 0;
			}

			private Partition(Vector3 p0, Vector3 p1, int underFaceIndex, int overFaceIndex)
				: this(IgnoreZ(p0), GetNormal(IgnoreZ(p1) - IgnoreZ(p0)), underFaceIndex, overFaceIndex)
			{
			}

			public Partition(Topology.VertexEdge edge, Vector3[] vertexPositions)
				: this(vertexPositions[edge.nearVertex], vertexPositions[edge.farVertex], edge.prevFace.index, edge.nextFace.index)
			{
			}

			public static Partition FromEndPoints(Vector2 p0, Vector2 p1, int underFaceIndex, int overFaceIndex)
			{
				return new Partition(p0, GetNormal(p1 - p0), underFaceIndex, overFaceIndex);
			}

			public static Partition FromEndPoints(Vector3 p0, Vector3 p1, int underFaceIndex, int overFaceIndex)
			{
				return new Partition(IgnoreZ(p0), GetNormal(IgnoreZ(p1) - IgnoreZ(p0)), underFaceIndex, overFaceIndex);
			}

			public bool IsUnder(Vector2 p)
			{
				return Vector2.Dot(_normal, p - _position) < 0f;
			}

			public bool IsOver(Vector2 p)
			{
				return Vector2.Dot(_normal, p - _position) >= 0f;
			}

			public int Compare(Vector2 p, float margin)
			{
				var cosine = Vector2.Dot(_normal, p - _position);
				if (cosine > margin) return 1;
				else if (cosine < -margin) return -1;
				else return 0;
			}

			public void Intersect(Vector2 p0, Vector2 p1, out Vector2 underIntersection, out Vector2 overIntersection)
			{
				var delta = p1 - p0;
				var denominator = Vector2.Dot(_normal, delta);
				if (denominator != 0f)
				{
					var numerator = Vector3.Dot(_normal, p0 - _position);
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
				return string.Format("{0} {1} ({2}, {3})", _normal, _position, _underFaceIndex, _overFaceIndex);
			}
		}

		[SerializeField]
		private Manifold _manifold;

		[SerializeField]
		private Partition[] _partitionBinaryTree;

		public PlanarPartitioning(Manifold manifold)
		{
			_manifold = manifold;

			var edges = manifold.vertexEdges;
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
		}

		private static Vector2 IgnoreZ(Vector3 v)
		{
			return new Vector2(v.x, v.y);
		}

		private int GetHeight(int partitionIndex)
		{
			var partition = _partitionBinaryTree[partitionIndex];
			return 1 + Mathf.Max(
				partition._underPartitionIndex != 0 ? GetHeight(partition._underPartitionIndex) : 0,
				partition._overPartitionIndex != 0 ? GetHeight(partition._overPartitionIndex) : 0);
		}

		private void PartitionEdge(Topology.VertexEdge edge, Vector3[] vertexPositions, ref int nextPartitionIndex)
		{
			PartitionEdge(0, IgnoreZ(vertexPositions[edge.nearVertex]), IgnoreZ(vertexPositions[edge.farVertex]), edge.index, edge.prevFace.index, edge.nextFace.index, ref nextPartitionIndex);
		}

		private void PartitionEdge(int partitionIndex, Vector2 p0, Vector2 p1, int edgeIndex, int underFaceIndex, int overFaceIndex, ref int nextPartitionIndex)
		{
			var partition = _partitionBinaryTree[partitionIndex];
			var relation = partition.Compare(p0, 0.0001f) * 3 + partition.Compare(p1, 0.0001f);
			Vector2 underIntersection;
			Vector2 overIntersection;
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

		private void PartitionEdgeUnder(int parentIndex, Vector2 p0, Vector2 p1, int edgeIndex, int underFaceIndex, int overFaceIndex, ref int nextPartitionIndex)
		{
			if (_partitionBinaryTree[parentIndex]._underPartitionIndex != 0)
			{
				PartitionEdge(_partitionBinaryTree[parentIndex]._underPartitionIndex, p0, p1, edgeIndex, underFaceIndex, overFaceIndex, ref nextPartitionIndex);
			}
			else
			{
				_partitionBinaryTree[parentIndex]._underPartitionIndex = nextPartitionIndex;
				if (nextPartitionIndex >= _partitionBinaryTree.Length) ExtendBinaryTree();
				_partitionBinaryTree[nextPartitionIndex] = Partition.FromEndPoints(p0, p1, underFaceIndex, overFaceIndex);
				++nextPartitionIndex;
			}
		}

		private void PartitionEdgeOver(int parentIndex, Vector2 p0, Vector2 p1, int edgeIndex, int underFaceIndex, int overFaceIndex, ref int nextPartitionIndex)
		{
			if (_partitionBinaryTree[parentIndex]._overPartitionIndex != 0)
			{
				PartitionEdge(_partitionBinaryTree[parentIndex]._overPartitionIndex, p0, p1, edgeIndex, underFaceIndex, overFaceIndex, ref nextPartitionIndex);
			}
			else
			{
				_partitionBinaryTree[parentIndex]._overPartitionIndex = nextPartitionIndex;
				if (nextPartitionIndex >= _partitionBinaryTree.Length) ExtendBinaryTree();
				_partitionBinaryTree[nextPartitionIndex] = Partition.FromEndPoints(p0, p1, underFaceIndex, overFaceIndex);
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

		private Topology.Face Intersect(Vector2 point, int partitionIndex)
		{
			var partition = _partitionBinaryTree[partitionIndex];
			if (partition.IsUnder(point))
			{
				if (partition._underPartitionIndex != 0) return Intersect(point, partition._underPartitionIndex);
				else return _manifold.internalFaces[partition._underFaceIndex];
			}
			else
			{
				if (partition._overPartitionIndex != 0) return Intersect(point, partition._overPartitionIndex);
				else return _manifold.internalFaces[partition._overFaceIndex];
			}
		}

		public Topology.Face Intersect(Vector2 point)
		{
			return Intersect(point, 0);
		}
	}
}
