using UnityEngine;

namespace Experilous
{
	// SplitMix64 PRNG, from http://xorshift.di.unimi.it/splitmix64.c
	// Algorithm by Sebastiano Vigna
	public class SplitMix64 : BaseRandomEngine, IRandomEngine
	{
		[SerializeField] private ulong _state;

		public static SplitMix64 Create()
		{
			var instance = CreateInstance<SplitMix64>();
			instance._state = RandomSeedUtility.Seed64();
			return instance;
		}

		public static SplitMix64 Create(int seed)
		{
			var instance = CreateInstance<SplitMix64>();
			instance._state = RandomSeedUtility.Seed64(seed);
			return instance;
		}

		public static SplitMix64 Create(params int[] seed)
		{
			var instance = CreateInstance<SplitMix64>();
			instance._state = RandomSeedUtility.Seed64(seed);
			return instance;
		}

		public static SplitMix64 Create(string seed)
		{
			var instance = CreateInstance<SplitMix64>();
			instance._state = RandomSeedUtility.Seed64(seed);
			return instance;
		}

		public static SplitMix64 Create(IRandomEngine seeder)
		{
			var instance = CreateInstance<SplitMix64>();
			instance._state = seeder.Next64();
			return instance;
		}

		public override uint Next32()
		{
			return (uint)Next64();
		}

		public override ulong Next64()
		{
			_state += 0x9E3779B97F4A7C15UL;
			var z = _state;
			z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
			z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
			return z ^ (z >> 31);
		}
	}
}
