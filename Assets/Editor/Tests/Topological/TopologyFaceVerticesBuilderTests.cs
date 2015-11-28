using UnityEngine;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Experilous.Topological;

public class TopologyFaceVerticesBuilderTests
{
	[Test]
	public void TriangleTest()
	{
		var builder = new Topology.FaceVerticesBuilder();
		builder.AddFace(0, 1, 2);
		var topology = builder.BuildTopology();

		Assert.AreEqual(3, topology.internalVertices.Count);
		Assert.AreEqual(6, topology.vertexEdges.Count);
		Assert.AreEqual(6, topology.faceEdges.Count);
		Assert.AreEqual(1, topology.internalFaces.Count);

		Assert.AreEqual(2, topology.internalVertices[0].neighborCount);
		Assert.AreEqual(2, topology.internalVertices[1].neighborCount);
		Assert.AreEqual(2, topology.internalVertices[2].neighborCount);

		Assert.AreEqual(3, topology.internalFaces[0].neighborCount);

		Topology.FaceEdge faceEdge;

		Assert.IsTrue(topology.internalFaces[0].TryFindEdge(topology.internalVertices[0], out faceEdge));
		Assert.AreEqual(faceEdge, faceEdge.next.next.next);
		Assert.AreEqual(topology.internalVertices[0], faceEdge.nextVertex);
		Assert.AreEqual(topology.internalVertices[1], faceEdge.next.nextVertex);
		Assert.AreEqual(topology.internalVertices[2], faceEdge.next.next.nextVertex);
		Assert.IsTrue(faceEdge.isBoundary);
		Assert.IsTrue(faceEdge.next.isBoundary);
		Assert.IsTrue(faceEdge.next.next.isBoundary);
		Assert.IsTrue(faceEdge.twin.isBoundary);
		Assert.IsTrue(faceEdge.next.twin.isBoundary);
		Assert.IsTrue(faceEdge.next.next.twin.isBoundary);

		Topology.VertexEdge vertexEdge;

		Assert.IsTrue(topology.internalVertices[0].TryFindEdge(topology.internalVertices[1], out vertexEdge));
		Assert.AreEqual(vertexEdge, vertexEdge.next.next);
		Assert.AreEqual(topology.internalVertices[1], vertexEdge.farVertex);
		Assert.AreEqual(topology.internalVertices[2], vertexEdge.next.farVertex);

		Assert.IsTrue(topology.internalVertices[1].TryFindEdge(topology.internalVertices[2], out vertexEdge));
		Assert.AreEqual(vertexEdge, vertexEdge.next.next);
		Assert.AreEqual(topology.internalVertices[2], vertexEdge.farVertex);
		Assert.AreEqual(topology.internalVertices[0], vertexEdge.next.farVertex);

		Assert.IsTrue(topology.internalVertices[2].TryFindEdge(topology.internalVertices[0], out vertexEdge));
		Assert.AreEqual(vertexEdge, vertexEdge.next.next);
		Assert.AreEqual(topology.internalVertices[0], vertexEdge.farVertex);
		Assert.AreEqual(topology.internalVertices[1], vertexEdge.next.farVertex);
	}

