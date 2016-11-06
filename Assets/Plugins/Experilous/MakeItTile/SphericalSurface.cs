/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using Experilous.Numerics;

namespace Experilous.MakeItTile
{
	public struct SphericalDescriptor
	{
		public Vector3 primaryPoleNormal;
		public float radius;

		public SphericalDescriptor(Vector3 primaryPole)
		{
			radius = primaryPole.magnitude;
			primaryPoleNormal = primaryPole / radius;
		}

		public SphericalDescriptor(Vector3 primaryPoleNormal, float radius)
		{
			this.primaryPoleNormal = primaryPoleNormal.normalized;
			this.radius = radius;
		}
	}

	public class SphericalSurface : Surface
	{
		public Vector3 primaryPoleNormal;
		public float radius;

		public static SphericalSurface Create(SphericalDescriptor descriptor)
		{
			return CreateInstance<SphericalSurface>().Reset(descriptor);
		}

		public SphericalSurface Reset(SphericalDescriptor descriptor)
		{
			primaryPoleNormal = descriptor.primaryPoleNormal;
			radius = descriptor.radius;
			return this;
		}

		public override bool isFlat { get { return false; } }
		public override bool isCurved { get { return true; } }

		public override bool isConvex { get { return true; } }
		public override bool isConcave { get { return false; } }

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
			return Geometry.IntersectForwardExternal(new Sphere(Vector3.zero, radius), ray, out intersection);
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
				return position.normalized;
			}
			else
			{
				return primaryPoleNormal;
			}
		}
	}
}
