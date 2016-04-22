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

	public class Visit
	{
		protected bool _ignore;
		protected bool _break;

		public void Ignore()
		{
			_ignore = true;
		}

		public void Break()
		{
			_break = true;
		}

		public class Controller<TVisit> where TVisit : Visit, new()
		{
			public TVisit visit;

			public Controller()
			{
				visit = new TVisit();
			}

			public TVisit Set()
			{
				visit._ignore = false;
				visit._break = false;
				return visit;
			}

			public bool shouldIgnore
			{
				get
				{
					return visit._ignore;
				}
			}

			public bool shouldBreak
			{
				get
				{
					return visit._break;
				}
			}
		}
	}

	public class FaceVisit : Visit
	{
		public delegate IEnumerable<Topology.Face> Delegate(FaceVisit visit);

		protected Topology.Face _face;
		protected int _depth;

		public Topology.Face face { get { return _face; } }
		public int depth { get { return _depth; } }

		public class Controller : Controller<FaceVisit>
		{
			public FaceVisit Set(Topology.Face face, int depth)
			{
				visit._face = face;
				visit._depth = depth;
				return Set();
			}
		}
	}

	public class FaceVisit<TDistance> : Visit
	{
		public struct Neighbor
		{
			public Topology.Face face;
			public TDistance distance;

			public Neighbor(Topology.Face face, TDistance distance)
			{
				this.face = face;
				this.distance = distance;
			}
		}

		public delegate IEnumerable<Neighbor> Delegate(FaceVisit<TDistance> visit);
		public delegate bool AreOrderedDelegate(TDistance lhs, TDistance rhs);

		protected Topology.Face _face;
		protected int _depth;
		protected TDistance _distance;

		public Topology.Face face { get { return _face; } }
		public int depth { get { return _depth; } }
		public TDistance distance { get { return _distance; } }

		public Neighbor MakeNeighbor(Topology.Face face, TDistance distance)
		{
			return new Neighbor(face, distance);
		}

		public class Controller : Controller<FaceVisit<TDistance>>
		{
			public FaceVisit<TDistance> Set(Topology.Face face, int depth, TDistance distance)
			{
				visit._face = face;
				visit._depth = depth;
				visit._distance = distance;
				return Set();
			}
		}
	}

	public class FaceEdgeVisit : Visit
	{
		public delegate IEnumerable<Topology.FaceEdge> Delegate(FaceEdgeVisit visit);

		protected Topology.Face _face;
		protected int _depth;
		protected Topology.FaceEdge _edge;

		public Topology.Face face { get { return _face; } }
		public int depth { get { return _depth; } }
		public Topology.FaceEdge edge { get { return _edge; } }

		public class Controller : Controller<FaceEdgeVisit>
		{
			public FaceEdgeVisit Set(Topology.Face face, int depth)
			{
				visit._face = face;
				visit._depth = depth;
				visit._edge = Topology.FaceEdge.none;
				return Set();
			}

			public FaceEdgeVisit Set(Topology.FaceEdge edge, int depth)
			{
				visit._face = edge.farFace;
				visit._depth = depth;
				visit._edge = edge;
				return Set();
			}
		}
	}

	public class FaceEdgeVisit<TDistance> : Visit
	{
		public struct Neighbor
		{
			public Topology.FaceEdge edge;
			public TDistance distance;

			public Neighbor(Topology.FaceEdge edge, TDistance distance)
			{
				this.edge = edge;
				this.distance = distance;
			}
		}

		public delegate IEnumerable<Neighbor> Delegate(FaceEdgeVisit<TDistance> visit);
		public delegate bool AreOrderedDelegate(TDistance lhs, TDistance rhs);

		protected Topology.Face _face;
		protected int _depth;
		protected Topology.FaceEdge _edge;
		protected TDistance _distance;

		public Topology.Face face { get { return _face; } }
		public int depth { get { return _depth; } }
		public Topology.FaceEdge edge { get { return _edge; } }
		public TDistance distance { get { return _distance; } }

		public Neighbor MakeNeighbor(Topology.FaceEdge edge, TDistance distance)
		{
			return new Neighbor(edge, distance);
		}

		public class Controller : Controller<FaceEdgeVisit<TDistance>>
		{
			public FaceEdgeVisit<TDistance> Set(Topology.Face face, int depth, TDistance distance)
			{
				visit._face = face;
				visit._depth = depth;
				visit._edge = Topology.FaceEdge.none;
				visit._distance = distance;
				return Set();
			}

			public FaceEdgeVisit<TDistance> Set(Topology.FaceEdge edge, int depth, TDistance distance)
			{
				visit._face = edge.farFace;
				visit._depth = depth;
				visit._edge = edge;
				visit._distance = distance;
				return Set();
			}
		}
	}

	public class ExtendedFaceEdgeVisit<TEdge> : Visit where TEdge : Topology.IExtendedFaceEdge
	{
		public delegate IEnumerable<TEdge> Delegate(ExtendedFaceEdgeVisit<TEdge> visit);

		protected Topology.Face _face;
		protected int _depth;
		protected TEdge _edge;

		public Topology.Face face { get { return _face; } }
		public int depth { get { return _depth; } }
		public TEdge edge { get { return _edge; } }

		public class Controller : Controller<ExtendedFaceEdgeVisit<TEdge>>
		{
			public ExtendedFaceEdgeVisit<TEdge> Set(Topology.Face face, int depth)
			{
				visit._face = face;
				visit._depth = depth;
				visit._edge = default(TEdge);
				return Set();
			}

			public ExtendedFaceEdgeVisit<TEdge> Set(TEdge edge, int depth)
			{
				visit._face = edge.farFace;
				visit._depth = depth;
				visit._edge = edge;
				return Set();
			}
		}
	}

	public class ExtendedFaceEdgeVisit<TEdge, TDistance> : Visit where TEdge : Topology.IExtendedFaceEdge
	{
		public struct Neighbor
		{
			public TEdge edge;
			public TDistance distance;

			public Neighbor(TEdge edge, TDistance distance)
			{
				this.edge = edge;
				this.distance = distance;
			}
		}

		public delegate IEnumerable<Neighbor> Delegate(ExtendedFaceEdgeVisit<TEdge, TDistance> visit);
		public delegate bool AreOrderedDelegate(TDistance lhs, TDistance rhs);

		protected Topology.Face _face;
		protected int _depth;
		protected TEdge _edge;
		protected TDistance _distance;

		public Topology.Face face { get { return _face; } }
		public int depth { get { return _depth; } }
		public TEdge edge { get { return _edge; } }
		public TDistance distance { get { return _distance; } }

		public Neighbor MakeNeighbor(TEdge edge, TDistance distance)
		{
			return new Neighbor(edge, distance);
		}

		public class Controller : Controller<ExtendedFaceEdgeVisit<TEdge, TDistance>>
		{
			public ExtendedFaceEdgeVisit<TEdge, TDistance> Set(Topology.Face face, int depth, TDistance distance)
			{
				visit._face = face;
				visit._depth = depth;
				visit._edge = default(TEdge);
				visit._distance = distance;
				return Set();
			}

			public ExtendedFaceEdgeVisit<TEdge, TDistance> Set(TEdge edge, int depth, TDistance distance)
			{
				visit._face = edge.farFace;
				visit._depth = depth;
				visit._edge = edge;
				visit._distance = distance;
				return Set();
			}
		}
	}

	#endregion

	public static class FaceVisitationUtility
	{
		#region Private Visitation Types

		private interface IQueue<T>
		{
			bool isEmpty { get; }
			void Push(T item);
			T Pop();
		}

		private interface INumeric<T>
		{
			bool LessThanOrEqual(T lhs, T rhs);
			bool GreaterThanOrEqual(T lhs, T rhs);
		}

		private class Numeric : INumeric<int>, INumeric<uint>, INumeric<float>, INumeric<double>
		{
			bool INumeric<int>.LessThanOrEqual(int lhs, int rhs) { return lhs <= rhs; }
			bool INumeric<uint>.LessThanOrEqual(uint lhs, uint rhs) { return lhs <= rhs; }
			bool INumeric<float>.LessThanOrEqual(float lhs, float rhs) { return lhs <= rhs; }
			bool INumeric<double>.LessThanOrEqual(double lhs, double rhs) { return lhs <= rhs; }

			bool INumeric<int>.GreaterThanOrEqual(int lhs, int rhs) { return lhs >= rhs; }
			bool INumeric<uint>.GreaterThanOrEqual(uint lhs, uint rhs) { return lhs >= rhs; }
			bool INumeric<float>.GreaterThanOrEqual(float lhs, float rhs) { return lhs >= rhs; }
			bool INumeric<double>.GreaterThanOrEqual(double lhs, double rhs) { return lhs >= rhs; }

			public static Numeric instance = new Numeric();
		}

		private struct FaceVisitation : IEquatable<FaceVisitation>
		{
			public int faceIndex;
			public int depth;

			public FaceVisitation(int faceIndex, int depth)
			{
				this.faceIndex = faceIndex;
				this.depth = depth;
			}

			public bool Equals(FaceVisitation other)
			{
				return faceIndex == other.faceIndex;
			}

			public static bool AreOrderedBreadthFirst(FaceVisitation lhs, FaceVisitation rhs)
			{
				return lhs.depth <= rhs.depth;
			}

			public static bool AreOrderedDepthFirst(FaceVisitation lhs, FaceVisitation rhs)
			{
				return lhs.depth >= rhs.depth;
			}

			public static void Visit(Topology.Face rootFace, FaceVisit.Delegate visitDelegate, IQueue<FaceVisitation> queue)
			{
				queue.Push(new FaceVisitation(rootFace.index, 0));
				Visit(rootFace.topology, visitDelegate, queue);
			}

			public static void Visit(IEnumerable<Topology.Face> rootFaces, FaceVisit.Delegate visitDelegate, IQueue<FaceVisitation> queue)
			{
				var rootFacesEnumerator = rootFaces.GetEnumerator();
				if (!rootFacesEnumerator.MoveNext()) return;

				var topology = rootFacesEnumerator.Current.topology;

				do
				{
					queue.Push(new FaceVisitation(rootFacesEnumerator.Current.index, 0));
				} while (rootFacesEnumerator.MoveNext());

				Visit(topology, visitDelegate, queue);
			}

			private static void Visit(Topology topology, FaceVisit.Delegate visitDelegate, IQueue<FaceVisitation> queue)
			{
				var visitedFaces = new BitArray(topology.faces.Count);

				var visitController = new FaceVisit.Controller();

				while (!queue.isEmpty)
				{
					var queueItem = queue.Pop();

					// Move on to the next queue item if we've already visited the target face.
					if (visitedFaces[queueItem.faceIndex] == true) continue;

					// Visit the face and get the list of adjacent neighbors.
					var neighbors = visitDelegate(visitController.Set(new Topology.Face(topology, queueItem.faceIndex), queueItem.depth));

					// Check the visitation action and respond accordingly.
					if (visitController.shouldBreak) return;
					if (!visitController.shouldIgnore) visitedFaces[queueItem.faceIndex] = true;

					// Push all unvisited neighbors into the queue.
					if (neighbors == null) continue;
					var neighborDepth = queueItem.depth + 1;
					foreach (Topology.Face neighbor in neighbors)
					{
						if (visitedFaces[neighbor.index] == false)
						{
							queue.Push(new FaceVisitation(neighbor.index, neighborDepth));
						}
					}
				}
			}
		}

		private struct FaceVisitation<TDistance> : IEquatable<FaceVisitation<TDistance>>
		{
			public int faceIndex;
			public int depth;
			public TDistance distance;

			public FaceVisitation(int faceIndex, int depth, TDistance distance)
			{
				this.faceIndex = faceIndex;
				this.depth = depth;
				this.distance = distance;
			}

			public bool Equals(FaceVisitation<TDistance> other)
			{
				return faceIndex == other.faceIndex;
			}

			public static bool AreOrderedBreadthFirst(FaceVisitation<TDistance> lhs, FaceVisitation<TDistance> rhs)
			{
				return lhs.depth <= rhs.depth;
			}

			public static bool AreOrderedDepthFirst(FaceVisitation<TDistance> lhs, FaceVisitation<TDistance> rhs)
			{
				return lhs.depth >= rhs.depth;
			}

			/*public static bool AreOrderedBreadthFirstByDistance(FaceVisitation<TDistance> lhs, FaceVisitation<TDistance> rhs)
			{
				return AreOrderedBreadthFirstByDistance<INumeric<TDistance>>(lhs, rhs, Numeric.instance);
			}

			private static bool AreOrderedBreadthFirstByDistance<TNumeric>(FaceVisitation<TDistance> lhs, FaceVisitation<TDistance> rhs, TNumeric numeric) where TNumeric : INumeric<TDistance>
			{
				return numeric.GreaterThanOrEqual(lhs.distance, rhs.distance);
			}

			public static bool AreOrderedDepthFirstByDistance(FaceVisitation<int> lhs, FaceVisitation<int> rhs)
			{
				return lhs.distance >= rhs.distance;
			}*/

			public static void Visit(Topology.Face rootFace, FaceVisit<TDistance>.Delegate visitDelegate, IQueue<FaceVisitation<TDistance>> queue)
			{
				queue.Push(new FaceVisitation<TDistance>(rootFace.index, 0, default(TDistance)));
				Visit(rootFace.topology, visitDelegate, queue);
			}

			public static void Visit(IEnumerable<Topology.Face> rootFaces, FaceVisit<TDistance>.Delegate visitDelegate, IQueue<FaceVisitation<TDistance>> queue)
			{
				var rootFacesEnumerator = rootFaces.GetEnumerator();
				if (!rootFacesEnumerator.MoveNext()) return;

				var topology = rootFacesEnumerator.Current.topology;

				do
				{
					queue.Push(new FaceVisitation<TDistance>(rootFacesEnumerator.Current.index, 0, default(TDistance)));
				} while (rootFacesEnumerator.MoveNext());

				Visit(topology, visitDelegate, queue);
			}

			private static void Visit(Topology topology, FaceVisit<TDistance>.Delegate visitDelegate, IQueue<FaceVisitation<TDistance>> queue)
			{
				var visitedFaces = new BitArray(topology.faces.Count);

				var visitController = new FaceVisit<TDistance>.Controller();

				while (!queue.isEmpty)
				{
					var queueItem = queue.Pop();

					// Move on to the next queue item if we've already visited the target face.
					if (visitedFaces[queueItem.faceIndex] == true) continue;

					// Visit the face and get the list of adjacent neighbors.
					var neighbors = visitDelegate(visitController.Set(new Topology.Face(topology, queueItem.faceIndex), queueItem.depth, queueItem.distance));

					// Check the visitation action and respond accordingly.
					if (visitController.shouldBreak) return;
					if (!visitController.shouldIgnore) visitedFaces[queueItem.faceIndex] = true;

					// Push all unvisited neighbors into the queue.
					if (neighbors == null) continue;
					var neighborDepth = queueItem.depth + 1;
					foreach (var neighbor in neighbors)
					{
						if (visitedFaces[neighbor.face.index] == false)
						{
							queue.Push(new FaceVisitation<TDistance>(neighbor.face.index, neighborDepth, neighbor.distance));
						}
					}
				}
			}
		}

		private struct FaceEdgeVisitation : IEquatable<FaceEdgeVisitation>
		{
			public int edgeIndex;
			public int depth;

			public FaceEdgeVisitation(int edgeIndex, int depth)
			{
				this.edgeIndex = edgeIndex;
				this.depth = depth;
			}

			public bool Equals(FaceEdgeVisitation other)
			{
				return edgeIndex == other.edgeIndex;
			}

			public static bool AreOrderedBreadthFirst(FaceEdgeVisitation lhs, FaceEdgeVisitation rhs)
			{
				return lhs.depth <= rhs.depth;
			}

			public static bool AreOrderedDepthFirst(FaceEdgeVisitation lhs, FaceEdgeVisitation rhs)
			{
				return lhs.depth >= rhs.depth;
			}

			public static void Visit(Topology.FaceEdge rootEdge, FaceEdgeVisit.Delegate visitDelegate, IQueue<FaceEdgeVisitation> queue)
			{
				queue.Push(new FaceEdgeVisitation(rootEdge.index, 0));
				Visit(rootEdge.topology, visitDelegate, queue);
			}

			public static void Visit(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisit.Delegate visitDelegate, IQueue<FaceEdgeVisitation> queue)
			{
				var rootEdgesEnumerator = rootEdges.GetEnumerator();
				if (!rootEdgesEnumerator.MoveNext()) return;

				var topology = rootEdgesEnumerator.Current.topology;

				do
				{
					queue.Push(new FaceEdgeVisitation(rootEdgesEnumerator.Current.index, 0));
				} while (rootEdgesEnumerator.MoveNext());

				Visit(topology, visitDelegate, queue);
			}

			public static void Visit(Topology.Face rootFace, FaceEdgeVisit.Delegate visitDelegate, IQueue<FaceEdgeVisitation> queue)
			{
				var topology = rootFace.topology;
				var visitController = new FaceEdgeVisit.Controller();
				var visitedFaces = new BitArray(topology.faces.Count);

				var neighbors = visitDelegate(visitController.Set(rootFace, 0));

				if (visitController.shouldBreak) return;
				if (!visitController.shouldIgnore) visitedFaces[rootFace.index] = true;

				if (neighbors == null) return;
				foreach (Topology.FaceEdge neighbor in neighbors)
				{
					if (visitedFaces[neighbor.farFace.index] == false)
					{
						queue.Push(new FaceEdgeVisitation(neighbor.index, 1));
					}
				}

				Visit(topology, visitDelegate, queue, visitController, visitedFaces);
			}

			public static void Visit(IEnumerable<Topology.Face> rootFaces, FaceEdgeVisit.Delegate visitDelegate, IQueue<FaceEdgeVisitation> queue)
			{
				var rootFacesEnumerator = rootFaces.GetEnumerator();
				if (!rootFacesEnumerator.MoveNext()) return;

				var topology = rootFacesEnumerator.Current.topology;

				var visitController = new FaceEdgeVisit.Controller();
				var visitedFaces = new BitArray(topology.faces.Count);

				do
				{
					var neighbors = visitDelegate(visitController.Set(rootFacesEnumerator.Current, 0));

					if (visitController.shouldBreak) return;
					if (!visitController.shouldIgnore) visitedFaces[rootFacesEnumerator.Current.index] = true;

					if (neighbors == null) continue;
					foreach (Topology.FaceEdge neighbor in neighbors)
					{
						if (visitedFaces[neighbor.farFace.index] == false)
						{
							queue.Push(new FaceEdgeVisitation(neighbor.index, 1));
						}
					}
				} while (rootFacesEnumerator.MoveNext());

				Visit(topology, visitDelegate, queue, visitController, visitedFaces);
			}

			private static void Visit(Topology topology, FaceEdgeVisit.Delegate visitDelegate, IQueue<FaceEdgeVisitation> queue)
			{
				Visit(topology, visitDelegate, queue, new FaceEdgeVisit.Controller(), new BitArray(topology.faces.Count));
			}

			private static void Visit(Topology topology, FaceEdgeVisit.Delegate visitDelegate, IQueue<FaceEdgeVisitation> queue, FaceEdgeVisit.Controller visitController, BitArray visitedFaces)
			{
				while (!queue.isEmpty)
				{
					var queueItem = queue.Pop();

					// Move on to the next queue item if we've already visited the target face.
					var faceIndex = topology.edgeData[queueItem.edgeIndex].face;
					if (visitedFaces[faceIndex] == true) continue;

					// Visit the face and get the list of adjacent neighbors.
					var neighbors = visitDelegate(visitController.Set(new Topology.FaceEdge(topology, queueItem.edgeIndex), queueItem.depth));

					// Check the visitation action and respond accordingly.
					if (visitController.shouldBreak) return;
					if (!visitController.shouldIgnore) visitedFaces[faceIndex] = true;

					// Push all unvisited neighbors into the queue.
					if (neighbors == null) continue;
					var neighborDepth = queueItem.depth + 1;
					foreach (Topology.FaceEdge neighbor in neighbors)
					{
						if (visitedFaces[neighbor.farFace.index] == false)
						{
							queue.Push(new FaceEdgeVisitation(neighbor.index, neighborDepth));
						}
					}
				}
			}
		}

		private struct FaceEdgeVisitation<TDistance> : IEquatable<FaceEdgeVisitation<TDistance>>
		{
			public int edgeIndex;
			public int depth;
			public TDistance distance;

			public FaceEdgeVisitation(int edgeIndex, int depth, TDistance distance)
			{
				this.edgeIndex = edgeIndex;
				this.depth = depth;
				this.distance = distance;
			}

			public bool Equals(FaceEdgeVisitation<TDistance> other)
			{
				return edgeIndex == other.edgeIndex;
			}

			public static bool AreOrderedBreadthFirst(FaceEdgeVisitation<TDistance> lhs, FaceEdgeVisitation<TDistance> rhs)
			{
				return lhs.depth <= rhs.depth;
			}

			public static bool AreOrderedDepthFirst(FaceEdgeVisitation<TDistance> lhs, FaceEdgeVisitation<TDistance> rhs)
			{
				return lhs.depth >= rhs.depth;
			}

			public static void Visit(Topology.FaceEdge rootEdge, FaceEdgeVisit<TDistance>.Delegate visitDelegate, IQueue<FaceEdgeVisitation<TDistance>> queue)
			{
				queue.Push(new FaceEdgeVisitation<TDistance>(rootEdge.index, 0, default(TDistance)));
				Visit(rootEdge.topology, visitDelegate, queue);
			}

			public static void Visit(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisit<TDistance>.Delegate visitDelegate, IQueue<FaceEdgeVisitation<TDistance>> queue)
			{
				var rootEdgesEnumerator = rootEdges.GetEnumerator();
				if (!rootEdgesEnumerator.MoveNext()) return;

				var topology = rootEdgesEnumerator.Current.topology;

				do
				{
					queue.Push(new FaceEdgeVisitation<TDistance>(rootEdgesEnumerator.Current.index, 0, default(TDistance)));
				} while (rootEdgesEnumerator.MoveNext());

				Visit(topology, visitDelegate, queue);
			}

			public static void Visit(Topology.Face rootFace, FaceEdgeVisit<TDistance>.Delegate visitDelegate, IQueue<FaceEdgeVisitation<TDistance>> queue)
			{
				var topology = rootFace.topology;
				var visitController = new FaceEdgeVisit<TDistance>.Controller();
				var visitedFaces = new BitArray(topology.faces.Count);

				var neighbors = visitDelegate(visitController.Set(rootFace, 0, default(TDistance)));

				if (visitController.shouldBreak) return;
				if (!visitController.shouldIgnore) visitedFaces[rootFace.index] = true;

				if (neighbors == null) return;
				foreach (var neighbor in neighbors)
				{
					if (visitedFaces[neighbor.edge.farFace.index] == false)
					{
						queue.Push(new FaceEdgeVisitation<TDistance>(neighbor.edge.index, 1, neighbor.distance));
					}
				}

				Visit(topology, visitDelegate, queue, visitController, visitedFaces);
			}

			public static void Visit(IEnumerable<Topology.Face> rootFaces, FaceEdgeVisit<TDistance>.Delegate visitDelegate, IQueue<FaceEdgeVisitation<TDistance>> queue)
			{
				var rootFacesEnumerator = rootFaces.GetEnumerator();
				if (!rootFacesEnumerator.MoveNext()) return;

				var topology = rootFacesEnumerator.Current.topology;

				var visitController = new FaceEdgeVisit<TDistance>.Controller();
				var visitedFaces = new BitArray(topology.faces.Count);

				do
				{
					var neighbors = visitDelegate(visitController.Set(rootFacesEnumerator.Current, 0, default(TDistance)));

					if (visitController.shouldBreak) return;
					if (!visitController.shouldIgnore) visitedFaces[rootFacesEnumerator.Current.index] = true;

					if (neighbors == null) continue;
					foreach (var neighbor in neighbors)
					{
						if (visitedFaces[neighbor.edge.farFace.index] == false)
						{
							queue.Push(new FaceEdgeVisitation<TDistance>(neighbor.edge.index, 1, neighbor.distance));
						}
					}
				} while (rootFacesEnumerator.MoveNext());

				Visit(topology, visitDelegate, queue, visitController, visitedFaces);
			}

			private static void Visit(Topology topology, FaceEdgeVisit<TDistance>.Delegate visitDelegate, IQueue<FaceEdgeVisitation<TDistance>> queue)
			{
				Visit(topology, visitDelegate, queue, new FaceEdgeVisit<TDistance>.Controller(), new BitArray(topology.faces.Count));
			}

			private static void Visit(Topology topology, FaceEdgeVisit<TDistance>.Delegate visitDelegate, IQueue<FaceEdgeVisitation<TDistance>> queue, FaceEdgeVisit<TDistance>.Controller visitController, BitArray visitedFaces)
			{
				while (!queue.isEmpty)
				{
					var queueItem = queue.Pop();

					// Move on to the next queue item if we've already visited the target face.
					var faceIndex = topology.edgeData[queueItem.edgeIndex].face;
					if (visitedFaces[faceIndex] == true) continue;

					// Visit the face and get the list of adjacent neighbors.
					var neighbors = visitDelegate(visitController.Set(new Topology.FaceEdge(topology, queueItem.edgeIndex), queueItem.depth, queueItem.distance));

					// Check the visitation action and respond accordingly.
					if (visitController.shouldBreak) return;
					if (!visitController.shouldIgnore) visitedFaces[faceIndex] = true;

					// Push all unvisited neighbors into the queue.
					if (neighbors == null) continue;
					var neighborDepth = queueItem.depth + 1;
					foreach (var neighbor in neighbors)
					{
						if (visitedFaces[neighbor.edge.farFace.index] == false)
						{
							queue.Push(new FaceEdgeVisitation<TDistance>(neighbor.edge.index, neighborDepth, neighbor.distance));
						}
					}
				}
			}
		}

		private struct ExtendedFaceEdgeVisitation<TEdge> : IEquatable<ExtendedFaceEdgeVisitation<TEdge>> where TEdge : Topology.IExtendedFaceEdge
		{
			public TEdge edge;
			public int depth;

			public ExtendedFaceEdgeVisitation(TEdge edge, int depth)
			{
				this.edge = edge;
				this.depth = depth;
			}

			public bool Equals(ExtendedFaceEdgeVisitation<TEdge> other)
			{
				return edge.Equals(other.edge);
			}

			public static bool AreOrderedBreadthFirst(ExtendedFaceEdgeVisitation<TEdge> lhs, ExtendedFaceEdgeVisitation<TEdge> rhs)
			{
				return lhs.depth <= rhs.depth;
			}

			public static bool AreOrderedDepthFirst(ExtendedFaceEdgeVisitation<TEdge> lhs, ExtendedFaceEdgeVisitation<TEdge> rhs)
			{
				return lhs.depth >= rhs.depth;
			}

			public static void Visit(TEdge rootEdge, ExtendedFaceEdgeVisit<TEdge>.Delegate visitDelegate, IQueue<ExtendedFaceEdgeVisitation<TEdge>> queue)
			{
				queue.Push(new ExtendedFaceEdgeVisitation<TEdge>(rootEdge, 0));
				Visit(rootEdge.topology, visitDelegate, queue);
			}

			public static void Visit(IEnumerable<TEdge> rootEdges, ExtendedFaceEdgeVisit<TEdge>.Delegate visitDelegate, IQueue<ExtendedFaceEdgeVisitation<TEdge>> queue)
			{
				var rootEdgesEnumerator = rootEdges.GetEnumerator();
				if (!rootEdgesEnumerator.MoveNext()) return;

				var topology = rootEdgesEnumerator.Current.topology;

				do
				{
					queue.Push(new ExtendedFaceEdgeVisitation<TEdge>(rootEdgesEnumerator.Current, 0));
				} while (rootEdgesEnumerator.MoveNext());

				Visit(topology, visitDelegate, queue);
			}

			public static void Visit(Topology.Face rootFace, ExtendedFaceEdgeVisit<TEdge>.Delegate visitDelegate, IQueue<ExtendedFaceEdgeVisitation<TEdge>> queue)
			{
				var topology = rootFace.topology;
				var visitController = new ExtendedFaceEdgeVisit<TEdge>.Controller();
				var visitedFaces = new BitArray(topology.faces.Count);

				var neighbors = visitDelegate(visitController.Set(rootFace, 0));

				if (visitController.shouldBreak) return;
				if (!visitController.shouldIgnore) visitedFaces[rootFace.index] = true;

				if (neighbors == null) return;
				foreach (TEdge neighbor in neighbors)
				{
					if (visitedFaces[neighbor.farFace.index] == false)
					{
						queue.Push(new ExtendedFaceEdgeVisitation<TEdge>(neighbor, 1));
					}
				}

				Visit(topology, visitDelegate, queue, visitController, visitedFaces);
			}

			public static void Visit(IEnumerable<Topology.Face> rootFaces, ExtendedFaceEdgeVisit<TEdge>.Delegate visitDelegate, IQueue<ExtendedFaceEdgeVisitation<TEdge>> queue)
			{
				var rootFacesEnumerator = rootFaces.GetEnumerator();
				if (!rootFacesEnumerator.MoveNext()) return;

				var topology = rootFacesEnumerator.Current.topology;

				var visitController = new ExtendedFaceEdgeVisit<TEdge>.Controller();
				var visitedFaces = new BitArray(topology.faces.Count);

				do
				{
					var neighbors = visitDelegate(visitController.Set(rootFacesEnumerator.Current, 0));

					if (visitController.shouldBreak) return;
					if (!visitController.shouldIgnore) visitedFaces[rootFacesEnumerator.Current.index] = true;

					if (neighbors == null) continue;
					foreach (TEdge neighbor in neighbors)
					{
						if (visitedFaces[neighbor.farFace.index] == false)
						{
							queue.Push(new ExtendedFaceEdgeVisitation<TEdge>(neighbor, 1));
						}
					}
				} while (rootFacesEnumerator.MoveNext());

				Visit(topology, visitDelegate, queue, visitController, visitedFaces);
			}

			private static void Visit(Topology topology, ExtendedFaceEdgeVisit<TEdge>.Delegate visitDelegate, IQueue<ExtendedFaceEdgeVisitation<TEdge>> queue)
			{
				Visit(topology, visitDelegate, queue, new ExtendedFaceEdgeVisit<TEdge>.Controller(), new BitArray(topology.faces.Count));
			}

			private static void Visit(Topology topology, ExtendedFaceEdgeVisit<TEdge>.Delegate visitDelegate, IQueue<ExtendedFaceEdgeVisitation<TEdge>> queue, ExtendedFaceEdgeVisit<TEdge>.Controller visitController, BitArray visitedFaces)
			{
				while (!queue.isEmpty)
				{
					var queueItem = queue.Pop();

					// Move on to the next queue item if we've already visited the target face.
					var faceIndex = queueItem.edge.farFace.index;
					if (visitedFaces[faceIndex] == true) continue;

					// Visit the face and get the list of adjacent neighbors.
					var neighbors = visitDelegate(visitController.Set(queueItem.edge, queueItem.depth));

					// Check the visitation action and respond accordingly.
					if (visitController.shouldBreak) return;
					if (!visitController.shouldIgnore) visitedFaces[faceIndex] = true;

					// Push all unvisited neighbors into the queue.
					if (neighbors == null) continue;
					var neighborDepth = queueItem.depth + 1;
					foreach (TEdge neighbor in neighbors)
					{
						if (visitedFaces[neighbor.farFace.index] == false)
						{
							queue.Push(new ExtendedFaceEdgeVisitation<TEdge>(neighbor, neighborDepth));
						}
					}
				}
			}
		}

		private struct ExtendedFaceEdgeVisitation<TEdge, TDistance> : IEquatable<ExtendedFaceEdgeVisitation<TEdge, TDistance>> where TEdge : Topology.IExtendedFaceEdge
		{
			public TEdge edge;
			public int depth;
			public TDistance distance;

			public ExtendedFaceEdgeVisitation(TEdge edge, int depth, TDistance distance)
			{
				this.edge = edge;
				this.depth = depth;
				this.distance = distance;
			}

			public bool Equals(ExtendedFaceEdgeVisitation<TEdge, TDistance> other)
			{
				return edge.Equals(other.edge);
			}

			public static bool AreOrderedBreadthFirst(ExtendedFaceEdgeVisitation<TEdge, TDistance> lhs, ExtendedFaceEdgeVisitation<TEdge, TDistance> rhs)
			{
				return lhs.depth <= rhs.depth;
			}

			public static bool AreOrderedDepthFirst(ExtendedFaceEdgeVisitation<TEdge, TDistance> lhs, ExtendedFaceEdgeVisitation<TEdge, TDistance> rhs)
			{
				return lhs.depth >= rhs.depth;
			}

			public static void Visit(TEdge rootEdge, ExtendedFaceEdgeVisit<TEdge, TDistance>.Delegate visitDelegate, IQueue<ExtendedFaceEdgeVisitation<TEdge, TDistance>> queue)
			{
				queue.Push(new ExtendedFaceEdgeVisitation<TEdge, TDistance>(rootEdge, 0, default(TDistance)));
				Visit(rootEdge.topology, visitDelegate, queue);
			}

			public static void Visit(IEnumerable<TEdge> rootEdges, ExtendedFaceEdgeVisit<TEdge, TDistance>.Delegate visitDelegate, IQueue<ExtendedFaceEdgeVisitation<TEdge, TDistance>> queue)
			{
				var rootEdgesEnumerator = rootEdges.GetEnumerator();
				if (!rootEdgesEnumerator.MoveNext()) return;

				var topology = rootEdgesEnumerator.Current.topology;

				do
				{
					queue.Push(new ExtendedFaceEdgeVisitation<TEdge, TDistance>(rootEdgesEnumerator.Current, 0, default(TDistance)));
				} while (rootEdgesEnumerator.MoveNext());

				Visit(topology, visitDelegate, queue);
			}

			public static void Visit(Topology.Face rootFace, ExtendedFaceEdgeVisit<TEdge, TDistance>.Delegate visitDelegate, IQueue<ExtendedFaceEdgeVisitation<TEdge, TDistance>> queue)
			{
				var topology = rootFace.topology;
				var visitController = new ExtendedFaceEdgeVisit<TEdge, TDistance>.Controller();
				var visitedFaces = new BitArray(topology.faces.Count);

				var neighbors = visitDelegate(visitController.Set(rootFace, 0, default(TDistance)));

				if (visitController.shouldBreak) return;
				if (!visitController.shouldIgnore) visitedFaces[rootFace.index] = true;

				if (neighbors == null) return;
				foreach (var neighbor in neighbors)
				{
					if (visitedFaces[neighbor.edge.farFace.index] == false)
					{
						queue.Push(new ExtendedFaceEdgeVisitation<TEdge, TDistance>(neighbor.edge, 1, neighbor.distance));
					}
				}

				Visit(topology, visitDelegate, queue, visitController, visitedFaces);
			}

			public static void Visit(IEnumerable<Topology.Face> rootFaces, ExtendedFaceEdgeVisit<TEdge, TDistance>.Delegate visitDelegate, IQueue<ExtendedFaceEdgeVisitation<TEdge, TDistance>> queue)
			{
				var rootFacesEnumerator = rootFaces.GetEnumerator();
				if (!rootFacesEnumerator.MoveNext()) return;

				var topology = rootFacesEnumerator.Current.topology;

				var visitController = new ExtendedFaceEdgeVisit<TEdge, TDistance>.Controller();
				var visitedFaces = new BitArray(topology.faces.Count);

				do
				{
					var neighbors = visitDelegate(visitController.Set(rootFacesEnumerator.Current, 0, default(TDistance)));

					if (visitController.shouldBreak) return;
					if (!visitController.shouldIgnore) visitedFaces[rootFacesEnumerator.Current.index] = true;

					if (neighbors == null) continue;
					foreach (var neighbor in neighbors)
					{
						if (visitedFaces[neighbor.edge.farFace.index] == false)
						{
							queue.Push(new ExtendedFaceEdgeVisitation<TEdge, TDistance>(neighbor.edge, 1, neighbor.distance));
						}
					}
				} while (rootFacesEnumerator.MoveNext());

				Visit(topology, visitDelegate, queue, visitController, visitedFaces);
			}

			private static void Visit(Topology topology, ExtendedFaceEdgeVisit<TEdge, TDistance>.Delegate visitDelegate, IQueue<ExtendedFaceEdgeVisitation<TEdge, TDistance>> queue)
			{
				Visit(topology, visitDelegate, queue, new ExtendedFaceEdgeVisit<TEdge, TDistance>.Controller(), new BitArray(topology.faces.Count));
			}

			private static void Visit(Topology topology, ExtendedFaceEdgeVisit<TEdge, TDistance>.Delegate visitDelegate, IQueue<ExtendedFaceEdgeVisitation<TEdge, TDistance>> queue, ExtendedFaceEdgeVisit<TEdge, TDistance>.Controller visitController, BitArray visitedFaces)
			{
				while (!queue.isEmpty)
				{
					var queueItem = queue.Pop();

					// Move on to the next queue item if we've already visited the target face.
					var faceIndex = queueItem.edge.farFace.index;
					if (visitedFaces[faceIndex] == true) continue;

					// Visit the face and get the list of adjacent neighbors.
					var neighbors = visitDelegate(visitController.Set(queueItem.edge, queueItem.depth, queueItem.distance));

					// Check the visitation action and respond accordingly.
					if (visitController.shouldBreak) return;
					if (!visitController.shouldIgnore) visitedFaces[faceIndex] = true;

					// Push all unvisited neighbors into the queue.
					if (neighbors == null) continue;
					var neighborDepth = queueItem.depth + 1;
					foreach (var neighbor in neighbors)
					{
						if (visitedFaces[neighbor.edge.farFace.index] == false)
						{
							queue.Push(new ExtendedFaceEdgeVisitation<TEdge, TDistance>(neighbor.edge, neighborDepth, neighbor.distance));
						}
					}
				}
			}
		}

		#endregion

		#region Queues

		private class ArbitraryOrderQueue<T> : IQueue<T>
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

		private class SortedOrderQueue<T> : IQueue<T> where T : IEquatable<T>
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

		private class RandomOrderQueue<T> : IQueue<T> where T : IEquatable<T>
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

		public static void VisitFaces(Topology.Face rootFace, FaceVisit.Delegate visitDelegate)
		{
			FaceVisitation.Visit(rootFace, visitDelegate, new ArbitraryOrderQueue<FaceVisitation>());
		}

		public static void VisitFaces(IEnumerable<Topology.Face> rootFaces, FaceVisit.Delegate visitDelegate)
		{
			FaceVisitation.Visit(rootFaces, visitDelegate, new ArbitraryOrderQueue<FaceVisitation>());
		}

		public static void VisitFaces<TDistance>(Topology.Face rootFace, FaceVisit<TDistance>.Delegate visitDelegate)
		{
			FaceVisitation<TDistance>.Visit(rootFace, visitDelegate, new ArbitraryOrderQueue<FaceVisitation<TDistance>>());
		}

		public static void VisitFaces<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceVisit<TDistance>.Delegate visitDelegate)
		{
			FaceVisitation<TDistance>.Visit(rootFaces, visitDelegate, new ArbitraryOrderQueue<FaceVisitation<TDistance>>());
		}

		public static void VisitFaces(Topology.FaceEdge rootEdge, FaceEdgeVisit.Delegate visitDelegate)
		{
			FaceEdgeVisitation.Visit(rootEdge, visitDelegate, new ArbitraryOrderQueue<FaceEdgeVisitation>());
		}

		public static void VisitFaces(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisit.Delegate visitDelegate)
		{
			FaceEdgeVisitation.Visit(rootEdges, visitDelegate, new ArbitraryOrderQueue<FaceEdgeVisitation>());
		}

		public static void VisitFaces(Topology.Face rootFace, FaceEdgeVisit.Delegate visitDelegate)
		{
			FaceEdgeVisitation.Visit(rootFace, visitDelegate, new ArbitraryOrderQueue<FaceEdgeVisitation>());
		}

		public static void VisitFaces(IEnumerable<Topology.Face> rootFaces, FaceEdgeVisit.Delegate visitDelegate)
		{
			FaceEdgeVisitation.Visit(rootFaces, visitDelegate, new ArbitraryOrderQueue<FaceEdgeVisitation>());
		}

		public static void VisitFaces<TDistance>(Topology.FaceEdge rootEdge, FaceEdgeVisit<TDistance>.Delegate visitDelegate)
		{
			FaceEdgeVisitation<TDistance>.Visit(rootEdge, visitDelegate, new ArbitraryOrderQueue<FaceEdgeVisitation<TDistance>>());
		}

		public static void VisitFaces<TDistance>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisit<TDistance>.Delegate visitDelegate)
		{
			FaceEdgeVisitation<TDistance>.Visit(rootEdges, visitDelegate, new ArbitraryOrderQueue<FaceEdgeVisitation<TDistance>>());
		}

		public static void VisitFaces<TDistance>(Topology.Face rootFace, FaceEdgeVisit<TDistance>.Delegate visitDelegate)
		{
			FaceEdgeVisitation<TDistance>.Visit(rootFace, visitDelegate, new ArbitraryOrderQueue<FaceEdgeVisitation<TDistance>>());
		}

		public static void VisitFaces<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceEdgeVisit<TDistance>.Delegate visitDelegate)
		{
			FaceEdgeVisitation<TDistance>.Visit(rootFaces, visitDelegate, new ArbitraryOrderQueue<FaceEdgeVisitation<TDistance>>());
		}

		public static void VisitFaces<TEdge>(TEdge rootEdge, ExtendedFaceEdgeVisit<TEdge>.Delegate visitDelegate) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge>.Visit(rootEdge, visitDelegate, new ArbitraryOrderQueue<ExtendedFaceEdgeVisitation<TEdge>>());
		}

		public static void VisitFaces<TEdge>(IEnumerable<TEdge> rootEdges, ExtendedFaceEdgeVisit<TEdge>.Delegate visitDelegate) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge>.Visit(rootEdges, visitDelegate, new ArbitraryOrderQueue<ExtendedFaceEdgeVisitation<TEdge>>());
		}

		public static void VisitFaces<TEdge>(Topology.Face rootFace, ExtendedFaceEdgeVisit<TEdge>.Delegate visitDelegate) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge>.Visit(rootFace, visitDelegate, new ArbitraryOrderQueue<ExtendedFaceEdgeVisitation<TEdge>>());
		}

		public static void VisitFaces<TEdge>(IEnumerable<Topology.Face> rootFaces, ExtendedFaceEdgeVisit<TEdge>.Delegate visitDelegate) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge>.Visit(rootFaces, visitDelegate, new ArbitraryOrderQueue<ExtendedFaceEdgeVisitation<TEdge>>());
		}

		public static void VisitFaces<TEdge, TDistance>(TEdge rootEdge, ExtendedFaceEdgeVisit<TEdge, TDistance>.Delegate visitDelegate) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge, TDistance>.Visit(rootEdge, visitDelegate, new ArbitraryOrderQueue<ExtendedFaceEdgeVisitation<TEdge, TDistance>>());
		}

		public static void VisitFaces<TEdge, TDistance>(IEnumerable<TEdge> rootEdges, ExtendedFaceEdgeVisit<TEdge, TDistance>.Delegate visitDelegate) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge, TDistance>.Visit(rootEdges, visitDelegate, new ArbitraryOrderQueue<ExtendedFaceEdgeVisitation<TEdge, TDistance>>());
		}

		public static void VisitFaces<TEdge, TDistance>(Topology.Face rootFace, ExtendedFaceEdgeVisit<TEdge, TDistance>.Delegate visitDelegate) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge, TDistance>.Visit(rootFace, visitDelegate, new ArbitraryOrderQueue<ExtendedFaceEdgeVisitation<TEdge, TDistance>>());
		}

		public static void VisitFaces<TEdge, TDistance>(IEnumerable<Topology.Face> rootFaces, ExtendedFaceEdgeVisit<TEdge, TDistance>.Delegate visitDelegate) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge, TDistance>.Visit(rootFaces, visitDelegate, new ArbitraryOrderQueue<ExtendedFaceEdgeVisitation<TEdge, TDistance>>());
		}

		#endregion

		#region Breadth First Visitation

		public static void VisitFacesBreadthFirst(Topology.Face rootFace, FaceVisit.Delegate visitDelegate)
		{
			FaceVisitation.Visit(rootFace, visitDelegate, new SortedOrderQueue<FaceVisitation>(FaceVisitation.AreOrderedBreadthFirst));
		}

		public static void VisitFacesBreadthFirst(IEnumerable<Topology.Face> rootFaces, FaceVisit.Delegate visitDelegate)
		{
			FaceVisitation.Visit(rootFaces, visitDelegate, new SortedOrderQueue<FaceVisitation>(FaceVisitation.AreOrderedBreadthFirst));
		}

		public static void VisitFacesBreadthFirst<TDistance>(Topology.Face rootFace, FaceVisit<TDistance>.Delegate visitDelegate, bool sortByDistance = true)
		{
			FaceVisitation<TDistance>.Visit(rootFace, visitDelegate, new SortedOrderQueue<FaceVisitation<TDistance>>(FaceVisitation<TDistance>.AreOrderedBreadthFirst));
		}

		public static void VisitFacesBreadthFirst<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceVisit<TDistance>.Delegate visitDelegate, bool sortByDistance = true)
		{
			FaceVisitation<TDistance>.Visit(rootFaces, visitDelegate, new SortedOrderQueue<FaceVisitation<TDistance>>(FaceVisitation<TDistance>.AreOrderedBreadthFirst));
		}

		public static void VisitFacesBreadthFirst(Topology.FaceEdge rootEdge, FaceEdgeVisit.Delegate visitDelegate)
		{
			FaceEdgeVisitation.Visit(rootEdge, visitDelegate, new SortedOrderQueue<FaceEdgeVisitation>(FaceEdgeVisitation.AreOrderedBreadthFirst));
		}

		public static void VisitFacesBreadthFirst(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisit.Delegate visitDelegate)
		{
			FaceEdgeVisitation.Visit(rootEdges, visitDelegate, new SortedOrderQueue<FaceEdgeVisitation>(FaceEdgeVisitation.AreOrderedBreadthFirst));
		}

		public static void VisitFacesBreadthFirst(Topology.Face rootFace, FaceEdgeVisit.Delegate visitDelegate)
		{
			FaceEdgeVisitation.Visit(rootFace, visitDelegate, new SortedOrderQueue<FaceEdgeVisitation>(FaceEdgeVisitation.AreOrderedBreadthFirst));
		}

		public static void VisitFacesBreadthFirst(IEnumerable<Topology.Face> rootFaces, FaceEdgeVisit.Delegate visitDelegate)
		{
			FaceEdgeVisitation.Visit(rootFaces, visitDelegate, new SortedOrderQueue<FaceEdgeVisitation>(FaceEdgeVisitation.AreOrderedBreadthFirst));
		}

		public static void VisitFacesBreadthFirst<TDistance>(Topology.FaceEdge rootEdge, FaceEdgeVisit<TDistance>.Delegate visitDelegate, bool sortByDistance = true)
		{
			FaceEdgeVisitation<TDistance>.Visit(rootEdge, visitDelegate, new SortedOrderQueue<FaceEdgeVisitation<TDistance>>(FaceEdgeVisitation<TDistance>.AreOrderedBreadthFirst));
		}

		public static void VisitFacesBreadthFirst<TDistance>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisit<TDistance>.Delegate visitDelegate, bool sortByDistance = true)
		{
			FaceEdgeVisitation<TDistance>.Visit(rootEdges, visitDelegate, new SortedOrderQueue<FaceEdgeVisitation<TDistance>>(FaceEdgeVisitation<TDistance>.AreOrderedBreadthFirst));
		}

		public static void VisitFacesBreadthFirst<TDistance>(Topology.Face rootFace, FaceEdgeVisit<TDistance>.Delegate visitDelegate, bool sortByDistance = true)
		{
			FaceEdgeVisitation<TDistance>.Visit(rootFace, visitDelegate, new SortedOrderQueue<FaceEdgeVisitation<TDistance>>(FaceEdgeVisitation<TDistance>.AreOrderedBreadthFirst));
		}

		public static void VisitFacesBreadthFirst<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceEdgeVisit<TDistance>.Delegate visitDelegate, bool sortByDistance = true)
		{
			FaceEdgeVisitation<TDistance>.Visit(rootFaces, visitDelegate, new SortedOrderQueue<FaceEdgeVisitation<TDistance>>(FaceEdgeVisitation<TDistance>.AreOrderedBreadthFirst));
		}

		public static void VisitFacesBreadthFirst<TEdge>(TEdge rootEdge, ExtendedFaceEdgeVisit<TEdge>.Delegate visitDelegate) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge>.Visit(rootEdge, visitDelegate, new SortedOrderQueue<ExtendedFaceEdgeVisitation<TEdge>>(ExtendedFaceEdgeVisitation<TEdge>.AreOrderedBreadthFirst));
		}

		public static void VisitFacesBreadthFirst<TEdge>(IEnumerable<TEdge> rootEdges, ExtendedFaceEdgeVisit<TEdge>.Delegate visitDelegate) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge>.Visit(rootEdges, visitDelegate, new SortedOrderQueue<ExtendedFaceEdgeVisitation<TEdge>>(ExtendedFaceEdgeVisitation<TEdge>.AreOrderedBreadthFirst));
		}

		public static void VisitFacesBreadthFirst<TEdge>(Topology.Face rootFace, ExtendedFaceEdgeVisit<TEdge>.Delegate visitDelegate) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge>.Visit(rootFace, visitDelegate, new SortedOrderQueue<ExtendedFaceEdgeVisitation<TEdge>>(ExtendedFaceEdgeVisitation<TEdge>.AreOrderedBreadthFirst));
		}

		public static void VisitFacesBreadthFirst<TEdge>(IEnumerable<Topology.Face> rootFaces, ExtendedFaceEdgeVisit<TEdge>.Delegate visitDelegate) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge>.Visit(rootFaces, visitDelegate, new SortedOrderQueue<ExtendedFaceEdgeVisitation<TEdge>>(ExtendedFaceEdgeVisitation<TEdge>.AreOrderedBreadthFirst));
		}

		public static void VisitFacesBreadthFirst<TEdge, TDistance>(TEdge rootEdge, ExtendedFaceEdgeVisit<TEdge, TDistance>.Delegate visitDelegate, bool sortByDistance = true) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge, TDistance>.Visit(rootEdge, visitDelegate, new SortedOrderQueue<ExtendedFaceEdgeVisitation<TEdge, TDistance>>(ExtendedFaceEdgeVisitation<TEdge, TDistance>.AreOrderedBreadthFirst));
		}

		public static void VisitFacesBreadthFirst<TEdge, TDistance>(IEnumerable<TEdge> rootEdges, ExtendedFaceEdgeVisit<TEdge, TDistance>.Delegate visitDelegate, bool sortByDistance = true) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge, TDistance>.Visit(rootEdges, visitDelegate, new SortedOrderQueue<ExtendedFaceEdgeVisitation<TEdge, TDistance>>(ExtendedFaceEdgeVisitation<TEdge, TDistance>.AreOrderedBreadthFirst));
		}

		public static void VisitFacesBreadthFirst<TEdge, TDistance>(Topology.Face rootFace, ExtendedFaceEdgeVisit<TEdge, TDistance>.Delegate visitDelegate, bool sortByDistance = true) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge, TDistance>.Visit(rootFace, visitDelegate, new SortedOrderQueue<ExtendedFaceEdgeVisitation<TEdge, TDistance>>(ExtendedFaceEdgeVisitation<TEdge, TDistance>.AreOrderedBreadthFirst));
		}

		public static void VisitFacesBreadthFirst<TEdge, TDistance>(IEnumerable<Topology.Face> rootFaces, ExtendedFaceEdgeVisit<TEdge, TDistance>.Delegate visitDelegate, bool sortByDistance = true) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge, TDistance>.Visit(rootFaces, visitDelegate, new SortedOrderQueue<ExtendedFaceEdgeVisitation<TEdge, TDistance>>(ExtendedFaceEdgeVisitation<TEdge, TDistance>.AreOrderedBreadthFirst));
		}

		#endregion

		#region Depth First Visitation

		public static void VisitFacesDepthFirst(Topology.Face rootFace, FaceVisit.Delegate visitDelegate)
		{
			FaceVisitation.Visit(rootFace, visitDelegate, new SortedOrderQueue<FaceVisitation>(FaceVisitation.AreOrderedDepthFirst));
		}

		public static void VisitFacesDepthFirst(IEnumerable<Topology.Face> rootFaces, FaceVisit.Delegate visitDelegate)
		{
			FaceVisitation.Visit(rootFaces, visitDelegate, new SortedOrderQueue<FaceVisitation>(FaceVisitation.AreOrderedDepthFirst));
		}

		public static void VisitFacesDepthFirst<TDistance>(Topology.Face rootFace, FaceVisit<TDistance>.Delegate visitDelegate)
		{
			FaceVisitation<TDistance>.Visit(rootFace, visitDelegate, new SortedOrderQueue<FaceVisitation<TDistance>>(FaceVisitation<TDistance>.AreOrderedDepthFirst));
		}

		public static void VisitFacesDepthFirst<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceVisit<TDistance>.Delegate visitDelegate)
		{
			FaceVisitation<TDistance>.Visit(rootFaces, visitDelegate, new SortedOrderQueue<FaceVisitation<TDistance>>(FaceVisitation<TDistance>.AreOrderedDepthFirst));
		}

		public static void VisitFacesDepthFirst(Topology.FaceEdge rootEdge, FaceEdgeVisit.Delegate visitDelegate)
		{
			FaceEdgeVisitation.Visit(rootEdge, visitDelegate, new SortedOrderQueue<FaceEdgeVisitation>(FaceEdgeVisitation.AreOrderedDepthFirst));
		}

		public static void VisitFacesDepthFirst(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisit.Delegate visitDelegate)
		{
			FaceEdgeVisitation.Visit(rootEdges, visitDelegate, new SortedOrderQueue<FaceEdgeVisitation>(FaceEdgeVisitation.AreOrderedDepthFirst));
		}

		public static void VisitFacesDepthFirst(Topology.Face rootFace, FaceEdgeVisit.Delegate visitDelegate)
		{
			FaceEdgeVisitation.Visit(rootFace, visitDelegate, new SortedOrderQueue<FaceEdgeVisitation>(FaceEdgeVisitation.AreOrderedDepthFirst));
		}

		public static void VisitFacesDepthFirst(IEnumerable<Topology.Face> rootFaces, FaceEdgeVisit.Delegate visitDelegate)
		{
			FaceEdgeVisitation.Visit(rootFaces, visitDelegate, new SortedOrderQueue<FaceEdgeVisitation>(FaceEdgeVisitation.AreOrderedDepthFirst));
		}

		public static void VisitFacesDepthFirst<TDistance>(Topology.FaceEdge rootEdge, FaceEdgeVisit<TDistance>.Delegate visitDelegate)
		{
			FaceEdgeVisitation<TDistance>.Visit(rootEdge, visitDelegate, new SortedOrderQueue<FaceEdgeVisitation<TDistance>>(FaceEdgeVisitation<TDistance>.AreOrderedDepthFirst));
		}

		public static void VisitFacesDepthFirst<TDistance>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisit<TDistance>.Delegate visitDelegate)
		{
			FaceEdgeVisitation<TDistance>.Visit(rootEdges, visitDelegate, new SortedOrderQueue<FaceEdgeVisitation<TDistance>>(FaceEdgeVisitation<TDistance>.AreOrderedDepthFirst));
		}

		public static void VisitFacesDepthFirst<TDistance>(Topology.Face rootFace, FaceEdgeVisit<TDistance>.Delegate visitDelegate)
		{
			FaceEdgeVisitation<TDistance>.Visit(rootFace, visitDelegate, new SortedOrderQueue<FaceEdgeVisitation<TDistance>>(FaceEdgeVisitation<TDistance>.AreOrderedDepthFirst));
		}

		public static void VisitFacesDepthFirst<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceEdgeVisit<TDistance>.Delegate visitDelegate)
		{
			FaceEdgeVisitation<TDistance>.Visit(rootFaces, visitDelegate, new SortedOrderQueue<FaceEdgeVisitation<TDistance>>(FaceEdgeVisitation<TDistance>.AreOrderedDepthFirst));
		}

		public static void VisitFacesDepthFirst<TEdge>(TEdge rootEdge, ExtendedFaceEdgeVisit<TEdge>.Delegate visitDelegate) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge>.Visit(rootEdge, visitDelegate, new SortedOrderQueue<ExtendedFaceEdgeVisitation<TEdge>>(ExtendedFaceEdgeVisitation<TEdge>.AreOrderedDepthFirst));
		}

		public static void VisitFacesDepthFirst<TEdge>(IEnumerable<TEdge> rootEdges, ExtendedFaceEdgeVisit<TEdge>.Delegate visitDelegate) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge>.Visit(rootEdges, visitDelegate, new SortedOrderQueue<ExtendedFaceEdgeVisitation<TEdge>>(ExtendedFaceEdgeVisitation<TEdge>.AreOrderedDepthFirst));
		}

		public static void VisitFacesDepthFirst<TEdge>(Topology.Face rootFace, ExtendedFaceEdgeVisit<TEdge>.Delegate visitDelegate) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge>.Visit(rootFace, visitDelegate, new SortedOrderQueue<ExtendedFaceEdgeVisitation<TEdge>>(ExtendedFaceEdgeVisitation<TEdge>.AreOrderedDepthFirst));
		}

		public static void VisitFacesDepthFirst<TEdge>(IEnumerable<Topology.Face> rootFaces, ExtendedFaceEdgeVisit<TEdge>.Delegate visitDelegate) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge>.Visit(rootFaces, visitDelegate, new SortedOrderQueue<ExtendedFaceEdgeVisitation<TEdge>>(ExtendedFaceEdgeVisitation<TEdge>.AreOrderedDepthFirst));
		}

		public static void VisitFacesDepthFirst<TEdge, TDistance>(TEdge rootEdge, ExtendedFaceEdgeVisit<TEdge, TDistance>.Delegate visitDelegate) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge, TDistance>.Visit(rootEdge, visitDelegate, new SortedOrderQueue<ExtendedFaceEdgeVisitation<TEdge, TDistance>>(ExtendedFaceEdgeVisitation<TEdge, TDistance>.AreOrderedDepthFirst));
		}

		public static void VisitFacesDepthFirst<TEdge, TDistance>(IEnumerable<TEdge> rootEdges, ExtendedFaceEdgeVisit<TEdge, TDistance>.Delegate visitDelegate) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge, TDistance>.Visit(rootEdges, visitDelegate, new SortedOrderQueue<ExtendedFaceEdgeVisitation<TEdge, TDistance>>(ExtendedFaceEdgeVisitation<TEdge, TDistance>.AreOrderedDepthFirst));
		}

		public static void VisitFacesDepthFirst<TEdge, TDistance>(Topology.Face rootFace, ExtendedFaceEdgeVisit<TEdge, TDistance>.Delegate visitDelegate) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge, TDistance>.Visit(rootFace, visitDelegate, new SortedOrderQueue<ExtendedFaceEdgeVisitation<TEdge, TDistance>>(ExtendedFaceEdgeVisitation<TEdge, TDistance>.AreOrderedDepthFirst));
		}

		public static void VisitFacesDepthFirst<TEdge, TDistance>(IEnumerable<Topology.Face> rootFaces, ExtendedFaceEdgeVisit<TEdge, TDistance>.Delegate visitDelegate) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge, TDistance>.Visit(rootFaces, visitDelegate, new SortedOrderQueue<ExtendedFaceEdgeVisitation<TEdge, TDistance>>(ExtendedFaceEdgeVisitation<TEdge, TDistance>.AreOrderedDepthFirst));
		}

		#endregion

		#region Distance-Sorted Order Visitation

		public static void VisitFacesSortedByDistance<TDistance>(Topology.Face rootFace, FaceVisit<TDistance>.Delegate visitDelegate, FaceVisit<TDistance>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceVisitation<TDistance>.Visit(rootFace, visitDelegate, new SortedOrderQueue<FaceVisitation<TDistance>>(
				(FaceVisitation<TDistance> lhs, FaceVisitation<TDistance> rhs) => areOrderedDelegate(lhs.distance, rhs.distance)));
		}

		public static void VisitFacesSortedByDistance<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceVisit<TDistance>.Delegate visitDelegate, FaceVisit<TDistance>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceVisitation<TDistance>.Visit(rootFaces, visitDelegate, new SortedOrderQueue<FaceVisitation<TDistance>>(
				(FaceVisitation<TDistance> lhs, FaceVisitation<TDistance> rhs) => areOrderedDelegate(lhs.distance, rhs.distance)));
		}

		public static void VisitFacesSortedByDistance<TDistance>(Topology.FaceEdge rootEdge, FaceEdgeVisit<TDistance>.Delegate visitDelegate, FaceEdgeVisit<TDistance>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceEdgeVisitation<TDistance>.Visit(rootEdge, visitDelegate, new SortedOrderQueue<FaceEdgeVisitation<TDistance>>(
				(FaceEdgeVisitation<TDistance> lhs, FaceEdgeVisitation<TDistance> rhs) => areOrderedDelegate(lhs.distance, rhs.distance)));
		}

		public static void VisitFacesSortedByDistance<TDistance>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisit<TDistance>.Delegate visitDelegate, FaceEdgeVisit<TDistance>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceEdgeVisitation<TDistance>.Visit(rootEdges, visitDelegate, new SortedOrderQueue<FaceEdgeVisitation<TDistance>>(
				(FaceEdgeVisitation<TDistance> lhs, FaceEdgeVisitation<TDistance> rhs) => areOrderedDelegate(lhs.distance, rhs.distance)));
		}

		public static void VisitFacesSortedByDistance<TDistance>(Topology.Face rootFace, FaceEdgeVisit<TDistance>.Delegate visitDelegate, FaceEdgeVisit<TDistance>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceEdgeVisitation<TDistance>.Visit(rootFace, visitDelegate, new SortedOrderQueue<FaceEdgeVisitation<TDistance>>(
				(FaceEdgeVisitation<TDistance> lhs, FaceEdgeVisitation<TDistance> rhs) => areOrderedDelegate(lhs.distance, rhs.distance)));
		}

		public static void VisitFacesSortedByDistance<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceEdgeVisit<TDistance>.Delegate visitDelegate, FaceEdgeVisit<TDistance>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceEdgeVisitation<TDistance>.Visit(rootFaces, visitDelegate, new SortedOrderQueue<FaceEdgeVisitation<TDistance>>(
				(FaceEdgeVisitation<TDistance> lhs, FaceEdgeVisitation<TDistance> rhs) => areOrderedDelegate(lhs.distance, rhs.distance)));
		}

		public static void VisitFacesSortedByDistance<TEdge, TDistance>(TEdge rootEdge, ExtendedFaceEdgeVisit<TEdge, TDistance>.Delegate visitDelegate, ExtendedFaceEdgeVisit<TEdge, TDistance>.AreOrderedDelegate areOrderedDelegate) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge, TDistance>.Visit(rootEdge, visitDelegate, new SortedOrderQueue<ExtendedFaceEdgeVisitation<TEdge, TDistance>>(
				(ExtendedFaceEdgeVisitation<TEdge, TDistance> lhs, ExtendedFaceEdgeVisitation<TEdge, TDistance> rhs) => areOrderedDelegate(lhs.distance, rhs.distance)));
		}

		public static void VisitFacesSortedByDistance<TEdge, TDistance>(IEnumerable<TEdge> rootEdges, ExtendedFaceEdgeVisit<TEdge, TDistance>.Delegate visitDelegate, ExtendedFaceEdgeVisit<TEdge, TDistance>.AreOrderedDelegate areOrderedDelegate) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge, TDistance>.Visit(rootEdges, visitDelegate, new SortedOrderQueue<ExtendedFaceEdgeVisitation<TEdge, TDistance>>(
				(ExtendedFaceEdgeVisitation<TEdge, TDistance> lhs, ExtendedFaceEdgeVisitation<TEdge, TDistance> rhs) => areOrderedDelegate(lhs.distance, rhs.distance)));
		}

		public static void VisitFacesSortedByDistance<TEdge, TDistance>(Topology.Face rootFace, ExtendedFaceEdgeVisit<TEdge, TDistance>.Delegate visitDelegate, ExtendedFaceEdgeVisit<TEdge, TDistance>.AreOrderedDelegate areOrderedDelegate) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge, TDistance>.Visit(rootFace, visitDelegate, new SortedOrderQueue<ExtendedFaceEdgeVisitation<TEdge, TDistance>>(
				(ExtendedFaceEdgeVisitation<TEdge, TDistance> lhs, ExtendedFaceEdgeVisitation<TEdge, TDistance> rhs) => areOrderedDelegate(lhs.distance, rhs.distance)));
		}

		public static void VisitFacesSortedByDistance<TEdge, TDistance>(IEnumerable<Topology.Face> rootFaces, ExtendedFaceEdgeVisit<TEdge, TDistance>.Delegate visitDelegate, ExtendedFaceEdgeVisit<TEdge, TDistance>.AreOrderedDelegate areOrderedDelegate) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge, TDistance>.Visit(rootFaces, visitDelegate, new SortedOrderQueue<ExtendedFaceEdgeVisitation<TEdge, TDistance>>(
				(ExtendedFaceEdgeVisitation<TEdge, TDistance> lhs, ExtendedFaceEdgeVisitation<TEdge, TDistance> rhs) => areOrderedDelegate(lhs.distance, rhs.distance)));
		}

		#endregion

		#region Random Order Visitation

		public static void VisitFacesInRandomOrder(Topology.Face rootFace, FaceVisit.Delegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceVisitation.Visit(rootFace, visitDelegate, new RandomOrderQueue<FaceVisitation>(randomEngine));
		}

		public static void VisitFacesInRandomOrder(IEnumerable<Topology.Face> rootFaces, FaceVisit.Delegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceVisitation.Visit(rootFaces, visitDelegate, new RandomOrderQueue<FaceVisitation>(randomEngine));
		}

		public static void VisitFacesInRandomOrder<TDistance>(Topology.Face rootFace, FaceVisit<TDistance>.Delegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceVisitation<TDistance>.Visit(rootFace, visitDelegate, new RandomOrderQueue<FaceVisitation<TDistance>>(randomEngine));
		}

		public static void VisitFacesInRandomOrder<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceVisit<TDistance>.Delegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceVisitation<TDistance>.Visit(rootFaces, visitDelegate, new RandomOrderQueue<FaceVisitation<TDistance>>(randomEngine));
		}

		public static void VisitFacesInRandomOrder(Topology.FaceEdge rootEdge, FaceEdgeVisit.Delegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceEdgeVisitation.Visit(rootEdge, visitDelegate, new RandomOrderQueue<FaceEdgeVisitation>(randomEngine));
		}

		public static void VisitFacesInRandomOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisit.Delegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceEdgeVisitation.Visit(rootEdges, visitDelegate, new RandomOrderQueue<FaceEdgeVisitation>(randomEngine));
		}

		public static void VisitFacesInRandomOrder(Topology.Face rootFace, FaceEdgeVisit.Delegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceEdgeVisitation.Visit(rootFace, visitDelegate, new RandomOrderQueue<FaceEdgeVisitation>(randomEngine));
		}

		public static void VisitFacesInRandomOrder(IEnumerable<Topology.Face> rootFaces, FaceEdgeVisit.Delegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceEdgeVisitation.Visit(rootFaces, visitDelegate, new RandomOrderQueue<FaceEdgeVisitation>(randomEngine));
		}

		public static void VisitFacesInRandomOrder<TDistance>(Topology.FaceEdge rootEdge, FaceEdgeVisit<TDistance>.Delegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceEdgeVisitation<TDistance>.Visit(rootEdge, visitDelegate, new RandomOrderQueue<FaceEdgeVisitation<TDistance>>(randomEngine));
		}

		public static void VisitFacesInRandomOrder<TDistance>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisit<TDistance>.Delegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceEdgeVisitation<TDistance>.Visit(rootEdges, visitDelegate, new RandomOrderQueue<FaceEdgeVisitation<TDistance>>(randomEngine));
		}

		public static void VisitFacesInRandomOrder<TDistance>(Topology.Face rootFace, FaceEdgeVisit<TDistance>.Delegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceEdgeVisitation<TDistance>.Visit(rootFace, visitDelegate, new RandomOrderQueue<FaceEdgeVisitation<TDistance>>(randomEngine));
		}

		public static void VisitFacesInRandomOrder<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceEdgeVisit<TDistance>.Delegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceEdgeVisitation<TDistance>.Visit(rootFaces, visitDelegate, new RandomOrderQueue<FaceEdgeVisitation<TDistance>>(randomEngine));
		}

		public static void VisitFacesInRandomOrder<TEdge>(TEdge rootEdge, ExtendedFaceEdgeVisit<TEdge>.Delegate visitDelegate, IRandomEngine randomEngine) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge>.Visit(rootEdge, visitDelegate, new RandomOrderQueue<ExtendedFaceEdgeVisitation<TEdge>>(randomEngine));
		}

		public static void VisitFacesInRandomOrder<TEdge>(IEnumerable<TEdge> rootEdges, ExtendedFaceEdgeVisit<TEdge>.Delegate visitDelegate, IRandomEngine randomEngine) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge>.Visit(rootEdges, visitDelegate, new RandomOrderQueue<ExtendedFaceEdgeVisitation<TEdge>>(randomEngine));
		}

		public static void VisitFacesInRandomOrder<TEdge>(Topology.Face rootFace, ExtendedFaceEdgeVisit<TEdge>.Delegate visitDelegate, IRandomEngine randomEngine) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge>.Visit(rootFace, visitDelegate, new RandomOrderQueue<ExtendedFaceEdgeVisitation<TEdge>>(randomEngine));
		}

		public static void VisitFacesInRandomOrder<TEdge>(IEnumerable<Topology.Face> rootFaces, ExtendedFaceEdgeVisit<TEdge>.Delegate visitDelegate, IRandomEngine randomEngine) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge>.Visit(rootFaces, visitDelegate, new RandomOrderQueue<ExtendedFaceEdgeVisitation<TEdge>>(randomEngine));
		}

		public static void VisitFacesInRandomOrder<TEdge, TDistance>(TEdge rootEdge, ExtendedFaceEdgeVisit<TEdge, TDistance>.Delegate visitDelegate, IRandomEngine randomEngine) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge, TDistance>.Visit(rootEdge, visitDelegate, new RandomOrderQueue<ExtendedFaceEdgeVisitation<TEdge, TDistance>>(randomEngine));
		}

		public static void VisitFacesInRandomOrder<TEdge, TDistance>(IEnumerable<TEdge> rootEdges, ExtendedFaceEdgeVisit<TEdge, TDistance>.Delegate visitDelegate, IRandomEngine randomEngine) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge, TDistance>.Visit(rootEdges, visitDelegate, new RandomOrderQueue<ExtendedFaceEdgeVisitation<TEdge, TDistance>>(randomEngine));
		}

		public static void VisitFacesInRandomOrder<TEdge, TDistance>(Topology.Face rootFace, ExtendedFaceEdgeVisit<TEdge, TDistance>.Delegate visitDelegate, IRandomEngine randomEngine) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge, TDistance>.Visit(rootFace, visitDelegate, new RandomOrderQueue<ExtendedFaceEdgeVisitation<TEdge, TDistance>>(randomEngine));
		}

		public static void VisitFacesInRandomOrder<TEdge, TDistance>(IEnumerable<Topology.Face> rootFaces, ExtendedFaceEdgeVisit<TEdge, TDistance>.Delegate visitDelegate, IRandomEngine randomEngine) where TEdge : Topology.IExtendedFaceEdge
		{
			ExtendedFaceEdgeVisitation<TEdge, TDistance>.Visit(rootFaces, visitDelegate, new RandomOrderQueue<ExtendedFaceEdgeVisitation<TEdge, TDistance>>(randomEngine));
		}

		#endregion