	[Test]
	public void TetrahedronTest()
	{
		var builder = new Topology.FaceVerticesBuilder();
		builder.AddFace(0, 1, 2);
		builder.AddFace(0, 2, 3);
		builder.AddFace(0, 3, 1);
		builder.AddFace(1, 3, 2);
		var topology = builder.BuildTopology();

		Assert.AreEqual(4, topology.internalVertices.Count);
		Assert.AreEqual(12, topology.vertexEdges.Count);
		Assert.AreEqual(12, topology.faceEdges.Count);
		Assert.AreEqual(4, topology.internalFaces.Count);

		Assert.AreEqual(3, topology.internalVertices[0].neighborCount);
		Assert.AreEqual(3, topology.internalVertices[1].neighborCount);
		Assert.AreEqual(3, topology.internalVertices[2].neighborCount);
		Assert.AreEqual(3, topology.internalVertices[3].neighborCount);

		Assert.AreEqual(3, topology.internalFaces[0].neighborCount);
		Assert.AreEqual(3, topology.internalFaces[1].neighborCount);
		Assert.AreEqual(3, topology.internalFaces[2].neighborCount);
		Assert.AreEqual(3, topology.internalFaces[3].neighborCount);

		Topology.FaceEdge faceEdge;

		Assert.IsTrue(topology.internalFaces[0].TryFindEdge(topology.internalVertices[0], out faceEdge));
		Assert.AreEqual(faceEdge, faceEdge.next.next.next);
		Assert.AreEqual(topology.internalVertices[0], faceEdge.nextVertex);
		Assert.AreEqual(topology.internalVertices[1], faceEdge.next.nextVertex);
		Assert.AreEqual(topology.internalVertices[2], faceEdge.next.next.nextVertex);
		Assert.IsFalse(faceEdge.isBoundary);
		Assert.IsFalse(faceEdge.next.isBoundary);
		Assert.IsFalse(faceEdge.next.next.isBoundary);

		Assert.IsTrue(topology.internalFaces[1].TryFindEdge(topology.internalVertices[0], out faceEdge));
		Assert.AreEqual(faceEdge, faceEdge.next.next.next);
		Assert.AreEqual(topology.internalVertices[0], faceEdge.nextVertex);
		Assert.AreEqual(topology.internalVertices[2], faceEdge.next.nextVertex);
		Assert.AreEqual(topology.internalVertices[3], faceEdge.next.next.nextVertex);
		Assert.IsFalse(faceEdge.isBoundary);
		Assert.IsFalse(faceEdge.next.isBoundary);
		Assert.IsFalse(faceEdge.next.next.isBoundary);

		Assert.IsTrue(topology.internalFaces[2].TryFindEdge(topology.internalVertices[0], out faceEdge));
		Assert.AreEqual(faceEdge, faceEdge.next.next.next);
		Assert.AreEqual(topology.internalVertices[0], faceEdge.nextVertex);
		Assert.AreEqual(topology.internalVertices[3], faceEdge.next.nextVertex);
		Assert.AreEqual(topology.internalVertices[1], faceEdge.next.next.nextVertex);
		Assert.IsFalse(faceEdge.isBoundary);
		Assert.IsFalse(faceEdge.next.isBoundary);
		Assert.IsFalse(faceEdge.next.next.isBoundary);

		Assert.IsTrue(topology.internalFaces[3].TryFindEdge(topology.internalVertices[1], out faceEdge));
		Assert.AreEqual(faceEdge, faceEdge.next.next.next);
		Assert.AreEqual(topology.internalVertices[1], faceEdge.nextVertex);
		Assert.AreEqual(topology.internalVertices[3], faceEdge.next.nextVertex);
		Assert.AreEqual(topology.internalVertices[2], faceEdge.next.next.nextVertex);
		Assert.IsFalse(faceEdge.isBoundary);
		Assert.IsFalse(faceEdge.next.isBoundary);
		Assert.IsFalse(faceEdge.next.next.isBoundary);

		Topology.VertexEdge vertexEdge;

		Assert.IsTrue(topology.internalVertices[0].TryFindEdge(topology.internalVertices[1], out vertexEdge));
		Assert.AreEqual(vertexEdge, vertexEdge.next.next.next);
		Assert.AreEqual(topology.internalVertices[1], vertexEdge.farVertex);
		Assert.AreEqual(topology.internalVertices[2], vertexEdge.next.farVertex);
		Assert.AreEqual(topology.internalVertices[3], vertexEdge.next.next.farVertex);

		Assert.IsTrue(topology.internalVertices[1].TryFindEdge(topology.internalVertices[0], out vertexEdge));
		Assert.AreEqual(vertexEdge, vertexEdge.next.next.next);
		Assert.AreEqual(topology.internalVertices[0], vertexEdge.farVertex);
		Assert.AreEqual(topology.internalVertices[3], vertexEdge.next.farVertex);
		Assert.AreEqual(topology.internalVertices[2], vertexEdge.next.next.farVertex);

		Assert.IsTrue(topology.internalVertices[2].TryFindEdge(topology.internalVertices[0], out vertexEdge));
		Assert.AreEqual(vertexEdge, vertexEdge.next.next.next);
		Assert.AreEqual(topology.internalVertices[0], vertexEdge.farVertex);
		Assert.AreEqual(topology.internalVertices[1], vertexEdge.next.farVertex);
		Assert.AreEqual(topology.internalVertices[3], vertexEdge.next.next.farVertex);

		Assert.IsTrue(topology.internalVertices[3].TryFindEdge(topology.internalVertices[0], out vertexEdge));
		Assert.AreEqual(vertexEdge, vertexEdge.next.next.next);
		Assert.AreEqual(topology.internalVertices[0], vertexEdge.farVertex);
		Assert.AreEqual(topology.internalVertices[2], vertexEdge.next.farVertex);
		Assert.AreEqual(topology.internalVertices[1], vertexEdge.next.next.farVertex);
	}

