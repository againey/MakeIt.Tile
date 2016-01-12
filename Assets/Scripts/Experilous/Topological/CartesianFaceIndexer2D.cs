using UnityEngine;

namespace Experilous.Topological
{
	public class CartesianFaceIndexer2D : FaceIndexer2D
	{
		[SerializeField] private Topology _topology;
		[SerializeField] private int _horizontalStride;
		[SerializeField] private int _verticalStride;
		[SerializeField] private int _originIndex;

		public static CartesianFaceIndexer2D CreateInstance(Topology topology, int horizontalStride, int verticalStride, int originIndex)
		{
			var indexer = CreateInstance<CartesianFaceIndexer2D>();
			indexer._topology = topology;
			indexer._horizontalStride = horizontalStride;
			indexer._verticalStride = verticalStride;
			indexer._originIndex = originIndex;
			return indexer;
		}

		public static CartesianFaceIndexer2D CreateInstance(Topology topology, int horizontalStride, int verticalStride, int originIndex, string name)
		{
			var indexer = CreateInstance(topology, horizontalStride, verticalStride, originIndex);
			indexer.name = name;
			return indexer;
		}

		public override Topology.Face GetFace(Coordinate coordinate)
		{
			return _topology.faces[GetFaceIndex(coordinate)];
		}

		public override Topology.Face GetFace(int x, int y)
		{
			return _topology.faces[GetFaceIndex(x, y)];
		}

		public override int GetFaceIndex(int x, int y)
		{
			return x * _horizontalStride + y * _verticalStride + _originIndex;
		}

		public override int GetFaceIndex(Coordinate coordinate)
		{
			return coordinate.x * _horizontalStride + coordinate.y * _verticalStride + _originIndex;
		}

		public override Coordinate GetCoordinate(Topology.Face face)
		{
			return GetCoordinate(face.index);
		}

		public override Coordinate GetCoordinate(int faceIndex)
		{
			throw new System.NotImplementedException();
		}
	}
}
