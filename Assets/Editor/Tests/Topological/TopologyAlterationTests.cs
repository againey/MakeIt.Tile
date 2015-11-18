using UnityEngine;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Experilous.Topological;

public class TopologyAlterationTests
{
	[Test]
	public void SpinEdgeForwardThrowsOnBoundaryVerticesWithTwoNeighbors()
	{
		var builder = new Topology.FaceVerticesBuilder();
		builder.AddFace(0, 1, 2);
		var topology = builder.BuildTopology();

		foreach (var edge in topology.vertexEdges)
		{
			Assert.Throws<InvalidOperationException>(() => { topology.SpinEdgeForward(edge); });
		}
	}

	[Test]
	public void SpinEdgeBackwardThrowsOnBoundaryVerticesWithTwoNeighbors()
	{
		var builder = new Topology.FaceVerticesBuilder();
		builder.AddFace(0, 1, 2);
		var topology = builder.BuildTopology();

		foreach (var edge in topology.vertexEdges)
		{
			Assert.Throws<InvalidOperationException>(() => { topology.SpinEdgeBackward(edge); });
		}
	}

	[Test]
	public void SpinEdgeForwardThrowsOnInteriorVerticesWithTwoNeighbors()
	{
		var builder = new Topology.FaceVerticesBuilder();
		builder.AddFace(0, 1, 2, 4);
		builder.AddFace(2, 3, 0, 4);
		var topology = builder.BuildTopology();

		Assert.Throws<InvalidOperationException>(() => { topology.SpinEdgeForward(topology.vertices[4].firstEdge); });
		Assert.Throws<InvalidOperationException>(() => { topology.SpinEdgeForward(topology.vertices[4].firstEdge.next); });
	}

	[Test]
	public void SpinEdgeBackwardThrowsOnInteriorVerticesWithTwoNeighbors()
	{
		var builder = new Topology.FaceVerticesBuilder();
		builder.AddFace(0, 1, 2, 4);
		builder.AddFace(2, 3, 0, 4);
		var topology = builder.BuildTopology();

		Assert.Throws<InvalidOperationException>(() => { topology.SpinEdgeBackward(topology.vertices[4].firstEdge); });
		Assert.Throws<InvalidOperationException>(() => { topology.SpinEdgeBackward(topology.vertices[4].firstEdge.next); });
	}

	[Test]
	public void SpinEdgeForwardThrowsOnBoundaryVerticesWithThreeNeighbors()
	{
		var builder = new Topology.FaceVerticesBuilder();
		builder.AddFace(0, 1, 3);
		builder.AddFace(1, 2, 3);
		builder.AddFace(2, 0, 3);
		var topology = builder.BuildTopology();

		Topology.VertexEdge vertexEdge;

		Assert.IsTrue(topology.vertices[0].TryFindEdge(topology.vertices[1], out vertexEdge));
		Assert.Throws<InvalidOperationException>(() => { topology.SpinEdgeForward(vertexEdge); });

		Assert.IsTrue(topology.vertices[1].TryFindEdge(topology.vertices[2], out vertexEdge));
		Assert.Throws<InvalidOperationException>(() => { topology.SpinEdgeForward(vertexEdge); });

		Assert.IsTrue(topology.vertices[2].TryFindEdge(topology.vertices[0], out vertexEdge));
		Assert.Throws<InvalidOperationException>(() => { topology.SpinEdgeForward(vertexEdge); });
	}

