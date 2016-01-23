using System;
using UnityEngine;

namespace Experilous.Topological
{
	public class Vector3OffsetWrappedVertexAttribute : WrappedVertexAttribute<Vector3, Vector3VertexAttribute>, ICloneable
	{
		public PlanarSurfaceDescriptor surfaceDescriptor;

		public static Vector3OffsetWrappedVertexAttribute CreateInstance()
		{
			return CreateInstance<Vector3OffsetWrappedVertexAttribute>();
		}

		public static Vector3OffsetWrappedVertexAttribute CreateInstance(Topology topology, EdgeWrapDataEdgeAttribute edgeWrapData, Vector3VertexAttribute vertexData, PlanarSurfaceDescriptor surfaceDescriptor)
		{
			var instance = CreateInstance<Vector3OffsetWrappedVertexAttribute>();
			instance.topology = topology;
			instance.edgeWrapData = edgeWrapData;
			instance.vertexData = vertexData;
			instance.surfaceDescriptor = surfaceDescriptor;
			return instance;
		}

		public static Vector3OffsetWrappedVertexAttribute CreateInstance(Topology topology, EdgeWrapDataEdgeAttribute edgeWrapData, Vector3VertexAttribute vertexData, PlanarSurfaceDescriptor surfaceDescriptor, string name)
		{
			var instance = CreateInstance<Vector3OffsetWrappedVertexAttribute>();
			instance.topology = topology;
			instance.edgeWrapData = edgeWrapData;
			instance.vertexData = vertexData;
			instance.surfaceDescriptor = surfaceDescriptor;
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
				topology,
				edgeWrapData,
				vertexData,
				surfaceDescriptor,
				name);
			clone.hideFlags = hideFlags;
			return clone;
		}

		protected override Vector3 GetAttribute(Vector3 vertexValue, EdgeWrap edgeWrap)
		{
			return surfaceDescriptor.OffsetPositive(vertexValue, edgeWrap);
		}
	}
}