#if false



		public static void VisitFacesBreadthFirst(Topology.Face rootFace, FaceVisitDelegate visitDelegate)
		{
			var topology = rootFace.topology;

			var queue = new PriorityQueue<QueueItem>((QueueItem lhs, QueueItem rhs) => lhs.depth <= rhs.depth, 0);
			var visitedFaces = new BitArray(topology.faces.Count);

			queue.Push(new QueueItem(rootFace.index, 0));

			var visitController = new FaceVisit.Controller();

			while (queue.Count > 0)
			{
				var queueItem = queue.front;
				queue.Pop();
				
				if (visitedFaces[queueItem.faceIndex] == true)
				{
					continue;
				}

				var neighbors = visitDelegate(visitController.Set(new Topology.Face(topology, queueItem.faceIndex), queueItem.depth));

				if (visitController.shouldBreak)
				{
					return;
				}

				if (!visitController.shouldIgnore)
				{
					visitedFaces[queueItem.faceIndex] = true;
				}

				var neighborDepth = queueItem.depth + 1;
				foreach (Topology.Face neighbor in neighbors)
				{
					if (visitedFaces[neighbor.index] == false)
					{
						queue.Push(new QueueItem(neighbor.index, neighborDepth));
					}
				}
			}
		}

		public static void VisitFacesDepthFirst(Topology.Face rootFace, FaceVisitDelegate visitDelegate)
		{
		}

		public static void VisitFacesRandomOrder(Topology.Face rootFace, IRandomEngine randomEngine, FaceVisitDelegate visitDelegate)
		{
			var topology = rootFace.topology;

			var queue = new List<QueueItem>();
			var visitedFaces = new BitArray(topology.faces.Count);

			queue.Add(new QueueItem(rootFace.index, 0));

			var visitController = new FaceVisit.Controller();

			while (queue.Count > 0)
			{
				var queueIndex = RandomUtility.HalfOpenRange(queue.Count, randomEngine);
				var queueItem = queue[queueIndex];
				var backIndex = queue.Count - 1;
				queue[queueIndex] = queue[backIndex];
				queue.RemoveAt(backIndex);

				if (visitedFaces[queueItem.faceIndex] == true)
				{
					continue;
				}

				var neighbors = visitDelegate(visitController.Set(new Topology.Face(topology, queueItem.faceIndex), queueItem.depth));

				if (visitController.shouldBreak)
				{
					return;
				}

				if (!visitController.shouldIgnore)
				{
					visitedFaces[queueItem.faceIndex] = true;
				}

				var neighborDepth = queueItem.depth + 1;
				foreach (Topology.Face neighbor in neighbors)
				{
					if (visitedFaces[neighbor.index] == false)
					{
						queue.Add(new QueueItem(neighbor.index, neighborDepth));
					}
				}
			}
		}


