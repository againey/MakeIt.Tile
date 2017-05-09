using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Experilous.Topologies;

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
		private static Color _nodeDefaultHandleColor = new Color(0f, 1f, 0f, 1f);
		private static Color _nodeWillAlterHandleColor = new Color(0.5f, 1f, 0.5f, 1f);
		private static Color _nodeWillRemoveHandleColor = new Color(1f, 0.5f, 0.5f, 1f);
		private static Color _nodeWillAddHandleColor = new Color(0.5f, 1f, 0.5f, 1f);
		private static Color _nodeMovingHandleColor = new Color(1f, 1f, 1f, 1f);
		private static Color _nodeConnectingHandleColor = new Color(0.5f, 1f, 0.5f, 1f);
		private static Color _nodeRemovingHandleColor = new Color(1f, 0f, 0f, 1f);
		private static Color _nodeAddingHandleColor = new Color(1f, 1f, 1f, 1f);

		private const float _edgeDefaultHandleWidth = 5f / 4f;
		private const float _edgeHoverHandleWidth = 5f / 2f;
		private const float _edgeActiveHandleWidth = 5f / 2f;
		private static Color _edgeDefaultHandleColor = new Color(0f, 1f, 0f, 1f);
		private static Color _edgeWillSplitHandleColor = new Color(0.5f, 1f, 0.5f, 1f);
		private static Color _edgeWillRemoveHandleColor = new Color(1f, 0.5f, 0.5f, 1f);
		private static Color _edgeSplittingHandleColor = new Color(0.5f, 1f, 0.5f, 1f);
		private static Color _edgeRemovingHandleColor = new Color(1f, 0f, 0f, 1f);
		private static Color _edgeAddingHandleColor = new Color(1f, 1f, 1f, 1f);

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
			WillSplitEdge, // nearest to edge with control key
			MovingNode, // left mouse buton down near node with no modifier key
			AddingEdge, // right mouse button down near node with no modifier key
			SplittingEdge, // left/right mouse button down nearest to edge with control key
			AddingNode, // left mouse button down nearest to node with control key
			AddingNodeEdge, // right mouse button down nearest to node with control key
			RemovingNode, // left/right mouse button down nearest to node with shift key
			RemovingEdge, // left/right mouse button down nearest to edge with shift key
		}

		private MouseButton _mouseButtonState = MouseButton.None;
		private HandleState _handleState = HandleState.None;

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
				controller.pointSitePositions = new List<Vector3>();
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

				switch (ev.type)
				{
					case EventType.Layout:
						if (EditorWindow.mouseOverWindow == SceneView.currentDrawingSceneView)
						{
							FindNearestNode(controller, mousePosition);
							FindNearestEdge(controller, mousePosition);

							if (_handleState != HandleState.Canceled)
							{
								_handleState = DetermineHandleState(ev);
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
									HandleUtility.AddControl(controlId, Vector2.Distance(_mouseDownNodePosition, _mouseDownPosition));
									break;
								case HandleState.WillAddNode:
								case HandleState.AddingNode:
								case HandleState.AddingNodeEdge:
								case HandleState.AddingEdge:
								case HandleState.SplittingEdge:
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

							_handleState = DetermineHandleState(ev);
						}
						break;
					case EventType.MouseDrag:
						if (GUIUtility.hotControl == controlId && _mouseButtonState != MouseButton.None && _handleState != HandleState.Canceled)
						{
							_handleState = DetermineHandleState(ev);

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
									break;
								case HandleState.AddingEdge:
									if (_nearestNode != _mouseDownNode && _nearestNodeDistance <= _nodeLongActiveDistance && !_mouseDownNode.FindEdge(_nearestNode))
									{
										Undo.RecordObject(controller, "Add Line Site");
										controller.siteGraph.Connect(_mouseDownNode, _nearestNode);
										EditorUtility.SetDirty(controller);
										GUI.changed = true;
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
									break;
								case HandleState.AddingNode:
									Undo.RecordObject(controller, "Add Point Site");
									controller.siteGraph.AddNode();
									controller.pointSitePositions.Add(mousePosition);
									EditorUtility.SetDirty(controller);
									GUI.changed = true;
									break;
								case HandleState.AddingNodeEdge:
									if (_nearestNode)
									{
										Undo.RecordObject(controller, "Add and Connect Point Site");
										controller.siteGraph.Connect(_nearestNode, controller.siteGraph.AddNode());
									}
									else
									{
										Undo.RecordObject(controller, "Add Point Site");
										controller.siteGraph.AddNode();
									}
									controller.pointSitePositions.Add(mousePosition);
									EditorUtility.SetDirty(controller);
									GUI.changed = true;
									break;
								default:
									throw new InvalidOperationException();
							}

							GUIUtility.hotControl = 0;
							EditorGUIUtility.SetWantsMouseJumping(0);

							_handleState = DetermineHandleState(ev);

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

				Handles.color = _edgeDefaultHandleColor;
				foreach (var edge in controller.siteGraph.edges)
				{
					if (edge.isFirstTwin)
					{
						Vector3 p0 = controller.pointSitePositions[edge.sourceNode.index];
						Vector3 p1 = controller.pointSitePositions[edge.targetNode.index];
						float size = HandleUtility.GetHandleSize((p0 + p1) / 2f) * _edgeDefaultHandleWidth;
						Handles.DrawAAPolyLine(size, p0, p1);
					}
				}

				Handles.color = _nodeDefaultHandleColor;
				foreach (var node in controller.siteGraph.nodes)
				{
					using (var changeCheck = new EditorGUI.ChangeCheckScope())
					{
						Vector3 nodePosition = controller.pointSitePositions[node.index];
						float size = HandleUtility.GetHandleSize(nodePosition) * _nodeDefaultHandleSize;
						Handles.DrawSolidDisc(nodePosition, Vector3.back, size);
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

		private HandleState DetermineHandleState(Event ev)
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
							return HandleState.MovingNode;
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
							return HandleState.SplittingEdge;
						}

						return HandleState.AddingNode;
					}
					break;
				case MouseButton.Right:
					if (!ev.shift && !ev.control)
					{
						if (_mouseDownNode && _mouseDownNodeDistance <= _nodeLongActiveDistance)
						{
							return HandleState.AddingEdge;
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
							return HandleState.SplittingEdge;
						}

						return HandleState.AddingNodeEdge;
					}
					break;
			}

			return HandleState.None;
		}

		private void PaintNode(ContourTriangulatorController controller, GraphNode node, Vector2 mousePosition)
		{
			Vector2 p0 = controller.pointSitePositions[node.index];

			float baseSize;

			if (node == _mouseDownNode)
			{
				switch (_handleState)
				{
					case HandleState.MovingNode:
						p0 = mousePosition + _mouseDownNodePosition - _mouseDownPosition;
						baseSize = HandleUtility.GetHandleSize(p0);
						Handles.color = _nodeMovingHandleColor;
						Handles.DrawSolidDisc(p0, Vector3.back, baseSize * _nodeActiveHandleSize);
						return;
					case HandleState.AddingEdge:
						p0 = _mouseDownNodePosition;
						baseSize = HandleUtility.GetHandleSize(p0);
						Handles.color = _nodeConnectingHandleColor;
						Handles.DrawSolidDisc(p0, Vector3.back, baseSize * _nodeActiveHandleSize);
						return;
					case HandleState.RemovingNode:
						p0 = _mouseDownNodePosition;
						baseSize = HandleUtility.GetHandleSize(p0);
						Handles.color = _nodeRemovingHandleColor;
						Handles.DrawSolidDisc(p0, Vector3.back, baseSize * _nodeActiveHandleSize);
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
						Handles.DrawSolidDisc(p0, Vector3.back, baseSize * _nodeHoverHandleSize);
						return;
					case HandleState.WillRemoveNode:
						p0 = _nearestNodePosition;
						baseSize = HandleUtility.GetHandleSize(p0);
						Handles.color = _nodeWillRemoveHandleColor;
						Handles.DrawSolidDisc(p0, Vector3.back, baseSize * _nodeHoverHandleSize);
						return;
					case HandleState.AddingEdge:
						if (node != _mouseDownNode && _nearestNodeDistance <= _nodeLongActiveDistance)
						{
							p0 = _nearestNodePosition;
							baseSize = HandleUtility.GetHandleSize(p0);
							Handles.color = _nodeConnectingHandleColor;
							Handles.DrawSolidDisc(p0, Vector3.back, baseSize * _nodeActiveHandleSize);
							return;
						}
						break;
				}
			}

			baseSize = HandleUtility.GetHandleSize(p0);
			Handles.color = _nodeDefaultHandleColor;
			Handles.DrawSolidDisc(p0, Vector3.back, baseSize * _nodeDefaultHandleSize);
		}

		private void PaintPossibleNode(ContourTriangulatorController controller, Vector2 mousePosition)
		{
			float baseSize;

			switch (_handleState)
			{
				case HandleState.WillAddNode:
					baseSize = HandleUtility.GetHandleSize(mousePosition);
					Handles.color = _nodeWillAddHandleColor;
					Handles.DrawSolidDisc(mousePosition, Vector3.back, baseSize * _nodeHoverHandleSize);
					break;
				case HandleState.WillSplitEdge:
					baseSize = HandleUtility.GetHandleSize(_nearestEdgePosition);
					Handles.color = _nodeWillAddHandleColor;
					Handles.DrawSolidDisc(_nearestEdgePosition, Vector3.back, baseSize * _nodeHoverHandleSize);
					break;
				case HandleState.AddingNode:
				case HandleState.AddingNodeEdge:
					baseSize = HandleUtility.GetHandleSize(mousePosition);
					Handles.color = _nodeAddingHandleColor;
					Handles.DrawSolidDisc(mousePosition, Vector3.back, baseSize * _nodeActiveHandleSize);
					break;
				case HandleState.SplittingEdge:
					baseSize = HandleUtility.GetHandleSize(mousePosition);
					Handles.color = _nodeAddingHandleColor;
					Handles.DrawSolidDisc(mousePosition, Vector3.back, baseSize * _nodeActiveHandleSize);
					break;
			}
		}

		private void PaintEdge(ContourTriangulatorController controller, GraphEdge edge, Vector2 mousePosition)
		{
			Vector2 p0 = controller.pointSitePositions[edge.sourceNode.index];
			Vector2 p1 = controller.pointSitePositions[edge.targetNode.index];

			float baseSize;

			if (edge == _mouseDownEdge)
			{
				switch (_handleState)
				{
					case HandleState.SplittingEdge:
						baseSize = HandleUtility.GetHandleSize(mousePosition);
						Handles.color = _edgeSplittingHandleColor;
						Handles.DrawAAPolyLine(baseSize * _edgeActiveHandleWidth, p0, mousePosition, p1);
						return;
					case HandleState.RemovingEdge:
						baseSize = HandleUtility.GetHandleSize(_mouseDownEdgePosition);
						Handles.color = _edgeRemovingHandleColor;
						Handles.DrawAAPolyLine(baseSize * _edgeActiveHandleWidth, p0, p1);
						return;
				}
			}

			if (edge == _nearestEdge)
			{
				switch (_handleState)
				{
					case HandleState.WillSplitEdge:
						baseSize = HandleUtility.GetHandleSize(_nearestEdgePosition);
						Handles.color = _edgeWillSplitHandleColor;
						Handles.DrawAAPolyLine(baseSize * _edgeHoverHandleWidth, p0, p1);
						return;
					case HandleState.WillRemoveEdge:
						baseSize = HandleUtility.GetHandleSize(_nearestEdgePosition);
						Handles.color = _edgeWillRemoveHandleColor;
						Handles.DrawAAPolyLine(baseSize * _edgeHoverHandleWidth, p0, p1);
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
						baseSize = HandleUtility.GetHandleSize(_mouseDownNodePosition);
						Handles.color = _edgeRemovingHandleColor;
						Handles.DrawAAPolyLine(baseSize * _edgeActiveHandleWidth, p0, p1);
						return;
				}
			}

			if (edge.sourceNode == _nearestNode || edge.targetNode == _nearestNode)
			{
				switch (_handleState)
				{
					case HandleState.WillRemoveNode:
						baseSize = HandleUtility.GetHandleSize(_mouseDownNodePosition);
						Handles.color = _edgeWillRemoveHandleColor;
						Handles.DrawAAPolyLine(baseSize * _edgeHoverHandleWidth, p0, p1);
						return;
				}
			}

			baseSize = HandleUtility.GetHandleSize((p0 + p1) / 2f);
			Handles.color = _edgeDefaultHandleColor;
			Handles.DrawAAPolyLine(baseSize * _edgeDefaultHandleWidth, p0, p1);
		}

		private void PaintPossibleEdge(ContourTriangulatorController controller, Vector2 mousePosition)
		{
			switch (_handleState)
			{
				case HandleState.AddingEdge:
					if (_nearestNode != _mouseDownNode && _nearestNodeDistance <= _nodeLongActiveDistance)
					{
						Vector2 p0 = controller.pointSitePositions[_mouseDownNode.index];
						Vector2 p1 = controller.pointSitePositions[_nearestNode.index];
						float baseSize = HandleUtility.GetHandleSize((p0 + p1) / 2f);
						Handles.color = _edgeAddingHandleColor;
						Handles.DrawAAPolyLine(baseSize * _edgeActiveHandleWidth, p0, p1);
					}
					else
					{
						Vector2 p0 = controller.pointSitePositions[_mouseDownNode.index];
						Vector2 p1 = mousePosition;
						float baseSize = HandleUtility.GetHandleSize((p0 + p1) / 2f);
						Handles.color = _edgeAddingHandleColor;
						Handles.DrawAAPolyLine(baseSize * _edgeDefaultHandleWidth, p0, p1);
					}
					break;
				case HandleState.AddingNodeEdge:
					if (_nearestNode && _nearestNodeDistance <= _nodeLongActiveDistance)
					{
						Vector2 p0 = controller.pointSitePositions[_nearestNode.index];
						Vector2 p1 = mousePosition;
						float baseSize = HandleUtility.GetHandleSize((p0 + p1) / 2f);
						Handles.color = _edgeAddingHandleColor;
						Handles.DrawAAPolyLine(baseSize * _edgeActiveHandleWidth, p0, p1);
					}
					break;
			}
		}
	}
}
