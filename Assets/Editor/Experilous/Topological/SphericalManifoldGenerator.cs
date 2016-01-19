﻿using UnityEngine;
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

		public GeneratedAsset topology;
		public GeneratedAsset vertexPositions;

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
			bundle.Add(CreateDefaultInstance(bundle, "Spherical Manifold"));
			bundle.CreateAsset();
		}

		public override IEnumerable<GeneratedAsset> dependencies
		{
			get
			{
				yield break;
			}
		}

		public override IEnumerable<GeneratedAsset> outputs
		{
			get
			{
				if (topology == null) topology = TopologyGeneratedAsset.CreateDefaultInstance(this, "Topology");
				if (vertexPositions == null) vertexPositions = Vector3VertexAttributeGeneratedAsset.CreateDefaultInstance(this, "Vertex Positions");

				yield return topology;
				yield return vertexPositions;
			}
		}

		public override void ResetDependency(GeneratedAsset dependency)
		{
			if (dependency == null) throw new System.ArgumentNullException("dependency");
			throw new System.ArgumentException(string.Format("Generated asset \"{0}\" of type {1} is not a dependency of this spherical manifold generator.", dependency.name, dependency.GetType().Name), "dependency");
		}

		public override void Generate(string location, string name)
		{
			Topology topology;
			Vector3[] vertexPositionsArray;

			switch (sphericalPolyhedron)
			{
				case SphericalPolyhedrons.Tetrahedron:
					SphericalManifoldUtility.CreateTetrahedron(1f, out topology, out vertexPositionsArray);
					break;
				case SphericalPolyhedrons.Cube:
					SphericalManifoldUtility.CreateCube(1f, out topology, out vertexPositionsArray);
					break;
				case SphericalPolyhedrons.Octahedron:
					SphericalManifoldUtility.CreateOctahedron(1f, out topology, out vertexPositionsArray);
					break;
				case SphericalPolyhedrons.Dodecahedron:
					if (subdivisionDegree == 0)
					{
						SphericalManifoldUtility.CreateDodecahedron(1f, out topology, out vertexPositionsArray);
					}
					else
					{
						SphericalManifoldUtility.CreateIcosahedron(1f, out topology, out vertexPositionsArray);
					}
					break;
				case SphericalPolyhedrons.Icosahedron:
					SphericalManifoldUtility.CreateIcosahedron(1f, out topology, out vertexPositionsArray);
					break;
				default:
					throw new System.NotImplementedException();
			}

			SphericalManifoldUtility.Subdivide(topology, vertexPositionsArray.AsVertexAttribute(), subdivisionDegree, 1f, out topology, out vertexPositionsArray);

			var alreadyDual = sphericalPolyhedron == SphericalPolyhedrons.Dodecahedron && subdivisionDegree != 0;
			if (useDualPolyhedron != alreadyDual)
			{
				SphericalManifoldUtility.MakeDual(topology, ref vertexPositionsArray, 1f);
			}

			this.topology.SetGeneratedInstance(location, name, topology);

			if (vertexPositions.isEnabled)
			{
				vertexPositions.SetGeneratedInstance(location, name, Vector3VertexAttribute.CreateInstance(vertexPositionsArray, "Vertex Positions"));
			}
			else
			{
				vertexPositions.ClearGeneratedInstance();
			}
		}

		public override bool CanGenerate()
		{
			return true;
		}
	}
}
