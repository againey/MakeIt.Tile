/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System;
using Experilous.MakeIt.Utilities;

namespace Experilous.MakeIt.Tile
{
	public class UniversalFaceSpatialPartitioning : ScriptableObject, IFaceSpatialPartitioning
	{
		[Serializable] private struct Partition
		{
			public SerializablePlane _plane;
			public int _underEdgeIndex;
			public int _overEdgeIndex;
			public int _underPartitionIndex;
			public int _overPartitionIndex;

			private Partition(Vector3 normal, Vector3 p0, int underEdgeIndex, int overEdgeIndex)
			{
				_plane = new SerializablePlane(normal, p0);
				_underEdgeIndex = underEdgeIndex;
				_overEdgeIndex = overEdgeIndex;
				_underPartitionIndex = 0;
				_overPartitionIndex = 0;
			}

			public static Partition Create(Topology.FaceEdge edge, IVertexAttribute<Vector3> vertexPositions, Surface surface)
			{
				var prevEdge = edge.prev;
				var vPos0 = vertexPositions[prevEdge];
				var vPos1 = vertexPositions[edge];
				var edgeVector = vPos1 - vPos0;
				var edgeNormal = surface.GetNormal(vPos0);
				var planeNormal = Vector3.Cross(edgeVector, edgeNormal).normalized;
				return new Partition(planeNormal, vPos0, edge.twinIndex, edge.index);
			}

			public bool IsUnder(Vector3 p)
			{
				return !_plane.GetSide(p);
			}

			public bool IsOver(Vector3 p)
			{
				return _plane.GetSide(p);
			}

			public int Compare(Vector3 p, float margin)
			{
				var distance = _plane.GetDistanceToPoint(p);
				if (distance > margin) return 1;
				else if (distance < -margin) return -1;
				else return 0;
			}

			public void Intersect(Vector3 p0, Vector3 p1, out Vector3 underIntersection, out Vector3 overIntersection)
			{
				if (GeometryTools.Intersect(_plane, p0, p1, out underIntersection))
				{
					overIntersection = underIntersection;
				}
				else
				{
					underIntersection = p0;
					overIntersection = p1;
				}
			}

			public override string ToString()
			{
				return string.Format("{0} {1} ({2}, {3})", _plane.normal, _plane.distance, _underEdgeIndex, _overEdgeIndex);
			}
		}

		[SerializeField] protected Topology _topology;
		[SerializeField] protected Surface _surface;
		[SerializeField] private Partition[] _partitionBinaryTree;

		public static UniversalFaceSpatialPartitioning Create(Topology topology, Surface surface, IVertexAttribute<Vector3> vertexPositions)
		{
			return CreateInstance<UniversalFaceSpatialPartitioning>().Initialize(topology, surface, vertexPositions);
		}