	[Test]
	public void TriangularCylinderTest()
	{
		var builder = new Topology.FaceVerticesBuilder();
		builder.AddFace(0, 1, 3, 2);
		builder.AddFace(2, 3, 5, 4);
		builder.AddFace(4, 5, 1, 0);
		var topology = builder.BuildTopology();

		Assert.AreEqual(6, topology.internalVertices.Count);
		Assert.AreEqual(18, topology.vertexEdges.Count);
		Assert.AreEqual(18, topology.faceEdges.Count);
		Assert.AreEqual(3, topology.internalFaces.Count);

		Assert.AreEqual(3, topology.internalVertices[0].neighborCount);
		Assert.AreEqual(3, topology.internalVertices[1].neighborCount);
		Assert.AreEqual(3, topology.internalVertices[2].neighborCount);
		Assert.AreEqual(3, topology.internalVertices[3].neighborCount);
		Assert.AreEqual(3, topology.internalVertices[4].neighborCount);
		Assert.AreEqual(3, topology.internalVertices[5].neighborCount);

		Assert.AreEqual(4, topology.internalFaces[0].neighborCount);
		Assert.AreEqual(4, topology.internalFaces[1].neighborCount);
		Assert.AreEqual(4, topology.internalFaces[2].neighborCount);

		Topology.FaceEdge faceEdge;

		Assert.IsTrue(topology.internalFaces[0].TryFindEdge(topology.internalVertices[0], out faceEdge));
		Assert.AreEqual(faceEdge, faceEdge.next.next.next.next);
		Assert.AreEqual(topology.internalVertices[0], faceEdge.nextVertex);
		Assert.AreEqual(topology.internalVertices[1], faceEdge.next.nextVertex);
		Assert.AreEqual(topology.internalVertices[3], faceEdge.next.next.nextVertex);
		Assert.AreEqual(topology.internalVertices[2], faceEdge.next.next.next.nextVertex);
		Assert.IsTrue(faceEdge.isBoundary);
		Assert.IsFalse(faceEdge.next.isBoundary);
		Assert.IsTrue(faceEdge.next.next.isBoundary);
		Assert.IsFalse(faceEdge.next.next.next.isBoundary);

		Assert.IsTrue(topology.internalFaces[1].TryFindEdge(topology.internalVertices[2], out faceEdge));
		Assert.AreEqual(faceEdge, faceEdge.next.next.next.next);
		Assert.AreEqual(topology.internalVertices[2], faceEdge.nextVertex);
		Assert.AreEqual(topology.internalVertices[3], faceEdge.next.nextVertex);
		Assert.AreEqual(topology.internalVertices[5], faceEdge.next.next.nextVertex);
		Assert.AreEqual(topology.internalVertices[4], faceEdge.next.next.next.nextVertex);
		Assert.IsTrue(faceEdge.isBoundary);
		Assert.IsFalse(faceEdge.next.isBoundary);
		Assert.IsTrue(faceEdge.next.next.isBoundary);
		Assert.IsFalse(faceEdge.next.next.next.isBoundary);

		Assert.IsTrue(topology.internalFaces[2].TryFindEdge(topology.internalVertices[4], out faceEdge));
		Assert.AreEqual(faceEdge, faceEdge.next.next.next.next);
		Assert.AreEqual(topology.internalVertices[4], faceEdge.nextVertex);
		Assert.AreEqual(topology.internalVertices[5], faceEdge.next.nextVertex);
		Assert.AreEqual(topology.internalVertices[1], faceEdge.next.next.nextVertex);
		Assert.AreEqual(topology.internalVertices[0], faceEdge.next.next.next.nextVertex);
		Assert.IsTrue(faceEdge.isBoundary);
		Assert.IsFalse(faceEdge.next.isBoundary);
		Assert.IsTrue(faceEdge.next.next.isBoundary);
		Assert.IsFalse(faceEdge.next.next.next.isBoundary);

		Topology.VertexEdge vertexEdge;

		Assert.IsTrue(topology.internalVertices[0].TryFindEdge(topology.internalVertices[1], out vertexEdge));
		Assert.AreEqual(vertexEdge, vertexEdge.next.next.next);
		Assert.AreEqual(topology.internalVertices[1], vertexEdge.farVertex);
		Assert.AreEqual(topology.internalVertices[2], vertexEdge.next.farVertex);
		Assert.AreEqual(topology.internalVertices[4], vertexEdge.next.next.farVertex);

		Assert.IsTrue(topology.internalVertices[1].TryFindEdge(topology.internalVertices[0], out vertexEdge));
		Assert.AreEqual(vertexEdge, vertexEdge.next.next.next);
		Assert.AreEqual(topology.internalVertices[0], vertexEdge.farVertex);
		Assert.AreEqual(topology.internalVertices[5], vertexEdge.next.farVertex);
		Assert.AreEqual(topology.internalVertices[3], vertexEdge.next.next.farVertex);

		Assert.IsTrue(topology.internalVertices[2].TryFindEdge(topology.internalVertices[3], out vertexEdge));
		Assert.AreEqual(vertexEdge, vertexEdge.next.next.next);
		Assert.AreEqual(topology.internalVertices[3], vertexEdge.farVertex);
		Assert.AreEqual(topology.internalVertices[4], vertexEdge.next.farVertex);
		Assert.AreEqual(topology.internalVertices[0], vertexEdge.next.next.farVertex);

		Assert.IsTrue(topology.internalVertices[3].TryFindEdge(topology.internalVertices[2], out vertexEdge));
		Assert.AreEqual(vertexEdge, vertexEdge.next.next.next);
		Assert.AreEqual(topology.internalVertices[2], vertexEdge.farVertex);
		Assert.AreEqual(topology.internalVertices[1], vertexEdge.next.farVertex);
		Assert.AreEqual(topology.internalVertices[5], vertexEdge.next.next.farVertex);

		Assert.IsTrue(topology.internalVertices[4].TryFindEdge(topology.internalVertices[5], out vertexEdge));
		Assert.AreEqual(vertexEdge, vertexEdge.next.next.next);
		Assert.AreEqual(topology.internalVertices[5], vertexEdge.farVertex);
		Assert.AreEqual(topology.internalVertices[0], vertexEdge.next.farVertex);
		Assert.AreEqual(topology.internalVertices[2], vertexEdge.next.next.farVertex);

		Assert.IsTrue(topology.internalVertices[5].TryFindEdge(topology.internalVertices[4], out vertexEdge));
		Assert.AreEqual(vertexEdge, vertexEdge.next.next.next);
		Assert.AreEqual(topology.internalVertices[4], vertexEdge.farVertex);
		Assert.AreEqual(topology.internalVertices[3], vertexEdge.next.farVertex);
		Assert.AreEqual(topology.internalVertices[1], vertexEdge.next.next.farVertex);
	}

