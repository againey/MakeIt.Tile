#if false
using UnityEngine;
using System.Collections.Generic;
using Experilous.Numerics;
using Experilous.MakeItTile;
using Experilous.Topologies;

namespace Experilous.Examples.MakeItTile
{
#if UNITY_EDITOR
	[ExecuteInEditMode]
	[RequireComponent(typeof(GeneratorExecutive))]
	public class HexGridGenerator : MonoBehaviour, IGenerator
	{
		private RectangularHexGrid _surface;
		private Topology _topology;
		private IVertexAttribute<Vector3> _vertexPositions;
		private IFaceAttribute<Vector3> _faceCentroids;
		private IEdgeAttribute<Vector3> _edgeMidpoints;

		public RectangularHexGrid surface { get { return _surface; } }
		public Topology topology { get { return _topology; } }
		public IVertexAttribute<Vector3> vertexPositions { get { return _vertexPositions; } }
		public IFaceAttribute<Vector3> faceCentroids { get { return _faceCentroids; } }
		public IEdgeAttribute<Vector3> edgeMidpoints { get { return _edgeMidpoints; } }

		public IEnumerable<IGenerator> dependencies
		{
			get
			{
				yield break;
			}
		}

		public void Generate()
		{
			_surface = RectangularHexGrid.Create(HexGridDescriptor.standardCornerUp, Vector3.zero, Quaternion.identity, false, false, new IntVector2(20, 20));

			Vector3[] vertexPositionsArray;
			_topology = _surface.CreateManifold(out vertexPositionsArray);

			_vertexPositions = vertexPositionsArray.AsVertexAttribute();
			_faceCentroids = FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(_topology.internalFaces, _vertexPositions);
			_edgeMidpoints = EdgeAttributeUtility.CalculateVertexEdgeMidpointsFromVertexPositions(_topology.vertexEdges, _vertexPositions);
		}
	}
#endif
}
#endif