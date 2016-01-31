using System;

namespace Experilous
{
	public class NativeRandomEngineFactory : RandomEngineFactory
	{
		public static NativeRandomEngineFactory Create()
		{
			return CreateInstance<NativeRandomEngineFactory>();
		}

		public static NativeRandomEngineFactory Create(string name)
		{
			var instance = Create();
			instance.name = name;
			return instance;
		}

		public override IRandomEngine CreateEngine()
		{
			return new NativeRandomEngine();
		}

		public override IRandomEngine CreateEngine(int seed)
		{
			return NativeRandomEngine.Create(seed);
		}

		public override IRandomEngine CreateEngine(params int[] seed)
		{
			return NativeRandomEngine.Create(seed);
		}

		public override IRandomEngine CreateEngine(string seed)
		{
			return NativeRandomEngine.Create(seed);
		}
	}
}
