using UnityEngine;

namespace Experilous.Topological
{
	public class PositionalEdgeAttribute : EdgeArrayAttribute<Vector3>
	{
		public static PositionalEdgeAttribute Create(Vector3[] array) { return CreateDerived<PositionalEdgeAttribute>(array); }
		public static PositionalEdgeAttribute Create(int edgeCount) { return CreateDerived<PositionalEdgeAttribute>(edgeCount); }

		public override Vector3 this[Topology.VertexEdge e]
		{
			get { return e.topology.TransformEdgePointFromVertex(e.index, array[e.index]); }
			set { array[e.index] = e.topology.InverseTransformEdgePointFromVertex(e.index, array[e.index]); }
		}

		public override Vector3 this[Topology.FaceEdge e]
		{
			get { return e.topology.TransformEdgePointFromFace(e.index, array[e.index]); }
			set { array[e.index] = e.topology.InverseTransformEdgePointFromFace(e.index, array[e.index]); }
		}
	}
}
