﻿/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System;
using Experilous.Randomization;

namespace Experilous.Generation
{
	[Serializable]
	public struct RandomnessDescriptor
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
			XorShift128PlusB,
			SplitMix64B,
			Native,
			XorShift128Plus,
			XorShift1024Star,
			XoroShiro128Plus,
			SplitMix64,
		}

		public RandomEngineType randomEngineType;
		public SeedSource seedSource;
		public int seedNumber;
		public string seedText;
		[AutoSelect] public InputSlot randomEngineSeedInputSlot;

		public void Initialize(Generator generator)
		{
			// Inputs
			InputSlot.CreateOrResetRequiredMutating<IRandomEngine>(ref randomEngineSeedInputSlot, generator);

			// Fields
			randomEngineType = RandomEngineType.XorShift128Plus;
			seedSource = SeedSource.Numerical;
			seedNumber = 0;
			seedText = "";
		}

		public void Update()
		{
			switch (seedSource)
			{
				case SeedSource.RandomEngine:
				case SeedSource.RandomEngineAndSystemTime:
				case SeedSource.RandomEngineAndNumerical:
				case SeedSource.RandomEngineAndTextual:
					randomEngineSeedInputSlot.isActive = true;
					break;
				default:
					randomEngineSeedInputSlot.isActive = false;
					break;
			}
		}

		public IRandomEngine GetRandomEngine()
		{
			switch (seedSource)
			{
				case SeedSource.SystemTime:
					switch (randomEngineType)
					{
						case RandomEngineType.Native: return SystemRandomEngine.Create();
						case RandomEngineType.SplitMix64: return SplitMix64.Create();
						case RandomEngineType.SplitMix64B: return SplitMix64B.Create();
						case RandomEngineType.XorShift128Plus: return XorShift128Plus.Create();
						case RandomEngineType.XorShift128PlusB: return XorShift128PlusB.Create();
						case RandomEngineType.XorShift1024Star: return XorShift1024Star.Create();
						case RandomEngineType.XoroShiro128Plus: return XoroShiro128Plus.Create();
						default: throw new NotImplementedException();
					}
				case SeedSource.Numerical:
					switch (randomEngineType)
					{
						case RandomEngineType.Native: return SystemRandomEngine.Create(seedNumber);
						case RandomEngineType.SplitMix64: return SplitMix64.Create(seedNumber);
						case RandomEngineType.SplitMix64B: return SplitMix64B.Create(seedNumber);
						case RandomEngineType.XorShift128Plus: return XorShift128Plus.Create(seedNumber);
						case RandomEngineType.XorShift128PlusB: return XorShift128PlusB.Create(seedNumber);
						case RandomEngineType.XorShift1024Star: return XorShift1024Star.Create(seedNumber);
						case RandomEngineType.XoroShiro128Plus: return XoroShiro128Plus.Create(seedNumber);
						default: throw new NotImplementedException();
					}
				case SeedSource.Textual:
					switch (randomEngineType)
					{
						case RandomEngineType.Native: return SystemRandomEngine.Create(seedText);
						case RandomEngineType.SplitMix64: return SplitMix64.Create(seedText);
						case RandomEngineType.SplitMix64B: return SplitMix64B.Create(seedText);
						case RandomEngineType.XorShift128Plus: return XorShift128Plus.Create(seedText);
						case RandomEngineType.XorShift128PlusB: return XorShift128PlusB.Create(seedText);
						case RandomEngineType.XorShift1024Star: return XorShift1024Star.Create(seedText);
						case RandomEngineType.XoroShiro128Plus: return XoroShiro128Plus.Create(seedText);
						default: throw new NotImplementedException();
					}
				case SeedSource.RandomEngine:
					switch (randomEngineType)
					{
						case RandomEngineType.Native: return SystemRandomEngine.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>());
						case RandomEngineType.SplitMix64: return SplitMix64.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>());
						case RandomEngineType.SplitMix64B: return SplitMix64B.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>());
						case RandomEngineType.XorShift128Plus: return XorShift128Plus.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>());
						case RandomEngineType.XorShift128PlusB: return XorShift128PlusB.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>());
						case RandomEngineType.XorShift1024Star: return XorShift1024Star.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>());
						case RandomEngineType.XoroShiro128Plus: return XoroShiro128Plus.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>());
						default: throw new NotImplementedException();
					}
				case SeedSource.RandomEngineAndSystemTime:
					switch (randomEngineType)
					{
						case RandomEngineType.Native: { var random = SystemRandomEngine.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); random.MergeSeed(); return random; }
						case RandomEngineType.SplitMix64: { var random = SplitMix64.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); random.MergeSeed(); return random; }
						case RandomEngineType.SplitMix64B: { var random = SplitMix64B.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); random.MergeSeed(); return random; }
						case RandomEngineType.XorShift128Plus: { var random = XorShift128Plus.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); random.MergeSeed(); return random; }
						case RandomEngineType.XorShift128PlusB: { var random = XorShift128PlusB.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); random.MergeSeed(); return random; }
						case RandomEngineType.XorShift1024Star: { var random = XorShift1024Star.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); random.MergeSeed(); return random; }
						case RandomEngineType.XoroShiro128Plus: { var random = XoroShiro128Plus.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); random.MergeSeed(); return random; }
						default: throw new NotImplementedException();
					}
				case SeedSource.RandomEngineAndNumerical:
					switch (randomEngineType)
					{
						case RandomEngineType.Native: { var random = SystemRandomEngine.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); random.MergeSeed(seedNumber); return random; }
						case RandomEngineType.SplitMix64: { var random = SplitMix64.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); random.MergeSeed(seedNumber); return random; }
						case RandomEngineType.SplitMix64B: { var random = SplitMix64B.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); random.MergeSeed(seedNumber); return random; }
						case RandomEngineType.XorShift128Plus: { var random = XorShift128Plus.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); random.MergeSeed(seedNumber); return random; }
						case RandomEngineType.XorShift128PlusB: { var random = XorShift128PlusB.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); random.MergeSeed(seedNumber); return random; }
						case RandomEngineType.XorShift1024Star: { var random = XorShift1024Star.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); random.MergeSeed(seedNumber); return random; }
						case RandomEngineType.XoroShiro128Plus: { var random = XoroShiro128Plus.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); random.MergeSeed(seedNumber); return random; }
						default: throw new NotImplementedException();
					}
				case SeedSource.RandomEngineAndTextual:
					switch (randomEngineType)
					{
						case RandomEngineType.Native: { var random = SystemRandomEngine.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); random.MergeSeed(seedText); return random; }
						case RandomEngineType.SplitMix64: { var random = SplitMix64.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); random.MergeSeed(seedText); return random; }
						case RandomEngineType.SplitMix64B: { var random = SplitMix64B.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); random.MergeSeed(seedText); return random; }
						case RandomEngineType.XorShift128Plus: { var random = XorShift128Plus.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); random.MergeSeed(seedText); return random; }
						case RandomEngineType.XorShift128PlusB: { var random = XorShift128PlusB.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); random.MergeSeed(seedText); return random; }
						case RandomEngineType.XorShift1024Star: { var random = XorShift1024Star.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); random.MergeSeed(seedText); return random; }
						case RandomEngineType.XoroShiro128Plus: { var random = XoroShiro128Plus.Create(randomEngineSeedInputSlot.GetAsset<IRandomEngine>()); random.MergeSeed(seedText); return random; }
						default: throw new NotImplementedException();
					}
				default:
					throw new NotImplementedException();
			}
		}
	}
}
