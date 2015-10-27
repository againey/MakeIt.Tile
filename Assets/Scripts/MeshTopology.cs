using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class BasicMeshTopology
{
	public int[,] _triangleVertices; //Vn x 3
	public int[,] _edgeVertices; //En x 2
}

public class MeshTopology : BasicMeshTopology
{
	public int[,] _triangleTriangles; //Vn x 3
	public int[,] _triangleEdges; //Vn x 3
	public int[,] _edgeTriangles; //En x 2
	public int[] _vertexNeighborOffsets;
	public int[] _vertexTriangles;
	public int[] _vertexEdges;
	public int[] _vertexVertices;

	public struct VerticesIndexer : IEnumerable<Vertex>
	{
		private MeshTopology _topology;
		public VerticesIndexer(MeshTopology topology) { _topology = topology; }
		public Vertex this[int i] { get { return new Vertex(_topology, i); } }
		public int Count { get { return _topology._vertexNeighborOffsets.Length - 1; } }
		public IEnumerator<Vertex> GetEnumerator() { for (int i = 0; i < _topology._vertexNeighborOffsets.Length - 1; ++i) yield return new Vertex(_topology, i); }
		IEnumerator IEnumerable.GetEnumerator() { return (this as IEnumerable<Vertex>).GetEnumerator(); }
	}

	public VerticesIndexer Vertices { get { return new VerticesIndexer(this); } }

	public struct EdgesIndexer : IEnumerable<Edge>
	{
		private MeshTopology _topology;
		public EdgesIndexer(MeshTopology topology) { _topology = topology; }
		public Edge this[int i] { get { return new Edge(_topology, i); } }
		public int Count { get { return _topology._edgeVertices.GetLength(0); } }
		public IEnumerator<Edge> GetEnumerator() { for (int i = 0; i < _topology._edgeVertices.GetLength(0); ++i) yield return new Edge(_topology, i); }
		IEnumerator IEnumerable.GetEnumerator() { return (this as IEnumerable<Edge>).GetEnumerator(); }
	}

	public EdgesIndexer Edges { get { return new EdgesIndexer(this); } }

	public struct TrianglesIndexer : IEnumerable<Triangle>
	{
		private MeshTopology _topology;
		public TrianglesIndexer(MeshTopology topology) { _topology = topology; }
		public Triangle this[int i] { get { return new Triangle(_topology, i); } }
		public int Count { get { return _topology._triangleVertices.GetLength(0); } }
		public IEnumerator<Triangle> GetEnumerator() { for (int i = 0; i < _topology._triangleVertices.GetLength(0); ++i) yield return new Triangle(_topology, i); }
		IEnumerator IEnumerable.GetEnumerator() { return (this as IEnumerable<Triangle>).GetEnumerator(); }
	}

	public TrianglesIndexer Triangles { get { return new TrianglesIndexer(this); } }

	public struct Vertex : IEquatable<Vertex>
	{
		private MeshTopology _topology;
		private int _index;

		private int _neighborOffset;
		private int _neighborCount;

		public Vertex(MeshTopology topology, int index)
		{
			_topology = topology;
			_index = index;
			_neighborOffset = _topology._vertexNeighborOffsets[_index];
			_neighborCount = _topology._vertexNeighborOffsets[_index + 1] - _neighborOffset;
		}

		public int Index { get { return _index; } }

		public int NeighborCount { get { return _neighborCount; } }

		public struct VerticesIndexer : IEnumerable<Vertex>
		{
			private Vertex _vertex;
			public VerticesIndexer(Vertex vertex) { _vertex = vertex; }
			public Vertex this[int i] { get { return new Vertex(_vertex._topology, _vertex._topology._vertexVertices[_vertex._neighborOffset + i]); } }
			public int Count { get { return _vertex._neighborCount; } }
			public IEnumerator<Vertex> GetEnumerator() { for (int i = 0; i < _vertex._neighborCount; ++i) yield return this[i]; }
			IEnumerator IEnumerable.GetEnumerator() { return (this as IEnumerable<Vertex>).GetEnumerator(); }
		}

		public VerticesIndexer Vertices { get { return new VerticesIndexer(this); } }

		public struct EdgesIndexer : IEnumerable<Edge>
		{
			private Vertex _vertex;
			public EdgesIndexer(Vertex vertex) { _vertex = vertex; }
			public Edge this[int i] { get { return new Edge(_vertex._topology, _vertex._topology._vertexEdges[_vertex._neighborOffset + i]); } }
			public int Count { get { return _vertex._neighborCount; } }
			public IEnumerator<Edge> GetEnumerator() { for (int i = 0; i < _vertex._neighborCount; ++i) yield return this[i]; }
			IEnumerator IEnumerable.GetEnumerator() { return (this as IEnumerable<Edge>).GetEnumerator(); }
		}

		public EdgesIndexer Edges { get { return new EdgesIndexer(this); } }

		public struct TrianglesIndexer : IEnumerable<Triangle>
		{
			private Vertex _vertex;
			public TrianglesIndexer(Vertex vertex) { _vertex = vertex; }
			public Triangle this[int i] { get { return new Triangle(_vertex._topology, _vertex._topology._vertexTriangles[_vertex._neighborOffset + i]); } }
			public int Count { get { return _vertex._neighborCount; } }
			public IEnumerator<Triangle> GetEnumerator() { for (int i = 0; i < _vertex._neighborCount; ++i) yield return this[i]; }
			IEnumerator IEnumerable.GetEnumerator() { return (this as IEnumerable<Triangle>).GetEnumerator(); }
		}

		public TrianglesIndexer Triangles { get { return new TrianglesIndexer(this); } }

		public int NeighborIndexOf(Vertex vertex)
		{
			for (int i = _neighborOffset; i < _neighborOffset + _neighborCount; ++i)
			{
				if (_topology._vertexVertices[i] == vertex.Index) return i - _neighborOffset;
			}
			throw new ApplicationException("The specified vertex is not a neighbor of this vertex.");
		}

		public int NeighborIndexOf(Edge edge)
		{
			for (int i = _neighborOffset; i < _neighborOffset + _neighborCount; ++i)
			{
				if (_topology._vertexEdges[i] == edge.Index) return i - _neighborOffset;
			}
			throw new ApplicationException("The specified edge is not a neighbor of this vertex.");
		}

		public int NeighborIndexOf(Triangle triangle)
		{
			for (int i = _neighborOffset; i < _neighborOffset + _neighborCount; ++i)
			{
				if (_topology._vertexTriangles[i] == triangle.Index) return i - _neighborOffset;
			}
			throw new ApplicationException("The specified triangle is not a neighbor of this vertex.");
		}

		public int RotateNeighborIndex(int index, int distance)
		{
			return (index + _neighborCount + distance) % _neighborCount;
		}

		public Vertex PrevVertex(Vertex neighbor) { return Vertices[RotateNeighborIndex(NeighborIndexOf(neighbor), -1)]; }
		public Edge PrevEdge(Edge neighbor) { return Edges[RotateNeighborIndex(NeighborIndexOf(neighbor), -1)]; }
		public Triangle PrevTriangle(Triangle neighbor) { return Triangles[RotateNeighborIndex(NeighborIndexOf(neighbor), -1)]; }

		public Vertex NextVertex(Vertex neighbor) { return Vertices[RotateNeighborIndex(NeighborIndexOf(neighbor), +1)]; }
		public Edge NextEdge(Edge neighbor) { return Edges[RotateNeighborIndex(NeighborIndexOf(neighbor), +1)]; }
		public Triangle NextTriangle(Triangle neighbor) { return Triangles[RotateNeighborIndex(NeighborIndexOf(neighbor), +1)]; }

