using UnityEngine;
using NUnit.Framework;
using System;

public class MeshTopologyTests
{
	private void Print(MeshTopology.Vertex vertex)
	{
		var sb = new System.Text.StringBuilder();
		sb.AppendFormat("Vertex Index: {0}\nNeighbors: {1}\n", vertex.Index, vertex.NeighborCount);
		for (int i = 0; i < vertex.NeighborCount; ++i)
		{
			var neighborVertex = vertex.Vertices[i];
			var neighborEdge = vertex.Edges[i];
			var neighborTriangle = vertex.Triangles[i];
			sb.AppendFormat("Neighbor {0}:\n", i);
			sb.AppendFormat("\tVertex (Index: {0})\n", neighborVertex.Index);
			sb.Append("\t\tVertices:");
			foreach (var neighborNeighborVertex in neighborVertex.Vertices) sb.AppendFormat(" {0},", neighborNeighborVertex.Index);
			sb.Append("\n");
			sb.Append("\t\tEdges:");
			foreach (var neighborNeighborEdge in neighborVertex.Edges) sb.AppendFormat(" {0},", neighborNeighborEdge.Index);
			sb.Append("\n");
			sb.Append("\t\tTriangles:");
			foreach (var neighborNeighborTriangle in neighborVertex.Triangles) sb.AppendFormat(" {0},", neighborNeighborTriangle.Index);
			sb.Append("\n");
			sb.AppendFormat("\tEdge (Index: {0})\n", neighborEdge.Index);
			sb.Append("\t\tVertices:");
			foreach (var neighborNeighborVertex in neighborEdge.Vertices) sb.AppendFormat(" {0},", neighborNeighborVertex.Index);
			sb.Append("\n");
			sb.Append("\t\tTriangles:");
			foreach (var neighborNeighborTriangle in neighborEdge.Triangles) sb.AppendFormat(" {0},", neighborNeighborTriangle.Index);
			sb.Append("\n");
			sb.AppendFormat("\tTriangle (Index: {0})\n", neighborTriangle.Index);
			sb.Append("\t\tVertices:");
			foreach (var neighborNeighborVertex in neighborTriangle.Vertices) sb.AppendFormat(" {0},", neighborNeighborVertex.Index);
			sb.Append("\n");
			sb.Append("\t\tEdges:");
			foreach (var neighborNeighborEdge in neighborTriangle.Edges) sb.AppendFormat(" {0},", neighborNeighborEdge.Index);
			sb.Append("\n");
			sb.Append("\t\tTriangles:");
			foreach (var neighborNeighborTriangle in neighborTriangle.Triangles) sb.AppendFormat(" {0},", neighborNeighborTriangle.Index);
			sb.Append("\n");
		}
		Debug.Log(sb.ToString());
	}

	private void Print(MeshTopology.Edge edge)
	{
		var sb = new System.Text.StringBuilder();
		sb.AppendFormat("Edge Index: {0}\nNeighbors: {1}\n", edge.Index, edge.NeighborCount);
		for (int i = 0; i < edge.NeighborCount; ++i)
		{
			var neighborVertex = edge.Vertices[i];
			var neighborTriangle = edge.Triangles[i];
			sb.AppendFormat("Neighbor {0}:\n", i);
			sb.AppendFormat("\tVertex (Index: {0})\n", neighborVertex.Index);
			sb.Append("\t\tVertices:");
			foreach (var neighborNeighborVertex in neighborVertex.Vertices) sb.AppendFormat(" {0},", neighborNeighborVertex.Index);
			sb.Append("\n");
			sb.Append("\t\tEdges:");
			foreach (var neighborNeighborEdge in neighborVertex.Edges) sb.AppendFormat(" {0},", neighborNeighborEdge.Index);
			sb.Append("\n");
			sb.Append("\t\tTriangles:");
			foreach (var neighborNeighborTriangle in neighborVertex.Triangles) sb.AppendFormat(" {0},", neighborNeighborTriangle.Index);
			sb.Append("\n");
			sb.AppendFormat("\tTriangle (Index: {0})\n", neighborTriangle.Index);
			sb.Append("\t\tVertices:");
			foreach (var neighborNeighborVertex in neighborTriangle.Vertices) sb.AppendFormat(" {0},", neighborNeighborVertex.Index);
			sb.Append("\n");
			sb.Append("\t\tEdges:");
			foreach (var neighborNeighborEdge in neighborTriangle.Edges) sb.AppendFormat(" {0},", neighborNeighborEdge.Index);
			sb.Append("\n");
			sb.Append("\t\tTriangles:");
			foreach (var neighborNeighborTriangle in neighborTriangle.Triangles) sb.AppendFormat(" {0},", neighborNeighborTriangle.Index);
			sb.Append("\n");
		}
		Debug.Log(sb.ToString());
	}

