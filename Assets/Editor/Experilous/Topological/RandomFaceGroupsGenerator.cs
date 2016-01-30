using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorCollection), typeof(UtilitiesCategory), "Random Face Groups")]
	public class RandomFaceGroupsGenerator : AssetGenerator
	{
		public bool clustered = true;
		public int groupCount = 1;

		public AssetInputSlot topologyInputSlot;

		public AssetDescriptor faceGroupCollectionDescriptor;
		public AssetDescriptor faceGroupIndicesDescriptor;
		public AssetDescriptor[] faceGroupDescriptors;

		protected override void Initialize(bool reset = true)
		{
			// Inputs
			if (reset || topologyInputSlot == null) topologyInputSlot = AssetInputSlot.CreateRequired(this, typeof(Topology));

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

		public override void Generate()
		{
			var topology = topologyInputSlot.GetAsset<Topology>();
			var faceGroupIndices = new int[topology.internalFaces.Count].AsFaceAttribute();
			List<int>[] faceGroupFaceIndices;

			//if (clustered || true) //TODO create an unclustered version
			{
				var random = new Random(new NativeRandomEngine(0));

				var visitor = new RandomAdjacentFaceVisitor(topology, random.engine);

				var clampedRootCount = Mathf.Clamp(groupCount, 1, topology.internalFaces.Count);

				faceGroupFaceIndices = new List<int>[clampedRootCount];

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
				var faceGroup = ArrayFaceGroup.Create(topology, faceGroupFaceIndices[faceGroupIndex].ToArray(), faceGroupDescriptors[faceGroupIndex].name);
				faceGroupCollection.faceGroups[faceGroupIndex] = faceGroupDescriptors[faceGroupIndex].SetAsset(faceGroup);
			}

			faceGroupCollectionDescriptor.path = faceGroupCollectionDescriptor.name;
			faceGroupIndicesDescriptor.path = faceGroupCollectionDescriptor.name;

			faceGroupCollectionDescriptor.SetAsset(faceGroupCollection);
			faceGroupIndicesDescriptor.SetAsset(IntFaceAttribute.CreateInstance(faceGroupIndices.array));
		}

		public override bool canGenerate
		{
			get
			{
				return base.canGenerate &&
					groupCount >= 1;
			}
		}
	}
}