		public Vertex AssociatedVertex(Edge neighbor) { return Vertices[NeighborIndexOf(neighbor)]; }
		public Vertex PrevVertex(Triangle neighbor) { return Vertices[NeighborIndexOf(neighbor)]; }
		public Vertex NextVertex(Triangle neighbor) { return Vertices[RotateNeighborIndex(NeighborIndexOf(neighbor), +1)]; }

		public Edge AssociatedEdge(Vertex neighbor) { return Edges[NeighborIndexOf(neighbor)]; }
		public Edge PrevEdge(Triangle neighbor) { return Edges[NeighborIndexOf(neighbor)]; }
		public Edge NextEdge(Triangle neighbor) { return Edges[RotateNeighborIndex(NeighborIndexOf(neighbor), +1)]; }

		public Triangle PrevTriangle(Vertex neighbor) { return Triangles[RotateNeighborIndex(NeighborIndexOf(neighbor), -1)]; }
		public Triangle NextTriangle(Vertex neighbor) { return Triangles[NeighborIndexOf(neighbor)]; }
		public Triangle PrevTriangle(Edge neighbor) { return Triangles[RotateNeighborIndex(NeighborIndexOf(neighbor), -1)]; }
		public Triangle NextTriangle(Edge neighbor) { return Triangles[NeighborIndexOf(neighbor)]; }

		public override bool Equals(object other) { return other is Vertex && _index == ((Vertex)other)._index; }
		public bool Equals(Vertex other) { return _index == other._index; }
		public static bool operator ==(Vertex lhs, Vertex rhs) { return lhs._index == rhs._index; }
		public static bool operator !=(Vertex lhs, Vertex rhs) { return lhs._index != rhs._index; }
		public override int GetHashCode() { return _index.GetHashCode(); }
	}

	public struct Edge : IEquatable<Edge>
	{
		private MeshTopology _topology;
		private int _index;

		public Edge(MeshTopology topology, int index)
		{
			_topology = topology;
			_index = index;
		}

		public int Index { get { return _index; } }

		public int NeighborCount { get { return 2; } }

		public struct VerticesIndexer : IEnumerable<Vertex>
		{
			private MeshTopology _topology;
			private int _index;

			public VerticesIndexer(MeshTopology topology, int index)
			{
				_topology = topology;
				_index = index;
			}

			public Vertex this[int i] { get { return new Vertex(_topology, _topology._edgeVertices[_index, i]); } }
			public int Count { get { return 2; } }
			public IEnumerator<Vertex> GetEnumerator() { for (int i = 0; i < 2; ++i) yield return this[i]; }
			IEnumerator IEnumerable.GetEnumerator() { return (this as IEnumerable<Vertex>).GetEnumerator(); }
		}

		public VerticesIndexer Vertices { get { return new VerticesIndexer(_topology, _index); } }

		public struct TrianglesIndexer : IEnumerable<Triangle>
		{
			private MeshTopology _topology;
			private int _index;

			public TrianglesIndexer(MeshTopology topology, int index)
			{
				_topology = topology;
				_index = index;
			}

			public Triangle this[int i] { get { return new Triangle(_topology, _topology._edgeTriangles[_index, i]); } }
			public int Count { get { return 2; } }
			public IEnumerator<Triangle> GetEnumerator() { for (int i = 0; i < 2; ++i) yield return this[i]; }
			IEnumerator IEnumerable.GetEnumerator() { return (this as IEnumerable<Triangle>).GetEnumerator(); }
		}

		public TrianglesIndexer Triangles { get { return new TrianglesIndexer(_topology, _index); } }

		public int NeighborIndexOf(Vertex vertex)
		{
			if (_topology._edgeVertices[_index, 0] == vertex.Index) return 0;
			else if (_topology._edgeVertices[_index, 1] == vertex.Index) return 1;
			throw new ApplicationException("The specified vertex is not a neighbor of this edge.");
		}

		public int NeighborIndexOf(Triangle triangle)
		{
			if (_topology._edgeTriangles[_index, 0] == triangle.Index) return 0;
			else if (_topology._edgeTriangles[_index, 1] == triangle.Index) return 1;
			throw new ApplicationException("The specified triangle is not a neighbor of this edge.");
		}

		public int RotateNeighborIndex(int index, int distance)
		{
			return (index + 2 + distance) % 2;
		}

		public Vertex OppositeVertex(Vertex neighbor) { return Vertices[RotateNeighborIndex(NeighborIndexOf(neighbor), +1)]; }
		public Triangle OppositeTriangle(Triangle neighbor) { return Triangles[RotateNeighborIndex(NeighborIndexOf(neighbor), +1)]; }

		public Triangle PrevTriangle(Vertex neighbor) { return Triangles[RotateNeighborIndex(NeighborIndexOf(neighbor), -1)]; }
		public Triangle NextTriangle(Vertex neighbor) { return Triangles[NeighborIndexOf(neighbor)]; }
		public Vertex PrevVertex(Triangle neighbor) { return Vertices[NeighborIndexOf(neighbor)]; }
		public Vertex NextVertex(Triangle neighbor) { return Vertices[RotateNeighborIndex(NeighborIndexOf(neighbor), +1)]; }

		public override bool Equals(object other) { return other is Edge && _index == ((Edge)other)._index; }
		public bool Equals(Edge other) { return _index == other._index; }
		public static bool operator ==(Edge lhs, Edge rhs) { return lhs._index == rhs._index; }
		public static bool operator !=(Edge lhs, Edge rhs) { return lhs._index != rhs._index; }
		public override int GetHashCode() { return _index.GetHashCode(); }
	}

	public struct Triangle : IEquatable<Triangle>
	{
		private MeshTopology _topology;
		private int _index;

		public Triangle(MeshTopology topology, int index)
		{
			_topology = topology;
			_index = index;
		}

		public int Index { get { return _index; } }

		public int NeighborCount { get { return 3; } }

		public struct VerticesIndexer : IEnumerable<Vertex>
		{
			private MeshTopology _topology;
			private int _index;

			public VerticesIndexer(MeshTopology topology, int index)
			{
				_topology = topology;
				_index = index;
			}

			public Vertex this[int i] { get { return new Vertex(_topology, _topology._triangleVertices[_index, i]); } }
			public int Count { get { return 3; } }
			public IEnumerator<Vertex> GetEnumerator() { for (int i = 0; i < 3; ++i) yield return this[i]; }
			IEnumerator IEnumerable.GetEnumerator() { return (this as IEnumerable<Vertex>).GetEnumerator(); }
		}

		public VerticesIndexer Vertices { get { return new VerticesIndexer(_topology, _index); } }

		public struct EdgesIndexer : IEnumerable<Edge>
		{
			private MeshTopology _topology;
			private int _index;

			public EdgesIndexer(MeshTopology topology, int index)
			{
				_topology = topology;
				_index = index;
			}

			public Edge this[int i] { get { return new Edge(_topology, _topology._triangleEdges[_index, i]); } }
			public int Count { get { return 3; } }
			public IEnumerator<Edge> GetEnumerator() { for (int i = 0; i < 3; ++i) yield return this[i]; }
			IEnumerator IEnumerable.GetEnumerator() { return (this as IEnumerable<Edge>).GetEnumerator(); }
		}

		public EdgesIndexer Edges { get { return new EdgesIndexer(_topology, _index); } }

		public struct TrianglesIndexer : IEnumerable<Triangle>
		{
			private MeshTopology _topology;
			private int _index;

			public TrianglesIndexer(MeshTopology topology, int index)
			{
				_topology = topology;
				_index = index;
			}

