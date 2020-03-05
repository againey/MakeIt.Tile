/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using ScaledRay = MakeIt.Numerics.ScaledRay;

namespace MakeIt.Tile
{
	/// <summary>
	/// Interface for identifying the face of a manifold to which points belong, and for performing ray picking against a manifold to find the face that was hit by the ray.
	/// </summary>
	public interface IFaceSpatialPartitioning
	{
		/// <summary>
		/// Finds the face to which the given point belongs.
		/// </summary>
		/// <param name="point">The point to use when searching for the corresponding face.</param>
		/// <returns>The face to which the given point belongs.</returns>
		/// <remarks><para>Typically, the point will be on or close to the surface of the manifold,
		/// such that it is clear that it belongs to a particular face.  It does not need to be near
		/// the surface, however, as each face implicitly defines a volume above and below it, and
		/// all points within that volume belong to that face.</para>
		/// <para>Depending on the shape of the manifold, not all faces returned by this method will
		/// be internal faces.  For example, a rectangular manifold with no wrap-around edges contains
		/// a single external face that represents all areas outside the rectangle, and any point that
		/// lies on, above, or below this outer area will map to the external face.  Implementations
		/// will typically be able to map all points to some face, however, and should not return an
		/// empty face except in unusual and well documented circumstances.</para></remarks>
		Topology.Face FindFace(Vector3 point);

		/// <summary>
		/// Finds the face with which the given ray intersects.
		/// </summary>
		/// <param name="ray">The ray to intersect with the manifold to find which face it intersects.</param>
		/// <returns>The first face which is intersected by the given ray, or an empty face if the ray does not intersect the manifold at all.</returns>
		Topology.Face FindFace(Ray ray);

		/// <summary>
		/// Finds the face with which the given ray intersects.
		/// </summary>
		/// <param name="ray">The ray to intersect with the manifold to find which face it intersects.</param>
		/// <returns>The first face which is intersected by the given ray, or an empty face if the ray does not intersect the manifold at all.</returns>
		Topology.Face FindFace(ScaledRay ray);
	}
}
