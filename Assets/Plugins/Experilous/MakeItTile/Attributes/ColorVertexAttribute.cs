/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// A concrete <see cref="Color"/>-typed vertex attributes collection, derived from <see cref="ScriptableObject"/>
	/// and can therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	/// <remarks><para>Because Unity will serialize neither intervertex nor generic type,
	/// this and other attribute classes drived from <see cref="ScriptableObject"/> are
	/// necessary for native serialization.</para></remarks>
	public class ColorVertexAttribute : VertexArrayAttribute<Color>
	{
		/// <summary>
		/// Creates a <see cref="Color"/>-typed vertex attributes collection using the given array as the vertex attribute values.
		/// </summary>
		/// <param name="array">The array of vertex attribute values which will be stored, by reference, within the created vertex attributes collection.</param>
		/// <returns>A <see cref="Color"/>-typed vertex attributes collection.</returns>
		public static ColorVertexAttribute Create(Color[] array) { return CreateDerived<ColorVertexAttribute>(array); }

		/// <summary>
		/// Creates a <see cref="Color"/>-typed vertex attributes collection, allocating an array suitable for the given number of vertices.
		/// </summary>
		/// <param name="vertexCount">The number of vertices for which the vertex attribute collection should have sufficient storage.</param>
		/// <returns>A <see cref="Color"/>-typed vertex attributes collection.</returns>
		public static ColorVertexAttribute Create(int vertexCount) { return CreateDerived<ColorVertexAttribute>(vertexCount); }
	}
}
