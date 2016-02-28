/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Experilous.Generation;

namespace Experilous.Topological
{
	[Generator(typeof(TopologyGeneratorCollection), "Utilities/Face Spatial Partitioning")]
	public class FaceSpatialPartitioningGenerator : Generator
	{
		[AutoSelect] public InputSlot topologyInputSlot;
		[AutoSelect] public InputSlot surfaceInputSlot;
		public InputSlot vertexPositionsInputSlot;

		public OutputSlot partitioningOutputSlot;

		protected override void Initialize()
		{
			// Inputs
			InputSlot.CreateOrResetRequired<Topology>(ref topologyInputSlot, this);
			InputSlot.CreateOrResetRequired<Surface>(ref surfaceInputSlot, this);
			InputSlot.CreateOrResetRequired<IVertexAttribute<Vector3>>(ref vertexPositionsInputSlot, this);

			// Outputs
			OutputSlot.CreateOrReset<UniversalFaceSpatialPartitioning>(ref partitioningOutputSlot, this, "Face Spatial Partitioning");
		}

		public override IEnumerable<InputSlot> inputs
		{
			get
			{
				yield return topologyInputSlot;
				yield return surfaceInputSlot;
				yield return vertexPositionsInputSlot;
			}
		}

		public override IEnumerable<OutputSlot> outputs
		{
			get
			{
				yield return partitioningOutputSlot;
			}
		}

		public override IEnumerable<InternalSlotConnection> internalConnections
		{
			get
			{
				if (topologyInputSlot.source != null) yield return partitioningOutputSlot.Uses(topologyInputSlot.source);
				if (surfaceInputSlot.source != null) yield return partitioningOutputSlot.Uses(surfaceInputSlot.source);
			}
		}

		public override IEnumerator BeginGeneration()
		{
			var partitioning = UniversalFaceSpatialPartitioning.Create(
				topologyInputSlot.GetAsset<Topology>(),
				surfaceInputSlot.GetAsset<Surface>(),
				vertexPositionsInputSlot.GetAsset<IVertexAttribute<Vector3>>());
			partitioningOutputSlot.SetAsset(partitioning);
			yield break;
		}
	}
}