	[Test]
	public void SpinEdgeBackwardThrowsOnBoundaryVerticesWithThreeNeighbors()
	{
		var builder = new Topology.FaceVerticesBuilder();
		builder.AddFace(0, 1, 3);
		builder.AddFace(1, 2, 3);
		builder.AddFace(2, 0, 3);
		var topology = builder.BuildTopology();

		Topology.VertexEdge vertexEdge;

		Assert.IsTrue(topology.vertices[0].TryFindEdge(topology.vertices[1], out vertexEdge));
		Assert.Throws<InvalidOperationException>(() => { topology.SpinEdgeBackward(vertexEdge); });

		Assert.IsTrue(topology.vertices[1].TryFindEdge(topology.vertices[2], out vertexEdge));
		Assert.Throws<InvalidOperationException>(() => { topology.SpinEdgeBackward(vertexEdge); });

		Assert.IsTrue(topology.vertices[2].TryFindEdge(topology.vertices[0], out vertexEdge));
		Assert.Throws<InvalidOperationException>(() => { topology.SpinEdgeBackward(vertexEdge); });
	}

	[Test]
	public void SpinEdgeBetweenTrianglesForward()
	{
		//   1         1
		//  /^\       /P\
		// 0P|N3 --> 0-->3
		//  \|/       \N/
		//   2         2

		var builder = new Topology.FaceVerticesBuilder();
		builder.AddFace(0, 1, 2);
		builder.AddFace(3, 2, 1);
		var topology = builder.BuildTopology();

		Topology.VertexEdge edgeToSpin;
		Assert.IsTrue(topology.vertices[2].TryFindEdge(topology.vertices[1], out edgeToSpin));

		topology.SpinEdgeForward(edgeToSpin);

		TopologyTests.CheckVerticesForInvalidEdgeCycles(topology);
		TopologyTests.CheckFacesForInvalidEdgeCycles(topology);

		Assert.AreEqual(3, topology.vertices[0].neighborCount);
		Assert.AreEqual(2, topology.vertices[1].neighborCount);
		Assert.AreEqual(2, topology.vertices[2].neighborCount);
		Assert.AreEqual(3, topology.vertices[3].neighborCount);

		Assert.AreEqual(3, topology.faces[0].neighborCount);
		Assert.AreEqual(3, topology.faces[1].neighborCount);

		Assert.AreEqual(topology.vertices[0], edgeToSpin.nearVertex);
		Assert.AreEqual(topology.vertices[3], edgeToSpin.farVertex);
		Assert.AreEqual(topology.faces[0], edgeToSpin.prevFace);
		Assert.AreEqual(topology.faces[1], edgeToSpin.nextFace);

		Assert.AreEqual(topology.vertices[1], edgeToSpin.prev.farVertex);
		Assert.AreEqual(topology.vertices[2], edgeToSpin.next.farVertex);
		Assert.AreEqual(topology.vertices[2], edgeToSpin.twin.prev.farVertex);
		Assert.AreEqual(topology.vertices[1], edgeToSpin.twin.next.farVertex);

		Assert.AreEqual(topology.vertices[3], edgeToSpin.faceEdge.nextVertex);
		Assert.AreEqual(topology.vertices[2], edgeToSpin.faceEdge.next.nextVertex);
		Assert.AreEqual(topology.vertices[0], edgeToSpin.faceEdge.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[3], edgeToSpin.faceEdge.next.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[0], edgeToSpin.twin.faceEdge.nextVertex);
		Assert.AreEqual(topology.vertices[1], edgeToSpin.twin.faceEdge.next.nextVertex);
		Assert.AreEqual(topology.vertices[3], edgeToSpin.twin.faceEdge.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[0], edgeToSpin.twin.faceEdge.next.next.next.nextVertex);
	}

