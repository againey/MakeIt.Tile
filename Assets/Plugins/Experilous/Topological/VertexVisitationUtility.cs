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
	public static class VertexVisitationUtility
	{
		#region Helper Types

		public struct DepthVisit
		{
			public delegate bool ShouldVisitDelegate(DepthVisit tentativeVisit);

			public Topology.VertexEdge edge;
			public int depth;

			public DepthVisit(Topology.VertexEdge edge, int depth)
			{
				this.edge = edge;
				this.depth = depth;
			}
		}

		public struct DistanceVisit<T>
		{
			public delegate bool ShouldVisitDelegate(DistanceVisit<T> tentativeVisit);

			public Topology.VertexEdge edge;
			public int depth;
			public T distance;

			public DistanceVisit(Topology.VertexEdge edge, int depth, T distance)
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

		#region GetVertexEdgesByDepth()

		public static IEnumerable<DepthVisit> GetVertexEdgesBreadthFirstByDepth(Topology.Vertex rootVertex)
		{
			return GetVertexEdgesByDepth(
				rootVertex,
				(DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; },
				(DepthVisit tentativeVisit) => { return true; });
		}

		public static IEnumerable<DepthVisit> GetVertexEdgesBreadthFirstByDepth(Topology.Vertex rootVertex, int maxDepth)
		{
			return GetVertexEdgesByDepth(
				rootVertex,
				(DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; },
				(DepthVisit tentativeVisit) => { return tentativeVisit.depth <= maxDepth; });
		}

		public static IEnumerable<DepthVisit> GetVertexEdgesBreadthFirstByDepth(Topology.Vertex rootVertex, DepthVisit.ShouldVisitDelegate shouldVisit)
		{
			return GetVertexEdgesByDepth(
				rootVertex,
				(DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; },
				shouldVisit);
		}

		public static IEnumerable<DepthVisit> GetVertexEdgesDepthFirstByDepth(Topology.Vertex rootVertex)
		{
			return GetVertexEdgesByDepth(
				rootVertex,
				(DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; },
				(DepthVisit tentativeVisit) => { return true; });
		}

		public static IEnumerable<DepthVisit> GetVertexEdgesDepthFirstByDepth(Topology.Vertex rootVertex, int maxDepth)
		{
			return GetVertexEdgesByDepth(
				rootVertex,
				(DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; },
				(DepthVisit tentativeVisit) => { return tentativeVisit.depth <= maxDepth; });
		}

		public static IEnumerable<DepthVisit> GetVertexEdgesDepthFirstByDepth(Topology.Vertex rootVertex, DepthVisit.ShouldVisitDelegate shouldVisit)
		{
			return GetVertexEdgesByDepth(
				rootVertex,
				(DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; },
				shouldVisit);
		}

		private static IEnumerable<DepthVisit> GetVertexEdgesByDepth(
			Topology.Vertex rootVertex,
			PriorityQueue<DepthQueueElement>.AreOrderedDelegate areOrdered,
			DepthVisit.ShouldVisitDelegate shouldVisit)
		{
			var topology = rootVertex.topology;
			var queue = new PriorityQueue<DepthQueueElement>(areOrdered, 256);
			BitArray visitedVertices = new BitArray(topology.vertices.Count);

			foreach (var edge in rootVertex.edges)
			{
				queue.Push(new DepthQueueElement(edge.index, 1));
			}

			while (queue.Count > 0)
			{
				var front = queue.front;
				queue.Pop();

				var farVertexIndex = topology.edgeData[front.edgeIndex].vertex;

				if (visitedVertices[farVertexIndex] == false)
				{
					var visit = new DepthVisit(new Topology.VertexEdge(topology, front.edgeIndex), front.depth);
					if (shouldVisit(visit))
					{
						yield return visit;

						foreach (var edge in new Topology.Vertex(topology, farVertexIndex).edges)
						{
							if (edge.twinIndex != front.edgeIndex && visitedVertices[edge.farVertex.index] == false)
							{
								queue.Push(new DepthQueueElement(edge.index, front.depth + 1));
							}
						}
					}
				}
			}
		}

		#endregion

		#region GetVertexEdgesByEdgeDistance()

		#region int

		public static IEnumerable<DistanceVisit<int>> GetVertexEdgesBreadthFirstByEdgeDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances)
		{
			return GetVertexEdgesByEdgeDistance(
				rootVertex, edgeDistances,
				(DistanceQueueElement<int> lhs, DistanceQueueElement<int> rhs) => { return lhs.distance <= rhs.distance; },
				(DistanceVisit<int> tentativeVisit) => { return true; });
		}

		public static IEnumerable<DistanceVisit<int>> GetVertexEdgesBreadthFirstByEdgeDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, int maxDistance)
		{
			return GetVertexEdgesByEdgeDistance(
				rootVertex, edgeDistances,
				(DistanceQueueElement<int> lhs, DistanceQueueElement<int> rhs) => { return lhs.distance <= rhs.distance; },
				(DistanceVisit<int> tentativeVisit) => { return tentativeVisit.distance <= maxDistance; });
		}

		public static IEnumerable<DistanceVisit<int>> GetVertexEdgesBreadthFirstByEdgeDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, DistanceVisit<int>.ShouldVisitDelegate shouldVisit)
		{
			return GetVertexEdgesByEdgeDistance(
				rootVertex, edgeDistances,
				(DistanceQueueElement<int> lhs, DistanceQueueElement<int> rhs) => { return lhs.distance <= rhs.distance; },
				shouldVisit);
		}

		public static IEnumerable<DistanceVisit<int>> GetVertexEdgesDepthFirstByEdgeDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances)
		{
			return GetVertexEdgesByEdgeDistance(
				rootVertex, edgeDistances,
				(DistanceQueueElement<int> lhs, DistanceQueueElement<int> rhs) => { return lhs.distance >= rhs.distance; },
				(DistanceVisit<int> tentativeVisit) => { return true; });
		}

		public static IEnumerable<DistanceVisit<int>> GetVertexEdgesDepthFirstByEdgeDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, int maxDistance)
		{
			return GetVertexEdgesByEdgeDistance(
				rootVertex, edgeDistances,
				(DistanceQueueElement<int> lhs, DistanceQueueElement<int> rhs) => { return lhs.distance >= rhs.distance; },
				(DistanceVisit<int> tentativeVisit) => { return tentativeVisit.distance <= maxDistance; });
		}

		public static IEnumerable<DistanceVisit<int>> GetVertexEdgesDepthFirstByEdgeDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, DistanceVisit<int>.ShouldVisitDelegate shouldVisit)
		{
			return GetVertexEdgesByEdgeDistance(
				rootVertex, edgeDistances,
				(DistanceQueueElement<int> lhs, DistanceQueueElement<int> rhs) => { return lhs.distance >= rhs.distance; },
				shouldVisit);
		}

		private static IEnumerable<DistanceVisit<int>> GetVertexEdgesByEdgeDistance(
			Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances,
			PriorityQueue<DistanceQueueElement<int>>.AreOrderedDelegate areOrdered,
			DistanceVisit<int>.ShouldVisitDelegate shouldVisit)
		{
			var queue = new PriorityQueue<DistanceQueueElement<int>>(areOrdered, 256);
			Func<int, int, int> addDistances = (int lhs, int rhs) => { return lhs + rhs; };
			return GetVertexEdgesByEdgeDistance(rootVertex, edgeDistances, queue, shouldVisit, 0, addDistances);
		}

		#endregion

		#region uint

		public static IEnumerable<DistanceVisit<uint>> GetVertexEdgesBreadthFirstByEdgeDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances)
		{
			return GetVertexEdgesByEdgeDistance(
				rootVertex, edgeDistances,
				(DistanceQueueElement<uint> lhs, DistanceQueueElement<uint> rhs) => { return lhs.distance <= rhs.distance; },
				(DistanceVisit<uint> tentativeVisit) => { return true; });
		}

		public static IEnumerable<DistanceVisit<uint>> GetVertexEdgesBreadthFirstByEdgeDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, uint maxDistance)
		{
			return GetVertexEdgesByEdgeDistance(
				rootVertex, edgeDistances,
				(DistanceQueueElement<uint> lhs, DistanceQueueElement<uint> rhs) => { return lhs.distance <= rhs.distance; },
				(DistanceVisit<uint> tentativeVisit) => { return tentativeVisit.distance <= maxDistance; });
		}

		public static IEnumerable<DistanceVisit<uint>> GetVertexEdgesBreadthFirstByEdgeDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, DistanceVisit<uint>.ShouldVisitDelegate shouldVisit)
		{
			return GetVertexEdgesByEdgeDistance(
				rootVertex, edgeDistances,
				(DistanceQueueElement<uint> lhs, DistanceQueueElement<uint> rhs) => { return lhs.distance <= rhs.distance; },
				shouldVisit);
		}

		public static IEnumerable<DistanceVisit<uint>> GetVertexEdgesDepthFirstByEdgeDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances)
		{
			return GetVertexEdgesByEdgeDistance(
				rootVertex, edgeDistances,
				(DistanceQueueElement<uint> lhs, DistanceQueueElement<uint> rhs) => { return lhs.distance >= rhs.distance; },
				(DistanceVisit<uint> tentativeVisit) => { return true; });
		}

		public static IEnumerable<DistanceVisit<uint>> GetVertexEdgesDepthFirstByEdgeDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, uint maxDistance)
		{
			return GetVertexEdgesByEdgeDistance(
				rootVertex, edgeDistances,
				(DistanceQueueElement<uint> lhs, DistanceQueueElement<uint> rhs) => { return lhs.distance >= rhs.distance; },
				(DistanceVisit<uint> tentativeVisit) => { return tentativeVisit.distance <= maxDistance; });
		}

		public static IEnumerable<DistanceVisit<uint>> GetVertexEdgesDepthFirstByEdgeDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, DistanceVisit<uint>.ShouldVisitDelegate shouldVisit)
		{
			return GetVertexEdgesByEdgeDistance(
				rootVertex, edgeDistances,
				(DistanceQueueElement<uint> lhs, DistanceQueueElement<uint> rhs) => { return lhs.distance >= rhs.distance; },
				shouldVisit);
		}

		private static IEnumerable<DistanceVisit<uint>> GetVertexEdgesByEdgeDistance(
			Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances,
			PriorityQueue<DistanceQueueElement<uint>>.AreOrderedDelegate areOrdered,
			DistanceVisit<uint>.ShouldVisitDelegate shouldVisit)
		{
			var queue = new PriorityQueue<DistanceQueueElement<uint>>(areOrdered, 256);
			Func<uint, uint, uint> addDistances = (uint lhs, uint rhs) => { return lhs + rhs; };
			return GetVertexEdgesByEdgeDistance(rootVertex, edgeDistances, queue, shouldVisit, 0U, addDistances);
		}

		#endregion

		#region float

		public static IEnumerable<DistanceVisit<float>> GetVertexEdgesBreadthFirstByEdgeDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances)
		{
			return GetVertexEdgesByEdgeDistance(
				rootVertex, edgeDistances,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance <= rhs.distance; },
				(DistanceVisit<float> tentativeVisit) => { return true; });
		}

		public static IEnumerable<DistanceVisit<float>> GetVertexEdgesBreadthFirstByEdgeDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, float maxDistance)
		{
			return GetVertexEdgesByEdgeDistance(
				rootVertex, edgeDistances,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance <= rhs.distance; },
				(DistanceVisit<float> tentativeVisit) => { return tentativeVisit.distance <= maxDistance; });
		}

		public static IEnumerable<DistanceVisit<float>> GetVertexEdgesBreadthFirstByEdgeDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, DistanceVisit<float>.ShouldVisitDelegate shouldVisit)
		{
			return GetVertexEdgesByEdgeDistance(
				rootVertex, edgeDistances,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance <= rhs.distance; },
				shouldVisit);
		}

		public static IEnumerable<DistanceVisit<float>> GetVertexEdgesDepthFirstByEdgeDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances)
		{
			return GetVertexEdgesByEdgeDistance(
				rootVertex, edgeDistances,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance >= rhs.distance; },
				(DistanceVisit<float> tentativeVisit) => { return true; });
		}

		public static IEnumerable<DistanceVisit<float>> GetVertexEdgesDepthFirstByEdgeDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, float maxDistance)
		{
			return GetVertexEdgesByEdgeDistance(
				rootVertex, edgeDistances,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance >= rhs.distance; },
				(DistanceVisit<float> tentativeVisit) => { return tentativeVisit.distance <= maxDistance; });
		}

		public static IEnumerable<DistanceVisit<float>> GetVertexEdgesDepthFirstByEdgeDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, DistanceVisit<float>.ShouldVisitDelegate shouldVisit)
		{
			return GetVertexEdgesByEdgeDistance(
				rootVertex, edgeDistances,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance >= rhs.distance; },
				shouldVisit);
		}

		private static IEnumerable<DistanceVisit<float>> GetVertexEdgesByEdgeDistance(
			Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances,
			PriorityQueue<DistanceQueueElement<float>>.AreOrderedDelegate areOrdered,
			DistanceVisit<float>.ShouldVisitDelegate shouldVisit)
		{
			var queue = new PriorityQueue<DistanceQueueElement<float>>(areOrdered, 256);
			Func<float, float, float> addDistances = (float lhs, float rhs) => { return lhs + rhs; };
			return GetVertexEdgesByEdgeDistance(rootVertex, edgeDistances, queue, shouldVisit, 0f, addDistances);
		}

		#endregion

		#region double

		public static IEnumerable<DistanceVisit<double>> GetVertexEdgesBreadthFirstByEdgeDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances)
		{
			return GetVertexEdgesByEdgeDistance(
				rootVertex, edgeDistances,
				(DistanceQueueElement<double> lhs, DistanceQueueElement<double> rhs) => { return lhs.distance <= rhs.distance; },
				(DistanceVisit<double> tentativeVisit) => { return true; });
		}

		public static IEnumerable<DistanceVisit<double>> GetVertexEdgesBreadthFirstByEdgeDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, double maxDistance)
		{
			return GetVertexEdgesByEdgeDistance(
				rootVertex, edgeDistances,
				(DistanceQueueElement<double> lhs, DistanceQueueElement<double> rhs) => { return lhs.distance <= rhs.distance; },
				(DistanceVisit<double> tentativeVisit) => { return tentativeVisit.distance <= maxDistance; });
		}

		public static IEnumerable<DistanceVisit<double>> GetVertexEdgesBreadthFirstByEdgeDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, DistanceVisit<double>.ShouldVisitDelegate shouldVisit)
		{
			return GetVertexEdgesByEdgeDistance(
				rootVertex, edgeDistances,
				(DistanceQueueElement<double> lhs, DistanceQueueElement<double> rhs) => { return lhs.distance <= rhs.distance; },
				shouldVisit);
		}

		public static IEnumerable<DistanceVisit<double>> GetVertexEdgesDepthFirstByEdgeDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances)
		{
			return GetVertexEdgesByEdgeDistance(
				rootVertex, edgeDistances,
				(DistanceQueueElement<double> lhs, DistanceQueueElement<double> rhs) => { return lhs.distance >= rhs.distance; },
				(DistanceVisit<double> tentativeVisit) => { return true; });
		}

		public static IEnumerable<DistanceVisit<double>> GetVertexEdgesDepthFirstByEdgeDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, double maxDistance)
		{
			return GetVertexEdgesByEdgeDistance(
				rootVertex, edgeDistances,
				(DistanceQueueElement<double> lhs, DistanceQueueElement<double> rhs) => { return lhs.distance >= rhs.distance; },
				(DistanceVisit<double> tentativeVisit) => { return tentativeVisit.distance <= maxDistance; });
		}

		public static IEnumerable<DistanceVisit<double>> GetVertexEdgesDepthFirstByEdgeDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, DistanceVisit<double>.ShouldVisitDelegate shouldVisit)
		{
			return GetVertexEdgesByEdgeDistance(
				rootVertex, edgeDistances,
				(DistanceQueueElement<double> lhs, DistanceQueueElement<double> rhs) => { return lhs.distance >= rhs.distance; },
				shouldVisit);
		}

		private static IEnumerable<DistanceVisit<double>> GetVertexEdgesByEdgeDistance(
			Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances,
			PriorityQueue<DistanceQueueElement<double>>.AreOrderedDelegate areOrdered,
			DistanceVisit<double>.ShouldVisitDelegate shouldVisit)
		{
			var queue = new PriorityQueue<DistanceQueueElement<double>>(areOrdered, 256);
			Func<double, double, double> addDistances = (double lhs, double rhs) => { return lhs + rhs; };
			return GetVertexEdgesByEdgeDistance(rootVertex, edgeDistances, queue, shouldVisit, 0.0, addDistances);
		}

		#endregion

		private static IEnumerable<DistanceVisit<T>> GetVertexEdgesByEdgeDistance<T>(
			Topology.Vertex rootVertex, IEdgeAttribute<T> edgeDistances,
			PriorityQueue<DistanceQueueElement<T>> queue,
			DistanceVisit<T>.ShouldVisitDelegate shouldVisit,
			T initialDistance,
			Func<T, T, T> addDistances)
		{
			var topology = rootVertex.topology;
			BitArray visitedVertices = new BitArray(topology.vertices.Count);

			foreach (var edge in rootVertex.edges)
			{
				queue.Push(new DistanceQueueElement<T>(edge.index, 1, initialDistance));
			}

			while (queue.Count > 0)
			{
				var front = queue.front;
				queue.Pop();

				var farVertexIndex = topology.edgeData[front.edgeIndex].vertex;

				if (visitedVertices[farVertexIndex] == false)
				{
					var visit = new DistanceVisit<T>(new Topology.VertexEdge(topology, front.edgeIndex), front.depth, front.distance);
					if (shouldVisit(visit))
					{
						yield return visit;

						foreach (var edge in new Topology.Vertex(topology, farVertexIndex).edges)
						{
							if (edge.twinIndex != front.edgeIndex && visitedVertices[edge.farVertex.index] == false)
							{
								queue.Push(new DistanceQueueElement<T>(edge.index, front.depth + 1, addDistances(front.distance, edgeDistances[edge])));
							}
						}
					}
				}
			}
		}

		#endregion

		#region GetVertexEdgesBySpatialDistance()

		#region GetVertexEdgesByEuclideanDistance

		public static IEnumerable<DistanceVisit<float>> GetVertexEdgesBreadthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions)
		{
			return GetVertexEdgesByEuclideanDistance(
				rootVertex, vertexPositions,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance <= rhs.distance; },
				(DistanceVisit<float> tentativeVisit) => { return true; });
		}

		public static IEnumerable<DistanceVisit<float>> GetVertexEdgesBreadthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float maxDistance)
		{
			return GetVertexEdgesByEuclideanDistance(
				rootVertex, vertexPositions,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance <= rhs.distance; },
				(DistanceVisit<float> tentativeVisit) => { return tentativeVisit.distance <= maxDistance; });
		}

		public static IEnumerable<DistanceVisit<float>> GetVertexEdgesBreadthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, DistanceVisit<float>.ShouldVisitDelegate shouldVisit)
		{
			return GetVertexEdgesByEuclideanDistance(
				rootVertex, vertexPositions,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance <= rhs.distance; },
				shouldVisit);
		}

		public static IEnumerable<DistanceVisit<float>> GetVertexEdgesDepthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions)
		{
			return GetVertexEdgesByEuclideanDistance(
				rootVertex, vertexPositions,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance >= rhs.distance; },
				(DistanceVisit<float> tentativeVisit) => { return true; });
		}

		public static IEnumerable<DistanceVisit<float>> GetVertexEdgesDepthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float maxDistance)
		{
			return GetVertexEdgesByEuclideanDistance(
				rootVertex, vertexPositions,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance >= rhs.distance; },
				(DistanceVisit<float> tentativeVisit) => { return tentativeVisit.distance <= maxDistance; });
		}

		public static IEnumerable<DistanceVisit<float>> GetVertexEdgesDepthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, DistanceVisit<float>.ShouldVisitDelegate shouldVisit)
		{
			return GetVertexEdgesByEuclideanDistance(
				rootVertex, vertexPositions,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance >= rhs.distance; },
				shouldVisit);
		}

		private static IEnumerable<DistanceVisit<float>> GetVertexEdgesByEuclideanDistance(
			Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions,
			PriorityQueue<DistanceQueueElement<float>>.AreOrderedDelegate areOrdered,
			DistanceVisit<float>.ShouldVisitDelegate shouldVisit)
		{
			var queue = new PriorityQueue<DistanceQueueElement<float>>(areOrdered, 256);
			return GetVertexEdgesBySpatialDistance(rootVertex, vertexPositions, queue, shouldVisit, Vector3.Distance);
		}

		#endregion

		#region GetVertexEdgesBySphericalDistance

		public static IEnumerable<DistanceVisit<float>> GetVertexEdgesBreadthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius)
		{
			return GetVertexEdgesBySphericalDistance(
				rootVertex, vertexPositions,  sphereRadius,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance <= rhs.distance; },
				(DistanceVisit<float> tentativeVisit) => { return true; });
		}

		public static IEnumerable<DistanceVisit<float>> GetVertexEdgesBreadthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, float maxDistance)
		{
			return GetVertexEdgesBySphericalDistance(
				rootVertex, vertexPositions, sphereRadius,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance <= rhs.distance; },
				(DistanceVisit<float> tentativeVisit) => { return tentativeVisit.distance <= maxDistance; });
		}

		public static IEnumerable<DistanceVisit<float>> GetVertexEdgesBreadthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, DistanceVisit<float>.ShouldVisitDelegate shouldVisit)
		{
			return GetVertexEdgesBySphericalDistance(
				rootVertex, vertexPositions, sphereRadius,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance <= rhs.distance; },
				shouldVisit);
		}

		public static IEnumerable<DistanceVisit<float>> GetVertexEdgesDepthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius)
		{
			return GetVertexEdgesBySphericalDistance(
				rootVertex, vertexPositions, sphereRadius,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance >= rhs.distance; },
				(DistanceVisit<float> tentativeVisit) => { return true; });
		}

		public static IEnumerable<DistanceVisit<float>> GetVertexEdgesDepthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, float maxDistance)
		{
			return GetVertexEdgesBySphericalDistance(
				rootVertex, vertexPositions, sphereRadius,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance >= rhs.distance; },
				(DistanceVisit<float> tentativeVisit) => { return tentativeVisit.distance <= maxDistance; });
		}

		public static IEnumerable<DistanceVisit<float>> GetVertexEdgesDepthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, DistanceVisit<float>.ShouldVisitDelegate shouldVisit)
		{
			return GetVertexEdgesBySphericalDistance(
				rootVertex, vertexPositions, sphereRadius,
				(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) => { return lhs.distance >= rhs.distance; },
				shouldVisit);
		}

		private static IEnumerable<DistanceVisit<float>> GetVertexEdgesBySphericalDistance(
			Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius,
			PriorityQueue<DistanceQueueElement<float>>.AreOrderedDelegate areOrdered,
			DistanceVisit<float>.ShouldVisitDelegate shouldVisit)
		{
			var queue = new PriorityQueue<DistanceQueueElement<float>>(areOrdered, 256);
			Func<Vector3, Vector3, float> computeDistance = (Vector3 lhs, Vector3 rhs) => { return GeometryUtility.AngleBetweenVectors(lhs, rhs) * sphereRadius; };
			return GetVertexEdgesBySpatialDistance(rootVertex, vertexPositions, queue, shouldVisit, computeDistance);
		}

		#endregion

		private static IEnumerable<DistanceVisit<float>> GetVertexEdgesBySpatialDistance(
			Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions,
			PriorityQueue<DistanceQueueElement<float>> queue,
			DistanceVisit<float>.ShouldVisitDelegate shouldVisit,
			Func<Vector3, Vector3, float> computeDistance)
		{
			var topology = rootVertex.topology;
			BitArray visitedVertices = new BitArray(topology.vertices.Count);

			foreach (var edge in rootVertex.edges)
			{
				queue.Push(new DistanceQueueElement<float>(edge.index, 1, 0f));
			}

			while (queue.Count > 0)
			{
				var front = queue.front;
				queue.Pop();

				var farVertexIndex = topology.edgeData[front.edgeIndex].vertex;

				if (visitedVertices[farVertexIndex] == false)
				{
					var visit = new DistanceVisit<float>(new Topology.VertexEdge(topology, front.edgeIndex), front.depth, front.distance);
					if (shouldVisit(visit))
					{
						yield return visit;

						foreach (var edge in new Topology.Vertex(topology, farVertexIndex).edges)
						{
							if (edge.twinIndex != front.edgeIndex && visitedVertices[edge.farVertex.index] == false)
							{
								var distance = front.distance + computeDistance(vertexPositions[edge.nearVertex], vertexPositions[edge]);
								queue.Push(new DistanceQueueElement<float>(edge.index, front.depth + 1, distance));
							}
						}
					}
				}
			}
		}

		#endregion

		#region GetVertexEdgesByRandomAdjacency()

		public static IEnumerable<DepthVisit> GetVertexEdgesByRandomAdjacency(IList<Topology.Vertex> rootVertices, IRandomEngine randomEngine)
		{
			return GetVertexEdgesByRandomAdjacency(rootVertices, randomEngine, (DepthVisit tentativeVisit) => { return true; });
		}

		public static IEnumerable<DepthVisit> GetVertexEdgesByRandomAdjacency(IList<Topology.Vertex> rootVertices, IRandomEngine randomEngine, int maxDepth)
		{
			return GetVertexEdgesByRandomAdjacency(rootVertices, randomEngine, (DepthVisit tentativeVisit) => { return tentativeVisit.depth <= maxDepth; });
		}

		public static IEnumerable<DepthVisit> GetVertexEdgesByRandomAdjacency(
			IList<Topology.Vertex> rootVertices, IRandomEngine randomEngine,
			DepthVisit.ShouldVisitDelegate shouldVisit)
		{
			if (rootVertices.Count == 0) yield break;

			var topology = rootVertices[0].topology;
			var queue = new List<DepthQueueElement>(256);
			BitArray visitedVertices = new BitArray(topology.vertices.Count);

			foreach (var rootVertex in rootVertices)
			{
				foreach (var edge in rootVertex.edges)
				{
					queue.Add(new DepthQueueElement(edge.index, 1));
				}
			}

			while (queue.Count > 0)
			{
				var queueIndex = RandomUtility.HalfOpenRange(queue.Count, randomEngine);
				var queueElement = queue[queueIndex];

				queue[queueIndex] = queue[queue.Count - 1];
				queue.RemoveAt(queue.Count - 1);

				var farVertexIndex = topology.edgeData[queueElement.edgeIndex].vertex;

				if (visitedVertices[farVertexIndex] == false)
				{
					var visit = new DepthVisit(new Topology.VertexEdge(topology, queueElement.edgeIndex), queueElement.depth);
					if (shouldVisit(visit))
					{
						yield return visit;

						foreach (var edge in new Topology.Vertex(topology, farVertexIndex).edges)
						{
							if (edge.twinIndex != queueElement.edgeIndex && visitedVertices[edge.farVertex.index] == false)
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
