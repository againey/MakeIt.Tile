﻿using UnityEngine;
using UnityEditor;
using System.Collections;
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
		public float radius = 1f;
		public int subdivisionDegree = 10;
		public bool useDualPolyhedron = true;

		public AssetDescriptor surfaceDescriptor;
		public AssetDescriptor topologyDescriptor;
		public AssetDescriptor vertexPositionsDescriptor;

		[MenuItem("Assets/Create/Topology/Spherical Manifold Generator")]
		public static void CreateDefaultGeneratorCollection()
		{
			var collection = TopologyGeneratorCollection.Create("New Spherical Manifold");
			collection.Add(CreateInstance<SphericalManifoldGenerator>(collection, "Manifold"));
			collection.Add(CreateInstance<RandomEngineGenerator>(collection));
			collection.Add(CreateInstance<TopologyRandomizerGenerator>(collection));
			collection.Add(CreateInstance<FaceCentroidsGenerator>(collection));
			collection.Add(CreateInstance<FaceNormalsGenerator>(collection));
			collection.Add(CreateInstance<RectangularFaceGroupsGenerator>(collection));
			collection.Add(CreateInstance<MeshGenerator>(collection));
			collection.Add(CreateInstance<PrefabGenerator>(collection));
			collection.CreateAsset();
		}

		protected override void Initialize(bool reset = true)
		{
			// Outputs
			if (reset || surfaceDescriptor == null) surfaceDescriptor = AssetDescriptor.CreateGrouped<SphericalSurface>(this, "Spherical Surface", "Descriptors");
			if (reset || topologyDescriptor == null) topologyDescriptor = AssetDescriptor.CreateGrouped<Topology>(this, "Topology", "Descriptors");
			if (reset || vertexPositionsDescriptor == null) vertexPositionsDescriptor = AssetDescriptor.CreateGrouped<IVertexAttribute<Vector3>>(this, "Vertex Positions", "Attributes");
		}

		public override IEnumerable<AssetDescriptor> outputs
		{
			get
			{
				yield return surfaceDescriptor;
				yield return topologyDescriptor;
				yield return vertexPositionsDescriptor;
			}
		}

		public override void Pregenerate()
		{
			var surface = surfaceDescriptor.GetAsset<SphericalSurface>();
			if (surface == null) surface = surfaceDescriptor.SetAsset(CreateInstance<SphericalSurface>(), false);
			surface.Reset(new SphericalDescriptor(Vector3.up, 1f));
		}

		public override IEnumerator BeginGeneration()
		{
			var surface = surfaceDescriptor.GetAsset<SphericalSurface>();
			if (surface == null) surface = surfaceDescriptor.SetAsset(CreateInstance<SphericalSurface>(), false);
			surface.Reset(new SphericalDescriptor(Vector3.up, 1f));

			Topology topology;
			Vector3[] vertexPositions;

			switch (sphericalPolyhedron)
			{
				case SphericalPolyhedrons.Tetrahedron:
					SphericalManifoldUtility.CreateTetrahedron(surface.radius, out topology, out vertexPositions);
					break;
				case SphericalPolyhedrons.Cube:
					SphericalManifoldUtility.CreateCube(surface.radius, out topology, out vertexPositions);
					break;
				case SphericalPolyhedrons.Octahedron:
					SphericalManifoldUtility.CreateOctahedron(surface.radius, out topology, out vertexPositions);
					break;
				case SphericalPolyhedrons.Dodecahedron:
					if (subdivisionDegree == 0)
					{
						SphericalManifoldUtility.CreateDodecahedron(surface.radius, out topology, out vertexPositions);
					}
					else
					{
						SphericalManifoldUtility.CreateIcosahedron(surface.radius, out topology, out vertexPositions);
					}
					break;
				case SphericalPolyhedrons.Icosahedron:
					SphericalManifoldUtility.CreateIcosahedron(surface.radius, out topology, out vertexPositions);
					break;
				default:
					throw new System.NotImplementedException();
			}

			SphericalManifoldUtility.Subdivide(topology, vertexPositions.AsVertexAttribute(), subdivisionDegree, surface.radius, out topology, out vertexPositions);

			var alreadyDual = sphericalPolyhedron == SphericalPolyhedrons.Dodecahedron && subdivisionDegree != 0;
			if (useDualPolyhedron != alreadyDual)
			{
				SphericalManifoldUtility.MakeDual(topology, ref vertexPositions, surface.radius);
			}

			surfaceDescriptor.Persist();
			topologyDescriptor.SetAsset(topology);
			vertexPositionsDescriptor.SetAsset(Vector3VertexAttribute.Create(vertexPositions).SetName("Vertex Positions"));

			yield break;
		}

		public override float estimatedGenerationTime
		{
			get
			{
				return 0.15f;
			}
		}
	}
}
