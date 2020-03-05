/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

namespace MakeIt.Tile
{
	/// <summary>
	/// Static utility class for working with edge wrap data.
	/// </summary>
	public static class EdgeWrapUtility
	{
		/// <summary>
		/// A simplification of the EdgeWrap enumeration focusing entirely on the axis and direction only.
		/// </summary>
		[System.Flags]
		public enum Generic
		{
			/// <summary>Default state of no wrapping relations of any kind.</summary>
			None = 0x00000000,
			/// <summary>Wrap in the positive direction during a transition along the first axis.</summary>
			PosAxis0 = 0x00000001,
			/// <summary>Wrap in the negative direction during a transition along the first axis.</summary>
			NegAxis0 = 0x00000002,
			/// <summary>Wrap in the positive direction during a transition along the secondaxis.</summary>
			PosAxis1 = 0x00000004,
			/// <summary>Wrap in the negative direction during a transition along the secondaxis.</summary>
			NegAxis1 = 0x00000008,
		}

		/// <summary>
		/// Derives the full edge wrap data from just the flags involving edges.
		/// </summary>
		/// <param name="edgeWrap">The partial edge wrap data which only specifies wrap behavior for transitions involving edges.</param>
		/// <returns>The full edge wrap data which specifies wrap behavior for all transition types.</returns>
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

		/// <summary>
		/// Eliminates any opposing wrap directions in the given edge wrap data, since a positive and negative wrap along the same axis essentially cancel each other out.
		/// </summary>
		/// <param name="edgeWrap">The edge wrap data to sanitize by eliminating opposing wrap directions.</param>
		/// <returns>A sanitized form of the provided edge wrap data without any opposing wrap directions.</returns>
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

		/// <summary>
		/// Inverts the edge relations of the edge wrap data, swapping positive and negative directions, and swapping edge-to-something and something-to-edge relations.
		/// </summary>
		/// <param name="edgeWrap">The edge wrap data to invert.</param>
		/// <returns>The inverted edge wrap data, with all non-edge relations cleared.</returns>
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

		/// <summary>
		/// Inverts the vertex/edge relations of the edge wrap data, swapping positive and negative directions, and swapping edge-to-vertex and vertex-to-edge relations.  Face/edge relations remain unchanged.
		/// </summary>
		/// <param name="edgeWrap">The edge wrap data to invert.</param>
		/// <returns>The inverted edge wrap data, with all non-edge relations cleared, and face/edge relations unmodified.</returns>
		public static EdgeWrap InvertVertexEdgeRelations(EdgeWrap edgeWrap)
		{
			return InvertEdgeRelations(edgeWrap) & EdgeWrap.VertToEdgeToVert |
				edgeWrap & EdgeWrap.FaceToEdgeToFace;
		}

		/// <summary>
		/// Inverts the face/edge relations of the edge wrap data, swapping positive and negative directions, and swapping edge-to-face and face-to-edge relations.  Vertex/edge relations remain unchanged.
		/// </summary>
		/// <param name="edgeWrap">The edge wrap data to invert.</param>
		/// <returns>The inverted edge wrap data, with all non-edge relations cleared, and vertex/edge relations unmodified.</returns>
		public static EdgeWrap InvertFaceEdgeRelations(EdgeWrap edgeWrap)
		{
			return InvertEdgeRelations(edgeWrap) & EdgeWrap.FaceToEdgeToFace |
				edgeWrap & EdgeWrap.VertToEdgeToVert;
		}

		/// <summary>
		/// Inverts all relations of the edge wrap data, swapping positive and negative directions, and swapping A-to-B and B-to-A relations.
		/// </summary>
		/// <param name="edgeWrap">The edge wrap data to invert.</param>
		/// <returns>The inverted edge wrap data.</returns>
		/// <seealso cref="InvertEdgeRelations(EdgeWrap)"/>
		/// <seealso cref="FromEdgeRelations(EdgeWrap)"/>
		public static EdgeWrap Invert(EdgeWrap edgeWrap)
		{
			return FromEdgeRelations(InvertEdgeRelations(edgeWrap));
		}

