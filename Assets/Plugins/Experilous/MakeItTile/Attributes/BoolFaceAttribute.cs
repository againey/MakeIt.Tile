﻿/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

namespace Experilous.MakeItTile
{
	/// <summary>
	/// A concrete <see cref="System.Boolean"/>-typed face attributes collection, derived from <see cref="UnityEngine.ScriptableObject"/>
	/// and can therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	/// <remarks><para>Because Unity will serialize neither interface nor generic type,
	/// this and other attribute classes drived from <see cref="UnityEngine.ScriptableObject"/> are
	/// necessary for native serialization.</para></remarks>
	public class BoolFaceAttribute : FaceArrayAttribute<bool>
	{
		/// <summary>
		/// Creates a <see cref="System.Boolean"/>-typed face attributes collection using the given array as the face attribute values.
		/// </summary>
		/// <param name="array">The array of face attribute values which will be stored, by reference, within the created face attributes collection.</param>
		/// <returns>A <see cref="System.Boolean"/>-typed face attributes collection.</returns>
		public static BoolFaceAttribute Create(bool[] array) { return CreateDerived<BoolFaceAttribute>(array); }

		/// <summary>
		/// Creates a <see cref="System.Boolean"/>-typed face attributes collection, allocating an array suitable for the given number of faces.
		/// </summary>
		/// <param name="faceCount">The number of faces for which the face attribute collection should have sufficient storage.</param>
		/// <returns>A <see cref="System.Boolean"/>-typed face attributes collection.</returns>
		public static BoolFaceAttribute Create(int faceCount) { return CreateDerived<BoolFaceAttribute>(faceCount); }
	}
}
