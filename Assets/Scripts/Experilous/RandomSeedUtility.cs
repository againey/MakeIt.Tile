using System;
using System.Text;

namespace Experilous
{
	public static class RandomSeedUtility
	{
		#region 32-bit Seeds

		public static uint Seed32()
		{
			return Seed32(Environment.TickCount);
		}

		public static uint Seed32(int data)
		{
			return Seed32(BitConverter.GetBytes(data));
		}

		public static uint Seed32(int[] data)
		{
			var byteData = new byte[data.Length * 4];
			Buffer.BlockCopy(data, 0, byteData, 0, byteData.Length);
			return Seed32(byteData);
		}

		public static uint Seed32(string data)
		{
			return Seed32(new UTF8Encoding().GetBytes(data));
		}

		// FNV-1a hash function, from http://www.isthe.com/chongo/tech/comp/fnv/
		// Algorithm by Glenn Fowler, Phong Vo, and Landon Curt Noll
		public static uint Seed32(byte[] data)
		{
			uint h = 2166136261U;
			for (int i = 0; i < data.Length; ++i)
			{
				h = (h ^ data[i]) * 16777619U;
			}
			return h;
		}

		#endregion

		#region 64-bit Seeds

		public static ulong Seed64()
		{
			return Seed64(Environment.TickCount);
		}

		public static ulong Seed64(int data)
		{
			return Seed64(BitConverter.GetBytes(data));
		}

		public static ulong Seed64(int[] data)
		{
			var byteData = new byte[data.Length * 4];
			Buffer.BlockCopy(data, 0, byteData, 0, byteData.Length);
			return Seed64(byteData);
		}

		public static ulong Seed64(string data)
		{
			return Seed64(new UTF8Encoding().GetBytes(data));
		}

		// FNV-1a hash function, from http://www.isthe.com/chongo/tech/comp/fnv/
		// Algorithm by Glenn Fowler, Phong Vo, and Landon Curt Noll
		public static ulong Seed64(byte[] data)
		{
			ulong h = 14695981039346656037UL;
			for (int i = 0; i < data.Length; ++i)
			{
				h = (h ^ data[i]) * 1099511628211UL;
			}
			return h;
		}

		#endregion
	}
}
