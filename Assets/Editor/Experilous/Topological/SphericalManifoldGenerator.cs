using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorBundle), typeof(TopologyCategory), "Spherical Manifold")]
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

		public AssetDescriptor topology;
		public AssetDescriptor vertexPositions;

		public static SphericalManifoldGenerator CreateDefaultInstance(AssetGeneratorBundle bundle, string name)
		{
			var generator = CreateInstance<SphericalManifoldGenerator>();
			generator.bundle = bundle;
			generator.name = name;
			generator.hideFlags = HideFlags.HideInHierarchy;
			return generator;
		}

		[MenuItem("Assets/Create/Topology/Spherical Manifold Generator")]
		public static void CreateDefaultGeneratorBundle()
		{
			var bundle = TopologyGeneratorBundle.CreateDefaultInstance("New Spherical Manifold");
			bundle.Add(CreateDefaultInstance(bundle, "Manifold"));
			bundle.CreateAsset();
		}

		public override IEnumerable<AssetDescriptor> dependencies
		{
			get
			{
				yield break;
			}
		}

		public override IEnumerable<AssetDescriptor> outputs
		{
			get
			{
				if (topology == null) topology = AssetDescriptor.Create(this, typeof(Topology), "Topology");
				if (vertexPositions == null) vertexPositions = AssetDescriptor.Create(this, typeof(IVertexAttribute<Vector3>), "Vertex Positions", "Attributes");

				yield return topology;
				yield return vertexPositions;
			}
		}

		public override void ResetDependency(AssetDescriptor dependency)
		{
			if (dependency == null) throw new System.ArgumentNullException("dependency");
			throw new System.ArgumentException(string.Format("Generated asset \"{0}\" of type {1} is not a dependency of this spherical manifold generator.", dependency.name, dependency.GetType().Name), "dependency");
		}

		public override void Generate()
		{
			Topology topologyAsset;
			Vector3[] vertexPositionsArray;

			switch (sphericalPolyhedron)
			{
				case SphericalPolyhedrons.Tetrahedron:
					SphericalManifoldUtility.CreateTetrahedron(1f, out topologyAsset, out vertexPositionsArray);
					break;
				case SphericalPolyhedrons.Cube:
					SphericalManifoldUtility.CreateCube(1f, out topologyAsset, out vertexPositionsArray);
					break;
				case SphericalPolyhedrons.Octahedron:
					SphericalManifoldUtility.CreateOctahedron(1f, out topologyAsset, out vertexPositionsArray);
					break;
				case SphericalPolyhedrons.Dodecahedron:
					if (subdivisionDegree == 0)
					{
						SphericalManifoldUtility.CreateDodecahedron(1f, out topologyAsset, out vertexPositionsArray);
					}
					else
					{
						SphericalManifoldUtility.CreateIcosahedron(1f, out topologyAsset, out vertexPositionsArray);
					}
					break;
				case SphericalPolyhedrons.Icosahedron:
					SphericalManifoldUtility.CreateIcosahedron(1f, out topologyAsset, out vertexPositionsArray);
					break;
				default:
					throw new System.NotImplementedException();
			}

			SphericalManifoldUtility.Subdivide(topologyAsset, vertexPositionsArray.AsVertexAttribute(), subdivisionDegree, 1f, out topologyAsset, out vertexPositionsArray);

			var alreadyDual = sphericalPolyhedron == SphericalPolyhedrons.Dodecahedron && subdivisionDegree != 0;
			if (useDualPolyhedron != alreadyDual)
			{
				SphericalManifoldUtility.MakeDual(topologyAsset, ref vertexPositionsArray, 1f);
			}

			topology.SetAsset(topologyAsset);
			vertexPositions.SetAsset(Vector3VertexAttribute.CreateInstance(vertexPositionsArray, "Vertex Positions"));
		}

		public override bool CanGenerate()
		{
			return true;
		}
	}
}
