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
	/// A concrete <see cref="Color"/>-typed face attributes collection for
	/// looking up attributes based on the group index for a face and the attribute value
	/// for that group, derived from <see cref="ScriptableObject"/> and can
	/// therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	/// <remarks><para>Because Unity will serialize neither interface group nor generic type,
	/// this and other attribute classes drived from <see cref="ScriptableObject"/> are
	/// necessary for native serialization.</para></remarks>
	public class ColorFaceGroupLookupFaceAttribute : FaceGroupLookupFaceAttribute<Color, ColorFaceGroupAttribute>
	{
		/// <summary>
		/// Creates a <see cref="Color"/>-typed face attributes collection using the given face group indices and face group color values.
		/// </summary>
		/// <param name="faceGroupIndices">The group index attributes collection mapping each face to a particular face group.</param>
		/// <param name="faceGroupColors">The colors of the face groups.</param>
		/// <returns>A <see cref="Color"/>-typed face attributes collection.</returns>
		public static ColorFaceGroupLookupFaceAttribute Create(IntFaceAttribute faceGroupIndices, ColorFaceGroupAttribute faceGroupColors)
		{
			var instance = CreateInstance<ColorFaceGroupLookupFaceAttribute>();
			instance.faceGroupData = faceGroupColors;
			instance.faceGroupIndices = faceGroupIndices;
			return instance;
		}
	}
}
