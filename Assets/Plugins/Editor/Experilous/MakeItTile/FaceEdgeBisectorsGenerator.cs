/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Experilous.MakeItGenerate;

namespace Experilous.MakeItTile
{
	[Generator(typeof(TopologyGeneratorCollection), "Face Edge/Bisectors")]
	public class FaceEdgeBisectorsGenerator : Generator
	{
		[AutoSelect] public InputSlot topologyInputSlot;
		public InputSlot vertexPositionsInputSlot;
		public InputSlot facePositionsInputSlot;

		public OutputSlot bisectorsOutputSlot;

		protected override void Initialize()
		{
			// Inputs
			InputSlot.CreateOrResetRequired<Topology>(ref topologyInputSlot, this);
			InputSlot.CreateOrResetRequired<IEdgeAttribute<Vector3>>(ref vertexPositionsInputSlot, this);
			InputSlot.CreateOrResetRequired<IFaceAttribute<Vector3>>(ref facePositionsInputSlot, this);

			// Outputs
			OutputSlot.CreateOrResetGrouped<IEdgeAttribute<Vector3>>(ref bisectorsOutputSlot, this, "Face Edge Bisectors", "Attributes");
		}

		public override IEnumerable<InputSlot> inputs
		{
			get
			{
				yield return topologyInputSlot;
				yield return vertexPositionsInputSlot;
				yield return facePositionsInputSlot;
			}
		}

		public override IEnumerable<OutputSlot> outputs
		{
			get
			{
				yield return bisectorsOutputSlot;
			}
		}

		public override IEnumerator BeginGeneration()
		{
			var topology = topologyInputSlot.GetAsset<Topology>();
			var vertexPositions = vertexPositionsInputSlot.GetAsset<IEdgeAttribute<Vector3>>();
			var facePositions = facePositionsInputSlot.GetAsset<IFaceAttribute<Vector3>>();
			var bisectors = Vector3EdgeAttribute.Create(topology.faceEdges.Count);
			EdgeAttributeUtility.CalculateFaceEdgeBisectorsFromVertexPositions(topology.faceEdges, topology.internalFaces, vertexPositions, facePositions, bisectors);
			bisectorsOutputSlot.SetAsset(bisectors);

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
