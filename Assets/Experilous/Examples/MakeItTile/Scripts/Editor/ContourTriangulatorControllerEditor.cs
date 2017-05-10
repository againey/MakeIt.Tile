using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Experilous.Topologies;
using Experilous.Numerics;

namespace Experilous.Examples.MakeItTile
{
	[CustomEditor(typeof(ContourTriangulatorController))]
	public class ContourTriangulatorControllerEditor : Editor
	{
		[SerializeField] bool _editSiteGraph = false;

		private static int _dynamicGraphHandleHash = "Experilous.Topologies.DynamicGraph.Handle".GetHashCode();

		private const float _nodeShortActiveDistance = 1f / 2f;
		private const float _nodeLongActiveDistance = 5f;
		private const float _edgeShortActiveDistance = 1f / 2f;
		private const float _edgeLongActiveDistance = 5f;

		private const float _nodeDefaultHandleSize = 1f / 25f;
		private const float _nodeHoverHandleSize = 1f / 20f;
		private const float _nodeActiveHandleSize = 1f / 20f;
		private static Color _nodeStaticHandleColor = new Color(0.5f, 0.75f, 1f, 1f);
		private static Color _nodeDefaultHandleColor = new Color(0f, 1f, 0f, 1f);
		private static Color _nodeWillAlterHandleColor = new Color(0.5f, 1f, 0.5f, 1f);
		private static Color _nodeWillRemoveHandleColor = new Color(1f, 0.5f, 0.5f, 1f);
		private static Color _nodeWillAddHandleColor = new Color(0.5f, 1f, 0.5f, 1f);
		private static Color _nodeMovingHandleColor = new Color(1f, 1f, 1f, 1f);
		private static Color _nodeConnectingHandleColor = new Color(0.5f, 1f, 0.5f, 1f);
		private static Color _nodeRemovingHandleColor = new Color(1f, 0f, 0f, 1f);
		private static Color _nodeAddingHandleColor = new Color(1f, 1f, 1f, 1f);
		private static Color _nodeInvalidHandleColor = new Color(1f, 0f, 0f, 0.5f);

		private const float _edgeDefaultHandleWidth = 2f;
		private const float _edgeHoverHandleWidth = 4f;
		private const float _edgeActiveHandleWidth = 4f;
		private static Color _edgeStaticHandleColor = new Color(0.25f, 0.5f, 0.75f, 1f);
		private static Color _edgeDefaultHandleColor = new Color(0f, 1f, 0f, 1f);
		private static Color _edgeWillSplitHandleColor = new Color(0.5f, 1f, 0.5f, 1f);
		private static Color _edgeWillRemoveHandleColor = new Color(1f, 0.5f, 0.5f, 1f);
		private static Color _edgeSplittingHandleColor = new Color(0.5f, 1f, 0.5f, 1f);
		private static Color _edgeRemovingHandleColor = new Color(1f, 0f, 0f, 1f);
		private static Color _edgeAddingHandleColor = new Color(1f, 1f, 1f, 1f);
		private static Color _edgeInvalidHandleColor = new Color(1f, 0f, 0f, 0.5f);

		private Vector2 _mouseDownPosition;

		private GraphNode _mouseDownNode;
		private Vector2 _mouseDownNodePosition;
		private float _mouseDownNodeDistance;
		private GraphEdge _mouseDownEdge;
		private Vector2 _mouseDownEdgePosition;
		private float _mouseDownEdgeDistance;

		private GraphNode _nearestNode;
		private Vector2 _nearestNodePosition;
		private float _nearestNodeDistance;
		private GraphEdge _nearestEdge;
		private Vector2 _nearestEdgePosition;
		private float _nearestEdgeDistance;

		private enum MouseButton
		{
			None = -1,
			Left = 0,
			Right = 1,
		}

		private enum HandleState
		{
			None,
			Canceled, // left and right mouse buttons down
			WillAlterNode, // near node with no modifier key
			WillRemoveNode, // nearest to node with shift key
			WillRemoveEdge, // nearest to edge with shift key
			WillAddNode, // nearest to node with control key
			WillAddEdge, // right mouse button down near node with no modifier key, no target node
			WillSplitEdge, // nearest to edge with control key
			MovingNode, // left mouse buton down near node with no modifier key
			AddingEdge, // right mouse button down near node with no modifier key
			SplittingEdge, // left/right mouse button down nearest to edge with control key
			AddingNode, // left mouse button down not near edge with control key, right mouse button down not near edge with control key but no nearest node
			AddingNodeEdge, // right mouse button down not near edge with control key
			RemovingNode, // left/right mouse button down nearest to node with shift key
			RemovingEdge, // left/right mouse button down nearest to edge with shift key
			CannotMoveNode, // left mouse buton down near node with no modifier key, invalid edge arrangement
			CannotAddEdge, // right mouse button down near node with no modifier key, invalid edge arrangement
			CannotAddNodeEdge, // right mouse button down nearest to node with control key, invalid edge arrangement
			CannotSplitEdge, // left/right mouse button down nearest to edge with control key, invalid edge arrangement
		}

		private MouseButton _mouseButtonState = MouseButton.None;
		private HandleState _handleState = HandleState.None;

		private PlanarVoronoiGenerator _voronoiGenerator;
		private VoronoiDiagram _voronoiDiagram;
		private TopologyNodeDataArray<Vector3> _finiteVoronoiNodePositions;
		private Sphere _voronoiBounds;
		private ContourTriangulator _contourTriangulator;

		protected void OnEnable()
		{
			ContourTriangulatorController controller = target as ContourTriangulatorController;
			if (controller == null) return;

			if (controller.siteGraph == null)
			{
				controller.siteGraph = new DynamicGraph();
			}

			if (controller.pointSitePositions == null)
			{
				controller.pointSitePositions = new ContourTriangulatorController.GraphNodePositionList();
			}

			while (controller.pointSitePositions.Count < controller.siteGraph.nodeCount)
			{
				controller.pointSitePositions.Add(Vector3.zero);
			}

			while (controller.pointSitePositions.Count > controller.siteGraph.nodeCount)
			{
				controller.pointSitePositions.RemoveAt(controller.pointSitePositions.Count - 1);
			}

			_mouseButtonState = MouseButton.None;
			_handleState = HandleState.None;

			if (_voronoiGenerator == null)
			{
				_voronoiGenerator = new PlanarVoronoiGenerator(0.0001f);
			}

			if (_contourTriangulator == null)
			{
				_contourTriangulator = new ContourTriangulator();
			}

			if (_voronoiDiagram == null)
			{
				RebuildContourMesh();
			}

			Tools.hidden = _editSiteGraph;
		}

