/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

namespace Experilous.Topological
{
	public interface IFaceGroup : IList<Topology.Face>
	{
		bool Contains(int faceIndex);
	}

	public abstract class FaceGroup : ScriptableObject, IFaceGroup
	{
		protected abstract Topology topology { get; }

		public abstract Topology.Face this[int index] { get; set; }

		public abstract int Count { get; }

		public bool IsReadOnly { get { return true; } }

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

		public virtual bool Contains(Topology.Face face)
		{
			return face.topology == topology && Contains(face.index);
		}

		public virtual void CopyTo(Topology.Face[] array, int arrayIndex)
		{
			foreach (var face in this)
			{
				array[arrayIndex++] = face;
			}
		}

		public abstract IEnumerator<Topology.Face> GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(Topology.Face item) { throw new NotSupportedException(); }
		public void Clear() { throw new NotSupportedException(); }
		public void Insert(int index, Topology.Face item) { throw new NotSupportedException(); }
		public bool Remove(Topology.Face item) { throw new NotSupportedException(); }
		public void RemoveAt(int index) { throw new NotSupportedException(); }
	}

	public interface IFaceGroupAttribute<T> : IList<T>
	{
		T this[FaceGroup faceGroup] { get; set; }
	}

	public abstract class FaceGroupAttribute<T> : ScriptableObject, IFaceGroupAttribute<T>
	{
		public abstract T this[int i] { get; set; }
		public abstract T this[FaceGroup faceGroup] { get; set; }

		public virtual int Count { get { throw new NotSupportedException(); } }
		public virtual bool IsReadOnly { get { return true; } }
		public virtual void Add(T item) { throw new NotSupportedException(); }
		public virtual void Clear() { throw new NotSupportedException(); }
		public virtual bool Contains(T item) { throw new NotSupportedException(); }
		public virtual void CopyTo(T[] array, int arrayIndex) { throw new NotSupportedException(); }
		public virtual IEnumerator<T> GetEnumerator() { throw new NotSupportedException(); }
		public virtual int IndexOf(T item) { throw new NotSupportedException(); }
		public virtual void Insert(int index, T item) { throw new NotSupportedException(); }
		public virtual bool Remove(T item) { throw new NotSupportedException(); }
		public virtual void RemoveAt(int index) { throw new NotSupportedException(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	public class FaceGroupArrayAttribute<T> : FaceGroupAttribute<T> where T : new()
	{
		public T[] array;

		protected static TDerived CreateDerived<TDerived>(T[] array) where TDerived : FaceGroupArrayAttribute<T>
		{
			var instance = CreateInstance<TDerived>();
			instance.array = array;
			return instance;
		}

		protected static TDerived CreateDerived<TDerived>(int groupCount) where TDerived : FaceGroupArrayAttribute<T>
		{
			return CreateDerived<TDerived>(new T[groupCount]);
		}

		public override T this[int i]
		{
			get { return array[i]; }
			set { array[i] = value; }
		}

		public override T this[FaceGroup faceGroup]
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public override int Count { get { return array.Length; } }
		public override bool Contains(T item) { return ((IList<T>)array).Contains(item); }
		public override void CopyTo(T[] array, int arrayIndex) { this.array.CopyTo(array, arrayIndex); }
		public override IEnumerator<T> GetEnumerator() { return ((IList<T>)array).GetEnumerator(); }
		public override int IndexOf(T item) { return ((IList<T>)array).IndexOf(item); }
	}
}
