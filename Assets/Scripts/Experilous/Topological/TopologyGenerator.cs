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

	[ExecuteInEditMode]
	public class TopologyGenerator : CompoundMesh
	{
		public TopologyClassification Classification = TopologyClassification.Sphere;
		public TopologyProjection Projection = TopologyProjection.Spherical;
		public FlatTopologyTiling FlatTiling = FlatTopologyTiling.Hexagonal;
		public SphericalTopologyTiling SphericalTiling = SphericalTopologyTiling.Icosahedron;
		public TopologyTileOrientation TileOrientation = TopologyTileOrientation.PointyTop;

		public int ColumnCount = 24;
		public int RowCount = 24;
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
		public RandomEngineFactory TopologyRandomizationEngine = null;
		public string TopologyRandomizationEngineSeed = "";

		public bool CalculateCentroids = true;
		public bool CalculateSpatialPartitioning = true;

		public bool GenerateRegions = false;
		public int DesiredRegionCount = 24;
		public RandomEngineFactory RegionRandomEngine = null;
		public string RegionRandomEngineSeed = "";

		public bool BuildMesh = true;

		private bool _invalidated = false;

		[SerializeField]
		private Manifold _manifold;

		[SerializeField]
		private Vector3[] _centroids;

		[SerializeField]
		private SphericalPartitioning _sphericalPartitioning;

		[SerializeField]
		private int[] _faceRegions;
		[SerializeField]
		private int _regionCount;
		[SerializeField]
		private List<Color> _regionColors;

		public int ExpectedFaceCount
		{
			get
			{
				return 144;
			}
		}

		public Manifold manifold { get { return _manifold; } }
		public Vector3[] centroids { get { return _centroids; } }
		public SphericalPartitioning sphericalPartitioning { get { return _sphericalPartitioning; } }
		public int[] faceRegions { get { return _faceRegions; } }
		public int regionCount { get { return _regionCount; } }
		public List<Color> regionColors { get { return _regionColors; } }

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

					foreach (var face in _manifold.topology.faces)
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
						foreach (var edge in _manifold.topology.vertexEdges)
						{
							var twinEdge = edge.twin;

							var verticesCanChange =
								edge.farVertex.neighborCount > TopologyRandomizationMinimumVertexNeighbors &&
								twinEdge.farVertex.neighborCount > TopologyRandomizationMinimumVertexNeighbors &&
								edge.faceEdge.next.nextVertex.neighborCount < TopologyRandomizationMaximumVertexNeighbors &&
								twinEdge.faceEdge.next.nextVertex.neighborCount < TopologyRandomizationMaximumVertexNeighbors;

							var facesCanChange =
								edge.prevFace.neighborCount > TopologyRandomizationMinimumFaceNeighbors &&
								twinEdge.prevFace.neighborCount > TopologyRandomizationMinimumFaceNeighbors &&
								edge.next.nextFace.neighborCount < TopologyRandomizationMaximumFaceNeighbors &&
								twinEdge.next.nextFace.neighborCount < TopologyRandomizationMaximumFaceNeighbors;

							if (verticesCanChange && facesCanChange)
							{
								var randomValue = random.HalfOpenFloatUnit();
								if (randomValue < perPassRandomizationFrequency)
								{
									if (randomValue < perPassRandomizationFrequency * 0.5f)
									{
										_manifold.topology.SpinEdgeForward(edge);
									}
									else
									{
										_manifold.topology.SpinEdgeForward(edge.faceEdge);
									}
								}
							}
							else if (verticesCanChange || facesCanChange)
							{
								if (random.HalfOpenFloatUnit() < perPassRandomizationFrequency)
								{
									if (verticesCanChange)
									{
										_manifold.topology.SpinEdgeForward(edge);
									}
									else
									{
										_manifold.topology.SpinEdgeForward(edge.faceEdge);
									}
								}
							}
						}

						float priorRelaxationAmount = 0f;
						for (int i = 0; i < TopologyRandomizationMaximumRelaxationPassCount; ++i)
						{
							SphericalManifold.RelaxForRegularity(_manifold, regularityRelaxedPositions);
							SphericalManifold.RelaxForEqualArea(_manifold, equalAreaRelaxedPositions, centroidsBuffer);

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
								if (SphericalManifold.ValidateAndRepair(_manifold, 0.5f))
								{
									break;
								}
							}
						}
					}
				}

				if (CalculateCentroids)
				{
					var topology = _manifold.topology;
					var vertexPositions = _manifold.vertexPositions;
					_centroids = new Vector3[topology.faces.Count];
					foreach (var face in topology.faces)
					{
						var sum = new Vector3();
						foreach (var edge in face.edges)
						{
							sum += vertexPositions.Of(edge.nextVertex);
						}
						_centroids[face] = sum;
					}

					if (Projection != TopologyProjection.Spherical)
					{
						foreach (var face in topology.faces)
						{
							_centroids[face] /= face.edges.Count;
						}
					}
					else
					{
						foreach (var face in topology.faces)
						{
							_centroids[face].Normalize();
						}
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
					_faceRegions = new int[_manifold.topology.faces.Count];

					var randomEngine = CreateRandomEngine(RegionRandomEngine, RegionRandomEngineSeed);

					if (DesiredRegionCount == 1)
					{
						_regionCount = 1;

						foreach (var face in _manifold.topology.faces)
						{
							_faceRegions[face] = 0;
						}
					}
					else if (DesiredRegionCount >= _manifold.topology.faces.Count)
					{
						_regionCount = _manifold.topology.faces.Count;

						foreach (var face in _manifold.topology.faces)
						{
							_faceRegions[face] = face.index;
						}
					}
					else
					{
						var visitor = new RandomAdjacentFaceVisitor(_manifold.topology, randomEngine);

						_regionCount = DesiredRegionCount;

						var faces = _manifold.topology.faces;
						int listIndex = 0;
						int regionIndex = 0;
						int remainingFaceCount = faces.Count;
						int remainingRegionCount = _regionCount;
						while (remainingRegionCount > 0)
						{
							if (Experilous.Random.HalfOpenRange(remainingFaceCount, randomEngine) < remainingRegionCount)
							{
								var faceIndex = faces[listIndex];
								visitor.AddSeed(faceIndex);
								_faceRegions[faceIndex] = regionIndex;
								++regionIndex;
								--remainingRegionCount;
							}

							++listIndex;
							--remainingFaceCount;
						}

						foreach (var edge in (IEnumerable<Topology.FaceEdge>)visitor)
						{
							_faceRegions[edge.farFace] = _faceRegions[edge.nearFace];
						}
					}

					_regionColors = new List<Color>(_regionCount);
					while (_regionColors.Count < _regionCount)
					{
						_regionColors.Add(new Color(
							Experilous.Random.ClosedFloatUnit(randomEngine),
							Experilous.Random.ClosedFloatUnit(randomEngine),
							Experilous.Random.ClosedFloatUnit(randomEngine)));
					}

				}
				else
				{
					_faceRegions = null;
					_regionCount = 0;
					_regionColors = null;
				}

				ClearSubmeshes();

				if (BuildMesh && SubmeshPrefab != null)
				{
					if (CalculateCentroids)
					{
						BuildSubmeshes(_manifold.topology.faces, _manifold.vertexPositions, _centroids);
					}
					else
					{
						BuildSubmeshes(_manifold.topology.faces, _manifold.vertexPositions);
					}
				}

				ClearSubmeshCache();
			}

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
			bool faceColorsExist = (_faceRegions != null && _faceRegions.Length > 0 && _regionColors != null && _regionColors.Count > 0);

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
					colors[meshVertex] = (faceColorsExist) ?  _regionColors[_faceRegions[face]] : new Color(1, 1, 1);
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
