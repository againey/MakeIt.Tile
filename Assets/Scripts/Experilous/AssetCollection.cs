using UnityEngine;
using System.Collections.Generic;

namespace Experilous
{
	public class AssetCollection : ScriptableObject
	{
		public List<Object> discreteAssets;
		public List<Object> embeddedAssets;

		public static AssetCollection Create()
		{
			var instance = CreateInstance<AssetCollection>();
			instance.discreteAssets = new List<Object>();
			instance.embeddedAssets = new List<Object>();
			return instance;
		}

		public static AssetCollection Create(string name)
		{
			var instance = Create();
			instance.name = name;
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
