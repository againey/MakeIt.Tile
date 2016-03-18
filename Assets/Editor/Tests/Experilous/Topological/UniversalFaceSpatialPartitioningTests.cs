/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

#if UNITY_5_3
using UnityEngine;
using NUnit.Framework;
using Experilous.Randomization;

namespace Experilous.Topological.Tests
{
	public class UniversalFaceSpatialPartitioningTests
	{
		Surface surface;
		Topology topology;
		IVertexAttribute<Vector3> vertexPositions;
		IEdgeAttribute<Vector3> edgeMidpoints;
		IFaceAttribute<Vector3> facePositions;
		UniversalFaceSpatialPartitioning partitioning;

		#region Validators

		private void FaceCenterIntersections()
		{
			foreach (var face in topology.internalFaces)
			{
				var position = facePositions[face];
				var normal = surface.GetNormal(position);
				var ray = new ScaledRay(position + normal, -normal);
				var foundFace = partitioning.FindFace(ray);
				Assert.AreEqual(face, foundFace, string.Format("Face center: {0}", facePositions[face]));
			}
		}

		private void FaceVertexWeightedIntersections()
		{
			foreach (var face in topology.internalFaces)
			{
				foreach (var edge in face.edges)
				{
					var position = (vertexPositions[edge] * 4f + facePositions[face]) * 0.2f;
					var normal = surface.GetNormal(position);
					var ray = new ScaledRay(position + normal, -normal);
					var foundFace = partitioning.FindFace(ray);
					Assert.AreEqual(face, foundFace, string.Format("Face center: {0}, Ray target: {1}, Vertex position: {2}", facePositions[face], position, vertexPositions[edge]));
				}
			}
		}

		private void FaceEdgeMidpointWeightedIntersections()
		{
			foreach (var face in topology.internalFaces)
			{
				foreach (var edge in face.edges)
				{
					var position = (edgeMidpoints[edge] * 4f + facePositions[face]) * 0.2f;
					var normal = surface.GetNormal(position);
					var ray = new ScaledRay(position + normal, -normal);
					var foundFace = partitioning.FindFace(ray);
					Assert.AreEqual(face, foundFace, string.Format("Face center: {0}, Ray target: {1}, Edge midpoint: {2}", facePositions[face], position, edgeMidpoints[edge]));
				}
			}
		}

		#endregion

		#region Creation Helpers

		public void CreateQuadGrid(int width, int height)
		{
			surface = RectangularQuadGrid.Create(new PlanarDescriptor(Vector3.right, Vector3.up), new Index2D(width, height));
			Vector3[] vertexPositionsArray;
			topology = ((RectangularQuadGrid)surface).CreateManifold(out vertexPositionsArray);
			vertexPositions = vertexPositionsArray.AsVertexAttribute();
			facePositions = FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions);
			partitioning = UniversalFaceSpatialPartitioning.Create(topology, surface, vertexPositions);
		}

