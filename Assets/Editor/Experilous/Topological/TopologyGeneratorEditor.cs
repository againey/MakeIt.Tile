using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System;

namespace Experilous.Topological
{
	[CustomEditor(typeof(TopologyGenerator))]
	public class TopologyGeneratorEditor : Editor
	{
		private static GUIContent[] _finitePlaneProjectionOptions =
		{
			new GUIContent("Planar"),
		};

		private static TopologyProjection[] _finitePlaneProjectionMapFrom =
		{
			TopologyProjection.Planar,
		};

		private static int[] _finitePlaneProjectionMapTo =
		{
			0, 0, 0, 0,
		};

		private static GUIContent[] _cylinderProjectionOptions =
		{
			new GUIContent("Planar"),
			new GUIContent("Cylindrical"),
			new GUIContent("Spherical"),
		};

		private static TopologyProjection[] _cylinderProjectionMapFrom =
		{
			TopologyProjection.Planar,
			TopologyProjection.Cylindrical,
			TopologyProjection.Spherical,
		};

		private static int[] _cylinderProjectionMapTo =
		{
			0, 1, 0, 2,
		};

		private static GUIContent[] _toroidProjectionOptions =
		{
			new GUIContent("Planar"),
			new GUIContent("Cylindrical"),
			new GUIContent("Toroidal"),
		};

		private static TopologyProjection[] _toroidProjectionMapFrom =
		{
			TopologyProjection.Planar,
			TopologyProjection.Cylindrical,
			TopologyProjection.Toroidal,
		};

		private static int[] _toroidProjectionMapTo =
		{
			0, 1, 2, 0,
		};

		private static GUIContent[] _sphereProjectionOptions =
		{
			new GUIContent("Spherical"),
		};

		private static TopologyProjection[] _sphereProjectionMapFrom =
		{
			TopologyProjection.Spherical,
		};

		private static int[] _sphereProjectionMapTo =
		{
			0, 0, 0, 0,
		};

		private Experilous.Random _random = null;

		private AnimBool _showTopologyRandomizationProperties;
		private AnimBool _showRegionProperties;
		private AnimBool _showMeshProperties;

		private Experilous.Random Random
		{
			get
			{
				if (_random == null)
				{
					_random = new Random(new NativeRandomEngine());
				}
				return _random;
			}
		}

		private void OnEnable()
		{
			var generator = (TopologyGenerator)target;

			_showTopologyRandomizationProperties = new AnimBool(generator.RandomizeTopology);
			_showRegionProperties = new AnimBool(generator.GenerateRegions);
			_showMeshProperties = new AnimBool(generator.BuildMesh);

			_showTopologyRandomizationProperties.valueChanged.AddListener(Repaint);
			_showRegionProperties.valueChanged.AddListener(Repaint);
			_showMeshProperties.valueChanged.AddListener(Repaint);
		}

		private void OnDisable()
		{
			_showTopologyRandomizationProperties.valueChanged.RemoveAllListeners();
			_showRegionProperties.valueChanged.RemoveAllListeners();
			_showMeshProperties.valueChanged.RemoveAllListeners();

			_showTopologyRandomizationProperties = null;
			_showRegionProperties = null;
			_showMeshProperties = null;
		}

