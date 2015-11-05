using UnityEngine;

namespace Experilous.Topological
{
	using VertexPositions = VertexAttribute<Vector3>;
	public class Manifold
	{
		private readonly Topology _topology;
		private readonly VertexPositions _vertexPositions;

		public Manifold(Topology topology, VertexPositions vertexPositions)
		{
			_topology = topology;
			_vertexPositions = vertexPositions;
		}

		public Manifold Clone()
		{
			return new Manifold(_topology.Clone(), _vertexPositions.Clone());
		}

		public Topology topology { get { return _topology; } }
		public VertexPositions vertexPositions { get { return _vertexPositions; } }
	}
}
