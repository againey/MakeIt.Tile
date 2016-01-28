using UnityEngine;
using System;

namespace Experilous.Topological
{
	public class PlanarSurfaceDescriptor : ScriptableObject
	{
		[Serializable] public struct Axis
		{
			public Vector3 vector;
			public bool isWrapped;

			public Axis(Vector3 vector, bool isWrapped = false)
			{
				this.vector = vector;
				this.isWrapped = isWrapped;
			}
		}

		public Axis axis0;
		public Axis axis1;

		public static PlanarSurfaceDescriptor Create(Vector3 axisVector0, Vector3 axisVector1)
		{
			return Create(new Axis(axisVector0), new Axis(axisVector1));
		}

		public static PlanarSurfaceDescriptor Create(Vector3 axisVector0, Vector3 axisVector1, string name)
		{
			return Create(new Axis(axisVector0), new Axis(axisVector1), name);
		}

		public static PlanarSurfaceDescriptor Create(Vector3 axisVector0, bool isAxisWrapped0, Vector3 axisVector1, bool isAxisWrapped1)
		{
			return Create(new Axis(axisVector0, isAxisWrapped0), new Axis(axisVector1, isAxisWrapped1));
		}

		public static PlanarSurfaceDescriptor Create(Vector3 axisVector0, bool isAxisWrapped0, Vector3 axisVector1, bool isAxisWrapped1, string name)
		{
			return Create(new Axis(axisVector0, isAxisWrapped0), new Axis(axisVector1, isAxisWrapped1), name);
		}

		public static PlanarSurfaceDescriptor Create(Axis axis0, Axis axis1)
		{
			var instance = CreateInstance<PlanarSurfaceDescriptor>();
			instance.axis0 = axis0;
			instance.axis1 = axis1;
			return instance;
		}

		public static PlanarSurfaceDescriptor Create(Axis axis0, Axis axis1, string name)
		{
			var instance = Create(axis0, axis1);
			instance.name = name;
			return instance;
		}

		public Axis this[int i]
		{
			get
			{
				switch (i)
				{
					case 0: return axis0;
					case 1: return axis1;
					default: throw new ArgumentOutOfRangeException("i");
				}
			}
			set
			{
				switch (i)
				{
					case 0: axis0 = value; break;
					case 1: axis1 = value; break;
					default: throw new ArgumentOutOfRangeException("i");
				}
			}
		}

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

		public Vector3 OffsetVertToVertAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return OffsetAttribute(position, EdgeWrapUtility.VertToVertAsGeneric(edgeWrap));
		}

		public Vector3 OffsetVertToEdgeAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return OffsetAttribute(position, EdgeWrapUtility.VertToEdgeAsGeneric(edgeWrap));
		}

		public Vector3 OffsetVertToFaceAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return OffsetAttribute(position, EdgeWrapUtility.VertToFaceAsGeneric(edgeWrap));
		}

		public Vector3 OffsetEdgeToVertAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return OffsetAttribute(position, EdgeWrapUtility.EdgeToVertAsGeneric(edgeWrap));
		}

		public Vector3 OffsetEdgeToFaceAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return OffsetAttribute(position, EdgeWrapUtility.EdgeToFaceAsGeneric(edgeWrap));
		}

		public Vector3 OffsetFaceToVertAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return OffsetAttribute(position, EdgeWrapUtility.FaceToVertAsGeneric(edgeWrap));
		}

		public Vector3 OffsetFaceToEdgeAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return OffsetAttribute(position, EdgeWrapUtility.FaceToEdgeAsGeneric(edgeWrap));
		}

		public Vector3 OffsetFaceToFaceAttribute(Vector3 position, EdgeWrap edgeWrap)
		{
			return OffsetAttribute(position, EdgeWrapUtility.FaceToFaceAsGeneric(edgeWrap));
		}
	}
}
