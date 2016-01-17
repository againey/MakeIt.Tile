using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public class DistanceOrderedFaceVisitor
	{
		public delegate float FaceDistanceCalculatorDelegate(Topology.FaceEdge edge, float priorDistance);
		public delegate bool DistanceComparerDelegate(float lhs, float rhs);

		public struct EdgeDistancePair
		{
			private Topology.FaceEdge _edge;
			private float _distance;

			public EdgeDistancePair(Topology.FaceEdge edge, float distance)
			{
				_edge = edge;
				_distance = distance;
			}

			public Topology.Face face { get { return _edge.farFace; } }
			public Topology.Face priorFace { get { return _edge.nearFace; } }
			public Topology.FaceEdge edge { get { return _edge; } }
			public float distance { get { return _distance; } }
		}

		private readonly Topology _topology;
		private readonly Topology.Face _root;
		private readonly FaceDistanceCalculatorDelegate _faceDistanceCalculator;

		public DistanceOrderedFaceVisitor(Topology topology, Topology.Face root, FaceDistanceCalculatorDelegate faceDistanceCalculator)
		{
			_topology = topology;
			_root = root;
			_faceDistanceCalculator = faceDistanceCalculator;
		}

		protected static FaceDistanceCalculatorDelegate GetFaceDistanceCalculatorBasedOnCumulativeEuclideanDistance(Vector3[] faceCentroids)
		{
			return (Topology.FaceEdge edge, float priorDistance) =>
			{
				return priorDistance + (faceCentroids[edge.nearFace] - faceCentroids[edge.farFace]).magnitude;
			};
		}

		protected static FaceDistanceCalculatorDelegate GetFaceDistanceCalculatorBasedOnRootEuclideanDistance(Topology.Face root, Vector3[] faceCentroids)
		{
			return (Topology.FaceEdge edge, float priorDistance) =>
			{
				return (faceCentroids[root] - faceCentroids[edge.farFace]).magnitude;
			};
		}

		protected static FaceDistanceCalculatorDelegate GetFaceDistanceCalculatorBasedOnCumulativeSphericalDistance(Vector3[] faceCentroids)
		{
			return (Topology.FaceEdge edge, float priorDistance) =>
			{
				return priorDistance + MathUtility.AngleBetweenUnitVectors(faceCentroids[edge.nearFace], faceCentroids[edge.farFace]);
			};
		}

		protected static FaceDistanceCalculatorDelegate GetFaceDistanceCalculatorBasedOnCumulativeSphericalDistance(Vector3[] faceCentroids, float sphereRadius)
		{
			return (Topology.FaceEdge edge, float priorDistance) =>
			{
				return priorDistance + MathUtility.SphericalArcLength(faceCentroids[edge.nearFace], faceCentroids[edge.farFace], sphereRadius);
			};
		}

		protected static FaceDistanceCalculatorDelegate GetFaceDistanceCalculatorBasedOnRootSphericalDistance(Topology.Face root, Vector3[] faceCentroids)
		{
			return (Topology.FaceEdge edge, float priorDistance) =>
			{
				return MathUtility.AngleBetweenUnitVectors(faceCentroids[root], faceCentroids[edge.farFace]);
			};
		}

		protected static FaceDistanceCalculatorDelegate GetFaceDistanceCalculatorBasedOnRootSphericalDistance(Topology.Face root, Vector3[] faceCentroids, float sphereRadius)
		{
			return (Topology.FaceEdge edge, float priorDistance) =>
			{
				return MathUtility.SphericalArcLength(faceCentroids[root], faceCentroids[edge.farFace], sphereRadius);
			};
		}

		protected static DistanceComparerDelegate breadthFirstComparer
		{
			get
			{
				return (float lhs, float rhs) => { return lhs < rhs; };
			}
		}

		protected static DistanceComparerDelegate depthFirstComparer
		{
			get
			{
				return (float lhs, float rhs) => { return lhs > rhs; };
			}
		}

		public IEnumerator<EdgeDistancePair> GetBreadthFirstEnumerator()
		{
			return new FaceEdgeDistanceEnumerator(this, breadthFirstComparer);
		}

		public IEnumerator<EdgeDistancePair> GetDepthFirstEnumerator()
		{
			return new FaceEdgeDistanceEnumerator(this, depthFirstComparer);
		}

		public class FaceEnumerator : FaceEdgeEnumerator, IEnumerator<Topology.Face>
		{
			public FaceEnumerator(DistanceOrderedFaceVisitor visitor, DistanceComparerDelegate distanceComparer)
				: base(visitor, distanceComparer)
			{
			}

			public new Topology.Face Current
			{
				get
				{
					return base.Current.farFace;
				}
			}

			object IEnumerator.Current
			{
				get
				{
					return base.Current.farFace;
				}
			}
		}

		public class FaceEdgeEnumerator : FaceEdgeDistanceEnumerator, IEnumerator<Topology.FaceEdge>
		{
			public FaceEdgeEnumerator(DistanceOrderedFaceVisitor visitor, DistanceComparerDelegate distanceComparer)
				: base(visitor, distanceComparer)
			{
			}

			public new Topology.FaceEdge Current
			{
				get
				{
					return base.Current.edge;
				}
			}

			object IEnumerator.Current
			{
				get
				{
					return base.Current.edge;
				}
			}
		}

		public class FaceEdgeDistanceEnumerator : IEnumerator<EdgeDistancePair>
		{
			private struct EdgeIndexDistancePair : IEquatable<EdgeIndexDistancePair>
			{
				public int _edgeIndex;
				public float _distance;

				public EdgeIndexDistancePair(int edgeIndex, float distance)
				{
					_edgeIndex = edgeIndex;
					_distance = distance;
				}

				public bool Equals(EdgeIndexDistancePair other)
				{
					return _edgeIndex == other._edgeIndex;
				}
			}

			private readonly Topology _topology;
			private readonly FaceDistanceCalculatorDelegate _faceDistanceCalculator;
			private readonly PriorityQueue<EdgeIndexDistancePair> _queuedEdgeIndices;
			private readonly BitArray _visitedFaces;
			private EdgeDistancePair _current;

			public FaceEdgeDistanceEnumerator(DistanceOrderedFaceVisitor visitor, DistanceComparerDelegate distanceComparer)
			{
				_topology = visitor._topology;
				_faceDistanceCalculator = visitor._faceDistanceCalculator;
				_queuedEdgeIndices = new PriorityQueue<EdgeIndexDistancePair>(
					(EdgeIndexDistancePair lhs, EdgeIndexDistancePair rhs) => { return distanceComparer(lhs._distance, rhs._distance); },
					Mathf.CeilToInt(Mathf.Sqrt(_topology.internalFaces.Count)));
				_visitedFaces = new BitArray(_topology.internalFaces.Count);

				_visitedFaces[visitor._root.index] = true;
				foreach (var edge in visitor._root.edges)
				{
					if (!edge.isOuterBoundary)
					{
						var farFaceIndex = edge.farFace.index;
						if (_visitedFaces[farFaceIndex] == false)
						{
							_queuedEdgeIndices.Push(new EdgeIndexDistancePair(edge.index, _faceDistanceCalculator(edge, 0f)));
						}
					}
				}
			}

			public EdgeDistancePair Current
			{
				get
				{
					return _current;
				}
			}

			object IEnumerator.Current
			{
				get
				{
					return _current;
				}
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				while (_queuedEdgeIndices.Count > 0)
				{
					var front = _queuedEdgeIndices.front;
					_queuedEdgeIndices.Pop();

					var edge = _topology.faceEdges[front._edgeIndex];
					var faceIndex = edge.farFace.index;

					if (_visitedFaces[faceIndex] == false)
					{
						_visitedFaces[faceIndex] = true;
						_current = new EdgeDistancePair(edge, front._distance);
						foreach (var faceEdge in edge.farFace.edges)
						{
							if (faceEdge.farFace.isInternal)
							{
								if (_visitedFaces[faceEdge.farFace.index] == false)
								{
									_queuedEdgeIndices.Push(new EdgeIndexDistancePair(faceEdge.index, _faceDistanceCalculator(faceEdge, front._distance)));
								}
							}
						}
						return true;
					}
				}
				return false;
			}

			public void Reset()
			{
				throw new NotSupportedException();
			}
		}
	}

	public class BreadthFirstFaceVisitor : DistanceOrderedFaceVisitor, IEnumerable<Topology.Face>, IEnumerable<Topology.FaceEdge>, IEnumerable<DistanceOrderedFaceVisitor.EdgeDistancePair>
	{
		public BreadthFirstFaceVisitor(Topology topology, Topology.Face root, FaceDistanceCalculatorDelegate faceDistanceCalculator)
			: base(topology, root, faceDistanceCalculator)
		{
		}

		public static BreadthFirstFaceVisitor CreateBasedOnCumulativeEuclideanDistance(Topology topology, Topology.Face root, Vector3[] faceCentroids)
		{
			return new BreadthFirstFaceVisitor(topology, root, GetFaceDistanceCalculatorBasedOnCumulativeEuclideanDistance(faceCentroids));
		}

		public static BreadthFirstFaceVisitor CreateBasedOnRootEuclideanDistance(Topology topology, Topology.Face root, Vector3[] faceCentroids)
		{
			return new BreadthFirstFaceVisitor(topology, root, GetFaceDistanceCalculatorBasedOnRootEuclideanDistance(root, faceCentroids));
		}

		public static BreadthFirstFaceVisitor CreateBasedOnCumulativeSphericalDistance(Topology topology, Topology.Face root, Vector3[] faceCentroids)
		{
			return new BreadthFirstFaceVisitor(topology, root, GetFaceDistanceCalculatorBasedOnCumulativeSphericalDistance(faceCentroids));
		}

		public static BreadthFirstFaceVisitor CreateBasedOnRootSphericalDistance(Topology topology, Topology.Face root, Vector3[] faceCentroids)
		{
			return new BreadthFirstFaceVisitor(topology, root, GetFaceDistanceCalculatorBasedOnRootSphericalDistance(root, faceCentroids));
		}

		public IEnumerator<Topology.Face> GetEnumerator()
		{
			return new FaceEnumerator(this, breadthFirstComparer);
		}

		IEnumerator<Topology.FaceEdge> IEnumerable<Topology.FaceEdge>.GetEnumerator()
		{
			return new FaceEdgeEnumerator(this, breadthFirstComparer);
		}

		IEnumerator<EdgeDistancePair> IEnumerable<EdgeDistancePair>.GetEnumerator()
		{
			return new FaceEdgeDistanceEnumerator(this, breadthFirstComparer);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new FaceEnumerator(this, breadthFirstComparer);
		}
	}

	public class DepthFirstFaceVisitor : DistanceOrderedFaceVisitor, IEnumerable<Topology.Face>, IEnumerable<Topology.FaceEdge>, IEnumerable<DistanceOrderedFaceVisitor.EdgeDistancePair>
	{
		public DepthFirstFaceVisitor(Topology topology, Topology.Face root, FaceDistanceCalculatorDelegate faceDistanceCalculator)
			: base(topology, root, faceDistanceCalculator)
		{
		}

		public static DepthFirstFaceVisitor CreateBasedOnCumulativeEuclideanDistance(Topology topology, Topology.Face root, Vector3[] faceCentroids)
		{
			return new DepthFirstFaceVisitor(topology, root, GetFaceDistanceCalculatorBasedOnCumulativeEuclideanDistance(faceCentroids));
		}

		public static DepthFirstFaceVisitor CreateBasedOnRootEuclideanDistance(Topology topology, Topology.Face root, Vector3[] faceCentroids)
		{
			return new DepthFirstFaceVisitor(topology, root, GetFaceDistanceCalculatorBasedOnRootEuclideanDistance(root, faceCentroids));
		}

		public static DepthFirstFaceVisitor CreateBasedOnCumulativeSphericalDistance(Topology topology, Topology.Face root, Vector3[] faceCentroids)
		{
			return new DepthFirstFaceVisitor(topology, root, GetFaceDistanceCalculatorBasedOnCumulativeSphericalDistance(faceCentroids));
		}

		public static DepthFirstFaceVisitor CreateBasedOnRootSphericalDistance(Topology topology, Topology.Face root, Vector3[] faceCentroids)
		{
			return new DepthFirstFaceVisitor(topology, root, GetFaceDistanceCalculatorBasedOnRootSphericalDistance(root, faceCentroids));
		}

		public IEnumerator<Topology.Face> GetEnumerator()
		{
			return new FaceEnumerator(this, depthFirstComparer);
		}

		IEnumerator<Topology.FaceEdge> IEnumerable<Topology.FaceEdge>.GetEnumerator()
		{
			return new FaceEdgeEnumerator(this, depthFirstComparer);
		}

		IEnumerator<EdgeDistancePair> IEnumerable<EdgeDistancePair>.GetEnumerator()
		{
			return new FaceEdgeDistanceEnumerator(this, depthFirstComparer);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new FaceEnumerator(this, depthFirstComparer);
		}
	}
}
