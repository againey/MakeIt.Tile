/******************************************************************************\
* Copyright Andy Gainey                                                        *
*                                                                              *
* Licensed under the Apache License, Version 2.0 (the "License");              *
* you may not use this file except in compliance with the License.             *
* You may obtain a copy of the License at                                      *
*                                                                              *
*     http://www.apache.org/licenses/LICENSE-2.0                               *
*                                                                              *
* Unless required by applicable law or agreed to in writing, software          *
* distributed under the License is distributed on an "AS IS" BASIS,            *
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.     *
* See the License for the specific language governing permissions and          *
* limitations under the License.                                               *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MakeIt.Generate;

namespace MakeIt.Tile
{
	[Generator(typeof(TopologyGeneratorCollection), "Utility/Face Spatial Partitioning")]
	public class FaceSpatialPartitioningGenerator : Generator, ISerializationCallbackReceiver
	{
		[AutoSelect] public InputSlot surfaceInputSlot;
		[AutoSelect] public InputSlot topologyInputSlot;
		public InputSlot vertexPositionsInputSlot;

		public OutputSlot partitioningOutputSlot;

		protected override void Initialize()
		{
			// Inputs
			InputSlot.CreateOrResetRequired<Surface>(ref surfaceInputSlot, this);
			InputSlot.CreateOrResetRequired<Topology>(ref topologyInputSlot, this);
			InputSlot.CreateOrResetRequired<IVertexAttribute<Vector3>>(ref vertexPositionsInputSlot, this);

			// Outputs
			OutputSlot.CreateOrReset<UniversalFaceSpatialPartitioning>(ref partitioningOutputSlot, this, "Face Spatial Partitioning");
		}

		public void OnAfterDeserialize()
		{
			InputSlot.ResetAssetTypeIfNull<Surface>(surfaceInputSlot);
			InputSlot.ResetAssetTypeIfNull<Topology>(topologyInputSlot);
			InputSlot.ResetAssetTypeIfNull<IVertexAttribute<Vector3>>(vertexPositionsInputSlot);

			OutputSlot.ResetAssetTypeIfNull<UniversalFaceSpatialPartitioning>(partitioningOutputSlot);
		}

		public void OnBeforeSerialize()
		{
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
				surfaceInputSlot.GetAsset<Surface>(),
				topologyInputSlot.GetAsset<Topology>(),
				vertexPositionsInputSlot.GetAsset<IVertexAttribute<Vector3>>());
			partitioningOutputSlot.SetAsset(partitioning);
			yield break;
		}
	}
}
