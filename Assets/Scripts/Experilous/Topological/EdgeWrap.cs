using System;

namespace Experilous.Topological
{
	[Serializable] [Flags]
	public enum EdgeWrap : byte
	{
		None = 0x00,
		PositiveAxis0 = 0x01,
		NegativeAxis0 = 0x02,
		PositiveAxis1 = 0x04,
		NegativeAxis1 = 0x08,
		PositiveAxis2 = 0x10,
		NegativeAxis2 = 0x20,
		PositiveAxis3 = 0x40,
		NegativeAxis3 = 0x80,
		Axis0 = PositiveAxis0 | NegativeAxis0,
		Axis1 = PositiveAxis1 | NegativeAxis1,
		Axis2 = PositiveAxis2 | NegativeAxis2,
		Axis3 = PositiveAxis3 | NegativeAxis3,
		Any = Axis0 | Axis1 | Axis2 | Axis3,
	}
}
