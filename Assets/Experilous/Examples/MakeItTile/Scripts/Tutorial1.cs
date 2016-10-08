/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using Experilous.MakeItTile;
using Experilous.Numerics;

namespace Experilous.Examples.MakeItTile
{
	public class Tutorial1 : MonoBehaviour
	{
		public MeshFilter submeshPrefab;

		[Range(1, 99)]
		public int topologyWidth = 55;

		[Range(1, 99)]
		public int topologyHeight = 33;

		[Range(0, 9)]
		public int clearRadius = 2;

		public Color normalColor = new Color(0.5f, 0.6f, 0.7f);
		public Color pressedColor = new Color(0.8f, 0.1f, 0.0f);
		public Color pathColor = new Color(0.6f, 0.2f, 0.1f);
		public Color blockedColor = new Color(0.2f, 0.1f, 0.3f);
		public Color borderColor = new Color(0f, 0f, 0f);

		private Surface _surface;
		private Topology _topology;

		private IVertexAttribute<Vector3> _vertexPositions;
		private IFaceAttribute<Vector3> _facePositions;
		private IEdgeAttribute<Vector3> _faceCornerBisectors;
		private IFaceAttribute<bool> _faceBlockedStates;

		private DynamicMesh _dynamicMesh;
		private DynamicMesh.ITriangulation _triangulationNormal;
		private DynamicMesh.ITriangulation _triangulationPressed;
		private DynamicMesh.ITriangulation _triangulationPath;
		private DynamicMesh.ITriangulation _triangulationBlocked;

		private Topology.Face _startFace;

		private PathFinder _pathFinder;
		private IFaceEdgePath _path;

