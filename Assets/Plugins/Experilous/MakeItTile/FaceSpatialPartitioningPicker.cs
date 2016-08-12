/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using Experilous.MakeIt.Utilities;

namespace Experilous.MakeIt.Tile
{
	[AddComponentMenu("Tile-Based Worlds/Face Spatial Partitioning Picker")]
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
		private Topology.Face _currentFace;

		void Start()
		{
			this.DisableAndThrowOnUnassignedReference(camera, "The FaceSpatialPartitioningPicker component requires a reference to a Camera component.");
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
			this.DisableAndThrowOnUnassignedReference(partitioning, "The FaceSpatialPartitioningPicker component requires a reference to a UniversalFaceSpatialPartitioning scriptable object.  Either create a saved asset using generators available in the Assets/Create/Topology menu, or create and assign one at runtime before the picker's Start() event runs.");

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
			this.DisableAndThrowOnUnassignedReference(partitioning, "The FaceSpatialPartitioningPicker component requires a reference to a UniversalFaceSpatialPartitioning scriptable object.  Either create a saved asset using generators available in the Assets/Create/Topology menu, or create and assign one at runtime before the picker's Start() event runs.");

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
			if (_picking[button] == true)
			{
				var endFace = _currentFace;
				_picking[button] = false;
				if ((_picking[0] || _picking[1] || _picking[2]) == false)
				{
					_currentFace = Topology.Face.none;
				}
				OnPickEnd.Invoke(endFace, button);
			}
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
