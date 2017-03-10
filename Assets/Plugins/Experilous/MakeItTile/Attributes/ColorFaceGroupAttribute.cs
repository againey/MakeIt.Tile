/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/
#if false
using UnityEngine;
using Experilous.Topologies;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// A concrete <see cref="Color"/>-typed face group attributes collection, derived from <see cref="ScriptableObject"/>
	/// and can therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	/// <remarks><para>Because Unity will serialize neither interface group nor generic type,
	/// this and other attribute classes drived from <see cref="ScriptableObject"/> are
	/// necessary for native serialization.</para></remarks>
	public class ColorFaceGroupAttribute : FaceGroupArrayAttribute<Color>
	{
		/// <summary>
		/// Creates a <see cref="Color"/>-typed face group attributes collection using the given array as the face group attribute values.
		/// </summary>
		/// <param name="array">The array of face group attribute values which will be stored, by reference, within the created face group attributes collection.</param>
		/// <returns>A <see cref="Color"/>-typed face group attributes collection.</returns>
		public static ColorFaceGroupAttribute Create(Color[] array) { return CreateDerived<ColorFaceGroupAttribute>(array); }

		/// <summary>
		/// Creates a <see cref="Color"/>-typed face group attributes collection, allocating an array suitable for the given number of face groups.
		/// </summary>
		/// <param name="groupCount">The number of face groups for which the face group attribute collection should have sufficient storage.</param>
		/// <returns>A <see cref="Color"/>-typed face group attributes collection.</returns>
		public static ColorFaceGroupAttribute Create(int groupCount) { return CreateDerived<ColorFaceGroupAttribute>(groupCount); }
	}
}
#endif