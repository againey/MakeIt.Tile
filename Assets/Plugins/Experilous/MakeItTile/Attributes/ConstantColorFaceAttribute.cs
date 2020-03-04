/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// A concrete <see cref="Color"/>-typed face attributes collection which returns
	/// the same constant value for all faces, derived from <see cref="ScriptableObject"/>
	/// and can therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	/// <remarks><para>Because Unity will serialize neither interface nor generic type,
	/// this and other attribute classes drived from <see cref="ScriptableObject"/> are
	/// necessary for native serialization.</para></remarks>
	public class ConstantColorFaceAttribute : FaceConstantAttribute<Color>
	{
		/// <summary>
		/// Creates a <see cref="Color"/>-typed face attributes collection using the given value as the shared constant facea attribute value.
		/// </summary>
		/// <param name="constant">The value to use as the shared constant facea attribute value.</param>
		/// <returns>A <see cref="Color"/>-typed constant face attributes collection.</returns>
		public static ConstantColorFaceAttribute Create(Color constant) { return CreateDerived<ConstantColorFaceAttribute>(constant); }
	}
}
