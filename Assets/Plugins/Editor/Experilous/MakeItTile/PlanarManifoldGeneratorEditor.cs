/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using UnityEditor;
using Experilous.MakeItGenerate;

namespace Experilous.MakeItTile
{
	[CustomEditor(typeof(PlanarManifoldGenerator))]
	public class PlanarManifoldGeneratorEditor : GeneratorEditor
	{
		public enum HexShapeOptions
		{
			PointySideUp,
			FlatSideUp,
			Custom,
		}

		public enum RestrictedVerticalAxisOptions
		{
			BottomToTop,
			TopToBottom,
			NearToFar,
			FarToNear,
		}

		[System.NonSerialized] private GUIStyle _previewStyle;

		protected override void OnEnable()
		{
			base.OnEnable();

			if (_previewStyle == null)
			{
				_previewStyle = new GUIStyle();
				_previewStyle.margin = new RectOffset(10, 10, 10, 10);
			}
		}

		protected override void OnPropertiesGUI()
		{
			var generator = (PlanarManifoldGenerator)target;

			EditorGUILayout.PropertyField(_serializedGenerator.FindProperty("size"), new GUIContent("Size"));

			EditorGUILayout.Space();

			generator.tileType = (PlanarManifoldGenerator.TileTypes)EditorGUILayout.EnumPopup("Tile Type", generator.tileType);

			switch (generator.tileType)
			{
				case PlanarManifoldGenerator.TileTypes.Quadrilateral:
				{
					generator.SetQuadTileShape((PlanarManifoldGenerator.QuadTileShapes)EditorGUILayout.EnumPopup("Tile Shape", generator.quadTileShape));

					bool canGenerate = generator.canGenerate;
					RectangularQuadGrid surface = null;
					Topology topology = null;
					Vector3[] vertexPositions = null;

					if (canGenerate && Event.current.type == EventType.Repaint)
					{
						surface = generator.ResetSurface(CreateInstance<RectangularQuadGrid>(), Vector3.zero, Quaternion.identity, new Numerics.IntVector2(3, 3));
						topology = surface.CreateManifold(out vertexPositions);
					}

					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.Space();

						float previewSize = 128f;
						var previewRect = GUILayoutUtility.GetRect(previewSize, previewSize + _previewStyle.margin.top + _previewStyle.margin.bottom, _previewStyle, GUILayout.ExpandWidth(false));
						if (canGenerate && Event.current.type == EventType.Repaint)
						{
							DrawQuadTilePreview(generator, previewRect.min, previewSize);
						}

						EditorGUILayout.Space();

						previewRect = GUILayoutUtility.GetRect(previewSize, previewSize + _previewStyle.margin.top + _previewStyle.margin.bottom, _previewStyle, GUILayout.ExpandWidth(false));
						if (canGenerate && Event.current.type == EventType.Repaint)
						{
							DrawQuadTile3x3Preview(generator, topology, vertexPositions, previewRect.min, previewSize);
						}

						EditorGUILayout.Space();
					}
					EditorGUILayout.EndHorizontal();

					EditorGUI.BeginChangeCheck();
					generator.axis0 = EditorGUILayout.Vector2Field("Axis 0", generator.axis0);
					generator.axis1 = EditorGUILayout.Vector2Field("Axis 1", generator.axis1);
					if (EditorGUI.EndChangeCheck())
					{
						generator.SetQuadTileShape(PlanarManifoldGenerator.QuadTileShapes.Custom);
					}
					break;
				}
				case PlanarManifoldGenerator.TileTypes.Hexagonal:
				{
					generator.SetHexTileShape((PlanarManifoldGenerator.HexTileShapes)EditorGUILayout.EnumPopup("Tile Shape", generator.hexTileShape));

					bool canGenerate = generator.canGenerate;
					RectangularHexGrid surface = null;
					Topology topology = null;
					Vector3[] vertexPositions = null;

					if (canGenerate && Event.current.type == EventType.Repaint)
					{
						surface = generator.ResetSurface(CreateInstance<RectangularHexGrid>(), Vector3.zero, Quaternion.identity, new Numerics.IntVector2(3, 3));
						topology = surface.CreateManifold(out vertexPositions);
					}

					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.Space();

						float previewSize = 128f;
						var previewRect = GUILayoutUtility.GetRect(previewSize, previewSize + _previewStyle.margin.top + _previewStyle.margin.bottom, _previewStyle, GUILayout.ExpandWidth(false));
						if (canGenerate && Event.current.type == EventType.Repaint)
						{
							DrawHexTilePreview(generator, surface, topology, vertexPositions, previewRect.min, previewSize);
						}

						EditorGUILayout.Space();

						previewRect = GUILayoutUtility.GetRect(previewSize, previewSize + _previewStyle.margin.top + _previewStyle.margin.bottom, _previewStyle, GUILayout.ExpandWidth(false));
						if (canGenerate && Event.current.type == EventType.Repaint)
						{
							DrawHexTile3x3Preview(generator, surface, topology, vertexPositions, previewRect.min, previewSize);
						}

						EditorGUILayout.Space();
					}
					EditorGUILayout.EndHorizontal();

					EditorGUI.BeginChangeCheck();
					generator.midpoint = EditorGUILayout.Vector2Field("Midpoint", generator.midpoint);
					generator.majorCorner = EditorGUILayout.Vector2Field("Major Corner", generator.majorCorner);
					generator.minorCorner = EditorGUILayout.Vector2Field("Minor Corner", generator.minorCorner);
					generator.hexGridAxisStyle = (HexGridAxisStyles)EditorGUILayout.EnumPopup("Axis Style", generator.hexGridAxisStyle);
					generator.swapAxes = EditorGUILayout.Toggle("Swap Axes", generator.swapAxes);
					if (EditorGUI.EndChangeCheck())
					{
						generator.SetHexTileShape(PlanarManifoldGenerator.HexTileShapes.Custom);
					}
					break;
				}
				default: throw new System.NotImplementedException();
			}

