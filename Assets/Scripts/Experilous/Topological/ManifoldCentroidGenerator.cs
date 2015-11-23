using UnityEngine;

namespace Experilous.Topological
{
	[ExecuteInEditMode]
	public class ManifoldCentroidGenerator : RefreshableMonoBehaviour, IFaceAttributeProvider<Vector3>, ISerializationCallbackReceiver
	{
		[SerializeField]
		private Component _serializableManifoldGenerator;
		private IManifoldProvider _manifoldGenerator;

		public IManifoldProvider ManifoldGenerator
		{
			get { return _manifoldGenerator; }
			set { Refreshable.Rechain(ref _manifoldGenerator, value, this); }
		}

		public bool Spherical = false;

		private FaceAttribute<Vector3> _centroids;

		public FaceAttribute<Vector3> centroids
		{
			get
			{
				RefreshImmediatelyIfAwaiting();
				return _centroids;
			}
		}

		public FaceAttribute<Vector3> attribute
		{
			get
			{
				RefreshImmediatelyIfAwaiting();
				return _centroids;
			}
		}

		protected override void RefreshContent()
		{
			if (ManifoldGenerator != null)
			{
				var manifold = ManifoldGenerator.manifold;
				if (manifold != null)
				{
					var topology = manifold.topology;
					var vertexPositions = manifold.vertexPositions;
					_centroids = new FaceAttribute<Vector3>(topology.faces.Count);
					foreach (var face in topology.faces)
					{
						var sum = new Vector3();
						foreach (var edge in face.edges)
						{
							sum += vertexPositions[edge.nextVertex];
						}
						_centroids[face] = sum;
					}

					if (!Spherical)
					{
						foreach (var face in topology.faces)
						{
							_centroids[face] /= face.edges.Count;
						}
					}
					else
					{
						foreach (var face in topology.faces)
						{
							_centroids[face] = _centroids[face].normalized;
						}
					}

					return;
				}
			}

			_centroids = new FaceAttribute<Vector3>();
		}

		private void OnEnable()
		{
			if (_manifoldGenerator != null) _manifoldGenerator.Refreshed += PropagateRefresh;
		}

		private void OnDisable()
		{
			if (_manifoldGenerator != null) _manifoldGenerator.Refreshed -= PropagateRefresh;
		}

		public void OnBeforeSerialize()
		{
			_serializableManifoldGenerator = (Component)_manifoldGenerator;
		}

		public void OnAfterDeserialize()
		{
			_manifoldGenerator = (_serializableManifoldGenerator != null ? (IManifoldProvider)_serializableManifoldGenerator : null);
		}
	}
}