		public void CreateHexGrid(int width, int height)
		{
			surface = RectangularHexGrid.Create(new PlanarDescriptor(Vector3.right, Vector3.up), new HexGridDescriptor(HexGridAxisStyles.Straight, HexGridAxisStyles.Staggered, HexGridAxisRelations.Obtuse), new Index2D(width, height));
			Vector3[] vertexPositionsArray;
			topology = ((RectangularHexGrid)surface).CreateManifold(out vertexPositionsArray);
			vertexPositions = vertexPositionsArray.AsVertexAttribute();
			facePositions = FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions);
			partitioning = UniversalFaceSpatialPartitioning.Create(topology, surface, vertexPositions);
		}

		public void CreateDistortedQuadGrid(int width, int height, int seed)
		{
			var quadGrid = RectangularQuadGrid.Create(new PlanarDescriptor(Vector3.right, Vector3.up), new Index2D(width, height));
			surface = quadGrid;
			Vector3[] vertexPositionsArray;
			topology = quadGrid.CreateManifold(out vertexPositionsArray);
			vertexPositions = vertexPositionsArray.AsVertexAttribute();
			var random = new RandomUtility(XorShift128Plus.Create(seed));
			var maxOffsetRadius = Mathf.Sqrt(2f) / 5f;
			for (int y = 1; y < height; ++y)
			{
				for (int x = 1; x < width; ++x)
				{
					vertexPositions[quadGrid.GetVertexIndex(x, y)] += (Vector3)random.CircleVector2(maxOffsetRadius);
				}
			}
			facePositions = FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions);
			partitioning = UniversalFaceSpatialPartitioning.Create(topology, surface, vertexPositions);
		}

		public void CreateDistortedHexGrid(int width, int height, int seed)
		{
			var hexGrid = RectangularHexGrid.Create(new PlanarDescriptor(Vector3.right, Vector3.up), new HexGridDescriptor(HexGridAxisStyles.Straight, HexGridAxisStyles.Staggered, HexGridAxisRelations.Obtuse), new Index2D(width, height));
			surface = hexGrid;
			Vector3[] vertexPositionsArray;
			topology = hexGrid.CreateManifold(out vertexPositionsArray);
			vertexPositions = vertexPositionsArray.AsVertexAttribute();
			var random = new RandomUtility(XorShift128Plus.Create(seed));
			var maxOffsetRadius = Mathf.Sqrt(3f) / 8f;
			foreach (var vertex in topology.vertices)
			{
				if (!vertex.hasExternalFaceNeighbor)
				{
					vertexPositions[vertex] += (Vector3)random.CircleVector2(maxOffsetRadius);
				}
			}
			facePositions = FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions);
			partitioning = UniversalFaceSpatialPartitioning.Create(topology, surface, vertexPositions);
		}

		public void CreateTetrahedron(float radius)
		{
			surface = SphericalSurface.Create(new SphericalDescriptor(Vector3.up, radius));
			Vector3[] vertexPositionsArray;
			SphericalManifoldUtility.CreateTetrahedron(radius, out topology, out vertexPositionsArray);
			vertexPositions = vertexPositionsArray.AsVertexAttribute();
			facePositions = FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions);
			partitioning = UniversalFaceSpatialPartitioning.Create(topology, surface, vertexPositions);
		}

		public void CreateCube(float radius)
		{
			surface = SphericalSurface.Create(new SphericalDescriptor(Vector3.up, radius));
			Vector3[] vertexPositionsArray;
			SphericalManifoldUtility.CreateCube(radius, out topology, out vertexPositionsArray);
			vertexPositions = vertexPositionsArray.AsVertexAttribute();
			facePositions = FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions);
			partitioning = UniversalFaceSpatialPartitioning.Create(topology, surface, vertexPositions);
		}

		public void CreateOctahedron(float radius)
		{
			surface = SphericalSurface.Create(new SphericalDescriptor(Vector3.up, radius));
			Vector3[] vertexPositionsArray;
			SphericalManifoldUtility.CreateOctahedron(radius, out topology, out vertexPositionsArray);
			vertexPositions = vertexPositionsArray.AsVertexAttribute();
			facePositions = FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions);
			partitioning = UniversalFaceSpatialPartitioning.Create(topology, surface, vertexPositions);
		}

		public void CreateDodecahedron(float radius)
		{
			surface = SphericalSurface.Create(new SphericalDescriptor(Vector3.up, radius));
			Vector3[] vertexPositionsArray;
			SphericalManifoldUtility.CreateDodecahedron(radius, out topology, out vertexPositionsArray);
			vertexPositions = vertexPositionsArray.AsVertexAttribute();
			facePositions = FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions);
			partitioning = UniversalFaceSpatialPartitioning.Create(topology, surface, vertexPositions);
		}

		public void CreateIcosahedron(float radius)
		{
			surface = SphericalSurface.Create(new SphericalDescriptor(Vector3.up, radius));
			Vector3[] vertexPositionsArray;
			SphericalManifoldUtility.CreateIcosahedron(radius, out topology, out vertexPositionsArray);
			vertexPositions = vertexPositionsArray.AsVertexAttribute();
			facePositions = FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions);
			partitioning = UniversalFaceSpatialPartitioning.Create(topology, surface, vertexPositions);
		}

		public void CreateSubdividedDodecahedron(float radius)
		{
			surface = SphericalSurface.Create(new SphericalDescriptor(Vector3.up, radius));
			Topology baseTopology;
			Vector3[] baseVertexPositionsArray;
			SphericalManifoldUtility.CreateIcosahedron(radius, out baseTopology, out baseVertexPositionsArray);
			Vector3[] vertexPositionsArray;
			SphericalManifoldUtility.Subdivide(baseTopology, baseVertexPositionsArray.AsVertexAttribute(), 5, radius, out topology, out vertexPositionsArray);
			SphericalManifoldUtility.MakeDual(topology, ref vertexPositionsArray, radius);
			vertexPositions = vertexPositionsArray.AsVertexAttribute();
			facePositions = FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions);
			partitioning = UniversalFaceSpatialPartitioning.Create(topology, surface, vertexPositions);
		}

		public void CreateSubdividedIcosahedron(float radius)
		{
			surface = SphericalSurface.Create(new SphericalDescriptor(Vector3.up, radius));
			Topology baseTopology;
			Vector3[] baseVertexPositionsArray;
			SphericalManifoldUtility.CreateIcosahedron(radius, out baseTopology, out baseVertexPositionsArray);
			Vector3[] vertexPositionsArray;
			SphericalManifoldUtility.Subdivide(baseTopology, baseVertexPositionsArray.AsVertexAttribute(), 5, radius, out topology, out vertexPositionsArray);
			vertexPositions = vertexPositionsArray.AsVertexAttribute();
			facePositions = FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions);
			partitioning = UniversalFaceSpatialPartitioning.Create(topology, surface, vertexPositions);
		}

		#endregion

		#region FaceCenterIntersections

		[Test]
		public void SmallQuadGridFaceCenterIntersections()
		{
			CreateQuadGrid(2, 2);
			FaceCenterIntersections();
		}

		[Test]
		public void LargeQuadGridFaceCenterIntersections()
		{
			CreateQuadGrid(8, 8);
			FaceCenterIntersections();
		}

		[Test]
		public void SmallHexGridFaceCenterIntersections()
		{
			CreateHexGrid(2, 2);
			FaceCenterIntersections();
		}

		[Test]
		public void LargeHexGridFaceCenterIntersections()
		{
			CreateHexGrid(8, 8);
			FaceCenterIntersections();
		}

		[Test]
		public void SmallDistortedQuadGridFaceCenterIntersections()
		{
			CreateDistortedQuadGrid(2, 2, 1);
			FaceCenterIntersections();
		}

		[Test]
		public void LargeDistortedQuadGridFaceCenterIntersections()
		{
			CreateDistortedQuadGrid(8, 8, 1);
			FaceCenterIntersections();
		}

		[Test]
		public void SmallDistortedHexGridFaceCenterIntersections()
		{
			CreateDistortedHexGrid(2, 2, 1);
			FaceCenterIntersections();
		}

		[Test]
		public void LargeDistortedHexGridFaceCenterIntersections()
		{
			CreateDistortedHexGrid(8, 8, 1);
			FaceCenterIntersections();
		}

		[Test]
		public void TetrahedronFaceCenterIntersections()
		{
			CreateTetrahedron(1f);
			FaceCenterIntersections();
		}

		[Test]
		public void CubeFaceCenterIntersections()
		{
			CreateCube(1f);
			FaceCenterIntersections();
		}

		[Test]
		public void OctahedronFaceCenterIntersections()
		{
			CreateOctahedron(1f);
			FaceCenterIntersections();
		}

		[Test]
		public void DodecahedronFaceCenterIntersections()
		{
			CreateDodecahedron(1f);
			FaceCenterIntersections();
		}

		[Test]
		public void IcosahedronFaceCenterIntersections()
		{
			CreateIcosahedron(1f);
			FaceCenterIntersections();
		}

		[Test]
		public void SubdividedDodecahedronFaceCenterIntersections()
		{
			CreateSubdividedDodecahedron(1f);
			FaceCenterIntersections();
		}

		[Test]
		public void SubdividedIcosahedronFaceCenterIntersections()
		{
			CreateSubdividedIcosahedron(1f);
			FaceCenterIntersections();
		}

		#endregion

		#region FaceVertexWeightedIntersections

		[Test]
		public void SmallQuadGridFaceVertexWeightedIntersections()
		{
			CreateQuadGrid(2, 2);
			FaceVertexWeightedIntersections();
		}

		[Test]
		public void LargeQuadGridFaceVertexWeightedIntersections()
		{
			CreateQuadGrid(8, 8);
			FaceVertexWeightedIntersections();
		}

		[Test]
		public void SmallHexGridFaceVertexWeightedIntersections()
		{
			CreateHexGrid(2, 2);
			FaceVertexWeightedIntersections();
		}

		[Test]
		public void LargeHexGridFaceVertexWeightedIntersections()
		{
			CreateHexGrid(8, 8);
			FaceVertexWeightedIntersections();
		}

		[Test]
		public void SmallDistortedQuadGridFaceVertexWeightedIntersections()
		{
			CreateDistortedQuadGrid(2, 2, 1);
			FaceVertexWeightedIntersections();
		}

		[Test]
		public void LargeDistortedQuadGridFaceVertexWeightedIntersections()
		{
			CreateDistortedQuadGrid(8, 8, 1);
			FaceVertexWeightedIntersections();
		}

		[Test]
		public void SmallDistortedHexGridFaceVertexWeightedIntersections()
		{
			CreateDistortedHexGrid(2, 2, 1);
			FaceVertexWeightedIntersections();
		}

		[Test]
		public void LargeDistortedHexGridFaceVertexWeightedIntersections()
		{
			CreateDistortedHexGrid(8, 8, 1);
			FaceVertexWeightedIntersections();
		}

		[Test]
		public void TetrahedronFaceVertexWeightedIntersections()
		{
			CreateTetrahedron(1f);
			FaceVertexWeightedIntersections();
		}

		[Test]
		public void CubeFaceVertexWeightedIntersections()
		{
			CreateCube(1f);
			FaceVertexWeightedIntersections();
		}

		[Test]
		public void OctahedronFaceVertexWeightedIntersections()
		{
			CreateOctahedron(1f);
			FaceVertexWeightedIntersections();
		}

		[Test]
		public void DodecahedronFaceVertexWeightedIntersections()
		{
			CreateDodecahedron(1f);
			FaceVertexWeightedIntersections();
		}

		[Test]
		public void IcosahedronFaceVertexWeightedIntersections()
		{
			CreateIcosahedron(1f);
			FaceVertexWeightedIntersections();
		}

		[Test]
		public void SubdividedDodecahedronFaceVertexWeightedIntersections()
		{
			CreateSubdividedDodecahedron(1f);
			FaceVertexWeightedIntersections();
		}

		[Test]
		public void SubdividedIcosahedronFaceVertexWeightedIntersections()
		{
			CreateSubdividedIcosahedron(1f);
			FaceVertexWeightedIntersections();
		}

		#endregion

		#region FaceEdgeMidpointWeightedIntersections

		[Test]
		public void SmallQuadGridFaceEdgeMidpointWeightedIntersections()
		{
			CreateQuadGrid(2, 2);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[Test]
		public void LargeQuadGridFaceEdgeMidpointWeightedIntersections()
		{
			CreateQuadGrid(8, 8);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[Test]
		public void SmallHexGridFaceEdgeMidpointWeightedIntersections()
		{
			CreateHexGrid(2, 2);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[Test]
		public void LargeHexGridFaceEdgeMidpointWeightedIntersections()
		{
			CreateHexGrid(8, 8);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[Test]
		public void SmallDistortedQuadGridFaceEdgeMidpointWeightedIntersections()
		{
			CreateDistortedQuadGrid(2, 2, 1);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[Test]
		public void LargeDistortedQuadGridFaceEdgeMidpointWeightedIntersections()
		{
			CreateDistortedQuadGrid(8, 8, 1);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[Test]
		public void SmallDistortedHexGridFaceEdgeMidpointWeightedIntersections()
		{
			CreateDistortedHexGrid(2, 2, 1);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[Test]
		public void LargeDistortedHexGridFaceEdgeMidpointWeightedIntersections()
		{
			CreateDistortedHexGrid(8, 8, 1);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[Test]
		public void TetrahedronFaceEdgeMidpointWeightedIntersections()
		{
			CreateTetrahedron(1f);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[Test]
		public void CubeFaceEdgeMidpointWeightedIntersections()
		{
			CreateCube(1f);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[Test]
		public void OctahedronFaceEdgeMidpointWeightedIntersections()
		{
			CreateOctahedron(1f);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[Test]
		public void DodecahedronFaceEdgeMidpointWeightedIntersections()
		{
			CreateDodecahedron(1f);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[Test]
		public void IcosahedronFaceEdgeMidpointWeightedIntersections()
		{
			CreateIcosahedron(1f);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[Test]
		public void SubdividedDodecahedronFaceEdgeMidpointWeightedIntersections()
		{
			CreateSubdividedDodecahedron(1f);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[Test]
		public void SubdividedIcosahedronFaceEdgeMidpointWeightedIntersections()
		{
			CreateSubdividedIcosahedron(1f);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		#endregion
	}
}
#endif
