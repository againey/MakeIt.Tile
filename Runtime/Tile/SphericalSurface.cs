/******************************************************************************\
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
using MakeIt.Numerics;

namespace MakeIt.Tile
{
	/// <summary>
	/// A surface in the shape of a sphere, centered at the world origin at (0, 0, 0).
	/// </summary>
	public class SphericalSurface : Surface
	{
		/// <summary>
		/// The normal vector defining the primary (or "north") pole of the sphere, where latitude would be defined as 90&#176;.
		/// </summary>
		public Vector3 primaryPoleNormal;

		/// <summary>
		/// The normal vector defining the secondary pole of the sphere, where latitude and longitude would both be defined as 0&#176;.
		/// </summary>
		public Vector3 equatorialPoleNormal;

		/// <summary>
		/// The radius of the sphere.
		/// </summary>
		public float radius;

		/// <summary>
		/// Indicates if the sphere is inverted, such that the upward surface normal points toward the center of the sphere, rather than away from it.
		/// </summary>
		public bool isInverted;

		/// <summary>
		/// Creates a spherical surface instance with the given primary and equatorial poles and radius.
		/// </summary>
		/// <param name="primaryPole">The unnormalized position of the primary (or "north") pole of the sphere, which also defines the radius of the sphere.</param>
		/// <param name="equatorialPoleNormal">The normal vector defining the secondary pole of the sphere along its equator.</param>
		/// <returns>A spherical surface.</returns>
		public static SphericalSurface Create(Vector3 primaryPole, Vector3 equatorialPoleNormal)
		{
			return CreateInstance<SphericalSurface>().Reset(primaryPole, equatorialPoleNormal);
		}

		/// <summary>
		/// Creates a spherical surface instance with the given primary and equatorial poles and radius.
		/// </summary>
		/// <param name="primaryPoleNormal">The normal vector defining the primary (or "north") pole of the sphere.</param>
		/// <param name="equatorialPoleNormal">The normal vector defining the secondary pole of the sphere along its equator.</param>
		/// <param name="radius">The radius of the sphere.</param>
		/// <returns>A spherical surface.</returns>
		public static SphericalSurface Create(Vector3 primaryPoleNormal, Vector3 equatorialPoleNormal, float radius)
		{
			return CreateInstance<SphericalSurface>().Reset(primaryPoleNormal, equatorialPoleNormal, radius);
		}

		/// <summary>
		/// Creates a spherical surface instance with the given primary and equatorial poles and radius, plus whether the sphere is inverted.
		/// </summary>
		/// <param name="primaryPole">The unnormalized position of the primary (or "north") pole of the sphere, which also defines the radius of the sphere.</param>
		/// <param name="equatorialPoleNormal">The normal vector defining the secondary pole of the sphere along its equator.</param>
		/// <param name="isInverted">Indicates if the sphere is inverted, such that the upward surface normal points toward the center of the sphere, rather than away from it.</param>
		/// <returns>A spherical surface.</returns>
		public static SphericalSurface Create(Vector3 primaryPole, Vector3 equatorialPoleNormal, bool isInverted)
		{
			return CreateInstance<SphericalSurface>().Reset(primaryPole, equatorialPoleNormal, isInverted);
		}

		/// <summary>
		/// Creates a spherical surface instance with the given primary and equatorial poles and radius, plus whether the sphere is inverted.
		/// </summary>
		/// <param name="primaryPoleNormal">The normal vector defining the primary (or "north") pole of the sphere.</param>
		/// <param name="equatorialPoleNormal">The normal vector defining the secondary pole of the sphere along its equator.</param>
		/// <param name="radius">The radius of the sphere.</param>
		/// <param name="isInverted">Indicates if the sphere is inverted, such that the upward surface normal points toward the center of the sphere, rather than away from it.</param>
		/// <returns>A spherical surface.</returns>
		public static SphericalSurface Create(Vector3 primaryPoleNormal, Vector3 equatorialPoleNormal, float radius, bool isInverted)
		{
			return CreateInstance<SphericalSurface>().Reset(primaryPoleNormal, equatorialPoleNormal, radius, isInverted);
		}

		/// <summary>
		/// Resets the current spherical surface with new values for the primary and equatorial poles and radius.
		/// </summary>
		/// <param name="primaryPole">The unnormalized position of the primary (or "north") pole of the sphere, which also defines the radius of the sphere.</param>
		/// <param name="equatorialPoleNormal">The normal vector defining the secondary pole of the sphere along its equator.</param>
		/// <returns>A reference to the current surface.</returns>
		public SphericalSurface Reset(Vector3 primaryPole, Vector3 equatorialPoleNormal)
		{
			radius = primaryPole.magnitude;
			primaryPoleNormal = primaryPole / radius;
			this.equatorialPoleNormal = Vector3.Cross(Vector3.Cross(primaryPoleNormal, equatorialPoleNormal), primaryPoleNormal).normalized;
			isInverted = false;
			return this;
		}

		/// <summary>
		/// Resets the current spherical surface with new values for the primary and equatorial poles and radius.
		/// </summary>
		/// <param name="primaryPoleNormal">The normal vector defining the primary (or "north") pole of the sphere.</param>
		/// <param name="equatorialPoleNormal">The normal vector defining the secondary pole of the sphere along its equator.</param>
		/// <param name="radius">The radius of the sphere.</param>
		/// <returns>A reference to the current surface.</returns>
		public SphericalSurface Reset(Vector3 primaryPoleNormal, Vector3 equatorialPoleNormal, float radius)
		{
			this.primaryPoleNormal = primaryPoleNormal.normalized;
			this.equatorialPoleNormal = Vector3.Cross(Vector3.Cross(primaryPoleNormal, equatorialPoleNormal), primaryPoleNormal).normalized;
			this.radius = radius;
			isInverted = false;
			return this;
		}

		/// <summary>
		/// Resets the current spherical surface with new values for the primary and equatorial poles and radius, plus whether the sphere is inverted.
		/// </summary>
		/// <param name="primaryPole">The unnormalized position of the primary (or "north") pole of the sphere, which also defines the radius of the sphere.</param>
		/// <param name="equatorialPoleNormal">The normal vector defining the secondary pole of the sphere along its equator.</param>
		/// <param name="isInverted">Indicates if the sphere is inverted, such that the upward surface normal points toward the center of the sphere, rather than away from it.</param>
		/// <returns>A reference to the current surface.</returns>
		public SphericalSurface Reset(Vector3 primaryPole, Vector3 equatorialPoleNormal, bool isInverted)
		{
			radius = primaryPole.magnitude;
			primaryPoleNormal = primaryPole / radius;
			this.equatorialPoleNormal = Vector3.Cross(Vector3.Cross(primaryPoleNormal, equatorialPoleNormal), primaryPoleNormal).normalized;
			this.isInverted = isInverted;
			return this;
		}

		/// <summary>
		/// Resets the current spherical surface with new values for the primary and equatorial poles and radius, plus whether the sphere is inverted.
		/// </summary>
		/// <param name="primaryPoleNormal">The normal vector defining the primary (or "north") pole of the sphere.</param>
		/// <param name="equatorialPoleNormal">The normal vector defining the secondary pole of the sphere along its equator.</param>
		/// <param name="radius">The radius of the sphere.</param>
		/// <param name="isInverted">Indicates if the sphere is inverted, such that the upward surface normal points toward the center of the sphere, rather than away from it.</param>
		/// <returns>A reference to the current surface.</returns>
		public SphericalSurface Reset(Vector3 primaryPoleNormal, Vector3 equatorialPoleNormal, float radius, bool isInverted)
		{
			this.primaryPoleNormal = primaryPoleNormal.normalized;
			this.equatorialPoleNormal = Vector3.Cross(Vector3.Cross(primaryPoleNormal, equatorialPoleNormal), primaryPoleNormal).normalized;
			this.radius = radius;
			this.isInverted = isInverted;
			return this;
		}

		/// <inheritdoc/>
		public override bool isFlat { get { return false; } }
		/// <inheritdoc/>
		public override bool isCurved { get { return true; } }

		/// <inheritdoc/>
		public override bool isConvex { get { return true; } }
		/// <inheritdoc/>
		public override bool isConcave { get { return isInverted; } }

		/// <summary>
		/// Gets a quaternion appropriate for transforming from the standard orthonormal basis (primary pole = +y, equatorial pole = +x) to that of the current spherical surface.
		/// </summary>
		public Quaternion orientation { get { return Quaternion.LookRotation(Vector3.Cross(equatorialPoleNormal, primaryPoleNormal), primaryPoleNormal); } }

		/// <inheritdoc/>
		public override Vector3 Intersect(Ray ray)
		{
			Vector3 intersection;
			Intersect(ray, out intersection);
			return intersection;
		}

		/// <inheritdoc/>
		public override Vector3 Intersect(ScaledRay ray)
		{
			Vector3 intersection;
			Intersect(ray, out intersection);
			return intersection;
		}

		/// <inheritdoc/>
		public override bool Intersect(Ray ray, out Vector3 intersection)
		{
			return Intersect((ScaledRay)ray, out intersection);
		}

		/// <inheritdoc/>
		public override bool Intersect(ScaledRay ray, out Vector3 intersection)
		{
			if (!isInverted)
			{
				return Geometry.IntersectForwardExternal(new Sphere(Vector3.zero, radius), ray, out intersection);
			}
			else
			{
				return Geometry.IntersectForwardInternal(new Sphere(Vector3.zero, radius), ray, out intersection);
			}
		}

		/// <inheritdoc/>
		public override Vector3 Project(Vector3 position)
		{
			if (position != Vector3.zero)
			{
				return position.WithMagnitude(radius);
			}
			else
			{
				return primaryPoleNormal.WithMagnitude(radius);
			}
		}

		/// <inheritdoc/>
		public override Vector3 GetNormal(Vector3 position)
		{
			if (position != Vector3.zero)
			{
				return (isInverted ? -position : position).normalized;
			}
			else
			{
				return isInverted ? -primaryPoleNormal : primaryPoleNormal;
			}
		}
	}
}
