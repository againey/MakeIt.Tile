/******************************************************************************\
* Copyright Andy Gainey                                                        *
*                                                                              *
* Licensed under the Apache License, Version 2.0 (the "License");              *
* you may not use this file except in compliance with the License.             *
* You may obtain a copy of the License at                                      *
*                                                                              *
*     http://www.apache.org/licenses/LICENSE-2.0                               *
*                                                                              *
* Unless required by applicable law or agreed to in writing, software          *
* distributed under the License is distributed on an "AS IS" BASIS,            *
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.     *
* See the License for the specific language governing permissions and          *
* limitations under the License.                                               *
\******************************************************************************/

using UnityEngine;
using System;

namespace MakeIt.Tile
{
	/// <summary>
	/// A two-dimensional axis that is optionally wrappable, that is, movement beyond the
	/// boundaries of a manifold along this axis will result in wrap-around of the moving
	/// object's position to the other side of the manifold.
	/// </summary>
	[Serializable] public struct WrappableAxis2
	{
		/// <summary>
		/// The direction vector of the axis, which is not necessarily a unit vector.
		/// </summary>
		public Vector2 vector;

		/// <summary>
		/// Indicates whether the axis exhibits wrap-around behavior.
		/// </summary>
		public bool isWrapped;

		/// <summary>
		/// Constructs a non-wrapping axis with the given direction vector.
		/// </summary>
		/// <param name="vector">The direction vector of the axis, which is necessarily a unit vector, nor will it be converted to one.</param>
		public WrappableAxis2(Vector2 vector)
		{
			this.vector = vector;
			isWrapped = false;
		}

		/// <summary>
		/// Constructs an optionally wrapping axis with the given direction vector.
		/// </summary>
		/// <param name="vector">The direction vector of the axis, which is necessarily a unit vector, nor will it be converted to one.</param>
		/// <param name="isWrapped">Indicates whether the axis exhibits wrap-around behavior.</param>
		public WrappableAxis2(Vector2 vector, bool isWrapped)
		{
			this.vector = vector;
			this.isWrapped = isWrapped;
		}

		/// <summary>
		/// Implicitly converts a wrappable axis to a <see cref="Vector2"/>, essentialy just dropping the wrappable field.
		/// </summary>
		/// <param name="axis">The axis to be converted.</param>
		/// <returns>The direction vector of the converted axis.</returns>
		public static implicit operator Vector2(WrappableAxis2 axis)
		{
			return axis.vector;
		}

		/// <summary>
		/// Implicitly converts a wrappable axis to a <see cref="Vector3"/>, essentialy just dropping the wrappable field and setting the z component to zero.
		/// </summary>
		/// <param name="axis">The axis to be converted.</param>
		/// <returns>The direction vector of the converted axis, with the z component set to zero.</returns>
		public static implicit operator Vector3(WrappableAxis2 axis)
		{
			return axis.vector;
		}

		/// <summary>
		/// Implicitly converts a <see cref="Vector2"/> direction vector to a non-wrapping axis.
		/// </summary>
		/// <param name="vector">The direction vector to be converted.</param>
		/// <returns>The a non-wrapping axis with the converted direction vector.</returns>
		public static implicit operator WrappableAxis2(Vector2 vector)
		{
			return new WrappableAxis2(vector);
		}
	}
}
