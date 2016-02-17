using UnityEngine;

namespace Experilous.Generation
{
	public static class GeneratorUtility
	{
		public static OutputSlot[] ResizeArray(OutputSlot[] oldArray, int newSize)
		{
			if (oldArray == null)
			{
				return new OutputSlot[newSize];
			}
			else if (oldArray.Length != newSize)
			{
				var newArray = new OutputSlot[newSize];
				if (oldArray.Length < newSize)
				{
					System.Array.Copy(oldArray, newArray, oldArray.Length);
				}
				else
				{
					System.Array.Copy(oldArray, newArray, newSize);
					for (int i = newSize; i < oldArray.Length; ++i)
					{
						oldArray[i].DisconnectAll();
					}
				}
				return newArray;
			}
			else
			{
				return oldArray;
			}
		}

		public static OutputSlot[] ResizeArray(OutputSlot[] oldArray, int newSize, System.Func<int, OutputSlot> initializer)
		{
			if (oldArray == null)
			{
				var newArray = new OutputSlot[newSize];
				for (int i = 0; i < newSize; ++i)
				{
					newArray[i] = initializer(i);
				}
				return newArray;
			}
			else if (oldArray.Length != newSize)
			{
				var newArray = new OutputSlot[newSize];
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
					for (int i = newSize; i < oldArray.Length; ++i)
					{
						oldArray[i].DisconnectAll();
					}
				}
				return newArray;
			}
			else
			{
				return oldArray;
			}
		}

		public static OutputSlot[] ResizeArray(OutputSlot[] oldArray, int newSize, System.Func<int, OutputSlot> initializer, System.Func<OutputSlot, int, OutputSlot> updater)
		{
			if (oldArray == null)
			{
				var newArray = new OutputSlot[newSize];
				for (int i = 0; i < newSize; ++i)
				{
					newArray[i] = initializer(i);
				}
				return newArray;
			}
			else if (oldArray.Length != newSize)
			{
				var newArray = new OutputSlot[newSize];
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
					for (int i = newSize; i < oldArray.Length; ++i)
					{
						oldArray[i].DisconnectAll();
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