	[Test]
	public void BadInputTest_0()
	{
		var builder = new Topology.FaceVerticesBuilder();
		builder.AddFace(2, 0, 4);
		builder.AddFace(3, 4, 2);
		builder.AddFace(3, 1, 5);
		builder.AddFace(2, 5, 3);
		builder.AddFace(4, 6, 2);
		builder.AddFace(4, 3, 7);
		builder.AddFace(5, 7, 3);
		builder.AddFace(5, 2, 6);
		Assert.Throws<InvalidOperationException>(() => builder.BuildTopology());
	}

	[Test]
	public void ChokepointRingTest()
	{
		var builder = new Topology.FaceVerticesBuilder();
		builder.AddFace(0, 3, 1, 4);
		builder.AddFace(1, 5, 2, 6);
		builder.AddFace(2, 7, 0, 8);
		var topology = builder.BuildTopology();

		Assert.AreEqual(9, topology.vertices.Count);
		Assert.AreEqual(9, topology.internalVertices.Count);
		Assert.AreEqual(0, topology.externalVertices.Count);

		Assert.AreEqual(24, topology.vertexEdges.Count);
		Assert.AreEqual(24, topology.faceEdges.Count);

		Assert.AreEqual(5, topology.faces.Count);
		Assert.AreEqual(3, topology.internalFaces.Count);
		Assert.AreEqual(2, topology.externalFaces.Count);

		Assert.AreEqual(4, topology.vertices[0].neighborCount);
		Assert.AreEqual(4, topology.vertices[1].neighborCount);
		Assert.AreEqual(4, topology.vertices[2].neighborCount);
		Assert.AreEqual(2, topology.vertices[3].neighborCount);
		Assert.AreEqual(2, topology.vertices[4].neighborCount);
		Assert.AreEqual(2, topology.vertices[5].neighborCount);
		Assert.AreEqual(2, topology.vertices[6].neighborCount);
		Assert.AreEqual(2, topology.vertices[7].neighborCount);
		Assert.AreEqual(2, topology.vertices[8].neighborCount);

		Assert.AreEqual(4, topology.internalFaces[0].neighborCount);
		Assert.AreEqual(4, topology.internalFaces[1].neighborCount);
		Assert.AreEqual(4, topology.internalFaces[2].neighborCount);

		Assert.AreEqual(6, topology.externalFaces[0].neighborCount);
		Assert.AreEqual(6, topology.externalFaces[1].neighborCount);

		Topology.FaceEdge faceEdge;

		Topology.Face upperFace;
		Topology.Face lowerFace;
		if (topology.externalFaces[0].TryFindEdge(topology.vertices[1], out faceEdge))
		{
			upperFace = topology.externalFaces[0];
			lowerFace = topology.externalFaces[1];
		}
		else
		{
			upperFace = topology.externalFaces[1];
			lowerFace = topology.externalFaces[0];
		}

		Assert.IsTrue(upperFace.TryFindEdge(topology.vertices[0], out faceEdge));
		Assert.AreEqual(topology.vertices[0], faceEdge.nextVertex);
		Assert.AreEqual(topology.vertices[7], faceEdge.next.nextVertex);
		Assert.AreEqual(topology.vertices[2], faceEdge.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[5], faceEdge.next.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[1], faceEdge.next.next.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[3], faceEdge.next.next.next.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[0], faceEdge.next.next.next.next.next.next.nextVertex);

		Assert.IsTrue(lowerFace.TryFindEdge(topology.vertices[0], out faceEdge));
		Assert.AreEqual(topology.vertices[0], faceEdge.nextVertex);
		Assert.AreEqual(topology.vertices[4], faceEdge.next.nextVertex);
		Assert.AreEqual(topology.vertices[1], faceEdge.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[6], faceEdge.next.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[2], faceEdge.next.next.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[8], faceEdge.next.next.next.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[0], faceEdge.next.next.next.next.next.next.nextVertex);

		Topology.VertexEdge vertexEdge;

		Assert.IsTrue(topology.vertices[0].TryFindEdge(upperFace, out vertexEdge));
		Assert.AreEqual(topology.vertices[3], vertexEdge.farVertex);
		Assert.AreEqual(topology.vertices[4], vertexEdge.next.farVertex);
		Assert.AreEqual(topology.vertices[8], vertexEdge.next.next.farVertex);
		Assert.AreEqual(topology.vertices[7], vertexEdge.next.next.next.farVertex);
		Assert.AreEqual(topology.vertices[3], vertexEdge.next.next.next.next.farVertex);

		Assert.IsTrue(topology.vertices[1].TryFindEdge(upperFace, out vertexEdge));
		Assert.AreEqual(topology.vertices[5], vertexEdge.farVertex);
		Assert.AreEqual(topology.vertices[6], vertexEdge.next.farVertex);
		Assert.AreEqual(topology.vertices[4], vertexEdge.next.next.farVertex);
		Assert.AreEqual(topology.vertices[3], vertexEdge.next.next.next.farVertex);
		Assert.AreEqual(topology.vertices[5], vertexEdge.next.next.next.next.farVertex);

		Assert.IsTrue(topology.vertices[2].TryFindEdge(upperFace, out vertexEdge));
		Assert.AreEqual(topology.vertices[7], vertexEdge.farVertex);
		Assert.AreEqual(topology.vertices[8], vertexEdge.next.farVertex);
		Assert.AreEqual(topology.vertices[6], vertexEdge.next.next.farVertex);
		Assert.AreEqual(topology.vertices[5], vertexEdge.next.next.next.farVertex);
		Assert.AreEqual(topology.vertices[7], vertexEdge.next.next.next.next.farVertex);
	}