			public Triangle this[int i] { get { return new Triangle(_topology, _topology._triangleTriangles[_index, i]); } }
			public int Count { get { return 3; } }
			public IEnumerator<Triangle> GetEnumerator() { for (int i = 0; i < 3; ++i) yield return this[i]; }
			IEnumerator IEnumerable.GetEnumerator() { return (this as IEnumerable<Triangle>).GetEnumerator(); }
		}

		public TrianglesIndexer Triangles { get { return new TrianglesIndexer(_topology, _index); } }

		public int NeighborIndexOf(Vertex vertex)
		{
			if (_topology._triangleVertices[_index, 0] == vertex.Index) return 0;
			else if (_topology._triangleVertices[_index, 1] == vertex.Index) return 1;
			else if (_topology._triangleVertices[_index, 2] == vertex.Index) return 2;
			throw new ApplicationException("The specified vertex is not a neighbor of this triangle.");
		}

		public int NeighborIndexOf(Edge edge)
		{
			if (_topology._triangleEdges[_index, 0] == edge.Index) return 0;
			else if (_topology._triangleEdges[_index, 1] == edge.Index) return 1;
			else if (_topology._triangleEdges[_index, 2] == edge.Index) return 2;
			throw new ApplicationException("The specified edge is not a neighbor of this triangle.");
		}

		public int NeighborIndexOf(Triangle triangle)
		{
			if (_topology._triangleTriangles[_index, 0] == triangle.Index) return 0;
			else if (_topology._triangleTriangles[_index, 1] == triangle.Index) return 1;
			else if (_topology._triangleTriangles[_index, 2] == triangle.Index) return 2;
			throw new ApplicationException("The specified triangle is not a neighbor of this triangle.");
		}

		public int RotateNeighborIndex(int index, int distance)
		{
			return (index + 3 + distance) % 3;
		}

		public Vertex PrevVertex(Vertex neighbor) { return Vertices[RotateNeighborIndex(NeighborIndexOf(neighbor), -1)]; }
		public Edge PrevEdge(Edge neighbor) { return Edges[RotateNeighborIndex(NeighborIndexOf(neighbor), -1)]; }
		public Triangle PrevTriangle(Triangle neighbor) { return Triangles[RotateNeighborIndex(NeighborIndexOf(neighbor), -1)]; }

		public Vertex NextVertex(Vertex neighbor) { return Vertices[RotateNeighborIndex(NeighborIndexOf(neighbor), +1)]; }
		public Edge NextEdge(Edge neighbor) { return Edges[RotateNeighborIndex(NeighborIndexOf(neighbor), 1)]; }
		public Triangle NextTriangle(Triangle neighbor) { return Triangles[RotateNeighborIndex(NeighborIndexOf(neighbor), +1)]; }

		public Vertex PrevVertex(Edge neighbor) { return Vertices[NeighborIndexOf(neighbor)]; }
		public Vertex NextVertex(Edge neighbor) { return Vertices[RotateNeighborIndex(NeighborIndexOf(neighbor), +1)]; }
		public Vertex PrevVertex(Triangle neighbor) { return Vertices[NeighborIndexOf(neighbor)]; }
		public Vertex NextVertex(Triangle neighbor) { return Vertices[RotateNeighborIndex(NeighborIndexOf(neighbor), +1)]; }

		public Edge PrevEdge(Vertex neighbor) { return Edges[RotateNeighborIndex(NeighborIndexOf(neighbor), -1)]; }
		public Edge NextEdge(Vertex neighbor) { return Edges[NeighborIndexOf(neighbor)]; }
		public Edge AssociatedEdge(Triangle neighbor) { return Edges[NeighborIndexOf(neighbor)]; }

		public Triangle PrevTriangle(Vertex neighbor) { return Triangles[RotateNeighborIndex(NeighborIndexOf(neighbor), -1)]; }
		public Triangle NextTriangle(Vertex neighbor) { return Triangles[NeighborIndexOf(neighbor)]; }
		public Triangle AssociatedTriangle(Edge neighbor) { return Triangles[NeighborIndexOf(neighbor)]; }

		public override bool Equals(object other) { return other is Triangle && _index == ((Triangle)other)._index; }
		public bool Equals(Triangle other) { return _index == other._index; }
		public static bool operator ==(Triangle lhs, Triangle rhs) { return lhs._index == rhs._index; }
		public static bool operator !=(Triangle lhs, Triangle rhs) { return lhs._index != rhs._index; }
		public override int GetHashCode() { return _index.GetHashCode(); }
	}

	public MeshTopology()
	{
	}

