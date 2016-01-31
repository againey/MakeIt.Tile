using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorCollection), typeof(MeshCategory), "Mesh")]
	public class MeshGenerator : AssetGenerator
	{
		public enum SourceType
		{
			InternalFaces,
			FaceGroupCollection,
			FaceGroup,
		}

		public SourceType sourceType = SourceType.InternalFaces;

		[AutoSelect] public AssetInputSlot topologyInputSlot;
		public AssetInputSlot faceGroupCollectionInputSlot;
		public AssetInputSlot faceGroupInputSlot;
		public AssetInputSlot vertexPositionsInputSlot;
		public AssetInputSlot faceCentroidsInputSlot;
		public AssetInputSlot faceNormalsInputSlot;
		public AssetInputSlot faceColorsInputSlot;

		public bool centerOnGroupAverage = false;

		public AssetDescriptor meshCollectionDescriptor;
		public AssetDescriptor[] meshDescriptors;

		protected override void Initialize(bool reset = true)
		{
			// Inputs
			if (reset || topologyInputSlot == null) topologyInputSlot = AssetInputSlot.CreateRequired(this, typeof(Topology));
			if (reset || faceGroupCollectionInputSlot == null) faceGroupCollectionInputSlot = AssetInputSlot.CreateRequired(this, typeof(FaceGroupCollection));
			if (reset || faceGroupInputSlot == null) faceGroupInputSlot = AssetInputSlot.CreateRequired(this, typeof(FaceGroup));
			if (reset || vertexPositionsInputSlot == null) vertexPositionsInputSlot = AssetInputSlot.CreateRequired(this, typeof(IVertexAttribute<Vector3>));
			if (reset || faceNormalsInputSlot == null) faceNormalsInputSlot = AssetInputSlot.CreateRequired(this, typeof(IFaceAttribute<Vector3>));
			if (reset || faceCentroidsInputSlot == null) faceCentroidsInputSlot = AssetInputSlot.CreateRequired(this, typeof(IFaceAttribute<Vector3>));
			if (reset || faceColorsInputSlot == null) faceColorsInputSlot = AssetInputSlot.CreateRequired(this, typeof(IFaceAttribute<Color>));

			// Outputs
			if (reset || meshCollectionDescriptor == null) meshCollectionDescriptor = AssetDescriptor.CreateGrouped<MeshCollection>(this, "Mesh Collection", "Meshes");
			if (reset || meshDescriptors == null) meshDescriptors = new AssetDescriptor[0];
		}

		public override IEnumerable<AssetInputSlot> inputs
		{
			get
			{
				switch (sourceType)
				{
					case SourceType.InternalFaces:
						yield return topologyInputSlot;
						break;
					case SourceType.FaceGroupCollection:
						yield return faceGroupCollectionInputSlot;
						break;
					case SourceType.FaceGroup:
						yield return faceGroupInputSlot;
						break;
				}
				yield return vertexPositionsInputSlot;
				yield return faceNormalsInputSlot;
				yield return faceCentroidsInputSlot;
				yield return faceColorsInputSlot;
			}
		}

		public override IEnumerable<AssetDescriptor> outputs
		{
			get
			{
				yield return meshCollectionDescriptor;

				foreach (var mesh in meshDescriptors)
				{
					yield return mesh;
				}
			}
		}

		public override IEnumerable<AssetReferenceDescriptor> references
		{
			get
			{
				foreach (var meshDescriptor in meshDescriptors)
				{
					yield return meshDescriptor.ReferencedBy(meshCollectionDescriptor);
				}
			}
		}

		public override void Generate()
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

			if (meshDescriptors.Length < meshes.Count)
			{
				var newMeshes = new AssetDescriptor[meshes.Count];

				for (var i = 0; i < meshDescriptors.Length; ++i)
				{
					newMeshes[i] = meshDescriptors[i];
				}
				for (var i = meshDescriptors.Length; i < newMeshes.Length; ++i)
				{
					newMeshes[i] = AssetDescriptor.CreateGrouped<Mesh>(this, "Mesh", "Meshes");
				}

				meshDescriptors = newMeshes;
			}
			else if (meshDescriptors.Length > meshes.Count)
			{
				var newMeshes = new AssetDescriptor[meshes.Count];

				for (var i = 0; i < newMeshes.Length; ++i)
				{
					newMeshes[i] = meshDescriptors[i];
				}

				meshDescriptors = newMeshes;
			}

			if (meshDescriptors.Length == 1)
			{
				meshDescriptors[0].name = "Mesh";
			}
			else if (meshDescriptors.Length > 1)
			{
				for (int i = 0; i < meshDescriptors.Length; ++i)
				{
					meshDescriptors[i].name = string.Format("Submesh {0}", i);
				}
			}

			for (int i = 0; i < meshDescriptors.Length; ++i)
			{
				meshDescriptors[i].SetAsset(meshes[i]);
			}

			var meshCollection = MeshCollection.Create(meshDescriptors.Length);
			for (int i = 0; i < meshDescriptors.Length; ++i)
			{
				meshCollection.meshes[i] = new MeshCollection.OrientedMesh(meshDescriptors[i].GetAsset<Mesh>(), meshOffsets[i]);
			}
			meshCollectionDescriptor.SetAsset(meshCollection);
		}

		private IFaceAttribute<Color> GetFaceColors()
		{
			if (faceColorsInputSlot.source != null)
			{
				return faceColorsInputSlot.GetAsset<IFaceAttribute<Color>>();
			}
			else
			{
				return ConstantColorFaceAttribute.CreateInstance(Color.white);
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
	}
}