	[Test]
	public void ChokepointTetrahedronTest()
	{
		var builder = new Topology.FaceVerticesBuilder();
		builder.AddFace(0, 4, 1, 5);
		builder.AddFace(0, 6, 2, 7);
		builder.AddFace(0, 8, 3, 9);
		builder.AddFace(1, 10, 2, 11);
		builder.AddFace(2, 12, 3, 13);
		builder.AddFace(3, 14, 1, 15);
		var topology = builder.BuildTopology();

		Assert.AreEqual(16, topology.vertices.Count);
		Assert.AreEqual(16, topology.internalVertices.Count);
		Assert.AreEqual(0, topology.externalVertices.Count);

		Assert.AreEqual(48, topology.vertexEdges.Count);
		Assert.AreEqual(48, topology.faceEdges.Count);

		Assert.AreEqual(10, topology.faces.Count);
		Assert.AreEqual(6, topology.internalFaces.Count);
		Assert.AreEqual(4, topology.externalFaces.Count);

		for (int i = 0; i < 4; ++i)
		{
			Assert.AreEqual(6, topology.vertices[i].neighborCount);
		}

		for (int i = 4; i < 16; ++i)
		{
			Assert.AreEqual(2, topology.vertices[i].neighborCount);
		}

		Assert.AreEqual(4, topology.internalFaces[0].neighborCount);
		Assert.AreEqual(4, topology.internalFaces[1].neighborCount);
		Assert.AreEqual(4, topology.internalFaces[2].neighborCount);
		Assert.AreEqual(4, topology.internalFaces[3].neighborCount);
		Assert.AreEqual(4, topology.internalFaces[4].neighborCount);
		Assert.AreEqual(4, topology.internalFaces[5].neighborCount);

		Assert.AreEqual(6, topology.externalFaces[0].neighborCount);
		Assert.AreEqual(6, topology.externalFaces[1].neighborCount);
		Assert.AreEqual(6, topology.externalFaces[2].neighborCount);
		Assert.AreEqual(6, topology.externalFaces[3].neighborCount);

		TopologyTests.AssertVertexNeighborVertices(topology.vertices[0], 4, 5, 6, 7, 8, 9);
		TopologyTests.AssertVertexNeighborVertices(topology.vertices[1], 4, 15, 14, 10, 11, 5);
		TopologyTests.AssertVertexNeighborVertices(topology.vertices[2], 6, 11, 10, 12, 13, 7);
		TopologyTests.AssertVertexNeighborVertices(topology.vertices[3], 8, 13, 12, 14, 15, 9);

		TopologyTests.AssertFaceNeighborVertices(topology, 0, 6, 2, 11, 1, 5);
		TopologyTests.AssertFaceNeighborVertices(topology, 0, 4, 1, 15, 3, 9);
		TopologyTests.AssertFaceNeighborVertices(topology, 0, 8, 3, 13, 2, 7);
		TopologyTests.AssertFaceNeighborVertices(topology, 1, 14, 3, 12, 2, 10);
	}

	[Test]
	public void HugeCylinderTest()
	{
		var builder = new Topology.FaceVerticesBuilder();
		for (int i = 0; i < 4095; ++i)
		{
			var i2 = i * 2;
			builder.AddFace(i2, i2 + 2, i2 + 3, i2 + 1);
		}
		builder.AddFace(8190, 0, 1, 8191);
		var topology = builder.BuildTopology();

		Assert.AreEqual(8192, topology.vertices.Count);
		Assert.AreEqual(8192, topology.internalVertices.Count);
		Assert.AreEqual(0, topology.externalVertices.Count);

		Assert.AreEqual(24576, topology.vertexEdges.Count);
		Assert.AreEqual(24576, topology.faceEdges.Count);

		Assert.AreEqual(4098, topology.faces.Count);
		Assert.AreEqual(4096, topology.internalFaces.Count);
		Assert.AreEqual(2, topology.externalFaces.Count);

		for (int i = 0; i < topology.internalFaces.Count; ++i)
		{
			Assert.AreEqual(4, topology.internalFaces[i].neighborCount);
		}

		Assert.AreEqual(4096, topology.externalFaces[0].neighborCount);
		Assert.AreEqual(4096, topology.externalFaces[1].neighborCount);
	}
}