		/// <summary>
		/// Merges the edge wrap data of two partially specified twin half edges so that the edge wrap data for each half edge is fully specified.
		/// </summary>
		/// <param name="halfEdge0">The edge wrap data of the first half edge twin to merge.</param>
		/// <param name="halfEdge1">The edge wrap data of the second half edge twin to merge.</param>
		public static void CrossMergeTwins(ref EdgeWrap halfEdge0, ref EdgeWrap halfEdge1)
		{
			var invertedHalfEdge0 = InvertEdgeRelations(halfEdge0);
			var invertedHalfEdge1 = InvertEdgeRelations(halfEdge1);
			halfEdge0 = FromEdgeRelations(halfEdge0 | invertedHalfEdge1);
			halfEdge1 = FromEdgeRelations(invertedHalfEdge0 | halfEdge1);
		}

		/// <summary>
		/// Swaps the first and second axis relations in the given edge wrap data.
		/// </summary>
		/// <param name="edgeWrap">The edge wrap data whose axis relations are to be swapped.</param>
		/// <returns>The modified edge wrap data with the axis relations swapped.</returns>
		public static EdgeWrap SwapAxes(EdgeWrap edgeWrap)
		{
			return
				ShiftAxis0AsAxis1(edgeWrap) & EdgeWrap.Axis1 |
				ShiftAxis1AsAxis0(edgeWrap) & EdgeWrap.Axis0;
		}

		/// <summary>
		/// Determines if the given edge wrap data includes any wrapping along axis 0, whether positive or negative.
		/// </summary>
		/// <param name="edgeWrap">The edge wrap data to examine.</param>
		/// <returns>True if the edge wrap data includes any wrapping along axis 0 in either direction, and false if it does not.</returns>
		public static bool WrapsOnAxis0(EdgeWrap edgeWrap)
		{
			return (edgeWrap & EdgeWrap.Axis0) != EdgeWrap.None;
		}

		/// <summary>
		/// Determines if the given edge wrap data includes any wrapping along axis 1, whether positive or negative.
		/// </summary>
		/// <param name="edgeWrap">The edge wrap data to examine.</param>
		/// <returns>True if the edge wrap data includes any wrapping along axis 1 in either direction, and false if it does not.</returns>
		public static bool WrapsOnAxis1(EdgeWrap edgeWrap)
		{
			return (edgeWrap & EdgeWrap.Axis1) != EdgeWrap.None;
		}

		/// <summary>
		/// Determines if the given edge wrap data includes any wrapping along axis 0, whether positive or negative, and no wrapping at all on axis 1.
		/// </summary>
		/// <param name="edgeWrap">The edge wrap data to examine.</param>
		/// <returns>True if the edge wrap data includes any wrapping along axis 0 in either direction and none on axis 1, and false otherwise.</returns>
		public static bool WrapsOnOnlyAxis0(EdgeWrap edgeWrap)
		{
			return (edgeWrap & EdgeWrap.Axis0) != EdgeWrap.None && (edgeWrap & EdgeWrap.Axis1) == EdgeWrap.None;
		}

		/// <summary>
		/// Determines if the given edge wrap data includes any wrapping along axis 1, whether positive or negative, and no wrapping at all on axis 0.
		/// </summary>
		/// <param name="edgeWrap">The edge wrap data to examine.</param>
		/// <returns>True if the edge wrap data includes any wrapping along axis 1 in either direction and none on axis 0, and false otherwise.</returns>
		public static bool WrapsOnOnlyAxis1(EdgeWrap edgeWrap)
		{
			return (edgeWrap & EdgeWrap.Axis1) != EdgeWrap.None && (edgeWrap & EdgeWrap.Axis0) == EdgeWrap.None;
		}

		/// <summary>
		/// Determines if the given edge wrap data includes any wrapping along both axis 0 and axis 1, whether positive or negative.
		/// </summary>
		/// <param name="edgeWrap">The edge wrap data to examine.</param>
		/// <returns>True if the edge wrap data includes any wrapping along both axes in either direction, and false if it does not.</returns>
		public static bool WrapsOnBothAxes(EdgeWrap edgeWrap)
		{
			return (edgeWrap & EdgeWrap.Axis1) != EdgeWrap.None && (edgeWrap & EdgeWrap.Axis0) != EdgeWrap.None;
		}

