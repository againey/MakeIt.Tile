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
	#region Public Visit Types

	public abstract class TopologyVisitor
	{
		public interface IQueue<T>
		{
			bool isEmpty { get; }
			void Push(T item);
			T Pop();
		}

		protected Topology _topology;

		protected int _depth;

		protected bool _ignore;
		protected bool _break;

		public int depth { get { return _depth; } }

		public void Ignore()
		{
			_ignore = true;
		}

		public void Break()
		{
			_break = true;
		}

		protected TopologyVisitor(Topology topology)
		{
			_topology = topology;
		}
	}

	public sealed class FaceVisitor : TopologyVisitor
	{
		public struct QueueItem : IEquatable<QueueItem>
		{
			public int faceIndex;
			public int depth;

			public QueueItem(int faceIndex, int depth)
			{
				this.faceIndex = faceIndex;
				this.depth = depth;
			}

			public bool Equals(QueueItem other)
			{
				return faceIndex == other.faceIndex;
			}
		}

		public delegate void VisitDelegate(FaceVisitor visitor);

		private IQueue<QueueItem> _queue;
		private VisitDelegate _visitDelegate;
		private BitArray _visitedFaces;

		private Topology.Face _face;

		public Topology.Face face { get { return _face; } }

		public void VisitNeighbor(Topology.Face face)
		{
			_queue.Push(new QueueItem(face.index, _depth + 1));
		}

		public void RevisitNeighbor(Topology.Face face)
		{
			_visitedFaces[face.index] = false;
			_queue.Push(new QueueItem(face.index, _depth + 1));
		}

		public static bool AreOrderedBreadthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth <= rhs.depth;
		}

		public static bool AreOrderedDepthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth >= rhs.depth;
		}

		private FaceVisitor(Topology topology, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
			: base(topology)
		{
			_queue = queue;
			_visitDelegate = visitDelegate;
			_visitedFaces = new BitArray(topology.faces.Count);
		}

		public static void VisitAll(Topology.Face rootFace, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootFace, queue), queue, visitDelegate);
		}

		public static void VisitAll(IEnumerable<Topology.Face> rootFaces, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootFaces, queue), queue, visitDelegate);
		}

		private static Topology InitializeQueue(Topology.Face rootFace, IQueue<QueueItem> queue)
		{
			queue.Push(new QueueItem(rootFace.index, 0));
			return rootFace.topology;
		}

		private static Topology InitializeQueue(IEnumerable<Topology.Face> rootFaces, IQueue<QueueItem> queue)
		{
			var rootFacesEnumerator = rootFaces.GetEnumerator();
			if (!rootFacesEnumerator.MoveNext()) return null;

			var topology = rootFacesEnumerator.Current.topology;

			do
			{
				queue.Push(new QueueItem(rootFacesEnumerator.Current.index, 0));
			} while (rootFacesEnumerator.MoveNext());

			return topology;
		}

		private static void VisitAll(Topology topology, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			if (topology == null) return;
			var visitor = new FaceVisitor(topology, queue, visitDelegate);
			visitor.VisitAll();
		}

		private void VisitAll()
		{
			while (_queue.isEmpty == false)
			{
				var queueItem = _queue.Pop();

				// Move on to the next queue item if we've already visited the target face.
				if (_visitedFaces[queueItem.faceIndex] == true) continue;

				// Visit the face and get the list of adjacent neighbors.
				_face = new Topology.Face(_topology, queueItem.faceIndex);
				_depth = queueItem.depth;
				_break = _ignore = false;
				_visitDelegate(this);

				// Check the visitation action and respond accordingly.
				if (_break) return;
				if (!_ignore) _visitedFaces[queueItem.faceIndex] = true;
			}
		}
	}

	public class FaceVisitor<TDistance> : TopologyVisitor
	{
		public struct QueueItem : IEquatable<QueueItem>
		{
			public int faceIndex;
			public int depth;
			public TDistance distance;

			public QueueItem(int faceIndex, int depth, TDistance distance)
			{
				this.faceIndex = faceIndex;
				this.depth = depth;
				this.distance = distance;
			}

			public bool Equals(QueueItem other)
			{
				return faceIndex == other.faceIndex;
			}
		}

		public delegate void VisitDelegate(FaceVisitor<TDistance> visitor);

		private IQueue<QueueItem> _queue;
		private VisitDelegate _visitDelegate;
		private BitArray _visitedFaces;

		private Topology.Face _face;
		private TDistance _distance;

		public Topology.Face face { get { return _face; } }
		public TDistance distance { get { return _distance; } }

		public void VisitNeighbor(Topology.Face face, TDistance distance)
		{
			_queue.Push(new QueueItem(face.index, _depth + 1, distance));
		}

		public void RevisitNeighbor(Topology.Face face, TDistance distance)
		{
			_visitedFaces[face.index] = false;
			_queue.Push(new QueueItem(face.index, _depth + 1, distance));
		}

		public static bool AreOrderedBreadthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth <= rhs.depth;
		}

		public static bool AreOrderedDepthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth >= rhs.depth;
		}

		private FaceVisitor(Topology topology, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
			: base(topology)
		{
			_queue = queue;
			_visitDelegate = visitDelegate;
			_visitedFaces = new BitArray(topology.faces.Count);
		}

		public static void VisitAll(Topology.Face rootFace, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootFace, queue), queue, visitDelegate);
		}

		public static void VisitAll(IEnumerable<Topology.Face> rootFaces, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootFaces, queue), queue, visitDelegate);
		}

		private static Topology InitializeQueue(Topology.Face rootFace, IQueue<QueueItem> queue)
		{
			queue.Push(new QueueItem(rootFace.index, 0, default(TDistance)));
			return rootFace.topology;
		}

		private static Topology InitializeQueue(IEnumerable<Topology.Face> rootFaces, IQueue<QueueItem> queue)
		{
			var rootFacesEnumerator = rootFaces.GetEnumerator();
			if (!rootFacesEnumerator.MoveNext()) return null;

			var topology = rootFacesEnumerator.Current.topology;

			do
			{
				queue.Push(new QueueItem(rootFacesEnumerator.Current.index, 0, default(TDistance)));
			} while (rootFacesEnumerator.MoveNext());

			return topology;
		}

		private static void VisitAll(Topology topology, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			if (topology == null) return;
			var visitor = new FaceVisitor<TDistance>(topology, queue, visitDelegate);
			visitor.VisitAll();
		}

		private void VisitAll()
		{
			while (_queue.isEmpty == false)
			{
				var queueItem = _queue.Pop();

				// Move on to the next queue item if we've already visited the target face.
				if (_visitedFaces[queueItem.faceIndex] == true) continue;

				// Visit the face and get the list of adjacent neighbors.
				_face = new Topology.Face(_topology, queueItem.faceIndex);
				_depth = queueItem.depth;
				_distance = queueItem.distance;
				_break = _ignore = false;
				_visitDelegate(this);

				// Check the visitation action and respond accordingly.
				if (_break) return;
				if (!_ignore) _visitedFaces[queueItem.faceIndex] = true;
			}
		}
	}

	public sealed class FaceEdgeVisitor : TopologyVisitor
	{
		public struct QueueItem : IEquatable<QueueItem>
		{
			public int edgeIndex;
			public int depth;

			public QueueItem(int edgeIndex, int depth)
			{
				this.edgeIndex = edgeIndex;
				this.depth = depth;
			}

			public bool Equals(QueueItem other)
			{
				return edgeIndex == other.edgeIndex;
			}
		}

		public delegate void VisitDelegate(FaceEdgeVisitor visitor);

		private IQueue<QueueItem> _queue;
		private VisitDelegate _visitDelegate;
		private BitArray _visitedFaces;

		private Topology.FaceEdge _edge;

		public Topology.FaceEdge edge { get { return _edge; } }

		public void VisitNeighbor(Topology.FaceEdge edge)
		{
			_queue.Push(new QueueItem(edge.index, _depth + 1));
		}

		public void RevisitNeighbor(Topology.FaceEdge edge)
		{
			_visitedFaces[edge.farFace.index] = false;
			_queue.Push(new QueueItem(edge.index, _depth + 1));
		}

		public static bool AreOrderedBreadthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth <= rhs.depth;
		}

		public static bool AreOrderedDepthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth >= rhs.depth;
		}

		private FaceEdgeVisitor(Topology topology, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
			: base(topology)
		{
			_queue = queue;
			_visitDelegate = visitDelegate;
			_visitedFaces = new BitArray(topology.faces.Count);
		}

		public static void VisitAll(Topology.FaceEdge rootEdge, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootEdge, queue), queue, visitDelegate);
		}

		public static void VisitAll(IEnumerable<Topology.FaceEdge> rootEdges, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootEdges, queue), queue, visitDelegate);
		}

		private static Topology InitializeQueue(Topology.FaceEdge rootEdge, IQueue<QueueItem> queue)
		{
			queue.Push(new QueueItem(rootEdge.index, 0));
			return rootEdge.topology;
		}

		private static Topology InitializeQueue(IEnumerable<Topology.FaceEdge> rootEdges, IQueue<QueueItem> queue)
		{
			var rootEdgesEnumerator = rootEdges.GetEnumerator();
			if (!rootEdgesEnumerator.MoveNext()) return null;

			var topology = rootEdgesEnumerator.Current.topology;

			do
			{
				queue.Push(new QueueItem(rootEdgesEnumerator.Current.index, 0));
			} while (rootEdgesEnumerator.MoveNext());

			return topology;
		}

		private static void VisitAll(Topology topology, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			if (topology == null) return;
			var visitor = new FaceEdgeVisitor(topology, queue, visitDelegate);
			visitor.VisitAll();
		}

		private void VisitAll()
		{
			while (_queue.isEmpty == false)
			{
				var queueItem = _queue.Pop();
				var faceIndex = _topology.edgeData[queueItem.edgeIndex].face;

				// Move on to the next queue item if we've already visited the target face.
				if (_visitedFaces[faceIndex] == true) continue;

				// Visit the face and get the list of adjacent neighbors.
				_edge = new Topology.FaceEdge(_topology, queueItem.edgeIndex);
				_depth = queueItem.depth;
				_break = _ignore = false;
				_visitDelegate(this);

				// Check the visitation action and respond accordingly.
				if (_break) return;
				if (!_ignore) _visitedFaces[faceIndex] = true;
			}
		}
	}

	public class FaceEdgeVisitor<TDistance> : TopologyVisitor
	{
		public struct QueueItem : IEquatable<QueueItem>
		{
			public int edgeIndex;
			public int depth;
			public TDistance distance;

			public QueueItem(int edgeIndex, int depth, TDistance distance)
			{
				this.edgeIndex = edgeIndex;
				this.depth = depth;
				this.distance = distance;
			}

			public bool Equals(QueueItem other)
			{
				return edgeIndex == other.edgeIndex;
			}
		}

		public delegate void VisitDelegate(FaceEdgeVisitor<TDistance> visitor);

		private IQueue<QueueItem> _queue;
		private VisitDelegate _visitDelegate;
		private BitArray _visitedFaces;

		private Topology.FaceEdge _edge;
		private TDistance _distance;

		public Topology.FaceEdge edge { get { return _edge; } }
		public TDistance distance { get { return _distance; } }

		public void VisitNeighbor(Topology.FaceEdge edge, TDistance distance)
		{
			_queue.Push(new QueueItem(edge.index, _depth + 1, distance));
		}

		public void RevisitNeighbor(Topology.FaceEdge edge, TDistance distance)
		{
			_visitedFaces[edge.farFace.index] = false;
			_queue.Push(new QueueItem(edge.index, _depth + 1, distance));
		}

		public static bool AreOrderedBreadthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth <= rhs.depth;
		}

		public static bool AreOrderedDepthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth >= rhs.depth;
		}

		private FaceEdgeVisitor(Topology topology, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
			: base(topology)
		{
			_queue = queue;
			_visitDelegate = visitDelegate;
			_visitedFaces = new BitArray(topology.faces.Count);
		}

		public static void VisitAll(Topology.FaceEdge rootEdge, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootEdge, queue), queue, visitDelegate);
		}

		public static void VisitAll(IEnumerable<Topology.FaceEdge> rootEdges, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootEdges, queue), queue, visitDelegate);
		}

		private static Topology InitializeQueue(Topology.FaceEdge rootEdge, IQueue<QueueItem> queue)
		{
			queue.Push(new QueueItem(rootEdge.index, 0, default(TDistance)));
			return rootEdge.topology;
		}

		private static Topology InitializeQueue(IEnumerable<Topology.FaceEdge> rootEdge, IQueue<QueueItem> queue)
		{
			var rootEdgesEnumerator = rootEdge.GetEnumerator();
			if (!rootEdgesEnumerator.MoveNext()) return null;

			var topology = rootEdgesEnumerator.Current.topology;

			do
			{
				queue.Push(new QueueItem(rootEdgesEnumerator.Current.index, 0, default(TDistance)));
			} while (rootEdgesEnumerator.MoveNext());

			return topology;
		}

		private static void VisitAll(Topology topology, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			if (topology == null) return;
			var visitor = new FaceEdgeVisitor<TDistance>(topology, queue, visitDelegate);
			visitor.VisitAll();
		}

		private void VisitAll()
		{
			while (_queue.isEmpty == false)
			{
				var queueItem = _queue.Pop();
				var faceIndex = _topology.edgeData[queueItem.edgeIndex].face;

				// Move on to the next queue item if we've already visited the target face.
				if (_visitedFaces[faceIndex] == true) continue;

				// Visit the face and get the list of adjacent neighbors.
				_edge = new Topology.FaceEdge(_topology, queueItem.edgeIndex);
				_depth = queueItem.depth;
				_distance = queueItem.distance;
				_break = _ignore = false;
				_visitDelegate(this);

				// Check the visitation action and respond accordingly.
				if (_break) return;
				if (!_ignore) _visitedFaces[faceIndex] = true;
			}
		}
	}

	#endregion

	public static class FaceVisitationUtility
	{
		#region Queues

		public class ArbitraryOrderQueue<T> : TopologyVisitor.IQueue<T>
		{
			private List<T> _items;

			public ArbitraryOrderQueue()
			{
				_items = new List<T>();
			}

			public bool isEmpty
			{
				get
				{
					return _items.Count == 0;
				}
			}

			public T Pop()
			{
				var index = _items.Count - 1;
				var item = _items[index];
				_items.RemoveAt(index);
				return item;
			}

			public void Push(T item)
			{
				_items.Add(item);
			}
		}

		public class SortedOrderQueue<T> : TopologyVisitor.IQueue<T> where T : IEquatable<T>
		{
			private PriorityQueue<T> _items;

			public SortedOrderQueue(PriorityQueue<T>.AreOrderedDelegate areOrderedDelegate)
			{
				_items = new PriorityQueue<T>(areOrderedDelegate, 0);
			}

			public bool isEmpty
			{
				get
				{
					return _items.Count == 0;
				}
			}

			public T Pop()
			{
				var item = _items.front;
				_items.Pop();
				return item;
			}

			public void Push(T item)
			{
				_items.Push(item);
			}
		}

		public class RandomOrderQueue<T> : TopologyVisitor.IQueue<T>
		{
			private List<T> _items;
			private IRandomEngine _randomEngine;

			public RandomOrderQueue(IRandomEngine randomEngine)
			{
				_items = new List<T>();
				_randomEngine = randomEngine;
			}

			public bool isEmpty
			{
				get
				{
					return _items.Count == 0;
				}
			}

			public T Pop()
			{
				var index = RandomUtility.HalfOpenRange(_items.Count, _randomEngine);
				var backIndex = _items.Count - 1;
				var item = _items[index];
				_items[index] = _items[backIndex];
				_items.RemoveAt(backIndex);
				return item;
			}

			public void Push(T item)
			{
				_items.Add(item);
			}
		}

		#endregion

		#region Arbitrary Order Visitation

		public static void VisitFaces(Topology.Face rootFace, FaceVisitor.VisitDelegate visitDelegate)
		{
			FaceVisitor.VisitAll(rootFace, new ArbitraryOrderQueue<FaceVisitor.QueueItem>(), visitDelegate);
		}

		public static void VisitFaces(IEnumerable<Topology.Face> rootFaces, FaceVisitor.VisitDelegate visitDelegate)
		{
			FaceVisitor.VisitAll(rootFaces, new ArbitraryOrderQueue<FaceVisitor.QueueItem>(), visitDelegate);
		}

		public static void VisitFaces<TDistance>(Topology.Face rootFace, FaceVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TDistance>.VisitAll(rootFace, new ArbitraryOrderQueue<FaceVisitor<TDistance>.QueueItem>(), visitDelegate);
		}

		public static void VisitFaces<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TDistance>.VisitAll(rootFaces, new ArbitraryOrderQueue<FaceVisitor<TDistance>.QueueItem>(), visitDelegate);
		}

		public static void VisitFaces(Topology.FaceEdge rootEdge, FaceEdgeVisitor.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor.VisitAll(rootEdge, new ArbitraryOrderQueue<FaceEdgeVisitor.QueueItem>(), visitDelegate);
		}

		public static void VisitFaces(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor.VisitAll(rootEdges, new ArbitraryOrderQueue<FaceEdgeVisitor.QueueItem>(), visitDelegate);
		}

		public static void VisitFaces<TDistance>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdge, new ArbitraryOrderQueue<FaceEdgeVisitor<TDistance>.QueueItem>(), visitDelegate);
		}

		public static void VisitFaces<TDistance>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdges, new ArbitraryOrderQueue<FaceEdgeVisitor<TDistance>.QueueItem>(), visitDelegate);
		}

		#endregion

		#region Breadth First Visitation

		public static void VisitFacesInBreadthFirstOrder(Topology.Face rootFace, FaceVisitor.VisitDelegate visitDelegate)
		{
			FaceVisitor.VisitAll(rootFace, new SortedOrderQueue<FaceVisitor.QueueItem>(FaceVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitFacesInBreadthFirstOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor.VisitDelegate visitDelegate)
		{
			FaceVisitor.VisitAll(rootFaces, new SortedOrderQueue<FaceVisitor.QueueItem>(FaceVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitFacesInBreadthFirstOrder<TDistance>(Topology.Face rootFace, FaceVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TDistance>.VisitAll(rootFace, new SortedOrderQueue<FaceVisitor<TDistance>.QueueItem>(FaceVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitFacesInBreadthFirstOrder<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TDistance>.VisitAll(rootFaces, new SortedOrderQueue<FaceVisitor<TDistance>.QueueItem>(FaceVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitFacesInBreadthFirstOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor.VisitAll(rootEdge, new SortedOrderQueue<FaceEdgeVisitor.QueueItem>(FaceEdgeVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitFacesInBreadthFirstOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor.VisitAll(rootEdges, new SortedOrderQueue<FaceEdgeVisitor.QueueItem>(FaceEdgeVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitFacesInBreadthFirstOrder<TDistance>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdge, new SortedOrderQueue<FaceEdgeVisitor<TDistance>.QueueItem>(FaceEdgeVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitFacesInBreadthFirstOrder<TDistance>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdges, new SortedOrderQueue<FaceEdgeVisitor<TDistance>.QueueItem>(FaceEdgeVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate);
		}

		#endregion

		#region Depth First Visitation

		public static void VisitFacesInDepthFirstOrder(Topology.Face rootFace, FaceVisitor.VisitDelegate visitDelegate)
		{
			FaceVisitor.VisitAll(rootFace, new SortedOrderQueue<FaceVisitor.QueueItem>(FaceVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitFacesInDepthFirstOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor.VisitDelegate visitDelegate)
		{
			FaceVisitor.VisitAll(rootFaces, new SortedOrderQueue<FaceVisitor.QueueItem>(FaceVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitFacesInDepthFirstOrder<TDistance>(Topology.Face rootFace, FaceVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TDistance>.VisitAll(rootFace, new SortedOrderQueue<FaceVisitor<TDistance>.QueueItem>(FaceVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitFacesInDepthFirstOrder<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TDistance>.VisitAll(rootFaces, new SortedOrderQueue<FaceVisitor<TDistance>.QueueItem>(FaceVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitFacesInDepthFirstOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor.VisitAll(rootEdge, new SortedOrderQueue<FaceEdgeVisitor.QueueItem>(FaceEdgeVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitFacesInDepthFirstOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor.VisitAll(rootEdges, new SortedOrderQueue<FaceEdgeVisitor.QueueItem>(FaceEdgeVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitFacesInDepthFirstOrder<TDistance>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdge, new SortedOrderQueue<FaceEdgeVisitor<TDistance>.QueueItem>(FaceEdgeVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitFacesInDepthFirstOrder<TDistance>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdges, new SortedOrderQueue<FaceEdgeVisitor<TDistance>.QueueItem>(FaceEdgeVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		#endregion

		#region Nearest Distance Visitation

		#region Int32

		private static bool AreOrderedByNearestDistance(FaceVisitor<int>.QueueItem lhs, FaceVisitor<int>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		public static void VisitFacesInNearestDistanceOrder(Topology.Face rootFace, FaceVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceVisitor<int>.VisitAll(rootFace, new SortedOrderQueue<FaceVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceVisitor<int>.VisitAll(rootFaces, new SortedOrderQueue<FaceVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		private static bool AreOrderedByNearestDistance(FaceEdgeVisitor<int>.QueueItem lhs, FaceEdgeVisitor<int>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		public static void VisitFacesInNearestDistanceOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<int>.VisitAll(rootEdge, new SortedOrderQueue<FaceEdgeVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<int>.VisitAll(rootEdges, new SortedOrderQueue<FaceEdgeVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		#endregion

		#region UInt32

		private static bool AreOrderedByNearestDistance(FaceVisitor<uint>.QueueItem lhs, FaceVisitor<uint>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		public static void VisitFacesInNearestDistanceOrder(Topology.Face rootFace, FaceVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceVisitor<uint>.VisitAll(rootFace, new SortedOrderQueue<FaceVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceVisitor<uint>.VisitAll(rootFaces, new SortedOrderQueue<FaceVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		private static bool AreOrderedByNearestDistance(FaceEdgeVisitor<uint>.QueueItem lhs, FaceEdgeVisitor<uint>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		public static void VisitFacesInNearestDistanceOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<uint>.VisitAll(rootEdge, new SortedOrderQueue<FaceEdgeVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<uint>.VisitAll(rootEdges, new SortedOrderQueue<FaceEdgeVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		#endregion

		#region Single

		private static bool AreOrderedByNearestDistance(FaceVisitor<float>.QueueItem lhs, FaceVisitor<float>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		public static void VisitFacesInNearestDistanceOrder(Topology.Face rootFace, FaceVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceVisitor<float>.VisitAll(rootFace, new SortedOrderQueue<FaceVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceVisitor<float>.VisitAll(rootFaces, new SortedOrderQueue<FaceVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		private static bool AreOrderedByNearestDistance(FaceEdgeVisitor<float>.QueueItem lhs, FaceEdgeVisitor<float>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		public static void VisitFacesInNearestDistanceOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<float>.VisitAll(rootEdge, new SortedOrderQueue<FaceEdgeVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<float>.VisitAll(rootEdges, new SortedOrderQueue<FaceEdgeVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		#endregion

		#region Double

		private static bool AreOrderedByNearestDistance(FaceVisitor<double>.QueueItem lhs, FaceVisitor<double>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		public static void VisitFacesInNearestDistanceOrder(Topology.Face rootFace, FaceVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceVisitor<double>.VisitAll(rootFace, new SortedOrderQueue<FaceVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceVisitor<double>.VisitAll(rootFaces, new SortedOrderQueue<FaceVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		private static bool AreOrderedByNearestDistance(FaceEdgeVisitor<double>.QueueItem lhs, FaceEdgeVisitor<double>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		public static void VisitFacesInNearestDistanceOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<double>.VisitAll(rootEdge, new SortedOrderQueue<FaceEdgeVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<double>.VisitAll(rootEdges, new SortedOrderQueue<FaceEdgeVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		#endregion

		#endregion

		#region Farthest Distance Visitation

		#region Int32

		private static bool AreOrderedByFarthestDistance(FaceVisitor<int>.QueueItem lhs, FaceVisitor<int>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		public static void VisitFacesInFarthestDistanceOrder(Topology.Face rootFace, FaceVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceVisitor<int>.VisitAll(rootFace, new SortedOrderQueue<FaceVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceVisitor<int>.VisitAll(rootFaces, new SortedOrderQueue<FaceVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		private static bool AreOrderedByFarthestDistance(FaceEdgeVisitor<int>.QueueItem lhs, FaceEdgeVisitor<int>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		public static void VisitFacesInFarthestDistanceOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<int>.VisitAll(rootEdge, new SortedOrderQueue<FaceEdgeVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<int>.VisitAll(rootEdges, new SortedOrderQueue<FaceEdgeVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		#endregion

		#region UInt32

		private static bool AreOrderedByFarthestDistance(FaceVisitor<uint>.QueueItem lhs, FaceVisitor<uint>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		public static void VisitFacesInFarthestDistanceOrder(Topology.Face rootFace, FaceVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceVisitor<uint>.VisitAll(rootFace, new SortedOrderQueue<FaceVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceVisitor<uint>.VisitAll(rootFaces, new SortedOrderQueue<FaceVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		private static bool AreOrderedByFarthestDistance(FaceEdgeVisitor<uint>.QueueItem lhs, FaceEdgeVisitor<uint>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		public static void VisitFacesInFarthestDistanceOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<uint>.VisitAll(rootEdge, new SortedOrderQueue<FaceEdgeVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<uint>.VisitAll(rootEdges, new SortedOrderQueue<FaceEdgeVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		#endregion

		#region Single

		private static bool AreOrderedByFarthestDistance(FaceVisitor<float>.QueueItem lhs, FaceVisitor<float>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		public static void VisitFacesInFarthestDistanceOrder(Topology.Face rootFace, FaceVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceVisitor<float>.VisitAll(rootFace, new SortedOrderQueue<FaceVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceVisitor<float>.VisitAll(rootFaces, new SortedOrderQueue<FaceVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		private static bool AreOrderedByFarthestDistance(FaceEdgeVisitor<float>.QueueItem lhs, FaceEdgeVisitor<float>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		public static void VisitFacesInFarthestDistanceOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<float>.VisitAll(rootEdge, new SortedOrderQueue<FaceEdgeVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<float>.VisitAll(rootEdges, new SortedOrderQueue<FaceEdgeVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		#endregion

		#region Double

		private static bool AreOrderedByFarthestDistance(FaceVisitor<double>.QueueItem lhs, FaceVisitor<double>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		public static void VisitFacesInFarthestDistanceOrder(Topology.Face rootFace, FaceVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceVisitor<double>.VisitAll(rootFace, new SortedOrderQueue<FaceVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceVisitor<double>.VisitAll(rootFaces, new SortedOrderQueue<FaceVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		private static bool AreOrderedByFarthestDistance(FaceEdgeVisitor<double>.QueueItem lhs, FaceEdgeVisitor<double>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		public static void VisitFacesInFarthestDistanceOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<double>.VisitAll(rootEdge, new SortedOrderQueue<FaceEdgeVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<double>.VisitAll(rootEdges, new SortedOrderQueue<FaceEdgeVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		#endregion

		#endregion

		#region Delegate Order Visitation

		public static void VisitFacesInOrder<TDistance>(Topology.Face rootFace, FaceVisitor<TDistance>.VisitDelegate visitDelegate, PriorityQueue<FaceVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceVisitor<TDistance>.VisitAll(rootFace, new SortedOrderQueue<FaceVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		public static void VisitFacesInOrder<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TDistance>.VisitDelegate visitDelegate, PriorityQueue<FaceVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceVisitor<TDistance>.VisitAll(rootFaces, new SortedOrderQueue<FaceVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		public static void VisitFacesInOrder<TDistance>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate, PriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdge, new SortedOrderQueue<FaceEdgeVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		public static void VisitFacesInOrder<TDistance>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate, PriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdges, new SortedOrderQueue<FaceEdgeVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		#endregion

		#region Random Order Visitation

		public static void VisitFacesInRandomOrder(Topology.Face rootFace, FaceVisitor.VisitDelegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceVisitor.VisitAll(rootFace, new RandomOrderQueue<FaceVisitor.QueueItem>(randomEngine), visitDelegate);
		}

		public static void VisitFacesInRandomOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor.VisitDelegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceVisitor.VisitAll(rootFaces, new RandomOrderQueue<FaceVisitor.QueueItem>(randomEngine), visitDelegate);
		}

		public static void VisitFacesInRandomOrder<TDistance>(Topology.Face rootFace, FaceVisitor<TDistance>.VisitDelegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceVisitor<TDistance>.VisitAll(rootFace, new RandomOrderQueue<FaceVisitor<TDistance>.QueueItem>(randomEngine), visitDelegate);
		}

		public static void VisitFacesInRandomOrder<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TDistance>.VisitDelegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceVisitor<TDistance>.VisitAll(rootFaces, new RandomOrderQueue<FaceVisitor<TDistance>.QueueItem>(randomEngine), visitDelegate);
		}

		public static void VisitFacesInRandomOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor.VisitDelegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceEdgeVisitor.VisitAll(rootEdge, new RandomOrderQueue<FaceEdgeVisitor.QueueItem>(randomEngine), visitDelegate);
		}

		public static void VisitFacesInRandomOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor.VisitDelegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceEdgeVisitor.VisitAll(rootEdges, new RandomOrderQueue<FaceEdgeVisitor.QueueItem>(randomEngine), visitDelegate);
		}

		public static void VisitFacesInRandomOrder<TDistance>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdge, new RandomOrderQueue<FaceEdgeVisitor<TDistance>.QueueItem>(randomEngine), visitDelegate);
		}

		public static void VisitFacesInRandomOrder<TDistance>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdges, new RandomOrderQueue<FaceEdgeVisitor<TDistance>.QueueItem>(randomEngine), visitDelegate);
		}

		#endregion
	}
}