	private void Print(MeshTopology topology)
	{
		var sb = new System.Text.StringBuilder();
		sb.Append("Vertex Offsets:");
		foreach (var offset in topology._vertexNeighborOffsets) sb.AppendFormat(" {0},", offset);
		sb.Append("\n");
		sb.Append("Vertex Vertices:");
		for (var i = 0; i < topology._vertexNeighborOffsets.Length - 1; ++i)
		{
			sb.Append(" [");
			for (var j = topology._vertexNeighborOffsets[i]; j < topology._vertexNeighborOffsets[i + 1]; ++j)
				sb.AppendFormat(" {0},", topology._vertexVertices[j]);
			sb.Append(" ],");
		}
		sb.Append("\n");
		sb.Append("Vertex Edges:");
		for (var i = 0; i < topology._vertexNeighborOffsets.Length - 1; ++i)
		{
			sb.Append(" [");
			for (var j = topology._vertexNeighborOffsets[i]; j < topology._vertexNeighborOffsets[i + 1]; ++j)
				sb.AppendFormat(" {0},", topology._vertexEdges[j]);
			sb.Append(" ],");
		}
		sb.Append("\n");
		sb.Append("Vertex Triangles:");
		for (var i = 0; i < topology._vertexNeighborOffsets.Length - 1; ++i)
		{
			sb.Append(" [");
			for (var j = topology._vertexNeighborOffsets[i]; j < topology._vertexNeighborOffsets[i + 1]; ++j)
				sb.AppendFormat(" {0},", topology._vertexTriangles[j]);
			sb.Append(" ],");
		}
		Debug.Log(sb.ToString());
	}

