using UnityEngine;
using System;

namespace Experilous.Topological
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
			this.primaryPoleNormal = primaryPoleNormal;
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
	}
}
