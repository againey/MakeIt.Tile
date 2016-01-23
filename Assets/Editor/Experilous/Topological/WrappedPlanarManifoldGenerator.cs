using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorBundle), typeof(TopologyCategory), "Wrapped Planar Manifold")]
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

		public AssetDescriptor topology;
		public AssetDescriptor vertexPositions;
		public AssetDescriptor edgeWrapData;
		public AssetDescriptor wrappedVertexPositions;
		public AssetDescriptor surfaceDescriptor;
		public AssetDescriptor vertexIndexer2D;
		public AssetDescriptor faceIndexer2D;
		public AssetDescriptor faceNeighborIndexer;

		public static WrappedPlanarManifoldGenerator CreateDefaultInstance(AssetGeneratorBundle bundle, string name)
		{
			var generator = CreateInstance<WrappedPlanarManifoldGenerator>();
			generator.bundle = bundle;
			generator.name = name;
			generator.hideFlags = HideFlags.HideInHierarchy;
			return generator;
		}

		[MenuItem("Assets/Create/Topology/Planar Manifold Generator")]
		public static void CreateDefaultGeneratorBundle()
		{
			var bundle = TopologyGeneratorBundle.CreateDefaultInstance("New Wrapped Planar Manifold");
			bundle.Add(CreateDefaultInstance(bundle, "Planar Manifold"));
			bundle.CreateAsset();
		}

		public override IEnumerable<AssetDescriptor> dependencies
		{
			get
			{
				yield break;
			}
		}

		public override IEnumerable<AssetDescriptor> outputs
		{
			get
			{
				if (topology == null) topology = AssetDescriptor.Create(this, typeof(Topology), "Topology", "Descriptors");
				if (vertexPositions == null) vertexPositions = AssetDescriptor.Create(this, typeof(IVertexAttribute<Vector3>), "Vertex Positions", "Attributes");
				if (edgeWrapData == null) edgeWrapData = AssetDescriptor.Create(this, typeof(IEdgeAttribute<EdgeWrap>), "Ege Wrap Data", "Attributes");
				if (wrappedVertexPositions == null) wrappedVertexPositions = AssetDescriptor.Create(this, typeof(IEdgeAttribute<Vector3>), "Wrapped Vertex Positions", "Attributes");
				if (surfaceDescriptor == null) surfaceDescriptor = AssetDescriptor.Create(this, typeof(PlanarSurfaceDescriptor), "Surface Descriptor", "Descriptors");
				if (vertexIndexer2D == null) vertexIndexer2D = AssetDescriptor.Create(this, typeof(VertexIndexer2D), "Vertex Indexer 2D", "Descriptors");
				if (faceIndexer2D == null) faceIndexer2D = AssetDescriptor.Create(this, typeof(FaceIndexer2D), "Face Indexer 2D", "Descriptors");
				if (faceNeighborIndexer == null) faceNeighborIndexer = AssetDescriptor.Create(this, typeof(FaceNeighborIndexer), "Face Neighbor Indexer", "Descriptors");

				yield return topology;
				yield return vertexPositions;
				yield return edgeWrapData;
				yield return wrappedVertexPositions;
				yield return surfaceDescriptor;
				yield return vertexIndexer2D;
				yield return faceIndexer2D;
				yield return faceNeighborIndexer;
			}
		}

		public override void ResetDependency(AssetDescriptor dependency)
		{
			if (dependency == null) throw new System.ArgumentNullException("dependency");
			throw new System.ArgumentException(string.Format("Generated asset \"{0}\" of type {1} is not a dependency of this planar manifold generator.", dependency.name, dependency.GetType().Name), "dependency");
		}

		public override void Generate()
		{
			switch (planarTileShape)
			{
				case PlanarTileShapes.Rectangular:
					CreateQuadGridManifold();
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

		private void CreateQuadGridManifold()
		{
			Topology topologyAsset;
			Vector3[] vertexPositionsArray;
			EdgeWrap[] edgeWrapDataArray;

			var isWrappedHorizontally = (wrapOption & PlanarTwoAxisWrapOptions.Horizontal) != 0;
			var isWrappedVertically = (wrapOption & PlanarTwoAxisWrapOptions.Vertical) != 0;
			var surfaceDescriptorAsset = PlanarSurfaceDescriptor.Create(horizontalAxis * size.x, isWrappedHorizontally, verticalAxis * size.y, isWrappedVertically);
			var vertexIndexer2DAsset = WrappedRowMajorQuadGridVertexIndexer2D.CreateInstance(size.x, size.y, isWrappedHorizontally, isWrappedVertically);
			var faceIndexer2DAsset = WrappedRowMajorQuadGridFaceIndexer2D.CreateInstance(size.x, size.y, isWrappedHorizontally, isWrappedVertically);
			var faceNeighborIndexerAsset = WrappedRowMajorQuadGridFaceNeighborIndexer.CreateInstance(size.x, size.y, isWrappedHorizontally, isWrappedVertically);
			PlanarManifoldUtility.Generate(faceNeighborIndexerAsset, vertexIndexer2DAsset, surfaceDescriptorAsset, originPosition, out topologyAsset, out vertexPositionsArray, out edgeWrapDataArray);

			topology.SetAsset(topologyAsset);
			vertexPositions.SetAsset(Vector3VertexAttribute.CreateInstance(vertexPositionsArray));
			edgeWrapData.SetAsset(EdgeWrapDataEdgeAttribute.CreateInstance(edgeWrapDataArray));

			surfaceDescriptor.SetAsset(surfaceDescriptorAsset);

			var wrappedVertexPositionsAsset = Vector3OffsetWrappedVertexAttribute.CreateInstance(
				topology.GetAsset<Topology>(),
				edgeWrapData.GetAsset<EdgeWrapDataEdgeAttribute>(),
				vertexPositions.GetAsset<Vector3VertexAttribute>(),
				surfaceDescriptor.GetAsset<PlanarSurfaceDescriptor>());

			wrappedVertexPositions.SetAsset(wrappedVertexPositionsAsset);

			topologyAsset = topology.GetAsset<Topology>();
			vertexIndexer2DAsset.topology = topologyAsset;
			faceIndexer2DAsset.topology = topologyAsset;
			faceNeighborIndexerAsset.topology = topologyAsset;

			vertexIndexer2D.SetAsset(vertexIndexer2DAsset);
			faceIndexer2D.SetAsset(faceIndexer2DAsset);
			faceNeighborIndexer.SetAsset(faceNeighborIndexerAsset);
		}
	}
}
