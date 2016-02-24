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

public class TopologyTests
{
#if false
	public static void CheckVerticesForInvalidEdgeCycles(Topology topology)
	{
		foreach (var vertex in topology.vertices)
		{
			var edge = vertex.firstEdge.next;
			for (int i = 1; i < vertex.neighborCount; ++i)
			{
				Assert.AreNotEqual(vertex.firstEdge, edge, string.Format("The cycle of edges around vertex {0} returned back to the first edge earlier than was expected, {1} iterations rather than {2}.", vertex.index, i, vertex.neighborCount));
				edge = edge.next;
			}
			Assert.AreEqual(vertex.firstEdge, edge, string.Format("The cycle of edges around vertex {0}  did not return back to the first edge in the {1} iterations expected.", vertex.index, vertex.neighborCount));
		}
	}

	public static void CheckFacesForInvalidEdgeCycles(Topology topology)
	{
		foreach (var face in topology.faces)
		{
			var edge = face.firstEdge.next;
			for (int i = 1; i < face.neighborCount; ++i)
			{
				Assert.AreNotEqual(face.firstEdge, edge, string.Format("The cycle of edges around face {0} returned back to the first edge earlier than was expected, {1} iterations rather than {2}.", face.index, i, face.neighborCount));
				edge = edge.next;
			}
			Assert.AreEqual(face.firstEdge, edge, string.Format("The cycle of edges around face {0} did not return back to the first edge in the {1} iterations expected.", face.index, face.neighborCount));
		}
	}

	public static void AssertVertexNeighborVertices(Topology.Vertex vertex, params int[] neighborVertexIndices)
	{
		var topology = vertex.topology;
		Topology.VertexEdge edge;
		Assert.IsTrue(vertex.TryFindEdge(topology.vertices[neighborVertexIndices[0]], out edge));
		for (int i = 0; i < neighborVertexIndices.Length; ++i)
		{
			Assert.AreEqual(topology.vertices[neighborVertexIndices[i]], edge.farVertex);
			edge = edge.next;
		}
		Assert.AreEqual(topology.vertices[neighborVertexIndices[0]], edge.farVertex);
	}

	public static void AssertFaceNeighborVertices(Topology.Face face, params int[] neighborVertexIndices)
	{
		var topology = face.topology;
		Topology.FaceEdge edge;
		Assert.IsTrue(face.TryFindEdge(topology.vertices[neighborVertexIndices[0]], out edge));
		for (int i = 0; i < neighborVertexIndices.Length; ++i)
		{
			Assert.AreEqual(topology.vertices[neighborVertexIndices[i]], edge.nextVertex);
			edge = edge.next;
		}
		Assert.AreEqual(topology.vertices[neighborVertexIndices[0]], edge.nextVertex);
	}

	public static void AssertFaceNeighborVertices(Topology topology, params int[] neighborVertexIndices)
	{
		Topology.VertexEdge edge;
		Assert.IsTrue(topology.vertices[neighborVertexIndices[0]].TryFindEdge(topology.vertices[neighborVertexIndices[1]], out edge));
		AssertFaceNeighborVertices(edge.prevFace, neighborVertexIndices);
	}

	[Test]
	public void TetrahedronComponentCountValidation()
	{
		var tetrahedron = SphericalManifoldUtility.CreateTetrahedron();
		Assert.AreEqual(4, tetrahedron.vertices.Count);
		Assert.AreEqual(12, tetrahedron.vertexEdges.Count);
		Assert.AreEqual(12, tetrahedron.faceEdges.Count);
		Assert.AreEqual(4, tetrahedron.internalFaces.Count);
	}

	[Test]
	public void TetrahedronComponentNeighborCountValidation()
	{
		var tetrahedron = SphericalManifoldUtility.CreateTetrahedron();
		Assert.AreEqual(3, tetrahedron.vertices[0].neighborCount);
		Assert.AreEqual(3, tetrahedron.vertices[1].neighborCount);
		Assert.AreEqual(3, tetrahedron.vertices[2].neighborCount);
		Assert.AreEqual(3, tetrahedron.vertices[3].neighborCount);
		Assert.AreEqual(3, tetrahedron.internalFaces[0].neighborCount);
		Assert.AreEqual(3, tetrahedron.internalFaces[1].neighborCount);
		Assert.AreEqual(3, tetrahedron.internalFaces[2].neighborCount);
		Assert.AreEqual(3, tetrahedron.internalFaces[3].neighborCount);
	}

