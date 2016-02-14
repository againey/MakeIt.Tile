﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Experilous.Generation;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorCollection), typeof(UtilitiesCategory), "Rectangular Face Groups")]
	public class RectangularFaceGroupsGenerator : Generator
	{
		public Index2D axisDivisions;

		[AutoSelect] public InputSlot surfaceInputSlot;
		[AutoSelect] public InputSlot topologyInputSlot;
		public InputSlot facePositionsInputSlot;

		public OutputSlot faceGroupCollectionDescriptor;
		public OutputSlot faceGroupIndicesDescriptor;
		public OutputSlot[] faceGroupDescriptors;

		protected override void Initialize()
		{
			// Inputs
			InputSlot.CreateOrResetRequired<PlanarSurface>(ref surfaceInputSlot, this);
			InputSlot.CreateOrResetRequired<Topology>(ref topologyInputSlot, this);
			InputSlot.CreateOrResetRequired<IFaceAttribute<Vector3>>(ref facePositionsInputSlot, this);

			// Fields
			axisDivisions = new Index2D(1, 1);

			// Outputs
			OutputSlot.CreateOrResetGrouped<FaceGroupCollection>(ref faceGroupCollectionDescriptor, this, "Face Groups", "Face Groups");
			OutputSlot.CreateOrResetGrouped<IFaceAttribute<int>>(ref faceGroupIndicesDescriptor, this, "Face Group Indices", "Attributes");
			faceGroupDescriptors = new OutputSlot[0];
		}

		public override IEnumerable<InputSlot> inputs
		{
			get
			{
				yield return surfaceInputSlot;
				yield return topologyInputSlot;
				yield return facePositionsInputSlot;
			}
		}

		public override IEnumerable<OutputSlot> outputs
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

		public override IEnumerable<InternalSlotConnection> internalConnections
		{
			get
			{
				foreach (var faceGroupDescriptor in faceGroupDescriptors)
				{
					yield return faceGroupCollectionDescriptor.Uses(faceGroupDescriptor);
				}
			}
		}

		public override IEnumerator BeginGeneration()
		{
			var surfaceDescriptor = surfaceInputSlot.GetAsset<PlanarSurface>();
			var topology = topologyInputSlot.GetAsset<Topology>();
			var facePositions = facePositionsInputSlot.GetAsset<IFaceAttribute<Vector3>>();

			int groupCount = 0;
			var faceGroupFaceIndices = new List<int>[axisDivisions.x * axisDivisions.y];

			var waitHandle = executive.GenerateConcurrently(() =>
			{
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

				foreach (var group in faceGroupFaceIndices)
				{
					if (group.Count > 0) ++groupCount;
				}
			});
			while (waitHandle.WaitOne(10) == false)
			{
				yield return null;
			}

			faceGroupDescriptors = GeneratorUtility.ResizeArray(faceGroupDescriptors, groupCount,
				(int index) =>
				{
					return null;
				},
				(OutputSlot output, int index) =>
				{
					output.DisconnectAll();
					return output;
				});

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
						var faceGroup = ArrayFaceGroup.Create(topology, group.ToArray()).SetName(faceGroupName);

						if (faceGroupDescriptors[groupIndex] == null)
						{
							faceGroupDescriptors[groupIndex] = OutputSlot.CreateGrouped<FaceGroup>(this, faceGroupName, faceGroupCollectionDescriptor.name);
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
			faceGroupIndicesDescriptor.SetAsset(IntFaceAttribute.Create(faceGroupIndices));

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
