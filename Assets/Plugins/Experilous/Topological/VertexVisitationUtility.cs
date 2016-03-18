/******************************************************************************\
* Copyright Andy Gainey                                                        *
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
		#region Core

		public enum VisitationState
		{
			Continue,
			Discontinue,
			Ignore,
			Stop,
		}

		private delegate void VisitorDelegate(int edgeIndex, ref VisitationState state);

		private abstract class Queue
		{
			public Topology topology;
			private BitArray _visitedVertices;

			public abstract void Push(int edgeIndex);
			public abstract bool Pop(out int edgeIndex);
			public abstract int edgeIndex { get; }

			public Topology PushRoots(Topology.Vertex rootVertex)
			{
				topology = rootVertex.topology;
				_visitedVertices = new BitArray(topology.vertices.Count);
				_visitedVertices[rootVertex.index] = true;

				foreach (var edge in rootVertex.edges)
				{
					Push(edge.index);
				}

				return topology;
			}

			public Topology PushRoots(IEnumerable<Topology.Vertex> rootVertices)
			{
				PushRoots(PrepareToPushRoots(rootVertices));
				return topology;
			}

			public IEnumerator<Topology.Vertex> PrepareToPushRoots(IEnumerable<Topology.Vertex> rootVertices)
			{
				var enumerator = rootVertices.GetEnumerator();

				if (!enumerator.MoveNext()) return null;

				topology = enumerator.Current.topology;
				_visitedVertices = new BitArray(topology.vertices.Count);

				return enumerator;
			}

			public void PushRoots(IEnumerator<Topology.Vertex> rootVertices)
			{
				do
				{
					_visitedVertices[rootVertices.Current.index] = true;

					foreach (var edge in rootVertices.Current.edges)
					{
						Push(edge.index);
					}
				} while (rootVertices.MoveNext());
			}

			public void Visit(VisitorDelegate visitor)
			{
				int edgeIndex;
				while (Pop(out edgeIndex))
				{
					int farVertexIndex = topology.edgeData[edgeIndex].vertex;
					if (_visitedVertices[farVertexIndex] == false)
					{
						VisitationState state = VisitationState.Continue;
						visitor(edgeIndex, ref state);
						switch (state)
						{
							case VisitationState.Continue:
								_visitedVertices[farVertexIndex] = true;
								foreach (var nextEdge in new Topology.Vertex(topology, farVertexIndex).edges)
								{
									if (nextEdge.twinIndex != edgeIndex && _visitedVertices[nextEdge.farVertex.index] == false)
									{
										Push(nextEdge.index);
									}
								}
								break;
							case VisitationState.Discontinue:
								_visitedVertices[farVertexIndex] = true;
								break;
							case VisitationState.Stop:
								return;
						}
					}
				}
			}
		}

		#endregion

		#region Any Order Connected Visitation

		public delegate void VisitVertexEdgeDelegate(Topology.VertexEdge vertexEdge);
		public delegate void VisitVertexDelegate(Topology.Vertex vertex);

		public delegate void VisitVertexEdgeWithStateDelegate(Topology.VertexEdge vertexEdge, ref VisitationState state);
		public delegate void VisitVertexWithStateDelegate(Topology.Vertex vertex, ref VisitationState state);

		#region Queue

		private class ConnectedQueue : Queue
		{
			private List<int> _queue;
			public int frontEdgeIndex;

			public ConnectedQueue()
			{
				_queue = new List<int>();
			}

			public override int edgeIndex { get { return frontEdgeIndex; } }

			public override void Push(int edgeIndex)
			{
				_queue.Add(edgeIndex);
			}

			public override bool Pop(out int edgeIndex)
			{
				if (_queue.Count == 0)
				{
					edgeIndex = -1;
					return false;
				}

				var queueIndex = _queue.Count - 1;
				edgeIndex = frontEdgeIndex = _queue[queueIndex];
				_queue.RemoveAt(queueIndex);
				return true;
			}
		}

		private static void VisitConnectedInAnyOrder(ConnectedQueue queue, Topology topology, VisitVertexEdgeDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex));
			});
		}

		private static void VisitConnectedInAnyOrder(ConnectedQueue queue, Topology topology, VisitVertexDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex).farVertex);
			});
		}

		private static void VisitConnectedInAnyOrder(ConnectedQueue queue, Topology topology, VisitVertexEdgeWithStateDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex), ref state);
			});
		}

		private static void VisitConnectedInAnyOrder(ConnectedQueue queue, Topology topology, VisitVertexWithStateDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex).farVertex, ref state);
			});
		}

		#endregion

		#region Visit From Single Root

		public static void VisitConnectedInAnyOrder(Topology.Vertex rootVertex, VisitVertexEdgeDelegate visitor)
		{
			var queue = new ConnectedQueue();
			VisitConnectedInAnyOrder(queue, queue.PushRoots(rootVertex), visitor);
		}

		public static void VisitConnectedInAnyOrder(Topology.Vertex rootVertex, VisitVertexDelegate visitor)
		{
			var queue = new ConnectedQueue();
			VisitConnectedInAnyOrder(queue, queue.PushRoots(rootVertex), visitor);
		}

		public static void VisitConnectedInAnyOrder(Topology.Vertex rootVertex, VisitVertexEdgeWithStateDelegate visitor)
		{
			var queue = new ConnectedQueue();
			VisitConnectedInAnyOrder(queue, queue.PushRoots(rootVertex), visitor);
		}

		public static void VisitConnectedInAnyOrder(Topology.Vertex rootVertex, VisitVertexWithStateDelegate visitor)
		{
			var queue = new ConnectedQueue();
			VisitConnectedInAnyOrder(queue, queue.PushRoots(rootVertex), visitor);
		}

		#endregion

		#region Visit From Multiple Roots

		public static void VisitConnectedInAnyOrder(IEnumerable<Topology.Vertex> rootVertices, VisitVertexEdgeDelegate visitor)
		{
			var queue = new ConnectedQueue();
			VisitConnectedInAnyOrder(queue, queue.PushRoots(rootVertices), visitor);
		}

		public static void VisitConnectedInAnyOrder(IEnumerable<Topology.Vertex> rootVertices, VisitVertexDelegate visitor)
		{
			var queue = new ConnectedQueue();
			VisitConnectedInAnyOrder(queue, queue.PushRoots(rootVertices), visitor);
		}

		public static void VisitConnectedInAnyOrder(IEnumerable<Topology.Vertex> rootVertices, VisitVertexEdgeWithStateDelegate visitor)
		{
			var queue = new ConnectedQueue();
			VisitConnectedInAnyOrder(queue, queue.PushRoots(rootVertices), visitor);
		}

		public static void VisitConnectedInAnyOrder(IEnumerable<Topology.Vertex> rootVertices, VisitVertexWithStateDelegate visitor)
		{
			var queue = new ConnectedQueue();
			VisitConnectedInAnyOrder(queue, queue.PushRoots(rootVertices), visitor);
		}

		#endregion

		#endregion

		#region Depth Ordered Visitation

		public delegate void VisitVertexEdgeWithDepthDelegate(Topology.VertexEdge vertexEdge, int depth);
		public delegate void VisitVertexWithDepthDelegate(Topology.Vertex vertex, int depth);

		public delegate void VisitVertexEdgeWithDepthStateDelegate(Topology.VertexEdge vertexEdge, int depth, ref VisitationState state);
		public delegate void VisitVertexWithDepthStateDelegate(Topology.Vertex vertex, int depth, ref VisitationState state);

		#region Queue

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

		private class DepthQueue : Queue
		{
			private PriorityQueue<DepthQueueElement> _queue;
			public DepthQueueElement front;

			public DepthQueue(PriorityQueue<DepthQueueElement>.AreOrderedDelegate areOrdered)
			{
				_queue = new PriorityQueue<DepthQueueElement>(areOrdered, 256);
			}

			public override int edgeIndex { get { return front.edgeIndex; } }

			public override void Push(int edgeIndex)
			{
				_queue.Push(new DepthQueueElement(edgeIndex, front.depth + 1));
			}

			public override bool Pop(out int edgeIndex)
			{
				if (_queue.Count == 0)
				{
					edgeIndex = -1;
					return false;
				}

				front = _queue.front;
				edgeIndex = front.edgeIndex;
				_queue.Pop();
				return true;
			}
		}

		private static void VisitByDepth(DepthQueue queue, Topology topology, VisitVertexEdgeDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex));
			});
		}

		private static void VisitByDepth(DepthQueue queue, Topology topology, VisitVertexDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex).farVertex);
			});
		}

		private static void VisitByDepth(DepthQueue queue, Topology topology, VisitVertexEdgeWithStateDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex), ref state);
			});
		}

		private static void VisitByDepth(DepthQueue queue, Topology topology, VisitVertexWithStateDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex).farVertex, ref state);
			});
		}

		private static void VisitByDepth(DepthQueue queue, Topology topology, VisitVertexEdgeWithDepthDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex), queue.front.depth);
			});
		}

		private static void VisitByDepth(DepthQueue queue, Topology topology, VisitVertexWithDepthDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex).farVertex, queue.front.depth);
			});
		}

		private static void VisitByDepth(DepthQueue queue, Topology topology, VisitVertexEdgeWithDepthStateDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex), queue.front.depth, ref state);
			});
		}

		private static void VisitByDepth(DepthQueue queue, Topology topology, VisitVertexWithDepthStateDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex).farVertex, queue.front.depth, ref state);
			});
		}

		#endregion

		#region Breadth First

		#region Visit From Single Root

		public static void VisitBreadthFirstByDepth(Topology.Vertex rootVertex, VisitVertexEdgeDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertex), visitor);
		}

		public static void VisitBreadthFirstByDepth(Topology.Vertex rootVertex, VisitVertexDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertex), visitor);
		}

		public static void VisitBreadthFirstByDepth(Topology.Vertex rootVertex, VisitVertexEdgeWithStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertex), visitor);
		}

		public static void VisitBreadthFirstByDepth(Topology.Vertex rootVertex, VisitVertexWithStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertex), visitor);
		}

		public static void VisitBreadthFirstByDepth(Topology.Vertex rootVertex, VisitVertexEdgeWithDepthDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertex), visitor);
		}

		public static void VisitBreadthFirstByDepth(Topology.Vertex rootVertex, VisitVertexWithDepthDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertex), visitor);
		}

		public static void VisitBreadthFirstByDepth(Topology.Vertex rootVertex, VisitVertexEdgeWithDepthStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertex), visitor);
		}

		public static void VisitBreadthFirstByDepth(Topology.Vertex rootVertex, VisitVertexWithDepthStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertex), visitor);
		}

		#endregion

		#region Visit From Multiple Roots

		public static void VisitBreadthFirstByDepth(IEnumerable<Topology.Vertex> rootVertices, VisitVertexEdgeDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertices), visitor);
		}

		public static void VisitBreadthFirstByDepth(IEnumerable<Topology.Vertex> rootVertices, VisitVertexDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertices), visitor);
		}

		public static void VisitBreadthFirstByDepth(IEnumerable<Topology.Vertex> rootVertices, VisitVertexEdgeWithStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertices), visitor);
		}

		public static void VisitBreadthFirstByDepth(IEnumerable<Topology.Vertex> rootVertices, VisitVertexWithStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertices), visitor);
		}

		public static void VisitBreadthFirstByDepth(IEnumerable<Topology.Vertex> rootVertices, VisitVertexEdgeWithDepthDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertices), visitor);
		}

		public static void VisitBreadthFirstByDepth(IEnumerable<Topology.Vertex> rootVertices, VisitVertexWithDepthDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertices), visitor);
		}

		public static void VisitBreadthFirstByDepth(IEnumerable<Topology.Vertex> rootVertices, VisitVertexEdgeWithDepthStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertices), visitor);
		}

		public static void VisitBreadthFirstByDepth(IEnumerable<Topology.Vertex> rootVertices, VisitVertexWithDepthStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertices), visitor);
		}

		#endregion

		#endregion

		#region Depth First

		#region Visit From Single Root

		public static void VisitDepthFirstByDepth(Topology.Vertex rootVertex, VisitVertexEdgeDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertex), visitor);
		}

		public static void VisitDepthFirstByDepth(Topology.Vertex rootVertex, VisitVertexDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertex), visitor);
		}

		public static void VisitDepthFirstByDepth(Topology.Vertex rootVertex, VisitVertexEdgeWithStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertex), visitor);
		}

		public static void VisitDepthFirstByDepth(Topology.Vertex rootVertex, VisitVertexWithStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertex), visitor);
		}

		public static void VisitDepthFirstByDepth(Topology.Vertex rootVertex, VisitVertexEdgeWithDepthDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertex), visitor);
		}

		public static void VisitDepthFirstByDepth(Topology.Vertex rootVertex, VisitVertexWithDepthDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertex), visitor);
		}

		public static void VisitDepthFirstByDepth(Topology.Vertex rootVertex, VisitVertexEdgeWithDepthStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertex), visitor);
		}

		public static void VisitDepthFirstByDepth(Topology.Vertex rootVertex, VisitVertexWithDepthStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertex), visitor);
		}

		#endregion

		#region Visit From Multiple Roots

		public static void VisitDepthFirstByDepth(IEnumerable<Topology.Vertex> rootVertices, VisitVertexEdgeDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertices), visitor);
		}

		public static void VisitDepthFirstByDepth(IEnumerable<Topology.Vertex> rootVertices, VisitVertexDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertices), visitor);
		}

		public static void VisitDepthFirstByDepth(IEnumerable<Topology.Vertex> rootVertices, VisitVertexEdgeWithStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertices), visitor);
		}

		public static void VisitDepthFirstByDepth(IEnumerable<Topology.Vertex> rootVertices, VisitVertexWithStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertices), visitor);
		}

		public static void VisitDepthFirstByDepth(IEnumerable<Topology.Vertex> rootVertices, VisitVertexEdgeWithDepthDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertices), visitor);
		}

		public static void VisitDepthFirstByDepth(IEnumerable<Topology.Vertex> rootVertices, VisitVertexWithDepthDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertices), visitor);
		}

		public static void VisitDepthFirstByDepth(IEnumerable<Topology.Vertex> rootVertices, VisitVertexEdgeWithDepthStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertices), visitor);
		}

		public static void VisitDepthFirstByDepth(IEnumerable<Topology.Vertex> rootVertices, VisitVertexWithDepthStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootVertices), visitor);
		}

		#endregion

		#endregion

		#endregion

		#region Distance Ordered Visitation

		public delegate void VisitVertexEdgeWithDistanceDelegate<T>(Topology.VertexEdge vertexEdge, T distance);
		public delegate void VisitVertexWithDistanceDelegate<T>(Topology.Vertex vertex, T distance);

		public delegate void VisitVertexEdgeWithDistanceStateDelegate<T>(Topology.VertexEdge vertexEdge, T distance, ref VisitationState state);
		public delegate void VisitVertexWithDistanceStateDelegate<T>(Topology.Vertex vertex, T distance, ref VisitationState state);

		public delegate void VisitVertexEdgeWithDepthDistanceDelegate<T>(Topology.VertexEdge vertexEdge, int depth, T distance);
		public delegate void VisitVertexWithDepthDistanceDelegate<T>(Topology.Vertex vertex, int depth, T distance);

		public delegate void VisitVertexEdgeWithDepthDistanceStateDelegate<T>(Topology.VertexEdge vertexEdge, int depth, T distance, ref VisitationState state);
		public delegate void VisitVertexWithDepthDistanceStateDelegate<T>(Topology.Vertex vertex, int depth, T distance, ref VisitationState state);

		#region Queue

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

		private abstract class DistanceQueue<T> : Queue
		{
			protected PriorityQueue<DistanceQueueElement<T>> _queue;
			public Func<int, T> edgeDistance;
			public DistanceQueueElement<T> front;

			protected DistanceQueue<T> Initialize(PriorityQueue<DistanceQueueElement<T>>.AreOrderedDelegate areOrdered)
			{
				_queue = new PriorityQueue<DistanceQueueElement<T>>(areOrdered, 256);
				return this;
			}

			public override int edgeIndex { get { return front.edgeIndex; } }

			public abstract T AccumulateDistance(T lhs, T rhs);

			public override void Push(int edgeIndex)
			{
				_queue.Push(new DistanceQueueElement<T>(edgeIndex, front.depth + 1, AccumulateDistance(front.distance, edgeDistance(edgeIndex))));
			}

			public override bool Pop(out int edgeIndex)
			{
				if (_queue.Count == 0)
				{
					edgeIndex = -1;
					return false;
				}

				front = _queue.front;
				edgeIndex = front.edgeIndex;
				_queue.Pop();
				return true;
			}
		}

		private static void VisitByDistance<T>(DistanceQueue<T> queue, VisitVertexEdgeDelegate visitor)
		{
			var topology = queue.topology;
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex));
			});
		}

		private static void VisitByDistance<T>(DistanceQueue<T> queue, VisitVertexDelegate visitor)
		{
			var topology = queue.topology;
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex).farVertex);
			});
		}

		private static void VisitByDistance<T>(DistanceQueue<T> queue, VisitVertexEdgeWithStateDelegate visitor)
		{
			var topology = queue.topology;
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex), ref state);
			});
		}

		private static void VisitByDistance<T>(DistanceQueue<T> queue, VisitVertexWithStateDelegate visitor)
		{
			var topology = queue.topology;
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex).farVertex, ref state);
			});
		}

		private static void VisitByDistance<T>(DistanceQueue<T> queue, VisitVertexEdgeWithDistanceDelegate<T> visitor)
		{
			var topology = queue.topology;
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex), queue.front.distance);
			});
		}

		private static void VisitByDistance<T>(DistanceQueue<T> queue, VisitVertexWithDistanceDelegate<T> visitor)
		{
			var topology = queue.topology;
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex).farVertex, queue.front.distance);
			});
		}

		private static void VisitByDistance<T>(DistanceQueue<T> queue, VisitVertexEdgeWithDistanceStateDelegate<T> visitor)
		{
			var topology = queue.topology;
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex), queue.front.distance, ref state);
			});
		}

		private static void VisitByDistance<T>(DistanceQueue<T> queue, VisitVertexWithDistanceStateDelegate<T> visitor)
		{
			var topology = queue.topology;
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex).farVertex, queue.front.distance, ref state);
			});
		}

		private static void VisitByDistance<T>(DistanceQueue<T> queue, VisitVertexEdgeWithDepthDistanceDelegate<T> visitor)
		{
			var topology = queue.topology;
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex), queue.front.depth, queue.front.distance);
			});
		}

		private static void VisitByDistance<T>(DistanceQueue<T> queue, VisitVertexWithDepthDistanceDelegate<T> visitor)
		{
			var topology = queue.topology;
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex).farVertex, queue.front.depth, queue.front.distance);
			});
		}

		private static void VisitByDistance<T>(DistanceQueue<T> queue, VisitVertexEdgeWithDepthDistanceStateDelegate<T> visitor)
		{
			var topology = queue.topology;
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex), queue.front.depth, queue.front.distance, ref state);
			});
		}

		private static void VisitByDistance<T>(DistanceQueue<T> queue, VisitVertexWithDepthDistanceStateDelegate<T> visitor)
		{
			var topology = queue.topology;
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex).farVertex, queue.front.depth, queue.front.distance, ref state);
			});
		}

		#endregion

		#region By Int Edge Attribute

		private class IntDistanceQueue : DistanceQueue<int>
		{
			public IntDistanceQueue(bool breadthFirst)
			{
				if (breadthFirst)
					Initialize(BreadthFirstComparison);
				else
					Initialize(DepthFirstComparison);
			}

			private static bool BreadthFirstComparison(DistanceQueueElement<int> lhs, DistanceQueueElement<int> rhs) { return lhs.distance <= rhs.distance; }
			private static bool DepthFirstComparison(DistanceQueueElement<int> lhs, DistanceQueueElement<int> rhs) { return lhs.distance >= rhs.distance; }
			public override int AccumulateDistance(int lhs, int rhs) { return lhs + rhs; }
		}

		private static DistanceQueue<int> CreateDistanceQueue(bool breadthFirst, Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances)
		{
			var queue = new IntDistanceQueue(breadthFirst);
			var topology = queue.PushRoots(rootVertex);
			queue.edgeDistance = (int edgeIndex) => edgeDistances[new Topology.VertexEdge(topology, edgeIndex)];
			return queue;
		}

		private static DistanceQueue<int> CreateDistanceQueue(bool breadthFirst, IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<int> edgeDistances)
		{
			var queue = new IntDistanceQueue(breadthFirst);
			var topology = queue.PushRoots(rootVertices);
			queue.edgeDistance = (int edgeIndex) => edgeDistances[new Topology.VertexEdge(topology, edgeIndex)];
			return queue;
		}

		#region Breadth First

		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, VisitVertexEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, VisitVertexDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, VisitVertexEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, VisitVertexWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, VisitVertexEdgeWithDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, VisitVertexWithDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, VisitVertexEdgeWithDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, VisitVertexWithDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, VisitVertexEdgeWithDepthDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, VisitVertexWithDepthDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, VisitVertexEdgeWithDepthDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, VisitVertexWithDepthDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }

		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<int> edgeDistances, VisitVertexEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<int> edgeDistances, VisitVertexDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<int> edgeDistances, VisitVertexEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<int> edgeDistances, VisitVertexWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<int> edgeDistances, VisitVertexEdgeWithDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<int> edgeDistances, VisitVertexWithDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<int> edgeDistances, VisitVertexEdgeWithDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<int> edgeDistances, VisitVertexWithDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<int> edgeDistances, VisitVertexEdgeWithDepthDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<int> edgeDistances, VisitVertexWithDepthDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<int> edgeDistances, VisitVertexEdgeWithDepthDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<int> edgeDistances, VisitVertexWithDepthDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }

		#endregion

		#region Depth First

		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, VisitVertexEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, VisitVertexDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, VisitVertexEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, VisitVertexWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, VisitVertexEdgeWithDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, VisitVertexWithDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, VisitVertexEdgeWithDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, VisitVertexWithDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, VisitVertexEdgeWithDepthDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, VisitVertexWithDepthDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, VisitVertexEdgeWithDepthDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<int> edgeDistances, VisitVertexWithDepthDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }

		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<int> edgeDistances, VisitVertexEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<int> edgeDistances, VisitVertexDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<int> edgeDistances, VisitVertexEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<int> edgeDistances, VisitVertexWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<int> edgeDistances, VisitVertexEdgeWithDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<int> edgeDistances, VisitVertexWithDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<int> edgeDistances, VisitVertexEdgeWithDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<int> edgeDistances, VisitVertexWithDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<int> edgeDistances, VisitVertexEdgeWithDepthDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<int> edgeDistances, VisitVertexWithDepthDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<int> edgeDistances, VisitVertexEdgeWithDepthDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<int> edgeDistances, VisitVertexWithDepthDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }

		#endregion

		#endregion

		#region By UInt Edge Attribute

		private class UIntDistanceQueue : DistanceQueue<uint>
		{
			public UIntDistanceQueue(bool breadthFirst)
			{
				if (breadthFirst)
					Initialize(BreadthFirstComparison);
				else
					Initialize(DepthFirstComparison);
			}

			private static bool BreadthFirstComparison(DistanceQueueElement<uint> lhs, DistanceQueueElement<uint> rhs) { return lhs.distance <= rhs.distance; }
			private static bool DepthFirstComparison(DistanceQueueElement<uint> lhs, DistanceQueueElement<uint> rhs) { return lhs.distance >= rhs.distance; }
			public override uint AccumulateDistance(uint lhs, uint rhs) { return lhs + rhs; }
		}

		private static DistanceQueue<uint> CreateDistanceQueue(bool breadthFirst, Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances)
		{
			var queue = new UIntDistanceQueue(breadthFirst);
			var topology = queue.PushRoots(rootVertex);
			queue.edgeDistance = (int edgeIndex) => edgeDistances[new Topology.VertexEdge(topology, edgeIndex)];
			return queue;
		}

		private static DistanceQueue<uint> CreateDistanceQueue(bool breadthFirst, IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<uint> edgeDistances)
		{
			var queue = new UIntDistanceQueue(breadthFirst);
			var topology = queue.PushRoots(rootVertices);
			queue.edgeDistance = (int edgeIndex) => edgeDistances[new Topology.VertexEdge(topology, edgeIndex)];
			return queue;
		}

		#region Breadth First

		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, VisitVertexEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, VisitVertexDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, VisitVertexEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, VisitVertexWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, VisitVertexEdgeWithDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, VisitVertexWithDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, VisitVertexEdgeWithDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, VisitVertexWithDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, VisitVertexEdgeWithDepthDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, VisitVertexWithDepthDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, VisitVertexEdgeWithDepthDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, VisitVertexWithDepthDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }

		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<uint> edgeDistances, VisitVertexEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<uint> edgeDistances, VisitVertexDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<uint> edgeDistances, VisitVertexEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<uint> edgeDistances, VisitVertexWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<uint> edgeDistances, VisitVertexEdgeWithDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<uint> edgeDistances, VisitVertexWithDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<uint> edgeDistances, VisitVertexEdgeWithDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<uint> edgeDistances, VisitVertexWithDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<uint> edgeDistances, VisitVertexEdgeWithDepthDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<uint> edgeDistances, VisitVertexWithDepthDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<uint> edgeDistances, VisitVertexEdgeWithDepthDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<uint> edgeDistances, VisitVertexWithDepthDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }

		#endregion

		#region Depth First

		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, VisitVertexEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, VisitVertexDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, VisitVertexEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, VisitVertexWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, VisitVertexEdgeWithDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, VisitVertexWithDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, VisitVertexEdgeWithDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, VisitVertexWithDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, VisitVertexEdgeWithDepthDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, VisitVertexWithDepthDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, VisitVertexEdgeWithDepthDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<uint> edgeDistances, VisitVertexWithDepthDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }

		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<uint> edgeDistances, VisitVertexEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<uint> edgeDistances, VisitVertexDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<uint> edgeDistances, VisitVertexEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<uint> edgeDistances, VisitVertexWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<uint> edgeDistances, VisitVertexEdgeWithDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<uint> edgeDistances, VisitVertexWithDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<uint> edgeDistances, VisitVertexEdgeWithDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<uint> edgeDistances, VisitVertexWithDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<uint> edgeDistances, VisitVertexEdgeWithDepthDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<uint> edgeDistances, VisitVertexWithDepthDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<uint> edgeDistances, VisitVertexEdgeWithDepthDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<uint> edgeDistances, VisitVertexWithDepthDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }

		#endregion

		#endregion

		#region By Float Edge Attribute

		private class FloatDistanceQueue : DistanceQueue<float>
		{
			public FloatDistanceQueue(bool breadthFirst)
			{
				if (breadthFirst)
					Initialize(BreadthFirstComparison);
				else
					Initialize(DepthFirstComparison);
			}

			private static bool BreadthFirstComparison(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) { return lhs.distance <= rhs.distance; }
			private static bool DepthFirstComparison(DistanceQueueElement<float> lhs, DistanceQueueElement<float> rhs) { return lhs.distance >= rhs.distance; }
			public override float AccumulateDistance(float lhs, float rhs) { return lhs + rhs; }
		}

		private static DistanceQueue<float> CreateDistanceQueue(bool breadthFirst, Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances)
		{
			var queue = new FloatDistanceQueue(breadthFirst);
			var topology = queue.PushRoots(rootVertex);
			queue.edgeDistance = (int edgeIndex) => edgeDistances[new Topology.VertexEdge(topology, edgeIndex)];
			return queue;
		}

		private static DistanceQueue<float> CreateDistanceQueue(bool breadthFirst, IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<float> edgeDistances)
		{
			var queue = new FloatDistanceQueue(breadthFirst);
			var topology = queue.PushRoots(rootVertices);
			queue.edgeDistance = (int edgeIndex) => edgeDistances[new Topology.VertexEdge(topology, edgeIndex)];
			return queue;
		}

		#region Breadth First

		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, VisitVertexEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, VisitVertexDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, VisitVertexEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, VisitVertexWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, VisitVertexEdgeWithDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, VisitVertexWithDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, VisitVertexEdgeWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, VisitVertexWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, VisitVertexEdgeWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, VisitVertexWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, VisitVertexEdgeWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, VisitVertexWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }

		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<float> edgeDistances, VisitVertexEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<float> edgeDistances, VisitVertexDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<float> edgeDistances, VisitVertexEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<float> edgeDistances, VisitVertexWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<float> edgeDistances, VisitVertexEdgeWithDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<float> edgeDistances, VisitVertexWithDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<float> edgeDistances, VisitVertexEdgeWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<float> edgeDistances, VisitVertexWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<float> edgeDistances, VisitVertexEdgeWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<float> edgeDistances, VisitVertexWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<float> edgeDistances, VisitVertexEdgeWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<float> edgeDistances, VisitVertexWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }

		#endregion

		#region Depth First

		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, VisitVertexEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, VisitVertexDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, VisitVertexEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, VisitVertexWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, VisitVertexEdgeWithDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, VisitVertexWithDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, VisitVertexEdgeWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, VisitVertexWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, VisitVertexEdgeWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, VisitVertexWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, VisitVertexEdgeWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<float> edgeDistances, VisitVertexWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }

		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<float> edgeDistances, VisitVertexEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<float> edgeDistances, VisitVertexDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<float> edgeDistances, VisitVertexEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<float> edgeDistances, VisitVertexWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<float> edgeDistances, VisitVertexEdgeWithDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<float> edgeDistances, VisitVertexWithDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<float> edgeDistances, VisitVertexEdgeWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<float> edgeDistances, VisitVertexWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<float> edgeDistances, VisitVertexEdgeWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<float> edgeDistances, VisitVertexWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<float> edgeDistances, VisitVertexEdgeWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<float> edgeDistances, VisitVertexWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }

		#endregion

		#endregion

		#region By Double Edge Attribute

		private class DoubleDistanceQueue : DistanceQueue<double>
		{
			public DoubleDistanceQueue(bool breadthFirst)
			{
				if (breadthFirst)
					Initialize(BreadthFirstComparison);
				else
					Initialize(DepthFirstComparison);
			}

			private static bool BreadthFirstComparison(DistanceQueueElement<double> lhs, DistanceQueueElement<double> rhs) { return lhs.distance <= rhs.distance; }
			private static bool DepthFirstComparison(DistanceQueueElement<double> lhs, DistanceQueueElement<double> rhs) { return lhs.distance >= rhs.distance; }
			public override double AccumulateDistance(double lhs, double rhs) { return lhs + rhs; }
		}

		private static DistanceQueue<double> CreateDistanceQueue(bool breadthFirst, Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances)
		{
			var queue = new DoubleDistanceQueue(breadthFirst);
			var topology = queue.PushRoots(rootVertex);
			queue.edgeDistance = (int edgeIndex) => edgeDistances[new Topology.VertexEdge(topology, edgeIndex)];
			return queue;
		}

		private static DistanceQueue<double> CreateDistanceQueue(bool breadthFirst, IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<double> edgeDistances)
		{
			var queue = new DoubleDistanceQueue(breadthFirst);
			var topology = queue.PushRoots(rootVertices);
			queue.edgeDistance = (int edgeIndex) => edgeDistances[new Topology.VertexEdge(topology, edgeIndex)];
			return queue;
		}

		#region Breadth First

		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, VisitVertexEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, VisitVertexDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, VisitVertexEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, VisitVertexWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, VisitVertexEdgeWithDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, VisitVertexWithDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, VisitVertexEdgeWithDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, VisitVertexWithDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, VisitVertexEdgeWithDepthDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, VisitVertexWithDepthDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, VisitVertexEdgeWithDepthDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, VisitVertexWithDepthDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertex, edgeDistances), visitor); }

		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<double> edgeDistances, VisitVertexEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<double> edgeDistances, VisitVertexDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<double> edgeDistances, VisitVertexEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<double> edgeDistances, VisitVertexWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<double> edgeDistances, VisitVertexEdgeWithDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<double> edgeDistances, VisitVertexWithDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<double> edgeDistances, VisitVertexEdgeWithDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<double> edgeDistances, VisitVertexWithDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<double> edgeDistances, VisitVertexEdgeWithDepthDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<double> edgeDistances, VisitVertexWithDepthDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<double> edgeDistances, VisitVertexEdgeWithDepthDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<double> edgeDistances, VisitVertexWithDepthDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootVertices, edgeDistances), visitor); }

		#endregion

		#region Depth First

		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, VisitVertexEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, VisitVertexDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, VisitVertexEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, VisitVertexWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, VisitVertexEdgeWithDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, VisitVertexWithDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, VisitVertexEdgeWithDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, VisitVertexWithDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, VisitVertexEdgeWithDepthDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, VisitVertexWithDepthDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, VisitVertexEdgeWithDepthDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Vertex rootVertex, IEdgeAttribute<double> edgeDistances, VisitVertexWithDepthDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertex, edgeDistances), visitor); }

		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<double> edgeDistances, VisitVertexEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<double> edgeDistances, VisitVertexDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<double> edgeDistances, VisitVertexEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<double> edgeDistances, VisitVertexWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<double> edgeDistances, VisitVertexEdgeWithDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<double> edgeDistances, VisitVertexWithDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<double> edgeDistances, VisitVertexEdgeWithDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<double> edgeDistances, VisitVertexWithDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<double> edgeDistances, VisitVertexEdgeWithDepthDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<double> edgeDistances, VisitVertexWithDepthDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<double> edgeDistances, VisitVertexEdgeWithDepthDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Vertex> rootVertices, IEdgeAttribute<double> edgeDistances, VisitVertexWithDepthDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootVertices, edgeDistances), visitor); }

		#endregion

		#endregion

		#region By Euclidean Distance

		private static DistanceQueue<float> CreateEuclideanDistanceQueue(bool breadthFirst, Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions)
		{
			var queue = new FloatDistanceQueue(breadthFirst);
			var topology = queue.PushRoots(rootVertex);
			queue.edgeDistance = (int edgeIndex) =>
			{
				var edge = new Topology.VertexEdge(topology, edgeIndex);
				return Vector3.Distance(vertexPositions[edge.nearVertex], vertexPositions[edge]);
			};
			return queue;
		}

		private static DistanceQueue<float> CreateEuclideanDistanceQueue(bool breadthFirst, IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions)
		{
			var queue = new FloatDistanceQueue(breadthFirst);
			var topology = queue.PushRoots(rootVertices);
			queue.edgeDistance = (int edgeIndex) =>
			{
				var edge = new Topology.VertexEdge(topology, edgeIndex);
				return Vector3.Distance(vertexPositions[edge.nearVertex], vertexPositions[edge]);
			};
			return queue;
		}

		#region Breadth First

		public static void VisitBreadthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, VisitVertexEdgeDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootVertex, vertexPositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, VisitVertexDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootVertex, vertexPositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, VisitVertexEdgeWithStateDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootVertex, vertexPositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, VisitVertexWithStateDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootVertex, vertexPositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, VisitVertexEdgeWithDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootVertex, vertexPositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, VisitVertexWithDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootVertex, vertexPositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, VisitVertexEdgeWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootVertex, vertexPositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, VisitVertexWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootVertex, vertexPositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, VisitVertexEdgeWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootVertex, vertexPositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, VisitVertexWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootVertex, vertexPositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, VisitVertexEdgeWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootVertex, vertexPositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, VisitVertexWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootVertex, vertexPositions), visitor); }

		public static void VisitBreadthFirstByEuclideanDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, VisitVertexEdgeDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootVertices, vertexPositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, VisitVertexDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootVertices, vertexPositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, VisitVertexEdgeWithStateDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootVertices, vertexPositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, VisitVertexWithStateDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootVertices, vertexPositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, VisitVertexEdgeWithDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootVertices, vertexPositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, VisitVertexWithDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootVertices, vertexPositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, VisitVertexEdgeWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootVertices, vertexPositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, VisitVertexWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootVertices, vertexPositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, VisitVertexEdgeWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootVertices, vertexPositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, VisitVertexWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootVertices, vertexPositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, VisitVertexEdgeWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootVertices, vertexPositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, VisitVertexWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootVertices, vertexPositions), visitor); }

		#endregion

		#region Depth First

		public static void VisitDepthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, VisitVertexEdgeDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootVertex, vertexPositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, VisitVertexDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootVertex, vertexPositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, VisitVertexEdgeWithStateDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootVertex, vertexPositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, VisitVertexWithStateDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootVertex, vertexPositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, VisitVertexEdgeWithDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootVertex, vertexPositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, VisitVertexWithDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootVertex, vertexPositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, VisitVertexEdgeWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootVertex, vertexPositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, VisitVertexWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootVertex, vertexPositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, VisitVertexEdgeWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootVertex, vertexPositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, VisitVertexWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootVertex, vertexPositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, VisitVertexEdgeWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootVertex, vertexPositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, VisitVertexWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootVertex, vertexPositions), visitor); }

		public static void VisitDepthFirstByEuclideanDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, VisitVertexEdgeDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootVertices, vertexPositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, VisitVertexDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootVertices, vertexPositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, VisitVertexEdgeWithStateDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootVertices, vertexPositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, VisitVertexWithStateDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootVertices, vertexPositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, VisitVertexEdgeWithDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootVertices, vertexPositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, VisitVertexWithDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootVertices, vertexPositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, VisitVertexEdgeWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootVertices, vertexPositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, VisitVertexWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootVertices, vertexPositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, VisitVertexEdgeWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootVertices, vertexPositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, VisitVertexWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootVertices, vertexPositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, VisitVertexEdgeWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootVertices, vertexPositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, VisitVertexWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootVertices, vertexPositions), visitor); }

		#endregion

		#endregion

		#region By Spherical Distance

		private static DistanceQueue<float> CreateSphericalDistanceQueue(bool breadthFirst, Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius)
		{
			var queue = new FloatDistanceQueue(breadthFirst);
			var topology = queue.PushRoots(rootVertex);
			queue.edgeDistance = (int edgeIndex) =>
			{
				var edge = new Topology.VertexEdge(topology, edgeIndex);
				return GeometryUtility.AngleBetweenVectors(vertexPositions[edge.nearVertex], vertexPositions[edge]) * sphereRadius;
			};
			return queue;
		}

		private static DistanceQueue<float> CreateSphericalDistanceQueue(bool breadthFirst, IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, float sphereRadius)
		{
			var queue = new FloatDistanceQueue(breadthFirst);
			var topology = queue.PushRoots(rootVertices);
			queue.edgeDistance = (int edgeIndex) =>
			{
				var edge = new Topology.VertexEdge(topology, edgeIndex);
				return GeometryUtility.AngleBetweenVectors(vertexPositions[edge.nearVertex], vertexPositions[edge]) * sphereRadius;
			};
			return queue;
		}

		#region Breadth First

		public static void VisitBreadthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexEdgeDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootVertex, vertexPositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootVertex, vertexPositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexEdgeWithStateDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootVertex, vertexPositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexWithStateDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootVertex, vertexPositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexEdgeWithDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootVertex, vertexPositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexWithDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootVertex, vertexPositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexEdgeWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootVertex, vertexPositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootVertex, vertexPositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexEdgeWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootVertex, vertexPositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootVertex, vertexPositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexEdgeWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootVertex, vertexPositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootVertex, vertexPositions, sphereRadius), visitor); }

		public static void VisitBreadthFirstBySphericalDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexEdgeDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootVertices, vertexPositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootVertices, vertexPositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexEdgeWithStateDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootVertices, vertexPositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexWithStateDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootVertices, vertexPositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexEdgeWithDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootVertices, vertexPositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexWithDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootVertices, vertexPositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexEdgeWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootVertices, vertexPositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootVertices, vertexPositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexEdgeWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootVertices, vertexPositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootVertices, vertexPositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexEdgeWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootVertices, vertexPositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootVertices, vertexPositions, sphereRadius), visitor); }

		#endregion

		#region Depth First

		public static void VisitDepthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexEdgeDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootVertex, vertexPositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootVertex, vertexPositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexEdgeWithStateDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootVertex, vertexPositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexWithStateDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootVertex, vertexPositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexEdgeWithDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootVertex, vertexPositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexWithDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootVertex, vertexPositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexEdgeWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootVertex, vertexPositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootVertex, vertexPositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexEdgeWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootVertex, vertexPositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootVertex, vertexPositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexEdgeWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootVertex, vertexPositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(Topology.Vertex rootVertex, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootVertex, vertexPositions, sphereRadius), visitor); }

		public static void VisitDepthFirstBySphericalDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexEdgeDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootVertices, vertexPositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootVertices, vertexPositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexEdgeWithStateDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootVertices, vertexPositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexWithStateDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootVertices, vertexPositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexEdgeWithDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootVertices, vertexPositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexWithDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootVertices, vertexPositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexEdgeWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootVertices, vertexPositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootVertices, vertexPositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexEdgeWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootVertices, vertexPositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootVertices, vertexPositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexEdgeWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootVertices, vertexPositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(IEnumerable<Topology.Vertex> rootVertices, IVertexAttribute<Vector3> vertexPositions, float sphereRadius, VisitVertexWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootVertices, vertexPositions, sphereRadius), visitor); }

		#endregion

		#endregion

		#endregion

		#region Random Order Connected Visitation

		#region Queue

		private class RandomConnectedQueue : Queue
		{
			private List<int> _queue;
			private IRandomEngine _randomEngine;
			public int frontEdgeIndex;

			public RandomConnectedQueue(IRandomEngine randomEngine)
			{
				_queue = new List<int>();
				_randomEngine = randomEngine;
			}

			public override int edgeIndex { get { return frontEdgeIndex; } }

			public override void Push(int edgeIndex)
			{
				_queue.Add(edgeIndex);
			}

			public override bool Pop(out int edgeIndex)
			{
				if (_queue.Count == 0)
				{
					edgeIndex = -1;
					return false;
				}

				var queueIndex = RandomUtility.HalfOpenRange(_queue.Count, _randomEngine);
				edgeIndex = frontEdgeIndex = _queue[queueIndex];
				var backIndex = _queue.Count - 1;
				_queue[queueIndex] = _queue[backIndex];
				_queue.RemoveAt(backIndex);
				return true;
			}
		}

		private static void VisitConnectedInRandomOrder(RandomConnectedQueue queue, Topology topology, VisitVertexEdgeDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex));
			});
		}

		private static void VisitConnectedInRandomOrder(RandomConnectedQueue queue, Topology topology, VisitVertexDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex).farVertex);
			});
		}

		private static void VisitConnectedInRandomOrder(RandomConnectedQueue queue, Topology topology, VisitVertexEdgeWithStateDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex), ref state);
			});
		}

		private static void VisitConnectedInRandomOrder(RandomConnectedQueue queue, Topology topology, VisitVertexWithStateDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.VertexEdge(topology, edgeIndex).farVertex, ref state);
			});
		}

		#endregion

		#region Visit From Single Root

		public static void VisitConnectedInRandomOrder(Topology.Vertex rootVertex, IRandomEngine randomEngine, VisitVertexEdgeDelegate visitor)
		{
			var queue = new RandomConnectedQueue(randomEngine);
			VisitConnectedInRandomOrder(queue, queue.PushRoots(rootVertex), visitor);
		}

		public static void VisitConnectedInRandomOrder(Topology.Vertex rootVertex, IRandomEngine randomEngine, VisitVertexDelegate visitor)
		{
			var queue = new RandomConnectedQueue(randomEngine);
			VisitConnectedInRandomOrder(queue, queue.PushRoots(rootVertex), visitor);
		}

		public static void VisitConnectedInRandomOrder(Topology.Vertex rootVertex, IRandomEngine randomEngine, VisitVertexEdgeWithStateDelegate visitor)
		{
			var queue = new RandomConnectedQueue(randomEngine);
			VisitConnectedInRandomOrder(queue, queue.PushRoots(rootVertex), visitor);
		}

		public static void VisitConnectedInRandomOrder(Topology.Vertex rootVertex, IRandomEngine randomEngine, VisitVertexWithStateDelegate visitor)
		{
			var queue = new RandomConnectedQueue(randomEngine);
			VisitConnectedInRandomOrder(queue, queue.PushRoots(rootVertex), visitor);
		}

		#endregion

		#region Visit From Multiple Roots

		public static void VisitConnectedInRandomOrder(IEnumerable<Topology.Vertex> rootVertices, IRandomEngine randomEngine, VisitVertexEdgeDelegate visitor)
		{
			var queue = new RandomConnectedQueue(randomEngine);
			VisitConnectedInRandomOrder(queue, queue.PushRoots(rootVertices), visitor);
		}

		public static void VisitConnectedInRandomOrder(IEnumerable<Topology.Vertex> rootVertices, IRandomEngine randomEngine, VisitVertexDelegate visitor)
		{
			var queue = new RandomConnectedQueue(randomEngine);
			VisitConnectedInRandomOrder(queue, queue.PushRoots(rootVertices), visitor);
		}

		public static void VisitConnectedInRandomOrder(IEnumerable<Topology.Vertex> rootVertices, IRandomEngine randomEngine, VisitVertexEdgeWithStateDelegate visitor)
		{
			var queue = new RandomConnectedQueue(randomEngine);
			VisitConnectedInRandomOrder(queue, queue.PushRoots(rootVertices), visitor);
		}

		public static void VisitConnectedInRandomOrder(IEnumerable<Topology.Vertex> rootVertices, IRandomEngine randomEngine, VisitVertexWithStateDelegate visitor)
		{
			var queue = new RandomConnectedQueue(randomEngine);
			VisitConnectedInRandomOrder(queue, queue.PushRoots(rootVertices), visitor);
		}

		#endregion

		#endregion
	}
}
