using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public enum TopologyClassification
	{
		FinitePlane,
		Cylinder,
		Toroid,
		Sphere,
	}

	public enum TopologyProjection
	{
		Planar,
		Cylindrical,
		Toroidal,
		Spherical,
	}

	public enum FlatTopologyTiling
	{
		Triangular,
		Quadrilateral,
		Hexagonal,
	}

	public enum SphericalTopologyTiling
	{
		Tetrahedron,
		Cube,
		Octahedron,
		Dodecahedron,
		Icosahedron,
	}

	public enum TopologyTileOrientation
	{
		FlatTop,
		PointyTop,
	}

	public enum TopologyDistanceCalculationMethod
	{
		None,
		BreadthFirstEuclideanCumulative,
		BreadthFirstEuclideanFromRoot,
		BreadthFirstSphericalCumulative,
		BreadthFirstSphericalFromRoot,
		DepthFirstEuclideanCumulative,
		DepthFirstEuclideanFromRoot,
		DepthFirstSphericalCumulative,
		DepthFirstSphericalFromRoot,
	}

	[ExecuteInEditMode]
	public class TopologyGenerator : CompoundMesh
	{
		public TopologyClassification Classification = TopologyClassification.Sphere;
		public TopologyProjection Projection = TopologyProjection.Spherical;
		public FlatTopologyTiling FlatTiling = FlatTopologyTiling.Hexagonal;
		public SphericalTopologyTiling SphericalTiling = SphericalTopologyTiling.Icosahedron;
		public TopologyTileOrientation TileOrientation = TopologyTileOrientation.PointyTop;

		// Flat Topologies
		public int ColumnCount = 24;
		public int RowCount = 24;
		public float MaximumLatitude = 60f;
		public float ProjectionRegularity = 0.5f;

		// Spherical Topologies
		public int SubdivisionDegree = 24;
		public bool UseDualTopology = true;

		public bool RandomizeTopology = false;
		public int TopologyRandomizationPassCount = 1;
		public float TopologyRandomizationFrequency = 0.1f;
		public int TopologyRandomizationMinimumVertexNeighbors = 3;
		public int TopologyRandomizationMaximumVertexNeighbors = 8;
		public int TopologyRandomizationMinimumFaceNeighbors = 3;
		public int TopologyRandomizationMaximumFaceNeighbors = 8;
		public float TopologyRandomizationRelaxationRegularity = 0.5f;
		public float TopologyRandomizationRelaxationRelativePrecision = 0.05f;
		public int TopologyRandomizationMaximumRelaxationPassCount = 20;
		public int TopologyRandomizationMaximumRepairPassCount = 20;
		public bool TopologyRandomizationLockBoundaryPositions = true;
		public RandomEngineFactory TopologyRandomizationEngine = null;
		public string TopologyRandomizationEngineSeed = "";

		public bool CalculateCentroids = true;
		public bool FlattenCentroids = true;
		public bool CalculateSpatialPartitioning = true;

		public bool GenerateRegions = false;
		public int DesiredRegionCount = 24;
		public RandomEngineFactory RegionRandomEngine = null;
		public string RegionRandomEngineSeed = "";

		public TopologyDistanceCalculationMethod DistanceCalculationMethod;
		public int DistanceCalculationRootFaceIndex;

		public bool BuildMesh = true;

		private bool _invalidated = false;

		[SerializeField] private Manifold _manifold;

		[SerializeField] private Vector3[] _centroids;

		[SerializeField] private SphericalPartitioning _sphericalPartitioning;

		[SerializeField] private int[] _faceRegions;
		[SerializeField] private int _regionCount;
		[SerializeField] private Color[] _regionColors;
		[SerializeField] private float[] _regionBorderTravelCosts;
		[SerializeField] private float[] _regionInternalTravelCosts;

		[SerializeField] private float[] _faceDistances;
		[SerializeField] private float[] _faceOrders;

		private int TriangularSubdividedVertexCount(int vertexCount, int edgeCount, int faceCount)
		{
			return vertexCount + edgeCount * SubdivisionDegree + faceCount * (SubdivisionDegree - 1) * SubdivisionDegree / 2;
		}

		private int TriangularSubdividedFaceCount(int faceCount)
		{
			return faceCount * (SubdivisionDegree + 1) * (SubdivisionDegree + 1);
		}

		private int QuadrilateralSubdividedVertexCount(int vertexCount, int edgeCount, int faceCount)
		{
			return vertexCount + edgeCount * SubdivisionDegree + faceCount * SubdivisionDegree * SubdivisionDegree;
		}

		private int QuadrilateralSubdividedFaceCount(int faceCount)
		{
			return faceCount * (SubdivisionDegree + 1) * (SubdivisionDegree + 1);
		}

		public int ExpectedFaceCount
		{
			get
			{
				if (Classification == TopologyClassification.Sphere && Projection == TopologyProjection.Spherical)
				{
					switch (SphericalTiling)
					{
						case SphericalTopologyTiling.Tetrahedron:
							if (!UseDualTopology)
								return TriangularSubdividedFaceCount(4);
							else
								return TriangularSubdividedVertexCount(4, 6, 4);
						case SphericalTopologyTiling.Cube:
							if (!UseDualTopology)
								return QuadrilateralSubdividedFaceCount(6);
							else
								return QuadrilateralSubdividedVertexCount(8, 12, 6);
						case SphericalTopologyTiling.Octahedron:
							if (!UseDualTopology)
								return TriangularSubdividedFaceCount(8);
							else
								return TriangularSubdividedVertexCount(6, 12, 8);
						case SphericalTopologyTiling.Dodecahedron:
							if (!UseDualTopology)
								return TriangularSubdividedVertexCount(12, 30, 20);
							else
								return TriangularSubdividedFaceCount(20);
						case SphericalTopologyTiling.Icosahedron:
							if (!UseDualTopology)
								return TriangularSubdividedFaceCount(20);
							else
								return TriangularSubdividedVertexCount(12, 30, 20);
					}
				}
				else if (Classification == TopologyClassification.Cylinder && Projection == TopologyProjection.Spherical)
				{
					switch (FlatTiling)
					{
						case FlatTopologyTiling.Triangular:
							return ColumnCount * RowCount;
						case FlatTopologyTiling.Quadrilateral:
							return ColumnCount * RowCount;
						case FlatTopologyTiling.Hexagonal:
							return ColumnCount * RowCount;
					}
				}

				return 0;
			}
		}

		public Manifold manifold { get { return _manifold; } }
		public Vector3[] centroids { get { return _centroids; } }
		public SphericalPartitioning sphericalPartitioning { get { return _sphericalPartitioning; } }
		public int[] faceRegions { get { return _faceRegions; } }
		public int regionCount { get { return _regionCount; } }
		public Color[] regionColors { get { return _regionColors; } }
		public float[] regionBorderTravelCosts { get { return _regionBorderTravelCosts; } }
		public float[] regionInternalTravelCosts { get { return _regionInternalTravelCosts; } }

		public void Invalidate()
		{
			_invalidated = true;
		}

		private void Update()
		{
			if (_invalidated)
			{
				Regenerate();
			}
		}

		public void Regenerate()
		{
			if (Classification == TopologyClassification.Sphere && Projection == TopologyProjection.Spherical)
			{
				switch (SphericalTiling)
				{
					case SphericalTopologyTiling.Tetrahedron:
						_manifold = SphericalManifold.CreateTetrahedron();
						break;
					case SphericalTopologyTiling.Cube:
						_manifold = SphericalManifold.CreateCube();
						break;
					case SphericalTopologyTiling.Octahedron:
						_manifold = SphericalManifold.CreateOctahedron();
						break;
					case SphericalTopologyTiling.Dodecahedron:
						if (SubdivisionDegree == 0)
						{
							_manifold = SphericalManifold.CreateDodecahedron();
						}
						else
						{
							_manifold = SphericalManifold.CreateIcosahedron();
						}
						break;
					case SphericalTopologyTiling.Icosahedron:
						_manifold = SphericalManifold.CreateIcosahedron();
						break;
				}

				if (SubdivisionDegree > 0)
				{
					_manifold = SphericalManifold.Subdivide(_manifold, SubdivisionDegree);
				}

				var takeDual = UseDualTopology;
				if (SphericalTiling == SphericalTopologyTiling.Dodecahedron && SubdivisionDegree > 0)
				{
					takeDual = !takeDual;
				}

				if (takeDual)
				{
					var topology = _manifold.topology.GetDualTopology();
					var vertexPositions = new Vector3[topology.vertices.Count];

					foreach (var face in _manifold.topology.internalFaces)
					{
						var average = new Vector3();
						foreach (var edge in face.edges)
						{
							average += _manifold.vertexPositions.Of(edge.nextVertex);
						}
						vertexPositions[face] = average.normalized;
					}

					_manifold = new Manifold(topology, vertexPositions);
				}
			}
			else if (Classification == TopologyClassification.Cylinder && Projection == TopologyProjection.Spherical)
			{
				switch (FlatTiling)
				{
					case FlatTopologyTiling.Triangular:
						switch (TileOrientation)
						{
							case TopologyTileOrientation.FlatTop:
								_manifold = CylindricalManifold.CreateFlatTopTriGridSphericalCylinder(ColumnCount, RowCount, MaximumLatitude * Mathf.Deg2Rad, ProjectionRegularity);
								break;
							case TopologyTileOrientation.PointyTop:
								_manifold = CylindricalManifold.CreatePointyTopTriGridSphericalCylinder(ColumnCount, RowCount, MaximumLatitude * Mathf.Deg2Rad, ProjectionRegularity);
								break;
						}
						break;
					case FlatTopologyTiling.Quadrilateral:
						_manifold = CylindricalManifold.CreateQuadGridSphericalCylinder(ColumnCount, RowCount, MaximumLatitude * Mathf.Deg2Rad, ProjectionRegularity);
						break;
					case FlatTopologyTiling.Hexagonal:
						switch (TileOrientation)
						{
							case TopologyTileOrientation.FlatTop:
								_manifold = CylindricalManifold.CreateFlatTopHexGridSphericalCylinder(ColumnCount, RowCount, MaximumLatitude * Mathf.Deg2Rad, ProjectionRegularity);
								break;
							case TopologyTileOrientation.PointyTop:
								_manifold = CylindricalManifold.CreatePointyTopHexGridSphericalCylinder(ColumnCount, RowCount, MaximumLatitude * Mathf.Deg2Rad, ProjectionRegularity);
								break;
						}
						break;
				}
			}

			if (RandomizeTopology && TopologyRandomizationPassCount > 0 && TopologyRandomizationFrequency > 0f)
			{
				var random = new Experilous.Random(CreateRandomEngine(TopologyRandomizationEngine, TopologyRandomizationEngineSeed));

				var perPassRandomizationFrequency = TopologyRandomizationFrequency / TopologyRandomizationPassCount;

				var vertexPositions = _manifold.vertexPositions;
				var regularityRelaxedPositions = new Vector3[vertexPositions.Length];
				var equalAreaRelaxedPositions = new Vector3[vertexPositions.Length];
				var centroidsBuffer = new Vector3[_manifold.topology.faces.Count];

				var regularityWeight = TopologyRandomizationRelaxationRegularity;
				var equalAreaWeight = (1f - TopologyRandomizationRelaxationRegularity);

				for (int topologyRandomizationPass = 0; topologyRandomizationPass < TopologyRandomizationPassCount; ++topologyRandomizationPass)
				{
					foreach (var vertexEdge in _manifold.topology.vertexEdges)
					{
						var twinVertexEdge = vertexEdge.twin;
						var faceEdge = vertexEdge.faceEdge;

						var verticesCanChange =
							_manifold.topology.CanSpinEdgeForward(vertexEdge) &&
							(!TopologyRandomizationLockBoundaryPositions || !vertexEdge.isBoundary && !faceEdge.isBoundary && !vertexEdge.nearVertex.hasExternalFaceNeighbor && !vertexEdge.farVertex.hasExternalFaceNeighbor) &&
							vertexEdge.farVertex.neighborCount > TopologyRandomizationMinimumVertexNeighbors &&
							twinVertexEdge.farVertex.neighborCount > TopologyRandomizationMinimumVertexNeighbors &&
							vertexEdge.faceEdge.next.nextVertex.neighborCount < TopologyRandomizationMaximumVertexNeighbors &&
							twinVertexEdge.faceEdge.next.nextVertex.neighborCount < TopologyRandomizationMaximumVertexNeighbors;

						var facesCanChange =
							_manifold.topology.CanSpinEdgeForward(faceEdge) &&
							(!TopologyRandomizationLockBoundaryPositions || !vertexEdge.isBoundary && !faceEdge.isBoundary && !vertexEdge.nearVertex.hasExternalFaceNeighbor && !vertexEdge.farVertex.hasExternalFaceNeighbor) &&
							(vertexEdge.prevFace.isExternal || vertexEdge.prevFace.neighborCount > TopologyRandomizationMinimumFaceNeighbors) &&
							(twinVertexEdge.prevFace.isExternal || twinVertexEdge.prevFace.neighborCount > TopologyRandomizationMinimumFaceNeighbors) &&
							(vertexEdge.next.nextFace.isExternal || vertexEdge.next.nextFace.neighborCount < TopologyRandomizationMaximumFaceNeighbors) &&
							(twinVertexEdge.next.nextFace.isExternal || twinVertexEdge.next.nextFace.neighborCount < TopologyRandomizationMaximumFaceNeighbors);

						if (verticesCanChange && facesCanChange)
						{
							var randomValue = random.HalfOpenFloatUnit();
							if (randomValue < perPassRandomizationFrequency)
							{
								if (randomValue < perPassRandomizationFrequency * 0.5f)
								{
									_manifold.topology.SpinEdgeForward(vertexEdge);
								}
								else
								{
									_manifold.topology.SpinEdgeForward(faceEdge);
								}
							}
						}
						else if (verticesCanChange || facesCanChange)
						{
							if (random.HalfOpenFloatUnit() < perPassRandomizationFrequency)
							{
								if (verticesCanChange)
								{
									_manifold.topology.SpinEdgeForward(vertexEdge);
								}
								else
								{
									_manifold.topology.SpinEdgeForward(faceEdge);
								}
							}
						}
					}

					float priorRelaxationAmount = 0f;
					for (int i = 0; i < TopologyRandomizationMaximumRelaxationPassCount; ++i)
					{
						SphericalManifold.RelaxForRegularity(_manifold, TopologyRandomizationLockBoundaryPositions, regularityRelaxedPositions);
						SphericalManifold.RelaxForEqualArea(_manifold, TopologyRandomizationLockBoundaryPositions, equalAreaRelaxedPositions, centroidsBuffer);

						float relaxationAmount = 0f;
						var weightedRelaxedPositions = regularityRelaxedPositions;
						for (int j = 0; j < vertexPositions.Length; ++j)
						{
							var weightedRelaxedPosition = regularityRelaxedPositions[j] * regularityWeight + equalAreaRelaxedPositions[j] * equalAreaWeight;
							relaxationAmount += (vertexPositions[j] - weightedRelaxedPosition).magnitude;
							weightedRelaxedPositions[j] = weightedRelaxedPosition;
						}

						if (relaxationAmount == 0f || (priorRelaxationAmount != 0f && 1f - relaxationAmount / priorRelaxationAmount < TopologyRandomizationRelaxationRelativePrecision))
						{
							break;
						}

						priorRelaxationAmount = relaxationAmount;

						regularityRelaxedPositions = vertexPositions;
						vertexPositions = weightedRelaxedPositions;

						_manifold.vertexPositions = vertexPositions;

						for (int j = 0; j < TopologyRandomizationMaximumRepairPassCount; ++j)
						{
							if (SphericalManifold.ValidateAndRepair(_manifold, 0.5f, TopologyRandomizationLockBoundaryPositions))
							{
								break;
							}
						}
					}
				}
			}

			if (CalculateCentroids)
			{
				if (Projection != TopologyProjection.Spherical || FlattenCentroids == true)
				{
					_centroids = Manifold.CalculateFaceCentroids(_manifold);
				}
				else
				{
					_centroids = SphericalManifold.CalculateFaceCentroids(_manifold);
				}
			}
			else
			{
				_centroids = null;
			}

			if (CalculateSpatialPartitioning)
			{
				if (Projection != TopologyProjection.Spherical)
				{
				}
				else
				{
					_sphericalPartitioning = new SphericalPartitioning(_manifold);
				}
			}
			else
			{
				_sphericalPartitioning = null;
			}

			if (GenerateRegions)
			{
				_faceRegions = new int[_manifold.topology.internalFaces.Count];

				var randomEngine = CreateRandomEngine(RegionRandomEngine, RegionRandomEngineSeed);

				if (DesiredRegionCount == 1)
				{
					_regionCount = 1;

					foreach (var face in _manifold.topology.internalFaces)
					{
						_faceRegions[face] = 0;
					}
				}
				else if (DesiredRegionCount >= _manifold.topology.internalFaces.Count)
				{
					_regionCount = _manifold.topology.internalFaces.Count;

					foreach (var face in _manifold.topology.internalFaces)
					{
						_faceRegions[face] = face.index;
					}
				}
				else
				{
					var visitor = new RandomAdjacentFaceVisitor(_manifold.topology, randomEngine);

					_regionCount = DesiredRegionCount;

					var faces = _manifold.topology.internalFaces;
					int faceIndex = 0;
					int regionIndex = 0;
					int remainingFaceCount = faces.Count;
					int remainingRegionCount = _regionCount;
					while (remainingRegionCount > 0)
					{
						if (Experilous.Random.HalfOpenRange(remainingFaceCount, randomEngine) < remainingRegionCount)
						{
							visitor.AddRoot(faces[faceIndex]);
							_faceRegions[faceIndex] = regionIndex;
							++regionIndex;
							--remainingRegionCount;
						}

						++faceIndex;
						--remainingFaceCount;
					}

					foreach (var edge in (IEnumerable<Topology.FaceEdge>)visitor)
					{
						_faceRegions[edge.farFace] = _faceRegions[edge.nearFace];
					}
				}

				_regionColors = new Color[_regionCount];
				_regionBorderTravelCosts = new float[_regionCount];
				_regionInternalTravelCosts = new float[_regionCount];
				for (int i = 0; i < _regionCount; ++i)
				{
					_regionColors[i] = new Color(
						Experilous.Random.ClosedFloatUnit(randomEngine),
						Experilous.Random.ClosedFloatUnit(randomEngine),
						Experilous.Random.ClosedFloatUnit(randomEngine));

					_regionInternalTravelCosts[i] = Experilous.Random.ClosedRange(1f, 4f, randomEngine);
					_regionBorderTravelCosts[i] = _regionInternalTravelCosts[i] + Experilous.Random.ClosedRange(8f, 32f, randomEngine);
				}
			}
			else
			{
				_faceRegions = null;
				_regionCount = 0;
				_regionColors = null;
				_regionBorderTravelCosts = null;
				_regionInternalTravelCosts = null;
			}

			if (CalculateCentroids)
			{
				var rootFace = _manifold.topology.faces[Mathf.Clamp(DistanceCalculationRootFaceIndex, 0, _manifold.topology.internalFaces.Count - 1)];

				IEnumerable<DistanceOrderedFaceVisitor.EdgeDistancePair> enumerable;
				switch (DistanceCalculationMethod)
				{
					case TopologyDistanceCalculationMethod.BreadthFirstEuclideanCumulative:
						enumerable = BreadthFirstFaceVisitor.CreateBasedOnCumulativeEuclideanDistance(_manifold.topology, rootFace, _centroids);
						break;
					case TopologyDistanceCalculationMethod.BreadthFirstEuclideanFromRoot:
						enumerable = BreadthFirstFaceVisitor.CreateBasedOnRootEuclideanDistance(_manifold.topology, rootFace, _centroids);
						break;
					case TopologyDistanceCalculationMethod.BreadthFirstSphericalCumulative:
						enumerable = BreadthFirstFaceVisitor.CreateBasedOnCumulativeSphericalDistance(_manifold.topology, rootFace, _centroids);
						break;
					case TopologyDistanceCalculationMethod.BreadthFirstSphericalFromRoot:
						enumerable = BreadthFirstFaceVisitor.CreateBasedOnRootSphericalDistance(_manifold.topology, rootFace, _centroids);
						break;
					case TopologyDistanceCalculationMethod.DepthFirstEuclideanCumulative:
						enumerable = DepthFirstFaceVisitor.CreateBasedOnCumulativeEuclideanDistance(_manifold.topology, rootFace, _centroids);
						break;
					case TopologyDistanceCalculationMethod.DepthFirstEuclideanFromRoot:
						enumerable = DepthFirstFaceVisitor.CreateBasedOnRootEuclideanDistance(_manifold.topology, rootFace, _centroids);
						break;
					case TopologyDistanceCalculationMethod.DepthFirstSphericalCumulative:
						enumerable = DepthFirstFaceVisitor.CreateBasedOnCumulativeSphericalDistance(_manifold.topology, rootFace, _centroids);
						break;
					case TopologyDistanceCalculationMethod.DepthFirstSphericalFromRoot:
						enumerable = DepthFirstFaceVisitor.CreateBasedOnRootSphericalDistance(_manifold.topology, rootFace, _centroids);
						break;
					default:
						enumerable = null;
						break;
				}

				if (enumerable != null)
				{
					_faceDistances = new float[_manifold.topology.internalFaces.Count];
					_faceOrders = new float[_manifold.topology.internalFaces.Count];

					_faceDistances[rootFace.index] = 0f;
					_faceOrders[rootFace.index] = 0f;

					float i = 1f;
					float divisor = _manifold.topology.internalFaces.Count - 1;

					foreach (var edgeDistancePair in enumerable)
					{
						var faceIndex = edgeDistancePair.face.index;
						_faceDistances[faceIndex] = edgeDistancePair.distance;
						_faceOrders[faceIndex] = i / divisor;
						++i;
					}
				}
				else
				{
					_faceDistances = null;
					_faceOrders = null;
				}
			}
			else
			{
				_faceDistances = null;
				_faceOrders = null;
			}

			ClearSubmeshes();

			if (BuildMesh && SubmeshPrefab != null)
			{
				if (CalculateCentroids)
				{
					BuildSubmeshes(_manifold.topology.internalFaces, _manifold.vertexPositions, _centroids);
				}
				else
				{
					BuildSubmeshes(_manifold.topology.internalFaces, _manifold.vertexPositions);
				}
			}

			ClearSubmeshCache();

			_invalidated = false;
		}

		private static IRandomEngine CreateRandomEngine(RandomEngineFactory factory, string seed)
		{
			if (factory != null)
			{
				if (seed == null || seed.Length == 0)
				{
					return factory.Create();
				}
				else
				{
					return factory.Create(seed);
				}
			}
			else
			{
				if (seed == null || seed.Length == 0)
				{
					return new Experilous.NativeRandomEngine();
				}
				else
				{
					return new Experilous.NativeRandomEngine(RandomEngineFactory.Hash(seed));
				}
			}
		}

		private void BuildSubmeshes(Topology.FacesIndexer faces, Vector3[] vertexPositions)
		{
		}

		private void BuildSubmeshes(Topology.FacesIndexer faces, Vector3[] vertexPositions, Vector3[] centroidPositions)
		{
			System.Func<int, Color> faceColorGenerator;

			if (_faceRegions != null && _faceRegions.Length > 0 && _regionCount > 0)
			{
				faceColorGenerator = (int i) => { return _regionColors[_faceRegions[i]]; };
			}
			else if (_faceOrders != null)
			{
				var maxDistance = 0f;
				foreach (var face in _manifold.topology.internalFaces)
				{
					maxDistance = Mathf.Max(maxDistance, _faceDistances[face]);
				}
				faceColorGenerator = (int i) => { return new Color(_faceDistances[i] / maxDistance, _faceOrders[i], 0f); };
			}
			else
			{
				faceColorGenerator = (int i) => { return new Color(1f, 1f, 1f); };
			}

			var faceIndex = 0;
			var faceCount = faces.Count;
			while (faceIndex < faceCount)
			{
				var endFaceIndex = faceIndex;
				var meshVertexCount = 0;
				var meshTriangleCount = 0;
				while (endFaceIndex < faceCount)
				{
					var face = faces[endFaceIndex];
					var neighborCount = face.neighborCount;
					var faceVertexCount = neighborCount + 1;
					if (meshVertexCount + faceVertexCount > 65534) break;
					++endFaceIndex;
					meshVertexCount += faceVertexCount;
					meshTriangleCount += neighborCount;
				}

				Vector3[] vertices = new Vector3[meshVertexCount];
				Color[] colors = new Color[meshVertexCount];
				int[] triangles = new int[meshTriangleCount * 3];

				int meshVertex = 0;
				int meshTriangle = 0;

				while (faceIndex < endFaceIndex)
				{
					var face = faces[faceIndex];
					var edge = face.firstEdge;
					var neighborCount = face.neighborCount;
					vertices[meshVertex] = centroidPositions[faceIndex];
					colors[meshVertex] = faceColorGenerator(face.index);
					for (int j = 0; j < neighborCount; ++j, edge = edge.next)
					{
						vertices[meshVertex + j + 1] = vertexPositions[edge.nextVertex];
						colors[meshVertex + j + 1] = new Color(0, 0, 0);
						triangles[meshTriangle + j * 3 + 0] = meshVertex;
						triangles[meshTriangle + j * 3 + 1] = meshVertex + 1 + j;
						triangles[meshTriangle + j * 3 + 2] = meshVertex + 1 + (j + 1) % neighborCount;
					}
					meshVertex += neighborCount + 1;
					meshTriangle += neighborCount * 3;
					++faceIndex;
				}

				PushSubmesh(vertices, colors, triangles);
			}
		}
	}
}