	public MeshTopology(BasicMeshTopology basicTopology)
	{
		_triangleVertices = new int[basicTopology._triangleVertices.GetLength(0), 3];
		System.Array.Copy(basicTopology._triangleVertices, 0, _triangleVertices, 0, basicTopology._triangleVertices.Length);
		_edgeVertices = new int[basicTopology._edgeVertices.GetLength(0), 2];
		System.Array.Copy(basicTopology._edgeVertices, 0, _edgeVertices, 0, basicTopology._edgeVertices.Length);

		// Determine the total number of tiles by finding the maximum tile index specified and adding 1.
		int vertexCount = 0;
		for (int i = 0; i < _edgeVertices.GetLength(0); ++i)
		{
			vertexCount = Mathf.Max(vertexCount, Mathf.Max(_edgeVertices[i, 0], _edgeVertices[i, 1]));
		}
		++vertexCount;

		// Determine the number of neighbors each tile has by examining all the edges and incrementing
		// the tile neighbor count for both neighboring tiles of every edge.
		var vertexNeighborCounts = new byte[vertexCount];
		for (int i = 0; i < _edgeVertices.GetLength(0); ++i)
		{
			++vertexNeighborCounts[_edgeVertices[i, 0]];
			++vertexNeighborCounts[_edgeVertices[i, 1]];
		}

		// Determine the offsets in the tile neighbor arrays by tracking a running sum of neighbor counts.
		_vertexNeighborOffsets = new int[vertexCount + 1];
		int totalNeighborCount = 0;
		for (int i = 0; i < vertexNeighborCounts.Length; ++i)
		{
			_vertexNeighborOffsets[i] = totalNeighborCount;
			totalNeighborCount += vertexNeighborCounts[i];
		}
		_vertexNeighborOffsets[_vertexNeighborOffsets.Length - 1] = totalNeighborCount;

		// Reset the tile neighbor counts, and then add the neighboring edges to the tiles one by one.
		System.Array.Clear(vertexNeighborCounts, 0, vertexNeighborCounts.Length);
		_vertexEdges = new int[totalNeighborCount];
		for (int i = 0; i < _edgeVertices.GetLength(0); ++i)
		{
			_vertexEdges[_vertexNeighborOffsets[_edgeVertices[i, 0]] + vertexNeighborCounts[_edgeVertices[i, 0]]++] = i;
			_vertexEdges[_vertexNeighborOffsets[_edgeVertices[i, 1]] + vertexNeighborCounts[_edgeVertices[i, 1]]++] = i;
		}

		// Reset the tile neighbor counts, and then add the neighboring vertices to the tiles one by one.
		System.Array.Clear(vertexNeighborCounts, 0, vertexNeighborCounts.Length);
		_vertexTriangles = new int[totalNeighborCount];
		for (int i = 0; i < _triangleVertices.GetLength(0); ++i)
		{
			_vertexTriangles[_vertexNeighborOffsets[_triangleVertices[i, 0]] + vertexNeighborCounts[_triangleVertices[i, 0]]++] = i;
			_vertexTriangles[_vertexNeighborOffsets[_triangleVertices[i, 1]] + vertexNeighborCounts[_triangleVertices[i, 1]]++] = i;
			_vertexTriangles[_vertexNeighborOffsets[_triangleVertices[i, 2]] + vertexNeighborCounts[_triangleVertices[i, 2]]++] = i;
		}

		// Reorder tile neighbors based on the order of vertices to their neighbor tiles.
		_vertexVertices = new int[totalNeighborCount];
		for (int i = 0; i < vertexNeighborCounts.Length; ++i)
		{
			var nextTriangle = _vertexTriangles[_vertexNeighborOffsets[i]];
			var triangleVertex = _triangleVertices[nextTriangle, 0] == i ? 0 : _triangleVertices[nextTriangle, 1] == i ? 1 : 2;
			var priorVertex = _triangleVertices[nextTriangle, (triangleVertex + 1) % 3];
			var nextVertex = _triangleVertices[nextTriangle, (triangleVertex + 2) % 3];

			// Reorder the tile's first neighbor edge to match the first neighbor vertex.
			for (int k = 0; k < vertexNeighborCounts[i]; ++k)
			{
				var vertexEdge = _vertexEdges[_vertexNeighborOffsets[i] + k];
				if (_edgeVertices[vertexEdge, 0] == priorVertex || _edgeVertices[vertexEdge, 1] == priorVertex)
				{
					if (k > 0)
					{
						var swapIntermediate = _vertexEdges[_vertexNeighborOffsets[i]];
						_vertexEdges[_vertexNeighborOffsets[i]] = _vertexEdges[_vertexNeighborOffsets[i] + k];
						_vertexEdges[_vertexNeighborOffsets[i] + k] = swapIntermediate;
					}
					break;
				}
			}

			// Set the first neighbor tile to match the first neighbor edge and vertex.
			_vertexVertices[_vertexNeighborOffsets[i]] = priorVertex;

			// Find each following vertex, given the preceding vertex, and then line up the edges and neighbor tiles to match.
			// The very last neighbor doesn't need to be reordered, since there will be nothing left to swap with.
			for (int j = 1; j < vertexNeighborCounts[i] - 1; ++j)
			{
				// Check each following vertex to see if it is the correct vertex to come next in the ordering.
				for (int k = j; k < vertexNeighborCounts[i]; ++k)
				{
					nextTriangle = _vertexTriangles[_vertexNeighborOffsets[i] + k];
					triangleVertex = _triangleVertices[nextTriangle, 0] == i ? 0 : _triangleVertices[nextTriangle, 1] == i ? 1 : 2;
					if (_triangleVertices[nextTriangle, (triangleVertex + 1) % 3] == nextVertex)
					{
						priorVertex = nextVertex;
						nextVertex = _triangleVertices[nextTriangle, (triangleVertex + 2) % 3];
						if (k > j)
						{
							var swapIntermediate = _vertexTriangles[_vertexNeighborOffsets[i] + j];
							_vertexTriangles[_vertexNeighborOffsets[i] + j] = _vertexTriangles[_vertexNeighborOffsets[i] + k];
							_vertexTriangles[_vertexNeighborOffsets[i] + k] = swapIntermediate;
						}
						break;
					}
				}

				// Reorder the tile's j-th neighbor edge to match the j-th neighbor vertex.
				for (int k = j; k < vertexNeighborCounts[i]; ++k)
				{
					var vertexEdge = _vertexEdges[_vertexNeighborOffsets[i] + k];
					if (_edgeVertices[vertexEdge, 0] == priorVertex || _edgeVertices[vertexEdge, 1] == priorVertex)
					{
						if (k > j)
						{
							var swapIntermediate = _vertexEdges[_vertexNeighborOffsets[i] + j];
							_vertexEdges[_vertexNeighborOffsets[i] + j] = _vertexEdges[_vertexNeighborOffsets[i] + k];
							_vertexEdges[_vertexNeighborOffsets[i] + k] = swapIntermediate;
						}
						break;
					}
				}

				// Set the j-th neighbor tile to match the j-th neighbor edge and vertex.
				_vertexVertices[_vertexNeighborOffsets[i] + j] = priorVertex;
			}

			// Set the last neighbor tile to match the last neighbor edge and vertex.
			_vertexVertices[_vertexNeighborOffsets[i + 1] - 1] = nextVertex;
		}

		// Build edge neighbors.
		_edgeTriangles = new int[_edgeVertices.GetLength(0), 2];

		for (int i = 0; i < _edgeVertices.GetLength(0); ++i)
		{
			var edgeVertex = _edgeVertices[i, 0];
			var vertexEdge = 0;
			for (; vertexEdge < vertexNeighborCounts[edgeVertex]; ++vertexEdge)
			{
				if (_vertexEdges[_vertexNeighborOffsets[edgeVertex] + vertexEdge] == i)
				{
					break;
				}
			}

			_edgeTriangles[i, 0] = _vertexTriangles[_vertexNeighborOffsets[edgeVertex] + vertexEdge];
			_edgeTriangles[i, 1] = _vertexTriangles[_vertexNeighborOffsets[edgeVertex] + (vertexEdge + vertexNeighborCounts[edgeVertex] - 1) % vertexNeighborCounts[edgeVertex]];
		}

		// Build vertex neighbors.
		_triangleEdges = new int[_triangleVertices.GetLength(0), 3];
		_triangleTriangles = new int[_triangleVertices.GetLength(0), 3];

		for (int i = 0; i < _triangleVertices.GetLength(0); ++i)
		{
			for (int j = 0; j < 3; ++j)
			{
				var triangleVertex = _triangleVertices[i, j];
				var vertexTriangle = 0;
				for (; vertexTriangle < vertexNeighborCounts[triangleVertex]; ++vertexTriangle)
				{
					if (_vertexTriangles[_vertexNeighborOffsets[triangleVertex] + vertexTriangle] == i)
					{
						break;
					}
				}
				var triangleEdge = _vertexEdges[_vertexNeighborOffsets[triangleVertex] + vertexTriangle];
				_triangleEdges[i, j] = triangleEdge;
				_triangleTriangles[i, j] = _vertexTriangles[_vertexNeighborOffsets[triangleVertex] + (vertexTriangle + vertexNeighborCounts[triangleVertex] - 1) % vertexNeighborCounts[triangleVertex]];
			}
		}
	}

