using UnityEngine;

namespace Experilous.Topological
{
	using VertexPositions = VertexAttribute<Vector3>;
	public class Manifold
	{
		private Topology _topology;
		private VertexPositions _vertexPositions;

		public Manifold(Topology topology, VertexPositions vertexPositions)
		{
			_topology = topology;
			_vertexPositions = vertexPositions;
		}

		public Manifold Clone()
		{
			return new Manifold(_topology.Clone(), _vertexPositions.Clone());
		}

		public Topology topology { get { return _topology; } set { _topology = value; } }
		public VertexPositions vertexPositions { get { return _vertexPositions; } set { _vertexPositions = value; } }
	}
}
