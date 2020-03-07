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

namespace MakeIt.Tile
{
	/// <summary>
	/// A concrete <see cref="Vector2"/>-typed edge attributes collection, derived from <see cref="ScriptableObject"/>
	/// and can therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	/// <remarks><para>Because Unity will serialize neither interface nor generic type,
	/// this and other attribute classes drived from <see cref="ScriptableObject"/> are
	/// necessary for native serialization.</para></remarks>
	public class Vector2EdgeAttribute : EdgeArrayAttribute<Vector2>
	{
		/// <summary>
		/// Creates a <see cref="Vector2"/>-typed edge attributes collection using the given array as the edge attribute values.
		/// </summary>
		/// <param name="array">The array of edge attribute values which will be stored, by reference, within the created edge attributes collection.</param>
		/// <returns>A <see cref="Vector2"/>-typed edge attributes collection.</returns>
		public static Vector2EdgeAttribute Create(Vector2[] array) { return CreateDerived<Vector2EdgeAttribute>(array); }

		/// <summary>
		/// Creates a <see cref="Vector2"/>-typed edge attributes collection, allocating an array suitable for the given number of edges.
		/// </summary>
		/// <param name="edgeCount">The number of edges for which the edge attribute collection should have sufficient storage.</param>
		/// <returns>A <see cref="Vector2"/>-typed edge attributes collection.</returns>
		public static Vector2EdgeAttribute Create(int edgeCount) { return CreateDerived<Vector2EdgeAttribute>(edgeCount); }
	}
}
