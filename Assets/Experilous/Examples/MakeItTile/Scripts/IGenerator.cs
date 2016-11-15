using System.Collections.Generic;

namespace Experilous.Examples.MakeItTile
{
#if UNITY_EDITOR
	public interface IGenerator
	{
		IEnumerable<IGenerator> dependencies { get; }
		void Generate();
	}
#endif
}