	private void ValidateInvariants(MeshTopology.Vertex vertex)
	{
		for (int i = 0; i < vertex.NeighborCount; ++i)
		{
			var neighborVertex = vertex.Vertices[i];
			var neighborEdge = vertex.Edges[i];
			var neighborTriangle = vertex.Triangles[i];

			// The vertex neighbors are findable as neighbors.
			Assert.DoesNotThrow(() => { vertex.NeighborIndexOf(neighborVertex); }, string.Format("Vertex invariant was violated.  (Neighbor {0})  The search for the neighbor vertex failed.", i));
			Assert.DoesNotThrow(() => { vertex.NeighborIndexOf(neighborEdge); }, string.Format("Vertex invariant was violated.  (Neighbor {0})  The search for the neighbor edge failed.", i));
			Assert.DoesNotThrow(() => { vertex.NeighborIndexOf(neighborTriangle); }, string.Format("Vertex invariant was violated.  (Neighbor {0})  The search for the neighbor triangle failed.", i));

			// The vertex neighbors have the expected neighbor index.
			Assert.That(vertex.NeighborIndexOf(neighborVertex) == i, string.Format("Vertex invariant was violated.  (Neighbor {0})  The search for the neighbor vertex did not produce the expected index.", i));
			Assert.That(vertex.NeighborIndexOf(neighborEdge) == i, string.Format("Vertex invariant was violated.  (Neighbor {0})  The search for the neighbor vertex did not produce the expected index.", i));
			Assert.That(vertex.NeighborIndexOf(neighborTriangle) == i, string.Format("Vertex invariant was violated.  (Neighbor {0})  The search for the neighbor vertex did not produce the expected index.", i));

			// The vertex is a neighbor of its neighbor objects.
			Assert.DoesNotThrow(() => { neighborVertex.NeighborIndexOf(vertex); }, string.Format("Vertex invariant was violated.  (Neighbor {0})  The search for the original vertex within the neighbor vertex failed.", i));
			Assert.DoesNotThrow(() => { neighborEdge.NeighborIndexOf(vertex); }, string.Format("Vertex invariant was violated.  (Neighbor {0})  The search for the original vertex within the neighbor edge failed.", i));
			Assert.DoesNotThrow(() => { neighborTriangle.NeighborIndexOf(vertex); }, string.Format("Vertex invariant was violated.  (Neighbor {0})  The search for the original vertex within the neighbor triangle failed.", i));

			// The previous and next neighbor vertices are neighbors on the neighbor vertex.
			Assert.DoesNotThrow(() => { neighborVertex.NeighborIndexOf(vertex.PrevVertex(neighborVertex)); }, string.Format("Vertex invariant was violated.  (Neighbor {0})  The search for the previous neighbor vertex within the neighbor vertex failed.", i));
			Assert.DoesNotThrow(() => { neighborVertex.NeighborIndexOf(vertex.NextVertex(neighborVertex)); }, string.Format("Vertex invariant was violated.  (Neighbor {0})  The search for the next neighbor vertex within the neighbor vertex failed.", i));

			// The previous and next neighbor triangles are neighbors on the neighbor triangle.
			Assert.DoesNotThrow(() => { neighborTriangle.NeighborIndexOf(vertex.PrevTriangle(neighborTriangle)); }, string.Format("Vertex invariant was violated.  (Neighbor {0})  The search for the previous neighbor triangle within the neighbor triangle failed.", i));
			Assert.DoesNotThrow(() => { neighborTriangle.NeighborIndexOf(vertex.NextTriangle(neighborTriangle)); }, string.Format("Vertex invariant was violated.  (Neighbor {0})  The search for the next neighbor triangle within the neighbor triangle failed.", i));

			Assert.That(neighborVertex.NextTriangle(vertex.NextVertex(neighborVertex)) == neighborTriangle, string.Format("Vertex invariant was violated.  (Neighbor {0})  The neighbor triangles of the neighbor vertex were not aligned with the neighbor triangles of this vertex.", i));
		}
	}

	private void ValidateInvariants(MeshTopology.Edge edge)
	{
		for (int i = 0; i < edge.NeighborCount; ++i)
		{
			var neighborVertex = edge.Vertices[i];
			var neighborTriangle = edge.Triangles[i];

			// The edge neighbors are findable as neighbors.
			Assert.DoesNotThrow(() => { edge.NeighborIndexOf(neighborVertex); }, "Edge invariant was violated.");
			Assert.DoesNotThrow(() => { edge.NeighborIndexOf(neighborTriangle); }, "Edge invariant was violated.");

			// The edge neighbors have the expected neighbor index.
			Assert.That(edge.NeighborIndexOf(neighborVertex) == i, "Edge invariant was violated.");
			Assert.That(edge.NeighborIndexOf(neighborTriangle) == i, "Edge invariant was violated.");

			// The edge is a neighbor of its neighbor objects.
			Assert.DoesNotThrow(() => { neighborVertex.NeighborIndexOf(edge); }, "Edge invariant was violated.");
			Assert.DoesNotThrow(() => { neighborTriangle.NeighborIndexOf(edge); }, "Edge invariant was violated.");

			Assert.That(edge.NextTriangle(neighborVertex) == neighborTriangle, "Edge invariant was violated.");
			Assert.That(edge.PrevTriangle(neighborVertex) == edge.OppositeTriangle(neighborTriangle), "Edge invariant was violated.");

			Assert.That(edge.NextVertex(neighborTriangle) == edge.OppositeVertex(neighborVertex), "Edge invariant was violated.");
			Assert.That(edge.PrevVertex(neighborTriangle) == neighborVertex, "Edge invariant was violated.");

			Assert.That(edge.OppositeVertex(neighborVertex).AssociatedVertex(edge) == neighborVertex, "Edge invariant was violated.");
			Assert.That(edge.OppositeTriangle(neighborTriangle).AssociatedTriangle(edge) == neighborTriangle, "Edge invariant was violated.");
		}
	}

