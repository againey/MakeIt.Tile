/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/
#if false
using UnityEngine;
using Experilous.Topologies;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// A concrete <see cref="Color"/>-typed face attributes collection for
	/// looking up attributes based on the group index for a face and the attribute value
	/// for that group, derived from <see cref="ScriptableObject"/> and can
	/// therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	/// <remarks><para>Because Unity will serialize neither interface group nor generic type,
	/// this and other attribute classes drived from <see cref="ScriptableObject"/> are
	/// necessary for native serialization.</para></remarks>
	public class ColorFaceGroupLookupFaceAttribute : FaceGroupLookupFaceAttribute<Color, ColorFaceGroupAttribute>
	{
		/// <summary>
		/// Creates a <see cref="Color"/>-typed face attributes collection using the given face group indices and face group color values.
		/// </summary>
		/// <param name="faceGroupIndices">The group index attributes collection mapping each face to a particular face group.</param>
		/// <param name="faceGroupColors">The colors of the face groups.</param>
		/// <returns>A <see cref="Color"/>-typed face attributes collection.</returns>
		public static ColorFaceGroupLookupFaceAttribute Create(IntFaceAttribute faceGroupIndices, ColorFaceGroupAttribute faceGroupColors)
		{
			var instance = CreateInstance<ColorFaceGroupLookupFaceAttribute>();
			instance.faceGroupData = faceGroupColors;
			instance.faceGroupIndices = faceGroupIndices;
			return instance;
		}
	}
}
#endif