		/// <summary>
		/// Determines if the given edge wrap data includes no wrapping along either axis 0 and axis 1.
		/// </summary>
		/// <param name="edgeWrap">The edge wrap data to examine.</param>
		/// <returns>True if the edge wrap data includes no wrapping along either axis in either direction, and false if it does.</returns>
		public static bool WrapsOnNeitherAxis(EdgeWrap edgeWrap)
		{
			return edgeWrap == EdgeWrap.None;
		}

		/// <summary>
		/// Extracts the direction and axis data for the vertex-to-vertex relations from the edge wrap data and converts it to the reduced <see cref="Generic"/> enumeration.
		/// </summary>
		/// <param name="edgeWrap">The edge wrap data to reduce.</param>
		/// <returns>The reduced direction and axis data for just the vertex-to-vertex relations of the given edge wrap data.</returns>
		public static Generic VertToVertAsGeneric(EdgeWrap edgeWrap)
		{
			return (Generic)((uint)(edgeWrap & EdgeWrap.VertToVert) >> 16);
		}

		/// <summary>
		/// Extracts the direction and axis data for the vertex-to-edge relations from the edge wrap data and converts it to the reduced <see cref="Generic"/> enumeration.
		/// </summary>
		/// <param name="edgeWrap">The edge wrap data to reduce.</param>
		/// <returns>The reduced direction and axis data for just the vertex-to-edge relations of the given edge wrap data.</returns>
		public static Generic VertToEdgeAsGeneric(EdgeWrap edgeWrap)
		{
			return (Generic)(edgeWrap & EdgeWrap.VertToEdge);
		}

		/// <summary>
		/// Extracts the direction and axis data for the vertex-to-face relations from the edge wrap data and converts it to the reduced <see cref="Generic"/> enumeration.
		/// </summary>
		/// <param name="edgeWrap">The edge wrap data to reduce.</param>
		/// <returns>The reduced direction and axis data for just the vertex-to-face relations of the given edge wrap data.</returns>
		public static Generic VertToFaceAsGeneric(EdgeWrap edgeWrap)
		{
			return (Generic)((uint)(edgeWrap & EdgeWrap.VertToFace) >> 20);
		}

		/// <summary>
		/// Extracts the direction and axis data for the edge-to-vertex relations from the edge wrap data and converts it to the reduced <see cref="Generic"/> enumeration.
		/// </summary>
		/// <param name="edgeWrap">The edge wrap data to reduce.</param>
		/// <returns>The reduced direction and axis data for just the edge-to-vertex relations of the given edge wrap data.</returns>
		public static Generic EdgeToVertAsGeneric(EdgeWrap edgeWrap)
		{
			return (Generic)((uint)(edgeWrap & EdgeWrap.EdgeToVert) >> 8);
		}

		/// <summary>
		/// Extracts the direction and axis data for the edge-to-face relations from the edge wrap data and converts it to the reduced <see cref="Generic"/> enumeration.
		/// </summary>
		/// <param name="edgeWrap">The edge wrap data to reduce.</param>
		/// <returns>The reduced direction and axis data for just the edge-to-face relations of the given edge wrap data.</returns>
		public static Generic EdgeToFaceAsGeneric(EdgeWrap edgeWrap)
		{
			return (Generic)((uint)(edgeWrap & EdgeWrap.EdgeToFace) >> 12);
		}

		/// <summary>
		/// Extracts the direction and axis data for the face-to-vertex relations from the edge wrap data and converts it to the reduced <see cref="Generic"/> enumeration.
		/// </summary>
		/// <param name="edgeWrap">The edge wrap data to reduce.</param>
		/// <returns>The reduced direction and axis data for just the face-to-vertex relations of the given edge wrap data.</returns>
		public static Generic FaceToVertAsGeneric(EdgeWrap edgeWrap)
		{
			return (Generic)((uint)(edgeWrap & EdgeWrap.FaceToVert) >> 24);
		}

		/// <summary>
		/// Extracts the direction and axis data for the face-to-edge relations from the edge wrap data and converts it to the reduced <see cref="Generic"/> enumeration.
		/// </summary>
		/// <param name="edgeWrap">The edge wrap data to reduce.</param>
		/// <returns>The reduced direction and axis data for just the face-to-edge relations of the given edge wrap data.</returns>
		public static Generic FaceToEdgeAsGeneric(EdgeWrap edgeWrap)
		{
			return (Generic)((uint)(edgeWrap & EdgeWrap.FaceToEdge) >> 4);
		}

