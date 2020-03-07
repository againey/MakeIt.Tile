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

namespace MakeIt.Generate
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