#region Core

		public enum VisitationState
		{
			Continue,
			Ignore,
			Stop,
		}

		private delegate void VisitorDelegate(int edgeIndex, ref VisitationState state);

		private abstract class Queue<TVisit>
		{
			public delegate IEnumerator<TVisit> GetFaceNeighborsEnumeratorDelegate(TVisit visitObject);

			public Topology topology;
			private BitArray _visitedFaces;
			private NeighborUtility.GetFaceNeighborFaceEnumeratorDelegate _getNeighbors;

			protected Queue(NeighborUtility.GetFaceNeighborFaceEnumeratorDelegate getNeighbors)
			{
				_getNeighbors = getNeighbors;
			}

			public abstract void Push(TVisit visitedObject);
			public abstract bool Pop(out TVisit visitedObject);

			public Topology PushRoots(Topology.Face rootFace)
			{
				topology = rootFace.topology;
				_visitedFaces = new BitArray(topology.faces.Count);
				_visitedFaces[rootFace.index] = true;

				foreach (var neighbor in getNeighbors(rootFace))
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

			public void Visit(NeighborUtility.GetFaceNeighborFaceEnumeratorDelegate getNeighbors, VisitorDelegate visitor)
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
								foreach (var nextEdge in getNeighbors(Topology.Face(topology, farFaceIndex))
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

#region Any Order Connected Visitation

		public delegate void VisitFaceEdgeDelegate(Topology.FaceEdge faceEdge);
		public delegate void VisitFaceDelegate(Topology.Face face);

		public delegate void VisitFaceEdgeWithStateDelegate(Topology.FaceEdge faceEdge, ref VisitationState state);
		public delegate void VisitFaceWithStateDelegate(Topology.Face face, ref VisitationState state);

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

		private static void VisitConnectedInAnyOrder(ConnectedQueue queue, Topology topology, NeighborUtility.GetFaceNeighborFaceEnumeratorDelegate getNeighbors, VisitFaceEdgeDelegate visitor)
		{
			queue.Visit(getNeighbors, (int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex));
			});
		}

		private static void VisitConnectedInAnyOrder(ConnectedQueue queue, Topology topology, VisitFaceDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex).farFace);
			});
		}

		private static void VisitConnectedInAnyOrder(ConnectedQueue queue, Topology topology, VisitFaceEdgeWithStateDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex), ref state);
			});
		}

		private static void VisitConnectedInAnyOrder(ConnectedQueue queue, Topology topology, VisitFaceWithStateDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex).farFace, ref state);
			});
		}

