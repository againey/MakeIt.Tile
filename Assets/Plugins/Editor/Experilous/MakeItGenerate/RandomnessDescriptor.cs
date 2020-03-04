/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System;
using Experilous.MakeItRandom;

namespace Experilous.MakeItGenerate
{
	[Serializable]
	public struct RandomnessDescriptor
	{
		public enum SeedSource
		{
			SystemTime,
			Numerical,
			Textual,
			Random,
			RandomAndSystemTime,
			RandomAndNumerical,
			RandomAndTextual,
		}

		public enum RandomType
		{
			XorShift128Plus,
			SplitMix64,
			System,
			Unity,
			XorShift1024Star,
			XoroShiro128Plus,
		}

		public RandomType randomType;
		public SeedSource seedSource;
		public int seedNumber;
		public string seedText;
		[AutoSelect] public InputSlot randomSeedInputSlot;

		public void Initialize(Generator generator)
		{
			// Inputs
			InputSlot.CreateOrResetRequiredMutating<IRandom>(ref randomSeedInputSlot, generator);

			// Fields
			randomType = RandomType.XorShift128Plus;
			seedSource = SeedSource.Numerical;
			seedNumber = 0;
			seedText = "";
		}

		public void Update()
		{
			switch (seedSource)
			{
				case SeedSource.Random:
				case SeedSource.RandomAndSystemTime:
				case SeedSource.RandomAndNumerical:
				case SeedSource.RandomAndTextual:
					randomSeedInputSlot.isActive = true;
					break;
				default:
					randomSeedInputSlot.isActive = false;
					break;
			}
		}

