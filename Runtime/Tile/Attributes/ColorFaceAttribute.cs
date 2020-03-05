/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;

namespace MakeIt.Tile
{
	/// <summary>
	/// A concrete <see cref="Color"/>-typed face attributes collection, derived from <see cref="ScriptableObject"/>
	/// and can therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	/// <remarks><para>Because Unity will serialize neither interface nor generic type,
	/// this and other attribute classes drived from <see cref="ScriptableObject"/> are
	/// necessary for native serialization.</para></remarks>
	public class ColorFaceAttribute : FaceArrayAttribute<Color>
	{
		/// <summary>
		/// Creates a <see cref="Color"/>-typed face attributes collection using the given array as the face attribute values.
		/// </summary>
		/// <param name="array">The array of face attribute values which will be stored, by reference, within the created face attributes collection.</param>
		/// <returns>A <see cref="Color"/>-typed face attributes collection.</returns>
		public static ColorFaceAttribute Create(Color[] array) { return CreateDerived<ColorFaceAttribute>(array); }

		/// <summary>
		/// Creates a <see cref="Color"/>-typed face attributes collection, allocating an array suitable for the given number of faces.
		/// </summary>
		/// <param name="faceCount">The number of faces for which the face attribute collection should have sufficient storage.</param>
		/// <returns>A <see cref="Color"/>-typed face attributes collection.</returns>
		public static ColorFaceAttribute Create(int faceCount) { return CreateDerived<ColorFaceAttribute>(faceCount); }
	}
}
