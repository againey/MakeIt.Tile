﻿/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

#if UNITY_5_3
using UnityEngine;
using NUnit.Framework;
using Experilous.Topological;

namespace Experilous.Topological.Tests
{
	public class TopologyNavigationTests
	{
		[Test]
		public void FaceNeighborFaces()
		{
			var surface = RectangularQuadGrid.Create(new PlanarDescriptor(Vector3.right, Vector3.up), new IntVector2(3, 3));
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

		[Test]
		public void FaceOutrVertexEdgeFaces()
		{
			var surface = RectangularQuadGrid.Create(new PlanarDescriptor(Vector3.right, Vector3.up), new IntVector2(3, 3));
			var topology = TopologyUtility.BuildTopology(surface);

			var face = topology.faces[surface.GetFaceIndex(1, 1)];
			var neighborFaceEdges = face.outerVertexEdges.GetEnumerator();

			Assert.True(neighborFaceEdges.MoveNext());
			Assert.AreEqual(surface.GetFaceIndex(1, 0), neighborFaceEdges.Current.prevFace.index);
			Assert.True(neighborFaceEdges.MoveNext());
			Assert.AreEqual(surface.GetFaceIndex(0, 0), neighborFaceEdges.Current.prevFace.index);
			Assert.True(neighborFaceEdges.MoveNext());
			Assert.AreEqual(surface.GetFaceIndex(0, 1), neighborFaceEdges.Current.prevFace.index);
			Assert.True(neighborFaceEdges.MoveNext());
			Assert.AreEqual(surface.GetFaceIndex(0, 2), neighborFaceEdges.Current.prevFace.index);
			Assert.True(neighborFaceEdges.MoveNext());
			Assert.AreEqual(surface.GetFaceIndex(1, 2), neighborFaceEdges.Current.prevFace.index);
			Assert.True(neighborFaceEdges.MoveNext());
			Assert.AreEqual(surface.GetFaceIndex(2, 2), neighborFaceEdges.Current.prevFace.index);
			Assert.True(neighborFaceEdges.MoveNext());
			Assert.AreEqual(surface.GetFaceIndex(2, 1), neighborFaceEdges.Current.prevFace.index);
			Assert.True(neighborFaceEdges.MoveNext());
			Assert.AreEqual(surface.GetFaceIndex(2, 0), neighborFaceEdges.Current.prevFace.index);
			Assert.False(neighborFaceEdges.MoveNext());
		}

		[Test]
		public void VertexNeighborVertices()
		{
			var surface = RectangularQuadGrid.Create(new PlanarDescriptor(Vector3.right, Vector3.up), new IntVector2(2, 2));
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

		[Test]
		public void VertexOuterFaceEdgeVertices()
		{
			var surface = RectangularQuadGrid.Create(new PlanarDescriptor(Vector3.right, Vector3.up), new IntVector2(2, 2));
			var topology = TopologyUtility.BuildTopology(surface);

			var vertex = topology.vertices[surface.GetVertexIndex(1, 1)];
			var neighborVertexEdges = vertex.outerFaceEdges.GetEnumerator();

			Assert.True(neighborVertexEdges.MoveNext());
			Assert.AreEqual(surface.GetVertexIndex(0, 0), neighborVertexEdges.Current.nextVertex.index);
			Assert.True(neighborVertexEdges.MoveNext());
			Assert.AreEqual(surface.GetVertexIndex(0, 1), neighborVertexEdges.Current.nextVertex.index);
			Assert.True(neighborVertexEdges.MoveNext());
			Assert.AreEqual(surface.GetVertexIndex(0, 2), neighborVertexEdges.Current.nextVertex.index);
			Assert.True(neighborVertexEdges.MoveNext());
			Assert.AreEqual(surface.GetVertexIndex(1, 2), neighborVertexEdges.Current.nextVertex.index);
			Assert.True(neighborVertexEdges.MoveNext());
			Assert.AreEqual(surface.GetVertexIndex(2, 2), neighborVertexEdges.Current.nextVertex.index);
			Assert.True(neighborVertexEdges.MoveNext());
			Assert.AreEqual(surface.GetVertexIndex(2, 1), neighborVertexEdges.Current.nextVertex.index);
			Assert.True(neighborVertexEdges.MoveNext());
			Assert.AreEqual(surface.GetVertexIndex(2, 0), neighborVertexEdges.Current.nextVertex.index);
			Assert.True(neighborVertexEdges.MoveNext());
			Assert.AreEqual(surface.GetVertexIndex(1, 0), neighborVertexEdges.Current.nextVertex.index);
			Assert.False(neighborVertexEdges.MoveNext());
		}
	}
}
#endif
