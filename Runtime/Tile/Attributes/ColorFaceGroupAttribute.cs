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
	/// A concrete <see cref="Color"/>-typed face group attributes collection, derived from <see cref="ScriptableObject"/>
	/// and can therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	/// <remarks><para>Because Unity will serialize neither interface group nor generic type,
	/// this and other attribute classes drived from <see cref="ScriptableObject"/> are
	/// necessary for native serialization.</para></remarks>
	public class ColorFaceGroupAttribute : FaceGroupArrayAttribute<Color>
	{
		/// <summary>
		/// Creates a <see cref="Color"/>-typed face group attributes collection using the given array as the face group attribute values.
		/// </summary>
		/// <param name="array">The array of face group attribute values which will be stored, by reference, within the created face group attributes collection.</param>
		/// <returns>A <see cref="Color"/>-typed face group attributes collection.</returns>
		public static ColorFaceGroupAttribute Create(Color[] array) { return CreateDerived<ColorFaceGroupAttribute>(array); }

		/// <summary>
		/// Creates a <see cref="Color"/>-typed face group attributes collection, allocating an array suitable for the given number of face groups.
		/// </summary>
		/// <param name="groupCount">The number of face groups for which the face group attribute collection should have sufficient storage.</param>
		/// <returns>A <see cref="Color"/>-typed face group attributes collection.</returns>
		public static ColorFaceGroupAttribute Create(int groupCount) { return CreateDerived<ColorFaceGroupAttribute>(groupCount); }
	}
}
