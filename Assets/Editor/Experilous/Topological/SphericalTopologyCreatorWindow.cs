using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace Experilous.Topological
{
	public class SphericalTopologyCreatorWindow : EditorWindow
	{
		private SphericalPolyhedron _basePolyhedron;
		private int _subdivisionDegree;
		private bool _useDualPolyhedron;

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

		[MenuItem("GameObject/Topology/Create Spherical Topology...", priority = 10)]
		public static void Create()
		{
			var window = GetWindow<SphericalTopologyCreatorWindow>(true, "Create Spherical Topology", true);
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

			_basePolyhedron = (SphericalPolyhedron)EditorGUILayout.EnumPopup("Base Polyhedron", _basePolyhedron);

			_subdivisionDegree = EditorGUILayout.IntField("Subdivision Degree", _subdivisionDegree);
			_useDualPolyhedron = EditorGUILayout.Toggle("Use Dual Polyhedron", _useDualPolyhedron);

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
				gameObject.name = "Spherical Topology";
				if (Selection.activeTransform != null)
				{
					gameObject.transform.SetParent(Selection.activeTransform, false);
				}
				Selection.activeGameObject = gameObject;

				UpdateGameObject(gameObject);
				Undo.RegisterCreatedObjectUndo(gameObject, "Create Spherical Topology");
			}

			GUI.enabled = (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<TopologyProvider>() != null);
			if (GUILayout.Button("Update"))
			{
				Undo.RegisterFullObjectHierarchyUndo(Selection.activeGameObject, "Update Spherical Topology");
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

			if (_topologyRandomizationRelaxationRegularity == 1f)
			{
				var regularityRelaxedPositions = new Vector3[vertexPositions.Length];

				relaxIterationFunction = () =>
				{
					SphericalManifoldUtility.RelaxForRegularity(manifold, false, regularityRelaxedPositions);

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
			else if (_topologyRandomizationRelaxationRegularity == 0f)
			{
				var equalAreaRelaxedPositions = new Vector3[vertexPositions.Length];
				var centroidsBuffer = new Vector3[manifold.faces.Count];

				relaxIterationFunction = () =>
				{
					SphericalManifoldUtility.RelaxForEqualArea(manifold, false, equalAreaRelaxedPositions, centroidsBuffer);

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
					SphericalManifoldUtility.RelaxForRegularity(manifold, false, regularityRelaxedPositions);
					SphericalManifoldUtility.RelaxForEqualArea(manifold, false, equalAreaRelaxedPositions, centroidsBuffer);

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
				return SphericalManifoldUtility.ValidateAndRepair(manifold, 0.5f, false);
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
				false,
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
			var manifold = SphericalManifoldUtility.Create(_basePolyhedron, _subdivisionDegree, _useDualPolyhedron);

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

		private int TriangularSubdividedVertexCount(int vertexCount, int edgeCount, int faceCount)
		{
			return vertexCount + edgeCount * _subdivisionDegree + faceCount * (_subdivisionDegree - 1) * _subdivisionDegree / 2;
		}

		private int TriangularSubdividedFaceCount(int faceCount)
		{
			return faceCount * (_subdivisionDegree + 1) * (_subdivisionDegree + 1);
		}

		private int QuadrilateralSubdividedVertexCount(int vertexCount, int edgeCount, int faceCount)
		{
			return vertexCount + edgeCount * _subdivisionDegree + faceCount * _subdivisionDegree * _subdivisionDegree;
		}

		private int QuadrilateralSubdividedFaceCount(int faceCount)
		{
			return faceCount * (_subdivisionDegree + 1) * (_subdivisionDegree + 1);
		}

		private int expectedFaceCount
		{
			get
			{
				switch (_basePolyhedron)
				{
					case SphericalPolyhedron.Tetrahedron:
						if (!_useDualPolyhedron)
							return TriangularSubdividedFaceCount(4);
						else
							return TriangularSubdividedVertexCount(4, 6, 4);
					case SphericalPolyhedron.Cube:
						if (!_useDualPolyhedron)
							return QuadrilateralSubdividedFaceCount(6);
						else
							return QuadrilateralSubdividedVertexCount(8, 12, 6);
					case SphericalPolyhedron.Octahedron:
						if (!_useDualPolyhedron)
							return TriangularSubdividedFaceCount(8);
						else
							return TriangularSubdividedVertexCount(6, 12, 8);
					case SphericalPolyhedron.Dodecahedron:
						if (!_useDualPolyhedron)
							return TriangularSubdividedVertexCount(12, 30, 20);
						else
							return TriangularSubdividedFaceCount(20);
					case SphericalPolyhedron.Icosahedron:
						if (!_useDualPolyhedron)
							return TriangularSubdividedFaceCount(20);
						else
							return TriangularSubdividedVertexCount(12, 30, 20);
					default:
						return 0;
				}
			}
		}

		private void SaveWindowData()
		{
			EditorPrefs.SetInt("SphericalTopologyCreatorWindow._basePolyhedron", (int)_basePolyhedron);
			EditorPrefs.SetInt("SphericalTopologyCreatorWindow._subdivisionDegree", _subdivisionDegree);
			EditorPrefs.SetBool("SphericalTopologyCreatorWindow._useDualPolyhedron", _useDualPolyhedron);

			EditorPrefs.SetBool("SphericalTopologyCreatorWindow._randomizeTopology", _randomizeTopology);
			EditorPrefs.SetInt("SphericalTopologyCreatorWindow._topologyRandomizationPassCount", _topologyRandomizationPassCount);
			EditorPrefs.SetFloat("SphericalTopologyCreatorWindow._topologyRandomizationFrequency", _topologyRandomizationFrequency);
			EditorPrefs.SetInt("SphericalTopologyCreatorWindow._topologyRandomizationMinVertexNeighbors", _topologyRandomizationMinVertexNeighbors);
			EditorPrefs.SetInt("SphericalTopologyCreatorWindow._topologyRandomizationMaxVertexNeighbors", _topologyRandomizationMaxVertexNeighbors);
			EditorPrefs.SetInt("SphericalTopologyCreatorWindow._topologyRandomizationMinFaceNeighbors", _topologyRandomizationMinFaceNeighbors);
			EditorPrefs.SetInt("SphericalTopologyCreatorWindow._topologyRandomizationMaxFaceNeighbors", _topologyRandomizationMaxFaceNeighbors);
			EditorPrefs.SetFloat("SphericalTopologyCreatorWindow._topologyRandomizationRelaxationRegularity", _topologyRandomizationRelaxationRegularity);
			EditorPrefs.SetFloat("SphericalTopologyCreatorWindow._topologyRandomizationRelaxationRelativePrecision", _topologyRandomizationRelaxationRelativePrecision);
			EditorPrefs.SetInt("SphericalTopologyCreatorWindow._topologyRandomizationMaxRelaxationPassCount", _topologyRandomizationMaxRelaxationPassCount);
			EditorPrefs.SetInt("SphericalTopologyCreatorWindow._topologyRandomizationMaxRepairPassCount", _topologyRandomizationMaxRepairPassCount);
			EditorPrefs.SetString("SphericalTopologyCreatorWindow._topologyRandomizationEngineSeed", _topologyRandomizationEngineSeed);

			EditorPrefs.SetBool("SphericalTopologyCreatorWindow._calculateCentroids", _calculateCentroids);
			EditorPrefs.SetBool("SphericalTopologyCreatorWindow._flattenCentroids", _flattenCentroids);
			EditorPrefs.SetBool("SphericalTopologyCreatorWindow._calculateSpatialPartitioning", _calculateSpatialPartitioning);

			EditorPrefs.SetBool("SphericalTopologyCreatorWindow._buildMesh", _buildMesh);
			var meshMaterialPath = (ReferenceEquals(_meshMaterial, defaultMeshMaterial) ? "" : AssetDatabase.GetAssetPath(_meshMaterial));
			EditorPrefs.SetString("SphericalTopologyCreatorWindow._meshMaterial", meshMaterialPath);
		}

		private void LoadWindowData()
		{
			_basePolyhedron = (SphericalPolyhedron)EditorPrefs.GetInt("SphericalTopologyCreatorWindow._basePolyhedron", (int)SphericalPolyhedron.Icosahedron);
			_subdivisionDegree = EditorPrefs.GetInt("SphericalTopologyCreatorWindow._subdivisionDegree", 10);
			_useDualPolyhedron = EditorPrefs.GetBool("SphericalTopologyCreatorWindow._useDualPolyhedron", true);

			_randomizeTopology = EditorPrefs.GetBool("SphericalTopologyCreatorWindow._randomizeTopology", false);
			_topologyRandomizationPassCount = EditorPrefs.GetInt("SphericalTopologyCreatorWindow._topologyRandomizationPassCount", 1);
			_topologyRandomizationFrequency = EditorPrefs.GetFloat("SphericalTopologyCreatorWindow._topologyRandomizationFrequency", 0.1f);
			_topologyRandomizationMinVertexNeighbors = EditorPrefs.GetInt("SphericalTopologyCreatorWindow._topologyRandomizationMinVertexNeighbors", 3);
			_topologyRandomizationMaxVertexNeighbors = EditorPrefs.GetInt("SphericalTopologyCreatorWindow._topologyRandomizationMaxVertexNeighbors", 3);
			_topologyRandomizationMinFaceNeighbors = EditorPrefs.GetInt("SphericalTopologyCreatorWindow._topologyRandomizationMinFaceNeighbors", 5);
			_topologyRandomizationMaxFaceNeighbors = EditorPrefs.GetInt("SphericalTopologyCreatorWindow._topologyRandomizationMaxFaceNeighbors", 7);
			_topologyRandomizationRelaxationRegularity = EditorPrefs.GetFloat("SphericalTopologyCreatorWindow._topologyRandomizationRelaxationRegularity", 0.5f);
			_topologyRandomizationRelaxationRelativePrecision = EditorPrefs.GetFloat("SphericalTopologyCreatorWindow._topologyRandomizationRelaxationRelativePrecision", 0.05f);
			_topologyRandomizationMaxRelaxationPassCount = EditorPrefs.GetInt("SphericalTopologyCreatorWindow._topologyRandomizationMaxRelaxationPassCount", 20);
			_topologyRandomizationMaxRepairPassCount = EditorPrefs.GetInt("SphericalTopologyCreatorWindow._topologyRandomizationMaxRepairPassCount", 20);
			_topologyRandomizationEngineSeed = EditorPrefs.GetString("SphericalTopologyCreatorWindow._topologyRandomizationEngineSeed", "");

			_calculateCentroids = EditorPrefs.GetBool("SphericalTopologyCreatorWindow._calculateCentroids", true);
			_flattenCentroids = EditorPrefs.GetBool("SphericalTopologyCreatorWindow._flattenCentroids", false);
			_calculateSpatialPartitioning = EditorPrefs.GetBool("SphericalTopologyCreatorWindow._calculateSpatialPartitioning", true);

			_buildMesh = EditorPrefs.GetBool("SphericalTopologyCreatorWindow._buildMesh", true);
			var meshMaterialPath = EditorPrefs.GetString("SphericalTopologyCreatorWindow._meshMaterial", "");
			_meshMaterial = (string.IsNullOrEmpty(meshMaterialPath) ? defaultMeshMaterial : AssetDatabase.LoadAssetAtPath<Material>(meshMaterialPath));
			if (_meshMaterial == null) _meshMaterial = defaultMeshMaterial;
		}

		private Material defaultMeshMaterial { get { return AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat"); } }
	}
}
