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
	public class FaceEdgeBisectorsGenerator : Generator, ISerializationCallbackReceiver
	{
		[AutoSelect] public InputSlot surfaceInputSlot;
		[AutoSelect] public InputSlot topologyInputSlot;
		public InputSlot vertexPositionsInputSlot;
		public InputSlot facePositionsInputSlot;

		public OutputSlot bisectorsOutputSlot;

		protected override void Initialize()
		{
			// Inputs
			InputSlot.CreateOrResetRequired<Surface>(ref surfaceInputSlot, this);
			InputSlot.CreateOrResetRequired<Topology>(ref topologyInputSlot, this);
			InputSlot.CreateOrResetRequired<IEdgeAttribute<Vector3>>(ref vertexPositionsInputSlot, this);
			InputSlot.CreateOrResetRequired<IFaceAttribute<Vector3>>(ref facePositionsInputSlot, this);

			// Outputs
			OutputSlot.CreateOrResetGrouped<IEdgeAttribute<Vector3>>(ref bisectorsOutputSlot, this, "Face Edge Bisectors", "Attributes");
		}

		public void OnAfterDeserialize()
		{
			if (surfaceInputSlot.generator == null) surfaceInputSlot = InputSlot.CreateRequired<Surface>(this);

			InputSlot.ResetAssetTypeIfNull<Surface>(surfaceInputSlot);
			InputSlot.ResetAssetTypeIfNull<Topology>(topologyInputSlot);
			InputSlot.ResetAssetTypeIfNull<IEdgeAttribute<Vector3>>(vertexPositionsInputSlot);
			InputSlot.ResetAssetTypeIfNull<IFaceAttribute<Vector3>>(facePositionsInputSlot);

			OutputSlot.ResetAssetTypeIfNull<IEdgeAttribute<Vector3>>(bisectorsOutputSlot);
		}

		public void OnBeforeSerialize()
		{
		}

		public override IEnumerable<InputSlot> inputs
		{
			get
			{
				yield return surfaceInputSlot;
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
			var surface = surfaceInputSlot.GetAsset<Surface>();
			var topology = topologyInputSlot.GetAsset<Topology>();
			var vertexPositions = vertexPositionsInputSlot.GetAsset<IEdgeAttribute<Vector3>>();
			var facePositions = facePositionsInputSlot.GetAsset<IFaceAttribute<Vector3>>();
			var bisectors = Vector3EdgeAttribute.Create(topology.faceEdges.Count);
			EdgeAttributeUtility.CalculateFaceEdgeBisectorsFromVertexPositions(topology.internalFaces, surface, vertexPositions, facePositions, bisectors);
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
