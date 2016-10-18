/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

namespace Experilous.MakeItTile
{
	/// <summary>
	/// A concrete <see cref="System.Single"/>-typed edge attributes collection, derived from <see cref="UnityEngine.ScriptableObject"/>
	/// and can therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	/// <remarks><para>Because Unity will serialize neither interface nor generic type,
	/// this and other attribute classes drived from <see cref="UnityEngine.ScriptableObject"/> are
	/// necessary for native serialization.</para></remarks>
	public class FloatEdgeAttribute : EdgeArrayAttribute<float>
	{
		/// <summary>
		/// Creates a <see cref="System.Single"/>-typed edge attributes collection using the given array as the edge attribute values.
		/// </summary>
		/// <param name="array">The array of edge attribute values which will be stored, by reference, within the created edge attributes collection.</param>
		/// <returns>A <see cref="System.Single"/>-typed edge attributes collection.</returns>
		public static FloatEdgeAttribute Create(float[] array) { return CreateDerived<FloatEdgeAttribute>(array); }

		/// <summary>
		/// Creates a <see cref="System.Single"/>-typed edge attributes collection, allocating an array suitable for the given number of edges.
		/// </summary>
		/// <param name="edgeCount">The number of edges for which the edge attribute collection should have sufficient storage.</param>
		/// <returns>A <see cref="System.Single"/>-typed edge attributes collection.</returns>
		public static FloatEdgeAttribute Create(int edgeCount) { return CreateDerived<FloatEdgeAttribute>(edgeCount); }
	}
}
