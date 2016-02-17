using UnityEngine;
using System;
using System.Collections.Generic;

namespace Experilous.Topological
{
	public class PositionalVertexAttribute : VertexArrayAttribute<Vector3>
	{
		public Surface surface;

		public static PositionalVertexAttribute Create(Surface surface, Vector3[] array)
		{
			var instance = CreateInstance<PositionalVertexAttribute>();
			instance.surface = surface;
			instance.array = array;
			return instance;
		}

		public static PositionalVertexAttribute Create(Surface surface, int vertexCount)
		{
			return Create(surface, new Vector3[vertexCount]);
		}

		public static PositionalVertexAttribute Create(Surface surface, Vector3[] array, string name)
		{
			return Create(surface, array).SetName(name);
		}

		public static PositionalVertexAttribute Create(Surface surface, int vertexCount, string name)
		{
			return Create(surface, vertexCount).SetName(name);
		}

		public override Vector3 this[Topology.HalfEdge e]
		{
			get { return surface.OffsetEdgeToVertAttribute(array[e.farVertex.index], e.wrap); }
			set { array[e.farVertex.index] = surface.ReverseOffsetEdgeToVertAttribute(array[e.farVertex.index], e.wrap); }
		}

		public override Vector3 this[Topology.VertexEdge e]
		{
			get { return surface.OffsetVertToVertAttribute(array[e.farVertex.index], e.wrap); }
			set { array[e.farVertex.index] = surface.ReverseOffsetVertToVertAttribute(array[e.farVertex.index], e.wrap); }
		}

		public override Vector3 this[Topology.FaceEdge e]
		{
			get { return surface.OffsetFaceToVertAttribute(array[e.nextVertex.index], e.wrap); }
			set { array[e.nextVertex.index] = surface.ReverseOffsetFaceToVertAttribute(array[e.nextVertex.index], e.wrap); }
		}
	}
}
