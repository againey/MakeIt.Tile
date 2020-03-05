/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;

namespace MakeIt.Tile
{
	/// <summary>
	/// A concrete <see cref="Vector3"/>-typed vertex attributes collection, derived from <see cref="ScriptableObject"/>
	/// and can therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	/// <remarks><para>Because Unity will serialize neither intervertex nor generic type,
	/// this and other attribute classes drived from <see cref="ScriptableObject"/> are
	/// necessary for native serialization.</para></remarks>
	public class Vector3VertexAttribute : VertexArrayAttribute<Vector3>
	{
		/// <summary>
		/// Creates a <see cref="Vector3"/>-typed vertex attributes collection using the given array as the vertex attribute values.
		/// </summary>
		/// <param name="array">The array of vertex attribute values which will be stored, by reference, within the created vertex attributes collection.</param>
		/// <returns>A <see cref="Vector3"/>-typed vertex attributes collection.</returns>
		public static Vector3VertexAttribute Create(Vector3[] array) { return CreateDerived<Vector3VertexAttribute>(array); }

		/// <summary>
		/// Creates a <see cref="Vector3"/>-typed vertex attributes collection, allocating an array suitable for the given number of vertices.
		/// </summary>
		/// <param name="vertexCount">The number of vertices for which the vertex attribute collection should have sufficient storage.</param>
		/// <returns>A <see cref="Vector3"/>-typed vertex attributes collection.</returns>
		public static Vector3VertexAttribute Create(int vertexCount) { return CreateDerived<Vector3VertexAttribute>(vertexCount); }
	}
}
