/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

namespace Experilous.Topological
{
	public class FloatVertexAttribute : VertexArrayAttribute<float>
	{
		public static FloatVertexAttribute Create(float[] array) { return CreateDerived<FloatVertexAttribute>(array); }
		public static FloatVertexAttribute Create(int vertexCount) { return CreateDerived<FloatVertexAttribute>(vertexCount); }
	}
}
