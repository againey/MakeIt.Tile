using UnityEngine;
using System;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public interface IFaceAttribute<T> where T : new()
	{
		T this[int faceIndex] { get; set; }
		T this[Topology.Face face] { get; set; }

		int Count { get; }

		void Clear();
	}

	[Serializable]
	public struct FaceAttribute<T> : IFaceAttribute<T> where T : new()
	{
		[SerializeField]
		private T[] _values;

		public FaceAttribute(int faceCount)
		{
			_values = new T[faceCount];
		}

		public FaceAttribute(T[] values)
		{
			_values = values.Clone() as T[];
		}

		public FaceAttribute(ICollection<T> collection)
		{
			_values = new T[collection.Count];
			collection.CopyTo(_values, 0);
		}

		public FaceAttribute<T> Clone()
		{
			return new FaceAttribute<T>(_values);
		}

		public T this[int faceIndex]
		{
			get {  return _values[faceIndex]; }
			set {  _values[faceIndex] = value; }
		}

		public T this[Topology.Face face]
		{
			get {  return _values[face.index]; }
			set { _values[face.index] = value; }
		}

		public int Count
		{
			get { return (_values != null) ? _values.Length : 0; }
		}

		public void Clear()
		{
			System.Array.Clear(_values, 0, _values.Length);
		}

		public void Reset()
		{
			_values = null;
		}

		public bool isEmpty
		{
			get { return _values == null || _values.Length == 0; }
		}

		public T[] underlyingArray
		{
			get { return _values; }
		}
	}
}
