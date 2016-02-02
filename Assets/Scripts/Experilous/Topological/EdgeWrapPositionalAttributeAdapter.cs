using UnityEngine;
using System;

namespace Experilous.Topological
{
	public class EdgeWrapPositionalAttributeAdapter : PositionalAttributeAdapter, ISerializationCallbackReceiver
	{
		[SerializeField] public PlanarSurfaceDescriptor surfaceDescriptor;
		[NonSerialized] public IEdgeAttribute<EdgeWrap> edgeWrap;

		[SerializeField] private ScriptableObject _edgeWrapReference;

		[Obsolete("An instance of EdgeWrapPositionalAttributeAdapter cannot be created without providing a surface descriptor and edge wrap attribute.", true)]
		public new static EdgeWrapPositionalAttributeAdapter Create()
		{
			throw new NotSupportedException("An instance of EdgeWrapPositionalAttributeAdapter cannot be created without providing a surface descriptor and edge wrap attribute.");
		}

		[Obsolete("An instance of EdgeWrapPositionalAttributeAdapter cannot be created without providing a surface descriptor and edge wrap attribute.", true)]
		public new static EdgeWrapPositionalAttributeAdapter Create(string name)
		{
			throw new NotSupportedException("An instance of EdgeWrapPositionalAttributeAdapter cannot be created without providing a surface descriptor and edge wrap attribute.");
		}

		public static EdgeWrapPositionalAttributeAdapter Create(PlanarSurfaceDescriptor surfaceDescriptor, IEdgeAttribute<EdgeWrap> edgeWrap)
		{
			var adapter = CreateInstance<EdgeWrapPositionalAttributeAdapter>();
			adapter.surfaceDescriptor = surfaceDescriptor;
			adapter.edgeWrap = edgeWrap;
			return adapter;
		}

		public static EdgeWrapPositionalAttributeAdapter Create(PlanarSurfaceDescriptor surfaceDescriptor, IEdgeAttribute<EdgeWrap> edgeWrap, string name)
		{
			var adapter = Create(surfaceDescriptor, edgeWrap);
			adapter.name = name;
			return adapter;
		}

		public override IVertexAttribute<Vector3> Adapt(IVertexAttribute<Vector3> attribute)
		{
			return Vector3OffsetWrappedVertexAttribute.Create(edgeWrap, attribute, surfaceDescriptor);
		}

		public override IEdgeAttribute<Vector3> Adapt(IEdgeAttribute<Vector3> attribute)
		{
			return Vector3OffsetWrappedEdgeAttribute.Create(edgeWrap, attribute, surfaceDescriptor);
		}

		public override IFaceAttribute<Vector3> Adapt(IFaceAttribute<Vector3> attribute)
		{
			return Vector3OffsetWrappedFaceAttribute.Create(edgeWrap, attribute, surfaceDescriptor);
		}

		public void OnBeforeSerialize()
		{
			_edgeWrapReference = (ScriptableObject)edgeWrap;
		}

		public void OnAfterDeserialize()
		{
			edgeWrap = (IEdgeAttribute<EdgeWrap>)_edgeWrapReference;
		}
	}
}
