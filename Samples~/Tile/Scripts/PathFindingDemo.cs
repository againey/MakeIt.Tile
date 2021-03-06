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

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using MakeIt.Random;
using MakeIt.Tile;
using MakeIt.Numerics;

namespace MakeIt.Tile.Samples
{
	[RequireComponent(typeof(FaceSpatialPartitioningPicker))]
	public class PathFindingDemo : MonoBehaviour
	{
		public MeshFilter planetMeshPrefab;
		public Transform unitPrefab;
		public Cubemap whiteCubeMap;

		public Transform planetMeshes;
		public Transform units;

		public GameObject controlsPanel;

		public OrbitalCameraController orbitalCamera;
		public PivotalCameraController pivotalCamera;
		public Light exteriorLight;
		public Light interiorLight;

		public float movementDuration = 0.1f;

		[Space(20)]
		public int topologySubdivision = 20;
		public bool isInverted = false;

		[Space(20)]
		public int geographicalRegionCount = 500;
		public int grassWeight = 5;
		public int waterWeight = 8;
		public int desertWeight = 2;
		public int mountainWeight = 1;
		public int fullSightDistance = 3;
		public int limitedSightDistance = 5;

		[Space(20)]
		public int unitCount = 20;

		private SphericalSurface _surface;
		private Topology _topology;
		private PositionalVertexAttribute _vertexPositions;
		private PositionalFaceAttribute _facePositions;
		private IEdgeAttribute<Vector3> _innerAngleBisectors;
		private IEdgeAttribute<Vector3> _innerVertexPositions;
		private IFaceAttribute<Vector3> _faceNormals;
		private IFaceAttribute<UVFrame3> _faceUVFrames;
		private IEdgeAttribute<Vector2> _faceOuterEdgeUVs;
		private IEdgeAttribute<Vector2> _faceInnerEdgeUVs;
		private IFaceAttribute<Vector2> _faceCenterUVs;
		private IFaceAttribute<int> _faceTerrainIndices;
		private IFaceAttribute<bool> _faceSeenStates;
		private IFaceAttribute<int> _faceSightCounts;
		private IFaceAttribute<Transform> _faceUnits;

		private DynamicMesh _dynamicMesh;

		private UniversalFaceSpatialPartitioning _partitioning;
		private FaceSpatialPartitioningPicker _picker;

		private float _maximumFaceDistance;

		private Topology.Face _selectedFace;

		[NonSerialized] private bool _initialized;

		[NonSerialized] private IRandom _random;

		[NonSerialized] private DynamicMesh.ITriangulation _faceTriangulation;
		[NonSerialized] private DynamicMesh.ITriangulation _selectedFaceTriangulation;

		[NonSerialized] private PathFinder _pathFinder;
		[NonSerialized] private IFaceEdgePath _path;
		[NonSerialized] private bool _moving;

		[NonSerialized] private Vector3 _lightAxis;
		[NonSerialized] private Vector3 _lightVector;

