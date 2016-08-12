/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Experilous.MakeIt.Generate;
using Experilous.Core;

namespace Experilous.MakeIt.Tile
{
	[Generator(typeof(TopologyGeneratorCollection), "Topology/Spherical Manifold")]
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

			var manifoldGenerator = collection.Add(CreateInstance<SphericalManifoldGenerator>(collection));
			collection.Add(CreateInstance<RandomnessGenerator>(collection));
			var faceCentroidsGenerator = collection.Add(CreateInstance<FaceCentroidsGenerator>(collection));
			var faceNormalsGenerator = collection.Add(CreateInstance<FaceNormalsGenerator>(collection));
			var vertexNormalsGenerator = collection.Add(CreateInstance<VertexNormalsGenerator>(collection));
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

			vertexNormalsGenerator.calculationMethod = VertexNormalsGenerator.CalculationMethod.FromSurfaceNormal;
			vertexNormalsGenerator.topologyInputSlot.source = manifoldGenerator.topologyOutputSlot;
			vertexNormalsGenerator.surfaceInputSlot.source = manifoldGenerator.surfaceOutputSlot;
			vertexNormalsGenerator.vertexPositionsInputSlot.source = manifoldGenerator.vertexPositionsOutputSlot;

			meshGenerator.sourceType = MeshGenerator.SourceType.InternalFaces;
			meshGenerator.topologyInputSlot.source = manifoldGenerator.topologyOutputSlot;
			meshGenerator.triangulation = MeshGenerator.Triangulation.Umbrella;
			meshGenerator.vertexAttributes = DynamicMesh.VertexAttributes.Position | DynamicMesh.VertexAttributes.Normal;
			meshGenerator.ringDepth = 1;
			meshGenerator.UpdateVertexAttributeInputSlots();
			meshGenerator.ringVertexPositionsInputSlots[0].source = manifoldGenerator.vertexPositionsOutputSlot;
			meshGenerator.centerVertexPositionsInputSlots[0].source = faceCentroidsGenerator.faceCentroidsOutputSlot;
			meshGenerator.ringVertexNormalsInputSlots[0].source = vertexNormalsGenerator.vertexNormalsOutputSlot;
			meshGenerator.centerVertexNormalsInputSlots[0].source = faceNormalsGenerator.faceNormalsOutputSlot;

			prefabGenerator.dynamicMeshInputSlot.source = meshGenerator.dynamicMeshOutputSlot;

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
