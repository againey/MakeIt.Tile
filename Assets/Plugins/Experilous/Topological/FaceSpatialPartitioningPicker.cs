﻿/******************************************************************************\
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
		public class PickStartEvent : UnityEvent<Topology.Face, int> { }
		public PickStartEvent OnPickStart;

		[Serializable]
		public class PickChangeEvent : UnityEvent<Topology.Face, Topology.Face> { }
		public PickChangeEvent OnPickChange;

		[Serializable]
		public class PickEndEvent : UnityEvent<Topology.Face, int> { }
		public PickEndEvent OnPickEnd;

		private Collider _collider = null;

		private bool[] _picking = new bool[3];
		private bool _pickingRight;
		private bool _pickingMiddle;
		private Topology.Face _currentFace;

		void Start()
		{
			this.DisableAndThrowOnUnassignedReference(camera, "The SpatialPartioningPicker component requires a reference to a Camera component.");
			_collider = GetComponent<Collider>();
		}

		void Update()
		{
			if (partitioning != null)
			{
				if (_picking[0] || _picking[1] || _picking[2])
				{
					PickChange(Input.mousePosition);
				}
			}

			if (_collider == null)
			{
				for (int i = 0; i < 3; ++i)
				{
					if (_picking[i])
					{
						if (Input.GetMouseButtonUp(i)) PickEnd(i);
					}
					else
					{
						if (Input.GetMouseButtonDown(i)) PickStart(Input.mousePosition, i);
					}
				}
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			switch (eventData.button)
			{
				case PointerEventData.InputButton.Left:
					PickStart(eventData.pressPosition, 0);
					break;
				case PointerEventData.InputButton.Right:
					PickStart(eventData.pressPosition, 1);
					break;
				case PointerEventData.InputButton.Middle:
					PickStart(eventData.pressPosition, 2);
					break;
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			switch (eventData.button)
			{
				case PointerEventData.InputButton.Left:
					PickEnd(0);
					break;
				case PointerEventData.InputButton.Right:
					PickEnd(1);
					break;
				case PointerEventData.InputButton.Middle:
					PickEnd(2);
					break;
			}
		}

		private void PickStart(Vector2 mousePosition, int button)
		{
			var ray = transform.InverseTransformRay(camera.ScreenPointToRay(mousePosition));
			var startFace = partitioning.FindFace(ray);
			if (startFace)
			{
				_currentFace = startFace;
				_picking[button] = true;
				OnPickStart.Invoke(startFace, button);
			}
			else
			{
				_picking[button] = false;
				_currentFace = Topology.Face.none;
			}
		}

		private void PickChange(Vector2 mousePosition)
		{
			var ray = transform.InverseTransformRay(camera.ScreenPointToRay(mousePosition));
			var face = partitioning.FindFace(ray);
			if (face && _currentFace != face)
			{
				var previousFace = _currentFace;
				_currentFace = face;
				OnPickChange.Invoke(previousFace, face);
			}
		}

		private void PickEnd(int button)
		{
			var endFace = _currentFace;
			_picking[button] = false;
			_currentFace = Topology.Face.none;
			OnPickEnd.Invoke(endFace, button);
		}

		public void LogPickStart(Topology.Face startFace, int button)
		{
			Debug.LogFormat("Pick Start ({1}):  {0}", startFace, button);
		}

		public void LogPickChange(Topology.Face previousFace, Topology.Face currentFace)
		{
			Debug.LogFormat("Pick Change:  {1} <- {0}", previousFace, currentFace);
		}

		public void LogPickEnd(Topology.Face endFace, int button)
		{
			Debug.LogFormat("Pick End ({1}):  {0}", endFace, button);
		}
	}
}