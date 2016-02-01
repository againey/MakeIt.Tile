using System;
using System.Collections.Generic;
using Experilous.Randomization;

namespace Experilous
{
	[Serializable]
	public struct AssetGeneratorRandomization
	{
		public enum SeedSource
		{
			SystemTime,
			Numerical,
			Textual,
			RandomEngine,
			RandomEngineAndSystemTime,
			RandomEngineAndNumerical,
			RandomEngineAndTextual,
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
				switch (seedSource)
				{
					case SeedSource.RandomEngine:
					case SeedSource.RandomEngineAndSystemTime:
					case SeedSource.RandomEngineAndNumerical:
					case SeedSource.RandomEngineAndTextual:
						yield return randomEngineSeedInputSlot;
						break;
				}
			}
		}

		public IRandomEngine GetRandomEngine()
		{
			switch (seedSource)
			{
				case SeedSource.SystemTime:
					switch (randomEngineType)
					{
						case RandomEngineType.Native: return NativeRandomEngine.Create();
						case RandomEngineType.SplitMix64: return SplitMix64.Create();
						case RandomEngineType.XorShift128Plus: return XorShift128Plus.Create();
						default: throw new NotImplementedException();
					}
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
				case SeedSource.RandomEngineAndSystemTime:
					switch (randomEngineType)
					{
						case RandomEngineType.Native: { var engine = NativeRandomEngine.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); engine.MergeSeed(); return engine; }
						case RandomEngineType.SplitMix64: { var engine = SplitMix64.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); engine.MergeSeed(); return engine; }
						case RandomEngineType.XorShift128Plus: { var engine = XorShift128Plus.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); engine.MergeSeed(); return engine; }
						default: throw new NotImplementedException();
					}
				case SeedSource.RandomEngineAndNumerical:
					switch (randomEngineType)
					{
						case RandomEngineType.Native: { var engine = NativeRandomEngine.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); engine.MergeSeed(seedNumber); return engine; }
						case RandomEngineType.SplitMix64: { var engine = SplitMix64.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); engine.MergeSeed(seedNumber); return engine; }
						case RandomEngineType.XorShift128Plus: { var engine = XorShift128Plus.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); engine.MergeSeed(seedNumber); return engine; }
						default: throw new NotImplementedException();
					}
				case SeedSource.RandomEngineAndTextual:
					switch (randomEngineType)
					{
						case RandomEngineType.Native: { var engine = NativeRandomEngine.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); engine.MergeSeed(seedText); return engine; }
						case RandomEngineType.SplitMix64: { var engine = SplitMix64.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); engine.MergeSeed(seedText); return engine; }
						case RandomEngineType.XorShift128Plus: { var engine = XorShift128Plus.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); engine.MergeSeed(seedText); return engine; }
						default: throw new NotImplementedException();
					}
				default:
					throw new NotImplementedException();
			}
		}
	}
}
