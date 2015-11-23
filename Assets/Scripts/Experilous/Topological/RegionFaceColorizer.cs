using UnityEngine;
using System;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public class RegionFaceColorizer : RefreshableMonoBehaviour, IFaceAttribute<Color>, ISerializationCallbackReceiver
	{
		[SerializeField]
		private RegionGenerator _regionGenerator;

		[SerializeField]
		private List<Color> _regionColors;

		public RegionGenerator RegionGenerator
		{
			get { return _regionGenerator; }
			set { Refreshable.Rechain(ref _regionGenerator, value, this); }
		}

		public int Count
		{
			get
			{
				return _regionGenerator != null ? _regionGenerator.faceRegions.Count : 0;
			}
		}

		public Color this[Topology.Face face]
		{
			get
			{
				return this[face.index];
			}

			set
			{
				this[face.index] = value;
			}
		}

		public Color this[int faceIndex]
		{
			get
			{
				if (_regionGenerator != null && _regionColors != null)
				{
					return _regionColors[_regionGenerator.faceRegions[faceIndex]];
				}
				else
				{
					return new Color(1f, 1f, 1f);
				}
			}

			set
			{
				if (_regionGenerator != null && _regionColors != null)
				{
					_regionColors[_regionGenerator.faceRegions[faceIndex]] = value;
					Refresh();
				}
				else
				{
					throw new InvalidOperationException("Region colors cannot be changed when no regions have been generated.");
				}
			}
		}

		protected override void RefreshContent()
		{
			if (_regionGenerator != null)
			{
				if (_regionGenerator.regionCount > 0)
				{
					var random = new System.Random();
					_regionColors = new List<Color>(_regionGenerator.regionCount);
					while (_regionColors.Count < _regionGenerator.regionCount)
					{
						_regionColors.Add(new Color((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble()));
					}

					return;
				}
			}

			_regionColors = null;
		}

		private void OnEnable()
		{
			if (_regionGenerator != null) _regionGenerator.Refreshed += PropagateRefresh;
		}

		private void OnDisable()
		{
			if (_regionGenerator != null) _regionGenerator.Refreshed -= PropagateRefresh;
		}

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			Refreshable.Chain(_regionGenerator, this);
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}
	}
}
