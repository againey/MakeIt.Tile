using UnityEngine;

namespace Experilous
{
	public abstract class BaseRandomEngine : ScriptableObject, IRandomEngine
	{
		private static sbyte[] _log2CeilLookupTable = // Table[i] = Ceil(Log2(i))
		{
			0, 0, 1, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4,
			4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
			5, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
			6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
			6, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			7, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		};

		private static sbyte[] _plus1Log2CeilLookupTable = // Table[i] = Ceil(Log2(i+1))
		{
			0, 1, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4,
			5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
			6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
			6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
			7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
		};

		protected static int Log2Ceil(uint n)
		{
			var high16 = n >> 16;
			if (high16 != 0)
			{
				var high8 = high16 >> 8;
				return (high8 != 0) ? 24 + _log2CeilLookupTable[high8] : 16 + _log2CeilLookupTable[high16];
			}
			else
			{
				var high8 = n >> 8;
				return (high8 != 0) ? 8 + _log2CeilLookupTable[high8] : _log2CeilLookupTable[n];
			}
		}

		protected static int Plus1Log2Ceil(uint n)
		{
			var high16 = n >> 16;
			if (high16 != 0)
			{
				var high8 = high16 >> 8;
				return (high8 != 0) ? 24 + _plus1Log2CeilLookupTable[high8] : 16 + _plus1Log2CeilLookupTable[high16];
			}
			else
			{
				var high8 = n >> 8;
				return (high8 != 0) ? 8 + _plus1Log2CeilLookupTable[high8] : _plus1Log2CeilLookupTable[n];
			}
		}

		protected static int Log2Ceil(ulong n)
		{
			var high32 = n >> 32;
			if (high32 != 0)
			{
				return 32 + Log2Ceil((uint)high32);
			}
			else
			{
				return Log2Ceil((uint)n);
			}
		}

		protected static int Plus1Log2Ceil(ulong n)
		{
			var high32 = n >> 32;
			if (high32 != 0)
			{
				return 32 + Plus1Log2Ceil((uint)high32);
			}
			else
			{
				return Plus1Log2Ceil((uint)n);
			}
		}

		public abstract uint Next32();
		public abstract ulong Next64();

		public uint Next32(int bitCount)
		{
			if (bitCount == 0) return 0U;
			return Next32() & (uint.MaxValue >> (32 - bitCount));
		}

		public ulong Next64(int bitCount)
		{
			if (bitCount == 0) return 0UL;
			return Next64() & (ulong.MaxValue >> (64 - bitCount));
		}

		public uint NextLessThan(uint upperBound)
		{
			if (upperBound == 0) throw new System.ArgumentOutOfRangeException("upperBound");
			var bitsNeeded = Log2Ceil(upperBound);
			uint random;
			int count = 0;
			do
			{
				random = Next32(bitsNeeded);
				if (++count > 1000) throw new System.InvalidOperationException(string.Format("{0}, {1}, {2}", upperBound, random, bitsNeeded));
			}
			while (random >= upperBound);
			return random;
		}

		public uint NextLessThanOrEqual(uint upperBound)
		{
			var bitsNeeded = Plus1Log2Ceil(upperBound);
			uint random;
			do
			{
				random = Next32(bitsNeeded);
			}
			while (random > upperBound);
			return random;
		}

		public ulong NextLessThan(ulong upperBound)
		{
			if (upperBound == 0) throw new System.ArgumentOutOfRangeException("upperBound");
			var bitsNeeded = Log2Ceil(upperBound);
			ulong random;
			do
			{
				random = Next64(bitsNeeded);
			}
			while (random >= upperBound);
			return random;
		}

		public ulong NextLessThanOrEqual(ulong upperBound)
		{
			var bitsNeeded = Plus1Log2Ceil(upperBound);
			ulong random;
			do
			{
				random = Next64(bitsNeeded);
			}
			while (random > upperBound);
			return random;
		}
	}
}
