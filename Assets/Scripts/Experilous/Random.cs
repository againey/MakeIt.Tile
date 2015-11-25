namespace Experilous
{
	public class Random
	{
		private IRandomEngine _engine;

		private char[] _hexadecimalCharacters =
		{
			'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F',
		};

		public Random(IRandomEngine engine)
		{
			if (engine == null) throw new System.ArgumentNullException("engine");
			_engine = engine;
		}

		public IRandomEngine engine { get { return _engine; } }

		public static int HalfOpenRange(int lowerInclusive, int upperExclusive, IRandomEngine engine)
		{
			return (int)engine.NextLessThan((uint)(upperExclusive - lowerInclusive)) + lowerInclusive;
		}

		public int HalfOpenRange(int lowerInclusive, int upperExclusive)
		{
			return HalfOpenRange(lowerInclusive, upperExclusive, _engine);
		}

		public static int HalfOpenRange(int upperExclusive, IRandomEngine engine)
		{
			return (int)engine.NextLessThan((uint)upperExclusive);
		}

		public int HalfOpenRange(int upperExclusive)
		{
			return HalfOpenRange(upperExclusive, _engine);
		}

		public static uint HalfOpenRange(uint lowerInclusive, uint upperExclusive, IRandomEngine engine)
		{
			return engine.NextLessThan(upperExclusive - lowerInclusive) + lowerInclusive;
		}

		public uint HalfOpenRange(uint lowerInclusive, uint upperExclusive)
		{
			return HalfOpenRange(lowerInclusive, upperExclusive, _engine);
		}

		public static uint HalfOpenRange(uint upperExclusive, IRandomEngine engine)
		{
			return engine.NextLessThan(upperExclusive);
		}

		public uint HalfOpenRange(uint upperExclusive)
		{
			return HalfOpenRange(upperExclusive, _engine);
		}

		public static float HalfOpenRange(float lowerInclusive, float upperExclusive, IRandomEngine engine)
		{
			return (engine.Next() / 4294967296f) * (upperExclusive - lowerInclusive) + lowerInclusive;
		}

		public float HalfOpenRange(float lowerInclusive, float upperExclusive)
		{
			return HalfOpenRange(lowerInclusive, upperExclusive, _engine);
		}

		public static float HalfOpenRange(float upperExclusive, IRandomEngine engine)
		{
			return (engine.Next() / 4294967296f) * upperExclusive;
		}

		public float HalfOpenRange(float upperExclusive)
		{
			return HalfOpenRange(upperExclusive, _engine);
		}

		public static float HalfOpenFloatUnit(IRandomEngine engine)
		{
			return engine.Next() / 4294967296f;
		}

		public float HalfOpenFloatUnit()
		{
			return HalfOpenFloatUnit(_engine);
		}

		public static int ClosedRange(int lowerInclusive, int upperInclusive, IRandomEngine engine)
		{
			return (int)engine.NextLessThanOrEqual((uint)(upperInclusive - lowerInclusive)) + lowerInclusive;
		}

		public int ClosedRange(int lowerInclusive, int upperInclusive)
		{
			return ClosedRange(lowerInclusive, upperInclusive, _engine);
		}

		public static int ClosedRange(int upperInclusive, IRandomEngine engine)
		{
			return (int)engine.NextLessThanOrEqual((uint)upperInclusive);
		}

		public int ClosedRange(int upperInclusive)
		{
			return ClosedRange(upperInclusive, _engine);
		}

		public static uint ClosedRange(uint lowerInclusive, uint upperInclusive, IRandomEngine engine)
		{
			return engine.NextLessThanOrEqual(upperInclusive - lowerInclusive) + lowerInclusive;
		}

		public uint ClosedRange(uint lowerInclusive, uint upperInclusive)
		{
			return ClosedRange(lowerInclusive, upperInclusive, _engine);
		}

		public static uint ClosedRange(uint upperInclusive, IRandomEngine engine)
		{
			return engine.NextLessThanOrEqual(upperInclusive);
		}

		public uint ClosedRange(uint upperExclusive)
		{
			return ClosedRange(upperExclusive, _engine);
		}

		public static float ClosedRange(float lowerInclusive, float upperExclusive, IRandomEngine engine)
		{
			return (engine.Next() / 4294967295f) * (upperExclusive - lowerInclusive) + lowerInclusive;
		}

		public float ClosedRange(float lowerInclusive, float upperExclusive)
		{
			return ClosedRange(lowerInclusive, upperExclusive, _engine);
		}

		public static float ClosedRange(float upperExclusive, IRandomEngine engine)
		{
			return (engine.Next() / 4294967295f) * upperExclusive;
		}

		public float ClosedRange(float upperExclusive)
		{
			return ClosedRange(upperExclusive, _engine);
		}

		public static float ClosedFloatUnit(IRandomEngine engine)
		{
			return engine.Next() / 4294967295f;
		}

		public float ClosedFloatUnit()
		{
			return ClosedFloatUnit(_engine);
		}

		public string HexadecimalString(int length)
		{
			char[] buffer = new char[length];
			for (int i = 0; i < length; ++i)
			{
				buffer[i] = _hexadecimalCharacters[_engine.NextBits(4)];
			}
			return new string(buffer);
		}
	}
}
