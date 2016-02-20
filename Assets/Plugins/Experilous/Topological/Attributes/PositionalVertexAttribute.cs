using UnityEngine;

namespace Experilous.Topological
{
	public class PositionalVertexAttribute : VertexArrayAttribute<Vector3>
	{
		public static Vector3VertexAttribute Create(Vector3[] array) { return CreateDerived<Vector3VertexAttribute>(array); }
		public static Vector3VertexAttribute Create(int vertexCount) { return CreateDerived<Vector3VertexAttribute>(vertexCount); }

		public override Vector3 this[Topology.HalfEdge e]
		{
			get { return e.topology.TransformVertexPointFromEdge(e.index, array[e.farVertex.index]); }
			set { array[e.farVertex.index] = e.topology.InverseTransformVertexPointFromEdge(e.index, array[e.farVertex.index]); }
		}

		public override Vector3 this[Topology.VertexEdge e]
		{
			get { return e.topology.TransformVertexPointFromVertex(e.index, array[e.farVertex.index]); }
			set { array[e.farVertex.index] = e.topology.InverseTransformVertexPointFromVertex(e.index, array[e.farVertex.index]); }
		}

		public override Vector3 this[Topology.FaceEdge e]
		{
			get { return e.topology.TransformVertexPointFromFace(e.index, array[e.nextVertex.index]); }
			set { array[e.nextVertex.index] = e.topology.InverseTransformVertexPointFromFace(e.index, array[e.nextVertex.index]); }
		}
	}
}
