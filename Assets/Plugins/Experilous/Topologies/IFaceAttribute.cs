/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System;
using System.Collections.Generic;

namespace Experilous.Topologies
{
	/// <summary>
	/// Generic interface for accessing attribute values of topology faces.
	/// </summary>
	/// <typeparam name="T">The type of the attribute values.</typeparam>
	/// <remarks>
	/// <para>Instead of working with integer indices everywhere, this interface allows attributes to be
	/// indexed by instances of the Face structure directly.</para>
	/// 
	/// <para>The three indexers that take an edge as an index permit the possibility of altering the face attribute
	/// lookup dependent upon the context of how the face is being accessed.  For most implementations, these three
	/// indexers are expected to simply defer to the primary indexer using the far/prev face of the edge.</para>
	/// </remarks>
	public interface IFaceAttribute<T> : IEdgeAttribute<T>
	{
		/// <summary>
		/// Lookup the attribute value for the face indicated.
		/// </summary>
		/// <param name="i">The index of the face whose attribute value is desired.</param>
		/// <returns>The attribute value for the face indicated.</returns>
		new T this[int i] { get; set; }

		/// <summary>
		/// Lookup the attribute value for the face indicated.
		/// </summary>
		/// <param name="f">The face whose attribute value is desired.</param>
		/// <returns>The attribute value for the face indicated.</returns>
		T this[Topology.Face f] { get; set; }
	}
}
