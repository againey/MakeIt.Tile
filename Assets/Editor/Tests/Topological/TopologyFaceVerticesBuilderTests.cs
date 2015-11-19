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

		Assert.AreEqual(3, topology.vertices.Count);
		Assert.AreEqual(6, topology.vertexEdges.Count);
		Assert.AreEqual(6, topology.faceEdges.Count);
		Assert.AreEqual(1, topology.faces.Count);

		Assert.AreEqual(2, topology.vertices[0].neighborCount);
		Assert.AreEqual(2, topology.vertices[1].neighborCount);
		Assert.AreEqual(2, topology.vertices[2].neighborCount);

		Assert.AreEqual(3, topology.faces[0].neighborCount);

		Topology.FaceEdge faceEdge;

		Assert.IsTrue(topology.faces[0].TryFindEdge(topology.vertices[0], out faceEdge));
		Assert.AreEqual(faceEdge, faceEdge.next.next.next);
		Assert.AreEqual(topology.vertices[0], faceEdge.nextVertex);
		Assert.AreEqual(topology.vertices[1], faceEdge.next.nextVertex);
		Assert.AreEqual(topology.vertices[2], faceEdge.next.next.nextVertex);
		Assert.IsTrue(faceEdge.isBoundary);
		Assert.IsTrue(faceEdge.next.isBoundary);
		Assert.IsTrue(faceEdge.next.next.isBoundary);
		Assert.IsTrue(faceEdge.twin.isBoundary);
		Assert.IsTrue(faceEdge.next.twin.isBoundary);
		Assert.IsTrue(faceEdge.next.next.twin.isBoundary);

		Topology.VertexEdge vertexEdge;

		Assert.IsTrue(topology.vertices[0].TryFindEdge(topology.vertices[1], out vertexEdge));
		Assert.AreEqual(vertexEdge, vertexEdge.next.next);
		Assert.AreEqual(topology.vertices[1], vertexEdge.farVertex);
		Assert.AreEqual(topology.vertices[2], vertexEdge.next.farVertex);

		Assert.IsTrue(topology.vertices[1].TryFindEdge(topology.vertices[2], out vertexEdge));
		Assert.AreEqual(vertexEdge, vertexEdge.next.next);
		Assert.AreEqual(topology.vertices[2], vertexEdge.farVertex);
		Assert.AreEqual(topology.vertices[0], vertexEdge.next.farVertex);

		Assert.IsTrue(topology.vertices[2].TryFindEdge(topology.vertices[0], out vertexEdge));
		Assert.AreEqual(vertexEdge, vertexEdge.next.next);
		Assert.AreEqual(topology.vertices[0], vertexEdge.farVertex);
		Assert.AreEqual(topology.vertices[1], vertexEdge.next.farVertex);
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

		Assert.AreEqual(4, topology.vertices.Count);
		Assert.AreEqual(12, topology.vertexEdges.Count);
		Assert.AreEqual(12, topology.faceEdges.Count);
		Assert.AreEqual(4, topology.faces.Count);

		Assert.AreEqual(3, topology.vertices[0].neighborCount);
		Assert.AreEqual(3, topology.vertices[1].neighborCount);
		Assert.AreEqual(3, topology.vertices[2].neighborCount);
		Assert.AreEqual(3, topology.vertices[3].neighborCount);

		Assert.AreEqual(3, topology.faces[0].neighborCount);
		Assert.AreEqual(3, topology.faces[1].neighborCount);
		Assert.AreEqual(3, topology.faces[2].neighborCount);
		Assert.AreEqual(3, topology.faces[3].neighborCount);

		Topology.FaceEdge faceEdge;

		Assert.IsTrue(topology.faces[0].TryFindEdge(topology.vertices[0], out faceEdge));
		Assert.AreEqual(faceEdge, faceEdge.next.next.next);
		Assert.AreEqual(topology.vertices[0], faceEdge.nextVertex);
		Assert.AreEqual(topology.vertices[1], faceEdge.next.nextVertex);
		Assert.AreEqual(topology.vertices[2], faceEdge.next.next.nextVertex);
		Assert.IsFalse(faceEdge.isBoundary);
		Assert.IsFalse(faceEdge.next.isBoundary);
		Assert.IsFalse(faceEdge.next.next.isBoundary);

		Assert.IsTrue(topology.faces[1].TryFindEdge(topology.vertices[0], out faceEdge));
		Assert.AreEqual(faceEdge, faceEdge.next.next.next);
		Assert.AreEqual(topology.vertices[0], faceEdge.nextVertex);
		Assert.AreEqual(topology.vertices[2], faceEdge.next.nextVertex);
		Assert.AreEqual(topology.vertices[3], faceEdge.next.next.nextVertex);
		Assert.IsFalse(faceEdge.isBoundary);
		Assert.IsFalse(faceEdge.next.isBoundary);
		Assert.IsFalse(faceEdge.next.next.isBoundary);

		Assert.IsTrue(topology.faces[2].TryFindEdge(topology.vertices[0], out faceEdge));
		Assert.AreEqual(faceEdge, faceEdge.next.next.next);
		Assert.AreEqual(topology.vertices[0], faceEdge.nextVertex);
		Assert.AreEqual(topology.vertices[3], faceEdge.next.nextVertex);
		Assert.AreEqual(topology.vertices[1], faceEdge.next.next.nextVertex);
		Assert.IsFalse(faceEdge.isBoundary);
		Assert.IsFalse(faceEdge.next.isBoundary);
		Assert.IsFalse(faceEdge.next.next.isBoundary);

		Assert.IsTrue(topology.faces[3].TryFindEdge(topology.vertices[1], out faceEdge));
		Assert.AreEqual(faceEdge, faceEdge.next.next.next);
		Assert.AreEqual(topology.vertices[1], faceEdge.nextVertex);
		Assert.AreEqual(topology.vertices[3], faceEdge.next.nextVertex);
		Assert.AreEqual(topology.vertices[2], faceEdge.next.next.nextVertex);
		Assert.IsFalse(faceEdge.isBoundary);
		Assert.IsFalse(faceEdge.next.isBoundary);
		Assert.IsFalse(faceEdge.next.next.isBoundary);

		Topology.VertexEdge vertexEdge;

		Assert.IsTrue(topology.vertices[0].TryFindEdge(topology.vertices[1], out vertexEdge));
		Assert.AreEqual(vertexEdge, vertexEdge.next.next.next);
		Assert.AreEqual(topology.vertices[1], vertexEdge.farVertex);
		Assert.AreEqual(topology.vertices[2], vertexEdge.next.farVertex);
		Assert.AreEqual(topology.vertices[3], vertexEdge.next.next.farVertex);

		Assert.IsTrue(topology.vertices[1].TryFindEdge(topology.vertices[0], out vertexEdge));
		Assert.AreEqual(vertexEdge, vertexEdge.next.next.next);
		Assert.AreEqual(topology.vertices[0], vertexEdge.farVertex);
		Assert.AreEqual(topology.vertices[3], vertexEdge.next.farVertex);
		Assert.AreEqual(topology.vertices[2], vertexEdge.next.next.farVertex);

		Assert.IsTrue(topology.vertices[2].TryFindEdge(topology.vertices[0], out vertexEdge));
		Assert.AreEqual(vertexEdge, vertexEdge.next.next.next);
		Assert.AreEqual(topology.vertices[0], vertexEdge.farVertex);
		Assert.AreEqual(topology.vertices[1], vertexEdge.next.farVertex);
		Assert.AreEqual(topology.vertices[3], vertexEdge.next.next.farVertex);

		Assert.IsTrue(topology.vertices[3].TryFindEdge(topology.vertices[0], out vertexEdge));
		Assert.AreEqual(vertexEdge, vertexEdge.next.next.next);
		Assert.AreEqual(topology.vertices[0], vertexEdge.farVertex);
		Assert.AreEqual(topology.vertices[2], vertexEdge.next.farVertex);
		Assert.AreEqual(topology.vertices[1], vertexEdge.next.next.farVertex);
	}

	[Test]
	public void TriangularCylinderTest()
	{
		var builder = new Topology.FaceVerticesBuilder();
		builder.AddFace(0, 1, 3, 2);
		builder.AddFace(2, 3, 5, 4);
		builder.AddFace(4, 5, 1, 0);
		var topology = builder.BuildTopology();

		Assert.AreEqual(6, topology.vertices.Count);
		Assert.AreEqual(18, topology.vertexEdges.Count);
		Assert.AreEqual(18, topology.faceEdges.Count);
		Assert.AreEqual(3, topology.faces.Count);

		Assert.AreEqual(3, topology.vertices[0].neighborCount);
		Assert.AreEqual(3, topology.vertices[1].neighborCount);
		Assert.AreEqual(3, topology.vertices[2].neighborCount);
		Assert.AreEqual(3, topology.vertices[3].neighborCount);
		Assert.AreEqual(3, topology.vertices[4].neighborCount);
		Assert.AreEqual(3, topology.vertices[5].neighborCount);

		Assert.AreEqual(4, topology.faces[0].neighborCount);
		Assert.AreEqual(4, topology.faces[1].neighborCount);
		Assert.AreEqual(4, topology.faces[2].neighborCount);

		Topology.FaceEdge faceEdge;

		Assert.IsTrue(topology.faces[0].TryFindEdge(topology.vertices[0], out faceEdge));
		Assert.AreEqual(faceEdge, faceEdge.next.next.next.next);
		Assert.AreEqual(topology.vertices[0], faceEdge.nextVertex);
		Assert.AreEqual(topology.vertices[1], faceEdge.next.nextVertex);
		Assert.AreEqual(topology.vertices[3], faceEdge.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[2], faceEdge.next.next.next.nextVertex);
		Assert.IsTrue(faceEdge.isBoundary);
		Assert.IsFalse(faceEdge.next.isBoundary);
		Assert.IsTrue(faceEdge.next.next.isBoundary);
		Assert.IsFalse(faceEdge.next.next.next.isBoundary);

		Assert.IsTrue(topology.faces[1].TryFindEdge(topology.vertices[2], out faceEdge));
		Assert.AreEqual(faceEdge, faceEdge.next.next.next.next);
		Assert.AreEqual(topology.vertices[2], faceEdge.nextVertex);
		Assert.AreEqual(topology.vertices[3], faceEdge.next.nextVertex);
		Assert.AreEqual(topology.vertices[5], faceEdge.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[4], faceEdge.next.next.next.nextVertex);
		Assert.IsTrue(faceEdge.isBoundary);
		Assert.IsFalse(faceEdge.next.isBoundary);
		Assert.IsTrue(faceEdge.next.next.isBoundary);
		Assert.IsFalse(faceEdge.next.next.next.isBoundary);

		Assert.IsTrue(topology.faces[2].TryFindEdge(topology.vertices[4], out faceEdge));
		Assert.AreEqual(faceEdge, faceEdge.next.next.next.next);
		Assert.AreEqual(topology.vertices[4], faceEdge.nextVertex);
		Assert.AreEqual(topology.vertices[5], faceEdge.next.nextVertex);
		Assert.AreEqual(topology.vertices[1], faceEdge.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[0], faceEdge.next.next.next.nextVertex);
		Assert.IsTrue(faceEdge.isBoundary);
		Assert.IsFalse(faceEdge.next.isBoundary);
		Assert.IsTrue(faceEdge.next.next.isBoundary);
		Assert.IsFalse(faceEdge.next.next.next.isBoundary);

		Topology.VertexEdge vertexEdge;

		Assert.IsTrue(topology.vertices[0].TryFindEdge(topology.vertices[1], out vertexEdge));
		Assert.AreEqual(vertexEdge, vertexEdge.next.next.next);
		Assert.AreEqual(topology.vertices[1], vertexEdge.farVertex);
		Assert.AreEqual(topology.vertices[2], vertexEdge.next.farVertex);
		Assert.AreEqual(topology.vertices[4], vertexEdge.next.next.farVertex);

		Assert.IsTrue(topology.vertices[1].TryFindEdge(topology.vertices[0], out vertexEdge));
		Assert.AreEqual(vertexEdge, vertexEdge.next.next.next);
		Assert.AreEqual(topology.vertices[0], vertexEdge.farVertex);
		Assert.AreEqual(topology.vertices[5], vertexEdge.next.farVertex);
		Assert.AreEqual(topology.vertices[3], vertexEdge.next.next.farVertex);

		Assert.IsTrue(topology.vertices[2].TryFindEdge(topology.vertices[3], out vertexEdge));
		Assert.AreEqual(vertexEdge, vertexEdge.next.next.next);
		Assert.AreEqual(topology.vertices[3], vertexEdge.farVertex);
		Assert.AreEqual(topology.vertices[4], vertexEdge.next.farVertex);
		Assert.AreEqual(topology.vertices[0], vertexEdge.next.next.farVertex);

		Assert.IsTrue(topology.vertices[3].TryFindEdge(topology.vertices[2], out vertexEdge));
		Assert.AreEqual(vertexEdge, vertexEdge.next.next.next);
		Assert.AreEqual(topology.vertices[2], vertexEdge.farVertex);
		Assert.AreEqual(topology.vertices[1], vertexEdge.next.farVertex);
		Assert.AreEqual(topology.vertices[5], vertexEdge.next.next.farVertex);

		Assert.IsTrue(topology.vertices[4].TryFindEdge(topology.vertices[5], out vertexEdge));
		Assert.AreEqual(vertexEdge, vertexEdge.next.next.next);
		Assert.AreEqual(topology.vertices[5], vertexEdge.farVertex);
		Assert.AreEqual(topology.vertices[0], vertexEdge.next.farVertex);
		Assert.AreEqual(topology.vertices[2], vertexEdge.next.next.farVertex);

		Assert.IsTrue(topology.vertices[5].TryFindEdge(topology.vertices[4], out vertexEdge));
		Assert.AreEqual(vertexEdge, vertexEdge.next.next.next);
		Assert.AreEqual(topology.vertices[4], vertexEdge.farVertex);
		Assert.AreEqual(topology.vertices[3], vertexEdge.next.farVertex);
		Assert.AreEqual(topology.vertices[1], vertexEdge.next.next.farVertex);
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
}