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
	/// <summary>
	/// A face attribute wrapper around a raw array of face attributes.
	/// </summary>
	/// <typeparam name="T">The type of the attribute values.</typeparam>
	public struct FaceAttributeArrayWrapper<T> : IFaceAttribute<T>
	{
		/// <summary>
		/// The array of face attribute values that is being wrapped.
		/// </summary>
		public readonly T[] array;

		/// <summary>
		/// Constructs a face attribute array wrapper around the given array.
		/// </summary>
		/// <param name="array">The array of face attribute values to be wrapped.</param>
		public FaceAttributeArrayWrapper(T[] array)
		{
			this.array = array;
		}

		/// <summary>
		/// Constructs a face attribute array wrapper around an internally created array with the given number of elements.
		/// </summary>
		/// <param name="elementCount">The number of elements that the created array will contain.</param>
		public FaceAttributeArrayWrapper(int elementCount)
		{
			array = new T[elementCount];
		}

		/// <summary>
		/// Implicitly converts an array wrapper to a raw array by simply returning the wrapped array.
		/// </summary>
		/// <param name="arrayWrapper">The array wrapper to convert.</param>
		/// <returns>The raw array wrapped by the converted array wrapper.</returns>
		public static implicit operator T[](FaceAttributeArrayWrapper<T> arrayWrapper)
		{
			return arrayWrapper.array;
		}

		/// <summary>
		/// Implicitly converts a raw array to an array wrapper by constructing a wrapper around the array.
		/// </summary>
		/// <param name="array">The array to be converted.</param>
		/// <returns>An array wrapper around the converted array.</returns>
		public static implicit operator FaceAttributeArrayWrapper<T>(T[] array)
		{
			return new FaceAttributeArrayWrapper<T>(array);
		}

		/// <summary>
		/// Accesses the face attribute value in the array corresponding to the face with the given index.
		/// </summary>
		/// <param name="i">The index of the face whose attribute value is to be accessed.</param>
		/// <returns>The face attribute value corresponding to the face with the given index.</returns>
		public T this[int i]
		{
			get { return array[i]; }
			set { array[i] = value; }
		}

		/// <summary>
		/// Accesses the face attribute value in the array corresponding to the given face.
		/// </summary>
		/// <param name="f">The face whose attribute value is to be accessed.</param>
		/// <returns>The face attribute value corresponding to the given face.</returns>
		public T this[Topology.Face f]
		{
			get { return array[f.index]; }
			set { array[f.index] = value; }
		}

		/// <summary>
		/// Accesses the face attribute value in the array corresponding to the face of the given edge.
		/// </summary>
		/// <param name="e">The edge whose face's attribute value is to be accessed.</param>
		/// <returns>The face attribute value corresponding to the face of the given edge.</returns>
		public T this[Topology.HalfEdge e]
		{
			get { return array[e.face.index]; }
			set { array[e.face.index] = value; }
		}

		/// <summary>
		/// Accesses the face attribute value in the array corresponding to the face of the given edge.
		/// </summary>
		/// <param name="e">The edge whose face's attribute value is to be accessed.</param>
		/// <returns>The face attribute value corresponding to the face of the given edge.</returns>
		public T this[Topology.VertexEdge e]
		{
			get { return array[e.face.index]; }
			set { array[e.face.index] = value; }
		}

		/// <summary>
		/// Accesses the face attribute value in the array corresponding to the face of the given edge.
		/// </summary>
		/// <param name="e">The edge whose face's attribute value is to be accessed.</param>
		/// <returns>The vertex attribute value corresponding to the face of the given edge.</returns>
		public T this[Topology.FaceEdge e]
		{
			get { return array[e.face.index]; }
			set { array[e.face.index] = value; }
		}

		/// <summary>
		/// The number of face attribute values stored in the wrapped array.
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
		/// Checks whether the given face attribute value is contained within the wrapped array.
		/// </summary>
		/// <param name="item">The face attribute value to be searched for.</param>
		/// <returns>True if the given face attribute value is stored within the wrapped array, and false if it is not.</returns>
		public bool Contains(T item) { return ((IList<T>)array).Contains(item); }

		/// <summary>
		/// Copies the face attribute values of the wrapped array into the specified array.
		/// </summary>
		/// <param name="array">The target array into which face attribute values will be copied.</param>
		/// <param name="arrayIndex">The starting index at which the copy of values will begin.</param>
		public void CopyTo(T[] array, int arrayIndex) { this.array.CopyTo(array, arrayIndex); }

		/// <summary>
		/// Gets an enumerator over all the face attribute values in the wrapped array.
		/// </summary>
		/// <returns>An enumerator over all the face attribute values in the wrapped array.</returns>
		public IEnumerator<T> GetEnumerator() { return ((IList<T>)array).GetEnumerator(); }

		/// <summary>
		/// Searches for the specified face attribute value and returns the index at which the first occurrence is found.
		/// </summary>
		/// <param name="item">The face attribute value to be searched for.</param>
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
		/// Gets an enumerator over all the face attribute values in the wrapped array.
		/// </summary>
		/// <returns>An enumerator over all the face attribute values in the wrapped array.</returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <summary>
		/// Lookup the attribute value for the face indicated.
		/// </summary>
		/// <param name="i">The index of the face whose attribute value is desired.</param>
		/// <returns>The attribute value for the face indicated.</returns>
		T IFaceAttribute<T>.this[int i]
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
		/// face attribute in this way.</para></remarks>
		T IEdgeAttribute<T>.this[int i]
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
	}

	/// <summary>
	/// Abstract base class for face attributes that also derives from <see cref="ScriptableObject"/>
	/// and can therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	/// <typeparam name="T">The type of the attribute values.</typeparam>
	public abstract class FaceAttribute<T> : ScriptableObject, IFaceAttribute<T>
	{
		/// <inheritdoc/>
		public abstract T this[int i] { get; set; }
		/// <inheritdoc/>
		public abstract T this[Topology.Face f] { get; set; }
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
		/// Lookup the attribute value for the face indicated.
		/// </summary>
		/// <param name="i">The index of the face whose attribute value is desired.</param>
		/// <returns>The attribute value for the face indicated.</returns>
		T IFaceAttribute<T>.this[int i]
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
		/// face attribute in this way.</para></remarks>
		T IEdgeAttribute<T>.this[int i]
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
	}

	/// <summary>
	/// A simple low-storage vertex attribute class for situations when all faces have the exact same value for a particular attribute.
	/// </summary>
	/// <typeparam name="T">The type of the constant attribute value.</typeparam>
	public class FaceConstantAttribute<T> : FaceAttribute<T> where T : new()
	{
		/// <summary>
		/// The attribute value shared by all faces.
		/// </summary>
		public T constant;

		/// <summary>
		/// Creates an instance of a class derived from this one, and assigns it the constant value.
		/// </summary>
		/// <typeparam name="TDerived">The type of the derived class.</typeparam>
		/// <param name="constant">The constant attribute value shared by all faces.</param>
		/// <returns>An instance of the derived class using the constant value provided.</returns>
		/// <remarks><para>This is a convenience function for derived classes to use, which are needed
		/// because Unity is unable to serialize generic classes directly, but can serialize non-generic
		/// classes which are derived from generic classes but given a concrete type.</para></remarks>
		protected static TDerived CreateDerived<TDerived>(T constant) where TDerived : FaceConstantAttribute<T>
		{
			var instance = CreateInstance<TDerived>();
			instance.constant = constant;
			return instance;
		}

		/// <inheritdoc/>
		public override T this[int i]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant face attribute cannot be changed."); }
		}

		/// <inheritdoc/>
		public override T this[Topology.Face f]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant face attribute cannot be changed."); }
		}

		/// <inheritdoc/>
		public override T this[Topology.HalfEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant face attribute cannot be changed."); }
		}

		/// <inheritdoc/>
		public override T this[Topology.VertexEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant face attribute cannot be changed."); }
		}

		/// <inheritdoc/>
		public override T this[Topology.FaceEdge e]
		{
			get { return constant; }
			set { throw new NotSupportedException("Values of a constant face attribute cannot be changed."); }
		}
	}

	/// <summary>
	/// A face attribute wrapper around a raw array of face attributes, which also derives from
	/// <see cref="ScriptableObject"/> and can therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	/// <typeparam name="T">The type of the attribute values.</typeparam>
	/// <seealso cref="FaceAttributeArrayWrapper{T}"/>
	public class FaceArrayAttribute<T> : FaceAttribute<T> where T : new()
	{
		/// <summary>
		/// The array of face attribute values that is being wrapped.
		/// </summary>
		public T[] array;

		/// <summary>
		/// Creates an instance of a class derived from this one, and assigns it the array data provided.
		/// </summary>
		/// <typeparam name="TDerived">The type of the derived class.</typeparam>
		/// <param name="array">The array of face attribute values to be wrapped.</param>
		/// <returns>An instance of the derived class using the array data provided.</returns>
		/// <remarks><para>This is a convenience function for derived classes to use, which are needed
		/// because Unity is unable to serialize generic classes directly, but can serialize non-generic
		/// classes which are derived from generic classes but given a concrete type.</para></remarks>
		protected static TDerived CreateDerived<TDerived>(T[] array) where TDerived : FaceArrayAttribute<T>
		{
			var instance = CreateInstance<TDerived>();
			instance.array = array;
			return instance;
		}

		/// <summary>
		/// Creates an instance of a class derived from this one, and creates an array of the specified size.
		/// </summary>
		/// <typeparam name="TDerived">The type of the derived class.</typeparam>
		/// <param name="faceCount">The number of elements that the created array will contain.</param>
		/// <returns>An instance of the derived class using an array created with the specified size.</returns>
		/// <remarks><para>This is a convenience function for derived classes to use, which are needed
		/// because Unity is unable to serialize generic classes directly, but can serialize non-generic
		/// classes which are derived from generic classes but given a concrete type.</para></remarks>
		protected static TDerived CreateDerived<TDerived>(int faceCount) where TDerived : FaceArrayAttribute<T>
		{
			return CreateDerived<TDerived>(new T[faceCount]);
		}

		/// <inheritdoc/>
		public override T this[int i]
		{
			get { return array[i]; }
			set { array[i] = value; }
		}

		/// <inheritdoc/>
		public override T this[Topology.Face f]
		{
			get { return array[f.index]; }
			set { array[f.index] = value; }
		}

		/// <inheritdoc/>
		public override T this[Topology.HalfEdge e]
		{
			get { return array[e.face.index]; }
			set { array[e.face.index] = value; }
		}

		/// <inheritdoc/>
		public override T this[Topology.VertexEdge e]
		{
			get { return array[e.face.index]; }
			set { array[e.face.index] = value; }
		}

		/// <inheritdoc/>
		public override T this[Topology.FaceEdge e]
		{
			get { return array[e.face.index]; }
			set { array[e.face.index] = value; }
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
	/// Abstract base class for face attributes which are shared within face groups, which also derives from
	/// <see cref="ScriptableObject"/> and can therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	/// <typeparam name="T">The type of the attribute values.</typeparam>
	/// <typeparam name="TFaceGroupAttribute">The type of the face group attribute collection.</typeparam>
	public abstract class FaceGroupLookupFaceAttribute<T, TFaceGroupAttribute> : FaceAttribute<T> where TFaceGroupAttribute : FaceGroupAttribute<T>
	{
		/// <summary>
		/// Indexes indicating which face group each face belongs to.
		/// </summary>
		public IntFaceAttribute faceGroupIndices;

		/// <summary>
		/// Attribute values for each face group.
		/// </summary>
		public TFaceGroupAttribute faceGroupData;

		/// <inheritdoc/>
		public override T this[int i]
		{
			get { return faceGroupData[faceGroupIndices[i]]; }
			set { throw new NotSupportedException("A face group lookup face attribute is read only and cannot be modified."); }
		}

		/// <inheritdoc/>
		public override T this[Topology.Face f]
		{
			get { return faceGroupData[faceGroupIndices[f]]; }
			set { throw new NotSupportedException("A face group lookup face attribute is read only and cannot be modified."); }
		}

		/// <inheritdoc/>
		public override T this[Topology.HalfEdge e]
		{
			get { return faceGroupData[faceGroupIndices[e.face]]; }
			set { throw new NotSupportedException("A face group lookup face attribute is read only and cannot be modified."); }
		}

		/// <inheritdoc/>
		public override T this[Topology.VertexEdge e]
		{
			get { return faceGroupData[faceGroupIndices[e.face]]; }
			set { throw new NotSupportedException("A face group lookup face attribute is read only and cannot be modified."); }
		}

		/// <inheritdoc/>
		public override T this[Topology.FaceEdge e]
		{
			get { return faceGroupData[faceGroupIndices[e.face]]; }
			set { throw new NotSupportedException("A face group lookup face attribute is read only and cannot be modified."); }
		}
	}

	/// <summary>
	/// Extension methods involving faces and face attributes.
	/// </summary>
	public static class FaceExtensions
	{
		/// <summary>
		/// Creates a face attribute wrapper around an array.
		/// </summary>
		/// <typeparam name="T">The type of elements in the array.</typeparam>
		/// <param name="array">The array to be wrapped.</param>
		/// <returns>A face attribute wrapper around the array.</returns>
		public static FaceAttributeArrayWrapper<T> AsFaceAttribute<T>(this T[] array)
		{
			return new FaceAttributeArrayWrapper<T>(array);
		}
	}
}
#endif