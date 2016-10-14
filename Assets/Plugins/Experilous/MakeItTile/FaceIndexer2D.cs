/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using Experilous.Numerics;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// Interface for accessing the internal faces of a topology using two-dimensional indices.
	/// </summary>
	public interface IFaceIndexer2D
	{
		/// <summary>
		/// The number of internal faces in the topology.
		/// </summary>
		int internalFaceCount { get; }

		/// <summary>
		/// Gets the face corresponding to the given two-dimensional index.
		/// </summary>
		/// <param name="index">The two-dimensional index used for lookup.</param>
		/// <returns>The face corresponding to the given two-dimensional index.</returns>
		Topology.Face GetFace(IntVector2 index);

		/// <summary>
		/// Gets the face corresponding to the given two-dimensional index.
		/// </summary>
		/// <param name="x">The first component of the two-dimensional index.</param>
		/// <param name="y">The second component of the two-dimensional index.</param>
		/// <returns>The face corresponding to the given two-dimensional index.</returns>
		Topology.Face GetFace(int x, int y);

		/// <summary>
		/// Gets the standard one-dimensional face index corresponding to the given two-dimensional index.
		/// </summary>
		/// <param name="index">The two-dimensional index used for lookup.</param>
		/// <returns>The standard one-dimensional face index corresponding to the given two-dimensional index.</returns>
		/// <seealso cref="Topology.Face.index"/>
		int GetFaceIndex(IntVector2 index);

		/// <summary>
		/// Gets the standard one-dimensional face index corresponding to the given two-dimensional index.
		/// </summary>
		/// <param name="x">The first component of the two-dimensional index.</param>
		/// <param name="y">The second component of the two-dimensional index.</param>
		/// <returns>The standard one-dimensional face index corresponding to the given two-dimensional index.</returns>
		/// <seealso cref="Topology.Face.index"/>
		int GetFaceIndex(int x, int y);

		/// <summary>
		/// Gets the two-dimensional index of the given face.
		/// </summary>
		/// <param name="face">The face whose two-dimensional index is to be returned.</param>
		/// <returns>The two-dimensional index of the given face.</returns>
		IntVector2 GetFaceIndex2D(Topology.Face internalFace);

		/// <summary>
		/// Gets the two-dimensional index of the face with the given standard one-dimesional index.
		/// </summary>
		/// <param name="face">The face whose two-dimensional index is to be returned.</param>
		/// <returns>The two-dimensional index of the face with the given standard one-dimesional index.</returns>
		IntVector2 GetFaceIndex2D(int internalFaceIndex);

		/// <summary>
		/// Wraps the first component of a two-dimensional face index if the topology wraps appropriately.
		/// </summary>
		/// <param name="x">The first component of a two-dimensional face index.  Can be outside the normal bounds for this component.</param>
		/// <returns>The first component of a two-dimensional face index, wrapped to be within the normal bounds for this component if the topology wraps along this dimension, or the unmodified input value if not.</returns>
		int FaceWrapX(int x);

		/// <summary>
		/// Wraps the second component of a two-dimensional face index if the topology wraps appropriately.
		/// </summary>
		/// <param name="y">The second component of a two-dimensional face index.  Can be outside the normal bounds for this component.</param>
		/// <returns>The second component of a two-dimensional face index, wrapped to be within the normal bounds for this component if the topology wraps along this dimension, or the unmodified input value if not.</returns>
		int FaceWrapY(int y);

		/// <summary>
		/// Wraps a two-dimensional face index if the topology wraps appropriately.
		/// </summary>
		/// <param name="x">The first component of a two-dimensional face index.  Can be outside the normal bounds for this component.</param>
		/// <param name="y">The second component of a two-dimensional face index.  Can be outside the normal bounds for this component.</param>
		/// <returns>The two-dimensional face index, wrapped to be within the normal bounds for each component if the topology wraps along the corresponding dimension, or the unmodified input value if not.</returns>
		IntVector2 FaceWrap(int x, int y);

		/// <summary>
		/// Wraps a two-dimensional face index if the topology wraps appropriately.
		/// </summary>
		/// <param name="index">The two-dimensional index to be wrapped.</param>
		/// <returns>The two-dimensional face index, wrapped to be within the normal bounds for each component if the topology wraps along the corresponding dimension, or the unmodified input value if not.</returns>
		IntVector2 FaceWrap(IntVector2 index);

		/// <summary>
		/// Gets the face corresponding to the given two-dimensional index, first wrapping the two-dimensional index if necessary.
		/// </summary>
		/// <param name="index">The two-dimensional index used for lookup.</param>
		/// <returns>The face corresponding to the given two-dimensional index after being wrapped.</returns>
		Topology.Face GetWrappedFace(IntVector2 index);

		/// <summary>
		/// Gets the face corresponding to the given two-dimensional index, first wrapping the two-dimensional index if necessary.
		/// </summary>
		/// <param name="x">The first component of the two-dimensional index.</param>
		/// <param name="y">The second component of the two-dimensional index.</param>
		/// <returns>The face corresponding to the given two-dimensional index after being wrapped.</returns>
		Topology.Face GetWrappedFace(int x, int y);

		/// <summary>
		/// Gets the standard one-dimensional face index corresponding to the given two-dimensional index, first wrapping the two-dimensional index if necessary.
		/// </summary>
		/// <param name="index">The two-dimensional index used for lookup.</param>
		/// <returns>The standard one-dimensional face index corresponding to the given two-dimensional index after being wrapped.</returns>
		/// <seealso cref="Topology.Face.index"/>
		int GetWrappedFaceIndex(IntVector2 index);

		/// <summary>
		/// Gets the standard one-dimensional face index corresponding to the given two-dimensional index, first wrapping the two-dimensional index if necessary.
		/// </summary>
		/// <param name="x">The first component of the two-dimensional index.</param>
		/// <param name="y">The second component of the two-dimensional index.</param>
		/// <returns>The standard one-dimensional face index corresponding to the given two-dimensional index after being wrapped.</returns>
		/// <seealso cref="Topology.Face.index"/>
		int GetWrappedFaceIndex(int x, int y);
	}

	/// <summary>
	/// Abstract base class for accessing the faces of a topology using two-dimensional indices, implementing some of the methods in terms of other methods.
	/// </summary>
	public abstract class FaceIndexer2D : ScriptableObject, IFaceIndexer2D
	{
		/// <summary>
		/// The topology containing the faces with which this object works.
		/// </summary>
		public Topology topology;

		/// <inheritdoc/>
		public abstract int internalFaceCount { get; }

		/// <inheritdoc/>
		public abstract int GetFaceIndex(int x, int y);
		/// <inheritdoc/>
		public abstract IntVector2 GetFaceIndex2D(int internalFaceIndex);

		/// <inheritdoc/>
		public abstract int FaceWrapX(int x);
		/// <inheritdoc/>
		public abstract int FaceWrapY(int y);

		/// <inheritdoc/>
		public Topology.Face GetFace(IntVector2 index) { return topology.faces[GetFaceIndex(index.x, index.y)]; }
		/// <inheritdoc/>
		public Topology.Face GetFace(int x, int y) { return topology.faces[GetFaceIndex(x, y)]; }
		/// <inheritdoc/>
		public int GetFaceIndex(IntVector2 index) { return GetFaceIndex(index.x, index.y); }

		/// <inheritdoc/>
		public IntVector2 GetFaceIndex2D(Topology.Face internalFace) { return GetFaceIndex2D(internalFace.index); }

		/// <inheritdoc/>
		public IntVector2 FaceWrap(int x, int y) { return new IntVector2(FaceWrapX(x), FaceWrapY(y)); }
		/// <inheritdoc/>
		public IntVector2 FaceWrap(IntVector2 index) { return new IntVector2(FaceWrapX(index.x), FaceWrapY(index.y)); }

		/// <inheritdoc/>
		public Topology.Face GetWrappedFace(IntVector2 index) { return GetFace(FaceWrap(index)); }
		/// <inheritdoc/>
		public Topology.Face GetWrappedFace(int x, int y) { return GetFace(FaceWrap(x, y)); }
		/// <inheritdoc/>
		public int GetWrappedFaceIndex(IntVector2 index) { return GetFaceIndex(FaceWrap(index)); }
		/// <inheritdoc/>
		public int GetWrappedFaceIndex(int x, int y) { return GetFaceIndex(FaceWrap(x, y)); }
	}
}
