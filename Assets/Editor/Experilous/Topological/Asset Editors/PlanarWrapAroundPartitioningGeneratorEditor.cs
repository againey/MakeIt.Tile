using UnityEditor;

namespace Experilous.Topological
{
#if false
	[CustomEditor(typeof(PlanarWrapAroundPartitioningGenerator))]
	public class PlanarWrapAroundPartitioningGeneratorEditor : AssetGeneratorEditor
	{
		protected override void OnPropertiesGUI()
		{
			var generator = (PlanarWrapAroundPartitioningGenerator)target;

			generator.topology = OnDependencyGUI("Topology", generator.topology, true);
			generator.vertexPositions = OnDependencyGUI("Vertex Positions", generator.vertexPositions, false);
		}
	}
#endif
}
