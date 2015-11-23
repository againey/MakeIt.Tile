using System;
using UnityEngine;

namespace Experilous.Topological
{
	[ExecuteInEditMode]
	public abstract class ManifoldGenerator : RefreshableMonoBehaviour, IManifoldGenerator
	{
		private Manifold _manifold;

		public Manifold manifold
		{
			get
			{
				RefreshImmediatelyIfAwaiting();
				return _manifold;
			}
		}

		protected override void RefreshContent()
		{
			_manifold = RebuildManifold();
		}

		protected abstract Manifold RebuildManifold();
	}
}
