/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Experilous.Generation;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorCollection), typeof(MeshCategory), "Prefab")]
	public class PrefabGenerator : Generator
	{
		public InputSlot meshCollectionInputSlot;

		public MeshFilter meshPrefab;

		public OutputSlot prefabOutputSlot;

		protected override void Initialize()
		{
			// Inputs
			InputSlot.CreateOrResetRequired<MeshCollection>(ref meshCollectionInputSlot, this);

			// Fields
			meshPrefab = null;

			// Outputs
			OutputSlot.CreateOrReset<GameObject>(ref prefabOutputSlot, this, "Prefab");
		}

		public override IEnumerable<InputSlot> inputs
		{
			get
			{
				yield return meshCollectionInputSlot;
			}
		}

		public override IEnumerable<OutputSlot> outputs
		{
			get
			{
				yield return prefabOutputSlot;
			}
		}

		public override IEnumerator BeginGeneration()
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

			prefabOutputSlot.SetAsset(prefab);

			yield break;
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
