/******************************************************************************\
* Copyright Andy Gainey                                                        *
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
