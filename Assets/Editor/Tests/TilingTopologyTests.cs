using UnityEngine;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Tiling;

public class MeshTopologyTests
{
	[Test]
	public void TetrahedronComponentCountValidation()
	{
		var tetrahedron = SphereTopology.CreateTetrahedron();
		Assert.AreEqual(4, tetrahedron.topology.vertices.Count);
		Assert.AreEqual(12, tetrahedron.topology.vertexEdges.Count);
		Assert.AreEqual(12, tetrahedron.topology.faceEdges.Count);
		Assert.AreEqual(4, tetrahedron.topology.faces.Count);
	}

	[Test]
	public void TetrahedronComponentNeighborCountValidation()
	{
		var tetrahedron = SphereTopology.CreateTetrahedron();
		Assert.AreEqual(3, tetrahedron.topology.vertices[0].neighborCount);
		Assert.AreEqual(3, tetrahedron.topology.vertices[1].neighborCount);
		Assert.AreEqual(3, tetrahedron.topology.vertices[2].neighborCount);
		Assert.AreEqual(3, tetrahedron.topology.vertices[3].neighborCount);
		Assert.AreEqual(3, tetrahedron.topology.faces[0].neighborCount);
		Assert.AreEqual(3, tetrahedron.topology.faces[1].neighborCount);
		Assert.AreEqual(3, tetrahedron.topology.faces[2].neighborCount);
		Assert.AreEqual(3, tetrahedron.topology.faces[3].neighborCount);
	}

