using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorCollection), typeof(MeshCategory), "Prefab")]
	public class PrefabGenerator : AssetGenerator
	{
		public AssetInputSlot meshCollectionInputSlot;

		public MeshFilter meshPrefab;

		public AssetDescriptor prefabDescriptor;

		protected override void Initialize(bool reset = true)
		{
			// Inputs
			if (reset || meshCollectionInputSlot == null) meshCollectionInputSlot = AssetInputSlot.CreateRequired(this, typeof(MeshCollection));

			// Outputs
			if (reset || prefabDescriptor == null) prefabDescriptor = AssetDescriptor.Create<GameObject>(this, "Prefab");
		}

		public override IEnumerable<AssetInputSlot> inputs
		{
			get
			{
				yield return meshCollectionInputSlot;
			}
		}

		public override IEnumerable<AssetDescriptor> outputs
		{
			get
			{
				yield return prefabDescriptor;
			}
		}

		public override void Generate()
		{
			GameObject prefab;

			var meshes = meshCollectionInputSlot.GetAsset<MeshCollection>().meshes;

			if (meshes.Length == 0)
			{
				if (meshPrefab == null)
				{
					prefab = new GameObject();
					prefab.AddComponent<MeshFilter>();
					prefab.AddComponent<MeshRenderer>();
				}
				else
				{
					prefab = Instantiate(meshPrefab).gameObject;
				}
			}
			else if (meshes.Length == 1)
			{
				if (meshPrefab == null)
				{
					prefab = new GameObject();
					prefab.AddComponent<MeshFilter>().mesh = meshes[0].mesh;
					prefab.AddComponent<MeshRenderer>();
				}
				else
				{
					var meshFilter = Instantiate(meshPrefab);
					meshFilter.mesh = meshes[0].mesh;
					prefab = meshFilter.gameObject;
				}
				prefab.transform.position = meshes[0].position;
				prefab.transform.rotation = meshes[0].rotation;
			}
			else
			{
				prefab = new GameObject();
				if (meshPrefab == null)
				{
					for (int i = 0; i < meshes.Length; ++i)
					{
						var submeshInstance = new GameObject();
						submeshInstance.name = string.Format("Mesh {0}", i);
						submeshInstance.AddComponent<MeshFilter>().mesh = meshes[i].mesh;
						submeshInstance.AddComponent<MeshRenderer>();
						submeshInstance.transform.SetParent(prefab.transform, false);
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
						meshFilter.transform.SetParent(prefab.transform, false);
						meshFilter.transform.position = meshes[i].position;
						meshFilter.transform.rotation = meshes[i].rotation;
					}
				}
			}

			prefab.name = name;

			prefabDescriptor.SetAsset(prefab);
		}
	}
}
