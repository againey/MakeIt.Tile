using UnityEngine;

namespace Experilous
{
	public class NativeRandomEngine : BaseRandomEngine
	{
		[SerializeField] private System.Random _random;
		[SerializeField] private byte[] _buffer = new byte[4];

		public static NativeRandomEngine Create()
		{
			var instance = CreateInstance<NativeRandomEngine>();
			instance._random = new System.Random();
			return instance;
		}

		public static NativeRandomEngine Create(int seed)
		{
			var instance = CreateInstance<NativeRandomEngine>();
			instance._random = new System.Random(seed);
			return instance;
		}

		public static NativeRandomEngine Create(params int[] seed)
		{
			var instance = CreateInstance<NativeRandomEngine>();
			instance._random = new System.Random((int)RandomSeedUtility.Seed32(seed));
			return instance;
		}

		public static NativeRandomEngine Create(string seed)
		{
			var instance = CreateInstance<NativeRandomEngine>();
			instance._random = new System.Random((int)RandomSeedUtility.Seed32(seed));
			return instance;
		}

		public static NativeRandomEngine Create(IRandomEngine seeder)
		{
			var instance = CreateInstance<NativeRandomEngine>();
			instance._random = new System.Random((int)seeder.Next32());
			return instance;
		}

		public override uint Next32()
		{
			_random.NextBytes(_buffer);
			return System.BitConverter.ToUInt32(_buffer, 0);
		}

		public override ulong Next64()
		{
			return ((ulong)Next32() << 32) | Next32();
		}
	}
}
