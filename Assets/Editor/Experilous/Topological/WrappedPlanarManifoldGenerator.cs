using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorCollection), typeof(TopologyCategory), "Wrapped Planar Manifold")]
	public class WrappedPlanarManifoldGenerator : AssetGenerator
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
		public enum PlanarTwoAxisWrapOptions
		{
			Horizontal = 1,
			Vertical = 2,
			Both = Horizontal | Vertical,
		}

		public PlanarTileShapes planarTileShape = PlanarTileShapes.Rectangular;
		public PlanarTwoAxisWrapOptions wrapOption = PlanarTwoAxisWrapOptions.Both;
		public Index2D size = new Index2D(64, 64);
		public Vector3 horizontalAxis = new Vector3(1f, 0f, 0f);
		public Vector3 verticalAxis = new Vector3(0f, 1f, 0f);
		public Vector3 originPosition = new Vector3(0f, 0f, 0f);

		public AssetDescriptor topologyDescriptor;
		public AssetDescriptor edgeWrapDataDescriptor;
		public AssetDescriptor surfaceDescriptorDescriptor;
		public AssetDescriptor positionalAttributeAdapterDescriptor;
		public AssetDescriptor vertexPositionsDescriptor;
		public AssetDescriptor wrappedVertexPositionsDescriptor;
		public AssetDescriptor vertexIndexer2DDescriptor;
		public AssetDescriptor faceIndexer2DDescriptor;
		public AssetDescriptor faceNeighborIndexerDescriptor;

		[MenuItem("Assets/Create/Topology/Wrapped Planar Manifold Generator")]
		public static void CreateDefaultGeneratorCollection()
		{
			var collection = TopologyGeneratorCollection.Create("New Wrapped Planar Manifold");
			collection.Add(CreateInstance<WrappedPlanarManifoldGenerator>(collection, "Manifold"));
			collection.CreateAsset();
		}

		protected override void Initialize(bool reset = true)
		{
			// Outputs
			if (reset || topologyDescriptor == null) topologyDescriptor = AssetDescriptor.CreateGrouped<Topology>(this, "Topology", "Descriptors");
			if (reset || edgeWrapDataDescriptor == null) edgeWrapDataDescriptor = AssetDescriptor.CreateGrouped<IEdgeAttribute<EdgeWrap>>(this, "Ege Wrap Data", "Attributes");
			if (reset || surfaceDescriptorDescriptor == null) surfaceDescriptorDescriptor = AssetDescriptor.CreateGrouped<PlanarSurfaceDescriptor>(this, "Surface Descriptor", "Descriptors");
			if (reset || positionalAttributeAdapterDescriptor == null) positionalAttributeAdapterDescriptor = AssetDescriptor.CreateGrouped<PositionalAttributeAdapter>(this, "Positional Attribute Adapter", "Descriptors");
			if (reset || vertexPositionsDescriptor == null) vertexPositionsDescriptor = AssetDescriptor.CreateGrouped<IVertexAttribute<Vector3>>(this, "Vertex Positions", "Attributes");
			if (reset || wrappedVertexPositionsDescriptor == null) wrappedVertexPositionsDescriptor = AssetDescriptor.CreateGrouped<IVertexAttribute<Vector3>>(this, "Wrapped Vertex Positions", "Attributes");
			if (reset || vertexIndexer2DDescriptor == null) vertexIndexer2DDescriptor = AssetDescriptor.CreateGrouped<VertexIndexer2D>(this, "Vertex Indexer 2D", "Descriptors");
			if (reset || faceIndexer2DDescriptor == null) faceIndexer2DDescriptor = AssetDescriptor.CreateGrouped<FaceIndexer2D>(this, "Face Indexer 2D", "Descriptors");
			if (reset || faceNeighborIndexerDescriptor == null) faceNeighborIndexerDescriptor = AssetDescriptor.CreateGrouped<FaceNeighborIndexer>(this, "Face Neighbor Indexer", "Descriptors");
		}

		public override IEnumerable<AssetDescriptor> outputs
		{
			get
			{
				yield return topologyDescriptor;
				yield return edgeWrapDataDescriptor;
				yield return surfaceDescriptorDescriptor;
				if (edgeWrapDataDescriptor.isAvailableDuringGeneration &&
					surfaceDescriptorDescriptor.isAvailableDuringGeneration)
					yield return positionalAttributeAdapterDescriptor;
				yield return vertexPositionsDescriptor;
				if (edgeWrapDataDescriptor.isAvailableDuringGeneration &&
					vertexPositionsDescriptor.isAvailableDuringGeneration &&
					surfaceDescriptorDescriptor.isAvailableDuringGeneration)
					yield return wrappedVertexPositionsDescriptor;

				if (topologyDescriptor.isAvailableDuringGeneration)
					yield return vertexIndexer2DDescriptor;
				if (topologyDescriptor.isAvailableDuringGeneration)
					yield return faceIndexer2DDescriptor;
				if (topologyDescriptor.isAvailableDuringGeneration)
					yield return faceNeighborIndexerDescriptor;
			}
		}

		public override IEnumerable<AssetReferenceDescriptor> references
		{
			get
			{
				yield return positionalAttributeAdapterDescriptor.References(edgeWrapDataDescriptor);
				yield return positionalAttributeAdapterDescriptor.References(surfaceDescriptorDescriptor);

				yield return wrappedVertexPositionsDescriptor.References(edgeWrapDataDescriptor);
				yield return wrappedVertexPositionsDescriptor.References(vertexPositionsDescriptor);
				yield return wrappedVertexPositionsDescriptor.References(surfaceDescriptorDescriptor);

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
			Topology topology;
			Vector3[] vertexPositions;
			EdgeWrap[] edgeWrap;

			var isWrappedHorizontally = (wrapOption & PlanarTwoAxisWrapOptions.Horizontal) != 0;
			var isWrappedVertically = (wrapOption & PlanarTwoAxisWrapOptions.Vertical) != 0;
			var surfaceDescriptor = PlanarSurfaceDescriptor.Create(horizontalAxis * size.x, isWrappedHorizontally, verticalAxis * size.y, isWrappedVertically);
			var vertexIndexer2D = WrappedRowMajorQuadGridVertexIndexer2D.CreateInstance(size.x, size.y, isWrappedHorizontally, isWrappedVertically);
			var faceIndexer2D = WrappedRowMajorQuadGridFaceIndexer2D.CreateInstance(size.x, size.y, isWrappedHorizontally, isWrappedVertically);
			var faceNeighborIndexer = WrappedRowMajorQuadGridFaceNeighborIndexer.CreateInstance(size.x, size.y, isWrappedHorizontally, isWrappedVertically);
			PlanarManifoldUtility.Generate(faceNeighborIndexer, vertexIndexer2D, surfaceDescriptor, originPosition, out topology, out vertexPositions, out edgeWrap);

			topologyDescriptor.SetAsset(topology);
			vertexPositionsDescriptor.SetAsset(Vector3VertexAttribute.CreateInstance(vertexPositions));
			edgeWrapDataDescriptor.SetAsset(EdgeWrapDataEdgeAttribute.CreateInstance(edgeWrap));

			surfaceDescriptorDescriptor.SetAsset(surfaceDescriptor);

			var positionalAttributeAdapter = EdgeWrapPositionalAttributeAdapter.Create(
				surfaceDescriptorDescriptor.GetAsset<PlanarSurfaceDescriptor>(),
				edgeWrapDataDescriptor.GetAsset<IEdgeAttribute<EdgeWrap>>());
			positionalAttributeAdapterDescriptor.SetAsset(positionalAttributeAdapter);

			var wrappedVertexPositions = positionalAttributeAdapter.Adapt(vertexPositionsDescriptor.GetAsset<IVertexAttribute<Vector3>>());
			wrappedVertexPositionsDescriptor.SetAsset(wrappedVertexPositions);

			topology = topologyDescriptor.GetAsset<Topology>();
			vertexIndexer2D.topology = topology;
			faceIndexer2D.topology = topology;
			faceNeighborIndexer.topology = topology;

			vertexIndexer2DDescriptor.SetAsset(vertexIndexer2D);
			faceIndexer2DDescriptor.SetAsset(faceIndexer2D);
			faceNeighborIndexerDescriptor.SetAsset(faceNeighborIndexer);
		}
	}
}
