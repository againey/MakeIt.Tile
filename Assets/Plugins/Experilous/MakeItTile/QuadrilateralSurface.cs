/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System;
using Experilous.Topologies;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// A planar surface defined by two non-colinear axes, and with a size determined by the magnitude of the two axes.
	/// </summary>
	public class QuadrilateralSurface : PlanarSurface, ISerializationCallbackReceiver
	{
		/// <summary>
		/// The untransformed first axis.
		/// </summary>
		public WrappableAxis2 axis0;

		/// <summary>
		/// The untransformed second axis.
		/// </summary>
		public WrappableAxis2 axis1;

		/// <summary>
		/// The first axis after being transformed by the planar orientation.
		/// </summary>
		protected Vector3 _axis0Vector;

		/// <summary>
		/// The second axis after being transformed by the planar orientation.
		/// </summary>
		protected Vector3 _axis1Vector;

		/// <summary>
		/// The orthogonal normal of the first axis, also orthogonal to the planar surface normal, and therefore coplanar with the surface.
		/// </summary>
		protected Vector3 _axis0Normal;

		/// <summary>
		/// The orthogonal normal of the second axis, also orthogonal to the planar surface normal, and therefore coplanar with the surface.
		/// </summary>
		protected Vector3 _axis1Normal;

		/// <summary>
		/// Creates a quadrilateral surface instance with the given axes, origin, and orientation.
		/// </summary>
		/// <param name="axis0">The first axis.</param>
		/// <param name="axis1">The second axis.</param>
		/// <param name="origin">The origin of the plane.</param>
		/// <param name="orientation">The orientation of the plane.</param>
		/// <returns>A quadrilateral surface.</returns>
		public static QuadrilateralSurface Create(WrappableAxis2 axis0, WrappableAxis2 axis1, Vector3 origin, Quaternion orientation)
		{
			return CreateInstance<QuadrilateralSurface>().Reset(axis0, axis1, origin, orientation);
		}

		/// <summary>
		/// Resets the current quadrilateral surface with new values for the axes, origin, and orientation.
		/// </summary>
		/// <param name="axis0">The new first axis.</param>
		/// <param name="axis1">The new second axis.</param>
		/// <param name="origin">The new origin of the plane.</param>
		/// <param name="orientation">The new orientation of the plane.</param>
		/// <returns>A reference to the current surface.</returns>
		public QuadrilateralSurface Reset(WrappableAxis2 axis0, WrappableAxis2 axis1, Vector3 origin, Quaternion orientation)
		{
			Reset(origin, orientation);
			this.axis0 = axis0;
			this.axis1 = axis1;

			OnAfterDeserialize();

			return this;
		}

		/// <summary>
		/// Called before Unity serializes the quadrilateral surface.
		/// </summary>
		public override void OnBeforeSerialize()
		{
			base.OnBeforeSerialize();
		}

		/// <summary>
		/// Called after Unity deserializes the quadrilateral surface.
		/// </summary>
		public override void OnAfterDeserialize()
		{
			base.OnAfterDeserialize();

			_axis0Vector = orientation * axis0;
			_axis1Vector = orientation * axis1;

			_axis0Normal = Vector3.Cross(axis0, _planeNormal);
			_axis1Normal = Vector3.Cross(axis1, _planeNormal);
			if (Vector3.Dot(_axis0Normal, axis1) < 0f) _axis0Normal = -_axis0Normal;
			if (Vector3.Dot(_axis1Normal, axis0) < 0f) _axis1Normal = -_axis1Normal;
		}

		private Vector3 OffsetAttribute(Vector3 position, EdgeWrapUtility.Generic edgeWrap)
		{
			switch (edgeWrap)
			{
				case EdgeWrapUtility.Generic.None:
					return position;
				case EdgeWrapUtility.Generic.PosAxis0:
					return position + _axis0Vector;
				case EdgeWrapUtility.Generic.NegAxis0:
					return position - _axis0Vector;
				case EdgeWrapUtility.Generic.PosAxis1:
					return position + _axis1Vector;
				case EdgeWrapUtility.Generic.NegAxis1:
					return position - _axis1Vector;
				case EdgeWrapUtility.Generic.PosAxis0 | EdgeWrapUtility.Generic.PosAxis1:
					return position + _axis0Vector + _axis1Vector;
				case EdgeWrapUtility.Generic.PosAxis0 | EdgeWrapUtility.Generic.NegAxis1:
					return position + _axis0Vector - _axis1Vector;
				case EdgeWrapUtility.Generic.NegAxis0 | EdgeWrapUtility.Generic.PosAxis1:
					return position - _axis0Vector + _axis1Vector;
				case EdgeWrapUtility.Generic.NegAxis0 | EdgeWrapUtility.Generic.NegAxis1:
					return position - _axis0Vector - _axis1Vector;
				default:
					throw new ArgumentException("edgeWrap");
			}
		}

		private Vector3 ReverseOffsetAttribute(Vector3 position, EdgeWrapUtility.Generic edgeWrap)
		{
			switch (edgeWrap)
			{
				case EdgeWrapUtility.Generic.None:
					return position;
				case EdgeWrapUtility.Generic.PosAxis0:
					return position - _axis0Vector;
				case EdgeWrapUtility.Generic.NegAxis0:
					return position + _axis0Vector;
				case EdgeWrapUtility.Generic.PosAxis1:
					return position - _axis1Vector;
				case EdgeWrapUtility.Generic.NegAxis1:
					return position + _axis1Vector;
				case EdgeWrapUtility.Generic.PosAxis0 | EdgeWrapUtility.Generic.PosAxis1:
					return position - _axis0Vector - _axis1Vector;
				case EdgeWrapUtility.Generic.PosAxis0 | EdgeWrapUtility.Generic.NegAxis1:
					return position - _axis0Vector + _axis1Vector;
				case EdgeWrapUtility.Generic.NegAxis0 | EdgeWrapUtility.Generic.PosAxis1:
					return position + _axis0Vector - _axis1Vector;
				case EdgeWrapUtility.Generic.NegAxis0 | EdgeWrapUtility.Generic.NegAxis1:
					return position + _axis0Vector + _axis1Vector;
				default:
					throw new ArgumentException("edgeWrap");
			}
		}

		/// <inheritdoc/>
		public override Vector3 OffsetVertToVertAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return OffsetAttribute(position, EdgeWrapUtility.VertToVertAsGeneric(edgeWrap));
		}

		/// <inheritdoc/>
		public override Vector3 OffsetVertToEdgeAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return OffsetAttribute(position, EdgeWrapUtility.VertToEdgeAsGeneric(edgeWrap));
		}

		/// <inheritdoc/>
		public override Vector3 OffsetVertToFaceAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return OffsetAttribute(position, EdgeWrapUtility.VertToFaceAsGeneric(edgeWrap));
		}

		/// <inheritdoc/>
		public override Vector3 OffsetEdgeToVertAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return OffsetAttribute(position, EdgeWrapUtility.EdgeToVertAsGeneric(edgeWrap));
		}

		/// <inheritdoc/>
		public override Vector3 OffsetEdgeToFaceAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return OffsetAttribute(position, EdgeWrapUtility.EdgeToFaceAsGeneric(edgeWrap));
		}

		/// <inheritdoc/>
		public override Vector3 OffsetFaceToVertAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return OffsetAttribute(position, EdgeWrapUtility.FaceToVertAsGeneric(edgeWrap));
		}

		/// <inheritdoc/>
		public override Vector3 OffsetFaceToEdgeAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return OffsetAttribute(position, EdgeWrapUtility.FaceToEdgeAsGeneric(edgeWrap));
		}

		/// <inheritdoc/>
		public override Vector3 OffsetFaceToFaceAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return OffsetAttribute(position, EdgeWrapUtility.FaceToFaceAsGeneric(edgeWrap));
		}

		/// <inheritdoc/>
		public override Vector3 ReverseOffsetVertToVertAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return ReverseOffsetAttribute(position, EdgeWrapUtility.VertToVertAsGeneric(edgeWrap));
		}

		/// <inheritdoc/>
		public override Vector3 ReverseOffsetVertToEdgeAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return ReverseOffsetAttribute(position, EdgeWrapUtility.VertToEdgeAsGeneric(edgeWrap));
		}

		/// <inheritdoc/>
		public override Vector3 ReverseOffsetVertToFaceAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return ReverseOffsetAttribute(position, EdgeWrapUtility.VertToFaceAsGeneric(edgeWrap));
		}

		/// <inheritdoc/>
		public override Vector3 ReverseOffsetEdgeToVertAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return ReverseOffsetAttribute(position, EdgeWrapUtility.EdgeToVertAsGeneric(edgeWrap));
		}

		/// <inheritdoc/>
		public override Vector3 ReverseOffsetEdgeToFaceAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return ReverseOffsetAttribute(position, EdgeWrapUtility.EdgeToFaceAsGeneric(edgeWrap));
		}

		/// <inheritdoc/>
		public override Vector3 ReverseOffsetFaceToVertAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return ReverseOffsetAttribute(position, EdgeWrapUtility.FaceToVertAsGeneric(edgeWrap));
		}

		/// <inheritdoc/>
		public override Vector3 ReverseOffsetFaceToEdgeAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return ReverseOffsetAttribute(position, EdgeWrapUtility.FaceToEdgeAsGeneric(edgeWrap));
		}

		/// <inheritdoc/>
		public override Vector3 ReverseOffsetFaceToFaceAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return ReverseOffsetAttribute(position, EdgeWrapUtility.FaceToFaceAsGeneric(edgeWrap));
		}
	}
}
