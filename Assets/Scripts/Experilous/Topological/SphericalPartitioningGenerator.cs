using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[ExecuteInEditMode]
	public class SphericalPartitioningGenerator : MonoBehaviour
	{
		public ManifoldGenerator ManifoldGenerator;

		private Manifold _manifold = null;

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

		void LateUpdate()
		{
			if (ManifoldGenerator.manifold != _manifold || _invalidated)
			{
				RebuildPartitioning();
			}
		}

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
				_manifold = null;
			}

			_invalidated = false;
		}
	}
}
