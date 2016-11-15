using UnityEngine;
using System.Collections.Generic;
using Experilous.MakeItRandom;

namespace Experilous.Examples.MakeItTile
{
#if UNITY_EDITOR
	[ExecuteInEditMode]
	[RequireComponent(typeof(GeneratorExecutive))]
	public class RandomEngineGenerator : MonoBehaviour, IGenerator
	{
		public string seed = "Planet Andy";

		private IRandom _random;

		public IEnumerable<IGenerator> dependencies
		{
			get
			{
				yield break;
			}
		}

		public IRandom random { get { return _random; } }

		public void Generate()
		{
			if (string.IsNullOrEmpty(seed))
			{
				_random = XorShift128Plus.Create();
			}
			else
			{
				_random = XorShift128Plus.Create(seed);
			}
		}
	}
#endif
}
