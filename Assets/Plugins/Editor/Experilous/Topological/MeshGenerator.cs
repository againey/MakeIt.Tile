using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Experilous.Generation;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorCollection), typeof(MeshCategory), "Mesh")]
	public class MeshGenerator : Generator
	{
		public enum SourceType
		{
			InternalFaces,
			FaceGroupCollection,
			FaceGroup,
		}

		public SourceType sourceType;

		[AutoSelect] public InputSlot topologyInputSlot;
		public InputSlot faceGroupCollectionInputSlot;
		public InputSlot faceGroupInputSlot;
		public InputSlot vertexPositionsInputSlot;
		public InputSlot faceCentroidsInputSlot;
		public InputSlot faceNormalsInputSlot;
		public InputSlot faceColorsInputSlot;

		public bool centerOnGroupAverage;

		public OutputSlot meshCollectionOutputSlot;
		public OutputSlot[] meshOutputSlots;

		protected override void Initialize()
		{
			// Inputs
			InputSlot.CreateOrResetRequired<Topology>(ref topologyInputSlot, this);
			InputSlot.CreateOrResetRequired<FaceGroupCollection>(ref faceGroupCollectionInputSlot, this);
			InputSlot.CreateOrResetRequired<FaceGroup>(ref faceGroupInputSlot, this);
			InputSlot.CreateOrResetRequired<IVertexAttribute<Vector3>>(ref vertexPositionsInputSlot, this);
			InputSlot.CreateOrResetRequired<IFaceAttribute<Vector3>>(ref faceNormalsInputSlot, this);
			InputSlot.CreateOrResetRequired<IFaceAttribute<Vector3>>(ref faceCentroidsInputSlot, this);
			InputSlot.CreateOrResetOptional<IFaceAttribute<Color>>(ref faceColorsInputSlot, this);

			// Fields
			sourceType = SourceType.InternalFaces;
			centerOnGroupAverage = false;

			// Outputs
			OutputSlot.CreateOrResetGrouped<MeshCollection>(ref meshCollectionOutputSlot, this, "Mesh Collection", "Meshes");
			meshOutputSlots = new OutputSlot[0];
		}

		protected override void OnUpdate()
		{
			topologyInputSlot.isActive = (sourceType == SourceType.InternalFaces);
			faceGroupCollectionInputSlot.isActive = (sourceType == SourceType.FaceGroupCollection);
			faceGroupInputSlot.isActive = (sourceType == SourceType.FaceGroup);
		}

		public override IEnumerable<InputSlot> inputs
		{
			get
			{
				yield return topologyInputSlot;
				yield return faceGroupCollectionInputSlot;
				yield return faceGroupInputSlot;
				yield return vertexPositionsInputSlot;
				yield return faceNormalsInputSlot;
				yield return faceCentroidsInputSlot;
				yield return faceColorsInputSlot;
			}
		}

		public override IEnumerable<OutputSlot> outputs
		{
			get
			{
				yield return meshCollectionOutputSlot;

				foreach (var mesh in meshOutputSlots)
				{
					yield return mesh;
				}
			}
		}

		public override IEnumerable<InternalSlotConnection> internalConnections
		{
			get
			{
				foreach (var meshOutputSlot in meshOutputSlots)
				{
					yield return meshCollectionOutputSlot.Uses(meshOutputSlot);
				}
			}
		}

		public override IEnumerator BeginGeneration()
		{
			var meshes = new List<Mesh>();
			var meshOffsets = new List<Vector3>();

			if (centerOnGroupAverage)
			{
				switch (sourceType)
				{
					case SourceType.InternalFaces:
						GenerateMeshes(topologyInputSlot.GetAsset<Topology>().internalFaces, GetAverageFromCentroids(topologyInputSlot.GetAsset<Topology>().internalFaces, faceCentroidsInputSlot.GetAsset<IFaceAttribute<Vector3>>()), meshes, meshOffsets);
						break;
					case SourceType.FaceGroupCollection:
						foreach (var faceGroup in faceGroupCollectionInputSlot.GetAsset<FaceGroupCollection>().faceGroups)
						{
							GenerateMeshes(faceGroup, GetAverageFromCentroids(faceGroup, faceCentroidsInputSlot.GetAsset<IFaceAttribute<Vector3>>()), meshes, meshOffsets);
						}
						break;
					case SourceType.FaceGroup:
						GenerateMeshes(faceGroupInputSlot.GetAsset<FaceGroup>(), GetAverageFromCentroids(faceGroupInputSlot.GetAsset<FaceGroup>(), faceCentroidsInputSlot.GetAsset<IFaceAttribute<Vector3>>()), meshes, meshOffsets);
						break;
				}
			}
			else
			{
				switch (sourceType)
				{
					case SourceType.InternalFaces:
						GenerateMeshes(topologyInputSlot.GetAsset<Topology>().internalFaces, meshes, meshOffsets);
						break;
					case SourceType.FaceGroupCollection:
						foreach (var faceGroup in faceGroupCollectionInputSlot.GetAsset<FaceGroupCollection>().faceGroups)
						{
							GenerateMeshes(faceGroup, meshes, meshOffsets);
						}
						break;
					case SourceType.FaceGroup:
						GenerateMeshes(faceGroupInputSlot.GetAsset<FaceGroup>(), meshes, meshOffsets);
						break;
				}
			}

			if (meshOutputSlots.Length < meshes.Count)
			{
				var newMeshes = new OutputSlot[meshes.Count];

				for (var i = 0; i < meshOutputSlots.Length; ++i)
				{
					newMeshes[i] = meshOutputSlots[i];
				}
				for (var i = meshOutputSlots.Length; i < newMeshes.Length; ++i)
				{
					newMeshes[i] = OutputSlot.CreateGrouped<Mesh>(this, "Mesh", "Meshes");
				}

				meshOutputSlots = newMeshes;
			}
			else if (meshOutputSlots.Length > meshes.Count)
			{
				var newMeshes = new OutputSlot[meshes.Count];

				for (var i = 0; i < newMeshes.Length; ++i)
				{
					newMeshes[i] = meshOutputSlots[i];
				}

				meshOutputSlots = newMeshes;
			}

			if (meshOutputSlots.Length == 1)
			{
				meshOutputSlots[0].name = "Mesh";
			}
			else if (meshOutputSlots.Length > 1)
			{
				for (int i = 0; i < meshOutputSlots.Length; ++i)
				{
					meshOutputSlots[i].name = string.Format("Submesh {0}", i);
				}
			}

			for (int i = 0; i < meshOutputSlots.Length; ++i)
			{
				meshOutputSlots[i].SetAsset(meshes[i]);
			}

			var meshCollection = MeshCollection.Create(meshOutputSlots.Length);
			for (int i = 0; i < meshOutputSlots.Length; ++i)
			{
				meshCollection.meshes[i] = new MeshCollection.OrientedMesh(meshOutputSlots[i].GetAsset<Mesh>(), meshOffsets[i]);
			}
			meshCollectionOutputSlot.SetAsset(meshCollection);

			yield break;
		}

		private IFaceAttribute<Color> GetFaceColors()
		{
			if (faceColorsInputSlot.source != null)
			{
				return faceColorsInputSlot.GetAsset<IFaceAttribute<Color>>();
			}
			else
			{
				return ConstantColorFaceAttribute.Create(Color.white);
			}
		}

		private void GenerateMeshes(IEnumerable<Topology.Face> faces, List<Mesh> meshes, List<Vector3> offsets)
		{
			foreach (var mesh in MeshBuilder.BuildMeshes(
				faces,
				vertexPositionsInputSlot.GetAsset<IVertexAttribute<Vector3>>(),
				faceCentroidsInputSlot.GetAsset<IFaceAttribute<Vector3>>(),
				faceNormalsInputSlot.GetAsset<IFaceAttribute<Vector3>>(),
				GetFaceColors()))
			{
				meshes.Add(mesh);
				offsets.Add(new Vector3(0f, 0f, 0f));
			}
		}

		private void GenerateMeshes(IEnumerable<Topology.Face> faces, Vector3 groupAverage, List<Mesh> meshes, List<Vector3> offsets)
		{
			foreach (var mesh in MeshBuilder.BuildMeshes(
				faces,
				-groupAverage,
				vertexPositionsInputSlot.GetAsset<IVertexAttribute<Vector3>>(),
				faceCentroidsInputSlot.GetAsset<IFaceAttribute<Vector3>>(),
				faceNormalsInputSlot.GetAsset<IFaceAttribute<Vector3>>(),
				GetFaceColors()))
			{
				meshes.Add(mesh);
				offsets.Add(groupAverage);
			}
		}

		private Vector3 GetAverageFromCentroids(IList<Topology.Face> faces, IFaceAttribute<Vector3> centroidPositions)
		{
			var sum = new Vector3(0f, 0f, 0f);
			foreach (var face in faces)
			{
				sum += centroidPositions[face];
			}

			return sum / faces.Count;
		}

		public override float estimatedGenerationTime
		{
			get
			{
				return 0.2f;
			}
		}
	}
}