	private void ValidateInvariants(MeshTopology.Triangle triangle)
	{
		for (int i = 0; i < triangle.NeighborCount; ++i)
		{
			var neighborVertex = triangle.Vertices[i];
			var neighborEdge = triangle.Edges[i];
			var neighborTriangle = triangle.Triangles[i];

			// The triangle neighbors are findable as neighbors.
			Assert.DoesNotThrow(() => { triangle.NeighborIndexOf(neighborVertex); }, "Triangle invariant was violated.");
			Assert.DoesNotThrow(() => { triangle.NeighborIndexOf(neighborEdge); }, "Triangle invariant was violated.");
			Assert.DoesNotThrow(() => { triangle.NeighborIndexOf(neighborTriangle); }, "Triangle invariant was violated.");

			// The triangle neighbors have the expected neighbor index.
			Assert.That(triangle.NeighborIndexOf(neighborVertex) == i, "Triangle invariant was violated.");
			Assert.That(triangle.NeighborIndexOf(neighborEdge) == i, "Triangle invariant was violated.");
			Assert.That(triangle.NeighborIndexOf(neighborTriangle) == i, "Triangle invariant was violated.");

			// The triangle is a neighbor of its neighbor objects.
			Assert.DoesNotThrow(() => { neighborVertex.NeighborIndexOf(triangle); }, "Triangle invariant was violated.");
			Assert.DoesNotThrow(() => { neighborEdge.NeighborIndexOf(triangle); }, "Triangle invariant was violated.");
			Assert.DoesNotThrow(() => { neighborTriangle.NeighborIndexOf(triangle); }, "Triangle invariant was violated.");

			// The neighbor vertex relates properly to the neighbor edge and triangle.
			Assert.That(triangle.NextEdge(neighborVertex) == neighborEdge, "Triangle invariant was violated.");
			Assert.That(triangle.NextTriangle(neighborVertex) == neighborTriangle, "Triangle invariant was violated.");
			Assert.That(triangle.PrevVertex(neighborEdge) == neighborVertex, "Triangle invariant was violated.");
			Assert.That(triangle.PrevVertex(neighborTriangle) == neighborVertex, "Triangle invariant was violated.");

			// The neighbor edge and triangle relate properly to each other.
			Assert.That(triangle.AssociatedEdge(neighborTriangle) == neighborEdge, "Triangle invariant was violated.");
			Assert.That(triangle.AssociatedTriangle(neighborEdge) == neighborTriangle, "Triangle invariant was violated.");

			Assert.That(neighborVertex.AssociatedVertex(neighborEdge) == triangle.NextVertex(neighborVertex), "Triangle Invariant was violated.");
			Assert.That(neighborVertex.AssociatedVertex(triangle.PrevEdge(neighborEdge)) == triangle.PrevVertex(neighborVertex), "Triangle Invariant was violated.");

			Assert.That(neighborVertex.NextEdge(neighborEdge) == triangle.PrevEdge(neighborEdge), "Triangle Invariant was violated.");
			Assert.That(neighborVertex.PrevEdge(triangle.PrevEdge(neighborEdge)) == neighborEdge, "Triangle Invariant was violated.");

			Assert.That(neighborVertex.NextTriangle(neighborTriangle) == triangle, "Triangle Invariant was violated.");
			Assert.That(neighborVertex.NextTriangle(triangle) == triangle.PrevTriangle(neighborTriangle), "Triangle Invariant was violated.");
			Assert.That(neighborVertex.PrevTriangle(triangle.PrevTriangle(neighborTriangle)) == triangle, "Triangle Invariant was violated.");
			Assert.That(neighborVertex.PrevTriangle(triangle) == neighborTriangle, "Triangle Invariant was violated.");
		}
	}

	private void ValidateInvariants(MeshTopology topology)
	{
		foreach (var vertex in topology.Vertices) try { ValidateInvariants(vertex); } catch (Exception) { Print(vertex); throw; }
		foreach (var edge in topology.Edges) try { ValidateInvariants(edge); } catch (Exception) { Print(edge); throw; }
		foreach (var triangle in topology.Triangles) ValidateInvariants(triangle);
	}