	[Test]
	public void TetrahedronVertexNeighborValidation()
	{
		var tetrahedron = SphericalManifoldUtility.CreateTetrahedron();

		var vertexEdges = tetrahedron.vertices[0].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(1, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(2, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(3, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = tetrahedron.vertices[1].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(0, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(3, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(2, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = tetrahedron.vertices[2].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(0, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(1, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(3, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = tetrahedron.vertices[3].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(0, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(2, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(1, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());
	}

	[Test]
	public void TetrahedronFaceNeighborValidation()
	{
		var tetrahedron = SphericalManifoldUtility.CreateTetrahedron();

		var vertex0 = tetrahedron.vertices[0];
		var vertexEdge = vertex0.firstEdge;
		var vertex1 = vertexEdge.farVertex;
		var vertex2 = vertexEdge.next.farVertex;
		var faceEdge = vertexEdge.faceEdge;
		Assert.AreEqual(vertex1, faceEdge.nextVertex);
		Assert.AreEqual(vertex2, faceEdge.next.nextVertex);
		Assert.AreEqual(vertex0, faceEdge.next.next.nextVertex);
		Assert.AreEqual(vertexEdge.next, faceEdge.next.next.twin);

		vertexEdge = vertexEdge.next;
		vertex1 = vertexEdge.farVertex;
		vertex2 = vertexEdge.next.farVertex;
		faceEdge = vertexEdge.faceEdge;
		Assert.AreEqual(vertex1, faceEdge.nextVertex);
		Assert.AreEqual(vertex2, faceEdge.next.nextVertex);
		Assert.AreEqual(vertex0, faceEdge.next.next.nextVertex);
		Assert.AreEqual(vertexEdge.next, faceEdge.next.next.twin);

		vertexEdge = vertexEdge.next;
		vertex1 = vertexEdge.farVertex;
		vertex2 = vertexEdge.next.farVertex;
		faceEdge = vertexEdge.faceEdge;
		Assert.AreEqual(vertex1, faceEdge.nextVertex);
		Assert.AreEqual(vertex2, faceEdge.next.nextVertex);
		Assert.AreEqual(vertex0, faceEdge.next.next.nextVertex);
		Assert.AreEqual(vertexEdge.next, faceEdge.next.next.twin);

		vertex0 = tetrahedron.vertices[1];
		vertexEdge = vertex0.firstEdge;
		vertex1 = vertexEdge.farVertex;
		vertex2 = vertexEdge.next.farVertex;
		faceEdge = vertexEdge.faceEdge;
		Assert.AreEqual(vertex1, faceEdge.nextVertex);
		Assert.AreEqual(vertex2, faceEdge.next.nextVertex);
		Assert.AreEqual(vertex0, faceEdge.next.next.nextVertex);
		Assert.AreEqual(vertexEdge.next, faceEdge.next.next.twin);

		vertexEdge = vertexEdge.next;
		vertex1 = vertexEdge.farVertex;
		vertex2 = vertexEdge.next.farVertex;
		faceEdge = vertexEdge.faceEdge;
		Assert.AreEqual(vertex1, faceEdge.nextVertex);
		Assert.AreEqual(vertex2, faceEdge.next.nextVertex);
		Assert.AreEqual(vertex0, faceEdge.next.next.nextVertex);
		Assert.AreEqual(vertexEdge.next, faceEdge.next.next.twin);

		vertexEdge = vertexEdge.next;
		vertex1 = vertexEdge.farVertex;
		vertex2 = vertexEdge.next.farVertex;
		faceEdge = vertexEdge.faceEdge;
		Assert.AreEqual(vertex1, faceEdge.nextVertex);
		Assert.AreEqual(vertex2, faceEdge.next.nextVertex);
		Assert.AreEqual(vertex0, faceEdge.next.next.nextVertex);
		Assert.AreEqual(vertexEdge.next, faceEdge.next.next.twin);

		vertex0 = tetrahedron.vertices[2];
		vertexEdge = vertex0.firstEdge;
		vertex1 = vertexEdge.farVertex;
		vertex2 = vertexEdge.next.farVertex;
		faceEdge = vertexEdge.faceEdge;
		Assert.AreEqual(vertex1, faceEdge.nextVertex);
		Assert.AreEqual(vertex2, faceEdge.next.nextVertex);
		Assert.AreEqual(vertex0, faceEdge.next.next.nextVertex);
		Assert.AreEqual(vertexEdge.next, faceEdge.next.next.twin);

		vertexEdge = vertexEdge.next;
		vertex1 = vertexEdge.farVertex;
		vertex2 = vertexEdge.next.farVertex;
		faceEdge = vertexEdge.faceEdge;
		Assert.AreEqual(vertex1, faceEdge.nextVertex);
		Assert.AreEqual(vertex2, faceEdge.next.nextVertex);
		Assert.AreEqual(vertex0, faceEdge.next.next.nextVertex);
		Assert.AreEqual(vertexEdge.next, faceEdge.next.next.twin);

		vertexEdge = vertexEdge.next;
		vertex1 = vertexEdge.farVertex;
		vertex2 = vertexEdge.next.farVertex;
		faceEdge = vertexEdge.faceEdge;
		Assert.AreEqual(vertex1, faceEdge.nextVertex);
		Assert.AreEqual(vertex2, faceEdge.next.nextVertex);
		Assert.AreEqual(vertex0, faceEdge.next.next.nextVertex);
		Assert.AreEqual(vertexEdge.next, faceEdge.next.next.twin);

		vertex0 = tetrahedron.vertices[3];
		vertexEdge = vertex0.firstEdge;
		vertex1 = vertexEdge.farVertex;
		vertex2 = vertexEdge.next.farVertex;
		faceEdge = vertexEdge.faceEdge;
		Assert.AreEqual(vertex1, faceEdge.nextVertex);
		Assert.AreEqual(vertex2, faceEdge.next.nextVertex);
		Assert.AreEqual(vertex0, faceEdge.next.next.nextVertex);
		Assert.AreEqual(vertexEdge.next, faceEdge.next.next.twin);

		vertexEdge = vertexEdge.next;
		vertex1 = vertexEdge.farVertex;
		vertex2 = vertexEdge.next.farVertex;
		faceEdge = vertexEdge.faceEdge;
		Assert.AreEqual(vertex1, faceEdge.nextVertex);
		Assert.AreEqual(vertex2, faceEdge.next.nextVertex);
		Assert.AreEqual(vertex0, faceEdge.next.next.nextVertex);
		Assert.AreEqual(vertexEdge.next, faceEdge.next.next.twin);

		vertexEdge = vertexEdge.next;
		vertex1 = vertexEdge.farVertex;
		vertex2 = vertexEdge.next.farVertex;
		faceEdge = vertexEdge.faceEdge;
		Assert.AreEqual(vertex1, faceEdge.nextVertex);
		Assert.AreEqual(vertex2, faceEdge.next.nextVertex);
		Assert.AreEqual(vertex0, faceEdge.next.next.nextVertex);
		Assert.AreEqual(vertexEdge.next, faceEdge.next.next.twin);
	}

	[Test]
	public void HexehedronComponentCountValidation()
	{
		var hexahedron = SphericalManifoldUtility.CreateCube();
		Assert.AreEqual(8, hexahedron.vertices.Count);
		Assert.AreEqual(24, hexahedron.vertexEdges.Count);
		Assert.AreEqual(24, hexahedron.faceEdges.Count);
		Assert.AreEqual(6, hexahedron.internalFaces.Count);
	}

	[Test]
	public void HexehedronComponentNeighborCountValidation()
	{
		var hexahedron = SphericalManifoldUtility.CreateCube();
		Assert.AreEqual(3, hexahedron.vertices[0].neighborCount);
		Assert.AreEqual(3, hexahedron.vertices[1].neighborCount);
		Assert.AreEqual(3, hexahedron.vertices[2].neighborCount);
		Assert.AreEqual(3, hexahedron.vertices[3].neighborCount);
		Assert.AreEqual(3, hexahedron.vertices[4].neighborCount);
		Assert.AreEqual(3, hexahedron.vertices[5].neighborCount);
		Assert.AreEqual(3, hexahedron.vertices[6].neighborCount);
		Assert.AreEqual(3, hexahedron.vertices[7].neighborCount);
		Assert.AreEqual(4, hexahedron.internalFaces[0].neighborCount);
		Assert.AreEqual(4, hexahedron.internalFaces[1].neighborCount);
		Assert.AreEqual(4, hexahedron.internalFaces[2].neighborCount);
		Assert.AreEqual(4, hexahedron.internalFaces[3].neighborCount);
		Assert.AreEqual(4, hexahedron.internalFaces[4].neighborCount);
		Assert.AreEqual(4, hexahedron.internalFaces[5].neighborCount);
	}

	[Test]
	public void HexehedronVertexNeighborValidation()
	{
		var hexahedron = SphericalManifoldUtility.CreateCube();

		var vertexEdges = hexahedron.vertices[0].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(1, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(3, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(4, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = hexahedron.vertices[1].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(0, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(5, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(2, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = hexahedron.vertices[2].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(1, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(6, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(3, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = hexahedron.vertices[3].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(0, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(2, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(7, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = hexahedron.vertices[4].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(0, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(7, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(5, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = hexahedron.vertices[5].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(1, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(4, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(6, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = hexahedron.vertices[6].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(2, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(5, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(7, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = hexahedron.vertices[7].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(3, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(6, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(4, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());
	}

	[Test]
	public void OctahedronComponentCountValidation()
	{
		var octahedron = SphericalManifoldUtility.CreateOctahedron();
		Assert.AreEqual(6, octahedron.vertices.Count);
		Assert.AreEqual(24, octahedron.vertexEdges.Count);
		Assert.AreEqual(24, octahedron.faceEdges.Count);
		Assert.AreEqual(8, octahedron.internalFaces.Count);
	}

	[Test]
	public void OctahedronComponentNeighborCountValidation()
	{
		var octahedron = SphericalManifoldUtility.CreateOctahedron();
		Assert.AreEqual(4, octahedron.vertices[0].neighborCount);
		Assert.AreEqual(4, octahedron.vertices[1].neighborCount);
		Assert.AreEqual(4, octahedron.vertices[2].neighborCount);
		Assert.AreEqual(4, octahedron.vertices[3].neighborCount);
		Assert.AreEqual(4, octahedron.vertices[4].neighborCount);
		Assert.AreEqual(4, octahedron.vertices[5].neighborCount);
		Assert.AreEqual(3, octahedron.internalFaces[0].neighborCount);
		Assert.AreEqual(3, octahedron.internalFaces[1].neighborCount);
		Assert.AreEqual(3, octahedron.internalFaces[2].neighborCount);
		Assert.AreEqual(3, octahedron.internalFaces[3].neighborCount);
		Assert.AreEqual(3, octahedron.internalFaces[4].neighborCount);
		Assert.AreEqual(3, octahedron.internalFaces[5].neighborCount);
		Assert.AreEqual(3, octahedron.internalFaces[6].neighborCount);
		Assert.AreEqual(3, octahedron.internalFaces[7].neighborCount);
	}

	[Test]
	public void OctahedronVertexNeighborValidation()
	{
		var octahedron = SphericalManifoldUtility.CreateOctahedron();

		var vertexEdges = octahedron.vertices[0].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(1, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(2, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(3, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(4, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = octahedron.vertices[1].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(0, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(4, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(5, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(2, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = octahedron.vertices[2].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(0, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(1, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(5, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(3, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = octahedron.vertices[3].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(0, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(2, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(5, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(4, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = octahedron.vertices[4].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(0, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(3, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(5, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(1, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = octahedron.vertices[5].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(1, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(4, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(3, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(2, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());
	}

	[Test]
	public void IcosahedronComponentCountValidation()
	{
		var icosahedron = SphericalManifoldUtility.CreateIcosahedron();
		Assert.AreEqual(12, icosahedron.vertices.Count);
		Assert.AreEqual(60, icosahedron.vertexEdges.Count);
		Assert.AreEqual(60, icosahedron.faceEdges.Count);
		Assert.AreEqual(20, icosahedron.internalFaces.Count);
	}

	[Test]
	public void IcosahedronComponentNeighborCountValidation()
	{
		var icosahedron = SphericalManifoldUtility.CreateIcosahedron();
		Assert.AreEqual(5, icosahedron.vertices[0].neighborCount);
		Assert.AreEqual(5, icosahedron.vertices[1].neighborCount);
		Assert.AreEqual(5, icosahedron.vertices[2].neighborCount);
		Assert.AreEqual(5, icosahedron.vertices[3].neighborCount);
		Assert.AreEqual(5, icosahedron.vertices[4].neighborCount);
		Assert.AreEqual(5, icosahedron.vertices[5].neighborCount);
		Assert.AreEqual(5, icosahedron.vertices[6].neighborCount);
		Assert.AreEqual(5, icosahedron.vertices[7].neighborCount);
		Assert.AreEqual(5, icosahedron.vertices[8].neighborCount);
		Assert.AreEqual(5, icosahedron.vertices[9].neighborCount);
		Assert.AreEqual(5, icosahedron.vertices[10].neighborCount);
		Assert.AreEqual(5, icosahedron.vertices[11].neighborCount);
		Assert.AreEqual(3, icosahedron.internalFaces[0].neighborCount);
		Assert.AreEqual(3, icosahedron.internalFaces[1].neighborCount);
		Assert.AreEqual(3, icosahedron.internalFaces[2].neighborCount);
		Assert.AreEqual(3, icosahedron.internalFaces[3].neighborCount);
		Assert.AreEqual(3, icosahedron.internalFaces[4].neighborCount);
		Assert.AreEqual(3, icosahedron.internalFaces[5].neighborCount);
		Assert.AreEqual(3, icosahedron.internalFaces[6].neighborCount);
		Assert.AreEqual(3, icosahedron.internalFaces[7].neighborCount);
		Assert.AreEqual(3, icosahedron.internalFaces[8].neighborCount);
		Assert.AreEqual(3, icosahedron.internalFaces[9].neighborCount);
		Assert.AreEqual(3, icosahedron.internalFaces[10].neighborCount);
		Assert.AreEqual(3, icosahedron.internalFaces[11].neighborCount);
		Assert.AreEqual(3, icosahedron.internalFaces[12].neighborCount);
		Assert.AreEqual(3, icosahedron.internalFaces[13].neighborCount);
		Assert.AreEqual(3, icosahedron.internalFaces[14].neighborCount);
		Assert.AreEqual(3, icosahedron.internalFaces[15].neighborCount);
		Assert.AreEqual(3, icosahedron.internalFaces[16].neighborCount);
		Assert.AreEqual(3, icosahedron.internalFaces[17].neighborCount);
		Assert.AreEqual(3, icosahedron.internalFaces[18].neighborCount);
		Assert.AreEqual(3, icosahedron.internalFaces[19].neighborCount);
	}

	[Test]
	public void IcosahedronVertexNeighborValidation()
	{
		var icosahedron = SphericalManifoldUtility.CreateIcosahedron();

		var vertexEdges = icosahedron.vertices[0].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(1, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(8, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(4, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(5, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(10, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = icosahedron.vertices[1].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(0, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(10, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(7, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(6, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(8, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = icosahedron.vertices[2].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(3, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(11, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(5, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(4, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(9, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = icosahedron.vertices[3].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(2, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(9, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(6, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(7, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(11, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = icosahedron.vertices[4].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(0, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(8, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(9, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(2, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(5, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = icosahedron.vertices[5].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(0, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(4, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(2, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(11, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(10, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = icosahedron.vertices[6].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(1, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(7, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(3, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(9, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(8, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = icosahedron.vertices[7].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(1, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(10, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(11, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(3, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(6, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = icosahedron.vertices[8].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(0, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(1, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(6, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(9, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(4, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = icosahedron.vertices[9].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(2, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(4, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(8, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(6, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(3, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = icosahedron.vertices[10].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(0, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(5, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(11, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(7, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(1, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = icosahedron.vertices[11].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(2, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(3, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(7, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(10, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(5, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());
	}

	[Test]
	public void TetrahedronSubdivisionDegree1()
	{
		var tetrahedron = SphericalManifoldUtility.CreateTetrahedron();
		var subdivided = SphericalManifoldUtility.Subdivide(tetrahedron, 1);
		Assert.AreEqual(10, subdivided.vertices.Count);
		Assert.AreEqual(48, subdivided.vertexEdges.Count);
		Assert.AreEqual(48, subdivided.faceEdges.Count);
		Assert.AreEqual(16, subdivided.internalFaces.Count);

		var vertexNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var vertex in subdivided.vertices)
		{
			if (!vertexNeighborCountFrequencies.ContainsKey(vertex.neighborCount)) vertexNeighborCountFrequencies.Add(vertex.neighborCount, 0);
			vertexNeighborCountFrequencies[vertex.neighborCount] += 1;
		}
		Assert.AreEqual(2, vertexNeighborCountFrequencies.Count);
		Assert.AreEqual(4, vertexNeighborCountFrequencies[3]);
		Assert.AreEqual(6, vertexNeighborCountFrequencies[6]);

		var faceNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var face in subdivided.internalFaces)
		{
			if (!faceNeighborCountFrequencies.ContainsKey(face.neighborCount)) faceNeighborCountFrequencies.Add(face.neighborCount, 0);
			faceNeighborCountFrequencies[face.neighborCount] += 1;
		}
		Assert.AreEqual(1, faceNeighborCountFrequencies.Count);
		Assert.AreEqual(16, faceNeighborCountFrequencies[3]);
	}

	[Test]
	public void TetrahedronSubdivisionDegree2()
	{
		var tetrahedron = SphericalManifoldUtility.CreateTetrahedron();
		var subdivided = SphericalManifoldUtility.Subdivide(tetrahedron, 2);
		Assert.AreEqual(20, subdivided.vertices.Count);
		Assert.AreEqual(108, subdivided.vertexEdges.Count);
		Assert.AreEqual(108, subdivided.faceEdges.Count);
		Assert.AreEqual(36, subdivided.internalFaces.Count);

		var vertexNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var vertex in subdivided.vertices)
		{
			if (!vertexNeighborCountFrequencies.ContainsKey(vertex.neighborCount)) vertexNeighborCountFrequencies.Add(vertex.neighborCount, 0);
			vertexNeighborCountFrequencies[vertex.neighborCount] += 1;
		}
		Assert.AreEqual(2, vertexNeighborCountFrequencies.Count);
		Assert.AreEqual(4, vertexNeighborCountFrequencies[3]);
		Assert.AreEqual(16, vertexNeighborCountFrequencies[6]);

		var faceNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var face in subdivided.internalFaces)
		{
			if (!faceNeighborCountFrequencies.ContainsKey(face.neighborCount)) faceNeighborCountFrequencies.Add(face.neighborCount, 0);
			faceNeighborCountFrequencies[face.neighborCount] += 1;
		}
		Assert.AreEqual(1, faceNeighborCountFrequencies.Count);
		Assert.AreEqual(36, faceNeighborCountFrequencies[3]);
	}

	[Test]
	public void TetrahedronSubdivisionDegree3()
	{
		var tetrahedron = SphericalManifoldUtility.CreateTetrahedron();
		var subdivided = SphericalManifoldUtility.Subdivide(tetrahedron, 3);
		Assert.AreEqual(4 + 3*6 + 3*4 /*34*/, subdivided.vertices.Count);
		Assert.AreEqual(((4 + 3*6 + 3*4) + (4*4 * 4) - 2) * 2 /*192*/, subdivided.vertexEdges.Count);
		Assert.AreEqual(((4 + 3*6 + 3*4) + (4*4 * 4) - 2) * 2 /*192*/, subdivided.faceEdges.Count);
		Assert.AreEqual(4*4 * 4 /*64*/, subdivided.internalFaces.Count);

		var vertexNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var vertex in subdivided.vertices)
		{
			if (!vertexNeighborCountFrequencies.ContainsKey(vertex.neighborCount)) vertexNeighborCountFrequencies.Add(vertex.neighborCount, 0);
			vertexNeighborCountFrequencies[vertex.neighborCount] += 1;
		}
		Assert.AreEqual(2, vertexNeighborCountFrequencies.Count);
		Assert.AreEqual(4, vertexNeighborCountFrequencies[3]);
		Assert.AreEqual(30, vertexNeighborCountFrequencies[6]);

		var faceNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var face in subdivided.internalFaces)
		{
			if (!faceNeighborCountFrequencies.ContainsKey(face.neighborCount)) faceNeighborCountFrequencies.Add(face.neighborCount, 0);
			faceNeighborCountFrequencies[face.neighborCount] += 1;
		}
		Assert.AreEqual(1, faceNeighborCountFrequencies.Count);
		Assert.AreEqual(64, faceNeighborCountFrequencies[3]);
	}

	[Test]
	public void TetrahedronSubdivisionDegree4()
	{
		var tetrahedron = SphericalManifoldUtility.CreateTetrahedron();
		var subdivided = SphericalManifoldUtility.Subdivide(tetrahedron, 4);
		Assert.AreEqual(4 + 4*6 + 6*4 /*52*/, subdivided.vertices.Count);
		Assert.AreEqual(((4 + 4*6 + 6*4) + (5*5 * 4) - 2) * 2 /*300*/, subdivided.vertexEdges.Count);
		Assert.AreEqual(((4 + 4*6 + 6*4) + (5*5 * 4) - 2) * 2 /*300*/, subdivided.faceEdges.Count);
		Assert.AreEqual(5*5 * 4 /*100*/, subdivided.internalFaces.Count);

		var vertexNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var vertex in subdivided.vertices)
		{
			if (!vertexNeighborCountFrequencies.ContainsKey(vertex.neighborCount)) vertexNeighborCountFrequencies.Add(vertex.neighborCount, 0);
			vertexNeighborCountFrequencies[vertex.neighborCount] += 1;
		}
		Assert.AreEqual(2, vertexNeighborCountFrequencies.Count);
		Assert.AreEqual(4, vertexNeighborCountFrequencies[3]);
		Assert.AreEqual(48, vertexNeighborCountFrequencies[6]);

		var faceNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var face in subdivided.internalFaces)
		{
			if (!faceNeighborCountFrequencies.ContainsKey(face.neighborCount)) faceNeighborCountFrequencies.Add(face.neighborCount, 0);
			faceNeighborCountFrequencies[face.neighborCount] += 1;
		}
		Assert.AreEqual(1, faceNeighborCountFrequencies.Count);
		Assert.AreEqual(100, faceNeighborCountFrequencies[3]);
	}

	[Test]
	public void TetrahedronSubdivisionDegree5()
	{
		var tetrahedron = SphericalManifoldUtility.CreateTetrahedron();
		var degree = 5;
		var vertexCount = 4 + degree * 6 + (degree * (degree - 1) / 2) * 4; /*74*/
		var faceCount = (degree + 1) * (degree + 1) * 4; /*144*/
		var edgeCount = (vertexCount + faceCount - 2) * 2; /*432*/
		var subdivided = SphericalManifoldUtility.Subdivide(tetrahedron, degree);
		Assert.AreEqual(vertexCount, subdivided.vertices.Count);
		Assert.AreEqual(edgeCount, subdivided.vertexEdges.Count);
		Assert.AreEqual(edgeCount, subdivided.faceEdges.Count);
		Assert.AreEqual(faceCount, subdivided.internalFaces.Count);

		var vertexNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var vertex in subdivided.vertices)
		{
			if (!vertexNeighborCountFrequencies.ContainsKey(vertex.neighborCount)) vertexNeighborCountFrequencies.Add(vertex.neighborCount, 0);
			vertexNeighborCountFrequencies[vertex.neighborCount] += 1;
		}
		Assert.AreEqual(2, vertexNeighborCountFrequencies.Count);
		Assert.AreEqual(4, vertexNeighborCountFrequencies[3]);
		Assert.AreEqual(70, vertexNeighborCountFrequencies[6]);

		var faceNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var face in subdivided.internalFaces)
		{
			if (!faceNeighborCountFrequencies.ContainsKey(face.neighborCount)) faceNeighborCountFrequencies.Add(face.neighborCount, 0);
			faceNeighborCountFrequencies[face.neighborCount] += 1;
		}
		Assert.AreEqual(1, faceNeighborCountFrequencies.Count);
		Assert.AreEqual(144, faceNeighborCountFrequencies[3]);
	}

	[Test]
	public void HexehedronSubdivisionDegree1()
	{
		var hexahedron = SphericalManifoldUtility.CreateCube();
		var subdivided = SphericalManifoldUtility.Subdivide(hexahedron, 1);
		Assert.AreEqual(8 + 1*12 + 1*1*6 /*26*/, subdivided.vertices.Count);
		Assert.AreEqual(((8 + 1*12 + 1*1*6) + (2*2 * 6) - 2) * 2 /*96*/, subdivided.vertexEdges.Count);
		Assert.AreEqual(((8 + 1*12 + 1*1*6) + (2*2 * 6) - 2) * 2 /*96*/, subdivided.faceEdges.Count);
		Assert.AreEqual(2*2 * 6 /*24*/, subdivided.internalFaces.Count);

		var vertexNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var vertex in subdivided.vertices)
		{
			if (!vertexNeighborCountFrequencies.ContainsKey(vertex.neighborCount)) vertexNeighborCountFrequencies.Add(vertex.neighborCount, 0);
			vertexNeighborCountFrequencies[vertex.neighborCount] += 1;
		}
		Assert.AreEqual(2, vertexNeighborCountFrequencies.Count);
		Assert.AreEqual(8, vertexNeighborCountFrequencies[3]);
		Assert.AreEqual(18, vertexNeighborCountFrequencies[4]);

		var faceNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var face in subdivided.internalFaces)
		{
			if (!faceNeighborCountFrequencies.ContainsKey(face.neighborCount)) faceNeighborCountFrequencies.Add(face.neighborCount, 0);
			faceNeighborCountFrequencies[face.neighborCount] += 1;
		}
		Assert.AreEqual(1, faceNeighborCountFrequencies.Count);
		Assert.AreEqual(24, faceNeighborCountFrequencies[4]);
	}

	[Test]
	public void HexehedronSubdivisionDegree2()
	{
		var hexahedron = SphericalManifoldUtility.CreateCube();
		var subdivided = SphericalManifoldUtility.Subdivide(hexahedron, 2);
		Assert.AreEqual(8 + 2*12 + 2*2*6 /*56*/, subdivided.vertices.Count);
		Assert.AreEqual(((8 + 2*12 + 2*2*6) + (3*3 * 6) - 2) * 2 /*216*/, subdivided.vertexEdges.Count);
		Assert.AreEqual(((8 + 2*12 + 2*2*6) + (3*3 * 6) - 2) * 2 /*216*/, subdivided.faceEdges.Count);
		Assert.AreEqual(3*3 * 6 /*54*/, subdivided.internalFaces.Count);

		var vertexNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var vertex in subdivided.vertices)
		{
			if (!vertexNeighborCountFrequencies.ContainsKey(vertex.neighborCount)) vertexNeighborCountFrequencies.Add(vertex.neighborCount, 0);
			vertexNeighborCountFrequencies[vertex.neighborCount] += 1;
		}
		Assert.AreEqual(2, vertexNeighborCountFrequencies.Count);
		Assert.AreEqual(8, vertexNeighborCountFrequencies[3]);
		Assert.AreEqual(48, vertexNeighborCountFrequencies[4]);

		var faceNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var face in subdivided.internalFaces)
		{
			if (!faceNeighborCountFrequencies.ContainsKey(face.neighborCount)) faceNeighborCountFrequencies.Add(face.neighborCount, 0);
			faceNeighborCountFrequencies[face.neighborCount] += 1;
		}
		Assert.AreEqual(1, faceNeighborCountFrequencies.Count);
		Assert.AreEqual(54, faceNeighborCountFrequencies[4]);
	}

	[Test]
	public void HexehedronSubdivisionDegree3()
	{
		var hexahedron = SphericalManifoldUtility.CreateCube();
		var subdivided = SphericalManifoldUtility.Subdivide(hexahedron, 3);
		Assert.AreEqual(8 + 3*12 + 3*3*6 /*98*/, subdivided.vertices.Count);
		Assert.AreEqual(((8 + 3*12 + 3*3*6) + (4*4 * 6) - 2) * 2 /*384*/, subdivided.vertexEdges.Count);
		Assert.AreEqual(((8 + 3*12 + 3*3*6) + (4*4 * 6) - 2) * 2 /*384*/, subdivided.faceEdges.Count);
		Assert.AreEqual(4*4 * 6 /*96*/, subdivided.internalFaces.Count);

		var vertexNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var vertex in subdivided.vertices)
		{
			if (!vertexNeighborCountFrequencies.ContainsKey(vertex.neighborCount)) vertexNeighborCountFrequencies.Add(vertex.neighborCount, 0);
			vertexNeighborCountFrequencies[vertex.neighborCount] += 1;
		}
		Assert.AreEqual(2, vertexNeighborCountFrequencies.Count);
		Assert.AreEqual(8, vertexNeighborCountFrequencies[3]);
		Assert.AreEqual(90, vertexNeighborCountFrequencies[4]);

		var faceNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var face in subdivided.internalFaces)
		{
			if (!faceNeighborCountFrequencies.ContainsKey(face.neighborCount)) faceNeighborCountFrequencies.Add(face.neighborCount, 0);
			faceNeighborCountFrequencies[face.neighborCount] += 1;
		}
		Assert.AreEqual(1, faceNeighborCountFrequencies.Count);
		Assert.AreEqual(96, faceNeighborCountFrequencies[4]);
	}

	[Test]
	public void HexehedronSubdivisionDegree4()
	{
		var hexahedron = SphericalManifoldUtility.CreateCube();
		var degree = 4;
		var vertexCount = 8 + degree * 12 + degree * degree * 6; /*152*/
		var faceCount = (degree + 1) * (degree + 1) * 6; /*150*/
		var edgeCount = (vertexCount + faceCount - 2) * 2; /*384*/
		var subdivided = SphericalManifoldUtility.Subdivide(hexahedron, degree);
		Assert.AreEqual(vertexCount, subdivided.vertices.Count);
		Assert.AreEqual(edgeCount, subdivided.vertexEdges.Count);
		Assert.AreEqual(edgeCount, subdivided.faceEdges.Count);
		Assert.AreEqual(faceCount, subdivided.internalFaces.Count);

		var vertexNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var vertex in subdivided.vertices)
		{
			if (!vertexNeighborCountFrequencies.ContainsKey(vertex.neighborCount)) vertexNeighborCountFrequencies.Add(vertex.neighborCount, 0);
			vertexNeighborCountFrequencies[vertex.neighborCount] += 1;
		}
		Assert.AreEqual(2, vertexNeighborCountFrequencies.Count);
		Assert.AreEqual(8, vertexNeighborCountFrequencies[3]);
		Assert.AreEqual(vertexCount - 8, vertexNeighborCountFrequencies[4]);

		var faceNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var face in subdivided.internalFaces)
		{
			if (!faceNeighborCountFrequencies.ContainsKey(face.neighborCount)) faceNeighborCountFrequencies.Add(face.neighborCount, 0);
			faceNeighborCountFrequencies[face.neighborCount] += 1;
		}
		Assert.AreEqual(1, faceNeighborCountFrequencies.Count);
		Assert.AreEqual(faceCount, faceNeighborCountFrequencies[4]);
	}

	[Test]
	public void HexehedronSubdivisionDegree5()
	{
		var hexahedron = SphericalManifoldUtility.CreateCube();
		var degree = 5;
		var vertexCount = 8 + degree * 12 + degree * degree * 6; /*218*/
		var faceCount = (degree + 1) * (degree + 1) * 6; /*216*/
		var edgeCount = (vertexCount + faceCount - 2) * 2; /*864*/
		var subdivided = SphericalManifoldUtility.Subdivide(hexahedron, degree);
		Assert.AreEqual(vertexCount, subdivided.vertices.Count);
		Assert.AreEqual(edgeCount, subdivided.vertexEdges.Count);
		Assert.AreEqual(edgeCount, subdivided.faceEdges.Count);
		Assert.AreEqual(faceCount, subdivided.internalFaces.Count);

		var vertexNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var vertex in subdivided.vertices)
		{
			if (!vertexNeighborCountFrequencies.ContainsKey(vertex.neighborCount)) vertexNeighborCountFrequencies.Add(vertex.neighborCount, 0);
			vertexNeighborCountFrequencies[vertex.neighborCount] += 1;
		}
		Assert.AreEqual(2, vertexNeighborCountFrequencies.Count);
		Assert.AreEqual(8, vertexNeighborCountFrequencies[3]);
		Assert.AreEqual(vertexCount - 8, vertexNeighborCountFrequencies[4]);

		var faceNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var face in subdivided.internalFaces)
		{
			if (!faceNeighborCountFrequencies.ContainsKey(face.neighborCount)) faceNeighborCountFrequencies.Add(face.neighborCount, 0);
			faceNeighborCountFrequencies[face.neighborCount] += 1;
		}
		Assert.AreEqual(1, faceNeighborCountFrequencies.Count);
		Assert.AreEqual(faceCount, faceNeighborCountFrequencies[4]);
	}

	[Test]
	public void TetrahedronDualValidation()
	{
		var tetrahedron = SphericalManifoldUtility.CreateTetrahedron();
		var dual = tetrahedron.GetDualTopology();

		foreach (var vertex in tetrahedron.vertices)
		{
			var edge = vertex.firstEdge;
			var dualFace = dual.internalFaces[vertex.index];
			Assert.AreEqual(vertex.neighborCount, dualFace.neighborCount);
			var dualEdge = dualFace.FindEdge(dual.internalFaces[edge.farVertex.index]);
			Assert.AreEqual(edge.farVertex.index, dualEdge.farFace.index);
			edge = edge.next;
			dualEdge = dualEdge.next;
			Assert.AreEqual(edge.farVertex.index, dualEdge.farFace.index);
			edge = edge.next;
			dualEdge = dualEdge.next;
			Assert.AreEqual(edge.farVertex.index, dualEdge.farFace.index);
		}
	}

	[Test]
	public void SpinEdgeForward()
	{
		var octahedron = SphericalManifoldUtility.CreateOctahedron();

		var oldVertex0 = octahedron.vertices[2];
		var oldVertex1 = octahedron.vertices[3];
		var newVertex0 = octahedron.vertices[0];
		var newVertex1 = octahedron.vertices[5];

		Assert.AreEqual(4, oldVertex0.neighborCount);
		Assert.AreEqual(4, oldVertex1.neighborCount);
		Assert.AreEqual(4, newVertex0.neighborCount);
		Assert.AreEqual(4, newVertex1.neighborCount);

		Topology.VertexEdge edgeToSpin;
		Assert.IsTrue(oldVertex0.TryFindEdge(oldVertex1, out edgeToSpin));

		var face0 = edgeToSpin.nextFace;
		var face1 = edgeToSpin.prevFace;

		Topology.FaceEdge faceEdge0;
		Topology.FaceEdge faceEdge1;

		Assert.IsTrue(face0.TryFindEdge(newVertex0, out faceEdge0));
		Assert.IsTrue(face1.TryFindEdge(newVertex1, out faceEdge1));

		Assert.AreEqual(newVertex0, faceEdge0.nextVertex);
		Assert.AreEqual(oldVertex0, faceEdge0.next.nextVertex);
		Assert.AreEqual(oldVertex1, faceEdge0.next.next.nextVertex);
		Assert.AreEqual(newVertex1, faceEdge1.nextVertex);
		Assert.AreEqual(oldVertex1, faceEdge1.next.nextVertex);
		Assert.AreEqual(oldVertex0, faceEdge1.next.next.nextVertex);

		octahedron.SpinEdgeForward(edgeToSpin);

		Assert.AreEqual(3, oldVertex0.neighborCount);
		Assert.AreEqual(3, oldVertex1.neighborCount);
		Assert.AreEqual(5, newVertex0.neighborCount);
		Assert.AreEqual(5, newVertex1.neighborCount);

		Assert.AreEqual(newVertex0, edgeToSpin.farVertex);
		Assert.AreEqual(newVertex1, edgeToSpin.nearVertex);
		Assert.AreEqual(newVertex1, edgeToSpin.twin.farVertex);
		Assert.AreEqual(newVertex0, edgeToSpin.twin.nearVertex);

		Assert.AreEqual(oldVertex0.firstEdge, oldVertex0.firstEdge.next.next.next);
		Assert.AreEqual(oldVertex1.firstEdge, oldVertex1.firstEdge.next.next.next);
		Assert.AreEqual(newVertex0.firstEdge, newVertex0.firstEdge.next.next.next.next.next);
		Assert.AreEqual(newVertex1.firstEdge, newVertex1.firstEdge.next.next.next.next.next);

		Assert.Throws<InvalidOperationException>(() => { oldVertex0.FindEdge(oldVertex1); });
		Assert.Throws<InvalidOperationException>(() => { oldVertex1.FindEdge(oldVertex0); });

		Assert.DoesNotThrow(() => { newVertex0.FindEdge(newVertex1); });
		Assert.DoesNotThrow(() => { newVertex1.FindEdge(newVertex0); });
		Assert.AreEqual(edgeToSpin, newVertex1.FindEdge(newVertex0));
		Assert.AreEqual(edgeToSpin.twin, newVertex0.FindEdge(newVertex1));

		Assert.AreEqual(face0, edgeToSpin.nextFace);
		Assert.AreEqual(face1, edgeToSpin.prevFace);

		Assert.IsTrue(face0.TryFindEdge(newVertex0, out faceEdge0));
		Assert.IsTrue(face1.TryFindEdge(newVertex1, out faceEdge1));

		Assert.AreEqual(newVertex0, faceEdge0.nextVertex);
		Assert.AreEqual(oldVertex0, faceEdge0.next.nextVertex);
		Assert.AreEqual(newVertex1, faceEdge0.next.next.nextVertex);
		Assert.AreEqual(newVertex1, faceEdge1.nextVertex);
		Assert.AreEqual(oldVertex1, faceEdge1.next.nextVertex);
		Assert.AreEqual(newVertex0, faceEdge1.next.next.nextVertex);
	}
#endif
}
#endif
