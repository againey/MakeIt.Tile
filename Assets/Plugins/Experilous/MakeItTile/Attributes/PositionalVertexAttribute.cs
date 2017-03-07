/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using Experilous.Topologies;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// A concrete <see cref="Vector3"/>-typed vertex attributes collection that understands
	/// wrap-around behavior and will adjust position values appropriately when accessed.
	/// Derived from <see cref="ScriptableObject"/> and can therefore be serialized,
	/// referenced, and turned into an asset by Unity.
	/// </summary>
	/// <remarks><para>Because Unity will serialize neither intervertex nor generic type,
	/// this and other attribute classes drived from <see cref="ScriptableObject"/> are
	/// necessary for native serialization.</para></remarks>
	public class PositionalVertexAttribute : VertexArrayAttribute<Vector3>
	{
		/// <summary>
		/// The surface that performs any positional offsets necessary.
		/// </summary>
		public Surface surface;

		/// <summary>
		/// Creates a <see cref="Vector3"/>-typed positional vertex attributes collection using the given array as the vertex attribute values.
		/// </summary>
		/// <param name="surface">The surface that will perform any positional offsets necessary.</param>
		/// <param name="array">The array of vertex attribute values which will be stored, by reference, within the created vertex attributes collection.</param>
		/// <returns>A <see cref="Vector3"/>-typed positional vertex attributes collection.</returns>
		public static PositionalVertexAttribute Create(Surface surface, Vector3[] array)
		{
			var instance = CreateInstance<PositionalVertexAttribute>();
			instance.surface = surface;
			instance.array = array;
			return instance;
		}

		/// <summary>
		/// Creates a <see cref="Vector3"/>-typed vertex attributes collection, allocating an array suitable for the given number of vertices.
		/// </summary>
		/// <param name="surface">The surface that will perform any positional offsets necessary.</param>
		/// <param name="vertexCount">The number of vertices for which the vertex attribute collection should have sufficient storage.</param>
		/// <returns>A <see cref="Vector3"/>-typed positional vertex attributes collection.</returns>
		public static PositionalVertexAttribute Create(Surface surface, int vertexCount)
		{
			return Create(surface, new Vector3[vertexCount]);
		}

		/// <summary>
		/// Lookup the attribute value for the vertex of the edge indicated, wrapping according to the attribute's surface if necessary.
		/// </summary>
		/// <param name="e">The edge whose vertex's attribute value is desired.</param>
		/// <returns>The attribute value for the vertex of the edge indicated, relative to the edge.</returns>
		/// <remarks><para>To get the raw value unwrapped, see <see cref="P:Experilous.MakeItTile.VertexArrayAttribute`1.Item(Experilous.MakeItTile.Topology.Vertex)"/></para></remarks>
		public override Vector3 this[Topology.HalfEdge e]
		{
			get { return surface.OffsetEdgeToVertAttribute(array[e.vertex.index], e.wrap); }
			set { array[e.vertex.index] = surface.ReverseOffsetEdgeToVertAttribute(array[e.vertex.index], e.wrap); }
		}

		/// <summary>
		/// Lookup the attribute value for the vertex of the edge indicated, wrapping according to the attribute's surface if necessary.
		/// </summary>
		/// <param name="e">The edge whose vertex's attribute value is desired.</param>
		/// <returns>The attribute value for the far vertex of the edge indicated, relative to the edge's near vertex.</returns>
		/// <remarks><para>To get the raw value unwrapped, see <see cref="P:Experilous.MakeItTile.VertexArrayAttribute`1.Item(Experilous.MakeItTile.Topology.Vertex)"/></para></remarks>
		public override Vector3 this[Topology.VertexEdge e]
		{
			get { return surface.OffsetVertToVertAttribute(array[e.vertex.index], e.wrap); }
			set { array[e.vertex.index] = surface.ReverseOffsetVertToVertAttribute(array[e.vertex.index], e.wrap); }
		}

		/// <summary>
		/// Lookup the attribute value for the vertex of the edge indicated, wrapping according to the attribute's surface if necessary.
		/// </summary>
		/// <param name="e">The edge whose vertex's attribute value is desired.</param>
		/// <returns>The attribute value for the next vertex of the edge indicated, relative to the edge's near face.</returns>
		/// <remarks><para>To get the raw value unwrapped, see <see cref="P:Experilous.MakeItTile.VertexArrayAttribute`1.Item(Experilous.MakeItTile.Topology.Vertex)"/></para></remarks>
		public override Vector3 this[Topology.FaceEdge e]
		{
			get { return surface.OffsetFaceToVertAttribute(array[e.vertex.index], e.wrap); }
			set { array[e.vertex.index] = surface.ReverseOffsetFaceToVertAttribute(array[e.vertex.index], e.wrap); }
		}
	}
}
