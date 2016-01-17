﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Experilous.Topological
{
#if false
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

		public TopologyGeneratedAsset topology;
		public Vector3VertexAttributeGeneratedAsset vertexPositions;
		public EdgeWrapDataEdgeAttributeGeneratedAsset edgeWrapData;
		public FaceIndexer2DGeneratedAsset faceIndexer;

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
				if (faceIndexer == null) faceIndexer = FaceIndexer2DGeneratedAsset.CreateOptionalInstance(this, "Face Indexer");

				yield return topology;
				yield return vertexPositions;
				yield return edgeWrapData;
				yield return faceIndexer;
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
					throw new System.NotImplementedException();
			}

			topology.SetGeneratedInstance(location, name, manifold);
			vertexPositions.SetGeneratedInstance(location, name, manifold.vertexPositions);
			edgeWrapData.SetGeneratedInstance(location, name, manifold.edgeWrapData);

			if (faceIndexer.isEnabled)
			{
				switch (planarTileShape)
				{
					case PlanarTileShapes.Rectangular:
						faceIndexer.SetGeneratedInstance(location, name, PlanarManifoldUtility.CreateQuadGridFaceIndexer2D(topology.generatedInstance, columns, rows));
						break;
					default:
						throw new System.NotImplementedException();
				}
			}
			else
			{
				faceIndexer.ClearGeneratedInstance();
			}
		}

		public override bool CanGenerate()
		{
			return true;
		}
	}
#endif
}
