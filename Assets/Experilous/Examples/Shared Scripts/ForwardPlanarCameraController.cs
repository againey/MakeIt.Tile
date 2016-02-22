using UnityEngine;

namespace Experilous.Examples.Shared
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public class ForwardPlanarCameraController : MonoBehaviour
	{
		public float lateralMovementSpeed = 1f;
		public float verticalMovementSpeed = 1f;
		public float rotationSpeed = 1f;

		public Vector2 lookCoordinates;
		public bool invertMouse;

		private Camera _camera;

		protected void Start()
		{
			_camera = GetComponent<Camera>();
		}

		protected void OnEnable()
		{
#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isPlaying)
			{
#endif
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
#if UNITY_EDITOR
			}
#endif
		}

		protected void OnDisable()
		{
#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isPlaying)
			{
#endif
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
#if UNITY_EDITOR
			}
#endif
		}

		protected void Update()
		{
#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
			{
				if (Input.GetKeyDown(KeyCode.Escape))
				{
					UnityEditor.EditorApplication.isPlaying = false;
				}

#endif
				var moveHorizontal = Input.GetAxis("Horizontal");
				var moveForward = Input.GetAxis("Vertical");
				var moveUp = Input.GetAxis("Mouse ScrollWheel");

				var localRight = transform.localRotation * Vector3.right;
				var localUp = transform.localRotation * Vector3.up;
				var localForward = transform.localRotation * Vector3.forward;

				var right = Vector3.Cross(Vector3.up, localForward);
				var forward = Vector3.Cross(localRight, Vector3.up);
				if (right == Vector3.zero) right = Vector3.Cross(Vector3.up, localUp);
				if (forward == Vector3.zero) forward = Vector3.Cross(localUp, Vector3.up);

				right.Normalize();
				forward.Normalize();

				transform.localPosition += right * moveHorizontal * lateralMovementSpeed + forward * moveForward * lateralMovementSpeed + Vector3.up * moveUp * verticalMovementSpeed;

				if (transform.localPosition.y < _camera.nearClipPlane * 1.1f)
				{
					var position = transform.localPosition;
					position.y = _camera.nearClipPlane * 1.1f;
					transform.localPosition = position;
				}

				var lookHorizontal = Input.GetAxisRaw("Mouse X") * rotationSpeed;
				var lookVertical = Input.GetAxisRaw("Mouse Y") * rotationSpeed * (invertMouse ? -1f : +1f);

				lookCoordinates.x = Mathf.Repeat(lookCoordinates.x + lookHorizontal, 360f);
				lookCoordinates.y = Mathf.Clamp(lookCoordinates.y + lookVertical, -90f, 90f);
#if UNITY_EDITOR
			}
#endif

			if (lookCoordinates.y == -90f)
			{
				transform.localRotation = Quaternion.Euler(-lookCoordinates.y, 0f, -lookCoordinates.x);
			}
			else if (lookCoordinates.y == +90f)
			{
				transform.localRotation = Quaternion.Euler(-lookCoordinates.y, 0f, lookCoordinates.x);
			}
			else
			{
				transform.localRotation = Quaternion.Euler(-lookCoordinates.y, lookCoordinates.x, 0f);
			}
		}
	}
}
