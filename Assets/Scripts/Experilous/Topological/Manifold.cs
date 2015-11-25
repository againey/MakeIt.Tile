using UnityEngine;
using System;

namespace Experilous.Topological
{
	[Serializable]
	public class Manifold
	{
		[SerializeField]
		private Topology _topology;

		[SerializeField]
		private VertexAttribute<Vector3> _vertexPositions;

		public Manifold(Topology topology, VertexAttribute<Vector3> vertexPositions)
		{
			_topology = topology;
			_vertexPositions = vertexPositions;
		}

		public Manifold Clone()
		{
			return new Manifold(_topology.Clone(), _vertexPositions.Clone());
		}

		public Topology topology { get { return _topology; } set { _topology = value; } }
		public VertexAttribute<Vector3> vertexPositions { get { return _vertexPositions; } set { _vertexPositions = value; } }
	}
}
