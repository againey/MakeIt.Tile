using UnityEngine;

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
			return (upperExclusive - lowerInclusive) * HalfOpenFloatUnit(engine) + lowerInclusive;
		}

		public float HalfOpenRange(float lowerInclusive, float upperExclusive)
		{
			return HalfOpenRange(lowerInclusive, upperExclusive, _engine);
		}

		public static float HalfOpenRange(float upperExclusive, IRandomEngine engine)
		{
			return upperExclusive * HalfOpenFloatUnit(engine);
		}

		public float HalfOpenRange(float upperExclusive)
		{
			return HalfOpenRange(upperExclusive, _engine);
		}

		public static double HalfOpenRange(double lowerInclusive, double upperExclusive, IRandomEngine engine)
		{
			return (upperExclusive - lowerInclusive) * HalfOpenDoubleUnit(engine) + lowerInclusive;
		}

		public double HalfOpenRange(double lowerInclusive, double upperExclusive)
		{
			return HalfOpenRange(lowerInclusive, upperExclusive, _engine);
		}

		public static double HalfOpenRange(double upperExclusive, IRandomEngine engine)
		{
			return upperExclusive * HalfOpenDoubleUnit(engine);
		}

		public double HalfOpenRange(double upperExclusive)
		{
			return HalfOpenRange(upperExclusive, _engine);
		}

		public static float HalfOpenFloatUnit(IRandomEngine engine)
		{
			return (float)System.BitConverter.Int64BitsToDouble((long)(0x3FF0000000000000UL | 0x000FFFFFE0000000UL & ((ulong)engine.Next32() << 29))) - 1.0f;
		}

		public float HalfOpenFloatUnit()
		{
			return HalfOpenFloatUnit(_engine);
		}

		public static double HalfOpenDoubleUnit(IRandomEngine engine)
		{
			return System.BitConverter.Int64BitsToDouble((long)(0x3FF0000000000000UL | 0x000FFFFFFFFFFFFFUL & engine.Next64())) - 1.0;
		}

		public double HalfOpenDoubleUnit()
		{
			return HalfOpenDoubleUnit(_engine);
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

		public static float ClosedRange(float lowerInclusive, float upperInclusive, IRandomEngine engine)
		{
			return (upperInclusive - lowerInclusive) * ClosedFloatUnit(engine) + lowerInclusive;
		}

		public float ClosedRange(float lowerInclusive, float upperInclusive)
		{
			return ClosedRange(lowerInclusive, upperInclusive, _engine);
		}

		public static float ClosedRange(float upperInclusive, IRandomEngine engine)
		{
			return upperInclusive * ClosedFloatUnit(engine);
		}

		public float ClosedRange(float upperInclusive)
		{
			return ClosedRange(upperInclusive, _engine);
		}

		public static double ClosedRange(double lowerInclusive, double upperInclusive, IRandomEngine engine)
		{
			return (upperInclusive - lowerInclusive) * ClosedDoubleUnit(engine) + lowerInclusive;
		}

		public double ClosedRange(double lowerInclusive, double upperInclusive)
		{
			return ClosedRange(lowerInclusive, upperInclusive, _engine);
		}

		public static double ClosedRange(double upperInclusive, IRandomEngine engine)
		{
			return upperInclusive * ClosedDoubleUnit(engine);
		}

		public double ClosedRange(double upperInclusive)
		{
			return ClosedRange(upperInclusive, _engine);
		}

		public static float ClosedFloatUnit(IRandomEngine engine)
		{
			var random = engine.NextLessThanOrEqual(0x00800000U);
			return (random != 0x00800000U) ? (float)System.BitConverter.Int64BitsToDouble((long)(0x3FF0000000000000UL | 0x000FFFFFE0000000UL & ((ulong)random << 29))) - 1.0f : 1.0f;
		}

		public float ClosedFloatUnit()
		{
			return ClosedFloatUnit(_engine);
		}

		public static double ClosedDoubleUnit(IRandomEngine engine)
		{
			var random = engine.NextLessThanOrEqual(0x0010000000000000UL);
			return (random != 0x0010000000000000UL) ? System.BitConverter.Int64BitsToDouble((long)(0x3FF0000000000000UL | 0x000FFFFFE0000000UL & random)) - 1.0 : 1.0;
		}

		public double ClosedDoubleUnit()
		{
			return ClosedDoubleUnit(_engine);
		}

		public static Vector2 UnitVector2(IRandomEngine engine)
		{
			var angle = HalfOpenRange(0f, Mathf.PI * 2f, engine);
			return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
		}

		public Vector2 UnitVector2()
		{
			return UnitVector2(_engine);
		}

		public string HexadecimalString(int length)
		{
			char[] buffer = new char[length];
			for (int i = 0; i < length; ++i)
			{
				buffer[i] = _hexadecimalCharacters[_engine.Next32(4)];
			}
			return new string(buffer);
		}
	}
}
