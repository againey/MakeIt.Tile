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
	/// A concrete <see cref="Vector3"/>-typed edge attributes collection that understands
	/// wrap-around behavior and will adjust position values appropriately when accessed.
	/// Derived from <see cref="ScriptableObject"/> and can therefore be serialized,
	/// referenced, and turned into an asset by Unity.
	/// </summary>
	/// <remarks><para>Because Unity will serialize neither interface nor generic type,
	/// this and other attribute classes drived from <see cref="ScriptableObject"/> are
	/// necessary for native serialization.</para></remarks>
	public class PositionalEdgeAttribute : EdgeArrayAttribute<Vector3>
	{
		/// <summary>
		/// The surface that performs any positional offsets necessary.
		/// </summary>
		public Surface surface;

		/// <summary>
		/// Creates a <see cref="Vector3"/>-typed positional edge attributes collection using the given array as the edge attribute values.
		/// </summary>
		/// <param name="surface">The surface that will perform any positional offsets necessary.</param>
		/// <param name="array">The array of edge attribute values which will be stored, by reference, within the created edge attributes collection.</param>
		/// <returns>A <see cref="Vector3"/>-typed positional edge attributes collection.</returns>
		public static PositionalEdgeAttribute Create(Surface surface, Vector3[] array)
		{
			var instance = CreateInstance<PositionalEdgeAttribute>();
			instance.surface = surface;
			instance.array = array;
			return instance;
		}

		/// <summary>
		/// Creates a <see cref="Vector3"/>-typed edge attributes collection, allocating an array suitable for the given number of edges.
		/// </summary>
		/// <param name="surface">The surface that will perform any positional offsets necessary.</param>
		/// <param name="edgeCount">The number of edges for which the edge attribute collection should have sufficient storage.</param>
		/// <returns>A <see cref="Vector3"/>-typed positional edge attributes collection.</returns>
		public static PositionalEdgeAttribute Create(Surface surface, int edgeCount)
		{
			return Create(surface, new Vector3[edgeCount]);
		}

		/// <summary>
		/// Lookup the attribute value for an edge relative to a neighboring vertex, wrapping according to the attribute's surface if necessary.
		/// </summary>
		/// <param name="e">The edge (represented as a vertex edge) whose attribute value is desired.</param>
		/// <returns>The attribute value for the indicated edge, relative to its near vertex.</returns>
		/// <remarks><para>To get the raw value unwrapped, see <see cref="P:MakeIt.Tile.EdgeArrayAttribute`1.Item(MakeIt.Tile.Topology.HalfEdge)"/></para></remarks>
		public override Vector3 this[Topology.VertexEdge e]
		{
			get { return surface.OffsetVertToEdgeAttribute(array[e.index], e.wrap); }
			set { array[e.index] = surface.ReverseOffsetVertToEdgeAttribute(array[e.index], e.wrap); }
		}

		/// <summary>
		/// Lookup the attribute value for an edge relative to a neighboring face, wrapping according to the attribute's surface if necessary.
		/// </summary>
		/// <param name="e">The edge (represented as a face edge) whose attribute value is desired.</param>
		/// <returns>The attribute value for the indicated edge, relative to its near face.</returns>
		/// <remarks><para>To get the raw value unwrapped, see <see cref="P:MakeIt.Tile.EdgeArrayAttribute`1.Item(MakeIt.Tile.Topology.HalfEdge)"/></para></remarks>
		public override Vector3 this[Topology.FaceEdge e]
		{
			get { return surface.OffsetFaceToEdgeAttribute(array[e.index], e.wrap); }
			set { array[e.index] = surface.ReverseOffsetFaceToEdgeAttribute(array[e.index], e.wrap); }
		}
	}
}
