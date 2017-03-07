/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

#if false

using UnityEngine;
using System;
using System.Collections.Generic;

namespace Experilous.Topologies
{
	/// <summary>
	/// Generic interface for accessing attribute values of topology vertices.
	/// </summary>
	/// <typeparam name="T">The type of the attribute values.</typeparam>
	/// <remarks>
	/// <para>Instead of working with integer indices everywhere, this interface allows attributes to be
	/// indexed by instances of the Vertex structure directly.</para>
	/// 
	/// <para>The three indexers that take an edge as an index permit the possibility of altering the vertex attribute
	/// lookup dependent upon the context of how the vertex is being accessed.  For most implementations, these three
	/// indexers are expected to simply defer to the primary indexer using the far/next vertex of the edge.</para>
	/// </remarks>
	public interface IVertexAttribute<T> : IEdgeAttribute<T>
	{
		/// <summary>
		/// Lookup the attribute value for the vertex indicated.
		/// </summary>
		/// <param name="i">The index of the vertex whose attribute value is desired.</param>
		/// <returns>The attribute value for the vertex indicated.</returns>
		new T this[int i] { get; set; }

		/// <summary>
		/// Lookup the attribute value for the vertex indicated.
		/// </summary>
		/// <param name="v">The vertex whose attribute value is desired.</param>
		/// <returns>The attribute value for the vertex indicated.</returns>
		T this[Topology.Vertex v] { get; set; }
	}
}

#endif
