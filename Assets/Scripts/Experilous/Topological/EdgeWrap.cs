using System;

namespace Experilous.Topological
{
	[Serializable] [Flags]
	public enum EdgeWrap : int //should be ushort, but Unity doesn't like to serialize it
	{
		None = 0x00,
		PosVertToVertAxis0 = 0x0001,
		NegVertToVertAxis0 = 0x0002,
		PosVertToVertAxis1 = 0x0004,
		NegVertToVertAxis1 = 0x0008,
		PosVertToFaceAxis0 = 0x0010,
		NegVertToFaceAxis0 = 0x0020,
		PosVertToFaceAxis1 = 0x0040,
		NegVertToFaceAxis1 = 0x0080,
		PosFaceToVertAxis0 = 0x0100,
		NegFaceToVertAxis0 = 0x0200,
		PosFaceToVertAxis1 = 0x0400,
		NegFaceToVertAxis1 = 0x0800,
		PosFaceToFaceAxis0 = 0x1000,
		NegFaceToFaceAxis0 = 0x2000,
		PosFaceToFaceAxis1 = 0x4000,
		NegFaceToFaceAxis1 = 0x8000, //~0x7FFF, //0x8000 is not a valid short, can't use ushort because Unity

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

		VertTo = VertToVert | VertToFace,
		FaceTo = FaceToVert | FaceToFace,

		ToVert = VertToVert | FaceToVert,
		ToFace = VertToFace | FaceToFace,

		PosAxis0 = PosVertToVertAxis0 | PosVertToFaceAxis0 | PosFaceToVertAxis0 | PosFaceToFaceAxis0,
		PosAxis1 = PosVertToVertAxis1 | PosVertToFaceAxis1 | PosFaceToVertAxis1 | PosFaceToFaceAxis1,
		Pos = PosAxis0 | PosAxis1,

		NegAxis0 = NegVertToVertAxis0 | NegVertToFaceAxis0 | NegFaceToVertAxis0 | NegFaceToFaceAxis0,
		NegAxis1 = NegVertToVertAxis1 | NegVertToFaceAxis1 | NegFaceToVertAxis1 | NegFaceToFaceAxis1,
		Neg = NegAxis0 | NegAxis1,

		All = VertToVert | VertToFace | FaceToVert | FaceToFace,
	}
}
