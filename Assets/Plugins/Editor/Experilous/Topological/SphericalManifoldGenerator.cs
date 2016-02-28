/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Experilous.Generation;

namespace Experilous.Topological
{
	[Generator(typeof(TopologyGeneratorCollection), "Topologies/Spherical Manifold")]
	public class SphericalManifoldGenerator : Generator
	{
		public enum SphericalPolyhedrons
		{
			Tetrahedron,
			Cube,
			Octahedron,
			Dodecahedron,
			Icosahedron,
		}

		public SphericalPolyhedrons sphericalPolyhedron;
		public float radius;
		public int subdivisionDegree;
		public bool useDualPolyhedron;

		public OutputSlot surfaceOutputSlot;
		public OutputSlot topologyOutputSlot;
		public OutputSlot vertexPositionsOutputSlot;

		[MenuItem("Assets/Create/Topology/Spherical Manifold Generator")]
		public static void CreateDefaultGeneratorCollection()
		{
			var collection = TopologyGeneratorCollection.Create("New Spherical Manifold");

			var manifoldGenerator = collection.Add(CreateInstance<SphericalManifoldGenerator>(collection, "Manifold"));
			collection.Add(CreateInstance<RandomEngineGenerator>(collection));
			var faceCentroidsGenerator = collection.Add(CreateInstance<FaceCentroidsGenerator>(collection));
			var faceNormalsGenerator = collection.Add(CreateInstance<FaceNormalsGenerator>(collection));
			var meshGenerator = collection.Add(CreateInstance<MeshGenerator>(collection));
			var prefabGenerator = collection.Add(CreateInstance<PrefabGenerator>(collection));
			var faceSpatialPartitioningGenerator = collection.Add(CreateInstance<FaceSpatialPartitioningGenerator>(collection));

			faceCentroidsGenerator.topologyInputSlot.source = manifoldGenerator.topologyOutputSlot;
			faceCentroidsGenerator.surfaceInputSlot.source = manifoldGenerator.surfaceOutputSlot;
			faceCentroidsGenerator.vertexPositionsInputSlot.source = manifoldGenerator.vertexPositionsOutputSlot;

			faceNormalsGenerator.calculationMethod = FaceNormalsGenerator.CalculationMethod.FromSurfaceNormal;
			faceNormalsGenerator.topologyInputSlot.source = manifoldGenerator.topologyOutputSlot;
			faceNormalsGenerator.surfaceInputSlot.source = manifoldGenerator.surfaceOutputSlot;
			faceNormalsGenerator.facePositionsInputSlot.source = faceCentroidsGenerator.faceCentroidsOutputSlot;

			meshGenerator.sourceType = MeshGenerator.SourceType.InternalFaces;
			meshGenerator.topologyInputSlot.source = manifoldGenerator.topologyOutputSlot;
			meshGenerator.vertexPositionsInputSlot.source = manifoldGenerator.vertexPositionsOutputSlot;
			meshGenerator.faceCentroidsInputSlot.source = faceCentroidsGenerator.faceCentroidsOutputSlot;
			meshGenerator.faceNormalsInputSlot.source = faceNormalsGenerator.faceNormalsOutputSlot;
			meshGenerator.centerOnGroupAverage = true;

			prefabGenerator.meshCollectionInputSlot.source = meshGenerator.meshCollectionOutputSlot;

			faceSpatialPartitioningGenerator.topologyInputSlot.source = manifoldGenerator.topologyOutputSlot;
			faceSpatialPartitioningGenerator.surfaceInputSlot.source = manifoldGenerator.surfaceOutputSlot;
			faceSpatialPartitioningGenerator.vertexPositionsInputSlot.source = manifoldGenerator.vertexPositionsOutputSlot;

			collection.CreateAsset();
		}

		protected override void Initialize()
		{
			// Fields
			sphericalPolyhedron = SphericalPolyhedrons.Icosahedron;
			radius = 1f;
			subdivisionDegree = 10;
			useDualPolyhedron = true;

			// Outputs
			OutputSlot.CreateOrReset<SphericalSurface>(ref surfaceOutputSlot, this, "Surface");
			OutputSlot.CreateOrReset<Topology>(ref topologyOutputSlot, this, "Topology");
			OutputSlot.CreateOrResetGrouped<IVertexAttribute<Vector3>>(ref vertexPositionsOutputSlot, this, "Vertex Positions", "Attributes");
		}

		public override IEnumerable<OutputSlot> outputs
		{
			get
			{
				yield return surfaceOutputSlot;
				yield return topologyOutputSlot;
				yield return vertexPositionsOutputSlot;
			}
		}

		protected override void OnUpdate()
		{
			var surface = surfaceOutputSlot.GetAsset<SphericalSurface>();
			if (surface == null) surface = surfaceOutputSlot.SetAsset(CreateInstance<SphericalSurface>(), false);
			surface.Reset(new SphericalDescriptor(Vector3.up, 1f));
		}

		public override IEnumerator BeginGeneration()
		{
			var surface = surfaceOutputSlot.GetAsset<SphericalSurface>();
			if (surface == null) surface = surfaceOutputSlot.SetAsset(CreateInstance<SphericalSurface>(), false);
			surface.Reset(new SphericalDescriptor(Vector3.up, radius));

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

			surfaceOutputSlot.Persist();
			topologyOutputSlot.SetAsset(topology);
			vertexPositionsOutputSlot.SetAsset(Vector3VertexAttribute.Create(vertexPositions).SetName("Vertex Positions"));

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