#endregion

#region Visit From Single Root

		public static void VisitConnectedInAnyOrder(Topology.Face rootFace, VisitFaceEdgeDelegate visitor)
		{
			VisitConnectedInAnyOrder(rootFace, NeighborUtility.AdjacentFaces, visitor);
		}

		public static void VisitConnectedInAnyOrder(Topology.Face rootFace, NeighborUtility.GetFaceNeighborFaceEnumeratorDelegate getNeighbors, VisitFaceEdgeDelegate visitor)
		{
			var queue = new ConnectedQueue();
			VisitConnectedInAnyOrder(queue, queue.PushRoots(rootFace), getNeighbors, visitor);
		}

		public static void VisitConnectedInAnyOrder(Topology.Face rootFace, VisitFaceDelegate visitor)
		{
			var queue = new ConnectedQueue();
			VisitConnectedInAnyOrder(queue, queue.PushRoots(rootFace), visitor);
		}

		public static void VisitConnectedInAnyOrder(Topology.Face rootFace, VisitFaceEdgeWithStateDelegate visitor)
		{
			var queue = new ConnectedQueue();
			VisitConnectedInAnyOrder(queue, queue.PushRoots(rootFace), visitor);
		}

		public static void VisitConnectedInAnyOrder(Topology.Face rootFace, VisitFaceWithStateDelegate visitor)
		{
			var queue = new ConnectedQueue();
			VisitConnectedInAnyOrder(queue, queue.PushRoots(rootFace), visitor);
		}