		public override void OnInspectorGUI()
		{
			var generator = (TopologyGenerator)target;

			generator.Classification = (TopologyClassification)EditorGUILayout.EnumPopup("Classification", generator.Classification);

			switch (generator.Classification)
			{
				case TopologyClassification.FinitePlane:
					generator.Projection = _finitePlaneProjectionMapFrom[EditorGUILayout.Popup(new GUIContent("Projection"), _finitePlaneProjectionMapTo[(int)generator.Projection], _finitePlaneProjectionOptions)];
					break;
				case TopologyClassification.Cylinder:
					generator.Projection = _cylinderProjectionMapFrom[EditorGUILayout.Popup(new GUIContent("Projection"), _cylinderProjectionMapTo[(int)generator.Projection], _cylinderProjectionOptions)];
					break;
				case TopologyClassification.Toroid:
					generator.Projection = _toroidProjectionMapFrom[EditorGUILayout.Popup(new GUIContent("Projection"), _toroidProjectionMapTo[(int)generator.Projection], _toroidProjectionOptions)];
					break;
				case TopologyClassification.Sphere:
					generator.Projection = _sphereProjectionMapFrom[EditorGUILayout.Popup(new GUIContent("Projection"), _sphereProjectionMapTo[(int)generator.Projection], _sphereProjectionOptions)];
					break;
				default:
					throw new InvalidOperationException();
			}

			switch (generator.Classification)
			{
				case TopologyClassification.FinitePlane:
				case TopologyClassification.Cylinder:
				case TopologyClassification.Toroid:
					OnInspectorGUI_Flat(generator);
					break;
				case TopologyClassification.Sphere:
					OnInspectorGUI_Spherical(generator);
					break;
				default:
					throw new InvalidOperationException();
			}

			EditorGUILayout.Space();

			generator.RandomizeTopology = EditorGUILayout.BeginToggleGroup("Randomize Topology", generator.RandomizeTopology);
			_showTopologyRandomizationProperties.target = generator.RandomizeTopology;
			if (EditorGUILayout.BeginFadeGroup(_showTopologyRandomizationProperties.faded))
			{
				generator.TopologyRandomizationPassCount = EditorGUILayout.IntSlider("Randomization Pass Count", generator.TopologyRandomizationPassCount, 1, 10);
				generator.TopologyRandomizationFrequency = EditorGUILayout.Slider("Randomization Frequency", generator.TopologyRandomizationFrequency, 0f, 1f);
				generator.TopologyRandomizationMinimumVertexNeighbors = EditorGUILayout.IntSlider("Minimum Vertex Neighbors", generator.TopologyRandomizationMinimumVertexNeighbors, 1, 10);
				generator.TopologyRandomizationMaximumVertexNeighbors = EditorGUILayout.IntSlider("Maximum Vertex Neighbors", generator.TopologyRandomizationMaximumVertexNeighbors, 1, 10);
				generator.TopologyRandomizationMinimumFaceNeighbors = EditorGUILayout.IntSlider("Minimum Face Neighbors", generator.TopologyRandomizationMinimumFaceNeighbors, 1, 10);
				generator.TopologyRandomizationMaximumFaceNeighbors = EditorGUILayout.IntSlider("Maximum Face Neighbors", generator.TopologyRandomizationMaximumFaceNeighbors, 1, 10);

				generator.TopologyRandomizationRelaxationRegularity = EditorGUILayout.Slider("Relaxation Relative Regularity", generator.TopologyRandomizationRelaxationRegularity, 0f, 1f);
				generator.TopologyRandomizationRelaxationRelativePrecision = EditorGUILayout.Slider("Relaxation Relative Precision", generator.TopologyRandomizationRelaxationRelativePrecision, 0f, 1f);
				generator.TopologyRandomizationMaximumRelaxationPassCount = EditorGUILayout.IntSlider("Maximum Relaxation Pass Count", generator.TopologyRandomizationMaximumRelaxationPassCount, 1, 100);
				generator.TopologyRandomizationMaximumRepairPassCount = EditorGUILayout.IntSlider("Maximum Repair Pass Count", generator.TopologyRandomizationMaximumRepairPassCount, 1, 100);

				generator.TopologyRandomizationEngine = (RandomEngineFactory)EditorGUILayout.ObjectField("Randomization Engine", generator.TopologyRandomizationEngine, typeof(RandomEngineFactory), true);

				EditorGUILayout.BeginHorizontal();
				generator.TopologyRandomizationEngineSeed = EditorGUILayout.TextField("Randomization Seed", generator.TopologyRandomizationEngineSeed);
				if (GUILayout.Button("Randomize", GUILayout.ExpandWidth(false)))
				{
					generator.TopologyRandomizationEngineSeed = Random.HexadecimalString(16);
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndFadeGroup();
			EditorGUILayout.EndToggleGroup();

			EditorGUILayout.Space();

			generator.CalculateCentroids = EditorGUILayout.Toggle("Calculate Centroids", generator.CalculateCentroids);
			generator.CalculateSpatialPartitioning = EditorGUILayout.Toggle("Calculate Spatial Partitioning", generator.CalculateSpatialPartitioning);

			EditorGUILayout.Space();

			generator.GenerateRegions = EditorGUILayout.BeginToggleGroup("Generate Regions", generator.GenerateRegions);
			_showRegionProperties.target = generator.GenerateRegions;
			if (EditorGUILayout.BeginFadeGroup(_showRegionProperties.faded))
			{
				generator.DesiredRegionCount = EditorGUILayout.IntSlider("Region Count", generator.DesiredRegionCount, 1, generator.ExpectedFaceCount);

				generator.RegionRandomEngine = (RandomEngineFactory)EditorGUILayout.ObjectField("Region Random Engine", generator.TopologyRandomizationEngine, typeof(RandomEngineFactory), true);

				EditorGUILayout.BeginHorizontal();
				generator.RegionRandomEngineSeed = EditorGUILayout.TextField("Region Random Seed", generator.RegionRandomEngineSeed);
				if (GUILayout.Button("Randomize", GUILayout.ExpandWidth(false)))
				{
					generator.RegionRandomEngineSeed = Random.HexadecimalString(16);
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndFadeGroup();
			EditorGUILayout.EndToggleGroup();

			EditorGUILayout.Space();

			generator.BuildMesh = EditorGUILayout.BeginToggleGroup("Build Mesh", generator.BuildMesh);
			_showMeshProperties.target = generator.BuildMesh;
			if (EditorGUILayout.BeginFadeGroup(_showMeshProperties.faded))
			{
				generator.SubmeshPrefab = (UniqueMesh)EditorGUILayout.ObjectField("Submesh Prefab", generator.SubmeshPrefab, typeof(UniqueMesh), true);
			}
			EditorGUILayout.EndFadeGroup();
			EditorGUILayout.EndToggleGroup();

			EditorGUILayout.Space();

			if (GUILayout.Button("Regenerate"))
			{
				generator.Invalidate();
				EditorUtility.SetDirty(generator);
			}
		}

		private void OnInspectorGUI_Flat(TopologyGenerator generator)
		{
			generator.FlatTiling = (FlatTopologyTiling)EditorGUILayout.EnumPopup("Tiling", generator.FlatTiling);

			switch (generator.FlatTiling)
			{
				case FlatTopologyTiling.Triangular:
				case FlatTopologyTiling.Hexagonal:
					generator.TileOrientation = (TopologyTileOrientation)EditorGUILayout.EnumPopup("Tile Orientation", generator.TileOrientation);
					break;
			}

			generator.ColumnCount = EditorGUILayout.IntField("Column Count", generator.ColumnCount);
			generator.RowCount = EditorGUILayout.IntField("Row Count", generator.RowCount);
			generator.MaximumLatitude = EditorGUILayout.Slider("Maximum Latitude", generator.MaximumLatitude, 0f, 90f);
			generator.ProjectionRegularity = EditorGUILayout.Slider("Regularity", generator.ProjectionRegularity, 0f, 1f);
		}

		private void OnInspectorGUI_Spherical(TopologyGenerator generator)
		{
			generator.SphericalTiling = (SphericalTopologyTiling)EditorGUILayout.EnumPopup("Tiling", generator.SphericalTiling);

			generator.SubdivisionDegree = EditorGUILayout.IntField("Subdivision Degree", generator.SubdivisionDegree);
			generator.UseDualTopology = EditorGUILayout.Toggle("Use Dual Topology", generator.UseDualTopology);
		}
	}
}