	public BasicMeshTopology Subdivide(int degree, System.Action<int> vertexAllocator, System.Action<int, int> vertexCopier, System.Action<int, int, int, float> vertexInterpolator)
	{
		var subdivided = new BasicMeshTopology();

		if (degree == 0)
		{
			subdivided._triangleVertices = new int[_triangleVertices.GetLength(0), 3];
			System.Array.Copy(_triangleVertices, 0, subdivided._triangleVertices, 0, _triangleVertices.Length);
			subdivided._edgeVertices = new int[_edgeVertices.GetLength(0), 2];
			System.Array.Copy(_edgeVertices, 0, subdivided._edgeVertices, 0, _edgeVertices.Length);
			vertexAllocator(_vertexNeighborOffsets.Length - 1);
			for (int i = 0; i < _vertexNeighborOffsets.Length - 1; ++i)
			{
				vertexCopier(i, i);
			}
			return subdivided;
		}

		var vertexCount = _vertexNeighborOffsets.Length - 1;
		var edgeCount = _edgeVertices.GetLength(0);
		var triangleCount = _triangleVertices.GetLength(0);

		var innerVerticesPerEdge = degree + 0;
		var innerEdgesPerEdge = degree + 1;
		var verticesPerTriangle = (degree + 2) * (degree + 3) / 2;
		var innerVerticesPerTriangle = (degree - 1) * degree / 2;
		var innerEdgesPerTriangle = degree * (degree + 1) * 3 / 2;
		var innerTrianglesPerTriangle = (degree + 1) * (degree + 1);

		int totalSubdividedVertexCount = innerVerticesPerTriangle * triangleCount + innerVerticesPerEdge * edgeCount + vertexCount;
		int totalSubdividedEdgeCount = innerEdgesPerTriangle * triangleCount + innerEdgesPerEdge * edgeCount;
		int totalSubdividedTriangleCount = innerTrianglesPerTriangle * triangleCount;

		vertexAllocator(totalSubdividedVertexCount);
		subdivided._edgeVertices = new int[totalSubdividedEdgeCount, 2];
		subdivided._triangleVertices = new int[totalSubdividedTriangleCount, 3];

		int subdividedVertexCount = 0;
		int subdividedEdgeCount = 0;
		int subdividedTriangleCount = 0;

		for (int i = 0; i < _vertexNeighborOffsets.Length - 1; ++i)
		{
			vertexCopier(i, i);
		}
		subdividedVertexCount = _vertexNeighborOffsets.Length - 1;

		System.Action<int, int, int> SubdivideLine = delegate (int i0, int i1, int count)
		{
			var dt = 1.0f / (float)(count + 1);
			var t = dt;
			var tEnd = 1f - dt * 0.5f;
			while (t < tEnd)
			{
				vertexInterpolator(i0, i1, subdividedVertexCount++, t);
				t += dt;
			}
		};

		var firstSubdividedEdgeVertex = subdividedVertexCount;
		for (int i = 0; i < edgeCount; ++i)
		{
			int firstEdgeVertex = subdividedVertexCount;

			SubdivideLine(_edgeVertices[i, 0], _edgeVertices[i, 1], innerVerticesPerEdge);

			subdivided._edgeVertices[subdividedEdgeCount, 0] = _edgeVertices[i, 0];
			subdivided._edgeVertices[subdividedEdgeCount++, 1] = firstEdgeVertex;
			for (int j = 1; j < degree; ++j)
			{
				subdivided._edgeVertices[subdividedEdgeCount, 0] = firstEdgeVertex + j - 1;
				subdivided._edgeVertices[subdividedEdgeCount++, 1] = firstEdgeVertex + j;
			}
			subdivided._edgeVertices[subdividedEdgeCount, 0] = firstEdgeVertex + degree - 1;
			subdivided._edgeVertices[subdividedEdgeCount++, 1] = _edgeVertices[i, 1];
		}

		var triangleVertexLookup = new int[verticesPerTriangle];

		for (int i = 0; i < triangleCount; ++i)
		{
			var edge0 = _triangleEdges[i, 0];
			var edge1 = _triangleEdges[i, 1];
			var edge2 = _triangleEdges[i, 2];
			var firstEdgeVertex0 = firstSubdividedEdgeVertex + edge0 * innerVerticesPerEdge;
			var firstEdgeVertex1 = firstSubdividedEdgeVertex + edge1 * innerVerticesPerEdge;
			var firstEdgeVertex2 = firstSubdividedEdgeVertex + edge2 * innerVerticesPerEdge;
			var vertexDelta0 = _edgeVertices[edge0, 0] == _triangleVertices[i, 1] ? 1 : -1;
			var vertexDelta1 = _edgeVertices[edge1, 0] == _triangleVertices[i, 1] ? 1 : -1;
			var vertexDelta2 = _edgeVertices[edge2, 0] == _triangleVertices[i, 0] ? 1 : -1;
			if (vertexDelta0 == -1) firstEdgeVertex0 += innerVerticesPerEdge - 1;
			if (vertexDelta1 == -1) firstEdgeVertex1 += innerVerticesPerEdge - 1;
			if (vertexDelta2 == -1) firstEdgeVertex2 += innerVerticesPerEdge - 1;

			triangleVertexLookup[0] = _triangleVertices[i, 1];
			triangleVertexLookup[1] = firstEdgeVertex0;
			triangleVertexLookup[2] = firstEdgeVertex1;
			var triangleVertexLookupCount = 3;

			var edgeVertex0 = firstEdgeVertex0 + vertexDelta0;
			var edgeVertex1 = firstEdgeVertex1 + vertexDelta1;

			for (int j = 1; j < innerVerticesPerEdge; ++j)
			{
				var firstRowVertex = subdividedVertexCount;
				SubdivideLine(edgeVertex0, edgeVertex1, j);

				triangleVertexLookup[triangleVertexLookupCount++] = edgeVertex0;
				for (int k = 0; k < j; ++k)
				{
					triangleVertexLookup[triangleVertexLookupCount++] = firstRowVertex + k;
				}
				triangleVertexLookup[triangleVertexLookupCount++] = edgeVertex1;

				edgeVertex0 += vertexDelta0;
				edgeVertex1 += vertexDelta1;
			}

			triangleVertexLookup[triangleVertexLookupCount++] = _triangleVertices[i, 0];
			var edgeVertex2 = firstEdgeVertex2;
			for (int k = 0; k < innerVerticesPerEdge; ++k)
			{
				triangleVertexLookup[triangleVertexLookupCount++] = edgeVertex2++;
			}
			triangleVertexLookup[triangleVertexLookupCount++] = _triangleVertices[i, 2];

			subdivided._triangleVertices[subdividedTriangleCount, 0] = triangleVertexLookup[0];
			subdivided._triangleVertices[subdividedTriangleCount, 1] = triangleVertexLookup[1];
			subdivided._triangleVertices[subdividedTriangleCount++, 2] = triangleVertexLookup[2];

			var triangleVertex = 1;
			for (int j = 1; j <= innerVerticesPerEdge; ++j)
			{
				for (int k = 0; k < j; ++k)
				{
					subdivided._edgeVertices[subdividedEdgeCount, 0] = triangleVertexLookup[triangleVertex];
					subdivided._edgeVertices[subdividedEdgeCount++, 1] = triangleVertexLookup[triangleVertex + 1];

					subdivided._edgeVertices[subdividedEdgeCount, 0] = triangleVertexLookup[triangleVertex];
					subdivided._edgeVertices[subdividedEdgeCount++, 1] = triangleVertexLookup[triangleVertex + j + 2];

					subdivided._edgeVertices[subdividedEdgeCount, 0] = triangleVertexLookup[triangleVertex + 1];
					subdivided._edgeVertices[subdividedEdgeCount++, 1] = triangleVertexLookup[triangleVertex + j + 2];

					subdivided._triangleVertices[subdividedTriangleCount, 0] = triangleVertexLookup[triangleVertex];
					subdivided._triangleVertices[subdividedTriangleCount, 1] = triangleVertexLookup[triangleVertex + j + 1];
					subdivided._triangleVertices[subdividedTriangleCount++, 2] = triangleVertexLookup[triangleVertex + j + 2];

					subdivided._triangleVertices[subdividedTriangleCount, 0] = triangleVertexLookup[triangleVertex];
					subdivided._triangleVertices[subdividedTriangleCount, 1] = triangleVertexLookup[triangleVertex + j + 2];
					subdivided._triangleVertices[subdividedTriangleCount++, 2] = triangleVertexLookup[triangleVertex + 1];

					++triangleVertex;
				}

				subdivided._triangleVertices[subdividedTriangleCount, 0] = triangleVertexLookup[triangleVertex];
				subdivided._triangleVertices[subdividedTriangleCount, 1] = triangleVertexLookup[triangleVertex + j + 1];
				subdivided._triangleVertices[subdividedTriangleCount++, 2] = triangleVertexLookup[triangleVertex + j + 2];

				++triangleVertex;
			}
		}

		return subdivided;
	}

