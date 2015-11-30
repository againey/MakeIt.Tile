using UnityEngine;

namespace Experilous
{
	[ExecuteInEditMode]
	public class OrbitalCameraController : MonoBehaviour
	{
		public Transform Origin;

		[Range(-90f, 90f)]
		public float MinimumLatitude = -80;
		[Range(-90f, 90f)]
		public float MaximumLatitude = 80;

		public float SurfaceRadius = 1f;
		public float MinimumRadius = 1.1f;
		public float MaximumRadius = 2.2f;

		[Range(0f, 90f)]
		public float MinimumRadiusLookAtAngle = 0f;
		[Range(0f, 90f)]
		public float MaximumRadiusLookAtAngle = 0f;

		[Range(-90f, 90f)]
		public float Latitude = 0f;

		[Range(0f, 360f)]
		public float Longitude = 0f;

		[Range(0f, 1f)]
		public float Zoom = 0f;

		public float ZoomVelocity = 1f;

		public bool ScaleInputRate = true;

		private float _zoomTarget = 0f;

		private void Update()
		{
			if (MinimumLatitude > MaximumLatitude ||
				SurfaceRadius <= 0f ||
				SurfaceRadius >= MinimumRadius ||
				MinimumRadius > MaximumRadius ||
				ZoomVelocity <= 0f)
			{
				return;
			}

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
			var zoomScale = 1f / (0.5f * radiusUpperOverTotal + radiusLowerOverTotal);
			var invertedZoom = 1f - Zoom;

			if (ScaleInputRate)
			{
				var scaledRate = (invertedZoom * radiusUpperOverTotal + radiusLowerOverTotal) * zoomScale;
				verticalInput *= scaledRate;
				horizontalInput *= scaledRate;
			}

			Latitude = Mathf.Clamp(Mathf.Clamp(Latitude + verticalInput, MinimumLatitude, MaximumLatitude), -89.9f, 89.9f);
			var latitudeRadians = Latitude * Mathf.Deg2Rad;
			var latitudeCosine = Mathf.Cos(latitudeRadians);

			if (ScaleInputRate)
			{
				horizontalInput /= (Zoom * latitudeCosine) + invertedZoom;
			}

			Longitude = (Longitude + horizontalInput) % 360f;
			if (Longitude < 0) Longitude += 360f;
			var longitudeRadians = Longitude * Mathf.Deg2Rad;

			float cameraElevation;
			if (ScaleInputRate)
			{
				var scaledZoom = invertedZoom * radiusUpperRange * zoomScale;
				cameraElevation = (0.5f * invertedZoom * radiusUpperOverTotal + radiusLowerOverTotal) * scaledZoom + MinimumRadius;
			}
			else
			{
				cameraElevation = radiusUpperRange * invertedZoom + MinimumRadius;
			}

			var cameraDirection = new Vector3(
				Mathf.Cos(longitudeRadians) * latitudeCosine,
				Mathf.Sin(latitudeRadians),
				Mathf.Sin(longitudeRadians) * latitudeCosine);

			var cameraLookAt = cameraDirection * SurfaceRadius;
			var cameraPosition = cameraDirection * cameraElevation;
			var sideVector = Vector3.Cross(cameraDirection, Origin.up);
			var cameraAngle = (Mathf.Min(MaximumRadiusLookAtAngle, 89.9f) - Mathf.Min(MinimumRadiusLookAtAngle, 89.9f)) * invertedZoom + MinimumRadiusLookAtAngle;
			if (cameraAngle == 0f)
			{
				transform.position = Origin.position + cameraPosition;
				transform.up = Vector3.Cross(sideVector, cameraPosition);
				transform.LookAt(cameraLookAt, transform.up);
			}
			else
			{
				var adjustedUpVector = Vector3.Cross(sideVector, cameraDirection);
				var angledCameraPosition = cameraLookAt + Vector3.Slerp(cameraPosition - cameraLookAt, adjustedUpVector * (SurfaceRadius - cameraElevation), cameraAngle / 90f);

				transform.position = Origin.position + angledCameraPosition;
				transform.up = Vector3.Cross(sideVector, angledCameraPosition);
				transform.LookAt(cameraLookAt, transform.up);
			}
		}
	}
}
