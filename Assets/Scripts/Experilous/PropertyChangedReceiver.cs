using UnityEngine;
using System.Reflection;

namespace Experilous
{
	public abstract class PropertyChangedReceiver : MonoBehaviour
	{
/*#if UNITY_EDITOR
		protected void OnEnable()
		{
			_enabled = true;
			if (ManifoldGenerator != null)
			{
				ManifoldGenerator.PropertyChanged += OnManifoldGeneratorPropertyChanged;
				_confirmedManifoldGenerator = ManifoldGenerator;
			}
		}

		protected void OnDisable()
		{
			_enabled = false;
			if (ManifoldGenerator != null)
			{
				ManifoldGenerator.PropertyChanged -= OnManifoldGeneratorPropertyChanged;
				_confirmedManifoldGenerator = null;
			}
		}

		protected void OnValidate()
		{
			if (_enabled)
			{
				if (_confirmedManifoldGenerator == ManifoldGenerator) return;

				if (_confirmedManifoldGenerator != null)
				{
					_confirmedManifoldGenerator.PropertyChanged -= OnManifoldGeneratorPropertyChanged;
				}

				if (ManifoldGenerator != null)
				{
					ManifoldGenerator.PropertyChanged += OnManifoldGeneratorPropertyChanged;
				}

				_confirmedManifoldGenerator = ManifoldGenerator;

				Invalidate();
			}
		}
#else
		protected void OnEnable()
		{
			if (ManifoldGenerator != null)
			{
				ManifoldGenerator.PropertyChanged += OnManifoldGeneratorPropertyChanged;
			}
		}

		protected void OnDisable()
		{
			if (ManifoldGenerator != null)
			{
				ManifoldGenerator.PropertyChanged -= OnManifoldGeneratorPropertyChanged;
			}
		}
#endif*/

	}
}