		void Awake()
		{
			_triangulationNormal = new SeparatedFacesUmbrellaTriangulation(2,
				(Topology.FaceEdge edge, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
				{
					vertexAttributes.Advance();

					vertexAttributes.normal = (vertexAttributes.position + Vector3.up - _facePositions[edge.nearFace]).normalized;
					vertexAttributes.color  = normalColor;
					vertexAttributes.Advance();
				},
				(Topology.Face face, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
				{
					vertexAttributes.color  = normalColor;
					vertexAttributes.Advance();
				});

			_triangulationPressed = new SeparatedFacesUmbrellaTriangulation(2,
				(Topology.FaceEdge edge, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
				{
					vertexAttributes.Advance();

					vertexAttributes.normal = (_facePositions[edge.nearFace] + Vector3.up * 0.5f - vertexAttributes.position).normalized;
					vertexAttributes.color  = pressedColor;
					vertexAttributes.Advance();
				},
				(Topology.Face face, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
				{
					vertexAttributes.color  = pressedColor;
					vertexAttributes.Advance();
				});

			_triangulationPath = new SeparatedFacesUmbrellaTriangulation(2,
				(Topology.FaceEdge edge, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
				{
					vertexAttributes.Advance();

					vertexAttributes.normal = (_facePositions[edge.nearFace] + Vector3.up * 0.5f - vertexAttributes.position).normalized;
					vertexAttributes.color  = pathColor;
					vertexAttributes.Advance();
				},
				(Topology.Face face, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
				{
					vertexAttributes.color  = pathColor;
					vertexAttributes.Advance();
				});

			_triangulationBlocked = new SeparatedFacesUmbrellaTriangulation(2,
				(Topology.FaceEdge edge, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
				{
					vertexAttributes.Advance();

					vertexAttributes.normal = (vertexAttributes.position + Vector3.up * 0.25f - _facePositions[edge.nearFace]).normalized;
					vertexAttributes.color  = blockedColor;
					vertexAttributes.Advance();
				},
				(Topology.Face face, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
				{
					vertexAttributes.color  = blockedColor;
					vertexAttributes.Advance();
				});

			_pathFinder = new PathFinder();
		}
	
		protected void Start()
		{
			var hexGridSurface = RectangularHexGrid.Create(
				HexGridDescriptor.CreateSideUp(true, HexGridAxisStyles.StaggeredSymmetric),
				Vector3.zero, Quaternion.Euler(90f, 0f, 0f),
				false, false,
				new IntVector2(topologyWidth, topologyHeight));

			_surface = hexGridSurface;

			Vector3[] vertexPositionsArray;
			_topology = hexGridSurface.CreateManifold(out vertexPositionsArray);
			_vertexPositions = vertexPositionsArray.AsVertexAttribute();
			_facePositions = FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(_topology.internalFaces, _vertexPositions);
			_faceCornerBisectors = EdgeAttributeUtility.CalculateFaceEdgeBisectorsFromVertexPositions(_topology.faceEdges, _topology.internalFaces, _vertexPositions, _facePositions);
			_faceBlockedStates = new bool[_topology.internalFaces.Count].AsFaceAttribute();

			var triangulation = new SeparatedFacesUmbrellaTriangulation(2,
				(Topology.FaceEdge edge, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
				{
					vertexAttributes.position = _vertexPositions[edge];
					vertexAttributes.normal = Vector3.up;
					vertexAttributes.color = borderColor;
					vertexAttributes.Advance();

					vertexAttributes.position = _vertexPositions[edge] + _faceCornerBisectors[edge] * 0.05f;
					vertexAttributes.normal = (vertexAttributes.position + Vector3.up - _facePositions[edge.nearFace]).normalized;
					vertexAttributes.color = normalColor;
					vertexAttributes.Advance();
				},
				(Topology.Face face, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
				{
					vertexAttributes.position = _facePositions[face];
					vertexAttributes.normal = Vector3.up;
					vertexAttributes.color = normalColor;
					vertexAttributes.Advance();
				});

			_dynamicMesh = DynamicMesh.Create(
				_topology.internalFaces,
				DynamicMesh.VertexAttributes.Position |
				DynamicMesh.VertexAttributes.Normal |
				DynamicMesh.VertexAttributes.Color,
				triangulation);

			foreach (var mesh in _dynamicMesh.submeshes)
			{
				var meshFilter = Instantiate(submeshPrefab);
				meshFilter.mesh = mesh;
				meshFilter.transform.SetParent(transform, false);
			}

			var partioning = UniversalFaceSpatialPartitioning.Create(_topology, _surface, _vertexPositions);
			var picker = GetComponent<FaceSpatialPartitioningPicker>();
			picker.partitioning = partioning;
		}

		public void OnPickStart(Topology.Face face, int button)
		{
			if (button == 0 && face.isInternal)
			{
				ClearPreviousPickState();
				_startFace = face;
				DrawCurrentPickState(face);
				_dynamicMesh.RebuildMesh(DynamicMesh.VertexAttributes.Normal | DynamicMesh.VertexAttributes.Color);
			}
		}

		public void OnPickChange(Topology.Face oldFace, Topology.Face newFace)
		{
			if (_startFace)
			{
				ClearPreviousPickState();

				if (newFace == _startFace || newFace.isExternal || _faceBlockedStates[_startFace] == true || _faceBlockedStates[newFace] == true)
				{
					_path = null;
				}
				else
				{
					_path = _pathFinder.FindPath(_startFace, newFace, CostHeuristic, Cost, _path);
				}

				DrawCurrentPickState(newFace);
				_dynamicMesh.RebuildMesh(DynamicMesh.VertexAttributes.Normal | DynamicMesh.VertexAttributes.Color);
			}
		}

		public void OnPickEnd(Topology.Face face, int button)
		{
			if (button == 0 && _startFace)
			{
				ClearPreviousPickState();

				if (face == _startFace)
				{
					ClearBlockedFaces(_startFace);
				}
				else if (_path != null && _path.Count > 0)
				{
					BlockPathFaces(_path);
				}

				_path = null;
				_startFace = Topology.Face.none;
				_dynamicMesh.RebuildMesh(DynamicMesh.VertexAttributes.Normal | DynamicMesh.VertexAttributes.Color);
			}
		}

		private void ClearPreviousPickState()
		{
			if (_startFace)
			{
				if (_path == null || _path.Count == 0)
				{
					_dynamicMesh.RebuildFace(_startFace, _triangulationNormal);
				}
				else
				{
					foreach (var face in _path.AsFacePath())
					{
						_dynamicMesh.RebuildFace(face, _triangulationNormal);
					}
				}
			}
		}

		private void DrawCurrentPickState(Topology.Face currentFace)
		{
			if (_startFace)
			{
				if (_startFace == currentFace)
				{
					_dynamicMesh.RebuildFace(_startFace, _triangulationPressed);
				}
				else if (_path != null && _path.Count > 0)
				{
					foreach (var face in _path.AsFacePath())
					{
						_dynamicMesh.RebuildFace(face, _triangulationPath);
					}
				}
			}
		}

		private float CostHeuristic(Topology.Face source, Topology.Face target, int pathLength)
		{
			if (source.isExternal || target.isExternal) return float.PositiveInfinity;
			return Vector3.Distance(_facePositions[source], _facePositions[target]);
		}

		private float Cost(Topology.FaceEdge edge, int pathLength)
		{
			if (edge.farFace.isInternal && _faceBlockedStates[edge.farFace] == false)
			{
				return 1f;
			}
			else
			{
				return float.PositiveInfinity;
			}
		}

		private void BlockPathFaces(IFaceEdgePath path)
		{
			foreach (var face in path.AsFacePath())
			{
				_faceBlockedStates[face] = true;
				_dynamicMesh.RebuildFace(face, _triangulationBlocked);
			}
		}

		private void ClearBlockedFaces(Topology.Face face)
		{
			_faceBlockedStates[face] = false;
			_dynamicMesh.RebuildFace(face, _triangulationNormal);

			TopologyVisitor.VisitFacesInBreadthFirstOrder(face,
				(FaceVisitor visitor) =>
				{
					if (_faceBlockedStates[visitor.face] == true)
					{
						_faceBlockedStates[visitor.face] = false;
						_dynamicMesh.RebuildFace(visitor.face, _triangulationNormal);
					}

					if (visitor.depth < clearRadius)
					{
						foreach (var edge in visitor.face.edges)
						{
							if (edge.farFace.isInternal)
							{
								visitor.VisitNeighbor(edge.farFace);
							}
						}
					}
				});
		}
	}
}
