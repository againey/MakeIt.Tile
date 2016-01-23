using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorBundle), typeof(MeshCategory), "Mesh")]
	public class MeshGenerator : AssetGenerator
	{
		public enum SourceType
		{
			InternalFaces,
			FaceGroupCollection,
			FaceGroup,
		}

		public SourceType sourceType = SourceType.InternalFaces;
		public bool centerOnGroupAverage = false;

		public AssetDescriptor topology;
		public AssetDescriptor faceGroupCollection;
		public AssetDescriptor faceGroup;
		public AssetDescriptor vertexPositions;
		public AssetDescriptor faceCentroids;
		public AssetDescriptor faceNormals;

		public AssetDescriptor[] meshes;
		public AssetDescriptor meshCollection;

		public static MeshGenerator CreateDefaultInstance(AssetGeneratorBundle bundle, string name)
		{
			var generator = CreateInstance<MeshGenerator>();
			generator.bundle = bundle;
			generator.name = name;
			generator.hideFlags = HideFlags.HideInHierarchy;
			return generator;
		}

		public override IEnumerable<AssetDescriptor> dependencies
		{
			get
			{
				switch (sourceType)
				{
					case SourceType.InternalFaces:
						if (topology != null) yield return topology;
						break;
					case SourceType.FaceGroupCollection:
						if (faceGroupCollection != null) yield return faceGroupCollection;
						break;
					case SourceType.FaceGroup:
						if (faceGroup != null) yield return faceGroup;
						break;
				}
				if (vertexPositions != null) yield return vertexPositions;
				if (faceNormals != null) yield return faceNormals;
				if (faceCentroids != null) yield return faceCentroids;
			}
		}

		public override IEnumerable<AssetDescriptor> outputs
		{
			get
			{
				if (meshes == null || meshes.Length == 0)
				{
					meshes = new AssetDescriptor[1] { AssetDescriptor.Create(this, typeof(Mesh), "Mesh", "Meshes") };
				}

				if (meshCollection == null) meshCollection = AssetDescriptor.Create(this, typeof(MeshCollection), "Mesh Collection", "Meshes");

				foreach (var mesh in meshes)
				{
					yield return mesh;
				}

				yield return meshCollection;
			}
		}

		public override void ResetDependency(AssetDescriptor dependency)
		{
			if (dependency == null) throw new System.ArgumentNullException("dependency");
			if (!ResetMemberDependency(dependency, ref topology, ref faceGroupCollection, ref faceGroup) &&
				!ResetMemberDependency(dependency, ref vertexPositions, ref faceCentroids, ref faceNormals))
			{
				throw new System.ArgumentException(string.Format("Generated asset \"{0}\" of type {1} is not a dependency of this mesh generator.", dependency.name, dependency.GetType().Name), "dependency");
			}
		}

		public override void Generate()
		{
			var generatedMeshes = new List<Mesh>();
			var meshOffsets = new List<Vector3>();

			if (centerOnGroupAverage)
			{
				switch (sourceType)
				{
					case SourceType.InternalFaces:
						GenerateMeshes(topology.GetAsset<Topology>().internalFaces, GetAverageFromCentroids(topology.GetAsset<Topology>().internalFaces, faceCentroids.GetAsset<IFaceAttribute<Vector3>>()), generatedMeshes, meshOffsets);
						break;
					case SourceType.FaceGroupCollection:
						foreach (var faceGroup in faceGroupCollection.GetAsset<FaceGroupCollection>().faceGroups)
						{
							GenerateMeshes(faceGroup, GetAverageFromCentroids(faceGroup, faceCentroids.GetAsset<IFaceAttribute<Vector3>>()), generatedMeshes, meshOffsets);
						}
						break;
					case SourceType.FaceGroup:
						GenerateMeshes(faceGroup.GetAsset<FaceGroup>(), GetAverageFromCentroids(faceGroup.GetAsset<FaceGroup>(), faceCentroids.GetAsset<IFaceAttribute<Vector3>>()), generatedMeshes, meshOffsets);
						break;
				}
			}
			else
			{
				switch (sourceType)
				{
					case SourceType.InternalFaces:
						GenerateMeshes(topology.GetAsset<Topology>().internalFaces, generatedMeshes, meshOffsets);
						break;
					case SourceType.FaceGroupCollection:
						foreach (var faceGroup in faceGroupCollection.GetAsset<FaceGroupCollection>().faceGroups)
						{
							GenerateMeshes(faceGroup, generatedMeshes, meshOffsets);
						}
						break;
					case SourceType.FaceGroup:
						GenerateMeshes(faceGroup.GetAsset<FaceGroup>(), generatedMeshes, meshOffsets);
						break;
				}
			}

			if (meshes.Length < generatedMeshes.Count)
			{
				var newMeshes = new AssetDescriptor[generatedMeshes.Count];

				for (var i = 0; i < meshes.Length; ++i)
				{
					newMeshes[i] = meshes[i];
				}
				for (var i = meshes.Length; i < newMeshes.Length; ++i)
				{
					newMeshes[i] = AssetDescriptor.Create(this, typeof(Mesh), "Mesh", "Meshes");
				}

				meshes = newMeshes;
			}
			else if (meshes.Length > generatedMeshes.Count)
			{
				var newMeshes = new AssetDescriptor[generatedMeshes.Count];

				for (var i = 0; i < newMeshes.Length; ++i)
				{
					newMeshes[i] = meshes[i];
				}

				meshes = newMeshes;
			}

			if (meshes.Length == 1)
			{
				meshes[0].name = "Mesh";
			}
			else if (meshes.Length > 1)
			{
				for (int i = 0; i < meshes.Length; ++i)
				{
					meshes[i].name = string.Format("Submesh {0}", i);
				}
			}

			for (int i = 0; i < meshes.Length; ++i)
			{
				meshes[i].SetAsset(generatedMeshes[i]);
			}

			var meshCollectionAsset = MeshCollection.Create(meshes.Length);
			for (int i = 0; i < meshes.Length; ++i)
			{
				meshCollectionAsset.meshes[i] = new MeshCollection.OrientedMesh(meshes[i].GetAsset<Mesh>(), meshOffsets[i]);
			}
			meshCollection.SetAsset(meshCollectionAsset);
		}

		private void GenerateMeshes(IEnumerable<Topology.Face> faces, List<Mesh> meshes, List<Vector3> offsets)
		{
			if (vertexPositions.asset is IVertexAttribute<Vector3>)
			{
				foreach (var mesh in MeshBuilder.BuildMeshes(
					faces,
					vertexPositions.GetAsset<IVertexAttribute<Vector3>>(),
					faceCentroids.GetAsset<IFaceAttribute<Vector3>>(),
					faceNormals.GetAsset<IFaceAttribute<Vector3>>(),
					ConstantColorFaceAttribute.CreateInstance(Color.white)))
				{
					meshes.Add(mesh);
					offsets.Add(new Vector3(0f, 0f, 0f));
				}
			}
			else if (vertexPositions.asset is IEdgeAttribute<Vector3>)
			{
				foreach (var mesh in MeshBuilder.BuildMeshes(
					faces,
					vertexPositions.GetAsset<IEdgeAttribute<Vector3>>(),
					faceCentroids.GetAsset<IFaceAttribute<Vector3>>(),
					faceNormals.GetAsset<IFaceAttribute<Vector3>>(),
					ConstantColorFaceAttribute.CreateInstance(Color.white)))
				{
					meshes.Add(mesh);
					offsets.Add(new Vector3(0f, 0f, 0f));
				}
			}
		}

		private void GenerateMeshes(IEnumerable<Topology.Face> faces, Vector3 groupAverage, List<Mesh> meshes, List<Vector3> offsets)
		{
			if (vertexPositions.asset is IVertexAttribute<Vector3>)
			{
				foreach (var mesh in MeshBuilder.BuildMeshes(
					faces,
					-groupAverage,
					vertexPositions.GetAsset<IVertexAttribute<Vector3>>(),
					faceCentroids.GetAsset<IFaceAttribute<Vector3>>(),
					faceNormals.GetAsset<IFaceAttribute<Vector3>>(),
					ConstantColorFaceAttribute.CreateInstance(Color.white)))
				{
					meshes.Add(mesh);
					offsets.Add(groupAverage);
				}
			}
			else if (vertexPositions.asset is IEdgeAttribute<Vector3>)
			{
				foreach (var mesh in MeshBuilder.BuildMeshes(
					faces,
					-groupAverage,
					vertexPositions.GetAsset<IEdgeAttribute<Vector3>>(),
					faceCentroids.GetAsset<IFaceAttribute<Vector3>>(),
					faceNormals.GetAsset<IFaceAttribute<Vector3>>(),
					ConstantColorFaceAttribute.CreateInstance(Color.white)))
				{
					meshes.Add(mesh);
					offsets.Add(groupAverage);
				}
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

		public override bool CanGenerate()
		{
			switch (sourceType)
			{
				case SourceType.InternalFaces:
					if (topology == null) return false;
					break;
				case SourceType.FaceGroupCollection:
					if (faceGroupCollection == null) return false;
					break;
				case SourceType.FaceGroup:
					if (faceGroup == null) return false;
					break;
			}

			return
				vertexPositions != null &&
				faceCentroids != null &&
				faceNormals != null;
		}
	}
}
