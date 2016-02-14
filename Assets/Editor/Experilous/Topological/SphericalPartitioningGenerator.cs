using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Experilous.Generation;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorCollection), typeof(UtilitiesCategory), "Spherical Partitioning")]
	public class SphericalPartitioningGenerator : Generator
	{
		[AutoSelect] public InputSlot topologyInputSlot;
		public InputSlot vertexPositionsInputSlot;

		public OutputSlot partitioningDescriptor;

		protected override void Initialize()
		{
			// Inputs
			InputSlot.CreateOrResetRequired<Topology>(ref topologyInputSlot, this);
			InputSlot.CreateOrResetRequired<IVertexAttribute<Vector3>>(ref vertexPositionsInputSlot, this);

			// Outputs
			OutputSlot.CreateOrReset<SphericalPartitioning>(ref partitioningDescriptor, this, "Partitioning");
		}

		public override IEnumerable<InputSlot> inputs
		{
			get
			{
				yield return topologyInputSlot;
				yield return vertexPositionsInputSlot;
			}
		}

		public override IEnumerable<OutputSlot> outputs
		{
			get
			{
				yield return partitioningDescriptor;
			}
		}

		public override IEnumerable<InternalSlotConnection> internalConnections
		{
			get
			{
				if (topologyInputSlot.source != null) yield return partitioningDescriptor.Uses(topologyInputSlot.source);
			}
		}

		public override IEnumerator BeginGeneration()
		{
			var partitioning = SphericalPartitioning.CreateInstance(topologyInputSlot.GetAsset<Topology>(), vertexPositionsInputSlot.GetAsset<IVertexAttribute<Vector3>>());
			partitioningDescriptor.SetAsset(partitioning);
			yield break;
		}
	}
}
