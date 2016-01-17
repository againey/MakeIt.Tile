using System;
using UnityEngine;

namespace Experilous.Topological
{
	public class Vector3OffsetWrappedVertexAttribute : WrappedVertexAttribute<Vector3, Vector3VertexAttribute>, ICloneable
	{
		public Vector3[] repetitionAxes;

		public static Vector3OffsetWrappedVertexAttribute CreateInstance()
		{
			return CreateInstance<Vector3OffsetWrappedVertexAttribute>();
		}

		public static Vector3OffsetWrappedVertexAttribute CreateInstance(EdgeWrapDataEdgeAttribute edgeWrapData, Vector3VertexAttribute vertexData, Vector3[] repetitionAxes)
		{
			var instance = CreateInstance<Vector3OffsetWrappedVertexAttribute>();
			instance.edgeWrapData = edgeWrapData;
			instance.vertexData = vertexData;
			instance.repetitionAxes = (Vector3[])repetitionAxes.Clone();
			return instance;
		}

		public static Vector3OffsetWrappedVertexAttribute CreateInstance(EdgeWrapDataEdgeAttribute edgeWrapData, Vector3VertexAttribute vertexData, Vector3[] repetitionAxes, string name)
		{
			var instance = CreateInstance<Vector3OffsetWrappedVertexAttribute>();
			instance.edgeWrapData = edgeWrapData;
			instance.vertexData = vertexData;
			instance.repetitionAxes = (Vector3[])repetitionAxes.Clone();
			instance.name = name;
			return instance;
		}

		public new static Vector3OffsetWrappedVertexAttribute CreateInstance(string name)
		{
			var instance = CreateInstance<Vector3OffsetWrappedVertexAttribute>();
			instance.name = name;
			return instance;
		}

		object ICloneable.Clone() { return Clone(); }

		public Vector3OffsetWrappedVertexAttribute Clone()
		{
			var clone = CreateInstance(
				edgeWrapData.Clone(),
				vertexData.Clone(),
				(Vector3[])repetitionAxes.Clone(),
				name);
			clone.hideFlags = hideFlags;
			return clone;
		}

		protected override Vector3 GetAttribute(Vector3 vertexValue)
		{
			return vertexValue;
		}

		protected override Vector3 GetAttribute(Vector3 vertexValue, int repetitionAxisIndex)
		{
			return vertexValue + repetitionAxes[repetitionAxisIndex];
		}

		protected override Vector3 GetAttribute(Vector3 vertexValue, int repetitionAxisIndex0, int repetitionAxisIndex1)
		{
			return vertexValue + repetitionAxes[repetitionAxisIndex0] + repetitionAxes[repetitionAxisIndex1];
		}
	}
}
