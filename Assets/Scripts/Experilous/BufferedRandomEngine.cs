namespace Experilous
{
	public abstract class BufferedRandomEngine : IRandomEngine
	{
		private uint _buffer = 0;
		private int _bufferBitCount = 0;

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

		public abstract uint Next();

		public uint NextBits(int bitCount)
		{
			if (_bufferBitCount < bitCount)
			{
				int existingBitCount = bitCount - _bufferBitCount;
				int remainingBitCount = 32 - existingBitCount;
				uint next = Next();
				uint random = (_buffer << existingBitCount) | (next >> remainingBitCount);
				_buffer = next & ((1U << remainingBitCount) - 1U);
				_bufferBitCount = remainingBitCount;
				return random;
			}
			else
			{
				int remainingBitCount = _bufferBitCount - bitCount;
				uint random = _buffer >> remainingBitCount;
				_buffer = _buffer & ((1U << remainingBitCount) - 1U);
				_bufferBitCount = remainingBitCount;
				return random;
			}
		}

		private static int Log2Ceil(uint n)
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

		private static int Plus1Log2Ceil(uint n)
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

		public uint NextLessThan(uint upperBound)
		{
			if (upperBound == 0) throw new System.ArgumentOutOfRangeException("upperBound");
			var bitsNeeded = Log2Ceil(upperBound);
			uint random;
			do
			{
				random = NextBits(bitsNeeded);
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
				random = NextBits(bitsNeeded);
			}
			while (random > upperBound);
			return random;
		}
	}
}