#endregion

#region Visit From Multiple Roots

		public static void VisitConnectedInAnyOrder(IEnumerable<Topology.Face> rootFaces, VisitFaceEdgeDelegate visitor)
		{
			var queue = new ConnectedQueue();
			VisitConnectedInAnyOrder(queue, queue.PushRoots(rootFaces), visitor);
		}

		public static void VisitConnectedInAnyOrder(IEnumerable<Topology.Face> rootFaces, VisitFaceDelegate visitor)
		{
			var queue = new ConnectedQueue();
			VisitConnectedInAnyOrder(queue, queue.PushRoots(rootFaces), visitor);
		}

		public static void VisitConnectedInAnyOrder(IEnumerable<Topology.Face> rootFaces, VisitFaceEdgeWithStateDelegate visitor)
		{
			var queue = new ConnectedQueue();
			VisitConnectedInAnyOrder(queue, queue.PushRoots(rootFaces), visitor);
		}

		public static void VisitConnectedInAnyOrder(IEnumerable<Topology.Face> rootFaces, VisitFaceWithStateDelegate visitor)
		{
			var queue = new ConnectedQueue();
			VisitConnectedInAnyOrder(queue, queue.PushRoots(rootFaces), visitor);
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

		private static void VisitConnectedInRandomOrder(RandomConnectedQueue queue, Topology topology, VisitFaceEdgeDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex));
			});
		}

		private static void VisitConnectedInRandomOrder(RandomConnectedQueue queue, Topology topology, VisitFaceDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex).farFace);
			});
		}

		private static void VisitConnectedInRandomOrder(RandomConnectedQueue queue, Topology topology, VisitFaceEdgeWithStateDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex), ref state);
			});
		}

		private static void VisitConnectedInRandomOrder(RandomConnectedQueue queue, Topology topology, VisitFaceWithStateDelegate visitor)
		{
			queue.Visit((int edgeIndex, ref VisitationState state) =>
			{
				visitor(new Topology.FaceEdge(topology, edgeIndex).farFace, ref state);
			});
		}

