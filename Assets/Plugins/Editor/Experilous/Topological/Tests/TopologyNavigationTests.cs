/******************************************************************************\
 *  Copyright (C) 2016 Experilous <againey@experilous.com>
 *  
 *  This file is subject to the terms and conditions defined in the file
 *  'Assets/Plugins/Experilous/License.txt', which is a part of this package.
 *
\******************************************************************************/

#if UNITY_5_3
using UnityEngine;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Experilous.Topological;

public class TopologyNavigationTests
{
	/*[Test]
	public void Something()
	{
		var builder = new Topology.HalfEdgeBuilder();
		builder.AddHalfEdge(0, 0);
		builder.AddHalfEdge(3, 1);
		builder.AddHalfEdge(1, 1);
		builder.AddHalfEdge(3, 2);
		builder.AddHalfEdge(2, 2);
		builder.AddHalfEdge(3, 0);
		builder.AddHalfEdge(2, 0);
		builder.AddHalfEdge(0, -1);
		builder.AddHalfEdge(0, 1);
		builder.AddHalfEdge(1, -1);
		builder.AddHalfEdge(1, 2);
		builder.AddHalfEdge(2, -1);
		builder.LinkTwins(0, 1);
		builder.LinkTwins(2, 3);
		builder.LinkTwins(4, 5);
		builder.LinkTwins(6, 7);
		builder.LinkTwins(8, 9);
		builder.LinkTwins(10, 11);
		builder.LinkToNext(0, 2, 9);
		builder.LinkToNext(1, 6, 4);
		builder.LinkToNext(2, 4, 11);
		builder.LinkToNext(3, 8, 0);
		builder.LinkToNext(4, 0, 7);
		builder.LinkToNext(5, 10, 2);
		builder.LinkToNext(6, 9, 10);
		builder.LinkToNext(7, 5, 1);
		builder.LinkToNext(8, 11, 6);
		builder.LinkToNext(9, 1, 3);
		builder.LinkToNext(10, 7, 8);
		builder.LinkToNext(11, 3, 5);
		var topology = builder.BuildTopology();

		Assert.AreEqual(4, topology.vertices.Count);
		Assert.AreEqual(12, topology.vertexEdges.Count);
		Assert.AreEqual(12, topology.faceEdges.Count);
		Assert.AreEqual(3, topology.faces.Count);

		Assert.AreEqual(topology.vertexEdges[2], topology.vertexEdges[0].next);
		Assert.AreEqual(topology.vertexEdges[6], topology.vertexEdges[1].next);
		Assert.AreEqual(topology.vertexEdges[4], topology.vertexEdges[2].next);
		Assert.AreEqual(topology.vertexEdges[8], topology.vertexEdges[3].next);
		Assert.AreEqual(topology.vertexEdges[0], topology.vertexEdges[4].next);
		Assert.AreEqual(topology.vertexEdges[10], topology.vertexEdges[5].next);
		Assert.AreEqual(topology.vertexEdges[9], topology.vertexEdges[6].next);
		Assert.AreEqual(topology.vertexEdges[5], topology.vertexEdges[7].next);
		Assert.AreEqual(topology.vertexEdges[11], topology.vertexEdges[8].next);
		Assert.AreEqual(topology.vertexEdges[1], topology.vertexEdges[9].next);
		Assert.AreEqual(topology.vertexEdges[7], topology.vertexEdges[10].next);
		Assert.AreEqual(topology.vertexEdges[3], topology.vertexEdges[11].next);

		Assert.AreEqual(topology.vertexEdges[4], topology.vertexEdges[0].prev);
		Assert.AreEqual(topology.vertexEdges[9], topology.vertexEdges[1].prev);
		Assert.AreEqual(topology.vertexEdges[0], topology.vertexEdges[2].prev);
		Assert.AreEqual(topology.vertexEdges[11], topology.vertexEdges[3].prev);
		Assert.AreEqual(topology.vertexEdges[2], topology.vertexEdges[4].prev);
		Assert.AreEqual(topology.vertexEdges[7], topology.vertexEdges[5].prev);
		Assert.AreEqual(topology.vertexEdges[1], topology.vertexEdges[6].prev);
		Assert.AreEqual(topology.vertexEdges[10], topology.vertexEdges[7].prev);
		Assert.AreEqual(topology.vertexEdges[3], topology.vertexEdges[8].prev);
		Assert.AreEqual(topology.vertexEdges[6], topology.vertexEdges[9].prev);
		Assert.AreEqual(topology.vertexEdges[5], topology.vertexEdges[10].prev);
		Assert.AreEqual(topology.vertexEdges[8], topology.vertexEdges[11].prev);

		Assert.AreEqual(topology.faceEdges[9], topology.faceEdges[0].next);
		Assert.AreEqual(topology.faceEdges[4], topology.faceEdges[1].next);
		Assert.AreEqual(topology.faceEdges[11], topology.faceEdges[2].next);
		Assert.AreEqual(topology.faceEdges[0], topology.faceEdges[3].next);
		Assert.AreEqual(topology.faceEdges[7], topology.faceEdges[4].next);
		Assert.AreEqual(topology.faceEdges[2], topology.faceEdges[5].next);
		Assert.AreEqual(topology.faceEdges[10], topology.faceEdges[6].next);
		Assert.AreEqual(topology.faceEdges[1], topology.faceEdges[7].next);
		Assert.AreEqual(topology.faceEdges[6], topology.faceEdges[8].next);
		Assert.AreEqual(topology.faceEdges[3], topology.faceEdges[9].next);
		Assert.AreEqual(topology.faceEdges[8], topology.faceEdges[10].next);
		Assert.AreEqual(topology.faceEdges[5], topology.faceEdges[11].next);

		Assert.AreEqual(topology.faceEdges[3], topology.faceEdges[0].prev);
		Assert.AreEqual(topology.faceEdges[7], topology.faceEdges[1].prev);
		Assert.AreEqual(topology.faceEdges[5], topology.faceEdges[2].prev);
		Assert.AreEqual(topology.faceEdges[9], topology.faceEdges[3].prev);
		Assert.AreEqual(topology.faceEdges[1], topology.faceEdges[4].prev);
		Assert.AreEqual(topology.faceEdges[11], topology.faceEdges[5].prev);
		Assert.AreEqual(topology.faceEdges[8], topology.faceEdges[6].prev);
		Assert.AreEqual(topology.faceEdges[4], topology.faceEdges[7].prev);
		Assert.AreEqual(topology.faceEdges[10], topology.faceEdges[8].prev);
		Assert.AreEqual(topology.faceEdges[0], topology.faceEdges[9].prev);
		Assert.AreEqual(topology.faceEdges[6], topology.faceEdges[10].prev);
		Assert.AreEqual(topology.faceEdges[2], topology.faceEdges[11].prev);
	}*/
}
#endif