		/// <summary>
		/// Extracts the direction and axis data for the face-to-face relations from the edge wrap data and converts it to the reduced <see cref="Generic"/> enumeration.
		/// </summary>
		/// <param name="edgeWrap">The edge wrap data to reduce.</param>
		/// <returns>The reduced direction and axis data for just the face-to-face relations of the given edge wrap data.</returns>
		public static Generic FaceToFaceAsGeneric(EdgeWrap edgeWrap)
		{
			return (Generic)((uint)(edgeWrap & EdgeWrap.FaceToFace) >> 28);
		}

		/// <summary>
		/// Shifts the something-to-edge relation bits of the given edge wrap data to become edge-to-something relations.
		/// </summary>
		/// <param name="toEdge">The edge wrap data to shift.</param>
		/// <returns>The edge wrap data shifted so that the something-to-edge relations are now edge-to-something relations.</returns>
		/// <remarks><note type="important">All bits other than the edge-to-something bits should be considered garbage,
		/// and should therefore be masked away or otherwise corrected before they are inadvertently used.</note></remarks>
		private static EdgeWrap ShiftToEdgeAsEdgeTo(EdgeWrap toEdge)
		{
			return (EdgeWrap)((uint)toEdge << 8);
		}

		/// <summary>
		/// Shifts the edge-to-something relation bits of the given edge wrap data to become something-to-edge relations.
		/// </summary>
		/// <param name="edgeTo">The edge wrap data to shift.</param>
		/// <returns>The edge wrap data shifted so that the edge-to-something relations are now something-to-edge relations.</returns>
		/// <remarks><note type="important">All bits other than the something-to-edge bits should be considered garbage,
		/// and should therefore be masked away or otherwise corrected before they are inadvertently used.</note></remarks>
		private static EdgeWrap ShiftEdgeToAsToEdge(EdgeWrap edgeTo)
		{
			return (EdgeWrap)((uint)edgeTo >> 8);
		}

		/// <summary>
		/// Shifts the vertex-to-vertex relation bits of the given edge wrap data to become vertex-to-edge relations.
		/// </summary>
		/// <param name="vertToVert">The edge wrap data to shift.</param>
		/// <returns>The edge wrap data shifted so that the vertex-to-vertex relations are now vertex-to-edge relations.</returns>
		/// <remarks><note type="important">All bits other than the vertex-to-edge bits should be considered garbage,
		/// and should therefore be masked away or otherwise corrected before they are inadvertently used.</note></remarks>
		private static EdgeWrap ShiftVertToVertAsVertToEdge(EdgeWrap vertToVert)
		{
			return (EdgeWrap)((uint)vertToVert >> 16);
		}

		/// <summary>
		/// Shifts the face-to-face relation bits of the given edge wrap data to become face-to-edge relations.
		/// </summary>
		/// <param name="faceToFace">The edge wrap data to shift.</param>
		/// <returns>The edge wrap data shifted so that the face-to-face relations are now face-to-edge relations.</returns>
		/// <remarks><note type="important">All bits other than the edge-to-edge bits should be considered garbage,
		/// and should therefore be masked away or otherwise corrected before they are inadvertently used.</note></remarks>
		private static EdgeWrap ShiftFaceToFaceAsFaceToEdge(EdgeWrap faceToFace)
		{
			return (EdgeWrap)((uint)faceToFace >> 24);
		}

		/// <summary>
		/// Shifts the vertex-to-vertex relation bits of the given edge wrap data to become edge-to-vertex relations.
		/// </summary>
		/// <param name="vertToVert">The edge wrap data to shift.</param>
		/// <returns>The edge wrap data shifted so that the vertex-to-vertex relations are now edge-to-vertex relations.</returns>
		/// <remarks><note type="important">All bits other than the edge-to-vertex bits should be considered garbage,
		/// and should therefore be masked away or otherwise corrected before they are inadvertently used.</note></remarks>
		private static EdgeWrap ShiftVertToVertAsEdgeToVert(EdgeWrap vertToVert)
		{
			return (EdgeWrap)((uint)vertToVert >> 8);
		}

