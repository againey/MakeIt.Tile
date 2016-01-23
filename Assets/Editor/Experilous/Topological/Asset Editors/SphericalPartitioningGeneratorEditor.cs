using UnityEngine;
using UnityEditor;

namespace Experilous.Topological
{
	[CustomEditor(typeof(SphericalPartitioningGenerator))]
	public class SphericalPartitioningGeneratorEditor : AssetGeneratorEditor
	{
		protected override void OnPropertiesGUI()
		{
			var generator = (SphericalPartitioningGenerator)target;

			generator.topology = OnDependencyGUI("Topology", generator.topology, typeof(Topology), true);
			generator.vertexPositions = OnDependencyGUI("Vertex Positions", generator.vertexPositions, typeof(IVertexAttribute<Vector3>), false);
		}
	}
}
