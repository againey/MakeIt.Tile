/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using System;

namespace Experilous.Topological
{
	/// <summary>
	/// Compactly stores the wrapping relations between an edge and it's various neighbors.
	/// </summary>
	/// <remarks>
	/// <para>For a topological world that allows wrapping around along some or all world boundaries, complications
	/// can arise when the topology is embedded within standard Euclidean space.  Consider a flat rectangular world
	/// that wraps around along the right and left sides (such as a map from the Civilization series).  The vertices
	/// along the left edge are in fact the same vertices along the right edge, and so each one will only have a
	/// single position in space, which I will dub the "canonical" position.  But when drawing the tiles on the left
	/// side of the map versus those on the right, the vertex positions need to be treated differently.  If the
	/// canonical positions are the ones that apply to the left side of the map, then when using those vertex
	/// positions along the right side of the map, the full width of the world needs to be added to the canonical
	/// positions to get the effective positions for rendering the tiles.</para>
	/// 
	/// <para>To enable this kind of transformation, one needs to know the context of how an attribute such as
	/// position is being accessed.  This context is defined in terms of which other topology object the attribute
	/// is relative to.  In the above example, vertex positions are being accessed relative to the face that is
	/// being rendered, so it is a vertex-to-face relation.  If we know the wrapping relation between any particular
	/// face and a neighboring vertex, then we can decide how to transform the canonical position to get the final
	/// position to be used.</para>
	/// 
	/// <para>Since wrapping is predominantly relevant only for planar worlds, and further is most commonly used
	/// for worlds with simple translational wrapping, these assumptions are built directly into the enum by
	/// representing wrapping relations as translational transformations along precisely two straight axes.  Along
	/// either axis, the translation could be either positive or negative.  The basic wrapping relation can
	/// therefore be stored in four bits: two bits for each axis, with one bit for the positive direction and one
	/// bit for the negative direction.  Having both the positive and negative bit unset is perfectly acceptable
	/// and will in fact be the most common arrangement.  Technically, having both the positive and negative bit
	/// set is also acceptable and can in fact arise through various bitwise transformations, but it is essentially
	/// equivalent to having no bits set (the two directions cancel each other out), and so the unset pair is
	/// treated as the canonical form of this case, and having both bits set should only occur during carefully
	/// managed intermediate steps of transformation.</para>
	/// 
	/// <para>These four bits are enough to represent a full relation, but only between two different topology
	/// elements.  The earlier example required face-to-vertex relations.  In total, there are eight relations that
	/// can be usefully represented.  These are vertex-to-edge, face-to-edge, edge-to-vertex, edge-to-face, vertex-
	/// to-vertex, vertex-to-face, face-to-vertex, and face-to-face.  Only the first four are necessary, and the
	/// last four can be fully derived from the first four.  However, the last four are likely to be more commonly
	/// required, so it is ideal to have them pre-computed.  Additionally, eight relations results in a 32-bit
	/// enum, and this operates more naturally with C# and with Unity's serialization system, so all eight
	/// relations are represented in this enum.</para>
	/// 
	/// <para>For examples of this enum in use, see the positional attributes for vertices, edges, and faces, along
	/// with the various offset functions implemented in <see cref="PlanarSurface"/>.  For utilities to transform
	/// and otherwise manipulate edge wrap values, see the static <see cref="EdgeWrapUtility"/> class.</para>
	/// </remarks>
	/// <seealso cref="Topology.EdgeData"/>
	/// <seealso cref="EdgeWrapUtility"/>
	/// <seealso cref="PositionalVertexAttribute"/>
	/// <seealso cref="PositionalEdgeAttribute"/>
	/// <seealso cref="PositionalFaceAttribute"/>
	/// <seealso cref="PlanarSurface"/>
	[Serializable, Flags]
	public enum EdgeWrap : int
	{
		// Default state of no bits set, and therefore no wrapping relations of any kind.
		None = 0,

		// Single bit enum items:

		PosVertToEdgeAxis0 = 0x00000001,
		NegVertToEdgeAxis0 = 0x00000002,
		PosVertToEdgeAxis1 = 0x00000004,
		NegVertToEdgeAxis1 = 0x00000008,

		PosFaceToEdgeAxis0 = 0x00000010,
		NegFaceToEdgeAxis0 = 0x00000020,
		PosFaceToEdgeAxis1 = 0x00000040,
		NegFaceToEdgeAxis1 = 0x00000080,

		PosEdgeToVertAxis0 = 0x00000100,
		NegEdgeToVertAxis0 = 0x00000200,
		PosEdgeToVertAxis1 = 0x00000400,
		NegEdgeToVertAxis1 = 0x00000800,

		PosEdgeToFaceAxis0 = 0x00001000,
		NegEdgeToFaceAxis0 = 0x00002000,
		PosEdgeToFaceAxis1 = 0x00004000,
		NegEdgeToFaceAxis1 = 0x00008000,

		PosVertToVertAxis0 = 0x00010000,
		NegVertToVertAxis0 = 0x00020000,
		PosVertToVertAxis1 = 0x00040000,
		NegVertToVertAxis1 = 0x00080000,

		PosVertToFaceAxis0 = 0x00100000,
		NegVertToFaceAxis0 = 0x00200000,
		PosVertToFaceAxis1 = 0x00400000,
		NegVertToFaceAxis1 = 0x00800000,

		PosFaceToVertAxis0 = 0x01000000,
		NegFaceToVertAxis0 = 0x02000000,
		PosFaceToVertAxis1 = 0x04000000,
		NegFaceToVertAxis1 = 0x08000000,

