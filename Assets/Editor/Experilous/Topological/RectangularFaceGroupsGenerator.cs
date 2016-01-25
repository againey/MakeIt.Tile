using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorBundle), typeof(UtilitiesCategory), "Rectangular Face Groups")]
	public class RectangularFaceGroupsGenerator : AssetGenerator
	{
		public Index2D axisDivisions = new Index2D(1, 1);

		public AssetDescriptor topology;
		public AssetDescriptor facePositions;
		public AssetDescriptor surfaceDescriptor;

		public AssetDescriptor faceGroupCollection;
		public AssetDescriptor[] faceGroups;

		public static RectangularFaceGroupsGenerator CreateDefaultInstance(AssetGeneratorBundle bundle, string name)
		{
			var generator = CreateInstance<RectangularFaceGroupsGenerator>();
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
				if (facePositions != null) yield return facePositions;
				if (surfaceDescriptor != null) yield return surfaceDescriptor;
			}
		}

		public override IEnumerable<AssetDescriptor> outputs
		{
			get
			{
				if (faceGroupCollection == null) faceGroupCollection = AssetDescriptor.Create(this, typeof(FaceGroupCollection), "Face Groups", "Face Groups");
				yield return faceGroupCollection;

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
			if (!ResetMemberDependency(dependency, ref topology, ref facePositions, ref surfaceDescriptor))
			{
				throw new System.ArgumentException(string.Format("Generated asset \"{0}\" of type {1} is not a dependency of this vertex normals generator.", dependency.name, dependency.GetType().Name), "dependency");
			}
		}

		public override void Generate()
		{
			var topologyAsset = topology.GetAsset<Topology>();
			var facePositionsAsset = facePositions.GetAsset<IFaceAttribute<Vector3>>();
			var surfaceDescriptorAsset = surfaceDescriptor.GetAsset<PlanarSurfaceDescriptor>();

			var axis0Vector = surfaceDescriptorAsset.axis0.vector;
			var axis1Vector = surfaceDescriptorAsset.axis1.vector;
			var surfaceNormal = Vector3.Cross(axis0Vector, axis1Vector).normalized;
			var axis0Normal = Vector3.Cross(axis0Vector, surfaceNormal).normalized;
			var axis1Normal = Vector3.Cross(axis1Vector, surfaceNormal).normalized;
			var axis0DividedVector = axis0Vector / axisDivisions.x;
			var axis1DividedVector = axis1Vector / axisDivisions.y;
			var axis0Dot = Vector3.Dot(axis1Normal, axis0DividedVector);
			var axis1Dot = Vector3.Dot(axis0Normal, axis1DividedVector);
			var origin = new Vector3(0f, 0f, 0f); //TODO this should come from somewhere, probably surface descriptor

			var faceGroupFaceIndices = new List<int>[axisDivisions.x * axisDivisions.y];
			for (int i = 0; i < faceGroupFaceIndices.Length; ++i)
			{
				faceGroupFaceIndices[i] = new List<int>();
			}

			foreach (var face in topologyAsset.internalFaces)
			{
				var facePosition = facePositionsAsset[face];

				var axis0Offset = Vector3.Dot(axis1Normal, facePosition - origin) / axis0Dot;
				var axis1Offset = Vector3.Dot(axis0Normal, facePosition - origin) / axis1Dot;

				var axis0Index = Mathf.Clamp(Mathf.FloorToInt(axis0Offset), 0, axisDivisions.x - 1);
				var axis1Index = Mathf.Clamp(Mathf.FloorToInt(axis1Offset), 0, axisDivisions.y - 1);

				faceGroupFaceIndices[axis0Index + axis1Index * axisDivisions.x].Add(face.index);
			}

			int groupCount = 0;
			foreach (var group in faceGroupFaceIndices)
			{
				if (group.Count > 0) ++groupCount;
			}

			faceGroups = AssetGeneratorUtility.ResizeArray(faceGroups, groupCount);

			var faceGroupCollectionAsset = FaceGroupCollection.Create(groupCount);

			var groupIndex = 0;
			for (int axis1Index = 0; axis1Index < axisDivisions.y; ++axis1Index)
			{
				for (int axis0Index = 0; axis0Index < axisDivisions.x; ++axis0Index)
				{
					var group = faceGroupFaceIndices[axis0Index + axis1Index * axisDivisions.x];
					if (group.Count > 0)
					{
						var faceGroupName = string.Format("Face Group [{0}, {1}]", axis0Index, axis1Index);
						var faceGroup = ArrayFaceGroup.Create(topologyAsset, group.ToArray(), faceGroupName);

						if (faceGroups[groupIndex] == null)
						{
							faceGroups[groupIndex] = AssetDescriptor.Create(this, typeof(FaceGroup), faceGroupName, faceGroupCollection.name);
						}
						else
						{
							faceGroups[groupIndex].name = faceGroupName;
							faceGroups[groupIndex].groupName = faceGroupCollection.name;
						}

						faceGroupCollectionAsset.faceGroups[groupIndex] = faceGroups[groupIndex].SetAsset(faceGroup);

						++groupIndex;
					}
				}
			}

			faceGroupCollection.groupName = faceGroupCollection.name;
			faceGroupCollection.SetAsset(faceGroupCollectionAsset);
		}

		public override bool CanGenerate()
		{
			return (
				topology != null &&
				facePositions != null &&
				surfaceDescriptor != null &&
				axisDivisions.x >= 1 &&
				axisDivisions.y >= 1);
		}
	}
}
