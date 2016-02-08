using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Experilous.Randomization;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorCollection), typeof(UtilitiesCategory), "Random Face Groups")]
	public class RandomFaceGroupsGenerator : AssetGenerator
	{
		[AutoSelect] public AssetInputSlot topologyInputSlot;

		public int groupCount = 1;
		public bool clustered = true;

		public AssetGeneratorRandomization randomization;

		public AssetDescriptor faceGroupCollectionDescriptor;
		public AssetDescriptor faceGroupIndicesDescriptor;
		public AssetDescriptor[] faceGroupDescriptors;

		protected override void Initialize(bool reset = true)
		{
			// Inputs
			if (reset || topologyInputSlot == null) topologyInputSlot = AssetInputSlot.CreateRequired(this, typeof(Topology));

			// Fields
			randomization.Initialize(this, reset);

			// Outputs
			if (reset || faceGroupCollectionDescriptor == null) faceGroupCollectionDescriptor = AssetDescriptor.CreateGrouped<FaceGroupCollection>(this, "Random Face Groups", "Random Face Groups");
			if (reset || faceGroupIndicesDescriptor == null) faceGroupIndicesDescriptor = AssetDescriptor.CreateGrouped<IFaceAttribute<int>>(this, "Face Group Indices", "Attributes");
			if (reset || faceGroupDescriptors == null) faceGroupDescriptors = new AssetDescriptor[0];
		}

		public override IEnumerable<AssetInputSlot> inputs
		{
			get
			{
				yield return topologyInputSlot;
				foreach (var input in randomization.inputs) yield return input;
			}
		}

		public override IEnumerable<AssetDescriptor> outputs
		{
			get
			{
				yield return faceGroupCollectionDescriptor;
				yield return faceGroupIndicesDescriptor;

				foreach (var faceGroupDescriptor in faceGroupDescriptors)
				{
					yield return faceGroupDescriptor;
				}
			}
		}

		public override IEnumerable<AssetReferenceDescriptor> references
		{
			get
			{
				foreach (var faceGroupDescriptor in faceGroupDescriptors)
				{
					yield return faceGroupDescriptor.ReferencedBy(faceGroupCollectionDescriptor);
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
				var random = new RandomUtility(randomization.GetRandomEngine());

				var visitor = new RandomAdjacentFaceVisitor(topology, random.engine);

				var clampedRootCount = Mathf.Clamp(groupCount, 1, topology.internalFaces.Count);

				faceGroupFaceIndices = new List<int>[clampedRootCount];

				var waitHandle = collection.GenerateConcurrently(() =>
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

			faceGroupDescriptors = AssetGeneratorUtility.ResizeArray(faceGroupDescriptors, faceGroupFaceIndices.Length,
				(int index) =>
				{
					return AssetDescriptor.CreateGrouped<FaceGroup>(this, string.Format("Face Cluster {0}", index), faceGroupCollectionDescriptor.name);
				},
				(AssetDescriptor descriptor, int index) =>
				{
					descriptor.path = faceGroupCollectionDescriptor.name;
					return descriptor;
				});

			var faceGroupCollection = FaceGroupCollection.Create(faceGroupFaceIndices.Length);

			for (int faceGroupIndex = 0; faceGroupIndex < faceGroupFaceIndices.Length; ++faceGroupIndex)
			{
				var faceGroup = ArrayFaceGroup.Create(topology, faceGroupFaceIndices[faceGroupIndex].ToArray()).SetName(faceGroupDescriptors[faceGroupIndex].name);
				faceGroupCollection.faceGroups[faceGroupIndex] = faceGroupDescriptors[faceGroupIndex].SetAsset(faceGroup);
			}

			faceGroupCollectionDescriptor.path = faceGroupCollectionDescriptor.name;
			faceGroupIndicesDescriptor.path = faceGroupCollectionDescriptor.name;

			faceGroupCollectionDescriptor.SetAsset(faceGroupCollection);
			faceGroupIndicesDescriptor.SetAsset(IntFaceAttribute.Create(faceGroupIndices.array));

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
