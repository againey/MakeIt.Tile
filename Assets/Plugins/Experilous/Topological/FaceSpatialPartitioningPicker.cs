/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

namespace Experilous.Topological
{
	public class FaceSpatialPartitioningPicker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		public new Camera camera;
		public UniversalFaceSpatialPartitioning partitioning;

		[Serializable]
		public class PickStartEvent : UnityEvent<Topology.Face, PointerEventData.InputButton> { }
		public PickStartEvent OnPickStart;

		[Serializable]
		public class PickChangeEvent : UnityEvent<Topology.Face, Topology.Face> { }
		public PickChangeEvent OnPickChange;

		[Serializable]
		public class PickEndEvent : UnityEvent<Topology.Face, Topology.Face, PointerEventData.InputButton> { }
		public PickEndEvent OnPickEnd;

		private Collider _collider = null;

		private Topology.Face _leftMouseButtonStartFace;
		private Topology.Face _rightMouseButtonStartFace;
		private Topology.Face _middleMouseButtonStartFace;

		private Topology.Face _currentFace;

		public bool IsPickingActive(PointerEventData.InputButton inputButton)
		{
			switch (inputButton)
			{
				case PointerEventData.InputButton.Left: return _leftMouseButtonStartFace;
				case PointerEventData.InputButton.Right: return _rightMouseButtonStartFace;
				case PointerEventData.InputButton.Middle: return _middleMouseButtonStartFace;
				default: throw new NotImplementedException();
			}
		}

		public Topology.Face GetPickStartFace(PointerEventData.InputButton inputButton)
		{
			switch (inputButton)
			{
				case PointerEventData.InputButton.Left: return _leftMouseButtonStartFace;
				case PointerEventData.InputButton.Right: return _rightMouseButtonStartFace;
				case PointerEventData.InputButton.Middle: return _middleMouseButtonStartFace;
				default: throw new NotImplementedException();
			}
		}

		public Topology.Face? GetPickCurrentFace()
		{
			return _currentFace;
		}

		void Start()
		{
			this.DisableAndThrowOnUnassignedReference(camera, "The SpatialPartioningPicker component requires a reference to a Camera component.");
			_collider = GetComponent<Collider>();
		}

		void Update()
		{
			if (partitioning != null)
			{
				if (_leftMouseButtonStartFace || _rightMouseButtonStartFace || _middleMouseButtonStartFace)
				{
					PickChange(Input.mousePosition);
				}
			}

			if (_collider == null)
			{
				CheckForPickStartOrEnd(0, PointerEventData.InputButton.Left, ref _leftMouseButtonStartFace);
				CheckForPickStartOrEnd(1, PointerEventData.InputButton.Right, ref _rightMouseButtonStartFace);
				CheckForPickStartOrEnd(2, PointerEventData.InputButton.Middle, ref _middleMouseButtonStartFace);
			}
		}

		private void CheckForPickStartOrEnd(int mouseButtonIndex, PointerEventData.InputButton button, ref Topology.Face startFace)
		{
			if (!startFace)
			{
				if (Input.GetMouseButtonDown(mouseButtonIndex))
				{
					PickStart(Input.mousePosition, button, ref startFace);
				}
			}
			else
			{
				if (Input.GetMouseButtonUp(mouseButtonIndex))
				{
					PickEnd(button, ref startFace);
				}
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			switch (eventData.button)
			{
				case PointerEventData.InputButton.Left:
					if (!_leftMouseButtonStartFace)
						PickStart(eventData.pressPosition, eventData.button, ref _leftMouseButtonStartFace);
					break;
				case PointerEventData.InputButton.Right:
					if (!_rightMouseButtonStartFace)
						PickStart(eventData.pressPosition, eventData.button, ref _rightMouseButtonStartFace);
					break;
				case PointerEventData.InputButton.Middle:
					if (!_middleMouseButtonStartFace)
						PickStart(eventData.pressPosition, eventData.button, ref _middleMouseButtonStartFace);
					break;
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			switch (eventData.button)
			{
				case PointerEventData.InputButton.Left:
					if (_leftMouseButtonStartFace)
						PickEnd(eventData.button, ref _leftMouseButtonStartFace);
					break;
				case PointerEventData.InputButton.Right:
					if (_rightMouseButtonStartFace)
						PickEnd(eventData.button, ref _rightMouseButtonStartFace);
					break;
				case PointerEventData.InputButton.Middle:
					if (_middleMouseButtonStartFace)
						PickEnd(eventData.button, ref _middleMouseButtonStartFace);
					break;
			}
		}

		private void PickStart(Vector2 mousePosition, PointerEventData.InputButton button, ref Topology.Face startFace)
		{
			var ray = transform.InverseTransformRay(camera.ScreenPointToRay(mousePosition));
			var face = partitioning.FindFace(ray);
			if (face)
			{
				startFace = _currentFace = face;
				OnPickStart.Invoke(face, button);
			}
			else
			{
				startFace = Topology.Face.none;
			}
		}

		private void PickChange(Vector2 mousePosition)
		{
			var ray = transform.InverseTransformRay(camera.ScreenPointToRay(mousePosition));
			var face = partitioning.FindFace(ray);
			if (face && _currentFace != face)
			{
				var prevFace = _currentFace;
				_currentFace = face;
				OnPickChange.Invoke(prevFace, face);
			}
		}

		private void PickEnd(PointerEventData.InputButton button, ref Topology.Face startFace)
		{
			var prevStartFace = startFace;
			startFace = Topology.Face.none;
			OnPickEnd.Invoke(prevStartFace, _currentFace, button);
		}

		public void LogPickStart(Topology.Face pickStart, PointerEventData.InputButton button)
		{
			Debug.LogFormat("Pick Start ({1}):  {0}", pickStart, button);
		}

		public void LogPickChange(Topology.Face pickPrev, Topology.Face pickEnd)
		{
			Debug.LogFormat("Pick Change:  {1} <- {0}", pickPrev, pickEnd);
		}

		public void LogPickEnd(Topology.Face pickStart, Topology.Face pickEnd, PointerEventData.InputButton button)
		{
			Debug.LogFormat("Pick End ({2}):  {1} <- {0}", pickStart, pickEnd, button);
		}
	}
}
