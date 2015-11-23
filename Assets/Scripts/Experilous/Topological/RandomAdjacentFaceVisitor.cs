using System;
using System.Collections;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public class RandomAdjacentFaceVisitor : IEnumerable<Topology.Face>, IEnumerable<Topology.FaceEdge>
	{
		private readonly Topology _topology;
		private List<int> _queuedEdgeIndices;
		private readonly BitArray _visitedFaces;

		public RandomAdjacentFaceVisitor(Topology topology)
		{
			_topology = topology;
			_queuedEdgeIndices = new List<int>();
			_visitedFaces = new BitArray(_topology.faces.Count);
		}

		public void AddSeed(Topology.Vertex vertex)
		{
			foreach (var edge in vertex.edges)
			{
				_visitedFaces[edge.prevFace.index] = true;
			}
			foreach (var edge in vertex.edges)
			{
				foreach (var faceEdge in edge.prevFace.edges)
				{
					var farFaceIndex = faceEdge.farFace.index;
					if (_visitedFaces[farFaceIndex] == false)
					{
						_queuedEdgeIndices.Add(faceEdge.index);
					}
				}
			}
		}

		public void AddSeed(Topology.VertexEdge edge)
		{
			_visitedFaces[edge.prevFace.index] = true;
			_visitedFaces[edge.nextFace.index] = true;
			foreach (var faceEdge in edge.prevFace.edges)
			{
				var farFaceIndex = faceEdge.farFace.index;
				if (_visitedFaces[farFaceIndex] == false)
				{
					_queuedEdgeIndices.Add(faceEdge.index);
				}
			}
			foreach (var faceEdge in edge.nextFace.edges)
			{
				var farFaceIndex = faceEdge.farFace.index;
				if (_visitedFaces[farFaceIndex] == false)
				{
					_queuedEdgeIndices.Add(faceEdge.index);
				}
			}
		}

		public void AddSeed(Topology.FaceEdge edge)
		{
			_visitedFaces[edge.farFace.index] = true;
			_visitedFaces[edge.nearFace.index] = true;
			foreach (var faceEdge in edge.farFace.edges)
			{
				var farFaceIndex = faceEdge.farFace.index;
				if (_visitedFaces[farFaceIndex] == false)
				{
					_queuedEdgeIndices.Add(faceEdge.index);
				}
			}
			foreach (var faceEdge in edge.nearFace.edges)
			{
				var farFaceIndex = faceEdge.farFace.index;
				if (_visitedFaces[farFaceIndex] == false)
				{
					_queuedEdgeIndices.Add(faceEdge.index);
				}
			}
		}

		public void AddSeed(Topology.Face face)
		{
			_visitedFaces[face.index] = true;
			foreach (var edge in face.edges)
			{
				if (!edge.isOuterBoundary)
				{
					var farFaceIndex = edge.farFace.index;
					if (_visitedFaces[farFaceIndex] == false)
					{
						_queuedEdgeIndices.Add(edge.index);
					}
				}
			}
		}

		public IEnumerator<Topology.Face> GetEnumerator()
		{
			return new FaceEnumerator(this);
		}

		IEnumerator<Topology.FaceEdge> IEnumerable<Topology.FaceEdge>.GetEnumerator()
		{
			return new FaceEdgeEnumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new FaceEnumerator(this);
		}

		public class FaceEnumerator : FaceEdgeEnumerator, IEnumerator<Topology.Face>
		{
			public FaceEnumerator(RandomAdjacentFaceVisitor visitor)
				: base(visitor)
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

		public class FaceEdgeEnumerator : IEnumerator<Topology.FaceEdge>
		{
			private readonly Topology _topology;
			private readonly List<int> _queuedEdgeIndices;
			private readonly BitArray _visitedFaces;
			private Topology.FaceEdge _current;
			private Random _random;

			public FaceEdgeEnumerator(RandomAdjacentFaceVisitor visitor)
			{
				_topology = visitor._topology;
				_queuedEdgeIndices = new List<int>(visitor._queuedEdgeIndices);
				_visitedFaces = new BitArray(_topology.faces.Count);
				_random = new Random();
			}

			public Topology.FaceEdge Current
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
					var queueIndex = _random.Next(_queuedEdgeIndices.Count);
					var lastIndex = _queuedEdgeIndices.Count - 1;
					var edgeIndex = _queuedEdgeIndices[queueIndex];
					var edge = _topology.faceEdges[edgeIndex];
					var faceIndex = edge.farFace.index;

					_queuedEdgeIndices[queueIndex] = _queuedEdgeIndices[lastIndex];
					_queuedEdgeIndices.RemoveAt(lastIndex);

					if (_visitedFaces[faceIndex] == false)
					{
						_visitedFaces[faceIndex] = true;
						_current = edge;
						foreach (var faceEdge in edge.farFace.edges)
						{
							if (faceEdge.farFace.isValid)
							{
								if (_visitedFaces[faceEdge.farFace.index] == false)
								{
									_queuedEdgeIndices.Add(faceEdge.index);
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
}
