using UnityEngine;

namespace Experilous
{
	public abstract class RandomEngineFactory : ScriptableObject
	{
		public abstract IRandomEngine CreateEngine();
		public abstract IRandomEngine CreateEngine(int seed);
		public abstract IRandomEngine CreateEngine(params int[] seed);
		public abstract IRandomEngine CreateEngine(string seed);
	}
}