#endregion

#region Visit From Single Root

		public static void VisitConnectedInRandomOrder(Topology.Face rootFace, IRandomEngine randomEngine, VisitFaceEdgeDelegate visitor)
		{
			var queue = new RandomConnectedQueue(randomEngine);
			VisitConnectedInRandomOrder(queue, queue.PushRoots(rootFace), visitor);
		}

		public static void VisitConnectedInRandomOrder(Topology.Face rootFace, IRandomEngine randomEngine, VisitFaceDelegate visitor)
		{
			var queue = new RandomConnectedQueue(randomEngine);
			VisitConnectedInRandomOrder(queue, queue.PushRoots(rootFace), visitor);
		}

		public static void VisitConnectedInRandomOrder(Topology.Face rootFace, IRandomEngine randomEngine, VisitFaceEdgeWithStateDelegate visitor)
		{
			var queue = new RandomConnectedQueue(randomEngine);
			VisitConnectedInRandomOrder(queue, queue.PushRoots(rootFace), visitor);
		}

		public static void VisitConnectedInRandomOrder(Topology.Face rootFace, IRandomEngine randomEngine, VisitFaceWithStateDelegate visitor)
		{
			var queue = new RandomConnectedQueue(randomEngine);
			VisitConnectedInRandomOrder(queue, queue.PushRoots(rootFace), visitor);
		}

