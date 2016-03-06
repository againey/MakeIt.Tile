/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Experilous.Randomization;

namespace Experilous.Topological
{
	public static class FaceVisitationUtility
	{
		#region Helper Types

		public struct DepthVisit
		{
			public delegate bool ShouldVisitDelegate(DepthVisit tentativeVisit);

			public Topology.FaceEdge edge;
			public int depth;

			public DepthVisit(Topology.FaceEdge edge, int depth)
			{
				this.edge = edge;
				this.depth = depth;
			}
		}

		public struct DistanceVisit<T>
		{
			public delegate bool ShouldVisitDelegate(DistanceVisit<T> tentativeVisit);

			public Topology.FaceEdge edge;
			public int depth;
			public T distance;

			public DistanceVisit(Topology.FaceEdge edge, int depth, T distance)
			{
				this.edge = edge;
				this.depth = depth;
				this.distance = distance;
			}
		}

		private struct DepthQueueElement : IEquatable<DepthQueueElement>
		{
			public int edgeIndex;
			public int depth;

			public DepthQueueElement(int edgeIndex, int depth)
			{
				this.edgeIndex = edgeIndex;
				this.depth = depth;
			}

			public bool Equals(DepthQueueElement other)
			{
				return edgeIndex == other.edgeIndex;
			}
		}

		private struct DistanceQueueElement<T> : IEquatable<DistanceQueueElement<T>>
		{
			public int edgeIndex;
			public int depth;
			public T distance;

			public DistanceQueueElement(int edgeIndex, int depth, T distance)
			{
				this.edgeIndex = edgeIndex;
				this.depth = depth;
				this.distance = distance;
			}

			public bool Equals(DistanceQueueElement<T> other)
			{
				return edgeIndex == other.edgeIndex;
			}
		}

		#endregion

		#region GetFaceEdgesByDepth()

		public static IEnumerable<DepthVisit> GetFaceEdgesBreadthFirstByDepth(Topology.Face rootFace)
		{
			return GetFaceEdgesByDepth(
				rootFace,
				(DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; },
				(DepthVisit tentativeVisit) => { return true; });
		}

		public static IEnumerable<DepthVisit> GetFaceEdgesBreadthFirstByDepth(Topology.Face rootFace, int maxDepth)
		{
			return GetFaceEdgesByDepth(
				rootFace,
				(DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; },
				(DepthVisit tentativeVisit) => { return tentativeVisit.depth <= maxDepth; });
		}

		public static IEnumerable<DepthVisit> GetFaceEdgesBreadthFirstByDepth(Topology.Face rootFace, DepthVisit.ShouldVisitDelegate shouldVisit)
		{
			return GetFaceEdgesByDepth(
				rootFace,
				(DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; },
				shouldVisit);
		}

		public static IEnumerable<DepthVisit> GetFaceEdgesDepthFirstByDepth(Topology.Face rootFace)
		{
			return GetFaceEdgesByDepth(
				rootFace,
				(DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; },
				(DepthVisit tentativeVisit) => { return true; });
		}

		public static IEnumerable<DepthVisit> GetFaceEdgesDepthFirstByDepth(Topology.Face rootFace, int maxDepth)
		{
			return GetFaceEdgesByDepth(
				rootFace,
				(DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; },
				(DepthVisit tentativeVisit) => { return tentativeVisit.depth <= maxDepth; });
		}

		public static IEnumerable<DepthVisit> GetFaceEdgesDepthFirstByDepth(Topology.Face rootFace, DepthVisit.ShouldVisitDelegate shouldVisit)
		{
			return GetFaceEdgesByDepth(
				rootFace,
				(DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; },
				shouldVisit);
		}

