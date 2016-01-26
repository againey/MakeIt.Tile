using System;
using UnityEngine;

namespace Experilous.Topological
{
	public class Vector3OffsetWrappedFaceAttribute : WrappedFaceAttribute<Vector3, Vector3FaceAttribute>
	{
		public PlanarSurfaceDescriptor surfaceDescriptor;

		public static Vector3OffsetWrappedFaceAttribute Create()
		{
			return CreateInstance<Vector3OffsetWrappedFaceAttribute>();
		}

		public static Vector3OffsetWrappedFaceAttribute Create(Topology topology, EdgeWrapDataEdgeAttribute edgeWrapData, Vector3FaceAttribute faceData, PlanarSurfaceDescriptor surfaceDescriptor)
		{
			var instance = CreateInstance<Vector3OffsetWrappedFaceAttribute>();
			instance.topology = topology;
			instance.edgeWrapData = edgeWrapData;
			instance.faceData = faceData;
			instance.surfaceDescriptor = surfaceDescriptor;
			return instance;
		}

		public static Vector3OffsetWrappedFaceAttribute Create(Topology topology, EdgeWrapDataEdgeAttribute edgeWrapData, Vector3FaceAttribute faceData, PlanarSurfaceDescriptor surfaceDescriptor, string name)
		{
			var instance = CreateInstance<Vector3OffsetWrappedFaceAttribute>();
			instance.topology = topology;
			instance.edgeWrapData = edgeWrapData;
			instance.faceData = faceData;
			instance.surfaceDescriptor = surfaceDescriptor;
			instance.name = name;
			return instance;
		}

		public static Vector3OffsetWrappedFaceAttribute Create(string name)
		{
			var instance = CreateInstance<Vector3OffsetWrappedFaceAttribute>();
			instance.name = name;
			return instance;
		}

		protected override Vector3 GetVertexRelativeAttribute(Vector3 faceValue, EdgeWrap edgeWrap)
		{
			return surfaceDescriptor.OffsetVertToFaceAttribute(faceValue, edgeWrap);
		}

		protected override Vector3 GetFaceRelativeAttribute(Vector3 faceValue, EdgeWrap edgeWrap)
		{
			return surfaceDescriptor.OffsetFaceToFaceAttribute(faceValue, edgeWrap);
		}
	}
}
