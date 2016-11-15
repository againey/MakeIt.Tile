using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Examples.MakeItTile
{
#if UNITY_EDITOR
	[ExecuteInEditMode]
	public class GeneratorExecutive : MonoBehaviour
	{
		private readonly List<IGenerator> _pendingGenerators = new List<IGenerator>();
		private readonly List<IGenerator> _completedGenerators = new List<IGenerator>();

		public void Generate()
		{
			_pendingGenerators.Clear();
			_completedGenerators.Clear();

			_pendingGenerators.AddRange(GetComponents<IGenerator>());

			while (_pendingGenerators.Count > 0)
			{
				int i = 0;
				bool progressMade = false;
				while (i < _pendingGenerators.Count)
				{
					var generator = _pendingGenerators[i];
					bool ready = true;
					foreach (var dependency in generator.dependencies)
					{
						if (!_completedGenerators.Contains(dependency))
						{
							if (!_pendingGenerators.Contains(dependency))
							{
								_pendingGenerators.Add(dependency);
							}
							ready = false;
							break;
						}
					}

					if (ready)
					{
						generator.Generate();
						_completedGenerators.Add(generator);
						_pendingGenerators.RemoveAt(i);
						progressMade = true;
					}
					else
					{
						++i;
					}
				}

				if (!progressMade)
				{
					Debug.Log("<b><color=red>Generation got stuck and was unable to make progress!</color></b>");
					break;
				}
			}
		}
	}
#endif
}
