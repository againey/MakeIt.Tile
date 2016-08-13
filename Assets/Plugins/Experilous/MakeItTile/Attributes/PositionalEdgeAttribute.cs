/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;

namespace Experilous.MakeItTile
{
	public class PositionalEdgeAttribute : EdgeArrayAttribute<Vector3>
	{
		public Surface surface;

		public static PositionalEdgeAttribute Create(Surface surface, Vector3[] array)
		{
			var instance = CreateInstance<PositionalEdgeAttribute>();
			instance.surface = surface;
			instance.array = array;
			return instance;
		}

		public static PositionalEdgeAttribute Create(Surface surface, int edgeCount)
		{
			return Create(surface, new Vector3[edgeCount]);
		}

		public override Vector3 this[Topology.VertexEdge e]
		{
			get { return surface.OffsetVertToEdgeAttribute(array[e.farVertex.index], e.wrap); }
			set { array[e.farVertex.index] = surface.ReverseOffsetVertToEdgeAttribute(array[e.farVertex.index], e.wrap); }
		}

		public override Vector3 this[Topology.FaceEdge e]
		{
			get { return surface.OffsetFaceToEdgeAttribute(array[e.nextVertex.index], e.wrap); }
			set { array[e.nextVertex.index] = surface.ReverseOffsetFaceToEdgeAttribute(array[e.nextVertex.index], e.wrap); }
		}
	}
}
