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

		public Vector3 Offset(Vector3 position, EdgeWrap edgeWrap)
		{
			switch (edgeWrap)
			{
				case EdgeWrap.None:
				case EdgeWrap.Axis0:
				case EdgeWrap.Axis1:
				case EdgeWrap.Axis0 | EdgeWrap.Axis1:
					return position;
				case EdgeWrap.PositiveAxis0:
				case EdgeWrap.PositiveAxis0 | EdgeWrap.Axis1:
					return position + axis0.vector;
				case EdgeWrap.NegativeAxis0:
				case EdgeWrap.NegativeAxis0 | EdgeWrap.Axis1:
					return position - axis0.vector;
				case EdgeWrap.PositiveAxis1:
				case EdgeWrap.PositiveAxis1 | EdgeWrap.Axis0:
					return position + axis1.vector;
				case EdgeWrap.NegativeAxis1:
				case EdgeWrap.NegativeAxis1 | EdgeWrap.Axis0:
					return position - axis1.vector;
				case EdgeWrap.PositiveAxis0 | EdgeWrap.PositiveAxis1:
					return position + axis0.vector + axis1.vector;
				case EdgeWrap.PositiveAxis0 | EdgeWrap.NegativeAxis1:
					return position + axis0.vector - axis1.vector;
				case EdgeWrap.NegativeAxis0 | EdgeWrap.PositiveAxis1:
					return position - axis0.vector + axis1.vector;
				case EdgeWrap.NegativeAxis0 | EdgeWrap.NegativeAxis1:
					return position - axis0.vector - axis1.vector;
				default:
					throw new NotSupportedException(string.Format("Cannot offset a position according to the specified edge wrap data {0} because it references an axis beyond the first two or references both the positive and negative of an axis.", edgeWrap));
			}
		}

		public Vector3 OffsetPositive(Vector3 position, EdgeWrap edgeWrap)
		{
			switch (edgeWrap)
			{
				case EdgeWrap.None:
				case EdgeWrap.Axis0:
				case EdgeWrap.Axis1:
				case EdgeWrap.Axis0 | EdgeWrap.Axis1:
				case EdgeWrap.NegativeAxis0:
				case EdgeWrap.NegativeAxis0 | EdgeWrap.Axis1:
				case EdgeWrap.NegativeAxis1:
				case EdgeWrap.NegativeAxis1 | EdgeWrap.Axis0:
				case EdgeWrap.NegativeAxis0 | EdgeWrap.NegativeAxis1:
					return position;
				case EdgeWrap.PositiveAxis0:
				case EdgeWrap.PositiveAxis0 | EdgeWrap.Axis1:
				case EdgeWrap.PositiveAxis0 | EdgeWrap.NegativeAxis1:
					return position + axis0.vector;
				case EdgeWrap.PositiveAxis1:
				case EdgeWrap.PositiveAxis1 | EdgeWrap.Axis0:
				case EdgeWrap.NegativeAxis0 | EdgeWrap.PositiveAxis1:
					return position + axis1.vector;
				case EdgeWrap.PositiveAxis0 | EdgeWrap.PositiveAxis1:
					return position + axis0.vector + axis1.vector;
				default:
					throw new NotSupportedException(string.Format("Cannot offset a position according to the specified edge wrap data {0} because it references an axis beyond the first two or references both the positive and negative of an axis.", edgeWrap));
			}
		}
	}
}
