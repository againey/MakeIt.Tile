/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System;

namespace Experilous.Topologies
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
	/// with the various offset functions implemented in <see cref="QuadrilateralSurface"/>.  For utilities to transform
	/// and otherwise manipulate edge wrap values, see the static <see cref="EdgeWrapUtility"/> class.</para>
	/// </remarks>
	/// <seealso cref="Topology.EdgeData"/>
	/// <seealso cref="EdgeWrapUtility"/>
	/// <seealso cref="PositionalVertexAttribute"/>
	/// <seealso cref="PositionalEdgeAttribute"/>
	/// <seealso cref="PositionalFaceAttribute"/>
	/// <seealso cref="QuadrilateralSurface"/>
	[Serializable, Flags]
	public enum EdgeWrap : int
	{
		/// <summary>Default state of no bits set, and therefore no wrapping relations of any kind.</summary>
		None = 0,

		// Single bit enum items:

		/// <summary>Wrap in the positive direction during the transition from vertex to edge along the first axis.</summary>
		PosVertToEdgeAxis0 = 0x00000001,
		/// <summary>Wrap in the negative direction during the transition from vertex to edge along the first axis.</summary>
		NegVertToEdgeAxis0 = 0x00000002,
		/// <summary>Wrap in the positive direction during the transition from vertex to edge along the second axis.</summary>
		PosVertToEdgeAxis1 = 0x00000004,
		/// <summary>Wrap in the negative direction during the transition from vertex to edge along the second axis.</summary>
		NegVertToEdgeAxis1 = 0x00000008,

		/// <summary>Wrap in the positive direction during the transition from face to edge along the first axis.</summary>
		PosFaceToEdgeAxis0 = 0x00000010,
		/// <summary>Wrap in the negative direction during the transition from face to edge along the first axis.</summary>
		NegFaceToEdgeAxis0 = 0x00000020,
		/// <summary>Wrap in the positive direction during the transition from face to edge along the second axis.</summary>
		PosFaceToEdgeAxis1 = 0x00000040,
		/// <summary>Wrap in the negative direction during the transition from face to edge along the second axis.</summary>
		NegFaceToEdgeAxis1 = 0x00000080,

		/// <summary>Wrap in the positive direction during the transition from edge to vertex along the first axis.</summary>
		PosEdgeToVertAxis0 = 0x00000100,
		/// <summary>Wrap in the negative direction during the transition from edge to vertex along the first axis.</summary>
		NegEdgeToVertAxis0 = 0x00000200,
		/// <summary>Wrap in the positive direction during the transition from edge to vertex along the second axis.</summary>
		PosEdgeToVertAxis1 = 0x00000400,
		/// <summary>Wrap in the negative direction during the transition from edge to vertex along the second axis.</summary>
		NegEdgeToVertAxis1 = 0x00000800,

		/// <summary>Wrap in the positive direction during the transition from edge to face along the first axis.</summary>
		PosEdgeToFaceAxis0 = 0x00001000,
		/// <summary>Wrap in the negative direction during the transition from edge to face along the first axis.</summary>
		NegEdgeToFaceAxis0 = 0x00002000,
		/// <summary>Wrap in the positive direction during the transition from edge to face along the second axis.</summary>
		PosEdgeToFaceAxis1 = 0x00004000,
		/// <summary>Wrap in the negative direction during the transition from edge to face along the second axis.</summary>
		NegEdgeToFaceAxis1 = 0x00008000,

		/// <summary>Wrap in the positive direction during the transition from vertex to vertex along the first axis.</summary>
		PosVertToVertAxis0 = 0x00010000,
		/// <summary>Wrap in the negative direction during the transition from vertex to vertex along the first axis.</summary>
		NegVertToVertAxis0 = 0x00020000,
		/// <summary>Wrap in the positive direction during the transition from vertex to vertex along the second axis.</summary>
		PosVertToVertAxis1 = 0x00040000,
		/// <summary>Wrap in the negative direction during the transition from vertex to vertex along the second axis.</summary>
		NegVertToVertAxis1 = 0x00080000,

		/// <summary>Wrap in the positive direction during the transition from vertex to face along the first axis.</summary>
		PosVertToFaceAxis0 = 0x00100000,
		/// <summary>Wrap in the negative direction during the transition from vertex to face along the first axis.</summary>
		NegVertToFaceAxis0 = 0x00200000,
		/// <summary>Wrap in the positive direction during the transition from vertex to face along the second axis.</summary>
		PosVertToFaceAxis1 = 0x00400000,
		/// <summary>Wrap in the negative direction during the transition from vertex to face along the second axis.</summary>
		NegVertToFaceAxis1 = 0x00800000,

		/// <summary>Wrap in the positive direction during the transition from face to vertex along the first axis.</summary>
		PosFaceToVertAxis0 = 0x01000000,
		/// <summary>Wrap in the negative direction during the transition from face to vertex along the first axis.</summary>
		NegFaceToVertAxis0 = 0x02000000,
		/// <summary>Wrap in the positive direction during the transition from face to vertex along the second axis.</summary>
		PosFaceToVertAxis1 = 0x04000000,
		/// <summary>Wrap in the negative direction during the transition from face to vertex along the second axis.</summary>
		NegFaceToVertAxis1 = 0x08000000,

		/// <summary>Wrap in the positive direction during the transition from face to face along the first axis.</summary>
		PosFaceToFaceAxis0 = 0x10000000,
		/// <summary>Wrap in the negative direction during the transition from face to face along the first axis.</summary>
		NegFaceToFaceAxis0 = 0x20000000,
		/// <summary>Wrap in the positive direction during the transition from face to face along the second axis.</summary>
		PosFaceToFaceAxis1 = 0x40000000,
		/// <summary>Wrap in the negative direction during the transition from face to face along the second axis.</summary>
		NegFaceToFaceAxis1 = ~0x7FFFFFFF, //0x80000000, except I need the enum type to be int to keep Unity sane.

		// Aggregated multi-bit enum items for convenience:

		/// <summary>Wrap during the transition from vertex to edge along the first axis.</summary>
		VertToEdgeAxis0 = PosVertToEdgeAxis0 | NegVertToEdgeAxis0,
		/// <summary>Wrap during the transition from vertex to edge along the second axis.</summary>
		VertToEdgeAxis1 = PosVertToEdgeAxis1 | NegVertToEdgeAxis1,
		/// <summary>Wrap during the transition from face to edge along the first axis.</summary>
		FaceToEdgeAxis0 = PosFaceToEdgeAxis0 | NegFaceToEdgeAxis0,
		/// <summary>Wrap during the transition from face to edge along the second axis.</summary>
		FaceToEdgeAxis1 = PosFaceToEdgeAxis1 | NegFaceToEdgeAxis1,
		/// <summary>Wrap during the transition from edge to vertex along the first axis.</summary>
		EdgeToVertAxis0 = PosEdgeToVertAxis0 | NegEdgeToVertAxis0,
		/// <summary>Wrap during the transition from edge to vertex along the second axis.</summary>
		EdgeToVertAxis1 = PosEdgeToVertAxis1 | NegEdgeToVertAxis1,
		/// <summary>Wrap during the transition from edge to face along the first axis.</summary>
		EdgeToFaceAxis0 = PosEdgeToFaceAxis0 | NegEdgeToFaceAxis0,
		/// <summary>Wrap during the transition from edge to face along the second axis.</summary>
		EdgeToFaceAxis1 = PosEdgeToFaceAxis1 | NegEdgeToFaceAxis1,

		/// <summary>Wrap during the transition from vertex to vertex along the first axis.</summary>
		VertToVertAxis0 = PosVertToVertAxis0 | NegVertToVertAxis0,
		/// <summary>Wrap during the transition from vertex to vertex along the second axis.</summary>
		VertToVertAxis1 = PosVertToVertAxis1 | NegVertToVertAxis1,
		/// <summary>Wrap during the transition from vertex to face along the first axis.</summary>
		VertToFaceAxis0 = PosVertToFaceAxis0 | NegVertToFaceAxis0,
		/// <summary>Wrap during the transition from vertex to face along the second axis.</summary>
		VertToFaceAxis1 = PosVertToFaceAxis1 | NegVertToFaceAxis1,
		/// <summary>Wrap during the transition from face to vertex along the first axis.</summary>
		FaceToVertAxis0 = PosFaceToVertAxis0 | NegFaceToVertAxis0,
		/// <summary>Wrap during the transition from face to vertex along the second axis.</summary>
		FaceToVertAxis1 = PosFaceToVertAxis1 | NegFaceToVertAxis1,
		/// <summary>Wrap during the transition from face to face along the first axis.</summary>
		FaceToFaceAxis0 = PosFaceToFaceAxis0 | NegFaceToFaceAxis0,
		/// <summary>Wrap during the transition from face to face along the second axis.</summary>
		FaceToFaceAxis1 = PosFaceToFaceAxis1 | NegFaceToFaceAxis1,

		/// <summary>Wrap during the transition from vertex to vertex along either axis.</summary>
		VertToVert = VertToVertAxis0 | VertToVertAxis1,
		/// <summary>Wrap during the transition from vertex to face along either axis.</summary>
		VertToFace = VertToFaceAxis0 | VertToFaceAxis1,
		/// <summary>Wrap during the transition from face to vertex along either axis.</summary>
		FaceToVert = FaceToVertAxis0 | FaceToVertAxis1,
		/// <summary>Wrap during the transition from face to face along either axis.</summary>
		FaceToFace = FaceToFaceAxis0 | FaceToFaceAxis1,

		/// <summary>Wrap during the transition from vertex to edge along either axis.</summary>
		VertToEdge = VertToEdgeAxis0 | VertToEdgeAxis1,
		/// <summary>Wrap during the transition from face to edge along either axis.</summary>
		FaceToEdge = FaceToEdgeAxis0 | FaceToEdgeAxis1,
		/// <summary>Wrap during the transition from edge to vert along either axis.</summary>
		EdgeToVert = EdgeToVertAxis0 | EdgeToVertAxis1,
		/// <summary>Wrap during the transition from edge to face along either axis.</summary>
		EdgeToFace = EdgeToFaceAxis0 | EdgeToFaceAxis1,

		/// <summary>Wrap during the transition from edge to vertex or face along either axis.</summary>
		EdgeTo = EdgeToVert | EdgeToFace,
		/// <summary>Wrap during the transition from vertex or face to edge along either axis.</summary>
		ToEdge = VertToEdge | FaceToEdge,

		/// <summary>Wrap during the transition between vertex and edge along either axis.</summary>
		VertToEdgeToVert = VertToEdge | EdgeToVert,
		/// <summary>Wrap during the transition between face and edge along either axis.</summary>
		FaceToEdgeToFace = FaceToEdge | EdgeToFace,

		/// <summary>Wrap during the transition from vertex to vertex or face along either axis.</summary>
		VertToNonEdge = VertToVert | VertToFace,
		/// <summary>Wrap during the transition from face to vertex or face along either axis.</summary>
		FaceToNonEdge = FaceToVert | FaceToFace,

		/// <summary>Wrap during the transition from vertex or face to vertex along either axis.</summary>
		NonEdgeToVert = VertToVert | FaceToVert,
		/// <summary>Wrap during the transition from vertex or face to face along either axis.</summary>
		NonEdgeToFace = VertToFace | FaceToFace,

		/// <summary>Wrap during the transition from vertex along either axis.</summary>
		VertTo = VertToNonEdge | VertToEdge,
		/// <summary>Wrap during the transition from face along either axis.</summary>
		FaceTo = FaceToNonEdge | FaceToEdge,

		/// <summary>Wrap during the transition to vertex along either axis.</summary>
		ToVert = NonEdgeToVert | EdgeToVert,
		/// <summary>Wrap during the transition to face along either axis.</summary>
		ToFace = NonEdgeToFace | EdgeToFace,

		/// <summary>Wrap during the transition to or from edge along either axis.</summary>
		Edge = EdgeTo | ToEdge,
		/// <summary>Wrap during the transition from vertex or face to vertex or face along either axis.</summary>
		NonEdge = NonEdgeToVert | NonEdgeToFace,

		/// <summary>Wrap in the positive direction during the transition from vertex or face to vertex or face along the first axis.</summary>
		NonEdgePosAxis0 = PosVertToVertAxis0 | PosVertToFaceAxis0 | PosFaceToVertAxis0 | PosFaceToFaceAxis0,
		/// <summary>Wrap in the positive direction during the transition from vertex or face to vertex or face along the second axis.</summary>
		NonEdgePosAxis1 = PosVertToVertAxis1 | PosVertToFaceAxis1 | PosFaceToVertAxis1 | PosFaceToFaceAxis1,
		/// <summary>Wrap in the positive direction during the transition to or from edge along the first axis.</summary>
		EdgePosAxis0 = PosVertToEdgeAxis0 | PosFaceToEdgeAxis0 | PosEdgeToVertAxis0 | PosEdgeToFaceAxis0,
		/// <summary>Wrap in the positive direction during the transition to or from edge along the second axis.</summary>
		EdgePosAxis1 = PosVertToEdgeAxis1 | PosFaceToEdgeAxis1 | PosEdgeToVertAxis1 | PosEdgeToFaceAxis1,
		/// <summary>Wrap in the positive direction during any transition along the first axis.</summary>
		PosAxis0 = NonEdgePosAxis0 | EdgePosAxis0,
		/// <summary>Wrap in the positive direction during any transition along the second axis.</summary>
		PosAxis1 = NonEdgePosAxis1 | EdgePosAxis1,
		/// <summary>Wrap in the positive direction during any transition along either axis.</summary>
		Pos = PosAxis0 | PosAxis1,

		/// <summary>Wrap in the negative direction during the transition from vertex or face to vertex or face along the first axis.</summary>
		NonEdgeNegAxis0 = NegVertToVertAxis0 | NegVertToFaceAxis0 | NegFaceToVertAxis0 | NegFaceToFaceAxis0,
		/// <summary>Wrap in the negative direction during the transition from vertex or face to vertex or face along the second axis.</summary>
		NonEdgeNegAxis1 = NegVertToVertAxis1 | NegVertToFaceAxis1 | NegFaceToVertAxis1 | NegFaceToFaceAxis1,
		/// <summary>Wrap in the negative direction during the transition to or from edge along the first axis.</summary>
		EdgeNegAxis0 = NegVertToEdgeAxis0 | NegFaceToEdgeAxis0 | NegEdgeToVertAxis0 | NegEdgeToFaceAxis0,
		/// <summary>Wrap in the negative direction during the transition to or from edge along the second axis.</summary>
		EdgeNegAxis1 = NegVertToEdgeAxis1 | NegFaceToEdgeAxis1 | NegEdgeToVertAxis1 | NegEdgeToFaceAxis1,
		/// <summary>Wrap in the negative direction during any transition along the first axis.</summary>
		NegAxis0 = NonEdgeNegAxis0 | EdgeNegAxis0,
		/// <summary>Wrap in the negative direction during any transition along the second axis.</summary>
		NegAxis1 = NonEdgeNegAxis1 | EdgeNegAxis1,
		/// <summary>Wrap in the negative direction during any transition along either axis.</summary>
		Neg = NegAxis0 | NegAxis1,

		/// <summary>Wrap during any transition along the first axis.</summary>
		Axis0 = PosAxis0 | NegAxis0,
		/// <summary>Wrap during any transition along the second axis.</summary>
		Axis1 = PosAxis1 | NegAxis1,

		/// <summary>Wrap during any transition along either axis.</summary>
		All = Pos | Neg,
	}
}