			generator.isAxis0Wrapped = EditorGUILayout.Toggle("Axis 0 Wraps", generator.isAxis0Wrapped, GUILayout.ExpandWidth(false));
			generator.isAxis1Wrapped = EditorGUILayout.Toggle("Axis 1 Wraps", generator.isAxis1Wrapped, GUILayout.ExpandWidth(false));
			EditorGUILayout.Space();

			generator.origin = EditorGUILayout.Vector3Field("Origin", generator.origin);
			generator.rotation = EditorGUILayout.Vector3Field("Rotation", generator.rotation);
		}

		private static Vector2 InvertY(Vector2 v, float h)
		{
			return new Vector2(v.x, h - v.y);
		}

		private static Color ChangeAlpha(Color c, float a)
		{
			return new Color(c.r, c.g, c.b, a);
		}

		private static Color Average(Color a, Color b)
		{
			return new Color((a.r + b.r) * 0.5f, (a.g + b.g) * 0.5f, (a.b + b.b) * 0.5f, Mathf.Max(a.a, b.a));
		}

		private static Color Embolden(Color c)
		{
			if (c.r < c.g)
			{
				if (c.r < c.b)
				{
					return new Color(0f, c.g + c.r * 0.5f, c.b + c.r * 0.5f, c.a);
				}
				else
				{
					return new Color(c.r + c.b * 0.5f, c.g + c.b * 0.5f, 0f, c.a);
				}
			}
			else
			{
				if (c.g < c.b)
				{
					return new Color(c.r + c.g * 0.5f, 0f, c.b + c.g * 0.5f, c.a);
				}
				else
				{
					return new Color(c.r + c.b * 0.5f, c.g + c.b * 0.5f, 0f, c.a);
				}
			}
		}

		private static Vector3 AdjustPosition(Vector3 position, Vector3 offset, Vector3 root, float height, float scale)
		{
			return root + (Vector3)(InvertY(position + offset, height)) * scale;
		}

		private Color outlineColor { get { return EditorGUIUtility.isProSkin ? new Color(0.75f, 0.75f, 0.75f, 1f) : new Color(0.25f, 0.25f, 0.25f, 1f); } }
		private Color fillColor { get { return EditorGUIUtility.isProSkin ? new Color(0.75f, 0.75f, 0.75f, 0.25f) : new Color(0.25f, 0.25f, 0.25f, 0.25f); } }

		private void DrawQuadTilePreview(PlanarManifoldGenerator generator, Vector3 root, float previewSize)
		{
			Vector2 p0 = Vector2.zero;
			Vector2 p1 = generator.axis1;
			Vector2 p2 = generator.axis0 + generator.axis1;
			Vector2 p3 = generator.axis0;

			Vector2 min = Vector2.Min(Vector2.Min(Vector2.Min(p0, p1), p2), p3);
			Vector2 max = Vector2.Max(Vector2.Max(Vector2.Max(p0, p1), p2), p3);
			float maxRange = Mathf.Max(max.x - min.x, max.y - min.y);
			Vector2 margin = new Vector2(maxRange - (max.x - min.x), maxRange - (max.y - min.y)) * 0.5f;
			float scale = previewSize / maxRange;
			Vector2 offset = margin - min;

			Vector3 v0 = root + (Vector3)(InvertY(p0 + offset, maxRange)) * scale;
			Vector3 v1 = root + (Vector3)(InvertY(p1 + offset, maxRange)) * scale;
			Vector3 v2 = root + (Vector3)(InvertY(p2 + offset, maxRange)) * scale;
			Vector3 v3 = root + (Vector3)(InvertY(p3 + offset, maxRange)) * scale;

			Handles.color = fillColor;
			Handles.DrawAAConvexPolygon(v0, v1, v2, v3);
			Handles.color = outlineColor;
			Handles.DrawAAPolyLine(v0, v1, v2, v3, v0);

			Handles.color = Handles.xAxisColor;
			Handles.DrawAAPolyLine(v0, v3);
			Handles.color = ChangeAlpha(Handles.xAxisColor, 0.25f);
			Handles.DrawSolidDisc(v3, Vector3.back, 5f);
			Handles.color = Handles.xAxisColor;
			Handles.DrawWireDisc(v3, Vector3.back, 5f);

			Handles.color = Handles.yAxisColor;
			Handles.DrawAAPolyLine(v0, v1);
			Handles.color = ChangeAlpha(Handles.yAxisColor, 0.25f);
			Handles.DrawSolidDisc(v1, Vector3.back, 5f);
			Handles.color = Handles.yAxisColor;
			Handles.DrawWireDisc(v1, Vector3.back, 5f);
		}

		private void DrawQuadTile3x3Preview(PlanarManifoldGenerator generator, Topology topology, Vector3[] vertexPositions, Vector3 root, float previewSize)
		{
			var adjustedVertexPositions = new Vector3[vertexPositions.Length].AsVertexAttribute();

			Vector3 min = vertexPositions[0];
			Vector3 max = vertexPositions[0];
			for (int i = 1; i < vertexPositions.Length; ++i)
			{
				min = Vector3.Min(min, vertexPositions[i]);
				max = Vector3.Max(max, vertexPositions[i]);
			}
			Vector3 range = max - min;
			float maxRange = Mathf.Max(range.x, range.y);
			Vector3 margin = (new Vector3(maxRange, maxRange, 0f) - range) * 0.5f;
			Vector3 offset = margin - min;
			float scale = previewSize / maxRange;

			for (int i = 0; i < vertexPositions.Length; ++i)
			{
				adjustedVertexPositions[i] = AdjustPosition(vertexPositions[i], offset, root, maxRange, scale);
			}

			var faceVertexPositions = new Vector3[4];
			Handles.color = fillColor;
			foreach (var face in topology.internalFaces)
			{
				var edge = face.firstEdge;
				for (int i = 0; i < 4; ++i)
				{
					faceVertexPositions[i] = adjustedVertexPositions[edge];
					edge = edge.next;
				}
				Handles.DrawAAConvexPolygon(faceVertexPositions);
			}

			Handles.color = outlineColor;
			foreach (var edge in topology.vertexEdges)
			{
				if (edge.nearVertex < edge.farVertex)
				{
					Handles.DrawAAPolyLine(adjustedVertexPositions[edge.nearVertex], adjustedVertexPositions[edge.farVertex]);
				}
			}

			var p0 = AdjustPosition(Vector3.zero, offset, root, maxRange, scale);
			var a01 = AdjustPosition(generator.axis0 * 1f, offset, root, maxRange, scale);
			var a02 = AdjustPosition(generator.axis0 * 2f, offset, root, maxRange, scale);
			var a03 = AdjustPosition(generator.axis0 * 3f, offset, root, maxRange, scale);
			var a11 = AdjustPosition(generator.axis1 * 1f, offset, root, maxRange, scale);
			var a12 = AdjustPosition(generator.axis1 * 2f, offset, root, maxRange, scale);
			var a13 = AdjustPosition(generator.axis1 * 3f, offset, root, maxRange, scale);

			Handles.color = Handles.xAxisColor;
			Handles.DrawAAPolyLine(p0, a03);
			Handles.DrawSolidDisc(a01, Vector3.back, 3f);
			Handles.DrawSolidDisc(a02, Vector3.back, 3f);
			Handles.DrawSolidDisc(a03, Vector3.back, 3f);

			Handles.color = Handles.yAxisColor;
			Handles.DrawAAPolyLine(p0, a13);
			Handles.DrawSolidDisc(a11, Vector3.back, 3f);
			Handles.DrawSolidDisc(a12, Vector3.back, 3f);
			Handles.DrawSolidDisc(a13, Vector3.back, 3f);
		}

		private void DrawHexTilePreview(PlanarManifoldGenerator generator, RectangularHexGrid surface, Topology topology, Vector3[] vertexPositions, Vector3 root, float previewSize)
		{
			var face = topology.internalFaces[0];
			var adjustedVertexPositions = new Vector3[face.neighborCount].AsVertexAttribute();

			var firstEdge = face.firstEdge;
			var pos = vertexPositions[firstEdge.nextVertex.index];
			Vector3 min = pos;
			Vector3 max = pos;
			var edge = firstEdge.next;
			while (edge != firstEdge)
			{
				pos = vertexPositions[edge.nextVertex.index];
				min = Vector3.Min(min, pos);
				max = Vector3.Max(max, pos);
				edge = edge.next;
			}
			Vector3 range = max - min;
			float maxRange = Mathf.Max(range.x, range.y);
			Vector3 margin = (new Vector3(maxRange, maxRange, 0f) - range) * 0.5f;
			Vector3 offset = margin - min;
			float scale = previewSize / maxRange;

			for (int i = 0; i < face.neighborCount; ++i)
			{
				adjustedVertexPositions[i] = AdjustPosition(vertexPositions[edge.nextVertex.index], offset, root, maxRange, scale);
				edge = edge.next;
			}

			Handles.color = fillColor;
			Handles.DrawAAConvexPolygon(adjustedVertexPositions.array);

			Handles.color = outlineColor;
			Handles.DrawAAPolyLine(adjustedVertexPositions.array);
			Handles.DrawAAPolyLine(adjustedVertexPositions[5], adjustedVertexPositions[0]);

			Vector2 pC = (vertexPositions[firstEdge.nextVertex.index] + vertexPositions[firstEdge.next.next.next.nextVertex.index]) * 0.5f;
			Vector3 vC = AdjustPosition(pC, offset, root, maxRange, scale);
			Vector3 v2 = AdjustPosition(pC + generator.majorCorner, offset, root, maxRange, scale);
			Vector3 v3 = AdjustPosition(pC + generator.minorCorner, offset, root, maxRange, scale);
			Vector3 vM = AdjustPosition(pC + generator.midpoint, offset, root, maxRange, scale);

			var midpointColor = generator.swapAxes ? Handles.yAxisColor : Handles.xAxisColor;
			Handles.color = midpointColor;
			Handles.DrawAAPolyLine(vC, vM);
			Handles.color = ChangeAlpha(midpointColor, 0.25f);
			Handles.DrawSolidDisc(vM, Vector3.back, 5f);
			Handles.color = midpointColor;
			Handles.DrawWireDisc(vM, Vector3.back, 5f);

			Color majorCornerColor;
			if (generator.hexGridAxisStyle == HexGridAxisStyles.Straight)
			{
				majorCornerColor = Embolden(Average(Handles.xAxisColor, Handles.yAxisColor));
			}
			else
			{
				majorCornerColor = generator.swapAxes ? Handles.xAxisColor : Handles.yAxisColor;
			}
			Handles.color = majorCornerColor;
			Handles.DrawAAPolyLine(vC, v2);
			Handles.color = ChangeAlpha(majorCornerColor, 0.25f);
			Handles.DrawSolidDisc(v2, Vector3.back, 5f);
			Handles.color = majorCornerColor;
			Handles.DrawWireDisc(v2, Vector3.back, 5f);

			Color minorCornerColor = Handles.zAxisColor;
			Handles.color = minorCornerColor;
			Handles.DrawAAPolyLine(vC, v3);
			Handles.color = ChangeAlpha(minorCornerColor, 0.25f);
			Handles.DrawSolidDisc(v3, Vector3.back, 5f);
			Handles.color = minorCornerColor;
			Handles.DrawWireDisc(v3, Vector3.back, 5f);

			if (generator.hexGridAxisStyle == HexGridAxisStyles.Straight)
			{
				Handles.color = generator.swapAxes ? Handles.xAxisColor : Handles.yAxisColor;
				Handles.DrawAAPolyLine(vC, (v2 + v3) * 0.5f);
			}
		}

		private void DrawHexTile3x3Preview(PlanarManifoldGenerator generator, RectangularHexGrid surface, Topology topology, Vector3[] vertexPositions, Vector3 root, float previewSize)
		{
			var adjustedVertexPositions = new Vector3[vertexPositions.Length].AsVertexAttribute();

			Vector3 min = vertexPositions[0];
			Vector3 max = vertexPositions[0];
			for (int i = 1; i < vertexPositions.Length; ++i)
			{
				min = Vector3.Min(min, vertexPositions[i]);
				max = Vector3.Max(max, vertexPositions[i]);
			}
			Vector3 range = max - min;
			float maxRange = Mathf.Max(range.x, range.y);
			Vector3 margin = (new Vector3(maxRange, maxRange, 0f) - range) * 0.5f;
			Vector3 offset = margin - min;
			float scale = previewSize / maxRange;

			for (int i = 0; i < vertexPositions.Length; ++i)
			{
				adjustedVertexPositions[i] = AdjustPosition(vertexPositions[i], offset, root, maxRange, scale);
			}

			var faceVertexPositions = new Vector3[6];
			Handles.color = fillColor;
			foreach (var face in topology.internalFaces)
			{
				var edge = face.firstEdge;
				for (int i = 0; i < 6; ++i)
				{
					faceVertexPositions[i] = adjustedVertexPositions[edge];
					edge = edge.next;
				}
				Handles.DrawAAConvexPolygon(faceVertexPositions);
			}

			Handles.color = outlineColor;
			foreach (var edge in topology.vertexEdges)
			{
				if (edge.nearVertex < edge.farVertex)
				{
					Handles.DrawAAPolyLine(adjustedVertexPositions[edge.nearVertex], adjustedVertexPositions[edge.farVertex]);
				}
			}

			var p0 = adjustedVertexPositions[surface.GetVertexIndex(0, 0)];;
			if (!generator.swapAxes)
			{
				var a01 = adjustedVertexPositions[surface.GetVertexIndex(2, 0)];
				var a02 = adjustedVertexPositions[surface.GetVertexIndex(4, 0)];
				var a11 = adjustedVertexPositions[surface.GetVertexIndex(0, 1)];
				var a12 = adjustedVertexPositions[surface.GetVertexIndex(0, 2)];

				Handles.color = Handles.xAxisColor;
				Handles.DrawAAPolyLine(p0, a01, a02);
				Handles.DrawSolidDisc(a01, Vector3.back, 3f);
				Handles.DrawSolidDisc(a02, Vector3.back, 3f);

				if (generator.hexGridAxisStyle != HexGridAxisStyles.Straight)
				{
					Handles.color = ChangeAlpha(Handles.yAxisColor + Color.white * 0.25f, 0.75f);
					Handles.DrawAAPolyLine(p0, a12);
				}
				Handles.color = Handles.yAxisColor;
				Handles.DrawAAPolyLine(p0, a11, a12);
				Handles.DrawSolidDisc(a11, Vector3.back, 3f);
				Handles.DrawSolidDisc(a12, Vector3.back, 3f);
			}
			else
			{
				var a01 = adjustedVertexPositions[surface.GetVertexIndex(1, 0)];
				var a02 = adjustedVertexPositions[surface.GetVertexIndex(2, 0)];
				var a11 = adjustedVertexPositions[surface.GetVertexIndex(0, 2)];
				var a12 = adjustedVertexPositions[surface.GetVertexIndex(0, 4)];

				if (generator.hexGridAxisStyle != HexGridAxisStyles.Straight)
				{
					Handles.color = ChangeAlpha(Handles.xAxisColor + Color.white * 0.25f, 0.75f);
					Handles.DrawAAPolyLine(p0, a02);
				}
				Handles.color = Handles.xAxisColor;
				Handles.DrawAAPolyLine(p0, a01, a02);
				Handles.DrawSolidDisc(a01, Vector3.back, 3f);
				Handles.DrawSolidDisc(a02, Vector3.back, 3f);

				Handles.color = Handles.yAxisColor;
				Handles.DrawAAPolyLine(p0, a11, a12);
				Handles.DrawSolidDisc(a11, Vector3.back, 3f);
				Handles.DrawSolidDisc(a12, Vector3.back, 3f);
			}
		}
	}
}
