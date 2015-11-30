using UnityEngine;

namespace Experilous
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public class OrbitalCameraController : MonoBehaviour
	{
		public Transform Origin;

		public float MinimumLatitude = -89;
		public float MaximumLatitude = 89;
		public float SurfaceRadius = 1f;
		public float MinimumRadius = 1.1f;
		public float MaximumRadius = 2.2f;
		public float MinimumRadiusLookAtAngle = 0f;
		public float MaximumRadiusLookAtAngle = 0f;

		public float Latitude = 0f;
		public float Longitude = 0f;
		public float Zoom = 0f;
		public float ZoomVelocity = 1f;
		public bool ScaleInputRate = true;

		private Camera _camera;

		private float _zoomTarget = 0f;

		private void Start()
		{
			_camera = GetComponent<Camera>();
		}

		private void Update()
		{
			var verticalInput = Input.GetAxis("Vertical");
			var horizontalInput = Input.GetAxis("Horizontal");
			var zoomInput = Input.GetAxis("Zoom");

			_zoomTarget = Mathf.Clamp(_zoomTarget + zoomInput, 0f, 1f);

			if (Zoom < _zoomTarget)
			{
				Zoom = Mathf.Min(Zoom + ZoomVelocity * Time.deltaTime, _zoomTarget);
			}
			else if (Zoom > _zoomTarget)
			{
				Zoom = Mathf.Max(Zoom - ZoomVelocity * Time.deltaTime, _zoomTarget);
			}

			var radiusUpperRange = MaximumRadius - MinimumRadius;
			var radiusLowerRange = MinimumRadius - SurfaceRadius;
			var radiusTotalRange = MaximumRadius - SurfaceRadius;
			var radiusUpperOverTotal = radiusUpperRange / radiusTotalRange;
			var radiusLowerOverTotal = radiusLowerRange / radiusTotalRange;
			var zoomScale = radiusUpperRange / (0.5f * radiusUpperOverTotal + radiusLowerOverTotal);
			var invertedZoom = 1f - Zoom;

			if (ScaleInputRate)
			{
				var scaledRate = (invertedZoom * radiusUpperOverTotal + radiusLowerOverTotal) * zoomScale;
				verticalInput *= scaledRate;
				horizontalInput *= scaledRate;
			}

			Latitude = Mathf.Clamp(Latitude + verticalInput, MinimumLatitude, MaximumLatitude);
			var latitudeRadians = Latitude * Mathf.Deg2Rad;
			var latitudeCosine = Mathf.Cos(latitudeRadians);

			if (ScaleInputRate)
			{
				horizontalInput /= (Zoom * latitudeCosine) + invertedZoom;
			}

			Longitude = (Longitude + horizontalInput) % 360f;
			if (Longitude < 0) Longitude += 360f;
			var longitudeRadians = Longitude * Mathf.Deg2Rad;

			float d;
			if (ScaleInputRate)
			{
				var scaledZoom = invertedZoom * zoomScale;
				d = (0.5f * invertedZoom * radiusUpperOverTotal + radiusLowerOverTotal) * scaledZoom + MinimumRadius;
			}
			else
			{
				d = radiusUpperRange * invertedZoom + MinimumRadius;
			}

			var x = Mathf.Cos(longitudeRadians) * latitudeCosine * d;
			var y = Mathf.Sin(latitudeRadians) * d;
			var z = Mathf.Sin(longitudeRadians) * latitudeCosine * d;

			transform.position = Origin.position + new Vector3(x, y, z);
			transform.up = Origin.up;
			transform.LookAt(Origin);
		}
	}
}