		private static IEnumerable<DepthVisit> GetFaceEdgesByDepth(
			Topology.Face rootFace,
			PriorityQueue<DepthQueueElement>.AreOrderedDelegate areOrdered,
			DepthVisit.ShouldVisitDelegate shouldVisit)
		{
			var topology = rootFace.topology;
			var queue = new PriorityQueue<DepthQueueElement>(areOrdered, 256);
			BitArray visitedFaces = new BitArray(topology.faces.Count);

			foreach (var edge in rootFace.edges)
			{
				if (!edge.isOuterBoundary)
				{
					queue.Push(new DepthQueueElement(edge.index, 1));
				}
			}

			while (queue.Count > 0)
			{
				var front = queue.front;
				queue.Pop();

				var farFaceIndex = topology.edgeData[front.edgeIndex].face;

				if (visitedFaces[farFaceIndex] == false)
				{
					var visit = new DepthVisit(new Topology.FaceEdge(topology, front.edgeIndex), front.depth);
					if (shouldVisit(visit))
					{
						yield return visit;

						foreach (var edge in new Topology.Face(topology, farFaceIndex).edges)
						{
							if (edge.twinIndex != front.edgeIndex && !edge.isOuterBoundary && visitedFaces[edge.farFace.index] == false)
							{
								queue.Push(new DepthQueueElement(edge.index, front.depth + 1));
							}
						}
					}
				}
			}
		}

		#endregion

		#region GetFaceEdgesByEdgeDistance()

		#region int

		public static IEnumerable<DistanceVisit<int>> GetFaceEdgesBreadthFirstByEdgeDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances)
		{
			return GetFaceEdgesByEdgeDistance(
				rootFace, edgeDistances,
				(DistanceQueueElement<int> lhs, DistanceQueueElement<int> rhs) => { return lhs.distance <= rhs.distance; },
				(DistanceVisit<int> tentativeVisit) => { return true; });
		}

