/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using System;
using Experilous.MakeItRandom;
using Experilous.MakeItTile;
using Experilous.Numerics;

namespace Experilous.Examples.MakeItTile
{
	[RequireComponent(typeof(FaceSpatialPartitioningPicker))]
	public class JumbleGoGameBoard : MonoBehaviour
	{
		public string randomSeedText = "";

		public Transform gameBoardMeshes;
		public Transform gameBoardPieces;

		public MeshFilter meshFilterRendererPrefab;
		public Transform blackPiecePrefab;
		public Transform whitePiecePrefab;

		public new Camera camera;

		public Toggle squaresToggle;
		public Toggle hexesToggle;
		public Toggle jumbledToggle;

		public Toggle smallToggle;
		public Toggle mediumToggle;
		public Toggle largeToggle;

		public Text whiteCountText;
		public Text blackCountText;

		private IRandom _random;

		private int _blackCount;
		private int _whiteCount;
		private int _moveCount;

		private enum BoardState
		{
			Empty,
			Black,
			White,
		}

		private QuadrilateralSurface _surface;
		private Topology _topology;
		private PositionalVertexAttribute _vertexPositions;
		private PositionalFaceAttribute _facePositions;
		private IEdgeAttribute<Vector3> _innerAngleBisectors;
		private IFaceAttribute<BoardState> _faceBoardStates;
		private IFaceAttribute<Transform> _facePieces;

		private DynamicMesh _dynamicMesh;
		private Bounds _gameBoardBounds;

		private UniversalFaceSpatialPartitioning _partitioning;
		private FaceSpatialPartitioningPicker _picker;

		private Topology.Face _activeFace;

		private DynamicMesh.ITriangulation _normalFaceTriangulation;
		private DynamicMesh.ITriangulation _selectedFaceTriangulation;

		private bool _gameActive = false;
		private BoardState _turn;

		protected void Start()
		{
			_picker = GetComponent<FaceSpatialPartitioningPicker>();
		}

