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
	/// Generic interface for accessing attribute values of topology edges.
	/// </summary>
	/// <typeparam name="T">The type of the attribute values.</typeparam>
	/// <remarks>
	/// <para>Instead of working with integer indices everywhere, this interface allows attributes to be
	/// indexed by instances of one of the three edge structures directly.</para>
	/// </remarks>
	public interface IEdgeAttribute<T> : IList<T>
	{
		/// <summary>
		/// Lookup the attribute value for the half edge indicated.
		/// </summary>
		/// <param name="i">The index of the half edge whose attribute value is desired.</param>
		/// <returns>The attribute value for the half edge indicated.</returns>
		new T this[int i] { get; set; }

		/// <summary>
		/// Lookup the attribute value for the half edge indicated.
		/// </summary>
		/// <param name="e">The half edge whose attribute value is desired.</param>
		/// <returns>The attribute value for the half edge indicated.</returns>
		T this[Topology.HalfEdge e] { get; set; }

		/// <summary>
		/// Lookup the attribute value for a half edge relative to a neighboring vertex.
		/// </summary>
		/// <param name="e">The half edge (represented as a vertex edge) whose attribute value is desired.</param>
		/// <returns>The attribute value for the indicated half edge, relative to its near vertex.</returns>
		T this[Topology.VertexEdge e] { get; set; }

		/// <summary>
		/// Lookup the attribute value for a half edge relative to a neighboring face.
		/// </summary>
		/// <param name="e">The half edge (represented as a face edge) whose attribute value is desired.</param>
		/// <returns>The attribute value for the indicated half edge, relative to its near face.</returns>
		T this[Topology.FaceEdge e] { get; set; }
	}
}

#endif
