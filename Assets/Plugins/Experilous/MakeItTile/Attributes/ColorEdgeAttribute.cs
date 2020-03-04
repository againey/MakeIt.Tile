/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// A concrete <see cref="Color"/>-typed edge attributes collection, derived from <see cref="ScriptableObject"/>
	/// and can therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	/// <remarks><para>Because Unity will serialize neither interface nor generic type,
	/// this and other attribute classes drived from <see cref="ScriptableObject"/> are
	/// necessary for native serialization.</para></remarks>
	public class ColorEdgeAttribute : EdgeArrayAttribute<Color>
	{
		/// <summary>
		/// Creates a <see cref="Color"/>-typed edge attributes collection using the given array as the edge attribute values.
		/// </summary>
		/// <param name="array">The array of edge attribute values which will be stored, by reference, within the created edge attributes collection.</param>
		/// <returns>A <see cref="Color"/>-typed edge attributes collection.</returns>
		public static ColorEdgeAttribute Create(Color[] array) { return CreateDerived<ColorEdgeAttribute>(array); }

		/// <summary>
		/// Creates a <see cref="Color"/>-typed edge attributes collection, allocating an array suitable for the given number of edges.
		/// </summary>
		/// <param name="edgeCount">The number of edges for which the edge attribute collection should have sufficient storage.</param>
		/// <returns>A <see cref="Color"/>-typed edge attributes collection.</returns>
		public static ColorEdgeAttribute Create(int edgeCount) { return CreateDerived<ColorEdgeAttribute>(edgeCount); }
	}
}