		/// <summary>
		/// Shifts the face-to-face relation bits of the given edge wrap data to become edge-to-face relations.
		/// </summary>
		/// <param name="faceToFace">The edge wrap data to shift.</param>
		/// <returns>The edge wrap data shifted so that the face-to-face relations are now edge-to-face relations.</returns>
		/// <remarks><note type="important">All bits other than the edge-to-face bits should be considered garbage,
		/// and should therefore be masked away or otherwise corrected before they are inadvertently used.</note></remarks>
		private static EdgeWrap ShiftFaceToFaceAsEdgeToFace(EdgeWrap faceToFace)
		{
			return (EdgeWrap)((uint)faceToFace >> 16);
		}

		/// <summary>
		/// Shifts the vertex-to-vertex relation bits of the given edge wrap data to become vertex-to-face relations.
		/// </summary>
		/// <param name="vertToVert">The edge wrap data to shift.</param>
		/// <returns>The edge wrap data shifted so that the vertex-to-vertex relations are now vertex-to-face relations.</returns>
		/// <remarks><note type="important">All bits other than the vertex-to-face bits should be considered garbage,
		/// and should therefore be masked away or otherwise corrected before they are inadvertently used.</note></remarks>
		private static EdgeWrap ShiftVertToVertAsVertToFace(EdgeWrap vertToVert)
		{
			return (EdgeWrap)((uint)vertToVert << 4);
		}

		/// <summary>
		/// Shifts the vertex-to-vertex relation bits of the given edge wrap data to become face-to-vertex relations.
		/// </summary>
		/// <param name="vertToVert">The edge wrap data to shift.</param>
		/// <returns>The edge wrap data shifted so that the vertex-to-vertex relations are now face-to-vertex relations.</returns>
		/// <remarks><note type="important">All bits other than the face-to-vertex bits should be considered garbage,
		/// and should therefore be masked away or otherwise corrected before they are inadvertently used.</note></remarks>
		private static EdgeWrap ShiftVertToVertAsFaceToVert(EdgeWrap vertToVert)
		{
			return (EdgeWrap)((uint)vertToVert << 8);
		}

		/// <summary>
		/// Shifts the face-to-face relation bits of the given edge wrap data to become vertex-to-face relations.
		/// </summary>
		/// <param name="faceToFace">The edge wrap data to shift.</param>
		/// <returns>The edge wrap data shifted so that the face-to-face relations are now vertex-to-face relations.</returns>
		/// <remarks><note type="important">All bits other than the vertex-to-face bits should be considered garbage,
		/// and should therefore be masked away or otherwise corrected before they are inadvertently used.</note></remarks>
		private static EdgeWrap ShiftFaceToFaceAsVertToFace(EdgeWrap faceToFace)
		{
			return (EdgeWrap)((uint)faceToFace >> 8);
		}

		/// <summary>
		/// Shifts the face-to-face relation bits of the given edge wrap data to become face-to-vertex relations.
		/// </summary>
		/// <param name="faceToFace">The edge wrap data to shift.</param>
		/// <returns>The edge wrap data shifted so that the face-to-face relations are now face-to-vertex relations.</returns>
		/// <remarks><note type="important">All bits other than the face-to-vertex bits should be considered garbage,
		/// and should therefore be masked away or otherwise corrected before they are inadvertently used.</note></remarks>
		private static EdgeWrap ShiftFaceToFaceAsFaceToVert(EdgeWrap faceToFace)
		{
			return (EdgeWrap)((uint)faceToFace >> 4);
		}

		/// <summary>
		/// Shifts the first axis relation bits of the given edge wrap data to become second axis relations.
		/// </summary>
		/// <param name="axis0">The edge wrap data to shift.</param>
		/// <returns>The edge wrap data shifted so that the first axis relations are now second axis relations.</returns>
		/// <remarks><note type="important">All bits other than the second axis bits should be considered garbage,
		/// and should therefore be masked away or otherwise corrected before they are inadvertently used.</note></remarks>
		private static EdgeWrap ShiftAxis0AsAxis1(EdgeWrap axis0)
		{
			return (EdgeWrap)((uint)axis0 << 2);
		}

