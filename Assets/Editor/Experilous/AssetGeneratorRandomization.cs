using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace Experilous
{
	[Serializable]
	public struct AssetGeneratorRandomization
	{
		public enum SeedSource
		{
			Numerical,
			Textual,
			RandomEngine,
			SystemTime,
		}

		public enum RandomEngineType
		{
			XorShift128Plus,
			SplitMix64,
			Native,
		}

		public RandomEngineType randomEngineType;
		public SeedSource seedSource;
		public int seedNumber;
		public string seedText;
		[AutoSelect] public AssetInputSlot randomEngineSeedInputSlot;

		public void Initialize(AssetGenerator generator, bool reset = true)
		{
			if (reset)
			{
				randomEngineType = RandomEngineType.XorShift128Plus;
				seedSource = SeedSource.Numerical;
				seedNumber = 0;
				seedText = "";
			}

			if (reset || randomEngineSeedInputSlot == null) randomEngineSeedInputSlot = AssetInputSlot.CreateRequiredMutating(generator, typeof(IRandomEngine));
		}

		public IEnumerable<AssetInputSlot> inputs
		{
			get
			{
				if (seedSource == SeedSource.RandomEngine) yield return randomEngineSeedInputSlot;
			}
		}

		public IRandomEngine GetRandomEngine()
		{
			switch (seedSource)
			{
				case SeedSource.Numerical:
					switch (randomEngineType)
					{
						case RandomEngineType.Native: return NativeRandomEngine.Create(seedNumber);
						case RandomEngineType.SplitMix64: return SplitMix64.Create(seedNumber);
						case RandomEngineType.XorShift128Plus: return XorShift128Plus.Create(seedNumber);
						default: throw new NotImplementedException();
					}
				case SeedSource.Textual:
					switch (randomEngineType)
					{
						case RandomEngineType.Native: return NativeRandomEngine.Create(seedText);
						case RandomEngineType.SplitMix64: return SplitMix64.Create(seedText);
						case RandomEngineType.XorShift128Plus: return XorShift128Plus.Create(seedText);
						default: throw new NotImplementedException();
					}
				case SeedSource.RandomEngine:
					switch (randomEngineType)
					{
						case RandomEngineType.Native: return NativeRandomEngine.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>());
						case RandomEngineType.SplitMix64: return SplitMix64.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>());
						case RandomEngineType.XorShift128Plus: return XorShift128Plus.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>());
						default: throw new NotImplementedException();
					}
				case SeedSource.SystemTime:
					switch (randomEngineType)
					{
						case RandomEngineType.Native: return NativeRandomEngine.Create();
						case RandomEngineType.SplitMix64: return SplitMix64.Create();
						case RandomEngineType.XorShift128Plus: return XorShift128Plus.Create();
						default: throw new NotImplementedException();
					}
				default:
					throw new NotImplementedException();
			}
		}
	}
}
