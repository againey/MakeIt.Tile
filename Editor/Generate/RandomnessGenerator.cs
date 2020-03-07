/******************************************************************************\
* Copyright Andy Gainey                                                        *
*                                                                              *
* Licensed under the Apache License, Version 2.0 (the "License");              *
* you may not use this file except in compliance with the License.             *
* You may obtain a copy of the License at                                      *
*                                                                              *
*     http://www.apache.org/licenses/LICENSE-2.0                               *
*                                                                              *
* Unless required by applicable law or agreed to in writing, software          *
* distributed under the License is distributed on an "AS IS" BASIS,            *
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.     *
* See the License for the specific language governing permissions and          *
* limitations under the License.                                               *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MakeIt.Random;
using MakeIt.Core;

namespace MakeIt.Generate
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
			randomOutputSlot.SetAsset(randomness.GetRandom());
			yield break;
		}
	}
}
