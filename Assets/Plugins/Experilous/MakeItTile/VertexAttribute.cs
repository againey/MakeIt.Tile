/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/
#if false
using UnityEngine;
using System;
using System.Collections.Generic;
using Experilous.Topologies;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// A vertex attribute wrapper around a raw array of vertex attributes.
	/// </summary>
	/// <typeparam name="T">The type of the attribute values.</typeparam>
	public struct VertexAttributeArrayWrapper<T> : IVertexAttribute<T>
	{
		/// <summary>
		/// The array of vertex attribute values that is being wrapped.
		/// </summary>
		public readonly T[] array;

		/// <summary>
		/// Constructs a vertex attribute array wrapper around the given array.
		/// </summary>
		/// <param name="array">The array of vertex attribute values to be wrapped.</param>
		public VertexAttributeArrayWrapper(T[] array)
		{
			this.array = array;
		}

		/// <summary>
		/// Constructs a vertex attribute array wrapper around an internally created array with the given number of elements.
		/// </summary>
		/// <param name="elementCount">The number of elements that the created array will contain.</param>
		public VertexAttributeArrayWrapper(int elementCount)
		{
			array = new T[elementCount];
		}

		/// <summary>
		/// Implicitly converts an array wrapper to a raw array by simply returning the wrapped array.
		/// </summary>
		/// <param name="arrayWrapper">The array wrapper to convert.</param>
		/// <returns>The raw array wrapped by the converted array wrapper.</returns>
		public static implicit operator T[](VertexAttributeArrayWrapper<T> arrayWrapper)
		{
			return arrayWrapper.array;
		}

		/// <summary>
		/// Implicitly converts a raw array to an array wrapper by constructing a wrapper around the array.
		/// </summary>
		/// <param name="array">The array to be converted.</param>
		/// <returns>An array wrapper around the converted array.</returns>
		public static implicit operator VertexAttributeArrayWrapper<T>(T[] array)
		{
			return new VertexAttributeArrayWrapper<T>(array);
		}

		/// <summary>
		/// Accesses the vertex attribute value in the array corresponding to the vertex with the given index.
		/// </summary>
		/// <param name="i">The index of the vertex whose attribute value is to be accessed.</param>
		/// <returns>The vertex attribute value corresponding to the vertex with the given index.</returns>
		public T this[int i]
		{
			get { return array[i]; }
			set { array[i] = value; }
		}

		/// <summary>
		/// Accesses the vertex attribute value in the array corresponding to the given vertex.
		/// </summary>
		/// <param name="v">The vertex whose attribute value is to be accessed.</param>
		/// <returns>The vertex attribute value corresponding to the given vertex.</returns>
		public T this[Topology.Vertex v]
		{
			get { return array[v.index]; }
			set { array[v.index] = value; }
		}

		/// <summary>
		/// Accesses the vertex attribute value in the array corresponding to the vertex of the given edge.
		/// </summary>
		/// <param name="e">The edge whose vertex's attribute value is to be accessed.</param>
		/// <returns>The vertex attribute value corresponding to the vertex of the given edge.</returns>
		public T this[Topology.HalfEdge e]
		{
			get { return array[e.vertex.index]; }
			set { array[e.vertex.index] = value; }
		}

		/// <summary>
		/// Accesses the vertex attribute value in the array corresponding to the vertex of the given edge.
		/// </summary>
		/// <param name="e">The edge whose vertex's attribute value is to be accessed.</param>
		/// <returns>The vertex attribute value corresponding to the vertex of the given edge.</returns>
		public T this[Topology.VertexEdge e]
		{
			get { return array[e.vertex.index]; }
			set { array[e.vertex.index] = value; }
		}

		/// <summary>
		/// Accesses the vertex attribute value in the array corresponding to the vertex of the given edge.
		/// </summary>
		/// <param name="e">The edge whose vertex's attribute value is to be accessed.</param>
		/// <returns>The vertex attribute value corresponding to the vertex of the given edge.</returns>
		public T this[Topology.FaceEdge e]
		{
			get { return array[e.vertex.index]; }
			set { array[e.vertex.index] = value; }
		}

		/// <summary>
		/// The number of vertex attribute values stored in the wrapped array.
		/// </summary>
		public int Count { get { return array.Length; } }

		/// <summary>
		/// Indicates if the collection is read-only and thus cannot have items inserted into or removed from it.  Always true.
		/// </summary>
		public bool IsReadOnly { get { return true; } }

		/// <summary>
		/// Not supported.
		/// </summary>
		/// <inheritdoc/>
		public void Add(T item) { throw new NotSupportedException(); }

		/// <summary>
		/// Not supported.
		/// </summary>
		/// <inheritdoc/>
		public void Clear() { throw new NotSupportedException(); }

		/// <summary>
		/// Checks whether the given vertex attribute value is contained within the wrapped array.
		/// </summary>
		/// <param name="item">The vertex attribute value to be searched for.</param>
		/// <returns>True if the given vertex attribute value is stored within the wrapped array, and false if it is not.</returns>
		public bool Contains(T item) { return ((IList<T>)array).Contains(item); }

		/// <summary>
		/// Copies the vertex attribute values of the wrapped array into the specified array.
		/// </summary>
		/// <param name="array">The target array into which vertex attribute values will be copied.</param>
		/// <param name="arrayIndex">The starting index at which the copy of values will begin.</param>
		public void CopyTo(T[] array, int arrayIndex) { this.array.CopyTo(array, arrayIndex); }

		/// <summary>
		/// Gets an enumerator over all the vertex attribute values in the wrapped array.
		/// </summary>
		/// <returns>An enumerator over all the vertex attribute values in the wrapped array.</returns>
		public IEnumerator<T> GetEnumerator() { return ((IList<T>)array).GetEnumerator(); }

		/// <summary>
		/// Searches for the specified vertex attribute value and returns the index at which the first occurrence is found.
		/// </summary>
		/// <param name="item">The vertex attribute value to be searched for.</param>
		/// <returns>The index of the first occurrence of the given value, or -1 if no occurrence was found.</returns>
		public int IndexOf(T item) { return ((IList<T>)array).IndexOf(item); }

		/// <summary>
		/// Not supported.
		/// </summary>
		/// <inheritdoc/>
		public void Insert(int index, T item) { throw new NotSupportedException(); }

		/// <summary>
		/// Not supported.
		/// </summary>
		/// <inheritdoc/>
		public bool Remove(T item) { throw new NotSupportedException(); }

		/// <summary>
		/// Not supported.
		/// </summary>
		/// <inheritdoc/>
		public void RemoveAt(int index) { throw new NotSupportedException(); }

		/// <summary>
		/// Gets an enumerator over all the vertex attribute values in the wrapped array.
		/// </summary>
		/// <returns>An enumerator over all the vertex attribute values in the wrapped array.</returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <summary>
		/// Lookup the attribute value for the vertex indicated.
		/// </summary>
		/// <param name="i">The index of the vertex whose attribute value is desired.</param>
		/// <returns>The attribute value for the vertex indicated.</returns>
		T IVertexAttribute<T>.this[int i]
		{
			get { return this[i]; }
			set { this[i] = value; }
		}

		/// <summary>
		/// Lookup the attribute value for the edge indicated.
		/// </summary>
		/// <param name="i">The index of the edge whose attribute value is desired.</param>
		/// <returns>The attribute value for the edge indicated.</returns>
		/// <remarks><para>Any code utilizing this indexer through the <c>IEdgeAttribute&lt;T&gt;</c> interface is likely to
		/// be treating the index as an edge index, but without access to the full edge data, nothing useful can be done with
		/// just an edge index.  Therefore, this indexer throws in order to identify any code that attempts to misuse a
		/// vertex attribute in this way.</para></remarks>
		T IEdgeAttribute<T>.this[int i]
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
	}

	/// <summary>
	/// Abstract base class for vertex attributes that also derives from <see cref="ScriptableObject"/>
	/// and can therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	/// <typeparam name="T">The type of the attribute values.</typeparam>
	public abstract class VertexAttribute<T> : ScriptableObject, IVertexAttribute<T>
	{
		/// <inheritdoc/>
		public abstract T this[int i] { get; set; }
		/// <inheritdoc/>
		public abstract T this[Topology.Vertex v] { get; set; }
		/// <inheritdoc/>
		public abstract T this[Topology.HalfEdge e] { get; set; }
		/// <inheritdoc/>
		public abstract T this[Topology.VertexEdge e] { get; set; }
		/// <inheritdoc/>
		public abstract T this[Topology.FaceEdge e] { get; set; }

		/// <inheritdoc/>
		public virtual int Count { get { throw new NotSupportedException(); } }
		/// <inheritdoc/>
		public virtual bool IsReadOnly { get { return true; } }
		/// <inheritdoc/>
		public virtual void Add(T item) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public virtual void Clear() { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public virtual bool Contains(T item) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public virtual void CopyTo(T[] array, int arrayIndex) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public virtual IEnumerator<T> GetEnumerator() { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public virtual int IndexOf(T item) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public virtual void Insert(int index, T item) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public virtual bool Remove(T item) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public virtual void RemoveAt(int index) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <summary>
		/// Lookup the attribute value for the vertex indicated.
		/// </summary>
		/// <param name="i">The index of the vertex whose attribute value is desired.</param>
		/// <returns>The attribute value for the vertex indicated.</returns>
		T IVertexAttribute<T>.this[int i]
		{
			get { return this[i]; }
			set { this[i] = value; }
		}

		/// <summary>
		/// Lookup the attribute value for the edge indicated.
		/// </summary>
		/// <param name="i">The index of the edge whose attribute value is desired.</param>
		/// <returns>The attribute value for the edge indicated.</returns>
		/// <remarks><para>Any code utilizing this indexer through the <c>IEdgeAttribute&lt;T&gt;</c> interface is likely to
		/// be treating the index as an edge index, but without access to the full edge data, nothing useful can be done with
		/// just an edge index.  Therefore, this indexer throws in order to identify any code that attempts to misuse a
		/// vertex attribute in this way.</para></remarks>
		T IEdgeAttribute<T>.this[int i]
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
	}

	/// <summary>
	/// A simple low-storage vertex attribute class for situations when all vertices have the exact same value for a particular attribute.
	/// </summary>
	/// <typeparam name="T">The type of the constant attribute value.</typeparam>
	public class VertexConstantAttribute<T> : VertexAttribute<T> where T : new()
	{
		/// <summary>
		/// The attribute value shared by all vertices.
		/// </summary>
		public T constant;

		/// <summary>
		/// Creates an instance of a class derived from this one, and assigns it the constant value.
		/// </summary>
		/// <typeparam name="TDerived">The type of the derived class.</typeparam>
		/// <param name="constant">The constant attribute value shared by all vertices.</param>
		/// <returns>An instance of the derived class using the constant value provided.</returns>
		/// <remarks><para>This is a convenience function for derived classes to use, which are needed
		/// because Unity is unable to serialize generic classes directly, but can serialize non-generic
		/// classes which are derived from generic classes but given a concrete type.</para></remarks>
		protected static TDerived CreateDerived<TDerived>(T constant) where TDerived : VertexConstantAttribute<T>
		{
			var instance = CreateInstance<TDerived>();
			instance.constant = constant;
			return instance;
		}

		/// <inheritdoc/>
		public override T this[int i]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant vertex attribute cannot be changed."); }
		}

		/// <inheritdoc/>
		public override T this[Topology.Vertex v]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant vertex attribute cannot be changed."); }
		}

		/// <inheritdoc/>
		public override T this[Topology.HalfEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant vertex attribute cannot be changed."); }
		}

		/// <inheritdoc/>
		public override T this[Topology.VertexEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant vertex attribute cannot be changed."); }
		}

		/// <inheritdoc/>
		public override T this[Topology.FaceEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant vertex attribute cannot be changed."); }
		}
	}

	/// <summary>
	/// A vertex attribute wrapper around a raw array of vertex attributes, which also derives from
	/// <see cref="ScriptableObject"/> and can therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	/// <typeparam name="T">The type of the attribute values.</typeparam>
	/// <seealso cref="VertexAttributeArrayWrapper{T}"/>
	public class VertexArrayAttribute<T> : VertexAttribute<T> where T : new()
	{
		/// <summary>
		/// The array of vertex attribute values that is being wrapped.
		/// </summary>
		public T[] array;

		/// <summary>
		/// Creates an instance of a class derived from this one, and assigns it the array data provided.
		/// </summary>
		/// <typeparam name="TDerived">The type of the derived class.</typeparam>
		/// <param name="array">The array of vertex attribute values to be wrapped.</param>
		/// <returns>An instance of the derived class using the array data provided.</returns>
		/// <remarks><para>This is a convenience function for derived classes to use, which are needed
		/// because Unity is unable to serialize generic classes directly, but can serialize non-generic
		/// classes which are derived from generic classes but given a concrete type.</para></remarks>
		protected static TDerived CreateDerived<TDerived>(T[] array) where TDerived : VertexArrayAttribute<T>
		{
			var attribute = CreateInstance<TDerived>();
			attribute.array = array;
			return attribute;
		}

		/// <summary>
		/// Creates an instance of a class derived from this one, and creates an array of the specified size.
		/// </summary>
		/// <typeparam name="TDerived">The type of the derived class.</typeparam>
		/// <param name="vertexCount">The number of elements that the created array will contain.</param>
		/// <returns>An instance of the derived class using an array created with the specified size.</returns>
		/// <remarks><para>This is a convenience function for derived classes to use, which are needed
		/// because Unity is unable to serialize generic classes directly, but can serialize non-generic
		/// classes which are derived from generic classes but given a concrete type.</para></remarks>
		protected static TDerived CreateDerived<TDerived>(int vertexCount) where TDerived : VertexArrayAttribute<T>
		{
			return CreateDerived<TDerived>(new T[vertexCount]);
		}

		/// <inheritdoc/>
		public override T this[int i]
		{
			get { return array[i]; }
			set { array[i] = value; }
		}

		/// <inheritdoc/>
		public override T this[Topology.Vertex v]
		{
			get { return array[v.index]; }
			set { array[v.index] = value; }
		}

		/// <inheritdoc/>
		public override T this[Topology.HalfEdge e]
		{
			get { return array[e.vertex.index]; }
			set { array[e.vertex.index] = value; }
		}

		/// <inheritdoc/>
		public override T this[Topology.VertexEdge e]
		{
			get { return array[e.vertex.index]; }
			set { array[e.vertex.index] = value; }
		}

		/// <inheritdoc/>
		public override T this[Topology.FaceEdge e]
		{
			get { return array[e.vertex.index]; }
			set { array[e.vertex.index] = value; }
		}

		/// <inheritdoc/>
		public override int Count { get { return array.Length; } }
		/// <inheritdoc/>
		public override bool Contains(T item) { return ((IList<T>)array).Contains(item); }
		/// <inheritdoc/>
		public override void CopyTo(T[] array, int arrayIndex) { this.array.CopyTo(array, arrayIndex); }
		/// <inheritdoc/>
		public override IEnumerator<T> GetEnumerator() { return ((IList<T>)array).GetEnumerator(); }
		/// <inheritdoc/>
		public override int IndexOf(T item) { return ((IList<T>)array).IndexOf(item); }
	}

	/// <summary>
	/// Extension methods involving vertices and vertex attributes.
	/// </summary>
	public static class VertexExtensions
	{
		/// <summary>
		/// Creates a vertex attribute wrapper around an array.
		/// </summary>
		/// <typeparam name="T">The type of elements in the array.</typeparam>
		/// <param name="array">The array to be wrapped.</param>
		/// <returns>A vertex attribute wrapper around the array.</returns>
		public static VertexAttributeArrayWrapper<T> AsVertexAttribute<T>(this T[] array)
		{
			return new VertexAttributeArrayWrapper<T>(array);
		}
	}
}
#endif