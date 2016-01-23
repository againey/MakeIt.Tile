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
}
