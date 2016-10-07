/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System;

namespace Experilous.MakeItTile
{
	[Serializable] public struct WrappableAxis2
	{
		public Vector2 vector;
		public bool isWrapped;

		public WrappableAxis2(Vector2 vector)
		{
			this.vector = vector;
			isWrapped = false;
		}

		public WrappableAxis2(Vector2 vector, bool isWrapped)
		{
			this.vector = vector;
			this.isWrapped = isWrapped;
		}

		public static implicit operator Vector2(WrappableAxis2 axis)
		{
			return axis.vector;
		}

		public static implicit operator Vector3(WrappableAxis2 axis)
		{
			return axis.vector;
		}

		public static implicit operator WrappableAxis2(Vector2 vector)
		{
			return new WrappableAxis2(vector);
		}
	}
}
