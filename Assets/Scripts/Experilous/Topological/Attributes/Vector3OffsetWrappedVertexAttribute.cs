using System;
using UnityEngine;

namespace Experilous.Topological
{
	public class Vector3OffsetWrappedVertexAttribute : WrappedVertexAttribute<Vector3, Vector3VertexAttribute>
	{
		public PlanarSurfaceDescriptor surfaceDescriptor;

		public static Vector3OffsetWrappedVertexAttribute Create()
		{
			return CreateInstance<Vector3OffsetWrappedVertexAttribute>();
		}

		public static Vector3OffsetWrappedVertexAttribute Create(Topology topology, EdgeWrapDataEdgeAttribute edgeWrapData, Vector3VertexAttribute vertexData, PlanarSurfaceDescriptor surfaceDescriptor)
		{
			var instance = CreateInstance<Vector3OffsetWrappedVertexAttribute>();
			instance.topology = topology;
			instance.edgeWrapData = edgeWrapData;
			instance.vertexData = vertexData;
			instance.surfaceDescriptor = surfaceDescriptor;
			return instance;
		}

		public static Vector3OffsetWrappedVertexAttribute Create(Topology topology, EdgeWrapDataEdgeAttribute edgeWrapData, Vector3VertexAttribute vertexData, PlanarSurfaceDescriptor surfaceDescriptor, string name)
		{
			var instance = CreateInstance<Vector3OffsetWrappedVertexAttribute>();
			instance.topology = topology;
			instance.edgeWrapData = edgeWrapData;
			instance.vertexData = vertexData;
			instance.surfaceDescriptor = surfaceDescriptor;
			instance.name = name;
			return instance;
		}

		public static Vector3OffsetWrappedVertexAttribute Create(string name)
		{
			var instance = CreateInstance<Vector3OffsetWrappedVertexAttribute>();
			instance.name = name;
			return instance;
		}

		protected override Vector3 GetVertexRelativeAttribute(Vector3 vertexValue, EdgeWrap edgeWrap)
		{
			return surfaceDescriptor.OffsetVertToVertAttribute(vertexValue, edgeWrap);
		}

		protected override Vector3 GetFaceRelativeAttribute(Vector3 vertexValue, EdgeWrap edgeWrap)
		{
			return surfaceDescriptor.OffsetFaceToVertAttribute(vertexValue, edgeWrap);
		}
	}
}
