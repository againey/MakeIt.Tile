using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorCollection), typeof(UtilitiesCategory), "Rectangular Face Groups")]
	public class RectangularFaceGroupsGenerator : AssetGenerator
	{
		public Index2D axisDivisions = new Index2D(1, 1);

		[AutoSelect] public AssetInputSlot topologyInputSlot;
		public AssetInputSlot facePositionsInputSlot;
		[AutoSelect] public AssetInputSlot surfaceDescriptorInputSlot;

		public AssetDescriptor faceGroupCollectionDescriptor;
		public AssetDescriptor faceGroupIndicesDescriptor;
		public AssetDescriptor[] faceGroupDescriptors;

		protected override void Initialize(bool reset = true)
		{
			// Inputs
			if (reset || topologyInputSlot == null) topologyInputSlot = AssetInputSlot.CreateRequired(this, typeof(Topology));
			if (reset || facePositionsInputSlot == null) facePositionsInputSlot = AssetInputSlot.CreateRequired(this, typeof(IFaceAttribute<Vector3>));
			if (reset || surfaceDescriptorInputSlot == null) surfaceDescriptorInputSlot = AssetInputSlot.CreateRequired(this, typeof(PlanarSurfaceDescriptor));

			// Outputs
			if (reset || faceGroupCollectionDescriptor == null) faceGroupCollectionDescriptor = AssetDescriptor.CreateGrouped<FaceGroupCollection>(this, "Face Groups", "Face Groups");
			if (reset || faceGroupIndicesDescriptor == null) faceGroupIndicesDescriptor = AssetDescriptor.CreateGrouped<IFaceAttribute<int>>(this, "Face Group Indices", "Attributes");
			if (reset || faceGroupDescriptors == null) faceGroupDescriptors = new AssetDescriptor[0];
		}

		public override IEnumerable<AssetInputSlot> inputs
		{
			get
			{
				yield return topologyInputSlot;
				yield return facePositionsInputSlot;
				yield return surfaceDescriptorInputSlot;
			}
		}

		public override IEnumerable<AssetDescriptor> outputs
		{
			get
			{
				yield return faceGroupCollectionDescriptor;
				yield return faceGroupIndicesDescriptor;

				foreach (var faceGroup in faceGroupDescriptors)
				{
					yield return faceGroup;
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
			var facePositions = facePositionsInputSlot.GetAsset<IFaceAttribute<Vector3>>();
			var surfaceDescriptor = surfaceDescriptorInputSlot.GetAsset<PlanarSurfaceDescriptor>();

			var axis0Vector = surfaceDescriptor.axis0.vector;
			var axis1Vector = surfaceDescriptor.axis1.vector;
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

			foreach (var face in topology.internalFaces)
			{
				var facePosition = facePositions[face];

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

			faceGroupDescriptors = AssetGeneratorUtility.ResizeArray(faceGroupDescriptors, groupCount);

			var faceGroupCollection = FaceGroupCollection.Create(groupCount);

			var faceGroupIndices = new int[topology.internalFaces.Count];

			var groupIndex = 0;
			for (int axis1Index = 0; axis1Index < axisDivisions.y; ++axis1Index)
			{
				for (int axis0Index = 0; axis0Index < axisDivisions.x; ++axis0Index)
				{
					var group = faceGroupFaceIndices[axis0Index + axis1Index * axisDivisions.x];
					if (group.Count > 0)
					{
						var faceGroupName = string.Format("Face Group [{0}, {1}]", axis0Index, axis1Index);
						var faceGroup = ArrayFaceGroup.Create(topology, group.ToArray(), faceGroupName);

						if (faceGroupDescriptors[groupIndex] == null)
						{
							faceGroupDescriptors[groupIndex] = AssetDescriptor.CreateGrouped<FaceGroup>(this, faceGroupName, faceGroupCollectionDescriptor.name);
						}
						else
						{
							faceGroupDescriptors[groupIndex].name = faceGroupName;
							faceGroupDescriptors[groupIndex].path = faceGroupCollectionDescriptor.name;
						}

						faceGroupCollection.faceGroups[groupIndex] = faceGroupDescriptors[groupIndex].SetAsset(faceGroup);

						foreach (var faceIndex in group)
						{
							faceGroupIndices[faceIndex] = groupIndex;
						}

						++groupIndex;
					}
				}
			}

			faceGroupCollectionDescriptor.path = faceGroupCollectionDescriptor.name;
			faceGroupCollectionDescriptor.SetAsset(faceGroupCollection);
			faceGroupIndicesDescriptor.SetAsset(IntFaceAttribute.CreateInstance(faceGroupIndices));

			yield break;
		}

		public override bool canGenerate
		{
			get
			{
				return base.canGenerate &&
					axisDivisions.x >= 1 &&
					axisDivisions.y >= 1;
			}
		}

		public override float estimatedGenerationTime
		{
			get
			{
				return 0.05f;
			}
		}
	}
}
