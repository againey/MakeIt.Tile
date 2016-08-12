/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

namespace Experilous.MakeIt.Tile
{
	public static class EdgeWrapUtility
	{
		public enum Generic
		{
			None = 0x00000000,
			PosAxis0 = 0x00000001,
			NegAxis0 = 0x00000002,
			PosAxis1 = 0x00000004,
			NegAxis1 = 0x00000008,
		}

		public static EdgeWrap FromEdgeRelations(EdgeWrap edgeWrap)
		{
			// Binary layout:
			//    28    24    20    16    12     8     4     0
			// [F->F][F->V][V->F][V->V][E->F][E->V][F->E][V->E]
			//
			// [F->E][F->E][V->E][V->E]    (toEdge)
			//        chained to
			// [E->F][E->V][E->F][E->V]    (edgeTo)
			//         produces
			// [F->F][F->V][V->F][V->V]    (non-edge relations, before contary direction elimination)

			var toEdge = ((uint)(edgeWrap & EdgeWrap.FaceToEdge) << 8) | ((uint)(edgeWrap & EdgeWrap.ToEdge) << 4) | (uint)(edgeWrap & EdgeWrap.VertToEdge);
			var edgeTo = (uint)(edgeWrap & EdgeWrap.EdgeTo) | ((uint)(edgeWrap & EdgeWrap.EdgeTo) >> 8);
			return EliminateContraryDirections((EdgeWrap)((toEdge | edgeTo) << 16) | (edgeWrap & EdgeWrap.Edge));
		}

		public static EdgeWrap EliminateContraryDirections(EdgeWrap edgeWrap)
		{
			// Take all the negative axis bits, shift them right to align with positive
			// axis bits, and then bitwise-and the negative and positive axis bits together.
			// The result are bits set only where both the negative and positive directions
			// are active for a given axis.
			var mask = ((uint)(edgeWrap & EdgeWrap.Neg) >> 1) & (uint)edgeWrap;

			// Duplicate the set bits back over into the negative axis bits, and then negate
			// the whole thing, causing negative/positive bit pairs to be 00 whenever both
			// bits were set originally, and 11 otherwise.
			mask = ~(mask | (mask << 1));

			// Use this mask to zero out any axes that had both positive and negative
			// directions set.
			return edgeWrap & (EdgeWrap)mask;
		}

		public static EdgeWrap InvertEdgeRelations(EdgeWrap edgeWrap)
		{
			// Invert the positive and negative directions.
			var negAsPos = (uint)(edgeWrap & EdgeWrap.Neg) >> 1;
			var posAsNeg = (uint)(edgeWrap & EdgeWrap.Pos) << 1;
			var invertedDirections = (EdgeWrap)(negAsPos | posAsNeg);

			// Swap the edge-to and to-edge relations.
			return
				ShiftToEdgeAsEdgeTo(invertedDirections) & EdgeWrap.EdgeTo |
				ShiftEdgeToAsToEdge(invertedDirections) & EdgeWrap.ToEdge;
		}

		public static EdgeWrap InvertVertexEdgeRelations(EdgeWrap edgeWrap)
		{
			return InvertEdgeRelations(edgeWrap) & (EdgeWrap.VertToEdge | EdgeWrap.EdgeToVert) |
				edgeWrap & (EdgeWrap.FaceToEdge | EdgeWrap.EdgeToFace);
		}

		public static EdgeWrap InvertFaceEdgeRelations(EdgeWrap edgeWrap)
		{
			return InvertEdgeRelations(edgeWrap) & (EdgeWrap.FaceToEdge | EdgeWrap.EdgeToFace) |
				edgeWrap & (EdgeWrap.VertToEdge | EdgeWrap.EdgeToVert);
		}

		public static EdgeWrap Invert(EdgeWrap edgeWrap)
		{
			return FromEdgeRelations(InvertEdgeRelations(edgeWrap));
		}

		public static void CrossMergeTwins(ref EdgeWrap halfEdge0, ref EdgeWrap halfEdge1)
		{
			var invertedHalfEdge0 = InvertEdgeRelations(halfEdge0);
			var invertedHalfEdge1 = InvertEdgeRelations(halfEdge1);
			halfEdge0 = FromEdgeRelations(halfEdge0 | invertedHalfEdge1);
			halfEdge1 = FromEdgeRelations(invertedHalfEdge0 | halfEdge1);
		}

		public static EdgeWrap SwapAxes(EdgeWrap edgeWrap)
		{
			return
				ShiftAxis0AsAxis1(edgeWrap) & EdgeWrap.Axis1 |
				ShiftAxis1AsAxis0(edgeWrap) & EdgeWrap.Axis0;
		}

		public static Generic VertToVertAsGeneric(EdgeWrap edgeWrap)
		{
			return (Generic)((uint)(edgeWrap & EdgeWrap.VertToVert) >> 16);
		}

