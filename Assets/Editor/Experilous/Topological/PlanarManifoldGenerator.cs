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
		public Index2D size = new Index2D(64, 64);
		public Vector3 horizontalAxis = new Vector3(1f, 0f, 0f);
		public Vector3 verticalAxis = new Vector3(0f, 1f, 0f);
		public Vector3 originPosition = new Vector3(0f, 0f, 0f);

		public GeneratedAsset topology;
		public GeneratedAsset vertexPositions;
		public GeneratedAsset vertexIndexer2D;
		public GeneratedAsset faceIndexer2D;
		public GeneratedAsset faceNeighborIndexer;

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
				if (vertexIndexer2D == null) vertexIndexer2D = VertexIndexer2DGeneratedAsset.CreateDefaultInstance(this, "Vertex Indexer 2D");
				if (faceIndexer2D == null) faceIndexer2D = FaceIndexer2DGeneratedAsset.CreateDefaultInstance(this, "Face Indexer 2D");
				if (faceNeighborIndexer == null) faceNeighborIndexer = FaceNeighborIndexerGeneratedAsset.CreateDefaultInstance(this, "Face Neighbor Indexer");

				yield return topology;
				yield return vertexPositions;
				yield return vertexIndexer2D;
				yield return faceIndexer2D;
				yield return faceNeighborIndexer;
			}
		}

		public override void ResetDependency(GeneratedAsset dependency)
		{
			if (dependency == null) throw new System.ArgumentNullException("dependency");
			throw new System.ArgumentException(string.Format("Generated asset \"{0}\" of type {1} is not a dependency of this planar manifold generator.", dependency.name, dependency.GetType().Name), "dependency");
		}

		public override void Generate(string location, string name)
		{
			switch (planarTileShape)
			{
				case PlanarTileShapes.Rectangular:
					CreateQuadGridManifold(location, name);
					break;
				default:
					throw new System.NotImplementedException();
			}
		}

		public override bool CanGenerate()
		{
			return
				size.x > 0 &&
				size.y > 0 &&
				horizontalAxis != new Vector3(0f, 0f, 0f) &&
				verticalAxis != new Vector3(0f, 0f, 0f);
		}

		private void CreateQuadGridManifold(string location, string name)
		{
			Topology topology;
			IList<Vector3> vertexPositions;

			var vertexIndexer2D = RowMajorQuadGridVertexIndexer2D.CreateInstance(size.x, size.y);
			var faceIndexer2D = RowMajorQuadGridFaceIndexer2D.CreateInstance(size.x, size.y);
			var faceNeighborIndexer = RowMajorQuadGridFaceNeighborIndexer.CreateInstance(size.x, size.y);
			PlanarManifoldUtility.Generate(faceNeighborIndexer, vertexIndexer2D, horizontalAxis, verticalAxis, originPosition, out topology, out vertexPositions);

			this.topology.SetGeneratedInstance(location, name, topology);
			SetVertexPositions(location, name, vertexPositions);

			vertexIndexer2D.topology = (Topology)this.topology.generatedInstance;
			faceIndexer2D.topology = (Topology)this.topology.generatedInstance;
			faceNeighborIndexer.topology = (Topology)this.topology.generatedInstance;

			this.vertexIndexer2D.SetGeneratedInstance(location, name, vertexIndexer2D);
			this.faceIndexer2D.SetGeneratedInstance(location, name, faceIndexer2D);
			this.faceNeighborIndexer.SetGeneratedInstance(location, name, faceNeighborIndexer);
		}

		private void SetVertexPositions(string location, string name, IList<Vector3> vertexPositions)
		{
			Vector3[] vertexPositionsArray;
			if (vertexPositions is Vector3[])
			{
				vertexPositionsArray = (Vector3[])vertexPositions;
			}
			else
			{
				vertexPositionsArray = new Vector3[((Topology)this.topology.generatedInstance).vertices.Count];
				vertexPositionsArray.CopyTo(vertexPositionsArray, 0);
			}
			this.vertexPositions.SetGeneratedInstance(location, name, Vector3VertexAttribute.CreateInstance(vertexPositionsArray, "Vertex Positions"));
		}
	}
}