		private UniversalFaceSpatialPartitioning Initialize(Topology topology, Surface surface, IVertexAttribute<Vector3> vertexPositions)
		{
			_topology = topology;
			_surface = surface;

			var edges = _topology.faceEdges;
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

			int edgeIndex = (edges.Count * 7) / 16;

			// Find the first edge that will work as the root.  It must be the earlier twin, and should not
			// have any face-to-face wrapping or be on the boundary between an internal face and external faces.
			while (edgesProcessed < edges.Count)
			{
				++edgesProcessed;
				var edge = edges[edgeIndex];
				if (edgeIndex < edge.twinIndex && (edge.wrap & EdgeWrap.FaceToFace) == EdgeWrap.None && edge.isNonBoundary)
				{
					_partitionBinaryTree[0] = Partition.Create(edge, vertexPositions, surface);
					edgeIndex = (edgeIndex + increment) % edges.Count;
					break;
				}
				edgeIndex = (edgeIndex + increment) % edges.Count;
			}

			int nextPartitionIndex = 1;

			// Process all remaining non-wrapping non-boundary edges.  As above, only concentrate on the earlier twins.
			while (edgesProcessed < edges.Count)
			{
				++edgesProcessed;
				var edge = edges[edgeIndex];
				if (edgeIndex < edge.twinIndex && (edge.wrap & EdgeWrap.FaceToFace) == EdgeWrap.None && edge.isNonBoundary)
				{
					PartitionEdge(edge, vertexPositions, ref nextPartitionIndex);
				}
				edgeIndex = (edgeIndex + increment) % edges.Count;
			}

			edgesProcessed = 0;

			// Process all wrapping edges, which should be at or near the leaves of the tree.
			while (edgesProcessed < edges.Count)
			{
				++edgesProcessed;
				var edge = edges[edgeIndex];
				// If the edge has face-to-face wrapping, then don't restrict to only the earlier twin.
				// Both twins should be processed.
				if ((edge.wrap & EdgeWrap.FaceToFace) != EdgeWrap.None && edge.isNonBoundary)
				{
					PartitionEdge(edge, vertexPositions, ref nextPartitionIndex);
				}
				edgeIndex = (edgeIndex + increment) % edges.Count;
			}

			edgesProcessed = 0;

			// Process all external edges, which should be at or near the leaves of the tree.
			// Note that it is critical that boundary edges occur after all wrapping edges.
			// Otherwise, false negatives (hitting an external face when a wrap should have
			// led to an internal face instead) or even infinite loops are possible.
			while (edgesProcessed < edges.Count)
			{
				++edgesProcessed;
				var edge = edges[edgeIndex];
				// If the edge is a boundary edge, then only process it if it's the external
				// edge.  The internal half doesn't need to be processed at all.
				if (edge.isBoundary && edge.isExternal)
				{
					PartitionEdge(edge, vertexPositions, ref nextPartitionIndex);
				}
				edgeIndex = (edgeIndex + increment) % edges.Count;
			}

			if (nextPartitionIndex < _partitionBinaryTree.Length)
			{
				TruncateBinaryTree(nextPartitionIndex);
			}

			return this;
		}

		private void PartitionEdge(Topology.FaceEdge edge, IVertexAttribute<Vector3> vertexPositions, ref int nextPartitionIndex)
		{
			PartitionEdge(0, vertexPositions[edge.prev], vertexPositions[edge], edge, vertexPositions, ref nextPartitionIndex);
		}

		private void PartitionEdge(int partitionIndex, Vector3 p0, Vector3 p1, Topology.FaceEdge edge, IVertexAttribute<Vector3> vertexPositions, ref int nextPartitionIndex)
		{
			var partition = _partitionBinaryTree[partitionIndex];
			var relation = partition.Compare(p0, 0.0001f) * 3 + partition.Compare(p1, 0.0001f);
			Vector3 underIntersection;
			Vector3 overIntersection;
			switch (relation)
			{
				case -4: // Both points are under.
				case -3:
				case -1:
					PartitionEdgeUnder(partitionIndex, p0, p1, edge, vertexPositions, ref nextPartitionIndex);
					break;

				case -2: // p0 is under and p1 is over.
					partition.Intersect(p0, p1, out underIntersection, out overIntersection);
					PartitionEdgeUnder(partitionIndex, p0, overIntersection, edge, vertexPositions, ref nextPartitionIndex);
					PartitionEdgeOver(partitionIndex, underIntersection, p1, edge, vertexPositions, ref nextPartitionIndex);
					break;

				case 0: // Both points are directly on the partition plane.
					break;

				case 2: // p0 is over and p1 is under.
					partition.Intersect(p1, p0, out underIntersection, out overIntersection);
					PartitionEdgeUnder(partitionIndex, overIntersection, p1, edge, vertexPositions, ref nextPartitionIndex);
					PartitionEdgeOver(partitionIndex, p0, underIntersection, edge, vertexPositions, ref nextPartitionIndex);
					break;

				case 1: // Both points are over.
				case 3:
				case 4:
					PartitionEdgeOver(partitionIndex, p0, p1, edge, vertexPositions, ref nextPartitionIndex);
					break;

				default:
					throw new NotImplementedException();
			}
		}

