using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[ExecuteInEditMode]
	public abstract class ManifoldGenerator : MonoBehaviour
	{
		private Manifold _manifold;

		private bool _invalidated = true;

		public Manifold manifold { get { return _manifold; } }

		public void Invalidate()
		{
			_invalidated = true;
		}

		void Awake()
		{
			Invalidate();
		}

		void OnValidate()
		{
			Invalidate();
		}

		void Update()
		{
			if (_invalidated)
			{
				_manifold = RebuildManifold();
				_invalidated = false;
			}
		}

		protected abstract Manifold RebuildManifold();
	}
}
