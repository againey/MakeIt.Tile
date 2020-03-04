/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using GeneralUtility = Experilous.Core.GeneralUtility;
using Geometry = Experilous.Numerics.Geometry;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// Component to attach to a game object to raise mouse picking events for the provided face spatial partitioning of a manifold.
	/// </summary>
	/// <remarks><para>The game object to which this component is attached should also be properly configured
	/// for receiving mouse events from the event system.  The minimal setup is typically to have an event
	/// system in the scene with an attached physics raycaster, and an appropriately shaped collider attached
	/// to the same game object to which this picker component is attached.  See the Unity manual on the
	/// <a href="https://docs.unity3d.com/Manual/EventSystem.html">Event System</a> for more information.
	/// </para>
	/// <para>If you have no collider, the picker will still intelligently poll the mouse for events and
	/// perform the necessary ray casts and raise the expected events, avoiding the Unity event system
	/// altogether, but this loses the benefits of integrating with the event system.</para></remarks>
	[AddComponentMenu("Make It Tile/Face Spatial Partitioning Picker")]
	public class FaceSpatialPartitioningPicker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		/// <summary>
		/// The camera used for generating world rays from screen coordinates produced by mouse events.
		/// </summary>
		public new Camera camera;

		/// <summary>
		/// The face spatial partitioning used to find an intersected face given a ray.
		/// </summary>
		public UniversalFaceSpatialPartitioning partitioning;

		/// <summary>
		/// The event type fired whenever a mouse-button down event begins on a face.
		/// </summary>
		[Serializable]
		public class PickStartEvent : UnityEvent<Topology.Face, int> { }

		/// <summary>
		/// The event fired whenever a mouse-button down event begins on a face.
		/// </summary>
		public PickStartEvent OnPickStart;

		/// <summary>
		/// The event type fired whenever a mouse move event transitions from one face to another while picking is active.
		/// </summary>
		[Serializable]
		public class PickChangeEvent : UnityEvent<Topology.Face, Topology.Face> { }

		/// <summary>
		/// The event fired whenever a mouse move event transitions from one face to another while picking is active.
		/// </summary>
		public PickChangeEvent OnPickChange;

		/// <summary>
		/// The event type fired whenever a mouse-button up event ends a picking session.
		/// </summary>
		[Serializable]
		public class PickEndEvent : UnityEvent<Topology.Face, int> { }

		/// <summary>
		/// The event fired whenever a mouse-button up event ends a picking session.
		/// </summary>
		public PickEndEvent OnPickEnd;

		private Collider _collider = null;

		private bool[] _picking = new bool[3];
		private Topology.Face _currentFace;

		void Start()
		{
			GeneralUtility.DisableAndThrowOnUnassignedReference(this, camera, "The FaceSpatialPartitioningPicker component requires a reference to a Camera component.");
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

		/// <summary>
		/// Informs the picker of a mouse button down event.
		/// </summary>
		/// <param name="eventData">The point event data with details of the mouse button event.</param>
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

		/// <summary>
		/// Informs the picker of a mouse button up event.
		/// </summary>
		/// <param name="eventData">The point event data with details of the mouse button event.</param>
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
			GeneralUtility.DisableAndThrowOnUnassignedReference(this, partitioning, "The FaceSpatialPartitioningPicker component requires a reference to a UniversalFaceSpatialPartitioning scriptable object.  Either create a saved asset using generators available in the Assets/Create/Topology menu, or create and assign one at runtime before the picker's Start() event runs.");

			var ray = Geometry.InverseTransformRay(transform, camera.ScreenPointToRay(mousePosition));
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
			GeneralUtility.DisableAndThrowOnUnassignedReference(this, partitioning, "The FaceSpatialPartitioningPicker component requires a reference to a UniversalFaceSpatialPartitioning scriptable object.  Either create a saved asset using generators available in the Assets/Create/Topology menu, or create and assign one at runtime before the picker's Start() event runs.");

			var ray = Geometry.InverseTransformRay(transform, camera.ScreenPointToRay(mousePosition));
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

		/// <summary>
		/// Logs information about a pick start event to the Unity console.
		/// </summary>
		/// <param name="startFace">The face on which picking was started.</param>
		/// <param name="button">The mouse button that began the picking session.</param>
		/// <remarks><para>This function is a convenient debugging tool that can be
		/// easily hooked up in the inspector even if actual pick event handling has
		/// not yet been implemented.</para></remarks>
		public void LogPickStart(Topology.Face startFace, int button)
		{
			Debug.LogFormat("Pick Start ({1}):  {0}", startFace, button);
		}

		/// <summary>
		/// Logs information about a pick change event to the Unity console.
		/// </summary>
		/// <param name="previousFace">The face that was previously being picked by the mouse.</param>
		/// <param name="currentFace">The new face that is now being picked by the mouse.</param>
		/// <remarks><para>This function is a convenient debugging tool that can be
		/// easily hooked up in the inspector even if actual pick event handling has
		/// not yet been implemented.</para></remarks>
		public void LogPickChange(Topology.Face previousFace, Topology.Face currentFace)
		{
			Debug.LogFormat("Pick Change:  {1} <- {0}", previousFace, currentFace);
		}

		/// <summary>
		/// Logs information about a pick end event to the Unity console.
		/// </summary>
		/// <param name="endFace">The last face that was being picked by the mouse when the picking session ended.</param>
		/// <param name="button">The mouse button that was released to end the picking session.</param>
		/// <remarks><para>This function is a convenient debugging tool that can be
		/// easily hooked up in the inspector even if actual pick event handling has
		/// not yet been implemented.</para></remarks>
		public void LogPickEnd(Topology.Face endFace, int button)
		{
			Debug.LogFormat("Pick End ({1}):  {0}", endFace, button);
		}
	}
}
