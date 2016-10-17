/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System;
using Experilous.Numerics;

namespace Experilous.MakeItTile
{
	public class PlanarSurface : Surface, ISerializationCallbackReceiver
	{
		public Vector3 origin;
		public Quaternion orientation;

		protected Vector3 _planeNormal;

		public static PlanarSurface Create(Vector3 origin, Quaternion orientation)
		{
			return CreateInstance<PlanarSurface>().Reset(origin, orientation);
		}

		public PlanarSurface Reset(Vector3 origin, Quaternion orientation)
		{
			this.origin = origin;
			this.orientation = orientation;

			OnAfterDeserialize();

			return this;
		}

		public virtual void OnAfterDeserialize()
		{
			_planeNormal = orientation * Vector3.back;
		}

		public virtual void OnBeforeSerialize()
		{
		}

		public override bool isFlat { get { return true; } }
		public override bool isCurved { get { return false; } }

		public override bool isConvex { get { return true; } }
		public override bool isConcave { get { return false; } }

		public Plane plane { get { return new Plane(_planeNormal, origin); } }
		public Vector3 normal { get { return _planeNormal; } }

		public override Vector3 Intersect(Ray ray)
		{
			return Geometry.Intersect(plane, ray);
		}

		public override Vector3 Intersect(ScaledRay ray)
		{
			return Geometry.Intersect(plane, ray);
		}

		public override bool Intersect(Ray ray, out Vector3 intersection)
		{
			return Geometry.Intersect(plane, ray, out intersection);
		}

		public override bool Intersect(ScaledRay ray, out Vector3 intersection)
		{
			return Geometry.Intersect(plane, ray, out intersection);
		}

		public override Vector3 Project(Vector3 position)
		{
			var line = new Ray(position, _planeNormal);
			return Geometry.Intersect(plane, line);
		}

		public override Vector3 GetNormal(Vector3 position)
		{
			return _planeNormal;
		}
	}
}
