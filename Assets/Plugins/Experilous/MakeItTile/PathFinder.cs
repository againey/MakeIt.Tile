/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using Experilous.Containers;
using Geometry = Experilous.Numerics.Geometry;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// Basic interface shared by all path instances over any element type.
	/// </summary>
	public interface IPath
	{
		/// <summary>
		/// Indicates if the current instance represents a complete path from
		/// the requested source to the requested target, or if there was no
		/// complete path found between the two.
		/// </summary>
		bool isComplete { get; }
	}

	/// <summary>
	/// Interface for paths over vertices, exposed as a list of vertices.
	/// </summary>
	public interface IVertexPath : IList<Topology.Vertex>, IPath
	{
		/// <summary>
		/// The source vertex at the beginning of the path.
		/// </summary>
		Topology.Vertex source { get; }

		/// <summary>
		/// The target vertex at the end of the path.
		/// </summary>
		Topology.Vertex target { get; }
	}

	/// <summary>
	/// Interface for paths over vertices, exposed as a list of vertex edges.
	/// </summary>
	public interface IVertexEdgePath : IList<Topology.VertexEdge>, IPath
	{
		/// <summary>
		/// The source vertex at the beginning of the path.
		/// </summary>
		Topology.Vertex source { get; }

		/// <summary>
		/// The target vertex at the end of the path.
		/// </summary>
		Topology.Vertex target { get; }

		/// <summary>
		/// Converts the current instance to a simpler vertex path, exposed as a list of vertices.
		/// </summary>
		/// <returns>A vertex path exposed as a list of vertices.</returns>
		IVertexPath AsVertexPath();
	}

	/// <summary>
	/// Interface for paths over faces, exposed as a list of faces.
	/// </summary>
	public interface IFacePath : IList<Topology.Face>, IPath
	{
		/// <summary>
		/// The source face at the beginning of the path.
		/// </summary>
		Topology.Face source { get; }

		/// <summary>
		/// The target face at the end of the path.
		/// </summary>
		Topology.Face target { get; }
	}

	/// <summary>
	/// Interface for paths over faces, exposed as a list of face edges.
	/// </summary>
	public interface IFaceEdgePath : IList<Topology.FaceEdge>, IPath
	{
		/// <summary>
		/// The source face at the beginning of the path.
		/// </summary>
		Topology.Face source { get; }

		/// <summary>
		/// The target face at the end of the path.
		/// </summary>
		Topology.Face target { get; }

		/// <summary>
		/// Converts the current instance to a simpler face path, exposed as a list of faces.
		/// </summary>
		/// <returns>A face path exposed as a list of faces.</returns>
		IFacePath AsFacePath();
	}

	/// <summary>
	/// A class for finding the shortest or otherwise least costly path between pairs of
	/// vertices or faces within a topology, implemented using the A* path finding algorithm.
	/// </summary>
	public class PathFinder
	{
		/// <summary>
		/// Delegate signature for estimating the cost of the path from the specified source
		/// vertex to the specified target vertex.
		/// </summary>
		/// <param name="source">The source vertex at the beginning of the path segment whose cost is to be estimated.</param>
		/// <param name="target">The target vertex at the end of the path segment whose cost is to be estimated.</param>
		/// <param name="pathLength">The length of the overall path so far, from the original source to the source of this call.</param>
		/// <returns>The estimated cost of pathing from the source to the target.</returns>
		public delegate float VertexCostHeuristicDelegate(Topology.Vertex source, Topology.Vertex target, int pathLength);

		/// <summary>
		/// Delegate signature for determining the actual cost of the path along the specified
		/// vertex edge, from its near vertex to its far vertex.
		/// </summary>
		/// <param name="edge">The edge whose path cost is to be determined.</param>
		/// <param name="pathLength">The length of the overall path so far, from the original source to the near vertex of this call's edge.</param>
		/// <returns>The actual cost of pathing along the specified edge.</returns>
		public delegate float VertexCostDelegate(Topology.VertexEdge edge, int pathLength);

		/// <summary>
		/// Delegate signature for estimating the cost of the path from the specified source
		/// face to the specified target face.
		/// </summary>
		/// <param name="source">The source face at the beginning of the path segment whose cost is to be estimated.</param>
		/// <param name="target">The target face at the end of the path segment whose cost is to be estimated.</param>
		/// <param name="pathLength">The length of the overall path so far, from the original source to the source of this call.</param>
		/// <returns>The estimated cost of pathing from the source to the target.</returns>
		public delegate float FaceCostHeuristicDelegate(Topology.Face source, Topology.Face target, int pathLength);

		/// <summary>
		/// Delegate signature for determining the actual cost of the path along the specified
		/// face edge, from its near face to its far face.
		/// </summary>
		/// <param name="edge">The edge whose path cost is to be determined.</param>
		/// <param name="pathLength">The length of the overall path so far, from the original source to the near face of this call's edge.</param>
		/// <returns>The actual cost of pathing along the specified edge.</returns>
		public delegate float FaceCostDelegate(Topology.FaceEdge edge, int pathLength);

		#region Private A* Types and Data

		private DelegateOrderedPriorityQueue<Node> _queue;
		private Dictionary<int, Node> _openSet;
		private Dictionary<int, Node> _closedSet;

		private struct Node : IEquatable<Node>
		{
			public float _f;
			public float _g;
			public float _h;
			public int _elementIndex;
			public int _edgeIndex;
			public int _fromIndex;
			public int _length;

			public Node(int elementIndex, int edgeIndex, int fromIndex)
			{
				_f = 0f;
				_g = 0f;
				_h = 0f;
				_elementIndex = elementIndex;
				_edgeIndex = edgeIndex;
				_fromIndex = fromIndex;
				_length = 0;
			}

			public Node(float f, float g, float h, int elementIndex, int edgeIndex, int fromIndex, int length)
			{
				_f = f;
				_g = g;
				_h = h;
				_elementIndex = elementIndex;
				_edgeIndex = edgeIndex;
				_fromIndex = fromIndex;
				_length = length;
			}

			public static bool AreOrdered(Node lhs, Node rhs)
			{
				return lhs._f <= rhs._f;
			}

			public bool Equals(Node other) { return _elementIndex == other._elementIndex; }
			public override bool Equals(object other) { return other is Node && _elementIndex == ((Node)other)._elementIndex; }
			public static bool operator ==(Node lhs, Node rhs) { return lhs._elementIndex == rhs._elementIndex; }
			public static bool operator !=(Node lhs, Node rhs) { return lhs._elementIndex != rhs._elementIndex; }

			public override int GetHashCode()
			{
				return _elementIndex.GetHashCode();
			}
		}

		#endregion

		#region Path Implementations

		private class EdgePath
		{
			protected Topology _topology;
			protected int _sourceElementIndex;
			protected int _targetElementIndex;
			protected int[] _edgeIndices;
			protected int _length;

			public bool isComplete
			{
				get
				{
					return _length > 0 || _sourceElementIndex == _targetElementIndex;
				}
			}

			public void Rebuild(Topology topology, int sourceElementIndex, int targetElementIndex)
			{
				_topology = topology;
				_sourceElementIndex = sourceElementIndex;
				_targetElementIndex = targetElementIndex;
				_length = 0;
			}

			public void Rebuild(Topology topology, int sourceElementIndex, int targetElementIndex, Node lastNode, Dictionary<int, Node> closedSet)
			{
				_topology = topology;
				_sourceElementIndex = sourceElementIndex;
				_targetElementIndex = targetElementIndex;

				var newLength = lastNode._length;

				if (_length > newLength)
				{
					_length = newLength;
				}
				else if (_length < newLength)
				{
					_edgeIndices = new int[newLength];
					_length = newLength;
				}

				if (newLength > 0)
				{
					var node = lastNode;

					for (int i = newLength - 1; i > 0; --i)
					{
						_edgeIndices[i] = node._edgeIndex;
						node = closedSet[node._fromIndex];
					}

					_edgeIndices[0] = node._edgeIndex;
				}
			}
		}

		private class VertexEdgePath : EdgePath, IVertexEdgePath
		{
			#region IList<Topology.VertexEdge> Implementation

			public bool IsReadOnly { get { return true; } }
			public int Count { get { return _length; } }

			public Topology.VertexEdge this[int index]
			{
				get { return new Topology.VertexEdge(_topology, _edgeIndices[index]); }
				set { throw new NotSupportedException(); }
			}

			public bool Contains(Topology.VertexEdge item)
			{
				return Array.IndexOf(_edgeIndices, item.index) != -1;
			}

			public int IndexOf(Topology.VertexEdge item)
			{
				return Array.IndexOf(_edgeIndices, item.index);
			}

			public void CopyTo(Topology.VertexEdge[] array, int arrayIndex)
			{
				for (int i = 0; i < _length; ++i)
				{
					array[arrayIndex++] = new Topology.VertexEdge(_topology, _edgeIndices[i]);
				}
			}

			public IEnumerator<Topology.VertexEdge> GetEnumerator()
			{
				for (int i = 0; i < _length; ++i)
				{
					yield return new Topology.VertexEdge(_topology, _edgeIndices[i]);
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public void Add(Topology.VertexEdge item) { throw new NotSupportedException(); }
			public void Insert(int index, Topology.VertexEdge item) { throw new NotSupportedException(); }
			public bool Remove(Topology.VertexEdge item) { throw new NotSupportedException(); }
			public void RemoveAt(int index) { throw new NotSupportedException(); }
			public void Clear() { throw new NotSupportedException(); }

			#endregion

			public Topology.Vertex source
			{
				get
				{
					return new Topology.Vertex(_topology, _sourceElementIndex);
				}
			}

			public Topology.Vertex target
			{
				get
				{
					return new Topology.Vertex(_topology, _targetElementIndex);
				}
			}

			private class VertexPathAdapter : IVertexPath
			{
				private VertexEdgePath _vertexEdgePath;

				public VertexPathAdapter(VertexEdgePath vertexEdgePath)
				{
					_vertexEdgePath = vertexEdgePath;
				}

				public bool isComplete { get { return _vertexEdgePath.isComplete; } }

				public Topology.Vertex source { get { return _vertexEdgePath.source; } }

				public Topology.Vertex target { get { return _vertexEdgePath.target; } }

				#region IList<Topology.Vertex> Implementation

				public bool IsReadOnly { get { return true; } }
				public int Count { get { return _vertexEdgePath.isComplete ? _vertexEdgePath._length + 1 : 0; } }

				public Topology.Vertex this[int index]
				{
					get
					{
						var topology = _vertexEdgePath._topology;
						if (index != 0)
						{
							var edgeIndex = _vertexEdgePath._edgeIndices[index - 1];
							var vertexIndex = topology.edgeData[edgeIndex].vertex;
							return new Topology.Vertex(topology, vertexIndex);
						}
						else
						{
							return new Topology.Vertex(topology, _vertexEdgePath._sourceElementIndex);
						}
					}
					set
					{
						throw new NotSupportedException();
					}
				}

				public bool Contains(Topology.Vertex item)
				{
					return IndexOf(item) != -1;
				}

				public int IndexOf(Topology.Vertex item)
				{
					if (!_vertexEdgePath.isComplete) return -1;
					var itemIndex = item.index;
					if (itemIndex == _vertexEdgePath._sourceElementIndex) return 0;
					var length = _vertexEdgePath._length;
					var edgeIndices = _vertexEdgePath._edgeIndices;
					var edgeData = _vertexEdgePath._topology.edgeData;
					for (int i = 0; i < length; ++i)
					{
						if (edgeData[edgeIndices[i]].vertex == itemIndex) return i + 1;
					}
					return -1;
				}

				public void CopyTo(Topology.Vertex[] array, int arrayIndex)
				{
					if (!_vertexEdgePath.isComplete) return;
					var topology = _vertexEdgePath._topology;
					array[arrayIndex++] = new Topology.Vertex(topology, _vertexEdgePath._sourceElementIndex);
					var length = _vertexEdgePath._length;
					var edgeIndices = _vertexEdgePath._edgeIndices;
					var edgeData = _vertexEdgePath._topology.edgeData;
					for (int i = 0; i < length; ++i)
					{
						array[arrayIndex++] = new Topology.Vertex(topology, edgeData[edgeIndices[i]].vertex);
					}
				}

				public IEnumerator<Topology.Vertex> GetEnumerator()
				{
					if (!_vertexEdgePath.isComplete) yield break;
					var topology = _vertexEdgePath._topology;
					yield return new Topology.Vertex(topology, _vertexEdgePath._sourceElementIndex);
					var length = _vertexEdgePath._length;
					var edgeIndices = _vertexEdgePath._edgeIndices;
					var edgeData = _vertexEdgePath._topology.edgeData;
					for (int i = 0; i < length; ++i)
					{
						yield return new Topology.Vertex(topology, edgeData[edgeIndices[i]].vertex);
					}
				}

				IEnumerator IEnumerable.GetEnumerator()
				{
					return GetEnumerator();
				}

				public void Add(Topology.Vertex item) { throw new NotSupportedException(); }
				public void Insert(int index, Topology.Vertex item) { throw new NotSupportedException(); }
				public bool Remove(Topology.Vertex item) { throw new NotSupportedException(); }
				public void RemoveAt(int index) { throw new NotSupportedException(); }
				public void Clear() { throw new NotSupportedException(); }

				#endregion
			}

			public IVertexPath AsVertexPath()
			{
				return new VertexPathAdapter(this);
			}

			public VertexEdgePath Rebuild(Topology.Vertex source, Topology.Vertex target)
			{
				Rebuild(source.topology, source.index, target.index);
				return this;
			}

			public VertexEdgePath Rebuild(Topology.Vertex source, Topology.Vertex target, Node lastNode, Dictionary<int, Node> closedSet)
			{
				Rebuild(source.topology, source.index, target.index, lastNode, closedSet);
				return this;
			}
		}

		private class FaceEdgePath : EdgePath, IFaceEdgePath
		{
			#region IList<Topology.FaceEdge> Implementation

			public bool IsReadOnly { get { return true; } }
			public int Count { get { return _length; } }

			public Topology.FaceEdge this[int index]
			{
				get { return new Topology.FaceEdge(_topology, _edgeIndices[index]); }
				set { throw new NotSupportedException(); }
			}

			public bool Contains(Topology.FaceEdge item)
			{
				return Array.IndexOf(_edgeIndices, item.index) != -1;
			}

			public int IndexOf(Topology.FaceEdge item)
			{
				return Array.IndexOf(_edgeIndices, item.index);
			}

			public void CopyTo(Topology.FaceEdge[] array, int arrayIndex)
			{
				for (int i = 0; i < _length; ++i)
				{
					array[arrayIndex++] = new Topology.FaceEdge(_topology, _edgeIndices[i]);
				}
			}

			public IEnumerator<Topology.FaceEdge> GetEnumerator()
			{
				for (int i = 0; i < _length; ++i)
				{
					yield return new Topology.FaceEdge(_topology, _edgeIndices[i]);
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public void Add(Topology.FaceEdge item) { throw new NotSupportedException(); }
			public void Insert(int index, Topology.FaceEdge item) { throw new NotSupportedException(); }
			public bool Remove(Topology.FaceEdge item) { throw new NotSupportedException(); }
			public void RemoveAt(int index) { throw new NotSupportedException(); }
			public void Clear() { throw new NotSupportedException(); }

			#endregion

			public Topology.Face source
			{
				get
				{
					return new Topology.Face(_topology, _sourceElementIndex);
				}
			}

			public Topology.Face target
			{
				get
				{
					return new Topology.Face(_topology, _targetElementIndex);
				}
			}

			private class FacePathAdapter : IFacePath
			{
				private FaceEdgePath _faceEdgePath;

				public FacePathAdapter(FaceEdgePath faceEdgePath)
				{
					_faceEdgePath = faceEdgePath;
				}

				public bool isComplete { get { return _faceEdgePath.isComplete; } }

				public Topology.Face source { get { return _faceEdgePath.source; } }

				public Topology.Face target { get { return _faceEdgePath.target; } }

				#region IList<Topology.Face> Implementation

				public bool IsReadOnly { get { return true; } }
				public int Count { get { return _faceEdgePath.isComplete ? _faceEdgePath._length + 1 : 0; } }

				public Topology.Face this[int index]
				{
					get
					{
						var topology = _faceEdgePath._topology;
						if (index != 0)
						{
							var edgeIndex = _faceEdgePath._edgeIndices[index - 1];
							var faceIndex = topology.edgeData[edgeIndex].face;
							return new Topology.Face(topology, faceIndex);
						}
						else
						{
							return new Topology.Face(topology, _faceEdgePath._sourceElementIndex);
						}
					}
					set
					{
						throw new NotSupportedException();
					}
				}

				public bool Contains(Topology.Face item)
				{
					return IndexOf(item) != -1;
				}

				public int IndexOf(Topology.Face item)
				{
					if (!_faceEdgePath.isComplete) return -1;
					var itemIndex = item.index;
					if (itemIndex == _faceEdgePath._sourceElementIndex) return 0;
					var length = _faceEdgePath._length;
					var edgeIndices = _faceEdgePath._edgeIndices;
					var edgeData = _faceEdgePath._topology.edgeData;
					for (int i = 0; i < length; ++i)
					{
						if (edgeData[edgeIndices[i]].face == itemIndex) return i + 1;
					}
					return -1;
				}

				public void CopyTo(Topology.Face[] array, int arrayIndex)
				{
					if (!_faceEdgePath.isComplete) return;
					var topology = _faceEdgePath._topology;
					array[arrayIndex++] = new Topology.Face(topology, _faceEdgePath._sourceElementIndex);
					var length = _faceEdgePath._length;
					var edgeIndices = _faceEdgePath._edgeIndices;
					var edgeData = _faceEdgePath._topology.edgeData;
					for (int i = 0; i < length; ++i)
					{
						array[arrayIndex++] = new Topology.Face(topology, edgeData[edgeIndices[i]].face);
					}
				}

				public IEnumerator<Topology.Face> GetEnumerator()
				{
					if (!_faceEdgePath.isComplete) yield break;
					var topology = _faceEdgePath._topology;
					yield return new Topology.Face(topology, _faceEdgePath._sourceElementIndex);
					var length = _faceEdgePath._length;
					var edgeIndices = _faceEdgePath._edgeIndices;
					var edgeData = _faceEdgePath._topology.edgeData;
					for (int i = 0; i < length; ++i)
					{
						yield return new Topology.Face(topology, edgeData[edgeIndices[i]].face);
					}
				}

				IEnumerator IEnumerable.GetEnumerator()
				{
					return GetEnumerator();
				}

				public void Add(Topology.Face item) { throw new NotSupportedException(); }
				public void Insert(int index, Topology.Face item) { throw new NotSupportedException(); }
				public bool Remove(Topology.Face item) { throw new NotSupportedException(); }
				public void RemoveAt(int index) { throw new NotSupportedException(); }
				public void Clear() { throw new NotSupportedException(); }

				#endregion
			}

			public IFacePath AsFacePath()
			{
				return new FacePathAdapter(this);
			}

			public FaceEdgePath Rebuild(Topology.Face source, Topology.Face target)
			{
				Rebuild(source.topology, source.index, target.index);
				return this;
			}

			public FaceEdgePath Rebuild(Topology.Face source, Topology.Face target, Node lastNode, Dictionary<int, Node> closedSet)
			{
				Rebuild(source.topology, source.index, target.index, lastNode, closedSet);
				return this;
			}
		}

		#endregion

		#region A* Implementations

		/// <summary>
		/// Finds the shortest or otherwise least costly path from the specified source vertex to
		/// the specified target vertex, using the A* algorithm and the supplied heuristic and cost
		/// delegates to measure costs between vertices and over vertex edges.
		/// </summary>
		/// <param name="source">The source vertex from which the path should start.</param>
		/// <param name="target">The target vertex that the path should attempt to reach.</param>
		/// <param name="costHeuristic">Delegate for estimating the cost of the path from the specified source vertex to the specified target vertex.</param>
		/// <param name="cost">Delegate for determining the actual cost of the path along the specified vertex edge, from its near vertex to its far vertex.</param>
		/// <param name="path">An optional existing path created by an earlier call to one of the <seealso cref="O:Experilous.MakeItTile.PathFinder.FindPath"/> functions, which will be overwritten with the new path data.</param>
		/// <returns>A vertex edge path instance describing the path found from source to target, or an incomplete object if no path was found.</returns>
		/// <remarks><para>The optional <paramref name="path"/> parameter is useful for reducing allocation activity
		/// and pressure on the garbage collector.  Reusing an existing path object will not require an additional
		/// allocation to store the path as long as the new path fits inside the capacity already available in the
		/// existing path.</para></remarks>
		public IVertexEdgePath FindPath(Topology.Vertex source, Topology.Vertex target, VertexCostHeuristicDelegate costHeuristic, VertexCostDelegate cost, IVertexEdgePath path = null)
		{
			if (!source) throw new ArgumentException("The source vertex must be a valid vertex.", "source");
			if (!target) throw new ArgumentException("The target vertex must be a valid vertex.", "target");
			if (!source.topology != target.topology) throw new ArgumentException("The target vertex must belong to the same topology as the source vertex.", "target");

			var concretePath = path as VertexEdgePath;
			if (concretePath == null)
			{
				if (path != null)
				{
					throw new ArgumentException("The provided pre-allocated path was not an instance of the necessary underlying type recognized by this path finder.", "path");
				}
				concretePath = new VertexEdgePath();
			}

			var topology = source.topology;

			if (source == target) return concretePath.Rebuild(source, target);

			if (_queue == null)
			{
				_queue = new DelegateOrderedPriorityQueue<Node>(Node.AreOrdered, Mathf.CeilToInt(Mathf.Sqrt(source.topology.vertices.Count)));
				_openSet = new Dictionary<int, Node>();
				_closedSet = new Dictionary<int, Node>();
			}
			else
			{
				_queue.Clear();
				_openSet.Clear();
				_closedSet.Clear();
			}

			_queue.Push(new Node(0f, 0f, costHeuristic(source, target, 0), source.index, -1, -1, 0));
			_openSet.Add(source.index, _queue.front);

			while (_queue.Count > 0)
			{
				var node = _queue.front;
				_queue.Pop();

				if (node._elementIndex == target.index)
				{
					return concretePath.Rebuild(source, target, node, _closedSet);
				}

				_closedSet.Add(node._elementIndex, node);
				var vertex = new Topology.Vertex(topology, node._elementIndex);

				foreach (var edge in vertex.edges)
				{
					if (_closedSet.ContainsKey(edge.vertex.index)) continue;

					var g = node._g + cost(edge, node._length);

					if (!float.IsPositiveInfinity(g))
					{
						Node neighborNode;
						if (!_openSet.TryGetValue(edge.vertex.index, out neighborNode))
						{
							var h = costHeuristic(edge.vertex, target, node._length);
							neighborNode = new Node(g + h, g, h, edge.vertex.index, edge.index, node._elementIndex, node._length + 1);
							_queue.Push(neighborNode);
							_openSet.Add(edge.vertex.index, neighborNode);
						}
						else if (g < neighborNode._g)
						{
							var h = costHeuristic(edge.vertex, target, node._length);
							neighborNode = new Node(g + h, g, h, edge.vertex.index, edge.index, node._elementIndex, node._length + 1);
							_openSet[edge.vertex.index] = neighborNode;
							_queue.Reprioritize(neighborNode);
						}
					}
				}
			}

			return concretePath.Rebuild(source, target);
		}

		/// <summary>
		/// Finds the shortest or otherwise least costly path from the specified source face to
		/// the specified target face, using the A* algorithm and the supplied heuristic and cost
		/// delegates to measure costs between faces and over face edges.
		/// </summary>
		/// <param name="source">The source face from which the path should start.</param>
		/// <param name="target">The target face that the path should attempt to reach.</param>
		/// <param name="costHeuristic">Delegate for estimating the cost of the path from the specified source face to the specified target face.</param>
		/// <param name="cost">Delegate for determining the actual cost of the path along the specified face edge, from its near vertex to its far face.</param>
		/// <param name="path">An optional existing path created by an earlier call to one of the <seealso cref="O:Experilous.MakeItTile.PathFinder.FindPath"/> functions, which will be overwritten with the new path data.</param>
		/// <returns>A face edge path instance describing the path found from source to target, or an incomplete object if no path was found.</returns>
		/// <remarks><para>The optional <paramref name="path"/> parameter is useful for reducing allocation activity
		/// and pressure on the garbage collector.  Reusing an existing path object will not require an additional
		/// allocation to store the path as long as the new path fits inside the capacity already available in the
		/// existing path.</para></remarks>
		public IFaceEdgePath FindPath(Topology.Face source, Topology.Face target, FaceCostHeuristicDelegate costHeuristic, FaceCostDelegate cost, IFaceEdgePath path = null)
		{
			if (!source) throw new ArgumentException("The source face must be a valid face.", "source");
			if (!target) throw new ArgumentException("The target face must be a valid face.", "target");
			if (source.topology != target.topology) throw new ArgumentException("The target face must belong to the same topology as the source face.", "target");

			var concretePath = path as FaceEdgePath;
			if (concretePath == null)
			{
				if (path != null)
				{
					throw new ArgumentException("The provided pre-allocated path was not an instance of the necessary underlying type recognized by this path finder.", "path");
				}
				concretePath = new FaceEdgePath();
			}

			var topology = source.topology;

			if (source == target) return concretePath.Rebuild(source, target);

			if (_queue == null)
			{
				_queue = new DelegateOrderedPriorityQueue<Node>(Node.AreOrdered, Mathf.CeilToInt(Mathf.Sqrt(source.topology.faces.Count)));
				_openSet = new Dictionary<int, Node>();
				_closedSet = new Dictionary<int, Node>();
			}
			else
			{
				_queue.Clear();
				_openSet.Clear();
				_closedSet.Clear();
			}

			_queue.Push(new Node(0f, 0f, costHeuristic(source, target, 0), source.index, -1, -1, 0));
			_openSet.Add(source.index, _queue.front);

			while (_queue.Count > 0)
			{
				var node = _queue.front;
				_queue.Pop();

				if (node._elementIndex == target.index)
				{
					return concretePath.Rebuild(source, target, node, _closedSet);
				}

				_closedSet.Add(node._elementIndex, node);
				var face = new Topology.Face(topology, node._elementIndex);

				foreach (var edge in face.edges)
				{
					if (_closedSet.ContainsKey(edge.face.index)) continue;

					var g = node._g + cost(edge, node._length);

					if (!float.IsPositiveInfinity(g))
					{
						Node neighborNode;
						if (!_openSet.TryGetValue(edge.face.index, out neighborNode))
						{
							var h = costHeuristic(edge.face, target, node._length);
							neighborNode = new Node(g + h, g, h, edge.face.index, edge.index, node._elementIndex, node._length + 1);
							_queue.Push(neighborNode);
							_openSet.Add(edge.face.index, neighborNode);
						}
						else if (g < neighborNode._g)
						{
							var h = costHeuristic(edge.face, target, node._length);
							neighborNode = new Node(g + h, g, h, edge.face.index, edge.index, node._elementIndex, node._length + 1);
							_openSet[edge.face.index] = neighborNode;
							_queue.Reprioritize(neighborNode);
						}
					}
				}
			}

			return concretePath.Rebuild(source, target);
		}

		#endregion

		#region Helper Utilities

		/// <summary>
		/// Finds the shortest or path from the specified source vertex to the specified target vertex,
		/// using the A* algorithm and the supplied vertex positions to measure standard Euclidean
		/// distance between vertices and over vertex edges.
		/// </summary>
		/// <param name="source">The source vertex from which the path should start.</param>
		/// <param name="target">The target vertex that the path should attempt to reach.</param>
		/// <param name="vertexPositions">The three dimensional positions of each face in the world.</param>
		/// <param name="path">An optional existing path created by an earlier call to one of the <seealso cref="O:Experilous.MakeItTile.PathFinder.FindPath"/> functions, which will be overwritten with the new path data.</param>
		/// <returns>A vertex edge path instance describing the path found from source to target, or an incomplete object if no path was found.</returns>
		/// <remarks><para>The optional <paramref name="path"/> parameter is useful for reducing allocation activity
		/// and pressure on the garbage collector.  Reusing an existing path object will not require an additional
		/// allocation to store the path as long as the new path fits inside the capacity already available in the
		/// existing path.</para></remarks>
		public IVertexEdgePath FindEuclideanPath(Topology.Vertex source, Topology.Vertex target, IVertexAttribute<Vector3> vertexPositions, IVertexEdgePath path = null)
		{
			return FindPath(source, target,
				(Topology.Vertex s, Topology.Vertex t, int pathLength) =>
				{
					return (vertexPositions[s] - vertexPositions[t]).magnitude;
				},
				(Topology.VertexEdge edge, int pathLength) =>
				{
					return (vertexPositions[edge.nearVertex] - vertexPositions[edge.farVertex]).magnitude;
				},
				path);
		}

		/// <summary>
		/// Finds the shortest or path from the specified source face to the specified target face,
		/// using the A* algorithm and the supplied face positions to measure standard Euclidean
		/// distance between faces and over face edges.
		/// </summary>
		/// <param name="source">The source face from which the path should start.</param>
		/// <param name="target">The target face that the path should attempt to reach.</param>
		/// <param name="facePositions">The three dimensional positions of each face in the world.</param>
		/// <param name="path">An optional existing path created by an earlier call to one of the <seealso cref="O:Experilous.MakeItTile.PathFinder.FindPath"/> functions, which will be overwritten with the new path data.</param>
		/// <returns>A face edge path instance describing the path found from source to target, or an incomplete object if no path was found.</returns>
		/// <remarks><para>The optional <paramref name="path"/> parameter is useful for reducing allocation activity
		/// and pressure on the garbage collector.  Reusing an existing path object will not require an additional
		/// allocation to store the path as long as the new path fits inside the capacity already available in the
		/// existing path.</para></remarks>
		public IFaceEdgePath FindEuclideanPath(Topology.Face source, Topology.Face target, IFaceAttribute<Vector3> facePositions, IFaceEdgePath path = null)
		{
			return FindPath(source, target,
				(Topology.Face s, Topology.Face t, int pathLength) =>
				{
					return (facePositions[s] - facePositions[t]).magnitude;
				},
				(Topology.FaceEdge edge, int pathLength) =>
				{
					if (edge.isOuterBoundary) return float.PositiveInfinity;
					return (facePositions[edge.nearFace] - facePositions[edge.farFace]).magnitude;
				},
				path);
		}

		/// <summary>
		/// Finds the shortest or path from the specified source vertex to the specified target vertex,
		/// using the A* algorithm and the supplied vertex positions to measure spherical arc distance
		/// between vertices and over vertex edges.
		/// </summary>
		/// <param name="source">The source vertex from which the path should start.</param>
		/// <param name="target">The target vertex that the path should attempt to reach.</param>
		/// <param name="surface">The surface describing the overall shape of the spherical manifold.</param>
		/// <param name="vertexPositions">The three dimensional positions of each face in the world.</param>
		/// <param name="path">An optional existing path created by an earlier call to one of the <seealso cref="O:Experilous.MakeItTile.PathFinder.FindPath"/> functions, which will be overwritten with the new path data.</param>
		/// <returns>A vertex edge path instance describing the path found from source to target, or an incomplete object if no path was found.</returns>
		/// <remarks><para>The optional <paramref name="path"/> parameter is useful for reducing allocation activity
		/// and pressure on the garbage collector.  Reusing an existing path object will not require an additional
		/// allocation to store the path as long as the new path fits inside the capacity already available in the
		/// existing path.</para></remarks>
		public IVertexEdgePath FindSphericalEuclideanPath(Topology.Vertex source, Topology.Vertex target, SphericalSurface surface, IVertexAttribute<Vector3> vertexPositions, IVertexEdgePath path = null)
		{
			return FindPath(source, target,
				(Topology.Vertex s, Topology.Vertex t, int pathLength) =>
				{
					var sourcePosition = vertexPositions[s];
					var targetPosition = vertexPositions[t];
					return Geometry.SphericalArcLength(sourcePosition, targetPosition, surface.radius);
				},
				(Topology.VertexEdge edge, int pathLength) =>
				{
					var sourcePosition = vertexPositions[edge.nearVertex];
					var targetPosition = vertexPositions[edge.farVertex];
					return Geometry.SphericalArcLength(sourcePosition, targetPosition, surface.radius);
				},
				path);
		}

		/// <summary>
		/// Finds the shortest or path from the specified source face to the specified target face,
		/// using the A* algorithm and the supplied face positions to measure spherical arc distance
		/// between faces and over face edges.
		/// </summary>
		/// <param name="source">The source face from which the path should start.</param>
		/// <param name="target">The target face that the path should attempt to reach.</param>
		/// <param name="surface">The surface describing the overall shape of the spherical manifold.</param>
		/// <param name="facePositions">The three dimensional positions of each face in the world.</param>
		/// <param name="path">An optional existing path created by an earlier call to one of the <seealso cref="O:Experilous.MakeItTile.PathFinder.FindPath"/> functions, which will be overwritten with the new path data.</param>
		/// <returns>A face edge path instance describing the path found from source to target, or an incomplete object if no path was found.</returns>
		/// <remarks><para>The optional <paramref name="path"/> parameter is useful for reducing allocation activity
		/// and pressure on the garbage collector.  Reusing an existing path object will not require an additional
		/// allocation to store the path as long as the new path fits inside the capacity already available in the
		/// existing path.</para></remarks>
		public IFaceEdgePath FindSphericalEuclideanPath(Topology.Face source, Topology.Face target, SphericalSurface surface, IFaceAttribute<Vector3> facePositions, IFaceEdgePath path = null)
		{
			return FindPath(source, target,
				(Topology.Face s, Topology.Face t, int pathLength) =>
				{
					var sourcePosition = facePositions[s];
					var targetPosition = facePositions[t];
					return Geometry.SphericalArcLength(sourcePosition, targetPosition, surface.radius);
				},
				(Topology.FaceEdge edge, int pathLength) =>
				{
					if (edge.isOuterBoundary) return float.PositiveInfinity;
					var sourcePosition = facePositions[edge.nearFace];
					var targetPosition = facePositions[edge.farFace];
					return Geometry.SphericalArcLength(sourcePosition, targetPosition, surface.radius);
				},
				path);
		}

		#endregion
	}
}
