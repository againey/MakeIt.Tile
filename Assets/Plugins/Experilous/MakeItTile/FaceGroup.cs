/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// An interface representing a collection of topology faces.
	/// </summary>
	public interface IFaceGroup : IList<Topology.Face>
	{
		/// <summary>
		/// Checks whether the face with the given face index is contained within the current face group.
		/// </summary>
		/// <param name="faceIndex">The index of the face to be searched for.</param>
		/// <returns>True if the face with the given face index is stored within the face group, and false if it is not.</returns>
		bool Contains(int faceIndex);
	}

	/// <summary>
	/// An abstract base class for storing a collection of topology faces, derived from <see cref="UnityEngine.ScriptableObject"/>.
	/// </summary>
	public abstract class FaceGroup : ScriptableObject, IFaceGroup
	{
		/// <summary>
		/// The topology to which the faces of the face group belong.
		/// </summary>
		protected abstract Topology topology { get; }

		/// <summary>
		/// Accesses the faces of the face group by the index within the group of the face.
		/// </summary>
		/// <param name="index">The index within the group of the face to be accessed.</param>
		/// <returns>The face at the indicated position within the group.</returns>
		/// <remarks><note type="warning">The index for this accessor is not the same as the face's index within the overall topology.</note></remarks>
		public abstract Topology.Face this[int index] { get; set; }

		/// <summary>
		/// The number of faces within the current face group.
		/// </summary>
		public abstract int Count { get; }

		/// <summary>
		/// Indicates if the collection is read-only and thus cannot have items inserted into or removed from it.  Always true.
		/// </summary>
		public bool IsReadOnly { get { return true; } }

		/// <inheritdoc/>
		public virtual bool Contains(int faceIndex)
		{
			foreach (var face in this)
			{
				if (face.index == faceIndex)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Searches for the specified face and returns the index at which it is found.
		/// </summary>
		/// <param name="item">The face to be searched for.</param>
		/// <returns>The index of the given face within the face group, or -1 if no occurrence was found.</returns>
		/// <remarks><note type="warning">The index returned by this method is not the same as the face's index within the overall topology.</note></remarks>
		public virtual int IndexOf(Topology.Face item)
		{
			if (item.topology != topology) return -1;

			for (int i = 0; i < Count; ++i)
			{
				if (this[i] == item)
				{
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		/// Checks whether the given face is contained within the current face group.
		/// </summary>
		/// <param name="face">The face to be searched for.</param>
		/// <returns>True if the given face is stored within the face group, and false if it is not.</returns>
		public virtual bool Contains(Topology.Face face)
		{
			return face.topology == topology && Contains(face.index);
		}

		/// <summary>
		/// Copies the faces of the current face group into the specified array.
		/// </summary>
		/// <param name="array">The target array into which faces will be copied.</param>
		/// <param name="arrayIndex">The starting index at which the copy of values will begin.</param>
		public virtual void CopyTo(Topology.Face[] array, int arrayIndex)
		{
			foreach (var face in this)
			{
				array[arrayIndex++] = face;
			}
		}

		/// <summary>
		/// Gets an enumerator over all the faces in the current face group.
		/// </summary>
		/// <returns>An enumerator over all the faces in the face group.</returns>
		public abstract IEnumerator<Topology.Face> GetEnumerator();

		/// <summary>
		/// Gets an enumerator over all the faces in the current face group.
		/// </summary>
		/// <returns>An enumerator over all the faces in the face group.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Not supported.
		/// </summary>
		/// <inheritdoc/>
		public void Add(Topology.Face item) { throw new NotSupportedException(); }

		/// <summary>
		/// Not supported.
		/// </summary>
		/// <inheritdoc/>
		public void Clear() { throw new NotSupportedException(); }

		/// <summary>
		/// Not supported.
		/// </summary>
		/// <inheritdoc/>
		public void Insert(int index, Topology.Face item) { throw new NotSupportedException(); }

		/// <summary>
		/// Not supported.
		/// </summary>
		/// <inheritdoc/>
		public bool Remove(Topology.Face item) { throw new NotSupportedException(); }

		/// <summary>
		/// Not supported.
		/// </summary>
		/// <inheritdoc/>
		public void RemoveAt(int index) { throw new NotSupportedException(); }
	}

	/// <summary>
	/// Generic interface for accessing attribute values of topology face groups.
	/// </summary>
	/// <typeparam name="T">The type of the attribute values.</typeparam>
	public interface IFaceGroupAttribute<T> : IList<T>
	{
		/// <summary>
		/// Lookup the attribute value for the face group indicated.
		/// </summary>
		/// <param name="faceGroup">The face group whose attribute value is desired.</param>
		/// <returns>The attribute value for the face group indicated.</returns>
		T this[FaceGroup faceGroup] { get; set; }
	}

	/// <summary>
	/// Abstract base class for face group attributes that also derives from <see cref="ScriptableObject"/>
	/// and can therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	/// <typeparam name="T">The type of the attribute values.</typeparam>
	public abstract class FaceGroupAttribute<T> : ScriptableObject, IFaceGroupAttribute<T>
	{
		/// <inheritdoc/>
		public abstract T this[int i] { get; set; }
		/// <inheritdoc/>
		public abstract T this[FaceGroup faceGroup] { get; set; }

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
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	/// <summary>
	/// A face group attribute wrapper around a raw array of face group attributes, which also derives from
	/// <see cref="ScriptableObject"/> and can therefore be serialized, referenced, and turned into an asset by Unity.
	/// </summary>
	/// <typeparam name="T">The type of the attribute values.</typeparam>
	public class FaceGroupArrayAttribute<T> : FaceGroupAttribute<T> where T : new()
	{
		/// <summary>
		/// The array of face group attribute values that is being wrapped.
		/// </summary>
		public T[] array;

		/// <summary>
		/// Creates an instance of a class derived from this one, and assigns it the array data provided.
		/// </summary>
		/// <typeparam name="TDerived">The type of the derived class.</typeparam>
		/// <param name="array">The array of face group attribute values to be wrapped.</param>
		/// <returns>An instance of the derived class using the array data provided.</returns>
		/// <remarks><para>This is a convenience function for derived classes to use, which are needed
		/// because Unity is unable to serialize generic classes directly, but can serialize non-generic
		/// classes which are derived from generic classes but given a concrete type.</para></remarks>
		protected static TDerived CreateDerived<TDerived>(T[] array) where TDerived : FaceGroupArrayAttribute<T>
		{
			var instance = CreateInstance<TDerived>();
			instance.array = array;
			return instance;
		}

		/// <summary>
		/// Creates an instance of a class derived from this one, and creates an array of the specified size.
		/// </summary>
		/// <typeparam name="TDerived">The type of the derived class.</typeparam>
		/// <param name="groupCount">The number of elements that the created array will contain.</param>
		/// <returns>An instance of the derived class using an array created with the specified size.</returns>
		/// <remarks><para>This is a convenience function for derived classes to use, which are needed
		/// because Unity is unable to serialize generic classes directly, but can serialize non-generic
		/// classes which are derived from generic classes but given a concrete type.</para></remarks>
		protected static TDerived CreateDerived<TDerived>(int groupCount) where TDerived : FaceGroupArrayAttribute<T>
		{
			return CreateDerived<TDerived>(new T[groupCount]);
		}

		/// <inheritdoc/>
		public override T this[int i]
		{
			get { return array[i]; }
			set { array[i] = value; }
		}

		/// <inheritdoc/>
		public override T this[FaceGroup faceGroup]
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
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
}
