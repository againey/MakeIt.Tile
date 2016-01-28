using UnityEngine;

namespace Experilous.Topological
{
	public class Vector3OffsetWrappedFaceAttribute : WrappedFaceAttribute<Vector3>
	{
		public PlanarSurfaceDescriptor surfaceDescriptor;

		public static Vector3OffsetWrappedFaceAttribute Create()
		{
			return CreateInstance<Vector3OffsetWrappedFaceAttribute>();
		}

		public static Vector3OffsetWrappedFaceAttribute Create(IEdgeAttribute<EdgeWrap> edgeWrapAttribute, IFaceAttribute<Vector3> faceAttribute, PlanarSurfaceDescriptor surfaceDescriptor)
		{
			var instance = CreateInstance<Vector3OffsetWrappedFaceAttribute>();
			instance.edgeWrapAttribute = edgeWrapAttribute;
			instance.faceAttribute = faceAttribute;
			instance.surfaceDescriptor = surfaceDescriptor;
			return instance;
		}

		public static Vector3OffsetWrappedFaceAttribute Create(IEdgeAttribute<EdgeWrap> edgeWrapAttribute, IFaceAttribute<Vector3> faceAttribute, PlanarSurfaceDescriptor surfaceDescriptor, string name)
		{
			var instance = CreateInstance<Vector3OffsetWrappedFaceAttribute>();
			instance.edgeWrapAttribute = edgeWrapAttribute;
			instance.faceAttribute = faceAttribute;
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

		protected override Vector3 GetEdgeRelativeAttribute(Vector3 faceValue, EdgeWrap edgeWrap)
		{
			return surfaceDescriptor.OffsetEdgeToFaceAttribute(faceValue, edgeWrap);
		}

		protected override Vector3 GetFaceRelativeAttribute(Vector3 faceValue, EdgeWrap edgeWrap)
		{
			return surfaceDescriptor.OffsetFaceToFaceAttribute(faceValue, edgeWrap);
		}
	}
}
