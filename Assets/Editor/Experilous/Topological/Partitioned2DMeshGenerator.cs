using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorBundle), typeof(MeshCategory), "Partitioned 2D Mesh")]
	public class Partitioned2DMeshGenerator : AssetGenerator
	{
		public int horizontalPartitionCount = 1;
		public int verticalPartitionCount = 1;

		public bool generatePrefab;
		public MeshFilter meshGameObjectPrefab;

		public TopologyGeneratedAsset topology;
		public Vector3VertexAttributeGeneratedAsset vertexPositions;
		public Vector3FaceAttributeGeneratedAsset faceCentroids;
		public Vector3FaceAttributeGeneratedAsset faceNormals;

		public MeshGeneratedAsset[] meshes;
		public GameObjectGeneratedAsset prefab;

		public static EditorMeshGenerator CreateDefaultInstance(AssetGeneratorBundle bundle, string name)
		{
			var generator = CreateInstance<EditorMeshGenerator>();
			generator.bundle = bundle;
			generator.name = name;
			generator.hideFlags = HideFlags.HideInHierarchy;
			return generator;
		}

		public override IEnumerable<GeneratedAsset> dependencies
		{
			get
			{
				if (topology != null) yield return topology;
				if (vertexPositions != null) yield return vertexPositions;
				if (faceNormals != null) yield return faceNormals;
				if (faceCentroids != null) yield return faceCentroids;
			}
		}

		public override IEnumerable<GeneratedAsset> outputs
		{
			get
			{
				if (meshes == null || meshes.Length == 0)
				{
					meshes = new MeshGeneratedAsset[1] {  MeshGeneratedAsset.CreateDefaultInstance(this, "Mesh") };
				}

				foreach (var mesh in meshes)
				{
					yield return mesh;
				}

				if (generatePrefab)
				{
					if (prefab == null) prefab = GameObjectGeneratedAsset.CreateDefaultInstance(this, "Prefab");
					yield return prefab;
				}
				else
				{
					prefab = null;
				}
			}
		}

		public override void ResetDependency(GeneratedAsset dependency)
		{
			if (dependency == null) throw new System.ArgumentNullException("dependency");
			if (!ResetMemberDependency(dependency, ref topology, ref vertexPositions, ref faceCentroids, ref faceNormals))
			{
				throw new System.ArgumentException(string.Format("Generated asset \"{0}\" of type {1} is not a dependency of this mesh generator.", dependency.name, dependency.GetType().Name), "dependency");
			}
		}

		public override void Generate(string location, string name)
		{
#if false
			var generatedMeshes = MeshGenerator.GenerateSubmeshes(
				topology.generatedInstance.internalFaces,
				vertexPositions.generatedInstance.array,
				(faceCentroids.generatedInstance).array,
				(faceNormals.generatedInstance).array,
				(int i) => { return Color.white; });

			if (meshes.Length < generatedMeshes.Length)
			{
				var newMeshes = new MeshGeneratedAsset[generatedMeshes.Length];

				for (var i = 0; i < meshes.Length; ++i)
				{
					newMeshes[i] = meshes[i];
				}
				for (var i = meshes.Length; i < newMeshes.Length; ++i)
				{
					newMeshes[i] = MeshGeneratedAsset.CreateDefaultInstance(this, "Mesh");
				}

				meshes = newMeshes;
			}
			else if (meshes.Length > generatedMeshes.Length)
			{
				var newMeshes = new MeshGeneratedAsset[generatedMeshes.Length];

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
				meshes[i].SetGeneratedInstance(location, name, generatedMeshes[i]);
			}

			if (generatePrefab)
			{
				GameObject prefabInstance;

				if (meshes.Length == 0)
				{
					if (meshGameObjectPrefab == null)
					{
						prefabInstance = new GameObject();
						prefabInstance.AddComponent<MeshFilter>();
						prefabInstance.AddComponent<MeshRenderer>();
					}
					else
					{
						prefabInstance = Instantiate(meshGameObjectPrefab).gameObject;
					}
				}
				else if (meshes.Length == 1)
				{
					if (meshGameObjectPrefab == null)
					{
						prefabInstance = new GameObject();
						prefabInstance.AddComponent<MeshFilter>().mesh = meshes[0].generatedInstance;
						prefabInstance.AddComponent<MeshRenderer>();
					}
					else
					{
						var meshFilter = Instantiate(meshGameObjectPrefab);
						meshFilter.mesh = meshes[0].generatedInstance;
						prefabInstance = meshFilter.gameObject;
					}
				}
				else
				{
					prefabInstance = new GameObject();
					if (meshGameObjectPrefab == null)
					{
						for (int i = 0; i < meshes.Length; ++i)
						{
							var submeshInstance = new GameObject();
							submeshInstance.name = string.Format("Submesh {0}", i);
							submeshInstance.AddComponent<MeshFilter>().mesh = meshes[i].generatedInstance;
							submeshInstance.AddComponent<MeshRenderer>();
							submeshInstance.transform.SetParent(prefabInstance.transform, false);
						}
					}
					else
					{
						for (int i = 0; i < meshes.Length; ++i)
						{
							var meshFilter = Instantiate(meshGameObjectPrefab);
							meshFilter.name = string.Format("Submesh {0}", i);
							meshFilter.mesh = meshes[i].generatedInstance;
							meshFilter.transform.SetParent(prefabInstance.transform, false);
						}
					}
				}

				prefabInstance.name = name;

				prefab.SetGeneratedInstance(location, name, prefabInstance);
			}
			else
			{
				prefab = null;
			}
#endif
		}

		public override bool CanGenerate()
		{
			return
				topology != null &&
				vertexPositions != null &&
				faceCentroids != null &&
				faceNormals != null;
		}
	}
}
