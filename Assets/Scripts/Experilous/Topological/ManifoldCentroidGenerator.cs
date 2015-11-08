using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[ExecuteInEditMode]
	public class ManifoldCentroidGenerator : MonoBehaviour
	{
		public ManifoldGenerator ManifoldGenerator;

		public bool Spherical = false;

	#if UNITY_EDITOR
		private Manifold _manifold = null;
	#endif

		private FaceAttribute<Vector3> _centroids;

		private bool _invalidated = true;

		public FaceAttribute<Vector3> centroids { get { return _centroids; } }

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
					RebuildCentroids();
				}
			}
			else
			{
				if (_invalidated)
				{
					RebuildCentroids();
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

		void RebuildCentroids()
		{
			if (ManifoldGenerator != null && ManifoldGenerator.manifold != null)
			{
				_manifold = ManifoldGenerator.manifold;
				_centroids = new FaceAttribute<Vector3>(_manifold.topology.faces.Count);
				foreach (var face in _manifold.topology.faces)
				{
					var sum = new Vector3();
					foreach (var edge in face.edges)
					{
						sum += _manifold.vertexPositions[edge.nextVertex];
					}
					_centroids[face] = sum;
				}

				if (!Spherical)
				{
					foreach (var face in _manifold.topology.faces)
					{
						_centroids[face] /= face.edges.Count;
					}
				}
				else
				{
					foreach (var face in _manifold.topology.faces)
					{
						_centroids[face] = _centroids[face].normalized;
					}
				}
			}
			else
			{
				_centroids.Clear();
			}

			_invalidated = false;
		}
	}
}
