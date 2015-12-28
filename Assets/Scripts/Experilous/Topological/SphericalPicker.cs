using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

namespace Experilous.Topological
{
	[RequireComponent(typeof(Collider))]
	[RequireComponent(typeof(SphericalPartitioningProvider))]
	public class SphericalPicker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		public Camera Camera;

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
		private SphericalPartitioning _partitioning = null;

		private bool _pickActive;
		private Topology.Face? _pickStart;
		private Topology.Face? _pickEnd;

		void Start()
		{
			_collider = GetComponent<Collider>();
			_partitioning = GetComponent<SphericalPartitioningProvider>().partitioning;
		}

		void Update()
		{
			if (_pickActive && _partitioning != null)
			{
				var ray = Camera.ScreenPointToRay(Input.mousePosition);
				RaycastHit raycastHit;
				if (_collider.Raycast(ray, out raycastHit, float.PositiveInfinity))
				{
					var face = _partitioning.Intersect(raycastHit.point - transform.position);
					if (_pickEnd.Value != face)
					{
						_pickEnd = face;
						OnPickChange.Invoke(_pickStart.Value, _pickEnd.Value);
					}
				}
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (!_pickActive)
			{
				var face = _partitioning.Intersect(eventData.pointerCurrentRaycast.worldPosition - transform.position);
				_pickStart = _pickEnd = face;
				_pickActive = true;
				OnPickStart.Invoke(_pickStart.Value);
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (_pickActive)
			{
				_pickActive = false;
				OnPickEnd.Invoke(_pickStart.Value, _pickEnd.Value);
				_pickStart = null;
				_pickEnd = null;
			}
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
