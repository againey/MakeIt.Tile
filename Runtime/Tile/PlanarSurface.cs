/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using MakeIt.Numerics;

namespace MakeIt.Tile
{
	/// <summary>
	/// A surface in the shape of a one-sided plane.
	/// </summary>
	public class PlanarSurface : Surface, ISerializationCallbackReceiver
	{
		/// <summary>
		/// The origin of the plane.  It is guaranteed that the plane passes through this point.
		/// </summary>
		public Vector3 origin;

		/// <summary>
		/// The orientation of the plane.  When this is the identity quaternion, the surface normal is <see cref="Vector3.back"/>.
		/// </summary>
		public Quaternion orientation;

		/// <summary>
		/// The calculated surface normal of the plane, derived by transforming <see cref="Vector3.back"/> by <see cref="orientation"/>.
		/// </summary>
		protected Vector3 _planeNormal;

		/// <summary>
		/// Creates a planar surface instance with the given origin and orientation.
		/// </summary>
		/// <param name="origin">The origin of the plane.</param>
		/// <param name="orientation">The orientation of the plane.</param>
		/// <returns>A planar surface.</returns>
		public static PlanarSurface Create(Vector3 origin, Quaternion orientation)
		{
			return CreateInstance<PlanarSurface>().Reset(origin, orientation);
		}

		/// <summary>
		/// Resets the current planar surface with new values for the origin and orientation.
		/// </summary>
		/// <param name="origin">The new origin of the plane.</param>
		/// <param name="orientation">The new orientation of the plane.</param>
		/// <returns>A reference to the current surface.</returns>
		public PlanarSurface Reset(Vector3 origin, Quaternion orientation)
		{
			this.origin = origin;
			this.orientation = orientation;

			OnAfterDeserialize();

			return this;
		}

		/// <summary>
		/// Called before Unity serializes the planar surface.
		/// </summary>
		public virtual void OnBeforeSerialize()
		{
		}

		/// <summary>
		/// Called after Unity deserializes the planar surface.
		/// </summary>
		public virtual void OnAfterDeserialize()
		{
			_planeNormal = orientation * Vector3.back;
		}

		/// <inheritdoc/>
		public override bool isFlat { get { return true; } }
		/// <inheritdoc/>
		public override bool isCurved { get { return false; } }

		/// <inheritdoc/>
		public override bool isConvex { get { return true; } }
		/// <inheritdoc/>
		public override bool isConcave { get { return false; } }

		/// <summary>
		/// The plane struct representing the current planar surface.
		/// </summary>
		public Plane plane { get { return new Plane(_planeNormal, origin); } }

		/// <summary>
		/// The surface normal of the current planar surface.
		/// </summary>
		public Vector3 normal { get { return _planeNormal; } }

		/// <inheritdoc/>
		public override Vector3 Intersect(Ray ray)
		{
			return Geometry.Intersect(plane, ray);
		}

		/// <inheritdoc/>
		public override Vector3 Intersect(ScaledRay ray)
		{
			return Geometry.Intersect(plane, ray);
		}

		/// <inheritdoc/>
		public override bool Intersect(Ray ray, out Vector3 intersection)
		{
			return Geometry.Intersect(plane, ray, out intersection);
		}

		/// <inheritdoc/>
		public override bool Intersect(ScaledRay ray, out Vector3 intersection)
		{
			return Geometry.Intersect(plane, ray, out intersection);
		}

		/// <inheritdoc/>
		public override Vector3 Project(Vector3 position)
		{
			var line = new Ray(position, _planeNormal);
			return Geometry.Intersect(plane, line);
		}

		/// <inheritdoc/>
		public override Vector3 GetNormal(Vector3 position)
		{
			return _planeNormal;
		}
	}
}
