/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

using UnityEngine;

namespace Experilous.Topological
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

		public static PositionalEdgeAttribute Create(Surface surface, int vertexCount)
		{
			return Create(surface, new Vector3[vertexCount]);
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
