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
	public class Vector2VertexAttribute : VertexArrayAttribute<Vector2>
	{
		public static Vector2VertexAttribute Create(Vector2[] array) { return CreateDerived<Vector2VertexAttribute>(array); }
		public static Vector2VertexAttribute Create(int vertexCount) { return CreateDerived<Vector2VertexAttribute>(vertexCount); }
	}
}
