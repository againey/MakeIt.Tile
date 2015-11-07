using System.Collections.Generic;

namespace Experilous.Topological
{
	public struct FaceAttribute<T> where T : new()
	{
		public T[] _values;

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

		public T this[int i]
		{
			get {  return _values[i]; }
			set {  _values[i] = value; }
		}

		public T this[Topology.Face face]
		{
			get {  return _values[face.index]; }
			set { _values[face.index] = value; }
		}

		public int Count
		{
			get { return _values.Length; }
		}

		public void Clear()
		{
			System.Array.Clear(_values, 0, _values.Length);
		}
	}
}