	[Test]
	public void IcosahedronValidation()
	{
		BasicMeshTopology basicTopology;
		Vector3[] vertexPositions;
		SphereTopology.Icosahedron(out basicTopology, out vertexPositions);
		var topology = new MeshTopology(basicTopology);
		ValidateInvariants(topology);
	}

	[Test]
	public void SubdividedIcosahedronValidation()
	{
		BasicMeshTopology basicTopology;
		Vector3[] vertexPositions;
		SphereTopology.Icosahedron(out basicTopology, out vertexPositions);
		var topology = new MeshTopology(basicTopology);
		ValidateInvariants(new MeshTopology(topology.Subdivide(1, (int count) => { }, (int lhs, int rhs) => { }, (int i, int j, int k, float t) => { })));
		ValidateInvariants(new MeshTopology(topology.Subdivide(2, (int count) => { }, (int lhs, int rhs) => { }, (int i, int j, int k, float t) => { })));
		ValidateInvariants(new MeshTopology(topology.Subdivide(5, (int count) => { }, (int lhs, int rhs) => { }, (int i, int j, int k, float t) => { })));
	}

	[Test]
	public void IcosahedronAlterTopologyValidation_SingleEdgeRotation()
	{
		BasicMeshTopology basicTopology;
		Vector3[] vertexPositions;
		SphereTopology.Icosahedron(out basicTopology, out vertexPositions);
		var topology = new MeshTopology(basicTopology);
		ValidateInvariants(topology
			.AlterTopology(1,
			(MeshTopology altered, int edge) =>
				{
					return edge == 0;
				},
				(MeshTopology altered) =>
				{
				}));
	}

	[Test]
	public void SubdividedIcosahedronAlterTopologyValidation_SingleEdgeRotation()
	{
		BasicMeshTopology basicTopology;
		Vector3[] vertexPositions;
		SphereTopology.Icosahedron(out basicTopology, out vertexPositions);
		var topology = new MeshTopology(basicTopology);
		ValidateInvariants(new MeshTopology(topology.Subdivide(1, (int count) => { }, (int lhs, int rhs) => { }, (int i, int j, int k, float t) => { }))
			.AlterTopology(1,
				(MeshTopology altered, int edge) =>
				{
					return edge == 5;
				},
				(MeshTopology altered) =>
				{
				}));
	}

	[Test]
	public void SubdividedIcosahedronAlterTopologyValidation_MultipleEdgeRotations()
	{
		BasicMeshTopology basicTopology;
		Vector3[] vertexPositions;
		SphereTopology.Icosahedron(out basicTopology, out vertexPositions);
		var topology = new MeshTopology(basicTopology);
		ValidateInvariants(new MeshTopology(topology.Subdivide(1, (int count) => { }, (int lhs, int rhs) => { }, (int i, int j, int k, float t) => { }))
			.AlterTopology(1,
				(MeshTopology altered, int edge) =>
				{
					return (edge % 41 == 0 || edge % 43 == 0);
				},
				(MeshTopology altered) =>
				{
				}));
	}

	[Test]
	public void IcosahedronAlterTopologyValidation_MultipleEdgeRotations_MultiplePasses()
	{
		BasicMeshTopology basicTopology;
		Vector3[] vertexPositions;
		SphereTopology.Icosahedron(out basicTopology, out vertexPositions);
		var topology = new MeshTopology(basicTopology);
		topology = topology
			.AlterTopology(3,
				(MeshTopology altered, int edge) =>
				{
					return edge == 0;
				},
				(MeshTopology altered) =>
				{
					ValidateInvariants(altered);
				});
	}

	[Test]
	public void SubdividedIcosahedronAlterTopologyValidation_MultipleEdgeRotations_MultiplePasses()
	{
		BasicMeshTopology basicTopology;
		Vector3[] vertexPositions;
		SphereTopology.Icosahedron(out basicTopology, out vertexPositions);
		var topology = new MeshTopology(basicTopology);
		topology = topology
			.AlterTopology(3,
				(MeshTopology altered, int edge) =>
				{
					return edge == 0;
				},
				(MeshTopology altered) =>
				{
					ValidateInvariants(altered);
				});
	}
}
