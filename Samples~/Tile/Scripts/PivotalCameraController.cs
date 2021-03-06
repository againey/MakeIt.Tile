﻿/******************************************************************************\
* Copyright Andy Gainey                                                        *
*                                                                              *
* Licensed under the Apache License, Version 2.0 (the "License");              *
* you may not use this file except in compliance with the License.             *
* You may obtain a copy of the License at                                      *
*                                                                              *
*     http://www.apache.org/licenses/LICENSE-2.0                               *
*                                                                              *
* Unless required by applicable law or agreed to in writing, software          *
* distributed under the License is distributed on an "AS IS" BASIS,            *
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.     *
* See the License for the specific language governing permissions and          *
* limitations under the License.                                               *
\******************************************************************************/

using UnityEngine;
using MakeIt.Core;

namespace MakeIt.Tile.Samples
{
	[ExecuteInEditMode]
	public class PivotalCameraController : MonoBehaviour
	{
		public Transform origin;

		[Range(-90f, 90f)]
		public float minimumLatitude = -80;
		[Range(-90f, 90f)]
		public float maximumLatitude = 80;

		public float surfaceRadius = 1f;
		public float outerRadius = 0.9f;
		public float innerRadius = 0f;

		[Range(0f, 90f)]
		public float minimumRadiusLookAtAngle = 0f;
		[Range(0f, 90f)]
		public float maximumRadiusLookAtAngle = 0f;

		[Range(-90f, 90f)]
		public float latitude = 0f;

		[Range(0f, 360f)]
		public float longitude = 0f;

		[Range(0f, 1f)]
		public float zoom = 0f;

		public float zoomVelocity = 1f;

		public bool scaleInputRate = true;

		private float _zoomTarget = 0f;

		protected void Start()
		{
			this.DisableAndThrowOnUnassignedReference(origin, "The OrbitalCameraController component requires a reference to a Transform origin.");
		}

		protected void Update()
		{
			if (origin == null ||
				minimumLatitude > maximumLatitude ||
				surfaceRadius <= 0f ||
				outerRadius >= surfaceRadius  ||
				innerRadius > outerRadius ||
				zoomVelocity <= 0f)
			{
				return;
			}

#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
			{
#endif
				UpdateFromInput();
#if UNITY_EDITOR
			}
#endif

			var radiusUpperRange = outerRadius - innerRadius;
			var radiusLowerRange = surfaceRadius - outerRadius;
			var radiusTotalRange = surfaceRadius - innerRadius;
			var radiusUpperOverTotal = radiusUpperRange / radiusTotalRange;
			var radiusLowerOverTotal = radiusLowerRange / radiusTotalRange;
			var zoomScale = 1f / (0.5f * radiusUpperOverTotal + radiusLowerOverTotal);
			var invertedZoom = 1f - zoom;

			var latitudeRadians = latitude * Mathf.Deg2Rad;
			var latitudeCosine = Mathf.Cos(latitudeRadians);
			var longitudeRadians = longitude * Mathf.Deg2Rad;

			float cameraElevation;
			if (scaleInputRate)
			{
				var scaledZoom = invertedZoom * radiusUpperRange * zoomScale;
				cameraElevation = outerRadius - (0.5f * invertedZoom * radiusUpperOverTotal + radiusLowerOverTotal) * scaledZoom;
			}
			else
			{
				cameraElevation = outerRadius - radiusUpperRange * invertedZoom;
			}

			var cameraDirection = new Vector3(
				Mathf.Cos(longitudeRadians) * latitudeCosine,
				Mathf.Sin(latitudeRadians),
				Mathf.Sin(longitudeRadians) * latitudeCosine);

			var cameraLookAt = cameraDirection * surfaceRadius;
			var cameraPosition = cameraDirection * cameraElevation;
			var sideVector = Vector3.Cross(cameraDirection, origin.up);
			var cameraAngle = (Mathf.Min(maximumRadiusLookAtAngle, 89.9f) - Mathf.Min(minimumRadiusLookAtAngle, 89.9f)) * invertedZoom + minimumRadiusLookAtAngle;
			if (cameraAngle == 0f)
			{
				transform.position = origin.position + cameraPosition;
				transform.up = Vector3.Cross(sideVector, cameraPosition);
				transform.LookAt(cameraLookAt, transform.up);
			}
			else
			{
				var adjustedUpVector = Vector3.Cross(cameraDirection, sideVector);
				var angledCameraPosition = cameraLookAt + Vector3.Slerp(cameraPosition - cameraLookAt, adjustedUpVector * (surfaceRadius - cameraElevation), cameraAngle / 90f);

				transform.position = origin.position + angledCameraPosition;
				transform.up = Vector3.Cross(sideVector, angledCameraPosition);
				transform.LookAt(cameraLookAt, transform.up);
			}
		}

		protected void UpdateFromInput()
		{
			var verticalInput = Input.GetAxis("Vertical");
			var horizontalInput = Input.GetAxis("Horizontal");
			var zoomInput = Input.GetAxis("Mouse ScrollWheel");

			_zoomTarget = Mathf.Clamp(_zoomTarget + zoomInput, 0f, 1f);

			if (zoom < _zoomTarget)
			{
				zoom = Mathf.Min(zoom + zoomVelocity * Time.deltaTime, _zoomTarget);
			}
			else if (zoom > _zoomTarget)
			{
				zoom = Mathf.Max(zoom - zoomVelocity * Time.deltaTime, _zoomTarget);
			}

			var radiusUpperRange = innerRadius - outerRadius;
			var radiusLowerRange = outerRadius - surfaceRadius;
			var radiusTotalRange = innerRadius - surfaceRadius;
			var radiusUpperOverTotal = radiusUpperRange / radiusTotalRange;
			var radiusLowerOverTotal = radiusLowerRange / radiusTotalRange;
			var zoomScale = 1f / (0.5f * radiusUpperOverTotal + radiusLowerOverTotal);
			var invertedZoom = 1f - zoom;

			if (scaleInputRate)
			{
				var scaledRate = (invertedZoom * radiusUpperOverTotal + radiusLowerOverTotal) * zoomScale;
				verticalInput *= scaledRate;
				horizontalInput *= scaledRate;
			}

			latitude = Mathf.Clamp(Mathf.Clamp(latitude + verticalInput, minimumLatitude, maximumLatitude), -89.9f, 89.9f);
			var latitudeRadians = latitude * Mathf.Deg2Rad;
			var latitudeCosine = Mathf.Cos(latitudeRadians);

			if (scaleInputRate)
			{
				horizontalInput /= (zoom * latitudeCosine) + invertedZoom;
			}

			longitude = Mathf.Repeat(longitude - horizontalInput, 360f);
			if (longitude < 0) longitude += 360f;
		}
	}
}