		/// <summary>
		/// Shifts the second axis relation bits of the given edge wrap data to become first axis relations.
		/// </summary>
		/// <param name="axis1">The edge wrap data to shift.</param>
		/// <returns>The edge wrap data shifted so that the second axis relations are now first axis relations.</returns>
		/// <remarks><note type="important">All bits other than the first axis bits should be considered garbage,
		/// and should therefore be masked away or otherwise corrected before they are inadvertently used.</note></remarks>
		private static EdgeWrap ShiftAxis1AsAxis0(EdgeWrap axis1)
		{
			return (EdgeWrap)((uint)axis1 >> 2);
		}

		/// <summary>
		/// Merges the edge wrap behavior of two transitions from vertex to vertex to edge.
		/// </summary>
		/// <param name="vert0ToVert1">The edge wrap data of the first transition.</param>
		/// <param name="vert1ToEdge">The edge wrap data of the second transition</param>
		/// <returns>The combined edge wrap behavior of chaining the two given transitions.</returns>
		public static EdgeWrap ChainVertToVertToEdge(EdgeWrap vert0ToVert1, EdgeWrap vert1ToEdge)
		{
			return EliminateContraryDirections((ShiftVertToVertAsVertToEdge(vert0ToVert1) | vert1ToEdge) & EdgeWrap.VertToEdge);
		}

		/// <summary>
		/// Merges the edge wrap behavior of two transitions from edge to vertex to vertex.
		/// </summary>
		/// <param name="edgeToVert0">The edge wrap data of the first transition.</param>
		/// <param name="vert0ToVert1">The edge wrap data of the second transition</param>
		/// <returns>The combined edge wrap behavior of chaining the two given transitions.</returns>
		public static EdgeWrap ChainEdgeToVertToVert(EdgeWrap edgeToVert0, EdgeWrap vert0ToVert1)
		{
			return EliminateContraryDirections((edgeToVert0 | ShiftVertToVertAsEdgeToVert(vert0ToVert1)) & EdgeWrap.EdgeToVert);
		}

		/// <summary>
		/// Merges the edge wrap behavior of two transitions from face to face to edge.
		/// </summary>
		/// <param name="face0ToFace1">The edge wrap data of the first transition.</param>
		/// <param name="face1ToEdge">The edge wrap data of the second transition</param>
		/// <returns>The combined edge wrap behavior of chaining the two given transitions.</returns>
		public static EdgeWrap ChainFaceToFaceToEdge(EdgeWrap face0ToFace1, EdgeWrap face1ToEdge)
		{
			return EliminateContraryDirections((ShiftFaceToFaceAsFaceToEdge(face0ToFace1) | face1ToEdge) & EdgeWrap.FaceToEdge);
		}

		/// <summary>
		/// Merges the edge wrap behavior of two transitions from edge to face to face.
		/// </summary>
		/// <param name="edgeToFace0">The edge wrap data of the first transition.</param>
		/// <param name="face0ToFace1">The edge wrap data of the second transition</param>
		/// <returns>The combined edge wrap behavior of chaining the two given transitions.</returns>
		public static EdgeWrap ChainEdgeToFaceToFace(EdgeWrap edgeToFace0, EdgeWrap face0ToFace1)
		{
			return EliminateContraryDirections((edgeToFace0 | ShiftFaceToFaceAsEdgeToFace(face0ToFace1)) & EdgeWrap.EdgeToFace);
		}

		/// <summary>
		/// Merges the vertex/edge wrap behavior of two transitions from vertex to vertex to vertex as if collapsing the two intermediate edges and middle vertex into a single edge.
		/// </summary>
		/// <param name="edge0">The edge wrap data of the first transition.</param>
		/// <param name="edge1">The edge wrap data of the second transition</param>
		/// <returns>The combined edge wrap behavior of chaining the two given transitions.</returns>
		public static EdgeWrap ChainVertEdgeRelations(EdgeWrap edge0, EdgeWrap edge1)
		{
			return EliminateContraryDirections(
				(ShiftVertToVertAsVertToEdge(edge0) | edge1) & EdgeWrap.VertToEdge |
				(edge0 | ShiftVertToVertAsEdgeToVert(edge1)) & EdgeWrap.EdgeToVert);
		}