	[Test]
	public void TetrahedronVertexNeighborValidation()
	{
		var tetrahedron = SphereTopology.CreateTetrahedron();

		var vertexEdges = tetrahedron.topology.vertices[0].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(1, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(2, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(3, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = tetrahedron.topology.vertices[1].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(0, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(3, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(2, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = tetrahedron.topology.vertices[2].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(0, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(1, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(3, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = tetrahedron.topology.vertices[3].edges.GetEnumerator();
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
		var tetrahedron = SphereTopology.CreateTetrahedron();

		var vertex0 = tetrahedron.topology.vertices[0];
		var vertexEdge = vertex0.firstEdge;
		var vertex1 = vertexEdge.farVertex;
		var vertex2 = vertexEdge.next.farVertex;
		var faceEdge = vertexEdge.faceEdge.twin;
		Assert.AreEqual(vertex1, faceEdge.nextVertex);
		Assert.AreEqual(vertex2, faceEdge.next.nextVertex);
		Assert.AreEqual(vertex0, faceEdge.next.next.nextVertex);
		Assert.AreEqual(vertexEdge.next, faceEdge.next.next);

		vertexEdge = vertexEdge.next;
		vertex1 = vertexEdge.farVertex;
		vertex2 = vertexEdge.next.farVertex;
		faceEdge = vertexEdge.faceEdge.twin;
		Assert.AreEqual(vertex1, faceEdge.nextVertex);
		Assert.AreEqual(vertex2, faceEdge.next.nextVertex);
		Assert.AreEqual(vertex0, faceEdge.next.next.nextVertex);
		Assert.AreEqual(vertexEdge.next, faceEdge.next.next);

		vertexEdge = vertexEdge.next;
		vertex1 = vertexEdge.farVertex;
		vertex2 = vertexEdge.next.farVertex;
		faceEdge = vertexEdge.faceEdge.twin;
		Assert.AreEqual(vertex1, faceEdge.nextVertex);
		Assert.AreEqual(vertex2, faceEdge.next.nextVertex);
		Assert.AreEqual(vertex0, faceEdge.next.next.nextVertex);
		Assert.AreEqual(vertexEdge.next, faceEdge.next.next);

		vertex0 = tetrahedron.topology.vertices[1];
		vertexEdge = vertex0.firstEdge;
		vertex1 = vertexEdge.farVertex;
		vertex2 = vertexEdge.next.farVertex;
		faceEdge = vertexEdge.faceEdge.twin;
		Assert.AreEqual(vertex1, faceEdge.nextVertex);
		Assert.AreEqual(vertex2, faceEdge.next.nextVertex);
		Assert.AreEqual(vertex0, faceEdge.next.next.nextVertex);
		Assert.AreEqual(vertexEdge.next, faceEdge.next.next);

		vertexEdge = vertexEdge.next;
		vertex1 = vertexEdge.farVertex;
		vertex2 = vertexEdge.next.farVertex;
		faceEdge = vertexEdge.faceEdge.twin;
		Assert.AreEqual(vertex1, faceEdge.nextVertex);
		Assert.AreEqual(vertex2, faceEdge.next.nextVertex);
		Assert.AreEqual(vertex0, faceEdge.next.next.nextVertex);
		Assert.AreEqual(vertexEdge.next, faceEdge.next.next);

		vertexEdge = vertexEdge.next;
		vertex1 = vertexEdge.farVertex;
		vertex2 = vertexEdge.next.farVertex;
		faceEdge = vertexEdge.faceEdge.twin;
		Assert.AreEqual(vertex1, faceEdge.nextVertex);
		Assert.AreEqual(vertex2, faceEdge.next.nextVertex);
		Assert.AreEqual(vertex0, faceEdge.next.next.nextVertex);
		Assert.AreEqual(vertexEdge.next, faceEdge.next.next);

		vertex0 = tetrahedron.topology.vertices[2];
		vertexEdge = vertex0.firstEdge;
		vertex1 = vertexEdge.farVertex;
		vertex2 = vertexEdge.next.farVertex;
		faceEdge = vertexEdge.faceEdge.twin;
		Assert.AreEqual(vertex1, faceEdge.nextVertex);
		Assert.AreEqual(vertex2, faceEdge.next.nextVertex);
		Assert.AreEqual(vertex0, faceEdge.next.next.nextVertex);
		Assert.AreEqual(vertexEdge.next, faceEdge.next.next);

		vertexEdge = vertexEdge.next;
		vertex1 = vertexEdge.farVertex;
		vertex2 = vertexEdge.next.farVertex;
		faceEdge = vertexEdge.faceEdge.twin;
		Assert.AreEqual(vertex1, faceEdge.nextVertex);
		Assert.AreEqual(vertex2, faceEdge.next.nextVertex);
		Assert.AreEqual(vertex0, faceEdge.next.next.nextVertex);
		Assert.AreEqual(vertexEdge.next, faceEdge.next.next);

		vertexEdge = vertexEdge.next;
		vertex1 = vertexEdge.farVertex;
		vertex2 = vertexEdge.next.farVertex;
		faceEdge = vertexEdge.faceEdge.twin;
		Assert.AreEqual(vertex1, faceEdge.nextVertex);
		Assert.AreEqual(vertex2, faceEdge.next.nextVertex);
		Assert.AreEqual(vertex0, faceEdge.next.next.nextVertex);
		Assert.AreEqual(vertexEdge.next, faceEdge.next.next);

		vertex0 = tetrahedron.topology.vertices[3];
		vertexEdge = vertex0.firstEdge;
		vertex1 = vertexEdge.farVertex;
		vertex2 = vertexEdge.next.farVertex;
		faceEdge = vertexEdge.faceEdge.twin;
		Assert.AreEqual(vertex1, faceEdge.nextVertex);
		Assert.AreEqual(vertex2, faceEdge.next.nextVertex);
		Assert.AreEqual(vertex0, faceEdge.next.next.nextVertex);
		Assert.AreEqual(vertexEdge.next, faceEdge.next.next);

		vertexEdge = vertexEdge.next;
		vertex1 = vertexEdge.farVertex;
		vertex2 = vertexEdge.next.farVertex;
		faceEdge = vertexEdge.faceEdge.twin;
		Assert.AreEqual(vertex1, faceEdge.nextVertex);
		Assert.AreEqual(vertex2, faceEdge.next.nextVertex);
		Assert.AreEqual(vertex0, faceEdge.next.next.nextVertex);
		Assert.AreEqual(vertexEdge.next, faceEdge.next.next);

		vertexEdge = vertexEdge.next;
		vertex1 = vertexEdge.farVertex;
		vertex2 = vertexEdge.next.farVertex;
		faceEdge = vertexEdge.faceEdge.twin;
		Assert.AreEqual(vertex1, faceEdge.nextVertex);
		Assert.AreEqual(vertex2, faceEdge.next.nextVertex);
		Assert.AreEqual(vertex0, faceEdge.next.next.nextVertex);
		Assert.AreEqual(vertexEdge.next, faceEdge.next.next);
	}

	[Test]
	public void CubeComponentCountValidation()
	{
		var cube = SphereTopology.CreateCube();
		Assert.AreEqual(8, cube.topology.vertices.Count);
		Assert.AreEqual(24, cube.topology.vertexEdges.Count);
		Assert.AreEqual(24, cube.topology.faceEdges.Count);
		Assert.AreEqual(6, cube.topology.faces.Count);
	}

	[Test]
	public void CubeComponentNeighborCountValidation()
	{
		var cube = SphereTopology.CreateCube();
		Assert.AreEqual(3, cube.topology.vertices[0].neighborCount);
		Assert.AreEqual(3, cube.topology.vertices[1].neighborCount);
		Assert.AreEqual(3, cube.topology.vertices[2].neighborCount);
		Assert.AreEqual(3, cube.topology.vertices[3].neighborCount);
		Assert.AreEqual(3, cube.topology.vertices[4].neighborCount);
		Assert.AreEqual(3, cube.topology.vertices[5].neighborCount);
		Assert.AreEqual(3, cube.topology.vertices[6].neighborCount);
		Assert.AreEqual(3, cube.topology.vertices[7].neighborCount);
		Assert.AreEqual(4, cube.topology.faces[0].neighborCount);
		Assert.AreEqual(4, cube.topology.faces[1].neighborCount);
		Assert.AreEqual(4, cube.topology.faces[2].neighborCount);
		Assert.AreEqual(4, cube.topology.faces[3].neighborCount);
		Assert.AreEqual(4, cube.topology.faces[4].neighborCount);
		Assert.AreEqual(4, cube.topology.faces[5].neighborCount);
	}

	[Test]
	public void CubeVertexNeighborValidation()
	{
		var cube = SphereTopology.CreateCube();

		var vertexEdges = cube.topology.vertices[0].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(1, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(3, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(4, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = cube.topology.vertices[1].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(0, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(5, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(2, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = cube.topology.vertices[2].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(1, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(6, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(3, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = cube.topology.vertices[3].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(0, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(2, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(7, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = cube.topology.vertices[4].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(0, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(7, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(5, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = cube.topology.vertices[5].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(1, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(4, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(6, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = cube.topology.vertices[6].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(2, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(5, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(7, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = cube.topology.vertices[7].edges.GetEnumerator();
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
		var octahedron = SphereTopology.CreateOctahedron();
		Assert.AreEqual(6, octahedron.topology.vertices.Count);
		Assert.AreEqual(24, octahedron.topology.vertexEdges.Count);
		Assert.AreEqual(24, octahedron.topology.faceEdges.Count);
		Assert.AreEqual(8, octahedron.topology.faces.Count);
	}

	[Test]
	public void OctahedronComponentNeighborCountValidation()
	{
		var octahedron = SphereTopology.CreateOctahedron();
		Assert.AreEqual(4, octahedron.topology.vertices[0].neighborCount);
		Assert.AreEqual(4, octahedron.topology.vertices[1].neighborCount);
		Assert.AreEqual(4, octahedron.topology.vertices[2].neighborCount);
		Assert.AreEqual(4, octahedron.topology.vertices[3].neighborCount);
		Assert.AreEqual(4, octahedron.topology.vertices[4].neighborCount);
		Assert.AreEqual(4, octahedron.topology.vertices[5].neighborCount);
		Assert.AreEqual(3, octahedron.topology.faces[0].neighborCount);
		Assert.AreEqual(3, octahedron.topology.faces[1].neighborCount);
		Assert.AreEqual(3, octahedron.topology.faces[2].neighborCount);
		Assert.AreEqual(3, octahedron.topology.faces[3].neighborCount);
		Assert.AreEqual(3, octahedron.topology.faces[4].neighborCount);
		Assert.AreEqual(3, octahedron.topology.faces[5].neighborCount);
		Assert.AreEqual(3, octahedron.topology.faces[6].neighborCount);
		Assert.AreEqual(3, octahedron.topology.faces[7].neighborCount);
	}

	[Test]
	public void OctahedronVertexNeighborValidation()
	{
		var octahedron = SphereTopology.CreateOctahedron();

		var vertexEdges = octahedron.topology.vertices[0].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(1, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(2, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(3, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(4, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = octahedron.topology.vertices[1].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(0, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(4, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(5, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(2, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = octahedron.topology.vertices[2].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(0, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(1, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(5, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(3, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = octahedron.topology.vertices[3].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(0, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(2, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(5, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(4, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = octahedron.topology.vertices[4].edges.GetEnumerator();
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(0, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(3, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(5, vertexEdges.Current.farVertex.index);
		Assert.IsTrue(vertexEdges.MoveNext());
		Assert.AreEqual(1, vertexEdges.Current.farVertex.index);
		Assert.IsFalse(vertexEdges.MoveNext());

		vertexEdges = octahedron.topology.vertices[5].edges.GetEnumerator();
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
		var icosahedron = SphereTopology.CreateIcosahedron();
		Assert.AreEqual(12, icosahedron.topology.vertices.Count);
		Assert.AreEqual(60, icosahedron.topology.vertexEdges.Count);
		Assert.AreEqual(60, icosahedron.topology.faceEdges.Count);
		Assert.AreEqual(20, icosahedron.topology.faces.Count);
	}

	[Test]
	public void IcosahedronComponentNeighborCountValidation()
	{
		var icosahedron = SphereTopology.CreateIcosahedron();
		Assert.AreEqual(5, icosahedron.topology.vertices[0].neighborCount);
		Assert.AreEqual(5, icosahedron.topology.vertices[1].neighborCount);
		Assert.AreEqual(5, icosahedron.topology.vertices[2].neighborCount);
		Assert.AreEqual(5, icosahedron.topology.vertices[3].neighborCount);
		Assert.AreEqual(5, icosahedron.topology.vertices[4].neighborCount);
		Assert.AreEqual(5, icosahedron.topology.vertices[5].neighborCount);
		Assert.AreEqual(5, icosahedron.topology.vertices[6].neighborCount);
		Assert.AreEqual(5, icosahedron.topology.vertices[7].neighborCount);
		Assert.AreEqual(5, icosahedron.topology.vertices[8].neighborCount);
		Assert.AreEqual(5, icosahedron.topology.vertices[9].neighborCount);
		Assert.AreEqual(5, icosahedron.topology.vertices[10].neighborCount);
		Assert.AreEqual(5, icosahedron.topology.vertices[11].neighborCount);
		Assert.AreEqual(3, icosahedron.topology.faces[0].neighborCount);
		Assert.AreEqual(3, icosahedron.topology.faces[1].neighborCount);
		Assert.AreEqual(3, icosahedron.topology.faces[2].neighborCount);
		Assert.AreEqual(3, icosahedron.topology.faces[3].neighborCount);
		Assert.AreEqual(3, icosahedron.topology.faces[4].neighborCount);
		Assert.AreEqual(3, icosahedron.topology.faces[5].neighborCount);
		Assert.AreEqual(3, icosahedron.topology.faces[6].neighborCount);
		Assert.AreEqual(3, icosahedron.topology.faces[7].neighborCount);
		Assert.AreEqual(3, icosahedron.topology.faces[8].neighborCount);
		Assert.AreEqual(3, icosahedron.topology.faces[9].neighborCount);
		Assert.AreEqual(3, icosahedron.topology.faces[10].neighborCount);
		Assert.AreEqual(3, icosahedron.topology.faces[11].neighborCount);
		Assert.AreEqual(3, icosahedron.topology.faces[12].neighborCount);
		Assert.AreEqual(3, icosahedron.topology.faces[13].neighborCount);
		Assert.AreEqual(3, icosahedron.topology.faces[14].neighborCount);
		Assert.AreEqual(3, icosahedron.topology.faces[15].neighborCount);
		Assert.AreEqual(3, icosahedron.topology.faces[16].neighborCount);
		Assert.AreEqual(3, icosahedron.topology.faces[17].neighborCount);
		Assert.AreEqual(3, icosahedron.topology.faces[18].neighborCount);
		Assert.AreEqual(3, icosahedron.topology.faces[19].neighborCount);
	}

	[Test]
	public void IcosahedronVertexNeighborValidation()
	{
		var icosahedron = SphereTopology.CreateIcosahedron();

		var vertexEdges = icosahedron.topology.vertices[0].edges.GetEnumerator();
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

		vertexEdges = icosahedron.topology.vertices[1].edges.GetEnumerator();
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

		vertexEdges = icosahedron.topology.vertices[2].edges.GetEnumerator();
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

		vertexEdges = icosahedron.topology.vertices[3].edges.GetEnumerator();
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

		vertexEdges = icosahedron.topology.vertices[4].edges.GetEnumerator();
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

		vertexEdges = icosahedron.topology.vertices[5].edges.GetEnumerator();
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

		vertexEdges = icosahedron.topology.vertices[6].edges.GetEnumerator();
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

		vertexEdges = icosahedron.topology.vertices[7].edges.GetEnumerator();
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

		vertexEdges = icosahedron.topology.vertices[8].edges.GetEnumerator();
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

		vertexEdges = icosahedron.topology.vertices[9].edges.GetEnumerator();
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

		vertexEdges = icosahedron.topology.vertices[10].edges.GetEnumerator();
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

		vertexEdges = icosahedron.topology.vertices[11].edges.GetEnumerator();
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
		var tetrahedron = SphereTopology.CreateTetrahedron();
		var subdivided = SphereTopology.Subdivide(tetrahedron, 1);
		Assert.AreEqual(10, subdivided.topology.vertices.Count);
		Assert.AreEqual(48, subdivided.topology.vertexEdges.Count);
		Assert.AreEqual(48, subdivided.topology.faceEdges.Count);
		Assert.AreEqual(16, subdivided.topology.faces.Count);

		var vertexNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var vertex in subdivided.topology.vertices)
		{
			if (!vertexNeighborCountFrequencies.ContainsKey(vertex.neighborCount)) vertexNeighborCountFrequencies.Add(vertex.neighborCount, 0);
			vertexNeighborCountFrequencies[vertex.neighborCount] += 1;
		}
		Assert.AreEqual(2, vertexNeighborCountFrequencies.Count);
		Assert.AreEqual(4, vertexNeighborCountFrequencies[3]);
		Assert.AreEqual(6, vertexNeighborCountFrequencies[6]);

		var faceNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var face in subdivided.topology.faces)
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
		var tetrahedron = SphereTopology.CreateTetrahedron();
		var subdivided = SphereTopology.Subdivide(tetrahedron, 2);
		Assert.AreEqual(20, subdivided.topology.vertices.Count);
		Assert.AreEqual(108, subdivided.topology.vertexEdges.Count);
		Assert.AreEqual(108, subdivided.topology.faceEdges.Count);
		Assert.AreEqual(36, subdivided.topology.faces.Count);

		var vertexNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var vertex in subdivided.topology.vertices)
		{
			if (!vertexNeighborCountFrequencies.ContainsKey(vertex.neighborCount)) vertexNeighborCountFrequencies.Add(vertex.neighborCount, 0);
			vertexNeighborCountFrequencies[vertex.neighborCount] += 1;
		}
		Assert.AreEqual(2, vertexNeighborCountFrequencies.Count);
		Assert.AreEqual(4, vertexNeighborCountFrequencies[3]);
		Assert.AreEqual(16, vertexNeighborCountFrequencies[6]);

		var faceNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var face in subdivided.topology.faces)
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
		var tetrahedron = SphereTopology.CreateTetrahedron();
		var subdivided = SphereTopology.Subdivide(tetrahedron, 3);
		Assert.AreEqual(4 + 3*6 + 3*4 /*34*/, subdivided.topology.vertices.Count);
		Assert.AreEqual(((4 + 3*6 + 3*4) + (4*4 * 4) - 2) * 2 /*192*/, subdivided.topology.vertexEdges.Count);
		Assert.AreEqual(((4 + 3*6 + 3*4) + (4*4 * 4) - 2) * 2 /*192*/, subdivided.topology.faceEdges.Count);
		Assert.AreEqual(4*4 * 4 /*64*/, subdivided.topology.faces.Count);

		var vertexNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var vertex in subdivided.topology.vertices)
		{
			if (!vertexNeighborCountFrequencies.ContainsKey(vertex.neighborCount)) vertexNeighborCountFrequencies.Add(vertex.neighborCount, 0);
			vertexNeighborCountFrequencies[vertex.neighborCount] += 1;
		}
		Assert.AreEqual(2, vertexNeighborCountFrequencies.Count);
		Assert.AreEqual(4, vertexNeighborCountFrequencies[3]);
		Assert.AreEqual(30, vertexNeighborCountFrequencies[6]);

		var faceNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var face in subdivided.topology.faces)
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
		var tetrahedron = SphereTopology.CreateTetrahedron();
		var subdivided = SphereTopology.Subdivide(tetrahedron, 4);
		Assert.AreEqual(4 + 4*6 + 6*4 /*52*/, subdivided.topology.vertices.Count);
		Assert.AreEqual(((4 + 4*6 + 6*4) + (5*5 * 4) - 2) * 2 /*300*/, subdivided.topology.vertexEdges.Count);
		Assert.AreEqual(((4 + 4*6 + 6*4) + (5*5 * 4) - 2) * 2 /*300*/, subdivided.topology.faceEdges.Count);
		Assert.AreEqual(5*5 * 4 /*100*/, subdivided.topology.faces.Count);

		var vertexNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var vertex in subdivided.topology.vertices)
		{
			if (!vertexNeighborCountFrequencies.ContainsKey(vertex.neighborCount)) vertexNeighborCountFrequencies.Add(vertex.neighborCount, 0);
			vertexNeighborCountFrequencies[vertex.neighborCount] += 1;
		}
		Assert.AreEqual(2, vertexNeighborCountFrequencies.Count);
		Assert.AreEqual(4, vertexNeighborCountFrequencies[3]);
		Assert.AreEqual(48, vertexNeighborCountFrequencies[6]);

		var faceNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var face in subdivided.topology.faces)
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
		var tetrahedron = SphereTopology.CreateTetrahedron();
		var degree = 5;
		var vertexCount = 4 + degree * 6 + (degree * (degree - 1) / 2) * 4; /*74*/
		var faceCount = (degree + 1) * (degree + 1) * 4; /*144*/
		var edgeCount = (vertexCount + faceCount - 2) * 2; /*432*/
		var subdivided = SphereTopology.Subdivide(tetrahedron, degree);
		Assert.AreEqual(vertexCount, subdivided.topology.vertices.Count);
		Assert.AreEqual(edgeCount, subdivided.topology.vertexEdges.Count);
		Assert.AreEqual(edgeCount, subdivided.topology.faceEdges.Count);
		Assert.AreEqual(faceCount, subdivided.topology.faces.Count);

		var vertexNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var vertex in subdivided.topology.vertices)
		{
			if (!vertexNeighborCountFrequencies.ContainsKey(vertex.neighborCount)) vertexNeighborCountFrequencies.Add(vertex.neighborCount, 0);
			vertexNeighborCountFrequencies[vertex.neighborCount] += 1;
		}
		Assert.AreEqual(2, vertexNeighborCountFrequencies.Count);
		Assert.AreEqual(4, vertexNeighborCountFrequencies[3]);
		Assert.AreEqual(70, vertexNeighborCountFrequencies[6]);

		var faceNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var face in subdivided.topology.faces)
		{
			if (!faceNeighborCountFrequencies.ContainsKey(face.neighborCount)) faceNeighborCountFrequencies.Add(face.neighborCount, 0);
			faceNeighborCountFrequencies[face.neighborCount] += 1;
		}
		Assert.AreEqual(1, faceNeighborCountFrequencies.Count);
		Assert.AreEqual(144, faceNeighborCountFrequencies[3]);
	}

	[Test]
	public void CubeSubdivisionDegree1()
	{
		var cube = SphereTopology.CreateCube();
		var subdivided = SphereTopology.Subdivide(cube, 1);
		Assert.AreEqual(8 + 1*12 + 1*1*6 /*26*/, subdivided.topology.vertices.Count);
		Assert.AreEqual(((8 + 1*12 + 1*1*6) + (2*2 * 6) - 2) * 2 /*96*/, subdivided.topology.vertexEdges.Count);
		Assert.AreEqual(((8 + 1*12 + 1*1*6) + (2*2 * 6) - 2) * 2 /*96*/, subdivided.topology.faceEdges.Count);
		Assert.AreEqual(2*2 * 6 /*24*/, subdivided.topology.faces.Count);

		var vertexNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var vertex in subdivided.topology.vertices)
		{
			if (!vertexNeighborCountFrequencies.ContainsKey(vertex.neighborCount)) vertexNeighborCountFrequencies.Add(vertex.neighborCount, 0);
			vertexNeighborCountFrequencies[vertex.neighborCount] += 1;
		}
		Assert.AreEqual(2, vertexNeighborCountFrequencies.Count);
		Assert.AreEqual(8, vertexNeighborCountFrequencies[3]);
		Assert.AreEqual(18, vertexNeighborCountFrequencies[4]);

		var faceNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var face in subdivided.topology.faces)
		{
			if (!faceNeighborCountFrequencies.ContainsKey(face.neighborCount)) faceNeighborCountFrequencies.Add(face.neighborCount, 0);
			faceNeighborCountFrequencies[face.neighborCount] += 1;
		}
		Assert.AreEqual(1, faceNeighborCountFrequencies.Count);
		Assert.AreEqual(24, faceNeighborCountFrequencies[4]);
	}

	[Test]
	public void CubeSubdivisionDegree2()
	{
		var cube = SphereTopology.CreateCube();
		var subdivided = SphereTopology.Subdivide(cube, 2);
		Assert.AreEqual(8 + 2*12 + 2*2*6 /*56*/, subdivided.topology.vertices.Count);
		Assert.AreEqual(((8 + 2*12 + 2*2*6) + (3*3 * 6) - 2) * 2 /*216*/, subdivided.topology.vertexEdges.Count);
		Assert.AreEqual(((8 + 2*12 + 2*2*6) + (3*3 * 6) - 2) * 2 /*216*/, subdivided.topology.faceEdges.Count);
		Assert.AreEqual(3*3 * 6 /*54*/, subdivided.topology.faces.Count);

		var vertexNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var vertex in subdivided.topology.vertices)
		{
			if (!vertexNeighborCountFrequencies.ContainsKey(vertex.neighborCount)) vertexNeighborCountFrequencies.Add(vertex.neighborCount, 0);
			vertexNeighborCountFrequencies[vertex.neighborCount] += 1;
		}
		Assert.AreEqual(2, vertexNeighborCountFrequencies.Count);
		Assert.AreEqual(8, vertexNeighborCountFrequencies[3]);
		Assert.AreEqual(48, vertexNeighborCountFrequencies[4]);

		var faceNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var face in subdivided.topology.faces)
		{
			if (!faceNeighborCountFrequencies.ContainsKey(face.neighborCount)) faceNeighborCountFrequencies.Add(face.neighborCount, 0);
			faceNeighborCountFrequencies[face.neighborCount] += 1;
		}
		Assert.AreEqual(1, faceNeighborCountFrequencies.Count);
		Assert.AreEqual(54, faceNeighborCountFrequencies[4]);
	}

	[Test]
	public void CubeSubdivisionDegree3()
	{
		var cube = SphereTopology.CreateCube();
		var subdivided = SphereTopology.Subdivide(cube, 3);
		Assert.AreEqual(8 + 3*12 + 3*3*6 /*98*/, subdivided.topology.vertices.Count);
		Assert.AreEqual(((8 + 3*12 + 3*3*6) + (4*4 * 6) - 2) * 2 /*384*/, subdivided.topology.vertexEdges.Count);
		Assert.AreEqual(((8 + 3*12 + 3*3*6) + (4*4 * 6) - 2) * 2 /*384*/, subdivided.topology.faceEdges.Count);
		Assert.AreEqual(4*4 * 6 /*96*/, subdivided.topology.faces.Count);

		var vertexNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var vertex in subdivided.topology.vertices)
		{
			if (!vertexNeighborCountFrequencies.ContainsKey(vertex.neighborCount)) vertexNeighborCountFrequencies.Add(vertex.neighborCount, 0);
			vertexNeighborCountFrequencies[vertex.neighborCount] += 1;
		}
		Assert.AreEqual(2, vertexNeighborCountFrequencies.Count);
		Assert.AreEqual(8, vertexNeighborCountFrequencies[3]);
		Assert.AreEqual(90, vertexNeighborCountFrequencies[4]);

		var faceNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var face in subdivided.topology.faces)
		{
			if (!faceNeighborCountFrequencies.ContainsKey(face.neighborCount)) faceNeighborCountFrequencies.Add(face.neighborCount, 0);
			faceNeighborCountFrequencies[face.neighborCount] += 1;
		}
		Assert.AreEqual(1, faceNeighborCountFrequencies.Count);
		Assert.AreEqual(96, faceNeighborCountFrequencies[4]);
	}

	[Test]
	public void CubeSubdivisionDegree4()
	{
		var cube = SphereTopology.CreateCube();
		var degree = 4;
		var vertexCount = 8 + degree * 12 + degree * degree * 6; /*152*/
		var faceCount = (degree + 1) * (degree + 1) * 6; /*150*/
		var edgeCount = (vertexCount + faceCount - 2) * 2; /*384*/
		var subdivided = SphereTopology.Subdivide(cube, degree);
		Assert.AreEqual(vertexCount, subdivided.topology.vertices.Count);
		Assert.AreEqual(edgeCount, subdivided.topology.vertexEdges.Count);
		Assert.AreEqual(edgeCount, subdivided.topology.faceEdges.Count);
		Assert.AreEqual(faceCount, subdivided.topology.faces.Count);

		var vertexNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var vertex in subdivided.topology.vertices)
		{
			if (!vertexNeighborCountFrequencies.ContainsKey(vertex.neighborCount)) vertexNeighborCountFrequencies.Add(vertex.neighborCount, 0);
			vertexNeighborCountFrequencies[vertex.neighborCount] += 1;
		}
		Assert.AreEqual(2, vertexNeighborCountFrequencies.Count);
		Assert.AreEqual(8, vertexNeighborCountFrequencies[3]);
		Assert.AreEqual(vertexCount - 8, vertexNeighborCountFrequencies[4]);

		var faceNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var face in subdivided.topology.faces)
		{
			if (!faceNeighborCountFrequencies.ContainsKey(face.neighborCount)) faceNeighborCountFrequencies.Add(face.neighborCount, 0);
			faceNeighborCountFrequencies[face.neighborCount] += 1;
		}
		Assert.AreEqual(1, faceNeighborCountFrequencies.Count);
		Assert.AreEqual(faceCount, faceNeighborCountFrequencies[4]);
	}

	[Test]
	public void CubeSubdivisionDegree5()
	{
		var cube = SphereTopology.CreateCube();
		var degree = 5;
		var vertexCount = 8 + degree * 12 + degree * degree * 6; /*218*/
		var faceCount = (degree + 1) * (degree + 1) * 6; /*216*/
		var edgeCount = (vertexCount + faceCount - 2) * 2; /*864*/
		var subdivided = SphereTopology.Subdivide(cube, degree);
		Assert.AreEqual(vertexCount, subdivided.topology.vertices.Count);
		Assert.AreEqual(edgeCount, subdivided.topology.vertexEdges.Count);
		Assert.AreEqual(edgeCount, subdivided.topology.faceEdges.Count);
		Assert.AreEqual(faceCount, subdivided.topology.faces.Count);

		var vertexNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var vertex in subdivided.topology.vertices)
		{
			if (!vertexNeighborCountFrequencies.ContainsKey(vertex.neighborCount)) vertexNeighborCountFrequencies.Add(vertex.neighborCount, 0);
			vertexNeighborCountFrequencies[vertex.neighborCount] += 1;
		}
		Assert.AreEqual(2, vertexNeighborCountFrequencies.Count);
		Assert.AreEqual(8, vertexNeighborCountFrequencies[3]);
		Assert.AreEqual(vertexCount - 8, vertexNeighborCountFrequencies[4]);

		var faceNeighborCountFrequencies = new Dictionary<int, int>();
		foreach (var face in subdivided.topology.faces)
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
		var tetrahedron = SphereTopology.CreateTetrahedron();
		var dual = tetrahedron.topology.GetDualTopology();

		foreach (var vertex in tetrahedron.topology.vertices)
		{
			var edge = vertex.firstEdge;
			var dualFace = dual.faces[vertex.index];
			Assert.AreEqual(vertex.neighborCount, dualFace.neighborCount);
			var dualEdge = dualFace.FindEdge(dual.faces[edge.farVertex.index]);
			Assert.AreEqual(edge.farVertex.index, dualEdge.farFace.index);
			edge = edge.next;
			dualEdge = dualEdge.next;
			Assert.AreEqual(edge.farVertex.index, dualEdge.farFace.index);
			edge = edge.next;
			dualEdge = dualEdge.next;
			Assert.AreEqual(edge.farVertex.index, dualEdge.farFace.index);
		}
	}
}
