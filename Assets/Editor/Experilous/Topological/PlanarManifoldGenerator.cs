using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorCollection), typeof(TopologyCategory), "Planar Manifold")]
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

		public AssetDescriptor topologyDescriptor;
		public AssetDescriptor surfaceDescriptorDescriptor;
		public AssetDescriptor vertexPositionsDescriptor;
		public AssetDescriptor vertexIndexer2DDescriptor;
		public AssetDescriptor faceIndexer2DDescriptor;
		public AssetDescriptor faceNeighborIndexerDescriptor;

		[MenuItem("Assets/Create/Topology/Planar Manifold Generator")]
		public static void CreateDefaultGeneratorCollection()
		{
			var collection = TopologyGeneratorCollection.Create("New Planar Manifold");
			collection.Add(CreateInstance<PlanarManifoldGenerator>(collection, "Manifold"));
			collection.CreateAsset();
		}

		protected override void Initialize(bool reset = true)
		{
			// Outputs
			if (reset || topologyDescriptor == null) topologyDescriptor = AssetDescriptor.CreateGrouped<Topology>(this, "Topology", "Descriptors");
			if (reset || surfaceDescriptorDescriptor == null) surfaceDescriptorDescriptor = AssetDescriptor.CreateGrouped<PlanarSurfaceDescriptor>(this, "Surface Descriptor", "Descriptors");
			if (reset || vertexPositionsDescriptor == null) vertexPositionsDescriptor = AssetDescriptor.CreateGrouped<IVertexAttribute<Vector3>>(this, "Vertex Positions", "Attributes");
			if (reset || vertexIndexer2DDescriptor == null) vertexIndexer2DDescriptor = AssetDescriptor.CreateGrouped<VertexIndexer2D>(this, "Vertex Indexer 2D", "Descriptors");
			if (reset || faceIndexer2DDescriptor == null) faceIndexer2DDescriptor = AssetDescriptor.CreateGrouped<FaceIndexer2D>(this, "Face Indexer 2D", "Descriptors");
			if (reset || faceNeighborIndexerDescriptor == null) faceNeighborIndexerDescriptor = AssetDescriptor.CreateGrouped<FaceNeighborIndexer>(this, "Face Neighbor Indexer", "Descriptors");
		}

		public override IEnumerable<AssetDescriptor> outputs
		{
			get
			{
				yield return topologyDescriptor;
				yield return vertexPositionsDescriptor;
				yield return surfaceDescriptorDescriptor;
				yield return vertexIndexer2DDescriptor;
				yield return faceIndexer2DDescriptor;
				yield return faceNeighborIndexerDescriptor;
			}
		}

		public override IEnumerable<AssetReferenceDescriptor> references
		{
			get
			{
				yield return topologyDescriptor.ReferencedBy(vertexIndexer2DDescriptor);
				yield return topologyDescriptor.ReferencedBy(faceIndexer2DDescriptor);
				yield return topologyDescriptor.ReferencedBy(faceNeighborIndexerDescriptor);
			}
		}

		public override IEnumerator BeginGeneration()
		{
			switch (planarTileShape)
			{
				case PlanarTileShapes.Rectangular:
					CreateQuadGridManifold();
					break;
				default:
					throw new System.NotImplementedException();
			}

			yield break;
		}

		public override bool canGenerate
		{
			get
			{
				return
					size.x > 0 &&
					size.y > 0 &&
					horizontalAxis != new Vector3(0f, 0f, 0f) &&
					verticalAxis != new Vector3(0f, 0f, 0f) &&
					Mathf.Abs(Vector3.Dot(horizontalAxis, verticalAxis)) < 0.99f; //Axes are not nearly parallel
			}
		}

		public override float estimatedGenerationTime
		{
			get
			{
				return 0.1f;
			}
		}

		private void CreateQuadGridManifold()
		{
			Topology topologyAsset;
			Vector3[] vertexPositionsArray;

			var surfaceDescriptorAsset = PlanarSurfaceDescriptor.Create(horizontalAxis * size.x, false, verticalAxis * size.y, false);
			var vertexIndexer2DAsset = RowMajorQuadGridVertexIndexer2D.CreateInstance(size.x, size.y);
			var faceIndexer2DAsset = RowMajorQuadGridFaceIndexer2D.CreateInstance(size.x, size.y);
			var faceNeighborIndexerAsset = RowMajorQuadGridFaceNeighborIndexer.CreateInstance(size.x, size.y);
			PlanarManifoldUtility.Generate(faceNeighborIndexerAsset, vertexIndexer2DAsset, surfaceDescriptorAsset, originPosition, out topologyAsset, out vertexPositionsArray);

			topologyDescriptor.SetAsset(topologyAsset);
			vertexPositionsDescriptor.SetAsset(Vector3VertexAttribute.CreateInstance(vertexPositionsArray));

			surfaceDescriptorDescriptor.SetAsset(surfaceDescriptorAsset);

			topologyAsset = topologyDescriptor.GetAsset<Topology>();
			vertexIndexer2DAsset.topology = topologyAsset;
			faceIndexer2DAsset.topology = topologyAsset;
			faceNeighborIndexerAsset.topology = topologyAsset;

			vertexIndexer2DDescriptor.SetAsset(vertexIndexer2DAsset);
			faceIndexer2DDescriptor.SetAsset(faceIndexer2DAsset);
			faceNeighborIndexerDescriptor.SetAsset(faceNeighborIndexerAsset);
		}
	}
}