		/// <summary>
		/// Merges the face/edge wrap behavior of two transitions from face to face to face as if collapsing the two intermediate edges and middle face into a single edge.
		/// </summary>
		/// <param name="edge0">The edge wrap data of the first transition.</param>
		/// <param name="edge1">The edge wrap data of the second transition</param>
		/// <returns>The combined edge wrap behavior of chaining the two given transitions.</returns>
		public static EdgeWrap ChainFaceEdgeRelations(EdgeWrap edge0, EdgeWrap edge1)
		{
			return EliminateContraryDirections(
				(ShiftFaceToFaceAsFaceToEdge(edge0) | edge1) & EdgeWrap.FaceToEdge |
				(edge0 | ShiftFaceToFaceAsEdgeToFace(edge1)) & EdgeWrap.EdgeToFace);
		}

		/// <summary>
		/// Adjusts the original edge wrap data to include additional vertex-to-vertex wrap behavior as if it were edge-to-vertex wrap behavior.
		/// </summary>
		/// <param name="original">The original edge wrap data.</param>
		/// <param name="adjustment">The vertex-to-vertex adjustment.</param>
		/// <returns>The original edge wrap data adjusted according to the additional vertex-to-vertex wrap behavior.</returns>
		public static EdgeWrap ModifyTargetVertEdgeRelations(EdgeWrap original, EdgeWrap adjustment)
		{
			return FromEdgeRelations(
				(original | ShiftVertToVertAsEdgeToVert(adjustment)) & EdgeWrap.EdgeToVert |
				original & ~EdgeWrap.EdgeToVert);
		}

		/// <summary>
		/// Adjusts the original edge wrap data to include additional vertex-to-vertex wrap behavior as if it were vertex-to-edge wrap behavior.
		/// </summary>
		/// <param name="original">The original edge wrap data.</param>
		/// <param name="adjustment">The vertex-to-vertex adjustment.</param>
		/// <returns>The original edge wrap data adjusted according to the additional vertex-to-vertex wrap behavior.</returns>
		public static EdgeWrap ModifySourceVertEdgeRelations(EdgeWrap original, EdgeWrap adjustment)
		{
			return FromEdgeRelations(
				(ShiftVertToVertAsVertToEdge(adjustment) | original) & EdgeWrap.VertToEdge |
				original & ~EdgeWrap.VertToEdge);
		}

		/// <summary>
		/// Adjusts the original edge wrap data to include additional face-to-face wrap behavior as if it were edge-to-face wrap behavior.
		/// </summary>
		/// <param name="original">The original edge wrap data.</param>
		/// <param name="adjustment">The face-to-face adjustment.</param>
		/// <returns>The original edge wrap data adjusted according to the additional face-to-face wrap behavior.</returns>
		public static EdgeWrap ModifyTargetFaceEdgeRelations(EdgeWrap original, EdgeWrap adjustment)
		{
			return FromEdgeRelations(
				(original | ShiftFaceToFaceAsEdgeToFace(adjustment)) & EdgeWrap.EdgeToFace |
				original & ~EdgeWrap.EdgeToFace);
		}

		/// <summary>
		/// Adjusts the original edge wrap data to include additional face-to-face wrap behavior as if it were face-to-edge wrap behavior.
		/// </summary>
		/// <param name="original">The original edge wrap data.</param>
		/// <param name="adjustment">The face-to-face adjustment.</param>
		/// <returns>The original edge wrap data adjusted according to the additional vertex-to-vertex wrap behavior.</returns>
		public static EdgeWrap ModifySourceFaceEdgeRelations(EdgeWrap original, EdgeWrap adjustment)
		{
			return FromEdgeRelations(
				(ShiftFaceToFaceAsFaceToEdge(adjustment) | original) & EdgeWrap.FaceToEdge |
				original & ~EdgeWrap.FaceToEdge);
		}

		/// <summary>
		/// Merges the edge wrap behavior of two transitions from vertex to vertex to vertex.
		/// </summary>
		/// <param name="vert0ToVert1">The edge wrap data of the first transition.</param>
		/// <param name="vert1ToVert2">The edge wrap data of the second transition</param>
		/// <returns>The combined edge wrap behavior of chaining the two given transitions.</returns>
		public static EdgeWrap ChainVertToVertToVert(EdgeWrap vert0ToVert1, EdgeWrap vert1ToVert2)
		{
			// Combine the two edge wraps together, masking to keep only the vert-to-vert bits,
			// and then cancel out any positive and negative directions on the same axis.
			return EliminateContraryDirections((vert0ToVert1 | vert1ToVert2) & EdgeWrap.VertToVert);
		}

