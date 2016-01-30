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

			OnDependencyGUI("Topology", generator.topologyInputSlot, true);
			OnDependencyGUI("Vertex Positions", generator.vertexPositionsInputSlot);
			OnDependencyGUI("Attribute Adapter", generator.positionalAttributeAdapterInputSlot, true);
		}
	}
}