		protected void OnEnable()
		{
			if (_random == null)
				_random = XorShift128Plus.Create(randomSeedText);

			if (_normalFaceTriangulation == null)
				_normalFaceTriangulation = new SeparatedFacesUmbrellaTriangulation(
					(Topology.FaceEdge edge, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
					{
						vertexAttributes.normal = (_facePositions[edge.nearFace] + _surface.normal * 5f - vertexAttributes.position).normalized;
						vertexAttributes.uv = new Vector2(0.25f, 0f);
						vertexAttributes.Advance();
						vertexAttributes.normal = (_facePositions[edge.nearFace] + _surface.normal * 5f - vertexAttributes.position).normalized;
						vertexAttributes.uv = new Vector2(0.25f, 0.5f);
						vertexAttributes.Advance();
					},
					(Topology.Face face, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
					{
						vertexAttributes.uv = new Vector2(0.25f, 1f);
						vertexAttributes.Advance();
					});

			if (_selectedFaceTriangulation == null)
				_selectedFaceTriangulation = new SeparatedFacesUmbrellaTriangulation(
					(Topology.FaceEdge edge, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
					{
						vertexAttributes.normal = (_facePositions[edge.nearFace] + _surface.normal * 1f - vertexAttributes.position).normalized;
						vertexAttributes.uv = new Vector2(0.75f, 0f);
						vertexAttributes.Advance();
						vertexAttributes.normal = (_facePositions[edge.nearFace] + _surface.normal * 1f - vertexAttributes.position).normalized;
						vertexAttributes.uv = new Vector2(0.75f, 0.5f);
						vertexAttributes.Advance();
					},
					(Topology.Face face, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
					{
						vertexAttributes.uv = new Vector2(0.75f, 1f);
						vertexAttributes.Advance();
					});
		}

		protected void Update()
		{
			AdjustCamera();
		}

		private void DestroyOldGame()
		{
			if (_surface == null) return;

			var existingMeshCount = gameBoardMeshes.childCount;
			for (int i = 0; i < existingMeshCount; ++i)
			{
				Destroy(gameBoardMeshes.GetChild(i).gameObject);
			}

			foreach (var face in _topology.internalFaces)
			{
				if (_facePieces[face] != null)
				{
					Destroy(_facePieces[face].gameObject);
				}
			}

			_surface = null;
			_topology = null;
			_vertexPositions = null;
			_facePositions = null;
			_innerAngleBisectors = null;
			_facePieces = null;
			_faceBoardStates = null;
			_partitioning = null;
			_dynamicMesh = null;

			_picker.partitioning = null;
			_picker.enabled = false;

			_gameActive = false;
		}

		public void StartNewGame()
		{
			DestroyOldGame();

			Vector3[] vertexPositionsArray;

			if (squaresToggle.isOn)
			{
				IntVector2 boardSize;
				if (smallToggle.isOn)
				{
					boardSize = new IntVector2(9, 9);
				}
				else if (mediumToggle.isOn)
				{
					boardSize = new IntVector2(13, 13);
				}
				else
				{
					boardSize = new IntVector2(19, 19);
				}
				_surface = RectangularQuadGrid.Create(Vector2.right, Vector2.up, Vector3.zero, Quaternion.identity, false, false, boardSize);
				_topology = ((RectangularQuadGrid)_surface).CreateManifold(out vertexPositionsArray);
				_vertexPositions = PositionalVertexAttribute.Create(_surface, vertexPositionsArray);
			}
			else if (hexesToggle.isOn)
			{
				IntVector2 boardSize;
				if (smallToggle.isOn)
				{
					boardSize = new IntVector2(9, 9);
				}
				else if (mediumToggle.isOn)
				{
					boardSize = new IntVector2(13, 13);
				}
				else
				{
					boardSize = new IntVector2(19, 19);
				}
				_surface = RectangularHexGrid.Create(
					HexGridDescriptor.CreateCornerUp(true, HexGridAxisStyles.StaggeredSymmetric),
					Vector3.zero, Quaternion.identity,
					false, false,
					boardSize);
				_topology = ((RectangularHexGrid)_surface).CreateManifold(out vertexPositionsArray);
				_vertexPositions = PositionalVertexAttribute.Create(_surface, vertexPositionsArray);
			}
			else
			{
				IntVector2 boardSize;
				if (smallToggle.isOn)
				{
					boardSize = new IntVector2(9, 9);
				}
				else if (mediumToggle.isOn)
				{
					boardSize = new IntVector2(13, 13);
				}
				else
				{
					boardSize = new IntVector2(19, 19);
				}
				_surface = RectangularHexGrid.Create(
					HexGridDescriptor.CreateCornerUp(true, HexGridAxisStyles.StaggeredSymmetric),
					Vector3.zero, Quaternion.identity,
					false, false,
					boardSize);
				_topology = ((RectangularHexGrid)_surface).CreateManifold(out vertexPositionsArray);
				_vertexPositions = PositionalVertexAttribute.Create(_surface, vertexPositionsArray);

				var regularityWeight = 0.5f;
				var equalAreaWeight = 1f - regularityWeight;

				var regularityRelaxedVertexPositions = new Vector3[_topology.vertices.Count].AsVertexAttribute();
				var equalAreaRelaxedVertexPositions = new Vector3[_topology.vertices.Count].AsVertexAttribute();
				var relaxedVertexPositions = regularityRelaxedVertexPositions;
				var faceCentroids = PositionalFaceAttribute.Create(_surface, _topology.internalFaces.Count);
				var vertexAreas = new float[_topology.vertices.Count].AsVertexAttribute();

				FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(_topology.internalFaces, _vertexPositions, faceCentroids);
				VertexAttributeUtility.CalculateVertexAreasFromVertexPositionsAndFaceCentroids(_topology.vertices, _vertexPositions, faceCentroids, vertexAreas);

				var totalArea = 0f;
				foreach (var vertexArea in vertexAreas)
				{
					totalArea += vertexArea;
				}

				Func<float> relaxIterationFunction = () =>
				{
					PlanarManifoldUtility.RelaxVertexPositionsForRegularity(_topology, _vertexPositions, true, regularityRelaxedVertexPositions);
					PlanarManifoldUtility.RelaxVertexPositionsForEqualArea(_topology, _vertexPositions, totalArea, true, equalAreaRelaxedVertexPositions, faceCentroids, vertexAreas);
					for (int i = 0; i < relaxedVertexPositions.Count; ++i)
					{
						relaxedVertexPositions[i] = regularityRelaxedVertexPositions[i] * regularityWeight + equalAreaRelaxedVertexPositions[i] * equalAreaWeight;
					}
					var relaxationAmount = PlanarManifoldUtility.CalculateRelaxationAmount(_vertexPositions, relaxedVertexPositions);
					for (int i = 0; i < _vertexPositions.Count; ++i)
					{
						_vertexPositions[i] = relaxedVertexPositions[i];
					}
					return relaxationAmount;
				};

				Func<bool> repairFunction = () =>
				{
					return PlanarManifoldUtility.ValidateAndRepair(_topology, _surface.normal, _vertexPositions, 0.5f, true);
				};

				Action relaxationLoopFunction = TopologyRandomizer.CreateRelaxationLoopFunction(20, 20, 0.95f, relaxIterationFunction, repairFunction);

				TopologyRandomizer.Randomize(
					_topology, 1, 0.1f,
					3, 3, 5, 7, true,
					_random,
					relaxationLoopFunction);
			}

			_facePositions = PositionalFaceAttribute.Create(_surface, _topology.internalFaces.Count);
			FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(_topology.internalFaces, _vertexPositions, _facePositions);

			_innerAngleBisectors = EdgeAttributeUtility.CalculateFaceEdgeBisectorsFromVertexPositions(_topology.internalFaces, PlanarSurface.Create(Vector3.zero, Quaternion.identity), _vertexPositions);

			_faceBoardStates = new BoardState[_topology.internalFaces.Count].AsFaceAttribute();

			foreach (var face in _topology.internalFaces)
			{
				_faceBoardStates[face] = BoardState.Empty;
			}

			_facePieces = new Transform[_topology.internalFaces.Count].AsFaceAttribute();

			_partitioning = UniversalFaceSpatialPartitioning.Create(_surface, _topology, _vertexPositions);
			_picker.partitioning = _partitioning;
			_picker.enabled = true;

			var centerVertexNormal = _surface.normal.normalized;

			var triangulation = new SeparatedFacesUmbrellaTriangulation(2,
				(Topology.FaceEdge edge, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
				{
					vertexAttributes.position = _vertexPositions[edge];
					vertexAttributes.normal = (_vertexPositions[edge] + _surface.normal * 5f - _facePositions[edge.nearFace]).normalized;
					vertexAttributes.uv = new Vector2(0.25f, 0f);
					vertexAttributes.Advance();

					vertexAttributes.position = _vertexPositions[edge] + _innerAngleBisectors[edge] * 0.05f;
					vertexAttributes.normal = (vertexAttributes.position + _surface.normal * 5f - _facePositions[edge.nearFace]).normalized;
					vertexAttributes.uv = new Vector2(0.25f, 0.5f);
					vertexAttributes.Advance();
				},
				(Topology.Face face, DynamicMesh.IIndexedVertexAttributes vertexAttributes) =>
				{
					vertexAttributes.position = _facePositions[face];
					vertexAttributes.normal = centerVertexNormal;
					vertexAttributes.uv = new Vector2(0.25f, 1f);
					vertexAttributes.Advance();
				});

			_dynamicMesh = DynamicMesh.Create(
				_topology.enumerableInternalFaces,
				DynamicMesh.VertexAttributes.Position |
				DynamicMesh.VertexAttributes.Normal |
				DynamicMesh.VertexAttributes.UV,
				triangulation);

			foreach (var mesh in _dynamicMesh.submeshes)
			{
				var meshObject = Instantiate(meshFilterRendererPrefab);
				meshObject.mesh = mesh;
				meshObject.transform.SetParent(gameBoardMeshes);
			}

			_gameBoardBounds = new Bounds(Vector3.zero, Vector3.zero);
			foreach (var vertex in _topology.vertices)
			{
				_gameBoardBounds.Encapsulate(_vertexPositions[vertex]);
			}

			AdjustCamera();

			var pickerCollider = GetComponent<BoxCollider>();
			pickerCollider.center = _gameBoardBounds.center;
			pickerCollider.size = _gameBoardBounds.size;

			_whiteCount = 0;
			_blackCount = 0;
			_moveCount = 0;

			whiteCountText.text = _whiteCount.ToString();
			blackCountText.text = _blackCount.ToString();

			_gameActive = true;
			_turn = BoardState.Black;
		}

		private void AdjustCamera()
		{
			if (_gameBoardBounds.size == Vector3.zero) return;

			camera.transform.position = new Vector3(_gameBoardBounds.center.x, _gameBoardBounds.center.y, -10f);

			var gameBoardRatio = _gameBoardBounds.size.x / _gameBoardBounds.size.y;

			var uiHeight = Screen.width * 64f / 800f;

			if (gameBoardRatio < Screen.width / (Screen.height - uiHeight))
			{
				camera.orthographicSize = _gameBoardBounds.size.y * (1f + uiHeight / (Screen.height - uiHeight)) * 0.5f;
			}
			else
			{
				camera.orthographicSize = (_gameBoardBounds.size.x * Screen.height / Screen.width) * 0.5f;
			}
		}

		private void Select(Topology.Face face)
		{
			_dynamicMesh.RebuildFace(face, _selectedFaceTriangulation);
			_dynamicMesh.RebuildMesh(DynamicMesh.VertexAttributes.Normal | DynamicMesh.VertexAttributes.UV);
		}

		private void Deselect(Topology.Face face)
		{
			_dynamicMesh.RebuildFace(face, _normalFaceTriangulation);
			_dynamicMesh.RebuildMesh(DynamicMesh.VertexAttributes.Normal | DynamicMesh.VertexAttributes.UV);
		}

		public void OnPickStart(Topology.Face startFace, int button)
		{
			if (!_gameActive || startFace.isExternal) return;

			if (button == 0)
			{
				if (_activeFace && _activeFace != startFace) Deselect(_activeFace);
				Select(startFace);
				_activeFace = startFace;
			}
		}

		public void OnPickChange(Topology.Face previousFace, Topology.Face currentFace)
		{
			if (_activeFace)
			{
				if (currentFace == _activeFace)
				{
					Select(_activeFace);
				}
				else
				{
					Deselect(_activeFace);
				}
			}
		}

		public void OnPickEnd(Topology.Face endFace, int button)
		{
			if (button == 0)
			{
				if (endFace == _activeFace && IsLegalMove(_activeFace))
				{
					MakeMove(_activeFace);
				}

				if (_activeFace)
				{
					Deselect(_activeFace);
					_activeFace = Topology.Face.none;
				}
			}
		}

		private BoardState OppositeColor(BoardState color)
		{
			switch (color)
			{
				case BoardState.Black: return BoardState.White;
				case BoardState.White: return BoardState.Black;
				case BoardState.Empty: return BoardState.Empty;
				default: throw new NotImplementedException();
			}
		}

		private bool HasLiberties(Topology.Face face)
		{
			var color = _faceBoardStates[face];
			if (color == BoardState.Empty) return true;

			bool hasLiberties = false;

			TopologyVisitor.VisitFaces(face, (FaceVisitor visitor) =>
			{
				var visitColor = _faceBoardStates[visitor.face];
				if (visitColor == BoardState.Empty)
				{
					hasLiberties = true;
					visitor.Break();
				}
				else if (visitColor == color)
				{
					visitor.VisitInternalNeighbors();
				}
			});

			return hasLiberties;
		}

		private void Capture(Topology.Face face)
		{
			var color = _faceBoardStates[face];
			if (color == BoardState.Empty) return;

			int captureCount = 0;

			TopologyVisitor.VisitFaces(face, (FaceVisitor visitor) =>
			{
				var visitColor = _faceBoardStates[visitor.face];
				if (visitColor == color)
				{
					SetPiece(visitor.face, null);
					_faceBoardStates[visitor.face] = BoardState.Empty;
					++captureCount;

					visitor.VisitInternalNeighbors();
				}
			});

			if (color == BoardState.Black)
			{
				_blackCount -= captureCount;
				blackCountText.text = _blackCount.ToString();
			}
			else
			{
				_whiteCount -= captureCount;
				whiteCountText.text = _whiteCount.ToString();
			}
		}

		private bool IsLegalMove(Topology.Face face)
		{
			if (_faceBoardStates[face] != BoardState.Empty) return false;

			_faceBoardStates[face] = _turn;

			if (HasLiberties(face))
			{
				_faceBoardStates[face] = BoardState.Empty;
				return true;
			}

			var opponentColor = OppositeColor(_turn);

			foreach (var edge in face.edges)
			{
				if (edge.isNonBoundary && _faceBoardStates[edge] == opponentColor && !HasLiberties(edge.face))
				{
					_faceBoardStates[face] = BoardState.Empty;
					return true;
				}
			}

			_faceBoardStates[face] = BoardState.Empty;
			return false;
		}

		private void MakeMove(Topology.Face face)
		{
			var opponentColor = OppositeColor(_turn);

			_faceBoardStates[face] = _turn;

			++_moveCount;

			if (_turn == BoardState.Black)
			{
				++_blackCount;
				blackCountText.text = _blackCount.ToString();
				SetPiece(face, blackPiecePrefab, string.Format("Black {0}", _moveCount));
				_turn = BoardState.White;
			}
			else
			{
				++_whiteCount;
				whiteCountText.text = _whiteCount.ToString();
				SetPiece(face, whitePiecePrefab, string.Format("White {0}", _moveCount));
				_turn = BoardState.Black;
			}

			foreach (var edge in face.edges)
			{
				if (edge.isNonBoundary && _faceBoardStates[edge] == opponentColor && !HasLiberties(edge.face))
				{
					Capture(edge.face);
				}
			}
		}

		private void SetPiece(Topology.Face face, Transform piecePrefab, string name = null)
		{
			if (_facePieces[face] != null)
			{
				Destroy(_facePieces[face].gameObject);
			}

			if (piecePrefab != null)
			{
				var piece = Instantiate(piecePrefab);
				piece.parent = gameBoardPieces;
				piece.position = _facePositions[face] - new Vector3(0f, 0f, 0.3f);
				if (!string.IsNullOrEmpty(name)) piece.name = name;
				_facePieces[face] = piece;
			}
			else
			{
				_facePieces[face] = null;
			}
		}
	}
}
