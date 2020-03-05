/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

#if UNITY_5_3_OR_NEWER
using UnityEngine;
using NUnit.Framework;
using MakeIt.Numerics;

namespace MakeIt.Tile.Tests
{
	class TopologyNavigationTests
	{
		[TestCase(Category = "Normal")]
		public void FaceNeighborFaces()
		{
			var surface = RectangularQuadGrid.Create(Vector2.right, Vector2.up, Vector3.zero, Quaternion.identity, false, false, new IntVector2(3, 3));
			var topology = TopologyUtility.BuildTopology(surface);

			var face = topology.faces[surface.GetFaceIndex(1, 1)];
			var neighborFaceEdges = face.edges.GetEnumerator();

			Assert.True(neighborFaceEdges.MoveNext());
			Assert.AreEqual(surface.GetFaceIndex(1, 0), neighborFaceEdges.Current.farFace.index);
			Assert.True(neighborFaceEdges.MoveNext());
			Assert.AreEqual(surface.GetFaceIndex(0, 1), neighborFaceEdges.Current.farFace.index);
			Assert.True(neighborFaceEdges.MoveNext());
			Assert.AreEqual(surface.GetFaceIndex(1, 2), neighborFaceEdges.Current.farFace.index);
			Assert.True(neighborFaceEdges.MoveNext());
			Assert.AreEqual(surface.GetFaceIndex(2, 1), neighborFaceEdges.Current.farFace.index);
			Assert.False(neighborFaceEdges.MoveNext());
		}

		[TestCase(Category = "Normal")]
		public void FaceOuterVertexEdgeFaces()
		{
			var surface = RectangularQuadGrid.Create(Vector2.right, Vector2.up, Vector3.zero, Quaternion.identity, false, false, new IntVector2(3, 3));
			var topology = TopologyUtility.BuildTopology(surface);

			var face = topology.faces[surface.GetFaceIndex(1, 1)];
			var neighborFaceEdges = face.outerEdges.GetEnumerator();

			Assert.True(neighborFaceEdges.MoveNext());
			Assert.AreEqual(surface.GetFaceIndex(1, 0), neighborFaceEdges.Current.face.index);
			Assert.True(neighborFaceEdges.MoveNext());
			Assert.AreEqual(surface.GetFaceIndex(0, 0), neighborFaceEdges.Current.face.index);
			Assert.True(neighborFaceEdges.MoveNext());
			Assert.AreEqual(surface.GetFaceIndex(0, 1), neighborFaceEdges.Current.face.index);
			Assert.True(neighborFaceEdges.MoveNext());
			Assert.AreEqual(surface.GetFaceIndex(0, 2), neighborFaceEdges.Current.face.index);
			Assert.True(neighborFaceEdges.MoveNext());
			Assert.AreEqual(surface.GetFaceIndex(1, 2), neighborFaceEdges.Current.face.index);
			Assert.True(neighborFaceEdges.MoveNext());
			Assert.AreEqual(surface.GetFaceIndex(2, 2), neighborFaceEdges.Current.face.index);
			Assert.True(neighborFaceEdges.MoveNext());
			Assert.AreEqual(surface.GetFaceIndex(2, 1), neighborFaceEdges.Current.face.index);
			Assert.True(neighborFaceEdges.MoveNext());
			Assert.AreEqual(surface.GetFaceIndex(2, 0), neighborFaceEdges.Current.face.index);
			Assert.False(neighborFaceEdges.MoveNext());
		}

		[TestCase(Category = "Normal")]
		public void VertexNeighborVertices()
		{
			var surface = RectangularQuadGrid.Create(Vector2.right, Vector2.up, Vector3.zero, Quaternion.identity, false, false, new IntVector2(2, 2));
			var topology = TopologyUtility.BuildTopology(surface);

			var vertex = topology.vertices[surface.GetVertexIndex(1, 1)];
			var neighborVertexEdges = vertex.edges.GetEnumerator();

			Assert.True(neighborVertexEdges.MoveNext());
			Assert.AreEqual(surface.GetVertexIndex(1, 0), neighborVertexEdges.Current.farVertex.index);
			Assert.True(neighborVertexEdges.MoveNext());
			Assert.AreEqual(surface.GetVertexIndex(0, 1), neighborVertexEdges.Current.farVertex.index);
			Assert.True(neighborVertexEdges.MoveNext());
			Assert.AreEqual(surface.GetVertexIndex(1, 2), neighborVertexEdges.Current.farVertex.index);
			Assert.True(neighborVertexEdges.MoveNext());
			Assert.AreEqual(surface.GetVertexIndex(2, 1), neighborVertexEdges.Current.farVertex.index);
			Assert.False(neighborVertexEdges.MoveNext());
		}

		[TestCase(Category = "Normal")]
		public void VertexOuterFaceEdgeVertices()
		{
			var surface = RectangularQuadGrid.Create(Vector2.right, Vector2.up, Vector3.zero, Quaternion.identity, false, false, new IntVector2(2, 2));
			var topology = TopologyUtility.BuildTopology(surface);

			var vertex = topology.vertices[surface.GetVertexIndex(1, 1)];
			var neighborVertexEdges = vertex.outerEdges.GetEnumerator();

			Assert.True(neighborVertexEdges.MoveNext());
			Assert.AreEqual(surface.GetVertexIndex(0, 0), neighborVertexEdges.Current.vertex.index);
			Assert.True(neighborVertexEdges.MoveNext());
			Assert.AreEqual(surface.GetVertexIndex(0, 1), neighborVertexEdges.Current.vertex.index);
			Assert.True(neighborVertexEdges.MoveNext());
			Assert.AreEqual(surface.GetVertexIndex(0, 2), neighborVertexEdges.Current.vertex.index);
			Assert.True(neighborVertexEdges.MoveNext());
			Assert.AreEqual(surface.GetVertexIndex(1, 2), neighborVertexEdges.Current.vertex.index);
			Assert.True(neighborVertexEdges.MoveNext());
			Assert.AreEqual(surface.GetVertexIndex(2, 2), neighborVertexEdges.Current.vertex.index);
			Assert.True(neighborVertexEdges.MoveNext());
			Assert.AreEqual(surface.GetVertexIndex(2, 1), neighborVertexEdges.Current.vertex.index);
			Assert.True(neighborVertexEdges.MoveNext());
			Assert.AreEqual(surface.GetVertexIndex(2, 0), neighborVertexEdges.Current.vertex.index);
			Assert.True(neighborVertexEdges.MoveNext());
			Assert.AreEqual(surface.GetVertexIndex(1, 0), neighborVertexEdges.Current.vertex.index);
			Assert.False(neighborVertexEdges.MoveNext());
		}
	}
}
#endif
