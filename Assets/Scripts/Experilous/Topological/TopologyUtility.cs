using System;

namespace Experilous.Topological
{
	public static class TopologyUtility
	{
		public static void CheckVerticesForInvalidEdgeCycles(Topology topology)
		{
			foreach (var vertex in topology.vertices)
			{
				var edge = vertex.firstEdge.next;
				for (int i = 1; i < vertex.neighborCount; ++i)
				{
					if (vertex.firstEdge == edge)
						throw new InvalidOperationException(string.Format("The cycle of edges around vertex {0} returned back to the first edge earlier than was expected, {1} iterations rather than {2}.", vertex.index, i, vertex.neighborCount));
					edge = edge.next;
				}
				if (vertex.firstEdge != edge)
					throw new InvalidOperationException(string.Format("The cycle of edges around vertex {0}  did not return back to the first edge in the {1} iterations expected.", vertex.index, vertex.neighborCount));
			}
		}

		public static void CheckFacesForInvalidEdgeCycles(Topology topology)
		{
			foreach (var face in topology.faces)
			{
				var edge = face.firstEdge.next;
				for (int i = 1; i < face.neighborCount; ++i)
				{
					if (face.firstEdge == edge)
						throw new InvalidOperationException(string.Format("The cycle of edges around face {0} returned back to the first edge earlier than was expected, {1} iterations rather than {2}.", face.index, i, face.neighborCount));
					edge = edge.next;
				}
				if (face.firstEdge != edge)
					throw new InvalidOperationException(string.Format("The cycle of edges around face {0} did not return back to the first edge in the {1} iterations expected.", face.index, face.neighborCount));
			}
		}
	}
}
