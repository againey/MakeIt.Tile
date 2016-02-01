using UnityEngine;

namespace Experilous
{
	public class XorShift128Plus : BaseRandomEngine, IRandomEngine
	{
		[SerializeField] private ulong _state0;
		[SerializeField] private ulong _state1;

		public static XorShift128Plus Create()
		{
			var instance = CreateInstance<XorShift128Plus>();
			instance.Seed();
			return instance;
		}

		public static XorShift128Plus Create(int seed)
		{
			var instance = CreateInstance<XorShift128Plus>();
			instance.Seed(seed);
			return instance;
		}

		public static XorShift128Plus Create(params int[] seed)
		{
			var instance = CreateInstance<XorShift128Plus>();
			instance.Seed(seed);
			return instance;
		}

		public static XorShift128Plus Create(string seed)
		{
			var instance = CreateInstance<XorShift128Plus>();
			instance.Seed(seed);
			return instance;
		}

		public static XorShift128Plus Create(IRandomEngine seeder)
		{
			var instance = CreateInstance<XorShift128Plus>();
			instance.Seed(seeder);
			return instance;
		}

		public void Seed()
		{
			Seed(SplitMix64.Create());
		}

		public void Seed(int seed)
		{
			Seed(SplitMix64.Create(seed));
		}

		public void Seed(params int[] seed)
		{
			Seed(SplitMix64.Create(seed));
		}

		public void Seed(string seed)
		{
			Seed(SplitMix64.Create(seed));
		}

		public void Seed(IRandomEngine seeder)
		{
			_state0 = seeder.Next64();
			_state1 = seeder.Next64();
		}

		public void MergeSeed()
		{
			MergeSeed(SplitMix64.Create());
		}

		public void MergeSeed(int seed)
		{
			MergeSeed(SplitMix64.Create(seed));
		}

		public void MergeSeed(params int[] seed)
		{
			MergeSeed(SplitMix64.Create(seed));
		}

		public void MergeSeed(string seed)
		{
			MergeSeed(SplitMix64.Create(seed));
		}

		public void MergeSeed(IRandomEngine seeder)
		{
			_state0 ^= seeder.Next64();
			_state1 ^= seeder.Next64();
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

		public System.Random AsSystemRandom()
		{
			return new SystemRandomWrapper64(this);
		}
	}
}
