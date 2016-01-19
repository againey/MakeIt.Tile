using UnityEngine;
using UnityEditor;

namespace Experilous.Topological
{
	[CustomEditor(typeof(FaceCentroidsGenerator))]
	public class FaceCentroidsGeneratorEditor : AssetGeneratorEditor
	{
		protected override void OnPropertiesGUI()
		{
			var generator = (FaceCentroidsGenerator)target;

			generator.surfaceType = (FaceCentroidsGenerator.SurfaceType)EditorGUILayout.EnumPopup("Surface Type", generator.surfaceType);

			generator.topology = OnDependencyGUI("Topology", generator.topology, true);
			generator.vertexPositions = OnDependencyGUI("Vertex Positions", generator.vertexPositions, false,
				(GeneratedAsset asset) =>
				{
					return
						typeof(IVertexAttribute<Vector3>).IsAssignableFrom(asset.generatedType) ||
						typeof(IEdgeAttribute<Vector3>).IsAssignableFrom(asset.generatedType);
				});
		}
	}
}
