using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorBundle), typeof(MeshCategory), "Prefab")]
	public class PrefabGenerator : AssetGenerator
	{
		public MeshFilter meshPrefab;

		public AssetDescriptor meshCollection;

		public AssetDescriptor prefab;

		public static PrefabGenerator CreateDefaultInstance(AssetGeneratorBundle bundle, string name)
		{
			var generator = CreateInstance<PrefabGenerator>();
			generator.bundle = bundle;
			generator.name = name;
			generator.hideFlags = HideFlags.HideInHierarchy;
			return generator;
		}

		public override IEnumerable<AssetDescriptor> dependencies
		{
			get
			{
				if (meshCollection != null) yield return meshCollection;
			}
		}

		public override IEnumerable<AssetDescriptor> outputs
		{
			get
			{
				if (prefab == null) prefab = AssetDescriptor.Create(this, typeof(GameObject), "Prefab");

				yield return prefab;
			}
		}

		public override void ResetDependency(AssetDescriptor dependency)
		{
			if (!ResetMemberDependency(dependency, ref meshCollection))
			{
				throw new System.ArgumentException(string.Format("Generated asset \"{0}\" of type {1} is not a dependency of this prefab generator.", dependency.name, dependency.GetType().Name), "dependency");
			}
		}

		public override void Generate()
		{
			GameObject prefabInstance;

			var meshes = meshCollection.GetAsset<MeshCollection>().meshes;

			if (meshes.Length == 0)
			{
				if (meshPrefab == null)
				{
					prefabInstance = new GameObject();
					prefabInstance.AddComponent<MeshFilter>();
					prefabInstance.AddComponent<MeshRenderer>();
				}
				else
				{
					prefabInstance = Instantiate(meshPrefab).gameObject;
				}
			}
			else if (meshes.Length == 1)
			{
				if (meshPrefab == null)
				{
					prefabInstance = new GameObject();
					prefabInstance.AddComponent<MeshFilter>().mesh = meshes[0].mesh;
					prefabInstance.AddComponent<MeshRenderer>();
				}
				else
				{
					var meshFilter = Instantiate(meshPrefab);
					meshFilter.mesh = meshes[0].mesh;
					prefabInstance = meshFilter.gameObject;
				}
				prefabInstance.transform.position = meshes[0].position;
				prefabInstance.transform.rotation = meshes[0].rotation;
			}
			else
			{
				prefabInstance = new GameObject();
				if (meshPrefab == null)
				{
					for (int i = 0; i < meshes.Length; ++i)
					{
						var submeshInstance = new GameObject();
						submeshInstance.name = string.Format("Mesh {0}", i);
						submeshInstance.AddComponent<MeshFilter>().mesh = meshes[i].mesh;
						submeshInstance.AddComponent<MeshRenderer>();
						submeshInstance.transform.SetParent(prefabInstance.transform, false);
						submeshInstance.transform.position = meshes[i].position;
						submeshInstance.transform.rotation = meshes[i].rotation;
					}
				}
				else
				{
					for (int i = 0; i < meshes.Length; ++i)
					{
						var meshFilter = Instantiate(meshPrefab);
						meshFilter.name = string.Format("Mesh {0}", i);
						meshFilter.mesh = meshes[i].mesh;
						meshFilter.transform.SetParent(prefabInstance.transform, false);
						meshFilter.transform.position = meshes[i].position;
						meshFilter.transform.rotation = meshes[i].rotation;
					}
				}
			}

			prefabInstance.name = name;

			prefab.SetAsset(prefabInstance);
		}

		public override bool CanGenerate()
		{
			return meshCollection != null;
		}
	}
}
