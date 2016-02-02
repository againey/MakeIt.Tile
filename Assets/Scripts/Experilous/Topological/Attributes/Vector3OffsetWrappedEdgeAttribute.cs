using UnityEngine;

namespace Experilous.Topological
{
	public class Vector3OffsetWrappedEdgeAttribute : WrappedEdgeAttribute<Vector3>
	{
		public PlanarSurfaceDescriptor surfaceDescriptor;

		public static Vector3OffsetWrappedEdgeAttribute Create()
		{
			return CreateInstance<Vector3OffsetWrappedEdgeAttribute>();
		}

		public static Vector3OffsetWrappedEdgeAttribute Create(IEdgeAttribute<EdgeWrap> edgeWrapAttribute, IEdgeAttribute<Vector3> edgeAttribute, PlanarSurfaceDescriptor surfaceDescriptor)
		{
			var instance = CreateInstance<Vector3OffsetWrappedEdgeAttribute>();
			instance.edgeWrapAttribute = edgeWrapAttribute;
			instance.edgeAttribute = edgeAttribute;
			instance.surfaceDescriptor = surfaceDescriptor;
			return instance;
		}

		public static Vector3OffsetWrappedEdgeAttribute Create(IEdgeAttribute<EdgeWrap> edgeWrapAttribute, IEdgeAttribute<Vector3> edgeAttribute, PlanarSurfaceDescriptor surfaceDescriptor, string name)
		{
			var instance = Create(edgeWrapAttribute, edgeAttribute, surfaceDescriptor);
			instance.name = name;
			return instance;
		}

		public static Vector3OffsetWrappedEdgeAttribute Create(string name)
		{
			var instance = CreateInstance<Vector3OffsetWrappedEdgeAttribute>();
			instance.name = name;
			return instance;
		}

		protected override Vector3 GetVertexRelativeAttribute(Vector3 edgeValue, EdgeWrap edgeWrap)
		{
			return surfaceDescriptor.OffsetVertToEdgeAttribute(edgeValue, edgeWrap);
		}

		protected override Vector3 GetFaceRelativeAttribute(Vector3 edgeValue, EdgeWrap edgeWrap)
		{
			return surfaceDescriptor.OffsetFaceToEdgeAttribute(edgeValue, edgeWrap);
		}
	}
}
