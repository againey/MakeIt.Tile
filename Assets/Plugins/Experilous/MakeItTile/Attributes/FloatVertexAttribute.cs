/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

namespace Experilous.MakeItTile
{
	public class FloatVertexAttribute : VertexArrayAttribute<float>
	{
		public static FloatVertexAttribute Create(float[] array) { return CreateDerived<FloatVertexAttribute>(array); }
		public static FloatVertexAttribute Create(int vertexCount) { return CreateDerived<FloatVertexAttribute>(vertexCount); }
	}
}