		public static Generic VertToEdgeAsGeneric(EdgeWrap edgeWrap)
		{
			return (Generic)(edgeWrap & EdgeWrap.VertToEdge);
		}

		public static Generic VertToFaceAsGeneric(EdgeWrap edgeWrap)
		{
			return (Generic)((uint)(edgeWrap & EdgeWrap.VertToFace) >> 20);
		}

		public static Generic EdgeToVertAsGeneric(EdgeWrap edgeWrap)
		{
			return (Generic)((uint)(edgeWrap & EdgeWrap.EdgeToVert) >> 8);
		}

		public static Generic EdgeToFaceAsGeneric(EdgeWrap edgeWrap)
		{
			return (Generic)((uint)(edgeWrap & EdgeWrap.EdgeToFace) >> 12);
		}

		public static Generic FaceToVertAsGeneric(EdgeWrap edgeWrap)
		{
			return (Generic)((uint)(edgeWrap & EdgeWrap.FaceToVert) >> 24);
		}

		public static Generic FaceToEdgeAsGeneric(EdgeWrap edgeWrap)
		{
			return (Generic)((uint)(edgeWrap & EdgeWrap.FaceToEdge) >> 4);
		}

		public static Generic FaceToFaceAsGeneric(EdgeWrap edgeWrap)
		{
			return (Generic)((uint)(edgeWrap & EdgeWrap.FaceToFace) >> 28);
		}

		public static EdgeWrap ShiftToEdgeAsEdgeTo(EdgeWrap toEdge)
		{
			return (EdgeWrap)((uint)toEdge << 8);
		}

		public static EdgeWrap ShiftEdgeToAsToEdge(EdgeWrap edgeTo)
		{
			return (EdgeWrap)((uint)edgeTo >> 8);
		}

		private static EdgeWrap ShiftVertToVertAsVertToEdge(EdgeWrap vertToVert)
		{
			return (EdgeWrap)((uint)vertToVert >> 16);
		}

		private static EdgeWrap ShiftFaceToFaceAsFaceToEdge(EdgeWrap faceToFace)
		{
			return (EdgeWrap)((uint)faceToFace >> 24);
		}

		private static EdgeWrap ShiftVertToVertAsEdgeToVert(EdgeWrap vertToVert)
		{
			return (EdgeWrap)((uint)vertToVert >> 8);
		}

		private static EdgeWrap ShiftFaceToFaceAsEdgeToFace(EdgeWrap faceToFace)
		{
			return (EdgeWrap)((uint)faceToFace >> 16);
		}

		private static EdgeWrap ShiftVertToVertAsVertToFace(EdgeWrap vertToVert)
		{
			return (EdgeWrap)((uint)vertToVert << 4);
		}

		private static EdgeWrap ShiftVertToVertAsFaceToVert(EdgeWrap vertToVert)
		{
			return (EdgeWrap)((uint)vertToVert << 8);
		}

		private static EdgeWrap ShiftFaceToFaceAsVertToFace(EdgeWrap faceToFace)
		{
			return (EdgeWrap)((uint)faceToFace >> 8);
		}

		private static EdgeWrap ShiftFaceToFaceAsFaceToVert(EdgeWrap faceToFace)
		{
			return (EdgeWrap)((uint)faceToFace >> 4);
		}

		private static EdgeWrap ShiftAxis0AsAxis1(EdgeWrap axis0)
		{
			return (EdgeWrap)((uint)axis0 << 2);
		}

		private static EdgeWrap ShiftAxis1AsAxis0(EdgeWrap axis1)
		{
			return (EdgeWrap)((uint)axis1 >> 2);
		}

		public static EdgeWrap ChainVertToVertToEdge(EdgeWrap vert0ToVert1, EdgeWrap vert1ToEdge)
		{
			return EliminateContraryDirections((ShiftVertToVertAsVertToEdge(vert0ToVert1) | vert1ToEdge) & EdgeWrap.VertToEdge);
		}

		public static EdgeWrap ChainEdgeToVertToVert(EdgeWrap edgeToVert0, EdgeWrap vert0ToVert1)
		{
			return EliminateContraryDirections((edgeToVert0 | ShiftVertToVertAsEdgeToVert(vert0ToVert1)) & EdgeWrap.EdgeToVert);
		}

		public static EdgeWrap ChainFaceToFaceToEdge(EdgeWrap face0ToFace1, EdgeWrap face1ToEdge)
		{
			return EliminateContraryDirections((ShiftFaceToFaceAsFaceToEdge(face0ToFace1) | face1ToEdge) & EdgeWrap.FaceToEdge);
		}

		public static EdgeWrap ChainEdgeToFaceToFace(EdgeWrap edgeToFace0, EdgeWrap face0ToFace1)
		{
			return EliminateContraryDirections((edgeToFace0 | ShiftFaceToFaceAsEdgeToFace(face0ToFace1)) & EdgeWrap.EdgeToFace);
		}