		protected void OnDisable()
		{
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.FlexibleSpace();
			using (var changeCheck = new EditorGUI.ChangeCheckScope())
			{
				_editSiteGraph = GUILayout.Toggle(_editSiteGraph, new GUIContent("Edit Site Graph"), GUI.skin.button, GUILayout.Width(160f));
				if (changeCheck.changed)
				{
					if (_editSiteGraph)
					{
						_mouseButtonState = MouseButton.None;
						_handleState = HandleState.None;
					}

					Tools.hidden = _editSiteGraph;
				}
			}
			if (GUILayout.Button(new GUIContent("Reset Site Graph"), GUILayout.Width(160f)))
			{
				var controller = (ContourTriangulatorController)target;

				Undo.RecordObject(controller, "Reset Site Graph");
				controller.siteGraph.Clear();
				controller.pointSitePositions.Clear();
				EditorUtility.SetDirty(controller);
				GUI.changed = true;
				Repaint();
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(12f);

			GUI.enabled = !_editSiteGraph;

			base.OnInspectorGUI();

			if (EditorGUI.EndChangeCheck())
			{
				EditorApplication.delayCall += RebuildContourMesh;
				SceneView.RepaintAll();
			}
		}

		protected void OnSceneGUI()
		{
			var controller = (ContourTriangulatorController)target;

			Handles.matrix = controller.transform.localToWorldMatrix;

			if (_editSiteGraph)
			{
				EditorGUI.BeginChangeCheck();

				var ev = Event.current;

				var diagramPlane = new Plane(-controller.transform.forward, controller.transform.position);
				var mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
				var mouseWorldPosition = Numerics.Geometry.Intersect(diagramPlane, mouseRay);
				Vector2 mousePosition = controller.transform.InverseTransformPoint(mouseWorldPosition);

				int controlId = GUIUtility.GetControlID(_dynamicGraphHandleHash, FocusType.Passive);

				switch (ev.GetTypeForControl(controlId))
				{
					case EventType.Layout:
						if (EditorWindow.mouseOverWindow == SceneView.currentDrawingSceneView)
						{
							FindNearestNode(controller, mousePosition);
							FindNearestEdge(controller, mousePosition);

							if (_handleState != HandleState.Canceled)
							{
								_handleState = DetermineHandleState(ev, controller, mousePosition);
							}

							switch (_handleState)
							{
								case HandleState.WillAlterNode:
								case HandleState.WillRemoveNode:
								case HandleState.RemovingNode:
									HandleUtility.AddControl(controlId, _nearestNodeDistance);
									break;
								case HandleState.WillSplitEdge:
								case HandleState.WillRemoveEdge:
								case HandleState.RemovingEdge:
									HandleUtility.AddControl(controlId, _nearestEdgeDistance);
									break;
								case HandleState.MovingNode:
								case HandleState.CannotMoveNode:
									HandleUtility.AddControl(controlId, Vector2.Distance(_mouseDownNodePosition, _mouseDownPosition));
									break;
								case HandleState.WillAddNode:
								case HandleState.AddingNode:
								case HandleState.AddingNodeEdge:
								case HandleState.AddingEdge:
								case HandleState.SplittingEdge:
								case HandleState.CannotAddEdge:
								case HandleState.CannotAddNodeEdge:
								case HandleState.CannotSplitEdge:
									HandleUtility.AddControl(controlId, 0f);
									break;
							}
						}

						break;
					case EventType.MouseDown:
						switch (ev.button)
						{
							case 0:
								if (_mouseButtonState == MouseButton.Right)
								{
									_handleState = HandleState.Canceled;
									GUIUtility.hotControl = 0;
									EditorGUIUtility.SetWantsMouseJumping(0);
								}
								_mouseButtonState = MouseButton.Left;
								break;
							case 1:
								if (_mouseButtonState == MouseButton.Left)
								{
									_handleState = HandleState.Canceled;
									GUIUtility.hotControl = 0;
									EditorGUIUtility.SetWantsMouseJumping(0);
								}
								_mouseButtonState = MouseButton.Right;
								break;
							default:
								if (_mouseButtonState != MouseButton.None)
								{
									_handleState = HandleState.Canceled;
									GUIUtility.hotControl = 0;
									EditorGUIUtility.SetWantsMouseJumping(0);
								}
								_mouseButtonState = MouseButton.None;
								break;
						}

						if (HandleUtility.nearestControl == controlId && _mouseButtonState != MouseButton.None && _handleState != HandleState.Canceled)
						{
							ev.Use();

							GUIUtility.hotControl = controlId;
							EditorGUIUtility.SetWantsMouseJumping(1);

							_mouseDownPosition = mousePosition;

							if (_nearestNode)
							{
								_mouseDownNode = _nearestNode;
								_mouseDownNodePosition = _nearestNodePosition;
								_mouseDownNodeDistance = _nearestNodeDistance;
							}

							if (_nearestEdge)
							{
								_mouseDownEdge = _nearestEdge;
								_mouseDownEdgePosition = _nearestEdgePosition;
								_mouseDownEdgeDistance = _nearestEdgeDistance;
							}

							_handleState = DetermineHandleState(ev, controller, mousePosition);
						}
						break;
					case EventType.MouseDrag:
						if (GUIUtility.hotControl == controlId && _mouseButtonState != MouseButton.None && _handleState != HandleState.Canceled)
						{
							_handleState = DetermineHandleState(ev, controller, mousePosition);

							if (_handleState != HandleState.None && _handleState != HandleState.Canceled)
							{
								ev.Use();
							}
						}
						break;
					case EventType.MouseUp:
						if (GUIUtility.hotControl == controlId && _mouseButtonState != MouseButton.None && _handleState != HandleState.Canceled && ev.button == (int)_mouseButtonState)
						{
							switch (_handleState)
							{
								case HandleState.MovingNode:
									Undo.RecordObject(controller, "Move Point Site");
									controller.pointSitePositions[_mouseDownNode.index] = controller.transform.InverseTransformPoint(_mouseDownNodePosition + (mousePosition - _mouseDownPosition));
									EditorUtility.SetDirty(controller);
									GUI.changed = true;
									EditorApplication.delayCall += RebuildContourMesh;
									break;
								case HandleState.AddingEdge:
									if (_nearestNode != _mouseDownNode && _nearestNodeDistance <= _nodeLongActiveDistance && !_mouseDownNode.FindEdge(_nearestNode))
									{
										Undo.RecordObject(controller, "Add Line Site");
										controller.siteGraph.Connect(_mouseDownNode, _nearestNode);
										EditorUtility.SetDirty(controller);
										GUI.changed = true;
										EditorApplication.delayCall += RebuildContourMesh;
									}
									break;
								case HandleState.SplittingEdge:
									{
										Undo.RecordObject(controller, "Split Line Site");
										var newNode = controller.siteGraph.AddNode();
										var sourceNode = _mouseDownEdge.sourceNode;
										var targetNode = _mouseDownEdge.targetNode;
										controller.pointSitePositions.Add(mousePosition);
										controller.siteGraph.Remove(_mouseDownEdge);
										controller.siteGraph.Connect(sourceNode, newNode);
										controller.siteGraph.Connect(newNode, targetNode);
										EditorUtility.SetDirty(controller);
										GUI.changed = true;
										EditorApplication.delayCall += RebuildContourMesh;
									}
									break;
								case HandleState.RemovingNode:
									Undo.RecordObject(controller, "Remove Point Site");
									controller.siteGraph.RemoveNodeAndEdges(_mouseDownNode.index,
										(int toNodeIndex, int fromNodeIndex) =>
										{
											controller.pointSitePositions[toNodeIndex] = controller.pointSitePositions[fromNodeIndex];
											controller.pointSitePositions.RemoveAt(fromNodeIndex);

											if (_nearestNode.index == toNodeIndex)
											{
												_nearestNode = GraphNode.none;
											}
											else if (_nearestNode.index == fromNodeIndex)
											{
												_nearestNode.index = toNodeIndex;
											}
										},
										(int toEdgeIndex, int toTwinIndex, int fromEdgeIndex, int fromTwinIndex) =>
										{
											if (_nearestEdge.index == toEdgeIndex || _nearestEdge.index == toTwinIndex)
											{
												_nearestEdge = GraphEdge.none;
											}
											else if (_nearestEdge.index == fromEdgeIndex)
											{
												_nearestEdge.index = toEdgeIndex;
											}
											else if (_nearestEdge.index == fromTwinIndex)
											{
												_nearestEdge.index = toTwinIndex;
											}
										});
									EditorUtility.SetDirty(controller);
									GUI.changed = true;
									EditorApplication.delayCall += RebuildContourMesh;
									break;
								case HandleState.RemovingEdge:
									Undo.RecordObject(controller, "Remove Line Site");
									controller.siteGraph.RemoveEdge(_mouseDownEdge.index,
										(int toEdgeIndex, int toTwinIndex, int fromEdgeIndex, int fromTwinIndex) =>
										{
											if (_nearestEdge.index == toEdgeIndex || _nearestEdge.index == toTwinIndex)
											{
												_nearestEdge = GraphEdge.none;
											}
											else if (_nearestEdge.index == fromEdgeIndex)
											{
												_nearestEdge.index = toEdgeIndex;
											}
											else if (_nearestEdge.index == fromTwinIndex)
											{
												_nearestEdge.index = toTwinIndex;
											}
										});
									EditorUtility.SetDirty(controller);
									GUI.changed = true;
									EditorApplication.delayCall += RebuildContourMesh;
									break;
								case HandleState.AddingNode:
									Undo.RecordObject(controller, "Add Point Site");
									controller.siteGraph.AddNode();
									controller.pointSitePositions.Add(mousePosition);
									EditorUtility.SetDirty(controller);
									GUI.changed = true;
									EditorApplication.delayCall += RebuildContourMesh;
									break;
								case HandleState.AddingNodeEdge:
									Undo.RecordObject(controller, "Add and Connect Point Site");
									controller.siteGraph.Connect(_nearestNode, controller.siteGraph.AddNode());
									controller.pointSitePositions.Add(mousePosition);
									EditorUtility.SetDirty(controller);
									GUI.changed = true;
									EditorApplication.delayCall += RebuildContourMesh;
									break;
								case HandleState.WillAddEdge:
								case HandleState.CannotMoveNode:
								case HandleState.CannotAddEdge:
								case HandleState.CannotAddNodeEdge:
								case HandleState.CannotSplitEdge:
									break;
								default:
									throw new InvalidOperationException();
							}

							GUIUtility.hotControl = 0;
							EditorGUIUtility.SetWantsMouseJumping(0);

							_handleState = DetermineHandleState(ev, controller, mousePosition);

							_mouseDownNode = GraphNode.none;
							_mouseDownEdge = GraphEdge.none;
						}

						if (ev.button == (int)_mouseButtonState)
						{
							_mouseButtonState = MouseButton.None;

							if (_handleState == HandleState.Canceled)
							{
								_handleState = HandleState.None;
							}
						}
						break;
					case EventType.Repaint:
						{
							PaintVoronoiDiagram();

							foreach (var edge in controller.siteGraph.edges)
							{
								if (edge.isFirstTwin)
								{
									PaintEdge(controller, edge, mousePosition);
								}
							}

							PaintPossibleEdge(controller, mousePosition);

							foreach (var node in controller.siteGraph.nodes)
							{
								PaintNode(controller, node, mousePosition);
							}

							PaintPossibleNode(controller, mousePosition);
						}
						break;
				}

				if (EditorGUI.EndChangeCheck())
				{
				}

				SceneView.RepaintAll();
			}
			else
			{
				if (controller.siteGraph.nodeCount == 0) return;

				if (Event.current.type == EventType.Repaint)
				{
					PaintVoronoiDiagram();

					Handles.color = _edgeStaticHandleColor;
					foreach (var edge in controller.siteGraph.edges)
					{
						if (edge.isFirstTwin)
						{
							Vector3 p0 = controller.pointSitePositions[edge.sourceNode.index];
							Vector3 p1 = controller.pointSitePositions[edge.targetNode.index];
							Handles.DrawAAPolyLine(_edgeDefaultHandleWidth, p0, p1);
						}
					}

					Handles.color = _nodeStaticHandleColor;
					Vector3 normal = -Camera.current.transform.forward;
					foreach (var node in controller.siteGraph.nodes)
					{
						using (var changeCheck = new EditorGUI.ChangeCheckScope())
						{
							Vector3 nodePosition = controller.pointSitePositions[node.index];
							float size = HandleUtility.GetHandleSize(nodePosition) * _nodeDefaultHandleSize;
							Handles.DrawSolidDisc(nodePosition, normal, size);
						}
					}
				}
			}
		}

		private void FindNearestNode(ContourTriangulatorController controller, Vector2 mousePosition)
		{
			_nearestNode = GraphNode.none;
			_nearestNodePosition = Vector2.zero;
			_nearestNodeDistance = float.PositiveInfinity;

			float nearestSqrDistance = float.PositiveInfinity;

			foreach (var node in controller.siteGraph.nodes)
			{
				Vector2 nodePosition = controller.pointSitePositions[node.index];

				float sqrDistance = (nodePosition - mousePosition).sqrMagnitude;
				if (sqrDistance < nearestSqrDistance)
				{
					_nearestNode = node;
					nearestSqrDistance = sqrDistance;
					_nearestNodePosition = nodePosition;
					_nearestNodeDistance = Mathf.Sqrt(sqrDistance);
				}
			}
		}

		private void FindNearestEdge(ContourTriangulatorController controller, Vector2 mousePosition)
		{
			_nearestEdge = GraphEdge.none;
			_nearestEdgePosition = Vector2.zero;
			_nearestEdgeDistance = float.PositiveInfinity;

			float nearestSqrDistance = float.PositiveInfinity;

			foreach (var edge in controller.siteGraph.edges)
			{
				if (edge.isFirstTwin)
				{
					Vector2 p0 = controller.pointSitePositions[edge.sourceNode.index];
					Vector2 p1 = controller.pointSitePositions[edge.targetNode.index];
					Vector2 d = p1 - p0;
					float t = HandleUtility.PointOnLineParameter(mousePosition, p0, d);
					if (t > 0f && t < 1f)
					{
						Vector2 q = p0 + d * t;

						float sqrDistance = (q - mousePosition).sqrMagnitude;
						if (sqrDistance < nearestSqrDistance)
						{
							_nearestEdge = edge;
							nearestSqrDistance = sqrDistance;
							_nearestEdgePosition = q;
							_nearestEdgeDistance = Mathf.Sqrt(sqrDistance);
						}
					}
				}
			}
		}

		private HandleState DetermineHandleState(Event ev, ContourTriangulatorController controller, Vector2 mousePosition)
		{
			switch (_mouseButtonState)
			{
				case MouseButton.None:
					if (!ev.shift && !ev.control)
					{
						if (_nearestNode && _nearestNodeDistance <= _nodeLongActiveDistance)
						{
							return HandleState.WillAlterNode;
						}
					}
					else if (ev.shift && !ev.control)
					{
						if (_nearestNode && _nearestNodeDistance <= _nodeLongActiveDistance && (!_nearestEdge || _nearestNodeDistance <= _nodeShortActiveDistance || _nearestNodeDistance < _nearestEdgeDistance))
						{
							return HandleState.WillRemoveNode;
						}

						if (_nearestEdge && _nearestEdgeDistance <= _edgeLongActiveDistance)
						{
							return HandleState.WillRemoveEdge;
						}
					}
					else if (ev.control && !ev.shift)
					{
						if (_nearestEdge && _nearestEdgeDistance <= _edgeShortActiveDistance)
						{
							return HandleState.WillSplitEdge;
						}

						return HandleState.WillAddNode;
					}
					break;
				case MouseButton.Left:
					if (!ev.shift && !ev.control)
					{
						if (_mouseDownNode && _mouseDownNodeDistance <= _nodeLongActiveDistance)
						{
							if (MoveIsValid(controller, _mouseDownNode, mousePosition + _mouseDownNodePosition - _mouseDownPosition))
							{
								return HandleState.MovingNode;
							}
							else
							{
								return HandleState.CannotMoveNode;
							}
						}
					}
					else if (ev.shift && !ev.control)
					{
						if (_mouseDownNode && _mouseDownNodeDistance <= _nodeLongActiveDistance && (!_mouseDownEdge || _mouseDownNodeDistance <= _nodeShortActiveDistance || _mouseDownNodeDistance < _mouseDownEdgeDistance))
						{
							return HandleState.RemovingNode;
						}

						if (_mouseDownEdge && _mouseDownEdgeDistance <= _edgeLongActiveDistance)
						{
							return HandleState.RemovingEdge;
						}
					}
					else if (ev.control && !ev.shift)
					{
						if (_mouseDownEdge && _mouseDownEdgeDistance <= _edgeShortActiveDistance)
						{
							if (SplitIsValid(controller, _mouseDownEdge, mousePosition))
							{
								return HandleState.SplittingEdge;
							}
							else
							{
								return HandleState.CannotSplitEdge;
							}
						}

						return HandleState.AddingNode;
					}
					break;
				case MouseButton.Right:
					if (!ev.shift && !ev.control)
					{
						if (_mouseDownNode && _mouseDownNodeDistance <= _nodeLongActiveDistance)
						{
							if (!_nearestNode || _nearestNode == _mouseDownNode || _nearestNodeDistance > _nodeLongActiveDistance)
							{
								return HandleState.WillAddEdge;
							}
							else if (NewEdgeIsValid(controller, _mouseDownNode, _nearestNode))
							{
								return HandleState.AddingEdge;
							}
							else
							{
								return HandleState.CannotAddEdge;
							}
						}
					}
					else if (ev.shift && !ev.control)
					{
						if (_mouseDownNode && _mouseDownNodeDistance <= _nodeLongActiveDistance && (!_mouseDownEdge || _mouseDownNodeDistance <= _nodeShortActiveDistance || _mouseDownNodeDistance < _mouseDownEdgeDistance))
						{
							return HandleState.RemovingNode;
						}

						if (_mouseDownEdge && _mouseDownEdgeDistance <= _edgeLongActiveDistance)
						{
							return HandleState.RemovingEdge;
						}
					}
					else if (ev.control && !ev.shift)
					{
						if (_mouseDownEdge && _mouseDownEdgeDistance <= _edgeShortActiveDistance)
						{
							if (SplitIsValid(controller, _mouseDownEdge, mousePosition))
							{
								return HandleState.SplittingEdge;
							}
							else
							{
								return HandleState.CannotSplitEdge;
							}
						}

						if (!_nearestNode)
						{
							return HandleState.AddingNode;
						}
						else if (NewEdgeIsValid(controller, _nearestNode, mousePosition))
						{
							return HandleState.AddingNodeEdge;
						}
						else
						{
							return HandleState.CannotAddNodeEdge;
						}
					}
					break;
			}

			return HandleState.None;
		}

		private bool MoveIsValid(ContourTriangulatorController controller, GraphNode node, Vector2 p)
		{
			foreach (var nodeEdge in node.edges)
			{
				var targetNode = nodeEdge.targetNode;

				Vector2 p0 = p;
				Vector2 p1 = controller.pointSitePositions[targetNode.index];
				if ((p1 - p0).sqrMagnitude < 0.1f) return false;

				var nodeRay = new ScaledRay2D(p0, p1 - p0);
				foreach (var otherEdge in controller.siteGraph.edges)
				{
					if (otherEdge.isFirstTwin && otherEdge.sourceNode != node && otherEdge.targetNode != node && otherEdge.sourceNode != targetNode && otherEdge.targetNode != targetNode)
					{
						Vector2 q0 = controller.pointSitePositions[otherEdge.sourceNode.index];
						Vector2 q1 = controller.pointSitePositions[otherEdge.targetNode.index];
						var otherRay = new ScaledRay2D(q0, q1 - q0);
						float t0, t1;
						if (Numerics.Geometry.GetIntersectionParameters(nodeRay, otherRay, out t0, out t1) && t0 >= 0f && t0 <= 1f && t1 >= 0f && t1 <= 1f)
						{
							return false;
						}
					}
				}
			}

			return true;
		}

		private bool NewEdgeIsValid(ContourTriangulatorController controller, GraphNode node, Vector2 p)
		{
			Vector2 p0 = p;
			Vector2 p1 = controller.pointSitePositions[node.index];
			if ((p1 - p0).sqrMagnitude < 0.1f) return false;

			var newRay = new ScaledRay2D(p0, p1 - p0);
			foreach (var otherEdge in controller.siteGraph.edges)
			{
				if (otherEdge.isFirstTwin && otherEdge.sourceNode != node && otherEdge.targetNode != node)
				{
					Vector2 q0 = controller.pointSitePositions[otherEdge.sourceNode.index];
					Vector2 q1 = controller.pointSitePositions[otherEdge.targetNode.index];
					var otherRay = new ScaledRay2D(q0, q1 - q0);
					float t0, t1;
					if (Numerics.Geometry.GetIntersectionParameters(newRay, otherRay, out t0, out t1) && t0 >= 0f && t0 <= 1f && t1 >= 0f && t1 <= 1f)
					{
						return false;
					}
				}
			}

			return true;
		}

		private bool NewEdgeIsValid(ContourTriangulatorController controller, GraphNode sourceNode, GraphNode targetNode)
		{
			Vector2 p0 = controller.pointSitePositions[sourceNode.index];
			Vector2 p1 = controller.pointSitePositions[targetNode.index];
			if ((p1 - p0).sqrMagnitude < 0.1f) return false;

			var newRay = new ScaledRay2D(p0, p1 - p0);
			foreach (var otherEdge in controller.siteGraph.edges)
			{
				if (otherEdge.isFirstTwin && otherEdge.sourceNode != sourceNode && otherEdge.targetNode != sourceNode && otherEdge.sourceNode != targetNode && otherEdge.targetNode != targetNode)
				{
					Vector2 q0 = controller.pointSitePositions[otherEdge.sourceNode.index];
					Vector2 q1 = controller.pointSitePositions[otherEdge.targetNode.index];
					var otherRay = new ScaledRay2D(q0, q1 - q0);
					float t0, t1;
					if (Numerics.Geometry.GetIntersectionParameters(newRay, otherRay, out t0, out t1) && t0 >= 0f && t0 <= 1f && t1 >= 0f && t1 <= 1f)
					{
						return false;
					}
				}
			}

			return true;
		}

		private bool SplitIsValid(ContourTriangulatorController controller, GraphEdge edge, Vector2 p)
		{
			var sourceNode = edge.sourceNode;
			var targetNode = edge.targetNode;
			Vector2 p0 = controller.pointSitePositions[sourceNode.index];
			Vector2 p1 = p;
			Vector2 p2 = controller.pointSitePositions[targetNode.index];
			if ((p1 - p0).sqrMagnitude < 0.1f || (p2 - p1).sqrMagnitude < 0.1f) return false;

			var newRay0 = new ScaledRay2D(p0, p1 - p0);
			var newRay1 = new ScaledRay2D(p1, p2 - p1);
			foreach (var otherEdge in controller.siteGraph.edges)
			{
				if (otherEdge.isFirstTwin)
				{
					Vector2 q0 = controller.pointSitePositions[otherEdge.sourceNode.index];
					Vector2 q1 = controller.pointSitePositions[otherEdge.targetNode.index];
					var otherRay = new ScaledRay2D(q0, q1 - q0);
					float t0, t1;
					if (otherEdge.sourceNode != sourceNode && otherEdge.targetNode != sourceNode && Numerics.Geometry.GetIntersectionParameters(newRay0, otherRay, out t0, out t1) && t0 >= 0f && t0 <= 1f && t1 >= 0f && t1 <= 1f)
					{
						return false;
					}
					if (otherEdge.sourceNode != targetNode && otherEdge.targetNode != targetNode && Numerics.Geometry.GetIntersectionParameters(newRay1, otherRay, out t0, out t1) && t0 >= 0f && t0 <= 1f && t1 >= 0f && t1 <= 1f)
					{
						return false;
					}
				}
			}

			return true;
		}

		private void PaintNode(ContourTriangulatorController controller, GraphNode node, Vector2 mousePosition)
		{
			Vector2 p0 = controller.pointSitePositions[node.index];
			Vector3 normal = -Camera.current.transform.forward;

			float baseSize;

			if (node == _mouseDownNode)
			{
				switch (_handleState)
				{
					case HandleState.MovingNode:
						p0 = mousePosition + _mouseDownNodePosition - _mouseDownPosition;
						baseSize = HandleUtility.GetHandleSize(p0);
						Handles.color = _nodeMovingHandleColor;
						Handles.DrawSolidDisc(p0, normal, baseSize * _nodeActiveHandleSize);
						return;
					case HandleState.AddingEdge:
						p0 = _mouseDownNodePosition;
						baseSize = HandleUtility.GetHandleSize(p0);
						Handles.color = _nodeConnectingHandleColor;
						Handles.DrawSolidDisc(p0, normal, baseSize * _nodeActiveHandleSize);
						return;
					case HandleState.RemovingNode:
						p0 = _mouseDownNodePosition;
						baseSize = HandleUtility.GetHandleSize(p0);
						Handles.color = _nodeRemovingHandleColor;
						Handles.DrawSolidDisc(p0, normal, baseSize * _nodeActiveHandleSize);
						return;
					case HandleState.CannotMoveNode:
						p0 = mousePosition + _mouseDownNodePosition - _mouseDownPosition;
						baseSize = HandleUtility.GetHandleSize(p0);
						Handles.color = _nodeInvalidHandleColor;
						Handles.DrawSolidDisc(p0, normal, baseSize * _nodeDefaultHandleSize);
						return;
				}
			}

			if (node == _nearestNode)
			{
				switch (_handleState)
				{
					case HandleState.WillAlterNode:
						p0 = _nearestNodePosition;
						baseSize = HandleUtility.GetHandleSize(p0);
						Handles.color = _nodeWillAlterHandleColor;
						Handles.DrawSolidDisc(p0, normal, baseSize * _nodeHoverHandleSize);
						return;
					case HandleState.WillRemoveNode:
						p0 = _nearestNodePosition;
						baseSize = HandleUtility.GetHandleSize(p0);
						Handles.color = _nodeWillRemoveHandleColor;
						Handles.DrawSolidDisc(p0, normal, baseSize * _nodeHoverHandleSize);
						return;
					case HandleState.AddingEdge:
						if (node != _mouseDownNode && _nearestNodeDistance <= _nodeLongActiveDistance)
						{
							p0 = _nearestNodePosition;
							baseSize = HandleUtility.GetHandleSize(p0);
							Handles.color = _nodeConnectingHandleColor;
							Handles.DrawSolidDisc(p0, normal, baseSize * _nodeActiveHandleSize);
							return;
						}
						break;
					case HandleState.CannotAddEdge:
						if (node != _mouseDownNode && _nearestNodeDistance <= _nodeLongActiveDistance)
						{
							p0 = _nearestNodePosition;
							baseSize = HandleUtility.GetHandleSize(p0);
							Handles.color = _nodeInvalidHandleColor;
							Handles.DrawSolidDisc(p0, normal, baseSize * _nodeDefaultHandleSize);
							return;
						}
						break;
				}
			}

			baseSize = HandleUtility.GetHandleSize(p0);
			Handles.color = _nodeDefaultHandleColor;
			Handles.DrawSolidDisc(p0, normal, baseSize * _nodeDefaultHandleSize);
		}

		private void PaintPossibleNode(ContourTriangulatorController controller, Vector2 mousePosition)
		{
			float baseSize;
			Vector3 normal = -Camera.current.transform.forward;

			switch (_handleState)
			{
				case HandleState.WillAddNode:
					baseSize = HandleUtility.GetHandleSize(mousePosition);
					Handles.color = _nodeWillAddHandleColor;
					Handles.DrawSolidDisc(mousePosition, normal, baseSize * _nodeHoverHandleSize);
					break;
				case HandleState.WillSplitEdge:
					baseSize = HandleUtility.GetHandleSize(_nearestEdgePosition);
					Handles.color = _nodeWillAddHandleColor;
					Handles.DrawSolidDisc(_nearestEdgePosition, normal, baseSize * _nodeHoverHandleSize);
					break;
				case HandleState.AddingNode:
				case HandleState.AddingNodeEdge:
					baseSize = HandleUtility.GetHandleSize(mousePosition);
					Handles.color = _nodeAddingHandleColor;
					Handles.DrawSolidDisc(mousePosition, normal, baseSize * _nodeActiveHandleSize);
					break;
				case HandleState.SplittingEdge:
					baseSize = HandleUtility.GetHandleSize(mousePosition);
					Handles.color = _nodeAddingHandleColor;
					Handles.DrawSolidDisc(mousePosition, normal, baseSize * _nodeActiveHandleSize);
					break;
				case HandleState.CannotAddNodeEdge:
					baseSize = HandleUtility.GetHandleSize(mousePosition);
					Handles.color = _nodeInvalidHandleColor;
					Handles.DrawSolidDisc(mousePosition, normal, baseSize * _nodeDefaultHandleSize);
					break;
				case HandleState.CannotSplitEdge:
					baseSize = HandleUtility.GetHandleSize(mousePosition);
					Handles.color = _nodeInvalidHandleColor;
					Handles.DrawSolidDisc(mousePosition, normal, baseSize * _nodeDefaultHandleSize);
					break;
			}
		}

		private void PaintEdge(ContourTriangulatorController controller, GraphEdge edge, Vector2 mousePosition)
		{
			Vector2 p0 = controller.pointSitePositions[edge.sourceNode.index];
			Vector2 p1 = controller.pointSitePositions[edge.targetNode.index];

			if (edge == _mouseDownEdge)
			{
				switch (_handleState)
				{
					case HandleState.SplittingEdge:
						Handles.color = _edgeSplittingHandleColor;
						Handles.DrawAAPolyLine(_edgeActiveHandleWidth, p0, mousePosition, p1);
						return;
					case HandleState.RemovingEdge:
						Handles.color = _edgeRemovingHandleColor;
						Handles.DrawAAPolyLine(_edgeActiveHandleWidth, p0, p1);
						return;
					case HandleState.CannotSplitEdge:
						Handles.color = _edgeInvalidHandleColor;
						Handles.DrawAAPolyLine(_edgeDefaultHandleWidth, p0, mousePosition, p1);
						return;
				}
			}

			if (edge == _nearestEdge)
			{
				switch (_handleState)
				{
					case HandleState.WillSplitEdge:
						Handles.color = _edgeWillSplitHandleColor;
						Handles.DrawAAPolyLine(_edgeHoverHandleWidth, p0, p1);
						return;
					case HandleState.WillRemoveEdge:
						Handles.color = _edgeWillRemoveHandleColor;
						Handles.DrawAAPolyLine(_edgeHoverHandleWidth, p0, p1);
						return;
				}
			}

			if (edge.sourceNode == _mouseDownNode || edge.targetNode == _mouseDownNode)
			{
				switch (_handleState)
				{
					case HandleState.MovingNode:
						if (edge.sourceNode == _mouseDownNode)
						{
							p0 = mousePosition + _mouseDownNodePosition - _mouseDownPosition;
						}
						else //if (edge.targetNode == _mouseDownNode)
						{
							p1 = mousePosition + _mouseDownNodePosition - _mouseDownPosition;
						}
						break;
					case HandleState.RemovingNode:
						Handles.color = _edgeRemovingHandleColor;
						Handles.DrawAAPolyLine(_edgeActiveHandleWidth, p0, p1);
						return;
					case HandleState.CannotMoveNode:
						if (edge.sourceNode == _mouseDownNode)
						{
							p0 = mousePosition + _mouseDownNodePosition - _mouseDownPosition;
							Handles.color = _edgeInvalidHandleColor;
							Handles.DrawAAPolyLine(_edgeDefaultHandleWidth, p0, p1);
						}
						else //if (edge.targetNode == _mouseDownNode)
						{
							p1 = mousePosition + _mouseDownNodePosition - _mouseDownPosition;
							Handles.color = _edgeInvalidHandleColor;
							Handles.DrawAAPolyLine(_edgeDefaultHandleWidth, p0, p1);
						}
						return;
				}
			}

			if (edge.sourceNode == _nearestNode || edge.targetNode == _nearestNode)
			{
				switch (_handleState)
				{
					case HandleState.WillRemoveNode:
						Handles.color = _edgeWillRemoveHandleColor;
						Handles.DrawAAPolyLine(_edgeHoverHandleWidth, p0, p1);
						return;
				}
			}

			Handles.color = _edgeDefaultHandleColor;
			Handles.DrawAAPolyLine(_edgeDefaultHandleWidth, p0, p1);
		}

		private void PaintPossibleEdge(ContourTriangulatorController controller, Vector2 mousePosition)
		{
			switch (_handleState)
			{
				case HandleState.WillAddEdge:
					{
						Vector2 p0 = controller.pointSitePositions[_mouseDownNode.index];
						Vector2 p1 = mousePosition;
						Handles.color = _edgeAddingHandleColor;
						Handles.DrawAAPolyLine(_edgeDefaultHandleWidth, p0, p1);
					}
					break;
				case HandleState.AddingEdge:
					{
						Vector2 p0 = controller.pointSitePositions[_mouseDownNode.index];
						Vector2 p1 = controller.pointSitePositions[_nearestNode.index];
						Handles.color = _edgeAddingHandleColor;
						Handles.DrawAAPolyLine(_edgeActiveHandleWidth, p0, p1);
					}
					break;
				case HandleState.AddingNodeEdge:
					{
						Vector2 p0 = controller.pointSitePositions[_nearestNode.index];
						Vector2 p1 = mousePosition;
						Handles.color = _edgeAddingHandleColor;
						Handles.DrawAAPolyLine(_edgeActiveHandleWidth, p0, p1);
					}
					break;
				case HandleState.CannotAddEdge:
					{
						Vector2 p0 = controller.pointSitePositions[_mouseDownNode.index];
						Vector2 p1 = controller.pointSitePositions[_nearestNode.index];
						Handles.color = _edgeInvalidHandleColor;
						Handles.DrawAAPolyLine(_edgeDefaultHandleWidth, p0, p1);
					}
					break;
				case HandleState.CannotAddNodeEdge:
					{
						Vector2 p0 = controller.pointSitePositions[_nearestNode.index];
						Vector2 p1 = mousePosition;
						Handles.color = _edgeInvalidHandleColor;
						Handles.DrawAAPolyLine(_edgeDefaultHandleWidth, p0, p1);
					}
					break;
			}
		}

		private void PaintVoronoiDiagram()
		{
			if (_voronoiDiagram == null) return;

			var topology = _voronoiDiagram._voronoiTopology;
			var voronoiEdgeShapes = _voronoiDiagram._voronoiEdgeShapes;

			foreach (var edge in topology.edges)
			{
				if (edge.isFirstTwin)
				{
					var p0 = _finiteVoronoiNodePositions[edge.sourceNode];
					var p1 = _finiteVoronoiNodePositions[edge.targetNode];
					var edgeShape = voronoiEdgeShapes[edge];
					if (edgeShape.isFinite)
					{
						if (edgeShape.isStraight)
						{
							Handles.color = new Color(0.5f, 0.5f, 0.5f, 1f);
							Handles.DrawAAPolyLine(2f, p0, p1);
						}
						else if (edgeShape.type == VoronoiEdgeShapeType.Parabola)
						{
							float cumulativeCurvature = Mathf.Abs(edgeShape.GetCurvatureSum(edgeShape.t0, edgeShape.t1));
							int segmentCount = Mathf.CeilToInt(cumulativeCurvature * 256f);
							var points = new Vector3[segmentCount + 1];

							points[0] = p0;
							for (int i = 1; i < segmentCount; ++i)
							{
								float tSegment = edgeShape.GetCurvatureSumOffset(edgeShape.t0, (cumulativeCurvature * i) / segmentCount);
								points[i] = edgeShape.Evaluate(tSegment);
							}
							points[segmentCount] = p1;

							Handles.color = new Color(0.5f, 0.5f, 0.5f, 1f);
							Handles.DrawAAPolyLine(2f, points);
						}
					}
					else
					{
						var v0 = p0 - _voronoiBounds.center;
						var v1 = p1 - _voronoiBounds.center;
						var v = new Vector2(
							Vector3.Dot(v1, v0),
							Vector3.Dot(v1, new Vector3(v0.y, -v0.x)));
						var angle = Mathf.Atan2(v.x, v.y) * Mathf.Rad2Deg;
						Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
						Handles.DrawWireArc(_voronoiBounds.center, Vector3.back, v0, angle, v0.magnitude);
					}
				}
			}

			foreach (var node in topology.nodes)
			{
				var p = _finiteVoronoiNodePositions[node];
				Handles.color = Color.gray;
				Handles.DrawSolidDisc(p, Vector3.back, HandleUtility.GetHandleSize(p) / 25f);
			}
		}

		private static bool IsFinite(Vector3 v)
		{
			return
				!float.IsNaN(v.x) && !float.IsInfinity(v.x) &&
				!float.IsNaN(v.y) && !float.IsInfinity(v.y) &&
				!float.IsNaN(v.z) && !float.IsInfinity(v.z);
		}

		private void FinitizeVoronoiNodePositions()
		{
			var siteGraph = _voronoiDiagram._siteGraph;
			var pointSitePositions = _voronoiDiagram._pointSitePositions;
			var voronoiTopology = _voronoiDiagram._voronoiTopology;
			var voronoiNodePositions = _voronoiDiagram._voronoiNodePositions;
			var voronoiFaceSiteTypes = _voronoiDiagram._voronoiFaceSiteTypes;
			var voronoiFaceSiteIndices = _voronoiDiagram._voronoiFaceSiteIndices;

			_finiteVoronoiNodePositions = new TopologyNodeDataArray<Vector3>(voronoiNodePositions.Count);

			var positionSum = Vector3.zero;
			var positionCount = 0;

			foreach (var position in pointSitePositions)
			{
				positionSum += position;
				++positionCount;
			}

			for (int i = 0; i < voronoiTopology.nodeCount; ++i)
			{
				var position = voronoiNodePositions[i];
				if (IsFinite(position))
				{
					positionSum += position;
					++positionCount;
				}
			}
			var positionAvg = positionSum / positionCount;

			float maxDistance = 0f;

			foreach (var position in pointSitePositions)
			{
				maxDistance = Mathf.Max(maxDistance, Vector3.Distance(position, positionAvg));
			}

			for (int i = 0; i < voronoiTopology.nodeCount; ++i)
			{
				var position = voronoiNodePositions[i];
				if (IsFinite(position))
				{
					maxDistance = Mathf.Max(maxDistance, Vector3.Distance(position, positionAvg));
				}
			}

			_voronoiBounds = new Sphere(positionAvg, maxDistance * 2f);

			for (int i = 0; i < voronoiTopology.nodeCount; ++i)
			{
				_finiteVoronoiNodePositions[i] = voronoiNodePositions[i];
				if (!IsFinite(voronoiNodePositions[i]))
				{
					foreach (var edge in voronoiTopology.nodes[i].edges)
					{
						if (voronoiFaceSiteTypes[edge] != VoronoiSiteType.None && voronoiFaceSiteTypes[edge.twin] != VoronoiSiteType.None)
						{
							switch (voronoiFaceSiteTypes[edge])
							{
								case VoronoiSiteType.Point:
									{
										var siteIndex0 = voronoiFaceSiteIndices[edge];
										var p0 = pointSitePositions[siteIndex0];

										switch (voronoiFaceSiteTypes[edge.twin])
										{
											case VoronoiSiteType.Point:
												{
													var siteIndex1 = voronoiFaceSiteIndices[edge.twin];
													var p1 = pointSitePositions[siteIndex1];

													var ray = new ScaledRay((p0 + p1) / 2f, Vector3.Cross(Vector3.back, p1 - p0));
													Vector3 confinedPosition;
													if (Geometry.IntersectForwardInternal(_voronoiBounds, ray, out confinedPosition))
													{
														_finiteVoronoiNodePositions[i] = confinedPosition;
													}
													break;
												}
											case VoronoiSiteType.Line:
												{
													var siteIndex1 = voronoiFaceSiteIndices[edge.twin];
													var siteIndex1a = siteGraph.GetEdgeTargetNodeIndex(siteIndex1 ^ 1);
													var siteIndex1b = siteGraph.GetEdgeTargetNodeIndex(siteIndex1);
													var p1a = pointSitePositions[siteIndex1a];
													var p1b = pointSitePositions[siteIndex1b];
													var v1 = p1b - p1a;

													if (siteIndex0 == siteIndex1b)
													{
														var ray = new ScaledRay(p1b, Vector3.Cross(v1, Vector3.back));

														Vector3 confinedPosition;
														if (Geometry.IntersectForwardInternal(_voronoiBounds, ray, out confinedPosition))
														{
															_finiteVoronoiNodePositions[i] = confinedPosition;
														}
													}
													else
													{
														var parabola = Parabola.FromFocusDirectrix(p0, new ScaledRay(p1a, v1));

														Vector3 confinedPosition;
														if (Geometry.IntersectAscendingExit(parabola, _voronoiBounds, out confinedPosition))
														{
															_finiteVoronoiNodePositions[i] = confinedPosition;
														}
													}
													break;
												}
											default: throw new NotImplementedException();
										}
										break;
									}
								case VoronoiSiteType.Line:
									{
										var siteIndex0 = voronoiFaceSiteIndices[edge];
										var siteIndex0a = siteGraph.GetEdgeTargetNodeIndex(siteIndex0 ^ 1);
										var siteIndex0b = siteGraph.GetEdgeTargetNodeIndex(siteIndex0);
										var p0a = pointSitePositions[siteIndex0a];
										var p0b = pointSitePositions[siteIndex0b];
										var v0 = p0b - p0a;

										switch (voronoiFaceSiteTypes[edge.twin])
										{
											case VoronoiSiteType.Point:
												{
													var siteIndex1 = voronoiFaceSiteIndices[edge.twin];
													if (siteIndex0a == siteIndex1)
													{
														var ray = new ScaledRay(p0a, Vector3.Cross(v0, Vector3.back));

														Vector3 confinedPosition;
														if (Geometry.IntersectForwardInternal(_voronoiBounds, ray, out confinedPosition))
														{
															_finiteVoronoiNodePositions[i] = confinedPosition;
														}
													}
													else
													{
														var p1 = pointSitePositions[siteIndex1];
														var parabola = Parabola.FromFocusDirectrix(p1, new ScaledRay(p0a, v0));

														Vector3 confinedPosition;
														if (Geometry.IntersectAscendingExit(parabola, _voronoiBounds, out confinedPosition))
														{
															_finiteVoronoiNodePositions[i] = confinedPosition;
														}
													}
													break;
												}
											case VoronoiSiteType.Line:
												{
													var siteIndex1 = voronoiFaceSiteIndices[edge.twin];
													var siteIndex1a = siteGraph.GetEdgeTargetNodeIndex(siteIndex1 ^ 1);
													var siteIndex1b = siteGraph.GetEdgeTargetNodeIndex(siteIndex1);
													var p1a = pointSitePositions[siteIndex1a];
													var p1b = pointSitePositions[siteIndex1b];
													var v1 = p1b - p1a;

													var ray0 = new Ray(p0a, p0b - p0a);
													var ray1 = new Ray(p1a, p1b - p1a);

													var origin = Geometry.GetNearestPoint(ray0, ray1);
													var direction = ray1.direction - ray0.direction;

													var ray = new ScaledRay(origin, direction);

													Vector3 confinedPosition;
													if (Geometry.IntersectForwardInternal(_voronoiBounds, ray, out confinedPosition))
													{
														_finiteVoronoiNodePositions[i] = confinedPosition;
													}
													break;
												}
											default: throw new NotImplementedException();
										}
										break;
									}
								default: throw new NotImplementedException();
							}
							break;
						}
					}
				}
			}
		}

		private void RebuildContourMesh()
		{
			var controller = (ContourTriangulatorController)target;

			var meshFilter = controller.GetComponent<MeshFilter>();
			var mesh = meshFilter.sharedMesh;
			if (mesh == null)
			{
				mesh = meshFilter.sharedMesh = new Mesh();
			}
			mesh.Clear();

			if (controller.siteGraph.nodeCount > 0)
			{
				_voronoiGenerator.SetSites(controller.siteGraph, controller.pointSitePositions, Vector3.zero, Vector3.right, Vector3.up);
				_voronoiDiagram = _voronoiGenerator.Generate();
				FinitizeVoronoiNodePositions();

				var vertexIndexMap = new Dictionary<ContourTriangulator.PositionId, int>();
				var vertexPositions = new List<Vector3>();
				var vertexColors = new List<Color>();
				var triangleIndices = new List<int>();

				var contourColors = controller.contourColors;
				var contourDistances = controller.contourDistances;

				ContourTriangulator.OnVertexDelegate onVertex =
					(ContourTriangulator.PositionId positionId, Vector3 position, VoronoiSiteType siteType, int siteIndex, int contourIndex, float distance) =>
					{
						if (!vertexIndexMap.ContainsKey(positionId))
						{
							vertexIndexMap.Add(positionId, vertexPositions.Count);
							vertexPositions.Add(position);
							if (contourIndex > 0)
							{
								vertexColors.Add(Color.Lerp(contourColors[contourIndex - 1], contourColors[contourIndex], (distance - contourDistances[contourIndex - 1]) / (contourDistances[contourIndex] - contourDistances[contourIndex - 1])));
							}
							else
							{
								vertexColors.Add(contourColors[0]);
							}
						//Debug.LogFormat("Add Vertex({0}):  {1}, {2}, {3}, {4}, {5}, {6:F4}, {7}", vertexPositions.Count - 1, positionId, position.ToString("F2"), siteType, siteIndex, contourIndex, distance, vertexColors[vertexColors.Count - 1]);
					}
						else
						{
						//Debug.LogFormat("Reuse Vertex({0}):  {1}, {2}, {3}, {4}, {5}, {6:F4}, {7}", vertexIndexMap[positionId], positionId, position.ToString("F2"), siteType, siteIndex, contourIndex, distance, vertexColors[vertexColors.Count - 1]);
					}
					};

				ContourTriangulator.OnTriangleDelegate onTriangle =
					(ContourTriangulator.PositionId positionId0, ContourTriangulator.PositionId positionId1, ContourTriangulator.PositionId positionId2) =>
					{
						triangleIndices.Add(vertexIndexMap[positionId0]);
						triangleIndices.Add(vertexIndexMap[positionId1]);
						triangleIndices.Add(vertexIndexMap[positionId2]);
					//Debug.LogFormat("Triangle({0}, {1}, {2}):  {1}, {2}, {3}", vertexIndexMap[positionId0], vertexIndexMap[positionId1], vertexIndexMap[positionId2], vertexPositions[vertexIndexMap[positionId0]].ToString("F2"), vertexPositions[vertexIndexMap[positionId1]].ToString("F2"), vertexPositions[vertexIndexMap[positionId2]].ToString("F2"));
				};

				try
				{
					_contourTriangulator.maxCurvaturePerSegment = controller.maxCurvaturPerSegment;
				}
				catch (Exception)
				{
				}

				if (_voronoiDiagram._siteEdgeFirstVoronoiEdgeIndices.Count > 0)
				{
					var edge = new TopologyEdge(_voronoiDiagram._voronoiTopology, _voronoiDiagram._siteEdgeFirstVoronoiEdgeIndices[0]);
					_contourTriangulator.Triangulate(_voronoiDiagram, edge.sourceFace, onVertex, onTriangle, Vector3.back, contourDistances);

					mesh.SetVertices(vertexPositions);
					mesh.SetColors(vertexColors);
					mesh.SetIndices(triangleIndices.ToArray(), MeshTopology.Triangles, 0);
				}
			}

			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.UploadMeshData(true);

			EditorUtility.SetDirty(controller);
		}
	}
}
