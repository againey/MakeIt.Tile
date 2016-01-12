using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorBundle), typeof(TopologyCategory), "Planar Wrap-Around Manifold")]
	public class PlanarWrapAroundManifoldGenerator : AssetGenerator
	{
		public enum PlanarTileShapes
		{
			Rectangular,
			TriangularRows,
			TriangularColumns,
			HexagonalRows,
			HexagonalColumns,
		}

		[System.Flags]
		public enum PlanarTwoAxisWrapAroundOptions
		{
			Horizontal = 1,
			Vertical = 2,
			Both = Horizontal | Vertical,
		}

		public PlanarTileShapes planarTileShape = PlanarTileShapes.Rectangular;
		public int columns = 64;
		public int rows = 64;
		public PlanarTwoAxisWrapAroundOptions planarTwoAxisWrapAroundOption = PlanarTwoAxisWrapAroundOptions.Both;

		public GeneratedAsset topology;
		public GeneratedAsset vertexPositions;
		public GeneratedAsset edgeWrapData;

		public static PlanarWrapAroundManifoldGenerator CreateDefaultInstance(AssetGeneratorBundle bundle, string name)
		{
			var generator = CreateInstance<PlanarWrapAroundManifoldGenerator>();
			generator.bundle = bundle;
			generator.name = name;
			generator.hideFlags = HideFlags.HideInHierarchy;
			return generator;
		}

		[MenuItem("Assets/Create/Topology/Planar Wrap-Around Manifold Generator")]
		public static void CreateDefaultGeneratorBundle()
		{
			var bundle = TopologyGeneratorBundle.CreateDefaultInstance("New Planar Wrap-Around Manifold");
			bundle.Add(CreateDefaultInstance(bundle, "Planar Wrap-Around Manifold"));
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
				if (edgeWrapData == null) edgeWrapData = EdgeWrapDataEdgeAttributeGeneratedAsset.CreateDefaultInstance(this, "Edge Wrap Data");

				yield return topology;
				yield return vertexPositions;
				yield return edgeWrapData;
			}
		}

		public override void ResetDependency(GeneratedAsset dependency)
		{
			if (dependency == null) throw new System.ArgumentNullException("dependency");
			throw new System.ArgumentException(string.Format("Generated asset \"{0}\" of type {1} is not a dependency of this planar wrap-around manifold generator.", dependency.name, dependency.GetType().Name), "dependency");
		}

		public override void Generate(string location, string name)
		{
			PlanarWrapAroundManifold manifold;
			switch (planarTileShape)
			{
				case PlanarTileShapes.Rectangular:
					switch (planarTwoAxisWrapAroundOption)
					{
						case PlanarTwoAxisWrapAroundOptions.Horizontal:
							manifold = PlanarManifoldUtility.CreateHorizontalWrapAroundQuadGrid(columns, rows);
							break;
						case PlanarTwoAxisWrapAroundOptions.Vertical:
							manifold = PlanarManifoldUtility.CreateVerticalWrapAroundQuadGrid(columns, rows);
							break;
						case PlanarTwoAxisWrapAroundOptions.Both:
							manifold = PlanarManifoldUtility.CreateFullWrapAroundQuadGrid(columns, rows);
							break;
						default:
							throw new System.NotImplementedException();
					}
					break;
				default:
					topology = null;
					vertexPositions = null;
					edgeWrapData = null;
					return;
			}

			topology.SetGeneratedInstance(location, name, manifold);
			vertexPositions.SetGeneratedInstance(location, name, manifold.vertexPositions);
			edgeWrapData.SetGeneratedInstance(location, name, manifold.edgeWrapData);
		}

		public override bool CanGenerate()
		{
			return true;
		}
	}
}
