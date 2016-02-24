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
	public class Vector3VertexAttribute : VertexArrayAttribute<Vector3>
	{
		public static Vector3VertexAttribute Create(Vector3[] array) { return CreateDerived<Vector3VertexAttribute>(array); }
		public static Vector3VertexAttribute Create(int vertexCount) { return CreateDerived<Vector3VertexAttribute>(vertexCount); }
	}
}
