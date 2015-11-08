using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[ExecuteInEditMode]
	public class SphericalPartitioningGenerator : MonoBehaviour
	{
		public ManifoldGenerator ManifoldGenerator;

	#if UNITY_EDITOR
		private Manifold _manifold = null;
	#endif

		private SphericalPartitioning _partitioning;

		private bool _invalidated = true;

		public SphericalPartitioning partitioning { get { return _partitioning; } }

		public void Invalidate()
		{
			_invalidated = true;
		}

		void Start()
		{
		}

		void OnValidate()
		{
			Invalidate();
		}

	#if UNITY_EDITOR
		void LateUpdate()
		{
			if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
			{
				if (ManifoldGenerator.manifold != _manifold)
				{
					RebuildPartitioning();
				}
			}
			else
			{
				if (_invalidated)
				{
					RebuildPartitioning();
				}
			}
		}
	#else
		void Update()
		{
			if (_invalidated)
			{
			}
		}
	#endif

		void RebuildPartitioning()
		{
			if (ManifoldGenerator != null && ManifoldGenerator.manifold != null)
			{
				_manifold = ManifoldGenerator.manifold;
				_partitioning = new SphericalPartitioning(_manifold);
			}
			else
			{
				_partitioning = null;
			}

			_invalidated = false;
		}
	}
}
