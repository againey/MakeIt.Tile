/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Experilous.MakeItRandom;
using Experilous.Core;

namespace Experilous.MakeItGenerate
{
	[Generator(typeof(GeneratorExecutive), "Utility/Randomness")]
	public class RandomnessGenerator : Generator, ISerializationCallbackReceiver
	{
		[Label(null)] public RandomnessDescriptor randomness;

		public OutputSlot randomOutputSlot;

		protected override void Initialize()
		{
			// Fields
			randomness.Initialize(this);

			// Outputs
			OutputSlot.CreateOrResetUnpersisted<IRandom>(ref randomOutputSlot, this, "Randomness", true);
		}

		public void OnAfterDeserialize()
		{
			randomness.ResetIfBroken(this);

			if (randomOutputSlot != null && randomOutputSlot.generator != null)
			{
				OutputSlot.ResetAssetTypeIfNull<IRandom>(randomOutputSlot);
			}
			else
			{
				OutputSlot.CreateOrResetUnpersisted<IRandom>(ref randomOutputSlot, this, "Randomness", true);
			}
		}

		public void OnBeforeSerialize()
		{
		}

		protected override void OnUpdate()
		{
			randomness.Update();
		}

		public override IEnumerable<InputSlot> inputs
		{
			get
			{
				yield return randomness.randomSeedInputSlot;
			}
		}

		public override IEnumerable<OutputSlot> outputs
		{
			get
			{
				yield return randomOutputSlot;
			}
		}

		public override IEnumerator BeginGeneration()
		{
			randomOutputSlot.SetAsset(ScriptableObjectRandomWrapper.CreateInstance(randomness.GetRandom()));
			yield break;
		}
	}
}