		protected void OnEnable()
		{
			if (!_initialized)
			{
				_random = XorShift128Plus.Create(948675);

				_faceTriangulation = new SeparatedFacesUmbrellaTriangulation(
					(Topology.FaceEdge edge, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
					{
						var face = edge.nearFace;
						var gridOverlayU = GetGridOverlayU(false, _faceSeenStates[face], _faceSightCounts[face]);
						vertexAttributes.uv2 = new Vector2(gridOverlayU, 0f);
						vertexAttributes.Advance();
						vertexAttributes.uv2 = new Vector2(gridOverlayU, 0.5f);
						vertexAttributes.Advance();
					},
					(Topology.Face face, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
					{
						vertexAttributes.uv2 = new Vector2(GetGridOverlayU(false, _faceSeenStates[face], _faceSightCounts[face]), 1f);
						vertexAttributes.Advance();
					});

				_selectedFaceTriangulation = new SeparatedFacesUmbrellaTriangulation(
					(Topology.FaceEdge edge, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
					{
						var face = edge.nearFace;
						var gridOverlayU = GetGridOverlayU(true, _faceSeenStates[face], _faceSightCounts[face]);
						vertexAttributes.uv2 = new Vector2(gridOverlayU, 0f);
						vertexAttributes.Advance();
						vertexAttributes.uv2 = new Vector2(gridOverlayU, 0.5f);
						vertexAttributes.Advance();
					},
					(Topology.Face face, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
					{
						vertexAttributes.uv2 = new Vector2(GetGridOverlayU(true, _faceSeenStates[face], _faceSightCounts[face]), 1f);
						vertexAttributes.Advance();
					});

				_pathFinder = new PathFinder();

				_initialized = true;
			}
		}

		protected void Start()
		{
			orbitalCamera.enabled = !isInverted;
			pivotalCamera.enabled = isInverted;

			exteriorLight.enabled = !isInverted;
			interiorLight.enabled = isInverted;

			_lightVector = (isInverted ? interiorLight : exteriorLight).transform.position;
			_lightAxis = Vector3.Cross(Vector3.Cross(Vector3.up, _lightVector), _lightVector);

			RenderSettings.ambientLight = new Color(0.3f, 0.4f, 0.5f);
			RenderSettings.ambientIntensity = 0.25f;
			RenderSettings.customReflection = whiteCubeMap;
			RenderSettings.reflectionIntensity = 0.125f;
			RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;

			_picker = GetComponent<FaceSpatialPartitioningPicker>();

			_surface = SphericalSurface.Create(Vector3.up, Vector3.right, 10f, isInverted);
			Vector3[] baseVertexPositionsArray;
			Topology baseTopology;
			SphericalManifoldUtility.CreateIcosahedron(_surface, out baseTopology, out baseVertexPositionsArray);

			Vector3[] vertexPositionsArray;
			SphericalManifoldUtility.Subdivide(_surface, baseTopology, baseVertexPositionsArray.AsVertexAttribute(), topologySubdivision, out _topology, out vertexPositionsArray);

			_vertexPositions = PositionalVertexAttribute.Create(_surface, vertexPositionsArray);

			SphericalManifoldUtility.MakeDual(_surface, _topology, _vertexPositions, out vertexPositionsArray);
			_vertexPositions = PositionalVertexAttribute.Create(_surface, vertexPositionsArray);

			var regularityWeight = 0.5f;
			var equalAreaWeight = 1f - regularityWeight;

			var regularityRelaxedVertexPositions = new Vector3[_topology.vertices.Count].AsVertexAttribute();
			var equalAreaRelaxedVertexPositions = new Vector3[_topology.vertices.Count].AsVertexAttribute();
			var relaxedVertexPositions = regularityRelaxedVertexPositions;
			var faceCentroids = PositionalFaceAttribute.Create(_surface, _topology.internalFaces.Count);
			var faceCentroidAngles = new float[_topology.faceEdges.Count].AsEdgeAttribute();
			var vertexAreas = new float[_topology.vertices.Count].AsVertexAttribute();

			FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(_topology.internalFaces, _vertexPositions, faceCentroids);
			VertexAttributeUtility.CalculateVertexAreasFromVertexPositionsAndFaceCentroids(_topology.vertices, _vertexPositions, faceCentroids, vertexAreas);

			Func<float> relaxIterationFunction = () =>
			{
				SphericalManifoldUtility.RelaxVertexPositionsForRegularity(_surface, _topology, _vertexPositions, true, regularityRelaxedVertexPositions);
				SphericalManifoldUtility.RelaxVertexPositionsForEqualArea(_surface, _topology, _vertexPositions, true, equalAreaRelaxedVertexPositions, faceCentroids, faceCentroidAngles, vertexAreas);
				for (int i = 0; i < relaxedVertexPositions.Count; ++i)
				{
					relaxedVertexPositions[i] = regularityRelaxedVertexPositions[i] * regularityWeight + equalAreaRelaxedVertexPositions[i] * equalAreaWeight;
				}
				var relaxationAmount = SphericalManifoldUtility.CalculateRelaxationAmount(_vertexPositions, relaxedVertexPositions);
				for (int i = 0; i < _vertexPositions.Count; ++i)
				{
					_vertexPositions[i] = relaxedVertexPositions[i];
				}
				return relaxationAmount;
			};

			Func<bool> repairFunction = () =>
			{
				return SphericalManifoldUtility.ValidateAndRepair(_surface, _topology, _vertexPositions, 0.5f, true);
			};

			Action relaxationLoopFunction = TopologyRandomizer.CreateRelaxationLoopFunction(20, 20, 0.95f, relaxIterationFunction, repairFunction);

			TopologyRandomizer.Randomize(
				_topology, 1, 0.1f,
				3, 3, 5, 7, true,
				_random,
				relaxationLoopFunction);

			_facePositions = PositionalFaceAttribute.Create(_surface, _topology.internalFaces.Count);
			FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(_topology.internalFaces, _vertexPositions, _facePositions);

			_maximumFaceDistance = 0f;
			foreach (var edge in _topology.faceEdges)
			{
				var distance = Geometry.AngleBetweenVectors(_facePositions[edge.nearFace], _facePositions[edge.farFace]) * _surface.radius;
				_maximumFaceDistance = Mathf.Max(_maximumFaceDistance, distance);
			}

			_innerAngleBisectors = EdgeAttributeUtility.CalculateFaceEdgeBisectorsFromVertexPositions(_topology.internalFaces, _vertexPositions, _facePositions);

			_innerVertexPositions = new Vector3[_topology.faceEdges.Count].AsEdgeAttribute();
			foreach (var edge in _topology.faceEdges)
			{
				_innerVertexPositions[edge] = _vertexPositions[edge] + _innerAngleBisectors[edge] * 0.03f;
			}

			_faceNormals = FaceAttributeUtility.CalculateFaceNormalsFromSurface(_topology.faces, _surface, _facePositions);
			_faceUVFrames = FaceAttributeUtility.CalculatePerFaceSphericalUVFramesFromFaceNormals(_topology.faces, _faceNormals, Quaternion.identity);
			_faceOuterEdgeUVs = EdgeAttributeUtility.CalculatePerFaceUnnormalizedUVsFromVertexPositions(_topology.faces, _vertexPositions, _faceUVFrames);
			_faceInnerEdgeUVs = EdgeAttributeUtility.CalculatePerFaceUnnormalizedUVsFromVertexPositions(_topology.faces, _innerVertexPositions, _faceUVFrames);
			_faceCenterUVs = FaceAttributeUtility.CalculateUnnormalizedUVsFromFacePositions(_topology.faces, _facePositions, _faceUVFrames);

			var faceMinUVs = new Vector2[_topology.faces.Count].AsFaceAttribute();
			var faceRangeUVs = new Vector2[_topology.faces.Count].AsFaceAttribute();

			FaceAttributeUtility.CalculateFaceEdgeMinAndRangeValues(_topology.faces, _faceOuterEdgeUVs, faceMinUVs, faceRangeUVs);

			foreach (var face in _topology.faces)
			{
				var uvMin = faceMinUVs[face];
				var uvRange = faceRangeUVs[face];
				var adjusted = AspectRatioUtility.Expand(new Rect(uvMin.x, uvMin.y, uvRange.x, uvRange.y), 1f);
				faceMinUVs[face] = adjusted.min;
				faceRangeUVs[face] = adjusted.size;
			}

			_faceOuterEdgeUVs = EdgeAttributeUtility.CalculatePerFaceUniformlyNormalizedUVsFromFaceUVMinAndRange(_topology.faces, faceMinUVs, faceRangeUVs, _faceOuterEdgeUVs);
			_faceInnerEdgeUVs = EdgeAttributeUtility.CalculatePerFaceUniformlyNormalizedUVsFromFaceUVMinAndRange(_topology.faces, faceMinUVs, faceRangeUVs, _faceInnerEdgeUVs);
			_faceCenterUVs = FaceAttributeUtility.CalculateUniformlyNormalizedUVsFromFaceUVMinAndRange(_topology.faces, faceMinUVs, faceRangeUVs, _faceCenterUVs);

			_partitioning = UniversalFaceSpatialPartitioning.Create(_surface, _topology, _vertexPositions);
			_picker.partitioning = _partitioning;
			_picker.enabled = true;

			_faceTerrainIndices = new int[_topology.faces.Count].AsFaceAttribute();
			var terrainWeights = new int[] { grassWeight, waterWeight, desertWeight, mountainWeight };
			int terrainWeightSum = 0;
			foreach (var weight in terrainWeights) terrainWeightSum += weight;

			var rootFaces = new List<Topology.Face>();
			var rootFaceEdges = new List<Topology.FaceEdge>();
			for (int regionIndex = 0; regionIndex < geographicalRegionCount; ++regionIndex)
			{
				Topology.Face face;
				do
				{
					face = _topology.internalFaces[_random.Index(_topology.internalFaces.Count)];
				} while (rootFaces.Contains(face));
				rootFaces.Add(face);
				foreach (var edge in face.edges)
				{
					rootFaceEdges.Add(edge);
				}
				_faceTerrainIndices[face] = _random.WeightedIndex(terrainWeights, terrainWeightSum);
			}

			TopologyVisitor.VisitFacesInRandomOrder(rootFaceEdges, (FaceEdgeVisitor visitor) =>
			{
				_faceTerrainIndices[visitor.edge.farFace] = _faceTerrainIndices[visitor.edge.nearFace];

				visitor.VisitInternalNeighborsExceptSource();
			},
			_random);

			_faceSeenStates = new bool[_topology.faces.Count].AsFaceAttribute();
			_faceSightCounts = new int[_topology.faces.Count].AsFaceAttribute();

			var triangulation = new SeparatedFacesUmbrellaTriangulation(2,
				(Topology.FaceEdge edge, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
				{
					var face = edge.nearFace;
					var faceNormal = _faceNormals[face];
					var gridOverlayU = GetGridOverlayU(false, _faceSeenStates[face], _faceSightCounts[face]);

					vertexAttributes.position = _vertexPositions[edge];
					vertexAttributes.normal = faceNormal;
					vertexAttributes.uv1 = AdjustSurfaceUV(_faceOuterEdgeUVs[edge], _faceTerrainIndices[face]);
					vertexAttributes.uv2 = new Vector2(gridOverlayU, 0f);
					vertexAttributes.Advance();

					vertexAttributes.position = _innerVertexPositions[edge];
					vertexAttributes.normal = faceNormal;
					vertexAttributes.uv1 = AdjustSurfaceUV(_faceInnerEdgeUVs[edge], _faceTerrainIndices[face]);
					vertexAttributes.uv2 = new Vector2(gridOverlayU, 0.5f);
					vertexAttributes.Advance();
				},
				(Topology.Face face, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
				{
					vertexAttributes.position = _facePositions[face];
					vertexAttributes.normal = _faceNormals[face];
					vertexAttributes.uv1 = AdjustSurfaceUV(_faceCenterUVs[face], _faceTerrainIndices[face]);
					vertexAttributes.uv2 = new Vector2(GetGridOverlayU(false, _faceSeenStates[face], _faceSightCounts[face]), 1f);
					vertexAttributes.Advance();
				});

			_dynamicMesh = DynamicMesh.Create(
				_topology.enumerableInternalFaces,
				DynamicMesh.VertexAttributes.Position |
				DynamicMesh.VertexAttributes.Normal |
				DynamicMesh.VertexAttributes.UV1 |
				DynamicMesh.VertexAttributes.UV2,
				triangulation);

			foreach (var mesh in _dynamicMesh.submeshes)
			{
				var meshObject = Instantiate(planetMeshPrefab);
				meshObject.mesh = mesh;
				meshObject.transform.SetParent(planetMeshes, false);
			}

			_faceUnits = new Transform[_topology.faces.Count].AsFaceAttribute();
			for (int i = 0; i < unitCount; ++i)
			{
				Topology.Face face;
				do
				{
					face = _topology.internalFaces[_random.Index(_topology.internalFaces.Count)];
				} while (_faceTerrainIndices[face] == 1 || _faceUnits[face] != null);

				var unit = Instantiate(unitPrefab);
				unit.SetParent(units, false);
				unit.transform.position = _facePositions[face] + _faceNormals[face] * 0.15f;
				_faceUnits[face] = unit;

				RevealUnitVicinity(face);
			}
			_dynamicMesh.RebuildMesh(DynamicMesh.VertexAttributes.UV2);
		}

		private Vector2 AdjustSurfaceUV(Vector2 uv, int terrainIndex)
		{
			uv *= 0.5f;
			switch (terrainIndex)
			{
				case 0: return new Vector2(uv.x, uv.y);
				case 1: return new Vector2(uv.x + 0.5f, uv.y);
				case 2: return new Vector2(uv.x, uv.y + 0.5f);
				case 3: return new Vector2(uv.x + 0.5f, uv.y + 0.5f);
				default: throw new NotImplementedException();
			}
		}

		private float GetGridOverlayU(bool selected, bool seen, int sightCount)
		{
			if (!seen) return 0.875f;
			if (sightCount == 0) return 0.625f;
			if (!selected) return 0.125f;
			return 0.375f;
		}

		private void RevealUnitVicinity(Topology.Face face)
		{
			_faceSeenStates[face] = true;
			_faceSightCounts[face] += 1;
			_dynamicMesh.RebuildFace(face, _faceTriangulation);

			TopologyVisitor.VisitFacesInBreadthFirstOrder(face, (FaceVisitor visitor) =>
			{
				_faceSeenStates[visitor.face] = true;
				if (visitor.depth <= fullSightDistance)
				{
					_faceSightCounts[visitor.face] += 1;
				}
				_dynamicMesh.RebuildFace(visitor.face, _faceTriangulation);
				if (visitor.depth < limitedSightDistance)
				{
					visitor.VisitAllNeighbors();
				}
			});
		}

		private void ObscureUnitVicinity(Topology.Face face)
		{
			_faceSightCounts[face] -= 1;
			_dynamicMesh.RebuildFace(face, _faceTriangulation);

			TopologyVisitor.VisitFacesInBreadthFirstOrder(face, (FaceVisitor visitor) =>
			{
				_faceSightCounts[visitor.face] -= 1;
				_dynamicMesh.RebuildFace(visitor.face, _faceTriangulation);

				if (visitor.depth < 3)
				{
					visitor.VisitAllNeighbors();
				}
			});
		}

		protected void Update()
		{
			if (Input.GetKeyUp(KeyCode.F1))
			{
				controlsPanel.SetActive(!controlsPanel.activeSelf);
			}

			exteriorLight.transform.position = interiorLight.transform.position = Quaternion.AngleAxis(Mathf.Repeat(Time.time * 10f, 360f), _lightAxis) * _lightVector;
		}

		private void Select(Topology.Face face)
		{
			_dynamicMesh.RebuildFace(face, _selectedFaceTriangulation);
			_dynamicMesh.RebuildMesh(DynamicMesh.VertexAttributes.UV2);
		}

		private void Deselect(Topology.Face face)
		{
			_dynamicMesh.RebuildFace(face, _faceTriangulation);
			_dynamicMesh.RebuildMesh(DynamicMesh.VertexAttributes.UV2);
		}

		public void OnPickStart(Topology.Face pickedFace, int button)
		{
			if (_moving || pickedFace.isExternal) return;

			if (button == 0)
			{
				if (_selectedFace && _selectedFace != pickedFace)
				{
					Deselect(_selectedFace);
					_selectedFace = Topology.Face.none;
				}

				if (_faceUnits[pickedFace] != null)
				{
					Select(pickedFace);
					_selectedFace = pickedFace;
				}
			}
			else if (button == 1)
			{
				if (_selectedFace && _selectedFace != pickedFace && pickedFace.isInternal)
				{
					_path = _pathFinder.FindPath(_selectedFace, pickedFace, CostHeuristic, Cost, _path);

					if (_path.Count > 0)
					{
						StartCoroutine(MoveUnitAlongPath(_path));
					}
				}
			}
		}

		private float CostHeuristic(Topology.Face source, Topology.Face target, int pathLength)
		{
			var distance = Geometry.AngleBetweenVectors(_facePositions[source], _facePositions[target]) * _surface.radius;
			var minimumTileCount = Mathf.Floor(distance / _maximumFaceDistance);
			return minimumTileCount * 1f;
		}

		private float Cost(Topology.FaceEdge edge, int pathLength)
		{
			if (_faceUnits[edge] != null) return float.PositiveInfinity;

			switch (_faceTerrainIndices[edge])
			{
				case 0: return 1f;
				case 1: return float.PositiveInfinity;
				case 2: return 3f;
				case 3: return 9f;
				default: throw new NotImplementedException();
			}
		}

		private void UpdateUnitFace(Transform unit, Topology.Face oldFace, Topology.Face newFace)
		{
			ObscureUnitVicinity(oldFace);
			RevealUnitVicinity(newFace);
			_dynamicMesh.RebuildFace(oldFace, _faceTriangulation);
			_dynamicMesh.RebuildFace(newFace, _selectedFaceTriangulation);
			_faceUnits[oldFace] = null;
			_faceUnits[newFace] = unit;
			_selectedFace = newFace;
			_dynamicMesh.RebuildMesh(DynamicMesh.VertexAttributes.UV2);
		}

		private IEnumerator MoveUnitAlongPath(IFaceEdgePath path)
		{
			_moving = true;

			var edge = path[0];
			var prevFace = edge.nearFace;
			var nextFace = edge.farFace;

			var unit = _faceUnits[prevFace];

			if (_selectedFace != prevFace)
			{
				if (_selectedFace)
				{
					_dynamicMesh.RebuildFace(_selectedFace, _faceTriangulation);
				}
				_selectedFace = prevFace;
				_dynamicMesh.RebuildFace(_selectedFace, _selectedFaceTriangulation);
				_dynamicMesh.RebuildMesh(DynamicMesh.VertexAttributes.UV2);
			}

			var accumulatedTime = 0f;
			var pathIndex = 0;

			var effectiveMovementDuration = movementDuration * Cost(edge, pathIndex);

			while (pathIndex < path.Count)
			{
				yield return null;

				accumulatedTime += Time.deltaTime;

				var stepProgress = accumulatedTime / effectiveMovementDuration;

				bool rebuildMesh = false;

				do
				{
					if (stepProgress >= 0.5f && _selectedFace == prevFace)
					{
						UpdateUnitFace(unit, prevFace, nextFace);
						rebuildMesh = true;
					}

					if (accumulatedTime >= effectiveMovementDuration)
					{
						accumulatedTime -= effectiveMovementDuration;
						++pathIndex;

						if (pathIndex == path.Count)
						{
							var endPosition = _facePositions[_selectedFace];
							unit.position = endPosition + _surface.GetNormal(endPosition) * 0.15f;

							_moving = false;
							yield break;
						}
						else
						{
							edge = path[pathIndex];
							prevFace = edge.nearFace;
							nextFace = edge.farFace;
							effectiveMovementDuration = movementDuration * Cost(edge, pathIndex);
							stepProgress = accumulatedTime / effectiveMovementDuration;
						}
					}

				} while ((stepProgress >= 0.5f && _selectedFace == prevFace) || (accumulatedTime >= effectiveMovementDuration));

				if (rebuildMesh)
				{
					_dynamicMesh.RebuildMesh(DynamicMesh.VertexAttributes.UV2);
				}

				var position = Vector3.Slerp(_facePositions[prevFace], _facePositions[nextFace], (-2f * stepProgress + 3f) * stepProgress * stepProgress);
				unit.position = position + _surface.GetNormal(position) * 0.15f;
			}
		}
	}
}
