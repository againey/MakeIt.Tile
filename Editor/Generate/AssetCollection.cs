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
using System.Collections.Generic;

namespace MakeIt.Generate
{
	public class AssetCollection : ScriptableObject
	{
		public GeneratorExecutive executive;
		public List<Object> discreteAssets;
		public List<Object> embeddedAssets;

		public static AssetCollection Create(GeneratorExecutive executive)
		{
			var instance = CreateInstance<AssetCollection>();
			instance.executive = executive;
			instance.discreteAssets = new List<Object>();
			instance.embeddedAssets = new List<Object>();
			return instance;
		}

		public void AddDiscrete(Object asset)
		{
			if (!discreteAssets.Contains(asset))
			{
				embeddedAssets.Remove(asset);
				discreteAssets.Add(asset);
			}
		}

		public void AddEmbedded(Object asset)
		{
			if (!embeddedAssets.Contains(asset))
			{
				discreteAssets.Remove(asset);
				embeddedAssets.Add(asset);
			}
		}

		public void Remove(Object asset)
		{
			discreteAssets.Remove(asset);
			embeddedAssets.Remove(asset);
		}
	}
}
