using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

namespace Experilous.Topological
{
	public class SpatialPartitioningPicker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		public new Camera camera;
		public UniversalFaceSpatialPartitioning partitioning;

		[Serializable]
		public class PickStartEvent : UnityEvent<Topology.Face> { }
		public PickStartEvent OnPickStart;

		[Serializable]
		public class PickChangeEvent : UnityEvent<Topology.Face, Topology.Face> { }
		public PickChangeEvent OnPickChange;

		[Serializable]
		public class PickEndEvent : UnityEvent<Topology.Face, Topology.Face> { }
		public PickEndEvent OnPickEnd;

		private Collider _collider = null;

		private bool _pickActive;
		private Topology.Face? _pickStart;
		private Topology.Face? _pickEnd;

		void Start()
		{
			this.DisableAndThrowOnMissingReference(camera, "The SpatialPartioningPicker component requires a reference to a Camera component.");
			_collider = GetComponent<Collider>();
		}

		void Update()
		{
			if (_pickActive && partitioning != null)
			{
				PickChange(Input.mousePosition);
			}

			if (_collider == null)
			{
				if (!_pickActive)
				{
					if (Input.GetMouseButtonDown(0))
					{
						PickStart(Input.mousePosition);
					}
				}
				else
				{
					if (Input.GetMouseButtonUp(0))
					{
						PickEnd();
					}
				}
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (!_pickActive)
			{
				PickStart(eventData.pressPosition);
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (_pickActive)
			{
				PickEnd();
			}
		}

		private void PickStart(Vector2 mousePosition)
		{
			var ray = transform.InverseTransformRay(camera.ScreenPointToRay(mousePosition));
			var face = partitioning.FindFace(ray);
			if (face)
			{
				_pickStart = _pickEnd = face;
				_pickActive = true;
				OnPickStart.Invoke(_pickStart.Value);
			}
		}

		private void PickChange(Vector2 mousePosition)
		{
			var ray = transform.InverseTransformRay(camera.ScreenPointToRay(mousePosition));
			var face = partitioning.FindFace(ray);
			if (face && _pickEnd.Value != face)
			{
				_pickEnd = face;
				OnPickChange.Invoke(_pickStart.Value, _pickEnd.Value);
			}
		}

		private void PickEnd()
		{
			_pickActive = false;
			OnPickEnd.Invoke(_pickStart.Value, _pickEnd.Value);
			_pickStart = null;
			_pickEnd = null;
		}

		public void LogPickStart(Topology.Face pickStart)
		{
			Debug.LogFormat("Pick Start:  {0}", pickStart);
		}

		public void LogPickChange(Topology.Face pickStart, Topology.Face pickEnd)
		{
			Debug.LogFormat("Pick Change:  {0} -> {1}", pickStart, pickEnd);
		}

		public void LogPickEnd(Topology.Face pickStart, Topology.Face pickEnd)
		{
			Debug.LogFormat("Pick End:  {0} -> {1}", pickStart, pickEnd);
		}
	}
}
