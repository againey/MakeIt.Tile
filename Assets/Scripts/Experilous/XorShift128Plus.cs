using UnityEngine;

namespace Experilous
{
	public class XorShift128Plus : BaseRandomEngine, IRandomEngine
	{
		[SerializeField] private ulong _state0;
		[SerializeField] private ulong _state1;

		public static XorShift128Plus Create()
		{
			return Create(SplitMix64.Create());
		}

		public static XorShift128Plus Create(int seed)
		{
			return Create(SplitMix64.Create(seed));
		}

		public static XorShift128Plus Create(params int[] seed)
		{
			return Create(SplitMix64.Create(seed));
		}

		public static XorShift128Plus Create(string seed)
		{
			return Create(SplitMix64.Create(seed));
		}

		public static XorShift128Plus Create(IRandomEngine seeder)
		{
			var instance = CreateInstance<XorShift128Plus>();
			instance._state0 = seeder.Next64();
			instance._state1 = seeder.Next64();
			return instance;
		}

		public override uint Next32()
		{
			return (uint)Next64();
		}

		public override ulong Next64()
		{
			var x = _state0;
			var y = _state1;
			_state0 = y;
			x ^= x << 23;
			_state1 = x ^ y ^ (x >> 17) ^ (y >> 26);
			return _state1 + y;
		}
	}
}
