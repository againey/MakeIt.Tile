using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorCollection), typeof(UtilitiesCategory), "Spherical Partitioning")]
	public class SphericalPartitioningGenerator : AssetGenerator
	{
		public AssetInputSlot topologyInputSlot;
		public AssetInputSlot vertexPositionsInputSlot;

		public AssetDescriptor partitioningDescriptor;

		protected override void Initialize(bool reset = true)
		{
			// Inputs
			if (reset || topologyInputSlot == null) topologyInputSlot = AssetInputSlot.CreateRequired(this, typeof(Topology));
			if (reset || vertexPositionsInputSlot == null) vertexPositionsInputSlot = AssetInputSlot.CreateRequired(this, typeof(IVertexAttribute<Vector3>));

			// Outputs
			if (reset || partitioningDescriptor == null) partitioningDescriptor = AssetDescriptor.Create<SphericalPartitioning>(this, "Partitioning");
		}

		public override IEnumerable<AssetInputSlot> inputs
		{
			get
			{
				yield return topologyInputSlot;
				yield return vertexPositionsInputSlot;
			}
		}

		public override IEnumerable<AssetDescriptor> outputs
		{
			get
			{
				yield return partitioningDescriptor;
			}
		}

		public override IEnumerable<AssetReferenceDescriptor> references
		{
			get
			{
				if (topologyInputSlot.source != null) yield return topologyInputSlot.source.ReferencedBy(partitioningDescriptor);
			}
		}

		public override void Generate()
		{
			var partitioning = SphericalPartitioning.CreateInstance(topologyInputSlot.GetAsset<Topology>(), vertexPositionsInputSlot.GetAsset<IVertexAttribute<Vector3>>());
			partitioningDescriptor.SetAsset(partitioning);
		}
	}
}
