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

			generator.topologyDescriptor = OnDependencyGUI("Topology", generator.topologyDescriptor, typeof(Topology), true);
			generator.vertexPositionsDescriptor = OnDependencyGUI("Vertex Positions", generator.vertexPositionsDescriptor, typeof(IVertexAttribute<Vector3>), false);
			generator.positionalAttributeAdapterDescriptor = OnOptionalDependencyGUI("Attribute Adapter", generator.positionalAttributeAdapterDescriptor, typeof(PositionalAttributeAdapter), true, true);
		}
	}
}
