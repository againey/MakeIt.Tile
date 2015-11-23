using UnityEngine;

namespace Experilous.Topological
{
	public interface IManifoldProvider : IRefreshable
	{
		Manifold manifold { get; }
	}
}
