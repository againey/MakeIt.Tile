/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using Experilous.Topologies;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// A concrete <see cref="Color"/>-typed edge attributes collection which returns
	/// the same constant value for all edges, derived from <see cref="ScriptableObject"/>
	/// and can therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	/// <remarks><para>Because Unity will serialize neither interface nor generic type,
	/// this and other attribute classes drived from <see cref="ScriptableObject"/> are
	/// necessary for native serialization.</para></remarks>
	public class ConstantColorEdgeAttribute : EdgeConstantAttribute<Color>
	{
		/// <summary>
		/// Creates a <see cref="Color"/>-typed edge attributes collection using the given value as the shared constant edgea attribute value.
		/// </summary>
		/// <param name="constant">The value to use as the shared constant edgea attribute value.</param>
		/// <returns>A <see cref="Color"/>-typed constant edge attributes collection.</returns>
		public static ConstantColorEdgeAttribute Create(Color constant) { return CreateDerived<ConstantColorEdgeAttribute>(constant); }
	}
}