	[Test]
	public void SpinEdgeBetweenTrianglesBackward()
	{
		//   1         1
		//  /^\       /N\
		// 0P|N3 --> 0<--3
		//  \|/       \P/
		//   2         2

		var builder = new Topology.FaceVerticesBuilder();
		builder.AddFace(0, 1, 2);
		builder.AddFace(3, 2, 1);
		var topology = builder.BuildTopology();

		Topology.VertexEdge edgeToSpin;
		Assert.IsTrue(topology.vertices[2].TryFindEdge(topology.vertices[1], out edgeToSpin));

		topology.SpinEdgeBackward(edgeToSpin);

		TopologyTests.CheckVerticesForInvalidEdgeCycles(topology);
		TopologyTests.CheckFacesForInvalidEdgeCycles(topology);

		Assert.AreEqual(3, topology.vertices[0].neighborCount);
		Assert.AreEqual(2, topology.vertices[1].neighborCount);
		Assert.AreEqual(2, topology.vertices[2].neighborCount);
		Assert.AreEqual(3, topology.vertices[3].neighborCount);

		Assert.AreEqual(3, topology.faces[0].neighborCount);
		Assert.AreEqual(3, topology.faces[1].neighborCount);

		Assert.AreEqual(topology.vertices[3], edgeToSpin.nearVertex);
		Assert.AreEqual(topology.vertices[0], edgeToSpin.farVertex);
		Assert.AreEqual(topology.faces[0], edgeToSpin.prevFace);
		Assert.AreEqual(topology.faces[1], edgeToSpin.nextFace);

		Assert.AreEqual(topology.vertices[2], edgeToSpin.prev.farVertex);
		Assert.AreEqual(topology.vertices[1], edgeToSpin.next.farVertex);
		Assert.AreEqual(topology.vertices[1], edgeToSpin.twin.prev.farVertex);
		Assert.AreEqual(topology.vertices[2], edgeToSpin.twin.next.farVertex);

		Assert.AreEqual(topology.vertices[0], edgeToSpin.faceEdge.nextVertex);
		Assert.AreEqual(topology.vertices[1], edgeToSpin.faceEdge.next.nextVertex);
		Assert.AreEqual(topology.vertices[3], edgeToSpin.faceEdge.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[0], edgeToSpin.faceEdge.next.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[3], edgeToSpin.twin.faceEdge.nextVertex);
		Assert.AreEqual(topology.vertices[2], edgeToSpin.twin.faceEdge.next.nextVertex);
		Assert.AreEqual(topology.vertices[0], edgeToSpin.twin.faceEdge.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[3], edgeToSpin.twin.faceEdge.next.next.next.nextVertex);
	}

	[Test]
	public void SpinEdgeBetweenSquaresForward()
	{
		// 0---1     0---1
		// | P |     |\ P|
		// 3-->2 --> 3 \ 2
		// | N |     |N v|
		// 4---5     4---5

		var builder = new Topology.FaceVerticesBuilder();
		builder.AddFace(0, 1, 2, 3);
		builder.AddFace(5, 4, 3, 2);
		var topology = builder.BuildTopology();

		Topology.VertexEdge edgeToSpin;
		Assert.IsTrue(topology.vertices[3].TryFindEdge(topology.vertices[2], out edgeToSpin));

		topology.SpinEdgeForward(edgeToSpin);

		TopologyTests.CheckVerticesForInvalidEdgeCycles(topology);
		TopologyTests.CheckFacesForInvalidEdgeCycles(topology);

		Assert.AreEqual(3, topology.vertices[0].neighborCount);
		Assert.AreEqual(2, topology.vertices[1].neighborCount);
		Assert.AreEqual(2, topology.vertices[2].neighborCount);
		Assert.AreEqual(2, topology.vertices[3].neighborCount);
		Assert.AreEqual(2, topology.vertices[4].neighborCount);
		Assert.AreEqual(3, topology.vertices[5].neighborCount);

		Assert.AreEqual(4, topology.faces[0].neighborCount);
		Assert.AreEqual(4, topology.faces[1].neighborCount);

		Assert.AreEqual(topology.vertices[0], edgeToSpin.nearVertex);
		Assert.AreEqual(topology.vertices[5], edgeToSpin.farVertex);
		Assert.AreEqual(topology.faces[0], edgeToSpin.prevFace);
		Assert.AreEqual(topology.faces[1], edgeToSpin.nextFace);

		Assert.AreEqual(topology.vertices[1], edgeToSpin.prev.farVertex);
		Assert.AreEqual(topology.vertices[3], edgeToSpin.next.farVertex);
		Assert.AreEqual(topology.vertices[4], edgeToSpin.twin.prev.farVertex);
		Assert.AreEqual(topology.vertices[2], edgeToSpin.twin.next.farVertex);

		Assert.AreEqual(topology.vertices[5], edgeToSpin.faceEdge.nextVertex);
		Assert.AreEqual(topology.vertices[4], edgeToSpin.faceEdge.next.nextVertex);
		Assert.AreEqual(topology.vertices[3], edgeToSpin.faceEdge.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[0], edgeToSpin.faceEdge.next.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[5], edgeToSpin.faceEdge.next.next.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[0], edgeToSpin.twin.faceEdge.nextVertex);
		Assert.AreEqual(topology.vertices[1], edgeToSpin.twin.faceEdge.next.nextVertex);
		Assert.AreEqual(topology.vertices[2], edgeToSpin.twin.faceEdge.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[5], edgeToSpin.twin.faceEdge.next.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[0], edgeToSpin.twin.faceEdge.next.next.next.next.nextVertex);
	}

