/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;

namespace Experilous.MakeItTile
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

		public static PositionalFaceAttribute Create(Surface surface, int faceCount)
		{
			return Create(surface, new Vector3[faceCount]);
		}

		public override Vector3 this[Topology.HalfEdge e]
		{
			get { return surface.OffsetEdgeToFaceAttribute(array[e.face.index], e.wrap); }
			set { array[e.face.index] = surface.ReverseOffsetEdgeToFaceAttribute(array[e.face.index], e.wrap); }
		}

		public override Vector3 this[Topology.VertexEdge e]
		{
			get { return surface.OffsetVertToFaceAttribute(array[e.face.index], e.wrap); }
			set { array[e.face.index] = surface.ReverseOffsetVertToFaceAttribute(array[e.face.index], e.wrap); }
		}

		public override Vector3 this[Topology.FaceEdge e]
		{
			get { return surface.OffsetFaceToFaceAttribute(array[e.face.index], e.wrap); }
			set { array[e.face.index] = surface.ReverseOffsetFaceToFaceAttribute(array[e.face.index], e.wrap); }
		}
	}
}
