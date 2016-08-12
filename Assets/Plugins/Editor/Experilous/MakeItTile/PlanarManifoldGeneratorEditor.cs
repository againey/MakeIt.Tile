/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using UnityEditor;
using Experilous.Generation;

namespace Experilous.Topological
{
	[CustomEditor(typeof(PlanarManifoldGenerator))]
	public class PlanarManifoldGeneratorEditor : GeneratorEditor
	{
		public enum RestrictedHorizontalAxisOptions
		{
			LeftToRight,
			RightToLeft,
		}

		public enum RestrictedVerticalAxisOptions
		{
			BottomToTop,
			TopToBottom,
			NearToFar,
			FarToNear,
		}

		protected override void OnPropertiesGUI()
		{
			var generator = (PlanarManifoldGenerator)target;

			generator.planarTileShape = (PlanarManifoldGenerator.PlanarTileShapes)EditorGUILayout.EnumPopup("Tile Shape", generator.planarTileShape);

			EditorGUILayout.PropertyField(_serializedGenerator.FindProperty("size"), new GUIContent("Size"));

			switch (generator.planarTileShape)
			{
				case PlanarManifoldGenerator.PlanarTileShapes.Quadrilateral:
					generator.quadGridHorizontalAxisOptions = (PlanarManifoldGenerator.HorizontalAxisOptions)EditorGUILayout.EnumPopup("Horizontal Axis Style", generator.quadGridHorizontalAxisOptions);
					switch (generator.quadGridHorizontalAxisOptions)
					{
						case PlanarManifoldGenerator.HorizontalAxisOptions.LeftToRight:
						case PlanarManifoldGenerator.HorizontalAxisOptions.RightToLeft:
							generator.horizontalAxisLength = EditorGUILayout.FloatField("Horizontal Axis Length", generator.horizontalAxisLength);
							break;
						case PlanarManifoldGenerator.HorizontalAxisOptions.Custom:
							generator.horizontalAxis = EditorGUILayout.Vector3Field("Horizontal Axis Length", generator.horizontalAxis);
							break;
						default: throw new System.NotImplementedException();
					}

					generator.quadGridVerticalAxisOptions = (PlanarManifoldGenerator.VerticalAxisOptions)EditorGUILayout.EnumPopup("Vertical Axis Style", generator.quadGridVerticalAxisOptions);
					switch (generator.quadGridVerticalAxisOptions)
					{
						case PlanarManifoldGenerator.VerticalAxisOptions.BottomToTop:
						case PlanarManifoldGenerator.VerticalAxisOptions.TopToBottom:
						case PlanarManifoldGenerator.VerticalAxisOptions.NearToFar:
						case PlanarManifoldGenerator.VerticalAxisOptions.FarToNear:
							generator.verticalAxisLength = EditorGUILayout.FloatField("Vertical Axis Length", generator.verticalAxisLength);
							break;
						case PlanarManifoldGenerator.VerticalAxisOptions.Custom:
							generator.verticalAxis = EditorGUILayout.Vector3Field("Vertical Axis Length", generator.verticalAxis);
							break;
						default: throw new System.NotImplementedException();
					}

					break;

				case PlanarManifoldGenerator.PlanarTileShapes.Hexagonal:
					generator.hexGridAxisStyleOptions = (PlanarManifoldGenerator.HexGridAxisStyleOptions)EditorGUILayout.EnumPopup("Axis Relation", generator.hexGridAxisStyleOptions);
					switch (generator.hexGridAxisStyleOptions)
					{
						case PlanarManifoldGenerator.HexGridAxisStyleOptions.ObliqueAcute:
						case PlanarManifoldGenerator.HexGridAxisStyleOptions.ObliqueObtuse:
							generator.hexGridAxisOptions = (PlanarManifoldGenerator.HexGridAxisOptions)EditorGUILayout.EnumPopup("Axis Orientation", generator.hexGridAxisOptions);
							switch (generator.hexGridAxisOptions)
							{
								case PlanarManifoldGenerator.HexGridAxisOptions.AngledVerticalAxis:
								case PlanarManifoldGenerator.HexGridAxisOptions.AngledHorizontalAxis:
								case PlanarManifoldGenerator.HexGridAxisOptions.RightAngleAxes:
									generator.hexGridHorizontalAxisOptions = (PlanarManifoldGenerator.HorizontalAxisOptions)EditorGUILayout.EnumPopup("Horizontal Axis Style", (RestrictedHorizontalAxisOptions)generator.hexGridHorizontalAxisOptions);
									generator.horizontalAxisLength = EditorGUILayout.FloatField("Horizontal Axis Length", generator.horizontalAxisLength);
									generator.hexGridVerticalAxisOptions = (PlanarManifoldGenerator.VerticalAxisOptions)EditorGUILayout.EnumPopup("Vertical Axis Style", (RestrictedVerticalAxisOptions)generator.hexGridVerticalAxisOptions);
									generator.verticalAxisLength = EditorGUILayout.FloatField("Vertical Axis Length", generator.verticalAxisLength);
									break;
								case PlanarManifoldGenerator.HexGridAxisOptions.Custom:
									generator.horizontalAxis = EditorGUILayout.Vector3Field("Horizontal Axis Length", generator.horizontalAxis);
									generator.verticalAxis = EditorGUILayout.Vector3Field("Vertical Axis Length", generator.verticalAxis);
									break;
								default: throw new System.NotImplementedException();
							}

							break;

						case PlanarManifoldGenerator.HexGridAxisStyleOptions.StaggeredVerticallyAcute:
						case PlanarManifoldGenerator.HexGridAxisStyleOptions.StaggeredVerticallyObtuse:
						case PlanarManifoldGenerator.HexGridAxisStyleOptions.StaggeredHorizontallyAcute:
						case PlanarManifoldGenerator.HexGridAxisStyleOptions.StaggeredHorizontallyObtuse:
							generator.hexGridHorizontalAxisOptions = (PlanarManifoldGenerator.HorizontalAxisOptions)EditorGUILayout.EnumPopup("Horizontal Axis Style", generator.hexGridHorizontalAxisOptions);
							switch (generator.hexGridHorizontalAxisOptions)
							{
								case PlanarManifoldGenerator.HorizontalAxisOptions.LeftToRight:
								case PlanarManifoldGenerator.HorizontalAxisOptions.RightToLeft:
									generator.horizontalAxisLength = EditorGUILayout.FloatField("Horizontal Axis Length", generator.horizontalAxisLength);
									break;
								case PlanarManifoldGenerator.HorizontalAxisOptions.Custom:
									generator.horizontalAxis = EditorGUILayout.Vector3Field("Horizontal Axis", generator.horizontalAxis);
									break;
								default: throw new System.NotImplementedException();
							}

							generator.hexGridVerticalAxisOptions = (PlanarManifoldGenerator.VerticalAxisOptions)EditorGUILayout.EnumPopup("Vertical Axis Style", generator.hexGridVerticalAxisOptions);
							switch (generator.hexGridVerticalAxisOptions)
							{
								case PlanarManifoldGenerator.VerticalAxisOptions.BottomToTop:
								case PlanarManifoldGenerator.VerticalAxisOptions.TopToBottom:
								case PlanarManifoldGenerator.VerticalAxisOptions.NearToFar:
								case PlanarManifoldGenerator.VerticalAxisOptions.FarToNear:
									generator.verticalAxisLength = EditorGUILayout.FloatField("Vertical Axis Length", generator.verticalAxisLength);
									break;
								case PlanarManifoldGenerator.VerticalAxisOptions.Custom:
									generator.verticalAxis = EditorGUILayout.Vector3Field("Vertical Axis", generator.verticalAxis);
									break;
								default: throw new System.NotImplementedException();
							}

							generator.variableRowLength = EditorGUILayout.Toggle("Variable Row Length", generator.variableRowLength);

							break;

						default: throw new System.NotImplementedException();
					}

					break;

				default: throw new System.NotImplementedException();
			}

			generator.normalOptions = (PlanarManifoldGenerator.NormalOptions)EditorGUILayout.EnumPopup("Surface Normal", generator.normalOptions);
			if (generator.normalOptions == PlanarManifoldGenerator.NormalOptions.Custom)
			{
				generator.normal = EditorGUILayout.Vector3Field("Surface Normal Vector", generator.normal);
			}

			generator.originPosition = EditorGUILayout.Vector3Field("Origin", generator.originPosition);

			generator.wrapOptions = (PlanarManifoldGenerator.WrapOptions)EditorGUILayout.EnumPopup("Edge Wrapping", generator.wrapOptions);
		}
	}
}