	[Test]
	public void SpinEdgeBetweenSquaresBackward()
	{
		// 0---1     0---1
		// | P |     |P ^|
		// 3-->2 --> 3 / 2
		// | N |     |/ N|
		// 4---5     4---5

		var builder = new Topology.FaceVerticesBuilder();
		builder.AddFace(0, 1, 2, 3);
		builder.AddFace(5, 4, 3, 2);
		var topology = builder.BuildTopology();

		Topology.VertexEdge edgeToSpin;
		Assert.IsTrue(topology.vertices[3].TryFindEdge(topology.vertices[2], out edgeToSpin));

		topology.SpinEdgeBackward(edgeToSpin);

		TopologyTests.CheckVerticesForInvalidEdgeCycles(topology);
		TopologyTests.CheckFacesForInvalidEdgeCycles(topology);

		Assert.AreEqual(2, topology.vertices[0].neighborCount);
		Assert.AreEqual(3, topology.vertices[1].neighborCount);
		Assert.AreEqual(2, topology.vertices[2].neighborCount);
		Assert.AreEqual(2, topology.vertices[3].neighborCount);
		Assert.AreEqual(3, topology.vertices[4].neighborCount);
		Assert.AreEqual(2, topology.vertices[5].neighborCount);

		Assert.AreEqual(4, topology.faces[0].neighborCount);
		Assert.AreEqual(4, topology.faces[1].neighborCount);

		Assert.AreEqual(topology.vertices[4], edgeToSpin.nearVertex);
		Assert.AreEqual(topology.vertices[1], edgeToSpin.farVertex);
		Assert.AreEqual(topology.faces[0], edgeToSpin.prevFace);
		Assert.AreEqual(topology.faces[1], edgeToSpin.nextFace);

		Assert.AreEqual(topology.vertices[3], edgeToSpin.prev.farVertex);
		Assert.AreEqual(topology.vertices[5], edgeToSpin.next.farVertex);
		Assert.AreEqual(topology.vertices[2], edgeToSpin.twin.prev.farVertex);
		Assert.AreEqual(topology.vertices[0], edgeToSpin.twin.next.farVertex);

		Assert.AreEqual(topology.vertices[1], edgeToSpin.faceEdge.nextVertex);
		Assert.AreEqual(topology.vertices[2], edgeToSpin.faceEdge.next.nextVertex);
		Assert.AreEqual(topology.vertices[5], edgeToSpin.faceEdge.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[4], edgeToSpin.faceEdge.next.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[1], edgeToSpin.faceEdge.next.next.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[4], edgeToSpin.twin.faceEdge.nextVertex);
		Assert.AreEqual(topology.vertices[3], edgeToSpin.twin.faceEdge.next.nextVertex);
		Assert.AreEqual(topology.vertices[0], edgeToSpin.twin.faceEdge.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[1], edgeToSpin.twin.faceEdge.next.next.next.nextVertex);
		Assert.AreEqual(topology.vertices[4], edgeToSpin.twin.faceEdge.next.next.next.next.nextVertex);
	}
}
