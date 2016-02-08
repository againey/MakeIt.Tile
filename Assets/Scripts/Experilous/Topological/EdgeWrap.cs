using System;

namespace Experilous.Topological
{
	[Serializable] [Flags]
	public enum EdgeWrap : int
	{
		None = 0x00,

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