		public static IEnumerable<DistanceVisit<int>> GetFaceEdgesBreadthFirstByEdgeDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, int maxDistance)
		{
			return GetFaceEdgesByEdgeDistance(
				rootFace, edgeDistances,
				(DistanceQueueElement<int> lhs, DistanceQueueElement<int> rhs) => { return lhs.distance <= rhs.distance; },
				(DistanceVisit<int> tentativeVisit) => { return tentativeVisit.distance <= maxDistance; });
		}

		public static IEnumerable<DistanceVisit<int>> GetFaceEdgesBreadthFirstByEdgeDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, DistanceVisit<int>.ShouldVisitDelegate shouldVisit)
		{
			return GetFaceEdgesByEdgeDistance(
				rootFace, edgeDistances,
				(DistanceQueueElement<int> lhs, DistanceQueueElement<int> rhs) => { return lhs.distance <= rhs.distance; },
				shouldVisit);
		}

		public static IEnumerable<DistanceVisit<int>> GetFaceEdgesDepthFirstByEdgeDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances)
		{
			return GetFaceEdgesByEdgeDistance(
				rootFace, edgeDistances,
				(DistanceQueueElement<int> lhs, DistanceQueueElement<int> rhs) => { return lhs.distance >= rhs.distance; },
				(DistanceVisit<int> tentativeVisit) => { return true; });
		}

		public static IEnumerable<DistanceVisit<int>> GetFaceEdgesDepthFirstByEdgeDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, int maxDistance)
		{
			return GetFaceEdgesByEdgeDistance(
				rootFace, edgeDistances,
				(DistanceQueueElement<int> lhs, DistanceQueueElement<int> rhs) => { return lhs.distance >= rhs.distance; },
				(DistanceVisit<int> tentativeVisit) => { return tentativeVisit.distance <= maxDistance; });
		}

		public static IEnumerable<DistanceVisit<int>> GetFaceEdgesDepthFirstByEdgeDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, DistanceVisit<int>.ShouldVisitDelegate shouldVisit)
		{
			return GetFaceEdgesByEdgeDistance(
				rootFace, edgeDistances,
				(DistanceQueueElement<int> lhs, DistanceQueueElement<int> rhs) => { return lhs.distance >= rhs.distance; },
				shouldVisit);
		}

		private static IEnumerable<DistanceVisit<int>> GetFaceEdgesByEdgeDistance(
			Topology.Face rootFace, IEdgeAttribute<int> edgeDistances,
			PriorityQueue<DistanceQueueElement<int>>.AreOrderedDelegate areOrdered,
			DistanceVisit<int>.ShouldVisitDelegate shouldVisit)
		{
			var queue = new PriorityQueue<DistanceQueueElement<int>>(areOrdered, 256);
			Func<int, int, int> addDistances = (int lhs, int rhs) => { return lhs + rhs; };
			return GetFaceEdgesByEdgeDistance(rootFace, edgeDistances, queue, shouldVisit, 0, addDistances);
		}

		#endregion

		#region uint

		public static IEnumerable<DistanceVisit<uint>> GetFaceEdgesBreadthFirstByEdgeDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances)
		{
			return GetFaceEdgesByEdgeDistance(
				rootFace, edgeDistances,
				(DistanceQueueElement<uint> lhs, DistanceQueueElement<uint> rhs) => { return lhs.distance <= rhs.distance; },
				(DistanceVisit<uint> tentativeVisit) => { return true; });
		}

		public static IEnumerable<DistanceVisit<uint>> GetFaceEdgesBreadthFirstByEdgeDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, uint maxDistance)
		{
			return GetFaceEdgesByEdgeDistance(
				rootFace, edgeDistances,
				(DistanceQueueElement<uint> lhs, DistanceQueueElement<uint> rhs) => { return lhs.distance <= rhs.distance; },
				(DistanceVisit<uint> tentativeVisit) => { return tentativeVisit.distance <= maxDistance; });
		}

		public static IEnumerable<DistanceVisit<uint>> GetFaceEdgesBreadthFirstByEdgeDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, DistanceVisit<uint>.ShouldVisitDelegate shouldVisit)
		{
			return GetFaceEdgesByEdgeDistance(
				rootFace, edgeDistances,
				(DistanceQueueElement<uint> lhs, DistanceQueueElement<uint> rhs) => { return lhs.distance <= rhs.distance; },
				shouldVisit);
		}

		public static IEnumerable<DistanceVisit<uint>> GetFaceEdgesDepthFirstByEdgeDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances)
		{
			return GetFaceEdgesByEdgeDistance(
				rootFace, edgeDistances,
				(DistanceQueueElement<uint> lhs, DistanceQueueElement<uint> rhs) => { return lhs.distance >= rhs.distance; },
				(DistanceVisit<uint> tentativeVisit) => { return true; });
		}

		public static IEnumerable<DistanceVisit<uint>> GetFaceEdgesDepthFirstByEdgeDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, uint maxDistance)
		{
			return GetFaceEdgesByEdgeDistance(
				rootFace, edgeDistances,
				(DistanceQueueElement<uint> lhs, DistanceQueueElement<uint> rhs) => { return lhs.distance >= rhs.distance; },
				(DistanceVisit<uint> tentativeVisit) => { return tentativeVisit.distance <= maxDistance; });
		}

		public static IEnumerable<DistanceVisit<uint>> GetFaceEdgesDepthFirstByEdgeDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, DistanceVisit<uint>.ShouldVisitDelegate shouldVisit)
		{
			return GetFaceEdgesByEdgeDistance(
				rootFace, edgeDistances,
				(DistanceQueueElement<uint> lhs, DistanceQueueElement<uint> rhs) => { return lhs.distance >= rhs.distance; },
				shouldVisit);
		}

		private static IEnumerable<DistanceVisit<uint>> GetFaceEdgesByEdgeDistance(
			Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances,
			PriorityQueue<DistanceQueueElement<uint>>.AreOrderedDelegate areOrdered,
			DistanceVisit<uint>.ShouldVisitDelegate shouldVisit)
		{
			var queue = new PriorityQueue<DistanceQueueElement<uint>>(areOrdered, 256);
			Func<uint, uint, uint> addDistances = (uint lhs, uint rhs) => { return lhs + rhs; };
			return GetFaceEdgesByEdgeDistance(rootFace, edgeDistances, queue, shouldVisit, 0U, addDistances);
		}

		#endregion

		#region float

		public static IEnumerable<DistanceVisit<float>> GetFaceEdgesBreadthFirstByEdgeDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances)
		{
			return GetFaceEdgesByEdgeDistance(
				rootFace, edgeDistances,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance <= rhs.distance; },
				(DistanceVisit<float> tentativeVisit) => { return true; });
		}

		public static IEnumerable<DistanceVisit<float>> GetFaceEdgesBreadthFirstByEdgeDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, float maxDistance)
		{
			return GetFaceEdgesByEdgeDistance(
				rootFace, edgeDistances,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance <= rhs.distance; },
				(DistanceVisit<float> tentativeVisit) => { return tentativeVisit.distance <= maxDistance; });
		}

		public static IEnumerable<DistanceVisit<float>> GetFaceEdgesBreadthFirstByEdgeDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, DistanceVisit<float>.ShouldVisitDelegate shouldVisit)
		{
			return GetFaceEdgesByEdgeDistance(
				rootFace, edgeDistances,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance <= rhs.distance; },
				shouldVisit);
		}

		public static IEnumerable<DistanceVisit<float>> GetFaceEdgesDepthFirstByEdgeDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances)
		{
			return GetFaceEdgesByEdgeDistance(
				rootFace, edgeDistances,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance >= rhs.distance; },
				(DistanceVisit<float> tentativeVisit) => { return true; });
		}

		public static IEnumerable<DistanceVisit<float>> GetFaceEdgesDepthFirstByEdgeDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, float maxDistance)
		{
			return GetFaceEdgesByEdgeDistance(
				rootFace, edgeDistances,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance >= rhs.distance; },
				(DistanceVisit<float> tentativeVisit) => { return tentativeVisit.distance <= maxDistance; });
		}

		public static IEnumerable<DistanceVisit<float>> GetFaceEdgesDepthFirstByEdgeDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, DistanceVisit<float>.ShouldVisitDelegate shouldVisit)
		{
			return GetFaceEdgesByEdgeDistance(
				rootFace, edgeDistances,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance >= rhs.distance; },
				shouldVisit);
		}

		private static IEnumerable<DistanceVisit<float>> GetFaceEdgesByEdgeDistance(
			Topology.Face rootFace, IEdgeAttribute<float> edgeDistances,
			PriorityQueue<DistanceQueueElement<float>>.AreOrderedDelegate areOrdered,
			DistanceVisit<float>.ShouldVisitDelegate shouldVisit)
		{
			var queue = new PriorityQueue<DistanceQueueElement<float>>(areOrdered, 256);
			Func<float, float, float> addDistances = (float lhs, float rhs) => { return lhs + rhs; };
			return GetFaceEdgesByEdgeDistance(rootFace, edgeDistances, queue, shouldVisit, 0f, addDistances);
		}

		#endregion

		#region double

		public static IEnumerable<DistanceVisit<double>> GetFaceEdgesBreadthFirstByEdgeDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances)
		{
			return GetFaceEdgesByEdgeDistance(
				rootFace, edgeDistances,
				(DistanceQueueElement<double> lhs, DistanceQueueElement<double> rhs) => { return lhs.distance <= rhs.distance; },
				(DistanceVisit<double> tentativeVisit) => { return true; });
		}

		public static IEnumerable<DistanceVisit<double>> GetFaceEdgesBreadthFirstByEdgeDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, double maxDistance)
		{
			return GetFaceEdgesByEdgeDistance(
				rootFace, edgeDistances,
				(DistanceQueueElement<double> lhs, DistanceQueueElement<double> rhs) => { return lhs.distance <= rhs.distance; },
				(DistanceVisit<double> tentativeVisit) => { return tentativeVisit.distance <= maxDistance; });
		}

		public static IEnumerable<DistanceVisit<double>> GetFaceEdgesBreadthFirstByEdgeDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, DistanceVisit<double>.ShouldVisitDelegate shouldVisit)
		{
			return GetFaceEdgesByEdgeDistance(
				rootFace, edgeDistances,
				(DistanceQueueElement<double> lhs, DistanceQueueElement<double> rhs) => { return lhs.distance <= rhs.distance; },
				shouldVisit);
		}

		public static IEnumerable<DistanceVisit<double>> GetFaceEdgesDepthFirstByEdgeDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances)
		{
			return GetFaceEdgesByEdgeDistance(
				rootFace, edgeDistances,
				(DistanceQueueElement<double> lhs, DistanceQueueElement<double> rhs) => { return lhs.distance >= rhs.distance; },
				(DistanceVisit<double> tentativeVisit) => { return true; });
		}

		public static IEnumerable<DistanceVisit<double>> GetFaceEdgesDepthFirstByEdgeDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, double maxDistance)
		{
			return GetFaceEdgesByEdgeDistance(
				rootFace, edgeDistances,
				(DistanceQueueElement<double> lhs, DistanceQueueElement<double> rhs) => { return lhs.distance >= rhs.distance; },
				(DistanceVisit<double> tentativeVisit) => { return tentativeVisit.distance <= maxDistance; });
		}

		public static IEnumerable<DistanceVisit<double>> GetFaceEdgesDepthFirstByEdgeDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, DistanceVisit<double>.ShouldVisitDelegate shouldVisit)
		{
			return GetFaceEdgesByEdgeDistance(
				rootFace, edgeDistances,
				(DistanceQueueElement<double> lhs, DistanceQueueElement<double> rhs) => { return lhs.distance >= rhs.distance; },
				shouldVisit);
		}

		private static IEnumerable<DistanceVisit<double>> GetFaceEdgesByEdgeDistance(
			Topology.Face rootFace, IEdgeAttribute<double> edgeDistances,
			PriorityQueue<DistanceQueueElement<double>>.AreOrderedDelegate areOrdered,
			DistanceVisit<double>.ShouldVisitDelegate shouldVisit)
		{
			var queue = new PriorityQueue<DistanceQueueElement<double>>(areOrdered, 256);
			Func<double, double, double> addDistances = (double lhs, double rhs) => { return lhs + rhs; };
			return GetFaceEdgesByEdgeDistance(rootFace, edgeDistances, queue, shouldVisit, 0.0, addDistances);
		}

		#endregion

		private static IEnumerable<DistanceVisit<T>> GetFaceEdgesByEdgeDistance<T>(
			Topology.Face rootFace, IEdgeAttribute<T> edgeDistances,
			PriorityQueue<DistanceQueueElement<T>> queue,
			DistanceVisit<T>.ShouldVisitDelegate shouldVisit,
			T initialDistance,
			Func<T, T, T> addDistances)
		{
			var topology = rootFace.topology;
			BitArray visitedFaces = new BitArray(topology.faces.Count);

			foreach (var edge in rootFace.edges)
			{
				if (!edge.isOuterBoundary)
				{
					queue.Push(new DistanceQueueElement<T>(edge.index, 1, initialDistance));
				}
			}

			while (queue.Count > 0)
			{
				var front = queue.front;
				queue.Pop();

				var farFaceIndex = topology.edgeData[front.edgeIndex].face;

				if (visitedFaces[farFaceIndex] == false)
				{
					var visit = new DistanceVisit<T>(new Topology.FaceEdge(topology, front.edgeIndex), front.depth, front.distance);
					if (shouldVisit(visit))
					{
						yield return visit;

						foreach (var edge in new Topology.Face(topology, farFaceIndex).edges)
						{
							if (edge.twinIndex != front.edgeIndex && !edge.isOuterBoundary && visitedFaces[edge.farFace.index] == false)
							{
								queue.Push(new DistanceQueueElement<T>(edge.index, front.depth + 1, addDistances(front.distance, edgeDistances[edge])));
							}
						}
					}
				}
			}
		}

		#endregion

		#region GetFaceEdgesBySpatialDistance()

		#region GetFaceEdgesByEuclideanDistance

		public static IEnumerable<DistanceVisit<float>> GetFaceEdgesBreadthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions)
		{
			return GetFaceEdgesByEuclideanDistance(
				rootFace, facePositions,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance <= rhs.distance; },
				(DistanceVisit<float> tentativeVisit) => { return true; });
		}

		public static IEnumerable<DistanceVisit<float>> GetFaceEdgesBreadthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float maxDistance)
		{
			return GetFaceEdgesByEuclideanDistance(
				rootFace, facePositions,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance <= rhs.distance; },
				(DistanceVisit<float> tentativeVisit) => { return tentativeVisit.distance <= maxDistance; });
		}

		public static IEnumerable<DistanceVisit<float>> GetFaceEdgesBreadthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, DistanceVisit<float>.ShouldVisitDelegate shouldVisit)
		{
			return GetFaceEdgesByEuclideanDistance(
				rootFace, facePositions,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance <= rhs.distance; },
				shouldVisit);
		}

		public static IEnumerable<DistanceVisit<float>> GetFaceEdgesDepthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions)
		{
			return GetFaceEdgesByEuclideanDistance(
				rootFace, facePositions,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance >= rhs.distance; },
				(DistanceVisit<float> tentativeVisit) => { return true; });
		}

		public static IEnumerable<DistanceVisit<float>> GetFaceEdgesDepthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float maxDistance)
		{
			return GetFaceEdgesByEuclideanDistance(
				rootFace, facePositions,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance >= rhs.distance; },
				(DistanceVisit<float> tentativeVisit) => { return tentativeVisit.distance <= maxDistance; });
		}

		public static IEnumerable<DistanceVisit<float>> GetFaceEdgesDepthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, DistanceVisit<float>.ShouldVisitDelegate shouldVisit)
		{
			return GetFaceEdgesByEuclideanDistance(
				rootFace, facePositions,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance >= rhs.distance; },
				shouldVisit);
		}

		private static IEnumerable<DistanceVisit<float>> GetFaceEdgesByEuclideanDistance(
			Topology.Face rootFace, IFaceAttribute<Vector3> facePositions,
			PriorityQueue<DistanceQueueElement<float>>.AreOrderedDelegate areOrdered,
			DistanceVisit<float>.ShouldVisitDelegate shouldVisit)
		{
			var queue = new PriorityQueue<DistanceQueueElement<float>>(areOrdered, 256);
			return GetFaceEdgesBySpatialDistance(rootFace, facePositions, queue, shouldVisit, Vector3.Distance);
		}

		#endregion

		#region GetFaceEdgesBySphericalDistance

		public static IEnumerable<DistanceVisit<float>> GetFaceEdgesBreadthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius)
		{
			return GetFaceEdgesBySphericalDistance(
				rootFace, facePositions,  sphereRadius,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance <= rhs.distance; },
				(DistanceVisit<float> tentativeVisit) => { return true; });
		}

		public static IEnumerable<DistanceVisit<float>> GetFaceEdgesBreadthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, float maxDistance)
		{
			return GetFaceEdgesBySphericalDistance(
				rootFace, facePositions, sphereRadius,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance <= rhs.distance; },
				(DistanceVisit<float> tentativeVisit) => { return tentativeVisit.distance <= maxDistance; });
		}

		public static IEnumerable<DistanceVisit<float>> GetFaceEdgesBreadthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, DistanceVisit<float>.ShouldVisitDelegate shouldVisit)
		{
			return GetFaceEdgesBySphericalDistance(
				rootFace, facePositions, sphereRadius,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance <= rhs.distance; },
				shouldVisit);
		}

		public static IEnumerable<DistanceVisit<float>> GetFaceEdgesDepthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius)
		{
			return GetFaceEdgesBySphericalDistance(
				rootFace, facePositions, sphereRadius,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance >= rhs.distance; },
				(DistanceVisit<float> tentativeVisit) => { return true; });
		}

		public static IEnumerable<DistanceVisit<float>> GetFaceEdgesDepthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, float maxDistance)
		{
			return GetFaceEdgesBySphericalDistance(
				rootFace, facePositions, sphereRadius,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance >= rhs.distance; },
				(DistanceVisit<float> tentativeVisit) => { return tentativeVisit.distance <= maxDistance; });
		}

		public static IEnumerable<DistanceVisit<float>> GetFaceEdgesDepthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, DistanceVisit<float>.ShouldVisitDelegate shouldVisit)
		{
			return GetFaceEdgesBySphericalDistance(
				rootFace, facePositions, sphereRadius,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance >= rhs.distance; },
				shouldVisit);
		}

		private static IEnumerable<DistanceVisit<float>> GetFaceEdgesBySphericalDistance(
			Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius,
			PriorityQueue<DistanceQueueElement<float>>.AreOrderedDelegate areOrdered,
			DistanceVisit<float>.ShouldVisitDelegate shouldVisit)
		{
			var queue = new PriorityQueue<DistanceQueueElement<float>>(areOrdered, 256);
			Func<Vector3, Vector3, float> computeDistance = (Vector3 lhs, Vector3 rhs) => { return GeometryUtility.AngleBetweenVectors(lhs, rhs) * sphereRadius; };
			return GetFaceEdgesBySpatialDistance(rootFace, facePositions, queue, shouldVisit, computeDistance);
		}

		#endregion

		private static IEnumerable<DistanceVisit<float>> GetFaceEdgesBySpatialDistance(
			Topology.Face rootFace, IFaceAttribute<Vector3> facePositions,
			PriorityQueue<DistanceQueueElement<float>> queue,
			DistanceVisit<float>.ShouldVisitDelegate shouldVisit,
			Func<Vector3, Vector3, float> computeDistance)
		{
			var topology = rootFace.topology;
			BitArray visitedFaces = new BitArray(topology.faces.Count);

			foreach (var edge in rootFace.edges)
			{
				if (!edge.isOuterBoundary)
				{
					queue.Push(new DistanceQueueElement<float>(edge.index, 1, 0f));
				}
			}

			while (queue.Count > 0)
			{
				var front = queue.front;
				queue.Pop();

				var farFaceIndex = topology.edgeData[front.edgeIndex].face;

				if (visitedFaces[farFaceIndex] == false)
				{
					var visit = new DistanceVisit<float>(new Topology.FaceEdge(topology, front.edgeIndex), front.depth, front.distance);
					if (shouldVisit(visit))
					{
						yield return visit;

						foreach (var edge in new Topology.Face(topology, farFaceIndex).edges)
						{
							if (edge.twinIndex != front.edgeIndex && !edge.isOuterBoundary && visitedFaces[edge.farFace.index] == false)
							{
								var distance = front.distance + computeDistance(facePositions[edge.nearFace], facePositions[edge]);
								queue.Push(new DistanceQueueElement<float>(edge.index, front.depth + 1, distance));
							}
						}
					}
				}
			}
		}

		#endregion

		#region GetFaceEdgesByRandomAdjacency()

		public static IEnumerable<DepthVisit> GetFaceEdgesByRandomAdjacency(IList<Topology.Face> rootFaces, IRandomEngine randomEngine)
		{
			return GetFaceEdgesByRandomAdjacency(rootFaces, randomEngine, (DepthVisit tentativeVisit) => { return true; });
		}

		public static IEnumerable<DepthVisit> GetFaceEdgesByRandomAdjacency(IList<Topology.Face> rootFaces, IRandomEngine randomEngine, int maxDepth)
		{
			return GetFaceEdgesByRandomAdjacency(rootFaces, randomEngine, (DepthVisit tentativeVisit) => { return tentativeVisit.depth <= maxDepth; });
		}

		public static IEnumerable<DepthVisit> GetFaceEdgesByRandomAdjacency(
			IList<Topology.Face> rootFaces, IRandomEngine randomEngine,
			DepthVisit.ShouldVisitDelegate shouldVisit)
		{
			if (rootFaces.Count == 0) yield break;

			var topology = rootFaces[0].topology;
			var queue = new List<DepthQueueElement>(256);
			BitArray visitedFaces = new BitArray(topology.faces.Count);

			foreach (var rootFace in rootFaces)
			{
				foreach (var edge in rootFace.edges)
				{
					if (!edge.isOuterBoundary)
					{
						queue.Add(new DepthQueueElement(edge.index, 1));
					}
				}
			}

			while (queue.Count > 0)
			{
				var queueIndex = RandomUtility.HalfOpenRange(queue.Count, randomEngine);
				var queueElement = queue[queueIndex];

				queue[queueIndex] = queue[queue.Count - 1];
				queue.RemoveAt(queue.Count - 1);

				var farFaceIndex = topology.edgeData[queueElement.edgeIndex].face;

				if (visitedFaces[farFaceIndex] == false)
				{
					var visit = new DepthVisit(new Topology.FaceEdge(topology, queueElement.edgeIndex), queueElement.depth);
					if (shouldVisit(visit))
					{
						yield return visit;

						foreach (var edge in new Topology.Face(topology, farFaceIndex).edges)
						{
							if (edge.twinIndex != queueElement.edgeIndex && !edge.isOuterBoundary && visitedFaces[edge.farFace.index] == false)
							{
								queue.Add(new DepthQueueElement(edge.index, queueElement.depth + 1));
							}
						}
					}
				}
			}
		}

		#endregion
	}
}
