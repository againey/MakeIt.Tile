/******************************************************************************\
* Copyright Andy Gainey                                                        *
*                                                                              *
* Licensed under the Apache License, Version 2.0 (the "License");              *
* you may not use this file except in compliance with the License.             *
* You may obtain a copy of the License at                                      *
*                                                                              *
*     http://www.apache.org/licenses/LICENSE-2.0                               *
*                                                                              *
* Unless required by applicable law or agreed to in writing, software          *
* distributed under the License is distributed on an "AS IS" BASIS,            *
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.     *
* See the License for the specific language governing permissions and          *
* limitations under the License.                                               *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MakeIt.Generate;

namespace MakeIt.Tile
{
	[Generator(typeof(TopologyGeneratorCollection), "Unity Asset/Prefab")]
	public class PrefabGenerator : Generator, ISerializationCallbackReceiver
	{
		public InputSlot dynamicMeshInputSlot;

		public MeshFilter meshPrefab;

		public OutputSlot prefabOutputSlot;

		protected override void Initialize()
		{
			// Inputs
			InputSlot.CreateOrResetRequired<DynamicMesh>(ref dynamicMeshInputSlot, this);

			// Fields
			meshPrefab = null;

			// Outputs
			OutputSlot.CreateOrReset<GameObject>(ref prefabOutputSlot, this, "Prefab");
		}

		public void OnAfterDeserialize()
		{
			InputSlot.ResetAssetTypeIfNull<DynamicMesh>(dynamicMeshInputSlot);

			OutputSlot.ResetAssetTypeIfNull<GameObject>(prefabOutputSlot);
		}

		public void OnBeforeSerialize()
		{
		}

		public override IEnumerable<InputSlot> inputs
		{
			get
			{
				yield return dynamicMeshInputSlot;
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

			var dynamicMesh = dynamicMeshInputSlot.GetAsset<DynamicMesh>();

			if (dynamicMesh.submeshCount == 0)
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
			else if (dynamicMesh.submeshCount == 1)
			{
				if (meshPrefab == null)
				{
					prefab = new GameObject();
					prefab.AddComponent<MeshFilter>().mesh = dynamicMesh.GetSubmesh(0);
					prefab.AddComponent<MeshRenderer>();
				}
				else
				{
					var meshFilter = Instantiate(meshPrefab);
					meshFilter.mesh = dynamicMesh.GetSubmesh(0);
					prefab = meshFilter.gameObject;
				}
			}
			else
			{
				prefab = new GameObject();
				if (meshPrefab == null)
				{
					for (int i = 0; i < dynamicMesh.submeshCount; ++i)
					{
						var submeshInstance = new GameObject();
						submeshInstance.name = string.Format("Mesh {0}", i);
						submeshInstance.AddComponent<MeshFilter>().mesh = dynamicMesh.GetSubmesh(i);
						submeshInstance.AddComponent<MeshRenderer>();
						submeshInstance.transform.SetParent(prefab.transform, false);
					}
				}
				else
				{
					for (int i = 0; i < dynamicMesh.submeshCount; ++i)
					{
						var meshFilter = Instantiate(meshPrefab);
						meshFilter.name = string.Format("Mesh {0}", i);
						meshFilter.mesh = dynamicMesh.GetSubmesh(i);
						meshFilter.transform.SetParent(prefab.transform, false);
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
