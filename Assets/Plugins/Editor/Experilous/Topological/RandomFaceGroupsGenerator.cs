﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Experilous.Randomization;
using Experilous.Generation;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorCollection), typeof(UtilitiesCategory), "Random Face Groups")]
	public class RandomFaceGroupsGenerator : Generator
	{
		[AutoSelect] public InputSlot topologyInputSlot;

		public int groupCount;
		public bool clustered;

		public RandomnessDescriptor randomness;

		public OutputSlot faceGroupCollectionOutputSlot;
		public OutputSlot faceGroupIndicesOutputSlot;
		public OutputSlot[] faceGroupOutputSlots;

		protected override void Initialize()
		{
			// Inputs
			InputSlot.CreateOrResetRequired<Topology>(ref topologyInputSlot, this);

			// Fields
			groupCount = 1;
			clustered = true;
			randomness.Initialize(this);

			// Outputs
			OutputSlot.CreateOrResetGrouped<FaceGroupCollection>(ref faceGroupCollectionOutputSlot, this, "Random Face Groups", "Face Groups");
			OutputSlot.CreateOrResetGrouped<IFaceAttribute<int>>(ref faceGroupIndicesOutputSlot, this, "Random Face Group Indices", "Attributes");
			faceGroupOutputSlots = new OutputSlot[0];
		}

		protected override void OnUpdate()
		{
			randomness.Update();
		}

		public override IEnumerable<InputSlot> inputs
		{
			get
			{
				yield return topologyInputSlot;
				yield return randomness.randomEngineSeedInputSlot;
			}
		}

		public override IEnumerable<OutputSlot> outputs
		{
			get
			{
				yield return faceGroupCollectionOutputSlot;
				yield return faceGroupIndicesOutputSlot;

				foreach (var faceGroupOutputSlot in faceGroupOutputSlots)
				{
					yield return faceGroupOutputSlot;
				}
			}
		}

		public override IEnumerable<InternalSlotConnection> internalConnections
		{
			get
			{
				foreach (var faceGroupOutputSlot in faceGroupOutputSlots)
				{
					yield return faceGroupCollectionOutputSlot.Uses(faceGroupOutputSlot);
				}
			}
		}

		public override IEnumerator BeginGeneration()
		{
			var topology = topologyInputSlot.GetAsset<Topology>();
			var faceGroupIndices = new int[topology.internalFaces.Count].AsFaceAttribute();
			List<int>[] faceGroupFaceIndices;

			//if (clustered || true) //TODO create an unclustered version
			{
				var random = new RandomUtility(randomness.GetRandomEngine());

				var visitor = new RandomAdjacentFaceVisitor(topology, random.engine);

				var clampedRootCount = Mathf.Clamp(groupCount, 1, topology.internalFaces.Count);

				faceGroupFaceIndices = new List<int>[clampedRootCount];

				var waitHandle = executive.GenerateConcurrently(() =>
				{
					for (int faceGroupIndex = 0; faceGroupIndex < clampedRootCount; ++faceGroupIndex)
					{
						faceGroupFaceIndices[faceGroupIndex] = new List<int>();

						Topology.Face face;
						do
						{
							face = topology.faces[random.HalfOpenRange(0, topology.internalFaces.Count)];
						} while (visitor.IsRoot(face));
						visitor.AddRoot(face);
						faceGroupIndices[face] = faceGroupIndex;
						faceGroupFaceIndices[faceGroupIndex].Add(face.index);
					}

					foreach (var edge in (IEnumerable<Topology.FaceEdge>)visitor)
					{
						var faceGroupIndex = faceGroupIndices[edge.nearFace];
						faceGroupIndices[edge.farFace] = faceGroupIndex;
						faceGroupFaceIndices[faceGroupIndex].Add(edge.farFace.index);
					}
				});
				while (waitHandle.WaitOne(10) == false)
				{
					yield return null;
				}
			}

			faceGroupOutputSlots = GeneratorUtility.ResizeArray(faceGroupOutputSlots, faceGroupFaceIndices.Length,
				(int index) =>
				{
					return OutputSlot.CreateGrouped<FaceGroup>(this, string.Format("Face Group {0}", index), faceGroupCollectionOutputSlot.name, true, OutputSlot.Availability.DuringGeneration);
				},
				(OutputSlot output, int index) =>
				{
					output.DisconnectAll();
					output.path = faceGroupCollectionOutputSlot.name;
					return output;
				});

			var faceGroupCollection = FaceGroupCollection.Create(faceGroupFaceIndices.Length);

			for (int faceGroupIndex = 0; faceGroupIndex < faceGroupFaceIndices.Length; ++faceGroupIndex)
			{
				var faceGroup = ArrayFaceGroup.Create(topology, faceGroupFaceIndices[faceGroupIndex].ToArray()).SetName(faceGroupOutputSlots[faceGroupIndex].name);
				faceGroupCollection.faceGroups[faceGroupIndex] = faceGroupOutputSlots[faceGroupIndex].SetAsset(faceGroup);
			}

			faceGroupCollectionOutputSlot.SetAsset(faceGroupCollection);
			faceGroupIndicesOutputSlot.SetAsset(IntFaceAttribute.Create(faceGroupIndices.array));

			yield break;
		}

		public override bool canGenerate
		{
			get
			{
				return base.canGenerate &&
					groupCount >= 1;
			}
		}

		public override float estimatedGenerationTime
		{
			get
			{
				return 0.1f;
			}
		}
	}
}