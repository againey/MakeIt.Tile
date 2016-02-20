﻿using UnityEngine;

namespace Experilous.Topological
{
	public class PositionalFaceAttribute : FaceArrayAttribute<Vector3>
	{
		public Surface surface;

		public static PositionalFaceAttribute Create(Surface surface, Vector3[] array)
		{
			var instance = CreateInstance<PositionalFaceAttribute>();
			instance.surface = surface;
			instance.array = array;
			return instance;
		}

		public static PositionalFaceAttribute Create(Surface surface, int vertexCount)
		{
			return Create(surface, new Vector3[vertexCount]);
		}

		public override Vector3 this[Topology.HalfEdge e]
		{
			get { return surface.OffsetEdgeToFaceAttribute(array[e.farFace.index], e.wrap); }
			set { array[e.farVertex.index] = surface.ReverseOffsetEdgeToFaceAttribute(array[e.farFace.index], e.wrap); }
		}

		public override Vector3 this[Topology.VertexEdge e]
		{
			get { return surface.OffsetVertToFaceAttribute(array[e.prevFace.index], e.wrap); }
			set { array[e.farVertex.index] = surface.ReverseOffsetVertToFaceAttribute(array[e.prevFace.index], e.wrap); }
		}

		public override Vector3 this[Topology.FaceEdge e]
		{
			get { return surface.OffsetFaceToFaceAttribute(array[e.farFace.index], e.wrap); }
			set { array[e.nextVertex.index] = surface.ReverseOffsetFaceToFaceAttribute(array[e.farFace.index], e.wrap); }
		}
	}
}