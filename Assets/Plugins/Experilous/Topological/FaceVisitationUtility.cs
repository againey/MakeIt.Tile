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
			private BitArray _visitedFaces;

			public abstract void Push(int edgeIndex);
			public abstract bool Pop(out int edgeIndex);
			public abstract int edgeIndex { get; }

			public Topology PushRoots(Topology.Face rootFace)
			{
				topology = rootFace.topology;
				_visitedFaces = new BitArray(topology.faces.Count);
				_visitedFaces[rootFace.index] = true;

				foreach (var edge in rootFace.edges)
				{
					if (!edge.isOuterBoundary)
					{
						Push(edge.index);
					}
				}

				return topology;
			}

			public Topology PushRoots(IEnumerable<Topology.Face> rootFaces)
			{
				PushRoots(PrepareToPushRoots(rootFaces));
				return topology;
			}

			public IEnumerator<Topology.Face> PrepareToPushRoots(IEnumerable<Topology.Face> rootFaces)
			{
				var enumerator = rootFaces.GetEnumerator();

				if (!enumerator.MoveNext()) return null;

				topology = enumerator.Current.topology;
				_visitedFaces = new BitArray(topology.faces.Count);

				return enumerator;
			}

			public void PushRoots(IEnumerator<Topology.Face> rootFaces)
			{
				do
				{
					_visitedFaces[rootFaces.Current.index] = true;

					foreach (var edge in rootFaces.Current.edges)
					{
						if (!edge.isOuterBoundary)
						{
							Push(edge.index);
						}
					}
				} while (rootFaces.MoveNext());
			}

			public void Visit(VisitorDelegate visitor)
			{
				int edgeIndex;
				while (Pop(out edgeIndex))
				{
					int farFaceIndex = topology.edgeData[edgeIndex].face;
					if (_visitedFaces[farFaceIndex] == false)
					{
						VisitationState state = VisitationState.Continue;
						visitor(edgeIndex, ref state);
						switch (state)
						{
							case VisitationState.Continue:
								_visitedFaces[farFaceIndex] = true;
								foreach (var nextEdge in new Topology.Face(topology, farFaceIndex).edges)
								{
									if (nextEdge.twinIndex != edgeIndex && !nextEdge.isOuterBoundary && _visitedFaces[nextEdge.farFace.index] == false)
									{
										Push(nextEdge.index);
									}
								}
								break;
							case VisitationState.Discontinue:
								_visitedFaces[farFaceIndex] = true;
								break;
							case VisitationState.Stop:
								return;
						}
					}
				}
			}
		}

		#endregion

		#region Any Order Adjacency Visitation

		public delegate void VisitFaceEdgeDelegate(Topology.FaceEdge faceEdge);
		public delegate void VisitFaceDelegate(Topology.Face face);

		public delegate void VisitFaceEdgeWithStateDelegate(Topology.FaceEdge faceEdge, ref VisitationState state);
		public delegate void VisitFaceWithStateDelegate(Topology.Face face, ref VisitationState state);

		#region Queue

		private class AdjacencyQueue : Queue
		{
			private List<int> _queue;
			public int frontEdgeIndex;

			public AdjacencyQueue()
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

		private static void VisitAdjacentInAnyOrder(AdjacencyQueue queue, Topology topology, VisitFaceEdgeDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex));
			});
		}

		private static void VisitAdjacentInAnyOrder(AdjacencyQueue queue, Topology topology, VisitFaceDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex).farFace);
			});
		}

		private static void VisitAdjacentInAnyOrder(AdjacencyQueue queue, Topology topology, VisitFaceEdgeWithStateDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex), ref state);
			});
		}

		private static void VisitAdjacentInAnyOrder(AdjacencyQueue queue, Topology topology, VisitFaceWithStateDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex).farFace, ref state);
			});
		}

		#endregion

		#region Visit From Single Root

		public static void VisitAdjacentInAnyOrder(Topology.Face rootFace, VisitFaceEdgeDelegate visitor)
		{
			var queue = new AdjacencyQueue();
			VisitAdjacentInAnyOrder(queue, queue.PushRoots(rootFace), visitor);
		}

		public static void VisitAdjacentInAnyOrder(Topology.Face rootFace, VisitFaceDelegate visitor)
		{
			var queue = new AdjacencyQueue();
			VisitAdjacentInAnyOrder(queue, queue.PushRoots(rootFace), visitor);
		}

		public static void VisitAdjacentInAnyOrder(Topology.Face rootFace, VisitFaceEdgeWithStateDelegate visitor)
		{
			var queue = new AdjacencyQueue();
			VisitAdjacentInAnyOrder(queue, queue.PushRoots(rootFace), visitor);
		}

		public static void VisitAdjacentInAnyOrder(Topology.Face rootFace, VisitFaceWithStateDelegate visitor)
		{
			var queue = new AdjacencyQueue();
			VisitAdjacentInAnyOrder(queue, queue.PushRoots(rootFace), visitor);
		}

		#endregion

		#region Visit From Multiple Roots

		public static void VisitAdjacentInAnyOrder(IEnumerable<Topology.Face> rootFaces, VisitFaceEdgeDelegate visitor)
		{
			var queue = new AdjacencyQueue();
			VisitAdjacentInAnyOrder(queue, queue.PushRoots(rootFaces), visitor);
		}

		public static void VisitAdjacentInAnyOrder(IEnumerable<Topology.Face> rootFaces, VisitFaceDelegate visitor)
		{
			var queue = new AdjacencyQueue();
			VisitAdjacentInAnyOrder(queue, queue.PushRoots(rootFaces), visitor);
		}

		public static void VisitAdjacentInAnyOrder(IEnumerable<Topology.Face> rootFaces, VisitFaceEdgeWithStateDelegate visitor)
		{
			var queue = new AdjacencyQueue();
			VisitAdjacentInAnyOrder(queue, queue.PushRoots(rootFaces), visitor);
		}

		public static void VisitAdjacentInAnyOrder(IEnumerable<Topology.Face> rootFaces, VisitFaceWithStateDelegate visitor)
		{
			var queue = new AdjacencyQueue();
			VisitAdjacentInAnyOrder(queue, queue.PushRoots(rootFaces), visitor);
		}

		#endregion

		#endregion

		#region Depth Ordered Visitation

		public delegate void VisitFaceEdgeWithDepthDelegate(Topology.FaceEdge faceEdge, int depth);
		public delegate void VisitFaceWithDepthDelegate(Topology.Face face, int depth);

		public delegate void VisitFaceEdgeWithDepthStateDelegate(Topology.FaceEdge faceEdge, int depth, ref VisitationState state);
		public delegate void VisitFaceWithDepthStateDelegate(Topology.Face face, int depth, ref VisitationState state);

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

		private static void VisitByDepth(DepthQueue queue, Topology topology, VisitFaceEdgeDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex));
			});
		}

		private static void VisitByDepth(DepthQueue queue, Topology topology, VisitFaceDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex).farFace);
			});
		}

		private static void VisitByDepth(DepthQueue queue, Topology topology, VisitFaceEdgeWithStateDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex), ref state);
			});
		}

		private static void VisitByDepth(DepthQueue queue, Topology topology, VisitFaceWithStateDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex).farFace, ref state);
			});
		}

		private static void VisitByDepth(DepthQueue queue, Topology topology, VisitFaceEdgeWithDepthDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex), queue.front.depth);
			});
		}

		private static void VisitByDepth(DepthQueue queue, Topology topology, VisitFaceWithDepthDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex).farFace, queue.front.depth);
			});
		}

		private static void VisitByDepth(DepthQueue queue, Topology topology, VisitFaceEdgeWithDepthStateDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex), queue.front.depth, ref state);
			});
		}

		private static void VisitByDepth(DepthQueue queue, Topology topology, VisitFaceWithDepthStateDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex).farFace, queue.front.depth, ref state);
			});
		}

		#endregion

		#region Breadth First

		#region Visit From Single Root

		public static void VisitBreadthFirstByDepth(Topology.Face rootFace, VisitFaceEdgeDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFace), visitor);
		}

		public static void VisitBreadthFirstByDepth(Topology.Face rootFace, VisitFaceDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFace), visitor);
		}

		public static void VisitBreadthFirstByDepth(Topology.Face rootFace, VisitFaceEdgeWithStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFace), visitor);
		}

		public static void VisitBreadthFirstByDepth(Topology.Face rootFace, VisitFaceWithStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFace), visitor);
		}

		public static void VisitBreadthFirstByDepth(Topology.Face rootFace, VisitFaceEdgeWithDepthDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFace), visitor);
		}

		public static void VisitBreadthFirstByDepth(Topology.Face rootFace, VisitFaceWithDepthDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFace), visitor);
		}

		public static void VisitBreadthFirstByDepth(Topology.Face rootFace, VisitFaceEdgeWithDepthStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFace), visitor);
		}

		public static void VisitBreadthFirstByDepth(Topology.Face rootFace, VisitFaceWithDepthStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFace), visitor);
		}

		#endregion

		#region Visit From Multiple Roots

		public static void VisitBreadthFirstByDepth(IEnumerable<Topology.Face> rootFaces, VisitFaceEdgeDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFaces), visitor);
		}

		public static void VisitBreadthFirstByDepth(IEnumerable<Topology.Face> rootFaces, VisitFaceDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFaces), visitor);
		}

		public static void VisitBreadthFirstByDepth(IEnumerable<Topology.Face> rootFaces, VisitFaceEdgeWithStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFaces), visitor);
		}

		public static void VisitBreadthFirstByDepth(IEnumerable<Topology.Face> rootFaces, VisitFaceWithStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFaces), visitor);
		}

		public static void VisitBreadthFirstByDepth(IEnumerable<Topology.Face> rootFaces, VisitFaceEdgeWithDepthDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFaces), visitor);
		}

		public static void VisitBreadthFirstByDepth(IEnumerable<Topology.Face> rootFaces, VisitFaceWithDepthDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFaces), visitor);
		}

		public static void VisitBreadthFirstByDepth(IEnumerable<Topology.Face> rootFaces, VisitFaceEdgeWithDepthStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFaces), visitor);
		}

		public static void VisitBreadthFirstByDepth(IEnumerable<Topology.Face> rootFaces, VisitFaceWithDepthStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth <= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFaces), visitor);
		}

		#endregion

		#endregion

		#region Depth First

		#region Visit From Single Root

		public static void VisitDepthFirstByDepth(Topology.Face rootFace, VisitFaceEdgeDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFace), visitor);
		}

		public static void VisitDepthFirstByDepth(Topology.Face rootFace, VisitFaceDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFace), visitor);
		}

		public static void VisitDepthFirstByDepth(Topology.Face rootFace, VisitFaceEdgeWithStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFace), visitor);
		}

		public static void VisitDepthFirstByDepth(Topology.Face rootFace, VisitFaceWithStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFace), visitor);
		}

		public static void VisitDepthFirstByDepth(Topology.Face rootFace, VisitFaceEdgeWithDepthDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFace), visitor);
		}

		public static void VisitDepthFirstByDepth(Topology.Face rootFace, VisitFaceWithDepthDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFace), visitor);
		}

		public static void VisitDepthFirstByDepth(Topology.Face rootFace, VisitFaceEdgeWithDepthStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFace), visitor);
		}

		public static void VisitDepthFirstByDepth(Topology.Face rootFace, VisitFaceWithDepthStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFace), visitor);
		}

		#endregion

		#region Visit From Multiple Roots

		public static void VisitDepthFirstByDepth(IEnumerable<Topology.Face> rootFaces, VisitFaceEdgeDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFaces), visitor);
		}

		public static void VisitDepthFirstByDepth(IEnumerable<Topology.Face> rootFaces, VisitFaceDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFaces), visitor);
		}

		public static void VisitDepthFirstByDepth(IEnumerable<Topology.Face> rootFaces, VisitFaceEdgeWithStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFaces), visitor);
		}

		public static void VisitDepthFirstByDepth(IEnumerable<Topology.Face> rootFaces, VisitFaceWithStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFaces), visitor);
		}

		public static void VisitDepthFirstByDepth(IEnumerable<Topology.Face> rootFaces, VisitFaceEdgeWithDepthDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFaces), visitor);
		}

		public static void VisitDepthFirstByDepth(IEnumerable<Topology.Face> rootFaces, VisitFaceWithDepthDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFaces), visitor);
		}

		public static void VisitDepthFirstByDepth(IEnumerable<Topology.Face> rootFaces, VisitFaceEdgeWithDepthStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFaces), visitor);
		}

		public static void VisitDepthFirstByDepth(IEnumerable<Topology.Face> rootFaces, VisitFaceWithDepthStateDelegate visitor)
		{
			var queue = new DepthQueue((DepthQueueElement lhs, DepthQueueElement rhs) => { return lhs.depth >= rhs.depth; });
			VisitByDepth(queue, queue.PushRoots(rootFaces), visitor);
		}

		#endregion

		#endregion

		#endregion

		#region Distance Ordered Visitation

		public delegate void VisitFaceEdgeWithDistanceDelegate<T>(Topology.FaceEdge faceEdge, T distance);
		public delegate void VisitFaceWithDistanceDelegate<T>(Topology.Face face, T distance);

		public delegate void VisitFaceEdgeWithDistanceStateDelegate<T>(Topology.FaceEdge faceEdge, T distance, ref VisitationState state);
		public delegate void VisitFaceWithDistanceStateDelegate<T>(Topology.Face face, T distance, ref VisitationState state);

		public delegate void VisitFaceEdgeWithDepthDistanceDelegate<T>(Topology.FaceEdge faceEdge, int depth, T distance);
		public delegate void VisitFaceWithDepthDistanceDelegate<T>(Topology.Face face, int depth, T distance);

		public delegate void VisitFaceEdgeWithDepthDistanceStateDelegate<T>(Topology.FaceEdge faceEdge, int depth, T distance, ref VisitationState state);
		public delegate void VisitFaceWithDepthDistanceStateDelegate<T>(Topology.Face face, int depth, T distance, ref VisitationState state);

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

		private static void VisitByDistance<T>(DistanceQueue<T> queue, VisitFaceEdgeDelegate visitor)
		{
			var topology = queue.topology;
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex));
			});
		}

		private static void VisitByDistance<T>(DistanceQueue<T> queue, VisitFaceDelegate visitor)
		{
			var topology = queue.topology;
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex).farFace);
			});
		}

		private static void VisitByDistance<T>(DistanceQueue<T> queue, VisitFaceEdgeWithStateDelegate visitor)
		{
			var topology = queue.topology;
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex), ref state);
			});
		}

		private static void VisitByDistance<T>(DistanceQueue<T> queue, VisitFaceWithStateDelegate visitor)
		{
			var topology = queue.topology;
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex).farFace, ref state);
			});
		}

		private static void VisitByDistance<T>(DistanceQueue<T> queue, VisitFaceEdgeWithDistanceDelegate<T> visitor)
		{
			var topology = queue.topology;
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex), queue.front.distance);
			});
		}

		private static void VisitByDistance<T>(DistanceQueue<T> queue, VisitFaceWithDistanceDelegate<T> visitor)
		{
			var topology = queue.topology;
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex).farFace, queue.front.distance);
			});
		}

		private static void VisitByDistance<T>(DistanceQueue<T> queue, VisitFaceEdgeWithDistanceStateDelegate<T> visitor)
		{
			var topology = queue.topology;
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex), queue.front.distance, ref state);
			});
		}

		private static void VisitByDistance<T>(DistanceQueue<T> queue, VisitFaceWithDistanceStateDelegate<T> visitor)
		{
			var topology = queue.topology;
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex).farFace, queue.front.distance, ref state);
			});
		}

		private static void VisitByDistance<T>(DistanceQueue<T> queue, VisitFaceEdgeWithDepthDistanceDelegate<T> visitor)
		{
			var topology = queue.topology;
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex), queue.front.depth, queue.front.distance);
			});
		}

		private static void VisitByDistance<T>(DistanceQueue<T> queue, VisitFaceWithDepthDistanceDelegate<T> visitor)
		{
			var topology = queue.topology;
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex).farFace, queue.front.depth, queue.front.distance);
			});
		}

		private static void VisitByDistance<T>(DistanceQueue<T> queue, VisitFaceEdgeWithDepthDistanceStateDelegate<T> visitor)
		{
			var topology = queue.topology;
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex), queue.front.depth, queue.front.distance, ref state);
			});
		}

		private static void VisitByDistance<T>(DistanceQueue<T> queue, VisitFaceWithDepthDistanceStateDelegate<T> visitor)
		{
			var topology = queue.topology;
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex).farFace, queue.front.depth, queue.front.distance, ref state);
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

		private static DistanceQueue<int> CreateDistanceQueue(bool breadthFirst, Topology.Face rootFace, IEdgeAttribute<int> edgeDistances)
		{
			var queue = new IntDistanceQueue(breadthFirst);
			var topology = queue.PushRoots(rootFace);
			queue.edgeDistance = (int edgeIndex) => edgeDistances[new Topology.FaceEdge(topology, edgeIndex)];
			return queue;
		}

		private static DistanceQueue<int> CreateDistanceQueue(bool breadthFirst, IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<int> edgeDistances)
		{
			var queue = new IntDistanceQueue(breadthFirst);
			var topology = queue.PushRoots(rootFaces);
			queue.edgeDistance = (int edgeIndex) => edgeDistances[new Topology.FaceEdge(topology, edgeIndex)];
			return queue;
		}

		#region Breadth First

		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, VisitFaceEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, VisitFaceDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, VisitFaceEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, VisitFaceWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, VisitFaceEdgeWithDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, VisitFaceWithDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, VisitFaceEdgeWithDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, VisitFaceWithDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, VisitFaceEdgeWithDepthDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, VisitFaceWithDepthDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, VisitFaceEdgeWithDepthDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, VisitFaceWithDepthDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }

		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<int> edgeDistances, VisitFaceEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<int> edgeDistances, VisitFaceDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<int> edgeDistances, VisitFaceEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<int> edgeDistances, VisitFaceWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<int> edgeDistances, VisitFaceEdgeWithDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<int> edgeDistances, VisitFaceWithDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<int> edgeDistances, VisitFaceEdgeWithDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<int> edgeDistances, VisitFaceWithDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<int> edgeDistances, VisitFaceEdgeWithDepthDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<int> edgeDistances, VisitFaceWithDepthDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<int> edgeDistances, VisitFaceEdgeWithDepthDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<int> edgeDistances, VisitFaceWithDepthDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }

		#endregion

		#region Depth First

		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, VisitFaceEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, VisitFaceDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, VisitFaceEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, VisitFaceWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, VisitFaceEdgeWithDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, VisitFaceWithDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, VisitFaceEdgeWithDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, VisitFaceWithDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, VisitFaceEdgeWithDepthDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, VisitFaceWithDepthDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, VisitFaceEdgeWithDepthDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<int> edgeDistances, VisitFaceWithDepthDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }

		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<int> edgeDistances, VisitFaceEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<int> edgeDistances, VisitFaceDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<int> edgeDistances, VisitFaceEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<int> edgeDistances, VisitFaceWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<int> edgeDistances, VisitFaceEdgeWithDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<int> edgeDistances, VisitFaceWithDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<int> edgeDistances, VisitFaceEdgeWithDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<int> edgeDistances, VisitFaceWithDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<int> edgeDistances, VisitFaceEdgeWithDepthDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<int> edgeDistances, VisitFaceWithDepthDistanceDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<int> edgeDistances, VisitFaceEdgeWithDepthDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<int> edgeDistances, VisitFaceWithDepthDistanceStateDelegate<int> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }

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

		private static DistanceQueue<uint> CreateDistanceQueue(bool breadthFirst, Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances)
		{
			var queue = new UIntDistanceQueue(breadthFirst);
			var topology = queue.PushRoots(rootFace);
			queue.edgeDistance = (int edgeIndex) => edgeDistances[new Topology.FaceEdge(topology, edgeIndex)];
			return queue;
		}

		private static DistanceQueue<uint> CreateDistanceQueue(bool breadthFirst, IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<uint> edgeDistances)
		{
			var queue = new UIntDistanceQueue(breadthFirst);
			var topology = queue.PushRoots(rootFaces);
			queue.edgeDistance = (int edgeIndex) => edgeDistances[new Topology.FaceEdge(topology, edgeIndex)];
			return queue;
		}

		#region Breadth First

		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, VisitFaceEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, VisitFaceDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, VisitFaceEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, VisitFaceWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, VisitFaceEdgeWithDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, VisitFaceWithDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, VisitFaceEdgeWithDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, VisitFaceWithDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, VisitFaceEdgeWithDepthDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, VisitFaceWithDepthDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, VisitFaceEdgeWithDepthDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, VisitFaceWithDepthDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }

		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<uint> edgeDistances, VisitFaceEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<uint> edgeDistances, VisitFaceDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<uint> edgeDistances, VisitFaceEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<uint> edgeDistances, VisitFaceWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<uint> edgeDistances, VisitFaceEdgeWithDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<uint> edgeDistances, VisitFaceWithDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<uint> edgeDistances, VisitFaceEdgeWithDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<uint> edgeDistances, VisitFaceWithDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<uint> edgeDistances, VisitFaceEdgeWithDepthDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<uint> edgeDistances, VisitFaceWithDepthDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<uint> edgeDistances, VisitFaceEdgeWithDepthDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<uint> edgeDistances, VisitFaceWithDepthDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }

		#endregion

		#region Depth First

		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, VisitFaceEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, VisitFaceDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, VisitFaceEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, VisitFaceWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, VisitFaceEdgeWithDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, VisitFaceWithDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, VisitFaceEdgeWithDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, VisitFaceWithDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, VisitFaceEdgeWithDepthDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, VisitFaceWithDepthDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, VisitFaceEdgeWithDepthDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<uint> edgeDistances, VisitFaceWithDepthDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }

		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<uint> edgeDistances, VisitFaceEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<uint> edgeDistances, VisitFaceDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<uint> edgeDistances, VisitFaceEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<uint> edgeDistances, VisitFaceWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<uint> edgeDistances, VisitFaceEdgeWithDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<uint> edgeDistances, VisitFaceWithDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<uint> edgeDistances, VisitFaceEdgeWithDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<uint> edgeDistances, VisitFaceWithDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<uint> edgeDistances, VisitFaceEdgeWithDepthDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<uint> edgeDistances, VisitFaceWithDepthDistanceDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<uint> edgeDistances, VisitFaceEdgeWithDepthDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<uint> edgeDistances, VisitFaceWithDepthDistanceStateDelegate<uint> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }

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

		private static DistanceQueue<float> CreateDistanceQueue(bool breadthFirst, Topology.Face rootFace, IEdgeAttribute<float> edgeDistances)
		{
			var queue = new FloatDistanceQueue(breadthFirst);
			var topology = queue.PushRoots(rootFace);
			queue.edgeDistance = (int edgeIndex) => edgeDistances[new Topology.FaceEdge(topology, edgeIndex)];
			return queue;
		}

		private static DistanceQueue<float> CreateDistanceQueue(bool breadthFirst, IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<float> edgeDistances)
		{
			var queue = new FloatDistanceQueue(breadthFirst);
			var topology = queue.PushRoots(rootFaces);
			queue.edgeDistance = (int edgeIndex) => edgeDistances[new Topology.FaceEdge(topology, edgeIndex)];
			return queue;
		}

		#region Breadth First

		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, VisitFaceEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, VisitFaceDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, VisitFaceEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, VisitFaceWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, VisitFaceEdgeWithDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, VisitFaceWithDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, VisitFaceEdgeWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, VisitFaceWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, VisitFaceEdgeWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, VisitFaceWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, VisitFaceEdgeWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, VisitFaceWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }

		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<float> edgeDistances, VisitFaceEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<float> edgeDistances, VisitFaceDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<float> edgeDistances, VisitFaceEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<float> edgeDistances, VisitFaceWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<float> edgeDistances, VisitFaceEdgeWithDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<float> edgeDistances, VisitFaceWithDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<float> edgeDistances, VisitFaceEdgeWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<float> edgeDistances, VisitFaceWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<float> edgeDistances, VisitFaceEdgeWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<float> edgeDistances, VisitFaceWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<float> edgeDistances, VisitFaceEdgeWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<float> edgeDistances, VisitFaceWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }

		#endregion

		#region Depth First

		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, VisitFaceEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, VisitFaceDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, VisitFaceEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, VisitFaceWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, VisitFaceEdgeWithDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, VisitFaceWithDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, VisitFaceEdgeWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, VisitFaceWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, VisitFaceEdgeWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, VisitFaceWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, VisitFaceEdgeWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<float> edgeDistances, VisitFaceWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }

		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<float> edgeDistances, VisitFaceEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<float> edgeDistances, VisitFaceDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<float> edgeDistances, VisitFaceEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<float> edgeDistances, VisitFaceWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<float> edgeDistances, VisitFaceEdgeWithDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<float> edgeDistances, VisitFaceWithDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<float> edgeDistances, VisitFaceEdgeWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<float> edgeDistances, VisitFaceWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<float> edgeDistances, VisitFaceEdgeWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<float> edgeDistances, VisitFaceWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<float> edgeDistances, VisitFaceEdgeWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<float> edgeDistances, VisitFaceWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }

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

		private static DistanceQueue<double> CreateDistanceQueue(bool breadthFirst, Topology.Face rootFace, IEdgeAttribute<double> edgeDistances)
		{
			var queue = new DoubleDistanceQueue(breadthFirst);
			var topology = queue.PushRoots(rootFace);
			queue.edgeDistance = (int edgeIndex) => edgeDistances[new Topology.FaceEdge(topology, edgeIndex)];
			return queue;
		}

		private static DistanceQueue<double> CreateDistanceQueue(bool breadthFirst, IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<double> edgeDistances)
		{
			var queue = new DoubleDistanceQueue(breadthFirst);
			var topology = queue.PushRoots(rootFaces);
			queue.edgeDistance = (int edgeIndex) => edgeDistances[new Topology.FaceEdge(topology, edgeIndex)];
			return queue;
		}

		#region Breadth First

		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, VisitFaceEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, VisitFaceDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, VisitFaceEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, VisitFaceWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, VisitFaceEdgeWithDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, VisitFaceWithDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, VisitFaceEdgeWithDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, VisitFaceWithDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, VisitFaceEdgeWithDepthDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, VisitFaceWithDepthDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, VisitFaceEdgeWithDepthDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, VisitFaceWithDepthDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFace, edgeDistances), visitor); }

		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<double> edgeDistances, VisitFaceEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<double> edgeDistances, VisitFaceDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<double> edgeDistances, VisitFaceEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<double> edgeDistances, VisitFaceWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<double> edgeDistances, VisitFaceEdgeWithDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<double> edgeDistances, VisitFaceWithDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<double> edgeDistances, VisitFaceEdgeWithDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<double> edgeDistances, VisitFaceWithDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<double> edgeDistances, VisitFaceEdgeWithDepthDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<double> edgeDistances, VisitFaceWithDepthDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<double> edgeDistances, VisitFaceEdgeWithDepthDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }
		public static void VisitBreadthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<double> edgeDistances, VisitFaceWithDepthDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(true, rootFaces, edgeDistances), visitor); }

		#endregion

		#region Depth First

		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, VisitFaceEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, VisitFaceDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, VisitFaceEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, VisitFaceWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, VisitFaceEdgeWithDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, VisitFaceWithDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, VisitFaceEdgeWithDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, VisitFaceWithDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, VisitFaceEdgeWithDepthDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, VisitFaceWithDepthDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, VisitFaceEdgeWithDepthDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(Topology.Face rootFace, IEdgeAttribute<double> edgeDistances, VisitFaceWithDepthDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFace, edgeDistances), visitor); }

		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<double> edgeDistances, VisitFaceEdgeDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<double> edgeDistances, VisitFaceDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<double> edgeDistances, VisitFaceEdgeWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<double> edgeDistances, VisitFaceWithStateDelegate visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<double> edgeDistances, VisitFaceEdgeWithDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<double> edgeDistances, VisitFaceWithDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<double> edgeDistances, VisitFaceEdgeWithDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<double> edgeDistances, VisitFaceWithDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<double> edgeDistances, VisitFaceEdgeWithDepthDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<double> edgeDistances, VisitFaceWithDepthDistanceDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<double> edgeDistances, VisitFaceEdgeWithDepthDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }
		public static void VisitDepthFirstByDistance(IEnumerable<Topology.Face> rootFaces, IEdgeAttribute<double> edgeDistances, VisitFaceWithDepthDistanceStateDelegate<double> visitor) { VisitByDistance(CreateDistanceQueue(false, rootFaces, edgeDistances), visitor); }

		#endregion

		#endregion

		#region By Euclidean Distance

		private static DistanceQueue<float> CreateEuclideanDistanceQueue(bool breadthFirst, Topology.Face rootFace, IFaceAttribute<Vector3> facePositions)
		{
			var queue = new FloatDistanceQueue(breadthFirst);
			var topology = queue.PushRoots(rootFace);
			queue.edgeDistance = (int edgeIndex) =>
			{
				var edge = new Topology.FaceEdge(topology, edgeIndex);
				return Vector3.Distance(facePositions[edge.nearFace], facePositions[edge]);
			};
			return queue;
		}

		private static DistanceQueue<float> CreateEuclideanDistanceQueue(bool breadthFirst, IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions)
		{
			var queue = new FloatDistanceQueue(breadthFirst);
			var topology = queue.PushRoots(rootFaces);
			queue.edgeDistance = (int edgeIndex) =>
			{
				var edge = new Topology.FaceEdge(topology, edgeIndex);
				return Vector3.Distance(facePositions[edge.nearFace], facePositions[edge]);
			};
			return queue;
		}

		#region Breadth First

		public static void VisitBreadthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, VisitFaceEdgeDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootFace, facePositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, VisitFaceDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootFace, facePositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, VisitFaceEdgeWithStateDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootFace, facePositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, VisitFaceWithStateDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootFace, facePositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, VisitFaceEdgeWithDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootFace, facePositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, VisitFaceWithDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootFace, facePositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, VisitFaceEdgeWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootFace, facePositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, VisitFaceWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootFace, facePositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, VisitFaceEdgeWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootFace, facePositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, VisitFaceWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootFace, facePositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, VisitFaceEdgeWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootFace, facePositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, VisitFaceWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootFace, facePositions), visitor); }

		public static void VisitBreadthFirstByEuclideanDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, VisitFaceEdgeDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootFaces, facePositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, VisitFaceDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootFaces, facePositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, VisitFaceEdgeWithStateDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootFaces, facePositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, VisitFaceWithStateDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootFaces, facePositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, VisitFaceEdgeWithDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootFaces, facePositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, VisitFaceWithDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootFaces, facePositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, VisitFaceEdgeWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootFaces, facePositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, VisitFaceWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootFaces, facePositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, VisitFaceEdgeWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootFaces, facePositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, VisitFaceWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootFaces, facePositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, VisitFaceEdgeWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootFaces, facePositions), visitor); }
		public static void VisitBreadthFirstByEuclideanDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, VisitFaceWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(true, rootFaces, facePositions), visitor); }

		#endregion

		#region Depth First

		public static void VisitDepthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, VisitFaceEdgeDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootFace, facePositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, VisitFaceDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootFace, facePositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, VisitFaceEdgeWithStateDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootFace, facePositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, VisitFaceWithStateDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootFace, facePositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, VisitFaceEdgeWithDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootFace, facePositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, VisitFaceWithDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootFace, facePositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, VisitFaceEdgeWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootFace, facePositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, VisitFaceWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootFace, facePositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, VisitFaceEdgeWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootFace, facePositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, VisitFaceWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootFace, facePositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, VisitFaceEdgeWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootFace, facePositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, VisitFaceWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootFace, facePositions), visitor); }

		public static void VisitDepthFirstByEuclideanDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, VisitFaceEdgeDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootFaces, facePositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, VisitFaceDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootFaces, facePositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, VisitFaceEdgeWithStateDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootFaces, facePositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, VisitFaceWithStateDelegate visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootFaces, facePositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, VisitFaceEdgeWithDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootFaces, facePositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, VisitFaceWithDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootFaces, facePositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, VisitFaceEdgeWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootFaces, facePositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, VisitFaceWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootFaces, facePositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, VisitFaceEdgeWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootFaces, facePositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, VisitFaceWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootFaces, facePositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, VisitFaceEdgeWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootFaces, facePositions), visitor); }
		public static void VisitDepthFirstByEuclideanDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, VisitFaceWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateEuclideanDistanceQueue(false, rootFaces, facePositions), visitor); }

		#endregion

		#endregion

		#region By Spherical Distance

		private static DistanceQueue<float> CreateSphericalDistanceQueue(bool breadthFirst, Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius)
		{
			var queue = new FloatDistanceQueue(breadthFirst);
			var topology = queue.PushRoots(rootFace);
			queue.edgeDistance = (int edgeIndex) =>
			{
				var edge = new Topology.FaceEdge(topology, edgeIndex);
				return GeometryUtility.AngleBetweenVectors(facePositions[edge.nearFace], facePositions[edge]) * sphereRadius;
			};
			return queue;
		}

		private static DistanceQueue<float> CreateSphericalDistanceQueue(bool breadthFirst, IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, float sphereRadius)
		{
			var queue = new FloatDistanceQueue(breadthFirst);
			var topology = queue.PushRoots(rootFaces);
			queue.edgeDistance = (int edgeIndex) =>
			{
				var edge = new Topology.FaceEdge(topology, edgeIndex);
				return GeometryUtility.AngleBetweenVectors(facePositions[edge.nearFace], facePositions[edge]) * sphereRadius;
			};
			return queue;
		}

		#region Breadth First

		public static void VisitBreadthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceEdgeDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootFace, facePositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootFace, facePositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceEdgeWithStateDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootFace, facePositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceWithStateDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootFace, facePositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceEdgeWithDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootFace, facePositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceWithDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootFace, facePositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceEdgeWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootFace, facePositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootFace, facePositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceEdgeWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootFace, facePositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootFace, facePositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceEdgeWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootFace, facePositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootFace, facePositions, sphereRadius), visitor); }

		public static void VisitBreadthFirstBySphericalDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceEdgeDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootFaces, facePositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootFaces, facePositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceEdgeWithStateDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootFaces, facePositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceWithStateDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootFaces, facePositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceEdgeWithDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootFaces, facePositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceWithDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootFaces, facePositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceEdgeWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootFaces, facePositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootFaces, facePositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceEdgeWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootFaces, facePositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootFaces, facePositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceEdgeWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootFaces, facePositions, sphereRadius), visitor); }
		public static void VisitBreadthFirstBySphericalDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(true, rootFaces, facePositions, sphereRadius), visitor); }

		#endregion

		#region Depth First

		public static void VisitDepthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceEdgeDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootFace, facePositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootFace, facePositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceEdgeWithStateDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootFace, facePositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceWithStateDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootFace, facePositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceEdgeWithDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootFace, facePositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceWithDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootFace, facePositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceEdgeWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootFace, facePositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootFace, facePositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceEdgeWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootFace, facePositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootFace, facePositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceEdgeWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootFace, facePositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(Topology.Face rootFace, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootFace, facePositions, sphereRadius), visitor); }

		public static void VisitDepthFirstBySphericalDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceEdgeDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootFaces, facePositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootFaces, facePositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceEdgeWithStateDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootFaces, facePositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceWithStateDelegate visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootFaces, facePositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceEdgeWithDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootFaces, facePositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceWithDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootFaces, facePositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceEdgeWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootFaces, facePositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceWithDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootFaces, facePositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceEdgeWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootFaces, facePositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceWithDepthDistanceDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootFaces, facePositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceEdgeWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootFaces, facePositions, sphereRadius), visitor); }
		public static void VisitDepthFirstBySphericalDistance(IEnumerable<Topology.Face> rootFaces, IFaceAttribute<Vector3> facePositions, float sphereRadius, VisitFaceWithDepthDistanceStateDelegate<float> visitor) { VisitByDistance(CreateSphericalDistanceQueue(false, rootFaces, facePositions, sphereRadius), visitor); }

		#endregion

		#endregion

		#endregion

		#region Random Order Adjacency Visitation

		#region Queue

		private class RandomAdjacencyQueue : Queue
		{
			private List<int> _queue;
			private IRandomEngine _randomEngine;
			public int frontEdgeIndex;

			public RandomAdjacencyQueue(IRandomEngine randomEngine)
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

		private static void VisitAdjacentInRandomOrder(RandomAdjacencyQueue queue, Topology topology, VisitFaceEdgeDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex));
			});
		}

		private static void VisitAdjacentInRandomOrder(RandomAdjacencyQueue queue, Topology topology, VisitFaceDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex).farFace);
			});
		}

		private static void VisitAdjacentInRandomOrder(RandomAdjacencyQueue queue, Topology topology, VisitFaceEdgeWithStateDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex), ref state);
			});
		}

		private static void VisitAdjacentInRandomOrder(RandomAdjacencyQueue queue, Topology topology, VisitFaceWithStateDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex).farFace, ref state);
			});
		}

		#endregion

		#region Visit From Single Root

		public static void VisitAdjacentInRandomOrder(Topology.Face rootFace, IRandomEngine randomEngine, VisitFaceEdgeDelegate visitor)
		{
			var queue = new RandomAdjacencyQueue(randomEngine);
			VisitAdjacentInRandomOrder(queue, queue.PushRoots(rootFace), visitor);
		}

		public static void VisitAdjacentInRandomOrder(Topology.Face rootFace, IRandomEngine randomEngine, VisitFaceDelegate visitor)
		{
			var queue = new RandomAdjacencyQueue(randomEngine);
			VisitAdjacentInRandomOrder(queue, queue.PushRoots(rootFace), visitor);
		}

		public static void VisitAdjacentInRandomOrder(Topology.Face rootFace, IRandomEngine randomEngine, VisitFaceEdgeWithStateDelegate visitor)
		{
			var queue = new RandomAdjacencyQueue(randomEngine);
			VisitAdjacentInRandomOrder(queue, queue.PushRoots(rootFace), visitor);
		}

		public static void VisitAdjacentInRandomOrder(Topology.Face rootFace, IRandomEngine randomEngine, VisitFaceWithStateDelegate visitor)
		{
			var queue = new RandomAdjacencyQueue(randomEngine);
			VisitAdjacentInRandomOrder(queue, queue.PushRoots(rootFace), visitor);
		}

		#endregion

		#region Visit From Multiple Roots

		public static void VisitAdjacentInRandomOrder(IEnumerable<Topology.Face> rootFaces, IRandomEngine randomEngine, VisitFaceEdgeDelegate visitor)
		{
			var queue = new RandomAdjacencyQueue(randomEngine);
			VisitAdjacentInRandomOrder(queue, queue.PushRoots(rootFaces), visitor);
		}

		public static void VisitAdjacentInRandomOrder(IEnumerable<Topology.Face> rootFaces, IRandomEngine randomEngine, VisitFaceDelegate visitor)
		{
			var queue = new RandomAdjacencyQueue(randomEngine);
			VisitAdjacentInRandomOrder(queue, queue.PushRoots(rootFaces), visitor);
		}

		public static void VisitAdjacentInRandomOrder(IEnumerable<Topology.Face> rootFaces, IRandomEngine randomEngine, VisitFaceEdgeWithStateDelegate visitor)
		{
			var queue = new RandomAdjacencyQueue(randomEngine);
			VisitAdjacentInRandomOrder(queue, queue.PushRoots(rootFaces), visitor);
		}

		public static void VisitAdjacentInRandomOrder(IEnumerable<Topology.Face> rootFaces, IRandomEngine randomEngine, VisitFaceWithStateDelegate visitor)
		{
			var queue = new RandomAdjacencyQueue(randomEngine);
			VisitAdjacentInRandomOrder(queue, queue.PushRoots(rootFaces), visitor);
		}

		#endregion

		#endregion
	}
}