#endregion

#region Visit From Multiple Roots

		public static void VisitConnectedInRandomOrder(IEnumerable<Topology.Face> rootFaces, IRandomEngine randomEngine, VisitFaceEdgeDelegate visitor)
		{
			var queue = new RandomConnectedQueue(randomEngine);
			VisitConnectedInRandomOrder(queue, queue.PushRoots(rootFaces), visitor);
		}

		public static void VisitConnectedInRandomOrder(IEnumerable<Topology.Face> rootFaces, IRandomEngine randomEngine, VisitFaceDelegate visitor)
		{
			var queue = new RandomConnectedQueue(randomEngine);
			VisitConnectedInRandomOrder(queue, queue.PushRoots(rootFaces), visitor);
		}

		public static void VisitConnectedInRandomOrder(IEnumerable<Topology.Face> rootFaces, IRandomEngine randomEngine, VisitFaceEdgeWithStateDelegate visitor)
		{
			var queue = new RandomConnectedQueue(randomEngine);
			VisitConnectedInRandomOrder(queue, queue.PushRoots(rootFaces), visitor);
		}

		public static void VisitConnectedInRandomOrder(IEnumerable<Topology.Face> rootFaces, IRandomEngine randomEngine, VisitFaceWithStateDelegate visitor)
		{
			var queue = new RandomConnectedQueue(randomEngine);
			VisitConnectedInRandomOrder(queue, queue.PushRoots(rootFaces), visitor);
		}

#endregion

#endregion
#endif
	}
}
