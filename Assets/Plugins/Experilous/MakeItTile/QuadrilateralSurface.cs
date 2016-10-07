/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System;
using Experilous.Numerics;

namespace Experilous.MakeItTile
{
	public class QuadrilateralSurface : Surface, ISerializationCallbackReceiver
	{
		public WrappableAxis2 axis0;
		public WrappableAxis2 axis1;
		public Vector3 origin;
		public Quaternion orientation;

		protected Vector3 _planeNormal;
		protected Vector3 _axis0Vector;
		protected Vector3 _axis1Vector;
		protected Vector3 _axis0Normal;
		protected Vector3 _axis1Normal;

		public static QuadrilateralSurface Create(WrappableAxis2 axis0, WrappableAxis2 axis1, Vector3 origin, Quaternion orientation)
		{
			return CreateInstance<QuadrilateralSurface>().Reset(axis0, axis1, origin, orientation);
		}

		public QuadrilateralSurface Reset(WrappableAxis2 axis0, WrappableAxis2 axis1, Vector3 origin, Quaternion orientation)
		{
			this.axis0 = axis0;
			this.axis1 = axis1;
			this.origin = origin;
			this.orientation = orientation;

			OnAfterDeserialize();

			return this;
		}

		public void OnAfterDeserialize()
		{
			_planeNormal = orientation * Vector3.back;
			_axis0Vector = orientation * axis0;
			_axis1Vector = orientation * axis1;

			_axis0Normal = Vector3.Cross(axis0, _planeNormal);
			_axis1Normal = Vector3.Cross(axis1, _planeNormal);
			if (Vector3.Dot(_axis0Normal, axis1) < 0f) _axis0Normal = -_axis0Normal;
			if (Vector3.Dot(_axis1Normal, axis0) < 0f) _axis1Normal = -_axis1Normal;
		}

		public void OnBeforeSerialize()
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

		public IntVector2 GetWrapIndex2D(Vector3 position)
		{
			IntVector2 index2D = new IntVector2(0, 0);
			var relativePosition = position - origin;
			if (axis0.isWrapped)
			{
				var numerator = Vector3.Dot(_axis1Normal, relativePosition);
				var denominator = Vector3.Dot(_axis1Normal, axis0.vector);
				index2D.x = Mathf.FloorToInt(numerator / -denominator);
			}
			if (axis1.isWrapped)
			{
				var numerator = Vector3.Dot(_axis0Normal, relativePosition);
				var denominator = Vector3.Dot(_axis0Normal, axis1.vector);
				index2D.y = Mathf.FloorToInt(numerator / -denominator);
			}
			return index2D;
		}

		public Vector3 GetWrappedPosition(Vector3 position)
		{
			var relativePosition = position - origin;
			if (axis0.isWrapped)
			{
				var numerator = Vector3.Dot(_axis1Normal, relativePosition);
				var denominator = Vector3.Dot(_axis1Normal, axis0.vector);
				position += Mathf.Floor(numerator / denominator) * _axis0Vector;
			}
			if (axis1.isWrapped)
			{
				var numerator = Vector3.Dot(_axis0Normal, relativePosition);
				var denominator = Vector3.Dot(_axis0Normal, axis1.vector);
				position += Mathf.Floor(numerator / denominator) * _axis1Vector;
			}
			return position;
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

		public override Vector3 OffsetVertToVertAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return OffsetAttribute(position, EdgeWrapUtility.VertToVertAsGeneric(edgeWrap));
		}

		public override Vector3 OffsetVertToEdgeAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return OffsetAttribute(position, EdgeWrapUtility.VertToEdgeAsGeneric(edgeWrap));
		}

		public override Vector3 OffsetVertToFaceAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return OffsetAttribute(position, EdgeWrapUtility.VertToFaceAsGeneric(edgeWrap));
		}

		public override Vector3 OffsetEdgeToVertAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return OffsetAttribute(position, EdgeWrapUtility.EdgeToVertAsGeneric(edgeWrap));
		}

		public override Vector3 OffsetEdgeToFaceAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return OffsetAttribute(position, EdgeWrapUtility.EdgeToFaceAsGeneric(edgeWrap));
		}

		public override Vector3 OffsetFaceToVertAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return OffsetAttribute(position, EdgeWrapUtility.FaceToVertAsGeneric(edgeWrap));
		}

		public override Vector3 OffsetFaceToEdgeAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return OffsetAttribute(position, EdgeWrapUtility.FaceToEdgeAsGeneric(edgeWrap));
		}

		public override Vector3 OffsetFaceToFaceAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return OffsetAttribute(position, EdgeWrapUtility.FaceToFaceAsGeneric(edgeWrap));
		}

		public override Vector3 ReverseOffsetVertToVertAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return ReverseOffsetAttribute(position, EdgeWrapUtility.VertToVertAsGeneric(edgeWrap));
		}

		public override Vector3 ReverseOffsetVertToEdgeAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return ReverseOffsetAttribute(position, EdgeWrapUtility.VertToEdgeAsGeneric(edgeWrap));
		}

		public override Vector3 ReverseOffsetVertToFaceAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return ReverseOffsetAttribute(position, EdgeWrapUtility.VertToFaceAsGeneric(edgeWrap));
		}

		public override Vector3 ReverseOffsetEdgeToVertAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return ReverseOffsetAttribute(position, EdgeWrapUtility.EdgeToVertAsGeneric(edgeWrap));
		}

		public override Vector3 ReverseOffsetEdgeToFaceAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return ReverseOffsetAttribute(position, EdgeWrapUtility.EdgeToFaceAsGeneric(edgeWrap));
		}

		public override Vector3 ReverseOffsetFaceToVertAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return ReverseOffsetAttribute(position, EdgeWrapUtility.FaceToVertAsGeneric(edgeWrap));
		}

		public override Vector3 ReverseOffsetFaceToEdgeAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return ReverseOffsetAttribute(position, EdgeWrapUtility.FaceToEdgeAsGeneric(edgeWrap));
		}

		public override Vector3 ReverseOffsetFaceToFaceAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return ReverseOffsetAttribute(position, EdgeWrapUtility.FaceToFaceAsGeneric(edgeWrap));
		}
	}
}
