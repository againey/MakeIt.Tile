/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;

namespace MakeIt.Tile
{
	/// <summary>
	/// A concrete <see cref="Vector3"/>-typed face attributes collection that understands
	/// wrap-around behavior and will adjust position values appropriately when accessed.
	/// Derived from <see cref="ScriptableObject"/> and can therefore be serialized,
	/// referenced, and turned into an asset by Unity.
	/// </summary>
	/// <remarks><para>Because Unity will serialize neither interface nor generic type,
	/// this and other attribute classes drived from <see cref="ScriptableObject"/> are
	/// necessary for native serialization.</para></remarks>
	public class PositionalFaceAttribute : FaceArrayAttribute<Vector3>
	{
		/// <summary>
		/// The surface that performs any positional offsets necessary.
		/// </summary>
		public Surface surface;

		/// <summary>
		/// Creates a <see cref="Vector3"/>-typed positional face attributes collection using the given array as the face attribute values.
		/// </summary>
		/// <param name="surface">The surface that will perform any positional offsets necessary.</param>
		/// <param name="array">The array of face attribute values which will be stored, by reference, within the created face attributes collection.</param>
		/// <returns>A <see cref="Vector3"/>-typed positional face attributes collection.</returns>
		public static PositionalFaceAttribute Create(Surface surface, Vector3[] array)
		{
			var instance = CreateInstance<PositionalFaceAttribute>();
			instance.surface = surface;
			instance.array = array;
			return instance;
		}

		/// <summary>
		/// Creates a <see cref="Vector3"/>-typed face attributes collection, allocating an array suitable for the given number of faces.
		/// </summary>
		/// <param name="surface">The surface that will perform any positional offsets necessary.</param>
		/// <param name="faceCount">The number of faces for which the face attribute collection should have sufficient storage.</param>
		/// <returns>A <see cref="Vector3"/>-typed positional face attributes collection.</returns>
		public static PositionalFaceAttribute Create(Surface surface, int faceCount)
		{
			return Create(surface, new Vector3[faceCount]);
		}

		/// <summary>
		/// Lookup the attribute value for the face of the edge indicated, wrapping according to the attribute's surface if necessary.
		/// </summary>
		/// <param name="e">The edge whose face's attribute value is desired.</param>
		/// <returns>The attribute value for the face of the edge indicated, relative to the edge.</returns>
		/// <remarks><para>To get the raw value unwrapped, see <see cref="P:MakeIt.Tile.FaceArrayAttribute`1.Item(MakeIt.Tile.Topology.Face)"/></para></remarks>
		public override Vector3 this[Topology.HalfEdge e]
		{
			get { return surface.OffsetEdgeToFaceAttribute(array[e.face.index], e.wrap); }
			set { array[e.face.index] = surface.ReverseOffsetEdgeToFaceAttribute(array[e.face.index], e.wrap); }
		}

		/// <summary>
		/// Lookup the attribute value for the face of the edge indicated, wrapping according to the attribute's surface if necessary.
		/// </summary>
		/// <param name="e">The edge whose face's attribute value is desired.</param>
		/// <returns>The attribute value for the previous face of the edge indicated, relative to the edge's near vertex.</returns>
		/// <remarks><para>To get the raw value unwrapped, see <see cref="P:MakeIt.Tile.FaceArrayAttribute`1.Item(MakeIt.Tile.Topology.Face)"/></para></remarks>
		public override Vector3 this[Topology.VertexEdge e]
		{
			get { return surface.OffsetVertToFaceAttribute(array[e.face.index], e.wrap); }
			set { array[e.face.index] = surface.ReverseOffsetVertToFaceAttribute(array[e.face.index], e.wrap); }
		}

		/// <summary>
		/// Lookup the attribute value for the face of the edge indicated, wrapping according to the attribute's surface if necessary.
		/// </summary>
		/// <param name="e">The edge whose face's attribute value is desired.</param>
		/// <returns>The attribute value for the far face of the edge indicated, relative to the edge's near face.</returns>
		/// <remarks><para>To get the raw value unwrapped, see <see cref="P:MakeIt.Tile.FaceArrayAttribute`1.Item(MakeIt.Tile.Topology.Face)"/></para></remarks>
		public override Vector3 this[Topology.FaceEdge e]
		{
			get { return surface.OffsetFaceToFaceAttribute(array[e.face.index], e.wrap); }
			set { array[e.face.index] = surface.ReverseOffsetFaceToFaceAttribute(array[e.face.index], e.wrap); }
		}
	}
}