		PosFaceToFaceAxis0 = 0x10000000,
		NegFaceToFaceAxis0 = 0x20000000,
		PosFaceToFaceAxis1 = 0x40000000,
		NegFaceToFaceAxis1 = ~0x7FFFFFFF, //0x80000000, except I need the enum type to be int to keep Unity sane.

		// Aggregated multi-bit enum items for convenience:

		VertToEdgeAxis0 = PosVertToEdgeAxis0 | NegVertToEdgeAxis0,
		VertToEdgeAxis1 = PosVertToEdgeAxis1 | NegVertToEdgeAxis1,
		FaceToEdgeAxis0 = PosFaceToEdgeAxis0 | NegFaceToEdgeAxis0,
		FaceToEdgeAxis1 = PosFaceToEdgeAxis1 | NegFaceToEdgeAxis1,
		EdgeToVertAxis0 = PosEdgeToVertAxis0 | NegEdgeToVertAxis0,
		EdgeToVertAxis1 = PosEdgeToVertAxis1 | NegEdgeToVertAxis1,
		EdgeToFaceAxis0 = PosEdgeToFaceAxis0 | NegEdgeToFaceAxis0,
		EdgeToFaceAxis1 = PosEdgeToFaceAxis1 | NegEdgeToFaceAxis1,

		VertToVertAxis0 = PosVertToVertAxis0 | NegVertToVertAxis0,
		VertToVertAxis1 = PosVertToVertAxis1 | NegVertToVertAxis1,
		VertToFaceAxis0 = PosVertToFaceAxis0 | NegVertToFaceAxis0,
		VertToFaceAxis1 = PosVertToFaceAxis1 | NegVertToFaceAxis1,
		FaceToVertAxis0 = PosFaceToVertAxis0 | NegFaceToVertAxis0,
		FaceToVertAxis1 = PosFaceToVertAxis1 | NegFaceToVertAxis1,
		FaceToFaceAxis0 = PosFaceToFaceAxis0 | NegFaceToFaceAxis0,
		FaceToFaceAxis1 = PosFaceToFaceAxis1 | NegFaceToFaceAxis1,

		VertToVert = VertToVertAxis0 | VertToVertAxis1,
		VertToFace = VertToFaceAxis0 | VertToFaceAxis1,
		FaceToVert = FaceToVertAxis0 | FaceToVertAxis1,
		FaceToFace = FaceToFaceAxis0 | FaceToFaceAxis1,

		VertToEdge = VertToEdgeAxis0 | VertToEdgeAxis1,
		FaceToEdge = FaceToEdgeAxis0 | FaceToEdgeAxis1,
		EdgeToVert = EdgeToVertAxis0 | EdgeToVertAxis1,
		EdgeToFace = EdgeToFaceAxis0 | EdgeToFaceAxis1,

		EdgeTo = EdgeToVert | EdgeToFace,
		ToEdge = VertToEdge | FaceToEdge,

		VertToEdgeToVert = VertToEdge | EdgeToVert,
		FaceToEdgeToFace = FaceToEdge | EdgeToFace,

		VertToNonEdge = VertToVert | VertToFace,
		FaceToNonEdge = FaceToVert | FaceToFace,

		NonEdgeToVert = VertToVert | FaceToVert,
		NonEdgeToFace = VertToFace | FaceToFace,

		VertTo = VertToNonEdge | VertToEdge,
		FaceTo = FaceToNonEdge | FaceToEdge,

		ToVert = NonEdgeToVert | EdgeToVert,
		ToFace = NonEdgeToFace | EdgeToFace,

		Edge = EdgeTo | ToEdge,
		NonEdge = NonEdgeToVert | NonEdgeToFace,

		NonEdgePosAxis0 = PosVertToVertAxis0 | PosVertToFaceAxis0 | PosFaceToVertAxis0 | PosFaceToFaceAxis0,
		NonEdgePosAxis1 = PosVertToVertAxis1 | PosVertToFaceAxis1 | PosFaceToVertAxis1 | PosFaceToFaceAxis1,
		EdgePosAxis0 = PosVertToEdgeAxis0 | PosFaceToEdgeAxis0 | PosEdgeToVertAxis0 | PosEdgeToFaceAxis0,
		EdgePosAxis1 = PosVertToEdgeAxis1 | PosFaceToEdgeAxis1 | PosEdgeToVertAxis1 | PosEdgeToFaceAxis1,
		PosAxis0 = NonEdgePosAxis0 | EdgePosAxis0,
		PosAxis1 = NonEdgePosAxis1 | EdgePosAxis1,
		Pos = PosAxis0 | PosAxis1,

		NonEdgeNegAxis0 = NegVertToVertAxis0 | NegVertToFaceAxis0 | NegFaceToVertAxis0 | NegFaceToFaceAxis0,
		NonEdgeNegAxis1 = NegVertToVertAxis1 | NegVertToFaceAxis1 | NegFaceToVertAxis1 | NegFaceToFaceAxis1,
		EdgeNegAxis0 = NegVertToEdgeAxis0 | NegFaceToEdgeAxis0 | NegEdgeToVertAxis0 | NegEdgeToFaceAxis0,
		EdgeNegAxis1 = NegVertToEdgeAxis1 | NegFaceToEdgeAxis1 | NegEdgeToVertAxis1 | NegEdgeToFaceAxis1,
		NegAxis0 = NonEdgeNegAxis0 | EdgeNegAxis0,
		NegAxis1 = NonEdgeNegAxis1 | EdgeNegAxis1,
		Neg = NegAxis0 | NegAxis1,

		Axis0 = PosAxis0 | NegAxis0,
		Axis1 = PosAxis1 | NegAxis1,

		All = Pos | Neg,
	}
}
