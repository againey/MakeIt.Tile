using UnityEngine;
using System;

namespace Experilous.Topological
{
	[Serializable]
	public struct EdgeWrapData
	{
		[SerializeField] private byte _data;

		[Flags]
		private enum DataFlags : byte
		{
			None = (byte)0x00u,
			RepetitionAxisBits = (byte)0x03u, //0b00000011
			UnusedBits = (byte)0x38u, //0b01111100
			IsNegatedAxis = (byte)0x80u, //0b10000000
		}

		public EdgeWrapData(int repetitionAxis, bool isNegatedAxis)
		{
			_data = (byte)
			(
				(repetitionAxis & (int)DataFlags.RepetitionAxisBits) |
				(isNegatedAxis ? (int)DataFlags.IsNegatedAxis : 0)
			);
		}

		public bool isWrapped
		{
			get { return (_data & (int)DataFlags.RepetitionAxisBits) != 0; }
		}

		public int repetitionAxis
		{
			get { return _data & (int)DataFlags.RepetitionAxisBits; }
			set { _data = (byte)((_data & ~(int)DataFlags.RepetitionAxisBits) | (value & (int)DataFlags.RepetitionAxisBits)); }
		}

		public bool isNegatedAxis
		{
			get { return (_data & (int)DataFlags.IsNegatedAxis) != 0; }
			set { _data = (byte)((_data & ~(int)DataFlags.IsNegatedAxis) | (value ? (int)DataFlags.IsNegatedAxis : 0)); }
		}
	}
}
