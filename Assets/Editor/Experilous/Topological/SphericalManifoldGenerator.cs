using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorCollection), typeof(TopologyCategory), "Spherical Manifold")]
	public class SphericalManifoldGenerator : AssetGenerator
	{
		public enum SphericalPolyhedrons
		{
			Tetrahedron,
			Cube,
			Octahedron,
			Dodecahedron,
			Icosahedron,
		}

		public SphericalPolyhedrons sphericalPolyhedron = SphericalPolyhedrons.Icosahedron;
		public int subdivisionDegree = 10;
		public bool useDualPolyhedron = true;

		public AssetDescriptor topologyDescriptor;
		public AssetDescriptor vertexPositionsDescriptor;

		[MenuItem("Assets/Create/Topology/Spherical Manifold Generator")]
		public static void CreateDefaultGeneratorCollection()
		{
			var collection = TopologyGeneratorCollection.Create("New Spherical Manifold");
			collection.Add(CreateInstance<SphericalManifoldGenerator>(collection, "Manifold"));
			collection.CreateAsset();
		}

		protected override void Initialize(bool reset = true)
		{
			// Outputs
			if (reset || topologyDescriptor == null) topologyDescriptor = AssetDescriptor.CreateGrouped<Topology>(this, "Topology", "Descriptors");
			if (reset || vertexPositionsDescriptor == null) vertexPositionsDescriptor = AssetDescriptor.CreateGrouped<IVertexAttribute<Vector3>>(this, "Vertex Positions", "Attributes");
		}

		public override IEnumerable<AssetDescriptor> outputs
		{
			get
			{
				yield return topologyDescriptor;
				yield return vertexPositionsDescriptor;
			}
		}

		public override void Generate()
		{
			Topology topology;
			Vector3[] vertexPositions;

			switch (sphericalPolyhedron)
			{
				case SphericalPolyhedrons.Tetrahedron:
					SphericalManifoldUtility.CreateTetrahedron(1f, out topology, out vertexPositions);
					break;
				case SphericalPolyhedrons.Cube:
					SphericalManifoldUtility.CreateCube(1f, out topology, out vertexPositions);
					break;
				case SphericalPolyhedrons.Octahedron:
					SphericalManifoldUtility.CreateOctahedron(1f, out topology, out vertexPositions);
					break;
				case SphericalPolyhedrons.Dodecahedron:
					if (subdivisionDegree == 0)
					{
						SphericalManifoldUtility.CreateDodecahedron(1f, out topology, out vertexPositions);
					}
					else
					{
						SphericalManifoldUtility.CreateIcosahedron(1f, out topology, out vertexPositions);
					}
					break;
				case SphericalPolyhedrons.Icosahedron:
					SphericalManifoldUtility.CreateIcosahedron(1f, out topology, out vertexPositions);
					break;
				default:
					throw new System.NotImplementedException();
			}

			SphericalManifoldUtility.Subdivide(topology, vertexPositions.AsVertexAttribute(), subdivisionDegree, 1f, out topology, out vertexPositions);

			var alreadyDual = sphericalPolyhedron == SphericalPolyhedrons.Dodecahedron && subdivisionDegree != 0;
			if (useDualPolyhedron != alreadyDual)
			{
				SphericalManifoldUtility.MakeDual(topology, ref vertexPositions, 1f);
			}

			topologyDescriptor.SetAsset(topology);
			vertexPositionsDescriptor.SetAsset(Vector3VertexAttribute.CreateInstance(vertexPositions, "Vertex Positions"));
		}
	}
}