		/// <summary>
		/// Merges the edge wrap behavior of two transitions from vertex to vertex to face.
		/// </summary>
		/// <param name="vert0ToVert1">The edge wrap data of the first transition.</param>
		/// <param name="vert1ToFace">The edge wrap data of the second transition</param>
		/// <returns>The combined edge wrap behavior of chaining the two given transitions.</returns>
		public static EdgeWrap ChainVertToVertToFace(EdgeWrap vert0ToVert1, EdgeWrap vert1ToFace)
		{
			// Move the vert-to-vert data over into the vert-to-face region, and then combine
			// the two together, masking to keep only the vert-to-face bits, and then cancel
			// out any positive and negative directions on the same axis.
			return EliminateContraryDirections((ShiftVertToVertAsVertToFace(vert0ToVert1) | vert1ToFace) & EdgeWrap.VertToFace);
		}

		/// <summary>
		/// Merges the edge wrap behavior of two transitions from vertex to face to face.
		/// </summary>
		/// <param name="vertToFace0">The edge wrap data of the first transition.</param>
		/// <param name="face0ToFace1">The edge wrap data of the second transition</param>
		/// <returns>The combined edge wrap behavior of chaining the two given transitions.</returns>
		public static EdgeWrap ChainVertToFaceToFace(EdgeWrap vertToFace0, EdgeWrap face0ToFace1)
		{
			// Move the face-to-face data over into the vert-to-face region, and then combine
			// the two together, masking to keep only the vert-to-face bits, and then cancel
			// out any positive and negative directions on the same axis.
			return EliminateContraryDirections((vertToFace0 | ShiftFaceToFaceAsVertToFace(face0ToFace1)) & EdgeWrap.VertToFace);
		}

		/// <summary>
		/// Merges the edge wrap behavior of two transitions from face to vertex to vertex.
		/// </summary>
		/// <param name="faceToVert0">The edge wrap data of the first transition.</param>
		/// <param name="vert0ToVert1">The edge wrap data of the second transition</param>
		/// <returns>The combined edge wrap behavior of chaining the two given transitions.</returns>
		public static EdgeWrap ChainFaceToVertToVert(EdgeWrap faceToVert0, EdgeWrap vert0ToVert1)
		{
			// Move the vert-to-vert data over into the face-to-vert region, and then combine
			// the two together, masking to keep only the face-to-vert bits, and then cancel
			// out any positive and negative directions on the same axis.
			return EliminateContraryDirections((faceToVert0 | ShiftVertToVertAsFaceToVert(vert0ToVert1)) & EdgeWrap.FaceToVert);
		}

		/// <summary>
		/// Merges the edge wrap behavior of two transitions from face to face to vertex.
		/// </summary>
		/// <param name="face0ToFace1">The edge wrap data of the first transition.</param>
		/// <param name="face1ToVert">The edge wrap data of the second transition</param>
		/// <returns>The combined edge wrap behavior of chaining the two given transitions.</returns>
		public static EdgeWrap ChainFaceToFaceToVert(EdgeWrap face0ToFace1, EdgeWrap face1ToVert)
		{
			// Move the face-to-face data over into the face-to-vert region, and then combine
			// the two together, masking to keep only the face-to-vert bits, and then cancel
			// out any positive and negative directions on the same axis.
			return EliminateContraryDirections((ShiftFaceToFaceAsFaceToVert(face0ToFace1) | face1ToVert) & EdgeWrap.FaceToVert);
		}

		/// <summary>
		/// Merges the edge wrap behavior of two transitions from face to face to face.
		/// </summary>
		/// <param name="face0ToFace1">The edge wrap data of the first transition.</param>
		/// <param name="face1ToFace2">The edge wrap data of the second transition</param>
		/// <returns>The combined edge wrap behavior of chaining the two given transitions.</returns>
		public static EdgeWrap ChainFaceToFaceToFace(EdgeWrap face0ToFace1, EdgeWrap face1ToFace2)
		{
			// Combine the two edge wraps together, masking to keep only the face-to-face bits,
			// and then cancel out any positive and negative directions on the same axis.
			return EliminateContraryDirections((face0ToFace1 | face1ToFace2) & EdgeWrap.FaceToFace);
		}
	}
}
