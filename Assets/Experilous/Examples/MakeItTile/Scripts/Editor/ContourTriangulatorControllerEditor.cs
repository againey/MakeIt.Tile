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

		private static float _voronoiNodeDefaultRadius = 0.04f;
		private static float _voronoiNodeLabeledRadius = 0.12f;
		private static float _voronoidEdgeWidth = 2f;
		private static float _voronoiEdgeOffset = 0.03f;
		private static float _voronoiEdgeArrowOffset = 0.075f;
		private static float _voronoiEdgeLabelOffset = 0.12f;
		private static float _voronoiEdgeMaxAngleChangePerSegment = 1f / 256f;
		private static GUIStyle _voronoiNodeLabelStyle;
		private static GUIStyle _voronoiEdgeLabelStyle;
		private static Color _voronoiNodeColor = new Color(1f, 1f, 1f, 0.25f);
		private static Color _voronoiFiniteEdgeColor = new Color(0.5f, 0.5f, 0.5f, 1f);
		private static Color _voronoiInfiniteEdgeColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

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

		[SerializeField] private bool _showVoronoiDiagram;
		[SerializeField] private bool _showVoronoiLabels;

		private PlanarVoronoiGenerator _voronoiGenerator;
		private VoronoiDiagram _voronoiDiagram;
		private TopologyNodeDataArray<Vector3> _finiteVoronoiNodePositions;
		private Sphere _voronoiBounds;
		private ContourTriangulator _contourTriangulator;
		private List<ContourDescriptor> _contourLoops;

		private DynamicGraph _mouseDragSiteGraph;
		private ContourTriangulatorController.GraphNodePositionList _mouseDragPointSitePositions;
		private bool[] _mouseDragContoursEnabled;

		[SerializeField] private bool _contourLayerListExpanded = false;
		[SerializeField] private bool _contourListExpanded = false;
		private GUIStyle _addButtonStyle;
		private Texture2D _addButtonImage;
		private GUIStyle _removeButtonStyle;
		private Texture2D _removeButtonImage;

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
				EditorApplication.delayCall += () =>
				{
					RebuildVoronoiDiagram(controller.siteGraph, controller.pointSitePositions, out controller.contoursEnabled);
					RebuildContourMesh(controller.siteGraph, controller.pointSitePositions, controller.contoursEnabled);
				};
			}

			if (_mouseDragSiteGraph == null)
			{
				_mouseDragSiteGraph = new DynamicGraph();
			}

			if (_mouseDragPointSitePositions == null)
			{
				_mouseDragPointSitePositions = new ContourTriangulatorController.GraphNodePositionList();
			}

			_addButtonStyle = null;
			_removeButtonStyle = null;

			Tools.hidden = _editSiteGraph;
		}

		protected void OnDisable()
		{
		}

		public override void OnInspectorGUI()
		{
			var controller = (ContourTriangulatorController)target;

			EditorGUI.BeginChangeCheck();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.FlexibleSpace();
			using (var changeCheck = new EditorGUI.ChangeCheckScope())
			{
				_editSiteGraph = GUILayout.Toggle(_editSiteGraph, new GUIContent("Edit Site Graph"), GUI.skin.button, GUILayout.Width(128f));
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
			if (GUILayout.Button(new GUIContent("Reset Site Graph"), GUILayout.Width(128f)))
			{
				Undo.RecordObject(controller, "Reset Site Graph");
				controller.siteGraph.Clear();
				controller.pointSitePositions.Clear();
				EditorUtility.SetDirty(controller);
				GUI.changed = true;
				Repaint();
			}
			if (GUILayout.Button(new GUIContent("Rebuild Mesh"), GUILayout.Width(128f)))
			{
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			_showVoronoiDiagram = GUILayout.Toggle(_showVoronoiDiagram, new GUIContent("Show Voronoi Diagram"));
			if (_showVoronoiDiagram)
			{
				_showVoronoiLabels = GUILayout.Toggle(_showVoronoiLabels, new GUIContent("Show Voronoi Labels"));
			}
			else
			{
				bool wasEnabled = GUI.enabled;
				GUI.enabled = false;
				GUILayout.Toggle(false, new GUIContent("Show Voronoi Labels"));
				GUI.enabled = wasEnabled;
			}
			GUILayout.Space(12f);

			controller.twinEdge = EditorGUILayout.Toggle("Use Twin Edge", controller.twinEdge);
			controller.maxCurvaturePerSegment = EditorGUILayout.Slider("Max Curvature Per Segment", controller.maxCurvaturePerSegment, 0.01f, Mathf.PI / 2f);
			controller.contourDistanceScale = EditorGUILayout.Slider("Contour Distance Scale", controller.contourDistanceScale, 0.1f, 10f);

			_contourLayerListExpanded = EditorGUILayout.Foldout(_contourLayerListExpanded, "Contour Layers");
			if (_contourLayerListExpanded)
			{
				EditorGUI.indentLevel += 1;

				if (_addButtonStyle == null)
				{
					_addButtonStyle = new GUIStyle(GUI.skin.GetStyle("ol plus"));
					int width = _addButtonStyle.normal.background.width;
					int height = _addButtonStyle.normal.background.height;
					_addButtonStyle.fixedWidth = width;
					_addButtonStyle.fixedHeight = height;
					_addButtonStyle.margin = new RectOffset(0, 0, 0, 0);
					_addButtonStyle.border = new RectOffset(width, 0, height, 0);
					_addButtonStyle.padding = new RectOffset(0, 0, 0, 0);
					_addButtonStyle.contentOffset = Vector2.zero;
					_addButtonStyle.stretchWidth = _addButtonStyle.stretchHeight = false;
				}

				if (_removeButtonStyle == null)
				{
					_removeButtonStyle = new GUIStyle(GUI.skin.GetStyle("ol minus"));
					int width = _removeButtonStyle.normal.background.width;
					int height = _removeButtonStyle.normal.background.height;
					_removeButtonStyle.fixedWidth = width;
					_removeButtonStyle.fixedHeight = height;
					_removeButtonStyle.margin = new RectOffset(0, 0, 0, 0);
					_removeButtonStyle.border = new RectOffset(width, 0, height, 0);
					_removeButtonStyle.padding = new RectOffset(0, 0, 0, 0);
					_removeButtonStyle.contentOffset = Vector2.zero;
					_removeButtonStyle.stretchWidth = _removeButtonStyle.stretchHeight = false;
				}

				GUILayout.BeginHorizontal();

				GUILayout.Label("#", GUILayout.Width(30f));
				GUILayout.Space(15f);
				GUILayout.Label("Offset", GUILayout.ExpandWidth(true));
				GUILayout.Space(15f);
				GUILayout.Label("Color", GUILayout.Width(85f));
				GUILayout.Space(10f + _addButtonStyle.fixedWidth + _removeButtonStyle.fixedWidth);

				GUILayout.EndHorizontal();

				for (int i = 0; i < controller.contourOffsets.Length; ++i)
				{
					GUILayout.BeginHorizontal();

					GUILayout.Label(i.ToString(), GUILayout.Width(30f));
					controller.contourOffsets[i] = EditorGUILayout.Slider(controller.contourOffsets[i], 0f, 10f, GUILayout.ExpandWidth(true));
					controller.contourColors[i] = EditorGUILayout.ColorField(controller.contourColors[i], GUILayout.Width(100f));

					GUILayout.Space(10f);

					if (GUILayout.Button(new GUIContent("", "Add Contour Layer"), _addButtonStyle))
					{
						int index = i;
						EditorApplication.delayCall += () => { AddContour(index); };
						GUI.changed = true;
					}

					Core.GUIExtensions.PushEnable(controller.contourOffsets.Length > 2);

					if (GUILayout.Button(new GUIContent("", "Remove Contour Layer"), _removeButtonStyle))
					{
						int index = i;
						EditorApplication.delayCall += () => { RemoveContour(index); };
						GUI.changed = true;
					}

					Core.GUIExtensions.PopEnable();

					GUILayout.EndHorizontal();
				}

				EditorGUI.indentLevel -= 1;
			}

			_contourListExpanded = EditorGUILayout.Foldout(_contourListExpanded, "Contours");
			if (_contourListExpanded)
			{
				EditorGUI.indentLevel += 1;

				for (int i = 0; i < controller.contoursEnabled.Length; ++i)
				{
					controller.contoursEnabled[i] = EditorGUILayout.Toggle(new GUIContent(string.Format("Contour Loop {0}", i)), controller.contoursEnabled[i]);
				}

				EditorGUI.indentLevel -= 1;
			}

			if (EditorGUI.EndChangeCheck())
			{
				if (Event.current.type != EventType.Repaint)
				{
					EditorApplication.delayCall += () =>
					{
						RebuildVoronoiDiagram(controller.siteGraph, controller.pointSitePositions, out controller.contoursEnabled);
						RebuildContourMesh(controller.siteGraph, controller.pointSitePositions, controller.contoursEnabled);
						EditorUtility.SetDirty(controller);
					};
				}
				SceneView.RepaintAll();
			}
		}

		private void AddContour(int index)
		{
			var controller = (ContourTriangulatorController)target;

			Undo.RecordObject(controller, "Add Contour");

			int oldLength = controller.contourOffsets.Length;
			int newLength = oldLength + 1;

			var newOffsets = new float[newLength];
			var newColors = new Color[newLength];

			Array.Copy(controller.contourOffsets, 0, newOffsets, 0, index);
			Array.Copy(controller.contourColors, 0, newColors, 0, index);

			Array.Copy(controller.contourOffsets, index, newOffsets, index + 1, oldLength - index);
			Array.Copy(controller.contourColors, index, newColors, index + 1, oldLength - index);

			newOffsets[index] = 0f;
			newColors[index] = controller.contourColors[index];

			controller.contourOffsets = newOffsets;
			controller.contourColors = newColors;

			RebuildVoronoiDiagram(controller.siteGraph, controller.pointSitePositions, out controller.contoursEnabled);
			RebuildContourMesh(controller.siteGraph, controller.pointSitePositions, controller.contoursEnabled);
			EditorUtility.SetDirty(controller);
		}

		private void RemoveContour(int index)
		{
			var controller = (ContourTriangulatorController)target;

			Undo.RecordObject(controller, "Remove Contour");

			int oldLength = controller.contourOffsets.Length;
			int newLength = oldLength - 1;

			var newOffsets = new float[newLength];
			var newColors = new Color[newLength];

			Array.Copy(controller.contourOffsets, 0, newOffsets, 0, index);
			Array.Copy(controller.contourColors, 0, newColors, 0, index);

			Array.Copy(controller.contourOffsets, index + 1, newOffsets, index, newLength - index);
			Array.Copy(controller.contourColors, index + 1, newColors, index, newLength - index);

			if (index < newLength)
			{
				newOffsets[index] = controller.contourOffsets[index] + controller.contourOffsets[index + 1];
			}

			controller.contourOffsets = newOffsets;
			controller.contourColors = newColors;

			RebuildVoronoiDiagram(controller.siteGraph, controller.pointSitePositions, out controller.contoursEnabled);
			RebuildContourMesh(controller.siteGraph, controller.pointSitePositions, controller.contoursEnabled);
			EditorUtility.SetDirty(controller);
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

							ApplyChangeAndRebuildImmediateContourMesh(controller, mousePosition);
						}
						break;
					case EventType.MouseDrag:
						if (GUIUtility.hotControl == controlId && _mouseButtonState != MouseButton.None && _handleState != HandleState.Canceled)
						{
							_handleState = DetermineHandleState(ev, controller, mousePosition);

							ApplyChangeAndRebuildImmediateContourMesh(controller, mousePosition);

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
									EditorApplication.delayCall += () =>
									{
										RebuildVoronoiDiagram(controller.siteGraph, controller.pointSitePositions, out controller.contoursEnabled);
										RebuildContourMesh(controller.siteGraph, controller.pointSitePositions, controller.contoursEnabled);
										EditorUtility.SetDirty(controller);
									};
									GUI.changed = true;
									break;
								case HandleState.AddingEdge:
									if (_nearestNode != _mouseDownNode && _nearestNodeDistance <= _nodeLongActiveDistance && !_mouseDownNode.FindEdge(_nearestNode))
									{
										Undo.RecordObject(controller, "Add Line Site");
										controller.siteGraph.Connect(_mouseDownNode, _nearestNode);
										EditorApplication.delayCall += () =>
										{
											RebuildVoronoiDiagram(controller.siteGraph, controller.pointSitePositions, out controller.contoursEnabled);
											RebuildContourMesh(controller.siteGraph, controller.pointSitePositions, controller.contoursEnabled);
											EditorUtility.SetDirty(controller);
										};
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
										EditorApplication.delayCall += () =>
										{
											RebuildVoronoiDiagram(controller.siteGraph, controller.pointSitePositions, out controller.contoursEnabled);
											RebuildContourMesh(controller.siteGraph, controller.pointSitePositions, controller.contoursEnabled);
											EditorUtility.SetDirty(controller);
										};
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
									EditorApplication.delayCall += () =>
									{
										RebuildVoronoiDiagram(controller.siteGraph, controller.pointSitePositions, out controller.contoursEnabled);
										RebuildContourMesh(controller.siteGraph, controller.pointSitePositions, controller.contoursEnabled);
										EditorUtility.SetDirty(controller);
									};
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
									EditorApplication.delayCall += () =>
									{
										RebuildVoronoiDiagram(controller.siteGraph, controller.pointSitePositions, out controller.contoursEnabled);
										RebuildContourMesh(controller.siteGraph, controller.pointSitePositions, controller.contoursEnabled);
										EditorUtility.SetDirty(controller);
									};
									GUI.changed = true;
									break;
								case HandleState.AddingNode:
									Undo.RecordObject(controller, "Add Point Site");
									controller.siteGraph.AddNode();
									controller.pointSitePositions.Add(mousePosition);
									EditorApplication.delayCall += () =>
									{
										RebuildVoronoiDiagram(controller.siteGraph, controller.pointSitePositions, out controller.contoursEnabled);
										RebuildContourMesh(controller.siteGraph, controller.pointSitePositions, controller.contoursEnabled);
										EditorUtility.SetDirty(controller);
									};
									GUI.changed = true;
									break;
								case HandleState.AddingNodeEdge:
									Undo.RecordObject(controller, "Add and Connect Point Site");
									controller.siteGraph.Connect(_nearestNode, controller.siteGraph.AddNode());
									controller.pointSitePositions.Add(mousePosition);
									EditorApplication.delayCall += () =>
									{
										RebuildVoronoiDiagram(controller.siteGraph, controller.pointSitePositions, out controller.contoursEnabled);
										RebuildContourMesh(controller.siteGraph, controller.pointSitePositions, controller.contoursEnabled);
										EditorUtility.SetDirty(controller);
									};
									GUI.changed = true;
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

							PaintVoronoiDiagram();
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

					PaintVoronoiDiagram();
				}
			}
		}

		private void ApplyChangeAndRebuildImmediateContourMesh(ContourTriangulatorController controller, Vector2 mousePosition)
		{
			switch (_handleState)
			{
				case HandleState.MovingNode:
					controller.siteGraph.CopyTo(_mouseDragSiteGraph);
					controller.pointSitePositions.CopyTo(_mouseDragPointSitePositions);
					_mouseDragPointSitePositions[_mouseDownNode.index] = controller.transform.InverseTransformPoint(_mouseDownNodePosition + (mousePosition - _mouseDownPosition));
					RebuildVoronoiDiagram(_mouseDragSiteGraph, _mouseDragPointSitePositions, out _mouseDragContoursEnabled);
					RebuildContourMesh(_mouseDragSiteGraph, _mouseDragPointSitePositions, _mouseDragContoursEnabled);
					break;
				case HandleState.AddingEdge:
					if (_nearestNode != _mouseDownNode && _nearestNodeDistance <= _nodeLongActiveDistance && !_mouseDownNode.FindEdge(_nearestNode))
					{
						controller.siteGraph.CopyTo(_mouseDragSiteGraph);
						controller.pointSitePositions.CopyTo(_mouseDragPointSitePositions);
						_mouseDragSiteGraph.ConnectNodes(_mouseDownNode.index, _nearestNode.index);
						RebuildVoronoiDiagram(_mouseDragSiteGraph, _mouseDragPointSitePositions, out _mouseDragContoursEnabled);
						RebuildContourMesh(_mouseDragSiteGraph, _mouseDragPointSitePositions, _mouseDragContoursEnabled);
					}
					break;
				case HandleState.SplittingEdge:
					{
						controller.siteGraph.CopyTo(_mouseDragSiteGraph);
						controller.pointSitePositions.CopyTo(_mouseDragPointSitePositions);
						var newNode = _mouseDragSiteGraph.AddNode();
						var sourceNode = _mouseDownEdge.sourceNode;
						var targetNode = _mouseDownEdge.targetNode;
						_mouseDragPointSitePositions.Add(mousePosition);
						_mouseDragSiteGraph.RemoveEdge(_mouseDownEdge.index);
						_mouseDragSiteGraph.ConnectNodes(sourceNode.index, newNode.index);
						_mouseDragSiteGraph.ConnectNodes(newNode.index, targetNode.index);
						RebuildVoronoiDiagram(_mouseDragSiteGraph, _mouseDragPointSitePositions, out _mouseDragContoursEnabled);
						RebuildContourMesh(_mouseDragSiteGraph, _mouseDragPointSitePositions, _mouseDragContoursEnabled);
					}
					break;
				case HandleState.RemovingNode:
					controller.siteGraph.CopyTo(_mouseDragSiteGraph);
					controller.pointSitePositions.CopyTo(_mouseDragPointSitePositions);
					_mouseDragSiteGraph.RemoveNodeAndEdges(_mouseDownNode.index,
						(int toNodeIndex, int fromNodeIndex) =>
						{
							_mouseDragPointSitePositions[toNodeIndex] = _mouseDragPointSitePositions[fromNodeIndex];
							_mouseDragPointSitePositions.RemoveAt(fromNodeIndex);
						});
					RebuildVoronoiDiagram(_mouseDragSiteGraph, _mouseDragPointSitePositions, out _mouseDragContoursEnabled);
					RebuildContourMesh(_mouseDragSiteGraph, _mouseDragPointSitePositions, _mouseDragContoursEnabled);
					break;
				case HandleState.RemovingEdge:
					controller.siteGraph.CopyTo(_mouseDragSiteGraph);
					controller.pointSitePositions.CopyTo(_mouseDragPointSitePositions);
					_mouseDragSiteGraph.RemoveEdge(_mouseDownEdge.index);
					RebuildVoronoiDiagram(_mouseDragSiteGraph, _mouseDragPointSitePositions, out _mouseDragContoursEnabled);
					RebuildContourMesh(_mouseDragSiteGraph, _mouseDragPointSitePositions, _mouseDragContoursEnabled);
					break;
				case HandleState.AddingNode:
					controller.siteGraph.CopyTo(_mouseDragSiteGraph);
					controller.pointSitePositions.CopyTo(_mouseDragPointSitePositions);
					_mouseDragSiteGraph.AddNode();
					_mouseDragPointSitePositions.Add(mousePosition);
					RebuildVoronoiDiagram(_mouseDragSiteGraph, _mouseDragPointSitePositions, out _mouseDragContoursEnabled);
					RebuildContourMesh(_mouseDragSiteGraph, _mouseDragPointSitePositions, _mouseDragContoursEnabled);
					break;
				case HandleState.AddingNodeEdge:
					controller.siteGraph.CopyTo(_mouseDragSiteGraph);
					controller.pointSitePositions.CopyTo(_mouseDragPointSitePositions);
					_mouseDragSiteGraph.ConnectNodes(_nearestNode.index, _mouseDragSiteGraph.AddNode().index);
					_mouseDragPointSitePositions.Add(mousePosition);
					RebuildVoronoiDiagram(_mouseDragSiteGraph, _mouseDragPointSitePositions, out _mouseDragContoursEnabled);
					RebuildContourMesh(_mouseDragSiteGraph, _mouseDragPointSitePositions, _mouseDragContoursEnabled);
					break;
				default:
					return;
			}

			RebuildVoronoiDiagram(_mouseDragSiteGraph, _mouseDragPointSitePositions, out _mouseDragContoursEnabled);
			RebuildContourMesh(_mouseDragSiteGraph, _mouseDragPointSitePositions, _mouseDragContoursEnabled);
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
			if (_voronoiDiagram == null || !_showVoronoiDiagram) return;

			if (_voronoiNodeLabelStyle == null)
			{
				_voronoiNodeLabelStyle = new GUIStyle(EditorStyles.boldLabel);
				_voronoiNodeLabelStyle.alignment = TextAnchor.UpperLeft;
				_voronoiNodeLabelStyle.fontSize = 12;
				_voronoiNodeLabelStyle.normal.textColor = Color.white;
				_voronoiNodeLabelStyle.padding.top = 1;
				_voronoiNodeLabelStyle.padding.bottom = 4;
			}

			if (_voronoiEdgeLabelStyle == null)
			{
				_voronoiEdgeLabelStyle = new GUIStyle(EditorStyles.label);
				_voronoiEdgeLabelStyle.alignment = TextAnchor.UpperLeft;
				_voronoiEdgeLabelStyle.fontSize = 10;
				_voronoiEdgeLabelStyle.normal.textColor = Color.white;
				_voronoiEdgeLabelStyle.padding.top = 1;
				_voronoiEdgeLabelStyle.padding.bottom = 4;
			}

			var topology = _voronoiDiagram._voronoiTopology;
			var voronoiEdgeShapes = _voronoiDiagram._voronoiEdgeShapes;
			var voronoiEdgeShapeLowerBounds = _voronoiDiagram._voronoiEdgeShapeLowerBounds;
			var voronoiEdgeShapeUpperBounds = _voronoiDiagram._voronoiEdgeShapeUpperBounds;

			var normal = -Camera.current.transform.forward;

			float voronoiNodeRadius = _showVoronoiLabels ? _voronoiNodeLabeledRadius : _voronoiNodeDefaultRadius;

			foreach (var edge in topology.edges)
			{
				var p0 = _finiteVoronoiNodePositions[edge.sourceNode];
				var p1 = _finiteVoronoiNodePositions[edge.targetNode];
				var s0 = HandleUtility.GetHandleSize(p0);
				var s1 = HandleUtility.GetHandleSize(p1);
				var e0 = s0 * _voronoiEdgeOffset;
				var e1 = s1 * _voronoiEdgeOffset;
				var a0 = s0 * _voronoiEdgeArrowOffset;
				var a1 = s1 * _voronoiEdgeArrowOffset;
				var n0 = s0 * voronoiNodeRadius + e0;
				var n1 = s1 * voronoiNodeRadius + e1;
				var edgeShape = voronoiEdgeShapes[edge];
				var edgeShapeLowerBound = voronoiEdgeShapeLowerBounds[edge];
				var edgeShapeUpperBound = voronoiEdgeShapeUpperBounds[edge];

				if (edgeShape.isFinite)
				{
					Handles.color = _voronoiFiniteEdgeColor;

					if (edgeShape.isStraight)
					{
						var v = (p1 - p0).normalized;
						var u = Vector3.Cross(normal, v);
						Handles.DrawAAPolyLine(_voronoidEdgeWidth, p0 - u * e0 + v * n0, p1 - u * e1 - v * n1, p1 - u * (e1 + a1) - v * (n1 + e1 + a1));

						if (_showVoronoiLabels)
						{
							var midpoint = (p0 + p1) / 2f;
							var label = new GUIContent(edge.index.ToString());
							var labelSize = _voronoiEdgeLabelStyle.CalcSize(label);
							var labelPosition = HandleUtility.WorldToGUIPoint(midpoint - u * HandleUtility.GetHandleSize(midpoint) * _voronoiEdgeLabelOffset);
							Handles.BeginGUI();
							GUI.Label(new Rect(labelPosition - labelSize / 2f, labelSize), label, _voronoiEdgeLabelStyle);
							Handles.EndGUI();
						}
					}
					else if (edgeShape.type == VoronoiEdgeShapeType.Parabola)
					{
						var v0 = edgeShape.GetDirection(edgeShapeLowerBound);
						var u0 = Vector3.Cross(normal, v0);
						var v1 = edgeShape.GetDirection(edgeShapeUpperBound);
						var u1 = Vector3.Cross(normal, v1);

						float angleChange = Mathf.Abs(edgeShape.GetAngleChange(edgeShapeLowerBound, edgeShapeUpperBound));
						int segmentCount = Mathf.CeilToInt(angleChange / _voronoiEdgeMaxAngleChangePerSegment);
						var points = new Vector3[segmentCount + 2];

						points[0] = p0 - u0 * e0 + v0 * n0;
						for (int i = 1; i < segmentCount; ++i)
						{
							float tSegment = edgeShape.GetAngleChangeOffset(edgeShapeLowerBound, (angleChange * i) / segmentCount);
							float tCurve = 1f - 2f * (tSegment - edgeShapeLowerBound) / (edgeShapeUpperBound - edgeShapeLowerBound);
							var pSegment = edgeShape.Evaluate(tSegment);
							var vSegment = edgeShape.GetDirection(tSegment);
							var uSegment = Vector3.Cross(normal, vSegment);
							points[i] = pSegment - uSegment * e0 + vSegment * n0 * tCurve;
						}
						points[segmentCount] = p1 - u1 * e1 - v1 * n1;
						points[segmentCount + 1] = p1 - u1 * (e1 + a1) - v1 * (n1 + e1 + a1);

						Handles.DrawAAPolyLine(_voronoidEdgeWidth, points);

						if (_showVoronoiLabels)
						{
							float tMidpoint = edgeShape.GetAngleChangeOffset(edgeShapeLowerBound, angleChange / 2f);
							var pMidpoint = edgeShape.Evaluate(tMidpoint);
							var vMidpoint = edgeShape.GetDirection(tMidpoint);
							var uMidpoint = Vector3.Cross(normal, vMidpoint);
							var label = new GUIContent(edge.index.ToString());
							var labelSize = _voronoiEdgeLabelStyle.CalcSize(label);
							var labelPosition = HandleUtility.WorldToGUIPoint(pMidpoint - uMidpoint * HandleUtility.GetHandleSize(pMidpoint) * _voronoiEdgeLabelOffset);
							Handles.BeginGUI();
							GUI.Label(new Rect(labelPosition - labelSize / 2f, labelSize), label, _voronoiEdgeLabelStyle);
							Handles.EndGUI();
						}
					}
				}
				else
				{
					var wU = p0 - _voronoiBounds.center;
					var wE = p1 - _voronoiBounds.center;
					var wR = Vector3.Cross(Vector3.back, wU);
					var dot0 = Vector3.Dot(wE.normalized, wU.normalized);
					var dot1 = Vector3.Dot(wE.normalized, wR.normalized);
					var arc = Mathf.Atan2(Vector3.Dot(wE, wR), Vector3.Dot(wE, wU));

					float sign;

					if (_voronoiDiagram._voronoiFaceSiteTypes[edge] == VoronoiSiteType.None)
					{
						arc = Mathf.Repeat(arc, Mathf.PI * 2f);
						sign = +1f;
					}
					else
					{
						arc = Mathf.PI * 2f - Mathf.Repeat(arc, Mathf.PI * 2f);
						sign = -1f;
					}

					float radius = _voronoiBounds.radius + (e0 + e1) / 2f * sign;
					float arrowRadius = radius + a1 * sign;
					wU = wU.WithMagnitude(radius);
					wR = wR.WithMagnitude(radius) * sign;
					float arcEndOffset0 = n0 / radius;
					float arcEndOffset1 = n1 / radius;
					float arcArrowOffset = a1 / radius;

					float angleChange = Mathf.Repeat(arc, Mathf.PI * 2f) - (arcEndOffset0 + arcEndOffset1);
					if (angleChange > 0f)
					{
						int segmentCount = Mathf.CeilToInt(angleChange / _voronoiEdgeMaxAngleChangePerSegment);
						var points = new Vector3[segmentCount + 2];

						points[0] = _voronoiBounds.center + wU * Mathf.Cos(arcEndOffset0) + wR * Mathf.Sin(arcEndOffset0);
						for (int i = 1; i < segmentCount; ++i)
						{
							float angleSegment = arcEndOffset0 + angleChange * i / segmentCount;
							points[i] = _voronoiBounds.center + wU * Mathf.Cos(angleSegment) + wR * Mathf.Sin(angleSegment);
						}
						points[segmentCount] = _voronoiBounds.center + wU * Mathf.Cos(arc - arcEndOffset1) + wR * Mathf.Sin(arc - arcEndOffset1);
						points[segmentCount + 1] = _voronoiBounds.center + (wU * Mathf.Cos(arc - arcEndOffset1 - arcArrowOffset) + wR * Mathf.Sin(arc - arcEndOffset1 - arcArrowOffset)).WithMagnitude(arrowRadius);

						Handles.color = _voronoiInfiniteEdgeColor;
						Handles.DrawAAPolyLine(_voronoidEdgeWidth, points);
					}

					if (_showVoronoiLabels)
					{
						float angleMidpoint = arcEndOffset0 + angleChange / 2f;
						var uMidpoint = (wU * Mathf.Cos(angleMidpoint) + wR * Mathf.Sin(angleMidpoint)).normalized;
						var pMidpoint = _voronoiBounds.center + uMidpoint * _voronoiBounds.radius;
						var label = new GUIContent(edge.index.ToString());
						var labelSize = _voronoiEdgeLabelStyle.CalcSize(label);
						var labelPosition = HandleUtility.WorldToGUIPoint(pMidpoint + uMidpoint * HandleUtility.GetHandleSize(pMidpoint) * _voronoiEdgeLabelOffset * sign);
						Handles.BeginGUI();
						GUI.Label(new Rect(labelPosition - labelSize / 2f, labelSize), label, _voronoiEdgeLabelStyle);
						Handles.EndGUI();
					}
				}
			}

			if (_showVoronoiLabels)
			{
				foreach (var node in topology.nodes)
				{
					var p = _finiteVoronoiNodePositions[node];
					Handles.color = _voronoiNodeColor;
					Handles.DrawSolidDisc(p, Vector3.back, HandleUtility.GetHandleSize(p) * _voronoiNodeLabeledRadius);
					var label = new GUIContent(node.index.ToString());
					var labelSize = _voronoiNodeLabelStyle.CalcSize(label);
					var labelPosition = HandleUtility.WorldToGUIPoint(p);
					Handles.BeginGUI();
					GUI.Label(new Rect(labelPosition - labelSize / 2f, labelSize), label, _voronoiNodeLabelStyle);
					Handles.EndGUI();
				}
			}
			else
			{
				foreach (var node in topology.nodes)
				{
					var p = _finiteVoronoiNodePositions[node];
					Handles.color = Color.gray;
					Handles.DrawSolidDisc(p, Vector3.back, HandleUtility.GetHandleSize(p) * _voronoiNodeDefaultRadius);
				}
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

		private void RebuildVoronoiDiagram(IGraph siteGraph, IGraphNodeData<Vector3> pointSitePositions, out bool[] contoursEnabled)
		{
			var controller = (ContourTriangulatorController)target;
			contoursEnabled = controller.contoursEnabled;

			var stopwatch = new System.Diagnostics.Stopwatch();

			if (siteGraph.nodeCount > 0)
			{
				stopwatch.Start();
				try
				{
					_voronoiGenerator.SetSites(siteGraph, pointSitePositions, Vector3.zero, Vector3.right, Vector3.up);
					_voronoiDiagram = _voronoiGenerator.Generate();
					FinitizeVoronoiNodePositions();
				}
				catch (Exception ex)
				{
					Debug.LogErrorFormat("Exception ({0}) while generating voronoi diagram:  \"{1}\"\n{2}", ex.GetType().Name, ex.Message, ex.StackTrace);
					_voronoiDiagram = null;
				}

				try
				{
					_contourLoops = _voronoiDiagram.FindAllContourLoops(controller.contourOffsets[0] * controller.contourDistanceScale, _contourLoops);

					if (controller.contoursEnabled == null || _contourLoops.Count != controller.contoursEnabled.Length)
					{
						var oldContoursEnabled = controller.contoursEnabled;
						var newContoursEnabled = new bool[_contourLoops.Count];

						if (oldContoursEnabled != null && oldContoursEnabled.Length > 0)
						{
							Array.Copy(oldContoursEnabled, newContoursEnabled, System.Math.Min(oldContoursEnabled.Length, newContoursEnabled.Length));
						}
						else
						{
							for (int i = 0; i < newContoursEnabled.Length; ++i)
							{
								newContoursEnabled[i] = true;
							}
						}

						contoursEnabled = newContoursEnabled;
					}
				}
				catch (Exception ex)
				{
					Debug.LogErrorFormat("Exception ({0}) while generating contour loops:  \"{1}\"\n{2}", ex.GetType().Name, ex.Message, ex.StackTrace);
				}
				stopwatch.Stop();

				//Debug.LogFormat("Time to generate voronoi diagram:  {0:F6} ms", 1000d * stopwatch.ElapsedTicks / System.Diagnostics.Stopwatch.Frequency);
			}
		}

		private void RebuildContourMesh(IGraph siteGraph, IGraphNodeData<Vector3> pointSitePositions, bool[] contoursEnabled)
		{
			var controller = (ContourTriangulatorController)target;

			var meshFilter = controller.GetComponent<MeshFilter>();
			var mesh = meshFilter.sharedMesh;
			if (mesh == null)
			{
				mesh = meshFilter.sharedMesh = new Mesh();
			}
			mesh.Clear();

			if (siteGraph.nodeCount > 0 && _voronoiDiagram != null && _voronoiDiagram._siteEdgeFirstVoronoiEdgeIndices.Count > 0 && controller.GetComponent<MeshRenderer>().enabled)
			{
				var vertexIndexMap = new Dictionary<ContourTriangulator.PositionId, int>();
				var vertexPositions = new List<Vector3>();
				var vertexColors = new List<Color>();
				var triangleIndices = new List<int>();

				var contourColors = controller.contourColors;
				var contourDistances = new float[controller.contourOffsets.Length];
				float cumulativeDistance = 0f;
				for (int i = 0; i < contourDistances.Length; ++i)
				{
					cumulativeDistance += controller.contourOffsets[i] * controller.contourDistanceScale;
					contourDistances[i] = cumulativeDistance;
				}

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
						int i0 = vertexIndexMap[positionId0];
						int i1 = vertexIndexMap[positionId1];
						int i2 = vertexIndexMap[positionId2];
						var p0 = vertexPositions[i0];
						var p1 = vertexPositions[i1];
						var p2 = vertexPositions[i2];
						triangleIndices.Add(vertexIndexMap[positionId0]);
						triangleIndices.Add(vertexIndexMap[positionId1]);
						triangleIndices.Add(vertexIndexMap[positionId2]);
					//Debug.LogFormat("Triangle({0}, {1}, {2}):  {1}, {2}, {3}", vertexIndexMap[positionId0], vertexIndexMap[positionId1], vertexIndexMap[positionId2], vertexPositions[vertexIndexMap[positionId0]].ToString("F2"), vertexPositions[vertexIndexMap[positionId1]].ToString("F2"), vertexPositions[vertexIndexMap[positionId2]].ToString("F2"));
				};

				if (_voronoiDiagram != null && _voronoiDiagram._siteEdgeFirstVoronoiEdgeIndices.Count > 0 && controller.GetComponent<MeshRenderer>().enabled)
				{
					var edge = new TopologyEdge(_voronoiDiagram._voronoiTopology, _voronoiDiagram._siteEdgeFirstVoronoiEdgeIndices[0]);
					if (controller.twinEdge) edge = edge.twin;
					_contourTriangulator.maxAngleChangePerSegment = controller.maxCurvaturePerSegment;

					var stopwatch = new System.Diagnostics.Stopwatch();

					stopwatch.Start();

					try
					{
						for (int i = 0; i < controller.contoursEnabled.Length; ++i)
						{
							if (contoursEnabled[i])
							{
								_contourTriangulator.Triangulate(_voronoiDiagram, onVertex, onTriangle, Vector3.back, _contourLoops[i], contourDistances);
							}
						}
					}
					catch (Exception ex)
					{
						Debug.LogErrorFormat("Exception ({0}) while triangulating contour:  \"{1}\"\n{2}", ex.GetType().Name, ex.Message, ex.StackTrace);
					}

					stopwatch.Stop();

					//Debug.LogFormat("Time to triangulate contour:  {0:F6} ms", 1000d * stopwatch.ElapsedTicks / System.Diagnostics.Stopwatch.Frequency);

					mesh.SetVertices(vertexPositions);
					mesh.SetColors(vertexColors);
					mesh.SetIndices(triangleIndices.ToArray(), MeshTopology.Triangles, 0);
				}
			}

			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.UploadMeshData(true);
		}
	}
}
