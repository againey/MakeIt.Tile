using UnityEngine;
using System;

namespace Experilous.Topological
{
	[Serializable] public struct PlanarAxis
	{
		public Vector3 vector;
		public bool isWrapped;

		public PlanarAxis(Vector3 vector, bool isWrapped = false)
		{
			this.vector = vector;
			this.isWrapped = isWrapped;
		}
	}

	public struct PlanarDescriptor
	{
		public PlanarAxis axis0;
		public PlanarAxis axis1;
		public Vector3 normal;
		public bool isInverted;

		public PlanarDescriptor(Vector3 axisVector0, Vector3 axisVector1, bool isInverted = false)
		{
			axis0 = new PlanarAxis(axisVector0);
			axis1 = new PlanarAxis(axisVector1);
			normal = Vector3.Cross(axis1.vector, axis0.vector).normalized * (isInverted ? -1f : 1f);
			this.isInverted = isInverted;
		}

		public PlanarDescriptor(Vector3 axisVector0, bool isAxisWrapped0, Vector3 axisVector1, bool isAxisWrapped1, bool isInverted = false)
		{
			axis0 = new PlanarAxis(axisVector0, isAxisWrapped0);
			axis1 = new PlanarAxis(axisVector1, isAxisWrapped1);
			normal = Vector3.Cross(axis1.vector, axis0.vector).normalized * (isInverted ? -1f : 1f);
			this.isInverted = isInverted;
		}

		public PlanarDescriptor(PlanarAxis axis0, PlanarAxis axis1, bool isInverted = false)
		{
			this.axis0 = axis0;
			this.axis1 = axis1;
			normal = Vector3.Cross(axis1.vector, axis0.vector).normalized * (isInverted ? -1f : 1f);
			this.isInverted = isInverted;
		}

		public PlanarDescriptor(Vector3 axisVector0, Vector3 axisVector1, Vector3 normal)
		{
			axis0 = new PlanarAxis(axisVector0);
			axis1 = new PlanarAxis(axisVector1);
			this.normal = normal.normalized;
			isInverted = Vector3.Dot(normal, Vector3.Cross(axis1.vector, axis0.vector)) < 0f;
		}

		public PlanarDescriptor(Vector3 axisVector0, bool isAxisWrapped0, Vector3 axisVector1, bool isAxisWrapped1, Vector3 normal)
		{
			axis0 = new PlanarAxis(axisVector0, isAxisWrapped0);
			axis1 = new PlanarAxis(axisVector1, isAxisWrapped1);
			this.normal = normal.normalized;
			isInverted = Vector3.Dot(normal, Vector3.Cross(axis1.vector, axis0.vector)) < 0f;
		}

		public PlanarDescriptor(PlanarAxis axis0, PlanarAxis axis1, Vector3 normal)
		{
			this.axis0 = axis0;
			this.axis1 = axis1;
			this.normal = normal.normalized;
			isInverted = Vector3.Dot(normal, Vector3.Cross(axis1.vector, axis0.vector)) < 0f;
		}
	}

	public class PlanarSurface : Surface
	{
		public PlanarAxis axis0;
		public PlanarAxis axis1;
		public Vector3 normal;
		public bool isInverted;

		public static PlanarSurface Create(PlanarDescriptor descriptor)
		{
			var instance = CreateInstance<PlanarSurface>();
			instance.axis0 = descriptor.axis0;
			instance.axis1 = descriptor.axis1;
			instance.normal = descriptor.normal;
			instance.isInverted = descriptor.isInverted;
			return instance;
		}

		public static PlanarSurface Create(PlanarDescriptor descriptor, string name)
		{
			var instance = Create(descriptor);
			instance.name = name;
			return instance;
		}

		public PlanarAxis GetAxis(int axisIndex)
		{
			switch (axisIndex)
			{
				case 0: return axis0;
				case 1: return axis1;
				default: throw new ArgumentOutOfRangeException("axisIndex");
			}
		}

		public void SetAxis(int axisIndex, PlanarAxis axis)
		{
			switch (axisIndex)
			{
				case 0: axis0 = axis; break;
				case 1: axis1 = axis; break;
				default: throw new ArgumentOutOfRangeException("axisIndex");
			}
		}

		public override bool isFlat { get { return true; } }
		public override bool isCurved { get { return false; } }

		public override bool isConvex { get { return true; } }
		public override bool isConcave { get { return false; } }

		private Vector3 OffsetAttribute(Vector3 position, EdgeWrapUtility.Generic edgeWrap)
		{
			switch (edgeWrap)
			{
				case EdgeWrapUtility.Generic.None:
					return position;
				case EdgeWrapUtility.Generic.PosAxis0:
					return position + axis0.vector;
				case EdgeWrapUtility.Generic.NegAxis0:
					return position - axis0.vector;
				case EdgeWrapUtility.Generic.PosAxis1:
					return position + axis1.vector;
				case EdgeWrapUtility.Generic.NegAxis1:
					return position - axis1.vector;
				case EdgeWrapUtility.Generic.PosAxis0 | EdgeWrapUtility.Generic.PosAxis1:
					return position + axis0.vector + axis1.vector;
				case EdgeWrapUtility.Generic.PosAxis0 | EdgeWrapUtility.Generic.NegAxis1:
					return position + axis0.vector - axis1.vector;
				case EdgeWrapUtility.Generic.NegAxis0 | EdgeWrapUtility.Generic.PosAxis1:
					return position - axis0.vector + axis1.vector;
				case EdgeWrapUtility.Generic.NegAxis0 | EdgeWrapUtility.Generic.NegAxis1:
					return position - axis0.vector - axis1.vector;
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
					return position - axis0.vector;
				case EdgeWrapUtility.Generic.NegAxis0:
					return position + axis0.vector;
				case EdgeWrapUtility.Generic.PosAxis1:
					return position - axis1.vector;
				case EdgeWrapUtility.Generic.NegAxis1:
					return position + axis1.vector;
				case EdgeWrapUtility.Generic.PosAxis0 | EdgeWrapUtility.Generic.PosAxis1:
					return position - axis0.vector - axis1.vector;
				case EdgeWrapUtility.Generic.PosAxis0 | EdgeWrapUtility.Generic.NegAxis1:
					return position - axis0.vector + axis1.vector;
				case EdgeWrapUtility.Generic.NegAxis0 | EdgeWrapUtility.Generic.PosAxis1:
					return position + axis0.vector - axis1.vector;
				case EdgeWrapUtility.Generic.NegAxis0 | EdgeWrapUtility.Generic.NegAxis1:
					return position + axis0.vector + axis1.vector;
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