		private void PartitionEdgeUnder(int parentIndex, Vector3 p0, Vector3 p1, Topology.FaceEdge edge, IVertexAttribute<Vector3> vertexPositions, ref int nextPartitionIndex)
		{
			if (_partitionBinaryTree[parentIndex]._underPartitionIndex != 0)
			{
				PartitionEdge(_partitionBinaryTree[parentIndex]._underPartitionIndex, p0, p1, edge, vertexPositions, ref nextPartitionIndex);
			}
			else
			{
				_partitionBinaryTree[parentIndex]._underPartitionIndex = nextPartitionIndex;
				if (nextPartitionIndex >= _partitionBinaryTree.Length) ExtendBinaryTree();
				_partitionBinaryTree[nextPartitionIndex] = Partition.Create(edge, vertexPositions, _surface);
				++nextPartitionIndex;
			}
		}

		private void PartitionEdgeOver(int parentIndex, Vector3 p0, Vector3 p1, Topology.FaceEdge edge, IVertexAttribute<Vector3> vertexPositions, ref int nextPartitionIndex)
		{
			if (_partitionBinaryTree[parentIndex]._overPartitionIndex != 0)
			{
				PartitionEdge(_partitionBinaryTree[parentIndex]._overPartitionIndex, p0, p1, edge, vertexPositions, ref nextPartitionIndex);
			}
			else
			{
				_partitionBinaryTree[parentIndex]._overPartitionIndex = nextPartitionIndex;
				if (nextPartitionIndex >= _partitionBinaryTree.Length) ExtendBinaryTree();
				_partitionBinaryTree[nextPartitionIndex] = Partition.Create(edge, vertexPositions, _surface);
				++nextPartitionIndex;
			}
		}

		private void ExtendBinaryTree()
		{
			var extendedPartitionBinaryTree = new Partition[_partitionBinaryTree.Length * 2];
			Array.Copy(_partitionBinaryTree, extendedPartitionBinaryTree, _partitionBinaryTree.Length);
			_partitionBinaryTree = extendedPartitionBinaryTree;
		}

		private void TruncateBinaryTree(int length)
		{
			var truncatedPartitionBinaryTree = new Partition[length];
			Array.Copy(_partitionBinaryTree, truncatedPartitionBinaryTree, length);
			_partitionBinaryTree = truncatedPartitionBinaryTree;
		}

		public Topology.Face FindFace(Vector3 point)
		{
			var partitionIndex = 0;
			while (true)
			{
				var partition = _partitionBinaryTree[partitionIndex];
				if (partition.IsOver(point))
				{
					if (partition._overPartitionIndex != 0)
					{
						partitionIndex = partition._overPartitionIndex;
					}
					else
					{
						var edge = _topology.faceEdges[partition._overEdgeIndex];
						if ((edge.wrap & EdgeWrap.FaceToFace) == EdgeWrap.None || edge.isBoundary)
						{
							return _topology.faceEdges[partition._overEdgeIndex].farFace;
						}
						else
						{
							point = _surface.ReverseOffsetFaceToFaceAttribute(point, edge.wrap);
							partitionIndex = 0;
						}
					}
				}
				else
				{
					if (partition._underPartitionIndex != 0)
					{
						partitionIndex = partition._underPartitionIndex;
					}
					else
					{
						return _topology.faceEdges[partition._underEdgeIndex].farFace;
					}
				}
			}
		}

		public Topology.Face FindFace(Ray ray)
		{
			Vector3 intersection;
			if (_surface.Intersect(ray, out intersection))
			{
				return FindFace(intersection);
			}
			else
			{
				return new Topology.Face();
			}
		}

		public Topology.Face FindFace(ScaledRay ray)
		{
			Vector3 intersection;
			if (_surface.Intersect(ray, out intersection))
			{
				return FindFace(intersection);
			}
			else
			{
				return new Topology.Face();
			}
		}
	}
}
