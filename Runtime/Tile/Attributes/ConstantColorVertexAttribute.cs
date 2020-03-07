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
	/// A concrete <see cref="Color"/>-typed vertex attributes collection which returns
	/// the same constant value for all vertexs, derived from <see cref="ScriptableObject"/>
	/// and can therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	/// <remarks><para>Because Unity will serialize neither interface nor generic type,
	/// this and other attribute classes drived from <see cref="ScriptableObject"/> are
	/// necessary for native serialization.</para></remarks>
	public class ConstantColorVertexAttribute : VertexConstantAttribute<Color>
	{
		/// <summary>
		/// Creates a <see cref="Color"/>-typed vertex attributes collection using the given value as the shared constant vertexa attribute value.
		/// </summary>
		/// <param name="constant">The value to use as the shared constant vertexa attribute value.</param>
		/// <returns>A <see cref="Color"/>-typed constant vertex attributes collection.</returns>
		public static ConstantColorVertexAttribute Create(Color constant) { return CreateDerived<ConstantColorVertexAttribute>(constant); }
	}
}
