using UnityEngine;

namespace Experilous.Topological
{
	public class PositionalFaceAttribute : FaceArrayAttribute<Vector3>
	{
		public static Vector3FaceAttribute Create(Vector3[] array) { return CreateDerived<Vector3FaceAttribute>(array); }
		public static Vector3FaceAttribute Create(int faceCount) { return CreateDerived<Vector3FaceAttribute>(faceCount); }

		public override Vector3 this[Topology.HalfEdge e]
		{
			get { return e.topology.TransformFacePointFromEdge(e.index, array[e.farFace.index]); }
			set { array[e.farFace.index] = e.topology.InverseTransformFacePointFromEdge(e.index, array[e.farFace.index]); }
		}

		public override Vector3 this[Topology.VertexEdge e]
		{
			get { return e.topology.TransformFacePointFromVertex(e.index, array[e.prevFace.index]); }
			set { array[e.prevFace.index] = e.topology.InverseTransformFacePointFromVertex(e.index, array[e.prevFace.index]); }
		}

		public override Vector3 this[Topology.FaceEdge e]
		{
			get { return e.topology.TransformFacePointFromFace(e.index, array[e.farFace.index]); }
			set { array[e.farFace.index] = e.topology.InverseTransformFacePointFromFace(e.index, array[e.farFace.index]); }
		}
	}
}