	private struct VertexAlteration
	{
		public int _neighbor;
		public int _vertex;
		public int _edge;
		public int _triangle;

		public VertexAlteration(int neighbor)
		{
			_neighbor = neighbor;
			_vertex = -1;
			_edge = -1;
			_triangle = -1;
		}

		public VertexAlteration(int neighbor, int vertex, int edge, int triangle)
		{
			_neighbor = neighbor;
			_vertex = vertex;
			_edge = edge;
			_triangle = triangle;
		}
	}

	private int GetVertexVertexNeighborIndex(int vertex, int neighborVertex)
	{
		for (int i = _vertexNeighborOffsets[vertex]; i < _vertexNeighborOffsets[vertex + 1]; ++i)
		{
			if (_vertexVertices[i] == neighborVertex) return i - _vertexNeighborOffsets[vertex];
		}
		throw new ApplicationException("The provided neighbor vertex was not actually a neighbor of the specified vertex.");
	}

	private int GetVertexEdgeNeighborIndex(int vertex, int edge)
	{
		for (int i = _vertexNeighborOffsets[vertex]; i < _vertexNeighborOffsets[vertex + 1]; ++i)
		{
			if (_vertexEdges[i] == edge) return i - _vertexNeighborOffsets[vertex];
		}
		throw new ApplicationException("The provided edge was not a neighbor of the specified vertex.");
	}

	private int GetEdgeTriangleNeighborIndex(int edge, int triangle)
	{
		if (_edgeTriangles[edge, 0] == triangle) return 0;
		else if (_edgeTriangles[edge, 1] == triangle) return 1;
		else throw new ApplicationException("The provided triangle was not a neighbor of the specified edge.");
	}

	private int GetTriangleEdgeNeighborIndex(int triangle, int edge)
	{
		if (_triangleEdges[triangle, 0] == edge) return 0;
		else if (_triangleEdges[triangle, 1] == edge) return 1;
		else if (_triangleEdges[triangle, 2] == edge) return 2;
		else throw new ApplicationException("The provided edge was not a neighbor of the specified triangle.");
	}

	private int GetTriangleTriangleNeighborIndex(int triangle, int neighborTriangle)
	{
		if (_triangleTriangles[triangle, 0] == neighborTriangle) return 0;
		else if (_triangleTriangles[triangle, 1] == neighborTriangle) return 1;
		else if (_triangleTriangles[triangle, 2] == neighborTriangle) return 2;
		else throw new ApplicationException("The provided triangle was not a neighbor of the specified triangle.");
	}

	private int RotateVertexNeighborIndex(int vertex, int neighborIndex, int distance)
	{
		var neighborCount = _vertexNeighborOffsets[vertex + 1] - _vertexNeighborOffsets[vertex];
		return (neighborIndex + neighborCount + distance) % neighborCount;
	}

	private int RotateTriangleNeighborIndex(int neighborIndex, int distance)
	{
		return (neighborIndex + 3 + distance) % 3;
	}

	private void InsertVertexAlteration(int vertex, VertexAlteration alteration, List<int> vertexAlterationIndexQueue, List<VertexAlteration> vertexAlterationQueue)
	{
		var insertionIndex = vertexAlterationIndexQueue.BinarySearch(vertex);
		if (insertionIndex >= 0) throw new ApplicationException("The specified vertex already has an alteration queued, and cannot be altered a second time in a single pass.");
		insertionIndex = ~insertionIndex;

		vertexAlterationIndexQueue.Insert(insertionIndex, vertex);
		vertexAlterationQueue.Insert(insertionIndex, alteration);
	}

	private void ApplyVertexAlteration(List<int> vertexAlterationIndexQueue, List<VertexAlteration> vertexAlterationQueue, int[] newVertexNeighborOffsets, int[] newVertexVertices, int[] newVertexEdges, int[] newVertexTriangles, ref int newOffset, ref int vertexAlterationQueuePosition)
	{
		var vertex = vertexAlterationIndexQueue[vertexAlterationQueuePosition];
		var firstOffset = _vertexNeighborOffsets[vertex];
		var neighborCount = _vertexNeighborOffsets[vertex + 1] - firstOffset;
		newVertexNeighborOffsets[vertex] = newOffset;

		var neighborIndex = vertexAlterationQueue[vertexAlterationQueuePosition]._neighbor;
		if (vertexAlterationQueue[vertexAlterationQueuePosition]._vertex >= 0)
		{
			if (neighborIndex > 0)
			{
				System.Array.Copy(_vertexVertices, firstOffset, newVertexVertices, newOffset, neighborIndex);
				System.Array.Copy(_vertexEdges, firstOffset, newVertexEdges, newOffset, neighborIndex);
				System.Array.Copy(_vertexTriangles, firstOffset, newVertexTriangles, newOffset, neighborIndex);
			}

			newVertexVertices[newOffset + neighborIndex] = vertexAlterationQueue[vertexAlterationQueuePosition]._vertex;
			newVertexEdges[newOffset + neighborIndex] = vertexAlterationQueue[vertexAlterationQueuePosition]._edge;
			newVertexTriangles[newOffset + neighborIndex] = _vertexTriangles[firstOffset + (neighborIndex + neighborCount - 1) % neighborCount];

			if (neighborIndex < neighborCount)
			{
				System.Array.Copy(_vertexVertices, firstOffset + neighborIndex, newVertexVertices, newOffset + neighborIndex + 1, neighborCount - neighborIndex);
				System.Array.Copy(_vertexEdges, firstOffset + neighborIndex, newVertexEdges, newOffset + neighborIndex + 1, neighborCount - neighborIndex);
				System.Array.Copy(_vertexTriangles, firstOffset + neighborIndex, newVertexTriangles, newOffset + neighborIndex + 1, neighborCount - neighborIndex);
			}

			newVertexTriangles[newOffset + (neighborIndex + neighborCount) % (neighborCount + 1)] = vertexAlterationQueue[vertexAlterationQueuePosition]._triangle;

			newOffset += neighborCount + 1;
		}
		else
		{
			if (neighborIndex > 0)
			{
				System.Array.Copy(_vertexVertices, firstOffset, newVertexVertices, newOffset, neighborIndex);
				System.Array.Copy(_vertexEdges, firstOffset, newVertexEdges, newOffset, neighborIndex);
				System.Array.Copy(_vertexTriangles, firstOffset, newVertexTriangles, newOffset, neighborIndex);
			}

			if (neighborIndex < neighborCount - 1)
			{
				System.Array.Copy(_vertexVertices, firstOffset + neighborIndex + 1, newVertexVertices, newOffset + neighborIndex, neighborCount - neighborIndex - 1);
				System.Array.Copy(_vertexEdges, firstOffset + neighborIndex + 1, newVertexEdges, newOffset + neighborIndex, neighborCount - neighborIndex - 1);
				System.Array.Copy(_vertexTriangles, firstOffset + neighborIndex + 1, newVertexTriangles, newOffset + neighborIndex, neighborCount - neighborIndex - 1);
			}

			newOffset += neighborCount - 1;
		}

		++vertexAlterationQueuePosition;
	}

