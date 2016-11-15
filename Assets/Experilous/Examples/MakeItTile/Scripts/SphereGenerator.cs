using UnityEngine;
using System.Collections.Generic;
using Experilous.MakeItTile;

namespace Experilous.Examples.MakeItTile
{
#if UNITY_EDITOR
	[ExecuteInEditMode]
	[RequireComponent(typeof(GeneratorExecutive))]
	public class SphereGenerator : MonoBehaviour, IGenerator
	{
		public float radius = 100f;
		public int subdivisionDegree = 10;

		private SphericalSurface _surface;
		private Topology _topology;
		private IVertexAttribute<Vector3> _vertexPositions;
		private IFaceAttribute<Vector3> _faceCentroids;
		private IEdgeAttribute<Vector3> _edgeMidpoints;

		public SphericalSurface surface { get { return _surface; } }
		public Topology topology { get { return _topology; } }
		public IVertexAttribute<Vector3> vertexPositions { get { return _vertexPositions; } }
		public IFaceAttribute<Vector3> faceCentroids { get { return _faceCentroids; } }
		public IEdgeAttribute<Vector3> edgeMidpoints { get { return _edgeMidpoints; } }

		public IEnumerable<IGenerator> dependencies
		{
			get
			{
				yield break;
			}
		}

		public void Generate()
		{
			_surface = SphericalSurface.Create(Vector3.up, Vector3.right, radius, false);

			Topology baseTopology;
			Vector3[] baseVertexPositionsArray;
			SphericalManifoldUtility.CreateIcosahedron(_surface, out baseTopology, out baseVertexPositionsArray);

			Vector3[] vertexPositionsArray;
			SphericalManifoldUtility.Subdivide(_surface, baseTopology, baseVertexPositionsArray.AsVertexAttribute(), subdivisionDegree, out _topology, out vertexPositionsArray);

			_vertexPositions = vertexPositionsArray.AsVertexAttribute();

			SphericalManifoldUtility.MakeDual(_surface, _topology, _vertexPositions, out vertexPositionsArray);
			_vertexPositions = vertexPositionsArray.AsVertexAttribute();
			_faceCentroids = FaceAttributeUtility.CalculateSphericalFaceCentroidsFromVertexPositions(_topology.internalFaces, _surface, _vertexPositions);
			_edgeMidpoints = EdgeAttributeUtility.CalculateSphericalVertexEdgeMidpointsFromVertexPositions(_topology.vertexEdges, _surface, _vertexPositions);
		}
	}
#endif
}
