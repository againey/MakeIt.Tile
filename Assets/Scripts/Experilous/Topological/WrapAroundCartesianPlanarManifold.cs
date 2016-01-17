using UnityEngine;

namespace Experilous.Topological
{
#if false
	public class WrapAroundCartesianPlanarManifold : CartesianPlanarManifold
	{
		public EdgeWrapDataEdgeAttribute edgeWrapData;
		public Vector3OffsetWrappedVertexAttribute wrappedVertexPositions;

		public new static WrapAroundCartesianPlanarManifold CreateInstance()
		{
			return CreateInstance<WrapAroundCartesianPlanarManifold>();
		}

		public new static WrapAroundCartesianPlanarManifold CreateInstance(string name)
		{
			var instance = CreateInstance();
			instance.name = name;
			return instance;
		}

		public override object Clone()
		{
			return CreateInstance<WrapAroundCartesianPlanarManifold>().CloneFields(this);
		}

		protected WrapAroundCartesianPlanarManifold CloneFields(WrapAroundCartesianPlanarManifold original)
		{
			base.CloneFields(original);
			edgeWrapData = original.edgeWrapData.Clone();
			wrappedVertexPositions = original.wrappedVertexPositions.Clone();
			return this;
		}

		public new WrapAroundCartesianPlanarManifoldDescriptor descriptor
		{
			get { return (WrapAroundCartesianPlanarManifoldDescriptor)_descriptor; }
			set { _descriptor = value; }
		}
	}
#endif
}
