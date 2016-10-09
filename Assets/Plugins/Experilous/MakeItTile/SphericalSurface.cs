/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using Experilous.Numerics;

namespace Experilous.MakeItTile
{
	public class SphericalSurface : Surface
	{
		public Vector3 primaryPoleNormal;
		public Vector3 equatorialPoleNormal;
		public float radius;
		public bool isInverted;

		public static SphericalSurface Create(Vector3 primaryPole, Vector3 equatorialPoleNormal)
		{
			return CreateInstance<SphericalSurface>().Reset(primaryPole, equatorialPoleNormal);
		}

		public static SphericalSurface Create(Vector3 primaryPoleNormal, Vector3 equatorialPoleNormal, float radius)
		{
			return CreateInstance<SphericalSurface>().Reset(primaryPoleNormal, equatorialPoleNormal, radius);
		}

		public static SphericalSurface Create(Vector3 primaryPole, Vector3 equatorialPoleNormal, bool isInverted)
		{
			return CreateInstance<SphericalSurface>().Reset(primaryPole, equatorialPoleNormal, isInverted);
		}

		public static SphericalSurface Create(Vector3 primaryPoleNormal, Vector3 equatorialPoleNormal, float radius, bool isInverted)
		{
			return CreateInstance<SphericalSurface>().Reset(primaryPoleNormal, equatorialPoleNormal, radius, isInverted);
		}

		public SphericalSurface Reset(Vector3 primaryPole, Vector3 equatorialPoleNormal)
		{
			radius = primaryPole.magnitude;
			primaryPoleNormal = primaryPole / radius;
			this.equatorialPoleNormal = Vector3.Cross(Vector3.Cross(primaryPoleNormal, equatorialPoleNormal), primaryPoleNormal).normalized;
			isInverted = false;
			return this;
		}

		public SphericalSurface Reset(Vector3 primaryPoleNormal, Vector3 equatorialPoleNormal, float radius)
		{
			this.primaryPoleNormal = primaryPoleNormal.normalized;
			this.equatorialPoleNormal = Vector3.Cross(Vector3.Cross(primaryPoleNormal, equatorialPoleNormal), primaryPoleNormal).normalized;
			this.radius = radius;
			isInverted = false;
			return this;
		}

		public SphericalSurface Reset(Vector3 primaryPole, Vector3 equatorialPoleNormal, bool isInverted)
		{
			radius = primaryPole.magnitude;
			primaryPoleNormal = primaryPole / radius;
			this.equatorialPoleNormal = Vector3.Cross(Vector3.Cross(primaryPoleNormal, equatorialPoleNormal), primaryPoleNormal).normalized;
			this.isInverted = isInverted;
			return this;
		}

		public SphericalSurface Reset(Vector3 primaryPoleNormal, Vector3 equatorialPoleNormal, float radius, bool isInverted)
		{
			this.primaryPoleNormal = primaryPoleNormal.normalized;
			this.equatorialPoleNormal = Vector3.Cross(Vector3.Cross(primaryPoleNormal, equatorialPoleNormal), primaryPoleNormal).normalized;
			this.radius = radius;
			this.isInverted = isInverted;
			return this;
		}

		public override bool isFlat { get { return false; } }
		public override bool isCurved { get { return true; } }

		public override bool isConvex { get { return true; } }
		public override bool isConcave { get { return false; } }

		public Quaternion orientation { get { return Quaternion.LookRotation(Vector3.Cross(equatorialPoleNormal, primaryPoleNormal), primaryPoleNormal); } }

		public override Vector3 Intersect(Ray ray)
		{
			Vector3 intersection;
			Intersect(ray, out intersection);
			return intersection;
		}

		public override Vector3 Intersect(ScaledRay ray)
		{
			Vector3 intersection;
			Intersect(ray, out intersection);
			return intersection;
		}

		public override bool Intersect(Ray ray, out Vector3 intersection)
		{
			return Intersect((ScaledRay)ray, out intersection);
		}

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