	private void RotateEdge(int edge, List<int> vertexAlterationIndexQueue, List<VertexAlteration> vertexAlterationQueue)
	{
		var oldVertex0 = _edgeVertices[edge, 0];
		var oldVertex1 = _edgeVertices[edge, 1];
		var triangle0 = _edgeTriangles[edge, 0];
		var triangle1 = _edgeTriangles[edge, 1];
		var oldVertexNeighborIndex0 = GetVertexEdgeNeighborIndex(oldVertex0, edge);
		var oldVertexNeighborIndex1 = GetVertexEdgeNeighborIndex(oldVertex1, edge);
		var newVertex0 = _vertexVertices[_vertexNeighborOffsets[oldVertex0] + RotateVertexNeighborIndex(oldVertex0, oldVertexNeighborIndex0, 1)];
		var newVertex1 = _vertexVertices[_vertexNeighborOffsets[oldVertex1] + RotateVertexNeighborIndex(oldVertex1, oldVertexNeighborIndex1, 1)];

		_triangleVertices[triangle0, 0] = newVertex0;
		_triangleVertices[triangle0, 1] = newVertex1;
		_triangleVertices[triangle0, 2] = oldVertex1;
		_triangleVertices[triangle1, 0] = newVertex1;
		_triangleVertices[triangle1, 1] = newVertex0;
		_triangleVertices[triangle1, 2] = oldVertex0;

		var triangleNeighborIndex0 = GetTriangleEdgeNeighborIndex(triangle0, edge);
		var triangleNeighborIndex1 = GetTriangleEdgeNeighborIndex(triangle1, edge);
		var triangleNeighborNextIndex0 = RotateTriangleNeighborIndex(triangleNeighborIndex0, 1);
		var triangleNeighborNextIndex1 = RotateTriangleNeighborIndex(triangleNeighborIndex1, 1);
		var triangleOuterNeighborIndex0 = RotateTriangleNeighborIndex(triangleNeighborNextIndex0, 1);
		var triangleOuterNeighborIndex1 = RotateTriangleNeighborIndex(triangleNeighborNextIndex1, 1);

		var triangleOuterEdge00 = _triangleEdges[triangle0, triangleNeighborNextIndex0];
		var triangleOuterEdge01 = _triangleEdges[triangle0, triangleOuterNeighborIndex0];
		var triangleOuterEdge10 = _triangleEdges[triangle1, triangleNeighborNextIndex1];
		var triangleOuterEdge11 = _triangleEdges[triangle1, triangleOuterNeighborIndex1];

		_triangleEdges[triangle0, 0] = edge;
		_triangleEdges[triangle0, 1] = triangleOuterEdge11;
		_triangleEdges[triangle0, 2] = triangleOuterEdge00;
		_triangleEdges[triangle1, 0] = edge;
		_triangleEdges[triangle1, 1] = triangleOuterEdge01;
		_triangleEdges[triangle1, 2] = triangleOuterEdge10;

		var triangleOuterTriangle00 = _triangleTriangles[triangle0, triangleNeighborNextIndex0];
		var triangleOuterTriangle01 = _triangleTriangles[triangle0, triangleOuterNeighborIndex0];
		var triangleOuterTriangle10 = _triangleTriangles[triangle1, triangleNeighborNextIndex1];
		var triangleOuterTriangle11 = _triangleTriangles[triangle1, triangleOuterNeighborIndex1];

		_triangleTriangles[triangle0, 0] = triangle1;
		_triangleTriangles[triangle0, 1] = triangleOuterTriangle11;
		_triangleTriangles[triangle0, 2] = triangleOuterTriangle00;
		_triangleTriangles[triangle1, 0] = triangle0;
		_triangleTriangles[triangle1, 1] = triangleOuterTriangle01;
		_triangleTriangles[triangle1, 2] = triangleOuterTriangle10;

		_edgeVertices[edge, 0] = newVertex0;
		_edgeVertices[edge, 1] = newVertex1;

		_edgeTriangles[triangleOuterEdge01, GetEdgeTriangleNeighborIndex(triangleOuterEdge01, triangle0)] = triangle1;
		_edgeTriangles[triangleOuterEdge11, GetEdgeTriangleNeighborIndex(triangleOuterEdge11, triangle1)] = triangle0;

		_triangleTriangles[triangleOuterTriangle01, GetTriangleTriangleNeighborIndex(triangleOuterTriangle01, triangle0)] = triangle1;
		_triangleTriangles[triangleOuterTriangle11, GetTriangleTriangleNeighborIndex(triangleOuterTriangle11, triangle1)] = triangle0;

		InsertVertexAlteration(oldVertex0, new VertexAlteration(oldVertexNeighborIndex0), vertexAlterationIndexQueue, vertexAlterationQueue);
		InsertVertexAlteration(oldVertex1, new VertexAlteration(oldVertexNeighborIndex1), vertexAlterationIndexQueue, vertexAlterationQueue);

		var newVertexNeighborIndex0 = GetVertexEdgeNeighborIndex(newVertex0, triangleOuterEdge00);
		var newVertexNeighborIndex1 = GetVertexEdgeNeighborIndex(newVertex1, triangleOuterEdge10);
		InsertVertexAlteration(newVertex0, new VertexAlteration(newVertexNeighborIndex0, newVertex1, edge, triangle1), vertexAlterationIndexQueue, vertexAlterationQueue);
		InsertVertexAlteration(newVertex1, new VertexAlteration(newVertexNeighborIndex1, newVertex0, edge, triangle0), vertexAlterationIndexQueue, vertexAlterationQueue);
	}

