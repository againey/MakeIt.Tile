using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorBundle), typeof(UtilitiesCategory), "Random Face Groups")]
	public class RandomFaceGroupsGenerator : AssetGenerator
	{
		public bool clustered = true;
		public int groupCount = 1;

		public AssetDescriptor topology;

		public AssetDescriptor faceGroupCollection;
		public AssetDescriptor faceGroupIndices;
		public AssetDescriptor[] faceGroups;

		public static RandomFaceGroupsGenerator CreateDefaultInstance(AssetGeneratorBundle bundle, string name)
		{
			var generator = CreateInstance<RandomFaceGroupsGenerator>();
			generator.bundle = bundle;
			generator.name = name;
			generator.hideFlags = HideFlags.HideInHierarchy;
			return generator;
		}

		public override IEnumerable<AssetDescriptor> dependencies
		{
			get
			{
				if (topology != null) yield return topology;
			}
		}

		public override IEnumerable<AssetDescriptor> outputs
		{
			get
			{
				if (faceGroupCollection == null) faceGroupCollection = AssetDescriptor.Create(this, typeof(FaceGroupCollection), "Random Face Groups", "Random Face Groups");
				if (faceGroupIndices == null) faceGroupIndices = AssetDescriptor.Create(this, typeof(IFaceAttribute<int>), "Face Group Indices", "Random Face Groups");

				yield return faceGroupCollection;
				yield return faceGroupIndices;

				if (faceGroups != null && faceGroups.Length > 0)
				{
					foreach (var faceGroup in faceGroups)
					{
						yield return faceGroup;
					}
				}
			}
		}

		public override void ResetDependency(AssetDescriptor dependency)
		{
			if (dependency == null) throw new System.ArgumentNullException("dependency");
			if (!ResetMemberDependency(dependency, ref topology))
			{
				throw new System.ArgumentException(string.Format("Generated asset \"{0}\" of type {1} is not a dependency of this vertex normals generator.", dependency.name, dependency.GetType().Name), "dependency");
			}
		}

		public override void Generate()
		{
			var topologyAsset = topology.GetAsset<Topology>();
			var faceGroupIndicesArray = new int[topologyAsset.internalFaces.Count];
			List<int>[] faceGroupFaceIndices;

			//if (clustered || true) //TODO create an unclustered version
			{
				var random = new Random(new NativeRandomEngine(0));

				var visitor = new RandomAdjacentFaceVisitor(topologyAsset, random.engine);

				var clampedRootCount = Mathf.Clamp(groupCount, 1, topologyAsset.internalFaces.Count);

				faceGroupFaceIndices = new List<int>[clampedRootCount];

				for (int faceGroupIndex = 0; faceGroupIndex < clampedRootCount; ++faceGroupIndex)
				{
					faceGroupFaceIndices[faceGroupIndex] = new List<int>();

					Topology.Face face;
					do
					{
						face = topologyAsset.faces[random.HalfOpenRange(0, topologyAsset.internalFaces.Count)];
					} while (visitor.IsRoot(face));
					visitor.AddRoot(face);
					faceGroupIndicesArray[face] = faceGroupIndex;
					faceGroupFaceIndices[faceGroupIndex].Add(face);
				}

				foreach (var edge in (IEnumerable<Topology.FaceEdge>)visitor)
				{
					var faceGroupIndex = faceGroupIndicesArray[edge.nearFace];
					faceGroupIndicesArray[edge.farFace] = faceGroupIndex;
					faceGroupFaceIndices[faceGroupIndex].Add(edge.farFace);
				}
			}

			faceGroups = AssetGeneratorUtility.ResizeArray(faceGroups, faceGroupFaceIndices.Length,
				(int index) =>
				{
					return AssetDescriptor.Create(this, typeof(FaceGroup), string.Format("Face Cluster {0}", index), faceGroupCollection.name);
				},
				(AssetDescriptor descriptor, int index) =>
				{
					descriptor.groupName = faceGroupCollection.name;
					return descriptor;
				});

			var faceGroupCollectionAsset = FaceGroupCollection.Create(faceGroupFaceIndices.Length);

			for (int faceGroupIndex = 0; faceGroupIndex < faceGroupFaceIndices.Length; ++faceGroupIndex)
			{
				var faceGroup = ArrayFaceGroup.Create(topologyAsset, faceGroupFaceIndices[faceGroupIndex].ToArray(), faceGroups[faceGroupIndex].name);
				faceGroupCollectionAsset.faceGroups[faceGroupIndex] = faceGroups[faceGroupIndex].SetAsset(faceGroup);
			}

			faceGroupCollection.groupName = faceGroupCollection.name;
			faceGroupIndices.groupName = faceGroupCollection.name;

			faceGroupCollection.SetAsset(faceGroupCollectionAsset);
			faceGroupIndices.SetAsset(IntFaceAttribute.CreateInstance(faceGroupIndicesArray));
		}

		public override bool CanGenerate()
		{
			return (
				topology != null &&
				groupCount >= 1);
		}
	}
}
