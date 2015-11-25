using System;

namespace Experilous
{
	public class NativeRandomEngineFactory : RandomEngineFactory
	{
		public override IRandomEngine Create()
		{
			return new NativeRandomEngine();
		}

		public override IRandomEngine Create(int seed)
		{
			return new NativeRandomEngine(seed);
		}

		public override IRandomEngine Create(params int[] seed)
		{
			if (seed == null || seed.Length == 0) return new NativeRandomEngine();
			return new NativeRandomEngine(Hash(seed));
		}

		public override IRandomEngine Create(string seed)
		{
			if (seed == null || seed.Length == 0) return new NativeRandomEngine();
			return new NativeRandomEngine(Hash(seed));
		}
	}
}