		public static EdgeWrap ChainVertEdgeRelations(EdgeWrap edge0, EdgeWrap edge1)
		{
			return EliminateContraryDirections(
				(ShiftVertToVertAsVertToEdge(edge0) | edge1) & EdgeWrap.VertToEdge |
				(edge0 | ShiftVertToVertAsEdgeToVert(edge1)) & EdgeWrap.EdgeToVert);
		}

		public static EdgeWrap ChainFaceEdgeRelations(EdgeWrap edge0, EdgeWrap edge1)
		{
			return EliminateContraryDirections(
				(ShiftFaceToFaceAsFaceToEdge(edge0) | edge1) & EdgeWrap.FaceToEdge |
				(edge0 | ShiftFaceToFaceAsEdgeToFace(edge1)) & EdgeWrap.EdgeToFace);
		}

		public static EdgeWrap ModifyTargetVertEdgeRelations(EdgeWrap original, EdgeWrap adjustment)
		{
			return FromEdgeRelations(
				(original | ShiftVertToVertAsEdgeToVert(adjustment)) & EdgeWrap.EdgeToVert |
				original & ~EdgeWrap.EdgeToVert);
		}

		public static EdgeWrap ModifySourceVertEdgeRelations(EdgeWrap original, EdgeWrap adjustment)
		{
			return FromEdgeRelations(
				(ShiftVertToVertAsVertToEdge(adjustment) | original) & EdgeWrap.VertToEdge |
				original & ~EdgeWrap.VertToEdge);
		}

		public static EdgeWrap ModifyTargetFaceEdgeRelations(EdgeWrap original, EdgeWrap adjustment)
		{
			return FromEdgeRelations(
				(original | ShiftFaceToFaceAsEdgeToFace(adjustment)) & EdgeWrap.EdgeToFace |
				original & ~EdgeWrap.EdgeToFace);
		}

		public static EdgeWrap ModifySourceFaceEdgeRelations(EdgeWrap original, EdgeWrap adjustment)
		{
			return FromEdgeRelations(
				(ShiftFaceToFaceAsFaceToEdge(adjustment) | original) & EdgeWrap.FaceToEdge |
				original & ~EdgeWrap.FaceToEdge);
		}

		public static EdgeWrap ChainVertToVertToVert(EdgeWrap vert0ToVert1, EdgeWrap vert1ToVert2)
		{
			// Combine the two edge wraps together, masking to keep only the vert-to-vert bits,
			// and then cancel out any positive and negative directions on the same axis.
			return EliminateContraryDirections((vert0ToVert1 | vert1ToVert2) & EdgeWrap.VertToVert);
		}

		public static EdgeWrap ChainVertToVertToFace(EdgeWrap vert0ToVert1, EdgeWrap vert1ToFace)
		{
			// Move the vert-to-vert data over into the vert-to-face region, and then combine
			// the two together, masking to keep only the vert-to-face bits, and then cancel
			// out any positive and negative directions on the same axis.
			return EliminateContraryDirections((ShiftVertToVertAsVertToFace(vert0ToVert1) | vert1ToFace) & EdgeWrap.VertToFace);
		}

		public static EdgeWrap ChainVertToFaceToFace(EdgeWrap vertToFace0, EdgeWrap face0ToFace1)
		{
			// Move the face-to-face data over into the vert-to-face region, and then combine
			// the two together, masking to keep only the vert-to-face bits, and then cancel
			// out any positive and negative directions on the same axis.
			return EliminateContraryDirections((vertToFace0 | ShiftFaceToFaceAsVertToFace(face0ToFace1)) & EdgeWrap.VertToFace);
		}

		public static EdgeWrap ChainFaceToVertToVert(EdgeWrap faceToVert0, EdgeWrap vert0ToVert1)
		{
			// Move the vert-to-vert data over into the face-to-vert region, and then combine
			// the two together, masking to keep only the face-to-vert bits, and then cancel
			// out any positive and negative directions on the same axis.
			return EliminateContraryDirections((faceToVert0 | ShiftVertToVertAsFaceToVert(vert0ToVert1)) & EdgeWrap.FaceToVert);
		}

		public static EdgeWrap ChainFaceToFaceToVert(EdgeWrap face0ToFace1, EdgeWrap face1ToVert)
		{
			// Move the face-to-face data over into the face-to-vert region, and then combine
			// the two together, masking to keep only the face-to-vert bits, and then cancel
			// out any positive and negative directions on the same axis.
			return EliminateContraryDirections((ShiftFaceToFaceAsFaceToVert(face0ToFace1) | face1ToVert) & EdgeWrap.FaceToVert);
		}

		public static EdgeWrap ChainFaceToFaceToFace(EdgeWrap face0ToFace1, EdgeWrap face1ToFace2)
		{
			// Combine the two edge wraps together, masking to keep only the face-to-face bits,
			// and then cancel out any positive and negative directions on the same axis.
			return EliminateContraryDirections((face0ToFace1 | face1ToFace2) & EdgeWrap.FaceToFace);
		}
	}
}
