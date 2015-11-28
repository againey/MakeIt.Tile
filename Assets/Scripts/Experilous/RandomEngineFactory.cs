using UnityEngine;

namespace Experilous
{
	public abstract class RandomEngineFactory : MonoBehaviour
	{
		public abstract IRandomEngine Create();
		public abstract IRandomEngine Create(int seed);
		public abstract IRandomEngine Create(params int[] seed);
		public abstract IRandomEngine Create(string seed);

		public static int Hash(int[] data)
		{
			var byteData = new byte[data.Length * 4];
			System.Buffer.BlockCopy(data, 0, byteData, 0, byteData.Length);
			return Hash(byteData);
		}

		public static int Hash(string data)
		{
			return Hash(new System.Text.UTF8Encoding().GetBytes(data));
		}

		// FNV-1a hash function, from http://www.isthe.com/chongo/tech/comp/fnv/
		public static int Hash(byte[] data)
		{
			uint h = 2166136261U;
			for (int i = 0; i < data.Length; ++i)
			{
				h = (h ^ data[i]) * 16777619U;
			}
			return (int)h;
		}
	}
}
