using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorBundle), typeof(TopologyCategory), "Planar Manifold")]
	public class PlanarManifoldGenerator : AssetGenerator
	{
		public enum PlanarTileShapes
		{
			Rectangular,
			TriangularRows,
			TriangularColumns,
			HexagonalRows,
			HexagonalColumns,
		}

		public PlanarTileShapes planarTileShape = PlanarTileShapes.Rectangular;
		public int columns = 64;
		public int rows = 64;

		public GeneratedAsset topology;
		public GeneratedAsset vertexPositions;

		public static PlanarManifoldGenerator CreateDefaultInstance(AssetGeneratorBundle bundle, string name)
		{
			var generator = CreateInstance<PlanarManifoldGenerator>();
			generator.bundle = bundle;
			generator.name = name;
			generator.hideFlags = HideFlags.HideInHierarchy;
			return generator;
		}

		[MenuItem("Assets/Create/Topology/Planar Manifold Generator")]
		public static void CreateDefaultGeneratorBundle()
		{
			var bundle = TopologyGeneratorBundle.CreateDefaultInstance("New Planar Manifold");
			bundle.Add(CreateDefaultInstance(bundle, "Planar Manifold"));
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
			throw new System.ArgumentException(string.Format("Generated asset \"{0}\" of type {1} is not a dependency of this planar manifold generator.", dependency.name, dependency.GetType().Name), "dependency");
		}

		public override void Generate(string location, string name)
		{
			Topology topology;
			IList<Vector3> vertexPositions;

			switch (planarTileShape)
			{
				case PlanarTileShapes.Rectangular:
				{
					var faceNeighborIndexer = RowMajorQuadGridFaceNeighborIndexer.CreateInstance(columns, rows);
					var vertexIndexer = RowMajorQuadGridVertexIndexer2D.CreateInstance(columns, rows);
					PlanarManifoldUtility.Generate(faceNeighborIndexer, vertexIndexer, new Vector3(1f, 0f, 0f), new Vector3(0f, 1f, 0f), new Vector3(0f, 0f, 0f), out topology, out vertexPositions);
					break;
				}
				default:
					throw new System.NotImplementedException();
			}

			this.topology.SetGeneratedInstance(location, name, topology);

			if (this.vertexPositions.isEnabled)
			{
				Vector3[] vertexPositionsArray;
				if (vertexPositions is Vector3[])
				{
					vertexPositionsArray = (Vector3[])vertexPositions;
				}
				else
				{
					vertexPositionsArray = new Vector3[topology.vertices.Count];
					vertexPositionsArray.CopyTo(vertexPositionsArray, 0);
				}
				this.vertexPositions.SetGeneratedInstance(location, name, Vector3VertexAttribute.CreateInstance(vertexPositionsArray, "Vertex Positions"));
			}
			else
			{
				this.vertexPositions.ClearGeneratedInstance();
			}
		}

		public override bool CanGenerate()
		{
			return true;
		}
	}
}
