﻿/******************************************************************************\
* Copyright Andy Gainey                                                        *
*                                                                              *
* Licensed under the Apache License, Version 2.0 (the "License");              *
* you may not use this file except in compliance with the License.             *
* You may obtain a copy of the License at                                      *
*                                                                              *
*     http://www.apache.org/licenses/LICENSE-2.0                               *
*                                                                              *
* Unless required by applicable law or agreed to in writing, software          *
* distributed under the License is distributed on an "AS IS" BASIS,            *
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.     *
* See the License for the specific language governing permissions and          *
* limitations under the License.                                               *
\******************************************************************************/

#if UNITY_5_3_OR_NEWER
using UnityEngine;
using NUnit.Framework;
using MakeIt.Random;
using MakeIt.Numerics;

namespace MakeIt.Tile.Tests
{
	class UniversalFaceSpatialPartitioningTests
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
			surface = RectangularQuadGrid.Create(Vector2.right, Vector2.up, Vector3.zero, Quaternion.identity, false, false, new IntVector2(width, height));
			Vector3[] vertexPositionsArray;
			topology = ((RectangularQuadGrid)surface).CreateManifold(out vertexPositionsArray);
			vertexPositions = vertexPositionsArray.AsVertexAttribute();
			facePositions = FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions);
			partitioning = UniversalFaceSpatialPartitioning.Create(surface, topology, vertexPositions);
		}

		public void CreateHexGrid(int width, int height)
		{
			surface = RectangularHexGrid.Create(HexGridDescriptor.standardCornerUp, Vector3.zero, Quaternion.identity, false, false, new IntVector2(width, height));
			Vector3[] vertexPositionsArray;
			topology = ((RectangularHexGrid)surface).CreateManifold(out vertexPositionsArray);
			vertexPositions = vertexPositionsArray.AsVertexAttribute();
			facePositions = FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions);
			partitioning = UniversalFaceSpatialPartitioning.Create(surface, topology, vertexPositions);
		}

		public void CreateDistortedQuadGrid(int width, int height, int seed)
		{
			var quadGrid = RectangularQuadGrid.Create(Vector2.right, Vector2.up, Vector3.zero, Quaternion.identity, false, false, new IntVector2(width, height));
			surface = quadGrid;
			Vector3[] vertexPositionsArray;
			topology = quadGrid.CreateManifold(out vertexPositionsArray);
			vertexPositions = vertexPositionsArray.AsVertexAttribute();
			var random = XorShift128Plus.Create(seed);
			var maxOffsetRadius = Mathf.Sqrt(2f) / 5f;
			for (int y = 1; y < height; ++y)
			{
				for (int x = 1; x < width; ++x)
				{
					vertexPositions[quadGrid.GetVertexIndex(x, y)] += (Vector3)random.PointWithinCircle(maxOffsetRadius);
				}
			}
			facePositions = FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions);
			partitioning = UniversalFaceSpatialPartitioning.Create(surface, topology, vertexPositions);
		}

		public void CreateDistortedHexGrid(int width, int height, int seed)
		{
			var hexGrid = RectangularHexGrid.Create(HexGridDescriptor.standardCornerUp, Vector3.zero, Quaternion.identity, false, false, new IntVector2(width, height));
			surface = hexGrid;
			Vector3[] vertexPositionsArray;
			topology = hexGrid.CreateManifold(out vertexPositionsArray);
			vertexPositions = vertexPositionsArray.AsVertexAttribute();
			var random = XorShift128Plus.Create(seed);
			var maxOffsetRadius = Mathf.Sqrt(3f) / 8f;
			foreach (var vertex in topology.vertices)
			{
				if (!vertex.hasExternalFaceNeighbor)
				{
					vertexPositions[vertex] += (Vector3)random.PointWithinCircle(maxOffsetRadius);
				}
			}
			facePositions = FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions);
			partitioning = UniversalFaceSpatialPartitioning.Create(surface, topology, vertexPositions);
		}

		public void CreateTetrahedron(float radius)
		{
			surface = SphericalSurface.Create(Vector3.up, Vector3.right, radius);
			Vector3[] vertexPositionsArray;
			SphericalManifoldUtility.CreateTetrahedron((SphericalSurface)surface, out topology, out vertexPositionsArray);
			vertexPositions = vertexPositionsArray.AsVertexAttribute();
			facePositions = FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions);
			partitioning = UniversalFaceSpatialPartitioning.Create(surface, topology, vertexPositions);
		}

		public void CreateCube(float radius)
		{
			surface = SphericalSurface.Create(Vector3.up, Vector3.right, radius);
			Vector3[] vertexPositionsArray;
			SphericalManifoldUtility.CreateCube((SphericalSurface)surface, out topology, out vertexPositionsArray);
			vertexPositions = vertexPositionsArray.AsVertexAttribute();
			facePositions = FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions);
			partitioning = UniversalFaceSpatialPartitioning.Create(surface, topology, vertexPositions);
		}

		public void CreateOctahedron(float radius)
		{
			surface = SphericalSurface.Create(Vector3.up, Vector3.right, radius);
			Vector3[] vertexPositionsArray;
			SphericalManifoldUtility.CreateOctahedron((SphericalSurface)surface, out topology, out vertexPositionsArray);
			vertexPositions = vertexPositionsArray.AsVertexAttribute();
			facePositions = FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions);
			partitioning = UniversalFaceSpatialPartitioning.Create(surface, topology, vertexPositions);
		}

		public void CreateDodecahedron(float radius)
		{
			surface = SphericalSurface.Create(Vector3.up, Vector3.right, radius);
			Vector3[] vertexPositionsArray;
			SphericalManifoldUtility.CreateDodecahedron((SphericalSurface)surface, out topology, out vertexPositionsArray);
			vertexPositions = vertexPositionsArray.AsVertexAttribute();
			facePositions = FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions);
			partitioning = UniversalFaceSpatialPartitioning.Create(surface, topology, vertexPositions);
		}

		public void CreateIcosahedron(float radius)
		{
			surface = SphericalSurface.Create(Vector3.up, Vector3.right, radius);
			Vector3[] vertexPositionsArray;
			SphericalManifoldUtility.CreateIcosahedron((SphericalSurface)surface, out topology, out vertexPositionsArray);
			vertexPositions = vertexPositionsArray.AsVertexAttribute();
			facePositions = FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions);
			partitioning = UniversalFaceSpatialPartitioning.Create(surface, topology, vertexPositions);
		}

		public void CreateSubdividedDodecahedron(float radius)
		{
			surface = SphericalSurface.Create(Vector3.up, Vector3.right, radius);
			Topology baseTopology;
			Vector3[] baseVertexPositionsArray;
			SphericalManifoldUtility.CreateIcosahedron((SphericalSurface)surface, out baseTopology, out baseVertexPositionsArray);
			Vector3[] vertexPositionsArray;
			SphericalManifoldUtility.Subdivide((SphericalSurface)surface, baseTopology, baseVertexPositionsArray.AsVertexAttribute(), 5, out topology, out vertexPositionsArray);
			SphericalManifoldUtility.MakeDual((SphericalSurface)surface, topology, ref vertexPositionsArray);
			vertexPositions = vertexPositionsArray.AsVertexAttribute();
			facePositions = FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions);
			partitioning = UniversalFaceSpatialPartitioning.Create(surface, topology, vertexPositions);
		}

		public void CreateSubdividedIcosahedron(float radius)
		{
			surface = SphericalSurface.Create(Vector3.up, Vector3.right, radius);
			Topology baseTopology;
			Vector3[] baseVertexPositionsArray;
			SphericalManifoldUtility.CreateIcosahedron((SphericalSurface)surface, out baseTopology, out baseVertexPositionsArray);
			Vector3[] vertexPositionsArray;
			SphericalManifoldUtility.Subdivide((SphericalSurface)surface, baseTopology, baseVertexPositionsArray.AsVertexAttribute(), 5, out topology, out vertexPositionsArray);
			vertexPositions = vertexPositionsArray.AsVertexAttribute();
			facePositions = FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions);
			partitioning = UniversalFaceSpatialPartitioning.Create(surface, topology, vertexPositions);
		}

		#endregion

		#region FaceCenterIntersections

		[TestCase(Category = "Normal")]
		public void SmallQuadGridFaceCenterIntersections()
		{
			CreateQuadGrid(2, 2);
			FaceCenterIntersections();
		}

		[TestCase(Category = "Normal")]
		public void LargeQuadGridFaceCenterIntersections()
		{
			CreateQuadGrid(8, 8);
			FaceCenterIntersections();
		}

		[TestCase(Category = "Normal")]
		public void SmallHexGridFaceCenterIntersections()
		{
			CreateHexGrid(2, 2);
			FaceCenterIntersections();
		}

		[TestCase(Category = "Normal")]
		public void LargeHexGridFaceCenterIntersections()
		{
			CreateHexGrid(8, 8);
			FaceCenterIntersections();
		}

		[TestCase(Category = "Normal")]
		public void SmallDistortedQuadGridFaceCenterIntersections()
		{
			CreateDistortedQuadGrid(2, 2, 1);
			FaceCenterIntersections();
		}

		[TestCase(Category = "Normal")]
		public void LargeDistortedQuadGridFaceCenterIntersections()
		{
			CreateDistortedQuadGrid(8, 8, 1);
			FaceCenterIntersections();
		}

		[TestCase(Category = "Normal")]
		public void SmallDistortedHexGridFaceCenterIntersections()
		{
			CreateDistortedHexGrid(2, 2, 1);
			FaceCenterIntersections();
		}

		[TestCase(Category = "Normal")]
		public void LargeDistortedHexGridFaceCenterIntersections()
		{
			CreateDistortedHexGrid(8, 8, 1);
			FaceCenterIntersections();
		}

		[TestCase(Category = "Normal")]
		public void TetrahedronFaceCenterIntersections()
		{
			CreateTetrahedron(1f);
			FaceCenterIntersections();
		}

		[TestCase(Category = "Normal")]
		public void CubeFaceCenterIntersections()
		{
			CreateCube(1f);
			FaceCenterIntersections();
		}

		[TestCase(Category = "Normal")]
		public void OctahedronFaceCenterIntersections()
		{
			CreateOctahedron(1f);
			FaceCenterIntersections();
		}

		[TestCase(Category = "Normal")]
		public void DodecahedronFaceCenterIntersections()
		{
			CreateDodecahedron(1f);
			FaceCenterIntersections();
		}

		[TestCase(Category = "Normal")]
		public void IcosahedronFaceCenterIntersections()
		{
			CreateIcosahedron(1f);
			FaceCenterIntersections();
		}

		[TestCase(Category = "Normal")]
		public void SubdividedDodecahedronFaceCenterIntersections()
		{
			CreateSubdividedDodecahedron(1f);
			FaceCenterIntersections();
		}

		[TestCase(Category = "Normal")]
		public void SubdividedIcosahedronFaceCenterIntersections()
		{
			CreateSubdividedIcosahedron(1f);
			FaceCenterIntersections();
		}

		#endregion

		#region FaceVertexWeightedIntersections

		[TestCase(Category = "Normal")]
		public void SmallQuadGridFaceVertexWeightedIntersections()
		{
			CreateQuadGrid(2, 2);
			FaceVertexWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void LargeQuadGridFaceVertexWeightedIntersections()
		{
			CreateQuadGrid(8, 8);
			FaceVertexWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void SmallHexGridFaceVertexWeightedIntersections()
		{
			CreateHexGrid(2, 2);
			FaceVertexWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void LargeHexGridFaceVertexWeightedIntersections()
		{
			CreateHexGrid(8, 8);
			FaceVertexWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void SmallDistortedQuadGridFaceVertexWeightedIntersections()
		{
			CreateDistortedQuadGrid(2, 2, 1);
			FaceVertexWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void LargeDistortedQuadGridFaceVertexWeightedIntersections()
		{
			CreateDistortedQuadGrid(8, 8, 1);
			FaceVertexWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void SmallDistortedHexGridFaceVertexWeightedIntersections()
		{
			CreateDistortedHexGrid(2, 2, 1);
			FaceVertexWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void LargeDistortedHexGridFaceVertexWeightedIntersections()
		{
			CreateDistortedHexGrid(8, 8, 1);
			FaceVertexWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void TetrahedronFaceVertexWeightedIntersections()
		{
			CreateTetrahedron(1f);
			FaceVertexWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void CubeFaceVertexWeightedIntersections()
		{
			CreateCube(1f);
			FaceVertexWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void OctahedronFaceVertexWeightedIntersections()
		{
			CreateOctahedron(1f);
			FaceVertexWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void DodecahedronFaceVertexWeightedIntersections()
		{
			CreateDodecahedron(1f);
			FaceVertexWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void IcosahedronFaceVertexWeightedIntersections()
		{
			CreateIcosahedron(1f);
			FaceVertexWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void SubdividedDodecahedronFaceVertexWeightedIntersections()
		{
			CreateSubdividedDodecahedron(1f);
			FaceVertexWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void SubdividedIcosahedronFaceVertexWeightedIntersections()
		{
			CreateSubdividedIcosahedron(1f);
			FaceVertexWeightedIntersections();
		}

		#endregion

		#region FaceEdgeMidpointWeightedIntersections

		[TestCase(Category = "Normal")]
		public void SmallQuadGridFaceEdgeMidpointWeightedIntersections()
		{
			CreateQuadGrid(2, 2);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void LargeQuadGridFaceEdgeMidpointWeightedIntersections()
		{
			CreateQuadGrid(8, 8);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void SmallHexGridFaceEdgeMidpointWeightedIntersections()
		{
			CreateHexGrid(2, 2);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void LargeHexGridFaceEdgeMidpointWeightedIntersections()
		{
			CreateHexGrid(8, 8);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void SmallDistortedQuadGridFaceEdgeMidpointWeightedIntersections()
		{
			CreateDistortedQuadGrid(2, 2, 1);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void LargeDistortedQuadGridFaceEdgeMidpointWeightedIntersections()
		{
			CreateDistortedQuadGrid(8, 8, 1);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void SmallDistortedHexGridFaceEdgeMidpointWeightedIntersections()
		{
			CreateDistortedHexGrid(2, 2, 1);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void LargeDistortedHexGridFaceEdgeMidpointWeightedIntersections()
		{
			CreateDistortedHexGrid(8, 8, 1);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void TetrahedronFaceEdgeMidpointWeightedIntersections()
		{
			CreateTetrahedron(1f);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void CubeFaceEdgeMidpointWeightedIntersections()
		{
			CreateCube(1f);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void OctahedronFaceEdgeMidpointWeightedIntersections()
		{
			CreateOctahedron(1f);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void DodecahedronFaceEdgeMidpointWeightedIntersections()
		{
			CreateDodecahedron(1f);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void IcosahedronFaceEdgeMidpointWeightedIntersections()
		{
			CreateIcosahedron(1f);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
		public void SubdividedDodecahedronFaceEdgeMidpointWeightedIntersections()
		{
			CreateSubdividedDodecahedron(1f);
			edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(topology.vertexEdges, vertexPositions);
			FaceEdgeMidpointWeightedIntersections();
		}

		[TestCase(Category = "Normal")]
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
