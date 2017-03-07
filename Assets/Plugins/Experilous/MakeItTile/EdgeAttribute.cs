/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System;
using System.Collections.Generic;
using Experilous.Topologies;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// A simple low-storage edge attribute class for situations when all edges have the exact same value for a particular attribute.
	/// </summary>
	/// <typeparam name="T">The type of the constant attribute value.</typeparam>
	public struct EdgeAttributeConstantWrapper<T> : IEdgeAttribute<T>
	{
		/// <summary>
		/// The attribute value shared by all edges.
		/// </summary>
		public T constant;

		/// <summary>
		/// Constructs an edge attribute constant wrapper with the given constant value.
		/// </summary>
		/// <param name="constant">The constant attribute value shared by all edges.</param>
		public EdgeAttributeConstantWrapper(T constant)
		{
			this.constant = constant;
		}

		/// <inheritdoc/>
		public T this[int i]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant edge attribute cannot be changed."); }
		}

		/// <inheritdoc/>
		public T this[Topology.HalfEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant edge attribute cannot be changed."); }
		}

		/// <inheritdoc/>
		public T this[Topology.VertexEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant edge attribute cannot be changed."); }
		}

		/// <inheritdoc/>
		public T this[Topology.FaceEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant edge attribute cannot be changed."); }
		}

		/// <inheritdoc/>
		public int Count { get { throw new NotSupportedException(); } }
		/// <inheritdoc/>
		public bool IsReadOnly { get { return true; } }
		/// <inheritdoc/>
		public void Add(T item) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public void Clear() { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public bool Contains(T item) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public void CopyTo(T[] array, int arrayIndex) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public IEnumerator<T> GetEnumerator() { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public int IndexOf(T item) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public void Insert(int index, T item) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public bool Remove(T item) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		public void RemoveAt(int index) { throw new NotSupportedException(); }
		/// <inheritdoc/>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	/// <summary>
	/// An edge attribute wrapper around a raw array of edge attributes.
	/// </summary>
	/// <typeparam name="T">The type of the attribute values.</typeparam>
	public struct EdgeAttributeArrayWrapper<T> : IEdgeAttribute<T>
	{
		/// <summary>
		/// The array of edge attribute values that is being wrapped.
		/// </summary>
		public readonly T[] array;

		/// <summary>
		/// Constructs an edge attribute array wrapper around the given array.
		/// </summary>
		/// <param name="array">The array of edge attribute values to be wrapped.</param>
		public EdgeAttributeArrayWrapper(T[] array)
		{
			this.array = array;
		}

		/// <summary>
		/// Constructs an edge attribute array wrapper around an internally created array with the given number of elements.
		/// </summary>
		/// <param name="elementCount">The number of elements that the created array will contain.</param>
		public EdgeAttributeArrayWrapper(int elementCount)
		{
			array = new T[elementCount];
		}

		/// <summary>
		/// Implicitly converts an array wrapper to a raw array by simply returning the wrapped array.
		/// </summary>
		/// <param name="arrayWrapper">The array wrapper to convert.</param>
		/// <returns>The raw array wrapped by the converted array wrapper.</returns>
		public static implicit operator T[](EdgeAttributeArrayWrapper<T> arrayWrapper)
		{
			return arrayWrapper.array;
		}

		/// <summary>
		/// Implicitly converts a raw array to an array wrapper by constructing a wrapper around the array.
		/// </summary>
		/// <param name="array">The array to be converted.</param>
		/// <returns>An array wrapper around the converted array.</returns>
		public static implicit operator EdgeAttributeArrayWrapper<T>(T[] array)
		{
			return new EdgeAttributeArrayWrapper<T>(array);
		}

		/// <summary>
		/// Accesses the edge attribute value in the array corresponding to the edge with the given index.
		/// </summary>
		/// <param name="i">The index of the edge whose attribute value is to be accessed.</param>
		/// <returns>The edge attribute value corresponding to the edge with the given index.</returns>
		public T this[int i]
		{
			get { return array[i]; }
			set { array[i] = value; }
		}

		/// <summary>
		/// Accesses the edge attribute value in the array corresponding to the given edge.
		/// </summary>
		/// <param name="e">The edge whose attribute value is to be accessed.</param>
		/// <returns>The edge attribute value corresponding to the given edge.</returns>
		public T this[Topology.HalfEdge e]
		{
			get { return array[e.index]; }
			set { array[e.index] = value; }
		}

		/// <summary>
		/// Accesses the edge attribute value in the array corresponding to the given edge.
		/// </summary>
		/// <param name="e">The edge whose attribute value is to be accessed.</param>
		/// <returns>The edge attribute value corresponding to the given edge.</returns>
		public T this[Topology.VertexEdge e]
		{
			get { return array[e.index]; }
			set { array[e.index] = value; }
		}

		/// <summary>
		/// Accesses the edge attribute value in the array corresponding to the given edge.
		/// </summary>
		/// <param name="e">The edge whose attribute value is to be accessed.</param>
		/// <returns>The edge attribute value corresponding to the given edge.</returns>
		public T this[Topology.FaceEdge e]
		{
			get { return array[e.index]; }
			set { array[e.index] = value; }
		}

		/// <summary>
		/// The number of edge attribute values stored in the wrapped array.
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
		/// Checks whether the given edge attribute value is contained within the wrapped array.
		/// </summary>
		/// <param name="item">The edge attribute value to be searched for.</param>
		/// <returns>True if the given edge attribute value is stored within the wrapped array, and false if it is not.</returns>
		public bool Contains(T item) { return ((IList<T>)array).Contains(item); }

		/// <summary>
		/// Copies the edge attribute values of the wrapped array into the specified array.
		/// </summary>
		/// <param name="array">The target array into which edge attribute values will be copied.</param>
		/// <param name="arrayIndex">The starting index at which the copy of values will begin.</param>
		public void CopyTo(T[] array, int arrayIndex) { this.array.CopyTo(array, arrayIndex); }

		/// <summary>
		/// Gets an enumerator over all the edge attribute values in the wrapped array.
		/// </summary>
		/// <returns>An enumerator over all the edge attribute values in the wrapped array.</returns>
		public IEnumerator<T> GetEnumerator() { return ((IList<T>)array).GetEnumerator(); }

		/// <summary>
		/// Searches for the specified edge attribute value and returns the index at which the first occurrence is found.
		/// </summary>
		/// <param name="item">The edge attribute value to be searched for.</param>
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
		/// Gets an enumerator over all the edge attribute values in the wrapped array.
		/// </summary>
		/// <returns>An enumerator over all the edge attribute values in the wrapped array.</returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	/// <summary>
	/// An edge attribute wrapper around another edge attribute collection, which accesses.
	/// the edge attributes of every edge's twin, instead of the edge's own attribute value.
	/// </summary>
	/// <typeparam name="T">The type of the attribute values.</typeparam>
	public struct TwinEdgeAttributeWrapper<T> : IEdgeAttribute<T>
	{
		/// <summary>
		/// The topology to which the edges belong.
		/// </summary>
		public Topology topology;

		/// <summary>
		/// The underlying edge attributes collection that is being wrapped.
		/// </summary>
		public IEdgeAttribute<T> underlyingAttribute;

		/// <summary>
		/// Constructs a twin edge attribute wrapper around the given underlying attribute collection.
		/// </summary>
		/// <param name="topology">The topology to which the edges belong.</param>
		/// <param name="underlyingAttribute">The underlying edge attributes to be wrapped.</param>
		public TwinEdgeAttributeWrapper(Topology topology, IEdgeAttribute<T> underlyingAttribute)
		{
			this.topology = topology;
			this.underlyingAttribute = underlyingAttribute;
		}

		/// <summary>
		/// Accesses the edge attribute value in the underling edge attributes collection corresponding to the twin of the edge with the given index.
		/// </summary>
		/// <param name="i">The index of the edge whose twin's attribute value is to be accessed.</param>
		/// <returns>The edge attribute value corresponding to the twin of the edge with the given index.</returns>
		public T this[int i]
		{
			get { return underlyingAttribute[topology.halfEdges[i].twinIndex]; }
			set { underlyingAttribute[topology.halfEdges[i].twinIndex] = value; }
		}

		/// <summary>
		/// Accesses the edge attribute value in the underling edge attributes collection corresponding to the given edge's twin.
		/// </summary>
		/// <param name="e">The edge whose twin's attribute value is to be accessed.</param>
		/// <returns>The underlying edge attribute value corresponding to the given edge's twin.</returns>
		public T this[Topology.HalfEdge e]
		{
			get { return underlyingAttribute[e.twin]; }
			set { underlyingAttribute[e.twin] = value; }
		}

		/// <summary>
		/// Accesses the edge attribute value in the underling edge attributes collection corresponding to the given edge's twin.
		/// </summary>
		/// <param name="e">The edge whose twin's attribute value is to be accessed.</param>
		/// <returns>The underlying edge attribute value corresponding to the given edge's twin.</returns>
		public T this[Topology.VertexEdge e]
		{
			get { return underlyingAttribute[e.twin]; }
			set { underlyingAttribute[e.twin] = value; }
		}

		/// <summary>
		/// Accesses the edge attribute value in the underling edge attributes collection corresponding to the given edge's twin.
		/// </summary>
		/// <param name="e">The edge whose twin's attribute value is to be accessed.</param>
		/// <returns>The underlying edge attribute value corresponding to the given edge's twin.</returns>
		public T this[Topology.FaceEdge e]
		{
			get { return underlyingAttribute[e.twin]; }
			set { underlyingAttribute[e.twin] = value; }
		}

		/// <summary>
		/// The number of edge attribute values stored in the wrapped attributes collection.
		/// </summary>
		public int Count { get { return underlyingAttribute.Count; } }

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
		/// Checks whether the given edge attribute value is contained within the wrapped attributes collection.
		/// </summary>
		/// <param name="item">The edge attribute value to be searched for.</param>
		/// <returns>True if the given edge attribute value is stored within the wrapped attributes collection, and false if it is not.</returns>
		public bool Contains(T item) { return underlyingAttribute.Contains(item); }

		/// <summary>
		/// Copies the edge attribute values of the wrapped attributes collection into the specified array.
		/// </summary>
		/// <param name="array">The target array into which edge attribute values will be copied.</param>
		/// <param name="arrayIndex">The starting index at which the copy of values will begin.</param>
		public void CopyTo(T[] array, int arrayIndex) { underlyingAttribute.CopyTo(array, arrayIndex); }

		/// <summary>
		/// Gets an enumerator over all the edge attribute values in the wrapped attributes collection.
		/// </summary>
		/// <returns>An enumerator over all the edge attribute values in the wrapped attributes collection.</returns>
		public IEnumerator<T> GetEnumerator() { return underlyingAttribute.GetEnumerator(); }

		/// <summary>
		/// Searches for the specified edge attribute value and returns the index at which the first occurrence is found.
		/// </summary>
		/// <param name="item">The edge attribute value to be searched for.</param>
		/// <returns>The index of the first occurrence of the given value, or -1 if no occurrence was found.</returns>
		public int IndexOf(T item) { return underlyingAttribute.IndexOf(item); }

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
		/// Gets an enumerator over all the edge attribute values in the wrapped attributes collection.
		/// </summary>
		/// <returns>An enumerator over all the edge attribute values in the wrapped attributes collection.</returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	/// <summary>
	/// Abstract base class for edge attributes that also derives from <see cref="ScriptableObject"/>
	/// and can therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	/// <typeparam name="T">The type of the attribute values.</typeparam>
	public abstract class EdgeAttribute<T> : ScriptableObject, IEdgeAttribute<T>
	{
		/// <inheritdoc/>
		public abstract T this[int i] { get; set; }
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
	}

	/// <summary>
	/// A simple low-storage edge attribute class for situations when all edges have the exact same value for a particular attribute.
	/// </summary>
	/// <typeparam name="T">The type of the constant attribute value.</typeparam>
	public class EdgeConstantAttribute<T> : EdgeAttribute<T> where T : new()
	{
		/// <summary>
		/// The attribute value shared by all edges.
		/// </summary>
		public T constant;

		/// <summary>
		/// Creates an instance of a class derived from this one, and assigns it the constant value.
		/// </summary>
		/// <typeparam name="TDerived">The type of the derived class.</typeparam>
		/// <param name="constant">The constant attribute value shared by all edges.</param>
		/// <returns>An instance of the derived class using the constant value provided.</returns>
		/// <remarks><para>This is a convenience function for derived classes to use, which are needed
		/// because Unity is unable to serialize generic classes directly, but can serialize non-generic
		/// classes which are derived from generic classes but given a concrete type.</para></remarks>
		protected static TDerived CreateDerived<TDerived>(T constant) where TDerived : EdgeConstantAttribute<T>
		{
			var instance = CreateInstance<TDerived>();
			instance.constant = constant;
			return instance;
		}

		/// <inheritdoc/>
		public override T this[int i]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant edge attribute cannot be changed."); }
		}

		/// <inheritdoc/>
		public override T this[Topology.HalfEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant edge attribute cannot be changed."); }
		}

		/// <inheritdoc/>
		public override T this[Topology.VertexEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant edge attribute cannot be changed."); }
		}

		/// <inheritdoc/>
		public override T this[Topology.FaceEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant edge attribute cannot be changed."); }
		}
	}

	/// <summary>
	/// An edge attribute wrapper around a raw array of edge attributes, which also derives from
	/// <see cref="ScriptableObject"/> and can therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	/// <typeparam name="T">The type of the attribute values.</typeparam>
	/// <seealso cref="EdgeAttributeArrayWrapper{T}"/>
	public class EdgeArrayAttribute<T> : EdgeAttribute<T> where T : new()
	{
		/// <summary>
		/// The array of edge attribute values that is being wrapped.
		/// </summary>
		public T[] array;

		/// <summary>
		/// Creates an instance of a class derived from this one, and assigns it the array data provided.
		/// </summary>
		/// <typeparam name="TDerived">The type of the derived class.</typeparam>
		/// <param name="array">The array of edge attribute values to be wrapped.</param>
		/// <returns>An instance of the derived class using the array data provided.</returns>
		/// <remarks><para>This is a convenience function for derived classes to use, which are needed
		/// because Unity is unable to serialize generic classes directly, but can serialize non-generic
		/// classes which are derived from generic classes but given a concrete type.</para></remarks>
		protected static TDerived CreateDerived<TDerived>(T[] array) where TDerived : EdgeArrayAttribute<T>
		{
			var instance = CreateInstance<TDerived>();
			instance.array = array;
			return instance;
		}

		/// <summary>
		/// Creates an instance of a class derived from this one, and creates an array of the specified size.
		/// </summary>
		/// <typeparam name="TDerived">The type of the derived class.</typeparam>
		/// <param name="edgeCount">The number of elements that the created array will contain.</param>
		/// <returns>An instance of the derived class using an array created with the specified size.</returns>
		/// <remarks><para>This is a convenience function for derived classes to use, which are needed
		/// because Unity is unable to serialize generic classes directly, but can serialize non-generic
		/// classes which are derived from generic classes but given a concrete type.</para></remarks>
		protected static TDerived CreateDerived<TDerived>(int edgeCount) where TDerived : EdgeArrayAttribute<T>
		{
			return CreateDerived<TDerived>(new T[edgeCount]);
		}

		/// <inheritdoc/>
		public override T this[int i]
		{
			get { return array[i]; }
			set { array[i] = value; }
		}

		/// <inheritdoc/>
		public override T this[Topology.HalfEdge e]
		{
			get { return array[e.index]; }
			set { array[e.index] = value; }
		}

		/// <inheritdoc/>
		public override T this[Topology.VertexEdge e]
		{
			get { return array[e.index]; }
			set { array[e.index] = value; }
		}

		/// <inheritdoc/>
		public override T this[Topology.FaceEdge e]
		{
			get { return array[e.index]; }
			set { array[e.index] = value; }
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
	/// Extension methods involving edges and edge attributes.
	/// </summary>
	public static class EdgeExtensions
	{
		/// <summary>
		/// Creates an edgeattribute wrapper around an array.
		/// </summary>
		/// <typeparam name="T">The type of elements in the array.</typeparam>
		/// <param name="array">The array to be wrapped.</param>
		/// <returns>An edge attribute wrapper around the array.</returns>
		public static EdgeAttributeArrayWrapper<T> AsEdgeAttribute<T>(this T[] array)
		{
			return new EdgeAttributeArrayWrapper<T>(array);
		}
	}
}