	public MeshTopology AlterTopology(int passCount, System.Func<MeshTopology, int, bool> edgeRotationPredicate, System.Action<MeshTopology> afterPassAction)
	{
		MeshTopology altered = new MeshTopology();

		altered._triangleTriangles = _triangleTriangles.Clone() as int[,];
		altered._triangleEdges = _triangleEdges.Clone() as int[,];
		altered._triangleVertices = _triangleVertices.Clone() as int[,];
		altered._edgeTriangles = _edgeTriangles.Clone() as int[,];
		altered._edgeVertices = _edgeVertices.Clone() as int[,];
		altered._vertexNeighborOffsets = _vertexNeighborOffsets.Clone() as int[];
		altered._vertexTriangles = _vertexTriangles.Clone() as int[];
		altered._vertexEdges = _vertexEdges.Clone() as int[];
		altered._vertexVertices = _vertexVertices.Clone() as int[];

		if (passCount == 0) return altered;

		//Begin Loop
			//Do Alter Pass
				//For each vertex V
					//If vertex V is at the beginning of alterations queue
						//Write vertex V alterations to the new buffer
					//Else
						//For each neighbor edge E
							//If vertex V has the smallest index of the four associated vertices
								//If none of the other three vertices are in the alterations queue (binary search)
									//If random chance is true
										//If edge E is in judged to be reasonably shaped for a topology rotation
											//   o          o   <- newVertexIndex1
											//  / \        /|\
											// o---o  ->  o | o <- oldVertexIndex0/1
											//  \ /        \|/
											//   o          o   <- newVertexIndex0
											//Alter all relevant triangles
											//Alter all relevant edges
											//Write vertex V alterations to the new buffer
											//Store alterations to the other three vertices in the alterations queue (insert sorted)
											//Skip remaining neighbor edges of vertex V
						//If vertex did not get altered
							//Write existing vertex data to the new buffer
			//Do Relax Pass
		//End Loop

		var newVertexNeighborOffsets = new int[_vertexNeighborOffsets.Length];
		var newVertexVertices = new int[_vertexVertices.Length];
		var newVertexEdges = new int[_vertexEdges.Length];
		var newVertexTriangles = new int[_vertexTriangles.Length];

		var vertexAlterationIndexQueue = new List<int>();
		var vertexAlterationQueue = new List<VertexAlteration>();

		for (int pass = 0; pass < passCount; ++pass)
		{
			var newOffset = 0;
			int vertexAlterationQueuePosition = 0;

			for (int vertex = 0; vertex < altered._vertexNeighborOffsets.Length - 1; ++vertex)
			{
				bool vertexAltered = false;

				var firstOffset = altered._vertexNeighborOffsets[vertex];
				var neighborCount = altered._vertexNeighborOffsets[vertex + 1] - firstOffset;
				if (vertexAlterationQueuePosition >= vertexAlterationIndexQueue.Count || vertexAlterationIndexQueue[vertexAlterationQueuePosition] != vertex)
				{
					for (int neighbor = 0; neighbor < neighborCount; ++neighbor)
					{
						var neighborLeft = (neighbor + neighborCount - 1) % neighborCount;
						var neighborRight = (neighbor + 1) % neighborCount;

						var vertexLeft = altered._vertexVertices[firstOffset + neighborLeft];
						var vertexMiddle = altered._vertexVertices[firstOffset + neighbor];
						var vertexRight = altered._vertexVertices[firstOffset + neighborRight];

						if (vertex > vertexLeft || vertex > vertexMiddle) continue;
						if (vertexAlterationIndexQueue.BinarySearch(vertexLeft) >= 0) continue;
						if (vertexAlterationIndexQueue.BinarySearch(vertexMiddle) >= 0) continue;

						int edgeToRotate;

						if (vertex > vertexRight) goto tryFarEdge;
						if (vertexAlterationIndexQueue.BinarySearch(vertexRight) >= 0) goto tryFarEdge;
						edgeToRotate = altered._vertexEdges[firstOffset + neighbor];
						if (!edgeRotationPredicate(altered, edgeToRotate)) goto tryFarEdge;
						goto rotate;

						tryFarEdge:
						var neighborLeftMiddle = altered.GetVertexVertexNeighborIndex(vertexLeft, vertexMiddle);
						var neighborLeftLeft = altered.RotateVertexNeighborIndex(vertexLeft, neighborLeftMiddle, -1);
						var leftFirstOffset = altered._vertexNeighborOffsets[vertexLeft];
						var vertexLeftLeft = altered._vertexVertices[leftFirstOffset + neighborLeftLeft];

						if (vertex > vertexLeftLeft) continue;
						if (vertexAlterationIndexQueue.BinarySearch(vertexLeftLeft) >= 0) continue;
						edgeToRotate = altered._vertexEdges[leftFirstOffset + neighborLeftMiddle];
						if (!edgeRotationPredicate(altered, edgeToRotate)) continue;

						rotate: altered.RotateEdge(edgeToRotate, vertexAlterationIndexQueue, vertexAlterationQueue);

						vertexAltered = true;
						break;
					}
				}
				else
				{
					vertexAltered = true;
				}

				if (!vertexAltered)
				{
					newVertexNeighborOffsets[vertex] = newOffset;
					System.Array.Copy(altered._vertexVertices, firstOffset, newVertexVertices, newOffset, neighborCount);
					System.Array.Copy(altered._vertexEdges, firstOffset, newVertexEdges, newOffset, neighborCount);
					System.Array.Copy(altered._vertexTriangles, firstOffset, newVertexTriangles, newOffset, neighborCount);

					newOffset += neighborCount;
				}
				else
				{
					altered.ApplyVertexAlteration(vertexAlterationIndexQueue, vertexAlterationQueue, newVertexNeighborOffsets, newVertexVertices, newVertexEdges, newVertexTriangles, ref newOffset, ref vertexAlterationQueuePosition);
				}
			}

			newVertexNeighborOffsets[newVertexNeighborOffsets.Length - 1] = newOffset;

			Utility.Swap(ref altered._vertexNeighborOffsets, ref newVertexNeighborOffsets);
			Utility.Swap(ref altered._vertexVertices, ref newVertexVertices);
			Utility.Swap(ref altered._vertexEdges, ref newVertexEdges);
			Utility.Swap(ref altered._vertexTriangles, ref newVertexTriangles);

			vertexAlterationIndexQueue.Clear();
			vertexAlterationQueue.Clear();

			afterPassAction(altered);
		}

		return altered;
	}

	public void RelaxForRegularity(Vector3[] originalPositions, Vector3[] relaxedPositions)
	{
		System.Array.Clear(relaxedPositions, 0, relaxedPositions.Length);

		for (int i = 0; i < originalPositions.Length; ++i)
		{
			for (int j = _vertexNeighborOffsets[i]; j < _vertexNeighborOffsets[i + 1]; ++j)
			{
				relaxedPositions[i] += originalPositions[_vertexVertices[j]];
			}
			relaxedPositions[i].Normalize();
		}
	}

	public void RelaxForArea(Vector3[] originalPositions, Vector3[] relaxedPositions, float idealArea)
	{
		System.Array.Clear(relaxedPositions, 0, relaxedPositions.Length);

		for (int i = 0; i < originalPositions.Length; ++i)
		{
			var centerPosition = originalPositions[i];
			var prevNeighborPosition = originalPositions[_vertexVertices[_vertexNeighborOffsets[i + 1] - 1]];
			float surroundingArea = 0;
			for (int j = _vertexNeighborOffsets[i]; j < _vertexNeighborOffsets[i + 1]; ++j)
			{
				var neighborPosition = originalPositions[_vertexVertices[j]];
				surroundingArea += Vector3.Cross(neighborPosition - centerPosition, prevNeighborPosition - centerPosition).magnitude * 0.5f;
				prevNeighborPosition = neighborPosition;
			}
			surroundingArea /= 3f;
			var multiplier = idealArea / surroundingArea;
			for (int j = _vertexNeighborOffsets[i]; j < _vertexNeighborOffsets[i + 1]; ++j)
			{
				var neighborPosition = originalPositions[_vertexVertices[j]];
				relaxedPositions[_vertexVertices[j]] += (neighborPosition - centerPosition) * multiplier + centerPosition;
			}
		}

		for (int i = 0; i < originalPositions.Length; ++i)
		{
			relaxedPositions[i].Normalize();
		}
	}

	public bool ValidateAndRepairPositions(Vector3[] vertexPositions, float adjustmentWeight)
	{
		bool adjusted = false;
		float originalWeight = 1f - adjustmentWeight;
		for (int i = 0; i < vertexPositions.Length; ++i)
		{
			var centerPosition = vertexPositions[i];
			var neighborPosition0 = vertexPositions[_vertexVertices[_vertexNeighborOffsets[i + 1] - 2]];
			var neighborPosition1 = vertexPositions[_vertexVertices[_vertexNeighborOffsets[i + 1] - 1]];
			var centroid0 = (centerPosition + neighborPosition0 + neighborPosition1) / 3f;
			for (int j = _vertexNeighborOffsets[i]; j < _vertexNeighborOffsets[i + 1]; ++j)
			{
				var neighborPosition2 = vertexPositions[_vertexVertices[j]];
				var centroid1 = (centerPosition + neighborPosition1 + neighborPosition2) / 3f;
				var normal = Vector3.Cross(centroid0 - centerPosition, centroid1 - centerPosition);
				if (Vector3.Dot(normal, centerPosition) < 0f)
				{
					adjusted = true;
					var averageNeighborPosition = new Vector3(0f, 0f, 0f);
					for (int k = _vertexNeighborOffsets[i]; k < _vertexNeighborOffsets[i + 1]; ++k)
					{
						averageNeighborPosition += vertexPositions[_vertexVertices[k]];
					}
					averageNeighborPosition /= _vertexNeighborOffsets[i + 1] - _vertexNeighborOffsets[i];
					vertexPositions[i] = (centerPosition * originalWeight + averageNeighborPosition * adjustmentWeight).normalized;
					break;
				}
				neighborPosition0 = neighborPosition1;
				neighborPosition1 = neighborPosition2;
				centroid0 = centroid1;
			}
		}
		return !adjusted;
	}
}

