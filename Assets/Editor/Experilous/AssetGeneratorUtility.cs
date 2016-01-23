using UnityEngine;

namespace Experilous
{
	public static class AssetGeneratorUtility
	{
		public static AssetDescriptor[] ResizeArray(AssetDescriptor[] oldArray, int newSize)
		{
			if (oldArray == null)
			{
				return new AssetDescriptor[newSize];
			}
			else if (oldArray.Length != newSize)
			{
				var newArray = new AssetDescriptor[newSize];
				System.Array.Copy(oldArray, newArray, Mathf.Min(oldArray.Length, newSize));
				return newArray;
			}
			else
			{
				return oldArray;
			}
		}

		public static AssetDescriptor[] ResizeArray(AssetDescriptor[] oldArray, int newSize, System.Func<int, AssetDescriptor> initializer)
		{
			if (oldArray == null)
			{
				var newArray = new AssetDescriptor[newSize];
				for (int i = 0; i < newSize; ++i)
				{
					newArray[i] = initializer(i);
				}
				return newArray;
			}
			else if (oldArray.Length != newSize)
			{
				var newArray = new AssetDescriptor[newSize];
				if (oldArray.Length < newSize)
				{
					System.Array.Copy(oldArray, newArray, oldArray.Length);
					for (int i = oldArray.Length; i < newSize; ++i)
					{
						newArray[i] = initializer(i);
					}
				}
				else
				{
					System.Array.Copy(oldArray, newArray, newSize);
				}
				return newArray;
			}
			else
			{
				return oldArray;
			}
		}

		public static AssetDescriptor[] ResizeArray(AssetDescriptor[] oldArray, int newSize, System.Func<int, AssetDescriptor> initializer, System.Func<AssetDescriptor, int, AssetDescriptor> updater)
		{
			if (oldArray == null)
			{
				var newArray = new AssetDescriptor[newSize];
				for (int i = 0; i < newSize; ++i)
				{
					newArray[i] = initializer(i);
				}
				return newArray;
			}
			else if (oldArray.Length != newSize)
			{
				var newArray = new AssetDescriptor[newSize];
				if (oldArray.Length < newSize)
				{
					for (int i = 0; i < oldArray.Length; ++i)
					{
						newArray[i] = updater(oldArray[i], i);
					}
					for (int i = oldArray.Length; i < newSize; ++i)
					{
						newArray[i] = initializer(i);
					}
				}
				else
				{
					for (int i = 0; i < newSize; ++i)
					{
						newArray[i] = updater(oldArray[i], i);
					}
				}
				return newArray;
			}
			else
			{
				return oldArray;
			}
		}
	}
}
