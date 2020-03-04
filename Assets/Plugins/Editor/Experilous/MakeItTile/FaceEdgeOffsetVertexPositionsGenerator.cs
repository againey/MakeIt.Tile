/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Experilous.MakeItGenerate;

namespace Experilous.MakeItTile
{
	[Generator(typeof(TopologyGeneratorCollection), "Face Edge/Offset Vertex Positions")]
	public class FaceEdgeOffsetVertexPositionsGenerator : Generator, ISerializationCallbackReceiver
	{
		[AutoSelect] public InputSlot topologyInputSlot;
		public InputSlot vertexPositionsInputSlot;
		public InputSlot offsetsInputSlot;

		public float scale;

		public OutputSlot offsetVertexPositionsOutputSlot;

		protected override void Initialize()
		{
			// Inputs
			InputSlot.CreateOrResetRequired<Topology>(ref topologyInputSlot, this);
			InputSlot.CreateOrResetRequired<IEdgeAttribute<Vector3>>(ref vertexPositionsInputSlot, this);
			InputSlot.CreateOrResetRequired<IEdgeAttribute<Vector3>>(ref offsetsInputSlot, this);

			// Fields
			scale = 1f;

			// Outputs
			OutputSlot.CreateOrResetGrouped<IEdgeAttribute<Vector3>>(ref offsetVertexPositionsOutputSlot, this, "Offset Vertex Positions", "Attributes");
		}

		public void OnAfterDeserialize()
		{
			InputSlot.ResetAssetTypeIfNull<Topology>(topologyInputSlot);
			InputSlot.ResetAssetTypeIfNull<IEdgeAttribute<Vector3>>(vertexPositionsInputSlot);
			InputSlot.ResetAssetTypeIfNull<IEdgeAttribute<Vector3>>(offsetsInputSlot);

			OutputSlot.ResetAssetTypeIfNull<IEdgeAttribute<Vector3>>(offsetVertexPositionsOutputSlot);
		}

		public void OnBeforeSerialize()
		{
		}

		public override IEnumerable<InputSlot> inputs
		{
			get
			{
				yield return topologyInputSlot;
				yield return vertexPositionsInputSlot;
				yield return offsetsInputSlot;
			}
		}

		public override IEnumerable<OutputSlot> outputs
		{
			get
			{
				yield return offsetVertexPositionsOutputSlot;
			}
		}

		public override IEnumerator BeginGeneration()
		{
			var topology = topologyInputSlot.GetAsset<Topology>();
			var vertexPositions = vertexPositionsInputSlot.GetAsset<IEdgeAttribute<Vector3>>();
			var offsets = offsetsInputSlot.GetAsset<IEdgeAttribute<Vector3>>();
			var offsetPositions = Vector3EdgeAttribute.Create(topology.faceEdges.Count);
			foreach (var edge in topology.faceEdges)
			{
				offsetPositions[edge] = vertexPositions[edge] + offsets[edge] * scale;
			}
			offsetVertexPositionsOutputSlot.SetAsset(offsetPositions);

			yield break;
		}

		public override float estimatedGenerationTime
		{
			get
			{
				return 0.02f;
			}
		}
	}
}
