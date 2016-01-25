using System;

namespace Experilous.Topological
{
	public static class EdgeWrapUtility
	{
		public static EdgeWrap CancelPosNeg(EdgeWrap edgeWrap)
		{
			// Take all the negative axis bits, shift them right to align with positive
			// axis bits, and then bitwise-and the negative and positive axis bits together.
			// The result are bits set only where both the negative and positive directions
			// are active for a given axis.
			var mask = ((int)(edgeWrap & EdgeWrap.Neg) >> 1) & (int)edgeWrap;

			// Duplicate the set bits back over into the negative axis bits, and then negate
			// the whole thing, causing negative/positive bit pairs to be 00 whenever both
			// bits were set originally, and 11 otherwise.
			mask = ~(mask | (mask << 1));

			// Use this mask to zero out any axes that had both positive and negative
			// directions set.
			return edgeWrap & (EdgeWrap)mask;
		}

		private static EdgeWrap ShiftVertToVertAsVertToFace(EdgeWrap vertToVert)
		{
			return (EdgeWrap)((int)vertToVert << 4);
		}

		private static EdgeWrap ShiftVertToVertAsFaceToVert(EdgeWrap vertToVert)
		{
			return (EdgeWrap)((int)vertToVert << 8);
		}

		private static EdgeWrap ShiftFaceToFaceAsVertToFace(EdgeWrap faceToFace)
		{
			return (EdgeWrap)((int)faceToFace >> 8);
		}

		private static EdgeWrap ShiftFaceToFaceAsFaceToVert(EdgeWrap faceToFace)
		{
			return (EdgeWrap)((int)faceToFace >> 4);
		}

		public static EdgeWrap ChainVertToVertToVert(EdgeWrap vert0ToVert1, EdgeWrap vert1ToVert2)
		{
			// Combine the two edge wraps together, masking to keep only the vert-to-vert bits,
			// and then cancel out any positive and negative directions on the same axis.
			return CancelPosNeg((vert0ToVert1 | vert1ToVert2) & EdgeWrap.VertToVert);
		}

		public static EdgeWrap ChainVertToVertToFace(EdgeWrap vert0ToVert1, EdgeWrap vert1ToFace)
		{
			// Move the vert-to-vert data over into the vert-to-face region, and then combine
			// the two together, masking to keep only the vert-to-face bits, and then cancel
			// out any positive and negative directions on the same axis.
			return CancelPosNeg((ShiftVertToVertAsVertToFace(vert0ToVert1) | vert1ToFace) & EdgeWrap.VertToFace);
		}

		public static EdgeWrap ChainVertToFaceToFace(EdgeWrap vertToFace0, EdgeWrap face0ToFace1)
		{
			// Move the face-to-face data over into the vert-to-face region, and then combine
			// the two together, masking to keep only the vert-to-face bits, and then cancel
			// out any positive and negative directions on the same axis.
			return CancelPosNeg((vertToFace0 | ShiftFaceToFaceAsVertToFace(face0ToFace1)) & EdgeWrap.VertToFace);
		}

		public static EdgeWrap ChainFaceToVertToVert(EdgeWrap faceToVert0, EdgeWrap vert0ToVert1)
		{
			// Move the vert-to-vert data over into the face-to-vert region, and then combine
			// the two together, masking to keep only the face-to-vert bits, and then cancel
			// out any positive and negative directions on the same axis.
			return CancelPosNeg((faceToVert0 | ShiftVertToVertAsFaceToVert(vert0ToVert1)) & EdgeWrap.FaceToVert);
		}

		public static EdgeWrap ChainFaceToFaceToVert(EdgeWrap face0ToFace1, EdgeWrap face1ToVert)
		{
			// Move the face-to-face data over into the face-to-vert region, and then combine
			// the two together, masking to keep only the face-to-vert bits, and then cancel
			// out any positive and negative directions on the same axis.
			return CancelPosNeg((ShiftFaceToFaceAsFaceToVert(face0ToFace1) | face1ToVert) & EdgeWrap.FaceToVert);
		}

		public static EdgeWrap ChainFaceToFaceToFace(EdgeWrap face0ToFace1, EdgeWrap face1ToFace2)
		{
			// Combine the two edge wraps together, masking to keep only the face-to-face bits,
			// and then cancel out any positive and negative directions on the same axis.
			return CancelPosNeg((face0ToFace1 | face1ToFace2) & EdgeWrap.FaceToFace);
		}
	}
}
