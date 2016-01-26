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

			generator.topology = OnDependencyGUI("Topology", generator.topology, typeof(Topology), true);
			generator.vertexPositions = OnDependencyGUI("Vertex Positions", generator.vertexPositions, typeof(IVertexAttribute<Vector3>), false);

			if (generator.wrappedFaceCentroids.isUsed)
			{
				generator.surfaceDescriptor = OnDependencyGUI("Surface Descriptor", generator.surfaceDescriptor, typeof(PlanarSurfaceDescriptor), false);
				generator.edgeWrapData = OnDependencyGUI("Edge Wrap Data", generator.edgeWrapData, typeof(IEdgeAttribute<EdgeWrap>), false);
			}
		}
	}
}