		public IRandom GetRandom()
		{
			switch (seedSource)
			{
				case SeedSource.SystemTime:
					switch (randomType)
					{
						case RandomType.System: return SystemRandom.Create();
						case RandomType.Unity: return UnityRandom.Create();
						case RandomType.SplitMix64: return SplitMix64.Create();
						case RandomType.XorShift128Plus: return XorShift128Plus.Create();
						case RandomType.XorShift1024Star: return XorShift1024Star.Create();
						case RandomType.XoroShiro128Plus: return XoroShiro128Plus.Create();
						default: throw new NotImplementedException();
					}
				case SeedSource.Numerical:
					switch (randomType)
					{
						case RandomType.System: return SystemRandom.Create(seedNumber);
						case RandomType.Unity: return UnityRandom.Create(seedNumber);
						case RandomType.SplitMix64: return SplitMix64.Create(seedNumber);
						case RandomType.XorShift128Plus: return XorShift128Plus.Create(seedNumber);
						case RandomType.XorShift1024Star: return XorShift1024Star.Create(seedNumber);
						case RandomType.XoroShiro128Plus: return XoroShiro128Plus.Create(seedNumber);
						default: throw new NotImplementedException();
					}
				case SeedSource.Textual:
					switch (randomType)
					{
						case RandomType.System: return SystemRandom.Create(seedText);
						case RandomType.Unity: return UnityRandom.Create(seedText);
						case RandomType.SplitMix64: return SplitMix64.Create(seedText);
						case RandomType.XorShift128Plus: return XorShift128Plus.Create(seedText);
						case RandomType.XorShift1024Star: return XorShift1024Star.Create(seedText);
						case RandomType.XoroShiro128Plus: return XoroShiro128Plus.Create(seedText);
						default: throw new NotImplementedException();
					}
				case SeedSource.Random:
					switch (randomType)
					{
						case RandomType.System: return SystemRandom.Create(randomSeedInputSlot.GetAsset<IRandom>());
						case RandomType.Unity: return UnityRandom.Create(randomSeedInputSlot.GetAsset<IRandom>());
						case RandomType.SplitMix64: return SplitMix64.Create(randomSeedInputSlot.GetAsset<IRandom>());
						case RandomType.XorShift128Plus: return XorShift128Plus.Create(randomSeedInputSlot.GetAsset<IRandom>());
						case RandomType.XorShift1024Star: return XorShift1024Star.Create(randomSeedInputSlot.GetAsset<IRandom>());
						case RandomType.XoroShiro128Plus: return XoroShiro128Plus.Create(randomSeedInputSlot.GetAsset<IRandom>());
						default: throw new NotImplementedException();
					}
				case SeedSource.RandomAndSystemTime:
					switch (randomType)
					{
						case RandomType.System: { var random = SystemRandom.Create(randomSeedInputSlot.GetAsset<IRandom>()); random.MergeSeed(); return random; }
						case RandomType.Unity: { var random = UnityRandom.Create(randomSeedInputSlot.GetAsset<IRandom>()); random.MergeSeed(); return random; }
						case RandomType.SplitMix64: { var random = SplitMix64.Create(randomSeedInputSlot.GetAsset<IRandom>()); random.MergeSeed(); return random; }
						case RandomType.XorShift128Plus: { var random = XorShift128Plus.Create(randomSeedInputSlot.GetAsset<IRandom>()); random.MergeSeed(); return random; }
						case RandomType.XorShift1024Star: { var random = XorShift1024Star.Create(randomSeedInputSlot.GetAsset<IRandom>()); random.MergeSeed(); return random; }
						case RandomType.XoroShiro128Plus: { var random = XoroShiro128Plus.Create(randomSeedInputSlot.GetAsset<IRandom>()); random.MergeSeed(); return random; }
						default: throw new NotImplementedException();
					}
				case SeedSource.RandomAndNumerical:
					switch (randomType)
					{
						case RandomType.System: { var random = SystemRandom.Create(randomSeedInputSlot.GetAsset<IRandom>()); random.MergeSeed(seedNumber); return random; }
						case RandomType.Unity: { var random = UnityRandom.Create(randomSeedInputSlot.GetAsset<IRandom>()); random.MergeSeed(seedNumber); return random; }
						case RandomType.SplitMix64: { var random = SplitMix64.Create(randomSeedInputSlot.GetAsset<IRandom>()); random.MergeSeed(seedNumber); return random; }
						case RandomType.XorShift128Plus: { var random = XorShift128Plus.Create(randomSeedInputSlot.GetAsset<IRandom>()); random.MergeSeed(seedNumber); return random; }
						case RandomType.XorShift1024Star: { var random = XorShift1024Star.Create(randomSeedInputSlot.GetAsset<IRandom>()); random.MergeSeed(seedNumber); return random; }
						case RandomType.XoroShiro128Plus: { var random = XoroShiro128Plus.Create(randomSeedInputSlot.GetAsset<IRandom>()); random.MergeSeed(seedNumber); return random; }
						default: throw new NotImplementedException();
					}
				case SeedSource.RandomAndTextual:
					switch (randomType)
					{
						case RandomType.System: { var random = SystemRandom.Create(randomSeedInputSlot.GetAsset<IRandom>()); random.MergeSeed(seedText); return random; }
						case RandomType.Unity: { var random = UnityRandom.Create(randomSeedInputSlot.GetAsset<IRandom>()); random.MergeSeed(seedText); return random; }
						case RandomType.SplitMix64: { var random = SplitMix64.Create(randomSeedInputSlot.GetAsset<IRandom>()); random.MergeSeed(seedText); return random; }
						case RandomType.XorShift128Plus: { var random = XorShift128Plus.Create(randomSeedInputSlot.GetAsset<IRandom>()); random.MergeSeed(seedText); return random; }
						case RandomType.XorShift1024Star: { var random = XorShift1024Star.Create(randomSeedInputSlot.GetAsset<IRandom>()); random.MergeSeed(seedText); return random; }
						case RandomType.XoroShiro128Plus: { var random = XoroShiro128Plus.Create(randomSeedInputSlot.GetAsset<IRandom>()); random.MergeSeed(seedText); return random; }
						default: throw new NotImplementedException();
					}
				default:
					throw new NotImplementedException();
			}
		}

		public void ResetIfBroken(Generator generator)
		{
			if (randomSeedInputSlot != null && randomSeedInputSlot.generator != null)
			{
				InputSlot.ResetAssetTypeIfNull<IRandom>(randomSeedInputSlot);
			}
			else
			{
				InputSlot.CreateOrResetRequiredMutating<IRandom>(ref randomSeedInputSlot, generator);
			}
		}
	}
}
