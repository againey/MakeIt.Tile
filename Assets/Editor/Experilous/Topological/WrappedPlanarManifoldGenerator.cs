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

		public GeneratedAsset topology;
		public GeneratedAsset vertexPositions;
		public GeneratedAsset edgeWrapData;
		public GeneratedAsset wrappedVertexPositions;
		public GeneratedAsset vertexIndexer2D;
		public GeneratedAsset faceIndexer2D;
		public GeneratedAsset faceNeighborIndexer;

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
				if (edgeWrapData == null) edgeWrapData = EdgeWrapDataEdgeAttributeGeneratedAsset.CreateDefaultInstance(this, "Ege Wrap Data");
				if (wrappedVertexPositions == null) wrappedVertexPositions = Vector3OffsetWrappedVertexAttributeGeneratedAsset.CreateDefaultInstance(this, "Wrapped Vertex Positions");
				if (vertexIndexer2D == null) vertexIndexer2D = VertexIndexer2DGeneratedAsset.CreateDefaultInstance(this, "Vertex Indexer 2D");
				if (faceIndexer2D == null) faceIndexer2D = FaceIndexer2DGeneratedAsset.CreateDefaultInstance(this, "Face Indexer 2D");
				if (faceNeighborIndexer == null) faceNeighborIndexer = FaceNeighborIndexerGeneratedAsset.CreateDefaultInstance(this, "Face Neighbor Indexer");

				yield return topology;
				yield return vertexPositions;
				yield return edgeWrapData;
				yield return wrappedVertexPositions;
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
			IList<EdgeWrapData> edgeWrapData;
			Vector3[] repetitionAxes;

			var isWrappedHorizontally = (wrapOption & PlanarTwoAxisWrapOptions.Horizontal) != 0;
			var isWrappedVertically = (wrapOption & PlanarTwoAxisWrapOptions.Vertical) != 0;
			var vertexIndexer2D = WrappedRowMajorQuadGridVertexIndexer2D.CreateInstance(size.x, size.y, isWrappedHorizontally, isWrappedVertically);
			var faceIndexer2D = WrappedRowMajorQuadGridFaceIndexer2D.CreateInstance(size.x, size.y, isWrappedHorizontally, isWrappedVertically);
			var faceNeighborIndexer = WrappedRowMajorQuadGridFaceNeighborIndexer.CreateInstance(size.x, size.y, isWrappedHorizontally, isWrappedVertically);
			PlanarManifoldUtility.Generate(faceNeighborIndexer, vertexIndexer2D, horizontalAxis, verticalAxis, originPosition, out topology, out vertexPositions, out edgeWrapData, out repetitionAxes);

			this.topology.SetGeneratedInstance(location, name, topology);
			SetVertexPositions(location, name, vertexPositions);
			SetEdgeWrapData(location, name, edgeWrapData);

			var wrappedVertexPositions = Vector3OffsetWrappedVertexAttribute.CreateInstance(
				(Topology)this.topology.generatedInstance,
				(EdgeWrapDataEdgeAttribute)this.edgeWrapData.generatedInstance,
				(Vector3VertexAttribute)this.vertexPositions.generatedInstance,
				repetitionAxes);

			this.wrappedVertexPositions.SetGeneratedInstance(location, name, wrappedVertexPositions);

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
				vertexPositionsArray = new Vector3[((Topology)topology.generatedInstance).vertices.Count];
				vertexPositions.CopyTo(vertexPositionsArray, 0);
			}
			this.vertexPositions.SetGeneratedInstance(location, name, Vector3VertexAttribute.CreateInstance(vertexPositionsArray, "Vertex Positions"));
		}

		private void SetEdgeWrapData(string location, string name, IList<EdgeWrapData> edgeWrapData)
		{
			EdgeWrapData[] edgeWrapDataArray;
			if (edgeWrapData is EdgeWrapData[])
			{
				edgeWrapDataArray = (EdgeWrapData[])edgeWrapData;
			}
			else
			{
				edgeWrapDataArray = new EdgeWrapData[((Topology)topology.generatedInstance).faceEdges.Count];
				edgeWrapData.CopyTo(edgeWrapDataArray, 0);
			}
			this.edgeWrapData.SetGeneratedInstance(location, name, EdgeWrapDataEdgeAttribute.CreateInstance(edgeWrapDataArray, "Edge Wrap Data"));
		}
	}
}
