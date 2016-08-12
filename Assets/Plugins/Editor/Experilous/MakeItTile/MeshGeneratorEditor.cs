/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using UnityEditor;
using Experilous.MakeIt.Generate;

namespace Experilous.MakeIt.Tile
{
	[CustomEditor(typeof(MeshGenerator))]
	public class MeshGeneratorEditor : GeneratorEditor
	{
		private static string[] vertexAttributeLabels = new string[]
			{
				"Position",
				"Normal",
				"Color (ARGB floats)",
				"Color (ARGB bytes)",
				"UV 1",
				"UV 2",
				"UV 3",
				"UV 4",
				"Tangent",
			};

		[System.NonSerialized] private static GUIStyle _vertexAttributeGroupLabel;

		protected override void OnPropertiesGUI()
		{
			if (_vertexAttributeGroupLabel == null)
			{
				_vertexAttributeGroupLabel = new GUIStyle(EditorStyles.boldLabel);
				_vertexAttributeGroupLabel.margin = EditorStyles.label.margin;
				_vertexAttributeGroupLabel.padding = EditorStyles.label.padding;
			}

			var generator = (MeshGenerator)target;

			var sourceType = generator.sourceType;

			generator.sourceType = (MeshGenerator.SourceType)EditorGUILayout.EnumPopup("Source Type", generator.sourceType);

			switch (sourceType)
			{
				case MeshGenerator.SourceType.InternalFaces:
					break;
				case MeshGenerator.SourceType.FaceGroupCollection:
					InputSlotEditor.OnInspectorGUI(_serializedGenerator.FindProperty("faceGroupCollectionInputSlot"));
					break;
				case MeshGenerator.SourceType.FaceGroup:
					InputSlotEditor.OnInspectorGUI(_serializedGenerator.FindProperty("faceGroupInputSlot"));
					break;
				default:
					throw new System.NotImplementedException();
			}

			generator.triangulation = (MeshGenerator.Triangulation)EditorGUILayout.EnumPopup("Triangulation", generator.triangulation);
			generator.ringDepth = EditorGUILayout.IntSlider("Ring Depth", generator.ringDepth, 1, 4);
			generator.maxVerticesPerSubmesh = EditorGUILayout.IntSlider("Max Vertices Per Submesh", generator.maxVerticesPerSubmesh, 256, 65534);
			generator.vertexAttributes = (DynamicMesh.VertexAttributes)EditorGUILayout.MaskField("Vertex Attributes", (int)generator.vertexAttributes, vertexAttributeLabels);

			InputSlotEditor.OnInspectorGUI(_serializedGenerator.FindProperty("topologyInputSlot"));

			generator.UpdateVertexAttributeInputSlots();

			ShowVertexAttributeUI<Vector3>(new GUIContent("Positions"), generator.ringVertexPositionsInputSlots, generator.centerVertexPositionsInputSlots);
			ShowVertexAttributeUI<Vector3>(new GUIContent("Normals"), generator.ringVertexNormalsInputSlots, generator.centerVertexNormalsInputSlots);
			ShowVertexAttributeUI<Color>(new GUIContent("Colors"), generator.ringVertexColorsInputSlots, generator.centerVertexColorsInputSlots);
			ShowVertexAttributeUI<Color32>(new GUIContent("Color32s"), generator.ringVertexColor32sInputSlots, generator.centerVertexColor32sInputSlots);
			ShowVertexAttributeUI<Vector2>(new GUIContent("UV1s"), generator.ringVertexUV1sInputSlots, generator.centerVertexUV1sInputSlots);
			ShowVertexAttributeUI<Vector2>(new GUIContent("UV2s"), generator.ringVertexUV2sInputSlots, generator.centerVertexUV2sInputSlots);
			ShowVertexAttributeUI<Vector2>(new GUIContent("UV3s"), generator.ringVertexUV3sInputSlots, generator.centerVertexUV3sInputSlots);
			ShowVertexAttributeUI<Vector2>(new GUIContent("UV4s"), generator.ringVertexUV4sInputSlots, generator.centerVertexUV4sInputSlots);
			ShowVertexAttributeUI<Vector4>(new GUIContent("Tangents"), generator.ringVertexTangentsInputSlots, generator.centerVertexTangentsInputSlots);

			generator.showMeshOutputs = EditorGUILayout.Toggle("Show Mesh Outputs", generator.showMeshOutputs);
		}

		private void ShowVertexAttributeUI<TAttributeValue>(GUIContent label, InputSlot[] ringInputSlots, InputSlot[] centerInputSlots)
		{
			if (ringInputSlots.Length > 0)
			{
				if (ringInputSlots.Length + centerInputSlots.Length > 1)
				{
					GUILayout.Label(label, _vertexAttributeGroupLabel);
					EditorGUI.indentLevel += 1;
					for (int i = 0; i < ringInputSlots.Length; ++i)
					{
						InputSlotEditor.OnInspectorGUI(new GUIContent(string.Format("Ring {0}", i + 1)), ringInputSlots[i]);
					}
					if (centerInputSlots.Length > 0)
					{
						InputSlotEditor.OnInspectorGUI(new GUIContent("Center"), centerInputSlots[0]);
					}
					EditorGUI.indentLevel -= 1;
				}
				else if (ringInputSlots.Length == 1)
				{
					InputSlotEditor.OnInspectorGUI(label, ringInputSlots[0]);
				}
			}
		}
	}
}
