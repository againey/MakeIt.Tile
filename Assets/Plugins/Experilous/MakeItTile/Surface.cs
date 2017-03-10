/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/
#if false
using UnityEngine;
using Experilous.Numerics;
using Experilous.Topologies;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// An interface for describing the overall shape of a two dimensional surface in three dimensional space.
	/// </summary>
	public interface ISurface
	{
		/// <summary>
		/// Indicates if the surface is flat, meaning that it has a curvature of zero at all locations and is therefore planar.
		/// </summary>
		bool isFlat { get; }

		/// <summary>
		/// Indicates if the surface is curved, meaning that it has a non-zero curvature at some or all locations and is therefore not planar.
		/// </summary>
		bool isCurved { get; }

		/// <summary>
		/// Indicates if the surface never has a positive curvature at any location, which also means that no two surface normals from different locations on the surface ever point toward the same position.
		/// </summary>
		bool isConvex { get; }

		/// <summary>
		/// Indicates if the surface has a positive curvature at some or all locations, which also means that two surface normals from different locations on the surface at least sometimes point toward the same position.
		/// </summary>
		bool isConcave { get; }

		/// <summary>
		/// Determines the position at which the given ray intersects the current surface.
		/// </summary>
		/// <param name="ray">The ray to intersect with the surface.</param>
		/// <returns>The position at which the ray intersects the surface.</returns>
		/// <remarks><note type="caution">If the ray does not actually intersect the surface, the return value is
		/// undefined.  Use <see cref="Intersect(Ray, out Vector3)"/> instead, if this is a possibility.</note></remarks>
		/// <seealso cref="Intersect(Ray, out Vector3)"/>
		Vector3 Intersect(Ray ray);

		/// <summary>
		/// Determines the position at which the given ray intersects the current surface.
		/// </summary>
		/// <param name="ray">The ray to intersect with the surface.</param>
		/// <returns>The position at which the ray intersects the surface.</returns>
		/// <remarks><note type="caution">If the ray does not actually intersect the surface, the return value is
		/// undefined.  Use <see cref="Intersect(ScaledRay, out Vector3)"/> instead, if this is a possibility.</note></remarks>
		/// <seealso cref="Intersect(ScaledRay, out Vector3)"/>
		Vector3 Intersect(ScaledRay ray);

		/// <summary>
		/// Determines if the given ray intersects the current surface, and the position at which it intersects.
		/// </summary>
		/// <param name="ray">The ray to intersect with the surface.</param>
		/// <param name="intersection">The position at which the ray intersects the surface.</param>
		/// <returns>True if the ray does intersect the surface, and false if it does not.</returns>
		bool Intersect(Ray ray, out Vector3 intersection);

		/// <summary>
		/// Determines if the given ray intersects the current surface, and the position at which it intersects.
		/// </summary>
		/// <param name="ray">The ray to intersect with the surface.</param>
		/// <param name="intersection">The position at which the ray intersects the surface.</param>
		/// <returns>True if the ray does intersect the surface, and false if it does not.</returns>
		bool Intersect(ScaledRay ray, out Vector3 intersection);

		/// <summary>
		/// Projects the given point onto the current surface, generally the point on the surface nearest to the given point.
		/// </summary>
		/// <param name="position">The position of the point to project.</param>
		/// <returns>The position on the surface to which the given point was projected.</returns>
		Vector3 Project(Vector3 position);

		/// <summary>
		/// Gets the surface normal of the current surface at or near the given position.
		/// </summary>
		/// <param name="position">The position at which to calculate the surface normal.  Should typically be on or near the actual surface, but does not need to be.</param>
		/// <returns>The surface normal of the point where the given position projects onto the surface.</returns>
		Vector3 GetNormal(Vector3 position);

		/// <summary>
		/// Shifts the given position according to details of the surface and the specified edge wrap behavior,
		/// if the transition from one vertex to a neighboring vertex involves a wrapping behavior.
		/// </summary>
		/// <param name="position">The original position to be shifted.</param>
		/// <param name="edgeWrap">The edge wrap data determining if and how the position should be shifted.</param>
		/// <returns>The shifted position, or the original position if no wrapping occurred.</returns>
		Vector3 OffsetVertToVertAttribute(Vector3 position, EdgeWrap edgeWrap);

		/// <summary>
		/// Shifts the given position according to details of the surface and the specified edge wrap behavior,
		/// if the transition from a vertex to a neighboring edge involves a wrapping behavior.
		/// </summary>
		/// <param name="position">The original position to be shifted.</param>
		/// <param name="edgeWrap">The edge wrap data determining if and how the position should be shifted.</param>
		/// <returns>The shifted position, or the original position if no wrapping occurred.</returns>
		Vector3 OffsetVertToEdgeAttribute(Vector3 position, EdgeWrap edgeWrap);

		/// <summary>
		/// Shifts the given position according to details of the surface and the specified edge wrap behavior,
		/// if the transition from a vertex to a neighboring face involves a wrapping behavior.
		/// </summary>
		/// <param name="position">The original position to be shifted.</param>
		/// <param name="edgeWrap">The edge wrap data determining if and how the position should be shifted.</param>
		/// <returns>The shifted position, or the original position if no wrapping occurred.</returns>
		Vector3 OffsetVertToFaceAttribute(Vector3 position, EdgeWrap edgeWrap);

		/// <summary>
		/// Shifts the given position according to details of the surface and the specified edge wrap behavior,
		/// if the transition from an edge to its target vertex involves a wrapping behavior.
		/// </summary>
		/// <param name="position">The original position to be shifted.</param>
		/// <param name="edgeWrap">The edge wrap data determining if and how the position should be shifted.</param>
		/// <returns>The shifted position, or the original position if no wrapping occurred.</returns>
		Vector3 OffsetEdgeToVertAttribute(Vector3 position, EdgeWrap edgeWrap);

		/// <summary>
		/// Shifts the given position according to details of the surface and the specified edge wrap behavior,
		/// if the transition from an to its taret face involves a wrapping behavior.
		/// </summary>
		/// <param name="position">The original position to be shifted.</param>
		/// <param name="edgeWrap">The edge wrap data determining if and how the position should be shifted.</param>
		/// <returns>The shifted position, or the original position if no wrapping occurred.</returns>
		Vector3 OffsetEdgeToFaceAttribute(Vector3 position, EdgeWrap edgeWrap);

		/// <summary>
		/// Shifts the given position according to details of the surface and the specified edge wrap behavior,
		/// if the transition from a face to a neighboring vertex involves a wrapping behavior.
		/// </summary>
		/// <param name="position">The original position to be shifted.</param>
		/// <param name="edgeWrap">The edge wrap data determining if and how the position should be shifted.</param>
		/// <returns>The shifted position, or the original position if no wrapping occurred.</returns>
		Vector3 OffsetFaceToVertAttribute(Vector3 position, EdgeWrap edgeWrap);

		/// <summary>
		/// Shifts the given position according to details of the surface and the specified edge wrap behavior,
		/// if the transition from a face to a neighboring edge involves a wrapping behavior.
		/// </summary>
		/// <param name="position">The original position to be shifted.</param>
		/// <param name="edgeWrap">The edge wrap data determining if and how the position should be shifted.</param>
		/// <returns>The shifted position, or the original position if no wrapping occurred.</returns>
		Vector3 OffsetFaceToEdgeAttribute(Vector3 position, EdgeWrap edgeWrap);

		/// <summary>
		/// Shifts the given position according to details of the surface and the specified edge wrap behavior,
		/// if the transition from one face to a neighboring face involves a wrapping behavior.
		/// </summary>
		/// <param name="position">The original position to be shifted.</param>
		/// <param name="edgeWrap">The edge wrap data determining if and how the position should be shifted.</param>
		/// <returns>The shifted position, or the original position if no wrapping occurred.</returns>
		Vector3 OffsetFaceToFaceAttribute(Vector3 position, EdgeWrap edgeWrap);

		/// <summary>
		/// Reverses the shift of the given position according to details of the surface and the specified edge
		/// wrap behavior, if the transition from one vertex to a neighboring vertex involves a wrapping behavior.
		/// </summary>
		/// <param name="position">The potentially shifted position to be reversed.</param>
		/// <param name="edgeWrap">The edge wrap data determining if and how the position should be shifted.</param>
		/// <returns>The position with the shift reversed, or the original position if no wrapping occurred.</returns>
		Vector3 ReverseOffsetVertToVertAttribute(Vector3 position, EdgeWrap edgeWrap);

		/// <summary>
		/// Reverses the shift of the given position according to details of the surface and the specified edge
		/// wrap behavior, if the transition from a vertex to a neighboring edge involves a wrapping behavior.
		/// </summary>
		/// <param name="position">The potentially shifted position to be reversed.</param>
		/// <param name="edgeWrap">The edge wrap data determining if and how the position should be shifted.</param>
		/// <returns>The position with the shift reversed, or the original position if no wrapping occurred.</returns>
		Vector3 ReverseOffsetVertToEdgeAttribute(Vector3 position, EdgeWrap edgeWrap);

		/// <summary>
		/// Reverses the shift of the given position according to details of the surface and the specified edge
		/// wrap behavior, if the transition from a vertex to a neighboring face involves a wrapping behavior.
		/// </summary>
		/// <param name="position">The potentially shifted position to be reversed.</param>
		/// <param name="edgeWrap">The edge wrap data determining if and how the position should be shifted.</param>
		/// <returns>The position with the shift reversed, or the original position if no wrapping occurred.</returns>
		Vector3 ReverseOffsetVertToFaceAttribute(Vector3 position, EdgeWrap edgeWrap);

		/// <summary>
		/// Reverses the shift of the given position according to details of the surface and the specified edge
		/// wrap behavior, if the transition from an edge to its target vertex involves a wrapping behavior.
		/// </summary>
		/// <param name="position">The potentially shifted position to be reversed.</param>
		/// <param name="edgeWrap">The edge wrap data determining if and how the position should be shifted.</param>
		/// <returns>The position with the shift reversed, or the original position if no wrapping occurred.</returns>
		Vector3 ReverseOffsetEdgeToVertAttribute(Vector3 position, EdgeWrap edgeWrap);

		/// <summary>
		/// Reverses the shift of the given position according to details of the surface and the specified edge
		/// wrap behavior, if the transition from an edge to its target face involves a wrapping behavior.
		/// </summary>
		/// <param name="position">The potentially shifted position to be reversed.</param>
		/// <param name="edgeWrap">The edge wrap data determining if and how the position should be shifted.</param>
		/// <returns>The position with the shift reversed, or the original position if no wrapping occurred.</returns>
		Vector3 ReverseOffsetEdgeToFaceAttribute(Vector3 position, EdgeWrap edgeWrap);

		/// <summary>
		/// Reverses the shift of the given position according to details of the surface and the specified edge
		/// wrap behavior, if the transition from a face to a neighboring vert involves a wrapping behavior.
		/// </summary>
		/// <param name="position">The potentially shifted position to be reversed.</param>
		/// <param name="edgeWrap">The edge wrap data determining if and how the position should be shifted.</param>
		/// <returns>The position with the shift reversed, or the original position if no wrapping occurred.</returns>
		Vector3 ReverseOffsetFaceToVertAttribute(Vector3 position, EdgeWrap edgeWrap);

		/// <summary>
		/// Reverses the shift of the given position according to details of the surface and the specified edge
		/// wrap behavior, if the transition from a face to a neighboring edge involves a wrapping behavior.
		/// </summary>
		/// <param name="position">The potentially shifted position to be reversed.</param>
		/// <param name="edgeWrap">The edge wrap data determining if and how the position should be shifted.</param>
		/// <returns>The position with the shift reversed, or the original position if no wrapping occurred.</returns>
		Vector3 ReverseOffsetFaceToEdgeAttribute(Vector3 position, EdgeWrap edgeWrap);

		/// <summary>
		/// Reverses the shift of the given position according to details of the surface and the specified edge
		/// wrap behavior, if the transition from one face to a neighboring face involves a wrapping behavior.
		/// </summary>
		/// <param name="position">The potentially shifted position to be reversed.</param>
		/// <param name="edgeWrap">The edge wrap data determining if and how the position should be shifted.</param>
		/// <returns>The position with the shift reversed, or the original position if no wrapping occurred.</returns>
		Vector3 ReverseOffsetFaceToFaceAttribute(Vector3 position, EdgeWrap edgeWrap);
	}

	/// <summary>
	/// Abstract base class for surfaces that also derives from <see cref="ScriptableObject"/>
	/// and can therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	public abstract class Surface : ScriptableObject, ISurface
	{
		/// <inheritdoc/>
		public abstract bool isFlat { get; }
		/// <inheritdoc/>
		public abstract bool isCurved { get; }

		/// <inheritdoc/>
		public abstract bool isConvex { get; }
		/// <inheritdoc/>
		public abstract bool isConcave { get; }

		/// <inheritdoc/>
		public abstract Vector3 Intersect(Ray ray);
		/// <inheritdoc/>
		public abstract Vector3 Intersect(ScaledRay ray);
		/// <inheritdoc/>
		public abstract bool Intersect(Ray ray, out Vector3 intersection);
		/// <inheritdoc/>
		public abstract bool Intersect(ScaledRay ray, out Vector3 intersection);

		/// <inheritdoc/>
		public abstract Vector3 Project(Vector3 position);
		/// <inheritdoc/>
		public abstract Vector3 GetNormal(Vector3 position);

		/// <inheritdoc/>
		public virtual Vector3 OffsetVertToVertAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		/// <inheritdoc/>
		public virtual Vector3 OffsetVertToEdgeAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		/// <inheritdoc/>
		public virtual Vector3 OffsetVertToFaceAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		/// <inheritdoc/>
		public virtual Vector3 OffsetEdgeToVertAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		/// <inheritdoc/>
		public virtual Vector3 OffsetEdgeToFaceAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		/// <inheritdoc/>
		public virtual Vector3 OffsetFaceToVertAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		/// <inheritdoc/>
		public virtual Vector3 OffsetFaceToEdgeAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		/// <inheritdoc/>
		public virtual Vector3 OffsetFaceToFaceAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }

		/// <inheritdoc/>
		public virtual Vector3 ReverseOffsetVertToVertAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		/// <inheritdoc/>
		public virtual Vector3 ReverseOffsetVertToEdgeAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		/// <inheritdoc/>
		public virtual Vector3 ReverseOffsetVertToFaceAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		/// <inheritdoc/>
		public virtual Vector3 ReverseOffsetEdgeToVertAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		/// <inheritdoc/>
		public virtual Vector3 ReverseOffsetEdgeToFaceAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		/// <inheritdoc/>
		public virtual Vector3 ReverseOffsetFaceToVertAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		/// <inheritdoc/>
		public virtual Vector3 ReverseOffsetFaceToEdgeAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
		/// <inheritdoc/>
		public virtual Vector3 ReverseOffsetFaceToFaceAttribute(Vector3 position, EdgeWrap edgeWrap) { return position; }
	}
}
#endif