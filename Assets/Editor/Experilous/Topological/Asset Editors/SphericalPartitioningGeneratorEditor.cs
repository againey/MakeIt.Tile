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

			OnDependencyGUI("Topology", generator.topologyInputSlot, true);
			OnDependencyGUI("Vertex Positions", generator.vertexPositionsInputSlot);
		}
	}
}
