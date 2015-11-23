using System;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public class Region
	{
		private readonly Topology _topology;
		private readonly List<int> _faceIndices = new List<int>();

		public Region(Topology topology)
		{
			_topology = topology;
		}

		public void Add(Topology.Face face)
		{
			_faceIndices.Add(face.index);
		}

		public struct FacesIndexer
		{
			private readonly Topology.FacesIndexer _topologyFaces;
			private readonly List<int> _faceIndices;

			public FacesIndexer(Region region){ _topologyFaces = region._topology.faces; _faceIndices = region._faceIndices; }
			public Topology.Face this[int i] { get { return _topologyFaces[_faceIndices[i]]; } }
			public int Count { get { return _topologyFaces.Count; } }
			public FaceEnumerator GetEnumerator() { return new FaceEnumerator(_topologyFaces, _faceIndices); }

			public struct FaceEnumerator
			{
				private readonly Topology.FacesIndexer _topologyFaces;
				private readonly List<int> _faceIndices;
				private int _index;

				public FaceEnumerator(Topology.FacesIndexer topologyFaces, List<int> faceIndices) { _topologyFaces = topologyFaces; _faceIndices = faceIndices; _index = -1; }
				public Topology.Face Current { get { return _topologyFaces[_faceIndices[_index]]; } }
				public bool MoveNext() { return (++_index < _faceIndices.Count); }
				public void Reset() { _index = -1; }
			}
		}

		public FacesIndexer faces { get { return new FacesIndexer(this); } }
	}
}
