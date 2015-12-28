using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace Experilous.Topological
{
	public class PlanarTopologyCreatorWindow : EditorWindow
	{
		private enum Tiling
		{
			Triangular,
			Quadrilateral,
			Hexagonal,
		}

		private enum TileOrientation
		{
			FlatTop,
			PointyTop,
		}

		private Tiling _tiling = Tiling.Hexagonal;
		private TileOrientation _tileOrientation = TileOrientation.PointyTop;

		private int _columnCount = 24;
		private int _rowCount = 24;

		private bool _horizontalWrapAround = false;
		private bool _verticalWrapAround = false;

		private bool _randomizeTopology = false;
		private int _topologyRandomizationPassCount = 1;
		private float _topologyRandomizationFrequency = 0.1f;
		private int _topologyRandomizationMinVertexNeighbors = 3;
		private int _topologyRandomizationMaxVertexNeighbors = 8;
		private int _topologyRandomizationMinFaceNeighbors = 3;
		private int _topologyRandomizationMaxFaceNeighbors = 8;
		private float _topologyRandomizationRelaxationRegularity = 0.5f;
		private float _topologyRandomizationRelaxationRelativePrecision = 0.05f;
		private int _topologyRandomizationMaxRelaxationPassCount = 20;
		private int _topologyRandomizationMaxRepairPassCount = 20;
		private bool _topologyRandomizationLockBoundaryPositions = true;
		private string _topologyRandomizationEngineSeed = "";

		private bool _calculateCentroids = true;
		private bool _calculateSpatialPartitioning = true;

		private bool _buildMesh = true;
		private int _horizontalMeshSegments = 1;
		private int _verticalMeshSegments = 1;
		private bool _includeWrapAroundWorld = true;
		private WrapAround.Viewport _wrapAroundViewport = null;
		private Material _meshMaterial;

		private Vector2 _scrollPosition;

		private AnimBool _showTopologyRandomizationProperties;
		private AnimBool _showMeshProperties;

		Experilous.Random _random;

		[MenuItem("GameObject/Topology/Create Planar Topology...", priority = 10)]
		public static void Create()
		{
			var window = GetWindow<PlanarTopologyCreatorWindow>(true, "Create Planar Topology", true);
			window._random = new Random(new NativeRandomEngine());

			window._showTopologyRandomizationProperties = new AnimBool(window._randomizeTopology);
			window._showMeshProperties = new AnimBool(window._buildMesh);

			window._showTopologyRandomizationProperties.valueChanged.AddListener(window.Repaint);
			window._showMeshProperties.valueChanged.AddListener(window.Repaint);

			window._random = new Experilous.Random(new NativeRandomEngine());

			window.LoadWindowData();
		}

		protected void OnGUI()
		{
			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

			EditorGUIUtility.labelWidth = 200;

			_tiling = (Tiling)EditorGUILayout.EnumPopup("Tiling", _tiling);
			if (_tiling != Tiling.Quadrilateral)
			{
				_tileOrientation = (TileOrientation)EditorGUILayout.EnumPopup("Tile Orientation", _tileOrientation);
			}

			_columnCount = EditorGUILayout.IntField("Column Count", _columnCount);
			_rowCount = EditorGUILayout.IntField("Row Count", _rowCount);

			_horizontalWrapAround = EditorGUILayout.Toggle("Horizontal Wrap-Around", _horizontalWrapAround);
			_verticalWrapAround = EditorGUILayout.Toggle("Vertical Wrap-Around", _verticalWrapAround);

			EditorGUILayout.Space();

			_randomizeTopology = EditorGUILayout.BeginToggleGroup("Randomize Topology", _randomizeTopology);
			_showTopologyRandomizationProperties.target = _randomizeTopology;
			if (EditorGUILayout.BeginFadeGroup(_showTopologyRandomizationProperties.faded))
			{
				_topologyRandomizationPassCount = EditorGUILayout.IntSlider("Randomization Pass Count", _topologyRandomizationPassCount, 1, 10);
				_topologyRandomizationFrequency = EditorGUILayout.Slider("Randomization Frequency", _topologyRandomizationFrequency, 0f, 1f);
				_topologyRandomizationMinVertexNeighbors = EditorGUILayout.IntSlider("Min Vertex Neighbors", _topologyRandomizationMinVertexNeighbors, 1, 10);
				_topologyRandomizationMaxVertexNeighbors = EditorGUILayout.IntSlider("Max Vertex Neighbors", _topologyRandomizationMaxVertexNeighbors, 1, 10);
				_topologyRandomizationMinFaceNeighbors = EditorGUILayout.IntSlider("Min Face Neighbors", _topologyRandomizationMinFaceNeighbors, 1, 10);
				_topologyRandomizationMaxFaceNeighbors = EditorGUILayout.IntSlider("Max Face Neighbors", _topologyRandomizationMaxFaceNeighbors, 1, 10);

				_topologyRandomizationRelaxationRegularity = EditorGUILayout.Slider("Relaxation Relative Regularity", _topologyRandomizationRelaxationRegularity, 0f, 1f);
				_topologyRandomizationRelaxationRelativePrecision = EditorGUILayout.Slider("Relaxation Relative Precision", _topologyRandomizationRelaxationRelativePrecision, 0f, 1f);
				_topologyRandomizationMaxRelaxationPassCount = EditorGUILayout.IntSlider("Max Relaxation Pass Count", _topologyRandomizationMaxRelaxationPassCount, 1, 100);
				_topologyRandomizationMaxRepairPassCount = EditorGUILayout.IntSlider("Max Repair Pass Count", _topologyRandomizationMaxRepairPassCount, 1, 100);
				_topologyRandomizationLockBoundaryPositions = EditorGUILayout.Toggle("Lock Boundary Positions", _topologyRandomizationLockBoundaryPositions);

				EditorGUILayout.BeginHorizontal();
				_topologyRandomizationEngineSeed = EditorGUILayout.TextField("Randomization Seed", _topologyRandomizationEngineSeed);
				if (GUILayout.Button("Randomize", GUILayout.ExpandWidth(false)))
				{
					_topologyRandomizationEngineSeed = _random.HexadecimalString(16);
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndFadeGroup();
			EditorGUILayout.EndToggleGroup();

			EditorGUILayout.Space();

			_calculateCentroids = EditorGUILayout.Toggle("Calculate Centroids", _calculateCentroids);

			_calculateSpatialPartitioning = EditorGUILayout.Toggle("Calculate Spatial Partitioning", _calculateSpatialPartitioning);

			EditorGUILayout.Space();

			_buildMesh = EditorGUILayout.BeginToggleGroup("Build Mesh", _buildMesh);
			_showMeshProperties.target = _buildMesh;
			if (EditorGUILayout.BeginFadeGroup(_showMeshProperties.faded))
			{
				_horizontalMeshSegments = EditorGUILayout.IntSlider("Horizontal Segments", _horizontalMeshSegments, 1, _columnCount);
				_verticalMeshSegments = EditorGUILayout.IntSlider("Vertical Segments", _verticalMeshSegments, 1, _rowCount);

				GUI.enabled = _horizontalWrapAround || _verticalWrapAround;
				_includeWrapAroundWorld = EditorGUILayout.Toggle("Include Wrap-Around World", _includeWrapAroundWorld);
				GUI.enabled = (_horizontalWrapAround || _verticalWrapAround) && _includeWrapAroundWorld;
				_wrapAroundViewport = (WrapAround.Viewport)EditorGUILayout.ObjectField("Wrap-Around Viewport", _wrapAroundViewport, typeof(WrapAround.Viewport), true);
				GUI.enabled = true;

				if (_meshMaterial == null) _meshMaterial = defaultMeshMaterial;
				_meshMaterial = (Material)EditorGUILayout.ObjectField("Material", _meshMaterial, typeof(Material), true);
				if (_meshMaterial == null) _meshMaterial = defaultMeshMaterial;
			}
			EditorGUILayout.EndFadeGroup();
			EditorGUILayout.EndToggleGroup();

			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button("Create"))
			{
				var gameObject = new GameObject();
				gameObject.name = "Planar Topology";
				if (Selection.activeTransform != null)
				{
					gameObject.transform.SetParent(Selection.activeTransform, false);
				}
				Selection.activeGameObject = gameObject;

				UpdateGameObject(gameObject);
				Undo.RegisterCreatedObjectUndo(gameObject, "Create Planar Topology");
			}

			GUI.enabled = (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<TopologyProvider>() != null);
			if (GUILayout.Button("Update"))
			{
				Undo.RegisterFullObjectHierarchyUndo(Selection.activeGameObject, "Update Planar Topology");
				UpdateGameObject(Selection.activeGameObject);
			}
			GUI.enabled = true;

			if (GUILayout.Button("Cancel"))
			{
				Close();
			}

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndScrollView();
		}

		protected void OnDestroy()
		{
			SaveWindowData();
		}

		protected void OnSelectionChange()
		{
			Repaint();
		}

		private void RandomizeTopology(Manifold manifold)
		{
			//var topology = manifold.topology;
			var vertexPositions = manifold.vertexPositions;

			var randomEngine = new NativeRandomEngine(RandomEngineFactory.Hash(_topologyRandomizationEngineSeed));

			System.Func<float> relaxIterationFunction;

			if (_topologyRandomizationRelaxationRegularity == 0f)
			{
				var regularityRelaxedPositions = new Vector3[vertexPositions.Length];

				relaxIterationFunction = () =>
				{
					//SphericalManifold.RelaxForRegularity(manifold, _topologyRandomizationLockBoundaryPositions, regularityRelaxedPositions);

					float relaxationAmount = 0f;
					for (int j = 0; j < vertexPositions.Length; ++j)
					{
						relaxationAmount += (vertexPositions[j] - regularityRelaxedPositions[j]).magnitude;
					}

					Utility.Swap(ref regularityRelaxedPositions, ref vertexPositions);
					manifold.vertexPositions = vertexPositions;

					return relaxationAmount;
				};
			}
			else if (_topologyRandomizationRelaxationRegularity == 1f)
			{
				var equalAreaRelaxedPositions = new Vector3[vertexPositions.Length];
				//var centroidsBuffer = new Vector3[topology.faces.Count];

				relaxIterationFunction = () =>
				{
					//SphericalManifold.RelaxForEqualArea(manifold, _topologyRandomizationLockBoundaryPositions, equalAreaRelaxedPositions, centroidsBuffer);

					float relaxationAmount = 0f;
					for (int j = 0; j < vertexPositions.Length; ++j)
					{
						relaxationAmount += (vertexPositions[j] - equalAreaRelaxedPositions[j]).magnitude;
					}

					Utility.Swap(ref equalAreaRelaxedPositions, ref vertexPositions);
					manifold.vertexPositions = vertexPositions;

					return relaxationAmount;
				};
			}
			else
			{
				var regularityRelaxedPositions = new Vector3[vertexPositions.Length];
				var equalAreaRelaxedPositions = new Vector3[vertexPositions.Length];
				//var centroidsBuffer = new Vector3[topology.faces.Count];

				var regularityWeight = _topologyRandomizationRelaxationRegularity;
				var equalAreaWeight = 1f - _topologyRandomizationRelaxationRegularity;

				relaxIterationFunction = () =>
				{
					//SphericalManifold.RelaxForRegularity(manifold, _topologyRandomizationLockBoundaryPositions, regularityRelaxedPositions);
					//SphericalManifold.RelaxForEqualArea(manifold, _topologyRandomizationLockBoundaryPositions, equalAreaRelaxedPositions, centroidsBuffer);

					float relaxationAmount = 0f;
					var weightedRelaxedPositions = regularityRelaxedPositions;
					for (int j = 0; j < vertexPositions.Length; ++j)
					{
						var weightedRelaxedPosition = regularityRelaxedPositions[j] * regularityWeight + equalAreaRelaxedPositions[j] * equalAreaWeight;
						relaxationAmount += (vertexPositions[j] - weightedRelaxedPosition).magnitude;
						weightedRelaxedPositions[j] = weightedRelaxedPosition;
					}

					regularityRelaxedPositions = vertexPositions;
					vertexPositions = weightedRelaxedPositions;

					manifold.vertexPositions = vertexPositions;

					return relaxationAmount;
				};
			}

			System.Func<bool> repairFunction = () =>
			{
				//return SphericalManifold.ValidateAndRepair(manifold, 0.5f, _topologyRandomizationLockBoundaryPositions);
				return true;
			};

			var relaxFunction = TopologyRandomizer.CreateRelaxationLoopFunction(
				_topologyRandomizationMaxRelaxationPassCount,
				_topologyRandomizationMaxRepairPassCount,
				_topologyRandomizationRelaxationRelativePrecision,
				relaxIterationFunction,
				repairFunction);

			TopologyRandomizer.Randomize(
				manifold.topology,
				_topologyRandomizationPassCount,
				_topologyRandomizationFrequency,
				_topologyRandomizationMinVertexNeighbors,
				_topologyRandomizationMaxVertexNeighbors,
				_topologyRandomizationMinFaceNeighbors,
				_topologyRandomizationMaxFaceNeighbors,
				_topologyRandomizationLockBoundaryPositions,
				new Experilous.Random(randomEngine),
				relaxFunction);
		}

		private static TComponent GetOrAddComponent<TComponent>(GameObject gameObject) where TComponent : Component
		{
			var component = gameObject.GetComponent<TComponent>();
			return (component != null) ? component : gameObject.AddComponent<TComponent>();
		}

		private static void DestroyComponents<TComponent>(GameObject gameObject) where TComponent : Component
		{
			var components = gameObject.GetComponents<TComponent>();
			foreach (var component in components) DestroyImmediate(component);
		}

		private void UpdateGameObject(GameObject gameObject)
		{
			Manifold manifold;
			Vector3[] repetitionAxes = null;
			EdgeWrapData[] edgeWrapData = null;

			switch (_tiling)
			{
				case Tiling.Triangular:
					switch (_tileOrientation)
					{
						case TileOrientation.FlatTop:
							manifold = PlanarManifold.CreateFlatTopTriGrid(_columnCount, _rowCount, _horizontalWrapAround, _verticalWrapAround);
							break;
						case TileOrientation.PointyTop:
							manifold = PlanarManifold.CreatePointyTopTriGrid(_columnCount, _rowCount, _horizontalWrapAround, _verticalWrapAround);
							break;
						default:
							throw new System.InvalidOperationException("Unexpected tile orientation value.");
					}
					break;
				case Tiling.Quadrilateral:
					manifold = PlanarManifold.CreateQuadGrid(_columnCount, _rowCount, _horizontalWrapAround, _verticalWrapAround, out repetitionAxes, out edgeWrapData);
					break;
				case Tiling.Hexagonal:
					switch (_tileOrientation)
					{
						case TileOrientation.FlatTop:
							manifold = PlanarManifold.CreateFlatTopHexGrid(_columnCount, _rowCount, _horizontalWrapAround, _verticalWrapAround);
							break;
						case TileOrientation.PointyTop:
							manifold = PlanarManifold.CreatePointyTopHexGrid(_columnCount, _rowCount, _horizontalWrapAround, _verticalWrapAround);
							break;
						default:
							throw new System.InvalidOperationException("Unexpected tile orientation value.");
					}
					break;
				default:
					throw new System.InvalidOperationException("Unexpected tiling value.");
			}

			if (_randomizeTopology) RandomizeTopology(manifold);

			GetOrAddComponent<TopologyProvider>(gameObject).topology = manifold.topology;
			GetOrAddComponent<VertexPositionsProvider>(gameObject).vertexPositions = manifold.vertexPositions;

			if (_calculateCentroids)
			{
				GetOrAddComponent<FaceCentroidsProvider>(gameObject).faceCentroids = Manifold.CalculateFaceCentroids(manifold, repetitionAxes, edgeWrapData);
			}
			else
			{
				DestroyComponents<FaceCentroidsProvider>(gameObject);
			}

			/*if (_calculateSpatialPartitioning)
			{
				GetOrAddComponent<SphericalPartitioningProvider>(gameObject).partitioning = new SphericalPartitioning(manifold);
			}
			else
			{
				DestroyComponents<SphericalPartitioningProvider>(gameObject);
			}*/

			Mesh[] meshes = null;

			var transform = gameObject.transform;
			var submeshNameRegex = new System.Text.RegularExpressions.Regex("Submesh \\d+");
			for (int i = transform.childCount - 1; i >= 0; --i)
			{
				var child = transform.GetChild(i);
				if (submeshNameRegex.IsMatch(child.name))
				{
					DestroyImmediate(child.gameObject);
				}
			}

			if (_buildMesh)
			{
				if ((_verticalMeshSegments > 1 || _horizontalMeshSegments > 1) && _includeWrapAroundWorld)
				{
					var world = GetOrAddComponent<WrapAround.AxisAlignedWrapXY2DWorld>(gameObject);
					world.minX = 0;
					world.minY = 0;
					world.maxX = _columnCount;
					world.maxY = _rowCount;
				}

				if (_calculateCentroids)
				{
					var centroids = gameObject.GetComponent<FaceCentroidsProvider>().faceCentroids;
					System.Func<int, Color> faceColorGenerator = (int faceIndex) => { return new Color(1f, 1f, 1f); };

					if (_tiling == Tiling.Quadrilateral && (_verticalMeshSegments > 1 || _horizontalMeshSegments > 1))
					{
						DestroyComponents<MeshRenderer>(gameObject);
						DestroyComponents<MeshFilter>(gameObject);

						var mapper = PlanarManifold.CreateQuadGridFaceMapper(_columnCount, _rowCount);

						var submeshIndex = 0;

						for (int y = 0; y < _verticalMeshSegments; ++y)
						{
							for (int x = 0; x < _horizontalMeshSegments; ++x)
							{
								var facesIndexer = mapper.GetRangeIndexer(
									manifold.topology,
									new CartesianIndex2D(_columnCount * x / _horizontalMeshSegments, _rowCount * y / _verticalMeshSegments),
									new CartesianIndex2D(_columnCount * (x + 1) / _horizontalMeshSegments - 1, _rowCount * (y + 1) / _verticalMeshSegments - 1));
								meshes = MeshGenerator.GenerateSubmeshes(facesIndexer, manifold.vertexPositions, repetitionAxes, edgeWrapData, centroids, centroids, faceColorGenerator);

								for (int i = 0; i < meshes.Length; ++i)
								{
									var submeshObject = new GameObject();
									submeshObject.name = "Submesh " + submeshIndex.ToString();
									submeshObject.transform.SetParent(gameObject.transform, false);
									submeshObject.AddComponent<MeshFilter>().mesh = meshes[i];
									submeshObject.AddComponent<MeshRenderer>().material = _meshMaterial;

									if ((_verticalMeshSegments > 1 || _horizontalMeshSegments > 1) && _includeWrapAroundWorld && _wrapAroundViewport != null)
									{
										var bounds = submeshObject.AddComponent<WrapAround.AxisAlignedBoxBounds>();
										bounds.box.SetMinMax(
											new Vector3(_columnCount * x / _horizontalMeshSegments, _rowCount * y / _verticalMeshSegments, 0f),
											new Vector3(_columnCount * (x + 1) / _horizontalMeshSegments, _rowCount * (y + 1) / _verticalMeshSegments, 0f));
										var element = submeshObject.AddComponent<WrapAround.RenderableElement>();
										element.viewport = _wrapAroundViewport;
										element.bounds = bounds;
									}

									++submeshIndex;
								}
							}
						}
					}
					else
					{
						meshes = MeshGenerator.GenerateSubmeshes(manifold.topology.internalFaces, manifold.vertexPositions, repetitionAxes, edgeWrapData, centroids, centroids, faceColorGenerator);

						if (meshes.Length == 1)
						{
							GetOrAddComponent<MeshFilter>(gameObject).mesh = meshes[0];
							GetOrAddComponent<MeshRenderer>(gameObject).material = _meshMaterial;
						}
						else
						{
							DestroyComponents<MeshRenderer>(gameObject);
							DestroyComponents<MeshFilter>(gameObject);

							for (int i = 0; i < meshes.Length; ++i)
							{
								var submeshObject = new GameObject();
								submeshObject.name = "Submesh " + i.ToString();
								submeshObject.transform.SetParent(gameObject.transform, false);
								submeshObject.AddComponent<MeshFilter>().mesh = meshes[i];
								submeshObject.AddComponent<MeshRenderer>().material = _meshMaterial;
							}
						}
					}
				}
				else
				{
				}
			}
			else
			{
				DestroyComponents<MeshRenderer>(gameObject);
				DestroyComponents<MeshFilter>(gameObject);
			}
		}

		private int expectedFaceCount
		{
			get
			{
				switch (_tiling)
				{
					case Tiling.Triangular:
						return _columnCount * _rowCount;
					case Tiling.Quadrilateral:
						return _columnCount * _rowCount;
					case Tiling.Hexagonal:
						return _columnCount * _rowCount;
					default:
						return 0;
				}
			}
		}

		private void SaveWindowData()
		{
			EditorPrefs.SetInt("PlanarTopologyCreatorWindow._tiling", (int)_tiling);
			EditorPrefs.SetInt("PlanarTopologyCreatorWindow._tileOrientation", (int)_tileOrientation);

			EditorPrefs.SetInt("PlanarTopologyCreatorWindow._columnCount", _columnCount);
			EditorPrefs.SetInt("PlanarTopologyCreatorWindow._rowCount", _rowCount);

			EditorPrefs.SetBool("PlanarTopologyCreatorWindow._horizontalWrapAround", _horizontalWrapAround);
			EditorPrefs.SetBool("PlanarTopologyCreatorWindow._verticalWrapAround", _verticalWrapAround);

			EditorPrefs.SetBool("PlanarTopologyCreatorWindow._randomizeTopology", _randomizeTopology);
			EditorPrefs.SetInt("PlanarTopologyCreatorWindow._topologyRandomizationPassCount", _topologyRandomizationPassCount);
			EditorPrefs.SetFloat("PlanarTopologyCreatorWindow._topologyRandomizationFrequency", _topologyRandomizationFrequency);
			EditorPrefs.SetInt("PlanarTopologyCreatorWindow._topologyRandomizationMinVertexNeighbors", _topologyRandomizationMinVertexNeighbors);
			EditorPrefs.SetInt("PlanarTopologyCreatorWindow._topologyRandomizationMaxVertexNeighbors", _topologyRandomizationMaxVertexNeighbors);
			EditorPrefs.SetInt("PlanarTopologyCreatorWindow._topologyRandomizationMinFaceNeighbors", _topologyRandomizationMinFaceNeighbors);
			EditorPrefs.SetInt("PlanarTopologyCreatorWindow._topologyRandomizationMaxFaceNeighbors", _topologyRandomizationMaxFaceNeighbors);
			EditorPrefs.SetFloat("PlanarTopologyCreatorWindow._topologyRandomizationRelaxationRegularity", _topologyRandomizationRelaxationRegularity);
			EditorPrefs.SetFloat("PlanarTopologyCreatorWindow._topologyRandomizationRelaxationRelativePrecision", _topologyRandomizationRelaxationRelativePrecision);
			EditorPrefs.SetInt("PlanarTopologyCreatorWindow._topologyRandomizationMaxRelaxationPassCount", _topologyRandomizationMaxRelaxationPassCount);
			EditorPrefs.SetInt("PlanarTopologyCreatorWindow._topologyRandomizationMaxRepairPassCount", _topologyRandomizationMaxRepairPassCount);
			EditorPrefs.SetBool("PlanarTopologyCreatorWindow._topologyRandomizationLockBoundaryPositions", _topologyRandomizationLockBoundaryPositions);
			EditorPrefs.SetString("PlanarTopologyCreatorWindow._topologyRandomizationEngineSeed", _topologyRandomizationEngineSeed);

			EditorPrefs.SetBool("PlanarTopologyCreatorWindow._calculateCentroids", _calculateCentroids);
			EditorPrefs.SetBool("PlanarTopologyCreatorWindow._calculateSpatialPartitioning", _calculateSpatialPartitioning);

			EditorPrefs.SetBool("PlanarTopologyCreatorWindow._buildMesh", _buildMesh);
			EditorPrefs.SetInt("PlanarTopologyCreatorWindow._horizontalMeshSegments", _horizontalMeshSegments);
			EditorPrefs.SetInt("PlanarTopologyCreatorWindow._verticalMeshSegments", _verticalMeshSegments);
			EditorPrefs.SetBool("PlanarTopologyCreatorWindow._includeWrapAroundWorld", _includeWrapAroundWorld);
			//EditorPrefs.SetBool("PlanarTopologyCreatorWindow._includeWrapAroundRenderableElements", _includeWrapAroundRenderableElements);
			var meshMaterialPath = (ReferenceEquals(_meshMaterial, defaultMeshMaterial) ? "" : AssetDatabase.GetAssetPath(_meshMaterial));
			EditorPrefs.SetString("PlanarTopologyCreatorWindow._meshMaterial", meshMaterialPath);
		}

		private void LoadWindowData()
		{
			_tiling = (Tiling)EditorPrefs.GetInt("PlanarTopologyCreatorWindow._tiling", (int)Tiling.Hexagonal);
			_tileOrientation = (TileOrientation)EditorPrefs.GetInt("PlanarTopologyCreatorWindow._tileOrientation", (int)TileOrientation.PointyTop);

			_columnCount = EditorPrefs.GetInt("PlanarTopologyCreatorWindow._columnCount", 24);
			_rowCount = EditorPrefs.GetInt("PlanarTopologyCreatorWindow._rowCount", 24);

			_horizontalWrapAround = EditorPrefs.GetBool("PlanarTopologyCreatorWindow._horizontalWrapAround", false);
			_verticalWrapAround = EditorPrefs.GetBool("PlanarTopologyCreatorWindow._verticalWrapAround", false);

			_randomizeTopology = EditorPrefs.GetBool("PlanarTopologyCreatorWindow._randomizeTopology", false);
			_topologyRandomizationPassCount = EditorPrefs.GetInt("PlanarTopologyCreatorWindow._topologyRandomizationPassCount", 1);
			_topologyRandomizationFrequency = EditorPrefs.GetFloat("PlanarTopologyCreatorWindow._topologyRandomizationFrequency", 0.1f);
			_topologyRandomizationMinVertexNeighbors = EditorPrefs.GetInt("PlanarTopologyCreatorWindow._topologyRandomizationMinVertexNeighbors", 3);
			_topologyRandomizationMaxVertexNeighbors = EditorPrefs.GetInt("PlanarTopologyCreatorWindow._topologyRandomizationMaxVertexNeighbors", 3);
			_topologyRandomizationMinFaceNeighbors = EditorPrefs.GetInt("PlanarTopologyCreatorWindow._topologyRandomizationMinFaceNeighbors", 5);
			_topologyRandomizationMaxFaceNeighbors = EditorPrefs.GetInt("PlanarTopologyCreatorWindow._topologyRandomizationMaxFaceNeighbors", 7);
			_topologyRandomizationRelaxationRegularity = EditorPrefs.GetFloat("PlanarTopologyCreatorWindow._topologyRandomizationRelaxationRegularity", 0.5f);
			_topologyRandomizationRelaxationRelativePrecision = EditorPrefs.GetFloat("PlanarTopologyCreatorWindow._topologyRandomizationRelaxationRelativePrecision", 0.05f);
			_topologyRandomizationMaxRelaxationPassCount = EditorPrefs.GetInt("PlanarTopologyCreatorWindow._topologyRandomizationMaxRelaxationPassCount", 20);
			_topologyRandomizationMaxRepairPassCount = EditorPrefs.GetInt("PlanarTopologyCreatorWindow._topologyRandomizationMaxRepairPassCount", 20);
			_topologyRandomizationLockBoundaryPositions = EditorPrefs.GetBool("PlanarTopologyCreatorWindow._topologyRandomizationLockBoundaryPositions", true);
			_topologyRandomizationEngineSeed = EditorPrefs.GetString("PlanarTopologyCreatorWindow._topologyRandomizationEngineSeed", "");

			_calculateCentroids = EditorPrefs.GetBool("PlanarTopologyCreatorWindow._calculateCentroids", true);
			_calculateSpatialPartitioning = EditorPrefs.GetBool("PlanarTopologyCreatorWindow._calculateSpatialPartitioning", true);

			_buildMesh = EditorPrefs.GetBool("PlanarTopologyCreatorWindow._buildMesh", true);
			_horizontalMeshSegments = EditorPrefs.GetInt("PlanarTopologyCreatorWindow._horizontalMeshSegments", 1);
			_verticalMeshSegments = EditorPrefs.GetInt("PlanarTopologyCreatorWindow._verticalMeshSegments", 1);
			_includeWrapAroundWorld = EditorPrefs.GetBool("PlanarTopologyCreatorWindow._includeWrapAroundWorld", true);
			//_includeWrapAroundRenderableElements = EditorPrefs.GetBool("PlanarTopologyCreatorWindow._includeWrapAroundRenderableElements", true);
			var meshMaterialPath = EditorPrefs.GetString("PlanarTopologyCreatorWindow._meshMaterial", "");
			_meshMaterial = (string.IsNullOrEmpty(meshMaterialPath) ? defaultMeshMaterial : AssetDatabase.LoadAssetAtPath<Material>(meshMaterialPath));
			if (_meshMaterial == null) _meshMaterial = defaultMeshMaterial;
		}

		private Material defaultMeshMaterial { get { return AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat"); } }
	}
}
