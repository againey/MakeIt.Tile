using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace Experilous.Topological
{
	public class CylindricalTopologyCreatorWindow : EditorWindow
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
		private float _maxLatitude = 60f;
		private float _projectionRegularity = 0.5f;

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
		private bool _flattenCentroids = true;
		private bool _calculateSpatialPartitioning = true;

		private bool _buildMesh = true;
		private Material _meshMaterial;

		private Vector2 _scrollPosition;

		private AnimBool _showTopologyRandomizationProperties;
		private AnimBool _showMeshProperties;

		Experilous.Random _random;

		[MenuItem("GameObject/Topology/Create Cylindrical Topology...", priority = 10)]
		public static void Create()
		{
			var window = GetWindow<CylindricalTopologyCreatorWindow>(true, "Create Cylindrical Topology", true);
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
			_maxLatitude = EditorGUILayout.FloatField("Max Latitude", _maxLatitude);

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
			if (_calculateCentroids)
			{
				_flattenCentroids = EditorGUILayout.Toggle("Flatten Centroids", _flattenCentroids);
			}

			_calculateSpatialPartitioning = EditorGUILayout.Toggle("Calculate Spatial Partitioning", _calculateSpatialPartitioning);

			EditorGUILayout.Space();

			_buildMesh = EditorGUILayout.BeginToggleGroup("Build Mesh", _buildMesh);
			_showMeshProperties.target = _buildMesh;
			if (EditorGUILayout.BeginFadeGroup(_showMeshProperties.faded))
			{
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
				gameObject.name = "Cylindrical Topology";
				if (Selection.activeTransform != null)
				{
					gameObject.transform.SetParent(Selection.activeTransform, false);
				}
				Selection.activeGameObject = gameObject;

				UpdateGameObject(gameObject);
				Undo.RegisterCreatedObjectUndo(gameObject, "Create Cylindrical Topology");
			}

			GUI.enabled = (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<TopologyProvider>() != null);
			if (GUILayout.Button("Update"))
			{
				Undo.RegisterFullObjectHierarchyUndo(Selection.activeGameObject, "Update Cylindrical Topology");
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
			var vertexPositions = manifold.vertexPositions;

			var randomEngine = new NativeRandomEngine(RandomEngineFactory.Hash(_topologyRandomizationEngineSeed));

			System.Func<float> relaxIterationFunction;

			if (_topologyRandomizationRelaxationRegularity == 0f)
			{
				var regularityRelaxedPositions = new Vector3[vertexPositions.Length];

				relaxIterationFunction = () =>
				{
					SphericalManifoldUtility.RelaxForRegularity(manifold, _topologyRandomizationLockBoundaryPositions, regularityRelaxedPositions);

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
				var centroidsBuffer = new Vector3[manifold.faces.Count];

				relaxIterationFunction = () =>
				{
					SphericalManifoldUtility.RelaxForEqualArea(manifold, _topologyRandomizationLockBoundaryPositions, equalAreaRelaxedPositions, centroidsBuffer);

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
				var centroidsBuffer = new Vector3[manifold.faces.Count];

				var regularityWeight = _topologyRandomizationRelaxationRegularity;
				var equalAreaWeight = 1f - _topologyRandomizationRelaxationRegularity;

				relaxIterationFunction = () =>
				{
					SphericalManifoldUtility.RelaxForRegularity(manifold, _topologyRandomizationLockBoundaryPositions, regularityRelaxedPositions);
					SphericalManifoldUtility.RelaxForEqualArea(manifold, _topologyRandomizationLockBoundaryPositions, equalAreaRelaxedPositions, centroidsBuffer);

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
				return SphericalManifoldUtility.ValidateAndRepair(manifold, 0.5f, _topologyRandomizationLockBoundaryPositions);
			};

			var relaxFunction = TopologyRandomizer.CreateRelaxationLoopFunction(
				_topologyRandomizationMaxRelaxationPassCount,
				_topologyRandomizationMaxRepairPassCount,
				_topologyRandomizationRelaxationRelativePrecision,
				relaxIterationFunction,
				repairFunction);

			TopologyRandomizer.Randomize(
				manifold,
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

			switch (_tiling)
			{
				case Tiling.Triangular:
					switch (_tileOrientation)
					{
						case TileOrientation.FlatTop:
							manifold = CylindricalManifoldUtility.CreateFlatTopTriGridSphericalCylinder(_columnCount, _rowCount, _maxLatitude * Mathf.Deg2Rad, _projectionRegularity);
							break;
						case TileOrientation.PointyTop:
							manifold = CylindricalManifoldUtility.CreatePointyTopTriGridSphericalCylinder(_columnCount, _rowCount, _maxLatitude * Mathf.Deg2Rad, _projectionRegularity);
							break;
						default:
							throw new System.InvalidOperationException("Unexpected tile orientation value.");
					}
					break;
				case Tiling.Quadrilateral:
					manifold = CylindricalManifoldUtility.CreateQuadGridSphericalCylinder(_columnCount, _rowCount, _maxLatitude * Mathf.Deg2Rad, _projectionRegularity);
					break;
				case Tiling.Hexagonal:
					switch (_tileOrientation)
					{
						case TileOrientation.FlatTop:
							manifold = CylindricalManifoldUtility.CreateFlatTopHexGridSphericalCylinder(_columnCount, _rowCount, _maxLatitude * Mathf.Deg2Rad, _projectionRegularity);
							break;
						case TileOrientation.PointyTop:
							manifold = CylindricalManifoldUtility.CreatePointyTopHexGridSphericalCylinder(_columnCount, _rowCount, _maxLatitude * Mathf.Deg2Rad, _projectionRegularity);
							break;
						default:
							throw new System.InvalidOperationException("Unexpected tile orientation value.");
					}
					break;
				default:
					throw new System.InvalidOperationException("Unexpected tiling value.");
			}

			if (_randomizeTopology) RandomizeTopology(manifold);

			GetOrAddComponent<TopologyProvider>(gameObject).topology = manifold;
			GetOrAddComponent<VertexPositionsProvider>(gameObject).vertexPositions = manifold.vertexPositions;

			if (_calculateCentroids)
			{
				if (_flattenCentroids)
				{
					GetOrAddComponent<FaceCentroidsProvider>(gameObject).faceCentroids = manifold.CalculateFaceCentroids();
				}
				else
				{
					GetOrAddComponent<FaceCentroidsProvider>(gameObject).faceCentroids = SphericalManifoldUtility.CalculateFaceCentroids(manifold);
				}
			}
			else
			{
				DestroyComponents<FaceCentroidsProvider>(gameObject);
			}

			if (_calculateSpatialPartitioning)
			{
				GetOrAddComponent<SphericalPartitioningProvider>(gameObject).partitioning = new SphericalPartitioning(manifold);
			}
			else
			{
				DestroyComponents<SphericalPartitioningProvider>(gameObject);
			}

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
				if (_calculateCentroids)
				{
					var centroids = gameObject.GetComponent<FaceCentroidsProvider>().faceCentroids;
					System.Func<int, Color> faceColorGenerator = (int faceIndex) => { return new Color(1f, 1f, 1f); };
					meshes = MeshGenerator.GenerateSubmeshes(manifold.internalFaces, manifold.vertexPositions, centroids, centroids, faceColorGenerator);

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
			EditorPrefs.SetInt("CylindricalTopologyCreatorWindow._tiling", (int)_tiling);
			EditorPrefs.SetInt("CylindricalTopologyCreatorWindow._tileOrientation", (int)_tileOrientation);

			EditorPrefs.SetInt("CylindricalTopologyCreatorWindow._columnCount", _columnCount);
			EditorPrefs.SetInt("CylindricalTopologyCreatorWindow._rowCount", _rowCount);
			EditorPrefs.SetFloat("CylindricalTopologyCreatorWindow._maxLatitude", _maxLatitude);
			EditorPrefs.SetFloat("CylindricalTopologyCreatorWindow._projectionRegularity", _projectionRegularity);

			EditorPrefs.SetBool("CylindricalTopologyCreatorWindow._randomizeTopology", _randomizeTopology);
			EditorPrefs.SetInt("CylindricalTopologyCreatorWindow._topologyRandomizationPassCount", _topologyRandomizationPassCount);
			EditorPrefs.SetFloat("CylindricalTopologyCreatorWindow._topologyRandomizationFrequency", _topologyRandomizationFrequency);
			EditorPrefs.SetInt("CylindricalTopologyCreatorWindow._topologyRandomizationMinVertexNeighbors", _topologyRandomizationMinVertexNeighbors);
			EditorPrefs.SetInt("CylindricalTopologyCreatorWindow._topologyRandomizationMaxVertexNeighbors", _topologyRandomizationMaxVertexNeighbors);
			EditorPrefs.SetInt("CylindricalTopologyCreatorWindow._topologyRandomizationMinFaceNeighbors", _topologyRandomizationMinFaceNeighbors);
			EditorPrefs.SetInt("CylindricalTopologyCreatorWindow._topologyRandomizationMaxFaceNeighbors", _topologyRandomizationMaxFaceNeighbors);
			EditorPrefs.SetFloat("CylindricalTopologyCreatorWindow._topologyRandomizationRelaxationRegularity", _topologyRandomizationRelaxationRegularity);
			EditorPrefs.SetFloat("CylindricalTopologyCreatorWindow._topologyRandomizationRelaxationRelativePrecision", _topologyRandomizationRelaxationRelativePrecision);
			EditorPrefs.SetInt("CylindricalTopologyCreatorWindow._topologyRandomizationMaxRelaxationPassCount", _topologyRandomizationMaxRelaxationPassCount);
			EditorPrefs.SetInt("CylindricalTopologyCreatorWindow._topologyRandomizationMaxRepairPassCount", _topologyRandomizationMaxRepairPassCount);
			EditorPrefs.SetBool("CylindricalTopologyCreatorWindow._topologyRandomizationLockBoundaryPositions", _topologyRandomizationLockBoundaryPositions);
			EditorPrefs.SetString("CylindricalTopologyCreatorWindow._topologyRandomizationEngineSeed", _topologyRandomizationEngineSeed);

			EditorPrefs.SetBool("CylindricalTopologyCreatorWindow._calculateCentroids", _calculateCentroids);
			EditorPrefs.SetBool("CylindricalTopologyCreatorWindow._flattenCentroids", _flattenCentroids);
			EditorPrefs.SetBool("CylindricalTopologyCreatorWindow._calculateSpatialPartitioning", _calculateSpatialPartitioning);

			EditorPrefs.SetBool("CylindricalTopologyCreatorWindow._buildMesh", _buildMesh);
			var meshMaterialPath = (ReferenceEquals(_meshMaterial, defaultMeshMaterial) ? "" : AssetDatabase.GetAssetPath(_meshMaterial));
			EditorPrefs.SetString("CylindricalTopologyCreatorWindow._meshMaterial", meshMaterialPath);
		}

		private void LoadWindowData()
		{
			_tiling = (Tiling)EditorPrefs.GetInt("CylindricalTopologyCreatorWindow._tiling", (int)Tiling.Hexagonal);
			_tileOrientation = (TileOrientation)EditorPrefs.GetInt("CylindricalTopologyCreatorWindow._tileOrientation", (int)TileOrientation.PointyTop);

			_columnCount = EditorPrefs.GetInt("CylindricalTopologyCreatorWindow._columnCount", 24);
			_rowCount = EditorPrefs.GetInt("CylindricalTopologyCreatorWindow._rowCount", 24);
			_maxLatitude = EditorPrefs.GetFloat("CylindricalTopologyCreatorWindow._maxLatitude", 60);
			_projectionRegularity = EditorPrefs.GetFloat("CylindricalTopologyCreatorWindow._projectionRegularity", 0.5f);

			_randomizeTopology = EditorPrefs.GetBool("CylindricalTopologyCreatorWindow._randomizeTopology", false);
			_topologyRandomizationPassCount = EditorPrefs.GetInt("CylindricalTopologyCreatorWindow._topologyRandomizationPassCount", 1);
			_topologyRandomizationFrequency = EditorPrefs.GetFloat("CylindricalTopologyCreatorWindow._topologyRandomizationFrequency", 0.1f);
			_topologyRandomizationMinVertexNeighbors = EditorPrefs.GetInt("CylindricalTopologyCreatorWindow._topologyRandomizationMinVertexNeighbors", 3);
			_topologyRandomizationMaxVertexNeighbors = EditorPrefs.GetInt("CylindricalTopologyCreatorWindow._topologyRandomizationMaxVertexNeighbors", 3);
			_topologyRandomizationMinFaceNeighbors = EditorPrefs.GetInt("CylindricalTopologyCreatorWindow._topologyRandomizationMinFaceNeighbors", 5);
			_topologyRandomizationMaxFaceNeighbors = EditorPrefs.GetInt("CylindricalTopologyCreatorWindow._topologyRandomizationMaxFaceNeighbors", 7);
			_topologyRandomizationRelaxationRegularity = EditorPrefs.GetFloat("CylindricalTopologyCreatorWindow._topologyRandomizationRelaxationRegularity", 0.5f);
			_topologyRandomizationRelaxationRelativePrecision = EditorPrefs.GetFloat("CylindricalTopologyCreatorWindow._topologyRandomizationRelaxationRelativePrecision", 0.05f);
			_topologyRandomizationMaxRelaxationPassCount = EditorPrefs.GetInt("CylindricalTopologyCreatorWindow._topologyRandomizationMaxRelaxationPassCount", 20);
			_topologyRandomizationMaxRepairPassCount = EditorPrefs.GetInt("CylindricalTopologyCreatorWindow._topologyRandomizationMaxRepairPassCount", 20);
			_topologyRandomizationLockBoundaryPositions = EditorPrefs.GetBool("CylindricalTopologyCreatorWindow._topologyRandomizationLockBoundaryPositions", true);
			_topologyRandomizationEngineSeed = EditorPrefs.GetString("CylindricalTopologyCreatorWindow._topologyRandomizationEngineSeed", "");

			_calculateCentroids = EditorPrefs.GetBool("CylindricalTopologyCreatorWindow._calculateCentroids", true);
			_flattenCentroids = EditorPrefs.GetBool("CylindricalTopologyCreatorWindow._flattenCentroids", false);
			_calculateSpatialPartitioning = EditorPrefs.GetBool("CylindricalTopologyCreatorWindow._calculateSpatialPartitioning", true);

			_buildMesh = EditorPrefs.GetBool("CylindricalTopologyCreatorWindow._buildMesh", true);
			var meshMaterialPath = EditorPrefs.GetString("CylindricalTopologyCreatorWindow._meshMaterial", "");
			_meshMaterial = (string.IsNullOrEmpty(meshMaterialPath) ? defaultMeshMaterial : AssetDatabase.LoadAssetAtPath<Material>(meshMaterialPath));
			if (_meshMaterial == null) _meshMaterial = defaultMeshMaterial;
		}

		private Material defaultMeshMaterial { get { return AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat"); } }
	}
}
