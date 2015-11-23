using UnityEngine;

namespace Experilous.Topological
{
	public interface IFaceColorizer : IRefreshable
	{
		Color this[int faceIndex] { get; }
		Color this[Topology.Face face] { get; }
	}
}
