/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using Experilous.Topologies;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// A concrete <see cref="Vector2"/>-typed edge attributes collection, derived from <see cref="ScriptableObject"/>
	/// and can therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	/// <remarks><para>Because Unity will serialize neither interface nor generic type,
	/// this and other attribute classes drived from <see cref="ScriptableObject"/> are
	/// necessary for native serialization.</para></remarks>
	public class Vector2EdgeAttribute : EdgeArrayAttribute<Vector2>
	{
		/// <summary>
		/// Creates a <see cref="Vector2"/>-typed edge attributes collection using the given array as the edge attribute values.
		/// </summary>
		/// <param name="array">The array of edge attribute values which will be stored, by reference, within the created edge attributes collection.</param>
		/// <returns>A <see cref="Vector2"/>-typed edge attributes collection.</returns>
		public static Vector2EdgeAttribute Create(Vector2[] array) { return CreateDerived<Vector2EdgeAttribute>(array); }

		/// <summary>
		/// Creates a <see cref="Vector2"/>-typed edge attributes collection, allocating an array suitable for the given number of edges.
		/// </summary>
		/// <param name="edgeCount">The number of edges for which the edge attribute collection should have sufficient storage.</param>
		/// <returns>A <see cref="Vector2"/>-typed edge attributes collection.</returns>
		public static Vector2EdgeAttribute Create(int edgeCount) { return CreateDerived<Vector2EdgeAttribute>(edgeCount); }
	}
}
