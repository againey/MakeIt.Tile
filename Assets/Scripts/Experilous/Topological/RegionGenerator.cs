using UnityEngine;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[ExecuteInEditMode]
	public class RegionGenerator : RefreshableMonoBehaviour, ISerializationCallbackReceiver
	{
		[SerializeField]
		private Component _serializableManifoldGenerator;
		private IManifoldProvider _manifoldGenerator;

		public IManifoldProvider ManifoldGenerator
		{
			get { return _manifoldGenerator; }
			set { Refreshable.Rechain(ref _manifoldGenerator, value, this); }
		}

		[SerializeField]
		private int _desiredRegionCount;

		public int DesiredRegionCount
		{
			get { return _desiredRegionCount; }
			set
			{
				int clampedValue = 0;
				if (_manifoldGenerator != null && _manifoldGenerator.manifold != null)
				{
					clampedValue = Mathf.Clamp(value, 1, _manifoldGenerator.manifold.topology.faces.Count);
				}

				if (_desiredRegionCount != clampedValue)
				{
					_desiredRegionCount = clampedValue;
					Refresh();
#if UNITY_EDITOR
					UnityEditor.EditorUtility.SetDirty(this);
#endif
				}
			}
		}

		private FaceAttribute<int> _faceRegions;
		private int _regionCount;

		public FaceAttribute<int> faceRegions
		{
			get
			{
				RefreshImmediatelyIfAwaiting();
				return _faceRegions;
			}
		}

		public int regionCount
		{
			get
			{
				RefreshImmediatelyIfAwaiting();
				return _regionCount;
			}
		}

		protected override void RefreshContent()
		{
			if (_desiredRegionCount > 0 && ManifoldGenerator != null)
			{
				var manifold = ManifoldGenerator.manifold;
				if (manifold != null)
				{
					_faceRegions = new FaceAttribute<int>(manifold.topology.faces.Count);

					if (_desiredRegionCount == 1)
					{
					}
					else if (_desiredRegionCount >= manifold.topology.faces.Count)
					{
					}
					else
					{
						var visitor = new RandomAdjacentFaceVisitor(manifold.topology);

						var random = new System.Random();

						_regionCount = DesiredRegionCount;

						var faces = manifold.topology.faces;
						int listIndex = 0;
						int regionIndex = 0;
						int remainingFaceCount = faces.Count;
						int remainingRegionCount = _regionCount;
						while (remainingRegionCount > 0)
						{
							if (random.Next(remainingFaceCount) < remainingRegionCount)
							{
								var faceIndex = faces[listIndex];
								visitor.AddSeed(faceIndex);
								_faceRegions[faceIndex] = regionIndex;
								++regionIndex;
								--remainingRegionCount;
							}

							++listIndex;
							--remainingFaceCount;
						}

						foreach (var edge in (IEnumerable<Topology.FaceEdge>)visitor)
						{
							_faceRegions[edge.farFace] = _faceRegions[edge.nearFace];
						}
					}
				}

				return;
			}

			_faceRegions.Reset();
			_regionCount = 0;
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
